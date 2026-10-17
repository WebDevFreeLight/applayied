using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using Infrastructure;
using Infrastructure.Attribute;
using Infrastructure.Enums;
using Infrastructure.Model;
using ZR.Admin.WebApi.Extensions;
using ZR.Admin.WebApi.Filters;
using ZR.Common;
using ZR.Model.Marketing.Dto;
using ZR.Service.Marketing.IMarketingService;

namespace ZR.Admin.WebApi.Controllers.Marketing
{
    /// <summary>
    /// Social Media Management Controller - إدارة التواصل الاجتماعي
    /// Centralized social media management with AI content generation
    /// </summary>
    [Verify]
    [Route("api/marketing/social")]
    [ApiExplorerSettings(GroupName = "marketing")]
    public class SocialMediaController : BaseController
    {
        private readonly ISocialMediaService _socialService;

        public SocialMediaController(ISocialMediaService socialService)
        {
            _socialService = socialService;
        }

        #region Accounts

        [HttpGet("accounts")]
        [ActionPermissionFilter("marketing:social:accounts")]
        public IActionResult GetAccounts([FromQuery] SocialAccountQueryDto dto)
        {
            var result = _socialService.GetAccounts(dto);
            return SUCCESS(result);
        }

        [HttpGet("accounts/{accountId}")]
        [ActionPermissionFilter("marketing:social:accounts")]
        public IActionResult GetAccountDetail(long accountId)
        {
            var result = _socialService.GetAccountDetail(accountId);
            return SUCCESS(result);
        }

        [HttpPost("accounts/connect")]
        [ActionPermissionFilter("marketing:social:accounts:add")]
        [Log(Title = "Social Account Connect", BusinessType = BusinessType.INSERT)]
        public IActionResult ConnectAccount([FromBody] ConnectAccountDto dto)
        {
            var result = _socialService.ConnectAccount(dto);
            return SUCCESS(result);
        }

        [HttpPost("accounts/{accountId}/disconnect")]
        [ActionPermissionFilter("marketing:social:accounts:edit")]
        [Log(Title = "Social Account Disconnect", BusinessType = BusinessType.UPDATE)]
        public IActionResult DisconnectAccount(long accountId)
        {
            var result = _socialService.DisconnectAccount(accountId);
            return ToResponse(result);
        }

        [HttpPost("accounts/{accountId}/sync")]
        [ActionPermissionFilter("marketing:social:accounts")]
        public IActionResult SyncAccount(long accountId)
        {
            var result = _socialService.SyncAccount(accountId);
            return ToResponse(result);
        }

        #endregion

        #region Posts

        [HttpGet("posts")]
        [ActionPermissionFilter("marketing:social:posts")]
        public IActionResult GetPosts([FromQuery] SocialPostQueryDto dto)
        {
            var result = _socialService.GetPosts(dto);
            return SUCCESS(result);
        }

        [HttpGet("posts/{postId}")]
        [ActionPermissionFilter("marketing:social:posts")]
        public IActionResult GetPostDetail(long postId)
        {
            var result = _socialService.GetPostDetail(postId);
            return SUCCESS(result);
        }

        [HttpPost("posts")]
        [ActionPermissionFilter("marketing:social:posts:add")]
        [Log(Title = "Social Post", BusinessType = BusinessType.INSERT)]
        public IActionResult CreatePost([FromBody] SocialPostDto dto)
        {
            var result = _socialService.CreatePost(dto);
            return SUCCESS(result);
        }

        [HttpPut("posts")]
        [ActionPermissionFilter("marketing:social:posts:edit")]
        [Log(Title = "Social Post", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdatePost([FromBody] SocialPostDto dto)
        {
            var result = _socialService.UpdatePost(dto);
            return ToResponse(result);
        }

        [HttpDelete("posts/{ids}")]
        [ActionPermissionFilter("marketing:social:posts:delete")]
        [Log(Title = "Social Post", BusinessType = BusinessType.DELETE)]
        public IActionResult DeletePosts(string ids)
        {
            var idArray = ids.Split(',').Select(long.Parse).ToArray();
            var result = _socialService.DeletePosts(idArray);
            return ToResponse(result);
        }

        [HttpPost("posts/{postId}/publish")]
        [ActionPermissionFilter("marketing:social:posts:publish")]
        [Log(Title = "Social Post Publish", BusinessType = BusinessType.OTHER)]
        public IActionResult PublishPost(long postId)
        {
            var result = _socialService.PublishPost(postId);
            return ToResponse(result);
        }

        [HttpPost("posts/generate-ai")]
        [ActionPermissionFilter("marketing:social:posts:ai")]
        [Log(Title = "AI Social Post", BusinessType = BusinessType.INSERT)]
        public IActionResult GenerateAiPost([FromBody] GenerateAiPostRequest dto)
        {
            var result = _socialService.GenerateAiPost(dto);
            return SUCCESS(result);
        }

        #endregion

        #region Analytics & Dashboard

        [HttpGet("analytics")]
        [ActionPermissionFilter("marketing:social:analytics")]
        public IActionResult GetAnalytics([FromQuery] SocialAnalyticsQueryDto dto)
        {
            var result = _socialService.GetAnalytics(dto);
            return SUCCESS(result);
        }

        [HttpGet("dashboard")]
        [ActionPermissionFilter("marketing:social:dashboard")]
        public IActionResult GetDashboard()
        {
            var result = _socialService.GetDashboard();
            return SUCCESS(result);
        }

        #endregion
    }
}
