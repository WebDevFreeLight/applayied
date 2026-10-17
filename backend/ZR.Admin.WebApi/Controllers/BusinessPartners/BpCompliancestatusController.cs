using Microsoft.AspNetCore.Mvc;
using ZR.Model.BusinessPartners.Dto;
using ZR.Model.BusinessPartners;
using ZR.Service.BusinessPartners.IService;
using MiniExcelLibs;

//创建时间：2025-08-31
namespace ZR.Admin.WebApi.Controllers.BusinessPartners
{
    /// <summary>
    /// compliancestatus
    /// </summary>
    [Route("partnertype/BpCompliancestatus")]
    public class BpCompliancestatusController : BaseController
    {
        /// <summary>
        /// compliancestatus接口
        /// </summary>
        private readonly IBpCompliancestatusService _BpCompliancestatusService;

        public BpCompliancestatusController(IBpCompliancestatusService BpCompliancestatusService)
        {
            _BpCompliancestatusService = BpCompliancestatusService;
        }

        /// <summary>
        /// 查询compliancestatus列表
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "bpcompliancestatus:list")]
        public IActionResult QueryBpCompliancestatus([FromQuery] BpCompliancestatusQueryDto parm)
        {
            var response = _BpCompliancestatusService.GetList(parm);
            return SUCCESS(response);
        }


        /// <summary>
        /// 查询compliancestatus详情
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpGet("{Id}")]
        [ActionPermissionFilter(Permission = "bpcompliancestatus:query")]
        public IActionResult GetBpCompliancestatus(long Id)
        {
            var response = _BpCompliancestatusService.GetInfo(Id);
            
            var info = response.Adapt<BpCompliancestatusDto>();
            return SUCCESS(info);
        }

        /// <summary>
        /// 添加compliancestatus
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ActionPermissionFilter(Permission = "bpcompliancestatus:add")]
        [Log(Title = "compliancestatus", BusinessType = BusinessType.INSERT)]
        public IActionResult AddBpCompliancestatus([FromBody] BpCompliancestatusDto parm)
        {
            var modal = parm.Adapt<BpCompliancestatus>().ToCreate(HttpContext);

            var response = _BpCompliancestatusService.AddBpCompliancestatus(modal);

            return SUCCESS(response);
        }

        /// <summary>
        /// 更新compliancestatus
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [ActionPermissionFilter(Permission = "bpcompliancestatus:edit")]
        [Log(Title = "compliancestatus", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdateBpCompliancestatus([FromBody] BpCompliancestatusDto parm)
        {
            var modal = parm.Adapt<BpCompliancestatus>().ToUpdate(HttpContext);
            var response = _BpCompliancestatusService.UpdateBpCompliancestatus(modal);

            return ToResponse(response);
        }

        /// <summary>
        /// 删除compliancestatus
        /// </summary>
        /// <returns></returns>
        [HttpPost("delete/{ids}")]
        [ActionPermissionFilter(Permission = "bpcompliancestatus:delete")]
        [Log(Title = "compliancestatus", BusinessType = BusinessType.DELETE)]
        public IActionResult DeleteBpCompliancestatus([FromRoute]string ids)
        {
            var idArr = Tools.SplitAndConvert<long>(ids);

            return ToResponse(_BpCompliancestatusService.Delete(idArr, "删除compliancestatus"));
        }

        /// <summary>
        /// 导出compliancestatus
        /// </summary>
        /// <returns></returns>
        [Log(Title = "compliancestatus", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        [HttpGet("export")]
        [ActionPermissionFilter(Permission = "bpcompliancestatus:export")]
        public IActionResult Export([FromQuery] BpCompliancestatusQueryDto parm)
        {
            var list = _BpCompliancestatusService.ExportList(parm).Result;
            if (list == null || list.Count <= 0)
            {
                return ToResponse(ResultCode.FAIL, "没有要导出的数据");
            }
            var result = ExportExcelMini(list, "compliancestatus", "compliancestatus");
            return ExportExcel(result.Item2, result.Item1);
        }

        /// <summary>
        /// 清空compliancestatus
        /// </summary>
        /// <returns></returns>
        [Log(Title = "compliancestatus", BusinessType = BusinessType.CLEAN)]
        [ActionPermissionFilter(Permission = "bpcompliancestatus:delete")]
        [HttpPost("clean")]
        public IActionResult Clear()
        {
            if (!HttpContextExtension.IsAdmin(HttpContext))
            {
                return ToResponse(ResultCode.FAIL, "操作失败");
            }
            return SUCCESS(_BpCompliancestatusService.TruncateBpCompliancestatus());
        }

        /// <summary>
        /// 导入
        /// </summary>
        /// <param name="formFile"></param>
        /// <returns></returns>
        [HttpPost("importData")]
        [Log(Title = "compliancestatus导入", BusinessType = BusinessType.IMPORT, IsSaveRequestData = false)]
        [ActionPermissionFilter(Permission = "bpcompliancestatus:import")]
        public IActionResult ImportData([FromForm(Name = "file")] IFormFile formFile)
        {
            List<BpCompliancestatusDto> list = new();
            using (var stream = formFile.OpenReadStream())
            {
                list = stream.Query<BpCompliancestatusDto>(startCell: "A1").ToList();
            }

            return SUCCESS(_BpCompliancestatusService.ImportBpCompliancestatus(list.Adapt<List<BpCompliancestatus>>()));
        }

        /// <summary>
        /// compliancestatus导入模板下载
        /// </summary>
        /// <returns></returns>
        [HttpGet("importTemplate")]
        [Log(Title = "compliancestatus模板", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        [AllowAnonymous]
        public IActionResult ImportTemplateExcel()
        {
            var result = DownloadImportTemplate(new List<BpCompliancestatusDto>() { }, "BpCompliancestatus");
            return ExportExcel(result.Item2, result.Item1);
        }

    }
}
