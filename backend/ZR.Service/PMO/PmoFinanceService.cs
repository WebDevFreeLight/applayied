using System;
using System.Collections.Generic;
using System.Linq;
using Infrastructure;
using Infrastructure.Attribute;
using SqlSugar;
using ZR.Model;
using ZR.Model.PMO;
using ZR.Model.PMO.Dto;
using ZR.Model.System;
using ZR.Repository;
using ZR.ServiceCore;
using ZR.Service.PMO.IPmoService;

namespace ZR.Service.PMO
{
    /// <summary>
    /// Financial management: budgets, revisions, cost transactions, forecasts and the derived
    /// financial summary. Posting a cost and approving a revision each commit their state change
    /// AND an audit event inside ONE database transaction (atomic — partial failure rolls back both).
    /// </summary>
    [AppService(ServiceType = typeof(IPmoFinanceService), ServiceLifetime = LifeTime.Transient)]
    public class PmoFinanceService : BaseService<PmoCostTransaction>, IPmoFinanceService
    {
        // ---------------- Derived calculations ----------------

        /// <summary>Current approved budget = initial + Σ(approved revisions). Pending/Rejected excluded.</summary>
        public decimal GetCurrentApprovedBudget(long projectId)
        {
            var project = Context.Queryable<PmoProject>().First(p => p.ProjectId == projectId)
                ?? throw new CustomException(ResultCode.FAIL, "Project not found");
            var approvedRevisions = Context.Queryable<PmoBudgetRevision>()
                .Where(r => r.ProjectId == projectId && r.Status == "Approved")
                .Sum(r => r.Amount);
            return project.InitialApprovedBudget + approvedRevisions;
        }

        /// <summary>Actual spend = Σ posted transactions with txn_date ≤ cutoff. Draft excluded.</summary>
        public decimal GetActualSpend(long projectId, DateTime cutoff)
        {
            return Context.Queryable<PmoCostTransaction>()
                .Where(t => t.ProjectId == projectId && t.Status == "Posted" && t.TxnDate <= cutoff)
                .Sum(t => t.Amount);
        }

        public PmoFinancialSummaryDto GetProjectSummary(long projectId, DateTime? cutoff, PmoActor actor)
        {
            var reportDate = cutoff ?? DateTime.Today;
            var project = Context.Queryable<PmoProject>().First(p => p.ProjectId == projectId)
                ?? throw new CustomException(ResultCode.FAIL, "Project not found");

            var approvedRevisions = Context.Queryable<PmoBudgetRevision>()
                .Where(r => r.ProjectId == projectId && r.Status == "Approved").Sum(r => r.Amount);
            var current = project.InitialApprovedBudget + approvedRevisions;
            var actual = GetActualSpend(projectId, reportDate);

            // Forecast-to-complete: Σ of ForecastToComplete rows (documented remaining-period method).
            var ftc = Context.Queryable<PmoFinancialForecast>()
                .Where(f => f.ProjectId == projectId && f.Kind == "ForecastToComplete").Sum(f => f.ForecastAmount);
            var hasForecast = Context.Queryable<PmoFinancialForecast>().Any(f => f.ProjectId == projectId);
            var eac = actual + ftc;

            var summary = new PmoFinancialSummaryDto
            {
                ProjectId = projectId,
                ProjectName = project.Name,
                Currency = project.Currency,
                Cutoff = reportDate,
                InitialApprovedBudget = project.InitialApprovedBudget,
                ApprovedRevisions = approvedRevisions,
                CurrentApprovedBudget = current,
                ActualSpend = actual,
                RemainingBudget = current - actual,
                UtilizationPct = current > 0 ? Math.Round(actual / current * 100m, 1) : (decimal?)null,
                ForecastToComplete = ftc,
                Eac = eac,
                ProjectedVariance = current - eac,
                IsOverBudget = actual > current,
                IsProjectedOverrun = eac > current,
                HasForecast = hasForecast
            };

            // Spend by category — grouped aggregation in SQL, then joined to category names.
            var byCat = Context.Queryable<PmoCostTransaction>()
                .Where(t => t.ProjectId == projectId && t.Status == "Posted" && t.TxnDate <= reportDate)
                .GroupBy(t => t.CategoryId)
                .Select(t => new { t.CategoryId, Amount = SqlFunc.AggregateSum(t.Amount) })
                .ToList();
            if (byCat.Count > 0)
            {
                var catIds = byCat.Select(c => c.CategoryId).ToList();
                var catNames = Context.Queryable<PmoCostCategory>().Where(c => catIds.Contains(c.CategoryId))
                    .ToList().ToDictionary(c => c.CategoryId, c => c.Name);
                summary.SpendByCategory = byCat
                    .Select(c => new PmoCategorySpendDto
                    {
                        CategoryName = catNames.TryGetValue(c.CategoryId, out var n) ? n : "Uncategorised",
                        Amount = c.Amount
                    }).OrderByDescending(c => c.Amount).ToList();
            }

            // Monthly trend — posted rows from SQL, bucketed by yyyy-MM in app (portable across DBs).
            var posted = Context.Queryable<PmoCostTransaction>()
                .Where(t => t.ProjectId == projectId && t.Status == "Posted" && t.TxnDate <= reportDate)
                .Select(t => new { t.TxnDate, t.Amount }).ToList();
            summary.MonthlyTrend = posted
                .GroupBy(t => t.TxnDate.ToString("yyyy-MM"))
                .Select(g => new PmoMonthlySpendDto { Period = g.Key, Amount = g.Sum(x => x.Amount) })
                .OrderBy(m => m.Period).ToList();

            return summary;
        }

