using Microsoft.AspNetCore.Mvc;
using ZR.Model.Hr.Dto;
using ZR.Service.Hr.IHrService;
using MiniExcelLibs;
using ZR.Model.HR.Attendance.Configuration;

//创建时间：2025-09-01
namespace ZR.Admin.WebApi.Controllers.HR.Attendance
{
    /// <summary>
    /// Holiday Table
    /// </summary>
    [Route("hr/HrHoliday")]
    public class HrHolidayController : BaseController
    {
        /// <summary>
        /// Holiday Table接口
        /// </summary>
        private readonly IHrHolidayService _HrHolidayService;

        public HrHolidayController(IHrHolidayService HrHolidayService)
        {
            _HrHolidayService = HrHolidayService;
        }

        /// <summary>
        /// 查询Holiday Table列表
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "hrholiday:list")]
        public IActionResult QueryHrHoliday([FromQuery] HrHolidayQueryDto parm)
        {
            var response = _HrHolidayService.GetList(parm);
            return SUCCESS(response);
        }


        /// <summary>
        /// 查询Holiday Table详情
        /// </summary>
        /// <param name="Holidayid"></param>
        /// <returns></returns>
        [HttpGet("{Holidayid}")]
        [ActionPermissionFilter(Permission = "hrholiday:query")]
        public IActionResult GetHrHoliday(long Holidayid)
        {
            var response = _HrHolidayService.GetInfo(Holidayid);
            
            var info = response.Adapt<HrHolidayDto>();
            return SUCCESS(info);
        }

        /// <summary>
        /// 添加Holiday Table
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ActionPermissionFilter(Permission = "hrholiday:add")]
        [Log(Title = "Holiday Table", BusinessType = BusinessType.INSERT)]
        public IActionResult AddHrHoliday([FromBody] HrHolidayDto parm)
        {
            var modal = parm.Adapt<HrHoliday>().ToCreate(HttpContext);

            var response = _HrHolidayService.AddHrHoliday(modal);

            return SUCCESS(response);
        }

        /// <summary>
        /// 更新Holiday Table
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [ActionPermissionFilter(Permission = "hrholiday:edit")]
        [Log(Title = "Holiday Table", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdateHrHoliday([FromBody] HrHolidayDto parm)
        {
            var modal = parm.Adapt<HrHoliday>().ToUpdate(HttpContext);
            var response = _HrHolidayService.UpdateHrHoliday(modal);

            return ToResponse(response);
        }

        /// <summary>
        /// 删除Holiday Table
        /// </summary>
        /// <returns></returns>
        [HttpPost("delete/{ids}")]
        [ActionPermissionFilter(Permission = "hrholiday:delete")]
        [Log(Title = "Holiday Table", BusinessType = BusinessType.DELETE)]
        public IActionResult DeleteHrHoliday([FromRoute]string ids)
        {
            var idArr = Tools.SplitAndConvert<long>(ids);

            return ToResponse(_HrHolidayService.Delete(idArr, "删除Holiday Table"));
        }

        /// <summary>
        /// 导出Holiday Table
        /// </summary>
        /// <returns></returns>
        [Log(Title = "Holiday Table", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        [HttpGet("export")]
        [ActionPermissionFilter(Permission = "hrholiday:export")]
        public IActionResult Export([FromQuery] HrHolidayQueryDto parm)
        {
            var list = _HrHolidayService.ExportList(parm).Result;
            if (list == null || list.Count <= 0)
            {
                return ToResponse(ResultCode.FAIL, "没有要导出的数据");
            }
            var result = ExportExcelMini(list, "Holiday Table", "Holiday Table");
            return ExportExcel(result.Item2, result.Item1);
        }

        /// <summary>
        /// 清空Holiday Table
        /// </summary>
        /// <returns></returns>
        [Log(Title = "Holiday Table", BusinessType = BusinessType.CLEAN)]
        [ActionPermissionFilter(Permission = "hrholiday:delete")]
        [HttpPost("clean")]
        public IActionResult Clear()
        {
            if (!HttpContextExtension.IsAdmin(HttpContext))
            {
                return ToResponse(ResultCode.FAIL, "操作失败");
            }
            return SUCCESS(_HrHolidayService.TruncateHrHoliday());
        }

        /// <summary>
        /// 导入
        /// </summary>
        /// <param name="formFile"></param>
        /// <returns></returns>
        [HttpPost("importData")]
        [Log(Title = "Holiday Table导入", BusinessType = BusinessType.IMPORT, IsSaveRequestData = false)]
        [ActionPermissionFilter(Permission = "hrholiday:import")]
        public IActionResult ImportData([FromForm(Name = "file")] IFormFile formFile)
        {
            List<HrHolidayDto> list = new();
            using (var stream = formFile.OpenReadStream())
            {
                list = stream.Query<HrHolidayDto>(startCell: "A1").ToList();
            }

            return SUCCESS(_HrHolidayService.ImportHrHoliday(list.Adapt<List<HrHoliday>>()));
        }

        /// <summary>
        /// Holiday Table导入模板下载
        /// </summary>
        /// <returns></returns>
        [HttpGet("importTemplate")]
        [Log(Title = "Holiday Table模板", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        [AllowAnonymous]
        public IActionResult ImportTemplateExcel()
        {
            var result = DownloadImportTemplate(new List<HrHolidayDto>() { }, "HrHoliday");
            return ExportExcel(result.Item2, result.Item1);
        }

    }
}
