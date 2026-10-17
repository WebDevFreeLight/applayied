using Microsoft.AspNetCore.Mvc;
using ZR.Model.Content;
using ZR.Model.Content.Dto;
using ZR.Service.Content.IService;

namespace ZR.Admin.WebApi.Controllers
{
    /// <summary>
    /// Article Category Controller
    /// </summary>
    [Route("article/ArticleCategory")]
    [ApiExplorerSettings(GroupName = "article")]
    public class ArticleCategoryController : BaseController
    {
        /// <summary>
        /// Article Category Service Interface
        /// </summary>
        private readonly IArticleCategoryService _ArticleCategoryService;

        public ArticleCategoryController(IArticleCategoryService ArticleCategoryService)
        {
            _ArticleCategoryService = ArticleCategoryService;
        }

        /// <summary>
        /// Query article category list
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        [HttpGet("list")]
        [AllowAnonymous]
        public IActionResult QueryArticleCategory([FromQuery] ArticleCategoryQueryDto parm)
        {
            var response = _ArticleCategoryService.GetList(parm);
            return SUCCESS(response);
        }

        /// <summary>
        /// Query article category tree list
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        [HttpGet("treeList")]
        [AllowAnonymous]
        public IActionResult QueryTreeArticleCategory([FromQuery] ArticleCategoryQueryDto parm)
        {
            var response = _ArticleCategoryService.GetTreeList(parm);
            return SUCCESS(response);
        }

        /// <summary>
        /// Query article category details
        /// </summary>
        /// <param name="CategoryId"></param>
        /// <returns></returns>
        [HttpGet("{CategoryId}")]
        [AllowAnonymous]
        //[ActionPermissionFilter(Permission = "articlecategory:query")]
        public IActionResult GetArticleCategory(int CategoryId)
        {
            var response = _ArticleCategoryService.GetFirst(x => x.CategoryId == CategoryId);

            return SUCCESS(response);
        }

        /// <summary>
        /// Query category by type
        /// </summary>
        /// <param name="categoryType"></param>
        /// <returns></returns>
        [HttpGet("type{categoryType}")]
        //[ActionPermissionFilter(Permission = "articlecategory:query")]
        public IActionResult GetArticleCategoryByType(int categoryType)
        {
            var response = _ArticleCategoryService.GetFirst(x => x.CategoryType == categoryType);

            return SUCCESS(response);
        }

        /// <summary>
        /// Add article category
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ActionPermissionFilter(Permission = "articlecategory:add")]
        [Log(Title = "Article Category", BusinessType = BusinessType.INSERT)]
        public IActionResult AddArticleCategory([FromBody] ArticleCategoryDto parm)
        {
            var modal = parm.Adapt<ArticleCategory>().ToCreate(HttpContext);
            var response = _ArticleCategoryService.AddArticleCategory(modal);

            return ToResponse(response);
        }

        /// <summary>
        /// Update article category
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [ActionPermissionFilter(Permission = "articlecategory:edit")]
        [Log(Title = "Article Category", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdateArticleCategory([FromBody] ArticleCategoryDto parm)
        {
            var modal = parm.Adapt<ArticleCategory>().ToUpdate(HttpContext);
            var response = _ArticleCategoryService.Update(modal);

            return ToResponse(response);
        }

        /// <summary>
        /// Delete article category
        /// </summary>
        /// <returns></returns>
        [HttpDelete("{ids}")]
        [ActionPermissionFilter(Permission = "articlecategory:delete")]
        [Log(Title = "Article Category", BusinessType = BusinessType.DELETE)]
        public IActionResult DeleteArticleCategory(string ids)
        {
            int[] idsArr = Tools.SpitIntArrary(ids);
            if (idsArr.Length <= 0) { return ToResponse(ApiResult.Error($"Delete failed, ID cannot be empty")); }

            var response = _ArticleCategoryService.Delete(idsArr);

            return ToResponse(response);
        }

        /// <summary>
        /// Export article category
        /// </summary>
        /// <returns></returns>
        [Log(Title = "Article Category", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        [HttpGet("export")]
        [ActionPermissionFilter(Permission = "articlecategory:export")]
        public IActionResult Export([FromQuery] ArticleCategoryQueryDto parm)
        {
            parm.PageSize = 10000;
            var list = _ArticleCategoryService.GetList(parm).Result;

            string sFileName = ExportExcel(list, "ArticleCategory", "Article Category");
            return SUCCESS(new { path = "/export/" + sFileName, fileName = sFileName });
        }

        /// <summary>
        /// Get article categories
        /// </summary>
        /// <returns></returns>
        [HttpGet("CategoryList")]
        public IActionResult CategoryList()
        {
            var response = _ArticleCategoryService.GetAll();
            return SUCCESS(response);
        }

        /// <summary>
        /// Save sort order
        /// </summary>
        /// <param name="id"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        [ActionPermissionFilter(Permission = "articlecategory:edit")]
        [HttpGet("ChangeSort")]
        [Log(Title = "Save Sort Order", BusinessType = BusinessType.UPDATE)]
        public IActionResult ChangeSort(int id = 0, int value = 0)
        {
            if (id <= 0) { return ToResponse(ApiResult.Error(101, "Request parameters are invalid")); }
            var response =  _ArticleCategoryService.Update(w => w.CategoryId == id, it => new ArticleCategory ()
            {
                CategoryId = id,
                OrderNum = value
            });
            return ToResponse(response);
        }
    }
}