        // ---------------- Cost transactions ----------------

        public PagedInfo<PmoCostTransaction> GetTransactions(PmoCostTransactionQueryDto parm, PmoActor actor)
        {
            var exp = Expressionable.Create<PmoCostTransaction>();
            exp.AndIF(parm.ProjectId != null, t => t.ProjectId == parm.ProjectId);
            exp.AndIF(parm.CategoryId != null, t => t.CategoryId == parm.CategoryId);
            exp.AndIF(!string.IsNullOrEmpty(parm.Status), t => t.Status == parm.Status);
            exp.AndIF(parm.BeginDate != null, t => t.TxnDate >= parm.BeginDate);
            exp.AndIF(parm.EndDate != null, t => t.TxnDate <= parm.EndDate);
            if (!string.IsNullOrWhiteSpace(parm.Keyword))
                exp.And(t => t.Description.Contains(parm.Keyword));

            var page = Queryable().Where(exp.ToExpression())
                .OrderBy(t => t.TxnDate, OrderByType.Desc).ToPage(parm);
            EnrichTransactions(page.Result);
            return page;
        }

        public PmoCostTransaction AddTransaction(PmoCostTransactionInputDto dto, PmoActor actor)
        {
            if (dto.Amount <= 0) throw new CustomException(ResultCode.FAIL, "Amount must be greater than zero");
            if (!Context.Queryable<PmoProject>().Any(p => p.ProjectId == dto.ProjectId))
                throw new CustomException(ResultCode.FAIL, "Project not found");
            if (!Context.Queryable<PmoCostCategory>().Any(c => c.CategoryId == dto.CategoryId))
                throw new CustomException(ResultCode.FAIL, "Cost category not found");

            var entity = new PmoCostTransaction
            {
                ProjectId = dto.ProjectId,
                CategoryId = dto.CategoryId,
                Amount = dto.Amount,
                TxnDate = dto.TxnDate == default ? DateTime.Today : dto.TxnDate,
                Description = dto.Description,
                Status = "Draft",                     // new costs start as Draft; posting is a controlled step
                Create_by = actor.UserName,
                Create_time = DateTime.Now
            };
            entity.TransactionId = Insertable(entity).ExecuteReturnSnowflakeId();
            return entity;
        }

        public int UpdateTransaction(PmoCostTransactionInputDto dto, PmoActor actor)
        {
            var existing = Queryable().First(t => t.TransactionId == dto.TransactionId)
                ?? throw new CustomException(ResultCode.FAIL, "Transaction not found");
            if (existing.Status == "Posted")
                throw new CustomException(ResultCode.FORBIDDEN, "A posted transaction cannot be edited (void/reverse instead)");
            if (dto.Amount <= 0) throw new CustomException(ResultCode.FAIL, "Amount must be greater than zero");

            existing.CategoryId = dto.CategoryId;
            existing.Amount = dto.Amount;
            existing.TxnDate = dto.TxnDate;
            existing.Description = dto.Description;
            existing.Update_by = actor.UserName;
            existing.Update_time = DateTime.Now;
            return Update(existing, true, "Updated cost transaction");
        }

