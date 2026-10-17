using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using ZR.Admin.WebApi.Filters;
using ZR.Model;
using ZR.Model.CustomerAssets.Dto;
using ZR.Service.IService.CustomerAssets;

namespace ZR.Admin.WebApi.Controllers.CustomerAssets
{
    /// <summary>
    /// Customer Asset Inventory Controller - متحكم مخزون أصول العملاء
    /// </summary>
    [Verify]
    [Route("erp/customer-assets/inventory")]
    [ApiController]
    public class CustomerAssetController : BaseController
    {
        private readonly ICustomerAssetService _customerAssetService;

        public CustomerAssetController(ICustomerAssetService customerAssetService)
        {
            _customerAssetService = customerAssetService;
        }

        /// <summary>
        /// Get list of assets - الحصول على قائمة الأصول
        /// </summary>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "erp:customer-assets:inventory:list")]
        public IActionResult GetList([FromQuery] CustomerAssetQueryDto query, [FromQuery] PagerInfo pager)
        {
            return SUCCESS(_customerAssetService.GetList(query, pager));
        }

        /// <summary>
        /// Get asset by ID
        /// </summary>
        [HttpGet("{id}")]
        [ActionPermissionFilter(Permission = "erp:customer-assets:inventory:list")]
        public IActionResult GetInfo(Guid id)
        {
            var result = _customerAssetService.GetById(id);
            if (result == null)
                return ToResponse(ResultCode.FAIL, "Asset not found - الأصل غير موجود");
            return SUCCESS(result);
        }

        /// <summary>
        /// Get available assets for assignment - الأصول المتاحة للتعيين
        /// </summary>
        [HttpGet("available")]
        [ActionPermissionFilter(Permission = "erp:customer-assets:inventory:list")]
        public IActionResult GetAvailable()
        {
            return SUCCESS(_customerAssetService.GetAvailableAssets());
        }

        /// <summary>
        /// Generate next asset code - توليد رمز الأصل التالي
        /// </summary>
        [HttpGet("next-code")]
        [ActionPermissionFilter(Permission = "erp:customer-assets:inventory:add")]
        public IActionResult GetNextCode()
        {
            return SUCCESS(new { code = _customerAssetService.GenerateAssetCode() });
        }

        /// <summary>
        /// Create asset - إنشاء أصل
        /// </summary>
        [HttpPost]
        [ActionPermissionFilter(Permission = "erp:customer-assets:inventory:add")]
        [Log(Title = "CustomerAsset", BusinessType = BusinessType.INSERT)]
        public IActionResult Create([FromBody] CreateCustomerAssetDto dto)
        {
            if (!ModelState.IsValid)
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid parameters - معاملات غير صالحة");

            var result = _customerAssetService.Create(dto, HttpContext.GetName());
            return SUCCESS(result);
        }

        /// <summary>
        /// Update asset - تحديث أصل
        /// </summary>
        [HttpPut]
        [ActionPermissionFilter(Permission = "erp:customer-assets:inventory:edit")]
        [Log(Title = "CustomerAsset", BusinessType = BusinessType.UPDATE)]
        public IActionResult Update([FromBody] UpdateCustomerAssetDto dto)
        {
            if (!ModelState.IsValid)
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid parameters - معاملات غير صالحة");

            return ToResponse(_customerAssetService.Update(dto, HttpContext.GetName()));
        }

        /// <summary>
        /// Delete assets - حذف أصول
        /// </summary>
        [HttpDelete("{ids}")]
        [ActionPermissionFilter(Permission = "erp:customer-assets:inventory:delete")]
        [Log(Title = "CustomerAsset", BusinessType = BusinessType.DELETE)]
        public IActionResult Delete(string ids)
        {
            var idsArr = ids.Split(',').Select(Guid.Parse).ToArray();
            if (idsArr.Length <= 0)
                return ToResponse(ResultCode.FAIL);

            var result = _customerAssetService.Delete(idsArr);
            if (result == 0)
                return ToResponse(ResultCode.FAIL, "Cannot delete assigned assets - لا يمكن حذف أصول مُعيّنة");

            return SUCCESS(result);
        }
    }
}
