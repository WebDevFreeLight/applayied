using Microsoft.AspNetCore.Mvc;
using ZR.Admin.WebApi.Filters;
using ZR.Model;
using ZR.Model.Finance.Assets.Dto;
using ZR.Model.Finance.Exceptions;
using ZR.Service.Finance.Assets;
using ZR.Service.IService.Finance.Assets;

namespace ZR.Admin.WebApi.Controllers.Finance.Assets
{
    /// <summary>
    /// Fixed Asset Controller - تحكم الأصول الثابتة
    /// Enhanced with GL preview and comprehensive error handling
    /// </summary>
    [Verify]
    [Route("erp/assets/[controller]")]
    [ApiController]
    public class FixedAssetController : BaseController
    {
        private readonly IFixedAssetService _fixedAssetService;
        private readonly IAssetGLIntegrationService _glIntegrationService;

        public FixedAssetController(
            IFixedAssetService fixedAssetService,
            IAssetGLIntegrationService glIntegrationService)
        {
            _fixedAssetService = fixedAssetService;
            _glIntegrationService = glIntegrationService;
        }

        /// <summary>
        /// Get asset list with paging - الحصول على قائمة الأصول مع ترقيم الصفحات
        /// </summary>
        /// <param name="parm">Query parameters</param>
        /// <returns>Paged list of assets</returns>
        [HttpGet("list")]
        public IActionResult GetList([FromQuery] FixedAssetQueryDto parm)
        {
            var response = _fixedAssetService.GetList(parm);
            return SUCCESS(response);
        }

        /// <summary>
        /// Get asset by ID - الحصول على أصل بالمعرف
        /// </summary>
        /// <param name="id">Asset ID</param>
        /// <returns>Asset details</returns>
        [HttpGet("{id}")]
        public IActionResult GetById(Guid id)
        {
            var response = _fixedAssetService.GetDetail(id);
            if (response == null)
            {
                return ToResponse(ResultCode.FAIL, "Asset not found - الأصل غير موجود");
            }
            return SUCCESS(response);
        }

        /// <summary>
        /// Create asset - إنشاء أصل
        /// </summary>
        /// <param name="dto">Asset data</param>
        /// <returns>Created asset</returns>
        [HttpPost]
        [Log(Title = "Create Fixed Asset", BusinessType = BusinessType.INSERT)]
        public IActionResult Create([FromBody] CreateFixedAssetDto dto)
        {
            if (dto == null)
            {
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid data - بيانات غير صالحة");
            }

            try
            {
                var userId = HttpContext.GetName();
                var result = _fixedAssetService.Create(dto, userId);
                return SUCCESS(result);
            }
            catch (CustomException ex)
            {
                return ToResponse(ResultCode.CUSTOM_ERROR, ex.Message);
            }
        }

        /// <summary>
        /// Update asset - تحديث أصل
        /// </summary>
        /// <param name="dto">Asset data</param>
        /// <returns>Update result</returns>
        [HttpPut]
        [Log(Title = "Update Fixed Asset", BusinessType = BusinessType.UPDATE)]
        public IActionResult Update([FromBody] UpdateFixedAssetDto dto)
        {
            if (dto == null)
            {
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid data - بيانات غير صالحة");
            }

            var userId = HttpContext.GetName();
            var result = _fixedAssetService.Update(dto, userId);
            return ToResponse(result);
        }

        /// <summary>
        /// Delete assets - حذف أصول
        /// </summary>
        /// <param name="ids">Asset IDs to delete</param>
        /// <returns>Delete result</returns>
        [HttpDelete("{ids}")]
        [Log(Title = "Delete Fixed Assets", BusinessType = BusinessType.DELETE)]
        public IActionResult Delete(string ids)
        {
            var idArray = ids.Split(',').Select(Guid.Parse).ToArray();
            var result = _fixedAssetService.Delete(idArray);
            return ToResponse(result);
        }

        /// <summary>
        /// Check if asset code exists - التحقق من وجود رمز الأصل
        /// </summary>
        /// <param name="code">Asset code</param>
        /// <param name="excludeId">ID to exclude from check</param>
        /// <param name="companyId">Company ID</param>
        /// <returns>True if exists</returns>
        [HttpGet("codeExists")]
        public IActionResult CodeExists(string code, Guid? excludeId = null, long? companyId = null)
        {
            var exists = _fixedAssetService.CodeExists(code, excludeId, companyId);
            return SUCCESS(exists);
        }

