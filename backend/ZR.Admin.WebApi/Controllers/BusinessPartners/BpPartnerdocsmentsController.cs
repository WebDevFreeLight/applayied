using Microsoft.AspNetCore.Mvc;
using ZR.Model.BusinessPartners.Dto;
using ZR.Model.BusinessPartners;
using ZR.Service.BusinessPartners.IService;
using MiniExcelLibs;

//创建时间：2025-10-05
namespace ZR.Admin.WebApi.Controllers.BusinessPartners
{
    /// <summary>
    /// Bp_PartnerDocsments
    /// </summary>
    [Route("partnertype/BpPartnerdocsments")]
    public class BpPartnerdocsmentsController : BaseController
    {
        /// <summary>
        /// Bp_PartnerDocsments接口
        /// </summary>
        private readonly IBpPartnerdocsmentsService _BpPartnerdocsmentsService;

        public BpPartnerdocsmentsController(IBpPartnerdocsmentsService BpPartnerdocsmentsService)
        {
            _BpPartnerdocsmentsService = BpPartnerdocsmentsService;
        }

        /// <summary>
        /// 查询Bp_PartnerDocsments列表
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "Partnertype:bppartnerdocsments:list")]
        public IActionResult QueryBpPartnerdocsments([FromQuery] BpPartnerdocsmentsQueryDto parm)
        {
            var response = _BpPartnerdocsmentsService.GetList(parm);
            return SUCCESS(response);
        }


        /// <summary>
        /// 查询Bp_PartnerDocsments详情
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpGet("{Id}")]
        [ActionPermissionFilter(Permission = "Partnertype:bppartnerdocsments:query")]
        public IActionResult GetBpPartnerdocsments(long Id)
        {
            var response = _BpPartnerdocsmentsService.GetInfo(Id);
            
            var info = response.Adapt<BpPartnerdocsmentsDto>();
            return SUCCESS(info);
        }

        /// <summary>
        /// 添加Bp_PartnerDocsments
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ActionPermissionFilter(Permission = "Partnertype:bppartnerdocsments:add")]
        [Log(Title = "Bp_PartnerDocsments", BusinessType = BusinessType.INSERT)]
        public IActionResult AddBpPartnerdocsments([FromBody] BpPartnerdocsmentsDto parm)
        {
            var modal = parm.Adapt<BpPartnerdocsments>().ToCreate(HttpContext);

            var response = _BpPartnerdocsmentsService.AddBpPartnerdocsments(modal);

            return SUCCESS(response);
        }

        /// <summary>
        /// 更新Bp_PartnerDocsments
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [ActionPermissionFilter(Permission = "Partnertype:bppartnerdocsments:edit")]
        [Log(Title = "Bp_PartnerDocsments", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdateBpPartnerdocsments([FromBody] BpPartnerdocsmentsDto parm)
        {
            var modal = parm.Adapt<BpPartnerdocsments>().ToUpdate(HttpContext);
            var response = _BpPartnerdocsmentsService.UpdateBpPartnerdocsments(modal);

            return ToResponse(response);
        }

        /// <summary>
        /// 删除Bp_PartnerDocsments
        /// </summary>
        /// <returns></returns>
        [HttpPost("delete/{ids}")]
        [ActionPermissionFilter(Permission = "Partnertype:bppartnerdocsments:delete")]
        [Log(Title = "Bp_PartnerDocsments", BusinessType = BusinessType.DELETE)]
        public IActionResult DeleteBpPartnerdocsments([FromRoute]string ids)
        {
            var idArr = Tools.SplitAndConvert<long>(ids);

            return ToResponse(_BpPartnerdocsmentsService.Delete(idArr, "删除Bp_PartnerDocsments"));
        }

        /// <summary>
        /// 导出Bp_PartnerDocsments
        /// </summary>
        /// <returns></returns>
        [Log(Title = "Bp_PartnerDocsments", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        [HttpGet("export")]
        [ActionPermissionFilter(Permission = "Partnertype:bppartnerdocsments:export")]
        public IActionResult Export([FromQuery] BpPartnerdocsmentsQueryDto parm)
        {
            var list = _BpPartnerdocsmentsService.ExportList(parm).Result;
            if (list == null || list.Count <= 0)
            {
                return ToResponse(ResultCode.FAIL, "没有要导出的数据");
            }
            var result = ExportExcelMini(list, "Bp_PartnerDocsments", "Bp_PartnerDocsments");
            return ExportExcel(result.Item2, result.Item1);
        }

        /// <summary>
        /// 清空Bp_PartnerDocsments
        /// </summary>
        /// <returns></returns>
        [Log(Title = "Bp_PartnerDocsments", BusinessType = BusinessType.CLEAN)]
        [ActionPermissionFilter(Permission = "Partnertype:bppartnerdocsments:delete")]
        [HttpPost("clean")]
        public IActionResult Clear()
        {
            if (!HttpContextExtension.IsAdmin(HttpContext))
            {
                return ToResponse(ResultCode.FAIL, "操作失败");
            }
            return SUCCESS(_BpPartnerdocsmentsService.TruncateBpPartnerdocsments());
        }

        /// <summary>
        /// 导入
        /// </summary>
        /// <param name="formFile"></param>
        /// <returns></returns>
        [HttpPost("importData")]
        [Log(Title = "Bp_PartnerDocsments导入", BusinessType = BusinessType.IMPORT, IsSaveRequestData = false)]
        [ActionPermissionFilter(Permission = "Partnertype:bppartnerdocsments:import")]
        public IActionResult ImportData([FromForm(Name = "file")] IFormFile formFile)
        {
            List<BpPartnerdocsmentsDto> list = new();
            using (var stream = formFile.OpenReadStream())
            {
                list = stream.Query<BpPartnerdocsmentsDto>(startCell: "A1").ToList();
            }

            return SUCCESS(_BpPartnerdocsmentsService.ImportBpPartnerdocsments(list.Adapt<List<BpPartnerdocsments>>()));
        }

        /// <summary>
        /// Bp_PartnerDocsments导入模板下载
        /// </summary>
        /// <returns></returns>
        [HttpGet("importTemplate")]
        [Log(Title = "Bp_PartnerDocsments模板", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        [AllowAnonymous]
        public IActionResult ImportTemplateExcel()
        {
            var result = DownloadImportTemplate(new List<BpPartnerdocsmentsDto>() { }, "BpPartnerdocsments");
            return ExportExcel(result.Item2, result.Item1);
        }

    }
}
