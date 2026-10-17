using Microsoft.AspNetCore.Mvc;
using ZR.Model.HR.Hrinsurancetype.Dto;
using ZR.Model.HR.Hrinsurancetype;
using ZR.Service.HR.Hrinsurancetype.IHrinsurancetypeService;
using MiniExcelLibs;

//创建时间：2025-09-02
namespace ZR.Admin.WebApi.Controllers.HR.Hrinsurancetype
{
    /// <summary>
    /// hrinsuranceplanype
    /// </summary>
    [Route("hrinsurancetype/HrInsurancePlanYpe")]
    public class HrInsurancePlanYpeController : BaseController
    {
        /// <summary>
        /// hrinsuranceplanype接口
        /// </summary>
        private readonly IHrInsurancePlanYpeService _HrInsurancePlanYpeService;

        public HrInsurancePlanYpeController(IHrInsurancePlanYpeService HrInsurancePlanYpeService)
        {
            _HrInsurancePlanYpeService = HrInsurancePlanYpeService;
        }

        /// <summary>
        /// 查询hrinsuranceplanype列表
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "hrinsuranceplanype:list")]
        public IActionResult QueryHrInsurancePlanYpe([FromQuery] HrInsurancePlanYpeQueryDto parm)
        {
            var response = _HrInsurancePlanYpeService.GetList(parm);
            return SUCCESS(response);
        }


        /// <summary>
        /// 查询hrinsuranceplanype详情
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpGet("{Id}")]
        [ActionPermissionFilter(Permission = "hrinsuranceplanype:query")]
        public IActionResult GetHrInsurancePlanYpe(long Id)
        {
            var response = _HrInsurancePlanYpeService.GetInfo(Id);
            
            var info = response.Adapt<HrInsurancePlanYpeDto>();
            return SUCCESS(info);
        }

        /// <summary>
        /// 添加hrinsuranceplanype
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ActionPermissionFilter(Permission = "hrinsuranceplanype:add")]
        [Log(Title = "hrinsuranceplanype", BusinessType = BusinessType.INSERT)]
        public IActionResult AddHrInsurancePlanYpe([FromBody] HrInsurancePlanYpeDto parm)
        {
            var modal = parm.Adapt<HrInsurancePlanYpe>().ToCreate(HttpContext);

            var response = _HrInsurancePlanYpeService.AddHrInsurancePlanYpe(modal);

            return SUCCESS(response);
        }

        /// <summary>
        /// 更新hrinsuranceplanype
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [ActionPermissionFilter(Permission = "hrinsuranceplanype:edit")]
        [Log(Title = "hrinsuranceplanype", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdateHrInsurancePlanYpe([FromBody] HrInsurancePlanYpeDto parm)
        {
            var modal = parm.Adapt<HrInsurancePlanYpe>().ToUpdate(HttpContext);
            var response = _HrInsurancePlanYpeService.UpdateHrInsurancePlanYpe(modal);

            return ToResponse(response);
        }

        /// <summary>
        /// 删除hrinsuranceplanype
        /// </summary>
        /// <returns></returns>
        [HttpPost("delete/{ids}")]
        [ActionPermissionFilter(Permission = "hrinsuranceplanype:delete")]
        [Log(Title = "hrinsuranceplanype", BusinessType = BusinessType.DELETE)]
        public IActionResult DeleteHrInsurancePlanYpe([FromRoute]string ids)
        {
            var idArr = Tools.SplitAndConvert<long>(ids);

            return ToResponse(_HrInsurancePlanYpeService.Delete(idArr, "删除hrinsuranceplanype"));
        }

        /// <summary>
        /// 导出hrinsuranceplanype
        /// </summary>
        /// <returns></returns>
        [Log(Title = "hrinsuranceplanype", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        [HttpGet("export")]
        [ActionPermissionFilter(Permission = "hrinsuranceplanype:export")]
        public IActionResult Export([FromQuery] HrInsurancePlanYpeQueryDto parm)
        {
            var list = _HrInsurancePlanYpeService.ExportList(parm).Result;
            if (list == null || list.Count <= 0)
            {
                return ToResponse(ResultCode.FAIL, "没有要导出的数据");
            }
            var result = ExportExcelMini(list, "hrinsuranceplanype", "hrinsuranceplanype");
            return ExportExcel(result.Item2, result.Item1);
        }

        /// <summary>
        /// 清空hrinsuranceplanype
        /// </summary>
        /// <returns></returns>
        [Log(Title = "hrinsuranceplanype", BusinessType = BusinessType.CLEAN)]
        [ActionPermissionFilter(Permission = "hrinsuranceplanype:delete")]
        [HttpPost("clean")]
        public IActionResult Clear()
        {
            if (!HttpContextExtension.IsAdmin(HttpContext))
            {
                return ToResponse(ResultCode.FAIL, "操作失败");
            }
            return SUCCESS(_HrInsurancePlanYpeService.TruncateHrInsurancePlanYpe());
        }

        /// <summary>
        /// 导入
        /// </summary>
        /// <param name="formFile"></param>
        /// <returns></returns>
        [HttpPost("importData")]
        [Log(Title = "hrinsuranceplanype导入", BusinessType = BusinessType.IMPORT, IsSaveRequestData = false)]
        [ActionPermissionFilter(Permission = "hrinsuranceplanype:import")]
        public IActionResult ImportData([FromForm(Name = "file")] IFormFile formFile)
        {
            List<HrInsurancePlanYpeDto> list = new();
            using (var stream = formFile.OpenReadStream())
            {
                list = stream.Query<HrInsurancePlanYpeDto>(startCell: "A1").ToList();
            }

            return SUCCESS(_HrInsurancePlanYpeService.ImportHrInsurancePlanYpe(list.Adapt<List<HrInsurancePlanYpe>>()));
        }

        /// <summary>
        /// hrinsuranceplanype导入模板下载
        /// </summary>
        /// <returns></returns>
        [HttpGet("importTemplate")]
        [Log(Title = "hrinsuranceplanype模板", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        [AllowAnonymous]
        public IActionResult ImportTemplateExcel()
        {
            var result = DownloadImportTemplate(new List<HrInsurancePlanYpeDto>() { }, "HrInsurancePlanYpe");
            return ExportExcel(result.Item2, result.Item1);
        }

    }
}
