using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;
using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.SwaggerUI;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ZR.Admin.WebApi.Extensions
{
    /// <summary>
    /// Unified API Documentation configuration for both Swagger and Scalar
    /// </summary>
    public static class ApiDocumentationExtension
    {
        public static   (string Name, string Title, string Description)[] ApiGroups = new[]
        {
            ("all", "All APIs", "Complete API  Documentation"),
            ("sys", "System Management", "System Management API Documentation"),
            ("hr", "HR Management", "Human Resources Management API Documentation"),
            ("monitor", "System Monitor", "System Monitoring API Documentation"),
            ("tools", "Tools & Utilities", "Tools and Code Generation API Documentation"),
            ("email", "Email Service", "Email Service API Documentation"),
            ("article", "Content Management", "Article/Content Management API Documentation"),
            ("shopping", "Shopping Mall", "Shopping Mall Management API Documentation"),
            ("public", "Public APIs", "Public API Documentation"),
            ("business", "Business Logic", "Business   Logic API Documentation"),
            ("boulevard", "boulevard   Logic", "boulevard   Logic API Documentation")
        };

        /// <summary>
        /// Add API Documentation services (Swagger + Scalar)
        /// </summary>
        public static IServiceCollection AddApiDocumentation(this IServiceCollection services, IConfiguration configuration)
        {
            var apiDocConfig = configuration.GetSection("ApiDocumentation");
            var enableSwagger = apiDocConfig.GetValue<bool>("EnableSwagger", true);
            var enableScalar = apiDocConfig.GetValue<bool>("EnableScalar", true);

            if (!enableSwagger && !enableScalar)
            {
                return services;
            }

            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
            {
                // Create documents for all API groups
                foreach (var (name, title, description) in ApiGroups)
                {
                    options.SwaggerDoc(name, new OpenApiInfo
                    {
                        Title = $"maham-ai.NET - {title}",
                        Version = "v1",
                        Description = description,
                        Contact = new OpenApiContact
                        {
                            Name = configuration["Application:Contact:Name"] ?? "API   Support",
                            Email = configuration["Application:Contact:Email"],
                            Url = new Uri(configuration["Application:Contact:Url"] ?? "https://github.com")
                        },
                        License = new OpenApiLicense
                        {
                            Name = configuration["Application:License:Name"] ?? "MIT 0",
                            Url = new Uri(configuration["Application:License:Url"] ?? "https://opensource.org/licenses/MIT")
                        }
                    });
                }

                // Include XML comments
                try
                {
                    var baseDir = AppContext.BaseDirectory;
                    var xmlFiles = System.IO.Directory.GetFiles(baseDir, "*.xml");
                    foreach (var xmlPath in xmlFiles)
                    {
                        try { options.IncludeXmlComments(xmlPath, true); } catch { }
                    }
                }
                catch { }

                // JWT Authentication
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "JWT Authorization header using 0 the Bearer scheme.\r\n\r\n" +
                                  "Enter 'Bearer' [space] and then your token.\r\n\r\n" +
                                  "Example: 'Bearer abcdef'"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
                    options.OperationFilter<AddResponseHeadersFilter>();
                    options.OperationFilter<AppendAuthorizeToSummaryOperationFilter>();
                    options.OperationFilter<SecurityRequirementsOperationFilter>();
                }
                catch { }

                // Document inclusion predicate
                options.DocInclusionPredicate((docName, apiDescription) =>
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

                        return docName switch
                        {
                            "sys" => IsSystemApi(controllerName, namespaceName),
                            "hr" => IsHRApi(controllerName, namespaceName),
                            "shopping" => IsShoppingApi(controllerName, namespaceName),
                            "article" => IsArticleApi(controllerName, namespaceName),
                            "email" => IsEmailApi(controllerName, namespaceName),
                            "monitor" => IsMonitorApi(controllerName, namespaceName),
                            "tools" => IsToolsApi(controllerName, namespaceName),
                            "public" => IsPublicApi(controllerName, namespaceName),
                            "business" => IsBusinessApi(controllerName, namespaceName),
                            "boulevard" => IsboulevardApi(controllerName, namespaceName),
                            _ => false
                        };
                    }

                    return docName == "business";
                });

                // Resolve conflicts
                options.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());

                // Custom schema IDs - Use full namespace to avoid conflicts
                options.CustomSchemaIds(type =>
                {
                    // Use full type name with namespace to avoid conflicts (e.g., two AccountType enums)
                    var fullName = type.FullName?.Replace("+", ".") ?? type.Name;

                    if (type.IsGenericType)
                    {
                        var genericName = fullName.Split('`')[0];
                        var genericArgs = string.Join("_", type.GetGenericArguments().Select(t => t.Name));
                        return $"{genericName}_{genericArgs}";
                    }

                    // Return full namespace path to ensure uniqueness
                    return fullName;
                });

                // Custom operation IDs
                options.CustomOperationIds(apiDescription =>
                {
                    if (apiDescription.ActionDescriptor is ControllerActionDescriptor actionDescriptor)
                    {
                        return $"{actionDescriptor.ControllerName}_{actionDescriptor.ActionName}_{apiDescription.HttpMethod?.ToLower() ?? "get"}";
                    }
                    return apiDescription.RelativePath?.Replace("/", "_").Replace("{", "").Replace("}", "") ?? "operation";
                });

                // Enable annotations
                try { options.EnableAnnotations(); } catch { }
            });

            return services;
        }

        /// <summary>
        /// Use API Documentation middleware (Swagger + Scalar)
        /// </summary>
        public static IApplicationBuilder UseApiDocumentation(this IApplicationBuilder app, IConfiguration configuration)
        {
            var apiDocConfig = configuration.GetSection("ApiDocumentation");
            var enableSwagger = apiDocConfig.GetValue<bool>("EnableSwagger", true);
            var enableScalar = apiDocConfig.GetValue<bool>("EnableScalar", true);
            var swaggerPrefix = apiDocConfig.GetValue<string>("SwaggerRoutePrefix", "swagger");
            
            // Force enable Swagger for debugging
            enableSwagger = true;

            if (!enableSwagger && !enableScalar)
            {
                return app;
            }

            // Always generate Swagger JSON if either is enabled
            app.UseSwagger(c =>
            {
                c.RouteTemplate = $"{swaggerPrefix}/{{documentName}}/swagger.json";
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

            // Use Swagger UI if enabled
            if (enableSwagger)
            {
                // Log that Swagger is being configured
                var logger = app.ApplicationServices.GetService<ILogger<Program>>();
                logger?.LogInformation("Configuring Swagger UI at /swagger/index.html");
                
                app.UseSwaggerUI(c =>
                {
                    // Add all group endpoints with emojis for better UX
                    c.SwaggerEndpoint($"/swagger/all/swagger.json", "📚 All APIs");
                    c.SwaggerEndpoint($"/swagger/sys/swagger.json", "⚙️ System Management");
                    c.SwaggerEndpoint($"/swagger/hr/swagger.json", "👥 HR Management");
                    c.SwaggerEndpoint($"/swagger/monitor/swagger.json", "📊 System Monitor");
                    c.SwaggerEndpoint($"/swagger/tools/swagger.json", "🔧 Tools & Utilities");
                    c.SwaggerEndpoint($"/swagger/email/swagger.json", "📧 Email Service");
                    c.SwaggerEndpoint($"/swagger/article/swagger.json", "📝 Content Management");
                    c.SwaggerEndpoint($"/swagger/shopping/swagger.json", "🛒 Shopping Mall");
                    c.SwaggerEndpoint($"/swagger/public/swagger.json", "🌐 Public APIs");
                    c.SwaggerEndpoint($"/swagger/business/swagger.json", "💼 Business Logic");
                    c.SwaggerEndpoint($"/swagger/boulevard/swagger.json", "💼 boulevard Logic");

                    // UI Configuration - set to "swagger" to serve at /swagger/index.html
                    c.RoutePrefix = "swagger";
                    c.DocumentTitle = apiDocConfig.GetValue<string>("DocumentTitle", "maham-ai.NET API Documentation");
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
                            .swagger-ui .topbar .download-url-wrapper { display: flex !important; }
                            .swagger-ui .topbar .download-url-wrapper select { min-width: 200px; }
                        </style>
                    ";
                });
            }

            return app;
        }

        /// <summary>
        /// Map Scalar documentation endpoints (Deprecated - Use ScalarApiDocumentationExtension.ConfigureScalarDocumentation instead)
        /// </summary>
        [Obsolete("Use ScalarApiDocumentationExtension.ConfigureScalarDocumentation for better group support")]
        public static void MapScalarDocumentation(this WebApplication app, IConfiguration configuration)
        {
            var apiDocConfig = configuration.GetSection("ApiDocumentation");
            var enableScalar = apiDocConfig.GetValue<bool>("EnableScalar", true);
            
            if (!enableScalar)
                return;

            var swaggerPrefix = apiDocConfig.GetValue<string>("SwaggerRoutePrefix", "swagger");
            var scalarPrefix = apiDocConfig.GetValue<string>("ScalarRoutePrefix", "scalar");
            var documentTitle = apiDocConfig.GetValue<string>("DocumentTitle", "maham-ai.NET API Documentation");

            // Create a custom HTML page that lists all API groups for Scalar
            app.MapGet($"/{scalarPrefix}", async context =>
            {
                var baseUrl = $"{context.Request.Scheme}://{context.Request.Host}";
                var html = $@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>{documentTitle} - Scalar API Documentation</title>
    <style>
        * {{ margin: 0; padding: 0; box-sizing: border-box; }}
        body {{
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            min-height: 100vh;
            display: flex;
            flex-direction: column;
            align-items: center;
            padding: 2rem;
        }}
        .container {{
            background: white;
            border-radius: 12px;
            box-shadow: 0 20px 40px rgba(0,0,0,0.1);
            padding: 2rem;
            max-width: 800px;
            width: 100%;
        }}
        h1 {{
            color: #333;
            margin-bottom: 0.5rem;
            font-size: 2rem;
        }}
        .subtitle {{
            color: #666;
            margin-bottom: 2rem;
            font-size: 1.1rem;
        }}
        .groups {{
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
            gap: 1rem;
        }}
        .group-card {{
            background: #f8f9fa;
            border: 2px solid transparent;
            border-radius: 8px;
            padding: 1.2rem;
            text-decoration: none;
            color: #333;
            transition: all 0.3s ease;
            display: flex;
            align-items: center;
            gap: 1rem;
        }}
        .group-card:hover {{
            background: white;
            border-color: #667eea;
            transform: translateY(-2px);
            box-shadow: 0 10px 20px rgba(102, 126, 234, 0.1);
        }}
        .emoji {{
            font-size: 1.5rem;
        }}
        .group-info {{
            flex: 1;
        }}
        .group-title {{
            font-weight: 600;
            margin-bottom: 0.25rem;
            color: #333;
        }}
        .group-desc {{
            font-size: 0.875rem;
            color: #666;
        }}
        .all-apis {{
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            grid-column: 1 / -1;
        }}
        .all-apis:hover {{
            transform: translateY(-3px);
            box-shadow: 0 15px 30px rgba(102, 126, 234, 0.3);
        }}
        .all-apis .group-title, .all-apis .group-desc {{
            color: white;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <h1>🎨 Scalar API Documentation</h1>
        <p class='subtitle'>Select an API group to explore the documentation</p>
        <div class='groups'>
            <a href='/{scalarPrefix}/all' class='group-card all-apis'>
                <span class='emoji'>📚</span>
                <div class='group-info'>
                    <div class='group-title'>All APIs</div>
                    <div class='group-desc'>Complete API Documentation</div>
                </div>
            </a>
            <a href='/{scalarPrefix}/sys' class='group-card'>
                <span class='emoji'>⚙️</span>
                <div class='group-info'>
                    <div class='group-title'>System Management</div>
                    <div class='group-desc'>User, Role, Menu, Config APIs</div>
                </div>
            </a>
            <a href='/{scalarPrefix}/hr' class='group-card'>
                <span class='emoji'>👥</span>
                <div class='group-info'>
                    <div class='group-title'>HR Management</div>
                    <div class='group-desc'>Employee, Payroll, Leave APIs</div>
                </div>
            </a>
            <a href='/{scalarPrefix}/monitor' class='group-card'>
                <span class='emoji'>📊</span>
                <div class='group-info'>
                    <div class='group-title'>System Monitor</div>
                    <div class='group-desc'>Logs, Performance, Online Users</div>
                </div>
            </a>
            <a href='/{scalarPrefix}/tools' class='group-card'>
                <span class='emoji'>🔧</span>
                <div class='group-info'>
                    <div class='group-title'>Tools & Utilities</div>
                    <div class='group-desc'>Code Generation, File Upload</div>
                </div>
            </a>
            <a href='/{scalarPrefix}/email' class='group-card'>
                <span class='emoji'>📧</span>
                <div class='group-info'>
                    <div class='group-title'>Email Service</div>
                    <div class='group-desc'>Email Templates, Logs</div>
                </div>
            </a>
            <a href='/{scalarPrefix}/article' class='group-card'>
                <span class='emoji'>📝</span>
                <div class='group-info'>
                    <div class='group-title'>Content Management</div>
                    <div class='group-desc'>Articles, Comments, Topics</div>
                </div>
            </a>
            <a href='/{scalarPrefix}/shopping' class='group-card'>
                <span class='emoji'>🛒</span>
                <div class='group-info'>
                    <div class='group-title'>Shopping Mall</div>
                    <div class='group-desc'>Products, Orders, Categories</div>
                </div>
            </a>
            <a href='/{scalarPrefix}/public' class='group-card'>
                <span class='emoji'>🌐</span>
                <div class='group-info'>
                    <div class='group-title'>Public APIs</div>
                    <div class='group-desc'>Banner, WeChat APIs</div>
                </div>
            </a>
            <a href='/{scalarPrefix}/business' class='group-card'>
                <span class='emoji'>💼</span>
                <div class='group-info'>
                    <div class='group-title'>Business Logic</div>
                    <div class='group-desc'>Custom Business APIs</div>
                </div>
            </a>


    <a href='/{scalarPrefix}/boulevard' class='group-card'>
                <span class='emoji'>💼</span>
                <div class='group-info'>
                    <div class='group-title'>boulevard Logic</div>
                    <div class='group-desc'>Custom boulevard APIs</div>
                </div>
            </a>

        </div>
    </div>
</body>
</html>";
                context.Response.ContentType = "text/html";
                await context.Response.WriteAsync(html);
            });



            // Map Scalar API reference with proper document name placeholder
            app.MapScalarApiReference(options =>
            {
                options.WithTitle(documentTitle)
                       .WithTheme(ScalarTheme.Purple)
                       .WithDarkMode(true)
                       .WithEndpointPrefix($"/{scalarPrefix}/{{documentName}}")
                       .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient)
                       .WithPreferredScheme("Bearer")
                       .WithOpenApiRoutePattern($"/{swaggerPrefix}/{{documentName}}/swagger.json")
                       .WithSidebar(true)
                       .WithSearchHotKey("k")
                       .WithModels(true)
                       .WithDownloadButton(true);
            });
        }

        /// <summary>
        /// Log API documentation URLs
        /// </summary>
        public static void LogApiDocumentationUrls(this IApplicationBuilder app, IConfiguration configuration)
        {
            var apiDocConfig = configuration.GetSection("ApiDocumentation");
            var enableSwagger = apiDocConfig.GetValue<bool>("EnableSwagger", true);
            var enableScalar = apiDocConfig.GetValue<bool>("EnableScalar", true);
            var swaggerPrefix = apiDocConfig.GetValue<string>("SwaggerRoutePrefix", "swagger");
            var scalarPrefix = apiDocConfig.GetValue<string>("ScalarRoutePrefix", "scalar");
            var urls = configuration["urls"] ?? "http://localhost:5000";

            var baseUrl = urls.Split(';')[0];

            Console.WriteLine("\n========================================");
            Console.WriteLine("🚀 API Documentation Available  :");
            Console.WriteLine("========================================");

            if (enableSwagger)
            {
                Console.WriteLine($"📖 Swagger   UI: {baseUrl}/{swaggerPrefix}");
                Console.WriteLine("   Available Groups:");
                foreach (var (name, title, _) in ApiGroups)
                {
                    Console.WriteLine($"   - {title}: {baseUrl}/{swaggerPrefix}/?urls.primaryName={name}");
                }
            }

            if (enableScalar)
            {
                Console.WriteLine($"\n🎨 Scalar   API Documentation:");
                Console.WriteLine("   Available Groups:");
                foreach (var (name, title, _) in ApiGroups)
                {
                    Console.WriteLine($"   - {title}: {baseUrl}/{scalarPrefix}/{name}");
                }
            }

            if (enableSwagger || enableScalar)
            {
                Console.WriteLine($"\n📄 OpenAPI JSON Endpoints:");
                foreach (var (name, title, _) in ApiGroups)
                {
                    Console.WriteLine($"   - {title}: {baseUrl}/{swaggerPrefix}/{name}/swagger.json");
                }
            }

            Console.WriteLine("========================================\n");
        }

        #region Helper Methods

        private static bool IsSystemApi(string controllerName, string namespaceName)
        {
            return (namespaceName.Contains("system") && !namespaceName.Contains("monitor")) ||
                   controllerName.StartsWith("sys") && !controllerName.Contains("online") ||
                   controllerName is "syslogincontroller" or "sysrolecontroller" or "sysmenucontroller" or
                   "sysdeptcontroller" or "sysusercontroller" or "syspostcontroller" or
                   "sysdictdatacontroller" or "sysdicttypecontroller" or "sysconfigcontroller" or
                   "sysnoticecontroller" or "sysfilecontroller" or "sysfilegroupcontroller" or
                   "sysprofilecontroller" or "sysuserrolecontroller" or "sysusermsgcontroller" or
                   "commonlangcontroller" or "taskscontroller" or "taskslogcontroller";
        }

        private static bool IsHRApi(string controllerName, string namespaceName)
        {
            return namespaceName.Contains(".hr") ||
                   controllerName.StartsWith("hr") ||
                   controllerName is "employeescontroller" or "attendancecontroller" or
                   "leavecontroller" or "payrollcontroller" or "performancecontroller" or
                   "recruitmentcontroller" ||
                   (controllerName == "documentcontroller" && namespaceName.Contains(".hr"));
        }

        private static bool IsShoppingApi(string controllerName, string namespaceName)
        {
            return namespaceName.Contains("mall") || namespaceName.Contains("shopping") ||
                   controllerName.Contains("product") || controllerName.Contains("category") ||
                   controllerName.Contains("brand") || controllerName.Contains("skus") ||
                   controllerName.Contains("order");
        }

        private static bool IsArticleApi(string controllerName, string namespaceName)
        {
            return namespaceName.Contains("content") ||
                   controllerName is "articlecontroller" or "articlecategorycontroller" or
                   "articletopiccontroller" or "frontarticlecontroller" or
                   "frontcommentcontroller" or "frontusercirclescontroller" or "momentscontroller";
        }

        private static bool IsEmailApi(string controllerName, string namespaceName)
        {
            return namespaceName.Contains("email") ||
                   controllerName is "emaillogcontroller" or "emailtplcontroller";
        }

        private static bool IsMonitorApi(string controllerName, string namespaceName)
        {
            return namespaceName.Contains("monitor") ||
                   controllerName is "monitorcontroller" or "syslogininforcontroller" or
                   "sysoperlogcontroller" or "sysuseronlinecontroller" or
                   "useronlinelogcontroller" or "smscodelogcontroller" or "sqldifflogcontroller";
        }

        private static bool IsToolsApi(string controllerName, string namespaceName)
        {
            return controllerName is "codegeneratorcontroller" or "commoncontroller";
        }

        private static bool IsPublicApi(string controllerName, string namespaceName)
        {
            return namespaceName.Contains("public") ||
                   controllerName is "bannerconfigcontroller" or "wxopencontroller";
        }

        private static bool IsboulevardApi(string controllerName, string namespaceName)
        {
            return namespaceName.Contains("boulevard") ||
                   controllerName is "bannerconfigcontroller" or "wxopencontroller";
        }

        private static bool IsBusinessApi(string controllerName, string namespaceName)
        {
            // Everything else goes to business
            return !IsSystemApi(controllerName, namespaceName) &&
                   !IsHRApi(controllerName, namespaceName) &&
                   !IsShoppingApi(controllerName, namespaceName) &&
                   !IsArticleApi(controllerName, namespaceName) &&
                   !IsEmailApi(controllerName, namespaceName) &&
                   !IsMonitorApi(controllerName, namespaceName) &&
                   !IsToolsApi(controllerName, namespaceName) &&
                   !IsboulevardApi(controllerName, namespaceName) &&
                   !IsPublicApi(controllerName, namespaceName);
        }

        #endregion
    }
}