        /// <summary>
        /// Post a draft cost. Atomic: the status change AND its audit event commit together.
        /// Re-posting is blocked (idempotent guard against double submission).
        /// </summary>
        public PmoCostTransaction PostTransaction(long transactionId, PmoActor actor)
        {
            var tx = Queryable().First(t => t.TransactionId == transactionId)
                ?? throw new CustomException(ResultCode.FAIL, "Transaction not found");
            if (tx.Status == "Posted")
                throw new CustomException(ResultCode.FAIL, "Transaction is already posted");

            var now = DateTime.Now;
            try
            {
                Context.Ado.BeginTran();

                tx.Status = "Posted";
                tx.PostedByUserId = actor.UserId;
                tx.PostedAt = now;
                tx.Update_by = actor.UserName;
                tx.Update_time = now;
                Update(tx, true, "Posted cost transaction");

                var audit = new PmoAuditEvent
                {
                    EntityType = "CostTransaction",
                    EntityId = tx.TransactionId,
                    Action = "Posted",
                    ActorUserId = actor.UserId,
                    Detail = $"Posted {tx.Amount:0.00} {tx.Currency()} on project {tx.ProjectId}",
                    ActedAt = now,
                    Create_by = actor.UserName,
                    Create_time = now
                };
                audit.AuditId = Context.Insertable(audit).ExecuteReturnSnowflakeId();

                Context.Ado.CommitTran();
            }
            catch
            {
                Context.Ado.RollbackTran();   // neither the post nor the audit survives a partial failure
                throw;
            }
            return tx;
        }

        // ---------------- Budget revisions ----------------

        public PagedInfo<PmoBudgetRevision> GetRevisions(PmoBudgetRevisionQueryDto parm, PmoActor actor)
        {
            var exp = Expressionable.Create<PmoBudgetRevision>();
            exp.AndIF(parm.ProjectId != null, r => r.ProjectId == parm.ProjectId);
            exp.AndIF(!string.IsNullOrEmpty(parm.Status), r => r.Status == parm.Status);
            return Context.Queryable<PmoBudgetRevision>().Where(exp.ToExpression())
                .OrderBy(r => r.RevisionId, OrderByType.Desc).ToPage(parm);
        }

        public PmoBudgetRevision AddRevision(PmoBudgetRevisionInputDto dto, PmoActor actor)
        {
            if (!Context.Queryable<PmoProject>().Any(p => p.ProjectId == dto.ProjectId))
                throw new CustomException(ResultCode.FAIL, "Project not found");
            if (string.IsNullOrWhiteSpace(dto.Reason))
                throw new CustomException(ResultCode.FAIL, "A rationale is required");

            var entity = new PmoBudgetRevision
            {
                ProjectId = dto.ProjectId,
                Amount = dto.Amount,
                Reason = dto.Reason,
                Status = "Pending",                 // has NO budget effect until approved
                RequestedByUserId = actor.UserId,
                Create_by = actor.UserName,
                Create_time = DateTime.Now
            };
            entity.RevisionId = Context.Insertable(entity).ExecuteReturnSnowflakeId();
            return entity;
        }

        /// <summary>Approve/Reject a revision. Atomic: decision + audit event in one transaction.</summary>
        public PmoBudgetRevision DecideRevision(PmoBudgetRevisionDecisionDto dto, PmoActor actor)
        {
            var rev = Context.Queryable<PmoBudgetRevision>().First(r => r.RevisionId == dto.RevisionId)
                ?? throw new CustomException(ResultCode.FAIL, "Revision not found");
            if (rev.Status != "Pending")
                throw new CustomException(ResultCode.FAIL, $"Revision already {rev.Status}");

            var now = DateTime.Now;
            var newStatus = dto.Approve ? "Approved" : "Rejected";
            try
            {
                Context.Ado.BeginTran();

                rev.Status = newStatus;
                rev.ApprovedByUserId = actor.UserId;
                rev.ApprovedAt = now;
                rev.Update_by = actor.UserName;
                rev.Update_time = now;
                Context.Updateable(rev).ExecuteCommand();

                var audit = new PmoAuditEvent
                {
                    EntityType = "BudgetRevision",
                    EntityId = rev.RevisionId,
                    Action = dto.Approve ? "RevisionApproved" : "RevisionRejected",
                    ActorUserId = actor.UserId,
                    Detail = $"{newStatus} revision of {rev.Amount:0.00}. {dto.Reason}",
                    ActedAt = now,
                    Create_by = actor.UserName,
                    Create_time = now
                };
                audit.AuditId = Context.Insertable(audit).ExecuteReturnSnowflakeId();

                Context.Ado.CommitTran();
            }
            catch
            {
                Context.Ado.RollbackTran();
                throw;
            }
            return rev;
        }

