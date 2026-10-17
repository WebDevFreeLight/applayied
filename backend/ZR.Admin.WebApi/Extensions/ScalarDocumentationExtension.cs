using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Scalar.AspNetCore;
using System.Linq;
using System.Threading.Tasks;

namespace ZR.Admin.WebApi.Extensions
{
    /// <summary>
    /// Simplified Scalar Documentation Extension
    /// </summary>
    public static class ScalarDocumentationExtension
    {
        /// <summary>
        /// Map Scalar API documentation with all groups
        /// </summary>
        public static void MapScalarApiDocumentation(this WebApplication app, IConfiguration configuration)
        {
            var apiDocConfig = configuration.GetSection("ApiDocumentation");
            var enableScalar = apiDocConfig.GetValue<bool>("EnableScalar", true);
            
            if (!enableScalar)
                return;

            var swaggerPrefix = apiDocConfig.GetValue<string>("SwaggerRoutePrefix", "swagger");
            var scalarPrefix = apiDocConfig.GetValue<string>("ScalarRoutePrefix", "scalar");
            var documentTitle = apiDocConfig.GetValue<string>("DocumentTitle", "API Documentation");

            // Define all API groups
            var apiGroups = new[] { "all", "sys", "hr", "monitor", "tools", "email", "article", "shopping", "public", "business" };

            // Map Scalar for each specific group to ensure correct OpenAPI URL
            foreach (var group in apiGroups)
            {
                app.MapScalarApiReference(options =>
                {
                    options
                        .WithTitle($"{documentTitle} - {GetGroupTitle(group)}")
                        .WithTheme(ScalarTheme.Purple)
                        .WithDarkMode(true)
                        .WithEndpointPrefix($"/{scalarPrefix}/{{documentName}}")
                        // Set the OpenAPI route pattern to the specific group's swagger.json
                        .WithOpenApiRoutePattern($"/{swaggerPrefix}/{group}/swagger.json") 
                        .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient)
                        .WithPreferredScheme("Bearer")
                        .WithSidebar(true)
                        .WithSearchHotKey("k")
                        .WithModels(true)
                        .WithDownloadButton(true)
                        .WithTestRequestButton(true);
                });
            }

            // Create a landing page that redirects to available groups
            app.MapGet($"/{scalarPrefix}", context =>
            {
                // Redirect to the 'all' group by default
                context.Response.Redirect($"/{scalarPrefix}/all");
                return Task.CompletedTask;
            });


            // Create a custom group selector page
            app.MapGet($"/{scalarPrefix}/select", async context =>
            {
                var html = GenerateGroupSelectorPage(scalarPrefix);
                context.Response.ContentType = "text/html; charset=utf-8";
                await context.Response.WriteAsync(html);
            });
        }

        private static string GetGroupTitle(string group)
        {
            return group switch
            {
                "all" => "All APIs",
                "sys" => "System Management",
                "hr" => "HR Management",
                "monitor" => "System Monitor",
                "tools" => "Tools & Utilities",
                "email" => "Email Service",
                "article" => "Content Management",
                "shopping" => "Shopping Mall",
                "public" => "Public APIs",
                "business" => "Business Logic",
                _ => group
            };
        }

        private static string GenerateGroupSelectorPage(string scalarPrefix)
        {
            var groups = new[]
            {
                ("all", "📚 All APIs", "Complete API Documentation"),
                ("sys", "⚙️ System", "System Management APIs"),
                ("hr", "👥 HR", "Human Resources APIs"),
                ("monitor", "📊 Monitor", "System Monitoring APIs"),
                ("tools", "🔧 Tools", "Tools & Utilities APIs"),
                ("email", "📧 Email", "Email Service APIs"),
                ("article", "📝 Content", "Content Management APIs"),
                ("shopping", "🛒 Shopping", "Shopping Mall APIs"),
                ("public", "🌐 Public", "Public APIs"),
                ("business", "💼 Business", "Business Logic APIs")
            };

            return $@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Scalar API Documentation - Select Group</title>
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
            background: white;
            border-radius: 16px;
            box-shadow: 0 20px 40px rgba(0,0,0,0.15);
            padding: 2.5rem;
            max-width: 1000px;
            width: 100%;
        }}
        h1 {{
            text-align: center;
            color: #1a202c;
            margin-bottom: 0.5rem;
            font-size: 2rem;
        }}
        .subtitle {{
            text-align: center;
            color: #718096;
            margin-bottom: 2rem;
        }}
        .groups {{
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
            gap: 1rem;
        }}
        .group {{
            display: flex;
            flex-direction: column;
            align-items: center;
            padding: 1.5rem;
            background: #f7fafc;
            border-radius: 12px;
            text-decoration: none;
            transition: all 0.3s ease;
            border: 2px solid transparent;
        }}
        .group:hover {{
            background: white;
            border-color: #667eea;
            transform: translateY(-4px);
            box-shadow: 0 10px 20px rgba(102, 126, 234, 0.1);
        }}
        .group-emoji {{
            font-size: 2.5rem;
            margin-bottom: 0.5rem;
        }}
        .group-name {{
            font-weight: 600;
            color: #2d3748;
            margin-bottom: 0.25rem;
        }}
        .group-desc {{
            font-size: 0.875rem;
            color: #718096;
            text-align: center;
        }}
        .all-group {{
            grid-column: 1 / -1;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
        }}
        .all-group .group-name,
        .all-group .group-desc {{
            color: white;
        }}
        .info {{
            margin-top: 2rem;
            padding-top: 2rem;
            border-top: 1px solid #e2e8f0;
            text-align: center;
            color: #718096;
            font-size: 0.875rem;
        }}
        .info a {{
            color: #667eea;
            text-decoration: none;
        }}
        .info a:hover {{
            text-decoration: underline;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <h1>🎨 Scalar API Documentation</h1>
        <p class='subtitle'>Select an API group to explore</p>
        
        <div class='groups'>
            {string.Join("\n", groups.Select(g => $@"
            <a href='/{scalarPrefix}/{g.Item1}' class='group {(g.Item1 == "all" ? "all-group" : "")}'>
                <div class='group-emoji'>{g.Item2.Substring(0, 2)}</div>
                <div class='group-name'>{g.Item2.Substring(2).Trim()}</div>
                <div class='group-desc'>{g.Item3}</div>
            </a>"))}
        </div>
        
        <div class='info'>
            <p>💡 Tip: You can directly access any group using: <code>/{scalarPrefix}/{{group-name}}</code></p>
            <p>📖 Also available: <a href='/swagger'>Swagger UI</a></p>
        </div>
    </div>
</body>
</html>";
        }
    }
}
