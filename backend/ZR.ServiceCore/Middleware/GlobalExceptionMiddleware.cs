using Infrastructure;
using Infrastructure.Attribute;
using Infrastructure.Exceptions;
using Infrastructure.Localization;
using Infrastructure.Model;
using Infrastructure.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using System;
using System.Diagnostics;
using System.Net;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using ZR.Common;
using ZR.Infrastructure.IPTools;
using ZR.Model.System;
using ZR.ServiceCore.Services;
using textJson = System.Text.Json;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace ZR.ServiceCore.Middleware
{
    /// <summary>
    /// Global exception handling middleware with localization support
    /// Handles both AppException and legacy exceptions
    /// </summary>
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        static readonly Logger NLogger = LogManager.GetCurrentClassLogger();

        private static readonly textJson.JsonSerializerOptions _jsonOptions = new()
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            PropertyNamingPolicy = textJson.JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        public GlobalExceptionMiddleware(
            RequestDelegate next,
            IWebHostEnvironment environment,
            ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _environment = environment;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Resolve scoped services from the request scope
            var localizationService = context.RequestServices.GetRequiredService<ILocalizationService>();

            try
            {
                // Detect and set language from request
                DetectAndSetLanguage(context, localizationService);

                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex, localizationService);
            }
        }

        private void DetectAndSetLanguage(HttpContext context, ILocalizationService localizationService)
        {
            // Priority 1: Query parameter ?lang=ar
            if (context.Request.Query.TryGetValue("lang", out var langQuery))
            {
                localizationService.SetLanguage(langQuery.ToString());
                return;
            }

            // Priority 2: Accept-Language header
            var acceptLanguage = context.Request.Headers["Accept-Language"].ToString();
            if (!string.IsNullOrEmpty(acceptLanguage))
            {
                // Parse "en-US,en;q=0.9,ar;q=0.8" -> "en"
                var primaryLanguage = acceptLanguage.Split(',')[0].Split('-')[0];
                localizationService.SetLanguage(primaryLanguage);
                return;
            }

            // Priority 3: JWT claim (if you store language preference)
            if (context.User?.Identity?.IsAuthenticated == true)
            {
                var languageClaim = context.User.FindFirst("language")?.Value;
                if (!string.IsNullOrEmpty(languageClaim))
                {
                    localizationService.SetLanguage(languageClaim);
                    return;
                }
            }

            // Default: English
            localizationService.SetLanguage("en");
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception, ILocalizationService localizationService)
        {
            var traceId = Activity.Current?.Id ?? context.TraceIdentifier;
            var path = context.Request.Path;
            var method = context.Request.Method;
            var user = HttpContextExtension.GetName(context) ?? "Anonymous";
            var isDevelopment = _environment.EnvironmentName == "Development" || _environment.EnvironmentName == "Staging";

            object responseObject;
            int httpStatusCode;

            if (exception is AppException appException)
            {
                // Handle new AppException with localization
                var errorResponse = HandleAppException(appException, path, traceId, isDevelopment, localizationService);
                httpStatusCode = appException.HttpStatusCode;
                responseObject = errorResponse;

                // Log with appropriate severity
                LogAppException(appException, traceId, path, method, user);

                // Also log to database using existing system
                await LogToDatabase(context, appException.Message, exception, user, path, method);
            }
            else if (exception is CustomException customException)
            {
                // Handle legacy CustomException
                httpStatusCode = customException.Code == (int)ResultCode.GLOBAL_ERROR ? 500 : 400;
                responseObject = new ApiResult(customException.Code, customException.Message);

                // Log to database
                await LogToDatabase(context, customException.Message, exception, user, path, method);

                // Log to NLog
                NLogger.Log(NLog.LogLevel.Error, exception, customException.Message);
            }
            else
            {
                // Handle unexpected exceptions
                var errorResponse = HandleGenericException(exception, path, traceId, isDevelopment, localizationService);
                httpStatusCode = 500;
                responseObject = errorResponse;

                // Log as critical error
                LogGenericException(exception, traceId, path, method, user);

                // Also log to database
                await LogToDatabase(context, exception.Message, exception, user, path, method);
            }

            // Set response
            context.Response.ContentType = "application/json;charset=utf-8";
            context.Response.StatusCode = httpStatusCode;

            var jsonResponse = System.Text.Json.JsonSerializer.Serialize(responseObject, _jsonOptions);
            await context.Response.WriteAsync(jsonResponse, System.Text.Encoding.UTF8);
        }

        private ErrorResponse HandleAppException(
            AppException exception,
            string path,
            string traceId,
            bool isDevelopment,
            ILocalizationService localizationService)
        {
            // Get localized message, fall back to exception.Message if not found
            var localizedMessage = localizationService.GetMessage(
                exception.Code,
                exception.Parameters);

            // If localization returned the code itself (not found), use the actual exception message
            if (localizedMessage == exception.Code && !string.IsNullOrEmpty(exception.Message) && exception.Message != exception.Code)
            {
                localizedMessage = exception.Message;
            }

            // Create error response with localized message
            return new ErrorResponse
            {
                Code = exception.Code,
                Message = localizedMessage,
                Details = exception.Details,
                Severity = exception.Severity.ToString(),
                Path = path,
                TraceId = traceId,
                StackTrace = isDevelopment ? exception.StackTrace : null,
                InnerException = isDevelopment ? exception.InnerException?.Message : null
            };
        }

        private ErrorResponse HandleGenericException(
            Exception exception,
            string path,
            string traceId,
            bool isDevelopment,
            ILocalizationService localizationService)
        {
            var localizedMessage = localizationService.GetMessage(ExceptionCode.INTERNAL_ERROR);

            return new ErrorResponse
            {
                Code = ExceptionCode.INTERNAL_ERROR,
                Message = localizedMessage,
                Severity = ExceptionSeverity.Critical.ToString(),
                Path = path,
                TraceId = traceId,
                StackTrace = isDevelopment ? exception.StackTrace : null,
                InnerException = isDevelopment ? exception.InnerException?.Message : null
            };
        }

        private void LogAppException(
            AppException exception,
            string traceId,
            string path,
            string method,
            string user)
        {
            var logLevel = exception.Severity switch
            {
                ExceptionSeverity.Info => Microsoft.Extensions.Logging.LogLevel.Information,
                ExceptionSeverity.Warning => Microsoft.Extensions.Logging.LogLevel.Warning,
                ExceptionSeverity.Error => Microsoft.Extensions.Logging.LogLevel.Error,
                ExceptionSeverity.Critical => Microsoft.Extensions.Logging.LogLevel.Critical,
                _ => Microsoft.Extensions.Logging.LogLevel.Error
            };

            _logger.Log(logLevel, exception,
                "AppException occurred. " +
                "Code: {Code}, " +
                "Severity: {Severity}, " +
                "TraceId: {TraceId}, " +
                "Path: {Path}, " +
                "Method: {Method}, " +
                "User: {User}, " +
                "Details: {Details}",
                exception.Code,
                exception.Severity,
                traceId,
                path,
                method,
                user,
                System.Text.Json.JsonSerializer.Serialize(exception.Details));

            // Also log to NLog for backward compatibility
            var nlogLevel = exception.Severity switch
            {
                ExceptionSeverity.Info => NLog.LogLevel.Info,
                ExceptionSeverity.Warning => NLog.LogLevel.Warn,
                ExceptionSeverity.Error => NLog.LogLevel.Error,
                ExceptionSeverity.Critical => NLog.LogLevel.Fatal,
                _ => NLog.LogLevel.Error
            };

            NLogger.Log(nlogLevel, exception,
                $"AppException: {exception.Code} - {exception.Message}");
        }

        private void LogGenericException(
            Exception exception,
            string traceId,
            string path,
            string method,
            string user)
        {
            _logger.LogCritical(exception,
                "Unhandled exception occurred. " +
                "TraceId: {TraceId}, " +
                "Path: {Path}, " +
                "Method: {Method}, " +
                "User: {User}, " +
                "Type: {ExceptionType}",
                traceId,
                path,
                method,
                user,
                exception.GetType().Name);

            // Also log to NLog
            NLogger.Fatal(exception, "Unhandled exception");
        }

        private async Task LogToDatabase(
            HttpContext context,
            string message,
            Exception exception,
            string user,
            string path,
            string method)
        {
            try
            {
                // Resolve scoped service from request scope
                var sysOperLogService = context.RequestServices.GetRequiredService<ISysOperLogService>();

                string ip = HttpContextExtension.GetClientUserIp(context);
                var ip_info = IpTool.Search(ip);

                SysOperLog sysOperLog = new()
                {
                    Status = 1,
                    OperIp = ip,
                    OperUrl = path,
                    RequestMethod = method,
                    JsonResult = message,
                    ErrorMsg = exception.Message,
                    OperName = user,
                    OperLocation = ip_info.Province + " " + ip_info.City,
                    OperTime = DateTime.Now,
                    OperParam = HttpContextExtension.GetRequestValue(context, method)
                };

                var endpoint = GetEndpoint(context);
                if (endpoint != null)
                {
                    var logAttribute = endpoint.Metadata.GetMetadata<LogAttribute>();
                    if (logAttribute != null)
                    {
                        sysOperLog.BusinessType = (int)logAttribute.BusinessType;
                        sysOperLog.Title = logAttribute?.Title;
                        sysOperLog.OperParam = logAttribute.IsSaveRequestData ? sysOperLog.OperParam : "";
                        sysOperLog.JsonResult = logAttribute.IsSaveResponseData ? sysOperLog.JsonResult : "";
                    }
                }

                sysOperLogService.InsertOperlog(sysOperLog);

                // WeChat notification for critical errors
                if (exception is AppException appEx && appEx.Severity == ExceptionSeverity.Critical)
                {
                    string errorMsg = $"> Operator: {user}" +
                        $"\n> Operation Location: {ip}({ip_info.Province} {ip_info.City})" +
                        $"\n> Operation URL: {path}" +
                        $"\n> Error Code: {appEx.Code}" +
                        $"\n> Error Message: {message}";

                    WxNoticeHelper.SendMsg("Critical Exception", errorMsg, msgType: WxNoticeHelper.MsgType.markdown);
                }
                else if (!(exception is AppException))
                {
                    // Send notification for unhandled exceptions
                    string errorMsg = $"> Operator: {user}" +
                        $"\n> Operation Location: {ip}({ip_info.Province} {ip_info.City})" +
                        $"\n> Operation URL: {path}" +
                        $"\n> Error Message: {message}";

                    WxNoticeHelper.SendMsg("System Exception", errorMsg, msgType: WxNoticeHelper.MsgType.markdown);
                }
            }
            catch (Exception ex)
            {
                // Don't fail the response if logging fails
                Console.WriteLine($"Failed to log exception to database: {ex.Message}");
                _logger.LogError(ex, "Failed to log exception to database");
            }
        }

        private static Endpoint GetEndpoint(HttpContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            return context.Features.Get<IEndpointFeature>()?.Endpoint;
        }
    }
}
