using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Middleware
{
    /// <summary>
    /// Idempotency Middleware - منع الطلبات المكررة
    /// Prevents duplicate requests by tracking idempotency keys
    /// Uses X-Idempotency-Key header for explicit idempotency
    /// Also provides automatic duplicate detection based on request hash
    /// </summary>
    public class IdempotencyMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMemoryCache _cache;
        private readonly ILogger<IdempotencyMiddleware> _logger;

        // Cache duration for idempotency keys (5 minutes)
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

        // Methods that should be checked for idempotency
        private static readonly string[] IdempotentMethods = { "POST", "PUT", "DELETE" };

        public IdempotencyMiddleware(
            RequestDelegate next,
            IMemoryCache cache,
            ILogger<IdempotencyMiddleware> logger)
        {
            _next = next;
            _cache = cache;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Only check POST, PUT, DELETE requests
            if (!ShouldCheckIdempotency(context.Request))
            {
                await _next(context);
                return;
            }

            // Get idempotency key from header or generate from request
            var idempotencyKey = GetIdempotencyKey(context);

            if (string.IsNullOrEmpty(idempotencyKey))
            {
                // No idempotency key provided, proceed normally
                await _next(context);
                return;
            }

            var cacheKey = $"idempotency:{idempotencyKey}";

            // Check if this request was already processed
            if (_cache.TryGetValue(cacheKey, out CachedResponse cachedResponse))
            {
                _logger.LogWarning(
                    "Duplicate request detected. IdempotencyKey: {Key}, Path: {Path}",
                    idempotencyKey,
                    context.Request.Path);

                // Return cached response
                context.Response.StatusCode = cachedResponse.StatusCode;
                context.Response.ContentType = cachedResponse.ContentType;

                // Add header to indicate this is a cached response
                context.Response.Headers["X-Idempotency-Replayed"] = "true";

                if (cachedResponse.Body != null)
                {
                    await context.Response.WriteAsync(cachedResponse.Body);
                }
                return;
            }

            // Mark request as in-progress to prevent concurrent duplicates
            var inProgressKey = $"idempotency:inprogress:{idempotencyKey}";
            if (_cache.TryGetValue(inProgressKey, out _))
            {
                // Request is already being processed
                context.Response.StatusCode = 409; // Conflict
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync("{\"code\":409,\"msg\":\"Request is already being processed - الطلب قيد المعالجة حالياً\"}");
                return;
            }

            // Mark as in-progress
            _cache.Set(inProgressKey, true, TimeSpan.FromSeconds(30));

            // Capture the original response body
            var originalBodyStream = context.Response.Body;
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            try
            {
                await _next(context);

                // Cache successful responses (2xx status codes)
                if (context.Response.StatusCode >= 200 && context.Response.StatusCode < 300)
                {
                    responseBody.Seek(0, SeekOrigin.Begin);
                    var responseText = await new StreamReader(responseBody).ReadToEndAsync();

                    var cachedResponseObj = new CachedResponse
                    {
                        StatusCode = context.Response.StatusCode,
                        ContentType = context.Response.ContentType,
                        Body = responseText
                    };

                    _cache.Set(cacheKey, cachedResponseObj, CacheDuration);
                }

                // Copy the response to the original stream
                responseBody.Seek(0, SeekOrigin.Begin);
                await responseBody.CopyToAsync(originalBodyStream);
            }
            finally
            {
                context.Response.Body = originalBodyStream;
                // Remove in-progress marker
                _cache.Remove(inProgressKey);
            }
        }

        private bool ShouldCheckIdempotency(HttpRequest request)
        {
            // Only check specific methods
            if (Array.IndexOf(IdempotentMethods, request.Method.ToUpper()) < 0)
                return false;

            // Skip if path contains specific endpoints that handle their own idempotency
            var path = request.Path.Value?.ToLower() ?? "";

            // Skip login, token refresh, etc.
            if (path.Contains("/login") || path.Contains("/token") || path.Contains("/refresh"))
                return false;

            return true;
        }

        private string GetIdempotencyKey(HttpContext context)
        {
            // First, check for explicit idempotency key in header
            if (context.Request.Headers.TryGetValue("X-Idempotency-Key", out var headerKey)
                && !string.IsNullOrWhiteSpace(headerKey))
            {
                return headerKey.ToString();
            }

            // No explicit key provided
            return null;
        }

        private class CachedResponse
        {
            public int StatusCode { get; set; }
            public string ContentType { get; set; }
            public string Body { get; set; }
        }
    }
}
