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
    /// GL Period Closing Repository - مستودع إقفال الفترات
    /// </summary>
    [AppService(ServiceType = typeof(IGLPeriodClosingRepository), ServiceLifetime = LifeTime.Transient)]
    public class GLPeriodClosingRepository : BaseRepository<GLPeriodClosing>, IGLPeriodClosingRepository
    {
        /// <summary>
        /// Get paginated list of periods
        /// </summary>
        public PagedInfo<GLPeriodClosing> GetList(GLPeriodClosing parm, PagerInfo pager)
        {
            var predicate = Expressionable.Create<GLPeriodClosing>();
            predicate = predicate.AndIF(parm.FiscalYear > 0, it => it.FiscalYear == parm.FiscalYear);
            predicate = predicate.AndIF(parm.FiscalPeriod > 0, it => it.FiscalPeriod == parm.FiscalPeriod);
            predicate = predicate.AndIF(parm.CompanyID.HasValue, it => it.CompanyID == parm.CompanyID);

            return Queryable()
                .Where(predicate.ToExpression())
                .OrderBy(x => x.FiscalYear, OrderByType.Desc)
                .OrderBy(x => x.FiscalPeriod, OrderByType.Desc)
                .ToPage(pager);
        }

        /// <summary>
        /// Get period by ID
        /// </summary>
        public GLPeriodClosing GetById(Guid id)
        {
            return GetFirst(x => x.Id == id);
        }

        /// <summary>
        /// Get period by ID async
        /// </summary>
        public async Task<GLPeriodClosing> GetByIdAsync(Guid id)
        {
            return await Queryable().FirstAsync(x => x.Id == id);
        }

        /// <summary>
        /// Get periods by status
        /// </summary>
        public List<GLPeriodClosing> GetByStatus(PeriodStatus status, long? companyId = null)
        {
            var predicate = Expressionable.Create<GLPeriodClosing>();
            predicate = predicate.And(it => it.Status == status);
            predicate = predicate.AndIF(companyId.HasValue, it => it.CompanyID == companyId);

            return Queryable()
                .Where(predicate.ToExpression())
                .OrderBy(x => x.FiscalYear, OrderByType.Desc)
                .OrderBy(x => x.FiscalPeriod, OrderByType.Desc)
                .ToList();
        }

        /// <summary>
        /// Get periods by status async
        /// </summary>
        public async Task<List<GLPeriodClosing>> GetByStatusAsync(PeriodStatus status, long? companyId = null)
        {
            var predicate = Expressionable.Create<GLPeriodClosing>();
            predicate = predicate.And(it => it.Status == status);
            predicate = predicate.AndIF(companyId.HasValue, it => it.CompanyID == companyId);

            return await Queryable()
                .Where(predicate.ToExpression())
                .OrderBy(x => x.FiscalYear, OrderByType.Desc)
                .OrderBy(x => x.FiscalPeriod, OrderByType.Desc)
                .ToListAsync();
        }

        /// <summary>
        /// Get period by dates
        /// </summary>
        public GLPeriodClosing GetByDates(DateTime startDate, DateTime endDate, PeriodType periodType, long? companyId = null)
        {
            var predicate = Expressionable.Create<GLPeriodClosing>();
            predicate = predicate.And(it => it.PeriodStartDate == startDate.Date && it.PeriodEndDate == endDate.Date);
            predicate = predicate.And(it => it.PeriodType == periodType);
            predicate = predicate.AndIF(companyId.HasValue, it => it.CompanyID == companyId);

            return GetFirst(predicate.ToExpression());
        }

        /// <summary>
        /// Get period by dates async
        /// </summary>
        public async Task<GLPeriodClosing> GetByDatesAsync(DateTime startDate, DateTime endDate, PeriodType periodType, long? companyId = null)
        {
            var predicate = Expressionable.Create<GLPeriodClosing>();
            predicate = predicate.And(it => it.PeriodStartDate == startDate.Date && it.PeriodEndDate == endDate.Date);
            predicate = predicate.And(it => it.PeriodType == periodType);
            predicate = predicate.AndIF(companyId.HasValue, it => it.CompanyID == companyId);

            return await Queryable().FirstAsync(predicate.ToExpression());
        }

        /// <summary>
        /// Get periods by fiscal year
        /// </summary>
        public List<GLPeriodClosing> GetByFiscalYear(int fiscalYear, long? companyId = null)
        {
            var predicate = Expressionable.Create<GLPeriodClosing>();
            predicate = predicate.And(it => it.FiscalYear == fiscalYear);
            predicate = predicate.AndIF(companyId.HasValue, it => it.CompanyID == companyId);

            return Queryable()
                .Where(predicate.ToExpression())
                .OrderBy(x => x.FiscalPeriod)
                .ToList();
        }

        /// <summary>
        /// Get periods by fiscal year async
        /// </summary>
        public async Task<List<GLPeriodClosing>> GetByFiscalYearAsync(int fiscalYear, long? companyId = null)
        {
            var predicate = Expressionable.Create<GLPeriodClosing>();
            predicate = predicate.And(it => it.FiscalYear == fiscalYear);
            predicate = predicate.AndIF(companyId.HasValue, it => it.CompanyID == companyId);

            return await Queryable()
                .Where(predicate.ToExpression())
                .OrderBy(x => x.FiscalPeriod)
                .ToListAsync();
        }

        /// <summary>
        /// Get open periods
        /// </summary>
        public List<GLPeriodClosing> GetOpenPeriods(long? companyId = null)
        {
            return GetByStatus(PeriodStatus.Open, companyId);
        }

        /// <summary>
        /// Get open periods async
        /// </summary>
        public async Task<List<GLPeriodClosing>> GetOpenPeriodsAsync(long? companyId = null)
        {
            return await GetByStatusAsync(PeriodStatus.Open, companyId);
        }

        /// <summary>
        /// Create period
        /// </summary>
        public int CreatePeriod(GLPeriodClosing model)
        {
            return Insert(model);
        }

        /// <summary>
        /// Create period async
        /// </summary>
        public async Task<int> CreatePeriodAsync(GLPeriodClosing model)
        {
            return await Context.Insertable(model).ExecuteCommandAsync();
        }

        /// <summary>
        /// Update period
        /// </summary>
        public int UpdatePeriod(GLPeriodClosing model)
        {
            return Update(model, it => new
            {
                it.DescriptionAr,
                it.DescriptionEn,
                it.TotalVouchers,
                it.PostedVouchers,
                it.TotalDebit,
                it.TotalCredit,
                it.ClosingNotes,
                it.Update_by,
                it.Update_time
            }, f => f.Id == model.Id);
        }

        /// <summary>
        /// Update period async
        /// </summary>
        public async Task<int> UpdatePeriodAsync(GLPeriodClosing model)
        {
            return await Context.Updateable(model)
                .UpdateColumns(it => new
                {
                    it.DescriptionAr,
                    it.DescriptionEn,
                    it.TotalVouchers,
                    it.PostedVouchers,
                    it.TotalDebit,
                    it.TotalCredit,
                    it.ClosingNotes,
                    it.Update_by,
                    it.Update_time
                })
                .Where(f => f.Id == model.Id)
                .ExecuteCommandAsync();
        }

        /// <summary>
        /// Update period status
        /// </summary>
        public int UpdateStatus(Guid id, PeriodStatus status, Guid? closedBy = null)
        {
            var updateable = Context.Updateable<GLPeriodClosing>()
                .SetColumns(it => it.Status == status)
                .SetColumns(it => it.Update_time == DateTime.Now);

            if (status == PeriodStatus.Closed)
            {
                updateable = updateable
                    .SetColumns(it => it.ClosedAt == DateTime.Now)
                    .SetColumns(it => it.ClosedBy == closedBy);
            }
            else if (status == PeriodStatus.Open)
            {
                updateable = updateable
                    .SetColumns(it => it.ReopenedAt == DateTime.Now)
                    .SetColumns(it => it.ReopenedBy == closedBy);
            }

            return updateable.Where(f => f.Id == id).ExecuteCommand();
        }

        /// <summary>
        /// Update period status async
        /// </summary>
        public async Task<int> UpdateStatusAsync(Guid id, PeriodStatus status, Guid? closedBy = null)
        {
            var updateable = Context.Updateable<GLPeriodClosing>()
                .SetColumns(it => it.Status == status)
                .SetColumns(it => it.Update_time == DateTime.Now);

            if (status == PeriodStatus.Closed)
            {
                updateable = updateable
                    .SetColumns(it => it.ClosedAt == DateTime.Now)
                    .SetColumns(it => it.ClosedBy == closedBy);
            }
            else if (status == PeriodStatus.Open)
            {
                updateable = updateable
                    .SetColumns(it => it.ReopenedAt == DateTime.Now)
                    .SetColumns(it => it.ReopenedBy == closedBy);
            }

            return await updateable.Where(f => f.Id == id).ExecuteCommandAsync();
        }

        /// <summary>
        /// Delete period
        /// </summary>
        public int DeletePeriod(Guid id)
        {
            return Context.Deleteable<GLPeriodClosing>()
                .Where(it => it.Id == id)
                .ExecuteCommand();
        }

        /// <summary>
        /// Delete period async
        /// </summary>
        public async Task<int> DeletePeriodAsync(Guid id)
        {
            return await Context.Deleteable<GLPeriodClosing>()
                .Where(it => it.Id == id)
                .ExecuteCommandAsync();
        }

        /// <summary>
        /// Check if period exists
        /// </summary>
        public bool PeriodExists(DateTime startDate, DateTime endDate, PeriodType periodType, long? companyId = null, Guid? excludeId = null)
        {
            var predicate = Expressionable.Create<GLPeriodClosing>();
            predicate = predicate.And(it => it.PeriodStartDate == startDate.Date && it.PeriodEndDate == endDate.Date);
            predicate = predicate.And(it => it.PeriodType == periodType);
            predicate = predicate.AndIF(companyId.HasValue, it => it.CompanyID == companyId);
            predicate = predicate.AndIF(excludeId.HasValue, it => it.Id != excludeId.Value);

            return Any(predicate.ToExpression());
        }

        /// <summary>
        /// Check if period exists async
        /// </summary>
        public async Task<bool> PeriodExistsAsync(DateTime startDate, DateTime endDate, PeriodType periodType, long? companyId = null, Guid? excludeId = null)
        {
            var predicate = Expressionable.Create<GLPeriodClosing>();
            predicate = predicate.And(it => it.PeriodStartDate == startDate.Date && it.PeriodEndDate == endDate.Date);
            predicate = predicate.And(it => it.PeriodType == periodType);
            predicate = predicate.AndIF(companyId.HasValue, it => it.CompanyID == companyId);
            predicate = predicate.AndIF(excludeId.HasValue, it => it.Id != excludeId.Value);

            return await Queryable().AnyAsync(predicate.ToExpression());
        }

        /// <summary>
        /// Check if date is in closed period
        /// </summary>
        public bool IsDateInClosedPeriod(DateTime date, long? companyId = null)
        {
            var predicate = Expressionable.Create<GLPeriodClosing>();
            predicate = predicate.And(it => it.PeriodStartDate <= date.Date && it.PeriodEndDate >= date.Date);
            predicate = predicate.And(it => it.Status == PeriodStatus.Closed || it.Status == PeriodStatus.Locked);
            predicate = predicate.AndIF(companyId.HasValue, it => it.CompanyID == companyId);

            return Any(predicate.ToExpression());
        }

        /// <summary>
        /// Check if date is in closed period async
        /// </summary>
        public async Task<bool> IsDateInClosedPeriodAsync(DateTime date, long? companyId = null)
        {
            var predicate = Expressionable.Create<GLPeriodClosing>();
            predicate = predicate.And(it => it.PeriodStartDate <= date.Date && it.PeriodEndDate >= date.Date);
            predicate = predicate.And(it => it.Status == PeriodStatus.Closed || it.Status == PeriodStatus.Locked);
            predicate = predicate.AndIF(companyId.HasValue, it => it.CompanyID == companyId);

            return await Queryable().AnyAsync(predicate.ToExpression());
        }

        /// <summary>
        /// Get period containing a specific date
        /// Returns the period where date falls between start and end dates
        /// </summary>
        public GLPeriodClosing GetPeriodByDate(DateTime date, long? companyId = null)
        {
            var predicate = Expressionable.Create<GLPeriodClosing>();
            predicate = predicate.And(it => it.PeriodStartDate <= date.Date && it.PeriodEndDate >= date.Date);
            predicate = predicate.AndIF(companyId.HasValue, it => it.CompanyID == companyId);

            // Prioritize monthly periods over quarterly/yearly
            return Queryable()
                .Where(predicate.ToExpression())
                .OrderBy(x => x.PeriodType) // Month=0, Quarter=1, Year=2
                .First();
        }

        /// <summary>
        /// Get period containing a specific date async
        /// </summary>
        public async Task<GLPeriodClosing> GetPeriodByDateAsync(DateTime date, long? companyId = null)
        {
            var predicate = Expressionable.Create<GLPeriodClosing>();
            predicate = predicate.And(it => it.PeriodStartDate <= date.Date && it.PeriodEndDate >= date.Date);
            predicate = predicate.AndIF(companyId.HasValue, it => it.CompanyID == companyId);

            return await Queryable()
                .Where(predicate.ToExpression())
                .OrderBy(x => x.PeriodType)
                .FirstAsync();
        }
    }
}

