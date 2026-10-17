using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using ZR.Admin.WebApi.Filters;
using ZR.Model;
using ZR.Model.Finance.ARAP.Dto;
using ZR.Service.IService.Finance.ARAP;

namespace ZR.Admin.WebApi.Controllers.Finance.ARAP
{
    /// <summary>
    /// Vendor Controller - متحكم الموردين
    /// </summary>
    [Verify]
    [Route("finance/arap/vendors")]
    [ApiController]
    public class VendorController : BaseController
    {
        private readonly IVendorService _vendorService;

        public VendorController(IVendorService vendorService)
        {
            _vendorService = vendorService;
        }

        /// <summary>
        /// Get list of vendors - الحصول على قائمة الموردين
        /// </summary>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "finance:arap:vendor:list")]
        public IActionResult GetList([FromQuery] VendorQueryDto query, [FromQuery] PagerInfo pager)
        {
            return SUCCESS(_vendorService.GetList(query, pager));
        }

        /// <summary>
        /// Get vendor by ID - الحصول على المورد بالمعرف
        /// </summary>
        [HttpGet("{id}")]
        [ActionPermissionFilter(Permission = "finance:arap:vendor:query")]
        public IActionResult GetInfo(Guid id)
        {
            var response = _vendorService.GetById(id);
            if (response == null)
                return ToResponse(ResultCode.FAIL, "Vendor not found - المورد غير موجود");
            return SUCCESS(response);
        }

        /// <summary>
        /// Create vendor - إنشاء مورد
        /// </summary>
        [HttpPost]
        [ActionPermissionFilter(Permission = "finance:arap:vendor:add")]
        [Log(Title = "Vendor", BusinessType = BusinessType.INSERT)]
        public IActionResult Create([FromBody] CreateVendorDto dto)
        {
            if (!ModelState.IsValid)
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid parameters - معاملات غير صالحة");

            if (_vendorService.CodeExists(dto.VendorCode))
                return ToResponse(ResultCode.FAIL, "Vendor code already exists - رمز المورد موجود مسبقاً");

            var result = _vendorService.Create(dto, HttpContext.GetName());
            return SUCCESS(result);
        }

        /// <summary>
        /// Update vendor - تحديث مورد
        /// </summary>
        [HttpPut]
        [ActionPermissionFilter(Permission = "finance:arap:vendor:edit")]
        [Log(Title = "Vendor", BusinessType = BusinessType.UPDATE)]
        public IActionResult Update([FromBody] UpdateVendorDto dto)
        {
            if (!ModelState.IsValid)
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid parameters - معاملات غير صالحة");

            if (_vendorService.CodeExists(dto.VendorCode, dto.Id))
                return ToResponse(ResultCode.FAIL, "Vendor code already exists - رمز المورد موجود مسبقاً");

            return ToResponse(_vendorService.Update(dto, HttpContext.GetName()));
        }

        /// <summary>
        /// Delete vendors - حذف موردين
        /// </summary>
        [HttpDelete("{ids}")]
        [ActionPermissionFilter(Permission = "finance:arap:vendor:delete")]
        [Log(Title = "Vendor", BusinessType = BusinessType.DELETE)]
        public IActionResult Delete(string ids)
        {
            var idsArr = ids.Split(',').Select(Guid.Parse).ToArray();
            if (idsArr.Length <= 0)
                return ToResponse(ResultCode.FAIL);

            var result = _vendorService.Delete(idsArr);
            if (result == 0)
                return ToResponse(ResultCode.FAIL, "Cannot delete vendors with invoices - لا يمكن حذف موردين لديهم فواتير");

            return ToResponse(result);
        }

        /// <summary>
        /// Check if code exists - التحقق من وجود رمز المورد
        /// </summary>
        [HttpGet("check-code")]
        [ActionPermissionFilter(Permission = "finance:arap:vendor:query")]
        public IActionResult CheckCodeExists([FromQuery] string code, [FromQuery] Guid? excludeId)
        {
            var exists = _vendorService.CodeExists(code, excludeId);
            return SUCCESS(new { Exists = exists });
        }

        /// <summary>
        /// Get active vendors for dropdown - الحصول على الموردين النشطين للقائمة المنسدلة
        /// </summary>
        [HttpGet("active")]
        [ActionPermissionFilter(Permission = "finance:arap:vendor:query")]
        public IActionResult GetActiveVendors()
        {
            var result = _vendorService.GetActiveVendors();
            return SUCCESS(result);
        }

        /// <summary>
        /// Generate next vendor code - توليد رمز المورد التالي
        /// </summary>
        [HttpGet("next-code")]
        [ActionPermissionFilter(Permission = "finance:arap:vendor:query")]
        public IActionResult GetNextVendorCode()
        {
            var code = _vendorService.GenerateNextVendorCode();
            return SUCCESS(new { VendorCode = code });
        }
    }
}


