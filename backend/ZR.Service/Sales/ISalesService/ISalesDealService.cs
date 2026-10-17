using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZR.Model.Sales;
using ZR.Model.Sales.Dto;

namespace ZR.Service.Sales.ISalesService
{
    public interface ISalesDealService
    {
        /// <summary>
        /// Get deals list with role-based filtering and joins
        /// </summary>
        Task<PagedInfo<SalesDeal>> GetDealsListAsync(DealQueryDto query);

        /// <summary>
        /// Get deal by ID with joined lead name and assigned-to name
        /// </summary>
        Task<SalesDeal> GetDealByIdAsync(long dealId);

        /// <summary>
        /// Get all deals for a specific lead
        /// </summary>
        Task<List<SalesDeal>> GetDealsByLeadAsync(long leadId);

        /// <summary>
        /// Create a new deal
        /// </summary>
        Task<SalesDeal> CreateDealAsync(SalesDeal deal, long? currentUserId = null, bool isAdmin = true);

        /// <summary>
        /// Update an existing deal
        /// </summary>
        Task<int> UpdateDealAsync(SalesDeal deal);

        /// <summary>
        /// Update deal status. If Won, also converts the lead to "Converted".
        /// </summary>
        Task<int> UpdateDealStatusAsync(long dealId, string newStatus, string notes);

        /// <summary>
        /// Delete deals by IDs
        /// </summary>
        Task<int> DeleteDealsAsync(long[] dealIds);

        /// <summary>
        /// Get dashboard statistics (counts by status, total values)
        /// </summary>
        Task<DealDashboardDto> GetDashboardStatsAsync(long? assignedTo);

        /// <summary>
        /// Get employee ID by user ID (for linking system users to HR employees)
        /// </summary>
        Task<long?> GetEmployeeIdByUserIdAsync(long userId);

        /// <summary>
        /// Recalculate deal summary (dealValue, dealType) from its units.
        /// Called after unit create/update/delete.
        /// </summary>
        Task RecalculateDealSummaryAsync(long dealId);
    }
}
