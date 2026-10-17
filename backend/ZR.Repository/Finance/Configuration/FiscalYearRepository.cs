using System;
using System.Collections.Generic;
using System.Linq;
using Infrastructure.Attribute;
using SqlSugar;
using ZR.Model;
using ZR.Model.Finance.Configuration;
using ZR.Model.Finance.Configuration.Enums;

namespace ZR.Repository.Finance.Configuration
{
    /// <summary>
    /// Fiscal Year Repository - مستودع السنوات المالية
    /// </summary>
    [AppService(ServiceType = typeof(IFiscalYearRepository), ServiceLifetime = LifeTime.Transient)]
    public class FiscalYearRepository : BaseRepository<FiscalYear>, IFiscalYearRepository
    {
        /// <summary>
        /// Get paginated list of fiscal years
        /// </summary>
        public PagedInfo<FiscalYear> GetList(FiscalYear parm, PagerInfo pager)
        {
            var predicate = Expressionable.Create<FiscalYear>();
            predicate = predicate.AndIF(parm.CompanyID.HasValue, it => it.CompanyID == parm.CompanyID);
            predicate = predicate.AndIF(parm.CompanyID.HasValue, it => it.CompanyID == parm.CompanyID);

            return Queryable()
                .Where(predicate.ToExpression())
                .OrderByDescending(x => x.YearStartDate)
                .ToPage(pager);
        }

        /// <summary>
        /// Get fiscal year by ID
        /// </summary>
        public FiscalYear GetById(Guid id)
        {
            return GetFirst(x => x.Id == id);
        }

        /// <summary>
        /// Get open fiscal year for company
        /// </summary>
        public FiscalYear GetOpenFiscalYear(long? companyId)
        {
            var predicate = Expressionable.Create<FiscalYear>();
            predicate = predicate.And(it => it.Status == FiscalYearStatus.Open);
            predicate = predicate.AndIF(companyId.HasValue, it => it.CompanyID == companyId);

            return GetFirst(predicate.ToExpression());
        }

        /// <summary>
        /// Check for overlapping fiscal years
        /// </summary>
        public bool HasOverlappingPeriod(long? companyId, DateTime startDate, DateTime endDate, Guid? excludeId = null)
        {
            var predicate = Expressionable.Create<FiscalYear>();
            predicate = predicate.AndIF(companyId.HasValue, it => it.CompanyID == companyId);
            predicate = predicate.AndIF(excludeId.HasValue, it => it.Id != excludeId.Value);

            // Check for overlap: (StartDate <= endDate) AND (EndDate >= startDate)
            predicate = predicate.And(it => it.YearStartDate <= endDate && it.YearEndDate >= startDate);

            return Any(predicate.ToExpression());
        }

        /// <summary>
        /// Get fiscal year containing a specific date
        /// </summary>
        public FiscalYear GetFiscalYearByDate(long? companyId, DateTime date)
        {
            var predicate = Expressionable.Create<FiscalYear>();
            predicate = predicate.AndIF(companyId.HasValue, it => it.CompanyID == companyId);
            predicate = predicate.And(it => it.YearStartDate <= date && it.YearEndDate >= date);

            return GetFirst(predicate.ToExpression());
        }

        /// <summary>
        /// Get all fiscal years for a company ordered by start date
        /// </summary>
        public List<FiscalYear> GetAllFiscalYears(long? companyId)
        {
            var predicate = Expressionable.Create<FiscalYear>();
            predicate = predicate.AndIF(companyId.HasValue, it => it.CompanyID == companyId);

            return Queryable()
                .Where(predicate.ToExpression())
                .OrderByDescending(x => x.YearStartDate)
                .ToList();
        }

        /// <summary>
        /// Get earliest fiscal year for a company
        /// </summary>
        public FiscalYear GetEarliestFiscalYear(long? companyId)
        {
            var predicate = Expressionable.Create<FiscalYear>();
            predicate = predicate.AndIF(companyId.HasValue, it => it.CompanyID == companyId);

            return Queryable()
                .Where(predicate.ToExpression())
                .OrderBy(x => x.YearStartDate)
                .First();
        }
    }
}


