using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using ZR.ServiceCore.Middleware;

namespace ZR.Admin.WebApi.Discovery
{
    /// <summary>
    /// Permission Discovery Tool
    ///
    /// Automatically scans all controllers and extracts:
    /// 1. All API endpoints and their routes
    /// 2. Required permissions for each endpoint
    /// 3. Suggested lookup endpoints
    ///
    /// Run this tool to generate: backend-permission-discovery.json
    /// </summary>
    public class PermissionDiscoveryTool
    {
        public class ControllerPermission
        {
            public string Controller { get; set; }
            public string Action { get; set; }
            public string Route { get; set; }
            public string HttpMethod { get; set; }
            public string Permission { get; set; }
            public bool IsLookupEndpoint { get; set; }
            public string SuggestedLookupPermission { get; set; }
            public List<string> Parameters { get; set; }
            public string ReturnType { get; set; }
        }

        public class DiscoveryOutput
        {
            public string Version { get; set; }
            public string Generated { get; set; }
            public int TotalControllers { get; set; }
            public int TotalEndpoints { get; set; }
            public int TotalPermissions { get; set; }
            public int SuggestedLookupEndpoints { get; set; }
            public List<ControllerPermission> Endpoints { get; set; }
            public List<string> UniquePermissions { get; set; }
            public Dictionary<string, List<string>> PermissionsByModule { get; set; }
            public List<LookupSuggestion> LookupSuggestions { get; set; }
        }

        public class LookupSuggestion
        {
            public string Entity { get; set; }
            public string ExistingEndpoint { get; set; }
            public string SuggestedLookupEndpoint { get; set; }
            public string SuggestedPermission { get; set; }
            public bool IsPublicLookup { get; set; }
        }

        /// <summary>
        /// Main discovery method
        /// </summary>
        public static DiscoveryOutput DiscoverPermissions()
        {
            Console.WriteLine("🔍 Starting Backend Permission Discovery...\n");

            var controllers = GetAllControllers();
            var permissions = new List<ControllerPermission>();
            var uniquePermissions = new HashSet<string>();
            var lookupSuggestions = new List<LookupSuggestion>();

            Console.WriteLine($"📋 Found {controllers.Count} controllers\n");

            foreach (var controller in controllers)
            {
                var controllerPermissions = DiscoverControllerPermissions(controller);
                permissions.AddRange(controllerPermissions);

                foreach (var perm in controllerPermissions)
                {
                    if (!string.IsNullOrEmpty(perm.Permission))
                    {
                        uniquePermissions.Add(perm.Permission);
                    }

                    if (perm.IsLookupEndpoint && !string.IsNullOrEmpty(perm.SuggestedLookupPermission))
                    {
                        lookupSuggestions.Add(new LookupSuggestion
                        {
                            Entity = ExtractEntityName(perm.Controller),
                            ExistingEndpoint = perm.Route,
                            SuggestedLookupEndpoint = perm.Route.Replace("api/", "api/") + "/lookup",
                            SuggestedPermission = perm.SuggestedLookupPermission,
                            IsPublicLookup = IsCommonLookup(perm.Controller)
                        });
                    }
                }

                Console.WriteLine($"   ✓ {controller.Name}: {controllerPermissions.Count} endpoints");
            }

            Console.WriteLine($"\n✅ Discovery complete!\n");

            var permissionsByModule = GroupPermissionsByModule(uniquePermissions.ToList());

            var output = new DiscoveryOutput
            {
                Version = "2.0",
                Generated = DateTime.UtcNow.ToString("o"),
                TotalControllers = controllers.Count,
                TotalEndpoints = permissions.Count,
                TotalPermissions = uniquePermissions.Count,
                SuggestedLookupEndpoints = lookupSuggestions.Count,
                Endpoints = permissions,
                UniquePermissions = uniquePermissions.OrderBy(p => p).ToList(),
                PermissionsByModule = permissionsByModule,
                LookupSuggestions = lookupSuggestions
            };

            return output;
        }

