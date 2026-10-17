using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SqlSugar;
using SqlSugar.IOC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ZR.HR
{

    /// <summary>
    /// ????
    /// </summary>
    public static class InitHRTable
    {


        public static List<Type> GetAllPropertyTypes(this Type filterType)
        {
            var propertyTypes = new List<Type>();

            if (filterType == null)
                return propertyTypes;

            // Get all properties from the ActionPermissionFilter class
            var properties = filterType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            foreach (var property in properties)
            {
                propertyTypes.Add(property.PropertyType);

                // For collections, get the generic type arguments
                if (property.PropertyType.IsGenericType)
                {
                    var genericArgs = property.PropertyType.GetGenericArguments();
                    propertyTypes.AddRange(genericArgs);
                }
            }

            return propertyTypes.Distinct().ToList();
        }



        public static List<Type> FindTypesInNamespaces(params string[] namespaces)
        {
            var allTypes = new List<Type>();

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    var types = assembly.GetTypes()
                        .Where(t => namespaces.Any(ns =>
                            t.Namespace != null &&
                            (t.Namespace.Equals(ns) || t.Namespace.StartsWith(ns + "."))))
                        .ToList();

                    allTypes.AddRange(types);
                }
                catch (ReflectionTypeLoadException)
                {
                    // Skip assemblies that can't be loaded
                    continue;
                }
            }

            return allTypes.Distinct().ToList();
        }

        // Specific method for ActionPermissionFilter and related types
        public static List<Type> GetActionPermissionRelatedTypes(params string[] namespaces)
        {
            var baseNamespaces = new List<string>
    {
        "ZR.ServiceCore.Middleware",
        "Infrastructure",
        "Infrastructure.Model",
        "ZR.Model.System.Dto",
        "ZR.ServiceCore.Services"
    };

            if (namespaces != null && namespaces.Length > 0)
            {
                baseNamespaces.AddRange(namespaces);
            }

            return FindTypesInNamespaces(baseNamespaces.ToArray());
        }


        public static List<Type> GetAllReferencedTypes(this Type type)
        {
            var referencedTypes = new HashSet<Type>();

            if (type == null)
                return referencedTypes.ToList();

            // Add the type itself
            referencedTypes.Add(type);

            // Get property types
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var property in properties)
            {
                referencedTypes.Add(property.PropertyType);

                if (property.PropertyType.IsGenericType)
                {
                    var genericArgs = property.PropertyType.GetGenericArguments();
                    foreach (var genericArg in genericArgs)
                    {
                        referencedTypes.Add(genericArg);
                    }
                }
            }

            // Get field types
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var field in fields)
            {
                referencedTypes.Add(field.FieldType);
            }

            // Get method return types and parameter types
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            foreach (var method in methods)
            {
                referencedTypes.Add(method.ReturnType);

                foreach (var parameter in method.GetParameters())
                {
                    referencedTypes.Add(parameter.ParameterType);
                }
            }

            return referencedTypes.Where(t => t != null).Distinct().ToList();
        }



        public static List<ActionPermissionFilter> GetAllActionPermissionFilters()
        {
            var filters = new List<ActionPermissionFilter>();

            // Scan all assemblies for ActionPermissionFilter usage
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    var controllerTypes = assembly.GetTypes()
                        .Where(t => typeof(ControllerBase).IsAssignableFrom(t))
                        .ToList();

                    foreach (var controllerType in controllerTypes)
                    {
                        // Get class-level filters
                        var classFilters = controllerType.GetCustomAttributes<ActionPermissionFilter>(true);
                        filters.AddRange(classFilters);

                        // Get method-level filters
                        var methods = controllerType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                        foreach (var method in methods)
                        {
                            var methodFilters = method.GetCustomAttributes<ActionPermissionFilter>(true);
                            filters.AddRange(methodFilters);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error scanning assembly {assembly.FullName}: {ex.Message}");
                }
            }

            return filters;
        }



        public static List<ActionPermissionFilter> GetFiltersByPermission(string permission)
        {
            var allFilters = GetAllActionPermissionFilters();

            return allFilters.Where(filter =>
                !string.IsNullOrEmpty(filter.Permission) &&
                filter.Permission.Equals(permission, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        public static List<ActionPermissionFilter> GetFiltersContainingPermission(string permissionPart)
        {
            var allFilters = GetAllActionPermissionFilters();

            return allFilters.Where(filter =>
                !string.IsNullOrEmpty(filter.Permission) &&
                filter.Permission.Contains(permissionPart, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
        public static List<ActionPermissionFilter> GetFiltersByPermissionPattern(string pattern)
        {
            var allFilters = GetAllActionPermissionFilters();

            return allFilters.Where(filter =>
                !string.IsNullOrEmpty(filter.Permission) &&
                System.Text.RegularExpressions.Regex.IsMatch(filter.Permission, pattern))
                .ToList();
        }


        public static List<string> GetAllUniquePermissions()
        {
            var allFilters = GetAllActionPermissionFilters();

            var allPermissions = new List<string>();

            foreach (var filter in allFilters)
            {
                if (!string.IsNullOrEmpty(filter.Permission))
                {
                    // Split comma-separated permissions and add individually
                    var permissions = filter.Permission.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    allPermissions.AddRange(permissions);
                }
            }

            return allPermissions.Distinct().OrderBy(p => p).ToList();
        }

        public static Dictionary<string, List<ActionPermissionFilter>> GetFiltersGroupedByPermission()
        {
            var allFilters = GetAllActionPermissionFilters();
            var grouped = new Dictionary<string, List<ActionPermissionFilter>>();

            foreach (var filter in allFilters)
            {
                if (string.IsNullOrEmpty(filter.Permission))
                {
                    // Handle filters without explicit permission
                    const string noPermissionKey = "[No Permission]";
                    if (!grouped.ContainsKey(noPermissionKey))
                        grouped[noPermissionKey] = new List<ActionPermissionFilter>();
                    grouped[noPermissionKey].Add(filter);
                    continue;
                }

                // Split comma-separated permissions
                var permissions = filter.Permission.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                foreach (var permission in permissions)
                {
                    if (!grouped.ContainsKey(permission))
                        grouped[permission] = new List<ActionPermissionFilter>();

                    grouped[permission].Add(filter);
                }
            }

            return grouped;
        }


        // Get filters with multiple permissions (comma-separated)
        public static List<ActionPermissionFilter> GetFiltersWithMultiplePermissions()
        {
            var allFilters = GetAllActionPermissionFilters();

            return allFilters.Where(filter =>
                !string.IsNullOrEmpty(filter.Permission) &&
                filter.Permission.Contains(','))
                .ToList();
        }


        public static void IDb(this IServiceCollection services, IWebHostEnvironment environment)
        {
            var db = DbScoped.SugarScope;
            var options = App.OptionsSetting;

            bool initDb1 = options.InitDb;
            if (!initDb1)
            {
                return;
            }


            if (environment.IsDevelopment())
            {
                // Automatically discover all entity types with [SugarTable] attribute
                var entityTypes = GetAllSugarTableTypes();

                Console.WriteLine($"Found {entityTypes.Count} entities with [SugarTable] attribute");
                Console.WriteLine("Starting table initialization...");
                Console.WriteLine(new string('=', 60));

                int totalTables = entityTypes.Count;
                int processedTables = 0;
                int createdTables = 0;
                int recreatedTables = 0;
                int errorTables = 0;

                foreach (var type in entityTypes)
                {
                    processedTables++;

                    // Show current progress
                    var progressPercentage = (double)processedTables / totalTables * 100;
                    var progressBar = CreateProgressBar(progressPercentage);

                    Console.Write($"\r{progressBar} {processedTables}/{totalTables} ({progressPercentage:F1}%) - Processing: {type.Name}".PadRight(80));
                var tableName = "";
                try
                {
                    tableName = db.EntityMaintenance.GetTableName(type);
                    bool tableExists = db.DbMaintenance.IsAnyTable(tableName, true);

                    if (tableExists)
                    {
                        // TABLE EXISTS - DO NOT DROP! Just skip or sync schema
                        recreatedTables++;
                        Console.WriteLine();
                        Console.WriteLine($"✓ Table already exists (skipped): {tableName}");

                        // Sync schema without dropping
                        try
                        {
                            db.CodeFirst.InitTables(type);
                        }
                        catch { /* Ignore schema sync errors */ }
                    }
                    else
                    {
                        try
                        {
                            db.CodeFirst.InitTables(type);
                            createdTables++;

                            Console.WriteLine();
                            Console.WriteLine($"✓ Created new table: {tableName}");
                        }
                        catch (Exception ex)
                        {
                            errorTables++;
                            Console.WriteLine();
                            Console.WriteLine($"✗ Error creating table {tableName}: {ex.Message}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    errorTables++;
                    Console.WriteLine();
                    Console.WriteLine($"✗ Error processing table {tableName}: {ex.Message}");
                }
                // Get table name from the type
              
                }

                // Final progress update
                Console.WriteLine();
                Console.WriteLine(new string('=', 60));
                Console.WriteLine("Table initialization completed!");
                Console.WriteLine($"Summary:");
                Console.WriteLine($"  � Total tables processed: {totalTables}");
                Console.WriteLine($"  � New tables created: {createdTables}");
                Console.WriteLine($"  � Tables recreated: {recreatedTables}");
                Console.WriteLine($"  � Errors encountered: {errorTables}");
                Console.WriteLine($"  � Success rate: {(double)(createdTables + recreatedTables) / totalTables * 100:F1}%");
                Console.WriteLine(new string('=', 60));
            }
        }

        /// <summary>
        /// Alternative InitDb method with namespace filtering and progress tracking
        /// </summary>
        public static void InitDbWithNamespaces(this IServiceCollection services, IWebHostEnvironment environment, params string[] namespaces)
        {
            var db = DbScoped.SugarScope;
            var options = App.OptionsSetting;

            if (environment.IsDevelopment())
            {
                // Get entity types from specific namespaces
                var entityTypes = namespaces.Length > 0
                    ? GetSugarTableTypesFromNamespaces(namespaces)
                    : GetAllSugarTableTypes();

                Console.WriteLine($"Found {entityTypes.Count} entities with [SugarTable] attribute in namespaces: {string.Join(", ", namespaces)}");
                Console.WriteLine("Starting table initialization...");
                Console.WriteLine(new string('=', 60));

                int totalTables = entityTypes.Count;
                int processedTables = 0;
                int createdTables = 0;
                int skippedTables = 0;
                int errorTables = 0;
                string tableName = "";
                bool tableExists = true;

                foreach (var type in entityTypes)
                {
                    processedTables++;

                    // Show current progress
                    var progressPercentage = (double)processedTables / totalTables * 100;
                    var progressBar = CreateProgressBar(progressPercentage);

                    Console.Write($"\r{progressBar} {processedTables}/{totalTables} ({progressPercentage:F1}%) - Processing: {type.Name}".PadRight(80));

                    try
                    {

                        tableName = db.EntityMaintenance.GetTableName(type);
                        tableExists = db.DbMaintenance.IsAnyTable(tableName, false);
                    }
                    catch (Exception ex)
                    {
                         Console.WriteLine(ex.Message );
                    }

                    if (tableExists)
                    {
                        skippedTables++;
                        Console.WriteLine();
                        Console.WriteLine($"? Table already exists (skipped): {tableName} (Type: {type.FullName})");
                    }
                    else
                    {
                        try
                        {
                            db.CodeFirst.InitTables(type);
                            createdTables++;

                            Console.WriteLine();
                            Console.WriteLine($"? Created new table: {tableName} (Type: {type.FullName})");
                        }
                        catch (Exception ex)
                        {
                            errorTables++;
                            Console.WriteLine();
                            Console.WriteLine($"? Error initializing table {tableName}: {ex.Message}");
                        }
                    }
                }

                // Final summary
                Console.WriteLine();
                Console.WriteLine(new string('=', 60));
                Console.WriteLine("Table initialization completed!");
                Console.WriteLine($"Summary:");
                Console.WriteLine($"  � Total tables processed: {totalTables}");
                Console.WriteLine($"  � New tables created: {createdTables}");
                Console.WriteLine($"  � Tables skipped (already exist): {skippedTables}");
                Console.WriteLine($"  � Errors encountered: {errorTables}");
                Console.WriteLine($"  � Success rate: {(double)(createdTables + skippedTables) / totalTables * 100:F1}%");
                Console.WriteLine(new string('=', 60));
            }
        }

        /// <summary>
        /// Alternative InitDb method with manual type list and progress tracking
        /// </summary>
        /// <param name="environment">The web host environment</param>
        /// <param name="additionalTypes">The types to initialize</param>
        /// <param name="forceRecreate">If true, will drop and recreate tables regardless of InitDb setting. Only affects the specified types.</param>
        public static void InitDbWithTypes(this IWebHostEnvironment environment, params Type[] additionalTypes)
        {
            InitDbWithTypes(environment, false, additionalTypes);
        }

        /// <summary>
        /// Alternative InitDb method with manual type list and progress tracking
        /// </summary>
        /// <param name="environment">The web host environment</param>
        /// <param name="forceRecreate">If true, will drop and recreate tables regardless of InitDb setting. Only affects the specified types.</param>
        /// <param name="additionalTypes">The types to initialize</param>
        public static void InitDbWithTypes(this IWebHostEnvironment environment, bool forceRecreate, params Type[] additionalTypes)
        {
            var db = DbScoped.SugarScope;
            var options = App.OptionsSetting;

            // If forceRecreate is true, skip the InitDb check - this allows targeting specific tables only
            // Otherwise, only run if InitDb is true in appsettings
            if (!forceRecreate && !options.InitDb)
            {
                Console.WriteLine("⏭️ InitDbWithTypes skipped - InitDb is false in appsettings");
                Console.WriteLine("   💡 Tip: Use forceRecreate: true to bypass this check and only recreate specified tables");
                return;
            }

            Console.WriteLine($"🔧 InitDbWithTypes called - Environment: {environment.EnvironmentName}");
            if (forceRecreate)
            {
                Console.WriteLine($"🔧 ForceRecreate: true - Will drop and recreate specified tables only (ignores InitDb setting)");
            }
            else
            {
                Console.WriteLine($"🔧 InitDb: {options.InitDb}");
            }

            if (environment.IsDevelopment() || environment.IsProduction())
            {
                // Get all automatic types
                List<Type> entityTypes = new List<Type>();

                // Add any additional manually specified types
                if (additionalTypes != null && additionalTypes.Length > 0)
                {
                    entityTypes.AddRange(additionalTypes);
                }

                // Remove duplicates
                entityTypes = entityTypes.Distinct().ToList();

                Console.WriteLine($"📋 Initializing {entityTypes.Count} tables");
                Console.WriteLine("Starting table initialization...");
                Console.WriteLine(new string('=', 60));

                int totalTables = entityTypes.Count;
                int processedTables = 0;
                int createdTables = 0;
                int recreatedTables = 0;
                int errorTables = 0;

                foreach (var type in entityTypes)
                {
                    processedTables++;

                    // Show current progress
                    var progressPercentage = (double)processedTables / totalTables * 100;
                    var progressBar = CreateProgressBar(progressPercentage);

                    Console.Write($"\r{progressBar} {processedTables}/{totalTables} ({progressPercentage:F1}%) - Processing: {type.Name}".PadRight(80));
                    var tableName = ";";

                    bool tableExists = false;




                    try
                    {
                        tableName = db.EntityMaintenance.GetTableName(type);
                        Console.WriteLine($"\n   Checking table: {tableName} for type: {type.FullName}");
                        tableExists = db.DbMaintenance.IsAnyTable(tableName, false);
                        Console.WriteLine($"   Table exists: {tableExists}");
                    }
                    catch (Exception ex)
                    {
                        // If checking table existence fails, assume it doesn't exist and try to create it
                        Console.WriteLine();
                        Console.WriteLine($"⚠️ Warning checking table {tableName}: {ex.Message}");
                        Console.WriteLine($"   Attempting to create table anyway...");
                        tableExists = false;
                    }
                    if (tableExists)
                    {
                        // DROP AND RECREATE TABLE - This will destroy existing data!
                        try
                        {
                            Console.WriteLine();
                            Console.WriteLine($"⚠️  Dropping existing table: {tableName}");
                            db.DbMaintenance.DropTable(tableName);
                            Console.WriteLine($"   ✓ Table dropped successfully");
                            
                            // Recreate the table
                            db.CodeFirst.InitTables(type);
                            recreatedTables++;
                            Console.WriteLine($"   ✓ Table recreated: {tableName}");
                        }
                        catch (Exception ex)
                        {
                            errorTables++;
                            Console.WriteLine();
                            Console.WriteLine($"✗ Error dropping/recreating table {tableName}: {ex.Message}");
                        }
                    }
                    else
                    {
                        try
                        {
                            db.CodeFirst.InitTables(type);
                            createdTables++;

                            Console.WriteLine();
                            Console.WriteLine($"✓ Created new table: {tableName}");
                        }
                        catch (Exception ex)
                        {
                            errorTables++;
                            Console.WriteLine();
                            Console.WriteLine($"✗ Error creating table {tableName}: {ex.Message}");
                        }
                    }
                }

                // Final summary
                Console.WriteLine();
                Console.WriteLine(new string('=', 60));
                Console.WriteLine("Table initialization completed!");
                Console.WriteLine($"Summary:");
                Console.WriteLine($"  � Total tables processed: {totalTables}");
                Console.WriteLine($"  � New tables created: {createdTables}");
                Console.WriteLine($"  � Tables recreated: {recreatedTables}");
                Console.WriteLine($"  � Errors encountered: {errorTables}");
                Console.WriteLine($"  � Success rate: {(double)(createdTables + recreatedTables) / totalTables * 100:F1}%");
                Console.WriteLine(new string('=', 60));
            }
        }

        /// <summary>
        /// Creates a visual progress bar for console output
        /// </summary>
        private static string CreateProgressBar(double percentage, int width = 30)
        {
            int filledWidth = (int)(percentage / 100.0 * width);
            int emptyWidth = width - filledWidth;

            string filled = new string('�', filledWidth);
            string empty = new string('�', emptyWidth);

            return $"[{filled}{empty}]";
        }

        /// <summary>
        /// Gets all types with [SugarTable] attribute from all loaded assemblies
        /// </summary>
        private static List<Type> GetAllSugarTableTypes()
        {
            var entityTypes = new List<Type>();

            // Get all loaded assemblies
            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.FullName.StartsWith("System") &&
                           !a.FullName.StartsWith("Microsoft") &&
                           !a.FullName.StartsWith("netstandard") &&
                           !a.FullName.StartsWith("mscorlib"));

            foreach (var assembly in assemblies)
            {
                try
                {
                    var types = assembly.GetTypes()
                        .Where(t => t.GetCustomAttribute<SugarTable>() != null &&
                                   !t.IsAbstract &&
                                   t.IsClass)
                        .ToList();

                    entityTypes.AddRange(types);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading types from assembly {assembly.FullName}: {ex.Message}");
                }
            }

            return entityTypes;
        }

        /// <summary>
        /// Gets all types with [SugarTable] attribute from specific assemblies
        /// </summary>
        public static List<Type> GetSugarTableTypesFromAssemblies(params Assembly[] assemblies)
        {
            var entityTypes = new List<Type>();

            foreach (var assembly in assemblies)
            {
                try
                {
                    var types = assembly.GetTypes()
                        .Where(t => t.GetCustomAttribute<SugarTable>() != null &&
                                   !t.IsAbstract &&
                                   t.IsClass)
                        .ToList();

                    entityTypes.AddRange(types);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading types from assembly {assembly.FullName}: {ex.Message}");
                }
            }

            return entityTypes;
        }

        /// <summary>
        /// Gets all types with [SugarTable] attribute from specific namespaces
        /// </summary>
        public static List<Type> GetSugarTableTypesFromNamespaces(params string[] namespaces)
        {
            var entityTypes = new List<Type>();

            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.FullName.StartsWith("System") &&
                           !a.FullName.StartsWith("Microsoft") &&
                           !a.FullName.StartsWith("netstandard") &&
                           !a.FullName.StartsWith("mscorlib"));

            foreach (var assembly in assemblies)
            {
                try
                {
                    var types = assembly.GetTypes()
                        .Where(t => t.GetCustomAttribute<SugarTable>() != null &&
                                   !t.IsAbstract &&
                                   t.IsClass &&
                                   namespaces.Any(ns => t.Namespace != null && t.Namespace.StartsWith(ns)))
                        .ToList();

                    entityTypes.AddRange(types);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading types from assembly {assembly.FullName}: {ex.Message}");
                }
            }

            return entityTypes;
        }
    }
}
