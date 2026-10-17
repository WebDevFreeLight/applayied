using Microsoft.AspNetCore.Mvc;
using ZR.Model.EventManagement.Dto;
using ZR.Model.EventManagement;
using ZR.Service.EventManagement.IEventManagementService;
using MiniExcelLibs;

//创建时间：2025-10-16
namespace ZR.Admin.WebApi.Controllers.EventManagement.Lookup
{
    /// <summary>
    /// Booth Type - أنواع الأكشاك
    /// </summary>
    [Route("EventManagement/EventBoothTypes")]
    public class BoothTypeController : BaseController
    {
        /// <summary>
        /// Booth Type - أنواع الأكشاك接口
        /// </summary>
        private readonly IBoothTypeService _BoothTypeService;

        public BoothTypeController(IBoothTypeService BoothTypeService)
        {
            _BoothTypeService = BoothTypeService;
        }

        /// <summary>
        /// 查询Booth Type - أنواع الأكشاك列表
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "EventManagement:eventboothtypes:list")]
        public IActionResult QueryBoothType([FromQuery] BoothTypeQueryDto parm)
        {
            var response = _BoothTypeService.GetList(parm);
            return SUCCESS(response);
        }


        /// <summary>
        /// 查询Booth Type - أنواع الأكشاك详情
        /// </summary>
        /// <param name="Typeid"></param>
        /// <returns></returns>
        [HttpGet("{Typeid}")]
        [ActionPermissionFilter(Permission = "EventManagement:eventboothtypes:query")]
        public IActionResult GetBoothType(long Typeid)
        {
            var response = _BoothTypeService.GetInfo(Typeid);
            
            var info = response.Adapt<BoothTypeDto>();
            return SUCCESS(info);
        }

        /// <summary>
        /// 添加Booth Type - أنواع الأكشاك
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ActionPermissionFilter(Permission = "EventManagement:eventboothtypes:add")]
        [Log(Title = "Booth Type - أنواع الأكشاك", BusinessType = BusinessType.INSERT)]
        public IActionResult AddBoothType([FromBody] BoothTypeDto parm)
        {
            var modal = parm.Adapt<BoothType>().ToCreate(HttpContext);

            var response = _BoothTypeService.AddBoothType(modal);

            return SUCCESS(response);
        }

        /// <summary>
        /// 更新Booth Type - أنواع الأكشاك
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [ActionPermissionFilter(Permission = "EventManagement:eventboothtypes:edit")]
        [Log(Title = "Booth Type - أنواع الأكشاك", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdateBoothType([FromBody] BoothTypeDto parm)
        {
            var modal = parm.Adapt<BoothType>().ToUpdate(HttpContext);
            var response = _BoothTypeService.UpdateBoothType(modal);

            return ToResponse(response);
        }

        /// <summary>
        /// 删除Booth Type - أنواع الأكشاك
        /// </summary>
        /// <returns></returns>
        [HttpPost("delete/{ids}")]
        [ActionPermissionFilter(Permission = "EventManagement:eventboothtypes:delete")]
        [Log(Title = "Booth Type - أنواع الأكشاك", BusinessType = BusinessType.DELETE)]
        public IActionResult DeleteBoothType([FromRoute]string ids)
        {
            var idArr = Tools.SplitAndConvert<long>(ids);

            return ToResponse(_BoothTypeService.Delete(idArr, "删除Booth Type - أنواع الأكشاك"));
        }

        /// <summary>
        /// 导出Booth Type - أنواع الأكشاك
        /// </summary>
        /// <returns></returns>
        [Log(Title = "Booth Type - أنواع الأكشاك", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        [HttpGet("export")]
        [ActionPermissionFilter(Permission = "EventManagement:eventboothtypes:export")]
        public IActionResult Export([FromQuery] BoothTypeQueryDto parm)
        {
            var list = _BoothTypeService.ExportList(parm).Result;
            if (list == null || list.Count <= 0)
            {
                return ToResponse(ResultCode.FAIL, "没有要导出的数据");
            }
            var result = ExportExcelMini(list, "Booth Type - أنواع الأكشاك", "Booth Type - أنواع الأكشاك");
            return ExportExcel(result.Item2, result.Item1);
        }

        /// <summary>
        /// 清空Booth Type - أنواع الأكشاك
        /// </summary>
        /// <returns></returns>
        [Log(Title = "Booth Type - أنواع الأكشاك", BusinessType = BusinessType.CLEAN)]
        [ActionPermissionFilter(Permission = "EventManagement:eventboothtypes:delete")]
        [HttpPost("clean")]
        public IActionResult Clear()
        {
            if (!HttpContext.IsAdmin())
            {
                return ToResponse(ResultCode.FAIL, "操作失败");
            }
            return SUCCESS(_BoothTypeService.TruncateBoothType());
        }

        /// <summary>
        /// 导入
        /// </summary>
        /// <param name="formFile"></param>
        /// <returns></returns>
        [HttpPost("importData")]
        [Log(Title = "Booth Type - أنواع الأكشاك导入", BusinessType = BusinessType.IMPORT, IsSaveRequestData = false)]
        [ActionPermissionFilter(Permission = "EventManagement:eventboothtypes:import")]
        public IActionResult ImportData([FromForm(Name = "file")] IFormFile formFile)
        {
            List<BoothTypeDto> list = new();
            using (var stream = formFile.OpenReadStream())
            {
                list = stream.Query<BoothTypeDto>(startCell: "A1").ToList();
            }

            return SUCCESS(_BoothTypeService.ImportBoothType(list.Adapt<List<BoothType>>()));
        }

        /// <summary>
        /// Booth Type - أنواع الأكشاك导入模板下载
        /// </summary>
        /// <returns></returns>
        [HttpGet("importTemplate")]
        [Log(Title = "Booth Type - أنواع الأكشاك模板", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        [AllowAnonymous]
        public IActionResult ImportTemplateExcel()
        {
            var result = DownloadImportTemplate(new List<BoothTypeDto>() { }, "BoothType");
            return ExportExcel(result.Item2, result.Item1);
        }

    }
}
