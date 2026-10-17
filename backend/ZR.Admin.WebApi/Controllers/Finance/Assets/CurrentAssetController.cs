using Microsoft.AspNetCore.Mvc;
using ZR.Admin.WebApi.Filters;
using ZR.Model;
using ZR.Model.Finance.Assets.Dto;
using ZR.Service.IService.Finance.Assets;

namespace ZR.Admin.WebApi.Controllers.Finance.Assets
{
    /// <summary>
    /// Current Asset Controller - تحكم الأصول المتداولة
    /// </summary>
    [Verify]
    [Route("erp/assets/[controller]")]
    [ApiController]
    public class CurrentAssetController : BaseController
    {
        private readonly ICurrentAssetService _currentAssetService;

        public CurrentAssetController(ICurrentAssetService currentAssetService)
        {
            _currentAssetService = currentAssetService;
        }

        /// <summary>
        /// Get asset list with paging - الحصول على قائمة الأصول مع ترقيم الصفحات
        /// </summary>
        /// <param name="parm">Query parameters</param>
        /// <returns>Paged list of assets</returns>
        [HttpGet("list")]
        public IActionResult GetList([FromQuery] CurrentAssetQueryDto parm)
        {
            var response = _currentAssetService.GetList(parm);
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
            var response = _currentAssetService.GetDetail(id);
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
        [Log(Title = "Create Current Asset", BusinessType = BusinessType.INSERT)]
        public IActionResult Create([FromBody] CreateCurrentAssetDto dto)
        {
            if (dto == null)
            {
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid data - بيانات غير صالحة");
            }

            var userId = HttpContext.GetName();
            var result = _currentAssetService.Create(dto, userId);
            return SUCCESS(result);
        }

        /// <summary>
        /// Update asset - تحديث أصل
        /// </summary>
        /// <param name="dto">Asset data</param>
        /// <returns>Update result</returns>
        [HttpPut]
        [Log(Title = "Update Current Asset", BusinessType = BusinessType.UPDATE)]
        public IActionResult Update([FromBody] UpdateCurrentAssetDto dto)
        {
            if (dto == null)
            {
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid data - بيانات غير صالحة");
            }

            var userId = HttpContext.GetName();
            var result = _currentAssetService.Update(dto, userId);
            return ToResponse(result);
        }

        /// <summary>
        /// Delete assets - حذف أصول
        /// </summary>
        /// <param name="ids">Asset IDs to delete</param>
        /// <returns>Delete result</returns>
        [HttpDelete("{ids}")]
        [Log(Title = "Delete Current Assets", BusinessType = BusinessType.DELETE)]
        public IActionResult Delete(string ids)
        {
            var idArray = ids.Split(',').Select(Guid.Parse).ToArray();
            var result = _currentAssetService.Delete(idArray);
            return ToResponse(result);
        }

        /// <summary>
        /// Check if item code exists - التحقق من وجود رمز الصنف
        /// </summary>
        /// <param name="code">Item code</param>
        /// <param name="excludeId">ID to exclude from check</param>
        /// <param name="companyId">Company ID</param>
        /// <returns>True if exists</returns>
        [HttpGet("codeExists")]
        public IActionResult CodeExists(string code, Guid? excludeId = null, long? companyId = null)
        {
            var exists = _currentAssetService.CodeExists(code, excludeId, companyId);
            return SUCCESS(exists);
        }

        /// <summary>
        /// Get assets by category - الحصول على الأصول حسب الفئة
        /// </summary>
        /// <param name="category">Category name</param>
        /// <param name="companyId">Company ID</param>
        /// <returns>List of assets</returns>
        [HttpGet("category/{category}")]
        public IActionResult GetByCategory(string category, long? companyId = null)
        {
            var response = _currentAssetService.GetByCategory(category, companyId);
            return SUCCESS(response);
        }

        /// <summary>
        /// Get total value of current assets - الحصول على إجمالي قيمة الأصول المتداولة
        /// </summary>
        /// <param name="companyId">Company ID</param>
        /// <returns>Total value</returns>
        [HttpGet("total-value")]
        public IActionResult GetTotalValue(long? companyId = null)
        {
            var total = _currentAssetService.GetTotalValue(companyId);
            return SUCCESS(total);
        }
    }
}

