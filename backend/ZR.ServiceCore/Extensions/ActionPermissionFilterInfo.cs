using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using ZR.ServiceCore.Middleware;

namespace ZR.Admin.WebApi.Extensions
{
    public class ActionPermissionFilterInfo
    {
        public ActionPermissionFilter Filter { get; set; }
        public string ControllerName { get; set; }
        public string MethodName { get; set; }
        public string FullName { get; set; }
        public bool IsClassLevel { get; set; }
        public List<string> IndividualPermissions { get; set; }


        public static List<ActionPermissionFilterInfo> GetAllActionPermissionFiltersWithInfo()
        {
            var filterInfos = new List<ActionPermissionFilterInfo>();

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    var controllerTypes = assembly.GetTypes()
                        .Where(t => typeof(ControllerBase).IsAssignableFrom(t))
                        .ToList();

                    foreach (var controllerType in controllerTypes)
                    {
                        // Class-level filters
                        var classFilters = controllerType.GetCustomAttributes<ActionPermissionFilter>(true);
                        foreach (var filter in classFilters)
                        {
                            filterInfos.Add(new ActionPermissionFilterInfo
                            {
                                Filter = filter,
                                ControllerName = controllerType.Name,
                                MethodName = "[Class Level]",
                                FullName = controllerType.FullName,
                                IsClassLevel = true,
                                IndividualPermissions = GetIndividualPermissions(filter.Permission)
                            });
                        }

                        // Method-level filters
                        var methods = controllerType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                        foreach (var method in methods)
                        {
                            var methodFilters = method.GetCustomAttributes<ActionPermissionFilter>(true);
                            foreach (var filter in methodFilters)
                            {
                                filterInfos.Add(new ActionPermissionFilterInfo
                                {
                                    Filter = filter,
                                    ControllerName = controllerType.Name,
                                    MethodName = method.Name,
                                    FullName = $"{controllerType.FullName}.{method.Name}",
                                    IsClassLevel = false,
                                    IndividualPermissions = GetIndividualPermissions(filter.Permission)
                                });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error scanning assembly {assembly.FullName}: {ex.Message}");
                }
            }

            return filterInfos;
        }

        public static List<string> GetIndividualPermissions(string permission)
        {
            if (string.IsNullOrEmpty(permission))
                return new List<string>();

            return permission.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                            .ToList();
        }


        // Example usage methods
        public static void PrintAllFiltersWithPermissions()
        {
            var filtersWithInfo = GetAllActionPermissionFiltersWithInfo();

            Console.WriteLine("=== All ActionPermissionFilter Instances ===");
            Console.WriteLine($"Total filters found: {filtersWithInfo.Count}");
            Console.WriteLine();

            foreach (var filterInfo in filtersWithInfo)
            {
                Console.WriteLine($"Controller: {filterInfo.ControllerName}");
                Console.WriteLine($"Method: {filterInfo.MethodName}");
                Console.WriteLine($"Permission: {filterInfo.Filter.Permission}");
                Console.WriteLine($"RolePermi: {filterInfo.Filter.RolePermi}");
                Console.WriteLine($"Is Class Level: {filterInfo.IsClassLevel}");
                Console.WriteLine($"Individual Permissions: {string.Join(", ", filterInfo.IndividualPermissions)}");
                Console.WriteLine(new string('-', 50));
            }
        }
    }
}

