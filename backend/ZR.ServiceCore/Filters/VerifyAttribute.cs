using Microsoft.AspNetCore.Mvc.Filters;

//This namespace is temporarily not changed, as the change is relatively large - September 2, 2023
namespace ZR.Admin.WebApi.Filters
{
    /// <summary>
    /// </summary>
    [AttributeUsage(AttributeTargets.All)]
    public class VerifyAttribute : Attribute, IAuthorizationFilter
    {
        private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        /// <summary>
        /// Only checks if token is correct, does not check permissions
        /// If permission checking is needed, add ApiActionPermission attribute on Action to identify permission category, ActionPermissionFilter handles permissions
        /// </summary>
        /// <param name="context"></param>
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            return;
        }
    }
}

