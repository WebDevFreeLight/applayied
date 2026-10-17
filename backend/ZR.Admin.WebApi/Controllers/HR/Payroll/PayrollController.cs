using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Infrastructure;
using Infrastructure.Attribute;
using Infrastructure.Enums;
using Infrastructure.Model;
using ZR.Admin.WebApi.Extensions;
using ZR.Admin.WebApi.Filters;
using ZR.Common;
using ZR.Model.HR.Dto;
using ZR.Service.HR.IService;

namespace ZR.Admin.WebApi.Controllers.HR.Payroll
{
    /// <summary>
    /// Payroll Management Controller
    /// </summary>
    [Verify]
    [Route("api/hr/payroll")]
    [ApiExplorerSettings(GroupName = "hr")]
    public class PayrollController : BaseController
    {
        private readonly IPayrollService _payrollService;

        public PayrollController(IPayrollService payrollService)
        {
            _payrollService = payrollService;
        }

        /// <summary>
        /// Get payroll list
        /// </summary>
        [HttpGet("list")]
        [ActionPermissionFilter("hr:payroll:list")]
        public async Task<IActionResult> GetPayrollList([FromQuery] PayrollQueryDto query)
        {
            var result = await _payrollService.GetPayrollListAsync(query);
            return SUCCESS(result);
        }

        /// <summary>
        /// Calculate payroll
        /// </summary>
        [HttpPost("calculate")]
        [ActionPermissionFilter("hr:payroll:calculate")]
        [Log(Title = "Payroll Calculation", BusinessType = BusinessType.OTHER)]
        public async Task<IActionResult> CalculatePayroll([FromBody] PayrollCalculationDto calculation)
        {
            var result = await _payrollService.CalculatePayrollAsync(calculation);
            return SUCCESS(result);
        }

        /// <summary>
        /// Process bulk payroll
        /// </summary>
        [HttpPost("bulk-process")]
        [ActionPermissionFilter("hr:payroll:process")]
        [Log(Title = "Bulk Payroll Processing", BusinessType = BusinessType.OTHER)]
        public async Task<IActionResult> ProcessBulkPayroll([FromBody] BulkPayrollDto bulk)
        {
            var result = await _payrollService.ProcessBulkPayrollAsync(bulk);
            return SUCCESS(result);
        }

