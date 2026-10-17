using System;
using System.Collections.Generic;
using System.Linq;
using Infrastructure.Attribute;
using SqlSugar;
using ZR.Model;
using ZR.Model.Finance.Budget;

namespace ZR.Repository.Finance.Budget
{
    /// <summary>
    /// Budget Header Repository - مستودع رأس الميزانية
    /// </summary>
    [AppService(ServiceType = typeof(IBudgetHeaderRepository), ServiceLifetime = LifeTime.Transient)]
    public class BudgetHeaderRepository : BaseRepository<BudgetHeader>, IBudgetHeaderRepository
    {
        /// <summary>
        /// Get paginated list of budget headers
        /// </summary>
        public PagedInfo<BudgetHeader> GetList(BudgetHeader parm, PagerInfo pager)
        {
            var predicate = Expressionable.Create<BudgetHeader>();

            // Company isolation - filter by CompanyID if provided
            predicate = predicate.AndIF(parm.CompanyID.HasValue,
                it => it.CompanyID == parm.CompanyID);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.BudgetCode),
                it => it.BudgetCode.Contains(parm.BudgetCode));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Name),
                it => it.Name.Contains(parm.Name));
            predicate = predicate.AndIF(parm.FiscalYear > 0,
                it => it.FiscalYear == parm.FiscalYear);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Type),
                it => it.Type == parm.Type);
            predicate = predicate.AndIF(parm.ProjectId.HasValue,
                it => it.ProjectId == parm.ProjectId);
            predicate = predicate.AndIF(parm.DepartmentId.HasValue,
                it => it.DepartmentId == parm.DepartmentId);
            predicate = predicate.AndIF(parm.CostCenterId.HasValue,
                it => it.CostCenterId == parm.CostCenterId);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Status),
                it => it.Status == parm.Status);

            return Queryable()
                .Where(predicate.ToExpression())
                .OrderByDescending(x => x.CreatedAt)
                .ToPage(pager);
        }

        /// <summary>
        /// Get budget header by ID
        /// </summary>
        public BudgetHeader GetById(Guid budgetId)
        {
            return GetFirst(x => x.Id == budgetId);
        }

        /// <summary>
        /// Get budget header by code with company isolation
        /// </summary>
        public BudgetHeader GetByCode(string budgetCode, long? companyId = null)
        {
            var predicate = Expressionable.Create<BudgetHeader>();
            predicate = predicate.And(it => it.BudgetCode == budgetCode);
            predicate = predicate.AndIF(companyId.HasValue, it => it.CompanyID == companyId);
            return GetFirst(predicate.ToExpression());
        }

        /// <summary>
        /// Check if budget code exists with company isolation
        /// </summary>
        public bool CodeExists(string code, long? companyId = null, Guid? excludeId = null)
        {
            var predicate = Expressionable.Create<BudgetHeader>();
            predicate = predicate.And(it => it.BudgetCode == code);
            predicate = predicate.AndIF(companyId.HasValue, it => it.CompanyID == companyId);
            predicate = predicate.AndIF(excludeId.HasValue, it => it.Id != excludeId.Value);

            return Any(predicate.ToExpression());
        }

        /// <summary>
        /// Get active budgets with company isolation
        /// </summary>
        public List<BudgetHeader> GetActiveBudgets(long? companyId = null)
        {
            var predicate = Expressionable.Create<BudgetHeader>();
            predicate = predicate.And(it => it.IsActive);
            predicate = predicate.AndIF(companyId.HasValue, it => it.CompanyID == companyId);

            return Queryable()
                .Where(predicate.ToExpression())
                .OrderByDescending(x => x.FiscalYear)
                .OrderBy(x => x.BudgetCode)
                .ToList();
        }

        /// <summary>
        /// Get budgets by fiscal year with company isolation
        /// </summary>
        public List<BudgetHeader> GetByFiscalYear(int fiscalYear, long? companyId = null)
        {
            var predicate = Expressionable.Create<BudgetHeader>();
            predicate = predicate.And(it => it.FiscalYear == fiscalYear);
            predicate = predicate.AndIF(companyId.HasValue, it => it.CompanyID == companyId);

            return Queryable()
                .Where(predicate.ToExpression())
                .OrderBy(x => x.BudgetCode)
                .ToList();
        }

        /// <summary>
        /// Get budgets by cost center with company isolation
        /// </summary>
        public List<BudgetHeader> GetByCostCenter(long costCenterId, long? companyId = null)
        {
            var predicate = Expressionable.Create<BudgetHeader>();
            predicate = predicate.And(it => it.CostCenterId == costCenterId && it.IsActive);
            predicate = predicate.AndIF(companyId.HasValue, it => it.CompanyID == companyId);

            return Queryable()
                .Where(predicate.ToExpression())
                .OrderByDescending(x => x.FiscalYear)
                .ToList();
        }

        /// <summary>
        /// Get budgets by project with company isolation
        /// </summary>
        public List<BudgetHeader> GetByProject(Guid projectId, long? companyId = null)
        {
            var predicate = Expressionable.Create<BudgetHeader>();
            predicate = predicate.And(it => it.ProjectId == projectId && it.IsActive);
            predicate = predicate.AndIF(companyId.HasValue, it => it.CompanyID == companyId);

            return Queryable()
                .Where(predicate.ToExpression())
                .OrderByDescending(x => x.FiscalYear)
                .ToList();
        }

        /// <summary>
        /// Get budgets by department with company isolation
        /// </summary>
        public List<BudgetHeader> GetByDepartment(long departmentId, long? companyId = null)
        {
            var predicate = Expressionable.Create<BudgetHeader>();
            predicate = predicate.And(it => it.DepartmentId == departmentId && it.IsActive);
            predicate = predicate.AndIF(companyId.HasValue, it => it.CompanyID == companyId);

            return Queryable()
                .Where(predicate.ToExpression())
                .OrderByDescending(x => x.FiscalYear)
                .ToList();
        }

        /// <summary>
        /// Get budgets by type with company isolation
        /// </summary>
        public List<BudgetHeader> GetByType(string budgetType, long? companyId = null)
        {
            var predicate = Expressionable.Create<BudgetHeader>();
            predicate = predicate.And(it => it.Type == budgetType && it.IsActive);
            predicate = predicate.AndIF(companyId.HasValue, it => it.CompanyID == companyId);

            return Queryable()
                .Where(predicate.ToExpression())
                .OrderByDescending(x => x.FiscalYear)
                .OrderBy(x => x.BudgetCode)
                .ToList();
        }

        /// <summary>
        /// Approve budget
        /// </summary>
        public int ApproveBudget(Guid budgetId, Guid approvedBy)
        {
            return Context.Updateable<BudgetHeader>()
                .SetColumns(it => new BudgetHeader
                {
                    Status = "Approved",
                    ApprovedBy = approvedBy,
                    ApprovedAt = DateTime.Now,
                    Update_time = DateTime.Now
                })
                .Where(it => it.Id == budgetId)
                .ExecuteCommand();
        }

        /// <summary>
        /// Close budget
        /// </summary>
        public int CloseBudget(Guid budgetId, Guid closedBy)
        {
            return Context.Updateable<BudgetHeader>()
                .SetColumns(it => new BudgetHeader
                {
                    Status = "Closed",
                    ClosedBy = closedBy,
                    ClosedAt = DateTime.Now,
                    Update_time = DateTime.Now
                })
                .Where(it => it.Id == budgetId)
                .ExecuteCommand();
        }

        /// <summary>
        /// Get budgets by status with company isolation
        /// </summary>
        public List<BudgetHeader> GetByStatus(string status, long? companyId = null)
        {
            var predicate = Expressionable.Create<BudgetHeader>();
            predicate = predicate.And(it => it.Status == status && it.IsActive);
            predicate = predicate.AndIF(companyId.HasValue, it => it.CompanyID == companyId);

            return Queryable()
                .Where(predicate.ToExpression())
                .OrderByDescending(x => x.FiscalYear)
                .OrderBy(x => x.BudgetCode)
                .ToList();
        }

        /// <summary>
        /// Get next budget code with company isolation
        /// </summary>
        public string GetNextCode(string prefix, long? companyId = null)
        {
            var predicate = Expressionable.Create<BudgetHeader>();
            predicate = predicate.And(it => it.BudgetCode.StartsWith(prefix));
            predicate = predicate.AndIF(companyId.HasValue, it => it.CompanyID == companyId);

            var maxCode = Queryable()
                .Where(predicate.ToExpression())
                .OrderByDescending(x => x.BudgetCode)
                .Select(x => x.BudgetCode)
                .First();

            if (string.IsNullOrEmpty(maxCode))
            {
                return $"{prefix}001";
            }

            // Extract number part and increment
            var numPart = maxCode.Substring(prefix.Length);
            if (int.TryParse(numPart, out int num))
            {
                return $"{prefix}{(num + 1):D3}";
            }

            return $"{prefix}001";
        }
    }
}
