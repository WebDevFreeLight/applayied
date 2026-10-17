using System;
using System.Collections.Generic;
using ZR.Model;
using ZR.Model.BusinessPartners.Meetings;
using ZR.Model.BusinessPartners.Meetings.Dto;

namespace ZR.Service.BusinessPartners.Meetings
{
    /// <summary>
    /// Meeting Service Interface - Generic meeting system for Leads and Customers
    /// Purpose:
    /// - Create and manage meetings for both Leads and Customers
    /// - Prevent double-booking through conflict lock management
    /// - Track participants and their attendance
    /// - Record meeting outcomes and follow-up actions
    /// </summary>
    public interface IBpMeetingService : IBaseService<BpMeeting>
    {
        // ==================== CORE MEETING OPERATIONS ====================

        /// <summary>
        /// Get paginated meeting list with filters
        /// Response: MeetingDto with entity names, participants, and computed properties
        /// </summary>
        PagedInfo<MeetingDto> GetMeetingList(MeetingQueryDto queryDto);

        /// <summary>
        /// Get meeting by ID with full details (participants, outcome)
        /// Response: Complete MeetingDto with navigation properties populated
        /// </summary>
        MeetingDto GetMeetingDetails(long meetingId);

        /// <summary>
        /// Create a new meeting
        /// - Validates: entity existence, time range, conflict detection
        /// - Creates: meeting, conflict locks (entity + all participants), participants
        /// - Response: Created MeetingDto with generated ID
        /// </summary>
        MeetingDto CreateMeeting(CreateMeetingDto createDto);

        /// <summary>
        /// Update an existing meeting
        /// - Validates: conflict detection (excluding current meeting)
        /// - Updates: meeting details, regenerates conflict locks if time changed
        /// - Response: Updated MeetingDto
        /// </summary>
        MeetingDto UpdateMeeting(UpdateMeetingDto updateDto);

        /// <summary>
        /// Cancel a meeting
        /// - Updates: status to CANCELLED, adds cancel reason
        /// - Deletes: all conflict locks
        /// - Response: Number of records affected
        /// </summary>
        int CancelMeeting(CancelMeetingDto cancelDto);

        /// <summary>
        /// Complete a meeting
        /// - Updates: status to COMPLETED
        /// - Creates: outcome record if outcome details provided
        /// - Deletes: conflict locks
        /// - Response: MeetingDto with outcome
        /// </summary>
       MeetingDto CompleteMeeting(CompleteMeetingDto completeDto);

        /// <summary>
        /// Mark meeting as NO_SHOW
        /// - Updates: status to NO_SHOW
        /// - Deletes: conflict locks
        /// - Response: Number of records affected
        /// </summary>
        int MarkNoShow(long meetingId, long updatedBy);

        // ==================== PARTICIPANT MANAGEMENT ====================

        /// <summary>
        /// Add participant to existing meeting
        /// - Validates: employee exists, not already participant, no time conflict
        /// - Creates: participant record, employee conflict lock
        /// - Response: MeetingParticipantDto
        /// </summary>
        MeetingParticipantDto AddParticipant(AddParticipantDto addDto);

        /// <summary>
        /// Remove participant from meeting
        /// - Validates: not the organizer
        /// - Deletes: participant record, employee conflict lock
        /// - Response: Number of records affected
        /// </summary>
        int RemoveParticipant(long participantId);

        /// <summary>
        /// Update participant attendance status
        /// - Updates: attendance status, response datetime, notes
        /// - Response: Updated MeetingParticipantDto
        /// </summary>
        MeetingParticipantDto UpdateParticipantAttendance(UpdateParticipantAttendanceDto updateDto);

        /// <summary>
        /// Get all participants for a meeting
        /// Response: List of MeetingParticipantDto with employee names
        /// </summary>
        List<MeetingParticipantDto> GetMeetingParticipants(long meetingId);

