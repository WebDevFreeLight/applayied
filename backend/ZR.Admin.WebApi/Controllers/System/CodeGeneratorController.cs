using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SqlSugar;
using ZR.CodeGenerator;
using ZR.CodeGenerator.Model;
using ZR.CodeGenerator.Service;
using ZR.Model;
using ZR.Model.System.Dto;
using ZR.Model.System.Generate;

namespace ZR.Admin.WebApi.Controllers
{
    /// <summary>
    /// Code Generation
    /// </summary>
    [Route("tool/gen")]
    [ApiExplorerSettings(GroupName = "sys")]
    public class CodeGeneratorController : BaseController
    {
        private readonly CodeGeneraterService _CodeGeneraterService = new CodeGeneraterService();
        private readonly IGenTableService GenTableService;
        private readonly IGenTableColumnService GenTableColumnService;
        private readonly ISysMenuService SysMenuService;
        private readonly IWebHostEnvironment WebHostEnvironment;
        private readonly OptionsSetting OptionsSetting;

        public CodeGeneratorController(
            IGenTableService genTableService,
            IGenTableColumnService genTableColumnService,
            IWebHostEnvironment webHostEnvironment,
            IOptions<OptionsSetting> options,
            ISysMenuService sysMenuService)
        {
            GenTableService = genTableService;
            GenTableColumnService = genTableColumnService;
            WebHostEnvironment = webHostEnvironment;
            SysMenuService = sysMenuService;
            OptionsSetting = options.Value;
        }

        /// <summary>
        /// Get information of all databases
        /// </summary>
        /// <returns></returns>
        [HttpGet("getDbList")]
        [ActionPermissionFilter(Permission = "tool:gen:list")]
        public IActionResult GetListDataBase()
        {
            var dbList = _CodeGeneraterService.GetAllDataBases();
            var defaultDb = dbList?[0];
            return SUCCESS(new { dbList, defaultDb });
        }

        /// <summary>
        /// Get all tables by database name
        /// </summary>
        /// <param name="dbName">Database name</param>
        /// <param name="tableName">Table name</param>
        /// <param name="pager">Pagination information</param>
        /// <returns></returns>
        [HttpGet("getTableList")]
        [ActionPermissionFilter(Permission = "tool:gen:list")]
        public IActionResult FindListTable(string dbName, string? tableName, PagerInfo pager)
        {
            List<DbTableInfo> list = _CodeGeneraterService.GetAllTables(dbName, tableName, pager);
            var page = new PagedInfo<DbTableInfo>
            {
                TotalNum = pager.TotalNum,
                PageSize = pager.PageSize,
                PageIndex = pager.PageNum,
                Result = list
            };
            return SUCCESS(page);
        }

        /// <summary>
        /// Query generated table data
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <param name="pagerInfo">Pagination information</param>
        /// <returns></returns>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "tool:gen:list")]
        public IActionResult GetGenTable(string? tableName, PagerInfo pagerInfo)
        {
            //Query original table data, map some fields to code generation table fields
            var rows = GenTableService.GetGenTables(new GenTable() { TableName = tableName }, pagerInfo);

            return SUCCESS(rows, "MMM dd HH:mm");
        }

        /// <summary>
        /// Modify code generation business query
        /// </summary>
        /// <param name="tableId">genTable table id</param>
        /// <returns></returns>
        [HttpGet("{tableId}")]
        [ActionPermissionFilter(Permission = "tool:gen:query")]
        public IActionResult GetColumnList(long tableId)
        {
            var tableInfo = GenTableService.GetGenTableInfo(tableId);
            var tables = GenTableService.GetGenTableAll();
            return SUCCESS(new { info = tableInfo, tables });
        }

        /// <summary>
        /// Query table columns by table id
        /// </summary>
        /// <param name="tableId">genTable table id</param>
        /// <returns></returns>
        [HttpGet("column/{tableId}")]
        [ActionPermissionFilter(Permission = "tool:gen:query")]
        public IActionResult GetTableColumnList(long tableId)
        {
            var tableColumns = GenTableColumnService.GenTableColumns(tableId);

            return SUCCESS(new { columns = tableColumns });
        }
        /// <summary>
        /// Delete code generation
        /// </summary>
        /// <param name="tableIds"></param>
        /// <returns></returns>
        [Log(Title = "Code Generation", BusinessType = BusinessType.DELETE)]
        [HttpDelete("{tableIds}")]
        [ActionPermissionFilter(Permission = "tool:gen:remove")]
        public IActionResult Remove(string tableIds)
        {
            long[] tableId = Tools.SpitLongArrary(tableIds);

            int result = GenTableService.DeleteGenTableByIds(tableId);
            return SUCCESS(result);
        }

