using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.Attribute;
using SqlSugar;
using ZR.Model;
using ZR.Model.Finance.Tax;

namespace ZR.Repository.Finance.Tax
{
    /// <summary>
    /// Tax Rate Repository Implementation
    /// تنفيذ مستودع معدلات الضرائب
    /// </summary>
    [AppService(ServiceType = typeof(ITaxRateRepository), ServiceLifetime = LifeTime.Transient)]
    public class TaxRateRepository : BaseRepository<TaxRate>, ITaxRateRepository
    {
        #region CRUD Operations

        /// <summary>
        /// Get paginated list of tax rates
        /// </summary>
        public PagedInfo<TaxRate> GetList(TaxRate parm, PagerInfo pager)
        {
            var predicate = Expressionable.Create<TaxRate>();

            // Filter by tax code
            predicate = predicate.AndIF(parm.TaxCodeId != Guid.Empty, it => it.TaxCodeId == parm.TaxCodeId);

            // Filter by effective date
            if (parm.EffectiveFrom != default)
            {
                predicate = predicate.And(it => it.EffectiveFrom <= parm.EffectiveFrom);
                predicate = predicate.And(it => it.EffectiveTo == null || it.EffectiveTo >= parm.EffectiveFrom);
            }

            // Filter by approval status
            predicate = predicate.AndIF(parm.IsApproved, it => it.IsApproved == parm.IsApproved);

            return Queryable()
                .Where(predicate.ToExpression())
                .OrderByDescending(x => x.EffectiveFrom)
                .OrderBy(x => x.TaxCodeId)
                .ToPage(pager);
        }

        /// <summary>
        /// Get tax rate by ID
        /// </summary>
        public TaxRate GetById(Guid id)
        {
            return GetFirst(x => x.Id == id);
        }

        /// <summary>
        /// Get tax rate by ID (async)
        /// </summary>
        public async Task<TaxRate> GetByIdAsync(Guid id)
        {
            return await GetFirstAsync(x => x.Id == id);
        }

        /// <summary>
        /// Insert tax rate
        /// </summary>
        public int InsertTaxRate(TaxRate model)
        {
            return Insert(model);
        }

        /// <summary>
        /// Insert tax rate (async)
        /// </summary>
        public async Task<int> InsertTaxRateAsync(TaxRate model)
        {
            var result = await InsertAsync(model);
            return result ? 1 : 0;
        }

        /// <summary>
        /// Update tax rate
        /// </summary>
        public int UpdateTaxRate(TaxRate model)
        {
            return Context.Updateable(model)
                .IgnoreColumns(it => new { it.Create_by, it.Create_time })
                .ExecuteCommand();
        }

        /// <summary>
        /// Update tax rate (async)
        /// </summary>
        public async Task<int> UpdateTaxRateAsync(TaxRate model)
        {
            return await Context.Updateable(model)
                .IgnoreColumns(it => new { it.Create_by, it.Create_time })
                .ExecuteCommandAsync();
        }

        /// <summary>
        /// Delete tax rates by IDs
        /// </summary>
        public int DeleteTaxRates(Guid[] ids)
        {
            return Context.Deleteable<TaxRate>()
                .Where(it => ids.Contains(it.Id))
                .ExecuteCommand();
        }

        /// <summary>
        /// Delete tax rates by IDs (async)
        /// </summary>
        public async Task<int> DeleteTaxRatesAsync(Guid[] ids)
        {
            return await Context.Deleteable<TaxRate>()
                .Where(it => ids.Contains(it.Id))
                .ExecuteCommandAsync();
        }

        #endregion

        #region Effective Date Operations

        /// <summary>
        /// Get effective rate for a tax code on a specific date
        /// This is the CRITICAL method for tax calculation
        /// Returns the rate where effective_from <= date AND (effective_to IS NULL OR effective_to >= date)
        /// </summary>
        public TaxRate GetEffectiveRate(Guid taxCodeId, DateTime date)
        {
            return Queryable()
                .Where(x => x.TaxCodeId == taxCodeId)
                .Where(x => x.IsApproved)
                .Where(x => x.EffectiveFrom <= date)
                .Where(x => x.EffectiveTo == null || x.EffectiveTo >= date)
                .OrderByDescending(x => x.EffectiveFrom)
                .First();
        }

        /// <summary>
        /// Get effective rate for a tax code on a specific date (async)
        /// </summary>
        public async Task<TaxRate> GetEffectiveRateAsync(Guid taxCodeId, DateTime date)
        {
            return await Context.Queryable<TaxRate>()
                .Where(x => x.TaxCodeId == taxCodeId)
                .Where(x => x.IsApproved)
                .Where(x => x.EffectiveFrom <= date)
                .Where(x => x.EffectiveTo == null || x.EffectiveTo >= date)
                .OrderByDescending(x => x.EffectiveFrom)
                .FirstAsync();
        }

        /// <summary>
        /// Get all rates for a tax code
        /// </summary>
        public List<TaxRate> GetRatesByTaxCode(Guid taxCodeId)
        {
            return Queryable()
                .Where(x => x.TaxCodeId == taxCodeId)
                .OrderByDescending(x => x.EffectiveFrom)
                .ToList();
        }

        /// <summary>
        /// Get all rates for a tax code (async)
        /// </summary>
        public async Task<List<TaxRate>> GetRatesByTaxCodeAsync(Guid taxCodeId)
        {
            return await Context.Queryable<TaxRate>()
                .Where(x => x.TaxCodeId == taxCodeId)
                .OrderByDescending(x => x.EffectiveFrom)
                .ToListAsync();
        }

