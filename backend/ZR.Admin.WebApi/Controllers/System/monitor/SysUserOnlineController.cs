using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using ZR.Model;
using ZR.Model.System.Dto;
using ZR.ServiceCore.Signalr;

namespace ZR.Admin.WebApi.Controllers.monitor
{
    /// <summary>
    /// Online users
    /// </summary>
    [Route("monitor/online")]
    [ApiExplorerSettings(GroupName = "sys")]
    public class SysUserOnlineController : BaseController
    {
        private readonly IHubContext<MessageHub> HubContext;

        public SysUserOnlineController(IHubContext<MessageHub> hubContext)
        {
            HubContext = hubContext;
        }

        /// <summary>
        /// Get online user list
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        [HttpGet("list")]
        public IActionResult Index([FromQuery] PagerInfo parm)
        {
            // Project the live SignalR connections into the shape the admin UI expects. tokenId is the
            // SignalR connection id, which is what Force uses to disconnect a specific session.
            var all = MessageHub.OnlineClients.Values
                .OrderByDescending(f => f.LoginTime)
                .Select(u => new
                {
                    tokenId = u.ConnnectionId,
                    userId = u.Userid,
                    userName = u.Name,
                    ipaddr = u.UserIP,
                    loginLocation = u.Location,
                    browser = u.Browser,
                    os = u.Platform,
                    loginTime = u.LoginTime
                })
                .ToList();

            var result = all.Skip((parm.PageNum - 1) * parm.PageSize).Take(parm.PageSize).ToList();
            return SUCCESS(new { result, totalNum = all.Count });
        }

        /// <summary>
        /// Single force logout
        /// </summary>
        /// <returns></returns>
        [HttpDelete("force")]
        [Log(Title = "Force Logout", BusinessType = BusinessType.FORCE)]
        [ActionPermissionFilter(Permission = "monitor:online:forceLogout")]
        public async Task<IActionResult> Force([FromQuery] string tokenId)
        {
            if (string.IsNullOrEmpty(tokenId)) { return ToResponse(ResultCode.PARAM_ERROR); }

            // Tell that specific connection to log out, then drop it from the live registry so the
            // Sessions list reflects the change immediately (the client also disconnects itself).
            await HubContext.Clients.Client(tokenId)
                .SendAsync(HubsConstant.ForceUser, new { Reason = "Your session was ended by an administrator." });
            MessageHub.OnlineClients.TryRemove(tokenId, out _);

            return SUCCESS(1);
        }

        /// <summary>
        /// Batch force logout
        /// </summary>
        /// <returns></returns>
        [HttpDelete("batchForce")]
        [Log(Title = "Force Logout", BusinessType = BusinessType.FORCE)]
        [ActionPermissionFilter(Permission = "monitor:online:batchLogout")]
        public async Task<IActionResult> BatchforceLogout([FromBody] LockUserDto dto)
        {
            if (dto == null) { return ToResponse(ResultCode.PARAM_ERROR); }

            await HubContext.Clients.All.SendAsync(HubsConstant.ForceUser, new { dto.Reason });

            return SUCCESS(1);
        }
    }
}

