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
    /// Automatic SQL Dictionary Generator Service
    /// Discovers ALL SugarTable entities and automatically generates SQL dictionaries
    /// by intelligently analyzing column names and types
    /// </summary>
    public class AutoSqlDictGeneratorService
    {
        /// <summary>
        /// Column name patterns for identifying value columns (code/ID fields)
        /// </summary>
        private readonly string[] ValueColumnPatterns = new[]
        {
            "code", "typecode", "statuscode", "id", "key", "value"
        };

        /// <summary>
        /// Column name patterns for identifying label columns (name/title fields)
        /// </summary>
        private readonly string[] LabelColumnPatterns = new[]
        {
            "name", "typename", "title", "label", "description", "desc"
        };

        /// <summary>
        /// Column name patterns for Arabic name columns
        /// </summary>
        private readonly string[] ArabicColumnPatterns = new[]
        {
            "namear", "typenamear", "titlear", "descriptionar", "namearabic", "arabicname"
        };

        /// <summary>
        /// Column name patterns for remark/description columns
        /// </summary>
        private readonly string[] RemarkColumnPatterns = new[]
        {
            "remark", "remarks", "description", "desc", "notes", "note", "comment"
        };

        /// <summary>
        /// Column name patterns for display order columns
        /// </summary>
        private readonly string[] OrderColumnPatterns = new[]
        {
            "displayorder", "sortorder", "orderby", "sort", "sequence", "position"
        };

        /// <summary>
        /// Table name patterns to exclude from dictionary generation
        /// </summary>
        private readonly string[] ExcludedTablePatterns = new[]
        {
            "sys_", "log_", "audit_", "temp_", "tmp_"
        };

        /// <summary>
        /// Entity class representing discovered SugarTable with analyzed columns
        /// </summary>
        public class DiscoveredEntity
        {
            public Type EntityType { get; set; }
            public string TableName { get; set; }
            public string DictType { get; set; }
            public string DictName { get; set; }
            public string ValueColumn { get; set; }
            public List<string> LabelColumns { get; set; }
            public string LabelExpression { get; set; }
            public string RemarkColumn { get; set; }
            public string OrderColumn { get; set; }
            public List<PropertyInfo> AllProperties { get; set; }
            public bool HasArabicColumn { get; set; }
            public string GeneratedSql { get; set; }
        }

        /// <summary>
        /// Discovers all SugarTable entities across all assemblies
        /// </summary>
        public List<Type> DiscoverAllSugarTableEntities()
        {
            var entityTypes = new List<Type>();

            try
            {
                var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(a => !a.FullName.StartsWith("System") &&
                               !a.FullName.StartsWith("Microsoft") &&
                               (a.FullName.StartsWith("ZR.") || a.FullName.Contains("Model")))
                    .ToList();

                Console.WriteLine($"Scanning {assemblies.Count} assemblies for SugarTable entities...");


                //assemblies = assemblies.Where(t => t.FullName.Contains("ZR.Mall")).ToList();
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
                        Console.WriteLine($"  {assembly.GetName().Name}: Found {types.Count} entities");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"  Error loading types from {assembly.FullName}: {ex.Message}");
                    }
                }

                Console.WriteLine($"Total entities discovered: {entityTypes.Count}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in DiscoverAllSugarTableEntities: {ex.Message}");
            }

            return entityTypes.Distinct().ToList();
        }

        /// <summary>
        /// Analyzes an entity to determine the best columns for dictionary generation
        /// </summary>
        public DiscoveredEntity AnalyzeEntity(Type entityType)
        {
            var db = DbScoped.SugarScope;
            var tableName = db.EntityMaintenance.GetTableName(entityType);

            // Check if table should be excluded
            if (ExcludedTablePatterns.Any(pattern => tableName.ToLower().StartsWith(pattern)))
            {
                return null;
            }

            var discovered = new DiscoveredEntity
            {
                EntityType = entityType,
                TableName = tableName,
                AllProperties = entityType.GetProperties()
                    .Where(p => p.GetCustomAttribute<SugarColumn>()?.IsIgnore != true)
                    .ToList(),
                LabelColumns = new List<string>()
            };

            // Check if entity has SqlDict attribute
            var sqlDictAttr = entityType.GetCustomAttribute<SqlDictAttribute>();

            // Find value column (code/ID field)
            // If SqlDict attribute exists and has ValueColumn, use it; otherwise search
            string v = FindValueColumn(discovered.AllProperties, sqlDictAttr);
            discovered.ValueColumn = v;

            // Find label columns (name fields)
            var labelCols = FindLabelColumns(discovered.AllProperties, sqlDictAttr);
            discovered.LabelColumns = labelCols;
            discovered.HasArabicColumn = labelCols.Any(c => ArabicColumnPatterns.Any(p => c.ToLower().Contains(p)));

            // Build label expression
            discovered.LabelExpression = BuildLabelExpression(discovered.ValueColumn, labelCols);

            // Find remark column
            discovered.RemarkColumn = FindRemarkColumn(discovered.AllProperties, sqlDictAttr);

            // Find order column
            discovered.OrderColumn = FindOrderColumn(discovered.AllProperties, sqlDictAttr);

            // Generate dict type from table name
            discovered.DictType = GenerateDictType(tableName);

            // Generate dict name
            discovered.DictName = GenerateDictName(entityType.Name, discovered.HasArabicColumn);

            // Generate SQL
            discovered.GeneratedSql = GenerateSqlQuery(discovered);

            return discovered;
        }

        /// <summary>
        /// Finds the best column to use as dictValue (code/ID field)
        /// Checks SqlDict attribute first, then searches if not defined
        /// </summary>
        private string FindValueColumn(List<PropertyInfo> properties, SqlDictAttribute sqlDictAttr = null)
        {
            // Priority 0: If SqlDict attribute exists and has ValueColumn defined, use it
            if (sqlDictAttr != null && !string.IsNullOrEmpty(sqlDictAttr.ValueColumn))
            {
                Console.WriteLine($"  Using ValueColumn from SqlDict attribute: {sqlDictAttr.ValueColumn}");
                return sqlDictAttr.ValueColumn.ToLower();
            }

            // Priority 1: Look for column explicitly named "Value" or contains "Value"
            var valueColumn = properties.FirstOrDefault(p =>
                p.Name.ToLower().Contains("value") &&
                !p.Name.ToLower().Contains("name") &&
                p.GetCustomAttribute<SugarColumn>()?.IsIgnore != true);

            if (valueColumn != null)
                return valueColumn.Name.ToLower();

            // Priority 2: Look for "Code" columns (typecode, statuscode, code)
            var codePatterns = new[] { "typecode", "statuscode", "code" };
            foreach (var pattern in codePatterns)
            {
                var match = properties.FirstOrDefault(p =>
                    p.Name.ToLower().Contains(pattern) &&
                    !p.Name.ToLower().Contains("name") &&
                    p.GetCustomAttribute<SugarColumn>()?.IsIgnore != true);

                if (match != null)
                    return match.Name.ToLower();
            }

            // Priority 3: Look for "ID" or "Key" columns
            var idPatterns = new[] { "id", "key" };
            foreach (var pattern in idPatterns)
            {
                var match = properties.FirstOrDefault(p =>
                    p.Name.ToLower().Contains(pattern) &&
                    !p.Name.ToLower().Contains("name") &&
                    p.GetCustomAttribute<SugarColumn>()?.IsIgnore != true &&
                    p.GetCustomAttribute<SugarColumn>()?.IsPrimaryKey != true);

                if (match != null)
                    return match.Name.ToLower();
            }

            // Priority 4: Look for columns matching ALL ValueColumnPatterns (fallback)
            foreach (var pattern in ValueColumnPatterns)
            {
                var match = properties.FirstOrDefault(p =>
                    p.Name.ToLower().Contains(pattern) &&
                    !p.Name.ToLower().Contains("name") &&
                    p.GetCustomAttribute<SugarColumn>()?.IsIgnore != true);

                if (match != null)
                    return match.Name.ToLower();
            }

            // Priority 5: Use primary key if it's not an identity column
            var primaryKey = properties.FirstOrDefault(p =>
            {
                var sugarCol = p.GetCustomAttribute<SugarColumn>();
                return sugarCol?.IsPrimaryKey == true && sugarCol?.IsIdentity != true;
            });

            if (primaryKey != null)
                return primaryKey.Name.ToLower();

            // Priority 6: Use first non-base column
            var firstColumn = properties.FirstOrDefault(p =>
                !p.Name.ToLower().StartsWith("create") &&
                !p.Name.ToLower().StartsWith("update") &&
                !p.Name.ToLower().Contains("time") &&
                !p.Name.ToLower().Contains("by") &&
                p.GetCustomAttribute<SugarColumn>()?.IsIgnore != true);

            return firstColumn?.Name.ToLower() ?? properties.First().Name.ToLower();
        }

        /// <summary>
        /// Finds columns to use for dictLabel (name/title fields)
        /// Checks SqlDict attribute first, then searches if not defined
        /// </summary>
        private List<string> FindLabelColumns(List<PropertyInfo> properties, SqlDictAttribute sqlDictAttr = null)
        {
            var labelColumns = new List<string>();

            // Priority 0: If SqlDict attribute exists and has LabelColumns defined, parse and use it
            if (sqlDictAttr != null && !string.IsNullOrEmpty(sqlDictAttr.LabelColumns))
            {
                Console.WriteLine($"  Using LabelColumns from SqlDict attribute: {sqlDictAttr.LabelColumns}");

                // Parse the LabelColumns expression to extract actual column names
                // Example: "typecode, ' - ', typename, ' - ', typenamear"
                var parts = sqlDictAttr.LabelColumns
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(p => p.Trim().Trim('\'', ' ', '-'))
                    .Where(p => !string.IsNullOrEmpty(p) && !p.Contains("' '") && p != "-")
                    .ToList();

                return parts.Select(p => p.ToLower()).ToList();
            }

            // Automatic search if not defined in attribute
            // Find English name column
            var englishName = properties.FirstOrDefault(p =>
                LabelColumnPatterns.Any(pattern => p.Name.ToLower().Contains(pattern)) &&
                !ArabicColumnPatterns.Any(ar => p.Name.ToLower().Contains(ar)));

            if (englishName != null)
                labelColumns.Add(englishName.Name.ToLower());

            // Find Arabic name column
            var arabicName = properties.FirstOrDefault(p =>
                ArabicColumnPatterns.Any(pattern => p.Name.ToLower().Contains(pattern)));

            if (arabicName != null)
                labelColumns.Add(arabicName.Name.ToLower());

            // If no label columns found, use first string column
            if (labelColumns.Count == 0)
            {
                var firstString = properties.FirstOrDefault(p =>
                    p.PropertyType == typeof(string) &&
                    !p.Name.ToLower().StartsWith("create") &&
                    !p.Name.ToLower().StartsWith("update"));

                if (firstString != null)
                    labelColumns.Add(firstString.Name.ToLower());
            }

            return labelColumns;
        }

        /// <summary>
        /// Builds CONCAT expression for label columns
        /// </summary>
        private string BuildLabelExpression(string valueColumn, List<string> labelColumns)
        {
            if (labelColumns.Count == 0)
                return valueColumn;

            if (labelColumns.Count == 1)
                return labelColumns[0];

            // Build: "valueColumn, ' - ', name1, ' - ', name2"
            var parts = new List<string> { valueColumn };
            foreach (var col in labelColumns)
            {
                parts.Add("' - '");
                parts.Add(col);
            }

            return string.Join(", ", parts);
        }

        /// <summary>
        /// Finds remark/description column
        /// Checks SqlDict attribute first, then searches if not defined
        /// </summary>
        private string FindRemarkColumn(List<PropertyInfo> properties, SqlDictAttribute sqlDictAttr = null)
        {
            // Priority 0: If SqlDict attribute exists and has RemarkColumn defined, use it
            if (sqlDictAttr != null && !string.IsNullOrEmpty(sqlDictAttr.RemarkColumn))
            {
                Console.WriteLine($"  Using RemarkColumn from SqlDict attribute: {sqlDictAttr.RemarkColumn}");
                return sqlDictAttr.RemarkColumn.ToLower();
            }

            // Automatic search if not defined in attribute
            var remark = properties.FirstOrDefault(p =>
                RemarkColumnPatterns.Any(pattern => p.Name.ToLower().Contains(pattern)));

            return remark?.Name.ToLower();
        }

        /// <summary>
        /// Finds display order column
        /// Checks SqlDict attribute first, then searches if not defined
        /// </summary>
        private string FindOrderColumn(List<PropertyInfo> properties, SqlDictAttribute sqlDictAttr = null)
        {
            // Priority 0: If SqlDict attribute exists and has OrderBy defined, extract column name
            if (sqlDictAttr != null && !string.IsNullOrEmpty(sqlDictAttr.OrderBy))
            {
                Console.WriteLine($"  Using OrderBy from SqlDict attribute: {sqlDictAttr.OrderBy}");

                // OrderBy may contain "column ASC" or "column DESC" or just "column"
                // Extract just the column name
                var orderByParts = sqlDictAttr.OrderBy.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                var columnName = orderByParts[0].Trim().ToLower();

                return columnName;
            }

            // Automatic search if not defined in attribute
            var order = properties.FirstOrDefault(p =>
                OrderColumnPatterns.Any(pattern => p.Name.ToLower().Contains(pattern)));

            return order?.Name.ToLower() ?? null;
        }

        /// <summary>
        /// Generates dict type from table name (prefixed with sql_)
        /// </summary>
        private string GenerateDictType(string tableName)
        {
            // Remove common prefixes
            var cleanName = tableName.ToLower()
                .Replace("event_", "")
                .Replace("hr_", "")
                .Replace("sys_", "")
                .Replace("_types", "_type")
                .Replace("types", "type");

            // Remove trailing 's' if it's a plural table name
            if (cleanName.EndsWith("s") && !cleanName.EndsWith("ss") && !cleanName.EndsWith("status"))
                cleanName = cleanName.Substring(0, cleanName.Length - 1);

            return cleanName;
        }

        /// <summary>
        /// Generates user-friendly dict name
        /// </summary>
        private string GenerateDictName(string entityName, bool hasBilingual)
        {
            // Convert PascalCase to Title Case with spaces
            var withSpaces = System.Text.RegularExpressions.Regex.Replace(entityName, "([A-Z])", " $1").Trim();

            if (hasBilingual)
                return $"{withSpaces} - أنواع";

            return withSpaces;
        }

        /// <summary>
        /// Generates complete SQL query for the entity
        /// </summary>
        private string GenerateSqlQuery(DiscoveredEntity entity)
        {
            var sql = new StringBuilder();
            sql.AppendLine("SELECT");

            // Value column
            sql.AppendLine($"    {entity.ValueColumn} AS dictValue,");

            // Label column(s)
            if (entity.LabelColumns.Count > 1 || entity.LabelExpression.Contains("' - '"))
            {
                sql.AppendLine($"    CONCAT({entity.LabelExpression}) AS dictLabel,");
            }
            else
            {
                sql.AppendLine($"    {entity.LabelExpression} AS dictLabel,");
            }

            // Remark column (optional)
            if (!string.IsNullOrEmpty(entity.RemarkColumn))
            {
                sql.AppendLine($"    {entity.RemarkColumn} AS remark,");
            }

            // DictType constant
            sql.AppendLine($"    'sql_{entity.DictType}' AS dictType");

            // FROM clause
            sql.AppendLine($"FROM {entity.TableName}");

            // ORDER BY clause
            if (!string.IsNullOrEmpty(entity.OrderColumn))
            {
                sql.AppendLine($"ORDER BY {entity.OrderColumn}");
            }
            else
            {
                sql.AppendLine($"ORDER BY {entity.ValueColumn}");
            }

            return sql.ToString().TrimEnd();
        }

        /// <summary>
        /// Analyzes all discovered entities
        /// </summary>
        public List<DiscoveredEntity> AnalyzeAllEntities()
        {
            Console.WriteLine("=== Starting Entity Analysis ===");
            var entities = DiscoverAllSugarTableEntities();
            var analyzed = new List<DiscoveredEntity>();

            foreach (var entityType in entities)
            {
                try
                {
                    var discovered = AnalyzeEntity(entityType);
                    if (discovered != null)
                    {
                        analyzed.Add(discovered);
                        Console.WriteLine($"✓ Analyzed: {entityType.Name} -> sql_{discovered.DictType}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"✗ Error analyzing {entityType.Name}: {ex.Message}");
                }
            }

            Console.WriteLine($"=== Analysis Complete: {analyzed.Count} entities ready ===");
            return analyzed;
        }

        /// <summary>
        /// Generates and saves SQL dictionaries for all discovered entities
        /// </summary>
        public List<string> GenerateAndSaveAllDiscoveredDictionaries()
        {
            var results = new List<string>();
            results.Add("=== Automatic SQL Dictionary Generation (All SugarTable Entities) ===");
            results.Add("");

            try
            {
                var analyzedEntities = AnalyzeAllEntities();
                results.Add($"Analyzed {analyzedEntities.Count} entities");
                results.Add("");

                var db = DbScoped.SugarScope;
                int createdCount = 0;
                int updatedCount = 0;
                int errorCount = 0;
                int skippedCount = 0;

                foreach (var entity in analyzedEntities)
                {
                    try
                    {
                        var dictType = $"sql_{entity.DictType}";

                        // Check if exists
                        var existing = db.Queryable<SysDictType>()
                            .First(d => d.DictType == dictType);

                        var dictTypeEntity = new SysDictType
                        {
                            DictName = entity.DictName,
                            DictType = dictType,
                            Status = "0",
                            Type = "Y", // Y = SQL-based
                            CustomSql = entity.GeneratedSql,
                            Remark = $"Auto-generated from {entity.EntityType.Name} table",
                            Create_by = "system",
                            Create_time = DateTime.Now
                        };

                        if (existing != null)
                        {
                            // Update existing
                            //existing.DictName = dictTypeEntity.DictName;
                            //existing.CustomSql = dictTypeEntity.CustomSql;
                            //existing.Remark = dictTypeEntity.Remark;
                            //existing.Update_by = "system";
                            //existing.Update_time = DateTime.Now;

                            //db.Updateable(existing)
                            //    .IgnoreColumns(it => new { it.Create_by, it.Create_time })
                            //    .ExecuteCommand();

                            //updatedCount++;
                            //results.Add($"✓ UPDATED: {entity.EntityType.Name} -> {dictType}");
                        }
                        else
                        {
                            // Insert new
                            db.Insertable(dictTypeEntity).ExecuteCommand();
                            createdCount++;
                            results.Add($"✓ CREATED: {entity.EntityType.Name} -> {dictType}");
                        }

                        results.Add($"  Table: {entity.TableName}");
                        results.Add($"  Value: {entity.ValueColumn}, Label: {string.Join(", ", entity.LabelColumns)}");
                        results.Add($"  SQL Preview: {entity.GeneratedSql.Substring(0, Math.Min(80, entity.GeneratedSql.Length))}...");
                        results.Add("");
                    }
                    catch (Exception ex)
                    {
                        errorCount++;
                        results.Add($"✗ ERROR: {entity.EntityType.Name} - {ex.Message}");
                        results.Add("");
                    }
                }

                results.Add("=== Summary ===");
                results.Add($"Created: {createdCount}");
                results.Add($"Updated: {updatedCount}");
                results.Add($"Errors: {errorCount}");
                results.Add($"Total Processed: {analyzedEntities.Count}");
            }
            catch (Exception ex)
            {
                results.Add($"Fatal error: {ex.Message}");
                results.Add($"Stack: {ex.StackTrace}");
            }

            return results;
        }

        /// <summary>
        /// Exports analysis results to JSON format
        /// </summary>
        public string ExportAnalysisToJson()
        {
            var analyzed = AnalyzeAllEntities();

            var export = analyzed.Select(e => new
            {
                entityName = e.EntityType.Name,
                tableName = e.TableName,
                dictType = $"sql_{e.DictType}",
                dictName = e.DictName,
                valueColumn = e.ValueColumn,
                labelColumns = e.LabelColumns,
                labelExpression = e.LabelExpression,
                remarkColumn = e.RemarkColumn,
                orderColumn = e.OrderColumn,
                hasBilingual = e.HasArabicColumn,
                generatedSql = e.GeneratedSql,
                properties = e.AllProperties.Select(p => new
                {
                    name = p.Name,
                    type = p.PropertyType.Name
                }).ToList()
            }).ToList();

            return System.Text.Json.JsonSerializer.Serialize(export, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });
        }
    }
}

