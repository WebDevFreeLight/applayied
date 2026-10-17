using SqlSugar;
using SqlSugar.IOC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ZR.Model;
using ZR.Model.Content;
using ZR.Model.Models;
using ZR.Model.System.Public;
using ZR.Model.social;
using ZR.Model.System;
using ZR.Model.System.Generate;
using ZR.Model.System.Model;

namespace ZR.ServiceCore.SqlSugar
{
    /// <summary>
    /// Initialize tables
    /// </summary>
    public class InitTable
    {
        /// <summary>
        /// Create database and tables
        /// </summary>
        public static void InitDb(bool init)
        {
            var db = DbScoped.SugarScope;
            
            if (!init) return;
            
            StaticConfig.CodeFirst_MySqlCollate = "utf8mb4_general_ci";
            
            // Create database: If it doesn't exist, create the database; if it exists, it won't be created again
            //db.DbMaintenance.CreateDatabase();// Note: Oracle and some domestic databases do not support this method and require manual database creation 

            // Automatically discover and initialize all tables with [SugarTable] attribute
            var entityTypes = GetAllSugarTableTypes();
            
            Console.WriteLine($"Found {entityTypes.Count} entities with [SugarTable] attribute");

            // Initialize tables in batches to avoid performance issues
            var batches = entityTypes.Batch(10);







            foreach (var batch in entityTypes)
            {
                try
                {





                    //var  entityTypes1 = batch.ToArray();

                    InitSpecificTables(batch);

                    //db.CodeFirst.InitTables(entityTypes1);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error initializing batch of tables: {ex.Message}");
                    // Try to initialize tables one by one if batch fails
                    //foreach (var type in batch)
                    //{
                    //    try
                    //    {
                    //        // Check if table exists before creating
                    //        var tableName = db.EntityMaintenance.GetTableName(type);
                    //        var exists = db.DbMaintenance.IsAnyTable(tableName, false);
                            
                    //        if (!exists)
                    //        {
                    //            db.CodeFirst.InitTables(type);
                    //            Console.WriteLine($"Created new table: {tableName}");
                    //        }
                    //        else
                    //        {
                    //            Console.WriteLine($"Table already exists: {tableName}");
                    //        }
                    //    }
                    //    catch (Exception innerEx)
                    //    {
                    //        Console.WriteLine($"Error initializing table for {type.Name}: {innerEx.Message}");
                    //    }
                    //}
                }
            }
        }

        /// <summary>
        /// Gets all types with [SugarTable] attribute from all loaded assemblies
        /// </summary>
        public static List<Type> GetAllSugarTableTypes()
        {
            var entityTypes = new List<Type>();

            // Get all loaded assemblies, filtering out system assemblies
            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.FullName.StartsWith("System") && 
                           !a.FullName.StartsWith("Microsoft") &&
                           !a.FullName.StartsWith("netstandard") &&
                           !a.FullName.StartsWith("mscorlib") &&
                           !a.FullName.StartsWith("WindowsBase") &&
                           !a.FullName.StartsWith("PresentationCore") &&
                           !a.FullName.StartsWith("PresentationFramework") &&
                           (a.FullName.StartsWith("ZR.") || a.FullName.Contains("Model")));

            foreach (var assembly in assemblies)
            {
                try
                {
                    var types = assembly.GetTypes()
                        .Where(t => t.GetCustomAttribute<SugarTable>() != null && 
                                   !t.IsAbstract && 
                                   t.IsClass &&
                                   !t.IsGenericType)
                        .ToList();

                    entityTypes.AddRange(types);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading types from assembly {assembly.FullName}: {ex.Message}");
                }
            }

            // Remove duplicates (in case of multiple definitions)
            entityTypes = entityTypes.Distinct().ToList();

            return entityTypes;
        }

        /// <summary>
        /// Gets all types with [SugarTable] attribute from specific namespaces
        /// </summary>
        public static List<Type> GetSugarTableTypesFromNamespaces(params string[] namespaces)
        {
            var allTypes = GetAllSugarTableTypes();
            
            if (namespaces == null || namespaces.Length == 0)
                return allTypes;
            
            return allTypes.Where(t => 
                t.Namespace != null && 
                namespaces.Any(ns => t.Namespace.StartsWith(ns)))
                .ToList();
        }

        /// <summary>
        /// Initialize specific tables by namespace
        /// </summary>
        public static void InitDbWithNamespaces(bool init, params string[] namespaces)
        {
            if (!init) return;
            
            var db = DbScoped.SugarScope;
            var entityTypes = GetSugarTableTypesFromNamespaces(namespaces);
            
            Console.WriteLine($"Found {entityTypes.Count} entities in namespaces: {string.Join(", ", namespaces)}");


            var types = entityTypes.ToArray();

            InitSpecificTables(types);

            // Remove this redundant loop since InitSpecificTables already handles all types
            // The InitSpecificTables method above already processes all types with existence check
        }

        /// <summary>
        /// Initialize a new table if it doesn't exist
        /// </summary>
        public static void InitNewTb()
        {
            var db = DbScoped.SugarScope;

            //db.DbMaintenance.DropTable(typeof(UserOnlineLog));


            try
            {
                var t1 = db.DbMaintenance.IsAnyTable(typeof(UserOnlineLog).Name);


                if (!t1)
                {
                    db.CodeFirst.InitTables(typeof(UserOnlineLog));
                }
            }
            catch (Exception ex)
            {


                Console.WriteLine(ex.Message);





            }
        }