        /// <summary>
        /// Get currently effective rates (for all tax codes)
        /// </summary>
        public List<TaxRate> GetCurrentlyEffectiveRates()
        {
            var today = DateTime.Today;
            return Queryable()
                .Where(x => x.IsApproved)
                .Where(x => x.EffectiveFrom <= today)
                .Where(x => x.EffectiveTo == null || x.EffectiveTo >= today)
                .OrderBy(x => x.TaxCodeId)
                .OrderByDescending(x => x.EffectiveFrom)
                .ToList();
        }

        /// <summary>
        /// Get future rates (not yet effective)
        /// </summary>
        public List<TaxRate> GetFutureRates()
        {
            var today = DateTime.Today;
            return Queryable()
                .Where(x => x.EffectiveFrom > today)
                .OrderBy(x => x.EffectiveFrom)
                .OrderBy(x => x.TaxCodeId)
                .ToList();
        }

        /// <summary>
        /// Get expired rates (past effective_to date)
        /// </summary>
        public List<TaxRate> GetExpiredRates()
        {
            var today = DateTime.Today;
            return Queryable()
                .Where(x => x.EffectiveTo != null && x.EffectiveTo < today)
                .OrderByDescending(x => x.EffectiveTo)
                .OrderBy(x => x.TaxCodeId)
                .ToList();
        }

        #endregion

        #region Validation Operations

        /// <summary>
        /// Check if date range overlaps with existing rates for the same tax code
        /// This prevents conflicting rate definitions
        /// </summary>
        public bool HasOverlappingRates(Guid taxCodeId, DateTime effectiveFrom, DateTime? effectiveTo, Guid? excludeId = null)
        {
            var query = Queryable()
                .Where(x => x.TaxCodeId == taxCodeId);

            if (excludeId.HasValue)
            {
                query = query.Where(x => x.Id != excludeId.Value);
            }

            // Overlap logic:
            // New range [A, B] overlaps with existing [C, D] if:
            // A <= D AND B >= C (considering nulls as infinity)
            return query.Any(x =>
                (effectiveTo == null || x.EffectiveFrom <= effectiveTo) &&
                (x.EffectiveTo == null || x.EffectiveTo >= effectiveFrom));
        }

        /// <summary>
        /// Check if date range overlaps with existing rates (async)
        /// </summary>
        public async Task<bool> HasOverlappingRatesAsync(Guid taxCodeId, DateTime effectiveFrom, DateTime? effectiveTo, Guid? excludeId = null)
        {
            var query = Context.Queryable<TaxRate>()
                .Where(x => x.TaxCodeId == taxCodeId);

            if (excludeId.HasValue)
            {
                query = query.Where(x => x.Id != excludeId.Value);
            }

            return await query.AnyAsync(x =>
                (effectiveTo == null || x.EffectiveFrom <= effectiveTo) &&
                (x.EffectiveTo == null || x.EffectiveTo >= effectiveFrom));
        }

        /// <summary>
        /// Check if tax rate can be deleted (not used in any posted tax journal lines)
        /// </summary>
        public bool CanDelete(Guid id)
        {
            // Check if this rate was used in any tax calculations
            // A rate can be deleted if it hasn't been used or if related journal lines are not posted
            var rate = GetById(id);
            if (rate == null) return false;

            // Check if any tax journal lines reference this tax code and fall within this rate's effective period
            return !Context.Queryable<TaxJournalLine>()
                .Where(x => x.TaxCodeId == rate.TaxCodeId)
                .Where(x => x.IsPosted)
                .Where(x => x.TaxDate >= rate.EffectiveFrom)
                .Where(x => rate.EffectiveTo == null || x.TaxDate <= rate.EffectiveTo)
                .Any();
        }

        /// <summary>
        /// Check if tax rate can be deleted (async)
        /// </summary>
        public async Task<bool> CanDeleteAsync(Guid id)
        {
            var rate = await GetByIdAsync(id);
            if (rate == null) return false;

            return !await Context.Queryable<TaxJournalLine>()
                .Where(x => x.TaxCodeId == rate.TaxCodeId)
                .Where(x => x.IsPosted)
                .Where(x => x.TaxDate >= rate.EffectiveFrom)
                .Where(x => rate.EffectiveTo == null || x.TaxDate <= rate.EffectiveTo)
                .AnyAsync();
        }

        #endregion

        #region Approval Operations

        /// <summary>
        /// Approve a tax rate
        /// </summary>
        public int ApproveRate(Guid id, string approvedBy)
        {
            return Context.Updateable<TaxRate>()
                .SetColumns(it => new TaxRate
                {
                    IsApproved = true,
                    ApprovedBy = approvedBy,
                    ApprovedAt = DateTime.Now
                })
                .Where(it => it.Id == id)
                .ExecuteCommand();
        }

        /// <summary>
        /// Approve a tax rate (async)
        /// </summary>
        public async Task<int> ApproveRateAsync(Guid id, string approvedBy)
        {
            return await Context.Updateable<TaxRate>()
                .SetColumns(it => new TaxRate
                {
                    IsApproved = true,
                    ApprovedBy = approvedBy,
                    ApprovedAt = DateTime.Now
                })
                .Where(it => it.Id == id)
                .ExecuteCommandAsync();
        }

        #endregion
    }
}
