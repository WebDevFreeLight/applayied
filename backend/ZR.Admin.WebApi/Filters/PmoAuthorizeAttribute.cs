using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ZR.Model.PMO;

namespace ZR.Admin.WebApi.Filters
{
    /// <summary>
    /// Coarse, bypass-proof role floor for PMO routes. Reads the JWT server-side, so a Contributor
    /// calling a PM/Admin endpoint directly (e.g. via curl) is refused with 403 regardless of the UI.
    /// Fine-grained rules (own task only, no self-approval, project scope) live in the services.
    /// </summary>
    public class PmoAuthorizeAttribute : ActionFilterAttribute
    {
        private readonly string _minRole;

        /// <param name="minRole">Minimum PMO role key, e.g. PmoRoleKeys.ProjectManager.</param>
        public PmoAuthorizeAttribute(string minRole)
        {
            _minRole = minRole;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // Resolve the request language (ar/en) so denial responses are localized too.
            var lang = ZR.ServiceCore.Localization.PmoMessageLocalizer.LanguageFromHeader(
                context.HttpContext.Request.Headers["Accept-Language"].ToString());

            var token = JwtUtil.GetLoginUser(context.HttpContext);
            if (token == null || token.UserId <= 0)
            {
                var msg = ZR.ServiceCore.Localization.PmoMessageLocalizer.Translate(
                    "Unauthorized - please sign in", lang);
                context.Result = new JsonResult(new { code = 401, msg }) { StatusCode = 401 };
                return;
            }

            var actor = PmoActor.From(token);
            if (actor.Level < PmoActor.RankOf(_minRole))
            {
                var msg = ZR.ServiceCore.Localization.PmoMessageLocalizer.Translate(
                    $"Forbidden - this action requires the '{_minRole}' role or higher", lang);
                context.Result = new JsonResult(new { code = 403, msg }) { StatusCode = 403 };
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}
