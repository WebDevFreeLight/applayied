using Microsoft.AspNetCore.Mvc;
using ZR.Model;
using ZR.Model.System;
using ZR.Model.System.Dto;
using ZR.ServiceCore.Services;

namespace ZR.Admin.WebApi.Controllers.System
{
    /// <summary>
    /// Data Dictionary Information
    /// </summary>
    [Route("system/dict/type")]
    [ApiExplorerSettings(GroupName = "sys")]
    public class SysDictTypeController : BaseController
    {
        private readonly ISysDictService SysDictService;
        private readonly SeedDataService SeedDataService;

        public SysDictTypeController(ISysDictService sysDictService)
        {
            SysDictService = sysDictService;
            SeedDataService = new SeedDataService();
        }

        /// <summary>
        /// Query
        /// </summary>
        /// <param name="dict"></param>
        /// <param name="pagerInfo"></param>
        /// <returns></returns>
        [ActionPermissionFilter(Permission = "system:dict:list")]
        [HttpGet("list")]
        public IActionResult List([FromQuery] SysDictType dict, [FromQuery] PagerInfo pagerInfo)
        {
            var list = SysDictService.SelectDictTypeList(dict, pagerInfo);

            return SUCCESS(list, TIME_FORMAT_FULL);
        }

        /// <summary>
        /// Query dictionary type details
        /// </summary>
        /// <param name="dictId"></param>
        /// <returns></returns>
        [HttpGet("{dictId}")]
        [ActionPermissionFilter(Permission = "system:dict:query")]
        public IActionResult GetInfo(long dictId = 0)
        {
            return SUCCESS(SysDictService.GetInfo(dictId));
        }

        /// <summary>
        /// Add dictionary type
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [ActionPermissionFilter(Permission = "system:dict:add")]
        [Log(Title = "Dictionary Operation", BusinessType = BusinessType.INSERT)]
        [HttpPost("edit")]
        public IActionResult Add([FromBody] SysDictTypeDto dto)
        {
            SysDictType dict = dto.Adapt<SysDictType>();
            if (UserConstants.NOT_UNIQUE.Equals(SysDictService.CheckDictTypeUnique(dict)))
            {
                return ToResponse(ApiResult.Error($"Add dictionary '{dict.DictName}' failed, dictionary type already exists"));
            }
            dict.Create_by = HttpContext.GetName();
            dict.Create_time = DateTime.Now;
            return SUCCESS(SysDictService.InsertDictType(dict));
        }

        /// <summary>
        /// Modify dictionary type
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [ActionPermissionFilter(Permission = "system:dict:edit")]
        [Log(Title = "Dictionary Operation", BusinessType = BusinessType.UPDATE)]
        [Route("edit")]
        [HttpPut]
        public IActionResult Edit([FromBody] SysDictTypeDto dto)
        {
            SysDictType dict = dto.Adapt<SysDictType>();
            if (UserConstants.NOT_UNIQUE.Equals(SysDictService.CheckDictTypeUnique(dict)))
            {
                return ToResponse(ApiResult.Error($"Modify dictionary '{dict.DictName}' failed, dictionary type already exists"));
            }
            //Set creator
            dict.Update_by = HttpContext.GetName();
            return SUCCESS(SysDictService.UpdateDictType(dict));
        }

        /// <summary>
        /// Delete dictionary type
        /// </summary>
        /// <returns></returns>
        [ActionPermissionFilter(Permission = "system:dict:remove")]
        [Log(Title = "Delete Dictionary Type", BusinessType = BusinessType.DELETE)]
        [HttpDelete("{ids}")]
        public IActionResult Remove(string ids)
        {
            long[] idss = Tools.SpitLongArrary(ids);

            return SUCCESS(SysDictService.DeleteDictTypeByIds(idss));
        }

        /// <summary>
        /// Dictionary export
        /// </summary>
        /// <returns></returns>
        [Log(BusinessType = BusinessType.EXPORT, IsSaveResponseData = false, Title = "Dictionary Export")]
        [HttpGet("export")]
        [ActionPermissionFilter(Permission = "system:dict:export")]
        public IActionResult Export()
        {
            var list = SysDictService.GetAll();

            string sFileName = ExportExcel(list, "sysdictType", "Dictionary");
            return SUCCESS(new { path = "/export/" + sFileName, fileName = sFileName });
        }

