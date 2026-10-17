using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;
using NLog;

namespace ZR.Admin.WebApi.Filters
{
    /// <summary>
    /// Controller-level exception handler attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true)]
    public class ControllerExceptionHandlerAttribute : ExceptionFilterAttribute
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly string _controllerName;
        private readonly bool _logFullException;

        public ControllerExceptionHandlerAttribute(string controllerName = null, bool logFullException = false)
        {
            _controllerName = controllerName;
            _logFullException = logFullException;
        }

        public override void OnException(ExceptionContext context)
        {
            HandleControllerException(context);
            base.OnException(context);
        }

        private void HandleControllerException(ExceptionContext context)
        {
            var controller = _controllerName ?? context.RouteData.Values["controller"]?.ToString();
            var action = context.RouteData.Values["action"]?.ToString();
            var exception = context.Exception;
            var requestId = Guid.NewGuid().ToString();

            // Log the exception
            if (_logFullException)
            {
                _logger.Error(exception, $"[{requestId}] Exception in {controller}/{action}: {exception.Message}");
            }
            else
            {
                _logger.Error($"[{requestId}] Exception in {controller}/{action}: {exception.Message}");
            }

            // Create appropriate response based on exception type
            IActionResult result = exception switch
            {
                UnauthorizedAccessException _ => new UnauthorizedObjectResult(new
                {
                    code = 401,
                    msg = "Unauthorized access",
                    requestId
                }),
                
                ArgumentNullException argNull => new BadRequestObjectResult(new
                {
                    code = 400,
                    msg = $"Missing required parameter: {argNull.ParamName}",
                    requestId
                }),
                
                ArgumentException arg => new BadRequestObjectResult(new
                {
                    code = 400,
                    msg = arg.Message,
                    requestId
                }),
                
                KeyNotFoundException _ => new NotFoundObjectResult(new
                {
                    code = 404,
                    msg = "Resource not found",
                    requestId
                }),
                
                InvalidOperationException invOp => new BadRequestObjectResult(new
                {
                    code = 400,
                    msg = invOp.Message,
                    requestId
                }),
                
                NotImplementedException _ => new StatusCodeResult((int)HttpStatusCode.NotImplemented),
                
                TimeoutException _ => new StatusCodeResult((int)HttpStatusCode.RequestTimeout),
                
                _ => new ObjectResult(new
                {
                    code = 500,
                    msg = "An error occurred while processing your request",
                    requestId
                })
                {
                    StatusCode = 500
                }
            };

            context.Result = result;
            context.ExceptionHandled = true;
        }
    }
}
