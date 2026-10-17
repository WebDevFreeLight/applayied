using Microsoft.AspNetCore.Mvc;
using ZR.Admin.WebApi.Filters;
using ZR.Service.IService.Customers.DataMine;
using Infrastructure;

namespace ZR.Admin.WebApi.Controllers.Customers.DataMine
{
    [Verify]
    [Route("api/datamine/categories")]
    public class DmCategoryController : BaseController
    {
        private readonly IDmCategoryService _categoryService;

        public DmCategoryController(IDmCategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        /// <summary>
        /// Get all active categories
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var categories = await _categoryService.GetAllAsync();
            return SUCCESS(categories);
        }

        /// <summary>
        /// Add a new category
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] AddCategoryRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Name))
                return ToResponse(ResultCode.FAIL, "Category name is required");

            var category = await _categoryService.AddAsync(request.Name.Trim(), request.NameAr?.Trim());
            return SUCCESS(category);
        }
    }

    public class AddCategoryRequest
    {
        public string Name { get; set; }
        public string NameAr { get; set; }
    }
}
