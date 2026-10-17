using Microsoft.AspNetCore.Mvc;
using ZR.Model;
using ZR.Model.Subscriptions;
using ZR.Service.Subscriptions.IService;
using ZR.Admin.WebApi.Filters;
using ZR.Common;
using Infrastructure.Enums;

namespace ZR.Admin.WebApi.Controllers.Subscriptions
{
    /// <summary>
    /// Activity Management - إدارة الأنشطة
    /// </summary>
    [Verify]
    [Route("api/erp/activities")]
    [ApiController]
    public class ErpActivityController : BaseController
    {
        private readonly IErpActivityService _activityService;

        public ErpActivityController(IErpActivityService activityService)
        {
            _activityService = activityService;
        }

        /// <summary>
        /// Get activity list - قائمة الأنشطة
        /// </summary>
        [HttpGet("list")]
        public IActionResult GetList([FromQuery] ErpActivity parm, [FromQuery] PagerInfo pager)
        {
            var response = _activityService.GetList(parm, pager);
            return SUCCESS(response);
        }

        /// <summary>
        /// Get activity by ID - الحصول على نشاط بالمعرف
        /// </summary>
        [HttpGet("{activityId}")]
        public IActionResult GetInfo(long activityId)
        {
            var response = _activityService.GetInfo(activityId);
            return SUCCESS(response);
        }

        /// <summary>
        /// Create activity - إنشاء نشاط
        /// </summary>
        [HttpPost]
        [Log(Title = "Create Activity", BusinessType = BusinessType.INSERT)]
        public IActionResult Create([FromBody] ErpActivity activity)
        {
            activity.Create_by = HttpContext.GetName();
            var response = _activityService.AddActivity(activity);
            return SUCCESS(response);
        }

        /// <summary>
        /// Update activity - تحديث نشاط
        /// </summary>
        [HttpPut]
        [Log(Title = "Update Activity", BusinessType = BusinessType.UPDATE)]
        public IActionResult Update([FromBody] ErpActivity activity)
        {
            activity.Update_by = HttpContext.GetName();
            activity.Update_time = DateTime.Now;
            var response = _activityService.UpdateActivity(activity);
            return ToResponse(response);
        }

        /// <summary>
        /// Delete activities - حذف الأنشطة
        /// </summary>
        [HttpDelete("{ids}")]
        [Log(Title = "Delete Activities", BusinessType = BusinessType.DELETE)]
        public IActionResult Delete(string ids)
        {
            long[] idArr = Tools.SpitLongArrary(ids);
            var response = _activityService.Delete(idArr);
            return ToResponse(response);
        }

        /// <summary>
        /// Get activities by category - الأنشطة حسب الفئة
        /// </summary>
        [HttpGet("by-category/{categoryId}")]
        public IActionResult GetByCategory(long categoryId)
        {
            var response = _activityService.GetActivitiesByCategory(categoryId);
            return SUCCESS(response);
        }

        /// <summary>
        /// Get activities by resource - الأنشطة حسب المورد
        /// </summary>
        [HttpGet("by-resource/{resourceId}")]
        public IActionResult GetByResource(long resourceId)
        {
            var response = _activityService.GetActivitiesByResource(resourceId);
            return SUCCESS(response);
        }

        /// <summary>
        /// Get activity schedule - جدول النشاط
        /// </summary>
        [HttpGet("schedule/{activityId}")]
        public IActionResult GetSchedule(long activityId, [FromQuery] DateTime? date)
        {
            var response = _activityService.GetActivitySchedule(activityId, date ?? DateTime.Today);
            return SUCCESS(response);
        }

        /// <summary>
        /// Get weekly schedule - الجدول الأسبوعي
        /// </summary>
        [HttpGet("weekly-schedule")]
        public IActionResult GetWeeklySchedule([FromQuery] long? branchId, [FromQuery] DateTime? startDate)
        {
            var response = _activityService.GetWeeklySchedule(branchId, startDate ?? DateTime.Today);
            return SUCCESS(response);
        }

        /// <summary>
        /// Register member for activity - تسجيل عضو في نشاط
        /// </summary>
        [HttpPost("register")]
        [Log(Title = "Register for Activity", BusinessType = BusinessType.INSERT)]
        public IActionResult RegisterMember([FromBody] ActivityRegistrationDto dto)
        {
            var response = _activityService.RegisterMember(dto.ActivityId, dto.MemberId, dto.SessionDate);
            return ToResponse(response);
        }

        /// <summary>
        /// Cancel registration - إلغاء التسجيل
        /// </summary>
        [HttpPost("cancel-registration")]
        [Log(Title = "Cancel Activity Registration", BusinessType = BusinessType.DELETE)]
        public IActionResult CancelRegistration([FromBody] ActivityRegistrationDto dto)
        {
            var response = _activityService.CancelRegistration(dto.ActivityId, dto.MemberId, dto.SessionDate);
            return ToResponse(response);
        }

        /////// <summary>
        /////// Record attendance - تسجيل الحضور
        /////// </summary>
        ////[HttpPost("attendance")]
        ////[Log(Title = "Record Activity Attendance", BusinessType = BusinessType.INSERT)]
        ////public IActionResult RecordAttendance([FromBody] AttendanceDto dto)
        ////{
        ////    var response = _activityService.RecordActivityAttendance(dto.ActivityId, dto.MemberId, dto.SessionDate, dto.Status);
        ////    return ToResponse(response);
        ////}

        /// <summary>
        /// Get session attendees - الحاضرين في الجلسة
        /// </summary>
        [HttpGet("attendees/{activityId}")]
        public IActionResult GetSessionAttendees(long activityId, [FromQuery] DateTime sessionDate)
        {
            var response = _activityService.GetSessionAttendees(activityId, sessionDate);
            return SUCCESS(response);
        }

        /// <summary>
        /// Get activity statistics - إحصائيات النشاط
        /// </summary>
        [HttpGet("statistics/{activityId}")]
        public IActionResult GetStatistics(long activityId)
        {
            var response = _activityService.GetActivityStatistics(activityId);
            return SUCCESS(response);
        }

        /// <summary>
        /// Get available slots - الأماكن المتاحة
        /// </summary>
        [HttpGet("available-slots/{activityId}")]
        public IActionResult GetAvailableSlots(long activityId, [FromQuery] DateTime sessionDate)
        {
            var response = _activityService.GetAvailableSlots(activityId, sessionDate);
            return SUCCESS(response);
        }
    }

    public class ActivityRegistrationDto
    {
        public long ActivityId { get; set; }
        public long MemberId { get; set; }
        public DateTime SessionDate { get; set; }
    }

    public class AttendanceDto
    {
        public long ActivityId { get; set; }
        public long MemberId { get; set; }
        public DateTime SessionDate { get; set; }
        public string Status { get; set; } = "present";
    }
}

