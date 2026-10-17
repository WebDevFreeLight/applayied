using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using ZR.Admin.WebApi.Filters;
using ZR.Model;
using ZR.Model.Contracts.Dto;
using ZR.Model.Contracts.Enums;
using ZR.Service.Contracts.IService;

namespace ZR.Admin.WebApi.Controllers.Contracts
{
    /// <summary>
    /// Contract Billing Schedule Controller - جداول الفوترة
    /// </summary>
    [Authorize]
    [Route("erp/contracts/billing-schedules")]
    [ApiController]
    public class ErpContractBillingScheduleController : BaseController
    {
        private readonly IContractBillingScheduleService _billingScheduleService;

        public ErpContractBillingScheduleController(IContractBillingScheduleService billingScheduleService)
        {
            _billingScheduleService = billingScheduleService;
        }

        /// <summary>
        /// Get billing schedules by contract ID
        /// الحصول على جداول الفوترة بمعرف العقد
        /// </summary>
        [HttpGet("contract/{contractId}")]
        public IActionResult GetByContractId(Guid contractId, [FromQuery] BillingScheduleStatus? status = null)
        {
            var result = _billingScheduleService.GetByContractId(contractId, status);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get billing schedule by ID
        /// الحصول على جدول الفوترة بالمعرف
        /// </summary>
        [HttpGet("{scheduleId}")]
        public IActionResult GetById(Guid scheduleId)
        {
            var result = _billingScheduleService.GetById(scheduleId);
            if (result == null)
            {
                return ToResponse(ResultCode.FAIL, "Billing schedule not found");
            }
            return SUCCESS(result);
        }

        /// <summary>
        /// Generate billing schedules for contract
        /// إنشاء جداول الفوترة للعقد
        /// </summary>
        [HttpPost("generate")]
        [Log(Title = "Contract Billing Schedule", BusinessType = BusinessType.INSERT)]
        public IActionResult GenerateBillingSchedule([FromBody] GenerateBillingScheduleDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ToResponse(ResultCode.PARAM_ERROR);
            }

            try
            {
                var result = _billingScheduleService.GenerateBillingSchedule(dto, HttpContext.GetName());
                return SUCCESS(new { GeneratedCount = result });
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Update billing schedule
        /// تحديث جدول الفوترة
        /// </summary>
        [HttpPut]
        [Log(Title = "Contract Billing Schedule", BusinessType = BusinessType.UPDATE)]
        public IActionResult Update([FromBody] UpdateBillingScheduleDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ToResponse(ResultCode.PARAM_ERROR);
            }

            try
            {
                var result = _billingScheduleService.Update(dto, HttpContext.GetName());
                return ToResponse(result);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Delete billing schedule
        /// حذف جدول الفوترة
        /// </summary>
        [HttpDelete("{scheduleId}")]
        [Log(Title = "Contract Billing Schedule", BusinessType = BusinessType.DELETE)]
        public IActionResult Delete(Guid scheduleId)
        {
            try
            {
                var result = _billingScheduleService.Delete(scheduleId);
                return ToResponse(result);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Mark schedule as invoiced
        /// تحديد الجدول كمفوتر
        /// </summary>
        [HttpPost("mark-invoiced")]
        [Log(Title = "Contract Billing Schedule", BusinessType = BusinessType.UPDATE)]
        public IActionResult MarkAsInvoiced([FromBody] MarkInvoicedDto dto)
        {
            try
            {
                var result = _billingScheduleService.MarkAsInvoiced(dto.ScheduleId, dto.InvoiceId, dto.InvoiceNumber);
                return ToResponse(result);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Mark schedule as paid
        /// تحديد الجدول كمدفوع
        /// </summary>
        [HttpPost("mark-paid")]
        [Log(Title = "Contract Billing Schedule", BusinessType = BusinessType.UPDATE)]
        public IActionResult MarkAsPaid([FromBody] MarkPaidDto dto)
        {
            try
            {
                var result = _billingScheduleService.MarkAsPaid(dto.ScheduleId, dto.PaidDate);
                return ToResponse(result);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Cancel schedule
        /// إلغاء الجدول
        /// </summary>
        [HttpPost("cancel/{scheduleId}")]
        [Log(Title = "Contract Billing Schedule", BusinessType = BusinessType.UPDATE)]
        public IActionResult Cancel(Guid scheduleId)
        {
            try
            {
                var result = _billingScheduleService.Cancel(scheduleId, HttpContext.GetName());
                return ToResponse(result);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Skip schedule
        /// تخطي الجدول
        /// </summary>
        [HttpPost("skip/{scheduleId}")]
        [Log(Title = "Contract Billing Schedule", BusinessType = BusinessType.UPDATE)]
        public IActionResult Skip(Guid scheduleId)
        {
            try
            {
                var result = _billingScheduleService.Skip(scheduleId, HttpContext.GetName());
                return ToResponse(result);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Get due schedules for invoicing
        /// الحصول على الجداول المستحقة للفوترة
        /// </summary>
        [HttpGet("due")]
        public IActionResult GetDueSchedules([FromQuery] DateTime? asOfDate = null)
        {
            var result = _billingScheduleService.GetDueSchedules(asOfDate);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get upcoming schedules
        /// الحصول على الجداول القادمة
        /// </summary>
        [HttpGet("upcoming/{contractId}")]
        public IActionResult GetUpcomingSchedules(Guid contractId, [FromQuery] int count = 5)
        {
            var result = _billingScheduleService.GetUpcomingSchedules(contractId, count);
            return SUCCESS(result);
        }

        /// <summary>
        /// Cancel future schedules
        /// إلغاء الجداول المستقبلية
        /// </summary>
        [HttpPost("cancel-future")]
        [Log(Title = "Contract Billing Schedule", BusinessType = BusinessType.UPDATE)]
        public IActionResult CancelFutureSchedules([FromBody] CancelFutureSchedulesDto dto)
        {
            try
            {
                var result = _billingScheduleService.CancelFutureSchedules(dto.ContractId, dto.FromDate, HttpContext.GetName());
                return SUCCESS(new { CancelledCount = result });
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Get schedule statistics
        /// الحصول على إحصائيات الجداول
        /// </summary>
        [HttpGet("statistics/{contractId}")]
        public IActionResult GetStatistics(Guid contractId)
        {
            var result = _billingScheduleService.GetStatistics(contractId);
            return SUCCESS(result);
        }
    }

    /// <summary>
    /// DTO for marking schedule as invoiced
    /// </summary>
    public class MarkInvoicedDto
    {
        public Guid ScheduleId { get; set; }
        public Guid InvoiceId { get; set; }
        public string InvoiceNumber { get; set; }
    }

    /// <summary>
    /// DTO for marking schedule as paid
    /// </summary>
    public class MarkPaidDto
    {
        public Guid ScheduleId { get; set; }
        public DateTime PaidDate { get; set; }
    }

    /// <summary>
    /// DTO for cancelling future schedules
    /// </summary>
    public class CancelFutureSchedulesDto
    {
        public Guid ContractId { get; set; }
        public DateTime FromDate { get; set; }
    }
}
