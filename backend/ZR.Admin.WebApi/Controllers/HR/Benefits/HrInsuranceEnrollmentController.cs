using Microsoft.AspNetCore.Mvc;
using ZR.Model.Hr.Dto;
using ZR.Model.Hr;
using ZR.Service.Hr.IHrService;
using MiniExcelLibs;

//创建时间：2025-09-02
namespace ZR.Admin.WebApi.Controllers.HR.Benefits
{
    /// <summary>
    /// insuranceenrollment
    /// </summary>
    [Route("hr/HrInsuranceEnrollment")]
    public class HrInsuranceEnrollmentController : BaseController
    {
        /// <summary>
        /// insuranceenrollment接口
        /// </summary>
        private readonly IHrInsuranceEnrollmentService _HrInsuranceEnrollmentService;

        public HrInsuranceEnrollmentController(IHrInsuranceEnrollmentService HrInsuranceEnrollmentService)
        {
            _HrInsuranceEnrollmentService = HrInsuranceEnrollmentService;
        }

        /// <summary>
        /// 查询insuranceenrollment列表
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "hrinsuranceenrollment:list")]
        public IActionResult QueryHrInsuranceEnrollment([FromQuery] HrInsuranceEnrollmentQueryDto parm)
        {
            var response = _HrInsuranceEnrollmentService.GetList(parm);
            return SUCCESS(response);
        }


        /// <summary>
        /// 查询insuranceenrollment详情
        /// </summary>
        /// <param name="Enrollmentid"></param>
        /// <returns></returns>
        [HttpGet("{Enrollmentid}")]
        [ActionPermissionFilter(Permission = "hrinsuranceenrollment:query")]
        public IActionResult GetHrInsuranceEnrollment(long Enrollmentid)
        {
            var response = _HrInsuranceEnrollmentService.GetInfo(Enrollmentid);
            
            var info = response.Adapt<HrInsuranceEnrollmentDto>();
            return SUCCESS(info);
        }

        /// <summary>
        /// 添加insuranceenrollment
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ActionPermissionFilter(Permission = "hrinsuranceenrollment:add")]
        [Log(Title = "insuranceenrollment", BusinessType = BusinessType.INSERT)]
        public IActionResult AddHrInsuranceEnrollment([FromBody] HrInsuranceEnrollmentDto parm)
        {
            var modal = parm.Adapt<HrInsuranceEnrollment>().ToCreate(HttpContext);

            var response = _HrInsuranceEnrollmentService.AddHrInsuranceEnrollment(modal);

            return SUCCESS(response);
        }

        /// <summary>
        /// 更新insuranceenrollment
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [ActionPermissionFilter(Permission = "hrinsuranceenrollment:edit")]
        [Log(Title = "insuranceenrollment", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdateHrInsuranceEnrollment([FromBody] HrInsuranceEnrollmentDto parm)
        {
            var modal = parm.Adapt<HrInsuranceEnrollment>().ToUpdate(HttpContext);
            var response = _HrInsuranceEnrollmentService.UpdateHrInsuranceEnrollment(modal);

            return ToResponse(response);
        }

        /// <summary>
        /// 删除insuranceenrollment
        /// </summary>
        /// <returns></returns>
        [HttpPost("delete/{ids}")]
        [ActionPermissionFilter(Permission = "hrinsuranceenrollment:delete")]
        [Log(Title = "insuranceenrollment", BusinessType = BusinessType.DELETE)]
        public IActionResult DeleteHrInsuranceEnrollment([FromRoute]string ids)
        {
            var idArr = Tools.SplitAndConvert<long>(ids);

            return ToResponse(_HrInsuranceEnrollmentService.Delete(idArr, "删除insuranceenrollment"));
        }

        /// <summary>
        /// 导出insuranceenrollment
        /// </summary>
        /// <returns></returns>
        [Log(Title = "insuranceenrollment", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        [HttpGet("export")]
        [ActionPermissionFilter(Permission = "hrinsuranceenrollment:export")]
        public IActionResult Export([FromQuery] HrInsuranceEnrollmentQueryDto parm)
        {
            var list = _HrInsuranceEnrollmentService.ExportList(parm).Result;
            if (list == null || list.Count <= 0)
            {
                return ToResponse(ResultCode.FAIL, "没有要导出的数据");
            }
            var result = ExportExcelMini(list, "insuranceenrollment", "insuranceenrollment");
            return ExportExcel(result.Item2, result.Item1);
        }

        /// <summary>
        /// 清空insuranceenrollment
        /// </summary>
        /// <returns></returns>
        [Log(Title = "insuranceenrollment", BusinessType = BusinessType.CLEAN)]
        [ActionPermissionFilter(Permission = "hrinsuranceenrollment:delete")]
        [HttpPost("clean")]
        public IActionResult Clear()
        {
            if (!HttpContextExtension.IsAdmin(HttpContext))
            {
                return ToResponse(ResultCode.FAIL, "操作失败");
            }
            return SUCCESS(_HrInsuranceEnrollmentService.TruncateHrInsuranceEnrollment());
        }

        /// <summary>
        /// 导入
        /// </summary>
        /// <param name="formFile"></param>
        /// <returns></returns>
        [HttpPost("importData")]
        [Log(Title = "insuranceenrollment导入", BusinessType = BusinessType.IMPORT, IsSaveRequestData = false)]
        [ActionPermissionFilter(Permission = "hrinsuranceenrollment:import")]
        public IActionResult ImportData([FromForm(Name = "file")] IFormFile formFile)
        {
            List<HrInsuranceEnrollmentDto> list = new();
            using (var stream = formFile.OpenReadStream())
            {
                list = stream.Query<HrInsuranceEnrollmentDto>(startCell: "A1").ToList();
            }

            return SUCCESS(_HrInsuranceEnrollmentService.ImportHrInsuranceEnrollment(list.Adapt<List<HrInsuranceEnrollment>>()));
        }

        /// <summary>
        /// insuranceenrollment导入模板下载
        /// </summary>
        /// <returns></returns>
        [HttpGet("importTemplate")]
        [Log(Title = "insuranceenrollment模板", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        [AllowAnonymous]
        public IActionResult ImportTemplateExcel()
        {
            var result = DownloadImportTemplate(new List<HrInsuranceEnrollmentDto>() { }, "HrInsuranceEnrollment");
            return ExportExcel(result.Item2, result.Item1);
        }

    }
}
