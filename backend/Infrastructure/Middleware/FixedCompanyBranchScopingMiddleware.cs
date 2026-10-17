using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Middleware
{
    /// <summary>
    /// FixedCompanyBranchScopingMiddleware
    /// For development/testing: forcibly injects fixed CompanyId/BranchId into HttpContext.Items for every request.
    ///
    /// Requirements:
    /// - Does NOT read from headers/claims/query/body/etc.
    /// - Uses hardcoded constants (easy to change).
    /// - Works for any code that reads HttpContext.Items["CompanyId"] / ["BranchId"].
    /// </summary>
    public sealed class FixedCompanyBranchScopingMiddleware
    {
        // Easy to change later
        private const long FixedCompanyId = 0L;
        private const long FixedBranchId = 0L;

        // Keys used by controllers/services/repositories
        public const string CompanyIdItemKey = "CompanyId";
        public const string BranchIdItemKey = "BranchId";

        private readonly RequestDelegate _next;
        private readonly ILogger<FixedCompanyBranchScopingMiddleware> _logger;

        public FixedCompanyBranchScopingMiddleware(
            RequestDelegate next,
            ILogger<FixedCompanyBranchScopingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Forcibly overwrite on every request (by design)
            context.Items["CompanyId"] = FixedCompanyId;
            context.Items["BranchId"] = FixedBranchId;

            // Optional logging (only emits if Debug enabled)
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug(
                    "Injected fixed scoping into HttpContext.Items: {CompanyKey}={CompanyId}, {BranchKey}={BranchId} (Path: {Path})",
                    CompanyIdItemKey,
                    FixedCompanyId,
                    BranchIdItemKey,
                    FixedBranchId,
                    context.Request.Path.Value);
            }

            await _next(context);
        }
    }

    public static class FixedCompanyBranchScopingMiddlewareExtensions
    {
        /// <summary>
        /// Register FixedCompanyBranchScopingMiddleware early in the pipeline (before auth).
        /// </summary>
        public static IApplicationBuilder UseFixedCompanyBranchScoping(this IApplicationBuilder app)
        {
            return app.UseMiddleware<FixedCompanyBranchScopingMiddleware>();
        }
    }
}


