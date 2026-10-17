using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ZR.Admin.WebApi.Filters
{
    /// <summary>
    /// Action-level exception filter for handling exceptions at action level
    /// </summary>
    public class ActionExceptionFilter : ExceptionFilterAttribute
    {
        private readonly ILogger<ActionExceptionFilter> _logger;

        public ActionExceptionFilter(ILogger<ActionExceptionFilter> logger)
        {
            _logger = logger;
        }

        public override void OnException(ExceptionContext context)
        {
            HandleActionException(context);
            base.OnException(context);
        }

        public override Task OnExceptionAsync(ExceptionContext context)
        {
            HandleActionException(context);
            return base.OnExceptionAsync(context);
        }

        private void HandleActionException(ExceptionContext context)
        {
            var actionName = context.ActionDescriptor.DisplayName;
            var exception = context.Exception;
            
            _logger.LogError(exception, $"Action exception in {actionName}");

            var result = new ApiResult
            {
                
            };

            //result.Add(Code = (int)HttpStatusCode.InternalServerError,
            //    Msg = GetErrorMessage(exception),
            //    Data = null);

            context.Result = new JsonResult(result)
            {
                StatusCode = (int)HttpStatusCode.OK // Return 200 with error in body for API consistency
            };

            context.ExceptionHandled = true;
        }

        private string GetErrorMessage(Exception exception)
        {
            return exception switch
            {
                ArgumentNullException => "Required parameter is missing",
                ArgumentException => "Invalid argument provided",
                UnauthorizedAccessException => "Access denied",
                KeyNotFoundException => "Requested resource not found",
                InvalidOperationException => "Invalid operation",
                TimeoutException => "Operation timed out",
                _ => "An error occurred while processing your request"
            };
        }
    }

    /// <summary>
    /// Custom action exception attribute that can be applied to controllers or actions
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class HandleActionExceptionAttribute : TypeFilterAttribute
    {
        public HandleActionExceptionAttribute() : base(typeof(ActionExceptionFilter))
        {
        }
    }
}
