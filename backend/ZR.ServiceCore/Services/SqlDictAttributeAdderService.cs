using Infrastructure;
using SqlSugar;
using SqlSugar.IOC;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using ZR.Common;
using ZR.Model;

namespace ZR.ServiceCore.Services
{
    /// <summary>
    /// Service to automatically add [SqlDict] attributes to all SugarTable entities
    /// that don't already have the attribute
    /// </summary>
    public class SqlDictAttributeAdderService
    {
        private readonly AutoSqlDictGeneratorService _autoGenerator;

        public SqlDictAttributeAdderService()
        {
            _autoGenerator = new AutoSqlDictGeneratorService();
        }

        /// <summary>
        /// Represents an entity file that needs SqlDict attribute added
        /// </summary>
        public class EntityFileInfo
        {
            public Type EntityType { get; set; }
            public string FilePath { get; set; }
            public string FileContent { get; set; }
            public AutoSqlDictGeneratorService.DiscoveredEntity Analysis { get; set; }
            public bool HasSqlDictAttribute { get; set; }
            public string GeneratedAttribute { get; set; }
            public string ModifiedContent { get; set; }
            public bool WasModified { get; set; }
        }

        /// <summary>
        /// Finds all entity files in the Model project
        /// </summary>
        public List<EntityFileInfo> FindAllEntityFiles()
        {
            var results = new List<EntityFileInfo>();

            // Discover all SugarTable entities
            var entities = _autoGenerator.DiscoverAllSugarTableEntities();

            Console.WriteLine($"Found {entities.Count} SugarTable entities");

            foreach (var entityType in entities)
            {
                try
                {
                    // Check if already has SqlDict attribute
                    var hasSqlDict = entityType.GetCustomAttribute<SqlDictAttribute>() != null;

                    // Find the source file
                    var filePath = FindEntityFilePath(entityType);

                    if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                    {
                        Console.WriteLine($"  ⚠ File not found for: {entityType.Name}");
                        continue;
                    }

                    var fileContent = File.ReadAllText(filePath);

                    // Analyze the entity
                    var analysis = _autoGenerator.AnalyzeEntity(entityType);

                    if (analysis == null)
                    {
                        Console.WriteLine($"  ⚠ Analysis skipped: {entityType.Name}");
                        continue;
                    }

                    var entityFile = new EntityFileInfo
                    {
                        EntityType = entityType,
                        FilePath = filePath,
                        FileContent = fileContent,
                        Analysis = analysis,
                        HasSqlDictAttribute = hasSqlDict,
                        GeneratedAttribute = GenerateSqlDictAttributeCode(analysis),
                        WasModified = false
                    };

                    results.Add(entityFile);

                    var status = hasSqlDict ? "HAS ATTR" : "NEEDS ATTR";
                    Console.WriteLine($"  {status}: {entityType.Name} -> {filePath}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  ✗ Error processing {entityType.Name}: {ex.Message}");
                }
            }

            return results;
        }

