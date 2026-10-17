using Microsoft.AspNetCore.Mvc;
using ZR.Model.EventManagement.Dto;
using ZR.Model.EventManagement;
using ZR.Service.EventManagement.IEventManagementService;
using MiniExcelLibs;

//创建时间：2025-10-22
namespace ZR.Admin.WebApi.Controllers.EventManagement
{
    /// <summary>
    /// Event Gate - بوابات الدخول
    /// </summary>
    [Route("EventManagement/EventGates")]
    public class EventGateController : BaseController
    {
        /// <summary>
        /// Event Gate - بوابات الدخول接口
        /// </summary>
        private readonly IEventGateService _EventGateService;

        public EventGateController(IEventGateService EventGateService)
        {
            _EventGateService = EventGateService;
        }

        /// <summary>
        /// 查询Event Gate - بوابات الدخول列表
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "EventManagement:eventgates:list")]
        public IActionResult QueryEventGate([FromQuery] EventGateQueryDto parm)
        {
            var response = _EventGateService.GetList(parm);
            return SUCCESS(response);
        }


        /// <summary>
        /// 查询Event Gate - بوابات الدخول详情
        /// </summary>
        /// <param name="Gateid"></param>
        /// <returns></returns>
        [HttpGet("{Gateid}")]
        [ActionPermissionFilter(Permission = "EventManagement:eventgates:query")]
        public IActionResult GetEventGate(long Gateid)
        {
            var response = _EventGateService.GetInfo(Gateid);
            
            var info = response.Adapt<EventGateDto>();
            return SUCCESS(info);
        }

        /// <summary>
        /// 添加Event Gate - بوابات الدخول
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ActionPermissionFilter(Permission = "EventManagement:eventgates:add")]
        [Log(Title = "Event Gate - بوابات الدخول", BusinessType = BusinessType.INSERT)]
        public IActionResult AddEventGate([FromBody] EventGateDto parm)
        {
            var modal = parm.Adapt<EventGate>().ToCreate(HttpContext);

            var response = _EventGateService.AddEventGate(modal);

            return SUCCESS(response);
        }

        /// <summary>
        /// 更新Event Gate - بوابات الدخول
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [ActionPermissionFilter(Permission = "EventManagement:eventgates:edit")]
        [Log(Title = "Event Gate - بوابات الدخول", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdateEventGate([FromBody] EventGateDto parm)
        {
            var modal = parm.Adapt<EventGate>().ToUpdate(HttpContext);
            var response = _EventGateService.UpdateEventGate(modal);

            return ToResponse(response);
        }

        /// <summary>
        /// 删除Event Gate - بوابات الدخول
        /// </summary>
        /// <returns></returns>
        [HttpPost("delete/{ids}")]
        [ActionPermissionFilter(Permission = "EventManagement:eventgates:delete")]
        [Log(Title = "Event Gate - بوابات الدخول", BusinessType = BusinessType.DELETE)]
        public IActionResult DeleteEventGate([FromRoute]string ids)
        {
            var idArr = Tools.SplitAndConvert<long>(ids);

            return ToResponse(_EventGateService.Delete(idArr, "删除Event Gate - بوابات الدخول"));
        }

        /// <summary>
        /// 导出Event Gate - بوابات الدخول
        /// </summary>
        /// <returns></returns>
        [Log(Title = "Event Gate - بوابات الدخول", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        [HttpGet("export")]
        [ActionPermissionFilter(Permission = "EventManagement:eventgates:export")]
        public IActionResult Export([FromQuery] EventGateQueryDto parm)
        {
            var list = _EventGateService.ExportList(parm).Result;
            if (list == null || list.Count <= 0)
            {
                return ToResponse(ResultCode.FAIL, "没有要导出的数据");
            }
            var result = ExportExcelMini(list, "Event Gate - بوابات الدخول", "Event Gate - بوابات الدخول");
            return ExportExcel(result.Item2, result.Item1);
        }

        /// <summary>
        /// 清空Event Gate - بوابات الدخول
        /// </summary>
        /// <returns></returns>
        [Log(Title = "Event Gate - بوابات الدخول", BusinessType = BusinessType.CLEAN)]
        [ActionPermissionFilter(Permission = "EventManagement:eventgates:delete")]
        [HttpPost("clean")]
        public IActionResult Clear()
        {
            if (!HttpContextExtension.IsAdmin(HttpContext))
            {
                return ToResponse(ResultCode.FAIL, "操作失败");
            }
            return SUCCESS(_EventGateService.TruncateEventGate());
        }

        /// <summary>
        /// 导入
        /// </summary>
        /// <param name="formFile"></param>
        /// <returns></returns>
        [HttpPost("importData")]
        [Log(Title = "Event Gate - بوابات الدخول导入", BusinessType = BusinessType.IMPORT, IsSaveRequestData = false)]
        [ActionPermissionFilter(Permission = "EventManagement:eventgates:import")]
        public IActionResult ImportData([FromForm(Name = "file")] IFormFile formFile)
        {
            List<EventGateDto> list = new();
            using (var stream = formFile.OpenReadStream())
            {
                list = stream.Query<EventGateDto>(startCell: "A1").ToList();
            }

            return SUCCESS(_EventGateService.ImportEventGate(list.Adapt<List<EventGate>>()));
        }

        /// <summary>
        /// Event Gate - بوابات الدخول导入模板下载
        /// </summary>
        /// <returns></returns>
        [HttpGet("importTemplate")]
        [Log(Title = "Event Gate - بوابات الدخول模板", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        [AllowAnonymous]
        public IActionResult ImportTemplateExcel()
        {
            var result = DownloadImportTemplate(new List<EventGateDto>() { }, "EventGate");
            return ExportExcel(result.Item2, result.Item1);
        }


        /// <summary>
        /// Get gates by event ID
        /// </summary>
        [HttpGet("byEvent/{eventId}")]
        [ActionPermissionFilter(Permission = "eventgate:list")]
        public IActionResult GetGatesByEvent(long eventId)
        {
            return SUCCESS(_EventGateService.GetGatesByEventId(eventId));
        }


        /// <summary>
        /// Update gate status
        /// </summary>
        [HttpPost("updateStatus")]
        [ActionPermissionFilter(Permission = "eventgate:edit")]
        [Log(Title = "Event Gate Status", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdateGateStatus([FromBody] UpdateGateStatusDto request)
        {
            return ToResponse(_EventGateService.UpdateGateStatus(request.GateId, request.Status));
        }

        /// <summary>
        /// Update queue count
        /// </summary>
        [HttpPost("updateQueueCount")]
        [ActionPermissionFilter(Permission = "eventgate:edit")]
        public IActionResult UpdateQueueCount([FromBody] UpdateQueueCountDto request)
        {
            return ToResponse(_EventGateService.UpdateQueueCount(request.GateId, request.QueueCount));
        }


    }
}
