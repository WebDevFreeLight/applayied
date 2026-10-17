using Microsoft.AspNetCore.Mvc;
using ZR.Model;
using ZR.Model.Operations;
using ZR.Service.Operations;
using ZR.Admin.WebApi.Filters;
using ZR.Common;
using Infrastructure.Enums;
namespace ZR.Admin.WebApi.Controllers.Operations
{
    public class OpenShiftDto
    {
        public long EmployeeId { get; set; }
        public long TreasuryId { get; set; }
        public decimal OpeningBalance { get; set; }
        public long BranchID { get; set; }
    }

    public class CloseShiftDto
    {
        public decimal ClosingBalance { get; set; }
        public string Notes { get; set; }
    }

    /// <summary>
    /// Shift Management - إدارة الورديات
    /// </summary>
    [Verify]
    [Route("api/erp/shifts")]
    [ApiController]
    public class ErpShiftController : BaseController
    {
        private readonly IErpShiftService _shiftService;

        public ErpShiftController(IErpShiftService shiftService)
        {
            _shiftService = shiftService;
        }

        /// <summary>
        /// Get shift list - قائمة الورديات
        /// </summary>
        [HttpGet("list")]
        public IActionResult GetList([FromQuery] ErpShift parm, [FromQuery] PagerInfo pager)
        {
            var response = _shiftService.GetList(parm, pager);
            return SUCCESS(response);
        }

        /// <summary>
        /// Get shift by ID - الحصول على وردية بالمعرف
        /// </summary>
        [HttpGet("{shiftId}")]
        public IActionResult GetInfo(long shiftId)
        {
            var response = _shiftService.GetInfo(shiftId);
            return SUCCESS(response);
        }

        /// <summary>
        /// Get current shift by employee - الوردية الحالية للموظف
        /// </summary>
        [HttpGet("current/employee/{employeeId}")]
        public IActionResult GetCurrentShift(long employeeId)
        {
            var response = _shiftService.GetCurrentShift(employeeId);
            return SUCCESS(response);
        }

        /// <summary>
        /// Get current shift by treasury - الوردية الحالية للخزينة
        /// </summary>
        [HttpGet("current/treasury/{treasuryId}")]
        public IActionResult GetCurrentShiftByTreasury(long treasuryId)
        {
            var response = _shiftService.GetCurrentShiftByTreasury(treasuryId);
            return SUCCESS(response);
        }

        /// <summary>
        /// Open new shift - فتح وردية جديدة
        /// </summary>
        [HttpPost("open")]
        [Log(Title = "Open Shift", BusinessType = BusinessType.INSERT)]
        public IActionResult OpenShift([FromBody] OpenShiftDto dto)
        {
            var response = _shiftService.OpenShift(dto.EmployeeId, dto.TreasuryId, dto.OpeningBalance, dto.BranchID);
            return SUCCESS(response);
        }

        /// <summary>
        /// Close shift - إغلاق الوردية
        /// </summary>
        [HttpPost("close/{shiftId}")]
        [Log(Title = "Close Shift", BusinessType = BusinessType.UPDATE)]
        public IActionResult CloseShift(long shiftId, [FromBody] CloseShiftDto dto)
        {
            var response = _shiftService.CloseShift(shiftId, dto.ClosingBalance, dto.Notes);
            return ToResponse(response);
        }

        /// <summary>
        /// Get shift summary - ملخص الوردية
        /// </summary>
        [HttpGet("summary/{shiftId}")]
        public IActionResult GetShiftSummary(long shiftId)
        {
            var response = _shiftService.GetShiftSummary(shiftId);
            return SUCCESS(response);
        }

        /// <summary>
        /// Get shifts by date - الورديات حسب التاريخ
        /// </summary>
        [HttpGet("by-date")]
        public IActionResult GetShiftsByDate([FromQuery] DateTime date, [FromQuery] long? branchId)
        {
            var response = _shiftService.GetShiftsByDate(date, branchId);
            return SUCCESS(response);
        }

        /// <summary>
        /// Get shift sales - مبيعات الوردية
        /// </summary>
        [HttpGet("sales/{shiftId}")]
        public IActionResult GetShiftSales(long shiftId)
        {
            var response = _shiftService.GetShiftSales(shiftId);
            return SUCCESS(new { ShiftId = shiftId, TotalSales = response });
        }

        /// <summary>
        /// Get shift receipts - مقبوضات الوردية
        /// </summary>
        [HttpGet("receipts/{shiftId}")]
        public IActionResult GetShiftReceipts(long shiftId)
        {
            var response = _shiftService.GetShiftReceipts(shiftId);
            return SUCCESS(new { ShiftId = shiftId, TotalReceipts = response });
        }

        /// <summary>
        /// Get shift payments - مدفوعات الوردية
        /// </summary>
        [HttpGet("payments/{shiftId}")]
        public IActionResult GetShiftPayments(long shiftId)
        {
            var response = _shiftService.GetShiftPayments(shiftId);
            return SUCCESS(new { ShiftId = shiftId, TotalPayments = response });
        }
    }

  
}

