using System;
using Microsoft.AspNetCore.Mvc;
using ZR.Admin.WebApi.Filters;
using ZR.Model;
using ZR.Model.CustomerAssets.Dto;
using ZR.Service.IService.CustomerAssets;

namespace ZR.Admin.WebApi.Controllers.CustomerAssets
{
    /// <summary>
    /// Customer Asset Assignment Controller - متحكم تعيينات أصول العملاء
    /// </summary>
    [Verify]
    [Route("erp/customer-assets/assignments")]
    [ApiController]
    public class CustomerAssetAssignmentController : BaseController
    {
        private readonly ICustomerAssetAssignmentService _assignmentService;
        private readonly ICustomerContractFileService _fileService;

        public CustomerAssetAssignmentController(
            ICustomerAssetAssignmentService assignmentService,
            ICustomerContractFileService fileService)
        {
            _assignmentService = assignmentService;
            _fileService = fileService;
        }

        /// <summary>
        /// Get list of assignments - الحصول على قائمة التعيينات
        /// </summary>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "erp:customer-assets:assignment:list")]
        public IActionResult GetList([FromQuery] AssignmentQueryDto query, [FromQuery] PagerInfo pager)
        {
            return SUCCESS(_assignmentService.GetList(query, pager));
        }

        /// <summary>
        /// Get assignment by ID
        /// </summary>
        [HttpGet("{id}")]
        [ActionPermissionFilter(Permission = "erp:customer-assets:assignment:list")]
        public IActionResult GetInfo(Guid id)
        {
            var result = _assignmentService.GetById(id);
            if (result == null)
                return ToResponse(ResultCode.FAIL, "Assignment not found - التعيين غير موجود");
            return SUCCESS(result);
        }

        /// <summary>
        /// Assign asset to customer - تعيين أصل لعميل
        /// </summary>
        [HttpPost("assign")]
        [ActionPermissionFilter(Permission = "erp:customer-assets:assignment:assign")]
        [Log(Title = "AssetAssignment", BusinessType = BusinessType.INSERT)]
        public IActionResult Assign([FromBody] AssignAssetDto dto)
        {
            if (!ModelState.IsValid)
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid parameters - معاملات غير صالحة");

            try
            {
                var result = _assignmentService.AssignAsset(dto, HttpContext.GetName());
                return SUCCESS(result);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Return asset - إرجاع أصل
        /// </summary>
        [HttpPost("return")]
        [ActionPermissionFilter(Permission = "erp:customer-assets:assignment:return")]
        [Log(Title = "AssetAssignment", BusinessType = BusinessType.UPDATE)]
        public IActionResult Return([FromBody] ReturnAssetDto dto)
        {
            if (!ModelState.IsValid)
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid parameters - معاملات غير صالحة");

            try
            {
                var result = _assignmentService.ReturnAsset(dto, HttpContext.GetName());
                return ToResponse(result);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Mark asset as lost - تسجيل فقدان الأصل
        /// </summary>
        [HttpPost("mark-lost")]
        [ActionPermissionFilter(Permission = "erp:customer-assets:assignment:manage")]
        [Log(Title = "AssetAssignment", BusinessType = BusinessType.UPDATE)]
        public IActionResult MarkLost([FromBody] MarkLostDamagedDto dto)
        {
            if (!ModelState.IsValid)
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid parameters - معاملات غير صالحة");

            try
            {
                var result = _assignmentService.MarkLost(dto, HttpContext.GetName());
                return ToResponse(result);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Mark asset as damaged - تسجيل تلف الأصل
        /// </summary>
        [HttpPost("mark-damaged")]
        [ActionPermissionFilter(Permission = "erp:customer-assets:assignment:manage")]
        [Log(Title = "AssetAssignment", BusinessType = BusinessType.UPDATE)]
        public IActionResult MarkDamaged([FromBody] MarkLostDamagedDto dto)
        {
            if (!ModelState.IsValid)
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid parameters - معاملات غير صالحة");

            try
            {
                var result = _assignmentService.MarkDamaged(dto, HttpContext.GetName());
                return ToResponse(result);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        #region File Endpoints

        /// <summary>
        /// Get files for an assignment - الحصول على ملفات التعيين
        /// </summary>
        [HttpGet("{assignmentId}/files")]
        [ActionPermissionFilter(Permission = "erp:customer-assets:assignment:list")]
        public IActionResult GetFiles(Guid assignmentId)
        {
            return SUCCESS(_fileService.GetByAssignment(assignmentId));
        }

        /// <summary>
        /// Add file to assignment - إضافة ملف للتعيين
        /// </summary>
        [HttpPost("files")]
        [ActionPermissionFilter(Permission = "erp:customer-assets:assignment:manage")]
        [Log(Title = "ContractFile", BusinessType = BusinessType.INSERT)]
        public IActionResult AddFile([FromBody] CreateContractFileDto dto)
        {
            if (!ModelState.IsValid)
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid parameters - معاملات غير صالحة");

            var result = _fileService.Create(dto, HttpContext.GetName());
            return SUCCESS(result);
        }

        /// <summary>
        /// Delete file - حذف ملف
        /// </summary>
        [HttpDelete("files/{id}")]
        [ActionPermissionFilter(Permission = "erp:customer-assets:assignment:manage")]
        [Log(Title = "ContractFile", BusinessType = BusinessType.DELETE)]
        public IActionResult DeleteFile(Guid id)
        {
            return ToResponse(_fileService.Delete(id));
        }

        #endregion
    }
}
