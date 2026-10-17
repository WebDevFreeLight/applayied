using Infrastructure.Attribute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZR.Model.Customers.Leads;
using ZR.Model.Customers.Dto;

namespace ZR.Service.Customers.ICustomersService
{

    public interface IBpLeadService
    {
        /// <summary>
        /// Get leads list with role-based filtering
        /// </summary>
        Task<PagedInfo<BpLead>> GetLeadsListAsync(LeadQueryDto query);

        Task<BpLead> GetLeadByIdAsync(long leadId);

        Task<List<BpLead>> GetLeadsByMarketerAsync(long assignedTo);

        /// <summary>
        /// Add lead with optional auto-assignment for non-admin users
        /// </summary>
        Task<BpLead> AddLeadAsync(BpLead lead, long? currentUserId = null, bool isAdmin = true);

        Task<int> UpdateLeadAsync(BpLead lead);

        Task<int> DeleteLeadsAsync(long[] leadIds);

        /// <summary>
        /// Update lead status with transactional side effects (Lost cleanup, Confirmed auto-set, audit logging)
        /// </summary>
        Task<int> UpdateLeadStatusWithSideEffectsAsync(long leadId, string newStatus, string notes, string updatedBy);

        /// <summary>
        /// Get employee ID by user ID
        /// </summary>
        Task<long?> GetEmployeeIdByUserIdAsync(long userId);

        Task<List<object>> GetLocationsLookupAsync();

        // Excel Import/Export methods
        Task<List<LeadExcelExportDto>> GetLeadsForExportAsync(LeadQueryDto query);

        Task<LeadImportPreviewDto> PreviewExcelImportAsync(string filePath);

        Task<LeadImportResultDto> ImportLeadsFromExcelAsync(string filePath, LeadImportRequestDto request);

        /// <summary>
        /// Sync active customers without lead records - auto-creates Confirmed leads for them
        /// </summary>
        Task<int> SyncCustomersToLeadsAsync(string userName);
    }

}