        /// <summary>
        /// Calculate depreciation amount for an asset - حساب مبلغ الإهلاك للأصل
        /// </summary>
        /// <param name="id">Asset ID</param>
        /// <returns>Depreciation amount</returns>
        [HttpGet("{id}/depreciation-amount")]
        public IActionResult CalculateDepreciationAmount(Guid id)
        {
            var amount = _fixedAssetService.CalculateDepreciationAmount(id);
            return SUCCESS(amount);
        }

        /// <summary>
        /// Check if asset can be deprecated - التحقق من إمكانية إهلاك الأصل
        /// </summary>
        /// <param name="id">Asset ID</param>
        /// <returns>True if can be deprecated</returns>
        [HttpGet("{id}/can-depreciate")]
        public IActionResult CanDepreciate(Guid id)
        {
            var canDepreciate = _fixedAssetService.CanDepreciate(id);
            return SUCCESS(canDepreciate);
        }

        /// <summary>
        /// Get depreciable assets - الحصول على الأصول القابلة للإهلاك
        /// </summary>
        /// <param name="companyId">Company ID</param>
        /// <returns>List of depreciable assets</returns>
        [HttpGet("depreciable")]
        public IActionResult GetDepreciableAssets(long? companyId = null)
        {
            var response = _fixedAssetService.GetDepreciableAssets(companyId);
            return SUCCESS(response);
        }

        /// <summary>
        /// Sell asset - بيع الأصل
        /// </summary>
        /// <param name="dto">Sale data</param>
        /// <returns>Sale transaction</returns>
        [HttpPost("sell")]
        [Log(Title = "Sell Fixed Asset", BusinessType = BusinessType.UPDATE)]
        public IActionResult SellAsset([FromBody] AssetSaleDto dto)
        {
            if (dto == null)
            {
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid data - بيانات غير صالحة");
            }

            try
            {
                var userId = HttpContext.GetName();
                var result = _fixedAssetService.SellAsset(dto, userId);
                return SUCCESS(result);
            }
            catch (CustomException ex)
            {
                return ToResponse(ResultCode.CUSTOM_ERROR, ex.Message);
            }
        }

        /// <summary>
        /// Dispose asset - استبعاد الأصل
        /// </summary>
        /// <param name="dto">Disposal data</param>
        /// <returns>Disposal transaction</returns>
        [HttpPost("dispose")]
        [Log(Title = "Dispose Fixed Asset", BusinessType = BusinessType.UPDATE)]
        public IActionResult DisposeAsset([FromBody] AssetDisposalDto dto)
        {
            if (dto == null)
            {
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid data - بيانات غير صالحة");
            }

            try
            {
                var userId = HttpContext.GetName();
                var result = _fixedAssetService.DisposeAsset(dto, userId);
                return SUCCESS(result);
            }
            catch (CustomException ex)
            {
                return ToResponse(ResultCode.CUSTOM_ERROR, ex.Message);
            }
        }

        /// <summary>
        /// Revalue asset - إعادة تقييم الأصل
        /// </summary>
        /// <param name="dto">Revaluation data</param>
        /// <returns>Revaluation transaction</returns>
        [HttpPost("revalue")]
        [Log(Title = "Revalue Fixed Asset", BusinessType = BusinessType.UPDATE)]
        public IActionResult RevalueAsset([FromBody] AssetRevaluationDto dto)
        {
            if (dto == null)
            {
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid data - بيانات غير صالحة");
            }

            try
            {
                var userId = HttpContext.GetName();
                var result = _fixedAssetService.RevalueAsset(dto, userId);
                return SUCCESS(result);
            }
            catch (CustomException ex)
            {
                return ToResponse(ResultCode.CUSTOM_ERROR, ex.Message);
            }
        }

        #region GL Preview Endpoints

        /// <summary>
        /// Preview depreciation GL journal - معاينة قيد الإهلاك
        /// </summary>
        /// <param name="id">Asset ID</param>
        /// <returns>GL journal preview</returns>
        [HttpGet("{id}/preview-depreciation-journal")]
        public IActionResult PreviewDepreciationJournal(Guid id)
        {
            var asset = _fixedAssetService.GetDetail(id);
            if (asset == null)
            {
                return ToResponse(ResultCode.FAIL, "Asset not found - الأصل غير موجود");
            }

            var amount = _fixedAssetService.CalculateDepreciationAmount(id);
            if (amount <= 0)
            {
                return ToResponse(ResultCode.FAIL, "Asset cannot be depreciated - لا يمكن إهلاك الأصل");
            }

            var fullAsset = new Model.Finance.Assets.FixedAsset
            {
                Id = asset.Id,
                AssetCode = asset.AssetCode,
                NameEn = asset.NameEn,
                NameAr = asset.NameAr,
                CategoryId = asset.CategoryId,
                CurrentBookValue = asset.CurrentBookValue,
                SalvageValue = asset.SalvageValue,
                GLDepreciationExpenseAccountId = asset.GLDepreciationExpenseAccountId,
                GLAccumulatedDepreciationAccountId = asset.GLAccumulatedDepreciationAccountId
            };

            var preview = _glIntegrationService.PreviewDepreciationJournal(fullAsset, amount, DateTime.Now);
            return SUCCESS(preview);
        }

