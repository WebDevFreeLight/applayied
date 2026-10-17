using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Reflection;
using System.Text.Json;

namespace ZR.Admin.WebApi.Extensions
{
    /// <summary>
    /// Dynamic Swagger configuration with comprehensive error handling
    /// </summary>
    public static class DynamicSwaggerExtension
    {
        private static readonly List<string> _swaggerErrors = new();
        private static readonly Dictionary<string, List<string>> _apiGroups = new();

        /// <summary>
        /// Add dynamic Swagger configuration with error handling
        /// </summary>
        public static IServiceCollection AddDynamicSwagger(this IServiceCollection services, IConfiguration configuration)
        {
            try
            {
                services.AddEndpointsApiExplorer();
                
                services.AddSwaggerGen(options =>
                {
                    // Configure Swagger generation with error handling
                    ConfigureSwaggerGen(options, configuration);
                });

                // Add API explorer for dynamic discovery
                services.AddMvcCore().AddApiExplorer();

                return services;
            }
            catch (Exception ex)
            {
                _swaggerErrors.Add($"Failed to configure Swagger services: {ex.Message}");
                Console.WriteLine($"[SWAGGER ERROR] {ex}");
                
                // Add minimal Swagger configuration as fallback
                services.AddSwaggerGen(c =>
                {
                    c.SwaggerDoc("v1", new OpenApiInfo 
                    { 
                        Title = "API Documentation", 
                        Version = "v1",
                        Description = "⚠️ Swagger configuration failed. Running in fallback mode."
                    });
                });
                
                return services;
            }
        }

        /// <summary>
        /// Configure Swagger generation options
        /// </summary>
        private static void ConfigureSwaggerGen(SwaggerGenOptions options, IConfiguration configuration)
        {
            // Discover and create API groups dynamically
            var apiGroups = DiscoverApiGroups();
            
            foreach (var group in apiGroups)
            {
                options.SwaggerDoc(group.Key, new OpenApiInfo
                {
                    Title = $"{configuration["Application:Name"] ?? "API"} - {group.Value}",
                    Version = configuration["Application:Version"] ?? "v1",
                    Description = $"{group.Value} API Documentation",
                    Contact = new OpenApiContact
                    {
                        Name = configuration["Application:Contact:Name"] ?? "API Support",
                        Email = configuration["Application:Contact:Email"],
                        Url = !string.IsNullOrEmpty(configuration["Application:Contact:Url"]) 
                            ? new Uri(configuration["Application:Contact:Url"]) 
                            : null
                    },
                    License = new OpenApiLicense
                    {
                        Name = configuration["Application:License:Name"] ?? "MIT",
                        Url = !string.IsNullOrEmpty(configuration["Application:License:Url"])
                            ? new Uri(configuration["Application:License:Url"])
                            : new Uri("https://opensource.org/licenses/MIT")
                    }
                });
            }

            // Add XML comments with error handling
            AddXmlComments(options);

            // Configure security
            ConfigureSecurity(options);

            // Add operation filters
            AddOperationFilters(options);

            // Configure schemas
            ConfigureSchemas(options);

            // CRITICAL: Add DocInclusionPredicate to filter APIs into groups
            options.DocInclusionPredicate((docName, apiDescription) =>
            {
                // The "all" group includes everything
                if (docName == "all")
                {
                    return true;
                }
                
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
                    var namespaceName = controllerActionDescriptor.ControllerTypeInfo.Namespace;
                    
                    if (namespaceName != null)
                    {
                        if (namespaceName.Contains("System"))
                            return docName == "sys";
                        if (namespaceName.Contains("HR"))
                            return docName == "hr";
                        if (namespaceName.Contains("Finance"))
                            return docName == "finance";
                        if (namespaceName.Contains("Sales"))
                            return docName == "sales";
                        if (namespaceName.Contains("Operations"))
                            return docName == "operations";
                        if (namespaceName.Contains("Marketing"))
                            return docName == "marketing";
                        if (namespaceName.Contains("Client") || namespaceName.Contains("Vendor"))
                            return docName == "clients";
                        if (namespaceName.Contains("Project"))
                            return docName == "projects";
                        if (namespaceName.Contains("Supply"))
                            return docName == "supplychain";
                        if (namespaceName.Contains("Legal"))
                            return docName == "legal";
                        if (namespaceName.Contains("Monitor"))
                            return docName == "monitor";
                        if (namespaceName.Contains("Tools"))
                            return docName == "tools";
                        if (namespaceName.Contains("Email"))
                            return docName == "email";
                        if (namespaceName.Contains("Public"))
                            return docName == "public";
                        if (namespaceName.Contains("Mall") || namespaceName.Contains("Shopping"))
                            return docName == "shopping";
                        if (namespaceName.Contains("Content") || namespaceName.Contains("Article"))
                            return docName == "article";
                        if (namespaceName.Contains("Business"))
                            return docName == "business";
                    }
                    
                    // Default group for unassigned controllers
                    return docName == "v1";
                }
                
                // For non-controller endpoints
                return docName == "v1";
            });

