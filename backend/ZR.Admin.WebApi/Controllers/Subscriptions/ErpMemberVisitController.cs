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
    /// Member Visit Management - إدارة زيارات الأعضاء
    /// </summary>
    [Verify]
    [Route("api/erp/member-visits")]
    [ApiController]
    public class ErpMemberVisitController : BaseController
    {
        private readonly IErpMemberVisitService _visitService;

        public ErpMemberVisitController(IErpMemberVisitService visitService)
        {
            _visitService = visitService;
        }

        /// <summary>
        /// Get visit list - قائمة الزيارات
        /// </summary>
        [HttpGet("list")]
        public IActionResult GetList([FromQuery] ErpMemberVisit parm, [FromQuery] PagerInfo pager)
        {
            var response = _visitService.GetList(parm, pager);
            return SUCCESS(response);
        }

        /// <summary>
        /// Get visit by ID - الحصول على زيارة بالمعرف
        /// </summary>
        [HttpGet("{visitId}")]
        public IActionResult GetInfo(long visitId)
        {
            var response = _visitService.GetInfo(visitId);
            return SUCCESS(response);
        }

        /// <summary>
        /// Member check-in - تسجيل دخول العضو
        /// </summary>
        [HttpPost("check-in")]
        [Log(Title = "Member Check-in", BusinessType = BusinessType.INSERT)]
        public IActionResult CheckIn([FromBody] CheckInDto dto)
        {
            try
            {
                var response = _visitService.CheckIn(dto.MemberId, dto.BranchId, dto.CheckInMethod, dto.DeviceId);
                return SUCCESS(response, "تم تسجيل الدخول بنجاح - Check-in successful");
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.CUSTOM_ERROR, ex.Message);
            }
        }

        /// <summary>
        /// Member check-out - تسجيل خروج العضو
        /// </summary>
        [HttpPost("check-out/{visitId}")]
        [Log(Title = "Member Check-out", BusinessType = BusinessType.UPDATE)]
        public IActionResult CheckOut(long visitId, [FromBody] CheckOutDto dto)
        {
            var response = _visitService.CheckOut(visitId, dto.CheckOutMethod);
            return ToResponse(response);
        }

        /// <summary>
        /// Quick check-in by member ID - تسجيل دخول سريع بمعرف العضو
        /// </summary>
        [HttpPost("quick-check-in/{memberId}")]
        [Log(Title = "Quick Check-in", BusinessType = BusinessType.INSERT)]
        public IActionResult QuickCheckIn(long memberId, [FromQuery] long branchId)
        {
            try
            {
                var response = _visitService.CheckIn(memberId, branchId, "quick", null);
                return SUCCESS(response);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.CUSTOM_ERROR, ex.Message);
            }
        }

        /// <summary>
        /// Get active visit - الحصول على الزيارة النشطة
        /// </summary>
        [HttpGet("active/{memberId}")]
        public IActionResult GetActiveVisit(long memberId)
        {
            var response = _visitService.GetActiveVisit(memberId);
            return SUCCESS(response);
        }

        /// <summary>
        /// Get member visits - زيارات العضو
        /// </summary>
        [HttpGet("member/{memberId}")]
        public IActionResult GetMemberVisits(long memberId, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            var response = _visitService.GetMemberVisits(memberId, fromDate, toDate);
            return SUCCESS(response);
        }

        /// <summary>
        /// Get today's visits - زيارات اليوم
        /// </summary>
        [HttpGet("today")]
        public IActionResult GetTodayVisits([FromQuery] long? branchId)
        {
            var response = _visitService.GetTodayVisits(branchId);
            return SUCCESS(response);
        }

        /// <summary>
        /// Get current occupancy - الإشغال الحالي
        /// </summary>
        [HttpGet("occupancy/{branchId}")]
        public IActionResult GetCurrentOccupancy(long branchId)
        {
            var response = _visitService.GetCurrentOccupancy(branchId);
            return SUCCESS(response);
        }

        /// <summary>
        /// Get member visit statistics - إحصائيات زيارات العضو
        /// </summary>
        [HttpGet("statistics/{memberId}")]
        public IActionResult GetVisitStatistics(long memberId)
        {
            var response = _visitService.GetVisitStatistics(memberId);
            return SUCCESS(response);
        }

        /// <summary>
        /// Get branch statistics - إحصائيات الفرع
        /// </summary>
        [HttpGet("branch-statistics/{branchId}")]
        public IActionResult GetBranchStatistics(long branchId, [FromQuery] DateTime date)
        {
            var response = _visitService.GetBranchStatistics(branchId, date);
            return SUCCESS(response);
        }

        /// <summary>
        /// Get peak hours - ساعات الذروة
        /// </summary>
        [HttpGet("peak-hours/{branchId}")]
        public IActionResult GetPeakHours(long branchId, [FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
        {
            var response = _visitService.GetPeakHours(branchId, fromDate, toDate);
            return SUCCESS(response);
        }

        /// <summary>
        /// Get inactive members - الأعضاء غير النشطين
        /// </summary>
        [HttpGet("inactive")]
        public IActionResult GetInactiveMembers([FromQuery] int daysInactive = 30, [FromQuery] long? branchId = null)
        {
            var response = _visitService.GetInactiveMembers(daysInactive, branchId);
            return SUCCESS(response);
        }

        /// <summary>
        /// Validate member access - التحقق من صلاحية دخول العضو
        /// </summary>
        [HttpGet("validate-access/{memberId}")]
        public IActionResult ValidateMemberAccess(long memberId)
        {
            var isValid = _visitService.ValidateMemberAccess(memberId);
            return SUCCESS(new { MemberId = memberId, IsValid = isValid });
        }
    }

    public class CheckInDto
    {
        public long MemberId { get; set; }
        public long BranchId { get; set; }
        public string CheckInMethod { get; set; } = "manual";
        public string DeviceId { get; set; }
    }

    public class CheckOutDto
    {
        public string CheckOutMethod { get; set; } = "manual";
    }
}

