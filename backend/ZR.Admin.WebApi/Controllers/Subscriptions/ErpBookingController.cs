using Microsoft.AspNetCore.Mvc;
using ZR.Model.Subscriptions;
using ZR.Service.Subscriptions.IService;
using ZR.Admin.WebApi.Filters;

namespace ZR.Admin.WebApi.Controllers.Subscriptions
{
    /// <summary>
    /// Booking Controller - متحكم الحجوزات
    /// </summary>
    [Verify]
    [Route("api/erp/bookings")]
    [ApiController]
    public class ErpBookingController : BaseController
    {
        private readonly IErpBookingService _bookingService;

        public ErpBookingController(IErpBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        /// <summary>
        /// Get booking list - قائمة الحجوزات
        /// </summary>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "erp:booking:list")]
        public IActionResult GetList([FromQuery] ErpBooking parm, [FromQuery] Model.PagerInfo pager)
        {
            var response = _bookingService.GetList(parm, pager);
            return SUCCESS(response);
        }

        /// <summary>
        /// Get booking by ID - الحصول على حجز
        /// </summary>
        [HttpGet("{id}")]
        [ActionPermissionFilter(Permission = "erp:booking:query")]
        public IActionResult GetInfo(long id)
        {
            var response = _bookingService.GetInfo(id);
            if (response == null)
                return ToResponse(ResultCode.FAIL, "Booking not found - الحجز غير موجود");
            return SUCCESS(response);
        }

        /// <summary>
        /// Create booking - إنشاء حجز
        /// </summary>
        [HttpPost]
        [ActionPermissionFilter(Permission = "erp:booking:add")]
        [Log(Title = "Booking", BusinessType = BusinessType.INSERT)]
        public IActionResult Create([FromBody] ErpBooking model)
        {
            var result = _bookingService.CreateBooking(model);
            return SUCCESS(result);
        }

        /// <summary>
        /// Update booking - تعديل حجز
        /// </summary>
        [HttpPut]
        [ActionPermissionFilter(Permission = "erp:booking:edit")]
        [Log(Title = "Booking", BusinessType = BusinessType.UPDATE)]
        public IActionResult Update([FromBody] ErpBooking model)
        {
            var result = _bookingService.UpdateBooking(model);
            return ToResponse(result);
        }

        /// <summary>
        /// Delete bookings - حذف حجوزات
        /// </summary>
        [HttpDelete("{ids}")]
        [ActionPermissionFilter(Permission = "erp:booking:delete")]
        [Log(Title = "Booking", BusinessType = BusinessType.DELETE)]
        public IActionResult Delete(string ids)
        {
            long[] idsArr = Tools.SpitLongArrary(ids);
            if (idsArr.Length <= 0) return ToResponse(ResultCode.FAIL);
            return ToResponse(_bookingService.Delete(idsArr));
        }

        /// <summary>
        /// Confirm booking - تأكيد الحجز
        /// </summary>
        [HttpPost("{id}/confirm")]
        [ActionPermissionFilter(Permission = "erp:booking:confirm")]
        [Log(Title = "Confirm Booking", BusinessType = BusinessType.UPDATE)]
        public IActionResult Confirm(long id)
        {
            var result = _bookingService.ConfirmBooking(id, HttpContext.GetUId().ToString());
            return ToResponse(result);
        }

        /// <summary>
        /// Cancel booking - إلغاء الحجز
        /// </summary>
        [HttpPost("{id}/cancel")]
        [ActionPermissionFilter(Permission = "erp:booking:cancel")]
        [Log(Title = "Cancel Booking", BusinessType = BusinessType.UPDATE)]
        public IActionResult Cancel(long id, [FromBody] CancelBookingRequest request)
        {
            var result = _bookingService.CancelBooking(id, request.Reason);
            return ToResponse(result);
        }

        /// <summary>
        /// Complete booking - إكمال الحجز
        /// </summary>
        [HttpPost("{id}/complete")]
        [ActionPermissionFilter(Permission = "erp:booking:complete")]
        [Log(Title = "Complete Booking", BusinessType = BusinessType.UPDATE)]
        public IActionResult Complete(long id)
        {
            var result = _bookingService.CompleteBooking(id);
            return ToResponse(result);
        }

        /// <summary>
        /// Check-in booking - تسجيل وصول
        /// </summary>
        [HttpPost("{id}/checkin")]
        [ActionPermissionFilter(Permission = "erp:booking:checkin")]
        public IActionResult CheckIn(long id)
        {
            var result = _bookingService.CheckIn(id);
            return ToResponse(result);
        }

        /// <summary>
        /// Mark as no-show - تسجيل عدم حضور
        /// </summary>
        [HttpPost("{id}/noshow")]
        [ActionPermissionFilter(Permission = "erp:booking:edit")]
        public IActionResult NoShow(long id)
        {
            var result = _bookingService.NoShow(id);
            return ToResponse(result);
        }

        /// <summary>
        /// Get member bookings - حجوزات العضو
        /// </summary>
        [HttpGet("member/{memberId}")]
        [ActionPermissionFilter(Permission = "erp:booking:list")]
        public IActionResult GetMemberBookings(long memberId, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            var result = _bookingService.GetMemberBookings(memberId, fromDate, toDate);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get resource bookings - حجوزات المورد
        /// </summary>
        [HttpGet("resource/{resourceId}")]
        [ActionPermissionFilter(Permission = "erp:booking:list")]
        public IActionResult GetResourceBookings(long resourceId, [FromQuery] DateTime date)
        {
            var result = _bookingService.GetResourceBookings(resourceId, date);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get staff bookings - حجوزات الموظف
        /// </summary>
        [HttpGet("staff/{staffId}")]
        [ActionPermissionFilter(Permission = "erp:booking:list")]
        public IActionResult GetStaffBookings(long staffId, [FromQuery] DateTime date)
        {
            var result = _bookingService.GetStaffBookings(staffId, date);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get available slots - المواعيد المتاحة
        /// </summary>
        [HttpGet("available-slots")]
        [ActionPermissionFilter(Permission = "erp:booking:list")]
        public IActionResult GetAvailableSlots([FromQuery] long resourceId, [FromQuery] DateTime date)
        {
            var result = _bookingService.GetAvailableSlots(resourceId, date);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get today's bookings - حجوزات اليوم
        /// </summary>
        [HttpGet("today")]
        [ActionPermissionFilter(Permission = "erp:booking:list")]
        public IActionResult GetTodayBookings([FromQuery] long? BranchID)
        {
            var result = _bookingService.GetTodayBookings(BranchID);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get upcoming bookings - الحجوزات القادمة
        /// </summary>
        [HttpGet("upcoming")]
        [ActionPermissionFilter(Permission = "erp:booking:list")]
        public IActionResult GetUpcomingBookings([FromQuery] long? BranchID = null, [FromQuery] int days = 7)
        {
            var result = _bookingService.GetUpcomingBookings(BranchID, days);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get total booking revenue - إجمالي إيرادات الحجوزات
        /// </summary>
        [HttpGet("reports/revenue")]
        [ActionPermissionFilter(Permission = "erp:booking:report")]
        public IActionResult GetTotalBookingRevenue([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, [FromQuery] long? BranchID)
        {
            var total = _bookingService.GetTotalBookingRevenue(fromDate, toDate, BranchID);
            return SUCCESS(new { TotalRevenue = total, FromDate = fromDate, ToDate = toDate });
        }

        /// <summary>
        /// Export bookings - تصدير الحجوزات
        /// </summary>
        [HttpGet("export")]
        [ActionPermissionFilter(Permission = "erp:booking:export")]
        [Log(Title = "Booking", BusinessType = BusinessType.EXPORT)]
        public IActionResult Export([FromQuery] ErpBooking parm, [FromQuery] Model.PagerInfo pager)
        {
            pager.PageSize = 10000;
            var list = _bookingService.GetList(parm, pager).Result;
            var result = ExportExcelMini(list, "Bookings", "Bookings - الحجوزات");
            return ExportExcel(result.Item2, result.Item1);
        }
    }

    public class CancelBookingRequest { public string Reason { get; set; } }
}

