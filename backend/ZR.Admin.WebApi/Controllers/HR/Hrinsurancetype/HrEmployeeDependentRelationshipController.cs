using Microsoft.AspNetCore.Mvc;
using ZR.Model.HR.Hrinsurancetype.Dto;
using ZR.Model.HR.Hrinsurancetype;
using ZR.Service.HR.Hrinsurancetype.IHrinsurancetypeService;
using MiniExcelLibs;

//创建时间：2025-09-02
namespace ZR.Admin.WebApi.Controllers.HR.Hrinsurancetype
{
    /// <summary>
    /// employeeependentrelationship
    /// </summary>
    [Route("hrinsurancetype/hr")]
    public class HrEmployeeDependentRelationshipController : BaseController
    {
        /// <summary>
        /// employeeependentrelationship接口
        /// </summary>
        private readonly IHrEmployeeDependentRelationshipService _HrEmployeeDependentRelationshipService;

        public HrEmployeeDependentRelationshipController(IHrEmployeeDependentRelationshipService HrEmployeeDependentRelationshipService)
        {
            _HrEmployeeDependentRelationshipService = HrEmployeeDependentRelationshipService;
        }

        /// <summary>
        /// 查询employeeependentrelationship列表
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "hremployeedependentrelationship:list")]
        public IActionResult QueryHrEmployeeDependentRelationship([FromQuery] HrEmployeeDependentRelationshipQueryDto parm)
        {
            var response = _HrEmployeeDependentRelationshipService.GetList(parm);
            return SUCCESS(response);
        }


        /// <summary>
        /// 查询employeeependentrelationship详情
        /// </summary>
        /// <param name="Dependentid"></param>
        /// <returns></returns>
        [HttpGet("{Dependentid}")]
        [ActionPermissionFilter(Permission = "hremployeedependentrelationship:query")]
        public IActionResult GetHrEmployeeDependentRelationship(long Dependentid)
        {
            var response = _HrEmployeeDependentRelationshipService.GetInfo(Dependentid);
            
            var info = response.Adapt<HrEmployeeDependentRelationshipDto>();
            return SUCCESS(info);
        }

        /// <summary>
        /// 添加employeeependentrelationship
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ActionPermissionFilter(Permission = "hremployeedependentrelationship:add")]
        [Log(Title = "employeeependentrelationship", BusinessType = BusinessType.INSERT)]
        public IActionResult AddHrEmployeeDependentRelationship([FromBody] HrEmployeeDependentRelationshipDto parm)
        {
            var modal = parm.Adapt<HrEmployeeDependentRelationship>().ToCreate(HttpContext);

            var response = _HrEmployeeDependentRelationshipService.AddHrEmployeeDependentRelationship(modal);

            return SUCCESS(response);
        }

        /// <summary>
        /// 更新employeeependentrelationship
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [ActionPermissionFilter(Permission = "hremployeedependentrelationship:edit")]
        [Log(Title = "employeeependentrelationship", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdateHrEmployeeDependentRelationship([FromBody] HrEmployeeDependentRelationshipDto parm)
        {
            var modal = parm.Adapt<HrEmployeeDependentRelationship>().ToUpdate(HttpContext);
            var response = _HrEmployeeDependentRelationshipService.UpdateHrEmployeeDependentRelationship(modal);

            return ToResponse(response);
        }

        /// <summary>
        /// 删除employeeependentrelationship
        /// </summary>
        /// <returns></returns>
        [HttpPost("delete/{ids}")]
        [ActionPermissionFilter(Permission = "hremployeedependentrelationship:delete")]
        [Log(Title = "employeeependentrelationship", BusinessType = BusinessType.DELETE)]
        public IActionResult DeleteHrEmployeeDependentRelationship([FromRoute]string ids)
        {
            var idArr = Tools.SplitAndConvert<long>(ids);

            return ToResponse(_HrEmployeeDependentRelationshipService.Delete(idArr, "删除employeeependentrelationship"));
        }

        /// <summary>
        /// 导出employeeependentrelationship
        /// </summary>
        /// <returns></returns>
        [Log(Title = "employeeependentrelationship", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        [HttpGet("export")]
        [ActionPermissionFilter(Permission = "hremployeedependentrelationship:export")]
        public IActionResult Export([FromQuery] HrEmployeeDependentRelationshipQueryDto parm)
        {
            var list = _HrEmployeeDependentRelationshipService.ExportList(parm).Result;
            if (list == null || list.Count <= 0)
            {
                return ToResponse(ResultCode.FAIL, "没有要导出的数据");
            }
            var result = ExportExcelMini(list, "employeeependentrelationship", "employeeependentrelationship");
            return ExportExcel(result.Item2, result.Item1);
        }

        /// <summary>
        /// 清空employeeependentrelationship
        /// </summary>
        /// <returns></returns>
        [Log(Title = "employeeependentrelationship", BusinessType = BusinessType.CLEAN)]
        [ActionPermissionFilter(Permission = "hremployeedependentrelationship:delete")]
        [HttpPost("clean")]
        public IActionResult Clear()
        {
            if (!HttpContextExtension.IsAdmin(HttpContext))
            {
                return ToResponse(ResultCode.FAIL, "操作失败");
            }
            return SUCCESS(_HrEmployeeDependentRelationshipService.TruncateHrEmployeeDependentRelationship());
        }

        /// <summary>
        /// 导入
        /// </summary>
        /// <param name="formFile"></param>
        /// <returns></returns>
        [HttpPost("importData")]
        [Log(Title = "employeeependentrelationship导入", BusinessType = BusinessType.IMPORT, IsSaveRequestData = false)]
        [ActionPermissionFilter(Permission = "hremployeedependentrelationship:import")]
        public IActionResult ImportData([FromForm(Name = "file")] IFormFile formFile)
        {
            List<HrEmployeeDependentRelationshipDto> list = new();
            using (var stream = formFile.OpenReadStream())
            {
                list = stream.Query<HrEmployeeDependentRelationshipDto>(startCell: "A1").ToList();
            }

            return SUCCESS(_HrEmployeeDependentRelationshipService.ImportHrEmployeeDependentRelationship(list.Adapt<List<HrEmployeeDependentRelationship>>()));
        }

        /// <summary>
        /// employeeependentrelationship导入模板下载
        /// </summary>
        /// <returns></returns>
        [HttpGet("importTemplate")]
        [Log(Title = "employeeependentrelationship模板", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        [AllowAnonymous]
        public IActionResult ImportTemplateExcel()
        {
            var result = DownloadImportTemplate(new List<HrEmployeeDependentRelationshipDto>() { }, "HrEmployeeDependentRelationship");
            return ExportExcel(result.Item2, result.Item1);
        }

    }
}
