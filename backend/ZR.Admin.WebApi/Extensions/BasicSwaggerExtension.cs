using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace ZR.Admin.WebApi.Extensions
{
    /// <summary>
    /// Most basic Swagger configuration that works
    /// </summary>
    public static class BasicSwaggerExtension
    {
        /// <summary>
        /// Add basic Swagger services
        /// </summary>
        public static IServiceCollection AddBasicSwagger(this IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo 
                { 
                    Title = "maham-ai.NET API", 
                    Version = "v1",
                    Description = "API Documentation"
                });

                // Try to include XML comments
                try
                {
                    var baseDir = AppContext.BaseDirectory;
                    var xmlFile = "ZR.Admin.WebApi.xml";
                    var xmlPath = Path.Combine(baseDir, xmlFile);
                    if (File.Exists(xmlPath))
                    {
                        c.IncludeXmlComments(xmlPath);
                    }
                }
                catch { }

                // Resolve conflicts
                c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
            });

            return services;
        }

        /// <summary>
        /// Use basic Swagger middleware
        /// </summary>
        public static IApplicationBuilder UseBasicSwagger(this IApplicationBuilder app)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "maham-ai.NET API v1");
                c.RoutePrefix = string.Empty; // Serve at root
            });

            return app;
        }
    }
}
