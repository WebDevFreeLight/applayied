using Microsoft.AspNetCore.Mvc;
using ZR.Model.EventManagement.Dto;
using ZR.Model.EventManagement;
using ZR.Service.EventManagement.IEventManagementService;
using MiniExcelLibs;

//创建时间：2025-10-15
namespace ZR.Admin.WebApi.Controllers.EventManagement.Lookup
{
    /// <summary>
    /// Gate Type - أنواع البوابات
    /// </summary>
    [Route("EventManagement/EventGateTypes")]
    public class EventGateTypeController : BaseController
    {
        /// <summary>
        /// Gate Type - أنواع البوابات接口
        /// </summary>
        private readonly IEventGateTypeService _EventGateTypeService;

        public EventGateTypeController(IEventGateTypeService EventGateTypeService)
        {
            _EventGateTypeService = EventGateTypeService;
        }

        /// <summary>
        /// 查询Gate Type - أنواع البوابات列表
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "EventManagement:eventgatetypes:list")]
        public IActionResult QueryEventGateType([FromQuery] EventGateTypeQueryDto parm)
        {
            var response = _EventGateTypeService.GetList(parm);
            return SUCCESS(response);
        }


        /// <summary>
        /// 查询Gate Type - أنواع البوابات详情
        /// </summary>
        /// <param name="Typeid"></param>
        /// <returns></returns>
        [HttpGet("{Typeid}")]
        [ActionPermissionFilter(Permission = "EventManagement:eventgatetypes:query")]
        public IActionResult GetEventGateType(long Typeid)
        {
            var response = _EventGateTypeService.GetInfo(Typeid);
            
            var info = response.Adapt<EventGateTypeDto>();
            return SUCCESS(info);
        }

        /// <summary>
        /// 添加Gate Type - أنواع البوابات
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ActionPermissionFilter(Permission = "EventManagement:eventgatetypes:add")]
        [Log(Title = "Gate Type - أنواع البوابات", BusinessType = BusinessType.INSERT)]
        public IActionResult AddEventGateType([FromBody] EventGateTypeDto parm)
        {
            var modal = parm.Adapt<EventGateType>().ToCreate(HttpContext);

            var response = _EventGateTypeService.AddEventGateType(modal);

            return SUCCESS(response);
        }

        /// <summary>
        /// 更新Gate Type - أنواع البوابات
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [ActionPermissionFilter(Permission = "EventManagement:eventgatetypes:edit")]
        [Log(Title = "Gate Type - أنواع البوابات", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdateEventGateType([FromBody] EventGateTypeDto parm)
        {
            var modal = parm.Adapt<EventGateType>().ToUpdate(HttpContext);
            var response = _EventGateTypeService.UpdateEventGateType(modal);

            return ToResponse(response);
        }

        /// <summary>
        /// 删除Gate Type - أنواع البوابات
        /// </summary>
        /// <returns></returns>
        [HttpPost("delete/{ids}")]
        [ActionPermissionFilter(Permission = "EventManagement:eventgatetypes:delete")]
        [Log(Title = "Gate Type - أنواع البوابات", BusinessType = BusinessType.DELETE)]
        public IActionResult DeleteEventGateType([FromRoute]string ids)
        {
            var idArr = Tools.SplitAndConvert<long>(ids);

            return ToResponse(_EventGateTypeService.Delete(idArr, "删除Gate Type - أنواع البوابات"));
        }

        /// <summary>
        /// 导出Gate Type - أنواع البوابات
        /// </summary>
        /// <returns></returns>
        [Log(Title = "Gate Type - أنواع البوابات", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        [HttpGet("export")]
        [ActionPermissionFilter(Permission = "EventManagement:eventgatetypes:export")]
        public IActionResult Export([FromQuery] EventGateTypeQueryDto parm)
        {
            var list = _EventGateTypeService.ExportList(parm).Result;
            if (list == null || list.Count <= 0)
            {
                return ToResponse(ResultCode.FAIL, "没有要导出的数据");
            }
            var result = ExportExcelMini(list, "Gate Type - أنواع البوابات", "Gate Type - أنواع البوابات");
            return ExportExcel(result.Item2, result.Item1);
        }

        /// <summary>
        /// 清空Gate Type - أنواع البوابات
        /// </summary>
        /// <returns></returns>
        [Log(Title = "Gate Type - أنواع البوابات", BusinessType = BusinessType.CLEAN)]
        [ActionPermissionFilter(Permission = "EventManagement:eventgatetypes:delete")]
        [HttpPost("clean")]
        public IActionResult Clear()
        {
            if (!HttpContext.IsAdmin())
            {
                return ToResponse(ResultCode.FAIL, "操作失败");
            }
            return SUCCESS(_EventGateTypeService.TruncateEventGateType());
        }

        /// <summary>
        /// 导入
        /// </summary>
        /// <param name="formFile"></param>
        /// <returns></returns>
        [HttpPost("importData")]
        [Log(Title = "Gate Type - أنواع البوابات导入", BusinessType = BusinessType.IMPORT, IsSaveRequestData = false)]
        [ActionPermissionFilter(Permission = "EventManagement:eventgatetypes:import")]
        public IActionResult ImportData([FromForm(Name = "file")] IFormFile formFile)
        {
            List<EventGateTypeDto> list = new();
            using (var stream = formFile.OpenReadStream())
            {
                list = stream.Query<EventGateTypeDto>(startCell: "A1").ToList();
            }

            return SUCCESS(_EventGateTypeService.ImportEventGateType(list.Adapt<List<EventGateType>>()));
        }

        /// <summary>
        /// Gate Type - أنواع البوابات导入模板下载
        /// </summary>
        /// <returns></returns>
        [HttpGet("importTemplate")]
        [Log(Title = "Gate Type - أنواع البوابات模板", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        [AllowAnonymous]
        public IActionResult ImportTemplateExcel()
        {
            var result = DownloadImportTemplate(new List<EventGateTypeDto>() { }, "EventGateType");
            return ExportExcel(result.Item2, result.Item1);
        }

    }
}
