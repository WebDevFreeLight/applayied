using System.Net;
using System.Text.Json;
using NLog;

namespace ZR.Admin.WebApi.Middleware
{
    /// <summary>
    /// Global error handling middleware
    /// </summary>
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;
        private readonly IWebHostEnvironment _env;
        private static readonly Logger nlog = LogManager.GetCurrentClassLogger();

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger, IWebHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var requestId = context.TraceIdentifier;
            var path = context.Request.Path;
            var method = context.Request.Method;
            
            // Log the exception
            _logger.LogError(exception, $"Unhandled exception - RequestId: {requestId}, Method: {method}, Path: {path}");
            nlog.Error(exception, $"Unhandled exception - RequestId: {requestId}");

            // Determine status code and message
            var (statusCode, message) = GetResponseDetails(exception);
            
            // Create error response
            var errorResponse = new
            {
                code = statusCode,
                msg = message,
                requestId,
                timestamp = DateTime.UtcNow,
                path = path.Value,
                method,
                details = _env.IsDevelopment() ? exception.ToString() : null
            };

            // Write response
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";
            
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
            
            var jsonResponse = JsonSerializer.Serialize(errorResponse, jsonOptions);
            await context.Response.WriteAsync(jsonResponse);
        }

        private (int statusCode, string message) GetResponseDetails(Exception exception)
        {
            return exception switch
            {
                UnauthorizedAccessException => ((int)HttpStatusCode.Unauthorized, "Unauthorized access"),
                ArgumentNullException argNull => ((int)HttpStatusCode.BadRequest, $"Missing required parameter: {argNull.ParamName}"),
                ArgumentException => ((int)HttpStatusCode.BadRequest, "Invalid argument"),
                KeyNotFoundException => ((int)HttpStatusCode.NotFound, "Resource not found"),
                NotImplementedException => ((int)HttpStatusCode.NotImplemented, "Feature not implemented"),
                TimeoutException => ((int)HttpStatusCode.RequestTimeout, "Request timeout"),
                InvalidOperationException => ((int)HttpStatusCode.BadRequest, "Invalid operation"),
                HttpRequestException => ((int)HttpStatusCode.BadGateway, "External service error"),
                TaskCanceledException => ((int)HttpStatusCode.RequestTimeout, "Request was cancelled"),
                OperationCanceledException => ((int)HttpStatusCode.RequestTimeout, "Operation was cancelled"),
                _ => ((int)HttpStatusCode.InternalServerError, "An error occurred while processing your request")
            };
        }
    }

    /// <summary>
    /// Extension method to register the middleware
    /// </summary>
    public static class ErrorHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseErrorHandling(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ErrorHandlingMiddleware>();
        }
    }
}
