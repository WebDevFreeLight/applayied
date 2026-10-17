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
    /// Asset Type Controller - متحكم أنواع الأصول
    /// </summary>
    [Verify]
    [Route("erp/customer-assets/types")]
    [ApiController]
    public class AssetTypeController : BaseController
    {
        private readonly IAssetTypeService _assetTypeService;

        public AssetTypeController(IAssetTypeService assetTypeService)
        {
            _assetTypeService = assetTypeService;
        }

        /// <summary>
        /// Get list of asset types - الحصول على قائمة أنواع الأصول
        /// </summary>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "erp:customer-assets:type:list")]
        public IActionResult GetList([FromQuery] AssetTypeQueryDto query, [FromQuery] PagerInfo pager)
        {
            return SUCCESS(_assetTypeService.GetList(query, pager));
        }

        /// <summary>
        /// Get asset type by ID
        /// </summary>
        [HttpGet("{id}")]
        [ActionPermissionFilter(Permission = "erp:customer-assets:type:list")]
        public IActionResult GetInfo(Guid id)
        {
            var result = _assetTypeService.GetById(id);
            if (result == null)
                return ToResponse(ResultCode.FAIL, "Asset type not found - نوع الأصل غير موجود");
            return SUCCESS(result);
        }

        /// <summary>
        /// Get select list of active asset types - قائمة منسدلة لأنواع الأصول
        /// </summary>
        [HttpGet("select")]
        [ActionPermissionFilter(Permission = "erp:customer-assets:type:list")]
        public IActionResult GetSelectList()
        {
            return SUCCESS(_assetTypeService.GetSelectList());
        }

        /// <summary>
        /// Create asset type - إنشاء نوع أصل
        /// </summary>
        [HttpPost]
        [ActionPermissionFilter(Permission = "erp:customer-assets:type:add")]
        [Log(Title = "AssetType", BusinessType = BusinessType.INSERT)]
        public IActionResult Create([FromBody] CreateAssetTypeDto dto)
        {
            if (!ModelState.IsValid)
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid parameters - معاملات غير صالحة");

            if (_assetTypeService.CodeExists(dto.Code))
                return ToResponse(ResultCode.FAIL, "Asset type code already exists - رمز نوع الأصل موجود مسبقاً");

            var result = _assetTypeService.Create(dto, HttpContext.GetName());
            return SUCCESS(result);
        }

        /// <summary>
        /// Update asset type - تحديث نوع أصل
        /// </summary>
        [HttpPut]
        [ActionPermissionFilter(Permission = "erp:customer-assets:type:edit")]
        [Log(Title = "AssetType", BusinessType = BusinessType.UPDATE)]
        public IActionResult Update([FromBody] UpdateAssetTypeDto dto)
        {
            if (!ModelState.IsValid)
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid parameters - معاملات غير صالحة");

            if (_assetTypeService.CodeExists(dto.Code, dto.Id))
                return ToResponse(ResultCode.FAIL, "Asset type code already exists - رمز نوع الأصل موجود مسبقاً");

            return ToResponse(_assetTypeService.Update(dto, HttpContext.GetName()));
        }

        /// <summary>
        /// Delete asset types - حذف أنواع الأصول
        /// </summary>
        [HttpDelete("{ids}")]
        [ActionPermissionFilter(Permission = "erp:customer-assets:type:delete")]
        [Log(Title = "AssetType", BusinessType = BusinessType.DELETE)]
        public IActionResult Delete(string ids)
        {
            var idsArr = ids.Split(',').Select(Guid.Parse).ToArray();
            if (idsArr.Length <= 0)
                return ToResponse(ResultCode.FAIL);

            return ToResponse(_assetTypeService.Delete(idsArr));
        }
    }
}