        /// <summary>
        /// Generate SQL Dictionaries from SqlDictAttribute
        /// Automatically discovers all entities marked with [SqlDict] attribute
        /// and generates SQL-based dictionary types with automatic queries
        /// </summary>
        /// <returns></returns>
        [Log(BusinessType = BusinessType.INSERT, Title = "Generate SQL Dictionaries")]
        [HttpPost("generate-sql-dictionaries")]
        [ActionPermissionFilter(Permission = "system:dict:add")]
        public IActionResult GenerateSqlDictionaries()
        {
            try
            {
                var results = SeedDataService.GenerateAndSaveSqlDictionaries();
                return SUCCESS(new {
                    message = "SQL Dictionary generation completed",
                    results = results
                });
            }
            catch (Exception ex)
            {
                return ToResponse(ApiResult.Error($"Error generating SQL dictionaries: {ex.Message}"));
            }
        }

        /// <summary>
        /// Discover entities with SqlDictAttribute
        /// Returns a list of all entities that have SqlDictAttribute for preview
        /// </summary>
        /// <returns></returns>
        [HttpGet("discover-sql-dict-entities")]
        [ActionPermissionFilter(Permission = "system:dict:list")]
        public IActionResult DiscoverSqlDictEntities()
        {
            try
            {
                var sqlDictGenerator = new SqlDictGeneratorService();
                var entities = sqlDictGenerator.DiscoverSqlDictEntities();

                var result = entities.Select(e => new
                {
                    entityName = e.EntityType.Name,
                    tableName = e.TableName,
                    dictType = e.Attribute.GetPrefixedDictType(),
                    dictName = e.Attribute.DictName,
                    valueColumn = e.Attribute.ValueColumn,
                    labelColumns = e.Attribute.LabelColumns,
                    orderBy = e.Attribute.OrderBy,
                    description = e.Attribute.Description
                }).ToList();

                return SUCCESS(new {
                    count = result.Count,
                    entities = result
                });
            }
            catch (Exception ex)
            {
                return ToResponse(ApiResult.Error($"Error discovering SQL dict entities: {ex.Message}"));
            }
        }

        /// <summary>
        /// Test SQL Query for a dictionary type
        /// Returns sample data from the SQL query
        /// </summary>
        /// <param name="dictType">The dictionary type to test (e.g., "sql_event_type")</param>
        /// <returns></returns>
        [HttpGet("test-sql-query/{dictType}")]
        [ActionPermissionFilter(Permission = "system:dict:query")]
        public IActionResult TestSqlQuery(string dictType)
        {
            try
            {
                var allDictTypes = SysDictService.GetAll();
                var dictTypeInfo = allDictTypes.FirstOrDefault(d => d.DictType == dictType);

                if (dictTypeInfo == null)
                {
                    return ToResponse(ApiResult.Error($"Dictionary type '{dictType}' not found"));
                }

                if (string.IsNullOrEmpty(dictTypeInfo.CustomSql))
                {
                    return ToResponse(ApiResult.Error($"Dictionary type '{dictType}' has no SQL query"));
                }

                var sqlDictGenerator = new SqlDictGeneratorService();
                var (success, message, sampleData) = sqlDictGenerator.TestSqlQuery(dictTypeInfo.CustomSql, 10);

                if (success)
                {
                    return SUCCESS(new {
                        message = message,
                        sql = dictTypeInfo.CustomSql,
                        sampleData = sampleData
                    });
                }
                else
                {
                    return ToResponse(ApiResult.Error(message));
                }
            }
            catch (Exception ex)
            {
                return ToResponse(ApiResult.Error($"Error testing SQL query: {ex.Message}"));
            }
        }

        /// <summary>
        /// AUTO-GENERATE SQL Dictionaries for ALL SugarTable Entities
        /// Automatically discovers all entities, analyzes columns intelligently,
        /// and generates SQL dictionaries with dictValue, dictLabel, and dictType
        /// </summary>
        /// <returns></returns>
        [Log(BusinessType = BusinessType.INSERT, Title = "Auto-Generate All SQL Dictionaries")]
        [HttpPost("auto-generate-all")]
        [ActionPermissionFilter(Permission = "system:dict:add")]
        public IActionResult AutoGenerateAllSqlDictionaries()
        {
            try
            {
                var results = SeedDataService.AutoGenerateAllSqlDictionaries();
                return SUCCESS(new {
                    message = "Automatic SQL Dictionary generation completed for all SugarTable entities",
                    results = results
                });
            }
            catch (Exception ex)
            {
                return ToResponse(ApiResult.Error($"Error auto-generating SQL dictionaries: {ex.Message}"));
            }
        }

