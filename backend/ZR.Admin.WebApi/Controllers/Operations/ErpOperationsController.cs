using Microsoft.AspNetCore.Mvc;
using ZR.Model.Operations;
using ZR.Service.Operations;
using ZR.Admin.WebApi.Filters;

namespace ZR.Admin.WebApi.Controllers.Operations
{
    /// <summary>
    /// Operations Controller - متحكم العمليات (الورديات، التقارير اليومية)
    /// </summary>
    [Verify]
    [Route("api/erp/operations")]
    [ApiController]
    public class ErpOperationsController : BaseController
    {
        private readonly IErpShiftService _shiftService;
        private readonly IErpDailyReportService _dailyReportService;

        public ErpOperationsController(IErpShiftService shiftService, IErpDailyReportService dailyReportService)
        {
            _shiftService = shiftService;
            _dailyReportService = dailyReportService;
        }

        #region Shifts - الورديات

        [HttpGet("shifts")]
        [ActionPermissionFilter(Permission = "erp:operations:list")]
        public IActionResult GetShifts([FromQuery] ErpShift parm, [FromQuery] Model.PagerInfo pager) => SUCCESS(_shiftService.GetList(parm, pager));

        [HttpGet("shifts/{id}")]
        [ActionPermissionFilter(Permission = "erp:operations:query")]
        public IActionResult GetShift(long id)
        {
            var response = _shiftService.GetInfo(id);
            if (response == null) return ToResponse(ResultCode.FAIL, "Shift not found - الوردية غير موجودة");
            return SUCCESS(response);
        }

        [HttpPost("shifts/open")]
        [ActionPermissionFilter(Permission = "erp:operations:shift")]
        [Log(Title = "Open Shift", BusinessType = BusinessType.INSERT)]
        public IActionResult OpenShift([FromBody] OpenShiftRequest request)
        {
            var result = _shiftService.OpenShift(request.EmployeeId, request.TreasuryId, request.OpeningBalance, request.BranchID);
            return SUCCESS(result);
        }

        [HttpPost("shifts/{id}/close")]
        [ActionPermissionFilter(Permission = "erp:operations:shift")]
        [Log(Title = "Close Shift", BusinessType = BusinessType.UPDATE)]
        public IActionResult CloseShift(long id, [FromBody] CloseShiftRequest request)
        {
            var result = _shiftService.CloseShift(id, request.ClosingBalance, request.Notes);
            return ToResponse(result);
        }

        [HttpGet("shifts/current")]
        [ActionPermissionFilter(Permission = "erp:operations:query")]
        public IActionResult GetCurrentShift([FromQuery] long employeeId)
        {
            var result = _shiftService.GetCurrentShift(employeeId);
            if (result == null) return ToResponse(ResultCode.FAIL, "No active shift - لا توجد وردية نشطة");
            return SUCCESS(result);
        }

        [HttpGet("shifts/treasury/{treasuryId}/current")]
        [ActionPermissionFilter(Permission = "erp:operations:query")]
        public IActionResult GetCurrentShiftByTreasury(long treasuryId)
        {
            var result = _shiftService.GetCurrentShiftByTreasury(treasuryId);
            if (result == null) return ToResponse(ResultCode.FAIL, "No active shift for this treasury - لا توجد وردية نشطة لهذه الخزينة");
            return SUCCESS(result);
        }

        [HttpGet("shifts/date")]
        [ActionPermissionFilter(Permission = "erp:operations:list")]
        public IActionResult GetShiftsByDate([FromQuery] DateTime date, [FromQuery] long? BranchID) => SUCCESS(_shiftService.GetShiftsByDate(date, BranchID));

        [HttpGet("shifts/{id}/summary")]
        [ActionPermissionFilter(Permission = "erp:operations:report")]
        public IActionResult GetShiftSummary(long id) => SUCCESS(_shiftService.GetShiftSummary(id));

        [HttpGet("shifts/{id}/sales")]
        [ActionPermissionFilter(Permission = "erp:operations:query")]
        public IActionResult GetShiftSales(long id)
        {
            var total = _shiftService.GetShiftSales(id);
            return SUCCESS(new { ShiftId = id, TotalSales = total });
        }

        [HttpGet("shifts/{id}/receipts")]
        [ActionPermissionFilter(Permission = "erp:operations:query")]
        public IActionResult GetShiftReceipts(long id)
        {
            var total = _shiftService.GetShiftReceipts(id);
            return SUCCESS(new { ShiftId = id, TotalReceipts = total });
        }