            // Handle conflicting actions
            options.ResolveConflictingActions(apiDescriptions =>
            {
                var descriptions = apiDescriptions.ToList();
                if (descriptions.Count > 1)
                {
                    _swaggerErrors.Add($"Conflicting actions: {string.Join(", ", descriptions.Select(d => d.RelativePath))}");
                }
                return descriptions.First();
            });

            // Custom operation IDs
            options.CustomOperationIds(apiDescription =>
            {
                if (apiDescription.ActionDescriptor is ControllerActionDescriptor actionDescriptor)
                {
                    return $"{actionDescriptor.ControllerName}_{actionDescriptor.ActionName}_{apiDescription.HttpMethod?.ToLower()}";
                }
                return apiDescription.RelativePath?.Replace("/", "_").Replace("{", "").Replace("}", "");
            });

            // Document filter for additional processing
            options.DocumentFilter<SwaggerDocumentFilter>();
        }

        /// <summary>
        /// Discover API groups dynamically from controllers
        /// </summary>
        private static Dictionary<string, string> DiscoverApiGroups()
        {
            var groups = new Dictionary<string, string>();
            
            try
            {
                // Get all controller types
                var controllerTypes = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(a => !a.IsDynamic && !a.FullName.StartsWith("System") && !a.FullName.StartsWith("Microsoft"))
                    .SelectMany(a =>
                    {
                        try { return a.GetTypes(); }
                        catch { return Array.Empty<Type>(); }
                    })
                    .Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(ControllerBase)))
                    .ToList();