        /// <summary>
        /// Analyze ALL SugarTable Entities (Preview Only)
        /// Shows what SQL dictionaries would be generated without saving
        /// </summary>
        /// <returns></returns>
        [HttpGet("analyze-all-entities")]
        [ActionPermissionFilter(Permission = "system:dict:list")]
        public IActionResult AnalyzeAllEntities()
        {
            try
            {
                var analyzed = SeedDataService.AnalyzeAllSugarTableEntities();

                var result = analyzed.Select(e => new
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
                    propertyCount = e.AllProperties.Count
                }).ToList();

                return SUCCESS(new {
                    message = $"Analyzed {result.Count} SugarTable entities",
                    count = result.Count,
                    entities = result
                });
            }
            catch (Exception ex)
            {
                return ToResponse(ApiResult.Error($"Error analyzing entities: {ex.Message}"));
            }
        }

        /// <summary>
        /// Export Entity Analysis to JSON
        /// Returns detailed analysis of all SugarTable entities in JSON format
        /// </summary>
        /// <returns></returns>
        [HttpGet("export-analysis-json")]
        [ActionPermissionFilter(Permission = "system:dict:list")]
        public IActionResult ExportAnalysisJson()
        {
            try
            {
                var json = SeedDataService.ExportEntityAnalysisToJson();
                return Content(json, "application/json");
            }
            catch (Exception ex)
            {
                return ToResponse(ApiResult.Error($"Error exporting analysis: {ex.Message}"));
            }
        }

        /// <summary>
        /// ADD [SqlDict] Attributes to ALL Entity Files
        /// Automatically adds [SqlDict(...)] attribute to all entity files that don't have it
        /// Creates backups before modifying files
        /// </summary>
        /// <param name="saveFiles">If true, actually saves files. If false, dry run only.</param>
        /// <returns></returns>
        [Log(BusinessType = BusinessType.UPDATE, Title = "Add SqlDict Attributes to Files")]
        [HttpPost("add-attributes-to-files")]
        [ActionPermissionFilter(Permission = "system:dict:add")]
        public IActionResult AddSqlDictAttributesToFiles([FromQuery] bool saveFiles = false)
        {
            try
            {
                var results = SeedDataService.AddSqlDictAttributesToAllEntityFiles(saveFiles);
                return SUCCESS(new {
                    message = saveFiles
                        ? "[SqlDict] attributes added to entity files (backups created)"
                        : "[SqlDict] attributes analyzed (DRY RUN - no files modified)",
                    saveFiles = saveFiles,
                    results = results
                });
            }
            catch (Exception ex)
            {
                return ToResponse(ApiResult.Error($"Error adding [SqlDict] attributes: {ex.Message}"));
            }
        }

        /// <summary>
        /// Preview [SqlDict] Attribute Addition
        /// Shows what attributes would be added to which files WITHOUT modifying anything
        /// </summary>
        /// <returns></returns>
        [HttpGet("preview-attribute-addition")]
        [ActionPermissionFilter(Permission = "system:dict:list")]
        public IActionResult PreviewAttributeAddition()
        {
            try
            {
                var results = SeedDataService.PreviewSqlDictAttributeAddition();
                return SUCCESS(new {
                    message = "Preview of [SqlDict] attribute addition",
                    results = results
                });
            }
            catch (Exception ex)
            {
                return ToResponse(ApiResult.Error($"Error previewing attribute addition: {ex.Message}"));
            }
        }

        /// <summary>
        /// Add [SqlDict] Attributes to Specific Module
        /// Adds attributes only to entities in a specific namespace (EventManagement, HR, Business, etc.)
        /// </summary>
        /// <param name="moduleName">Module name (e.g., "EventManagement", "HR", "Business")</param>
        /// <param name="saveFiles">If true, actually saves files</param>
        /// <returns></returns>
        [Log(BusinessType = BusinessType.UPDATE, Title = "Add SqlDict Attributes to Module")]
        [HttpPost("add-attributes-to-module/{moduleName}")]
        [ActionPermissionFilter(Permission = "system:dict:add")]
        public IActionResult AddAttributesToModule(string moduleName, [FromQuery] bool saveFiles = false)
        {
            try
            {
                var results = SeedDataService.AddSqlDictAttributesToModule(moduleName, saveFiles);
                return SUCCESS(new {
                    message = $"[SqlDict] attributes processed for module: {moduleName}",
                    moduleName = moduleName,
                    saveFiles = saveFiles,
                    results = results
                });
            }
            catch (Exception ex)
            {
                return ToResponse(ApiResult.Error($"Error adding attributes to module: {ex.Message}"));
            }
        }
    }
}

