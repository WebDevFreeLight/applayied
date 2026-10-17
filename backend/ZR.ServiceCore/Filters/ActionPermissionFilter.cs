using Infrastructure;
using Infrastructure.Attribute;
using Infrastructure.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;
using ZR.Model.System.Dto;
using ZR.ServiceCore.Services;

namespace ZR.ServiceCore.Middleware
{
    /// <summary>
    /// API authorization check
    /// </summary>
    public class ActionPermissionFilter : ActionFilterAttribute//, IAsyncActionFilter
    {
        private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Permission string, multiple permissions separated by commas, e.g. system:user:view,system:user:add
        /// </summary>
        public string Permission { get; set; } = string.Empty;
        /// <summary>
        /// Role string, e.g. common,admin
        /// </summary>
        public string RolePermi { get; set; } = string.Empty;

        /// <summary>Human-readable name for this permission (used when auto-registering the route).</summary>
        public string DisplayName { get; set; } = string.Empty;
        /// <summary>Longer description of what the permission allows (stored on the generated menu row).</summary>
        public string Description { get; set; } = string.Empty;
        /// <summary>Intended access tier, e.g. "ALL" | "PM" | "ADMIN" or a role key — advisory metadata for setup.</summary>
        public string Tier { get; set; } = string.Empty;

        private bool HasPermi { get; set; }
        public ActionPermissionFilter() { }
        public ActionPermissionFilter(string permission)
        {
            Permission = permission;
            HasPermi = !string.IsNullOrEmpty(Permission);
        }

        /// <summary>
        /// Validate permissions before executing Action
        /// </summary>
        /// <param name="context"></param>
        /// <param name="next"></param>
        /// <returns></returns>
        public override Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Get login user once (used by both lookup and regular permission checks)
            TokenModel info = JwtUtil.GetLoginUser(context.HttpContext);

            // CHECK 1: Lookup permission (new - takes priority)
            var lookupAttr = context.ActionDescriptor.EndpointMetadata
                .OfType<LookupPermissionAttribute>()
                .FirstOrDefault();

            if (lookupAttr != null)
            {
                // User must be authenticated
                if (info == null || info.UserId <= 0)
                {
                    logger.Info($"Lookup endpoint accessed without authentication");
                    context.Result = new JsonResult(new { code = 401, msg = "Unauthorized - Please login" });
                    return base.OnActionExecutionAsync(context, next);
                }

                // If public lookup, any authenticated user can access
                if (lookupAttr.IsPublicLookup)
                {
                    logger.Info($"User {info.UserName} accessing public lookup endpoint");
                    return base.OnActionExecutionAsync(context, next);
                }

                // If not public, check if user has the lookup permission
                List<string> perms = CacheService.GetUserPerms(GlobalConstant.UserPermKEY + info.UserId);
                if (perms == null)
                {
                    var sysPermissionService = App.GetService<ISysPermissionService>();
                    perms = sysPermissionService.GetMenuPermission(new SysUserDto() { UserId = info.UserId });
                    CacheService.SetUserPerms(GlobalConstant.UserPermKEY + info.UserId, perms);
                }

                // Check if user has admin permission or the specific lookup permission
                bool hasLookupPermission = perms.Exists(f => f.Equals(GlobalConstant.AdminPerm)) ||
                                           perms.Exists(f => f.Equals(lookupAttr.LookupPermission, StringComparison.OrdinalIgnoreCase));

                if (!hasLookupPermission)
                {
                    logger.Info($"User {info.UserName} does not have lookup permission [{lookupAttr.LookupPermission}]");
                    context.Result = new JsonResult(new { code = 403, msg = $"No lookup permission: {lookupAttr.LookupPermission}" });
                    return base.OnActionExecutionAsync(context, next);
                }

                logger.Info($"User {info.UserName} accessing lookup endpoint with permission [{lookupAttr.LookupPermission}]");
                return base.OnActionExecutionAsync(context, next);
            }

            // CHECK 2: Regular permission (existing logic)

            if (info != null && info?.UserId > 0)
            {
                long userId = info.UserId;
                List<string> perms = CacheService.GetUserPerms(GlobalConstant.UserPermKEY + userId);
                List<string> rolePerms = info.RoleKeys;

                if (perms == null)
                {
                    var sysPermissionService = App.GetService<ISysPermissionService>();
                    perms = sysPermissionService.GetMenuPermission(new SysUserDto() { UserId = userId });
                    logger.Info("Reading permissions from database");
                    CacheService.SetUserPerms(GlobalConstant.UserPermKEY + userId, perms);
                }
                info.Permissions = perms;
                if (perms.Exists(f => f.Equals(GlobalConstant.AdminPerm)))
                {
                    HasPermi = true;
                }
                else if (rolePerms.Exists(f => f.Equals(GlobalConstant.AdminRole)))
                {
                    HasPermi = true;
                }
                else if (!string.IsNullOrEmpty(Permission))
                {
                    //HasPermi = perms.Exists(f => f.ToLower() == Permission.ToLower());
                    var requiredPerms = Permission.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                                  .Select(p => p.ToLower()).ToList();
                    HasPermi = perms.Select(p => p.ToLower()).Intersect(requiredPerms).Any();
                }
                if (!HasPermi && !string.IsNullOrEmpty(RolePermi))
                {
                    HasPermi = info.RoleKeys.Contains(RolePermi);
                }
                bool isDemoMode = AppSettings.GetAppConfig("DemoMode", false);
                var url = context.HttpContext.Request.Path;
                // Demo public environment permission masking
                string[] denyPerms = ["update", "add", "remove", "add", "edit", "delete", "import", "run", "start", "stop", "clear", "send", "export", "upload", "common", "gencode", "reset", "forceLogout", "batchLogout"];
                if (isDemoMode)
                {
                    var requiredPerms = Permission.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    if (requiredPerms.Any(p => denyPerms.Any(d => p.Contains(d, StringComparison.OrdinalIgnoreCase))))
                    {
                        context.Result = new JsonResult(new { code = (int)ResultCode.FORBIDDEN, msg = "Demo mode, operation not allowed" });
                    }
                }
                if (!HasPermi && !Permission.Equals("common"))
                {
                    logger.Info($"User {info.UserName} does not have permission to access {url}, current permission [{Permission}]");
                    var apiResult = new ApiResult((int)ResultCode.FORBIDDEN, $"You do not have permission to access, please contact administrator", url);
                    apiResult.Put("permi", Permission);
                    JsonResult result = new(apiResult)
                    {
                        StatusCode = 403,
                        ContentType = "application/json",
                        Value = JsonConvert.SerializeObject(apiResult)
                    };
                    context.HttpContext.Response.StatusCode = 403;
                    context.Result = result;
                }
            }

            return base.OnActionExecutionAsync(context, next);
        }
    }
}

