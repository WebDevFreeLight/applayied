using Microsoft.AspNetCore.Mvc;
using ZR.Model;
using ZR.Model.Content;
using ZR.Model.Content.Dto;
using ZR.Service.Content.IService;

namespace ZR.Admin.WebApi.Controllers
{
    /// <summary>
    /// Comment
    /// </summary>
    [Route("front/comment")]
    [ApiExplorerSettings(GroupName = "article")]
    [ApiController]
    public class FrontCommentController : BaseController
    {
        private readonly IArticleCommentService messageService;
        private readonly IArticleService articleService;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="messageService"></param>
        /// <param name="articleService"></param>
        public FrontCommentController(
            IArticleCommentService messageService, IArticleService articleService)
        {
            this.messageService = messageService;
            this.articleService = articleService;
        }

        /// <summary>
        /// Query Comment List
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        [HttpGet("list")]
        [AllowAnonymous]
        public IActionResult QueryList([FromQuery] MessageQueryDto parm)
        {
            parm.PageSize = 10;
            PagedInfo<ArticleCommentDto>? response;
            //Query secondary comments
            if (parm.CommentId > 0)
            {
                response = messageService.GetReplyComments(parm.CommentId, parm);
            }
            else
            {
                response = messageService.GetMessageList(parm);
            }

            return SUCCESS(response);
        }

        /// <summary>
        /// Comment
        /// </summary>
        /// <returns></returns>
        [HttpPost("add")]
        [ActionPermissionFilter(Permission = "common")]
        public IActionResult Create([FromBody] ArticleCommentDto parm)
        {
            var uid = HttpContextExtension.GetUId(HttpContext);
            if (uid <= 0) { return ToResponse(ResultCode.DENY); }

            var addModel = parm.Adapt<ArticleComment>().ToCreate(context: HttpContext);
            addModel.UserIP = HttpContextExtension.GetClientUserIp(HttpContext);
            addModel.UserId = uid;
            return SUCCESS(messageService.AddMessage(addModel).Adapt<ArticleCommentDto>());
        }

        /// <summary>
        /// Comment Like/Praise
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost("praise")]
        [ActionPermissionFilter(Permission = "common")]
        public IActionResult Praise([FromBody] ArticleCommentDto dto)
        {
            if (dto == null || dto.CommentId <= 0) return ToResponse(ResultCode.PARAM_ERROR);
            //var uid = HttpContextExtension.GetUId(HttpContext);

            return SUCCESS(messageService.PraiseMessage(dto.CommentId));
        }

        /// <summary>
        /// Comment Delete
        /// </summary>
        /// <param name="mid"></param>
        /// <returns></returns>
        [HttpDelete("delete/{mid}")]
        [ActionPermissionFilter(Permission = "common")]
        public IActionResult Delete(long mid)
        {
            var uid = HttpContextExtension.GetUId(HttpContext);
            if (uid <= 0) { return ToResponse(ResultCode.DENY); }
            return SUCCESS(messageService.DeleteMessage(mid.ParseToLong(), uid));
        }

        /// <summary>
        /// Query My Comment List
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        [HttpGet("mylist")]
        public IActionResult QueryMyCommentList([FromQuery] MessageQueryDto parm)
        {
            PagedInfo<ArticleCommentDto> response = messageService.GetMyMessageList(parm);

            return SUCCESS(response);
        }

        /// <summary>
        /// Comment Pin to Top
        /// </summary>
        /// <returns></returns>
        [HttpPut("top")]
        [ActionPermissionFilter(Permission = "common")]
        public IActionResult Top([FromBody] ArticleCommentDto parm)
        {
            var uid = HttpContextExtension.GetUId(HttpContext);
            if (uid <= 0) { return ToResponse(ResultCode.DENY); }
            var contentInfo = articleService.GetArticle(parm.TargetId, uid);
            if (contentInfo == null) { return ToResponse(ResultCode.CUSTOM_ERROR, "Operation failed"); }
            return SUCCESS(messageService.TopMessage(parm.CommentId, parm.Top));
        }
    }
}

