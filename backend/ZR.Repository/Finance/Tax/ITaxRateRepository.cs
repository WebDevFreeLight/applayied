using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZR.Model;
using ZR.Model.Finance.Tax;

namespace ZR.Repository.Finance.Tax
{
    /// <summary>
    /// Tax Rate Repository Interface
    /// واجهة مستودع معدلات الضرائب
    /// </summary>
    public interface ITaxRateRepository : IBaseRepository<TaxRate>
    {
        #region CRUD Operations

        /// <summary>
        /// Get paginated list of tax rates
        /// </summary>
        PagedInfo<TaxRate> GetList(TaxRate parm, PagerInfo pager);

        /// <summary>
        /// Get tax rate by ID
        /// </summary>
        TaxRate GetById(Guid id);

        /// <summary>
        /// Get tax rate by ID (async)
        /// </summary>
        Task<TaxRate> GetByIdAsync(Guid id);

        /// <summary>
        /// Insert tax rate
        /// </summary>
        int InsertTaxRate(TaxRate model);

        /// <summary>
        /// Insert tax rate (async)
        /// </summary>
        Task<int> InsertTaxRateAsync(TaxRate model);

        /// <summary>
        /// Update tax rate
        /// </summary>
        int UpdateTaxRate(TaxRate model);

        /// <summary>
        /// Update tax rate (async)
        /// </summary>
        Task<int> UpdateTaxRateAsync(TaxRate model);

        /// <summary>
        /// Delete tax rates by IDs
        /// </summary>
        int DeleteTaxRates(Guid[] ids);

        /// <summary>
        /// Delete tax rates by IDs (async)
        /// </summary>
        Task<int> DeleteTaxRatesAsync(Guid[] ids);

        #endregion

        #region Effective Date Operations

        /// <summary>
        /// Get effective rate for a tax code on a specific date
        /// Critical for tax calculation
        /// </summary>
        TaxRate GetEffectiveRate(Guid taxCodeId, DateTime date);

        /// <summary>
        /// Get effective rate for a tax code on a specific date (async)
        /// </summary>
        Task<TaxRate> GetEffectiveRateAsync(Guid taxCodeId, DateTime date);

        /// <summary>
        /// Get all rates for a tax code
        /// </summary>
        List<TaxRate> GetRatesByTaxCode(Guid taxCodeId);

        /// <summary>
        /// Get all rates for a tax code (async)
        /// </summary>
        Task<List<TaxRate>> GetRatesByTaxCodeAsync(Guid taxCodeId);

        /// <summary>
        /// Get currently effective rates (for all tax codes)
        /// </summary>
        List<TaxRate> GetCurrentlyEffectiveRates();

        /// <summary>
        /// Get future rates (not yet effective)
        /// </summary>
        List<TaxRate> GetFutureRates();

        /// <summary>
        /// Get expired rates (past effective_to date)
        /// </summary>
        List<TaxRate> GetExpiredRates();

        #endregion

        #region Validation Operations

        /// <summary>
        /// Check if date range overlaps with existing rates
        /// </summary>
        bool HasOverlappingRates(Guid taxCodeId, DateTime effectiveFrom, DateTime? effectiveTo, Guid? excludeId = null);

        /// <summary>
        /// Check if date range overlaps with existing rates (async)
        /// </summary>
        Task<bool> HasOverlappingRatesAsync(Guid taxCodeId, DateTime effectiveFrom, DateTime? effectiveTo, Guid? excludeId = null);

        /// <summary>
        /// Check if tax rate can be deleted (not used in calculations)
        /// </summary>
        bool CanDelete(Guid id);

        /// <summary>
        /// Check if tax rate can be deleted (async)
        /// </summary>
        Task<bool> CanDeleteAsync(Guid id);

        #endregion

        #region Approval Operations

        /// <summary>
        /// Approve a tax rate
        /// </summary>
        int ApproveRate(Guid id, string approvedBy);

        /// <summary>
        /// Approve a tax rate (async)
        /// </summary>
        Task<int> ApproveRateAsync(Guid id, string approvedBy);

        #endregion
    }
}
