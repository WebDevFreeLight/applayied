using Microsoft.AspNetCore.Mvc;
using ZR.Model.EventManagement.Dto;
using ZR.Model.EventManagement;
using ZR.Service.EventManagement.IEventManagementService;
using MiniExcelLibs;

//创建时间：2025-10-16
namespace ZR.Admin.WebApi.Controllers.EventManagement.Lookup
{
    /// <summary>
    /// Ticket Type - أنواع التذاكر
    /// </summary>
    [Route("EventManagement/EventTicketTypes")]
    public class TicketTypeController : BaseController
    {
        /// <summary>
        /// Ticket Type - أنواع التذاكر接口
        /// </summary>
        private readonly ITicketTypeService _TicketTypeService;

        public TicketTypeController(ITicketTypeService TicketTypeService)
        {
            _TicketTypeService = TicketTypeService;
        }

        /// <summary>
        /// 查询Ticket Type - أنواع التذاكر列表
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "EventManagement:eventtickettypes:list")]
        public IActionResult QueryTicketType([FromQuery] TicketTypeQueryDto parm)
        {
            var response = _TicketTypeService.GetList(parm);
            return SUCCESS(response);
        }


        /// <summary>
        /// 查询Ticket Type - أنواع التذاكر详情
        /// </summary>
        /// <param name="Typeid"></param>
        /// <returns></returns>
        [HttpGet("{Typeid}")]
        [ActionPermissionFilter(Permission = "EventManagement:eventtickettypes:query")]
        public IActionResult GetTicketType(long Typeid)
        {
            var response = _TicketTypeService.GetInfo(Typeid);
            
            var info = response.Adapt<TicketTypeDto>();
            return SUCCESS(info);
        }

        /// <summary>
        /// 添加Ticket Type - أنواع التذاكر
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ActionPermissionFilter(Permission = "EventManagement:eventtickettypes:add")]
        [Log(Title = "Ticket Type - أنواع التذاكر", BusinessType = BusinessType.INSERT)]
        public IActionResult AddTicketType([FromBody] TicketTypeDto parm)
        {
            var modal = parm.Adapt<TicketType>().ToCreate(HttpContext);

            var response = _TicketTypeService.AddTicketType(modal);

            return SUCCESS(response);
        }

        /// <summary>
        /// 更新Ticket Type - أنواع التذاكر
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [ActionPermissionFilter(Permission = "EventManagement:eventtickettypes:edit")]
        [Log(Title = "Ticket Type - أنواع التذاكر", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdateTicketType([FromBody] TicketTypeDto parm)
        {
            var modal = parm.Adapt<TicketType>().ToUpdate(HttpContext);
            var response = _TicketTypeService.UpdateTicketType(modal);

            return ToResponse(response);
        }

        /// <summary>
        /// 删除Ticket Type - أنواع التذاكر
        /// </summary>
        /// <returns></returns>
        [HttpPost("delete/{ids}")]
        [ActionPermissionFilter(Permission = "EventManagement:eventtickettypes:delete")]
        [Log(Title = "Ticket Type - أنواع التذاكر", BusinessType = BusinessType.DELETE)]
        public IActionResult DeleteTicketType([FromRoute]string ids)
        {
            var idArr = Tools.SplitAndConvert<long>(ids);

            return ToResponse(_TicketTypeService.Delete(idArr, "删除Ticket Type - أنواع التذاكر"));
        }

        /// <summary>
        /// 导出Ticket Type - أنواع التذاكر
        /// </summary>
        /// <returns></returns>
        [Log(Title = "Ticket Type - أنواع التذاكر", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        [HttpGet("export")]
        [ActionPermissionFilter(Permission = "EventManagement:eventtickettypes:export")]
        public IActionResult Export([FromQuery] TicketTypeQueryDto parm)
        {
            var list = _TicketTypeService.ExportList(parm).Result;
            if (list == null || list.Count <= 0)
            {
                return ToResponse(ResultCode.FAIL, "没有要导出的数据");
            }
            var result = ExportExcelMini(list, "Ticket Type - أنواع التذاكر", "Ticket Type - أنواع التذاكر");
            return ExportExcel(result.Item2, result.Item1);
        }

        /// <summary>
        /// 清空Ticket Type - أنواع التذاكر
        /// </summary>
        /// <returns></returns>
        [Log(Title = "Ticket Type - أنواع التذاكر", BusinessType = BusinessType.CLEAN)]
        [ActionPermissionFilter(Permission = "EventManagement:eventtickettypes:delete")]
        [HttpPost("clean")]
        public IActionResult Clear()
        {
            if (!HttpContext.IsAdmin())
            {
                return ToResponse(ResultCode.FAIL, "操作失败");
            }
            return SUCCESS(_TicketTypeService.TruncateTicketType());
        }

        /// <summary>
        /// 导入
        /// </summary>
        /// <param name="formFile"></param>
        /// <returns></returns>
        [HttpPost("importData")]
        [Log(Title = "Ticket Type - أنواع التذاكر导入", BusinessType = BusinessType.IMPORT, IsSaveRequestData = false)]
        [ActionPermissionFilter(Permission = "EventManagement:eventtickettypes:import")]
        public IActionResult ImportData([FromForm(Name = "file")] IFormFile formFile)
        {
            List<TicketTypeDto> list = new();
            using (var stream = formFile.OpenReadStream())
            {
                list = stream.Query<TicketTypeDto>(startCell: "A1").ToList();
            }

            return SUCCESS(_TicketTypeService.ImportTicketType(list.Adapt<List<TicketType>>()));
        }

        /// <summary>
        /// Ticket Type - أنواع التذاكر导入模板下载
        /// </summary>
        /// <returns></returns>
        [HttpGet("importTemplate")]
        [Log(Title = "Ticket Type - أنواع التذاكر模板", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        [AllowAnonymous]
        public IActionResult ImportTemplateExcel()
        {
            var result = DownloadImportTemplate(new List<TicketTypeDto>() { }, "TicketType");
            return ExportExcel(result.Item2, result.Item1);
        }

    }
}
