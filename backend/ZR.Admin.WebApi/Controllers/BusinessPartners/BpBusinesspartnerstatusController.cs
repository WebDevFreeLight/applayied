using Microsoft.AspNetCore.Mvc;
using ZR.Model.BusinessPartners.Dto;
using ZR.Model.BusinessPartners;
using ZR.Service.BusinessPartners.IService;
using MiniExcelLibs;

//创建时间：2025-08-31
namespace ZR.Admin.WebApi.Controllers.BusinessPartners
{
    /// <summary>
    /// BusinessPartnerStatus
    /// </summary>
    [Route("partnertype/BpBusinesspartnerstatus")]
    public class BpBusinesspartnerstatusController : BaseController
    {
        /// <summary>
        /// BusinessPartnerStatus接口
        /// </summary>
        private readonly IBpBusinesspartnerstatusService _BpBusinesspartnerstatusService;

        public BpBusinesspartnerstatusController(IBpBusinesspartnerstatusService BpBusinesspartnerstatusService)
        {
            _BpBusinesspartnerstatusService = BpBusinesspartnerstatusService;
        }

        /// <summary>
        /// 查询BusinessPartnerStatus列表
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "bpbusinesspartnerstatus:list")]
        public IActionResult QueryBpBusinesspartnerstatus([FromQuery] BpBusinesspartnerstatusQueryDto parm)
        {
            var response = _BpBusinesspartnerstatusService.GetList(parm);
            return SUCCESS(response);
        }


        /// <summary>
        /// 查询BusinessPartnerStatus详情
        /// </summary>
        /// <param name="Statusid"></param>
        /// <returns></returns>
        [HttpGet("{Statusid}")]
        [ActionPermissionFilter(Permission = "bpbusinesspartnerstatus:query")]
        public IActionResult GetBpBusinesspartnerstatus(long Statusid)
        {
            var response = _BpBusinesspartnerstatusService.GetInfo(Statusid);
            
            var info = response.Adapt<BpBusinesspartnerstatusDto>();
            return SUCCESS(info);
        }

        /// <summary>
        /// 添加BusinessPartnerStatus
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ActionPermissionFilter(Permission = "bpbusinesspartnerstatus:add")]
        [Log(Title = "BusinessPartnerStatus", BusinessType = BusinessType.INSERT)]
        public IActionResult AddBpBusinesspartnerstatus([FromBody] BpBusinesspartnerstatusDto parm)
        {
            var modal = parm.Adapt<BpBusinesspartnerstatus>().ToCreate(HttpContext);

            var response = _BpBusinesspartnerstatusService.AddBpBusinesspartnerstatus(modal);

            return SUCCESS(response);
        }

        /// <summary>
        /// 更新BusinessPartnerStatus
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [ActionPermissionFilter(Permission = "bpbusinesspartnerstatus:edit")]
        [Log(Title = "BusinessPartnerStatus", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdateBpBusinesspartnerstatus([FromBody] BpBusinesspartnerstatusDto parm)
        {
            var modal = parm.Adapt<BpBusinesspartnerstatus>().ToUpdate(HttpContext);
            var response = _BpBusinesspartnerstatusService.UpdateBpBusinesspartnerstatus(modal);

            return ToResponse(response);
        }

        /// <summary>
        /// 删除BusinessPartnerStatus
        /// </summary>
        /// <returns></returns>
        [HttpPost("delete/{ids}")]
        [ActionPermissionFilter(Permission = "bpbusinesspartnerstatus:delete")]
        [Log(Title = "BusinessPartnerStatus", BusinessType = BusinessType.DELETE)]
        public IActionResult DeleteBpBusinesspartnerstatus([FromRoute]string ids)
        {
            var idArr = Tools.SplitAndConvert<long>(ids);

            return ToResponse(_BpBusinesspartnerstatusService.Delete(idArr, "删除BusinessPartnerStatus"));
        }

        /// <summary>
        /// 导出BusinessPartnerStatus
        /// </summary>
        /// <returns></returns>
        [Log(Title = "BusinessPartnerStatus", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        [HttpGet("export")]
        [ActionPermissionFilter(Permission = "bpbusinesspartnerstatus:export")]
        public IActionResult Export([FromQuery] BpBusinesspartnerstatusQueryDto parm)
        {
            var list = _BpBusinesspartnerstatusService.ExportList(parm).Result;
            if (list == null || list.Count <= 0)
            {
                return ToResponse(ResultCode.FAIL, "没有要导出的数据");
            }
            var result = ExportExcelMini(list, "BusinessPartnerStatus", "BusinessPartnerStatus");
            return ExportExcel(result.Item2, result.Item1);
        }

        /// <summary>
        /// 清空BusinessPartnerStatus
        /// </summary>
        /// <returns></returns>
        [Log(Title = "BusinessPartnerStatus", BusinessType = BusinessType.CLEAN)]
        [ActionPermissionFilter(Permission = "bpbusinesspartnerstatus:delete")]
        [HttpPost("clean")]
        public IActionResult Clear()
        {
            if (!HttpContextExtension.IsAdmin(HttpContext))
            {
                return ToResponse(ResultCode.FAIL, "操作失败");
            }
            return SUCCESS(_BpBusinesspartnerstatusService.TruncateBpBusinesspartnerstatus());
        }

        /// <summary>
        /// 导入
        /// </summary>
        /// <param name="formFile"></param>
        /// <returns></returns>
        [HttpPost("importData")]
        [Log(Title = "BusinessPartnerStatus导入", BusinessType = BusinessType.IMPORT, IsSaveRequestData = false)]
        [ActionPermissionFilter(Permission = "bpbusinesspartnerstatus:import")]
        public IActionResult ImportData([FromForm(Name = "file")] IFormFile formFile)
        {
            List<BpBusinesspartnerstatusDto> list = new();
            using (var stream = formFile.OpenReadStream())
            {
                list = stream.Query<BpBusinesspartnerstatusDto>(startCell: "A1").ToList();
            }

            return SUCCESS(_BpBusinesspartnerstatusService.ImportBpBusinesspartnerstatus(list.Adapt<List<BpBusinesspartnerstatus>>()));
        }

        /// <summary>
        /// BusinessPartnerStatus导入模板下载
        /// </summary>
        /// <returns></returns>
        [HttpGet("importTemplate")]
        [Log(Title = "BusinessPartnerStatus模板", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        [AllowAnonymous]
        public IActionResult ImportTemplateExcel()
        {
            var result = DownloadImportTemplate(new List<BpBusinesspartnerstatusDto>() { }, "BpBusinesspartnerstatus");
            return ExportExcel(result.Item2, result.Item1);
        }

    }
}
