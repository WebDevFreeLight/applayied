using Microsoft.AspNetCore.Mvc;
using ZR.Model.BusinessPartners.Dto;
using ZR.Model.BusinessPartners;
using ZR.Service.BusinessPartners.IService;
using MiniExcelLibs;

//创建时间：2025-08-31
namespace ZR.Admin.WebApi.Controllers.BusinessPartners
{
    /// <summary>
    /// BusinessPartnerType
    /// </summary>
    [Route("partnertype/BpBusinesspartnertype")]
    public class BpBusinesspartnertypeController : BaseController
    {
        /// <summary>
        /// BusinessPartnerType接口
        /// </summary>
        private readonly IBpBusinesspartnertypeService _BpBusinesspartnertypeService;

        public BpBusinesspartnertypeController(IBpBusinesspartnertypeService BpBusinesspartnertypeService)
        {
            _BpBusinesspartnertypeService = BpBusinesspartnertypeService;
        }

        /// <summary>
        /// 查询BusinessPartnerType列表
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "bpbusinesspartnertype:list")]
        public IActionResult QueryBpBusinesspartnertype([FromQuery] BpBusinesspartnertypeQueryDto parm)
        {
            var response = _BpBusinesspartnertypeService.GetList(parm);
            return SUCCESS(response);
        }


        /// <summary>
        /// 查询BusinessPartnerType详情
        /// </summary>
        /// <param name="Typeid"></param>
        /// <returns></returns>
        [HttpGet("{Typeid}")]
        [ActionPermissionFilter(Permission = "bpbusinesspartnertype:query")]
        public IActionResult GetBpBusinesspartnertype(long Typeid)
        {
            var response = _BpBusinesspartnertypeService.GetInfo(Typeid);
            
            var info = response.Adapt<BpBusinesspartnertypeDto>();
            return SUCCESS(info);
        }

        /// <summary>
        /// 添加BusinessPartnerType
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ActionPermissionFilter(Permission = "bpbusinesspartnertype:add")]
        [Log(Title = "BusinessPartnerType", BusinessType = BusinessType.INSERT)]
        public IActionResult AddBpBusinesspartnertype([FromBody] BpBusinesspartnertypeDto parm)
        {
            var modal = parm.Adapt<BpBusinesspartnertype>().ToCreate(HttpContext);

            var response = _BpBusinesspartnertypeService.AddBpBusinesspartnertype(modal);

            return SUCCESS(response);
        }

        /// <summary>
        /// 更新BusinessPartnerType
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [ActionPermissionFilter(Permission = "bpbusinesspartnertype:edit")]
        [Log(Title = "BusinessPartnerType", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdateBpBusinesspartnertype([FromBody] BpBusinesspartnertypeDto parm)
        {
            var modal = parm.Adapt<BpBusinesspartnertype>().ToUpdate(HttpContext);
            var response = _BpBusinesspartnertypeService.UpdateBpBusinesspartnertype(modal);

            return ToResponse(response);
        }

        /// <summary>
        /// 删除BusinessPartnerType
        /// </summary>
        /// <returns></returns>
        [HttpPost("delete/{ids}")]
        [ActionPermissionFilter(Permission = "bpbusinesspartnertype:delete")]
        [Log(Title = "BusinessPartnerType", BusinessType = BusinessType.DELETE)]
        public IActionResult DeleteBpBusinesspartnertype([FromRoute]string ids)
        {
            var idArr = Tools.SplitAndConvert<long>(ids);

            return ToResponse(_BpBusinesspartnertypeService.Delete(idArr, "删除BusinessPartnerType"));
        }

        /// <summary>
        /// 导出BusinessPartnerType
        /// </summary>
        /// <returns></returns>
        [Log(Title = "BusinessPartnerType", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        [HttpGet("export")]
        [ActionPermissionFilter(Permission = "bpbusinesspartnertype:export")]
        public IActionResult Export([FromQuery] BpBusinesspartnertypeQueryDto parm)
        {
            var list = _BpBusinesspartnertypeService.ExportList(parm).Result;
            if (list == null || list.Count <= 0)
            {
                return ToResponse(ResultCode.FAIL, "没有要导出的数据");
            }
            var result = ExportExcelMini(list, "BusinessPartnerType", "BusinessPartnerType");
            return ExportExcel(result.Item2, result.Item1);
        }

        /// <summary>
        /// 清空BusinessPartnerType
        /// </summary>
        /// <returns></returns>
        [Log(Title = "BusinessPartnerType", BusinessType = BusinessType.CLEAN)]
        [ActionPermissionFilter(Permission = "bpbusinesspartnertype:delete")]
        [HttpPost("clean")]
        public IActionResult Clear()
        {
            if (!HttpContextExtension.IsAdmin(HttpContext))
            {
                return ToResponse(ResultCode.FAIL, "操作失败");
            }
            return SUCCESS(_BpBusinesspartnertypeService.TruncateBpBusinesspartnertype());
        }

        /// <summary>
        /// 导入
        /// </summary>
        /// <param name="formFile"></param>
        /// <returns></returns>
        [HttpPost("importData")]
        [Log(Title = "BusinessPartnerType导入", BusinessType = BusinessType.IMPORT, IsSaveRequestData = false)]
        [ActionPermissionFilter(Permission = "bpbusinesspartnertype:import")]
        public IActionResult ImportData([FromForm(Name = "file")] IFormFile formFile)
        {
            List<BpBusinesspartnertypeDto> list = new();
            using (var stream = formFile.OpenReadStream())
            {
                list = stream.Query<BpBusinesspartnertypeDto>(startCell: "A1").ToList();
            }

            return SUCCESS(_BpBusinesspartnertypeService.ImportBpBusinesspartnertype(list.Adapt<List<BpBusinesspartnertype>>()));
        }

        /// <summary>
        /// BusinessPartnerType导入模板下载
        /// </summary>
        /// <returns></returns>
        [HttpGet("importTemplate")]
        [Log(Title = "BusinessPartnerType模板", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        [AllowAnonymous]
        public IActionResult ImportTemplateExcel()
        {
            var result = DownloadImportTemplate(new List<BpBusinesspartnertypeDto>() { }, "BpBusinesspartnertype");
            return ExportExcel(result.Item2, result.Item1);
        }

    }
}
