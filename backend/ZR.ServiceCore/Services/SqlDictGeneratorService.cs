using Infrastructure;
using SqlSugar;
using SqlSugar.IOC;
using System.Reflection;
using System.Text;
using ZR.Common;
using ZR.Model;
using ZR.Model.System;

namespace ZR.ServiceCore.Services
{
    /// <summary>
    /// SQL Dictionary Generator Service
    /// Automatically discovers entities with SqlDictAttribute and generates dictionary types with SQL queries
    /// </summary>
    public class SqlDictGeneratorService
    {
        /// <summary>
        /// Discovers all entities with SqlDictAttribute and returns them with their metadata
        /// </summary>
        public List<(Type EntityType, SqlDictAttribute Attribute, string TableName)> DiscoverSqlDictEntities()
        {
            var results = new List<(Type, SqlDictAttribute, string)>();
            var db = DbScoped.SugarScope;

            try
            {
                // Get all assemblies that might contain entities
                var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(a => !a.FullName.StartsWith("System") &&
                               !a.FullName.StartsWith("Microsoft") &&
                               (a.FullName.StartsWith("ZR.") || a.FullName.Contains("Model")))
                    .ToList();

                foreach (var assembly in assemblies)
                {
                    try
                    {
                        var types = assembly.GetTypes()
                            .Where(t => t.GetCustomAttribute<SugarTable>() != null &&
                                       t.GetCustomAttribute<SqlDictAttribute>() != null &&
                                       !t.IsAbstract &&
                                       t.IsClass &&
                                       !t.IsGenericType)
                            .ToList();

                        foreach (var type in types)
                        {
                            var sqlDictAttr = type.GetCustomAttribute<SqlDictAttribute>();
                            if (sqlDictAttr != null && sqlDictAttr.IsValid())
                            {
                                var tableName = db.EntityMaintenance.GetTableName(type);
                                results.Add((type, sqlDictAttr, tableName));
                                Console.WriteLine($"Discovered SqlDict entity: {type.Name} -> {tableName}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error loading types from assembly {assembly.FullName}: {ex.Message}");
                    }
                }

                Console.WriteLine($"Total SqlDict entities discovered: {results.Count}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in DiscoverSqlDictEntities: {ex.Message}");
            }

            return results;
        }

        /// <summary>
        /// Generates SQL query for a given entity based on SqlDictAttribute
        /// </summary>
        public string GenerateSqlQuery(string tableName, SqlDictAttribute attr)
        {
            // If custom SQL is provided, use it
            if (!string.IsNullOrEmpty(attr.CustomSql))
            {
                return attr.CustomSql.Replace("{tableName}", tableName);
            }

            var sql = new StringBuilder();
            sql.AppendLine("SELECT");

            // Value column
            sql.AppendLine($"    {attr.ValueColumn} AS dictValue,");

            // Label column(s) - can be a single column or CONCAT expression
            if (attr.LabelColumns.Contains(",") || attr.LabelColumns.Contains("'"))
            {
                // It's a CONCAT expression
                sql.AppendLine($"    CONCAT({attr.LabelColumns}) AS dictLabel,");
            }
            else
            {
                // Single column
                sql.AppendLine($"    {attr.LabelColumns} AS dictLabel,");
            }

            // Remark column (optional)
            if (!string.IsNullOrEmpty(attr.RemarkColumn))
            {
                sql.AppendLine($"    {attr.RemarkColumn} AS remark,");
            }

            // DictType constant
            sql.AppendLine($"    '{attr.GetPrefixedDictType()}' AS dictType");

            // FROM clause
            sql.AppendLine($"FROM {tableName}");

            // WHERE clause (optional)
            if (!string.IsNullOrEmpty(attr.WhereClause))
            {
                sql.AppendLine($"WHERE {attr.WhereClause}");
            }

            // ORDER BY clause (optional)
            if (!string.IsNullOrEmpty(attr.OrderBy))
            {
                sql.AppendLine($"ORDER BY {attr.OrderBy}");
            }
            else
            {
                // Default ordering by value column
                sql.AppendLine($"ORDER BY {attr.ValueColumn}");
            }

            return sql.ToString().TrimEnd();
        }

        /// <summary>
        /// Creates SysDictType from SqlDictAttribute
        /// </summary>
        public SysDictType CreateDictType(SqlDictAttribute attr, string sqlQuery)
        {
            return new SysDictType
            {
                DictName = attr.DictName,
                DictType = attr.GetPrefixedDictType(),
                Status = attr.Status,
                Type = "Y", // Y indicates SQL-based dictionary
                CustomSql = sqlQuery,
                Remark = attr.Description ?? $"Auto-generated from entity with SqlDictAttribute",
                Create_by = "system",
                Create_time = DateTime.Now
            };
        }

        /// <summary>
        /// Checks if a dictionary type already exists
        /// </summary>
        public bool DictTypeExists(string dictType)
        {
            var db = DbScoped.SugarScope;
            return db.Queryable<SysDictType>()
                .Any(d => d.DictType == dictType);
        }

        /// <summary>
        /// Saves or updates a dictionary type
        /// </summary>
        public (bool Success, string Message) SaveDictType(SysDictType dictType)
        {
            try
            {
                var db = DbScoped.SugarScope;

                var existing = db.Queryable<SysDictType>()
                    .First(d => d.DictType == dictType.DictType);

                if (existing != null)
                {
                    // Update existing
                    existing.DictName = dictType.DictName;
                    existing.CustomSql = dictType.CustomSql;
                    existing.Status = dictType.Status;
                    existing.Remark = dictType.Remark;
                    existing.Update_by = "system";
                    existing.Update_time = DateTime.Now;

                    db.Updateable(existing)
                        .IgnoreColumns(it => new { it.Create_by, it.Create_time })
                        .ExecuteCommand();

                    return (true, $"Updated existing dictionary type: {dictType.DictType}");
                }
                else
                {
                    // Insert new
                    db.Insertable(dictType).ExecuteCommand();
                    return (true, $"Created new dictionary type: {dictType.DictType}");
                }
            }
            catch (Exception ex)
            {
                return (false, $"Error saving dictionary type {dictType.DictType}: {ex.Message}");
            }
        }

        /// <summary>
        /// Generates and saves all SQL dictionaries from discovered entities
        /// </summary>
        public List<string> GenerateAndSaveAllSqlDictionaries()
        {
            var results = new List<string>();
            results.Add("=== SQL Dictionary Generation Started ===");

            try
            {
                // Discover all entities with SqlDictAttribute
                var entities = DiscoverSqlDictEntities();

                if (entities.Count == 0)
                {
                    results.Add("No entities found with SqlDictAttribute");
                    return results;
                }

                results.Add($"Found {entities.Count} entities with SqlDictAttribute");
                results.Add("");

                int createdCount = 0;
                int updatedCount = 0;
                int errorCount = 0;

                foreach (var (entityType, attr, tableName) in entities)
                {
                    try
                    {
                        // Generate SQL query
                        var sqlQuery = GenerateSqlQuery(tableName, attr);

                        // Create dictionary type
                        var dictType = CreateDictType(attr, sqlQuery);

                        // Check if it exists
                        bool exists = DictTypeExists(dictType.DictType);

                        // Save or update
                        var (success, message) = SaveDictType(dictType);

                        if (success)
                        {
                            if (exists)
                                updatedCount++;
                            else
                                createdCount++;

                            results.Add($"✓ {entityType.Name}: {message}");
                            results.Add($"  SQL: {sqlQuery.Replace(Environment.NewLine, " ").Substring(0, Math.Min(100, sqlQuery.Length))}...");
                        }
                        else
                        {
                            errorCount++;
                            results.Add($"✗ {entityType.Name}: {message}");
                        }
                    }
                    catch (Exception ex)
                    {
                        errorCount++;
                        results.Add($"✗ {entityType.Name}: Error - {ex.Message}");
                    }

                    results.Add("");
                }

                results.Add("=== Summary ===");
                results.Add($"Created: {createdCount}");
                results.Add($"Updated: {updatedCount}");
                results.Add($"Errors: {errorCount}");
                results.Add($"Total: {entities.Count}");
            }
            catch (Exception ex)
            {
                results.Add($"Fatal error in GenerateAndSaveAllSqlDictionaries: {ex.Message}");
            }

            return results;
        }

        /// <summary>
        /// Generates SQL dictionaries only for specific entity types
        /// </summary>
        public List<string> GenerateSqlDictionariesForTypes(params Type[] entityTypes)
        {
            var results = new List<string>();
            results.Add("=== SQL Dictionary Generation for Specific Types ===");

            var db = DbScoped.SugarScope;

            foreach (var entityType in entityTypes)
            {
                try
                {
                    var sqlDictAttr = entityType.GetCustomAttribute<SqlDictAttribute>();

                    if (sqlDictAttr == null)
                    {
                        results.Add($"✗ {entityType.Name}: No SqlDictAttribute found");
                        continue;
                    }

                    if (!sqlDictAttr.IsValid())
                    {
                        results.Add($"✗ {entityType.Name}: Invalid SqlDictAttribute configuration");
                        continue;
                    }

                    var tableName = db.EntityMaintenance.GetTableName(entityType);
                    var sqlQuery = GenerateSqlQuery(tableName, sqlDictAttr);
                    var dictType = CreateDictType(sqlDictAttr, sqlQuery);

                    var (success, message) = SaveDictType(dictType);
                    results.Add($"{(success ? "✓" : "✗")} {entityType.Name}: {message}");
                }
                catch (Exception ex)
                {
                    results.Add($"✗ {entityType.Name}: Error - {ex.Message}");
                }
            }

            return results;
        }

        /// <summary>
        /// Validates SQL query syntax (basic validation)
        /// </summary>
        public (bool IsValid, string Message) ValidateSqlQuery(string sql)
        {
            if (string.IsNullOrWhiteSpace(sql))
                return (false, "SQL query is empty");

            var upperSql = sql.Trim().ToUpper();

            if (!upperSql.StartsWith("SELECT"))
                return (false, "SQL must start with SELECT");

            if (!upperSql.Contains("DICTVALUE") || !upperSql.Contains("DICTLABEL") || !upperSql.Contains("DICTTYPE"))
                return (false, "SQL must include dictValue, dictLabel, and dictType columns");

            // Basic SQL injection protection
            var dangerousKeywords = new[] { "DROP", "DELETE", "TRUNCATE", "INSERT", "UPDATE", "ALTER", "CREATE", "EXEC", "EXECUTE" };
            foreach (var keyword in dangerousKeywords)
            {
                if (upperSql.Contains(keyword))
                    return (false, $"SQL contains potentially dangerous keyword: {keyword}");
            }

            return (true, "SQL query is valid");
        }

        /// <summary>
        /// Tests SQL query execution and returns sample data
        /// </summary>
        public (bool Success, string Message, List<dynamic> SampleData) TestSqlQuery(string sql, int sampleSize = 5)
        {
            try
            {
                var (isValid, validationMessage) = ValidateSqlQuery(sql);
                if (!isValid)
                    return (false, validationMessage, null);

                var db = DbScoped.SugarScope;

                // Execute query with limit
                var limitedSql = $"{sql} LIMIT {sampleSize}";
                var results = db.Ado.SqlQuery<dynamic>(limitedSql);

                return (true, $"Query executed successfully. Returned {results.Count} rows.", results);
            }
            catch (Exception ex)
            {
                return (false, $"Query execution failed: {ex.Message}", null);
            }
        }
    }
}

