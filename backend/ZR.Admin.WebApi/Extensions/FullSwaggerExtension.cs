using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Reflection;

namespace ZR.Admin.WebApi.Extensions
{
    /// <summary>
    /// Full Swagger configuration with all API groups d
    /// </summary>
    public static class FullSwaggerExtension
    {
        /// <summary>
        /// Add Swagger services with all groups
        /// </summary>
        public static IServiceCollection AddFullSwagger(this IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                // Create documents for each API group ds
                c.SwaggerDoc("v1", new OpenApiInfo 
                { 
                    Title = "maham-ai.NET API - Business", 
                    Version = "v1",
                    Description = "Business API Documentation"
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
                    Title = "maham-ai.NET API - Shopping Mall", 
                    Version = "v1",
                    Description = "Shopping Mall Management API Documentation"
                });
                
                c.SwaggerDoc("article", new OpenApiInfo 
                { 
                    Title = "maham-ai.NET API - Article", 
                    Version = "v1",
                    Description = "Article/Content Management API Documentation"
                });

                // Include XML comments
                try
                {
                    var baseDir = AppContext.BaseDirectory;
                    var xmlFiles = new[] 
                    { 
                        "ZR.Admin.WebApi.xml", 
                        "ZR.Model.xml", 
                        "ZR.ServiceCore.xml", 
                        "ZR.Service.xml",
                        "ZR.Mall.xml",
                        "ZR.Common.xml",
                        "ZR.CodeGenerator.xml",
                        "ZR.Tasks.xml",
                        "Infrastructure.xml",
                        "ZR.Repository.xml"
                    };
                    
                    foreach (var xmlFile in xmlFiles)
                    {
                        var xmlPath = Path.Combine(baseDir, xmlFile);
                        if (File.Exists(xmlPath))
                        {
                            try 
                            { 
                                c.IncludeXmlComments(xmlPath, true); 
                            } 
                            catch (Exception ex) 
                            { 
                                Console.WriteLine($"Warning: Could not load XML comments from {xmlFile}: {ex.Message}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Error loading XML comments: {ex.Message}");
                }

                // JWT Authentication
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n" +
                                  "Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\n" +
                                  "Example: 'Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...'"
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

                // Add operation filters safely
                try 
                { 
                    c.OperationFilter<AddResponseHeadersFilter>();
                    c.OperationFilter<AppendAuthorizeToSummaryOperationFilter>();
                    c.OperationFilter<SecurityRequirementsOperationFilter>();
                } 
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Could not add operation filters: {ex.Message}");
                }

                // Document inclusion predicate to assign APIs to correct groups
                c.DocInclusionPredicate((docName, apiDescription) =>
                {
                    if (apiDescription.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor)
                    {
                        var controller = controllerActionDescriptor.ControllerTypeInfo;
                        
                        // Check for explicit ApiExplorerSettings attribute
                        var apiExplorerSettings = controller.GetCustomAttribute<ApiExplorerSettingsAttribute>();
                        if (apiExplorerSettings != null && !string.IsNullOrEmpty(apiExplorerSettings.GroupName))
                        {
                            return apiExplorerSettings.GroupName.Equals(docName, StringComparison.OrdinalIgnoreCase);
                        }

                        // Get controller and namespace info
                        var controllerName = controller.Name.ToLower();
                        var namespaceName = controller.Namespace?.ToLower() ?? "";
                        
                        // System Management APIs
                        if (docName == "sys")
                        {
                            return namespaceName.Contains("system") || 
                                   //namespaceName.Contains(".sys") ||
                                   //controllerName.Contains("sys") ||
                                   //controllerName.Contains("role") ||
                                   //controllerName.Contains("menu") ||
                                   //controllerName.Contains("dept") ||
                                   //controllerName.Contains("user") && !controllerName.Contains("useraddress") ||
                                   //controllerName.Contains("dict") ||
                                   //controllerName.Contains("config") ||
                                   //controllerName.Contains("notice") ||
                                   //controllerName.Contains("log") ||
                                   //controllerName.Contains("post") && !controllerName.Contains("blog") ||
                                   //controllerName.Contains("oper") ||
                                   //controllerName.Contains("login") ||
                                   //controllerName.Contains("region") ||
                                   //controllerName.Contains("file") ||
                                   //controllerName.Contains("task") ||
                                   //controllerName.Contains("monitor") ||
                                   //controllerName.Contains("gen") ||
                                   //controllerName.Contains("email") ||
                                   controllerName.Contains("sms");
                        }
                        // HR Management APIs
                        else if (docName == "hr")
                        {
                            return namespaceName.Contains(".hr") || 
                                   //namespaceName.Contains("human") ||
                                   //controllerName.Contains("hr") ||
                                   //controllerName.Contains("employee") ||
                                   //controllerName.Contains("attendance") ||
                                   //controllerName.Contains("leave") ||
                                   //controllerName.Contains("salary") ||
                                   //controllerName.Contains("payroll") ||
                                   //controllerName.Contains("document") && namespaceName.Contains(".hr") ||
                                   //controllerName.Contains("recruitment") ||
                                   //controllerName.Contains("training") ||
                                   //controllerName.Contains("performance") ||
                                   //controllerName.Contains("benefit") ||
                                   //controllerName.Contains("contract") ||
                                   //controllerName.Contains("department") && namespaceName.Contains(".hr") ||
                                   //controllerName.Contains("position") ||
                                   //controllerName.Contains("shift") ||
                                   //controllerName.Contains("holiday") ||
                                   controllerName.Contains("timesheet");
                        }
                        // Shopping Mall APIs
                        else if (docName == "shopping")
                        {
                            return namespaceName.Contains("mall") || 
                                   //namespaceName.Contains("shopping") ||
                                   //namespaceName.Contains(".shop") ||
                                   //controllerName.Contains("product") ||
                                   //controllerName.Contains("category") && !namespaceName.Contains("article") ||
                                   //controllerName.Contains("brand") ||
                                   //controllerName.Contains("order") ||
                                   //controllerName.Contains("cart") ||
                                   //controllerName.Contains("payment") ||
                                   //controllerName.Contains("sku") ||
                                   //controllerName.Contains("inventory") ||
                                   //controllerName.Contains("shipping") ||
                                   //controllerName.Contains("coupon") ||
                                   //controllerName.Contains("discount") ||
                                   //controllerName.Contains("wishlist") ||
                                   //controllerName.Contains("review") ||
                                   //controllerName.Contains("rating") ||
                                   //controllerName.Contains("merchant") ||
                                   //controllerName.Contains("store") ||
                                   controllerName.Contains("useraddress");
                        }
                        // Article/Content Management APIs
                        else if (docName == "article")
                        {
                            return namespaceName.Contains("content") || 
                                   //namespaceName.Contains("article") ||
                                   //namespaceName.Contains(".cms") ||
                                   //controllerName.Contains("article") ||
                                   //controllerName.Contains("content") ||
                                   //controllerName.Contains("blog") ||
                                   //controllerName.Contains("post") && !controllerName.Contains("syspost") ||
                                   //controllerName.Contains("news") ||
                                   //controllerName.Contains("page") ||
                                   //controllerName.Contains("comment") ||
                                   //controllerName.Contains("tag") ||
                                   //controllerName.Contains("category") && namespaceName.Contains("article") ||
                                   //controllerName.Contains("media") ||
                                   controllerName.Contains("publish");
                        }
                        // Default Business APIs
                        else if (docName == "v1")
                        {
                            // Check if it's not already assigned to other groups
                            var isSys = namespaceName.Contains("system") || namespaceName.Contains(".sys") ||
                                       //controllerName.Contains("sys") || controllerName.Contains("role") ||
                                       //controllerName.Contains("menu") || controllerName.Contains("dept") ||
                                       //(controllerName.Contains("user") && !controllerName.Contains("useraddress")) ||
                                       //controllerName.Contains("dict") || controllerName.Contains("config") ||
                                       controllerName.Contains("log") || controllerName.Contains("login");
                                       
                            var isHr = namespaceName.Contains(".hr") || namespaceName.Contains("human") ||
                                      controllerName.Contains("hr") || controllerName.Contains("employee") ||
                                      controllerName.Contains("leave") || controllerName.Contains("salary");
                                      
                            var isShopping = namespaceName.Contains("mall") || namespaceName.Contains("shopping") ||
                                            controllerName.Contains("product") || controllerName.Contains("order") ||
                                            controllerName.Contains("cart") || controllerName.Contains("sku");
                                            
                            var isArticle = namespaceName.Contains("content") || namespaceName.Contains("article") ||
                                           controllerName.Contains("article") || controllerName.Contains("blog");
                            
                            // Return true only if not assigned to other groups
                            return !isSys && !isHr && !isShopping && !isArticle;
                        }
                    }
                    
                    // Default to v1 for non-controller endpoints
                    return docName == "v1";
                });

                // Resolve conflicts
                c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
                
                // Custom schema IDs to avoid conflicts
                c.CustomSchemaIds(type => 
                {
                    var name = type.FullName?.Replace("+", ".").Split('.').Last() ?? type.Name;
                    // Make schema names unique by including namespace if there are conflicts
                    if (type.Namespace != null && (
                        type.Name.Contains("Model") || 
                        type.Name.Contains("Dto") || 
                        type.Name.Contains("Request") || 
                        type.Name.Contains("Response")))
                    {
                        var namespacePrefix = type.Namespace.Split('.').Last();
                        return $"{namespacePrefix}_{name}";
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
                
                // Enable annotations if available
                try 
                { 
                    c.EnableAnnotations(); 
                } 
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Could not enable annotations: {ex.Message}");
                }
            });

            return services;
        }

        /// <summary>
        /// Use Swagger middleware with all groups
        /// </summary>
        public static IApplicationBuilder UseFullSwagger(this IApplicationBuilder app)
        {
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
                // Add endpoints for all groups
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Business API");
                c.SwaggerEndpoint("/swagger/sys/swagger.json", "System Management");
                c.SwaggerEndpoint("/swagger/hr/swagger.json", "HR Management");
                c.SwaggerEndpoint("/swagger/shopping/swagger.json", "Shopping Mall");
                c.SwaggerEndpoint("/swagger/article/swagger.json", "Article Management");
                
                // UI Configuration
                c.RoutePrefix = string.Empty; // Serve at root
                c.DocumentTitle = "maham-ai.NET API Documentation";
                c.DocExpansion(DocExpansion.List);
                c.DefaultModelsExpandDepth(-1); // Hide schemas section by default
                c.DisplayRequestDuration();
                c.EnableFilter();
                c.EnableDeepLinking();
                c.EnableTryItOutByDefault();
                c.EnablePersistAuthorization();
                c.DisplayOperationId();
                c.ShowExtensions();
                
                // Add custom CSS if needed
                c.InjectStylesheet("/swagger-ui/custom.css");
                
                // Custom index page with branding
                c.HeadContent = @"
                    <style>
                        .swagger-ui .topbar { display: none; }
                        .swagger-ui .info .title { font-size: 2em; }
                        .swagger-ui .info { margin-bottom: 20px; }
                    </style>
                ";
            });

            return app;
        }
    }
}
