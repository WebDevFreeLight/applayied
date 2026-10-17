using System;
using System.Collections.Generic;

namespace ZR.Model.Marketing.Dto
{
    /// <summary>
    /// Query parameters for daily activity report
    /// </summary>
    public class EmployeeActivityQueryDto
    {
        public long? EmployeeId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string ActivitySource { get; set; }
        public string ActivityType { get; set; }
    }

    /// <summary>
    /// Single timeline item (unified across all sources)
    /// </summary>
    public class ActivityTimelineItemDto
    {
        public long Id { get; set; }
        public string Source { get; set; }
        public string ActivityType { get; set; }
        public DateTime Timestamp { get; set; }
        public string Description { get; set; }
        public string EntityName { get; set; }
        public long? EntityId { get; set; }
        public string Outcome { get; set; }
        public int? DurationMinutes { get; set; }
        public string Notes { get; set; }
        public string EmployeeName { get; set; }
        public long EmployeeId { get; set; }
    }

    /// <summary>
    /// Summary KPIs for the date range
    /// </summary>
    public class ActivitySummaryDto
    {
        public int TotalActivities { get; set; }
        public int LeadsCreated { get; set; }
        public int LeadsContacted { get; set; }
        public int LeadsConverted { get; set; }
        public int CallsMade { get; set; }
        public int MeetingsHeld { get; set; }
        public int EmailsSent { get; set; }
        public int SiteVisitsCompleted { get; set; }
        public int DealsCreated { get; set; }
        public int DealsWon { get; set; }
        public decimal TotalDealValue { get; set; }
        public int AssignmentsReceived { get; set; }
        public int TotalDurationMinutes { get; set; }
        public Dictionary<string, int> BySource { get; set; } = new();
        public Dictionary<string, int> ByActivityType { get; set; } = new();
        public Dictionary<string, int> ByHour { get; set; } = new();
    }

    /// <summary>
    /// Per-employee row for team comparison table
    /// </summary>
    public class EmployeeDailyPerformanceDto
    {
        public long EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string EmployeeCode { get; set; }
        public int TotalActions { get; set; }
        public int Calls { get; set; }
        public int Meetings { get; set; }
        public int Emails { get; set; }
        public int SiteVisits { get; set; }
        public int LeadsCreated { get; set; }
        public int DealsCreated { get; set; }
        public int TotalMinutes { get; set; }
    }

    /// <summary>
    /// Full report response
    /// </summary>
    public class EmployeeActivityReportDto
    {
        public ActivitySummaryDto Summary { get; set; }
        public List<ActivityTimelineItemDto> Timeline { get; set; }
        public List<EmployeeDailyPerformanceDto> TeamPerformance { get; set; }
    }
}
