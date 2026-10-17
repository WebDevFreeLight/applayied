using Microsoft.AspNetCore.Mvc;
using ZR.Model.EventManagement.Dto;
using ZR.Model.EventManagement;
using ZR.Service.EventManagement.IEventManagementService;
using MiniExcelLibs;

//创建时间：2025-10-22
namespace ZR.Admin.WebApi.Controllers.EventManagement
{
    /// <summary>
    /// Event Information - معلومات الفعالية
    /// </summary>
    [Route("EventManagement/EventEvents")]
    public class EventController : BaseController
    {
        /// <summary>
        /// Event Information - معلومات الفعالية接口
        /// </summary>
        private readonly IEventService _EventService;

        public EventController(IEventService EventService)
        {
            _EventService = EventService;
        }

        /// <summary>
        /// 查询Event Information - معلومات الفعالية列表
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "EventManagement:eventevents:list")]
        public IActionResult QueryEvent([FromQuery] EventQueryDto parm)
        {
            var response = _EventService.GetList(parm);
            return SUCCESS(response);
        }


        /// <summary>
        /// 查询Event Information - معلومات الفعالية详情
        /// </summary>
        /// <param name="Eventid"></param>
        /// <returns></returns>
        [HttpGet("{Eventid}")]
        [ActionPermissionFilter(Permission = "EventManagement:eventevents:query")]
        public IActionResult GetEvent(long Eventid)
        {
            var response = _EventService.GetInfo(Eventid);
            
            var info = response.Adapt<EventDto>();
            return SUCCESS(info);
        }

        /// <summary>
        /// 添加Event Information - معلومات الفعالية
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ActionPermissionFilter(Permission = "EventManagement:eventevents:add")]
        [Log(Title = "Event Information - معلومات الفعالية", BusinessType = BusinessType.INSERT)]
        public IActionResult AddEvent([FromBody] EventDto parm)
        {
            var modal = parm.Adapt<Event>().ToCreate(HttpContext);

            var response = _EventService.AddEvent(modal);

            return SUCCESS(response);
        }

        /// <summary>
        /// 更新Event Information - معلومات الفعالية
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [ActionPermissionFilter(Permission = "EventManagement:eventevents:edit")]
        [Log(Title = "Event Information - معلومات الفعالية", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdateEvent([FromBody] EventDto parm)
        {
            var modal = parm.Adapt<Event>().ToUpdate(HttpContext);
            var response = _EventService.UpdateEvent(modal);

            return ToResponse(response);
        }

        /// <summary>
        /// 删除Event Information - معلومات الفعالية
        /// </summary>
        /// <returns></returns>
        [HttpPost("delete/{ids}")]
        [ActionPermissionFilter(Permission = "EventManagement:eventevents:delete")]
        [Log(Title = "Event Information - معلومات الفعالية", BusinessType = BusinessType.DELETE)]
        public IActionResult DeleteEvent([FromRoute]string ids)
        {
            var idArr = Tools.SplitAndConvert<long>(ids);

            return ToResponse(_EventService.Delete(idArr, "删除Event Information - معلومات الفعالية"));
        }

        /// <summary>
        /// 导出Event Information - معلومات الفعالية
        /// </summary>
        /// <returns></returns>
        [Log(Title = "Event Information - معلومات الفعالية", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        [HttpGet("export")]
        [ActionPermissionFilter(Permission = "EventManagement:eventevents:export")]
        public IActionResult Export([FromQuery] EventQueryDto parm)
        {
            var list = _EventService.ExportList(parm).Result;
            if (list == null || list.Count <= 0)
            {
                return ToResponse(ResultCode.FAIL, "没有要导出的数据");
            }
            var result = ExportExcelMini(list, "Event Information - معلومات الفعالية", "Event Information - معلومات الفعالية");
            return ExportExcel(result.Item2, result.Item1);
        }

        /// <summary>
        /// 清空Event Information - معلومات الفعالية
        /// </summary>
        /// <returns></returns>
        [Log(Title = "Event Information - معلومات الفعالية", BusinessType = BusinessType.CLEAN)]
        [ActionPermissionFilter(Permission = "EventManagement:eventevents:delete")]
        [HttpPost("clean")]
        public IActionResult Clear()
        {
            if (!HttpContextExtension.IsAdmin(HttpContext))
            {
                return ToResponse(ResultCode.FAIL, "操作失败");
            }
            return SUCCESS(_EventService.TruncateEvent());
        }

        /// <summary>
        /// 导入
        /// </summary>
        /// <param name="formFile"></param>
        /// <returns></returns>
        [HttpPost("importData")]
        [Log(Title = "Event Information - معلومات الفعالية导入", BusinessType = BusinessType.IMPORT, IsSaveRequestData = false)]
        [ActionPermissionFilter(Permission = "EventManagement:eventevents:import")]
        public IActionResult ImportData([FromForm(Name = "file")] IFormFile formFile)
        {
            List<EventDto> list = new();
            using (var stream = formFile.OpenReadStream())
            {
                list = stream.Query<EventDto>(startCell: "A1").ToList();
            }

            return SUCCESS(_EventService.ImportEvent(list.Adapt<List<Event>>()));
        }

        /// <summary>
        /// Event Information - معلومات الفعالية导入模板下载
        /// </summary>
        /// <returns></returns>
        [HttpGet("importTemplate")]
        [Log(Title = "Event Information - معلومات الفعالية模板", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        [AllowAnonymous]
        public IActionResult ImportTemplateExcel()
        {
            var result = DownloadImportTemplate(new List<EventDto>() { }, "Event");
            return ExportExcel(result.Item2, result.Item1);
        }

    }
}
