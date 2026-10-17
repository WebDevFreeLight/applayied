using Microsoft.AspNetCore.Mvc;
using ZR.Model.BusinessPartners.Dto;
using ZR.Model.BusinessPartners;
using ZR.Service.BusinessPartners.IService;
using MiniExcelLibs;

//创建时间：2025-10-05
namespace ZR.Admin.WebApi.Controllers.BusinessPartners
{
    /// <summary>
    /// BusinessPartnerDocType
    /// </summary>
    [Route("Partnertype/BpBusinesspartnerdoctype")]
    public class BpBusinesspartnerdoctypeController : BaseController
    {
        /// <summary>
        /// BusinessPartnerDocType接口
        /// </summary>
        private readonly IBpBusinesspartnerdoctypeService _BpBusinesspartnerdoctypeService;

        public BpBusinesspartnerdoctypeController(IBpBusinesspartnerdoctypeService BpBusinesspartnerdoctypeService)
        {
            _BpBusinesspartnerdoctypeService = BpBusinesspartnerdoctypeService;
        }

        /// <summary>
        /// 查询BusinessPartnerDocType列表
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "Partnertype:bpbusinesspartnerdoctype:list")]
        public IActionResult QueryBpBusinesspartnerdoctype([FromQuery] BpBusinesspartnerdoctypeQueryDto parm)
        {
            var response = _BpBusinesspartnerdoctypeService.GetList(parm);
            return SUCCESS(response);
        }


        /// <summary>
        /// 查询BusinessPartnerDocType详情
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpGet("{Id}")]
        [ActionPermissionFilter(Permission = "Partnertype:bpbusinesspartnerdoctype:query")]
        public IActionResult GetBpBusinesspartnerdoctype(long Id)
        {
            var response = _BpBusinesspartnerdoctypeService.GetInfo(Id);
            
            var info = response.Adapt<BpBusinesspartnerdoctypeDto>();
            return SUCCESS(info);
        }

        /// <summary>
        /// 添加BusinessPartnerDocType
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ActionPermissionFilter(Permission = "Partnertype:bpbusinesspartnerdoctype:add")]
        [Log(Title = "BusinessPartnerDocType", BusinessType = BusinessType.INSERT)]
        public IActionResult AddBpBusinesspartnerdoctype([FromBody] BpBusinesspartnerdoctypeDto parm)
        {
            var modal = parm.Adapt<BpBusinesspartnerdoctype>().ToCreate(HttpContext);

            var response = _BpBusinesspartnerdoctypeService.AddBpBusinesspartnerdoctype(modal);

            return SUCCESS(response);
        }

        /// <summary>
        /// 更新BusinessPartnerDocType
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [ActionPermissionFilter(Permission = "Partnertype:bpbusinesspartnerdoctype:edit")]
        [Log(Title = "BusinessPartnerDocType", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdateBpBusinesspartnerdoctype([FromBody] BpBusinesspartnerdoctypeDto parm)
        {
            var modal = parm.Adapt<BpBusinesspartnerdoctype>().ToUpdate(HttpContext);
            var response = _BpBusinesspartnerdoctypeService.UpdateBpBusinesspartnerdoctype(modal);

            return ToResponse(response);
        }

        /// <summary>
        /// 删除BusinessPartnerDocType
        /// </summary>
        /// <returns></returns>
        [HttpPost("delete/{ids}")]
        [ActionPermissionFilter(Permission = "Partnertype:bpbusinesspartnerdoctype:delete")]
        [Log(Title = "BusinessPartnerDocType", BusinessType = BusinessType.DELETE)]
        public IActionResult DeleteBpBusinesspartnerdoctype([FromRoute]string ids)
        {
            var idArr = Tools.SplitAndConvert<long>(ids);

            return ToResponse(_BpBusinesspartnerdoctypeService.Delete(idArr, "删除BusinessPartnerDocType"));
        }

        /// <summary>
        /// 导出BusinessPartnerDocType
        /// </summary>
        /// <returns></returns>
        [Log(Title = "BusinessPartnerDocType", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        [HttpGet("export")]
        [ActionPermissionFilter(Permission = "Partnertype:bpbusinesspartnerdoctype:export")]
        public IActionResult Export([FromQuery] BpBusinesspartnerdoctypeQueryDto parm)
        {
            var list = _BpBusinesspartnerdoctypeService.ExportList(parm).Result;
            if (list == null || list.Count <= 0)
            {
                return ToResponse(ResultCode.FAIL, "没有要导出的数据");
            }
            var result = ExportExcelMini(list, "BusinessPartnerDocType", "BusinessPartnerDocType");
            return ExportExcel(result.Item2, result.Item1);
        }

        /// <summary>
        /// 清空BusinessPartnerDocType
        /// </summary>
        /// <returns></returns>
        [Log(Title = "BusinessPartnerDocType", BusinessType = BusinessType.CLEAN)]
        [ActionPermissionFilter(Permission = "Partnertype:bpbusinesspartnerdoctype:delete")]
        [HttpPost("clean")]
        public IActionResult Clear()
        {
            if (!HttpContextExtension.IsAdmin(HttpContext))
            {
                return ToResponse(ResultCode.FAIL, "操作失败");
            }
            return SUCCESS(_BpBusinesspartnerdoctypeService.TruncateBpBusinesspartnerdoctype());
        }

        /// <summary>
        /// 导入
        /// </summary>
        /// <param name="formFile"></param>
        /// <returns></returns>
        [HttpPost("importData")]
        [Log(Title = "BusinessPartnerDocType导入", BusinessType = BusinessType.IMPORT, IsSaveRequestData = false)]
        [ActionPermissionFilter(Permission = "Partnertype:bpbusinesspartnerdoctype:import")]
        public IActionResult ImportData([FromForm(Name = "file")] IFormFile formFile)
        {
            List<BpBusinesspartnerdoctypeDto> list = new();
            using (var stream = formFile.OpenReadStream())
            {
                list = stream.Query<BpBusinesspartnerdoctypeDto>(startCell: "A1").ToList();
            }

            return SUCCESS(_BpBusinesspartnerdoctypeService.ImportBpBusinesspartnerdoctype(list.Adapt<List<BpBusinesspartnerdoctype>>()));
        }

        /// <summary>
        /// BusinessPartnerDocType导入模板下载
        /// </summary>
        /// <returns></returns>
        [HttpGet("importTemplate")]
        [Log(Title = "BusinessPartnerDocType模板", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        [AllowAnonymous]
        public IActionResult ImportTemplateExcel()
        {
            var result = DownloadImportTemplate(new List<BpBusinesspartnerdoctypeDto>() { }, "BpBusinesspartnerdoctype");
            return ExportExcel(result.Item2, result.Item1);
        }

    }
}
