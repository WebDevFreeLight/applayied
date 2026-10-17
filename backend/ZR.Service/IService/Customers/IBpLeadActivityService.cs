using System;
using System.Collections.Generic;
using ZR.Model;
using ZR.Model.Customers.Dto;
using ZR.Model.Customers.Leads;

namespace ZR.Service.Customers.ICustomersService
{
    /// <summary>
    /// Lead Activity Service Interface
    /// </summary>
    public interface IBpLeadActivityService : IBaseService<BpLeadActivity>
    {
        PagedInfo<LeadActivityDto> GetActivityList(LeadActivityQueryDto queryDto);
        LeadActivityDto GetActivityById(long activityId);
        LeadActivityDto CreateActivity(CreateLeadActivityDto createDto);
        List<LeadActivityDto> GetLeadActivityTimeline(long leadId);
        List<LeadActivityDto> GetRecentActivitiesByEmployee(long employeeId, int limit = 10);
        List<LeadActivityDto> GetActivitiesByType(string activityType, DateTime? startDate = null, DateTime? endDate = null);
        List<LeadActivityDto> GetUpcomingFollowUps(long? employeeId = null, int daysAhead = 7);
        List<LeadActivityDto> GetOverdueFollowUps(long? employeeId = null);
        ActivityStatisticsDto GetEmployeeActivityStatistics(long employeeId, DateTime? startDate = null, DateTime? endDate = null);
        int DeleteActivity(long activityId);
    }

    /// <summary>
    /// Activity Statistics DTO
    /// </summary>
    public class ActivityStatisticsDto
    {
        public int TotalActivities { get; set; }
        public int TotalCalls { get; set; }
        public int TotalMeetings { get; set; }
        public int TotalEmails { get; set; }
        public int TotalSMS { get; set; }
        public int TotalWhatsApp { get; set; }
        public int TotalNotes { get; set; }
        public int TotalFollowUps { get; set; }
        public int SuccessfulContacts { get; set; }
        public int FailedContacts { get; set; }
        public decimal AvgDurationMinutes { get; set; }
        public int UpcomingFollowUps { get; set; }
        public int OverdueFollowUps { get; set; }
    }
}
