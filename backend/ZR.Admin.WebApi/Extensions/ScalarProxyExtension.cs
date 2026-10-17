using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Scalar.AspNetCore;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace ZR.Admin.WebApi.Extensions
{
    /// <summary>
    /// Scalar extension that directly uses Swagger's configuration
    /// </summary>
    public static class ScalarProxyExtension
    {



        public static void UseStaticFilesOn(this WebApplication app, IConfiguration configuration)
        {





            //if (app.Environment.IsDevelopment())
            //{

            //    staticFilesPath = Path.Combine(app.Environment.WebRootPath, "forlocalhost", "assets");

            //}
            try
            {


                app.UseStaticFiles(new StaticFileOptions
                {
                    ServeUnknownFileTypes = true,


                    FileProvider = new PhysicalFileProvider(Path.Combine(app.Environment.WebRootPath)),
                    OnPrepareResponse = context =>
                    {
                        context.Context.Response.Headers.Add("Cache-Control", "no-store");
                    }
                });

                // Serve uploaded files from secure-uploads directory
                var uploadsPath = Path.Combine(app.Environment.ContentRootPath, "secure-uploads");
                if (!Directory.Exists(uploadsPath))
                {
                    Directory.CreateDirectory(uploadsPath);
                }
                app.UseStaticFiles(new StaticFileOptions
                {
                    FileProvider = new PhysicalFileProvider(uploadsPath),
                    OnPrepareResponse = context =>
                    {
                        context.Context.Response.Headers["Cache-Control"] = "public,max-age=86400";
                    }
                });




                var staticFilesPath = Path.Combine(app.Environment.WebRootPath, "forproductionhost", "assets");


                if (app.Environment.IsStaging())
                {
                    staticFilesPath = Path.Combine(app.Environment.WebRootPath, "forstagehost", "assets");
                }


                if (app.Environment.IsProduction())
                {

                    staticFilesPath = Path.Combine(app.Environment.WebRootPath, "forproductionhost", "assets");

                }

                if (!Directory.Exists(staticFilesPath))
                {
                    Console.WriteLine($"Directory not found: {staticFilesPath}");
                    // Optionally create the directory or log the error
                    Directory.CreateDirectory(staticFilesPath); // Be cautious with this in production
                }
                var fileProvider = new PhysicalFileProvider(staticFilesPath);
                var StaticFileOptionsda = new StaticFileOptions
                {
                    // Set the FileProvider to a custom directory if needed (optional)
                    FileProvider = fileProvider, // Serve from "wwwroot" folder
                    ServeUnknownFileTypes = true,

                    // Specify a custom request path if required
                    RequestPath = "/assets", // Access files via "/static" path
                };
                app.UseStaticFiles(StaticFileOptionsda);
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.ToString());
                Console.WriteLine(ex.InnerException.Message);

            }







        }


        private static readonly HttpClient httpClient = new HttpClient();

        /// <summary>
        /// Configure Scalar to use existing Swagger endpoints with advanced error handling
        /// </summary>
        public static void UseScalarWithSwagger(this WebApplication app, IConfiguration configuration)
        {
            try
            {
                var apiDocConfig = configuration.GetSection("ApiDocumentation");
                var enableScalar = apiDocConfig.GetValue<bool>("EnableScalar", true);
                
                if (!enableScalar)
                {
                    Console.WriteLine("[SCALAR] Scalar documentation is disabled in configuration.");
                    return;
                }

                var swaggerPrefix = apiDocConfig.GetValue<string>("SwaggerRoutePrefix", "swagger");
                var scalarPrefix = apiDocConfig.GetValue<string>("ScalarRoutePrefix", "scalar");
                var documentTitle = apiDocConfig.GetValue<string>("DocumentTitle", "API Documentation");
                
                Console.WriteLine($"[SCALAR] Initializing Scalar documentation at /{scalarPrefix}");

            // Define all API groups including all departments
            var apiGroups = new[] 
            { 
                "all",          // All APIs combined
                "v1",           // Default/Business API
                "sys",          // System Management
                "hr",           // Human Resources
                "shopping",     // Shopping Mall
                "article",      // Content Management
                "finance",      // Finance Department
                "sales",        // Sales Department
                "operations",   // Operations Department
                "marketing",    // Marketing Department
                "clients",      // Clients & Vendors
                "projects",     // Projects Management
                "supplychain",  // Supply Chain Management
                "legal",        // Legal Department
                "monitor",      // System Monitor
                "tools",        // Tools & Utilities
                "email",        // Email Service
                "public",       // Public APIs
                "business"      // Business Logic
            };

            // Map Scalar with the required {documentName} placeholder
            app.MapScalarApiReference(options =>
            {
                options
                    .WithTitle(documentTitle)
                    .WithTheme(ScalarTheme.Purple)
                     .WithDarkMode(true)
                            .WithDefaultOpenAllTags(false)
                    // REQUIRED: Must use {documentName} placeholder
                    .WithEndpointPrefix($"/{scalarPrefix}/{{documentName}}")
                    // Point to Swagger's JSON endpoints
                    .WithOpenApiRoutePattern($"/{swaggerPrefix}/{{documentName}}/swagger.json")
                    .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient)
                    .WithPreferredScheme("Bearer")
                    .WithSidebar(true)
                    .WithSearchHotKey("k")
                    .WithModels(true)
                    .WithDownloadButton(true)
                    .WithTestRequestButton(true);
            });

            // Map individual Scalar endpoints for each group with error handling
            try
            {
                foreach (var group in apiGroups)
                {
                    var groupTitle = GetGroupTitle(group);
                    var groupDescription = GetGroupDescription(group);
                    var endpoint = $"/{scalarPrefix}/{group}";
                    
                    app.MapScalarApiReference(options =>
                    {
                        options
                            .WithTitle($"{documentTitle} - {groupTitle}")
                            .WithTheme(ScalarTheme.Purple)
                            .WithDarkMode(true)
                            .WithDefaultOpenAllTags(false)
                            .WithEndpointPrefix(endpoint)
                            .WithOpenApiRoutePattern($"/{swaggerPrefix}/{group}/swagger.json")
                            .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient)
                            .WithPreferredScheme("Bearer")
                            .WithSidebar(true)
                            .WithSearchHotKey("k")
                            .WithModels(true)
                            .WithDownloadButton(true)
                            .WithTestRequestButton(true)
                            .WithCustomCss(@"
                                .scalar-app { font-family: 'Inter', system-ui, sans-serif; }
                                .scalar-sidebar { background-color: #1a1a2e; }
                                .scalar-card { border-radius: 12px; }
                            ");
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SCALAR ERROR] Failed to map individual endpoints: {ex.Message}");
            }

            // Add comprehensive route handlers for various URL patterns
            MapRouteHandlers(app, swaggerPrefix, scalarPrefix, apiGroups);
            
            // Log available Scalar URLs
            LogScalarUrls(configuration, apiGroups);
            
            Console.WriteLine("[SCALAR] Scalar documentation setup completed successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SCALAR ERROR] Failed to configure Scalar: {ex.Message}");
                Console.WriteLine($"[SCALAR ERROR] Stack trace: {ex.StackTrace}");
                
                // Add minimal fallback routes
                AddFallbackRoutes(app, configuration);
            }
        }
        
        /// <summary>
        /// Map comprehensive route handlers for Scalar and Swagger integration
        /// </summary>
        private static void MapRouteHandlers(WebApplication app, string swaggerPrefix, string scalarPrefix, string[] apiGroups)
        {
            // Handle root index.html - show a landing page or redirect
            app.MapGet("/index.html", async context =>
            {
                var html = GenerateMainLandingPage(scalarPrefix, swaggerPrefix);
                context.Response.ContentType = "text/html; charset=utf-8";
                await context.Response.WriteAsync(html);
            }).AllowAnonymous();
            
            // Root scalar redirect - redirect to all APIs by default
            //app.MapGet($"/{scalarPrefix}", context =>
            //{
            //    context.Response.Redirect($"/{scalarPrefix}/all");
            //    return Task.CompletedTask;
            //}).AllowAnonymous();
            
            // Handle /scalar/index.html - redirect to all APIs
            //app.MapGet($"/{scalarPrefix}/index.html", context =>
            //{
            //    context.Response.Redirect($"/{scalarPrefix}/all");
            //    return Task.CompletedTask;
            //}).AllowAnonymous();
            
            // Fix various incorrect swagger.json requests
            app.MapGet($"/{swaggerPrefix}/swagger.json", context =>
            {
                // Default to all APIs swagger.json
                context.Response.Redirect($"/{swaggerPrefix}/all/swagger.json");
                return Task.CompletedTask;
            }).AllowAnonymous();
            
        
            
            // Handle /scalar/swagger.json
            app.MapGet($"/{scalarPrefix}/swagger.json", context =>
            {
                context.Response.Redirect($"/{swaggerPrefix}/all/swagger.json");
                return Task.CompletedTask;
            }).AllowAnonymous();
      
            
            // Create a groups selection page
            app.MapGet($"/{scalarPrefix}/groups", async context =>
            {
                var html = GenerateGroupsPage(scalarPrefix, swaggerPrefix, apiGroups);
                context.Response.ContentType = "text/html; charset=utf-8";
                await context.Response.WriteAsync(html);
            }).AllowAnonymous();
            
            // Add catch-all handler for any /scalar/*/index.html patterns
            app.MapGet($"/{scalarPrefix}/{{group}}/index.html", context =>
            {
                var group = context.Request.RouteValues["group"]?.ToString();
                if (!string.IsNullOrEmpty(group) && apiGroups.Contains(group))
                {
                    context.Response.Redirect($"/{scalarPrefix}/{group}");
                }
                else
                {
                    context.Response.Redirect($"/{scalarPrefix}/all");
                }
                return Task.CompletedTask;
            }).AllowAnonymous();
            
            // Handle requests for swagger.json from specific groups
            app.MapGet($"/{scalarPrefix}/{{documentName}}/swagger.json", async context =>
            {
                var documentName = context.Request.RouteValues["documentName"]?.ToString();
                if (!string.IsNullOrEmpty(documentName) && !apiGroups.Contains(documentName))
                {
                    context.Response.StatusCode = 404;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(new
                    {
                        error = "Document not found",
                        message = $"API group '{documentName}' does not exist",
                        availableGroups = apiGroups,
                        suggestion = $"Try /{swaggerPrefix}/all/swagger.json instead"
                    }));
                }
                else
                {
                    context.Response.Redirect($"/{swaggerPrefix}/{documentName}/swagger.json");
                }
            }).AllowAnonymous();
            
            // Add a default handler for any unmatched swagger.json patterns
            app.MapGet("**/swagger.json", context =>
            {
                // Redirect any other swagger.json requests to the all APIs endpoint
                context.Response.Redirect($"/{swaggerPrefix}/all/swagger.json");
                return Task.CompletedTask;
            }).AllowAnonymous();
        }
        
        /// <summary>
        /// Add minimal fallback routes in case of configuration errors
        /// </summary>
        private static void AddFallbackRoutes(WebApplication app, IConfiguration configuration)
        {
            var swaggerPrefix = configuration.GetSection("ApiDocumentation").GetValue<string>("SwaggerRoutePrefix", "swagger");
            var scalarPrefix = configuration.GetSection("ApiDocumentation").GetValue<string>("ScalarRoutePrefix", "scalar");
            
            app.MapGet($"/{scalarPrefix}", async context =>
            {
                context.Response.ContentType = "text/html; charset=utf-8";
                await context.Response.WriteAsync($@"
                    <!DOCTYPE html>
                    <html>
                    <head>
                        <title>API Documentation - Fallback Mode</title>
                        <style>
                            body {{ 
                                font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
                                padding: 2rem;
                                background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
                                min-height: 100vh;
                                margin: 0;
                            }}
                            .container {{
                                max-width: 800px;
                                margin: 0 auto;
                                background: white;
                                padding: 2rem;
                                border-radius: 12px;
                                box-shadow: 0 20px 40px rgba(0,0,0,0.15);
                            }}
                            h1 {{ color: #1a202c; margin-bottom: 1rem; }}
                            .warning {{ 
                                background: #fff3cd; 
                                border: 1px solid #ffc107; 
                                color: #856404;
                                padding: 1rem; 
                                margin: 1rem 0; 
                                border-radius: 8px;
                            }}
                            .info {{ 
                                background: #f7fafc; 
                                padding: 1.5rem; 
                                margin: 1rem 0; 
                                border-radius: 8px;
                                border: 1px solid #e2e8f0;
                            }}
                            .links {{
                                display: flex;
                                gap: 1rem;
                                margin-top: 1rem;
                            }}
                            .link {{
                                padding: 0.75rem 1.5rem;
                                background: #667eea;
                                color: white;
                                text-decoration: none;
                                border-radius: 6px;
                                transition: background 0.2s;
                            }}
                            .link:hover {{
                                background: #5a67d8;
                            }}
                            .link.secondary {{
                                background: #48bb78;
                            }}
                            .link.secondary:hover {{
                                background: #38a169;
                            }}
                        </style>
                    </head>
                    <body>
                        <div class='container'>
                            <h1>📚 API Documentation</h1>
                            <div class='warning'>
                                <strong>⚠️ Running in Fallback Mode</strong><br>
                                Scalar documentation encountered an initialization error. Using simplified navigation.
                            </div>
                            <div class='info'>
                                <h3>Available Documentation:</h3>
                                <div class='links'>
                                    <a href='/{swaggerPrefix}' class='link'>Swagger UI</a>
                                    <a href='/{swaggerPrefix}/all/swagger.json' class='link secondary'>All APIs (JSON)</a>
                                    <a href='/{swaggerPrefix}/v1/swagger.json' class='link secondary'>Default APIs (JSON)</a>
                                </div>
                                <h3 style='margin-top: 1.5rem;'>Direct Group Access:</h3>
                                <div class='links' style='flex-wrap: wrap;'>
                                    <a href='/{scalarPrefix}/all' class='link'>All APIs</a>
                                    <a href='/{scalarPrefix}/v1' class='link'>Business API</a>
                                    <a href='/{scalarPrefix}/sys' class='link'>System</a>
                                    <a href='/{scalarPrefix}/hr' class='link'>HR</a>
                                    <a href='/{scalarPrefix}/groups' class='link secondary'>View All Groups</a>
                                </div>
                            </div>
                        </div>
                    </body>
                    </html>");
            }).AllowAnonymous();
        }

        private static string GenerateMainLandingPage(string scalarPrefix, string swaggerPrefix)
        {
            return $@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>API Documentation Portal</title>
    <style>
        * {{ margin: 0; padding: 0; box-sizing: border-box; }}
        body {{
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            min-height: 100vh;
            display: flex;
            align-items: center;
            justify-content: center;
            padding: 2rem;
        }}
        .container {{
            max-width: 900px;
            width: 100%;
            background: white;
            border-radius: 20px;
            padding: 3rem;
            box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.25);
            animation: fadeIn 0.5s ease-out;
        }}
        @keyframes fadeIn {{
            from {{ opacity: 0; transform: translateY(20px); }}
            to {{ opacity: 1; transform: translateY(0); }}
        }}
        h1 {{
            color: #1a202c;
            font-size: 2.5rem;
            margin-bottom: 0.5rem;
            text-align: center;
        }}
        .subtitle {{
            color: #718096;
            font-size: 1.125rem;
            text-align: center;
            margin-bottom: 3rem;
        }}
        .options {{
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
            gap: 1.5rem;
            margin-bottom: 2rem;
        }}
        .option-card {{
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            border-radius: 12px;
            padding: 2rem;
            text-decoration: none;
            color: white;
            transition: all 0.3s ease;
            text-align: center;
            position: relative;
            overflow: hidden;
        }}
        .option-card::before {{
            content: '';
            position: absolute;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            background: rgba(255, 255, 255, 0.1);
            transform: translateY(-100%);
            transition: transform 0.3s ease;
        }}
        .option-card:hover {{
            transform: translateY(-5px);
            box-shadow: 0 15px 30px rgba(0, 0, 0, 0.2);
        }}
        .option-card:hover::before {{
            transform: translateY(0);
        }}
        .option-icon {{
            font-size: 3rem;
            margin-bottom: 1rem;
        }}
        .option-title {{
            font-size: 1.5rem;
            font-weight: 600;
            margin-bottom: 0.5rem;
        }}
        .option-desc {{
            font-size: 0.875rem;
            opacity: 0.95;
        }}
        .secondary-options {{
            background: #f7fafc;
            border-radius: 12px;
            padding: 1.5rem;
            margin-top: 2rem;
        }}
        .secondary-title {{
            color: #2d3748;
            font-size: 1.125rem;
            font-weight: 600;
            margin-bottom: 1rem;
        }}
        .link-grid {{
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
            gap: 1rem;
        }}
        .link-item {{
            background: white;
            border: 2px solid #e2e8f0;
            border-radius: 8px;
            padding: 1rem;
            text-decoration: none;
            color: #2d3748;
            transition: all 0.2s;
            text-align: center;
        }}
        .link-item:hover {{
            border-color: #667eea;
            background: #f7f3ff;
            transform: translateY(-2px);
        }}
        .footer {{
            text-align: center;
            margin-top: 2rem;
            padding-top: 2rem;
            border-top: 1px solid #e2e8f0;
            color: #718096;
            font-size: 0.875rem;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <h1>🚀 API Documentation Portal</h1>
        <p class='subtitle'>Choose your preferred documentation interface</p>
        
        <div class='options'>
            <a href='/{scalarPrefix}/all' class='option-card'>
                <div class='option-icon'>📘</div>
                <div class='option-title'>Scalar Docs</div>
                <div class='option-desc'>Modern, interactive API documentation with dark mode support</div>
            </a>
            
            <a href='/{swaggerPrefix}' class='option-card' style='background: linear-gradient(135deg, #48bb78 0%, #38a169 100%);'>
                <div class='option-icon'>📚</div>
                <div class='option-title'>Swagger UI</div>
                <div class='option-desc'>Classic OpenAPI documentation with try-it-out functionality</div>
            </a>
        </div>
        
        <div class='secondary-options'>
            <div class='secondary-title'>Quick Access to API Groups</div>
            <div class='link-grid'>
                <a href='/{scalarPrefix}/all' class='link-item'>
                    <strong>📊 All APIs</strong><br>
                    <small>Complete documentation</small>
                </a>
                <a href='/{scalarPrefix}/sys' class='link-item'>
                    <strong>⚙️ System</strong><br>
                    <small>Core system APIs</small>
                </a>
                <a href='/{scalarPrefix}/hr' class='link-item'>
                    <strong>👥 HR</strong><br>
                    <small>Human Resources</small>
                </a>
                <a href='/{scalarPrefix}/groups' class='link-item'>
                    <strong>📋 All Groups</strong><br>
                    <small>View all categories</small>
                </a>
            </div>
        </div>
        
        <div class='secondary-options'>
            <div class='secondary-title'>Direct JSON Access</div>
            <div class='link-grid'>
                <a href='/{swaggerPrefix}/all/swagger.json' class='link-item'>
                    <strong>📄 All APIs JSON</strong><br>
                    <small>OpenAPI specification</small>
                </a>
                <a href='/{swaggerPrefix}/v1/swagger.json' class='link-item'>
                    <strong>📄 Default JSON</strong><br>
                    <small>Business APIs spec</small>
                </a>
            </div>
        </div>
        
        <div class='footer'>
            <p>Powered by Scalar & Swagger | OpenAPI 3.0</p>
        </div>
    </div>
</body>
</html>";
        }

        private static string GenerateGroupsPage(string scalarPrefix, string swaggerPrefix, string[] groups)
        {
            return $@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>API Documentation</title>
    <style>
        * {{ margin: 0; padding: 0; box-sizing: border-box; }}
        body {{
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            min-height: 100vh;
            padding: 2rem;
        }}
        .container {{
            max-width: 1200px;
            margin: 0 auto;
            background: white;
            border-radius: 16px;
            padding: 2.5rem;
            box-shadow: 0 20px 40px rgba(0,0,0,0.15);
        }}
        h1 {{
            text-align: center;
            color: #1a202c;
            margin-bottom: 0.5rem;
        }}
        .subtitle {{
            text-align: center;
            color: #718096;
            margin-bottom: 2rem;
        }}
        .groups {{
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
            gap: 1.5rem;
        }}
        .group-card {{
            background: #f7fafc;
            border: 2px solid #e2e8f0;
            border-radius: 12px;
            padding: 1.5rem;
            text-decoration: none;
            transition: all 0.3s ease;
            text-align: center;
        }}
        .group-card:hover {{
            transform: translateY(-4px);
            box-shadow: 0 10px 20px rgba(0,0,0,0.1);
            border-color: #667eea;
            background: white;
        }}
        .group-title {{
            color: #2d3748;
            font-size: 1.25rem;
            font-weight: 600;
            margin-bottom: 0.5rem;
        }}
        .group-desc {{
            color: #718096;
            font-size: 0.875rem;
            margin-bottom: 1rem;
        }}
        .links {{
            display: flex;
            justify-content: center;
            gap: 1rem;
        }}
        .link {{
            padding: 0.5rem 1rem;
            border-radius: 6px;
            text-decoration: none;
            font-size: 0.875rem;
            transition: all 0.2s;
        }}
        .scalar-link {{
            background: #667eea;
            color: white;
        }}
        .scalar-link:hover {{
            background: #5a67d8;
        }}
        .swagger-link {{
            background: #48bb78;
            color: white;
        }}
        .swagger-link:hover {{
            background: #38a169;
        }}
        .json-link {{
            background: #ed8936;
            color: white;
        }}
        .json-link:hover {{
            background: #dd6b20;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <h1>API Documentation</h1>
        <p class='subtitle'>Choose an API group to explore</p>
        
        <div class='groups'>
            {string.Join("\n", groups.Select(g => $@"
            <div class='group-card'>
                <div class='group-title'>{GetGroupTitle(g)}</div>
                <div class='group-desc'>{GetGroupDescription(g)}</div>
                <div class='links'>
                    <a href='/{scalarPrefix}/{g}' class='link scalar-link'>Scalar</a>
                    <a href='/{swaggerPrefix}/index.html?urls.primaryName={GetGroupTitle(g)}' class='link swagger-link'>Swagger</a>
                    <a href='/{swaggerPrefix}/{g}/swagger.json' class='link json-link'>JSON</a>
                </div>
            </div>"))}
        </div>
    </div>
</body>
</html>";
        }

        private static string GetGroupTitle(string group)
        {
            return group switch
            {
                "all" => "All APIs",
                "v1" => "Business API",
                "sys" => "System Management",
                "hr" => "Human Resources",
                "shopping" => "Shopping Mall",
                "article" => "Content Management",
                "finance" => "Finance",
                "sales" => "Sales",
                "operations" => "Operations",
                "marketing" => "Marketing",
                "clients" => "Clients & Vendors",
                "projects" => "Projects",
                "supplychain" => "Supply Chain",
                "legal" => "Legal",
                "monitor" => "System Monitor",
                "tools" => "Tools & Utilities",
                "email" => "Email Service",
                "public" => "Public APIs",
                "business" => "Business Logic",
                _ => group.ToUpper()
            };
        }

        private static string GetGroupDescription(string group)
        {
            return group switch
            {
                "all" => "Complete API documentation with all endpoints",
                "v1" => "Core business logic and operations",
                "sys" => "System configuration and management",
                "hr" => "Human resources and employee management",
                "shopping" => "E-commerce and shopping features",
                "article" => "Content and article management",
                "finance" => "Financial transaction management",
                "sales" => "Sales opportunity management",
                "operations" => "Operations management",
                "marketing" => "Marketing campaign management",
                "clients" => "Unified partner management system",
                "projects" => "Project management system",
                "supplychain" => "Supply chain management system",
                "legal" => "Legal department operations",
                "monitor" => "System monitoring and metrics",
                "tools" => "Development tools and utilities",
                "email" => "Email communication services",
                "public" => "Public facing API endpoints",
                "business" => "Core business logic APIs",
                _ => $"{group} API endpoints"
            };
        }

        private static void LogScalarUrls(IConfiguration configuration, string[] groups)
        {
            var scalarPrefix = configuration.GetSection("ApiDocumentation").GetValue<string>("ScalarRoutePrefix", "scalar");
            var swaggerPrefix = configuration.GetSection("ApiDocumentation").GetValue<string>("SwaggerRoutePrefix", "swagger");
            var urls = configuration["urls"] ?? "http://localhost:5000";
            var baseUrl = urls.Split(';')[0];
            
            Console.WriteLine("\n╔════════════════════════════════════════════════════════╗");
            Console.WriteLine("║              API DOCUMENTATION URLS                     ║");
            Console.WriteLine("╠════════════════════════════════════════════════════════╣");
            Console.WriteLine($"║ Groups Page:   {baseUrl}/{scalarPrefix}/groups         ║");
            Console.WriteLine($"║ Swagger UI:    {baseUrl}/{swaggerPrefix}               ║");
            Console.WriteLine("╠════════════════════════════════════════════════════════╣");
            Console.WriteLine("║ Scalar API Groups:                                      ║");
            foreach (var group in groups)
            {
                var title = GetGroupTitle(group);
                var url = $"{baseUrl}/{scalarPrefix}/{group}";
                Console.WriteLine($"║   {title,-20} {url,-30} ║");
            }
            Console.WriteLine("╚════════════════════════════════════════════════════════╝\n");
        }
    }
}
