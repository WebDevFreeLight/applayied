using Microsoft.AspNetCore.Mvc;
using ZR.Model.Hr.Dto;
using ZR.Service.Hr.IHrService;
using MiniExcelLibs;
using ZR.Model.HR.Payroll;

//创建时间：2025-09-01
namespace ZR.Admin.WebApi.Controllers.HR.Payroll
{
    /// <summary>
    /// Salary Compliance - امتثال الرواتب
    /// </summary>
    [Route("hr/HrSalaryCompliance")]
    public class HrSalaryComplianceController : BaseController
    {
        /// <summary>
        /// Salary Compliance - امتثال الرواتب接口
        /// </summary>
        private readonly IHrSalaryComplianceService _HrSalaryComplianceService;

        public HrSalaryComplianceController(IHrSalaryComplianceService HrSalaryComplianceService)
        {
            _HrSalaryComplianceService = HrSalaryComplianceService;
        }

        /// <summary>
        /// 查询Salary Compliance - امتثال الرواتب列表
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "hrsalarycompliance:list")]
        public IActionResult QueryHrSalaryCompliance([FromQuery] HrSalaryComplianceQueryDto parm)
        {
            var response = _HrSalaryComplianceService.GetList(parm);
            return SUCCESS(response);
        }


        /// <summary>
        /// 查询Salary Compliance - امتثال الرواتب详情
        /// </summary>
        /// <param name="Complianceid"></param>
        /// <returns></returns>
        [HttpGet("{Complianceid}")]
        [ActionPermissionFilter(Permission = "hrsalarycompliance:query")]
        public IActionResult GetHrSalaryCompliance(long Complianceid)
        {
            var response = _HrSalaryComplianceService.GetInfo(Complianceid);
            
            var info = response.Adapt<HrSalaryComplianceDto>();
            return SUCCESS(info);
        }

        /// <summary>
        /// 添加Salary Compliance - امتثال الرواتب
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ActionPermissionFilter(Permission = "hrsalarycompliance:add")]
        [Log(Title = "Salary Compliance - امتثال الرواتب", BusinessType = BusinessType.INSERT)]
        public IActionResult AddHrSalaryCompliance([FromBody] HrSalaryComplianceDto parm)
        {
            var modal = parm.Adapt<HrSalaryCompliance>().ToCreate(HttpContext);

            var response = _HrSalaryComplianceService.AddHrSalaryCompliance(modal);

            return SUCCESS(response);
        }

        /// <summary>
        /// 更新Salary Compliance - امتثال الرواتب
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [ActionPermissionFilter(Permission = "hrsalarycompliance:edit")]
        [Log(Title = "Salary Compliance - امتثال الرواتب", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdateHrSalaryCompliance([FromBody] HrSalaryComplianceDto parm)
        {
            var modal = parm.Adapt<HrSalaryCompliance>().ToUpdate(HttpContext);
            var response = _HrSalaryComplianceService.UpdateHrSalaryCompliance(modal);

            return ToResponse(response);
        }

        /// <summary>
        /// 删除Salary Compliance - امتثال الرواتب
        /// </summary>
        /// <returns></returns>
        [HttpPost("delete/{ids}")]
        [ActionPermissionFilter(Permission = "hrsalarycompliance:delete")]
        [Log(Title = "Salary Compliance - امتثال الرواتب", BusinessType = BusinessType.DELETE)]
        public IActionResult DeleteHrSalaryCompliance([FromRoute]string ids)
        {
            var idArr = Tools.SplitAndConvert<long>(ids);

            return ToResponse(_HrSalaryComplianceService.Delete(idArr, "删除Salary Compliance - امتثال الرواتب"));
        }

        /// <summary>
        /// 导出Salary Compliance - امتثال الرواتب
        /// </summary>
        /// <returns></returns>
        [Log(Title = "Salary Compliance - امتثال الرواتب", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        [HttpGet("export")]
        [ActionPermissionFilter(Permission = "hrsalarycompliance:export")]
        public IActionResult Export([FromQuery] HrSalaryComplianceQueryDto parm)
        {
            var list = _HrSalaryComplianceService.ExportList(parm).Result;
            if (list == null || list.Count <= 0)
            {
                return ToResponse(ResultCode.FAIL, "没有要导出的数据");
            }
            var result = ExportExcelMini(list, "امتثال الرواتب", "Salary Compliance - امتثال الرواتب");
            return ExportExcel(result.Item2, result.Item1);
        }

        /// <summary>
        /// 清空Salary Compliance - امتثال الرواتب
        /// </summary>
        /// <returns></returns>
        [Log(Title = "Salary Compliance - امتثال الرواتب", BusinessType = BusinessType.CLEAN)]
        [ActionPermissionFilter(Permission = "hrsalarycompliance:delete")]
        [HttpPost("clean")]
        public IActionResult Clear()
        {
            if (!HttpContextExtension.IsAdmin(HttpContext))
            {
                return ToResponse(ResultCode.FAIL, "操作失败");
            }
            return SUCCESS(_HrSalaryComplianceService.TruncateHrSalaryCompliance());
        }

        /// <summary>
        /// 导入
        /// </summary>
        /// <param name="formFile"></param>
        /// <returns></returns>
        [HttpPost("importData")]
        [Log(Title = "Salary Compliance - امتثال الرواتب导入", BusinessType = BusinessType.IMPORT, IsSaveRequestData = false)]
        [ActionPermissionFilter(Permission = "hrsalarycompliance:import")]
        public IActionResult ImportData([FromForm(Name = "file")] IFormFile formFile)
        {
            List<HrSalaryComplianceDto> list = new();
            using (var stream = formFile.OpenReadStream())
            {
                list = stream.Query<HrSalaryComplianceDto>(startCell: "A1").ToList();
            }

            return SUCCESS(_HrSalaryComplianceService.ImportHrSalaryCompliance(list.Adapt<List<HrSalaryCompliance>>()));
        }

        /// <summary>
        /// Salary Compliance - امتثال الرواتب导入模板下载
        /// </summary>
        /// <returns></returns>
        [HttpGet("importTemplate")]
        [Log(Title = "Salary Compliance - امتثال الرواتب模板", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        [AllowAnonymous]
        public IActionResult ImportTemplateExcel()
        {
            var result = DownloadImportTemplate(new List<HrSalaryComplianceDto>() { }, "HrSalaryCompliance");
            return ExportExcel(result.Item2, result.Item1);
        }

    }
}
