using Microsoft.AspNetCore.Mvc;
using ZR.Model.HR.Business.Dto;
using ZR.Model.HR.Business;
using ZR.Service.HR.Business.IBusinessService;
using MiniExcelLibs;

//创建时间：2025-09-02
namespace ZR.Admin.WebApi.Controllers.HR.Business
{
    /// <summary>
    /// Insurance Plans Table
    /// </summary>
    [Route("business/HrInsurancePlan")]
    public class HrInsurancePlanController : BaseController
    {
        /// <summary>
        /// Insurance Plans Table接口
        /// </summary>
        private readonly IHrInsurancePlanService _HrInsurancePlanService;

        public HrInsurancePlanController(IHrInsurancePlanService HrInsurancePlanService)
        {
            _HrInsurancePlanService = HrInsurancePlanService;
        }

        /// <summary>
        /// 查询Insurance Plans Table列表
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "hrinsuranceplan:list")]
        public IActionResult QueryHrInsurancePlan([FromQuery] HrInsurancePlanQueryDto parm)
        {
            var response = _HrInsurancePlanService.GetList(parm);
            return SUCCESS(response);
        }


        /// <summary>
        /// 查询Insurance Plans Table详情
        /// </summary>
        /// <param name="Planid"></param>
        /// <returns></returns>
        [HttpGet("{Planid}")]
        [ActionPermissionFilter(Permission = "hrinsuranceplan:query")]
        public IActionResult GetHrInsurancePlan(long Planid)
        {
            var response = _HrInsurancePlanService.GetInfo(Planid);
            
            var info = response.Adapt<HrInsurancePlanDto>();
            return SUCCESS(info);
        }

        /// <summary>
        /// 添加Insurance Plans Table
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ActionPermissionFilter(Permission = "hrinsuranceplan:add")]
        [Log(Title = "Insurance Plans Table", BusinessType = BusinessType.INSERT)]
        public IActionResult AddHrInsurancePlan([FromBody] HrInsurancePlanDto parm)
        {
            var modal = parm.Adapt<HrInsurancePlan>().ToCreate(HttpContext);

            var response = _HrInsurancePlanService.AddHrInsurancePlan(modal);

            return SUCCESS(response);
        }

        /// <summary>
        /// 更新Insurance Plans Table
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [ActionPermissionFilter(Permission = "hrinsuranceplan:edit")]
        [Log(Title = "Insurance Plans Table", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdateHrInsurancePlan([FromBody] HrInsurancePlanDto parm)
        {
            var modal = parm.Adapt<HrInsurancePlan>().ToUpdate(HttpContext);
            var response = _HrInsurancePlanService.UpdateHrInsurancePlan(modal);

            return ToResponse(response);
        }

        /// <summary>
        /// 删除Insurance Plans Table
        /// </summary>
        /// <returns></returns>
        [HttpPost("delete/{ids}")]
        [ActionPermissionFilter(Permission = "hrinsuranceplan:delete")]
        [Log(Title = "Insurance Plans Table", BusinessType = BusinessType.DELETE)]
        public IActionResult DeleteHrInsurancePlan([FromRoute]string ids)
        {
            var idArr = Tools.SplitAndConvert<long>(ids);

            return ToResponse(_HrInsurancePlanService.Delete(idArr, "删除Insurance Plans Table"));
        }

        /// <summary>
        /// 导出Insurance Plans Table
        /// </summary>
        /// <returns></returns>
        [Log(Title = "Insurance Plans Table", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        [HttpGet("export")]
        [ActionPermissionFilter(Permission = "hrinsuranceplan:export")]
        public IActionResult Export([FromQuery] HrInsurancePlanQueryDto parm)
        {
            var list = _HrInsurancePlanService.ExportList(parm).Result;
            if (list == null || list.Count <= 0)
            {
                return ToResponse(ResultCode.FAIL, "没有要导出的数据");
            }
            var result = ExportExcelMini(list, "Insurance Plans Table", "Insurance Plans Table");
            return ExportExcel(result.Item2, result.Item1);
        }

        /// <summary>
        /// 清空Insurance Plans Table
        /// </summary>
        /// <returns></returns>
        [Log(Title = "Insurance Plans Table", BusinessType = BusinessType.CLEAN)]
        [ActionPermissionFilter(Permission = "hrinsuranceplan:delete")]
        [HttpPost("clean")]
        public IActionResult Clear()
        {
            if (!HttpContextExtension.IsAdmin(HttpContext))
            {
                return ToResponse(ResultCode.FAIL, "操作失败");
            }
            return SUCCESS(_HrInsurancePlanService.TruncateHrInsurancePlan());
        }

        /// <summary>
        /// 导入
        /// </summary>
        /// <param name="formFile"></param>
        /// <returns></returns>
        [HttpPost("importData")]
        [Log(Title = "Insurance Plans Table导入", BusinessType = BusinessType.IMPORT, IsSaveRequestData = false)]
        [ActionPermissionFilter(Permission = "hrinsuranceplan:import")]
        public IActionResult ImportData([FromForm(Name = "file")] IFormFile formFile)
        {
            List<HrInsurancePlanDto> list = new();
            using (var stream = formFile.OpenReadStream())
            {
                list = stream.Query<HrInsurancePlanDto>(startCell: "A1").ToList();
            }

            return SUCCESS(_HrInsurancePlanService.ImportHrInsurancePlan(list.Adapt<List<HrInsurancePlan>>()));
        }

        /// <summary>
        /// Insurance Plans Table导入模板下载
        /// </summary>
        /// <returns></returns>
        [HttpGet("importTemplate")]
        [Log(Title = "Insurance Plans Table模板", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        [AllowAnonymous]
        public IActionResult ImportTemplateExcel()
        {
            var result = DownloadImportTemplate(new List<HrInsurancePlanDto>() { }, "HrInsurancePlan");
            return ExportExcel(result.Item2, result.Item1);
        }

    }
}
