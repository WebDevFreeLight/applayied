using System;
using System.Collections.Generic;
using Infrastructure.Converter;

namespace ZR.Model.BusinessPartners.Meetings.Dto
{
    // ==================== MEETING DTOs (Generic for LEAD & CUSTOMER) ====================

    /// <summary>
    /// Meeting Query DTO - For filtering meetings
    /// </summary>
    public class MeetingQueryDto : PagerInfo
    {
        public string EntityType { get; set; } // LEAD or CUSTOMER
        public long? EntityId { get; set; }
        public long? EmployeeId { get; set; }
        public string MeetingType { get; set; } // ONSITE, ONLINE, CALL
        public string Status { get; set; } // SCHEDULED, COMPLETED, CANCELLED, NO_SHOW
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string MeetingChannel { get; set; } // ZOOM, TEAMS, etc.
    }

    /// <summary>
    /// Create Meeting DTO
    /// </summary>
    public class CreateMeetingDto
    {
        public string EntityType { get; set; } // LEAD or CUSTOMER (required)
        public long EntityId { get; set; } // Lead ID or Customer ID (required)
        public string Title { get; set; } // Meeting title (required)
        public string Description { get; set; }
        public DateTime StartDatetime { get; set; } // Required
        public DateTime EndDatetime { get; set; } // Required
        public string Timezone { get; set; } = "Asia/Riyadh";
        public string MeetingType { get; set; } // ONSITE, ONLINE, CALL (required)
        public string MeetingChannel { get; set; } // ZOOM, GOOGLE_MEET, TEAMS, PHONE, OFFICE, OTHER
        public string Location { get; set; } // For ONSITE meetings
        public string MeetingUrl { get; set; } // For ONLINE meetings

        // Participants
        public long CreatedBy { get; set; } // Employee creating the meeting (will be ORGANIZER)
        public List<MeetingParticipantInput> Participants { get; set; } // Additional participants
    }

    /// <summary>
    /// Update Meeting DTO
    /// </summary>
    public class UpdateMeetingDto
    {
        public long MeetingId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime? StartDatetime { get; set; }
        public DateTime? EndDatetime { get; set; }
        public string Timezone { get; set; }
        public string MeetingType { get; set; }
        public string MeetingChannel { get; set; }
        public string Location { get; set; }
        public string MeetingUrl { get; set; }
        public long UpdatedBy { get; set; }
    }

    /// <summary>
    /// Cancel Meeting DTO
    /// </summary>
    public class CancelMeetingDto
    {
        public long MeetingId { get; set; }
        public string CancelReason { get; set; }
        public long UpdatedBy { get; set; }
    }

    /// <summary>
    /// Complete Meeting DTO
    /// </summary>
    public class CompleteMeetingDto
    {
        public long MeetingId { get; set; }
        public long UpdatedBy { get; set; }

        // Outcome details (optional - can be added later)
        public string OutcomeType { get; set; } // POSITIVE, NEUTRAL, NEGATIVE
        public string Notes { get; set; }
        public string NextAction { get; set; } // FOLLOW_UP, PROPOSAL, CLOSE, etc.
        public DateTime? NextActionDate { get; set; }
        public long? NextActionAssignee { get; set; }
    }

    /// <summary>
    /// Meeting Response DTO
    /// </summary>
    public class MeetingDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long MeetingId { get; set; }

        public string EntityType { get; set; } // LEAD or CUSTOMER

        [JsonConverter(typeof(ValueToStringConverter))]
        public long EntityId { get; set; }

        public string EntityName { get; set; } // Lead or Customer name

        public string Title { get; set; }
        public string Description { get; set; }

        public DateTime StartDatetime { get; set; }
        public DateTime EndDatetime { get; set; }
        public int DurationMinutes { get; set; }
        public string Timezone { get; set; }

        public string MeetingType { get; set; } // ONSITE, ONLINE, CALL
        public string MeetingChannel { get; set; }
        public string Location { get; set; }
        public string MeetingUrl { get; set; }

