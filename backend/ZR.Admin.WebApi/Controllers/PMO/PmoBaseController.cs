using Infrastructure;
using Infrastructure.Controllers;
using ZR.Model.PMO;

namespace ZR.Admin.WebApi.Controllers.PMO
{
    /// <summary>
    /// Base for PMO controllers: exposes the authenticated acting user (roles + id) from the JWT.
    /// </summary>
    public class PmoBaseController : BaseController
    {
        /// <summary>The current user as a PMO actor (never null on authorized routes).</summary>
        protected PmoActor CurrentActor
        {
            get
            {
                var token = JwtUtil.GetLoginUser(HttpContext);
                return token == null ? new PmoActor() : PmoActor.From(token);
            }
        }
    }
}
