using Microsoft.AspNetCore.Mvc;
using ZR.Admin.WebApi.Filters;
using ZR.Model.BusinessPartners.Meetings.Dto;
using ZR.Service.BusinessPartners.Meetings;

namespace ZR.Admin.WebApi.Controllers.BusinessPartners.Meetings
{
    /// <summary>
    /// Meeting Management API - Generic meeting system for Leads and Customers
    /// إدارة الاجتماعات - نظام اجتماعات عام للعملاء المحتملين والعملاء الفعليين
    /// </summary>
    [Verify]
    [Route("api/marketing/meetings")]
    public class BpMeetingController : BaseController
    {
        private readonly IBpMeetingService _meetingService;

        public BpMeetingController(IBpMeetingService meetingService)
        {
            _meetingService = meetingService;
        }

        // ==================== CORE MEETING OPERATIONS ====================

        /// <summary>
        /// Get paginated meeting list with filters
        /// الحصول على قائمة الاجتماعات مع الفلاتر
        /// </summary>
        [HttpGet("list")]
        [ActionPermissionFilter("marketing:meetings:list")]
        public IActionResult GetMeetingList([FromQuery] MeetingQueryDto queryDto)
        {
            var result = _meetingService.GetMeetingList(queryDto);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get meeting details by ID
        /// الحصول على تفاصيل الاجتماع
        /// </summary>
        [HttpGet("{meetingId}")]
        [ActionPermissionFilter("marketing:meetings:view")]
        public IActionResult GetMeetingDetails(long meetingId)
        {
            var result = _meetingService.GetMeetingDetails(meetingId);
            if (result == null)
                return ToResponse(ResultCode.FAIL, "Meeting not found");

            return SUCCESS(result);
        }

        /// <summary>
        /// Create a new meeting
        /// إنشاء اجتماع جديد
        /// </summary>
        [HttpPost("create")]
        [ActionPermissionFilter("marketing:meetings:add")]
        public IActionResult CreateMeeting([FromBody] CreateMeetingDto createDto)
        {
            try
            {
                var result = _meetingService.CreateMeeting(createDto);
                return SUCCESS(result, "Meeting created successfully");
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, $"Error creating meeting: {ex.Message}");
            }
        }

        /// <summary>
        /// Update meeting details
        /// تحديث تفاصيل الاجتماع
        /// </summary>
        [HttpPost("update")]
        [ActionPermissionFilter("marketing:meetings:edit")]
        public IActionResult UpdateMeeting([FromBody] UpdateMeetingDto updateDto)
        {
            try
            {
                var result = _meetingService.UpdateMeeting(updateDto);
                return SUCCESS(result, "Meeting updated successfully");
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, $"Error updating meeting: {ex.Message}");
            }
        }