        public string Status { get; set; } // SCHEDULED, COMPLETED, CANCELLED, NO_SHOW
        public string CancelReason { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long CreatedBy { get; set; }

        public string OrganizerName { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation data
        public List<MeetingParticipantDto> Participants { get; set; }
        public MeetingOutcomeDto Outcome { get; set; }

        // Computed
        public bool IsUpcoming { get; set; }
        public bool IsActive { get; set; }
        public bool IsPast { get; set; }
    }

    // ==================== PARTICIPANT DTOs ====================

    /// <summary>
    /// Participant Input DTO (for creating meetings)
    /// </summary>
    public class MeetingParticipantInput
    {
        public long EmployeeId { get; set; }
        public string Role { get; set; } = "ATTENDEE"; // ORGANIZER, ATTENDEE, OBSERVER
        public string Notes { get; set; }
    }

    /// <summary>
    /// Add Participant DTO
    /// </summary>
    public class AddParticipantDto
    {
        public long MeetingId { get; set; }
        public long EmployeeId { get; set; }
        public string Role { get; set; } = "ATTENDEE";
        public string Notes { get; set; }
    }

    /// <summary>
    /// Update Participant Attendance DTO
    /// </summary>
    public class UpdateParticipantAttendanceDto
    {
        public long ParticipantId { get; set; }
        public string AttendanceStatus { get; set; } // PENDING, ACCEPTED, DECLINED, ATTENDED, ABSENT
        public string Notes { get; set; }
    }

    /// <summary>
    /// Participant Response DTO
    /// </summary>
    public class MeetingParticipantDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ParticipantId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long MeetingId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }

        public string EmployeeName { get; set; }
        public string EmployeeEmail { get; set; }

        public string Role { get; set; } // ORGANIZER, ATTENDEE, OBSERVER
        public string AttendanceStatus { get; set; } // PENDING, ACCEPTED, DECLINED, ATTENDED, ABSENT

