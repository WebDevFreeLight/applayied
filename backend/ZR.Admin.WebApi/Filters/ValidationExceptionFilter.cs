using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;

namespace ZR.Admin.WebApi.Filters
{
    /// <summary>
    /// Validation exception filter for handling model validation errors
    /// </summary>
    public class ValidationExceptionFilter : IActionFilter
    {
        private readonly ILogger<ValidationExceptionFilter> _logger;

        public ValidationExceptionFilter(ILogger<ValidationExceptionFilter> logger)
        {
            _logger = logger;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                var errors = context.ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                    );

                _logger.LogWarning($"Validation failed for {context.ActionDescriptor.DisplayName}: {string.Join(", ", errors.Keys)}");

                var response = new ValidationErrorResponse
                {
                    Code = 400,
                    Msg = "Validation failed",
                    Errors = errors
                };

                context.Result = new BadRequestObjectResult(response);
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // Not needed for validation
        }
    }

    /// <summary>
    /// Validation error response model
    /// </summary>
    public class ValidationErrorResponse
    {
        public int Code { get; set; }
        public string Msg { get; set; }
        public Dictionary<string, string[]> Errors { get; set; }
    }
}
