using Microsoft.AspNetCore.Mvc;
using ZR.Admin.WebApi.Filters;
using ZR.Model;
using ZR.Model.Finance.Assets.Dto;
using ZR.Service.IService.Finance.Assets;

namespace ZR.Admin.WebApi.Controllers.Finance.Assets
{
    /// <summary>
    /// Depreciation Controller - تحكم الإهلاك
    /// </summary>
    [Verify]
    [Route("erp/assets/[controller]")]
    [ApiController]
    public class DepreciationController : BaseController
    {
        private readonly IDepreciationService _depreciationService;

        public DepreciationController(IDepreciationService depreciationService)
        {
            _depreciationService = depreciationService;
        }

        /// <summary>
        /// Get depreciation records with paging - الحصول على سجلات الإهلاك مع ترقيم الصفحات 0 0
        /// </summary>
        /// <param name="parm">Query parameters</param>
        /// <returns>Paged list of depreciation records</returns>
        [HttpGet("list")]
        public IActionResult GetList([FromQuery] AssetDepreciationQueryDto parm)
        {
            var response = _depreciationService.GetList(parm);
            return SUCCESS(response);
        }

        /// <summary>
        /// Get depreciation history for an asset - الحصول على تاريخ الإهلاك لأصل معين
        /// </summary>
        /// <param name="assetId">Asset ID</param>
        /// <returns>List of depreciation records</returns>
        [HttpGet("asset/{assetId}/history")]
        public IActionResult GetAssetDepreciationHistory(Guid assetId)
        {
            var response = _depreciationService.GetAssetDepreciationHistory(assetId);
            return SUCCESS(response);
        }

        /// <summary>
        /// Generate monthly depreciation for all active assets - توليد الإهلاك الشهري لجميع الأصول النشطة
        /// </summary>
        /// <param name="dto">Monthly depreciation generation data</param>
        /// <returns>Depreciation summary</returns>
        [HttpPost("generate-monthly")]
        [Log(Title = "Generate Monthly Depreciation", BusinessType = BusinessType.INSERT)]
        public IActionResult GenerateMonthlyDepreciation([FromBody] GenerateMonthlyDepreciationDto dto)
        {
            if (dto == null)
            {
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid data - بيانات غير صالحة");
            }

            var userId = HttpContext.GetName();
            var result = _depreciationService.GenerateMonthlyDepreciation(dto, userId);
            return SUCCESS(result);
        }

        /// <summary>
        /// Generate depreciation for a single asset - توليد الإهلاك لأصل واحد
        /// </summary>
        /// <param name="dto">Depreciation record data</param>
        /// <returns>Created depreciation record</returns>
        [HttpPost("depreciate")]
        [Log(Title = "Depreciate Asset", BusinessType = BusinessType.INSERT)]
        public IActionResult DepreciateAsset([FromBody] CreateDepreciationRecordDto dto)
        {
            if (dto == null)
            {
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid data - بيانات غير صالحة");
            }

            var userId = HttpContext.GetName();
            var result = _depreciationService.DepreciateAsset(dto, userId);
            return SUCCESS(result);
        }

        /// <summary>
        /// Check if depreciation exists for period - التحقق من وجود إهلاك للفترة
        /// </summary>
        /// <param name="assetId">Asset ID</param>
        /// <param name="fiscalYear">Fiscal year</param>
        /// <param name="fiscalPeriod">Fiscal period</param>
        /// <returns>True if exists</returns>
        [HttpGet("exists")]
        public IActionResult DepreciationExistsForPeriod(Guid assetId, int fiscalYear, int fiscalPeriod)
        {
            var exists = _depreciationService.DepreciationExistsForPeriod(assetId, fiscalYear, fiscalPeriod);
            return SUCCESS(exists);
        }

        /// <summary>
        /// Get total depreciation for an asset - الحصول على إجمالي الإهلاك لأصل
        /// </summary>
        /// <param name="assetId">Asset ID</param>
        /// <returns>Total depreciation amount</returns>
        [HttpGet("asset/{assetId}/total")]
        public IActionResult GetTotalDepreciation(Guid assetId)
        {
            var total = _depreciationService.GetTotalDepreciation(assetId);
            return SUCCESS(total);
        }

        /// <summary>
        /// Get depreciation summary by period - الحصول على ملخص الإهلاك بالفترة
        /// </summary>
        /// <param name="fiscalYear">Fiscal year</param>
        /// <param name="fiscalPeriod">Fiscal period</param>
        /// <param name="companyId">Company ID</param>
        /// <returns>Depreciation summary</returns>
        [HttpGet("summary")]
        public IActionResult GetPeriodSummary(int fiscalYear, int fiscalPeriod, long? companyId = null)
        {
            var summary = _depreciationService.GetPeriodSummary(fiscalYear, fiscalPeriod, companyId);
            return SUCCESS(summary);
        }
    }
}

