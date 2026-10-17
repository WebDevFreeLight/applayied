using Microsoft.AspNetCore.Mvc;
using System.Web;

namespace ZR.Admin.WebApi.Controllers
{
    /// <summary>
    /// WeChat Official Account
    /// </summary>
    [Route("[controller]/[action]")]
    [AllowAnonymous]
    public class WxOpenController : BaseController
    {
        /// <summary>
        /// Get signature
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        [Log(Title = "Get WeChat Signature")]
        [HttpGet]
        public IActionResult GetSignature(string url = "")
        {
            url = HttpUtility.UrlDecode(url);
            var appId = AppSettings.App(new string[] { "WxOpen", "AppID" });
            var noncestr = Guid.NewGuid().ToString().Replace("-", "");
            var timestamp = DateTimeHelper.GetUnixTimeSeconds(DateTime.Now);
            var ticketResult = WxHelper.GetTicket();
            if (appId.IsEmpty()) return ToResponse(ResultCode.CUSTOM_ERROR, "appId not configured");
            if (ticketResult?.errcode != 0)
            {
                return ToResponse(ResultCode.CUSTOM_ERROR, "Failed to get configuration");
            }

            var signature = WxHelper.GetSignature(ticketResult.ticket, timestamp.ToString(), noncestr, url);

            return SUCCESS(new { appId, signature, noncestr, timestamp, url });
        }

    }
}