        public DateTime? ResponseDatetime { get; set; }
        public string Notes { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    // ==================== OUTCOME DTOs ====================

    /// <summary>
    /// Create/Update Meeting Outcome DTO
    /// </summary>
    public class CreateMeetingOutcomeDto
    {
        public long MeetingId { get; set; }
        public string OutcomeType { get; set; } // POSITIVE, NEUTRAL, NEGATIVE
        public string Notes { get; set; }
        public string NextAction { get; set; } // FOLLOW_UP, PROPOSAL, CLOSE, SCHEDULE_MEETING, SEND_INFO, NONE
        public DateTime? NextActionDate { get; set; }
        public long? NextActionAssignee { get; set; }
        public long RecordedBy { get; set; }
    }

    /// <summary>
    /// Update Meeting Outcome DTO
    /// </summary>
    public class UpdateMeetingOutcomeDto
    {
        public long OutcomeId { get; set; }
        public string OutcomeType { get; set; }
        public string Notes { get; set; }
        public string NextAction { get; set; }
        public DateTime? NextActionDate { get; set; }
        public long? NextActionAssignee { get; set; }
    }

    /// <summary>
    /// Meeting Outcome Response DTO
    /// </summary>
    public class MeetingOutcomeDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long OutcomeId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long MeetingId { get; set; }

        public string OutcomeType { get; set; } // POSITIVE, NEUTRAL, NEGATIVE
        public string Notes { get; set; }

        public string NextAction { get; set; }
        public DateTime? NextActionDate { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? NextActionAssignee { get; set; }

        public string NextActionAssigneeName { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? RecordedBy { get; set; }

        public string RecordedByName { get; set; }

        public DateTime RecordedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Computed
        public bool HasNextAction { get; set; }
        public bool IsNextActionOverdue { get; set; }
    }

    // ==================== CONFLICT LOCK DTOs ====================

    /// <summary>
    /// Check Availability DTO
    /// </summary>
    public class CheckAvailabilityDto
    {
        public DateTime StartDatetime { get; set; }
        public DateTime EndDatetime { get; set; }
        public string Scope { get; set; } // COMPANY, EMPLOYEE, ENTITY
        public long? ScopeId { get; set; } // NULL for COMPANY, employee_id or entity_id
        public long? ExcludeMeetingId { get; set; } // When updating a meeting
    }

    /// <summary>
    /// Availability Response DTO
    /// </summary>
    public class AvailabilityResponseDto
    {
        public bool IsAvailable { get; set; }
        public List<ConflictDto> Conflicts { get; set; }
    }

    /// <summary>
    /// Conflict DTO
    /// </summary>
    public class ConflictDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long LockId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long MeetingId { get; set; }

        public string MeetingTitle { get; set; }
        public DateTime StartDatetime { get; set; }
        public DateTime EndDatetime { get; set; }
        public string Scope { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? ScopeId { get; set; }

        public string LockDescription { get; set; }
    }

    // ==================== CALENDAR DTOs ====================

    /// <summary>
    /// Calendar Query DTO
    /// </summary>
    public class CalendarQueryDto
    {
        public long? EmployeeId { get; set; } // NULL for company-wide calendar
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string ViewType { get; set; } = "Week"; // Day, Week, Month
        public bool IncludeCancelled { get; set; } = false;
    }

    /// <summary>
    /// Calendar Event DTO (simplified meeting view for calendar)
    /// </summary>
    public class CalendarEventDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long MeetingId { get; set; }

        public string Title { get; set; }
        public DateTime StartDatetime { get; set; }
        public DateTime EndDatetime { get; set; }
        public int DurationMinutes { get; set; }

        public string MeetingType { get; set; }
        public string MeetingChannel { get; set; }
        public string Status { get; set; }

        public string EntityType { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long EntityId { get; set; }

        public string EntityName { get; set; }

        public string OrganizerName { get; set; }
        public int ParticipantCount { get; set; }

        // For employee calendar
        public string MyRole { get; set; } // ORGANIZER, ATTENDEE, OBSERVER
        public string MyAttendanceStatus { get; set; }

        // Visual indicators
        public bool IsUpcoming { get; set; }
        public bool RequiresAction { get; set; } // PENDING attendance response
    }

    /// <summary>
    /// Employee Schedule DTO
    /// </summary>
    public class EmployeeScheduleDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }

        public string EmployeeName { get; set; }
        public DateTime DateStart { get; set; }
        public DateTime DateEnd { get; set; }

        public List<CalendarEventDto> Meetings { get; set; }
        public List<TimeSlotDto> AvailableSlots { get; set; }
        public List<TimeSlotDto> BusySlots { get; set; }
    }

    /// <summary>
    /// Time Slot DTO
    /// </summary>
    public class TimeSlotDto
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsAvailable { get; set; }
        public string ConflictReason { get; set; }
    }

    // ==================== DASHBOARD DTOs ====================

    /// <summary>
    /// Meeting Statistics DTO
    /// </summary>
    public class MeetingStatisticsDto
    {
        public int TotalScheduled { get; set; }
        public int TotalCompleted { get; set; }
        public int TotalCancelled { get; set; }
        public int TotalNoShow { get; set; }

        public int UpcomingToday { get; set; }
        public int UpcomingThisWeek { get; set; }
        public int UpcomingThisMonth { get; set; }

        public decimal CompletionRate { get; set; }
        public decimal CancellationRate { get; set; }
        public decimal NoShowRate { get; set; }

        public int TotalOnsite { get; set; }
        public int TotalOnline { get; set; }
        public int TotalCall { get; set; }

        public decimal AvgMeetingDurationMinutes { get; set; }
    }

    /// <summary>
    /// Meeting Outcome Statistics DTO
    /// </summary>
    public class MeetingOutcomeStatisticsDto
    {
        public int TotalPositive { get; set; }
        public int TotalNeutral { get; set; }
        public int TotalNegative { get; set; }

        public decimal PositiveRate { get; set; }
        public decimal NeutralRate { get; set; }
        public decimal NegativeRate { get; set; }

        public int PendingFollowUps { get; set; }
        public int OverdueFollowUps { get; set; }
    }
}
