using Microsoft.AspNetCore.Mvc;
using ZR.Admin.WebApi.Filters;
using ZR.Admin.WebApi.Middleware;
using ZR.Admin.WebApi.Services;

namespace ZR.Admin.WebApi.Extensions
{
    /// <summary>
    /// Extension methods for configuring error handling
    /// </summary>
    public static class ErrorHandlingExtensions
    {
        /// <summary>
        /// Add comprehensive error handling services
        /// </summary>
        public static IServiceCollection AddErrorHandling(this IServiceCollection services)
        {
            // Add error logging service
            services.AddSingleton<IErrorLoggingService, ErrorLoggingService>();
            
            // Add global exception filter
            services.AddScoped<GlobalExceptionFilter>();
            services.AddScoped<ActionExceptionFilter>();
            services.AddScoped<ValidationExceptionFilter>();
            
            // Configure MVC with error handling filters
            services.Configure<MvcOptions>(options =>
            {
                // Add global filters
                options.Filters.Add<GlobalExceptionFilter>();
                options.Filters.Add<ValidationExceptionFilter>();
                
                // Return proper status codes
                options.ReturnHttpNotAcceptable = true;
                options.RespectBrowserAcceptHeader = true;
            });

            // Configure API behavior
            services.Configure<ApiBehaviorOptions>(options =>
            {
                // Custom model state error response
                options.InvalidModelStateResponseFactory = context =>
                {
                    var errors = context.ModelState
                        .Where(e => e.Value.Errors.Count > 0)
                        .Select(e => new
                        {
                            Field = e.Key,
                            Messages = e.Value.Errors.Select(x => x.ErrorMessage)
                        });

                    var result = new
                    {
                        code = 400,
                        msg = "Validation failed",
                        errors = errors,
                        timestamp = DateTime.UtcNow,
                        traceId = context.HttpContext.TraceIdentifier
                    };

                    return new BadRequestObjectResult(result);
                };
            });

            return services;
        }

        /// <summary>
        /// Use comprehensive error handling middleware
        /// </summary>
        public static IApplicationBuilder UseErrorHandling(this IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Use error handling middleware first
            app.UseMiddleware<ErrorHandlingMiddleware>();
            
            // Use exception page in development
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // Use custom error page in production
                app.UseExceptionHandler("/error");
                app.UseStatusCodePagesWithReExecute("/error/{0}");
            }
            
            return app;
        }

        /// <summary>
        /// Add error handling to specific controllers
        /// </summary>
        public static IMvcBuilder AddControllersWithErrorHandling(this IServiceCollection services)
        {
            return services.AddControllers(options =>
            {
                // Add filters globally
                options.Filters.Add<GlobalExceptionFilter>();
                options.Filters.Add<ValidationExceptionFilter>();
                
                // Add model binding error handling
                options.ModelBindingMessageProvider.SetValueMustNotBeNullAccessor(
                    value => $"The field {value} is required.");
                options.ModelBindingMessageProvider.SetValueIsInvalidAccessor(
                    value => $"The value '{value}' is invalid.");
                options.ModelBindingMessageProvider.SetAttemptedValueIsInvalidAccessor(
                    (value, field) => $"The value '{value}' is not valid for {field}.");
            });
        }
    }
}