        /// <summary>
        /// Cancel a meeting
        /// إلغاء اجتماع
        /// </summary>
        [HttpPost("cancel")]
        [ActionPermissionFilter("marketing:meetings:cancel")]
        public IActionResult CancelMeeting([FromBody] CancelMeetingDto cancelDto)
        {
            try
            {
                var result = _meetingService.CancelMeeting(cancelDto);
                return SUCCESS(new { cancelledCount = result }, "Meeting cancelled successfully");
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, $"Error cancelling meeting: {ex.Message}");
            }
        }

        /// <summary>
        /// Complete a meeting with optional outcome
        /// إكمال اجتماع مع النتيجة
        /// </summary>
        [HttpPost("complete")]
        [ActionPermissionFilter("marketing:meetings:complete")]
        public IActionResult CompleteMeeting([FromBody] CompleteMeetingDto completeDto)
        {
            try
            {
                var result = _meetingService.CompleteMeeting(completeDto);
                return SUCCESS(result, "Meeting completed successfully");
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, $"Error completing meeting: {ex.Message}");
            }
        }

        /// <summary>
        /// Mark meeting as no-show
        /// تمييز الاجتماع كعدم حضور
        /// </summary>
        [HttpPost("no-show/{meetingId}")]
        [ActionPermissionFilter("marketing:meetings:edit")]
        public IActionResult MarkNoShow(long meetingId, [FromBody] long updatedBy)
        {
            try
            {
                var result = _meetingService.MarkNoShow(meetingId, updatedBy);
                return SUCCESS(new { updatedCount = result }, "Meeting marked as no-show");
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, $"Error marking meeting as no-show: {ex.Message}");
            }
        }

        // ==================== PARTICIPANT MANAGEMENT ====================

        /// <summary>
        /// Get all participants for a meeting
        /// الحصول على جميع المشاركين في الاجتماع
        /// </summary>
        [HttpGet("{meetingId}/participants")]
        [ActionPermissionFilter("marketing:meetings:view")]
        public IActionResult GetMeetingParticipants(long meetingId)
        {
            var result = _meetingService.GetMeetingParticipants(meetingId);
            return SUCCESS(result);
        }

        /// <summary>
        /// Add participant to meeting
        /// إضافة مشارك إلى الاجتماع
        /// </summary>
        [HttpPost("participants/add")]
        [ActionPermissionFilter("marketing:meetings:edit")]
        public IActionResult AddParticipant([FromBody] AddParticipantDto addDto)
        {
            try
            {
                var result = _meetingService.AddParticipant(addDto);
                return SUCCESS(result, "Participant added successfully");
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, $"Error adding participant: {ex.Message}");
            }
        }

        /// <summary>
        /// Remove participant from meeting
        /// إزالة مشارك من الاجتماع
        /// </summary>
        [HttpPost("participants/remove/{participantId}")]
        [ActionPermissionFilter("marketing:meetings:edit")]
        public IActionResult RemoveParticipant(long participantId)
        {
            try
            {
                var result = _meetingService.RemoveParticipant(participantId);
                return SUCCESS(new { removedCount = result }, "Participant removed successfully");
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, $"Error removing participant: {ex.Message}");
            }
        }

        /// <summary>
        /// Update participant attendance status
        /// تحديث حالة حضور المشارك
        /// </summary>
        [HttpPost("participants/update-attendance")]
        [ActionPermissionFilter("marketing:meetings:edit")]
        public IActionResult UpdateParticipantAttendance([FromBody] UpdateParticipantAttendanceDto updateDto)
        {
            try
            {
                var result = _meetingService.UpdateParticipantAttendance(updateDto);
                return SUCCESS(result, "Attendance status updated successfully");
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, $"Error updating attendance: {ex.Message}");
            }
        }

        /// <summary>
        /// Get pending meeting invitations for employee
        /// الحصول على دعوات الاجتماعات المعلقة للموظف
        /// </summary>
        [HttpGet("pending-invitations/{employeeId}")]
        [ActionPermissionFilter("marketing:meetings:view")]
        public IActionResult GetPendingInvitations(long employeeId)
        {
            var result = _meetingService.GetPendingInvitations(employeeId);
            return SUCCESS(result);
        }

        // ==================== CONFLICT DETECTION & AVAILABILITY ====================

        /// <summary>
        /// Check availability for a time slot
        /// التحقق من التوفر لفترة زمنية
        /// </summary>
        [HttpPost("check-availability")]
        [ActionPermissionFilter("marketing:meetings:view")]
        public IActionResult CheckAvailability([FromBody] CheckAvailabilityDto checkDto)
        {
            var result = _meetingService.CheckAvailability(checkDto);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get available time slots for employee on a specific date
        /// الحصول على الفترات الزمنية المتاحة للموظف في تاريخ محدد
        /// </summary>
        [HttpGet("available-slots/{employeeId}")]
        [ActionPermissionFilter("marketing:meetings:view")]
        public IActionResult GetAvailableTimeSlots(long employeeId, [FromQuery] DateTime date, [FromQuery] int durationMinutes = 60)
        {
            var result = _meetingService.GetAvailableTimeSlots(employeeId, date, durationMinutes);
            return SUCCESS(result);
        }

        // ==================== CALENDAR & SCHEDULING ====================

        /// <summary>
        /// Get employee's meeting calendar
        /// الحصول على تقويم اجتماعات الموظف
        /// </summary>
        [HttpGet("calendar/employee")]
        [ActionPermissionFilter("marketing:meetings:view")]
        public IActionResult GetEmployeeCalendar([FromQuery] CalendarQueryDto queryDto)
        {
            var result = _meetingService.GetEmployeeCalendar(queryDto);

            if (result == null)
                return ToResponse(ResultCode.FAIL, "Employee ID is required");

            return SUCCESS(result);
        }

        /// <summary>
        /// Get company-wide meeting calendar
        /// الحصول على تقويم اجتماعات الشركة
        /// </summary>
        [HttpGet("calendar/company")]
        [ActionPermissionFilter("marketing:meetings:view")]
        public IActionResult GetCompanyCalendar([FromQuery] CalendarQueryDto queryDto)
        {
            var result = _meetingService.GetCompanyCalendar(queryDto);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get upcoming meetings for entity (Lead or Customer)
        /// الحصول على الاجتماعات القادمة للكيان (عميل محتمل أو عميل فعلي)
        /// </summary>
        [HttpGet("entity/{entityType}/{entityId}/upcoming")]
        [ActionPermissionFilter("marketing:meetings:view")]
        public IActionResult GetEntityUpcomingMeetings(string entityType, long entityId)
        {
            var result = _meetingService.GetEntityUpcomingMeetings(entityType, entityId);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get upcoming meetings for employee
        /// الحصول على الاجتماعات القادمة للموظف
        /// </summary>
        [HttpGet("employee/{employeeId}/upcoming")]
        [ActionPermissionFilter("marketing:meetings:view")]
        public IActionResult GetEmployeeUpcomingMeetings(long employeeId, [FromQuery] int daysAhead = 7)
        {
            var result = _meetingService.GetEmployeeUpcomingMeetings(employeeId, daysAhead);
            return SUCCESS(result);
        }

        // ==================== OUTCOME MANAGEMENT ====================

        /// <summary>
        /// Create or update meeting outcome
        /// إنشاء أو تحديث نتيجة الاجتماع
        /// </summary>
        [HttpPost("outcome")]
        [ActionPermissionFilter("marketing:meetings:edit")]
        public IActionResult CreateOrUpdateOutcome([FromBody] CreateMeetingOutcomeDto createDto)
        {
            try
            {
                var result = _meetingService.CreateOrUpdateOutcome(createDto);
                return SUCCESS(result, "Outcome saved successfully");
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, $"Error saving outcome: {ex.Message}");
            }
        }

        /// <summary>
        /// Get meeting outcome
        /// الحصول على نتيجة الاجتماع
        /// </summary>
        [HttpGet("{meetingId}/outcome")]
        [ActionPermissionFilter("marketing:meetings:view")]
        public IActionResult GetMeetingOutcome(long meetingId)
        {
            var result = _meetingService.GetMeetingOutcome(meetingId);

            if (result == null)
                return ToResponse(ResultCode.FAIL, "Outcome not found");

            return SUCCESS(result);
        }

        /// <summary>
        /// Get pending follow-ups
        /// الحصول على المتابعات المعلقة
        /// </summary>
        [HttpGet("follow-ups/pending")]
        [ActionPermissionFilter("marketing:meetings:view")]
        public IActionResult GetPendingFollowUps([FromQuery] long? employeeId = null, [FromQuery] bool overdueOnly = false)
        {
            var result = _meetingService.GetPendingFollowUps(employeeId, overdueOnly);
            return SUCCESS(result);
        }

        // ==================== STATISTICS & REPORTING ====================

        /// <summary>
        /// Get meeting statistics
        /// الحصول على إحصائيات الاجتماعات
        /// </summary>
        [HttpGet("statistics")]
        [ActionPermissionFilter("marketing:meetings:stats")]
        public IActionResult GetMeetingStatistics([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null, [FromQuery] long? employeeId = null)
        {
            var result = _meetingService.GetMeetingStatistics(startDate, endDate, employeeId);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get outcome statistics
        /// الحصول على إحصائيات النتائج
        /// </summary>
        [HttpGet("statistics/outcomes")]
        [ActionPermissionFilter("marketing:meetings:stats")]
        public IActionResult GetOutcomeStatistics([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null, [FromQuery] long? employeeId = null)
        {
            var result = _meetingService.GetOutcomeStatistics(startDate, endDate, employeeId);
            return SUCCESS(result);
        }

        // ==================== HELPER ENDPOINTS ====================

        /// <summary>
        /// Validate entity exists (Lead or Customer)
        /// التحقق من وجود الكيان
        /// </summary>
        [HttpGet("validate-entity/{entityType}/{entityId}")]
        [ActionPermissionFilter("marketing:meetings:view")]
        public IActionResult ValidateEntityExists(string entityType, long entityId)
        {
            var result = _meetingService.ValidateEntityExists(entityType, entityId);
            return SUCCESS(new { exists = result, entityType, entityId });
        }
    }
}