        [HttpGet("shifts/{id}/payments")]
        [ActionPermissionFilter(Permission = "erp:operations:query")]
        public IActionResult GetShiftPayments(long id)
        {
            var total = _shiftService.GetShiftPayments(id);
            return SUCCESS(new { ShiftId = id, TotalPayments = total });
        }

        #endregion

        #region Daily Reports - التقارير اليومية

        [HttpGet("daily-reports")]
        [ActionPermissionFilter(Permission = "erp:operations:list")]
        public IActionResult GetDailyReports([FromQuery] ErpDailyReport parm, [FromQuery] Model.PagerInfo pager) => SUCCESS(_dailyReportService.GetList(parm, pager));

        [HttpGet("daily-reports/{id}")]
        [ActionPermissionFilter(Permission = "erp:operations:query")]
        public IActionResult GetDailyReport(long id)
        {
            var response = _dailyReportService.GetInfo(id);
            if (response == null) return ToResponse(ResultCode.FAIL, "Daily report not found - التقرير اليومي غير موجود");
            return SUCCESS(response);
        }

        [HttpPost("daily-reports")]
        [ActionPermissionFilter(Permission = "erp:operations:report")]
        [Log(Title = "Generate Daily Report", BusinessType = BusinessType.INSERT)]
        public IActionResult GenerateDailyReport([FromBody] CreateDailyReportRequest request)
        {
            var result = _dailyReportService.GenerateDailyReport(request.Date, request.BranchID);
            return SUCCESS(result);
        }

        [HttpPost("daily-reports/{id}/close")]
        [ActionPermissionFilter(Permission = "erp:operations:approve")]
        [Log(Title = "Close Daily Report", BusinessType = BusinessType.UPDATE)]
        public IActionResult CloseDailyReport(long id) => ToResponse(_dailyReportService.CloseDailyReport(id, HttpContext.GetUId().ToString()));

        [HttpGet("daily-reports/date")]
        [ActionPermissionFilter(Permission = "erp:operations:query")]
        public IActionResult GetDailyReportByDate([FromQuery] DateTime date, [FromQuery] long? BranchID)
        {
            var result = _dailyReportService.GetReportByDate(date, BranchID);
            if (result == null) return ToResponse(ResultCode.FAIL, "Daily report not found - التقرير اليومي غير موجود");
            return SUCCESS(result);
        }

        [HttpGet("daily-reports/monthly-summary")]
        [ActionPermissionFilter(Permission = "erp:operations:report")]
        public IActionResult GetMonthlySummary([FromQuery] int year, [FromQuery] int month, [FromQuery] long? BranchID) => SUCCESS(_dailyReportService.GetMonthlySummary(year, month, BranchID));

        [HttpGet("daily-reports/yearly-summary")]
        [ActionPermissionFilter(Permission = "erp:operations:report")]
        public IActionResult GetYearlySummary([FromQuery] int year, [FromQuery] long? BranchID) => SUCCESS(_dailyReportService.GetYearlySummary(year, BranchID));

        #endregion

        [HttpGet("shifts/export")]
        [ActionPermissionFilter(Permission = "erp:operations:export")]
        [Log(Title = "Shift", BusinessType = BusinessType.EXPORT)]
        public IActionResult ExportShifts([FromQuery] ErpShift parm, [FromQuery] Model.PagerInfo pager)
        {
            pager.PageSize = 10000;
            var list = _shiftService.GetList(parm, pager).Result;
            var result = ExportExcelMini(list, "Shifts", "Shifts - الورديات");
            return ExportExcel(result.Item2, result.Item1);
        }

        [HttpGet("daily-reports/export")]
        [ActionPermissionFilter(Permission = "erp:operations:export")]
        [Log(Title = "Daily Report", BusinessType = BusinessType.EXPORT)]
        public IActionResult ExportDailyReports([FromQuery] ErpDailyReport parm, [FromQuery] Model.PagerInfo pager)
        {
            pager.PageSize = 10000;
            var list = _dailyReportService.GetList(parm, pager).Result;
            var result = ExportExcelMini(list, "DailyReports", "Daily Reports - التقارير اليومية");
            return ExportExcel(result.Item2, result.Item1);
        }
    }

    public class OpenShiftRequest { public long EmployeeId { get; set; } public long TreasuryId { get; set; } public decimal OpeningBalance { get; set; } public long BranchID { get; set; } }
    public class CloseShiftRequest { public decimal ClosingBalance { get; set; } public string Notes { get; set; } }
    public class CreateDailyReportRequest { public DateTime Date { get; set; } public long? BranchID { get; set; } }
}