        /// <summary>
        /// Import table structure (Save)
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost("importTable")]
        [Log(Title = "Code Generation", BusinessType = BusinessType.IMPORT)]
        [ActionPermissionFilter(Permission = "tool:gen:import")]
        public IActionResult ImportTableSave([FromBody] ImportCodeGenTableDto dto)
        {
            if (string.IsNullOrEmpty(dto.DbName) || dto.Tables == null)
            {
                throw new CustomException("Table or database cannot be empty");
            }

            int result = 0;
            foreach (var table in dto.Tables)
            {
                List<OracleSeq> seqs = new();
                InitTableDto initTableDto = new()
                {
                    DbName = dto.DbName,
                    UserName = HttpContext.GetName(),
                    TableName = table.Name,
                    Desc = table.Description,
                    CodeGen = OptionsSetting.CodeGen,
                    FrontTpl = dto.FrontTpl,
                };

                GenTable genTable = CodeGeneratorTool.InitTable(initTableDto);
                genTable.TableId = GenTableService.ImportGenTable(genTable);
                if (OptionsSetting.CodeGenDbConfig.DbType == 3)
                {
                    seqs = _CodeGeneraterService.GetAllOracleSeqs(table.Name);
                }
                if (genTable.TableId > 0)
                {
                    //Save column information
                    List<DbColumnInfo> dbColumnInfos = _CodeGeneraterService.GetColumnInfo(dto.DbName, table.Name);
                    List<GenTableColumn> genTableColumns = CodeGeneratorTool.InitGenTableColumn(genTable, dbColumnInfos, seqs, OptionsSetting.CodeGen);
                    GenTableColumnService.DeleteGenTableColumnByTableName(table.Name);
                    GenTableColumnService.InsertGenTableColumn(genTableColumns);
                    genTable.Columns = genTableColumns;
                    result++;
                }
            }

            return ToResponse(result);
        }

        /// <summary>
        /// Modify and save code generation business
        /// </summary>
        /// <param name="genTableDto">Request parameter entity</param>
        /// <returns></returns>
        [HttpPut]
        [Log(Title = "Code Generation", BusinessType = BusinessType.GENCODE, IsSaveRequestData = false)]
        [ActionPermissionFilter(Permission = "tool:gen:edit")]
        public IActionResult EditSave([FromBody] GenTableDto genTableDto)
        {
            if (genTableDto == null) throw new CustomException("Request parameters are invalid");
            if (genTableDto.BusinessName.Equals(genTableDto.ModuleName, StringComparison.OrdinalIgnoreCase))
            {
                return ToResponse(ResultCode.CUSTOM_ERROR, "Module name cannot be the same as business name");
            }
            var genTable = genTableDto.Adapt<GenTable>().ToUpdate(HttpContext);

            //Convert frontend additional parameters to string and store in Options
            genTable.Options = genTableDto.Params.Adapt<CodeOptions>();
            DbResult<bool> result = GenTableService.UseTran(() =>
            {
                int rows = GenTableService.UpdateGenTable(genTable);
                if (rows > 0)
                {
                    GenTableColumnService.UpdateGenTableColumn(genTable.Columns);
                }
            });

            return SUCCESS(result.IsSuccess);
        }

        /// <summary>
        /// Preview code
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="tableId"></param>
        /// <returns></returns>
        [HttpPost("preview/{tableId}")]
        [ActionPermissionFilter(Permission = "tool:gen:preview")]
        public IActionResult Preview([FromQuery] GenerateDto dto, [FromRoute] int tableId = 0)
        {
            dto.TableId = tableId;
            if (dto == null || dto.TableId <= 0)
            {
                throw new CustomException(ResultCode.CUSTOM_ERROR, "Request parameters are empty");
            }
            var genTableInfo = GenTableService.GetGenTableInfo(dto.TableId);

            dto.DbType = OptionsSetting.CodeGenDbConfig.DbType;
            dto.GenTable = genTableInfo;
            dto.IsPreview = true;

            CodeGeneratorTool.Generate(dto);

            return SUCCESS(dto.GenCodes);
        }

