using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using ZR.Model.System;
using ZR.Repository;
using ZR.ServiceCore.Model.Dto;

namespace ZR.Admin.WebApi.Controllers.System
{
    /// <summary>
    /// Post Management
    /// </summary>
    [Route("system/post")]
    [ApiExplorerSettings(GroupName = "sys")]
    public class SysPostController : BaseController
    {
        private readonly ISysPostService PostService;
        public SysPostController(ISysPostService postService)
        {
            PostService = postService;
        }

        /// <summary>
        /// Post list query
        /// </summary>
        /// <returns></returns>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "system:post:list")]
        [LookupPermission("system:post:lookup", isPublic: true)]
        public IActionResult List([FromQuery] SysPostQueryDto dto)
        {
            var predicate = Expressionable.Create<SysPost>();
            predicate = predicate.AndIF(dto.Status.IfNotEmpty(), it => it.Status == dto.Status);
            predicate = predicate.AndIF(dto.PostName.IfNotEmpty(), it => it.PostName.Contains(dto.PostName));
            predicate = predicate.AndIF(dto.PostCode.IfNotEmpty(), it => it.PostCode.Contains(dto.PostCode));

            var list = PostService.Queryable()
             .Where(predicate.ToExpression())
                .Select((it) => new SysPostDto
                {
                    UserNum = SqlFunc.Subqueryable<SysUserPost>().Where(f => f.PostId == it.PostId).Sum(f => f.UserId)
                }, true)
                .ToPage(dto);

            return SUCCESS(list);
        }

        /// <summary>
        /// Post query
        /// </summary>
        /// <param name="postId"></param>
        /// <returns></returns>
        [HttpGet("{postId}")]
        [ActionPermissionFilter(Permission = "system:post:query")]
        public IActionResult Query(long postId = 0)
        {
            return SUCCESS(PostService.GetId(postId));
        }

        /// <summary>
        /// Post management
        /// </summary>
        /// <param name="post"></param>
        /// <returns></returns>
        [HttpPost]
        [ActionPermissionFilter(Permission = "system:post:add")]
        [Log(Title = "Post Add", BusinessType = BusinessType.INSERT)]
        public IActionResult Add([FromBody] SysPost post)
        {
            if (UserConstants.NOT_UNIQUE.Equals(PostService.CheckPostNameUnique(post)))
            {
                throw new CustomException($"Add post {post.PostName} failed, post name already exists");
            }
            if (UserConstants.NOT_UNIQUE.Equals(PostService.CheckPostCodeUnique(post)))
            {
                throw new CustomException($"Add post {post.PostName} failed, post code already exists");
            }
            post.ToCreate(HttpContext);

            return ToResponse(PostService.Add(post));
        }

        /// <summary>
        /// Post management
        /// </summary>
        /// <param name="post"></param>
        /// <returns></returns>
        [HttpPut]
        [ActionPermissionFilter(Permission = "system:post:edit")]
        [Log(Title = "Post Edit", BusinessType = BusinessType.UPDATE)]
        public IActionResult Update([FromBody] SysPost post)
        {
            if (UserConstants.NOT_UNIQUE.Equals(PostService.CheckPostNameUnique(post)))
            {
                throw new CustomException($"Modify post {post.PostName} failed, post name already exists");
            }
            if (UserConstants.NOT_UNIQUE.Equals(PostService.CheckPostCodeUnique(post)))
            {
                throw new CustomException($"Modify post {post.PostName} failed, post code already exists");
            }
            post.ToUpdate(HttpContext);
            return ToResponse(PostService.Update(post));
        }

        /// <summary>
        /// Post delete
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [ActionPermissionFilter(Permission = "system:post:remove")]
        [Log(Title = "Post Delete", BusinessType = BusinessType.DELETE)]
        public IActionResult Delete(string id)
        {
            int[] ids = Tools.SpitIntArrary(id);
            return ToResponse(PostService.Delete(ids));
        }

        /// <summary>
        /// Get post selection box list
        /// </summary>
        [HttpGet("optionselect")]
        public IActionResult Optionselect()
        {
            List<SysPost> posts = PostService.GetAll();
            return SUCCESS(posts);
        }

        /// <summary>
        /// Post export
        /// </summary>
        /// <returns></returns>
        [Log(BusinessType = BusinessType.EXPORT, IsSaveResponseData = false, Title = "Post Export")]
        [HttpGet("export")]
        [ActionPermissionFilter(Permission = "system:post:export")]
        public IActionResult Export()
        {
            var list = PostService.GetAll();

            var result = ExportExcelMini(list, "post", "Post List");
            return ExportExcel(result.Item2, result.Item1);
        }
    }
}