                // Group controllers by namespace or attribute
                foreach (var controllerType in controllerTypes)
                {
                    var apiExplorerSettings = controllerType.GetCustomAttribute<ApiExplorerSettingsAttribute>();
                    var groupName = apiExplorerSettings?.GroupName;

                    if (string.IsNullOrEmpty(groupName))
                    {
                        // Determine group from namespace
                        if (controllerType.Namespace?.Contains("System") == true)
                            groupName = "sys";
                        else if (controllerType.Namespace?.Contains("HR") == true)
                            groupName = "hr";
                        else if (controllerType.Namespace?.Contains("Finance") == true)
                            groupName = "finance";
                        else if (controllerType.Namespace?.Contains("Sales") == true)
                            groupName = "sales";
                        else if (controllerType.Namespace?.Contains("Operations") == true)
                            groupName = "operations";
                        else if (controllerType.Namespace?.Contains("Marketing") == true)
                            groupName = "marketing";
                        else if (controllerType.Namespace?.Contains("Client") == true || controllerType.Namespace?.Contains("Vendor") == true)
                            groupName = "clients";
                        else if (controllerType.Namespace?.Contains("Project") == true)
                            groupName = "projects";
                        else if (controllerType.Namespace?.Contains("Supply") == true)
                            groupName = "supplychain";
                        else if (controllerType.Namespace?.Contains("Legal") == true)
                            groupName = "legal";
                        else if (controllerType.Namespace?.Contains("Monitor") == true)
                            groupName = "monitor";
                        else if (controllerType.Namespace?.Contains("Tools") == true)
                            groupName = "tools";
                        else if (controllerType.Namespace?.Contains("Email") == true)
                            groupName = "email";
                        else if (controllerType.Namespace?.Contains("Public") == true)
                            groupName = "public";
                        else if (controllerType.Namespace?.Contains("Mall") == true || controllerType.Namespace?.Contains("Shopping") == true)
                            groupName = "shopping";
                        else if (controllerType.Namespace?.Contains("Content") == true || controllerType.Namespace?.Contains("Article") == true)
                            groupName = "article";
                        else if (controllerType.Namespace?.Contains("Business") == true)
                            groupName = "business";
                        else
                            groupName = "v1";
                    }

                    if (!groups.ContainsKey(groupName))
                    {
                        groups[groupName] = FormatGroupName(groupName);
                    }

                    // Track controllers per group
                    if (!_apiGroups.ContainsKey(groupName))
                        _apiGroups[groupName] = new List<string>();
                    _apiGroups[groupName].Add(controllerType.Name);
                }

                // Ensure at least one group exists
                if (groups.Count == 0)
                {
                    groups["v1"] = "Default API";
                }
                
