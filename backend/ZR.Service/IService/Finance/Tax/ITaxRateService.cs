using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZR.Model;
using ZR.Model.Finance.Tax.Dto;

namespace ZR.Service.IService.Finance.Tax
{
    /// <summary>
    /// Tax Rate Service Interface
    /// واجهة خدمة معدلات الضرائب
    /// </summary>
    public interface ITaxRateService
    {
        #region CRUD Operations

        /// <summary>
        /// Get paginated list of tax rates
        /// </summary>
        PagedInfo<TaxRateResponseDto> GetList(TaxRateQueryDto query);

        /// <summary>
        /// Get tax rate by ID
        /// </summary>
        TaxRateResponseDto GetById(Guid id);

        /// <summary>
        /// Get tax rate by ID (async)
        /// </summary>
        Task<TaxRateResponseDto> GetByIdAsync(Guid id);

        /// <summary>
        /// Create tax rate
        /// </summary>
        TaxRateResponseDto Create(CreateTaxRateDto dto);

        /// <summary>
        /// Create tax rate (async)
        /// </summary>
        Task<TaxRateResponseDto> CreateAsync(CreateTaxRateDto dto);

        /// <summary>
        /// Update tax rate
        /// </summary>
        TaxRateResponseDto Update(UpdateTaxRateDto dto);

        /// <summary>
        /// Update tax rate (async)
        /// </summary>
        Task<TaxRateResponseDto> UpdateAsync(UpdateTaxRateDto dto);

        /// <summary>
        /// End a tax rate (set effective_to date)
        /// </summary>
        TaxRateResponseDto EndRate(EndTaxRateDto dto);

        /// <summary>
        /// End a tax rate (async)
        /// </summary>
        Task<TaxRateResponseDto> EndRateAsync(EndTaxRateDto dto);

        /// <summary>
        /// Delete tax rates
        /// </summary>
        int Delete(Guid[] ids);

        /// <summary>
        /// Delete tax rates (async)
        /// </summary>
        Task<int> DeleteAsync(Guid[] ids);

        #endregion

        #region Query Operations

        /// <summary>
        /// Get all rates for a tax code
        /// </summary>
        List<TaxRateResponseDto> GetRatesByTaxCode(Guid taxCodeId);

        /// <summary>
        /// Get all rates for a tax code (async)
        /// </summary>
        Task<List<TaxRateResponseDto>> GetRatesByTaxCodeAsync(Guid taxCodeId);

        /// <summary>
        /// Get effective rate for a specific date
        /// </summary>
        GetRateForDateResponse GetRateForDate(GetRateForDateRequest request);

        /// <summary>
        /// Get effective rate for a specific date (async)
        /// </summary>
        Task<GetRateForDateResponse> GetRateForDateAsync(GetRateForDateRequest request);

        /// <summary>
        /// Get currently effective rates
        /// </summary>
        List<TaxRateResponseDto> GetCurrentlyEffectiveRates();

        /// <summary>
        /// Get future rates
        /// </summary>
        List<TaxRateResponseDto> GetFutureRates();

        /// <summary>
        /// Get expired rates
        /// </summary>
        List<TaxRateResponseDto> GetExpiredRates();

        #endregion

        #region Validation Operations

        /// <summary>
        /// Check for overlapping rates
        /// </summary>
        bool HasOverlappingRates(Guid taxCodeId, DateTime effectiveFrom, DateTime? effectiveTo, Guid? excludeId = null);

        /// <summary>
        /// Check for overlapping rates (async)
        /// </summary>
        Task<bool> HasOverlappingRatesAsync(Guid taxCodeId, DateTime effectiveFrom, DateTime? effectiveTo, Guid? excludeId = null);

        /// <summary>
        /// Check if rate can be deleted
        /// </summary>
        bool CanDelete(Guid id);

        /// <summary>
        /// Check if rate can be deleted (async)
        /// </summary>
        Task<bool> CanDeleteAsync(Guid id);

        #endregion

        #region Approval Operations

        /// <summary>
        /// Approve a tax rate
        /// </summary>
        TaxRateResponseDto Approve(Guid id, string approvedBy);

        /// <summary>
        /// Approve a tax rate (async)
        /// </summary>
        Task<TaxRateResponseDto> ApproveAsync(Guid id, string approvedBy);

        #endregion
    }
}
