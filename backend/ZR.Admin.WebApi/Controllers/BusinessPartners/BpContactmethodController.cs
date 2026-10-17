using Microsoft.AspNetCore.Mvc;
using ZR.Model.BusinessPartners.Dto;
using ZR.Model.BusinessPartners;
using ZR.Service.BusinessPartners.IService;
using MiniExcelLibs;

//创建时间：2025-08-31
namespace ZR.Admin.WebApi.Controllers.BusinessPartners
{
    /// <summary>
    /// ContactMethod
    /// </summary>
    [Route("partnertype/BpContactmethod")]
    public class BpContactmethodController : BaseController
    {
        /// <summary>
        /// ContactMethod接口
        /// </summary>
        private readonly IBpContactmethodService _BpContactmethodService;

        public BpContactmethodController(IBpContactmethodService BpContactmethodService)
        {
            _BpContactmethodService = BpContactmethodService;
        }

        /// <summary>
        /// 查询ContactMethod列表
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "bpcontactmethod:list")]
        public IActionResult QueryBpContactmethod([FromQuery] BpContactmethodQueryDto parm)
        {
            var response = _BpContactmethodService.GetList(parm);
            return SUCCESS(response);
        }


        /// <summary>
        /// 查询ContactMethod详情
        /// </summary>
        /// <param name="Contactmethodenumid"></param>
        /// <returns></returns>
        [HttpGet("{Contactmethodenumid}")]
        [ActionPermissionFilter(Permission = "bpcontactmethod:query")]
        public IActionResult GetBpContactmethod(long Contactmethodenumid)
        {
            var response = _BpContactmethodService.GetInfo(Contactmethodenumid);
            
            var info = response.Adapt<BpContactmethodDto>();
            return SUCCESS(info);
        }

        /// <summary>
        /// 添加ContactMethod
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ActionPermissionFilter(Permission = "bpcontactmethod:add")]
        [Log(Title = "ContactMethod", BusinessType = BusinessType.INSERT)]
        public IActionResult AddBpContactmethod([FromBody] BpContactmethodDto parm)
        {
            var modal = parm.Adapt<BpContactmethod>().ToCreate(HttpContext);

            var response = _BpContactmethodService.AddBpContactmethod(modal);

            return SUCCESS(response);
        }

        /// <summary>
        /// 更新ContactMethod
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [ActionPermissionFilter(Permission = "bpcontactmethod:edit")]
        [Log(Title = "ContactMethod", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdateBpContactmethod([FromBody] BpContactmethodDto parm)
        {
            var modal = parm.Adapt<BpContactmethod>().ToUpdate(HttpContext);
            var response = _BpContactmethodService.UpdateBpContactmethod(modal);

            return ToResponse(response);
        }

        /// <summary>
        /// 删除ContactMethod
        /// </summary>
        /// <returns></returns>
        [HttpPost("delete/{ids}")]
        [ActionPermissionFilter(Permission = "bpcontactmethod:delete")]
        [Log(Title = "ContactMethod", BusinessType = BusinessType.DELETE)]
        public IActionResult DeleteBpContactmethod([FromRoute]string ids)
        {
            var idArr = Tools.SplitAndConvert<int>(ids);

            return ToResponse(_BpContactmethodService.Delete(idArr, "删除ContactMethod"));
        }

        /// <summary>
        /// 导出ContactMethod
        /// </summary>
        /// <returns></returns>
        [Log(Title = "ContactMethod", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        [HttpGet("export")]
        [ActionPermissionFilter(Permission = "bpcontactmethod:export")]
        public IActionResult Export([FromQuery] BpContactmethodQueryDto parm)
        {
            var list = _BpContactmethodService.ExportList(parm).Result;
            if (list == null || list.Count <= 0)
            {
                return ToResponse(ResultCode.FAIL, "没有要导出的数据");
            }
            var result = ExportExcelMini(list, "ContactMethod", "ContactMethod");
            return ExportExcel(result.Item2, result.Item1);
        }

        /// <summary>
        /// 清空ContactMethod
        /// </summary>
        /// <returns></returns>
        [Log(Title = "ContactMethod", BusinessType = BusinessType.CLEAN)]
        [ActionPermissionFilter(Permission = "bpcontactmethod:delete")]
        [HttpPost("clean")]
        public IActionResult Clear()
        {
            if (!HttpContextExtension.IsAdmin(HttpContext))
            {
                return ToResponse(ResultCode.FAIL, "操作失败");
            }
            return SUCCESS(_BpContactmethodService.TruncateBpContactmethod());
        }

        /// <summary>
        /// 导入
        /// </summary>
        /// <param name="formFile"></param>
        /// <returns></returns>
        [HttpPost("importData")]
        [Log(Title = "ContactMethod导入", BusinessType = BusinessType.IMPORT, IsSaveRequestData = false)]
        [ActionPermissionFilter(Permission = "bpcontactmethod:import")]
        public IActionResult ImportData([FromForm(Name = "file")] IFormFile formFile)
        {
            List<BpContactmethodDto> list = new();
            using (var stream = formFile.OpenReadStream())
            {
                list = stream.Query<BpContactmethodDto>(startCell: "A1").ToList();
            }

            return SUCCESS(_BpContactmethodService.ImportBpContactmethod(list.Adapt<List<BpContactmethod>>()));
        }

        /// <summary>
        /// ContactMethod导入模板下载
        /// </summary>
        /// <returns></returns>
        [HttpGet("importTemplate")]
        [Log(Title = "ContactMethod模板", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        [AllowAnonymous]
        public IActionResult ImportTemplateExcel()
        {
            var result = DownloadImportTemplate(new List<BpContactmethodDto>() { }, "BpContactmethod");
            return ExportExcel(result.Item2, result.Item1);
        }

    }
}