                // Always add an "all" group that shows all APIs
                groups["all"] = "All APIs";
            }
            catch (Exception ex)
            {
                _swaggerErrors.Add($"Failed to discover API groups: {ex.Message}");
                groups["v1"] = "API Documentation";
            }

            return groups;
        }

        /// <summary>
        /// Format group name for display
        /// </summary>
        private static string FormatGroupName(string groupName)
        {
            return groupName switch
            {
                "sys" => "System Management",
                "hr" => "Human Resources",
                "finance" => "Finance Department",
                "sales" => "Sales Department",
                "operations" => "Operations Department",
                "marketing" => "Marketing Department",
                "clients" => "Clients & Vendors",
                "projects" => "Projects Management",
                "supplychain" => "Supply Chain Management",
                "legal" => "Legal Department",
                "monitor" => "System Monitor",
                "tools" => "Tools & Utilities",
                "email" => "Email Service",
                "public" => "Public APIs",
                "shopping" => "Shopping Mall",
                "article" => "Content Management",
                "business" => "Business Logic",
                "v1" => "Business API",
                _ => groupName.ToUpper()
            };
        }

        /// <summary>
        /// Add XML comments with error handling
        /// </summary>
        private static void AddXmlComments(SwaggerGenOptions options)
        {
            var xmlFiles = new[]
            {
                "ZR.Model.xml",
                "ZR.ServiceCore.xml",
                "ZR.Service.xml",
                "ZR.Admin.WebApi.xml",
                "ZR.Mall.xml",
                "ZR.CodeGenerator.xml",
                "ZR.Tasks.xml",
                "ZR.Common.xml",
                "Infrastructure.xml",
                "ZR.Repository.xml"
            };

            var baseDir = AppContext.BaseDirectory;
            
            foreach (var xmlFile in xmlFiles)
            {
                var xmlPath = Path.Combine(baseDir, xmlFile);
                if (File.Exists(xmlPath))
                {
                    try
                    {
                        options.IncludeXmlComments(xmlPath, true);
                    }
                    catch (Exception ex)
                    {
                        _swaggerErrors.Add($"Failed to load XML comments from {xmlFile}: {ex.Message}");
                    }
                }
            }

            // Try to include current assembly XML
            try
            {
                var currentAssembly = Assembly.GetExecutingAssembly();
                var xmlFile = $"{currentAssembly.GetName().Name}.xml";
                var xmlPath = Path.Combine(baseDir, xmlFile);
                if (File.Exists(xmlPath))
                {
                    options.IncludeXmlComments(xmlPath);
                }
            }
            catch { }
        }

        /// <summary>
        /// Configure security definitions with HTTPS support
        /// </summary>
        private static void ConfigureSecurity(SwaggerGenOptions options)
        {
            try
            {
                // JWT Bearer authentication
                options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = JwtBearerDefaults.AuthenticationScheme,
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "JWT Authorization header using the Bearer scheme. Example: 'Bearer {token}'"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = JwtBearerDefaults.AuthenticationScheme
                            }
                        },
                        Array.Empty<string>()
                    }
                });

                // Add server URLs for both HTTP and HTTPS
                var httpPort = Environment.GetEnvironmentVariable("HTTP_PORT") ?? "8080";
                var httpsPort = Environment.GetEnvironmentVariable("HTTPS_PORT") ?? "8443";
                var serverHost = Environment.GetEnvironmentVariable("SERVER_HOST") ?? "localhost";

                // Add HTTP server
                options.AddServer(new OpenApiServer
                {
                    Url = $"http://{serverHost}:{httpPort}",
                    Description = "HTTP Server"
                });

                // Add HTTPS server
                options.AddServer(new OpenApiServer
                {
                    Url = $"https://{serverHost}:{httpsPort}",
                    Description = "HTTPS Server (Secure)"
                });

                // Add localhost for development
                if (serverHost != "localhost")
                {
                    options.AddServer(new OpenApiServer
                    {
                        Url = $"http://localhost:{httpPort}",
                        Description = "Local HTTP Server"
                    });

                    options.AddServer(new OpenApiServer
                    {
                        Url = $"https://localhost:{httpsPort}",
                        Description = "Local HTTPS Server"
                    });
                }
            }
            catch (Exception ex)
            {
                _swaggerErrors.Add($"Failed to configure security: {ex.Message}");
            }
        }

        /// <summary>
        /// Add operation filters
        /// </summary>
        private static void AddOperationFilters(SwaggerGenOptions options)
        {
            try
            {
                //options.OperationFilter<AddResponseHeadersFilter>();
                //options.OperationFilter<AppendAuthorizeToSummaryOperationFilter>();
                //options.OperationFilter<SecurityRequirementsOperationFilter>();
                //options.OperationFilter<SwaggerDefaultValuesFilter>();
                //options.OperationFilter<SwaggerResponseExampleFilter>();
            }
            catch (Exception ex)
            {
                _swaggerErrors.Add($"Failed to add operation filters: {ex.Message}");
            }
        }

        /// <summary>
        /// Configure schema generation
        /// </summary>
        private static void ConfigureSchemas(SwaggerGenOptions options)
        {
            options.CustomSchemaIds(type =>
            {
                var name = type.FullName?.Replace("+", ".") ?? type.Name;
                return name.Split('.').Last();
            });

            options.SchemaFilter<EnumSchemaFilter>();
            
            // Enable annotations if available
            try
            {
                options.EnableAnnotations();
            }
            catch (Exception ex) {

                Console.WriteLine(ex.Message);



            }
        }

        /// <summary>
        /// Use dynamic Swagger middleware
        /// </summary>
        public static IApplicationBuilder UseDynamicSwagger(this IApplicationBuilder app, IConfiguration configuration)
        {
            try
            {
                app.UseSwagger(options =>
                {
                    options.RouteTemplate = "swagger/{documentName}/swagger.json";
                    options.PreSerializeFilters.Add((swaggerDoc, httpReq) =>
                    {
                        // Dynamically set servers based on current request and configuration
                        var servers = new List<OpenApiServer>
                        {
                            new OpenApiServer 
                            { 
                                Url = $"{httpReq.Scheme}://{httpReq.Host.Value}",
                                Description = "Current Server"
                            }
                        };

                        // Add both HTTP and HTTPS if configured
                        var useHttps = configuration.GetValue<bool>("UseHttps", true);
                        var httpPort = Environment.GetEnvironmentVariable("HTTP_PORT") ?? "8080";
                        var httpsPort = Environment.GetEnvironmentVariable("HTTPS_PORT") ?? "8443";
                        
                        // Add HTTP server
                        if (!servers.Any(s => s.Url.StartsWith("http://") && !s.Url.StartsWith("https://")))
                        {
                            servers.Add(new OpenApiServer
                            {
                                Url = $"http://{httpReq.Host.Host}:{httpPort}",
                                Description = "HTTP Server"
                            });
                        }

                        // Add HTTPS server if enabled
                        if (useHttps && !servers.Any(s => s.Url.StartsWith("https://")))
                        {
                            servers.Add(new OpenApiServer
                            {
                                Url = $"https://{httpReq.Host.Host}:{httpsPort}",
                                Description = "HTTPS Server (Secure)"
                            });
                        }

                        swaggerDoc.Servers = servers;

                        // Add errors as extension if any exist
                        if (_swaggerErrors.Any())
                        {
                            swaggerDoc.Extensions["x-swagger-errors"] = new Microsoft.OpenApi.Any.OpenApiString(
                                JsonSerializer.Serialize(_swaggerErrors));
                        }
                    });
                });

                app.UseSwaggerUI(options =>
                {
                    ConfigureSwaggerUI(options, configuration);
                });

                // Add health check endpoint for Swagger
                app.Map("/swagger/health", appBuilder =>
                {
                    appBuilder.Run(async context => await HandleSwaggerHealth(context));
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SWAGGER ERROR] Failed to configure Swagger middleware: {ex}");
                
                // Fallback UI configuration
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API Documentation (Fallback)");
                });
            }

            return app;
        }

        /// <summary>
        /// Configure Swagger UI options with HTTPS support
        /// </summary>
        private static void ConfigureSwaggerUI(SwaggerUIOptions options, IConfiguration configuration)
        {
            // Dynamically add endpoints for discovered groups
            var groups = DiscoverApiGroups();
            foreach (var group in groups)
            {
                options.SwaggerEndpoint($"/swagger/{group.Key}/swagger.json", group.Value);
            }

            // UI Configuration
            options.RoutePrefix = configuration["Swagger:RoutePrefix"] ?? string.Empty;
            options.DocumentTitle = configuration["Swagger:DocumentTitle"] ?? "API Documentation";
            options.DocExpansion(DocExpansion.None);
            
            // Enable HTTPS support in UI
            options.SupportedSubmitMethods(SubmitMethod.Get, SubmitMethod.Post, SubmitMethod.Put, SubmitMethod.Delete, SubmitMethod.Patch);
            
            // Configure OAuth2 if needed for HTTPS
            var useHttps = configuration.GetValue<bool>("UseHttps", true);
            if (useHttps)
            {
                options.UseRequestInterceptor(@"
                    (request) => {
                        // Force HTTPS for API calls if available
                        if (window.location.protocol === 'https:' && request.url.startsWith('http://')) {
                            request.url = request.url.replace('http://', 'https://');
                            // Update port if needed
                            request.url = request.url.replace(':8080', ':8443');
                        }
                        return request;
                    }
                ");
            }


            

            options.DefaultModelsExpandDepth(-1);
            options.DisplayRequestDuration();
            options.EnableFilter();
            options.EnableDeepLinking();
            options.EnableValidator();
            options.ShowExtensions();
            options.ShowCommonExtensions();
            options.EnableTryItOutByDefault();
            options.DisplayOperationId();
            options.EnablePersistAuthorization();

            // Add custom styling
            options.InjectStylesheet("/swagger-ui/custom.css");
            options.InjectJavascript("/swagger-ui/custom.js");

            // Add error display if errors exist
            if (_swaggerErrors.Any())
            {
                var errorHtml = string.Join("<br/>", _swaggerErrors.Select(e => $"⚠️ {e}"));
                options.HeadContent = $@"
                    <style>
                        .swagger-ui .info {{ position: relative; }}
                        .swagger-errors {{ 
                            background: #fff3cd; 
                            border: 1px solid #ffc107; 
                            padding: 10px; 
                            margin: 10px 0; 
                            border-radius: 5px;
                        }}
                    </style>
                    <script>
                        window.addEventListener('load', function() {{
                            var info = document.querySelector('.swagger-ui .info');
                            if (info) {{
                                var errorDiv = document.createElement('div');
                                errorDiv.className = 'swagger-errors';
                                errorDiv.innerHTML = '<strong>Configuration Warnings:</strong><br/>{errorHtml}';
                                info.appendChild(errorDiv);
                            }}
                        }});
                    </script>
                ";
            }
        }

        /// <summary>
        /// Handle Swagger health check endpoint
        /// </summary>
        private static async Task HandleSwaggerHealth(HttpContext context)
        {
            var health = new
            {
                status = _swaggerErrors.Any() ? "degraded" : "healthy",
                errors = _swaggerErrors,
                groups = _apiGroups.ToDictionary(g => g.Key, g => g.Value.Count),
                timestamp = DateTime.UtcNow
            };

            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(health, new JsonSerializerOptions
            {
                WriteIndented = true
            }));
        }
    }

    /// <summary>
    /// Swagger document filter for additional processing
    /// </summary>
    public class SwaggerDocumentFilter : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            // Add metadata
            swaggerDoc.Extensions["x-generated-at"] = new Microsoft.OpenApi.Any.OpenApiString(DateTime.UtcNow.ToString("O"));
            swaggerDoc.Extensions["x-api-count"] = new Microsoft.OpenApi.Any.OpenApiInteger(swaggerDoc.Paths.Count);
            
            // Sort paths alphabetically
            var sortedPaths = swaggerDoc.Paths.OrderBy(p => p.Key).ToList();
            swaggerDoc.Paths.Clear();
            foreach (var path in sortedPaths)
            {
                swaggerDoc.Paths.Add(path.Key, path.Value);
            }
        }
    }

    /// <summary>
    /// Schema filter for enums
    /// </summary>
    public class EnumSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (context.Type.IsEnum)
            {
                schema.Enum.Clear();
                var enumValues = Enum.GetValues(context.Type);
                
                foreach (var enumValue in enumValues)
                {
                    schema.Enum.Add(new Microsoft.OpenApi.Any.OpenApiString(enumValue.ToString()));
                }

                schema.Type = "string";
                schema.Format = null;
            }
        }
    }

    /// <summary>
    /// Filter to add default values
    /// </summary>
    public class SwaggerDefaultValuesFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            foreach (var parameter in operation.Parameters)
            {
                var description = context.ApiDescription.ParameterDescriptions
                    .FirstOrDefault(p => p.Name == parameter.Name);

                if (description?.DefaultValue != null)
                {
                    parameter.Schema.Default = new Microsoft.OpenApi.Any.OpenApiString(description.DefaultValue.ToString());
                }

                parameter.Required = description?.IsRequired ?? false;
            }
        }
    }

    /// <summary>
    /// Filter to add response examples
    /// </summary>
    public class SwaggerResponseExampleFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // Add common response codes if not present
            if (!operation.Responses.ContainsKey("400"))
            {
                operation.Responses.Add("400", new OpenApiResponse
                {
                    Description = "Bad Request - Invalid parameters"
                });
            }

            if (!operation.Responses.ContainsKey("401"))
            {
                operation.Responses.Add("401", new OpenApiResponse
                {
                    Description = "Unauthorized - Authentication required"
                });
            }

            if (!operation.Responses.ContainsKey("500"))
            {
                operation.Responses.Add("500", new OpenApiResponse
                {
                    Description = "Internal Server Error"
                });
            }
        }
    }
}
