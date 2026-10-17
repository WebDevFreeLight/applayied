using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Reflection;

namespace ZR.Admin.WebApi.Extensions
{
    /// <summary>
    /// Comprehensive Swagger configuration with all API groups
    /// </summary>
    public static class ComprehensiveSwaggerExtension
    {
        /// <summary>
        /// Add Swagger services with all groups
        /// </summary>
        public static IServiceCollection AddComprehensiveSwagger(this IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                // Create documents for all API groups
                c.SwaggerDoc("all", new OpenApiInfo 
                { 
                    Title = "maham-ai.NET API - All", 
                    Version = "v1",
                    Description = "All API Documentation"
                });
                
                c.SwaggerDoc("sys", new OpenApiInfo 
                { 
                    Title = "maham-ai.NET API - System", 
                    Version = "v1",
                    Description = "System Management API Documentation"
                });
                
                c.SwaggerDoc("hr", new OpenApiInfo 
                { 
                    Title = "maham-ai.NET API - HR", 
                    Version = "v1",
                    Description = "Human Resources Management API Documentation"
                });
                
                c.SwaggerDoc("shopping", new OpenApiInfo 
                { 
                    Title = "maham-ai.NET API - Shopping", 
                    Version = "v1",
                    Description = "Shopping Mall Management API Documentation"
                });
                
                c.SwaggerDoc("article", new OpenApiInfo 
                { 
                    Title = "maham-ai.NET API - Content", 
                    Version = "v1",
                    Description = "Article/Content Management API Documentation"
                });
                
                c.SwaggerDoc("email", new OpenApiInfo 
                { 
                    Title = "maham-ai.NET API - Email", 
                    Version = "v1",
                    Description = "Email Service API Documentation"
                });
                
                c.SwaggerDoc("monitor", new OpenApiInfo 
                { 
                    Title = "maham-ai.NET API - Monitor", 
                    Version = "v1",
                    Description = "System Monitoring API Documentation"
                });
                
                c.SwaggerDoc("tools", new OpenApiInfo 
                { 
                    Title = "maham-ai.NET API - Tools", 
                    Version = "v1",
                    Description = "Tools and Code Generation API Documentation"
                });
                
                c.SwaggerDoc("public", new OpenApiInfo 
                { 
                    Title = "maham-ai.NET API - Public", 
                    Version = "v1",
                    Description = "Public API Documentation"
                });
                
                c.SwaggerDoc("business", new OpenApiInfo 
                { 
                    Title = "maham-ai.NET API - Business", 
                    Version = "v1",
                    Description = "Business Logic API Documentation"
                });

                // Include XML comments
                try
                {
                    var baseDir = AppContext.BaseDirectory;
                    var xmlFiles = Directory.GetFiles(baseDir, "*.xml");
                    
                    foreach (var xmlPath in xmlFiles)
                    {
                        try 
                        { 
                            c.IncludeXmlComments(xmlPath, true); 
                        } 
                        catch { }
                    }
                }
                catch { }

                // JWT Authentication
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n" +
                                  "Enter 'Bearer' [space] and then your token.\r\n\r\n" +
                                  "Example: 'Bearer 12345abcdef'"
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
                catch { }

                // Document inclusion predicate
                c.DocInclusionPredicate((docName, apiDescription) =>
                {
                    // Show all APIs in "all" group
                    if (docName == "all")
                        return true;
                        
                    if (apiDescription.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor)
                    {
                        var controller = controllerActionDescriptor.ControllerTypeInfo;
                        
                        // Check for explicit group attribute
                        var apiExplorerSettings = controller.GetCustomAttribute<ApiExplorerSettingsAttribute>();
                        if (apiExplorerSettings != null && !string.IsNullOrEmpty(apiExplorerSettings.GroupName))
                        {
                            return apiExplorerSettings.GroupName.Equals(docName, StringComparison.OrdinalIgnoreCase);
                        }

                        var controllerName = controller.Name.ToLower();
                        var namespaceName = controller.Namespace?.ToLower() ?? "";
                        
                        switch (docName)
                        {
                            case "sys":
                                return namespaceName.Contains("system") && !namespaceName.Contains("monitor") ||
                                       controllerName.StartsWith("sys") && !controllerName.Contains("online") ||
                                       controllerName == "syslogincontroller" ||
                                       controllerName == "sysrolecontroller" ||
                                       controllerName == "sysmenucontroller" ||
                                       controllerName == "sysdeptcontroller" ||
                                       controllerName == "sysusercontroller" ||
                                       controllerName == "syspostcontroller" ||
                                       controllerName == "sysdictdatacontroller" ||
                                       controllerName == "sysdicttypecontroller" ||
                                       controllerName == "sysconfigcontroller" ||
                                       controllerName == "sysnoticecontroller" ||
                                       controllerName == "sysfilecontroller" ||
                                       controllerName == "sysfilegroupcontroller" ||
                                       controllerName == "sysprofilecontroller" ||
                                       controllerName == "sysuserrolecontroller" ||
                                       controllerName == "sysusermsgcontroller" ||
                                       controllerName == "commonlangcontroller" ||
                                       controllerName == "taskscontroller" ||
                                       controllerName == "taskslogcontroller";
                                       
                            case "hr":
                                return namespaceName.Contains(".hr") ||
                                       controllerName.StartsWith("hr") ||
                                       controllerName == "employeescontroller" ||
                                       controllerName == "attendancecontroller" ||
                                       controllerName == "leavecontroller" ||
                                       controllerName == "payrollcontroller" ||
                                       controllerName == "performancecontroller" ||
                                       controllerName == "recruitmentcontroller" ||
                                       controllerName == "documentcontroller" && namespaceName.Contains(".hr");
                                       
                            case "shopping":
                                return namespaceName.Contains("mall") ||
                                       namespaceName.Contains("shopping");
                                       
                            case "article":
                                return namespaceName.Contains("content") ||
                                       controllerName == "articlecontroller" ||
                                       controllerName == "articlecategorycontroller" ||
                                       controllerName == "articletopiccontroller" ||
                                       controllerName == "frontarticlecontroller" ||
                                       controllerName == "frontcommentcontroller" ||
                                       controllerName == "frontusercirclescontroller" ||
                                       controllerName == "momentscontroller";
                                       
                            case "email":
                                return namespaceName.Contains("email") ||
                                       controllerName == "emaillogcontroller" ||
                                       controllerName == "emailtplcontroller";
                                       
                            case "monitor":
                                return namespaceName.Contains("monitor") ||
                                       controllerName == "monitorcontroller" ||
                                       controllerName == "syslogininforcontroller" ||
                                       controllerName == "sysoperlogcontroller" ||
                                       controllerName == "sysuseronlinecontroller" ||
                                       controllerName == "useronlinelogcontroller" ||
                                       controllerName == "smscodelogcontroller" ||
                                       controllerName == "sqldifflogcontroller";
                                       
                            case "tools":
                                return controllerName == "codegeneratorcontroller" ||
                                       controllerName == "commoncontroller";
                                       
                            case "public":
                                return namespaceName.Contains("public") ||
                                       controllerName == "bannerconfigcontroller" ||
                                       controllerName == "wxopencontroller";
                                       
                            case "business":
                                // Everything else goes to business
                                return !namespaceName.Contains("system") &&
                                       !namespaceName.Contains(".hr") &&
                                       !namespaceName.Contains("mall") &&
                                       !namespaceName.Contains("content") &&
                                       !namespaceName.Contains("email") &&
                                       !namespaceName.Contains("monitor") &&
                                       !namespaceName.Contains("public") &&
                                       !controllerName.StartsWith("sys") &&
                                       !controllerName.StartsWith("hr") &&
                                       controllerName != "codegeneratorcontroller" &&
                                       controllerName != "commoncontroller";
                        }
                    }
                    
                    return docName == "business";
                });

                // Resolve conflicts
                c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
                
                // Custom schema IDs
                c.CustomSchemaIds(type => 
                {
                    var name = type.FullName?.Replace("+", ".").Split('.').Last() ?? type.Name;
                    if (type.IsGenericType)
                    {
                        var genericName = name.Split('`')[0];
                        var genericArgs = string.Join("_", type.GetGenericArguments().Select(t => t.Name));
                        return $"{genericName}_{genericArgs}";
                    }
                    return name;
                });
                
                // Custom operation IDs
                c.CustomOperationIds(apiDescription =>
                {
                    if (apiDescription.ActionDescriptor is ControllerActionDescriptor actionDescriptor)
                    {
                        return $"{actionDescriptor.ControllerName}_{actionDescriptor.ActionName}_{apiDescription.HttpMethod?.ToLower() ?? "get"}";
                    }
                    return apiDescription.RelativePath?.Replace("/", "_").Replace("{", "").Replace("}", "") ?? "operation";
                });
                
                // Enable annotations
                try { c.EnableAnnotations(); } catch { }
            });

