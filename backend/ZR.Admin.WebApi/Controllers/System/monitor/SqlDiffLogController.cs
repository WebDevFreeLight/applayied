using Microsoft.AspNetCore.Mvc;
using ZR.Model.System.Dto;


//Created: 2023-08-17
namespace ZR.Admin.WebApi.Controllers
{
    /// <summary>
    /// Data difference log
    /// </summary>
    [Route("monitor/SqlDiffLog")]
    [ApiExplorerSettings(GroupName = "sys")]
    public class SqlDiffLogController : BaseController
    {
        /// <summary>
        /// Data difference log interface
        /// </summary>
        private readonly ISqlDiffLogService _SqlDiffLogService;

        public SqlDiffLogController(ISqlDiffLogService SqlDiffLogService)
        {
            _SqlDiffLogService = SqlDiffLogService;
        }

        /// <summary>
        /// Query data difference log list
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "sqldifflog:list")]
        public IActionResult QuerySqlDiffLog([FromQuery] SqlDiffLogQueryDto parm)
        {
            var response = _SqlDiffLogService.GetList(parm);
            return SUCCESS(response);
        }

        /// <summary>
        /// Delete data difference log
        /// </summary>
        /// <returns></returns>
        [HttpDelete("{ids}")]
        [ActionPermissionFilter(Permission = "sqldifflog:delete")]
        [Log(Title = "Data Difference Log", BusinessType = BusinessType.DELETE)]
        public IActionResult DeleteSqlDiffLog(string ids)
        {
            long[] idsArr = Tools.SpitLongArrary(ids);
            if (idsArr.Length <= 0) { return ToResponse(ApiResult.Error($"Delete failed, ID cannot be empty")); }

            var response = _SqlDiffLogService.Delete(idsArr);

            return ToResponse(response);
        }

        /// <summary>
        /// Export data difference log
        /// </summary>
        /// <returns></returns>
        [Log(Title = "Data Difference Log", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        [HttpGet("export")]
        [ActionPermissionFilter(Permission = "sqldifflog:export")]
        public IActionResult Export([FromQuery] SqlDiffLogQueryDto parm)
        {
            parm.PageNum = 1;
            parm.PageSize = 100000;
            var list = _SqlDiffLogService.GetList(parm).Result;
            if (list == null || list.Count <= 0)
            {
                return ToResponse(ResultCode.FAIL, "No data to export");
            }
            var result = ExportExcelMini(list, "Data Difference Log", "Data Difference Log");
            return ExportExcel(result.Item2, result.Item1);
        }
    }
}
