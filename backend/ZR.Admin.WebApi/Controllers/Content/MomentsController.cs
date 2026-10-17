using Microsoft.AspNetCore.Mvc;
using ZR.Model.Content;
using ZR.Model.Content.Dto;
using ZR.Model.Content.Enums;
using ZR.Service.Content.IService;

namespace ZR.Admin.WebApi.Controllers
{
    [Route("moment")]
    [ApiExplorerSettings(GroupName = "article")]
    public class MomentsController : BaseController
    {
        /// <summary>
        /// Moments Interface
        /// </summary>
        private readonly IArticleService _ArticleService;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ArticleService"></param>
        public MomentsController(
            IArticleService ArticleService)
        {
            _ArticleService = ArticleService;
        }

        /// <summary>
        /// Query My (Backend)
        /// </summary>
        /// <returns></returns>
        [HttpGet("mylist")]
        public IActionResult QueryMyList([FromQuery] ArticleQueryDto parm)
        {
            parm.UserId = HttpContext.GetUId();
            parm.ArticleType = (int)ArticleTypeEnum.Monent;
            var response = _ArticleService.GetMyList(parm);

            return SUCCESS(response);
        }

        /// <summary>
        /// Query Moments List
        /// </summary>
        /// <returns></returns>
        [HttpGet("momentList")]
        [AllowAnonymous]
        public IActionResult QueryMonentList([FromQuery] ArticleQueryDto parm)
        {
            parm.UserId = HttpContext.GetUId();
            parm.ArticleType = (int)ArticleTypeEnum.Monent;
            parm.QueryMyJoin = parm.TabId == 100;//Query followed circle moments

            return SUCCESS(_ArticleService.GetMonentList(parm));
        }

        /// <summary>
        /// Moments Publish
        /// </summary>
        /// <returns></returns>
        [HttpPost("publishMoment")]
        [ActionPermissionFilter(Permission = "common")]
        public IActionResult PublishMoment([FromBody] ArticleDto parm)
        {
            if (parm == null) { return ToResponse(ResultCode.PARAM_ERROR); }
            var addModel = parm.Adapt<Article>().ToCreate(context: HttpContext);
            addModel.Tags = parm.TopicName;
            addModel.ArticleType = ArticleTypeEnum.Monent;

            return SUCCESS(_ArticleService.Publish(addModel));
        }

        /// <summary>
        /// Moments Information
        /// </summary>
        /// <returns></returns>
        [HttpGet("getInfo")]
        public IActionResult GetInfo()
        {
            var userId = HttpContext.GetUId();

            var monentNum = _ArticleService.Queryable()
                .Count(f => f.UserId == userId && f.ArticleType == ArticleTypeEnum.Monent);

            return SUCCESS(new { monentNum, commentNum = 0 });
        }
    }
}

