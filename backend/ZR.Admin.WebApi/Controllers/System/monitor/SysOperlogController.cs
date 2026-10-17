using Microsoft.AspNetCore.Mvc;
using ZR.Model.System.Dto;

namespace ZR.Admin.WebApi.Controllers.monitor
{
    /// <summary>
    /// Operation log record
    /// </summary>
    [Route("/monitor/operlog")]
    [ApiExplorerSettings(GroupName = "sys")]
    public class SysOperlogController : BaseController
    {
        private ISysOperLogService sysOperLogService;

        public SysOperlogController(ISysOperLogService sysOperLogService)
        {
            this.sysOperLogService = sysOperLogService;
        }

        /// <summary>
        /// Query operation log
        /// </summary>
        /// <param name="sysOperLog"></param>
        /// <returns></returns>
        [HttpGet("list")]
        public IActionResult OperList([FromQuery] SysOperLogQueryDto sysOperLog)
        {
            sysOperLog.OperName = !HttpContextExtension.IsAdmin(HttpContext) ? HttpContextExtension.GetName(HttpContext) : sysOperLog.OperName;
            var list = sysOperLogService.SelectOperLogList(sysOperLog);

            return SUCCESS(list);
        }

        /// <summary>
        /// Delete operation log
        /// </summary>
        /// <param name="operIds"></param>
        /// <returns></returns>
        [Log(Title = "Operation Log", BusinessType = BusinessType.DELETE)]
        [ActionPermissionFilter(Permission = "monitor:operlog:delete")]
        [HttpDelete("{operIds}")]
        public IActionResult Remove(string operIds)
        {
            if (!HttpContextExtension.IsAdmin(HttpContext))
            {
                return ToResponse(ApiResult.Error("Operation failed"));
            }
            long[] operIdss = Tools.SpitLongArrary(operIds);
            return SUCCESS(sysOperLogService.DeleteOperLogByIds(operIdss));
        }

        /// <summary>
        /// Clear operation log
        /// </summary>
        /// <returns></returns>
        [Log(Title = "Clear Operation Log", BusinessType = BusinessType.CLEAN)]
        [ActionPermissionFilter(Permission = "monitor:operlog:delete")]
        [HttpDelete("clean")]
        public IActionResult ClearOperLog()
        {
            if (!HttpContextExtension.IsAdmin(HttpContext))
            {
                return ToResponse(ResultCode.CUSTOM_ERROR,"Operation failed");
            }
            sysOperLogService.CleanOperLog();

            return SUCCESS(1);
        }

        /// <summary>
        /// Export operation log
        /// </summary>
        /// <returns></returns>
        [Log(Title = "Operation Log", BusinessType = BusinessType.EXPORT)]
        [ActionPermissionFilter(Permission = "monitor:operlog:export")]
        [HttpGet("export")]
        public IActionResult Export([FromQuery] SysOperLogQueryDto sysOperLog)
        {
            sysOperLog.PageSize = 100000;
            var list = sysOperLogService.SelectOperLogList(sysOperLog);
            var result = ExportExcelMini(list.Result, "Operation Log", "Operation Log");
            return ExportExcel(result.Item2, result.Item1);
        }

    }
}

