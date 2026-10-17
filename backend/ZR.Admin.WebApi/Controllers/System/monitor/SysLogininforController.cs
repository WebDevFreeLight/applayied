using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using ZR.Model.System;
using ZR.Model.System.Dto;

namespace ZR.Admin.WebApi.Controllers.monitor
{
    /// <summary>
    /// System access log
    /// </summary>
    [Route("/monitor/logininfor")]
    [ApiExplorerSettings(GroupName = "sys")]
    public class SysLogininforController : BaseController
    {
        private ISysLoginService sysLoginService;

        public SysLogininforController(ISysLoginService sysLoginService)
        {
            this.sysLoginService = sysLoginService;
        }

        /// <summary>
        /// Query login log
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "monitor:logininfor:list")]
        public IActionResult LoignLogList([FromQuery] SysLogininfoQueryDto param)
        {
            var list = sysLoginService.GetLoginLog(param);

            return SUCCESS(list);
        }
        
        /// <summary>
        /// Query my login log
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [HttpGet("mylist")]
        public IActionResult QueryMyLoignLogList([FromQuery] SysLogininfoQueryDto param)
        {
            param.UserId = HttpContext.GetUId();
            var list = sysLoginService.GetLoginLog(param);

            return SUCCESS(list);
        }

        /// <summary>
        /// Clear login log
        /// </summary>
        /// <returns></returns>
        [Log(Title = "Clear Login Log", BusinessType = BusinessType.CLEAN)]
        [ActionPermissionFilter(Permission = "monitor:logininfor:remove")]
        [HttpDelete("clean")]
        public IActionResult CleanLoginInfo()
        {
            if (!HttpContextExtension.IsAdmin(HttpContext))
            {
                return ToResponse(ApiResult.Error("Operation failed"));
            }
            sysLoginService.TruncateLogininfo();
            return SUCCESS(1);
        }

        /// <summary>
        /// </summary>
        /// <param name="infoIds"></param>
        /// <returns></returns>
        [Log(Title = "Delete Login Log", BusinessType = BusinessType.DELETE)]
        [HttpDelete("{infoIds}")]
        [ActionPermissionFilter(Permission = "monitor:logininfor:remove")]
        public IActionResult Remove(string infoIds)
        {
            if (!HttpContextExtension.IsAdmin(HttpContext))
            {
                return ToResponse(ApiResult.Error("Operation failed"));
            }
            long[] infoIdss = Tools.SpitLongArrary(infoIds);
            return SUCCESS(sysLoginService.DeleteLogininforByIds(infoIdss));
        }

        /// <summary>
        /// Login log export
        /// </summary>
        /// <returns></returns>
        [Log(BusinessType = BusinessType.EXPORT, IsSaveResponseData = false, Title = "Login Log Export")]
        [HttpGet("export")]
        [ActionPermissionFilter(Permission = "monitor:logininfor:export")]
        public IActionResult Export([FromQuery] SysLogininfor logininfoDto)
        {
            logininfoDto.BeginTime = DateTimeHelper.GetBeginTime(logininfoDto.BeginTime, -1);
            logininfoDto.EndTime = DateTimeHelper.GetBeginTime(logininfoDto.EndTime, 1);
            var exp = Expressionable.Create<SysLogininfor>()
                .And(it => it.LoginTime >= logininfoDto.BeginTime && it.LoginTime <= logininfoDto.EndTime);

            var list = sysLoginService.Queryable().Where(exp.ToExpression())
                .ToList();

            string sFileName = ExportExcel(list, "loginlog", "Login Log");
            return SUCCESS(new { path = "/export/" + sFileName, fileName = sFileName });
        }

        /// <summary>
        /// Query login log statistics
        /// </summary>
        /// <returns></returns>
        [HttpGet("statiLoginLog")]
        [ActionPermissionFilter(Permission = "common")]
        public IActionResult QueryStatiLoginLog()
        {
            var list = sysLoginService.GetStatiLoginlog();
            var categories = list.Select(x => x.Date.ToString("dd")).ToList();
            var numList = list.Select(x => x.Num).ToList();
            return SUCCESS(new { categories, numList });
        }
    }
}