        /// <summary>
        /// Finds the physical file path for an entity type
        /// Searches in all directories based on InjectClass configuration
        /// </summary>
        private string FindEntityFilePath(Type entityType)
        {
            // Get base directory (where backend folder is)
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var backendDir = Directory.GetParent(baseDir)?.Parent?.Parent?.Parent?.Parent?.FullName;

            if (string.IsNullOrEmpty(backendDir))
                return null;

            // Get InjectClass directories from configuration
            var injectClassDirs = new List<string>
            {
                "ZR.Model",
                //"ZR.Repository",
                //"ZR.Service",
                //"ZR.Tasks",
                //"ZR.ServiceCore",
                
                "ZR.Admin.WebApi",
                //"ZR.Pdf",
                //"ZR.Puppeteer",
                //"Infrastructure",
                //"ZR.Common",
                //"ZR.CodeGenerator",
                //"ZR.HR"
            };

            var fileName = $"{entityType.Name}.cs";

            // Search in each InjectClass directory
            foreach (var dirName in injectClassDirs)
            {
                var searchDir = Path.Combine(backendDir, dirName);

                if (!Directory.Exists(searchDir))
                    continue;

                try
                {
                    var files = Directory.GetFiles(searchDir, fileName, SearchOption.AllDirectories);
                    if (files.Length > 0)
                    {
                        return files.FirstOrDefault();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  ⚠ Error searching in {dirName}: {ex.Message}");
                }
            }

            // If not found in InjectClass dirs, search in entire backend directory as fallback
            try
            {
                var allFiles = Directory.GetFiles(backendDir, fileName, SearchOption.AllDirectories);
                if (allFiles.Length > 0)
                {
                    Console.WriteLine($"  ℹ Found {fileName} in fallback search: {allFiles.FirstOrDefault()}");
                    return allFiles.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ⚠ Error in fallback search: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// Generates the [SqlDict] attribute code as a string
        /// </summary>
        private string GenerateSqlDictAttributeCode(AutoSqlDictGeneratorService.DiscoveredEntity analysis)
        {
            var sb = new StringBuilder();

            sb.AppendLine("    [SqlDict(");
            sb.AppendLine($"        dictType: \"{analysis.DictType}\",");
            sb.AppendLine($"        dictName: \"{analysis.DictName}\",");
            sb.AppendLine($"        valueColumn: \"{analysis.ValueColumn}\",");

            // Label columns - build the expression
            if (analysis.LabelColumns.Count > 1 || analysis.LabelExpression.Contains("' - '"))
            {
                sb.AppendLine($"        labelColumns: \"{analysis.LabelExpression}\",");
            }
            else
            {
                sb.AppendLine($"        labelColumns: \"{analysis.LabelExpression}\",");
            }

            // Remark column (optional)
            if (!string.IsNullOrEmpty(analysis.RemarkColumn))
            {
                sb.AppendLine($"        RemarkColumn = \"{analysis.RemarkColumn}\",");
            }

            // Order column (optional)
            if (!string.IsNullOrEmpty(analysis.OrderColumn))
            {
                sb.AppendLine($"        OrderBy = \"{analysis.OrderColumn} ASC\",");
            }
            else
            {
                sb.AppendLine($"        OrderBy = \"{analysis.ValueColumn} ASC\",");
            }

            sb.AppendLine($"        Description = \"Auto-generated from {analysis.EntityType.Name} table\"");
            sb.Append("    )]");

            return sb.ToString();
        }

        /// <summary>
        /// Adds SqlDict attribute to an entity file
        /// </summary>
        public bool AddSqlDictAttributeToFile(EntityFileInfo entityFile)
        {
            if (entityFile.HasSqlDictAttribute)
            {
                Console.WriteLine($"  ⊘ Skipped (already has attribute): {entityFile.EntityType.Name}");
                return false;
            }

            try
            {
                var content = entityFile.FileContent;
                var className = entityFile.EntityType.Name;

                // Find the [SugarTable] attribute and class declaration
                var sugarTablePattern = @"(\[SugarTable\([^\]]+\)\]\s*(?:\[.*?\]\s*)*)(public\s+class\s+" + Regex.Escape(className) + @"\s*:?\s*\w*)";
                var match = Regex.Match(content, sugarTablePattern, RegexOptions.Multiline);

                if (!match.Success)
                {
                    Console.WriteLine($"  ✗ Could not find class declaration for: {className}");
                    return false;
                }

                // Insert SqlDict attribute after SugarTable attribute
                var beforeClass = match.Groups[1].Value;
                var classDeclaration = match.Groups[2].Value;

                var newContent = content.Replace(
                    match.Value,
                    beforeClass + "\n" + entityFile.GeneratedAttribute + "\n    " + classDeclaration
                );

                entityFile.ModifiedContent = newContent;
                entityFile.WasModified = true;

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ✗ Error modifying {entityFile.EntityType.Name}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Saves the modified content to the file
        /// </summary>
        public bool SaveModifiedFile(EntityFileInfo entityFile, bool createBackup = true)
        {
            if (!entityFile.WasModified || string.IsNullOrEmpty(entityFile.ModifiedContent))
            {
                return false;
            }

            try
            {
                // Create backup
                if (createBackup)
                {
                    var backupPath = entityFile.FilePath + ".backup";
                    File.Copy(entityFile.FilePath, backupPath, true);
                }

                // Write modified content
                File.WriteAllText(entityFile.FilePath, entityFile.ModifiedContent);

                Console.WriteLine($"  ✓ Saved: {entityFile.FilePath}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ✗ Error saving {entityFile.FilePath}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Main method: Add SqlDict attributes to all entities that don't have them
        /// </summary>
        public List<string> AddSqlDictAttributesToAllEntities(bool saveFiles = false)
        {
            var results = new List<string>();
            results.Add("=== Adding [SqlDict] Attributes to All Entities ===");
            results.Add("");

            try
            {
                // Find all entity files
                var entityFiles = FindAllEntityFiles();
                results.Add($"Found {entityFiles.Count} entity files");
                results.Add("");

                var needsAttribute = entityFiles.Where(e => !e.HasSqlDictAttribute).ToList();
                var hasAttribute = entityFiles.Where(e => e.HasSqlDictAttribute).ToList();

                results.Add($"Already have [SqlDict]: {hasAttribute.Count}");
                results.Add($"Need [SqlDict]: {needsAttribute.Count}");
                results.Add("");

                if (needsAttribute.Count == 0)
                {
                    results.Add("✓ All entities already have [SqlDict] attribute!");
                    return results;
                }

                int modifiedCount = 0;
                int savedCount = 0;
                int errorCount = 0;

                foreach (var entityFile in needsAttribute)
                {
                    try
                    {
                        results.Add($"Processing: {entityFile.EntityType.Name}");
                        results.Add($"  File: {Path.GetFileName(entityFile.FilePath)}");
                        results.Add($"  Dict Type: sql_{entityFile.Analysis.DictType}");
                        results.Add($"  Value Column: {entityFile.Analysis.ValueColumn}");
                        results.Add($"  Label Columns: {string.Join(", ", entityFile.Analysis.LabelColumns)}");

                        // Add attribute
                        var added = AddSqlDictAttributeToFile(entityFile);

                        if (added)
                        {
                            modifiedCount++;
                            results.Add($"  ✓ Attribute added");

                            // Save if requested
                            if (saveFiles)
                            {
                                var saved = SaveModifiedFile(entityFile, createBackup: true);
                                if (saved)
                                {
                                    savedCount++;
                                    results.Add($"  ✓ File saved (backup created)");
                                }
                                else
                                {
                                    results.Add($"  ⚠ File not saved");
                                }
                            }
                            else
                            {
                                results.Add($"  ⊘ Changes not saved (dry run mode)");
                            }
                        }
                        else
                        {
                            errorCount++;
                            results.Add($"  ✗ Failed to add attribute");
                        }

                        results.Add("");
                    }
                    catch (Exception ex)
                    {
                        errorCount++;
                        results.Add($"  ✗ Error: {ex.Message}");
                        results.Add("");
                    }
                }

                results.Add("=== Summary ===");
                results.Add($"Total entities: {entityFiles.Count}");
                results.Add($"Already had attribute: {hasAttribute.Count}");
                results.Add($"Modified: {modifiedCount}");
                if (saveFiles)
                {
                    results.Add($"Saved: {savedCount}");
                    results.Add($"Errors: {errorCount}");
                }
                else
                {
                    results.Add($"Mode: DRY RUN (files not saved)");
                    results.Add($"To save files, call with saveFiles=true");
                }
            }
            catch (Exception ex)
            {
                results.Add($"Fatal error: {ex.Message}");
                results.Add($"Stack: {ex.StackTrace}");
            }

            return results;
        }

        /// <summary>
        /// Generates a preview report of what would be added
        /// </summary>
        public List<string> GeneratePreviewReport()
        {
            var results = new List<string>();
            results.Add("=== [SqlDict] Attribute Addition Preview ===");
            results.Add("");

            var entityFiles = FindAllEntityFiles();
            var needsAttribute = entityFiles.Where(e => !e.HasSqlDictAttribute).ToList();

            results.Add($"Total Entities: {entityFiles.Count}");
            results.Add($"Already have [SqlDict]: {entityFiles.Count - needsAttribute.Count}");
            results.Add($"Need [SqlDict]: {needsAttribute.Count}");
            results.Add("");

            if (needsAttribute.Count == 0)
            {
                results.Add("✓ All entities already have [SqlDict] attribute!");
                return results;
            }

            results.Add("Entities that will be modified:");
            results.Add("");

            foreach (var entityFile in needsAttribute)
            {
                results.Add($"File: {entityFile.FilePath}");
                results.Add($"Entity: {entityFile.EntityType.Name}");
                results.Add($"Table: {entityFile.Analysis.TableName}");
                results.Add($"Dict Type: sql_{entityFile.Analysis.DictType}");
                results.Add("");
                results.Add("Attribute to be added:");
                results.Add(entityFile.GeneratedAttribute);
                results.Add("");
                results.Add("---");
                results.Add("");
            }

            return results;
        }

        /// <summary>
        /// Filters entities by namespace/module
        /// </summary>
        public List<EntityFileInfo> FilterByNamespace(List<EntityFileInfo> entityFiles, string namespaceFilter)
        {
            return entityFiles.Where(e =>
                e.EntityType.Namespace?.Contains(namespaceFilter, StringComparison.OrdinalIgnoreCase) == true
            ).ToList();
        }

        /// <summary>
        /// Add SqlDict attributes only to specific module
        /// </summary>
        public List<string> AddSqlDictAttributesToModule(string moduleName, bool saveFiles = false)
        {
            var results = new List<string>();
            results.Add($"=== Adding [SqlDict] to {moduleName} Module ===");
            results.Add("");

            try
            {
                var allFiles = FindAllEntityFiles();
                var moduleFiles = FilterByNamespace(allFiles, moduleName);

                results.Add($"Total entities in module: {moduleFiles.Count}");

                var needsAttribute = moduleFiles.Where(e => !e.HasSqlDictAttribute).ToList();
                results.Add($"Need [SqlDict]: {needsAttribute.Count}");
                results.Add("");

                if (needsAttribute.Count == 0)
                {
                    results.Add("✓ All entities in this module already have [SqlDict] attribute!");
                    return results;
                }

                int savedCount = 0;

                foreach (var entityFile in needsAttribute)
                {
                    var added = AddSqlDictAttributeToFile(entityFile);

                    if (added && saveFiles)
                    {
                        if (SaveModifiedFile(entityFile, createBackup: true))
                        {
                            savedCount++;
                            results.Add($"✓ {entityFile.EntityType.Name}");
                        }
                    }
                }

                results.Add("");
                results.Add($"Modified: {savedCount} files");
            }
            catch (Exception ex)
            {
                results.Add($"Error: {ex.Message}");
            }

            return results;
        }
    }
}

