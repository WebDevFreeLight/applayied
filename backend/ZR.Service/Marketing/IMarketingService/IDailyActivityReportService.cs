using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZR.Model.Marketing.Dto;

namespace ZR.Service.Marketing.IMarketingService
{
    /// <summary>
    /// Daily Activity Report Service Interface
    /// Aggregates employee activities from multiple business tables into a unified timeline
    /// </summary>
    public interface IDailyActivityReportService
    {
        Task<EmployeeActivityReportDto> GetDailyReportAsync(EmployeeActivityQueryDto query);
        Task<ActivitySummaryDto> GetSummaryAsync(EmployeeActivityQueryDto query);
        Task<List<ActivityTimelineItemDto>> GetTimelineAsync(EmployeeActivityQueryDto query);
        Task<List<EmployeeDailyPerformanceDto>> GetTeamPerformanceAsync(DateTime startDate, DateTime endDate);
    }
}
