using Microsoft.AspNetCore.Mvc;
using ZR.Model.EventManagement.Dto;
using ZR.Model.EventManagement;
using ZR.Service.EventManagement.IEventManagementService;
using MiniExcelLibs;

//创建时间：2025-10-19
namespace ZR.Admin.WebApi.Controllers.EventManagement.Lookup
{
    /// <summary>
    /// Event Status - أنواع حالة الفعاليات
    /// </summary>
    [Route("EventManagement/EventStatus")]
    public class EventStatusController : BaseController
    {
        /// <summary>
        /// Event Status - أنواع حالة الفعاليات接口
        /// </summary>
        private readonly IEventStatusService _EventStatusService;

        public EventStatusController(IEventStatusService EventStatusService)
        {
            _EventStatusService = EventStatusService;
        }

        /// <summary>
        /// 查询Event Status - أنواع حالة الفعاليات列表
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "EventManagement:eventstatus:list")]
        public IActionResult QueryEventStatus([FromQuery] EventStatusQueryDto parm)
        {
            var response = _EventStatusService.GetList(parm);
            return SUCCESS(response);
        }


        /// <summary>
        /// 查询Event Status - أنواع حالة الفعاليات详情
        /// </summary>
        /// <param name="Typeid"></param>
        /// <returns></returns>
        [HttpGet("{Typeid}")]
        [ActionPermissionFilter(Permission = "EventManagement:eventstatus:query")]
        public IActionResult GetEventStatus(long Typeid)
        {
            var response = _EventStatusService.GetInfo(Typeid);
            
            var info = response.Adapt<EventStatusDto>();
            return SUCCESS(info);
        }

        /// <summary>
        /// 添加Event Status - أنواع حالة الفعاليات
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ActionPermissionFilter(Permission = "EventManagement:eventstatus:add")]
        [Log(Title = "Event Status - أنواع حالة الفعاليات", BusinessType = BusinessType.INSERT)]
        public IActionResult AddEventStatus([FromBody] EventStatusDto parm)
        {
            var modal = parm.Adapt<EventStatus>().ToCreate(HttpContext);

            var response = _EventStatusService.AddEventStatus(modal);

            return SUCCESS(response);
        }

        /// <summary>
        /// 更新Event Status - أنواع حالة الفعاليات
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [ActionPermissionFilter(Permission = "EventManagement:eventstatus:edit")]
        [Log(Title = "Event Status - أنواع حالة الفعاليات", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdateEventStatus([FromBody] EventStatusDto parm)
        {
            var modal = parm.Adapt<EventStatus>().ToUpdate(HttpContext);
            var response = _EventStatusService.UpdateEventStatus(modal);

            return ToResponse(response);
        }

        /// <summary>
        /// 删除Event Status - أنواع حالة الفعاليات
        /// </summary>
        /// <returns></returns>
        [HttpPost("delete/{ids}")]
        [ActionPermissionFilter(Permission = "EventManagement:eventstatus:delete")]
        [Log(Title = "Event Status - أنواع حالة الفعاليات", BusinessType = BusinessType.DELETE)]
        public IActionResult DeleteEventStatus([FromRoute]string ids)
        {
            var idArr = Tools.SplitAndConvert<long>(ids);

            return ToResponse(_EventStatusService.Delete(idArr, "删除Event Status - أنواع حالة الفعاليات"));
        }

        /// <summary>
        /// 导出Event Status - أنواع حالة الفعاليات
        /// </summary>
        /// <returns></returns>
        [Log(Title = "Event Status - أنواع حالة الفعاليات", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        [HttpGet("export")]
        [ActionPermissionFilter(Permission = "EventManagement:eventstatus:export")]
        public IActionResult Export([FromQuery] EventStatusQueryDto parm)
        {
            var list = _EventStatusService.ExportList(parm).Result;
            if (list == null || list.Count <= 0)
            {
                return ToResponse(ResultCode.FAIL, "没有要导出的数据");
            }
            var result = ExportExcelMini(list, "Event Status-حالة الفعاليات", "Event Status - أنواع حالة الفعاليات");
            return ExportExcel(result.Item2, result.Item1);
        }

        /// <summary>
        /// 清空Event Status - أنواع حالة الفعاليات
        /// </summary>
        /// <returns></returns>
        [Log(Title = "Event Status - أنواع حالة الفعاليات", BusinessType = BusinessType.CLEAN)]
        [ActionPermissionFilter(Permission = "EventManagement:eventstatus:delete")]
        [HttpPost("clean")]
        public IActionResult Clear()
        {
            if (!HttpContext.IsAdmin())
            {
                return ToResponse(ResultCode.FAIL, "操作失败");
            }
            return SUCCESS(_EventStatusService.TruncateEventStatus());
        }

        /// <summary>
        /// 导入
        /// </summary>
        /// <param name="formFile"></param>
        /// <returns></returns>
        [HttpPost("importData")]
        [Log(Title = "Event Status - أنواع حالة الفعاليات导入", BusinessType = BusinessType.IMPORT, IsSaveRequestData = false)]
        [ActionPermissionFilter(Permission = "EventManagement:eventstatus:import")]
        public IActionResult ImportData([FromForm(Name = "file")] IFormFile formFile)
        {
            List<EventStatusDto> list = new();
            using (var stream = formFile.OpenReadStream())
            {
                list = stream.Query<EventStatusDto>(startCell: "A1").ToList();
            }

            return SUCCESS(_EventStatusService.ImportEventStatus(list.Adapt<List<EventStatus>>()));
        }

        /// <summary>
        /// Event Status - أنواع حالة الفعاليات导入模板下载
        /// </summary>
        /// <returns></returns>
        [HttpGet("importTemplate")]
        [Log(Title = "Event Status - أنواع حالة الفعاليات模板", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        [AllowAnonymous]
        public IActionResult ImportTemplateExcel()
        {
            var result = DownloadImportTemplate(new List<EventStatusDto>() { }, "EventStatus");
            return ExportExcel(result.Item2, result.Item1);
        }

    }
}
