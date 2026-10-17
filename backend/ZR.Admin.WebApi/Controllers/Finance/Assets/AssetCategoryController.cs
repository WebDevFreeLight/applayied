using Microsoft.AspNetCore.Mvc;
using ZR.Admin.WebApi.Filters;
using ZR.Model;
using ZR.Model.Finance.Assets.Dto;
using ZR.Service.IService.Finance.Assets;

namespace ZR.Admin.WebApi.Controllers.Finance.Assets
{
    /// <summary>
    /// Asset Category Controller - تحكم فئات الأصول
    /// </summary>
    [Verify]
    [Route("erp/assets/[controller]")]
    [ApiController]
    public class AssetCategoryController : BaseController
    {
        private readonly IAssetCategoryService _assetCategoryService;

        public AssetCategoryController(IAssetCategoryService assetCategoryService)
        {
            _assetCategoryService = assetCategoryService;
        }

        /// <summary>
        /// Get category list with paging - الحصول على قائمة الفئات مع ترقيم الصفحات
        /// </summary>
        /// <param name="parm">Query parameters</param>
        /// <returns>Paged list of categories</returns>
        [HttpGet("list")]
        public IActionResult GetList([FromQuery] AssetCategoryQueryDto parm)
        {
            var response = _assetCategoryService.GetList(parm);
            return SUCCESS(response);
        }

        /// <summary>
        /// Get category by ID - الحصول على فئة بالمعرف
        /// </summary>
        /// <param name="id">Category ID</param>
        /// <returns>Category details</returns>
        [HttpGet("{id}")]
        public IActionResult GetById(Guid id)
        {
            var response = _assetCategoryService.GetDetail(id);
            if (response == null)
            {
                return ToResponse(ResultCode.FAIL, "Category not found - الفئة غير موجودة");
            }
            return SUCCESS(response);
        }

        /// <summary>
        /// Create category - إنشاء فئة
        /// </summary>
        /// <param name="dto">Category data</param>
        /// <returns>Created category</returns>
        [HttpPost]
        [Log(Title = "Create Asset Category", BusinessType = BusinessType.INSERT)]
        public IActionResult Create([FromBody] CreateAssetCategoryDto dto)
        {
            if (dto == null)
            {
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid data - بيانات غير صالحة");
            }

            var userId = HttpContext.GetName();
            var result = _assetCategoryService.Create(dto, userId);
            return SUCCESS(result);
        }

        /// <summary>
        /// Update category - تحديث فئة
        /// </summary>
        /// <param name="dto">Category data</param>
        /// <returns>Update result</returns>
        [HttpPut]
        [Log(Title = "Update Asset Category", BusinessType = BusinessType.UPDATE)]
        public IActionResult Update([FromBody] UpdateAssetCategoryDto dto)
        {
            if (dto == null)
            {
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid data - بيانات غير صالحة");
            }

            var userId = HttpContext.GetName();
            var result = _assetCategoryService.Update(dto, userId);
            return ToResponse(result);
        }

        /// <summary>
        /// Delete categories - حذف فئات
        /// </summary>
        /// <param name="ids">Category IDs to delete</param>
        /// <returns>Delete result</returns>
        [HttpDelete("{ids}")]
        [Log(Title = "Delete Asset Categories", BusinessType = BusinessType.DELETE)]
        public IActionResult Delete(string ids)
        {
            var idArray = ids.Split(',').Select(Guid.Parse).ToArray();
            var result = _assetCategoryService.Delete(idArray);
            return ToResponse(result);
        }

        /// <summary>
        /// Check if category code exists - التحقق من وجود رمز الفئة
        /// </summary>
        /// <param name="code">Category code</param>
        /// <param name="excludeId">ID to exclude from check</param>
        /// <param name="companyId">Company ID</param>
        /// <returns>True if exists</returns>
        [HttpGet("codeExists")]
        public IActionResult CodeExists(string code, Guid? excludeId = null, long? companyId = null)
        {
            var exists = _assetCategoryService.CodeExists(code, excludeId, companyId);
            return SUCCESS(exists);
        }

        /// <summary>
        /// Get all active categories - الحصول على جميع الفئات النشطة
        /// </summary>
        /// <param name="companyId">Company ID</param>
        /// <returns>List of active categories</returns>
        [HttpGet("active")]
        public IActionResult GetActiveCategories(long? companyId = null)
        {
            var response = _assetCategoryService.GetActiveCategories(companyId);
            return SUCCESS(response);
        }
    }
}