        /// <summary>
        /// Get all controller types
        /// </summary>
        private static List<Type> GetAllControllers()
        {
            var assembly = Assembly.GetExecutingAssembly();
            return assembly.GetTypes()
                .Where(t => typeof(ControllerBase).IsAssignableFrom(t) && !t.IsAbstract)
                .OrderBy(t => t.Name)
                .ToList();
        }

        /// <summary>
        /// Discover permissions for a single controller
        /// </summary>
        private static List<ControllerPermission> DiscoverControllerPermissions(Type controller)
        {
            var permissions = new List<ControllerPermission>();

            // Get controller route
            var controllerRoute = GetControllerRoute(controller);

            // Get all public action methods
            var methods = controller.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(m => !m.IsSpecialName); // Exclude property getters/setters

            foreach (var method in methods)
            {
                var httpMethod = GetHttpMethod(method);
                if (string.IsNullOrEmpty(httpMethod)) continue; // Not an HTTP action

                var actionRoute = GetActionRoute(method);
                var fullRoute = CombineRoutes(controllerRoute, actionRoute);
                var permission = GetPermission(method);
                var isLookup = IsLookupEndpoint(method, fullRoute);

                permissions.Add(new ControllerPermission
                {
                    Controller = controller.Name.Replace("Controller", ""),
                    Action = method.Name,
                    Route = $"{httpMethod} {fullRoute}",
                    HttpMethod = httpMethod,
                    Permission = permission,
                    IsLookupEndpoint = isLookup,
                    SuggestedLookupPermission = isLookup ? SuggestLookupPermission(permission) : null,
                    Parameters = GetParameters(method),
                    ReturnType = method.ReturnType.Name
                });
            }

            return permissions;
        }

        /// <summary>
        /// Get controller route from RouteAttribute
        /// </summary>
        private static string GetControllerRoute(Type controller)
        {
            var routeAttr = controller.GetCustomAttribute<RouteAttribute>();
            return routeAttr?.Template ?? "api/" + controller.Name.Replace("Controller", "").ToLower();
        }

        /// <summary>
        /// Get action route from HttpMethodAttribute or RouteAttribute
        /// </summary>
        private static string GetActionRoute(MethodInfo method)
        {
            // Check for HttpGet, HttpPost, etc. with route template
            var httpAttr = method.GetCustomAttributes()
                .FirstOrDefault(a => a is HttpMethodAttribute) as HttpMethodAttribute;

            if (httpAttr != null && !string.IsNullOrEmpty(httpAttr.Template))
            {
                return httpAttr.Template;
            }

            // Check for RouteAttribute
            var routeAttr = method.GetCustomAttribute<RouteAttribute>();
            if (routeAttr != null)
            {
                return routeAttr.Template;
            }

            // Default: use method name
            return method.Name.ToLower();
        }

        /// <summary>
        /// Combine controller and action routes
        /// </summary>
        private static string CombineRoutes(string controllerRoute, string actionRoute)
        {
            if (string.IsNullOrEmpty(actionRoute)) return $"/{controllerRoute}";

            if (actionRoute.StartsWith("/")) return actionRoute;

            return $"/{controllerRoute}/{actionRoute}";
        }

        /// <summary>
        /// Get HTTP method from attributes
        /// </summary>
        private static string GetHttpMethod(MethodInfo method)
        {
            if (method.GetCustomAttribute<HttpGetAttribute>() != null) return "GET";
            if (method.GetCustomAttribute<HttpPostAttribute>() != null) return "POST";
            if (method.GetCustomAttribute<HttpPutAttribute>() != null) return "PUT";
            if (method.GetCustomAttribute<HttpDeleteAttribute>() != null) return "DELETE";
            if (method.GetCustomAttribute<HttpPatchAttribute>() != null) return "PATCH";

            return null; // Not an HTTP action
        }

        /// <summary>
        /// Get permission from ActionPermissionFilter
        /// </summary>
        private static string GetPermission(MethodInfo method)
        {
            var permAttr = method.GetCustomAttribute<ActionPermissionFilter>();
            return permAttr?.Permission;
        }

