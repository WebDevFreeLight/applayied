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
    [Route("business/Organizations")]
    public class OrganizationsController : BaseController
    {
        /// <summary>
        /// Organizations Information Table接口
        /// </summary>
        private readonly IOrganizationsService _OrganizationsService;

        public OrganizationsController(IOrganizationsService OrganizationsService)
        {
            _OrganizationsService = OrganizationsService;
        }

        /// <summary>
        /// 查询Organizations Information Table列表
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "organizations:list")]
        public IActionResult QueryOrganizations([FromQuery] OrganizationsQueryDto parm)
        {
            var response = _OrganizationsService.GetList(parm);
            return SUCCESS(response);
        }

        /// <summary>
        /// 查询Organizations Information Table列表树
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        [HttpGet("treeList")]
        [ActionPermissionFilter(Permission = "organizations:list")]
        public IActionResult QueryTreeOrganizations([FromQuery] OrganizationsQueryDto parm)
        {
            var response = _OrganizationsService.GetTreeList(parm);
            return SUCCESS(response);
        }

        /// <summary>
        /// 查询Organizations Information Table详情
        /// </summary>
        /// <param name="Organizationsid"></param>
        /// <returns></returns>
        [HttpGet("{Organizationsid}")]
        [ActionPermissionFilter(Permission = "organizations:query")]
        public IActionResult GetOrganizations(long Organizationsid)
        {
            var response = _OrganizationsService.GetInfo(Organizationsid);
            
            var info = response.Adapt<OrganizationsDto>();
            return SUCCESS(info);
        }

        /// <summary>
        /// 添加Organizations Information Table
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ActionPermissionFilter(Permission = "organizations:add")]
        [Log(Title = "Organizations Information Table", BusinessType = BusinessType.INSERT)]
        public IActionResult AddOrganizations([FromBody] OrganizationsDto parm)
        {
            var modal = parm.Adapt<Organizations>().ToCreate(HttpContext);

            var response = _OrganizationsService.AddOrganizations(modal);

            return SUCCESS(response);
        }

        /// <summary>
        /// 更新Organizations Information Table
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [ActionPermissionFilter(Permission = "organizations:edit")]
        [Log(Title = "Organizations Information Table", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdateOrganizations([FromBody] OrganizationsDto parm)
        {
            var modal = parm.Adapt<Organizations>().ToUpdate(HttpContext);
            var response = _OrganizationsService.UpdateOrganizations(modal);

            return ToResponse(response);
        }

        /// <summary>
        /// 删除Organizations Information Table
        /// </summary>
        /// <returns></returns>
        [HttpPost("delete/{ids}")]
        [ActionPermissionFilter(Permission = "organizations:delete")]
        [Log(Title = "Organizations Information Table", BusinessType = BusinessType.DELETE)]
        public IActionResult DeleteOrganizations([FromRoute]string ids)
        {
            var idArr = Tools.SplitAndConvert<long>(ids);

            return ToResponse(_OrganizationsService.Delete(idArr, "删除Organizations Information Table"));
        }

        /// <summary>
        /// 导出Organizations Information Table
        /// </summary>
        /// <returns></returns>
        [Log(Title = "Organizations Information Table", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        [HttpGet("export")]
        [ActionPermissionFilter(Permission = "organizations:export")]
        public IActionResult Export([FromQuery] OrganizationsQueryDto parm)
        {
            var list = _OrganizationsService.ExportList(parm).Result;
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
        [ActionPermissionFilter(Permission = "organizations:delete")]
        [HttpPost("clean")]
        public IActionResult Clear()
        {
            if (!HttpContextExtension.IsAdmin(HttpContext))
            {
                return ToResponse(ResultCode.FAIL, "操作失败");
            }
            return SUCCESS(_OrganizationsService.TruncateOrganizations());
        }

        /// <summary>
        /// 导入
        /// </summary>
        /// <param name="formFile"></param>
        /// <returns></returns>
        [HttpPost("importData")]
        [Log(Title = "Organizations Information Table导入", BusinessType = BusinessType.IMPORT, IsSaveRequestData = false)]
        [ActionPermissionFilter(Permission = "organizations:import")]
        public IActionResult ImportData([FromForm(Name = "file")] IFormFile formFile)
        {
            List<OrganizationsDto> list = new();
            using (var stream = formFile.OpenReadStream())
            {
                list = stream.Query<OrganizationsDto>(startCell: "A1").ToList();
            }

            return SUCCESS(_OrganizationsService.ImportOrganizations(list.Adapt<List<Organizations>>()));
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
            var result = DownloadImportTemplate(new List<OrganizationsDto>() { }, "Organizations");
            return ExportExcel(result.Item2, result.Item1);
        }

    }
}
