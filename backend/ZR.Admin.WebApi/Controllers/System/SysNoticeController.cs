using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using ZR.Model;
using ZR.Model.System;
using ZR.Model.System.Dto;
using ZR.ServiceCore.Signalr;

namespace ZR.Admin.WebApi.Controllers.System
{
    /// <summary>
    /// System Notice
    /// </summary>
    [Route("system/notice")]
    [ApiExplorerSettings(GroupName = "sys")]
    public class SysNoticeController : BaseController
    {
        /// <summary>
        /// Notice announcement table interface
        /// </summary>
        private readonly ISysNoticeService _SysNoticeService;
        private readonly IHubContext<MessageHub> _hubContext;

        public SysNoticeController(ISysNoticeService SysNoticeService, IHubContext<MessageHub> hubContext)
        {
            _SysNoticeService = SysNoticeService;
            _hubContext = hubContext;
        }

        /// <summary>
        /// Query notice announcement table list (for mobile)
        /// </summary>
        /// <returns></returns>
        [HttpGet("queryNotice")]
        public IActionResult QueryNotice([FromQuery] SysNoticeQueryDto parm)
        {
            var response = _SysNoticeService.GetSysNotices();
            return SUCCESS(response);
        }

        /// <summary>
        /// Query notice announcement table list
        /// </summary>
        /// <returns></returns>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "system:notice:list")]
        public IActionResult QuerySysNotice([FromQuery] SysNoticeQueryDto parm)
        {
            PagedInfo<SysNotice> response = _SysNoticeService.GetPageList(parm);
            return SUCCESS(response);
        }

        /// <summary>
        /// Query notice announcement table details
        /// </summary>
        /// <param name="NoticeId"></param>
        /// <returns></returns>
        [HttpGet("{NoticeId}")]
        public IActionResult GetSysNotice(int NoticeId)
        {
            var response = _SysNoticeService.GetFirst(x => x.NoticeId == NoticeId);

            return SUCCESS(response);
        }

        /// <summary>
        /// Add notice announcement table
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ActionPermissionFilter(Permission = "system:notice:add")]
        [Log(Title = "Publish Notice", BusinessType = BusinessType.INSERT, IsSaveRequestData = false)]
        public IActionResult AddSysNotice([FromBody] SysNoticeDto parm)
        {
            var modal = parm.Adapt<SysNotice>().ToCreate(HttpContext);
            
            int result = _SysNoticeService.InsertReturnIdentity(modal);

            return SUCCESS(result);
        }

        /// <summary>
        /// Update notice announcement table
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [ActionPermissionFilter(Permission = "system:notice:update")]
        [Log(Title = "Modify Notice", BusinessType = BusinessType.UPDATE, IsSaveRequestData = false)]
        public IActionResult UpdateSysNotice([FromBody] SysNoticeDto parm)
        {
            var model = parm.Adapt<SysNotice>().ToUpdate(HttpContext);
            
            var response = _SysNoticeService.Update(model);

            return SUCCESS(response);
        }

        /// <summary>
        /// Send notice announcement table
        /// </summary>
        /// <returns></returns>
        [HttpPut("send/{NoticeId}")]
        [ActionPermissionFilter(Permission = "system:notice:update")]
        [Log(Title = "Send Notice Announcement", BusinessType = BusinessType.OTHER)]
        public IActionResult SendNotice(int NoticeId = 0)
        {
            if (NoticeId <= 0)
            {
                throw new CustomException("Request entity cannot be empty");
            }
            var response = _SysNoticeService.GetFirst(x => x.NoticeId == NoticeId);
            if (response != null && response.Status == 0)
            {
                _hubContext.Clients.All.SendAsync(HubsConstant.ReceiveNotice, response.NoticeTitle, response.NoticeContent);
            }
            return SUCCESS(response);
        }

        /// <summary>
        /// Delete notice announcement table
        /// </summary>
        /// <returns></returns>
        [HttpDelete("{ids}")]
        [ActionPermissionFilter(Permission = "system:notice:delete")]
        [Log(Title = "Notice Announcement", BusinessType = BusinessType.DELETE)]
        public IActionResult DeleteSysNotice(string ids)
        {
            int[] idsArr = Tools.SpitIntArrary(ids);
            if (idsArr.Length <= 0) { return ToResponse(ApiResult.Error($"Delete failed, ID cannot be empty")); }

            var response = _SysNoticeService.Delete(idsArr);

            return SUCCESS(response);
        }

        /// <summary>
        /// Export notice announcement table
        /// </summary>
        /// <returns></returns>
        [Log(Title = "Notice Announcement Table", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        [HttpGet("export")]
        [ActionPermissionFilter(Permission = "system:notice:export")]
        public IActionResult Export([FromQuery] SysNoticeQueryDto parm)
        {
            parm.PageNum = 1;
            parm.PageSize = 100000;
            var list = _SysNoticeService.ExportList(parm).Result;
            if (list == null || list.Count <= 0)
            {
                return ToResponse(ResultCode.FAIL, "No data to export");
            }
            var result = ExportExcelMini(list, "Notice Announcement Table", "Notice Announcement Table");
            return ExportExcel(result.Item2, result.Item1);
        }
    }
}
