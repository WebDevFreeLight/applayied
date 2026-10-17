using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZR.Model;
using ZR.Model.Finance.Tax.Dto;

namespace ZR.Service.IService.Finance.Tax
{
    /// <summary>
    /// Tax Group Service Interface
    /// واجهة خدمة مجموعات الضرائب
    /// </summary>
    public interface ITaxGroupService
    {
        #region CRUD Operations

        /// <summary>
        /// Get paginated list of tax groups
        /// </summary>
        PagedInfo<TaxGroupResponseDto> GetList(TaxGroupQueryDto query);

        /// <summary>
        /// Get tax group by ID
        /// </summary>
        TaxGroupResponseDto GetById(Guid id);

        /// <summary>
        /// Get tax group by ID (async)
        /// </summary>
        Task<TaxGroupResponseDto> GetByIdAsync(Guid id);

        /// <summary>
        /// Get tax group by code
        /// </summary>
        TaxGroupResponseDto GetByCode(string code, long? companyId = null);

        /// <summary>
        /// Get tax group by code (async)
        /// </summary>
        Task<TaxGroupResponseDto> GetByCodeAsync(string code, long? companyId = null);

        /// <summary>
        /// Create tax group
        /// </summary>
        TaxGroupResponseDto Create(CreateTaxGroupDto dto);

        /// <summary>
        /// Create tax group (async)
        /// </summary>
        Task<TaxGroupResponseDto> CreateAsync(CreateTaxGroupDto dto);

        /// <summary>
        /// Update tax group
        /// </summary>
        TaxGroupResponseDto Update(UpdateTaxGroupDto dto);

        /// <summary>
        /// Update tax group (async)
        /// </summary>
        Task<TaxGroupResponseDto> UpdateAsync(UpdateTaxGroupDto dto);

        /// <summary>
        /// Delete tax groups
        /// </summary>
        int Delete(Guid[] ids);

        /// <summary>
        /// Delete tax groups (async)
        /// </summary>
        Task<int> DeleteAsync(Guid[] ids);

        #endregion

        #region Query Operations

        /// <summary>
        /// Get all active tax groups for dropdown
        /// </summary>
        List<TaxGroupListItemDto> GetActiveTaxGroups(long? companyId = null);

        /// <summary>
        /// Get all active tax groups for dropdown (async)
        /// </summary>
        Task<List<TaxGroupListItemDto>> GetActiveTaxGroupsAsync(long? companyId = null);

        /// <summary>
        /// Get tax groups containing a tax code
        /// </summary>
        List<TaxGroupListItemDto> GetGroupsByTaxCode(Guid taxCodeId);

        #endregion

        #region Line Operations

        /// <summary>
        /// Add a tax code to a group
        /// </summary>
        TaxGroupResponseDto AddTaxCode(Guid groupId, TaxGroupLineDto line);

        /// <summary>
        /// Add a tax code to a group (async)
        /// </summary>
        Task<TaxGroupResponseDto> AddTaxCodeAsync(Guid groupId, TaxGroupLineDto line);

        /// <summary>
        /// Remove a tax code from a group
        /// </summary>
        TaxGroupResponseDto RemoveTaxCode(Guid groupId, Guid lineId);

        /// <summary>
        /// Remove a tax code from a group (async)
        /// </summary>
        Task<TaxGroupResponseDto> RemoveTaxCodeAsync(Guid groupId, Guid lineId);

        /// <summary>
        /// Update line sequence/override in a group
        /// </summary>
        TaxGroupResponseDto UpdateLine(Guid groupId, TaxGroupLineDto line);

        /// <summary>
        /// Update line sequence/override in a group (async)
        /// </summary>
        Task<TaxGroupResponseDto> UpdateLineAsync(Guid groupId, TaxGroupLineDto line);

        #endregion

        #region Calculation Operations

        /// <summary>
        /// Calculate combined rate for a group
        /// </summary>
        decimal CalculateCombinedRate(Guid groupId, DateTime? effectiveDate = null);

        /// <summary>
        /// Calculate combined rate for a group (async)
        /// </summary>
        Task<decimal> CalculateCombinedRateAsync(Guid groupId, DateTime? effectiveDate = null);

        /// <summary>
        /// Calculate combined rate by code
        /// </summary>
        decimal CalculateCombinedRateByCode(string code, DateTime? effectiveDate = null, long? companyId = null);

        /// <summary>
        /// Calculate combined rate by code (async)
        /// </summary>
        Task<decimal> CalculateCombinedRateByCodeAsync(string code, DateTime? effectiveDate = null, long? companyId = null);

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
        /// Check if tax group can be deleted
        /// </summary>
        bool CanDelete(Guid id);

        /// <summary>
        /// Check if tax group can be deleted (async)
        /// </summary>
        Task<bool> CanDeleteAsync(Guid id);

        #endregion
    }
}
