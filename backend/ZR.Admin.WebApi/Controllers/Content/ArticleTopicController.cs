using Microsoft.AspNetCore.Mvc;
using ZR.Model.Content;
using ZR.Model.Content.Dto;
using ZR.Service.Content.IService;

//Created: 2024-04-29
namespace ZR.Admin.WebApi.Controllers
{
    /// <summary>
    /// Article Topic
    /// </summary>
    [ApiExplorerSettings(GroupName = "article")]
    [Route("article/ArticleTopic")]
    public class ArticleTopicController : BaseController
    {
        /// <summary>
        /// Article Topic Interface
        /// </summary>
        private readonly IArticleTopicService _ArticleTopicService;

        public ArticleTopicController(IArticleTopicService ArticleTopicService)
        {
            _ArticleTopicService = ArticleTopicService;
        }

        /// <summary>
        /// Query Article Topic List
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "articletopic:list")]
        public IActionResult QueryArticleTopic([FromQuery] ArticleTopicQueryDto parm)
        {
            var response = _ArticleTopicService.GetList(parm);
            return SUCCESS(response);
        }

        /// <summary>
        /// Query Article Topic Details
        /// </summary>
        /// <param name="TopicId"></param>
        /// <returns></returns>
        [HttpGet("{TopicId}")]
        [ActionPermissionFilter(Permission = "articletopic:query")]
        public IActionResult GetArticleTopic(long TopicId)
        {
            var response = _ArticleTopicService.GetInfo(TopicId);
            
            var info = response.Adapt<ArticleTopicDto>();
            return SUCCESS(info);
        }

        /// <summary>
        /// Add Article Topic
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ActionPermissionFilter(Permission = "articletopic:add")]
        [Log(Title = "Article Topic", BusinessType = BusinessType.INSERT)]
        public IActionResult AddArticleTopic([FromBody] ArticleTopicDto parm)
        {
            var modal = parm.Adapt<ArticleTopic>().ToCreate(HttpContext);

            var response = _ArticleTopicService.AddArticleTopic(modal);

            return SUCCESS(response);
        }

        /// <summary>
        /// Update Article Topic
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [ActionPermissionFilter(Permission = "articletopic:edit")]
        [Log(Title = "Article Topic", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdateArticleTopic([FromBody] ArticleTopicDto parm)
        {
            var modal = parm.Adapt<ArticleTopic>().ToUpdate(HttpContext);
            var response = _ArticleTopicService.UpdateArticleTopic(modal);

            return ToResponse(response);
        }

        /// <summary>
        /// Delete Article Topic
        /// </summary>
        /// <returns></returns>
        [HttpDelete("delete/{ids}")]
        [ActionPermissionFilter(Permission = "articletopic:delete")]
        [Log(Title = "Article Topic", BusinessType = BusinessType.DELETE)]
        public IActionResult DeleteArticleTopic([FromRoute] string ids)
        {
            var idArr = Tools.SplitAndConvert<long>(ids);

            return ToResponse(_ArticleTopicService.Delete(idArr));
        }

        /// <summary>
        /// Export Article Topic
        /// </summary>
        /// <returns></returns>
        [Log(Title = "Article Topic", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        [HttpGet("export")]
        [ActionPermissionFilter(Permission = "articletopic:export")]
        public IActionResult Export([FromQuery] ArticleTopicQueryDto parm)
        {
            parm.PageNum = 1;
            parm.PageSize = 100000;
            var list = _ArticleTopicService.ExportList(parm).Result;
            if (list == null || list.Count <= 0)
            {
                return ToResponse(ResultCode.FAIL, "No data to export");
            }
            var result = ExportExcelMini(list, "Article Topic", "Article Topic");
            return ExportExcel(result.Item2, result.Item1);
        }

    }
}