        /// <summary>
        /// Check if this is a potential lookup endpoint
        /// </summary>
        private static bool IsLookupEndpoint(MethodInfo method, string route)
        {
            // Heuristics to identify lookup endpoints
            var methodName = method.Name.ToLower();
            var routeLower = route.ToLower();

            // Check for explicit lookup routes
            if (routeLower.Contains("/lookup")) return true;

            // Check for methods that return list data with minimal parameters
            if (methodName.Contains("getlist") || methodName.Contains("get"))
            {
                var parameters = method.GetParameters();

                // Lookup endpoints typically have:
                // - No parameters OR only a search/filter parameter
                // - Return List<T> or similar
                if (parameters.Length == 0 ||
                    (parameters.Length == 1 && parameters[0].Name.ToLower().Contains("search")))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Suggest a lookup permission based on the main permission
        /// </summary>
        private static string SuggestLookupPermission(string mainPermission)
        {
            if (string.IsNullOrEmpty(mainPermission)) return null;

            // Convert "module:entity:list" to "module:entity:lookup"
            var parts = mainPermission.Split(':');
            if (parts.Length >= 3)
            {
                parts[2] = "lookup";
                return string.Join(":", parts);
            }

            return mainPermission + ":lookup";
        }

        /// <summary>
        /// Check if this is a common lookup that should be public
        /// </summary>
        private static bool IsCommonLookup(string controller)
        {
            var commonLookups = new[]
            {
                "HrEmployee",
                "HrDepartment",
                "SysUser",
                "SysRole",
                "FinanceCurrency",
                "FinanceTaxCode",
                "EventType",
                "EventStatus"
            };

            return commonLookups.Any(c => controller.Contains(c));
        }

        /// <summary>
        /// Extract entity name from controller name
        /// </summary>
        private static string ExtractEntityName(string controller)
        {
            // Remove common prefixes
            var name = controller
                .Replace("Controller", "")
                .Replace("Sys", "")
                .Replace("Hr", "")
                .Replace("Finance", "")
                .Replace("Event", "");

            return name;
        }

        /// <summary>
        /// Get method parameters
        /// </summary>
        private static List<string> GetParameters(MethodInfo method)
        {
            return method.GetParameters()
                .Select(p => $"{p.ParameterType.Name} {p.Name}")
                .ToList();
        }

        /// <summary>
        /// Group permissions by module
        /// </summary>
        private static Dictionary<string, List<string>> GroupPermissionsByModule(List<string> permissions)
        {
            var grouped = new Dictionary<string, List<string>>();

            foreach (var perm in permissions)
            {
                var module = perm.Split(':').FirstOrDefault() ?? "unknown";

                if (!grouped.ContainsKey(module))
                {
                    grouped[module] = new List<string>();
                }

                grouped[module].Add(perm);
            }

            return grouped;
        }

        /// <summary>
        /// Save discovery output to JSON file
        /// </summary>
        public static void SaveToFile(DiscoveryOutput output, string filePath)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var json = JsonSerializer.Serialize(output, options);
            File.WriteAllText(filePath, json);

            Console.WriteLine($"📁 Output saved to: {filePath}\n");
            Console.WriteLine("📊 Summary:");
            Console.WriteLine($"   - Controllers: {output.TotalControllers}");
            Console.WriteLine($"   - Endpoints: {output.TotalEndpoints}");
            Console.WriteLine($"   - Unique Permissions: {output.TotalPermissions}");
            Console.WriteLine($"   - Suggested Lookup Endpoints: {output.SuggestedLookupEndpoints}");
        }

        /// <summary>
        /// Entry point for running the discovery tool
        /// </summary>
        public static void Run(string outputPath = null)
        {
            outputPath = outputPath ?? Path.Combine(Directory.GetCurrentDirectory(), "backend-permission-discovery.json");

            var output = DiscoverPermissions();
            SaveToFile(output, outputPath);
        }
    }
}
