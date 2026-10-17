using System;
using System.IO;
using System.Runtime.CompilerServices;
using FluentAssertions;
using SqlSugar;
using Xunit;
using ZR.Model.PMO;
using ZR.Service.PMO;

namespace ZR.Service.Tests.PMO
{
    /// <summary>
    /// Financial-calculation tests exercising the REAL PmoFinanceService against an in-memory
    /// SQLite database (no external server needed). Validates the documented definitions:
    ///   approved budget = initial + Σ approved revisions   (pending/rejected excluded)
    ///   actual spend    = Σ posted txns with date ≤ cutoff  (draft + post-cutoff excluded)
    ///   EAC = actual + Σ forecast-to-complete; variance = budget − EAC; util = actual/budget
    /// The service is built with GetUninitializedObject so we can point its Context at the test
    /// database instead of the app's global SqlSugar scope.
    /// </summary>
    public class PmoFinanceCalcTests : IDisposable
    {
        private readonly string _dbPath;
        private readonly SqlSugarScope _db;
        private readonly PmoFinanceService _svc;

        // Inclusive end-of-day cutoff: "on or before the reporting cutoff" covers the whole cutoff
        // day. (Using midnight would wrongly drop a same-day txn under SQLite's text-date comparison;
        // the production Postgres path treats the date inclusively.)
        private static readonly DateTime Cutoff = new(2026, 9, 30, 23, 59, 59);

        public PmoFinanceCalcTests()
        {
            _dbPath = Path.Combine(Path.GetTempPath(), $"pmo_test_{Guid.NewGuid():N}.db");
            _db = new SqlSugarScope(new ConnectionConfig
            {
                DbType = DbType.Sqlite,
                ConnectionString = $"DataSource={_dbPath}",
                IsAutoCloseConnection = true,
                InitKeyType = InitKeyType.Attribute,
            });
            _db.CodeFirst.InitTables(typeof(PmoProject), typeof(PmoBudgetRevision),
                typeof(PmoCostTransaction), typeof(PmoFinancialForecast), typeof(PmoCostCategory));

            Seed();

            _svc = (PmoFinanceService)RuntimeHelpers.GetUninitializedObject(typeof(PmoFinanceService));
            _svc.Context = _db; // SimpleClient<T>.Context — all Queryable<T> calls use this
        }

        private void Seed()
        {
            // P1: budget 1,000,000 + 200,000 approved (+100k pending, +50k rejected → ignored) = 1,200,000
            _db.Insertable(new PmoProject { ProjectId = 1, Code = "P1", Name = "P1", Status = "Active",
                Priority = "High", InitialApprovedBudget = 1_000_000m, Currency = "AED" }).ExecuteCommand();
            _db.Insertable(new[]
            {
                new PmoBudgetRevision { RevisionId = 11, ProjectId = 1, Amount = 200_000m, Status = "Approved", Reason = "approved" },
                new PmoBudgetRevision { RevisionId = 12, ProjectId = 1, Amount = 100_000m, Status = "Pending", Reason = "pending" },
                new PmoBudgetRevision { RevisionId = 13, ProjectId = 1, Amount = 50_000m, Status = "Rejected", Reason = "rejected" },
            }).ExecuteCommand();
            _db.Insertable(new[]
            {
                // posted 300k (in range) + 200k (exactly on cutoff → included) = 500k actual
                new PmoCostTransaction { TransactionId = 101, ProjectId = 1, CategoryId = 1, Amount = 300_000m, TxnDate = new DateTime(2026,5,1), Status = "Posted", Description = "a" },
                new PmoCostTransaction { TransactionId = 102, ProjectId = 1, CategoryId = 1, Amount = 200_000m, TxnDate = new DateTime(2026,9,30,12,0,0), Status = "Posted", Description = "b" },
                // posted AFTER cutoff → excluded at baseline
                new PmoCostTransaction { TransactionId = 103, ProjectId = 1, CategoryId = 1, Amount = 100_000m, TxnDate = new DateTime(2026,10,15), Status = "Posted", Description = "c" },
                // draft → excluded
                new PmoCostTransaction { TransactionId = 104, ProjectId = 1, CategoryId = 1, Amount = 500_000m, TxnDate = new DateTime(2026,6,1), Status = "Draft", Description = "d" },
            }).ExecuteCommand();
            // forecast-to-complete 400k → EAC = 500k + 400k = 900k
            _db.Insertable(new PmoFinancialForecast { ForecastId = 201, ProjectId = 1, PeriodMonth = new DateTime(2026,10,1), ForecastAmount = 400_000m, Kind = "ForecastToComplete" }).ExecuteCommand();

            // P2: zero budget, no revisions → utilization must be null (no divide-by-zero)
            _db.Insertable(new PmoProject { ProjectId = 2, Code = "P2", Name = "P2", Status = "Planning",
                Priority = "Low", InitialApprovedBudget = 0m, Currency = "AED" }).ExecuteCommand();

            // P3: spend but NO forecast → EAC falls back to actual, HasForecast false
            _db.Insertable(new PmoProject { ProjectId = 3, Code = "P3", Name = "P3", Status = "Active",
                Priority = "Medium", InitialApprovedBudget = 500_000m, Currency = "AED" }).ExecuteCommand();
            _db.Insertable(new PmoCostTransaction { TransactionId = 301, ProjectId = 3, CategoryId = 1, Amount = 120_000m, TxnDate = new DateTime(2026,7,1), Status = "Posted", Description = "e" }).ExecuteCommand();
        }

        [Fact]
        public void Current_approved_budget_counts_only_approved_revisions()
            => _svc.GetCurrentApprovedBudget(1).Should().Be(1_200_000m);

        [Fact]
        public void Actual_spend_excludes_draft_and_post_cutoff()
            => _svc.GetActualSpend(1, Cutoff).Should().Be(500_000m);

        [Fact]
        public void Summary_computes_remaining_eac_and_variance()
        {
            var s = _svc.GetProjectSummary(1, Cutoff, null);
            s.CurrentApprovedBudget.Should().Be(1_200_000m);
            s.ActualSpend.Should().Be(500_000m);
            s.RemainingBudget.Should().Be(700_000m);
            s.Eac.Should().Be(900_000m);                 // actual + FTC
            s.ProjectedVariance.Should().Be(300_000m);   // budget − EAC (under budget)
            s.IsProjectedOverrun.Should().BeFalse();
            s.UtilizationPct.Should().Be(41.7m);         // 500k / 1.2m
            s.HasForecast.Should().BeTrue();
        }

        [Fact]
        public void Zero_budget_project_reports_null_utilization()
        {
            var s = _svc.GetProjectSummary(2, Cutoff, null);
            s.CurrentApprovedBudget.Should().Be(0m);
            s.UtilizationPct.Should().BeNull();          // no divide-by-zero
        }

        [Fact]
        public void Project_without_forecast_sets_eac_to_actual()
        {
            var s = _svc.GetProjectSummary(3, Cutoff, null);
            s.HasForecast.Should().BeFalse();
            s.Eac.Should().Be(s.ActualSpend);            // 120,000
            s.Eac.Should().Be(120_000m);
        }

        public void Dispose()
        {
            _db?.Dispose();
            try { if (File.Exists(_dbPath)) File.Delete(_dbPath); } catch { /* temp file */ }
        }
    }
}