        /// <summary>
        /// Approve payroll
        /// </summary>
        [HttpPost("{payrollId}/approve")]
        [ActionPermissionFilter("hr:payroll:approve")]
        [Log(Title = "Payroll Approval", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> ApprovePayroll(long payrollId)
        {
            var approvedBy = HttpContext.GetName();
            var result = await _payrollService.ApprovePayrollAsync(payrollId, approvedBy);
            return SUCCESS(result);
        }

        /// <summary>
        /// Process payment
        /// </summary>
        [HttpPost("{payrollId}/payment")]
        [ActionPermissionFilter("hr:payroll:payment")]
        [Log(Title = "Payroll Payment", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> ProcessPayment(long payrollId, [FromBody] PaymentDto payment)
        {
            var result = await _payrollService.ProcessPaymentAsync(payrollId, payment.PaymentMethod, payment.BankReference);
            return SUCCESS(result);
        }

        /// <summary>
        /// Generate payslip
        /// </summary>
        [HttpGet("{payrollId}/payslip")]
        [ActionPermissionFilter("hr:payroll:payslip")]
        public async Task<IActionResult> GeneratePayslip(long payrollId)
        {
            var bytes = await _payrollService.GeneratePayslipAsync(payrollId);
            if (bytes == null || bytes.Length == 0)
            {
                return ToResponse(ResultCode.FAIL, "Payslip not found");
            }

            // NOTE: service currently returns placeholder "PDF bytes" (JSON bytes). We still return as a file download.
            var fileName = $"payslip_{payrollId}.pdf";
            return File(bytes, "application/pdf", fileName);
        }

        /// <summary>
        /// Get payslip details
        /// </summary>
        [HttpGet("{payrollId}/payslip-details")]
        [ActionPermissionFilter("hr:payroll:view")]
        public async Task<IActionResult> GetPayslipDetails(long payrollId)
        {
            var result = await _payrollService.GetPayslipDetailsAsync(payrollId);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get GOSI compliance report
        /// </summary>
        [HttpGet("gosi-report")]
        [ActionPermissionFilter("hr:payroll:report")]
        public async Task<IActionResult> GetGOSIReport([FromQuery] string payPeriod)
        {
            var result = await _payrollService.GetGOSIComplianceReportAsync(payPeriod);
            return SUCCESS(result);
        }

        /// <summary>
        /// Generate bank transfer file
        /// </summary>
        [HttpPost("bank-transfer")]
        [ActionPermissionFilter("hr:payroll:transfer")]
        [Log(Title = "Bank Transfer Generation", BusinessType = BusinessType.EXPORT)]
        public async Task<IActionResult> GenerateBankTransfer([FromBody] BankTransferRequestDto request)
        {
            var result = await _payrollService.GenerateBankTransferFileAsync(request.PayPeriod, request.BankCode);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get payroll summary
        /// </summary>
        [HttpGet("summary")]
        [ActionPermissionFilter("hr:payroll:view")]
        public async Task<IActionResult> GetPayrollSummary([FromQuery] string payPeriod, [FromQuery] long? departmentId)
        {
            var result = await _payrollService.GetPayrollSummaryAsync(payPeriod, departmentId);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get payroll statistics
        /// </summary>
        [HttpGet("statistics")]
        [ActionPermissionFilter("hr:payroll:view")]
        public async Task<IActionResult> GetPayrollStatistics([FromQuery] string payPeriod)
        {
            var result = await _payrollService.GetPayrollStatisticsAsync(payPeriod);
            return SUCCESS(result);
        }

        /// <summary>
        /// Download Saudi WPS bank file (SIF format)
        /// </summary>
        [HttpGet("wps-file")]
        [ActionPermissionFilter("hr:payroll:transfer")]
        [Log(Title = "WPS File Generation", BusinessType = BusinessType.EXPORT)]
        public async Task<IActionResult> DownloadWPSFile([FromQuery] string payPeriod, [FromQuery] string bankCode = "DEFAULT")
        {
            var bytes = await _payrollService.GenerateWPSFileAsync(payPeriod, bankCode);
            if (bytes == null || bytes.Length == 0)
            {
                return ToResponse(ResultCode.FAIL, "No approved payroll records found for this period");
            }

            var fileName = $"WPS_{payPeriod}_{bankCode}.sif";
            return File(bytes, "text/csv", fileName);
        }

        /// <summary>
        /// Calculate End of Service Benefit
        /// </summary>
        [HttpGet("{employeeId}/esb")]
        [ActionPermissionFilter("hr:payroll:esb")]
        public async Task<IActionResult> CalculateESB(long employeeId, [FromQuery] string terminationType = "termination")
        {
            var result = await _payrollService.CalculateESBAsync(employeeId, terminationType);
            return SUCCESS(result);
        }

        /// <summary>
        /// Export payroll report
        /// </summary>
        [HttpGet("export")]
        [ActionPermissionFilter("hr:payroll:export")]
        [Log(Title = "Payroll Export", BusinessType = BusinessType.EXPORT)]
        public async Task<IActionResult> ExportPayroll([FromQuery] PayrollQueryDto query)
        {
            var list = await _payrollService.GetPayrollListAsync(query) as dynamic;
            
            var result = ExportExcelMini(list.Result, "payroll", "Payroll Report");

            return ExportExcel(result.Item2, result.Item1);
        }
    }

    public class PaymentDto
    {
        public string PaymentMethod { get; set; }
        public string BankReference { get; set; }
    }

    public class BankTransferRequestDto
    {
        public string PayPeriod { get; set; }
        public string BankCode { get; set; }
    }
}
