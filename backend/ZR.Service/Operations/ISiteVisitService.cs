using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZR.Model.Operations;
using ZR.Model.Operations.Dto;

namespace ZR.Service.Operations
{
    public interface ISiteVisitService
    {
        Task<PagedInfo<SiteVisit>> GetVisitsListAsync(SiteVisitQueryDto query);
        Task<SiteVisit> GetVisitByIdAsync(long visitId);
        Task<List<SiteVisit>> GetVisitsByLeadAsync(long leadId);
        Task<List<SiteVisit>> GetTodayVisitsAsync(long? staffId);
        Task<List<SiteVisit>> GetUpcomingVisitsAsync(int days, long? staffId);
        Task<SiteVisit> CreateVisitAsync(SiteVisit visit);
        Task<int> UpdateVisitAsync(SiteVisit visit);
        Task<int> UpdateVisitStatusAsync(long visitId, string newStatus, string notes);
        Task<int> DeleteVisitsAsync(long[] visitIds);
        Task<VisitDashboardDto> GetDashboardAsync(long? staffId);
        Task<long?> GetEmployeeIdByUserIdAsync(long userId);
        Task<PagedInfo<SiteVisit>> GetVisitsByCreatorAsync(SiteVisitQueryDto query, string createdBy);
    }
}
