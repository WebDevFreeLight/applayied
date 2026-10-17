using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Reflection;

namespace ZR.Admin.WebApi.Extensions
{
    /// <summary>
    /// Simplified Swagger configuration that works
    /// </summary>
    public static class SimpleSwaggerExtension
    {
        /// <summary>
        /// Add Swagger services
        /// </summary>
        public static IServiceCollection AddSimpleSwagger(this IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                // Create documents for each group
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "maham-ai.NET API", Version = "v1", Description = "Business API" });
                c.SwaggerDoc("sys", new OpenApiInfo { Title = "maham-ai.NET API", Version = "v1", Description = "System Management" });
                c.SwaggerDoc("hr", new OpenApiInfo { Title = "maham-ai.NET API", Version = "v1", Description = "HR Management" });
                c.SwaggerDoc("shopping", new OpenApiInfo { Title = "maham-ai.NET API", Version = "v1", Description = "Shopping Mall Management" });
                c.SwaggerDoc("article", new OpenApiInfo { Title = "maham-ai.NET API", Version = "v1", Description = "Article Management" });

                // Include XML comments
                var baseDir = AppContext.BaseDirectory;
                var xmlFiles = new[] { "ZR.Admin.WebApi.xml", "ZR.Model.xml", "ZR.ServiceCore.xml", "ZR.Service.xml" };
                foreach (var xmlFile in xmlFiles)
                {
                    var xmlPath = Path.Combine(baseDir, xmlFile);
                    if (File.Exists(xmlPath))
                    {
                        try { c.IncludeXmlComments(xmlPath, true); } catch { }
                    }
                }

                // JWT Authentication
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "JWT Authorization header using the Bearer scheme. Example: 'Bearer {token}'"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });

                // Add operation filters
                try 
                { 

                    c.OperationFilter<AddResponseHeadersFilter>();
                    c.OperationFilter<AppendAuthorizeToSummaryOperationFilter>();
                    c.OperationFilter<SecurityRequirementsOperationFilter>();
                } 
                catch {
                
                }

                // Document inclusion predicate
                c.DocInclusionPredicate((docName, apiDescription) =>
                {
                    if (apiDescription.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor)
                    {
                        // Check for explicit group attribute
                        var apiExplorerSettings = controllerActionDescriptor.ControllerTypeInfo
                            .GetCustomAttribute<ApiExplorerSettingsAttribute>();
                        
                        if (apiExplorerSettings != null && !string.IsNullOrEmpty(apiExplorerSettings.GroupName))
                        {
                            return apiExplorerSettings.GroupName == docName;
                        }

                        // Determine group from namespace
                        var namespaceName = controllerActionDescriptor.ControllerTypeInfo.Namespace ?? "";
                        
                        if (namespaceName.Contains("System"))
                            return docName == "sys";
                        if (namespaceName.Contains("HR"))
                            return docName == "hr";
                        if (namespaceName.Contains("Mall") || namespaceName.Contains("Shopping"))
                            return docName == "shopping";
                        if (namespaceName.Contains("Content") || namespaceName.Contains("Article"))
                            return docName == "article";
                        
                        // Default group
                        return docName == "v1";
                    }
                    
                    return docName == "v1";
                });

                // Resolve conflicts
                c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
                
                // Custom schema IDs
                c.CustomSchemaIds(type => type.FullName?.Replace("+", ".").Split('.').Last() ?? type.Name);
                
                // Enable annotations
                try { c.EnableAnnotations(); } catch { }
            });

            return services;
        }

        /// <summary>
        /// Use Swagger middleware
        /// </summary>
        public static IApplicationBuilder UseSimpleSwagger(this IApplicationBuilder app)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Business API");
                c.SwaggerEndpoint("/swagger/sys/swagger.json", "System Management");
                c.SwaggerEndpoint("/swagger/hr/swagger.json", "HR Management");
                c.SwaggerEndpoint("/swagger/shopping/swagger.json", "Shopping Mall");
                c.SwaggerEndpoint("/swagger/article/swagger.json", "Article Management");
                
                c.RoutePrefix = string.Empty; // Serve at root
                c.DocumentTitle = "maham-ai.NET API Documentation";
                c.DocExpansion(DocExpansion.None);
                c.DefaultModelsExpandDepth(-1);
                c.DisplayRequestDuration();
                c.EnableFilter();
                c.EnableDeepLinking();
                c.EnableTryItOutByDefault();
            });

            return app;
        }
    }
}
