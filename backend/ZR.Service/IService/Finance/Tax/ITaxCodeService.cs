using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZR.Model;
using ZR.Model.Finance.Tax.Dto;
using ZR.Model.Finance.Enums;
using ZR.Model.Finance.Tax;

namespace ZR.Service.IService.Finance.Tax
{
    /// <summary>
    /// Tax Code Service Interface
    /// واجهة خدمة رموز الضرائب
    /// </summary>
    public interface ITaxCodeService
    {
        #region CRUD Operations

        /// <summary>
        /// Get paginated list of tax codes
        /// </summary>
        PagedInfo<TaxCodeResponseDto> GetList(TaxCodeQueryDto query);

        /// <summary>
        /// Get tax code by ID
        /// </summary>
        TaxCodeResponseDto GetById(Guid id);

        /// <summary>
        /// Get tax code by ID (async)
        /// </summary>
        Task<TaxCodeResponseDto> GetByIdAsync(Guid id);

        /// <summary>
        /// Get tax code by code
        /// </summary>
        TaxCodeResponseDto GetByCode(string code, long? companyId = null);

        /// <summary>
        /// Get tax code by code (async)
        /// </summary>
        Task<TaxCodeResponseDto> GetByCodeAsync(string code, long? companyId = null);

        /// <summary>
        /// Create tax code
        /// </summary>
        TaxCodeResponseDto Create(CreateTaxCodeDto dto);

        /// <summary>
        /// Create tax code (async)
        /// </summary>
        Task<TaxCodeResponseDto> CreateAsync(CreateTaxCodeDto dto);

        /// <summary>
        /// Update tax code
        /// </summary>
        TaxCodeResponseDto Update(UpdateTaxCodeDto dto);

        /// <summary>
        /// Update tax code (async)
        /// </summary>
        Task<TaxCodeResponseDto> UpdateAsync(UpdateTaxCodeDto dto);

        /// <summary>
        /// Delete tax codes
        /// </summary>
        int Delete(Guid[] ids);

        /// <summary>
        /// Delete tax codes (async)
        /// </summary>
        Task<int> DeleteAsync(Guid[] ids);

        #endregion

        #region Query Operations

        /// <summary>
        /// Get all active tax codes for dropdown
        /// </summary>
        List<TaxCodeListItemDto> GetActiveTaxCodes(long? companyId = null);

        /// <summary>
        /// Get all active tax codes for dropdown (async)
        /// </summary>
        Task<List<TaxCodeListItemDto>> GetActiveTaxCodesAsync(long? companyId = null);

        /// <summary>
        /// Get default tax code
        /// </summary>
        TaxCodeResponseDto GetDefaultTaxCode(long? companyId = null);

        /// <summary>
        /// Get default tax code (async)
        /// </summary>
        Task<TaxCodeResponseDto> GetDefaultTaxCodeAsync(long? companyId = null);

        /// <summary>
        /// Get tax codes by type
        /// </summary>
        List<TaxCodeListItemDto> GetByTaxType(TaxType taxType, long? companyId = null);

        #endregion

        #region Rate Operations

        /// <summary>
        /// Get effective rate for tax code on a date
        /// </summary>
        decimal GetEffectiveRate(Guid taxCodeId, DateTime date);

        /// <summary>
        /// Get effective rate for tax code on a date (async)
        /// </summary>
        Task<decimal> GetEffectiveRateAsync(Guid taxCodeId, DateTime date);

        /// <summary>
        /// Get effective rate by code on a date
        /// </summary>
        decimal GetEffectiveRateByCode(string code, DateTime date, long? companyId = null);

        /// <summary>
        /// Get effective rate by code on a date (async)
        /// </summary>
        Task<decimal> GetEffectiveRateByCodeAsync(string code, DateTime date, long? companyId = null);

        #endregion

        #region Validation Operations

        /// <summary>
        /// Check if code exists
        /// </summary>
        bool CodeExists(string code, long? companyId, Guid? excludeId = null);

        /// <summary>
        /// Check if code exists (async)
        /// </summary>
        Task<bool> CodeExistsAsync(string code, long? companyId, Guid? excludeId = null);

        /// <summary>
        /// Check if tax code can be deleted
        /// </summary>
        bool CanDelete(Guid id);

        /// <summary>
        /// Check if tax code can be deleted (async)
        /// </summary>
        Task<bool> CanDeleteAsync(Guid id);

        #endregion
    }
}
