using System;
using System.Collections.Generic;
using JsonIgnoreAttribute = Newtonsoft.Json.JsonIgnoreAttribute;

namespace ZR.Model.Operations.Dto
{
    /// <summary>
    /// Site Visit Query DTO
    /// </summary>
    public class SiteVisitQueryDto : PagerInfo
    {
        public long? LeadId { get; set; }
        public long? DealId { get; set; }
        public string VisitStatus { get; set; }
        public long? AssignedStaffId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        [JsonIgnore]
        public long? CurrentUserId { get; set; }

        [JsonIgnore]
        public bool IsCurrentUserAdmin { get; set; }
    }

    /// <summary>
    /// Create Site Visit DTO
    /// </summary>
    public class CreateSiteVisitDto
    {
        public long LeadId { get; set; }
        public long? DealId { get; set; }
        public DateTime ScheduledDate { get; set; }
        public string ScheduledTime { get; set; }
        public long? AssignedStaffId { get; set; }
        public string VisitNotes { get; set; }
    }

    /// <summary>
    /// Update Site Visit DTO
    /// </summary>
    public class UpdateSiteVisitDto
    {
        public long VisitId { get; set; }
        public long LeadId { get; set; }
        public long? DealId { get; set; }
        public DateTime ScheduledDate { get; set; }
        public string ScheduledTime { get; set; }
        public long? AssignedStaffId { get; set; }
        public string VisitNotes { get; set; }
    }

    /// <summary>
    /// Update Visit Status DTO
    /// </summary>
    public class UpdateVisitStatusDto
    {
        public long VisitId { get; set; }
        public string NewStatus { get; set; }
        public string Notes { get; set; }
    }

    /// <summary>
    /// Visit Dashboard DTO
    /// </summary>
    public class VisitDashboardDto
    {
        public int ScheduledCount { get; set; }
        public int InProgressCount { get; set; }
        public int CompletedCount { get; set; }
        public int CancelledCount { get; set; }
        public int TodayVisits { get; set; }
        public int UpcomingVisits { get; set; }
        public int TotalVisits { get; set; }
    }
}