        /// <summary>
        /// Generate code (Download method)
        /// </summary>
        /// <param name="dto">Data transfer object</param>
        /// <returns></returns>
        [HttpPost("genCode")]
        [Log(Title = "Code Generation", BusinessType = BusinessType.GENCODE)]
        [ActionPermissionFilter(Permission = "tool:gen:code")]
        public IActionResult CodeGenerate([FromBody] GenerateDto dto)
        {
            if (dto == null || dto.TableId <= 0)
            {
                throw new CustomException(ResultCode.CUSTOM_ERROR, "Request parameters are empty");
            }

            dto.DbType = OptionsSetting.CodeGenDbConfig.DbType;
            dto.GenTable = GenTableService.GetGenTableInfo(dto.TableId);
            //Generate zip file
            string zipReturnFileName = $"maham-ai.NET-{dto.GenTable.TableName}-{DateTime.Now:MMddHHmmss}.zip";

            //Generate code to specified folder
            CodeGeneratorTool.Generate(dto);
            if (dto.GenTable.Options.GenerateMenu)
            {
                SysMenuService.AddSysMenu(dto.GenTable, dto.ReplaceDto.PermissionPrefix, dto.ReplaceDto.ShowBtnEdit, dto.ReplaceDto.ShowBtnExport, dto.ReplaceDto.ShowBtnImport);
            }
            if (dto.GenTable.GenType == "1")//Custom path
            {
                var genPath = dto.GenTable.GenPath;
                string parentPath;
                string tempPath = WebHostEnvironment.ContentRootPath;

                if (ComputerHelper.IsUnix())
                {
                    parentPath = Path.Combine(WebHostEnvironment.WebRootPath, "Generatecode");
                }
                else
                {
                    parentPath = tempPath[..tempPath.LastIndexOf(@"\")];
                }
                Console.WriteLine("Code generation path: " + parentPath);
                //Code generation folder path
                dto.GenCodePath = (genPath.IsEmpty() || genPath.Equals("/")) ? parentPath : genPath;
            }
            else
            {
                dto.ZipPath = Path.Combine(WebHostEnvironment.WebRootPath, "Generatecode");
                dto.GenCodePath = Path.Combine(dto.ZipPath, DateTime.Now.ToString("yyyyMMdd"));
            }

            //Write to file
            foreach (var item in dto.GenCodes)
            {
                item.Path = Path.Combine(dto.GenCodePath, item.Path);
                FileUtil.WriteAndSave(item.Path, item.Content);
            }
            if (dto.GenTable.GenType != "1")
            {
                //Compress file
                FileUtil.ZipGenCode(dto.ZipPath, dto.GenCodePath, zipReturnFileName);
            }
            return SUCCESS(new { path = "/Generatecode/" + zipReturnFileName, fileName = dto.ZipFileName });
        }

        /// <summary>
        /// Synchronize database
        /// </summary>
        /// <param name="tableId"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        [ActionPermissionFilter(Permission = "tool:gen:edit")]
        [Log(Title = "Code Generation", BusinessType = BusinessType.UPDATE)]
        [HttpGet("synchDb/{tableId}")]
        public IActionResult SynchDb(string tableName, long tableId = 0)
        {
            if (string.IsNullOrEmpty(tableName) || tableId <= 0) throw new CustomException("Invalid parameters");
            GenTable table = GenTableService.GetGenTableInfo(tableId);
            if (table == null) { throw new CustomException("Synchronization failed, original table structure does not exist"); }
            table.Update_by = HttpContext.GetName();

            var codeGen = AppSettings.Get<CodeGen>("codeGen");
            List<DbColumnInfo> dbColumnInfos = _CodeGeneraterService.GetColumnInfo(table.DbName, tableName);
            List<GenTableColumn> dbTableColumns = CodeGeneratorTool.InitGenTableColumn(table, dbColumnInfos, codeGen: codeGen);

            bool result = GenTableService.SynchDb(tableId, table, dbTableColumns);
            return SUCCESS(result);
        }
    }
}