        /// <summary>
        /// Initialize specific types
        /// </summary>
        public static void InitSpecificTables(params Type[] types)
        {
            var db = DbScoped.SugarScope;











            foreach (var type in types)
            {
                try
                {
                    var tableName = db.EntityMaintenance.GetTableName(type);

                    if (db.DbMaintenance.IsAnyTable(tableName, false))
                    {
                        db.DbMaintenance.DropTable(tableName);
                        Console.WriteLine($"Dropped table: {tableName}");
                    }

                    db.CodeFirst.InitTables(type);
                    Console.WriteLine($"Recreated table: {tableName}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error recreating table for {type.Name}: {ex.Message}");
                }
            }


            return;



            foreach (var type in types)
            {
                try
                {
                    var tableName = db.EntityMaintenance.GetTableName(type);
                    var exists = db.DbMaintenance.IsAnyTable(tableName, false);
                    //var exists = false;
                    Console.WriteLine($"table: {tableName}");


                    if (exists)
                    {
                        db.DbMaintenance.DropTable(type);


                        db.CodeFirst.InitTables(type);
                        Console.WriteLine($"Created new table: {tableName}");
                    }
                    else
                    {

                        db.CodeFirst.InitTables(type);

                        Console.WriteLine($"Table already exists: {tableName}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error initializing table for {type.Name}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Drop and recreate tables (use with caution!)
        /// </summary>
        public static void RecreateTablesInNamespace(string namespaceName)
        {
            var db = DbScoped.SugarScope;
            var types = GetSugarTableTypesFromNamespaces(namespaceName);
            
            foreach (var type in types)
            {
                try
                {
                    var tableName = db.EntityMaintenance.GetTableName(type);
                    
                    if (db.DbMaintenance.IsAnyTable(tableName, false))
                    {
                        db.DbMaintenance.DropTable(tableName);
                        Console.WriteLine($"Dropped table: {tableName}");
                    }
                    
                    db.CodeFirst.InitTables(type);
                    Console.WriteLine($"Recreated table: {tableName}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error recreating table for {type.Name}: {ex.Message}");
                }
            }
        }
        /// <summary>
        /// Initialize all tables with existence check
        /// </summary>
        public static void InitAllTables()
        {
            InitDb(true);
        }

        /// <summary>
        /// Initialize tables from specific namespaces with existence check
        /// </summary>
        public static void InitTablesFromNamespaces(params string[] namespaces)
        {
            InitDbWithNamespaces(true, namespaces);
        }

        /// <summary>
        /// Check if a table exists in the database
        /// </summary>
        public static bool IsTableExists(Type entityType)
        {
            var db = DbScoped.SugarScope;
            var tableName = db.EntityMaintenance.GetTableName(entityType);
            return db.DbMaintenance.IsAnyTable(tableName, false);
        }

        /// <summary>
        /// Get list of missing tables from specified namespaces
        /// </summary>
        public static List<string> GetMissingTables(params string[] namespaces)
        {
            var db = DbScoped.SugarScope;
            var types = namespaces?.Length > 0 
                ? GetSugarTableTypesFromNamespaces(namespaces) 
                : GetAllSugarTableTypes();
            
            var missingTables = new List<string>();
            
            foreach (var type in types)
            {
                try
                {
                    var tableName = db.EntityMaintenance.GetTableName(type);
                    if (!db.DbMaintenance.IsAnyTable(tableName, false))
                    {
                        missingTables.Add(tableName);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error checking table for {type.Name}: {ex.Message}");
                }
            }
            
            return missingTables;
        }

        /// <summary>
        /// Initialize only missing tables
        /// </summary>
        public static void InitMissingTables(params string[] namespaces)
        {
            var db = DbScoped.SugarScope;
            var types = namespaces?.Length > 0 
                ? GetSugarTableTypesFromNamespaces(namespaces) 
                : GetAllSugarTableTypes();
            
            int createdCount = 0;
            int existingCount = 0;
            
            foreach (var type in types)
            {
                try
                {
                    var tableName = db.EntityMaintenance.GetTableName(type);
                    var exists = db.DbMaintenance.IsAnyTable(tableName, false);
                    
                    if (!exists)
                    {
                        db.CodeFirst.InitTables(type);
                        Console.WriteLine($"? Created new table: {tableName}");
                        createdCount++;
                    }
                    else
                    {
                        existingCount++;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"? Error initializing table for {type.Name}: {ex.Message}");
                }
            }
            
            Console.WriteLine($"\nTable initialization complete:");
            Console.WriteLine($"  - Created: {createdCount} tables");
            Console.WriteLine($"  - Existing: {existingCount} tables");
            Console.WriteLine($"  - Total: {createdCount + existingCount} tables");
        }
    }

    /// <summary>
    /// Extension methods for batching
    /// </summary>
    public static class BatchExtensions
    {
        public static IEnumerable<IEnumerable<T>> Batch<T>(this IEnumerable<T> source, int batchSize)
        {
            using (var enumerator = source.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    yield return YieldBatchElements(enumerator, batchSize - 1);
                }
            }
        }

        private static IEnumerable<T> YieldBatchElements<T>(IEnumerator<T> source, int batchSize)
        {
            yield return source.Current;
            for (int i = 0; i < batchSize && source.MoveNext(); i++)
            {
                yield return source.Current;
            }
        }
    }
}

