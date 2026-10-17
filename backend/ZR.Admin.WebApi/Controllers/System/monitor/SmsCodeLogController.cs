using Microsoft.AspNetCore.Mvc;
using ZR.Model.Dto;

//Created: 2023-11-19
namespace ZR.Admin.WebApi.Controllers.System.monitor
{
    /// <summary>
    /// SMS verification code log
    /// </summary>
    [Route("system/SmscodeLog")]
    [ApiExplorerSettings(GroupName = "sys")]
    public class SmsCodeLogController : BaseController
    {
        /// <summary>
        /// SMS verification code log interface
        /// </summary>
        private readonly ISmsCodeLogService _SmscodeLogService;

        public SmsCodeLogController(ISmsCodeLogService SmscodeLogService)
        {
            _SmscodeLogService = SmscodeLogService;
        }

        /// <summary>
        /// Query SMS verification code log list
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "smscodelog:list")]
        public IActionResult QuerySmscodeLog([FromQuery] SmscodeLogQueryDto parm)
        {
            var response = _SmscodeLogService.GetList(parm);
            return SUCCESS(response);
        }

        /// <summary>
        /// Delete SMS verification code log
        /// </summary>
        /// <returns></returns>
        [HttpDelete("{ids}")]
        [ActionPermissionFilter(Permission = "smscodelog:delete")]
        [Log(Title = "SMS Verification Code Log", BusinessType = BusinessType.DELETE)]
        public IActionResult DeleteSmscodeLog(string ids)
        {
            long[] idsArr = Tools.SpitLongArrary(ids);
            if (idsArr.Length <= 0) { return ToResponse(ApiResult.Error($"Delete failed, ID cannot be empty")); }

            var response = _SmscodeLogService.Delete(idsArr);

            return ToResponse(response);
        }

        /// <summary>
        /// Export SMS verification code log
        /// </summary>
        /// <returns></returns>
        [Log(Title = "SMS Verification Code Log", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        [HttpGet("export")]
        [ActionPermissionFilter(Permission = "smscodelog:export")]
        public IActionResult Export([FromQuery] SmscodeLogQueryDto parm)
        {
            parm.PageNum = 1;
            parm.PageSize = 100000;
            var list = _SmscodeLogService.GetList(parm).Result;
            if (list == null || list.Count <= 0)
            {
                return ToResponse(ResultCode.FAIL, "No data to export");
            }
            var result = ExportExcelMini(list, "SMS Verification Code Log", "SMS Verification Code Log");
            return ExportExcel(result.Item2, result.Item1);
        }
    }
}
