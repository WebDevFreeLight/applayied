using Infrastructure;
using Infrastructure.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using ZR.Common;

namespace ZR.ServiceCore.Middleware
{
    /// <summary>
    /// JWT authentication middleware
    /// </summary>
    public class JwtAuthMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<JwtAuthMiddleware> _logger;
        private static readonly string[] _whitelistPaths = new[]
        {
            ".png",
            "/msgHub"
        };

        public JwtAuthMiddleware(RequestDelegate next, ILogger<JwtAuthMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value;
            
            // If the request has an extension (i.e., contains .)
            if (path.Contains('.'))
            {
                await _next(context);
                return;
            }
            if (_whitelistPaths.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
            {
                await _next(context);
                return;
            }
            var endpoint = context.GetEndpoint();
            var allowAnonymous = endpoint?.Metadata?.GetMetadata<AllowAnonymousAttribute>() != null;

            if (allowAnonymous)
            {
                await _next(context);
                return;
            }

            string ip = HttpContextExtension.GetClientUserIp(context);
            string url = context.Request.Path;
            string osType = context.Request.Headers["os"];

            TokenModel loginUser = JwtUtil.GetLoginUser(context);

            if (loginUser != null)
            {
                var now = DateTime.UtcNow;
                var ts = loginUser.ExpireTime - now;
                var cacheKey = $"token_{loginUser.UserId}";

                if (!CacheHelper.Exists(cacheKey) && ts.TotalMinutes < 5 && ts.TotalMinutes > 0)
                {
                    var newToken = JwtUtil.GenerateJwtToken(JwtUtil.AddClaims(loginUser));
                    CacheHelper.SetCache(cacheKey, cacheKey, 1);

                    if (!string.IsNullOrEmpty(osType))
                    {
                        context.Response.Headers.Append("Access-Control-Expose-Headers", "X-Refresh-Token");
                    }

                    context.Response.Headers.Append("X-Refresh-Token", newToken);
                    _logger.LogInformation($"Refresh Token: {loginUser.UserName}");
                }

                // Can also be mounted to context.User
                var identity = new ClaimsIdentity(JwtUtil.AddClaims(loginUser), "jwt");
                context.User = new ClaimsPrincipal(identity);

                await _next(context);
            }
            else
            {
                string msg = $"Request access [{url}] failed, Token is invalid or not logged in";
                _logger.LogWarning(msg);
                //context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(ApiResult.Error(ResultCode.DENY, msg));
            }
        }
    }
}

