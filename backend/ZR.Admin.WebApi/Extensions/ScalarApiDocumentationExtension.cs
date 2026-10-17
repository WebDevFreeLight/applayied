using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Scalar.AspNetCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ZR.Admin.WebApi.Extensions
{
    /// <summary>
    /// Scalar API Documentation Extension with full group support
    /// </summary>
    public static class ScalarApiDocumentationExtension
    {
        private static readonly List<ApiGroupInfo> ApiGroups = new()
        {
            new ApiGroupInfo("all", "📚 All APIs", "Complete API Documentation", "#6B46C1"),
            new ApiGroupInfo("sys", "⚙️ System Management", "User, Role, Menu, Config APIs", "#3B82F6"),
            new ApiGroupInfo("hr", "👥 HR Management", "Employee, Payroll, Leave APIs", "#10B981"),
            new ApiGroupInfo("monitor", "📊 System Monitor", "Logs, Performance, Online Users", "#F59E0B"),
            new ApiGroupInfo("tools", "🔧 Tools & Utilities", "Code Generation, File Upload", "#8B5CF6"),
            new ApiGroupInfo("email", "📧 Email Service", "Email Templates, Logs", "#EC4899"),
            new ApiGroupInfo("article", "📝 Content Management", "Articles, Comments, Topics", "#14B8A6"),
            new ApiGroupInfo("shopping", "🛒 Shopping Mall", "Products, Orders, Categories", "#F97316"),
            new ApiGroupInfo("public", "🌐 Public APIs", "Banner, WeChat APIs", "#06B6D4"),
            new ApiGroupInfo("business", "💼 Business Logic", "Custom Business APIs", "#6366F1")
        };

        /// <summary>
        /// Configure Scalar documentation for all API groups
        /// </summary>
        public static void ConfigureScalarDocumentation(this WebApplication app, IConfiguration configuration)
        {
            var apiDocConfig = configuration.GetSection("ApiDocumentation");
            var enableScalar = apiDocConfig.GetValue<bool>("EnableScalar", true);
            
            if (!enableScalar)
                return;

            var swaggerPrefix = apiDocConfig.GetValue<string>("SwaggerRoutePrefix", "swagger");
            var scalarPrefix = apiDocConfig.GetValue<string>("ScalarRoutePrefix", "scalar");
            var documentTitle = apiDocConfig.GetValue<string>("DocumentTitle", "API Documentation");

            // Map the main Scalar selection page
            MapScalarSelectionPage(app, scalarPrefix, documentTitle);
            
            // Map Scalar API reference endpoint with proper configuration
            MapScalarApiEndpoint(app, scalarPrefix, swaggerPrefix, documentTitle);
            
            // Map individual group redirects for better UX
            MapGroupRedirects(app, scalarPrefix);
        }

        private static void MapScalarSelectionPage(WebApplication app, string scalarPrefix, string documentTitle)
        {
            app.MapGet($"/{scalarPrefix}", async context =>
            {
                var currentGroup = context.Request.Query["group"].ToString();
                if (!string.IsNullOrEmpty(currentGroup))
                {
                    // Redirect to the specific group
                    context.Response.Redirect($"/{scalarPrefix}/{currentGroup}");
                    return;
                }

                var html = GenerateSelectionPageHtml(scalarPrefix, documentTitle);
                context.Response.ContentType = "text/html; charset=utf-8";
                await context.Response.WriteAsync(html);
            });
        }

        private static void MapScalarApiEndpoint(WebApplication app, string scalarPrefix, string swaggerPrefix, string documentTitle)
        {
            // Map Scalar endpoints for each specific API group
            foreach (var group in ApiGroups)
            {
                app.MapScalarApiReference(options =>
                {
                    options
                        .WithTitle($"{documentTitle} - {group.Title.Substring(2)}")
                        .WithTheme(ScalarTheme.Purple)
                        .WithDarkMode(true)
                        .WithEndpointPrefix($"/{scalarPrefix}/{group.Name}")
                        // .WithEndpointPrefix($"/{scalarPrefix}/{{documentName}}")
                        .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient)
                        .WithPreferredScheme("Bearer")
                        .WithSidebar(true)
                        .WithSearchHotKey("k")
                        .WithModels(true)
                        .WithDownloadButton(true)
                        .WithTestRequestButton(true)
                        .WithOpenApiRoutePattern($"/{swaggerPrefix}/{group.Name}/swagger.json");
                });
            }
        }

        private static void MapGroupRedirects(WebApplication app, string scalarPrefix)
        {
            // Create a combined selection page that shows current group and allows switching
            app.MapGet($"/{scalarPrefix}/select", async context =>
            {
                var currentGroup = context.Request.Query["current"].ToString() ?? "all";
                var html = GenerateGroupSelectorHtml(scalarPrefix, currentGroup);
                context.Response.ContentType = "text/html; charset=utf-8";
                await context.Response.WriteAsync(html);
            });
        }

        private static string GenerateSelectionPageHtml(string scalarPrefix, string documentTitle)
        {
            return $@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>{documentTitle} - Scalar Documentation</title>
    <style>
        * {{ margin: 0; padding: 0; box-sizing: border-box; }}
        body {{
            font-family: system-ui, -apple-system, 'Segoe UI', Roboto, sans-serif;
            background: linear-gradient(135deg, #6B46C1 0%, #4C1D95 100%);
            min-height: 100vh;
            display: flex;
            align-items: center;
            justify-content: center;
            padding: 1rem;
        }}
        .container {{
            background: rgba(255, 255, 255, 0.98);
            border-radius: 20px;
            box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.25);
            padding: 3rem;
            max-width: 1200px;
            width: 100%;
            animation: fadeIn 0.5s ease-out;
        }}
        @keyframes fadeIn {{
            from {{ opacity: 0; transform: translateY(20px); }}
            to {{ opacity: 1; transform: translateY(0); }}
        }}
        .header {{
            text-align: center;
            margin-bottom: 3rem;
        }}
        h1 {{
            color: #1F2937;
            font-size: 2.5rem;
            margin-bottom: 0.5rem;
            font-weight: 700;
        }}
        .subtitle {{
            color: #6B7280;
            font-size: 1.125rem;
        }}
        .groups {{
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
            gap: 1.5rem;
            margin-bottom: 2rem;
        }}
        .group-card {{
            background: white;
            border: 2px solid #E5E7EB;
            border-radius: 12px;
            padding: 1.5rem;
            text-decoration: none;
            transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
            display: flex;
            align-items: flex-start;
            gap: 1rem;
            position: relative;
            overflow: hidden;
        }}
        .group-card::before {{
            content: '';
            position: absolute;
            top: 0;
            left: 0;
            right: 0;
            height: 3px;
            background: var(--color);
            transform: scaleX(0);
            transition: transform 0.3s ease;
        }}
        .group-card:hover {{
            border-color: var(--color);
            transform: translateY(-4px);
            box-shadow: 0 20px 40px -15px rgba(0, 0, 0, 0.2);
        }}
        .group-card:hover::before {{
            transform: scaleX(1);
        }}
        .emoji {{
            font-size: 2rem;
            flex-shrink: 0;
        }}
        .group-info {{
            flex: 1;
        }}
        .group-title {{
            color: #1F2937;
            font-weight: 600;
            font-size: 1.125rem;
            margin-bottom: 0.375rem;
        }}
        .group-desc {{
            color: #6B7280;
            font-size: 0.875rem;
            line-height: 1.5;
        }}
        .all-apis {{
            grid-column: 1 / -1;
            background: linear-gradient(135deg, #6B46C1 0%, #4C1D95 100%);
            border: none;
            color: white;
        }}
        .all-apis .group-title,
        .all-apis .group-desc {{
            color: white;
        }}
        .all-apis:hover {{
            transform: translateY(-6px);
            box-shadow: 0 25px 50px -12px rgba(107, 70, 193, 0.5);
        }}
        .footer {{
            text-align: center;
            padding-top: 2rem;
            border-top: 1px solid #E5E7EB;
            color: #6B7280;
            font-size: 0.875rem;
        }}
        .quick-links {{
            display: flex;
            justify-content: center;
            gap: 2rem;
            margin-top: 1rem;
        }}
        .quick-link {{
            color: #6B46C1;
            text-decoration: none;
            font-weight: 500;
            transition: color 0.2s;
        }}
        .quick-link:hover {{
            color: #4C1D95;
            text-decoration: underline;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🎨 Scalar API Documentation</h1>
            <p class='subtitle'>Interactive API documentation with full OpenAPI support</p>
        </div>
        
        <div class='groups'>
            <a href='/{scalarPrefix}/all' class='group-card all-apis' style='--color: #6B46C1'>
                <span class='emoji'>📚</span>
                <div class='group-info'>
                    <div class='group-title'>All APIs</div>
                    <div class='group-desc'>Browse complete API documentation with all endpoints</div>
                </div>
            </a>
            {string.Join("\n", ApiGroups.Skip(1).Select(g => $@"
            <a href='/{scalarPrefix}/{g.Name}' class='group-card' style='--color: {g.Color}'>
                <span class='emoji'>{g.Emoji}</span>
                <div class='group-info'>
                    <div class='group-title'>{g.Title.Substring(2)}</div>
                    <div class='group-desc'>{g.Description}</div>
                </div>
            </a>"))}
        </div>
        
        <div class='footer'>
            <p>Powered by Scalar & OpenAPI 3.0</p>
            <div class='quick-links'>
                <a href='/swagger' class='quick-link'>Swagger UI</a>
                <a href='/swagger/all/swagger.json' class='quick-link'>OpenAPI JSON</a>
                <a href='#' class='quick-link' onclick='toggleTheme(); return false;'>Toggle Theme</a>
            </div>
        </div>
    </div>
    
    <script>
        function toggleTheme() {{
            document.body.style.background = document.body.style.background.includes('6B46C1') 
                ? 'linear-gradient(135deg, #1F2937 0%, #111827 100%)' 
                : 'linear-gradient(135deg, #6B46C1 0%, #4C1D95 100%)';
        }}
    </script>
</body>
</html>";
        }

        private static string GenerateGroupSelectorHtml(string scalarPrefix, string currentGroup)
        {
            var currentGroupInfo = ApiGroups.FirstOrDefault(g => g.Name == currentGroup) ?? ApiGroups[0];
            
            return $@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Select API Group</title>
    <style>
        body {{
            font-family: system-ui, -apple-system, sans-serif;
            margin: 0;
            padding: 20px;
            background: #F9FAFB;
        }}
        .selector {{
            max-width: 600px;
            margin: 0 auto;
            background: white;
            border-radius: 12px;
            padding: 1.5rem;
            box-shadow: 0 10px 25px rgba(0,0,0,0.1);
        }}
        h2 {{
            margin-bottom: 1rem;
            color: #1F2937;
        }}
        .current {{
            background: linear-gradient(135deg, #6B46C1 0%, #4C1D95 100%);
            color: white;
            padding: 1rem;
            border-radius: 8px;
            margin-bottom: 1.5rem;
        }}
        .group-list {{
            display: grid;
            gap: 0.5rem;
        }}
        .group-option {{
            display: flex;
            align-items: center;
            gap: 0.75rem;
            padding: 0.75rem;
            background: #F9FAFB;
            border: 2px solid transparent;
            border-radius: 8px;
            text-decoration: none;
            color: #1F2937;
            transition: all 0.2s;
        }}
        .group-option:hover {{
            background: white;
            border-color: #6B46C1;
            transform: translateX(4px);
        }}
        .group-option.active {{
            background: #EDE9FE;
            border-color: #6B46C1;
        }}
    </style>
</head>
<body>
    <div class='selector'>
        <h2>Switch API Group</h2>
        <div class='current'>
            <strong>Current:</strong> {currentGroupInfo.Emoji} {currentGroupInfo.Title.Substring(2)}
        </div>
        <div class='group-list'>
            {string.Join("\n", ApiGroups.Select(g => $@"
            <a href='/{scalarPrefix}/{g.Name}' class='group-option {(g.Name == currentGroup ? "active" : "")}'>
                <span>{g.Emoji}</span>
                <div>
                    <strong>{g.Title.Substring(2)}</strong>
                    <div style='font-size: 0.875rem; color: #6B7280;'>{g.Description}</div>
                </div>
            </a>"))}
        </div>
    </div>
</body>
</html>";
        }

        private class ApiGroupInfo
        {
            public string Name { get; }
            public string Emoji => Title.Substring(0, 2);
            public string Title { get; }
            public string Description { get; }
            public string Color { get; }

            public ApiGroupInfo(string name, string title, string description, string color)
            {
                Name = name;
                Title = title;
                Description = description;
                Color = color;
            }
        }
    }
}
