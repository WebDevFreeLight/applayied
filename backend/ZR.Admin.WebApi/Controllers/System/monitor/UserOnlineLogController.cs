using Microsoft.AspNetCore.Mvc;
using ZR.Model.Dto;
using ZR.ServiceCore.Monitor.IMonitorService;

//Created: 2024-03-27
namespace ZR.Admin.WebApi.Controllers
{
    /// <summary>
    /// User online duration
    /// </summary>
    [ApiExplorerSettings(GroupName = "sys")]
    [Route("monitor/UserOnlineLog")]
    public class UserOnlineLogController : BaseController
    {
        /// <summary>
        /// User online duration interface
        /// </summary>
        private readonly IUserOnlineLogService _UserOnlineLogService;

        public UserOnlineLogController(IUserOnlineLogService UserOnlineLogService)
        {
            _UserOnlineLogService = UserOnlineLogService;
        }

        /// <summary>
        /// Query user online duration list
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        [HttpGet("list")]
        //[ActionPermissionFilter(Permission = "useronlinelog:list")]
        public IActionResult QueryUserOnlineLog([FromQuery] UserOnlineLogQueryDto parm)
        {
            var response = _UserOnlineLogService.GetList(parm);
            return SUCCESS(response);
        }

        /// <summary>
        /// Delete user online duration
        /// </summary>
        /// <returns></returns>
        [HttpDelete("delete/{ids}")]
        [ActionPermissionFilter(Permission = "useronlinelog:delete")]
        [Log(Title = "User Online Duration", BusinessType = BusinessType.DELETE)]
        public IActionResult DeleteUserOnlineLog([FromRoute]string ids)
        {
            var idArr = Tools.SplitAndConvert<long>(ids);

            return ToResponse(_UserOnlineLogService.Delete(idArr));
        }

        /// <summary>
        /// Export user online duration
        /// </summary>
        /// <returns></returns>
        [Log(Title = "User Online Duration", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        [HttpGet("export")]
        [ActionPermissionFilter(Permission = "useronlinelog:export")]
        public IActionResult Export([FromQuery] UserOnlineLogQueryDto parm)
        {
            parm.PageNum = 1;
            parm.PageSize = 100000;
            var list = _UserOnlineLogService.ExportList(parm).Result;
            if (list == null || list.Count <= 0)
            {
                return ToResponse(ResultCode.FAIL, "No data to export");
            }
            var result = ExportExcelMini(list, "User Online Duration", "User Online Duration");
            return ExportExcel(result.Item2, result.Item1);
        }

    }
}
