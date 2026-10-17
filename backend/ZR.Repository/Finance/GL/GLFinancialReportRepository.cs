using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Infrastructure.Attribute;
using SqlSugar;
using ZR.Model;
using ZR.Model.Finance.GL;
using ZR.Model.Finance.Enums;

namespace ZR.Repository.Finance.GL
{
    /// <summary>
    /// GL Financial Report Repository - مستودع التقارير المالية
    /// </summary>
    [AppService(ServiceType = typeof(IGLFinancialReportRepository), ServiceLifetime = LifeTime.Transient)]
    public class GLFinancialReportRepository : BaseRepository<GLFinancialReport>, IGLFinancialReportRepository
    {
        /// <summary>
        /// Get paginated list of reports
        /// </summary>
        public PagedInfo<GLFinancialReport> GetList(GLFinancialReport parm, PagerInfo pager)
        {
            var predicate = Expressionable.Create<GLFinancialReport>();
            predicate = predicate.AndIF(parm.FiscalYear > 0, it => it.FiscalYear == parm.FiscalYear);
            predicate = predicate.AndIF(parm.CompanyID.HasValue, it => it.CompanyID == parm.CompanyID);

            return Queryable()
                .Where(predicate.ToExpression())
                .OrderBy(x => x.GeneratedAt, OrderByType.Desc)
                .ToPage(pager);
        }

        /// <summary>
        /// Get report by ID
        /// </summary>
        public GLFinancialReport GetById(Guid id)
        {
            return GetFirst(x => x.Id == id);
        }

        /// <summary>
        /// Get report by ID async
        /// </summary>
        public async Task<GLFinancialReport> GetByIdAsync(Guid id)
        {
            return await Queryable().FirstAsync(x => x.Id == id);
        }

        /// <summary>
        /// Get reports by type
        /// </summary>
        public List<GLFinancialReport> GetByType(ReportType reportType, long? companyId = null)
        {
            var predicate = Expressionable.Create<GLFinancialReport>();
            predicate = predicate.And(it => it.ReportType == reportType);
            predicate = predicate.AndIF(companyId.HasValue, it => it.CompanyID == companyId);

            return Queryable()
                .Where(predicate.ToExpression())
                .OrderBy(x => x.GeneratedAt, OrderByType.Desc)
                .ToList();
        }

        /// <summary>
        /// Get reports by type async
        /// </summary>
        public async Task<List<GLFinancialReport>> GetByTypeAsync(ReportType reportType, long? companyId = null)
        {
            var predicate = Expressionable.Create<GLFinancialReport>();
            predicate = predicate.And(it => it.ReportType == reportType);
            predicate = predicate.AndIF(companyId.HasValue, it => it.CompanyID == companyId);

            return await Queryable()
                .Where(predicate.ToExpression())
                .OrderBy(x => x.GeneratedAt, OrderByType.Desc)
                .ToListAsync();
        }

        /// <summary>
        /// Get reports by period
        /// </summary>
        public List<GLFinancialReport> GetByPeriod(DateTime periodStart, DateTime periodEnd, long? companyId = null)
        {
            var predicate = Expressionable.Create<GLFinancialReport>();
            predicate = predicate.And(it => it.PeriodStartDate >= periodStart && it.PeriodEndDate <= periodEnd);
            predicate = predicate.AndIF(companyId.HasValue, it => it.CompanyID == companyId);

            return Queryable()
                .Where(predicate.ToExpression())
                .OrderBy(x => x.GeneratedAt, OrderByType.Desc)
                .ToList();
        }

        /// <summary>
        /// Get reports by period async
        /// </summary>
        public async Task<List<GLFinancialReport>> GetByPeriodAsync(DateTime periodStart, DateTime periodEnd, long? companyId = null)
        {
            var predicate = Expressionable.Create<GLFinancialReport>();
            predicate = predicate.And(it => it.PeriodStartDate >= periodStart && it.PeriodEndDate <= periodEnd);
            predicate = predicate.AndIF(companyId.HasValue, it => it.CompanyID == companyId);

            return await Queryable()
                .Where(predicate.ToExpression())
                .OrderBy(x => x.GeneratedAt, OrderByType.Desc)
                .ToListAsync();
        }

        /// <summary>
        /// Get reports by fiscal year
        /// </summary>
        public List<GLFinancialReport> GetByFiscalYear(int fiscalYear, long? companyId = null)
        {
            var predicate = Expressionable.Create<GLFinancialReport>();
            predicate = predicate.And(it => it.FiscalYear == fiscalYear);
            predicate = predicate.AndIF(companyId.HasValue, it => it.CompanyID == companyId);

            return Queryable()
                .Where(predicate.ToExpression())
                .OrderBy(x => x.GeneratedAt, OrderByType.Desc)
                .ToList();
        }

