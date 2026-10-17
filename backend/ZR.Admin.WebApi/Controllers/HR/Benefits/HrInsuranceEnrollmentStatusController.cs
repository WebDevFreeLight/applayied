using Microsoft.AspNetCore.Mvc;
using ZR.Model.Hr.Dto;
using ZR.Model.Hr;
using ZR.Service.Hr.IHrService;
using MiniExcelLibs;

//创建时间：2025-09-02
namespace ZR.Admin.WebApi.Controllers.HR.Benefits
{
    /// <summary>
    /// hrinsuranceenrollmentstatus
    /// </summary>
    [Route("hr/HrInsuranceEnrollmentStatus")]
    public class HrInsuranceEnrollmentStatusController : BaseController
    {
        /// <summary>
        /// hrinsuranceenrollmentstatus接口
        /// </summary>
        private readonly IHrInsuranceEnrollmentStatusService _HrInsuranceEnrollmentStatusService;

        public HrInsuranceEnrollmentStatusController(IHrInsuranceEnrollmentStatusService HrInsuranceEnrollmentStatusService)
        {
            _HrInsuranceEnrollmentStatusService = HrInsuranceEnrollmentStatusService;
        }

        /// <summary>
        /// 查询hrinsuranceenrollmentstatus列表
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "hrinsuranceenrollmentstatus:list")]
        public IActionResult QueryHrInsuranceEnrollmentStatus([FromQuery] HrInsuranceEnrollmentStatusQueryDto parm)
        {
            var response = _HrInsuranceEnrollmentStatusService.GetList(parm);
            return SUCCESS(response);
        }


        /// <summary>
        /// 查询hrinsuranceenrollmentstatus详情
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpGet("{Id}")]
        [ActionPermissionFilter(Permission = "hrinsuranceenrollmentstatus:query")]
        public IActionResult GetHrInsuranceEnrollmentStatus(long Id)
        {
            var response = _HrInsuranceEnrollmentStatusService.GetInfo(Id);
            
            var info = response.Adapt<HrInsuranceEnrollmentStatusDto>();
            return SUCCESS(info);
        }

        /// <summary>
        /// 添加hrinsuranceenrollmentstatus
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ActionPermissionFilter(Permission = "hrinsuranceenrollmentstatus:add")]
        [Log(Title = "hrinsuranceenrollmentstatus", BusinessType = BusinessType.INSERT)]
        public IActionResult AddHrInsuranceEnrollmentStatus([FromBody] HrInsuranceEnrollmentStatusDto parm)
        {
            var modal = parm.Adapt<HrInsuranceEnrollmentStatus>().ToCreate(HttpContext);

            var response = _HrInsuranceEnrollmentStatusService.AddHrInsuranceEnrollmentStatus(modal);

            return SUCCESS(response);
        }

        /// <summary>
        /// 更新hrinsuranceenrollmentstatus
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [ActionPermissionFilter(Permission = "hrinsuranceenrollmentstatus:edit")]
        [Log(Title = "hrinsuranceenrollmentstatus", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdateHrInsuranceEnrollmentStatus([FromBody] HrInsuranceEnrollmentStatusDto parm)
        {
            var modal = parm.Adapt<HrInsuranceEnrollmentStatus>().ToUpdate(HttpContext);
            var response = _HrInsuranceEnrollmentStatusService.UpdateHrInsuranceEnrollmentStatus(modal);

            return ToResponse(response);
        }

        /// <summary>
        /// 删除hrinsuranceenrollmentstatus
        /// </summary>
        /// <returns></returns>
        [HttpPost("delete/{ids}")]
        [ActionPermissionFilter(Permission = "hrinsuranceenrollmentstatus:delete")]
        [Log(Title = "hrinsuranceenrollmentstatus", BusinessType = BusinessType.DELETE)]
        public IActionResult DeleteHrInsuranceEnrollmentStatus([FromRoute]string ids)
        {
            var idArr = Tools.SplitAndConvert<long>(ids);

            return ToResponse(_HrInsuranceEnrollmentStatusService.Delete(idArr, "删除hrinsuranceenrollmentstatus"));
        }

        /// <summary>
        /// 导出hrinsuranceenrollmentstatus
        /// </summary>
        /// <returns></returns>
        [Log(Title = "hrinsuranceenrollmentstatus", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        [HttpGet("export")]
        [ActionPermissionFilter(Permission = "hrinsuranceenrollmentstatus:export")]
        public IActionResult Export([FromQuery] HrInsuranceEnrollmentStatusQueryDto parm)
        {
            var list = _HrInsuranceEnrollmentStatusService.ExportList(parm).Result;
            if (list == null || list.Count <= 0)
            {
                return ToResponse(ResultCode.FAIL, "没有要导出的数据");
            }
            var result = ExportExcelMini(list, "hrinsuranceenrollmentstatus", "hrinsuranceenrollmentstatus");
            return ExportExcel(result.Item2, result.Item1);
        }

        /// <summary>
        /// 清空hrinsuranceenrollmentstatus
        /// </summary>
        /// <returns></returns>
        [Log(Title = "hrinsuranceenrollmentstatus", BusinessType = BusinessType.CLEAN)]
        [ActionPermissionFilter(Permission = "hrinsuranceenrollmentstatus:delete")]
        [HttpPost("clean")]
        public IActionResult Clear()
        {
            if (!HttpContextExtension.IsAdmin(HttpContext))
            {
                return ToResponse(ResultCode.FAIL, "操作失败");
            }
            return SUCCESS(_HrInsuranceEnrollmentStatusService.TruncateHrInsuranceEnrollmentStatus());
        }

        /// <summary>
        /// 导入
        /// </summary>
        /// <param name="formFile"></param>
        /// <returns></returns>
        [HttpPost("importData")]
        [Log(Title = "hrinsuranceenrollmentstatus导入", BusinessType = BusinessType.IMPORT, IsSaveRequestData = false)]
        [ActionPermissionFilter(Permission = "hrinsuranceenrollmentstatus:import")]
        public IActionResult ImportData([FromForm(Name = "file")] IFormFile formFile)
        {
            List<HrInsuranceEnrollmentStatusDto> list = new();
            using (var stream = formFile.OpenReadStream())
            {
                list = stream.Query<HrInsuranceEnrollmentStatusDto>(startCell: "A1").ToList();
            }

            return SUCCESS(_HrInsuranceEnrollmentStatusService.ImportHrInsuranceEnrollmentStatus(list.Adapt<List<HrInsuranceEnrollmentStatus>>()));
        }

        /// <summary>
        /// hrinsuranceenrollmentstatus导入模板下载
        /// </summary>
        /// <returns></returns>
        [HttpGet("importTemplate")]
        [Log(Title = "hrinsuranceenrollmentstatus模板", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        [AllowAnonymous]
        public IActionResult ImportTemplateExcel()
        {
            var result = DownloadImportTemplate(new List<HrInsuranceEnrollmentStatusDto>() { }, "HrInsuranceEnrollmentStatus");
            return ExportExcel(result.Item2, result.Item1);
        }

    }
}
