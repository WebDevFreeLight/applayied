using Microsoft.AspNetCore.Mvc;
using ZR.Model.EventManagement.Dto;
using ZR.Model.EventManagement;
using ZR.Service.EventManagement.IEventManagementService;
using MiniExcelLibs;

//创建时间：2025-10-16
namespace ZR.Admin.WebApi.Controllers.EventManagement.Lookup
{
    /// <summary>
    /// Incident Type - أنواع الحوادث
    /// </summary>
    [Route("EventManagement/EventIncidentTypes")]
    public class IncidentTypeController : BaseController
    {
        /// <summary>
        /// Incident Type - أنواع الحوادث接口
        /// </summary>
        private readonly IIncidentTypeService _IncidentTypeService;

        public IncidentTypeController(IIncidentTypeService IncidentTypeService)
        {
            _IncidentTypeService = IncidentTypeService;
        }

        /// <summary>
        /// 查询Incident Type - أنواع الحوادث列表
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "EventManagement:eventincidenttypes:list")]
        public IActionResult QueryIncidentType([FromQuery] IncidentTypeQueryDto parm)
        {
            var response = _IncidentTypeService.GetList(parm);
            return SUCCESS(response);
        }


        /// <summary>
        /// 查询Incident Type - أنواع الحوادث详情
        /// </summary>
        /// <param name="Typeid"></param>
        /// <returns></returns>
        [HttpGet("{Typeid}")]
        [ActionPermissionFilter(Permission = "EventManagement:eventincidenttypes:query")]
        public IActionResult GetIncidentType(long Typeid)
        {
            var response = _IncidentTypeService.GetInfo(Typeid);
            
            var info = response.Adapt<IncidentTypeDto>();
            return SUCCESS(info);
        }

        /// <summary>
        /// 添加Incident Type - أنواع الحوادث
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ActionPermissionFilter(Permission = "EventManagement:eventincidenttypes:add")]
        [Log(Title = "Incident Type - أنواع الحوادث", BusinessType = BusinessType.INSERT)]
        public IActionResult AddIncidentType([FromBody] IncidentTypeDto parm)
        {
            var modal = parm.Adapt<IncidentType>().ToCreate(HttpContext);

            var response = _IncidentTypeService.AddIncidentType(modal);

            return SUCCESS(response);
        }

        /// <summary>
        /// 更新Incident Type - أنواع الحوادث
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [ActionPermissionFilter(Permission = "EventManagement:eventincidenttypes:edit")]
        [Log(Title = "Incident Type - أنواع الحوادث", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdateIncidentType([FromBody] IncidentTypeDto parm)
        {
            var modal = parm.Adapt<IncidentType>().ToUpdate(HttpContext);
            var response = _IncidentTypeService.UpdateIncidentType(modal);

            return ToResponse(response);
        }

        /// <summary>
        /// 删除Incident Type - أنواع الحوادث
        /// </summary>
        /// <returns></returns>
        [HttpPost("delete/{ids}")]
        [ActionPermissionFilter(Permission = "EventManagement:eventincidenttypes:delete")]
        [Log(Title = "Incident Type - أنواع الحوادث", BusinessType = BusinessType.DELETE)]
        public IActionResult DeleteIncidentType([FromRoute]string ids)
        {
            var idArr = Tools.SplitAndConvert<long>(ids);

            return ToResponse(_IncidentTypeService.Delete(idArr, "删除Incident Type - أنواع الحوادث"));
        }

        /// <summary>
        /// 导出Incident Type - أنواع الحوادث
        /// </summary>
        /// <returns></returns>
        [Log(Title = "Incident Type - أنواع الحوادث", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        [HttpGet("export")]
        [ActionPermissionFilter(Permission = "EventManagement:eventincidenttypes:export")]
        public IActionResult Export([FromQuery] IncidentTypeQueryDto parm)
        {
            var list = _IncidentTypeService.ExportList(parm).Result;
            if (list == null || list.Count <= 0)
            {
                return ToResponse(ResultCode.FAIL, "没有要导出的数据");
            }
            var result = ExportExcelMini(list, "Incident Type - أنواع الحوادث", "Incident Type - أنواع الحوادث");
            return ExportExcel(result.Item2, result.Item1);
        }

        /// <summary>
        /// 清空Incident Type - أنواع الحوادث
        /// </summary>
        /// <returns></returns>
        [Log(Title = "Incident Type - أنواع الحوادث", BusinessType = BusinessType.CLEAN)]
        [ActionPermissionFilter(Permission = "EventManagement:eventincidenttypes:delete")]
        [HttpPost("clean")]
        public IActionResult Clear()
        {
            if (!HttpContext.IsAdmin())
            {
                return ToResponse(ResultCode.FAIL, "操作失败");
            }
            return SUCCESS(_IncidentTypeService.TruncateIncidentType());
        }

        /// <summary>
        /// 导入
        /// </summary>
        /// <param name="formFile"></param>
        /// <returns></returns>
        [HttpPost("importData")]
        [Log(Title = "Incident Type - أنواع الحوادث导入", BusinessType = BusinessType.IMPORT, IsSaveRequestData = false)]
        [ActionPermissionFilter(Permission = "EventManagement:eventincidenttypes:import")]
        public IActionResult ImportData([FromForm(Name = "file")] IFormFile formFile)
        {
            List<IncidentTypeDto> list = new();
            using (var stream = formFile.OpenReadStream())
            {
                list = stream.Query<IncidentTypeDto>(startCell: "A1").ToList();
            }

            return SUCCESS(_IncidentTypeService.ImportIncidentType(list.Adapt<List<IncidentType>>()));
        }

        /// <summary>
        /// Incident Type - أنواع الحوادث导入模板下载
        /// </summary>
        /// <returns></returns>
        [HttpGet("importTemplate")]
        [Log(Title = "Incident Type - أنواع الحوادث模板", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        [AllowAnonymous]
        public IActionResult ImportTemplateExcel()
        {
            var result = DownloadImportTemplate(new List<IncidentTypeDto>() { }, "IncidentType");
            return ExportExcel(result.Item2, result.Item1);
        }

    }
}
