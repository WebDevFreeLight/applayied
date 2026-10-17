using Microsoft.AspNetCore.Mvc;
using ZR.Model.HR.Hrtypes.Dto;
using ZR.Model.HR.Hrtypes;
using ZR.Service.HR.Hrtypes.IHrtypesService;
using MiniExcelLibs;

//创建时间：2025-09-01
namespace ZR.Admin.WebApi.Controllers.HR.Hrtypes
{
    /// <summary>
    /// HolidayType Table
    /// </summary>
    [Route("hrtypes/HrHolidaytype")]
    public class HrHolidaytypeController : BaseController
    {
        /// <summary>
        /// HolidayType Table接口
        /// </summary>
        private readonly IHrHolidaytypeService _HrHolidaytypeService;

        public HrHolidaytypeController(IHrHolidaytypeService HrHolidaytypeService)
        {
            _HrHolidaytypeService = HrHolidaytypeService;
        }

        /// <summary>
        /// 查询HolidayType Table列表
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "hrholidaytype:list")]
        public IActionResult QueryHrHolidaytype([FromQuery] HrHolidaytypeQueryDto parm)
        {
            var response = _HrHolidaytypeService.GetList(parm);
            return SUCCESS(response);
        }


        /// <summary>
        /// 查询HolidayType Table详情
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpGet("{Id}")]
        [ActionPermissionFilter(Permission = "hrholidaytype:query")]
        public IActionResult GetHrHolidaytype(long Id)
        {
            var response = _HrHolidaytypeService.GetInfo(Id);
            
            var info = response.Adapt<HrHolidaytypeDto>();
            return SUCCESS(info);
        }

        /// <summary>
        /// 添加HolidayType Table
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ActionPermissionFilter(Permission = "hrholidaytype:add")]
        [Log(Title = "HolidayType Table", BusinessType = BusinessType.INSERT)]
        public IActionResult AddHrHolidaytype([FromBody] HrHolidaytypeDto parm)
        {
            var modal = parm.Adapt<HrHolidaytype>().ToCreate(HttpContext);

            var response = _HrHolidaytypeService.AddHrHolidaytype(modal);

            return SUCCESS(response);
        }

        /// <summary>
        /// 更新HolidayType Table
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [ActionPermissionFilter(Permission = "hrholidaytype:edit")]
        [Log(Title = "HolidayType Table", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdateHrHolidaytype([FromBody] HrHolidaytypeDto parm)
        {
            var modal = parm.Adapt<HrHolidaytype>().ToUpdate(HttpContext);
            var response = _HrHolidaytypeService.UpdateHrHolidaytype(modal);

            return ToResponse(response);
        }

        /// <summary>
        /// 删除HolidayType Table
        /// </summary>
        /// <returns></returns>
        [HttpPost("delete/{ids}")]
        [ActionPermissionFilter(Permission = "hrholidaytype:delete")]
        [Log(Title = "HolidayType Table", BusinessType = BusinessType.DELETE)]
        public IActionResult DeleteHrHolidaytype([FromRoute]string ids)
        {
            var idArr = Tools.SplitAndConvert<long>(ids);

            return ToResponse(_HrHolidaytypeService.Delete(idArr, "删除HolidayType Table"));
        }

        /// <summary>
        /// 导出HolidayType Table
        /// </summary>
        /// <returns></returns>
        [Log(Title = "HolidayType Table", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        [HttpGet("export")]
        [ActionPermissionFilter(Permission = "hrholidaytype:export")]
        public IActionResult Export([FromQuery] HrHolidaytypeQueryDto parm)
        {
            var list = _HrHolidaytypeService.ExportList(parm).Result;
            if (list == null || list.Count <= 0)
            {
                return ToResponse(ResultCode.FAIL, "没有要导出的数据");
            }
            var result = ExportExcelMini(list, "HolidayType Table", "HolidayType Table");
            return ExportExcel(result.Item2, result.Item1);
        }

        /// <summary>
        /// 清空HolidayType Table
        /// </summary>
        /// <returns></returns>
        [Log(Title = "HolidayType Table", BusinessType = BusinessType.CLEAN)]
        [ActionPermissionFilter(Permission = "hrholidaytype:delete")]
        [HttpPost("clean")]
        public IActionResult Clear()
        {
            if (!HttpContextExtension.IsAdmin(HttpContext))
            {
                return ToResponse(ResultCode.FAIL, "操作失败");
            }
            return SUCCESS(_HrHolidaytypeService.TruncateHrHolidaytype());
        }

        /// <summary>
        /// 导入
        /// </summary>
        /// <param name="formFile"></param>
        /// <returns></returns>
        [HttpPost("importData")]
        [Log(Title = "HolidayType Table导入", BusinessType = BusinessType.IMPORT, IsSaveRequestData = false)]
        [ActionPermissionFilter(Permission = "hrholidaytype:import")]
        public IActionResult ImportData([FromForm(Name = "file")] IFormFile formFile)
        {
            List<HrHolidaytypeDto> list = new();
            using (var stream = formFile.OpenReadStream())
            {
                list = stream.Query<HrHolidaytypeDto>(startCell: "A1").ToList();
            }

            return SUCCESS(_HrHolidaytypeService.ImportHrHolidaytype(list.Adapt<List<HrHolidaytype>>()));
        }

        /// <summary>
        /// HolidayType Table导入模板下载
        /// </summary>
        /// <returns></returns>
        [HttpGet("importTemplate")]
        [Log(Title = "HolidayType Table模板", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        [AllowAnonymous]
        public IActionResult ImportTemplateExcel()
        {
            var result = DownloadImportTemplate(new List<HrHolidaytypeDto>() { }, "HrHolidaytype");
            return ExportExcel(result.Item2, result.Item1);
        }

    }
}
