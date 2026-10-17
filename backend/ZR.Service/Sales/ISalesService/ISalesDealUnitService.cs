using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZR.Model.Sales;
using ZR.Model.Sales.Dto;

namespace ZR.Service.Sales.ISalesService
{
    public interface ISalesDealUnitService
    {
        /// <summary>
        /// Get deal units with pagination and filtering
        /// </summary>
        Task<PagedInfo<DealUnitResponseDto>> GetListAsync(DealUnitQueryDto query);

        /// <summary>
        /// Get all units for a specific deal
        /// </summary>
        Task<List<DealUnitResponseDto>> GetByDealIdAsync(long dealId);

        /// <summary>
        /// Get deal unit by ID
        /// </summary>
        Task<SalesDealUnit> GetByIdAsync(long dealUnitId);

        /// <summary>
        /// Add a shop to a deal with pricing terms
        /// Validates: shop exists, shop is available, pricing integrity
        /// Auto-calculates discount from shop base price
        /// </summary>
        Task<SalesDealUnit> CreateAsync(CreateDealUnitDto dto, string createdBy);

        /// <summary>
        /// Bulk add multiple shops to a deal
        /// </summary>
        Task<List<SalesDealUnit>> BulkCreateAsync(BulkCreateDealUnitsDto dto, string createdBy);

        /// <summary>
        /// Update deal unit (only if not yet converted to contract)
        /// Allows: change shop, modify pricing, switch pricing type
        /// </summary>
        Task<int> UpdateAsync(UpdateDealUnitDto dto, string updatedBy);

        /// <summary>
        /// Remove a shop from a deal (only if not yet converted)
        /// </summary>
        Task<int> DeleteAsync(long dealUnitId);

        /// <summary>
        /// Check if a shop is already linked to an active (non-Lost) deal
        /// </summary>
        Task<bool> IsShopLinkedToActiveDealAsync(long shopId, long? excludeDealId = null);

        /// <summary>
        /// Get available shops for a season/zone (excluding already-linked shops)
        /// </summary>
        Task<List<AvailableShopDto>> GetAvailableShopsAsync(long seasonId, long? zoneId = null);

        /// <summary>
        /// Check availability of all shops in a deal's units (for Won status validation).
        /// Returns each unit with isAvailable flag and which deal reserved it.
        /// </summary>
        Task<List<DealUnitAvailabilityDto>> CheckDealUnitsAvailabilityAsync(long dealId);
    }
}