        /// <summary>
        /// Get pending meeting invitations for employee
        /// Response: List of MeetingDto where employee has PENDING status
        /// </summary>
        List<MeetingDto> GetPendingInvitations(long employeeId);

        // ==================== CONFLICT DETECTION & AVAILABILITY ====================

        /// <summary>
        /// Check availability for a time slot
        /// - Checks: entity availability, employee availability (if provided)
        /// - Returns: IsAvailable flag + list of conflicts
        /// - Response: AvailabilityResponseDto with conflict details
        /// </summary>
        AvailabilityResponseDto CheckAvailability(CheckAvailabilityDto checkDto);

        /// <summary>
        /// Get available time slots for an employee on a specific date
        /// - Queries: all locks for employee on that date
        /// - Returns: List of available time slots (e.g., 30-minute intervals)
        /// - Response: List of TimeSlotDto
        /// </summary>
        List<TimeSlotDto> GetAvailableTimeSlots(long employeeId, DateTime date, int durationMinutes = 60);

        // ==================== CALENDAR & SCHEDULING ====================

        /// <summary>
        /// Get employee's meeting calendar for date range
        /// - Returns: All meetings where employee is a participant
        /// - Response: EmployeeScheduleDto with meetings and availability
        /// </summary>
        EmployeeScheduleDto GetEmployeeCalendar(CalendarQueryDto queryDto);

        /// <summary>
        /// Get company-wide meeting calendar
        /// - Returns: All scheduled meetings in date range
        /// - Response: List of CalendarEventDto
        /// </summary>
        List<CalendarEventDto> GetCompanyCalendar(CalendarQueryDto queryDto);

        /// <summary>
        /// Get upcoming meetings for entity (Lead or Customer)
        /// Response: List of MeetingDto for specific entity
        /// </summary>
        List<MeetingDto> GetEntityUpcomingMeetings(string entityType, long entityId);

        /// <summary>
        /// Get upcoming meetings for employee
        /// Response: List of MeetingDto where employee is participant
        /// </summary>
        List<MeetingDto> GetEmployeeUpcomingMeetings(long employeeId, int daysAhead = 7);

        // ==================== OUTCOME MANAGEMENT ====================

        /// <summary>
        /// Create or update meeting outcome
        /// - Creates: outcome record linked to meeting
        /// - Response: MeetingOutcomeDto
        /// </summary>
        MeetingOutcomeDto CreateOrUpdateOutcome(CreateMeetingOutcomeDto createDto);

        /// <summary>
        /// Get pending follow-ups for employee
        /// Response: List of MeetingOutcomeDto with pending next actions
        /// </summary>
        List<MeetingOutcomeDto> GetPendingFollowUps(long? employeeId = null, bool overdueOnly = false);

        /// <summary>
        /// Get meeting outcome
        /// Response: MeetingOutcomeDto for specific meeting
        /// </summary>
        MeetingOutcomeDto GetMeetingOutcome(long meetingId);

        // ==================== STATISTICS & REPORTING ====================

        /// <summary>
        /// Get meeting statistics for period
        /// Response: MeetingStatisticsDto with counts and rates
        /// </summary>
        MeetingStatisticsDto GetMeetingStatistics(DateTime? startDate = null, DateTime? endDate = null, long? employeeId = null);

        /// <summary>
        /// Get outcome statistics for period
        /// Response: MeetingOutcomeStatisticsDto with outcome distribution
        /// </summary>
        MeetingOutcomeStatisticsDto GetOutcomeStatistics(DateTime? startDate = null, DateTime? endDate = null, long? employeeId = null);

        // ==================== HELPER METHODS ====================

        /// <summary>
        /// Validate entity exists (Lead or Customer)
        /// Returns: true if entity exists in respective table
        /// </summary>
        bool ValidateEntityExists(string entityType, long entityId);

        /// <summary>
        /// Get entity name for display
        /// Returns: Lead brand name or Customer partner name
        /// </summary>
        string GetEntityName(string entityType, long entityId);
    }
}
