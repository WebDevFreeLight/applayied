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
    /// Contract Amendment Controller - تعديلات العقد
    /// </summary>
    [Authorize]
    [Route("erp/contracts/amendments")]
    [ApiController]
    public class ErpContractAmendmentController : BaseController
    {
        private readonly IContractAmendmentService _amendmentService;

        public ErpContractAmendmentController(IContractAmendmentService amendmentService)
        {
            _amendmentService = amendmentService;
        }

        /// <summary>
        /// Get amendments by contract ID
        /// الحصول على التعديلات بمعرف العقد
        /// </summary>
        [HttpGet("contract/{contractId}")]
        public IActionResult GetByContractId(Guid contractId, [FromQuery] AmendmentStatus? status = null)
        {
            var result = _amendmentService.GetByContractId(contractId, status);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get amendment by ID
        /// الحصول على التعديل بالمعرف
        /// </summary>
        [HttpGet("{amendmentId}")]
        public IActionResult GetById(Guid amendmentId)
        {
            var result = _amendmentService.GetById(amendmentId);
            if (result == null)
            {
                return ToResponse(ResultCode.FAIL, "Amendment not found");
            }
            return SUCCESS(result);
        }

        /// <summary>
        /// Create amendment
        /// إنشاء تعديل
        /// </summary>
        [HttpPost]
        [Log(Title = "Contract Amendment", BusinessType = BusinessType.INSERT)]
        public IActionResult Create([FromBody] CreateErpContractAmendmentDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ToResponse(ResultCode.PARAM_ERROR);
            }

            try
            {
                var amendmentId = _amendmentService.Create(dto, HttpContext.GetName());
                return SUCCESS(new { AmendmentId = amendmentId });
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Update amendment
        /// تحديث التعديل
        /// </summary>
        [HttpPut]
        [Log(Title = "Contract Amendment", BusinessType = BusinessType.UPDATE)]
        public IActionResult Update([FromBody] UpdateErpContractAmendmentDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ToResponse(ResultCode.PARAM_ERROR);
            }

            try
            {
                var result = _amendmentService.Update(dto, HttpContext.GetName());
                return ToResponse(result);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Delete amendment
        /// حذف التعديل
        /// </summary>
        [HttpDelete("{amendmentId}")]
        [Log(Title = "Contract Amendment", BusinessType = BusinessType.DELETE)]
        public IActionResult Delete(Guid amendmentId)
        {
            try
            {
                var result = _amendmentService.Delete(amendmentId);
                return ToResponse(result);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Submit amendment for approval
        /// تقديم التعديل للموافقة
        /// </summary>
        [HttpPost("submit/{amendmentId}")]
        [Log(Title = "Contract Amendment", BusinessType = BusinessType.UPDATE)]
        public IActionResult SubmitForApproval(Guid amendmentId)
        {
            try
            {
                var result = _amendmentService.SubmitForApproval(amendmentId, HttpContext.GetName());
                return ToResponse(result);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Approve amendment
        /// الموافقة على التعديل
        /// </summary>
        [HttpPost("approve/{amendmentId}")]
        [Log(Title = "Contract Amendment", BusinessType = BusinessType.UPDATE)]
        public IActionResult Approve(Guid amendmentId)
        {
            try
            {
                var result = _amendmentService.Approve(amendmentId, HttpContext.GetName());
                return ToResponse(result);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Reject amendment
        /// رفض التعديل
        /// </summary>
        [HttpPost("reject/{amendmentId}")]
        [Log(Title = "Contract Amendment", BusinessType = BusinessType.UPDATE)]
        public IActionResult Reject(Guid amendmentId, [FromQuery] string reason)
        {
            try
            {
                var result = _amendmentService.Reject(amendmentId, HttpContext.GetName(), reason);
                return ToResponse(result);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Cancel amendment
        /// إلغاء التعديل
        /// </summary>
        [HttpPost("cancel/{amendmentId}")]
        [Log(Title = "Contract Amendment", BusinessType = BusinessType.UPDATE)]
        public IActionResult Cancel(Guid amendmentId)
        {
            try
            {
                var result = _amendmentService.Cancel(amendmentId, HttpContext.GetName());
                return ToResponse(result);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Get pending amendments count
        /// الحصول على عدد التعديلات المعلقة
        /// </summary>
        [HttpGet("pending-count")]
        public IActionResult GetPendingAmendmentsCount([FromQuery] Guid? contractId = null)
        {
            var result = _amendmentService.GetPendingAmendmentsCount(contractId);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get recent amendments
        /// الحصول على التعديلات الأخيرة
        /// </summary>
        [HttpGet("recent/{contractId}")]
        public IActionResult GetRecentAmendments(Guid contractId, [FromQuery] int count = 5)
        {
            var result = _amendmentService.GetRecentAmendments(contractId, count);
            return SUCCESS(result);
        }
    }
}
