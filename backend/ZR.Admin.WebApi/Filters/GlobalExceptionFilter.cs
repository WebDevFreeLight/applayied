using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;
using NLog;

namespace ZR.Admin.WebApi.Filters
{
    /// <summary>
    /// Global exception filter for handling all unhandled exceptions
    /// </summary>
    public class GlobalExceptionFilter : IExceptionFilter, IAsyncExceptionFilter
    {
        private readonly ILogger<GlobalExceptionFilter> _logger;
        private readonly IWebHostEnvironment _env;
        private static readonly Logger nlog = LogManager.GetCurrentClassLogger();

        public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger, IWebHostEnvironment env)
        {
            _logger = logger;
            _env = env;
        }

        public void OnException(ExceptionContext context)
        {
            HandleException(context);
        }

        public Task OnExceptionAsync(ExceptionContext context)
        {
            HandleException(context);
            return Task.CompletedTask;
        }

        private void HandleException(ExceptionContext context)
        {
            var exception = context.Exception;
            var controllerName = context.RouteData.Values["controller"]?.ToString();
            var actionName = context.RouteData.Values["action"]?.ToString();
            var requestId = context.HttpContext.TraceIdentifier;
            
            // Log the exception with full details
            var errorMessage = $"Exception in {controllerName}/{actionName} - RequestId: {requestId}";
            _logger.LogError(exception, errorMessage);
            nlog.Error(exception, errorMessage);

            // Create error response based on exception type
            var response = CreateErrorResponse(exception, requestId);
            
            // Set response
            context.Result = new ObjectResult(response)
            {
                StatusCode = response.StatusCode
            };

            context.ExceptionHandled = true;
        }

        private ErrorResponse CreateErrorResponse(Exception exception, string requestId)
        {
            var response = new ErrorResponse
            {
                RequestId = requestId,
                Timestamp = DateTime.UtcNow,
                //Path = HttpContext.Current?.Request.Path.Value
            };

            switch (exception)
            {
                case UnauthorizedAccessException _:
                    response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    response.Message = "Unauthorized access";
                    response.Details = "You do not have permission to access this resource";
                    break;

                case ArgumentNullException argNullEx:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Message = "Invalid request";
                    response.Details = $"Required parameter is missing: {argNullEx.ParamName}";
                    break;

                case ArgumentException argEx:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Message = "Invalid argument";
                    response.Details = argEx.Message;
                    break;

                case KeyNotFoundException _:
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    response.Message = "Resource not found";
                    response.Details = "The requested resource could not be found";
                    break;

                case NotImplementedException _:
                    response.StatusCode = (int)HttpStatusCode.NotImplemented;
                    response.Message = "Feature not implemented";
                    response.Details = "This feature is not yet implemented";
                    break;

                case TimeoutException _:
                    response.StatusCode = (int)HttpStatusCode.RequestTimeout;
                    response.Message = "Request timeout";
                    response.Details = "The request took too long to process";
                    break;

                case InvalidOperationException invOpEx:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Message = "Invalid operation";
                    response.Details = invOpEx.Message;
                    break;

                case HttpRequestException httpEx:
                    response.StatusCode = (int)HttpStatusCode.BadGateway;
                    response.Message = "External service error";
                    response.Details = "Error communicating with external service";
                    break;

                default:
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    response.Message = "An error occurred while processing your request";
                    response.Details = _env.IsDevelopment() ? exception.Message : "Internal server error";
                    break;
            }

            // Add stack trace in development
            if (_env.IsDevelopment())
            {
                response.StackTrace = exception.StackTrace;
                response.InnerException = exception.InnerException?.Message;
            }

            return response;
        }
    }

    /// <summary>
    /// Error response model
    /// </summary>
    public class ErrorResponse
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public string Details { get; set; }
        public string RequestId { get; set; }
        public DateTime Timestamp { get; set; }
        public string Path { get; set; }
        public string StackTrace { get; set; }
        public string InnerException { get; set; }
    }
}
