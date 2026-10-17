using Microsoft.AspNetCore.Mvc;
using ZR.Model.EventManagement.Dto;
using ZR.Model.EventManagement;
using ZR.Service.EventManagement.IEventManagementService;
using MiniExcelLibs;

//创建时间：2025-10-19
namespace ZR.Admin.WebApi.Controllers.EventManagement.Lookup
{
    /// <summary>
    /// Event Location Information - معلومات موقع الفعالية
    /// </summary>
    [Route("EventManagement/EventLocations")]
    public class EventLocationController : BaseController
    {
        /// <summary>
        /// Event Location Information - معلومات موقع الفعالية接口
        /// </summary>
        private readonly IEventLocationService _EventLocationService;

        public EventLocationController(IEventLocationService EventLocationService)
        {
            _EventLocationService = EventLocationService;
        }

        /// <summary>
        /// 查询Event Location Information - معلومات موقع الفعالية列表
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "EventManagement:eventlocations:list")]
        public IActionResult QueryEventLocation([FromQuery] EventLocationQueryDto parm)
        {
            var response = _EventLocationService.GetList(parm);
            return SUCCESS(response);
        }


        /// <summary>
        /// 查询Event Location Information - معلومات موقع الفعالية详情
        /// </summary>
        /// <param name="Locationid"></param>
        /// <returns></returns>
        [HttpGet("{Locationid}")]
        [ActionPermissionFilter(Permission = "EventManagement:eventlocations:query")]
        public IActionResult GetEventLocation(long Locationid)
        {
            var response = _EventLocationService.GetInfo(Locationid);
            
            var info = response.Adapt<EventLocationDto>();
            return SUCCESS(info);
        }

        /// <summary>
        /// 添加Event Location Information - معلومات موقع الفعالية
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ActionPermissionFilter(Permission = "EventManagement:eventlocations:add")]
        [Log(Title = "Event Location Information - معلومات موقع الفعالية", BusinessType = BusinessType.INSERT)]
        public IActionResult AddEventLocation([FromBody] EventLocationDto parm)
        {
            var modal = parm.Adapt<EventLocation>().ToCreate(HttpContext);

            var response = _EventLocationService.AddEventLocation(modal);

            return SUCCESS(response);
        }

        /// <summary>
        /// 更新Event Location Information - معلومات موقع الفعالية
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [ActionPermissionFilter(Permission = "EventManagement:eventlocations:edit")]
        [Log(Title = "Event Location Information - معلومات موقع الفعالية", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdateEventLocation([FromBody] EventLocationDto parm)
        {
            var modal = parm.Adapt<EventLocation>().ToUpdate(HttpContext);
            var response = _EventLocationService.UpdateEventLocation(modal);

            return ToResponse(response);
        }

        /// <summary>
        /// 删除Event Location Information - معلومات موقع الفعالية
        /// </summary>
        /// <returns></returns>
        [HttpPost("delete/{ids}")]
        [ActionPermissionFilter(Permission = "EventManagement:eventlocations:delete")]
        [Log(Title = "Event Location Information - معلومات موقع الفعالية", BusinessType = BusinessType.DELETE)]
        public IActionResult DeleteEventLocation([FromRoute]string ids)
        {
            var idArr = Tools.SplitAndConvert<long>(ids);

            return ToResponse(_EventLocationService.Delete(idArr, "删除Event Location Information - معلومات موقع الفعالية"));
        }

        /// <summary>
        /// 导出Event Location Information - معلومات موقع الفعالية
        /// </summary>
        /// <returns></returns>
        [Log(Title = "Event Location Information - معلومات موقع الفعالية", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        [HttpGet("export")]
        [ActionPermissionFilter(Permission = "EventManagement:eventlocations:export")]
        public IActionResult Export([FromQuery] EventLocationQueryDto parm)
        {
            var list = _EventLocationService.ExportList(parm).Result;
            if (list == null || list.Count <= 0)
            {
                return ToResponse(ResultCode.FAIL, "没有要导出的数据");
            }
            var result = ExportExcelMini(list, "معلومات موقع الفعالية", "معلومات موقع الفعالية");
            return ExportExcel(result.Item2, result.Item1);
        }

        /// <summary>
        /// 清空Event Location Information - معلومات موقع الفعالية
        /// </summary>
        /// <returns></returns>
        [Log(Title = "Event Location Information - معلومات موقع الفعالية", BusinessType = BusinessType.CLEAN)]
        [ActionPermissionFilter(Permission = "EventManagement:eventlocations:delete")]
        [HttpPost("clean")]
        public IActionResult Clear()
        {
            if (!HttpContext.IsAdmin())
            {
                return ToResponse(ResultCode.FAIL, "操作失败");
            }
            return SUCCESS(_EventLocationService.TruncateEventLocation());
        }

        /// <summary>
        /// 导入
        /// </summary>
        /// <param name="formFile"></param>
        /// <returns></returns>
        [HttpPost("importData")]
        [Log(Title = "Event Location Information - معلومات موقع الفعالية导入", BusinessType = BusinessType.IMPORT, IsSaveRequestData = false)]
        [ActionPermissionFilter(Permission = "EventManagement:eventlocations:import")]
        public IActionResult ImportData([FromForm(Name = "file")] IFormFile formFile)
        {
            List<EventLocationDto> list = new();
            using (var stream = formFile.OpenReadStream())
            {
                list = stream.Query<EventLocationDto>(startCell: "A1").ToList();
            }

            return SUCCESS(_EventLocationService.ImportEventLocation(list.Adapt<List<EventLocation>>()));
        }

        /// <summary>
        /// Event Location Information - معلومات موقع الفعالية导入模板下载
        /// </summary>
        /// <returns></returns>
        [HttpGet("importTemplate")]
        [Log(Title = "Event Location Information - معلومات موقع الفعالية模板", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        [AllowAnonymous]
        public IActionResult ImportTemplateExcel()
        {
            var result = DownloadImportTemplate(new List<EventLocationDto>() { }, "EventLocation");
            return ExportExcel(result.Item2, result.Item1);
        }

    }
}
