using Microsoft.AspNetCore.Mvc;
using ZR.Model.Business.Dto;
using ZR.Model.Business;
using ZR.Service.Business.IBusinessService;
using MiniExcelLibs;

//创建时间：2025-08-26
namespace ZR.Admin.WebApi.Controllers.Business
{
    /// <summary>
    /// Organizations Information Table
    /// </summary>
    [Route("business/Organizationtype")]
    public class OrganizationtypeController : BaseController
    {
        /// <summary>
        /// Organizations Information Table接口
        /// </summary>
        private readonly IOrganizationtypeService _OrganizationtypeService;

        public OrganizationtypeController(IOrganizationtypeService OrganizationtypeService)
        {
            _OrganizationtypeService = OrganizationtypeService;
        }

        /// <summary>
        /// 查询Organizations Information Table列表
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "organizationtype:list")]
        public IActionResult QueryOrganizationtype([FromQuery] OrganizationtypeQueryDto parm)
        {
            var response = _OrganizationtypeService.GetList(parm);
            return SUCCESS(response);
        }


        /// <summary>
        /// 查询Organizations Information Table详情
        /// </summary>
        /// <param name="Organizationtypeid"></param>
        /// <returns></returns>
        [HttpGet("{Organizationtypeid}")]
        [ActionPermissionFilter(Permission = "organizationtype:query")]
        public IActionResult GetOrganizationtype(long Organizationtypeid)
        {
            var response = _OrganizationtypeService.GetInfo(Organizationtypeid);
            
            var info = response.Adapt<OrganizationtypeDto>();
            return SUCCESS(info);
        }

        /// <summary>
        /// 添加Organizations Information Table
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ActionPermissionFilter(Permission = "organizationtype:add")]
        [Log(Title = "Organizations Information Table", BusinessType = BusinessType.INSERT)]
        public IActionResult AddOrganizationtype([FromBody] OrganizationtypeDto parm)
        {
            var modal = parm.Adapt<Organizationtype>().ToCreate(HttpContext);

            var response = _OrganizationtypeService.AddOrganizationtype(modal);

            return SUCCESS(response);
        }

        /// <summary>
        /// 更新Organizations Information Table
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [ActionPermissionFilter(Permission = "organizationtype:edit")]
        [Log(Title = "Organizations Information Table", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdateOrganizationtype([FromBody] OrganizationtypeDto parm)
        {
            var modal = parm.Adapt<Organizationtype>().ToUpdate(HttpContext);
            var response = _OrganizationtypeService.UpdateOrganizationtype(modal);

            return ToResponse(response);
        }

        /// <summary>
        /// 删除Organizations Information Table
        /// </summary>
        /// <returns></returns>
        [HttpPost("delete/{ids}")]
        [ActionPermissionFilter(Permission = "organizationtype:delete")]
        [Log(Title = "Organizations Information Table", BusinessType = BusinessType.DELETE)]
        public IActionResult DeleteOrganizationtype([FromRoute]string ids)
        {
            var idArr = Tools.SplitAndConvert<long>(ids);

            return ToResponse(_OrganizationtypeService.Delete(idArr, "删除Organizations Information Table"));
        }

        /// <summary>
        /// 导出Organizations Information Table
        /// </summary>
        /// <returns></returns>
        [Log(Title = "Organizations Information Table", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        [HttpGet("export")]
        [ActionPermissionFilter(Permission = "organizationtype:export")]
        public IActionResult Export([FromQuery] OrganizationtypeQueryDto parm)
        {
            var list = _OrganizationtypeService.ExportList(parm).Result;
            if (list == null || list.Count <= 0)
            {
                return ToResponse(ResultCode.FAIL, "没有要导出的数据");
            }
            var result = ExportExcelMini(list, "Organizations Information Table", "Organizations Information Table");
            return ExportExcel(result.Item2, result.Item1);
        }

        /// <summary>
        /// 清空Organizations Information Table
        /// </summary>
        /// <returns></returns>
        [Log(Title = "Organizations Information Table", BusinessType = BusinessType.CLEAN)]
        [ActionPermissionFilter(Permission = "organizationtype:delete")]
        [HttpPost("clean")]
        public IActionResult Clear()
        {
            if (!HttpContextExtension.IsAdmin(HttpContext))
            {
                return ToResponse(ResultCode.FAIL, "操作失败");
            }
            return SUCCESS(_OrganizationtypeService.TruncateOrganizationtype());
        }

        /// <summary>
        /// 导入
        /// </summary>
        /// <param name="formFile"></param>
        /// <returns></returns>
        [HttpPost("importData")]
        [Log(Title = "Organizations Information Table导入", BusinessType = BusinessType.IMPORT, IsSaveRequestData = false)]
        [ActionPermissionFilter(Permission = "organizationtype:import")]
        public IActionResult ImportData([FromForm(Name = "file")] IFormFile formFile)
        {
            List<OrganizationtypeDto> list = new();
            using (var stream = formFile.OpenReadStream())
            {
                list = stream.Query<OrganizationtypeDto>(startCell: "A1").ToList();
            }

            return SUCCESS(_OrganizationtypeService.ImportOrganizationtype(list.Adapt<List<Organizationtype>>()));
        }

        /// <summary>
        /// Organizations Information Table导入模板下载
        /// </summary>
        /// <returns></returns>
        [HttpGet("importTemplate")]
        [Log(Title = "Organizations Information Table模板", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        [AllowAnonymous]
        public IActionResult ImportTemplateExcel()
        {
            var result = DownloadImportTemplate(new List<OrganizationtypeDto>() { }, "Organizationtype");
            return ExportExcel(result.Item2, result.Item1);
        }

    }
}