        /// <summary>
        /// Preview sale GL journal - معاينة قيد البيع
        /// </summary>
        /// <param name="id">Asset ID</param>
        /// <param name="salePrice">Sale price</param>
        /// <returns>GL journal preview</returns>
        [HttpGet("{id}/preview-sale-journal")]
        public IActionResult PreviewSaleJournal(Guid id, decimal salePrice)
        {
            var asset = _fixedAssetService.GetDetail(id);
            if (asset == null)
            {
                return ToResponse(ResultCode.FAIL, "Asset not found - الأصل غير موجود");
            }

            var fullAsset = new Model.Finance.Assets.FixedAsset
            {
                Id = asset.Id,
                AssetCode = asset.AssetCode,
                NameEn = asset.NameEn,
                NameAr = asset.NameAr,
                CategoryId = asset.CategoryId,
                PurchaseCost = asset.PurchaseCost,
                AccumulatedDepreciation = asset.AccumulatedDepreciation,
                CurrentBookValue = asset.CurrentBookValue,
                SalvageValue = asset.SalvageValue,
                GLAssetAccountId = asset.GLAssetAccountId,
                GLAccumulatedDepreciationAccountId = asset.GLAccumulatedDepreciationAccountId
            };

            var preview = _glIntegrationService.PreviewSaleJournal(fullAsset, salePrice);
            return SUCCESS(preview);
        }

        /// <summary>
        /// Preview disposal GL journal - معاينة قيد الاستبعاد
        /// </summary>
        /// <param name="id">Asset ID</param>
        /// <returns>GL journal preview</returns>
        [HttpGet("{id}/preview-disposal-journal")]
        public IActionResult PreviewDisposalJournal(Guid id)
        {
            var asset = _fixedAssetService.GetDetail(id);
            if (asset == null)
            {
                return ToResponse(ResultCode.FAIL, "Asset not found - الأصل غير موجود");
            }

            var fullAsset = new Model.Finance.Assets.FixedAsset
            {
                Id = asset.Id,
                AssetCode = asset.AssetCode,
                NameEn = asset.NameEn,
                NameAr = asset.NameAr,
                CategoryId = asset.CategoryId,
                PurchaseCost = asset.PurchaseCost,
                AccumulatedDepreciation = asset.AccumulatedDepreciation,
                CurrentBookValue = asset.CurrentBookValue,
                GLAssetAccountId = asset.GLAssetAccountId,
                GLAccumulatedDepreciationAccountId = asset.GLAccumulatedDepreciationAccountId
            };

            var preview = _glIntegrationService.PreviewDisposalJournal(fullAsset);
            return SUCCESS(preview);
        }

        /// <summary>
        /// Preview revaluation GL journal - معاينة قيد إعادة التقييم
        /// </summary>
        /// <param name="id">Asset ID</param>
        /// <param name="newValue">New value</param>
        /// <returns>GL journal preview</returns>
        [HttpGet("{id}/preview-revaluation-journal")]
        public IActionResult PreviewRevaluationJournal(Guid id, decimal newValue)
        {
            var asset = _fixedAssetService.GetDetail(id);
            if (asset == null)
            {
                return ToResponse(ResultCode.FAIL, "Asset not found - الأصل غير موجود");
            }

            var fullAsset = new Model.Finance.Assets.FixedAsset
            {
                Id = asset.Id,
                AssetCode = asset.AssetCode,
                NameEn = asset.NameEn,
                NameAr = asset.NameAr,
                CategoryId = asset.CategoryId,
                CurrentBookValue = asset.CurrentBookValue,
                SalvageValue = asset.SalvageValue,
                GLAssetAccountId = asset.GLAssetAccountId
            };

            var preview = _glIntegrationService.PreviewRevaluationJournal(fullAsset, newValue);
            return SUCCESS(preview);
        }

        #endregion
    }
}

