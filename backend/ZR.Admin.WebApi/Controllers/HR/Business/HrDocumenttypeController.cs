using Microsoft.AspNetCore.Mvc;
using ZR.Model.HR.Business.Dto;
using ZR.Model.HR.Business;
using ZR.Service.HR.Business.IBusinessService;
using MiniExcelLibs;

//创建时间：2025-08-29
namespace ZR.Admin.WebApi.Controllers.HR.Business
{
    /// <summary>
    /// DocumentType
    /// </summary>
    [Route("business/HrDocumenttype")]
    public class HrDocumenttypeController : BaseController
    {
        /// <summary>
        /// DocumentType接口
        /// </summary>
        private readonly IHrDocumenttypeService _HrDocumenttypeService;

        public HrDocumenttypeController(IHrDocumenttypeService HrDocumenttypeService)
        {
            _HrDocumenttypeService = HrDocumenttypeService;
        }

        /// <summary>
        /// 查询DocumentType列表
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "hrdocumenttype:list")]
        public IActionResult QueryHrDocumenttype([FromQuery] HrDocumenttypeQueryDto parm)
        {
            var response = _HrDocumenttypeService.GetList(parm);
            return SUCCESS(response);
        }


        /// <summary>
        /// 查询DocumentType详情
        /// </summary>
        /// <param name="Documenttypeid"></param>
        /// <returns></returns>
        [HttpGet("{Documenttypeid}")]
        [ActionPermissionFilter(Permission = "hrdocumenttype:query")]
        public IActionResult GetHrDocumenttype(long Documenttypeid)
        {
            var response = _HrDocumenttypeService.GetInfo(Documenttypeid);
            
            var info = response.Adapt<HrDocumenttypeDto>();
            return SUCCESS(info);
        }

        /// <summary>
        /// 添加DocumentType
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ActionPermissionFilter(Permission = "hrdocumenttype:add")]
        [Log(Title = "DocumentType", BusinessType = BusinessType.INSERT)]
        public IActionResult AddHrDocumenttype([FromBody] HrDocumenttypeDto parm)
        {
            var modal = parm.Adapt<HrDocumenttype>().ToCreate(HttpContext);

            var response = _HrDocumenttypeService.AddHrDocumenttype(modal);

            return SUCCESS(response);
        }

        /// <summary>
        /// 更新DocumentType
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [ActionPermissionFilter(Permission = "hrdocumenttype:edit")]
        [Log(Title = "DocumentType", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdateHrDocumenttype([FromBody] HrDocumenttypeDto parm)
        {
            var modal = parm.Adapt<HrDocumenttype>().ToUpdate(HttpContext);
            var response = _HrDocumenttypeService.UpdateHrDocumenttype(modal);

            return ToResponse(response);
        }

        /// <summary>
        /// 删除DocumentType
        /// </summary>
        /// <returns></returns>
        [HttpPost("delete/{ids}")]
        [ActionPermissionFilter(Permission = "hrdocumenttype:delete")]
        [Log(Title = "DocumentType", BusinessType = BusinessType.DELETE)]
        public IActionResult DeleteHrDocumenttype([FromRoute]string ids)
        {
            var idArr = Tools.SplitAndConvert<long>(ids);

            return ToResponse(_HrDocumenttypeService.Delete(idArr, "删除DocumentType"));
        }

        /// <summary>
        /// 导出DocumentType
        /// </summary>
        /// <returns></returns>
        [Log(Title = "DocumentType", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        [HttpGet("export")]
        [ActionPermissionFilter(Permission = "hrdocumenttype:export")]
        public IActionResult Export([FromQuery] HrDocumenttypeQueryDto parm)
        {
            var list = _HrDocumenttypeService.ExportList(parm).Result;
            if (list == null || list.Count <= 0)
            {
                return ToResponse(ResultCode.FAIL, "没有要导出的数据");
            }
            var result = ExportExcelMini(list, "DocumentType", "DocumentType");
            return ExportExcel(result.Item2, result.Item1);
        }

        /// <summary>
        /// 清空DocumentType
        /// </summary>
        /// <returns></returns>
        [Log(Title = "DocumentType", BusinessType = BusinessType.CLEAN)]
        [ActionPermissionFilter(Permission = "hrdocumenttype:delete")]
        [HttpPost("clean")]
        public IActionResult Clear()
        {
            if (!HttpContextExtension.IsAdmin(HttpContext))
            {
                return ToResponse(ResultCode.FAIL, "操作失败");
            }
            return SUCCESS(_HrDocumenttypeService.TruncateHrDocumenttype());
        }

        /// <summary>
        /// 导入
        /// </summary>
        /// <param name="formFile"></param>
        /// <returns></returns>
        [HttpPost("importData")]
        [Log(Title = "DocumentType导入", BusinessType = BusinessType.IMPORT, IsSaveRequestData = false)]
        [ActionPermissionFilter(Permission = "hrdocumenttype:import")]
        public IActionResult ImportData([FromForm(Name = "file")] IFormFile formFile)
        {
            List<HrDocumenttypeDto> list = new();
            using (var stream = formFile.OpenReadStream())
            {
                list = stream.Query<HrDocumenttypeDto>(startCell: "A1").ToList();
            }

            return SUCCESS(_HrDocumenttypeService.ImportHrDocumenttype(list.Adapt<List<HrDocumenttype>>()));
        }

        /// <summary>
        /// DocumentType导入模板下载
        /// </summary>
        /// <returns></returns>
        [HttpGet("importTemplate")]
        [Log(Title = "DocumentType模板", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        [AllowAnonymous]
        public IActionResult ImportTemplateExcel()
        {
            var result = DownloadImportTemplate(new List<HrDocumenttypeDto>() { }, "HrDocumenttype");
            return ExportExcel(result.Item2, result.Item1);
        }

    }
}