        // ---------------- Forecasts ----------------

        public List<PmoFinancialForecast> GetForecasts(long projectId) =>
            Context.Queryable<PmoFinancialForecast>().Where(f => f.ProjectId == projectId)
                .OrderBy(f => f.PeriodMonth).ToList();

        public PmoFinancialForecast UpsertForecast(PmoForecastInputDto dto, PmoActor actor)
        {
            var month = new DateTime(dto.PeriodMonth.Year, dto.PeriodMonth.Month, 1);
            var existing = Context.Queryable<PmoFinancialForecast>()
                .First(f => f.ProjectId == dto.ProjectId && f.PeriodMonth == month && f.Kind == dto.Kind);
            if (existing != null)
            {
                existing.ForecastAmount = dto.ForecastAmount;
                existing.Update_by = actor.UserName;
                existing.Update_time = DateTime.Now;
                Context.Updateable(existing).ExecuteCommand();
                return existing;
            }
            var entity = new PmoFinancialForecast
            {
                ProjectId = dto.ProjectId,
                PeriodMonth = month,
                ForecastAmount = dto.ForecastAmount,
                Kind = string.IsNullOrEmpty(dto.Kind) ? "ForecastToComplete" : dto.Kind,
                Create_by = actor.UserName,
                Create_time = DateTime.Now
            };
            entity.ForecastId = Context.Insertable(entity).ExecuteReturnSnowflakeId();
            return entity;
        }

        // ---------------- Drill-down ----------------

        public List<PmoCostTransaction> GetTransactionDrilldown(long? projectId, long? categoryId, string period, DateTime? cutoff)
        {
            var exp = Expressionable.Create<PmoCostTransaction>();
            exp.And(t => t.Status == "Posted");
            exp.AndIF(projectId != null, t => t.ProjectId == projectId);
            exp.AndIF(categoryId != null, t => t.CategoryId == categoryId);
            exp.AndIF(cutoff != null, t => t.TxnDate <= cutoff);
            var list = Context.Queryable<PmoCostTransaction>().Where(exp.ToExpression())
                .OrderBy(t => t.TxnDate, OrderByType.Desc).ToList();
            if (!string.IsNullOrEmpty(period))
                list = list.Where(t => t.TxnDate.ToString("yyyy-MM") == period).ToList();
            EnrichTransactions(list);
            return list;
        }

        private void EnrichTransactions(List<PmoCostTransaction> list)
        {
            if (list == null || list.Count == 0) return;
            var projIds = list.Select(t => t.ProjectId).Distinct().ToList();
            var catIds = list.Select(t => t.CategoryId).Distinct().ToList();
            var projNames = Context.Queryable<PmoProject>().Where(p => projIds.Contains(p.ProjectId))
                .ToList().ToDictionary(p => p.ProjectId, p => p.Name);
            var catNames = Context.Queryable<PmoCostCategory>().Where(c => catIds.Contains(c.CategoryId))
                .ToList().ToDictionary(c => c.CategoryId, c => c.Name);
            foreach (var t in list)
            {
                t.ProjectName = projNames.TryGetValue(t.ProjectId, out var pn) ? pn : null;
                t.CategoryName = catNames.TryGetValue(t.CategoryId, out var cn) ? cn : null;
            }
        }
    }

    internal static class PmoCostTxnExtensions
    {
        /// <summary>Currency is carried on the project; fall back to AED for the audit text.</summary>
        public static string Currency(this PmoCostTransaction _) => "AED";
    }
}
