using Microsoft.AspNetCore.Mvc;
using ZR.Service.Content.IService;

//Created: 2025-04-07
namespace ZR.Admin.WebApi.Controllers
{
    /// <summary>
    /// User Join Circle
    /// </summary>
    [Route("front/circles")]
    [ApiExplorerSettings(GroupName = "userCircle")]
    [ApiController]
    public class ArticleUserCirclesController : BaseController
    {
        /// <summary>
        /// User Join Circle Interface
        /// </summary>
        private readonly IArticleUserCirclesService _ArticleUserCirclesService;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ArticleUserCirclesService"></param>
        public ArticleUserCirclesController(
            IArticleUserCirclesService ArticleUserCirclesService)
        {
            _ArticleUserCirclesService = ArticleUserCirclesService;
        }

        /// <summary>
        /// Query User Joined Circle List
        /// </summary>
        /// <returns></returns>
        [HttpGet("myCircleList")]
        public IActionResult QueryArticleUserCircles()
        {
            var userId = HttpContext.GetUId();
            var response = _ArticleUserCirclesService.GetMyJoinCircles((int)userId);
            return SUCCESS(response);
        }

        /// <summary>
        /// User Join Circle
        /// </summary>
        /// <returns></returns>
        [HttpPost("join/{id}")]
        public IActionResult JoinCircles([FromRoute] int id)
        {
            var userId = HttpContext.GetUId();
            return ToResponse(_ArticleUserCirclesService.JoinCircle((int)userId, id));
        }

        /// <summary>
        /// Delete User Joined Circle
        /// </summary>
        /// <returns></returns>
        [HttpPost("delete/{id}")]
        public IActionResult DeleteCircles([FromRoute] int id)
        {
            var userId = HttpContext.GetUId();
            return ToResponse(_ArticleUserCirclesService.RemoveCircle((int)userId, id));
        }
    }
}
