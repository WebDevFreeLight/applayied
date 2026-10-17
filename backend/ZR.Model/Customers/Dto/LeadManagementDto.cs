using System;
using System.Collections.Generic;
using Infrastructure.Converter;

namespace ZR.Model.Customers.Dto
{
    // ==================== ACTIVITY DTOs ====================

    public class LeadActivityQueryDto : PagerInfo
    {
        public long? LeadId { get; set; }
        public long? EmployeeId { get; set; }
        public string ActivityType { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Outcome { get; set; }
    }

    public class CreateLeadActivityDto
    {
        public long LeadId { get; set; }
        public long EmployeeId { get; set; }
        public string ActivityType { get; set; }
        public DateTime ActivityDate { get; set; }
        public int? DurationMinutes { get; set; }
        public string Outcome { get; set; }
        public string Notes { get; set; }
        public DateTime? NextFollowUpDate { get; set; }
    }

    public class LeadActivityDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ActivityId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long LeadId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }

        public string EmployeeName { get; set; }
        public string LeadName { get; set; }
        public string ActivityType { get; set; }
        public DateTime ActivityDate { get; set; }
        public int? DurationMinutes { get; set; }
        public string Outcome { get; set; }
        public string Notes { get; set; }
        public DateTime? NextFollowUpDate { get; set; }
        public bool IsAutomated { get; set; }
        public DateTime? CreateTime { get; set; }
    }

    // ==================== ASSIGNMENT DTOs ====================

    public class LeadAssignmentQueryDto : PagerInfo
    {
        public long? LeadId { get; set; }
        public long? FromEmployeeId { get; set; }
        public long? ToEmployeeId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string ReassignmentType { get; set; }
    }

    public class AssignLeadDto
    {
        public long LeadId { get; set; }
        public long ToEmployeeId { get; set; }
        public string Reason { get; set; }
        public string ReassignmentType { get; set; } = "Manual";
    }

    public class BulkAssignLeadsDto
    {
        public long[] LeadIds { get; set; }
        public long ToEmployeeId { get; set; }
        public string Reason { get; set; }
    }

    public class LeadAssignmentLogDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long AssignmentLogId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long LeadId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? FromEmployeeId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long ToEmployeeId { get; set; }

        public string FromEmployeeName { get; set; }
        public string ToEmployeeName { get; set; }
        public string LeadName { get; set; }
        public DateTime AssignmentDate { get; set; }
        public string Reason { get; set; }
        public string ReassignmentType { get; set; }
    }

    // ==================== MEETING DTOs ====================

    public class LeadMeetingQueryDto : PagerInfo
    {
        public long? LeadId { get; set; }
        public long? EmployeeId { get; set; }
        public string MeetingType { get; set; }
        public string MeetingStatus { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class CreateLeadMeetingDto
    {
        public long LeadId { get; set; }
        public long AssignedEmployeeId { get; set; }
        public string MeetingTitle { get; set; }
        public string MeetingType { get; set; }
        public DateTime StartDatetime { get; set; }
        public DateTime EndDatetime { get; set; }
        public string Location { get; set; }
        public string MeetingLink { get; set; }
        public string Attendees { get; set; }
        public string Agenda { get; set; }
    }

    public class UpdateLeadMeetingDto : CreateLeadMeetingDto
    {
        public long MeetingId { get; set; }
    }

    public class LeadMeetingDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long MeetingId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long LeadId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long AssignedEmployeeId { get; set; }

        public string EmployeeName { get; set; }
        public string LeadName { get; set; }
        public string MeetingTitle { get; set; }
        public string MeetingType { get; set; }
        public string MeetingStatus { get; set; }
        public DateTime StartDatetime { get; set; }
        public DateTime EndDatetime { get; set; }
        public int DurationMinutes { get; set; }
        public string Location { get; set; }
        public string MeetingLink { get; set; }
        public string Attendees { get; set; }
        public string Agenda { get; set; }
        public string MeetingNotes { get; set; }
        public string Outcome { get; set; }
        public string CancelledReason { get; set; }
        public bool IsUpcoming { get; set; }
        public bool IsActive { get; set; }
    }

    // ==================== PERFORMANCE DTOs ====================

    public class MarketingEmployeePerformanceDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }

        public string EmployeeCode { get; set; }
        public string EmployeeName { get; set; }
        public string DepartmentName { get; set; }
        public string PositionName { get; set; }
        public int TotalLeads { get; set; }
        public int NewLeads { get; set; }
        public int ContactedLeads { get; set; }
        public int QualifiedLeads { get; set; }
        public int ConvertedLeads { get; set; }
        public int LostLeads { get; set; }
        public decimal ConversionRate { get; set; }
        public decimal QualificationRate { get; set; }
        public decimal LostRate { get; set; }
        public int TotalActivities { get; set; }
        public int TotalCalls { get; set; }
        public int TotalMeetings { get; set; }
        public int TotalEmails { get; set; }
        public decimal AvgActivitiesPerLead { get; set; }
        public decimal AvgFollowUpTimeDays { get; set; }
        public int TotalScheduledMeetings { get; set; }
        public int CompletedMeetings { get; set; }
        public int CancelledMeetings { get; set; }
        public decimal MeetingCompletionRate { get; set; }
        public decimal TotalExpectedRevenue { get; set; }
        public decimal ConvertedRevenue { get; set; }
        public DateTime? PeriodStart { get; set; }
        public DateTime? PeriodEnd { get; set; }
    }

    public class LeadStatusDistributionDto
    {
        public string Status { get; set; }
        public int Count { get; set; }
        public decimal Percentage { get; set; }
    }

    public class LeadSourceDistributionDto
    {
        public string Source { get; set; }
        public int Count { get; set; }
        public int Converted { get; set; }
        public decimal ConversionRate { get; set; }
    }

    public class LostReasonDistributionDto
    {
        public string Reason { get; set; }
        public int Count { get; set; }
        public decimal Percentage { get; set; }
    }

    public class ActivityTypeDistributionDto
    {
        public string ActivityType { get; set; }
        public int Count { get; set; }
        public decimal AvgDurationMinutes { get; set; }
    }

    public class PerformanceReportRequestDto
    {
        public long? EmployeeId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string GroupBy { get; set; }
    }

    public class PerformanceReportDto
    {
        public MarketingEmployeePerformanceDto OverallMetrics { get; set; }
        public List<LeadStatusDistributionDto> StatusDistribution { get; set; }
        public List<LeadSourceDistributionDto> SourceDistribution { get; set; }
        public List<LostReasonDistributionDto> LostReasonDistribution { get; set; }
        public List<ActivityTypeDistributionDto> ActivityDistribution { get; set; }
        public List<LeadActivityDto> RecentActivities { get; set; }
        public List<LeadMeetingDto> UpcomingMeetings { get; set; }
    }

    // ==================== DASHBOARD DTOs ====================

    public class LeadFunnelStageDto
    {
        public string Stage { get; set; }
        public int Count { get; set; }
        public decimal DropoffRate { get; set; }
    }

    public class TimeSeriesDataPointDto
    {
        public DateTime Date { get; set; }
        public int NewLeads { get; set; }
        public int Converted { get; set; }
        public int Lost { get; set; }
    }

    public class MeetingAvailabilitySlotDto
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsAvailable { get; set; }
        public string ConflictReason { get; set; }
    }

    // ==================== EXCEL IMPORT/EXPORT DTOs ====================

    public class LeadExcelImportDto
    {
        public string LeadType { get; set; }
        public string BrandName { get; set; }
        public string OwnerName { get; set; }
        public string OwnerPhone { get; set; }
        public string OwnerEmail { get; set; }
        public string LeadSource { get; set; }
        public string BudgetRange { get; set; }
        public string Priority { get; set; }
        public decimal? ExpectedRevenue { get; set; }
        public string InternalNotes { get; set; }

        [MiniExcelLibs.Attributes.ExcelColumnName("notes")]
        public string Notes { get => InternalNotes; set => InternalNotes = value; }

        public string AssignedToEmployeeCode { get; set; }

        [MiniExcelLibs.Attributes.ExcelColumnName("assignto name")]
        public string AssignToName { get; set; }

        [MiniExcelLibs.Attributes.ExcelIgnore]
        public string ValidationError { get; set; }

        [MiniExcelLibs.Attributes.ExcelIgnore]
        public bool IsValid => string.IsNullOrEmpty(ValidationError);
    }

    public class LeadExcelExportDto
    {
        public string LeadType { get; set; }
        public string BrandName { get; set; }
        public string OwnerName { get; set; }
        public string OwnerPhone { get; set; }
        public string OwnerEmail { get; set; }
        public string AssignedToName { get; set; }
        public string LeadStatus { get; set; }
        public string LeadSource { get; set; }
        public string BudgetRange { get; set; }
        public string Priority { get; set; }
        public decimal? ExpectedRevenue { get; set; }
        public DateTime? FirstContactDate { get; set; }
        public DateTime? LastContactDate { get; set; }
        public string InternalNotes { get; set; }
        public DateTime? CreateTime { get; set; }
    }

    public class LeadImportPreviewDto
    {
        public int TotalRows { get; set; }
        public int ValidRows { get; set; }
        public int InvalidRows { get; set; }
        public List<LeadExcelImportDto> PreviewData { get; set; }
        public List<string> ValidationErrors { get; set; }
    }

    public class LeadImportRequestDto
    {
        public string AssignedToEmployeeId { get; set; }
        public bool OverwriteExisting { get; set; } = false;
        public bool SkipInvalidRows { get; set; } = true;
    }

    public class LeadImportResultDto
    {
        public int TotalRows { get; set; }
        public int SuccessCount { get; set; }
        public int FailedCount { get; set; }
        public int SkippedCount { get; set; }
        public List<string> Errors { get; set; }
        public List<long> ImportedLeadIds { get; set; }
    }
}