        /// <summary>
        /// Create report
        /// </summary>
        public int CreateReport(GLFinancialReport model)
        {
            return Insert(model);
        }

        /// <summary>
        /// Create report async
        /// </summary>
        public async Task<int> CreateReportAsync(GLFinancialReport model)
        {
            return await Context.Insertable(model).ExecuteCommandAsync();
        }

        /// <summary>
        /// Update report
        /// </summary>
        public int UpdateReport(GLFinancialReport model)
        {
            return Update(model, it => new
            {
                it.DescriptionAr,
                it.DescriptionEn,
                it.ReportData,
                it.TotalDebit,
                it.TotalCredit,
                it.Update_time
            }, f => f.Id == model.Id);
        }

        /// <summary>
        /// Update report async
        /// </summary>
        public async Task<int> UpdateReportAsync(GLFinancialReport model)
        {
            return await Context.Updateable(model)
                .UpdateColumns(it => new
                {
                    it.DescriptionAr,
                    it.DescriptionEn,
                    it.ReportData,
                    it.TotalDebit,
                    it.TotalCredit,
                    it.Update_time
                })
                .Where(f => f.Id == model.Id)
                .ExecuteCommandAsync();
        }

        /// <summary>
        /// Delete report
        /// </summary>
        public int DeleteReport(Guid id)
        {
            return Context.Deleteable<GLFinancialReport>()
                .Where(it => it.Id == id)
                .ExecuteCommand();
        }

        /// <summary>
        /// Delete report async
        /// </summary>
        public async Task<int> DeleteReportAsync(Guid id)
        {
            return await Context.Deleteable<GLFinancialReport>()
                .Where(it => it.Id == id)
                .ExecuteCommandAsync();
        }

        /// <summary>
        /// Generate report number
        /// </summary>
        public string GenerateReportNumber(ReportType reportType, long? companyId = null, int? fiscalYear = null)
        {
            var year = fiscalYear ?? DateTime.Now.Year;
            var typePrefix = GetReportTypePrefix(reportType);
            var prefix = $"{typePrefix}-{year}-";

            var predicate = Expressionable.Create<GLFinancialReport>();
            predicate = predicate.And(it => it.ReportNumber.StartsWith(prefix));
            predicate = predicate.AndIF(companyId.HasValue, it => it.CompanyID == companyId);

            var lastReport = Queryable()
                .Where(predicate.ToExpression())
                .OrderBy(x => x.ReportNumber, OrderByType.Desc)
                .First();

            int nextNumber = 1;
            if (lastReport != null && !string.IsNullOrEmpty(lastReport.ReportNumber))
            {
                var lastNumberStr = lastReport.ReportNumber.Replace(prefix, "");
                if (int.TryParse(lastNumberStr, out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            return $"{prefix}{nextNumber:D4}";
        }

        /// <summary>
        /// Generate report number async
        /// </summary>
        public async Task<string> GenerateReportNumberAsync(ReportType reportType, long? companyId = null, int? fiscalYear = null)
        {
            var year = fiscalYear ?? DateTime.Now.Year;
            var typePrefix = GetReportTypePrefix(reportType);
            var prefix = $"{typePrefix}-{year}-";

            var predicate = Expressionable.Create<GLFinancialReport>();
            predicate = predicate.And(it => it.ReportNumber.StartsWith(prefix));
            predicate = predicate.AndIF(companyId.HasValue, it => it.CompanyID == companyId);

            var lastReport = await Queryable()
                .Where(predicate.ToExpression())
                .OrderBy(x => x.ReportNumber, OrderByType.Desc)
                .FirstAsync();

            int nextNumber = 1;
            if (lastReport != null && !string.IsNullOrEmpty(lastReport.ReportNumber))
            {
                var lastNumberStr = lastReport.ReportNumber.Replace(prefix, "");
                if (int.TryParse(lastNumberStr, out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            return $"{prefix}{nextNumber:D4}";
        }

        private static string GetReportTypePrefix(ReportType reportType) => reportType switch
        {
            ReportType.TrialBalance => "TB",
            ReportType.BalanceSheet => "BS",
            ReportType.IncomeStatement => "IS",
            ReportType.CashFlow => "CF",
            ReportType.GeneralLedger => "GL",
            ReportType.AccountStatement => "AS",
            _ => "RPT"
        };
    }
}

