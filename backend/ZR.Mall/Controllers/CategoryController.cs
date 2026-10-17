using Infrastructure.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZR.Common;
using ZR.Mall.Model;
using ZR.Mall.Model.Dto;
using ZR.Mall.Service.IService;

namespace ZR.Mall.Controllers
{
    /// <summary>
    /// Product category Controller
    /// </summary>
    [Route("shopping/category")]
    [ApiExplorerSettings(GroupName = "shopping")]
    public class CategoryController : BaseController
    {
        /// <summary>
        /// Product category interface
        /// </summary>
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        /// <summary>
        /// Query product category list
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "shop:category:list")]
        public IActionResult QueryCategory([FromQuery] ShoppingCategoryQueryDto parm)
        {
            var response = _categoryService.GetList(parm);
            return SUCCESS(response);
        }

        /// <summary>
        /// Query product category list tree
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        [HttpGet("treeList")]
        //[AllowAnonymous]
        public IActionResult QueryTreeCategory([FromQuery] ShoppingCategoryQueryDto parm)
        {
            var response = _categoryService.GetTreeList(parm);
            return SUCCESS(response);
        }

        /// <summary>
        /// Query details
        /// </summary>
        /// <param name="CategoryId"></param>
        /// <returns></returns>
        [HttpGet("{CategoryId}")]
        [AllowAnonymous]
        public IActionResult GetCategory(int CategoryId)
        {
            var response = _categoryService.GetFirst(x => x.CategoryId == CategoryId);

            return SUCCESS(response);
        }

        /// <summary>
        /// Add mall category
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ActionPermissionFilter(Permission = "shop:category:add")]
        [Log(Title = "Mall category", BusinessType = BusinessType.INSERT)]
        public IActionResult AddCategory([FromBody] CategoryDto parm)
        {
            var modal = parm.Adapt<Category>().ToCreate(HttpContext);
            var response = _categoryService.AddCategory(modal);

            return ToResponse(response);
        }

        /// <summary>
        /// Update mall category
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [ActionPermissionFilter(Permission = "shop:category:edit")]
        [Log(Title = "Mall category", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdateShoppingCategory([FromBody] CategoryDto parm)
        {
            var modal = parm.Adapt<Category>().ToUpdate(HttpContext);
            var response = _categoryService.Update(modal);

            return ToResponse(response);
        }

        /// <summary>
        /// Delete mall category
        /// </summary>
        /// <returns></returns>
        [HttpDelete("{ids}")]
        [ActionPermissionFilter(Permission = "shop:category:delete")]
        [Log(Title = "Mall category", BusinessType = BusinessType.DELETE)]
        public IActionResult DeleteCategory(string ids)
        {
            int[] idsArr = Tools.SpitIntArrary(ids);
            if (idsArr.Length <= 0) { return ToResponse(ApiResult.Error($"Delete failed, Id cannot be empty")); }

            var response = _categoryService.Delete(idsArr);

            return ToResponse(response);
        }

        /// <summary>
        /// Export category
        /// </summary>
        /// <returns></returns>
        [Log(Title = "Mall category", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        [HttpGet("export")]
        [ActionPermissionFilter(Permission = "shop:category:export")]
        public IActionResult Export([FromQuery] ShoppingCategoryQueryDto parm)
        {
            parm.PageNum = 1;
            parm.PageSize = 100000;
            var list = _categoryService.ExportList(parm).Result;
            if (list == null || list.Count <= 0)
            {
                return ToResponse(ResultCode.FAIL, "No data to export");
            }
            var result = ExportExcelMini(list, "Product category", "Product category");
            return ExportExcel(result.Item2, result.Item1);
        }

        /// <summary>
        /// Get category
        /// </summary>
        /// <returns></returns>
        [HttpGet("CategoryList")]
        public IActionResult CategoryList()
        {
            var response = _categoryService.GetAll();
            return SUCCESS(response);
        }

        /// <summary>
        /// Save sorting
        /// </summary>
        /// <param name="id"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        [ActionPermissionFilter(Permission = "shop:category:edit")]
        [HttpGet("ChangeSort")]
        [Log(Title = "Save sorting", BusinessType = BusinessType.UPDATE)]
        public IActionResult ChangeSort(int id = 0, int value = 0)
        {
            if (id <= 0) { return ToResponse(ApiResult.Error(101, "Request parameter error")); }
            var response = _categoryService.Update(w => w.CategoryId == id, it => new Category()
            {
                CategoryId = id,
                OrderNum = value
            });
            return ToResponse(response);
        }
    }
}