            return services;
        }

        /// <summary>
        /// Use Swagger middleware with all groups
        /// </summary>
        public static IApplicationBuilder UseComprehensiveSwagger(this IApplicationBuilder app)
        {
            // First, use Dynamic Swagger configuration if available
            var config = app.ApplicationServices.GetService<IConfiguration>();
            if (config != null)
            {
                app.UseDynamicSwagger(config);
            }
            else
            {
                // Fallback to basic Swagger configuration
                app.UseSwagger(c =>
                {
                    c.RouteTemplate = "swagger/{documentName}/swagger.json";
                    c.PreSerializeFilters.Add((swaggerDoc, httpReq) =>
                    {
                        swaggerDoc.Servers = new List<OpenApiServer>
                        {
                            new OpenApiServer 
                            { 
                                Url = $"{httpReq.Scheme}://{httpReq.Host.Value}",
                                Description = "Current Server"
                            }
                        };
                    });
                });
                
                app.UseSwaggerUI(c =>
                {
                    // Add all group endpoints
                    c.SwaggerEndpoint("/swagger/all/swagger.json", "📚 All APIs");
                    c.SwaggerEndpoint("/swagger/sys/swagger.json", "⚙️ System Management");
                    c.SwaggerEndpoint("/swagger/hr/swagger.json", "👥 HR Management");
                    c.SwaggerEndpoint("/swagger/monitor/swagger.json", "📊 System Monitor");
                    c.SwaggerEndpoint("/swagger/tools/swagger.json", "🔧 Tools & Utilities");
                    c.SwaggerEndpoint("/swagger/email/swagger.json", "📧 Email Service");
                    c.SwaggerEndpoint("/swagger/article/swagger.json", "📝 Content Management");
                    c.SwaggerEndpoint("/swagger/shopping/swagger.json", "🛒 Shopping Mall");
                    c.SwaggerEndpoint("/swagger/public/swagger.json", "🌐 Public APIs");
                    c.SwaggerEndpoint("/swagger/business/swagger.json", "💼 Business Logic");
                    
                    // UI Configuration
                    c.RoutePrefix = "swagger"; // Serve at /swagger
                    c.DocumentTitle = "maham-ai.NET API Documentation";
                    c.DocExpansion(DocExpansion.None);
                    c.DefaultModelsExpandDepth(-1);
                    c.DisplayRequestDuration();
                    c.EnableFilter();
                    c.EnableDeepLinking();
                    c.EnableTryItOutByDefault();
                    c.EnablePersistAuthorization();
                    c.DisplayOperationId();
                    c.ShowExtensions();
                    c.ShowCommonExtensions();
                    
                    // Custom CSS for better appearance
                    c.InjectStylesheet("/swagger-ui/custom.css");
                    
                    // Add custom header content
                    c.HeadContent = @"
                        <style>
                            .swagger-ui .topbar { background-color: #2c3e50; }
                            .swagger-ui .info .title { color: #2c3e50; }
                            .swagger-ui select { padding: 5px 10px; border-radius: 4px; }
                            .swagger-ui .scheme-container { background: #f7f7f7; padding: 15px; border-radius: 4px; }
                        </style>
                    ";
                });
            }
            
            // Now configure Scalar to work alongside Swagger
            ConfigureScalarIntegration(app, config);

            return app;
        }
        
        /// <summary>
        /// Configure Scalar integration with Swagger
        /// </summary>
        private static void ConfigureScalarIntegration(IApplicationBuilder app, IConfiguration config)
        {
            if (config == null) return;
            
            // Use the existing Scalar configuration if available
            var useScalar = config.GetValue<bool>("Scalar:Enabled", true);
            if (useScalar && app is WebApplication webApp)
            {
                webApp.UseScalarWithSwagger(config);
            }
        }
    }
}
