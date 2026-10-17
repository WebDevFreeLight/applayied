using Microsoft.AspNetCore.Mvc;
using ZR.Model.BusinessPartners.Dto;
using ZR.Model.BusinessPartners;
using ZR.Service.BusinessPartners.IService;
using MiniExcelLibs;

//创建时间：2025-08-31
namespace ZR.Admin.WebApi.Controllers.BusinessPartners
{
    /// <summary>
    /// BusinessPartnerkind
    /// </summary>
    [Route("partnertype/BpBusinesspartnerkind")]
    public class BpBusinesspartnerkindController : BaseController
    {
        /// <summary>
        /// BusinessPartnerkind接口
        /// </summary>
        private readonly IBpBusinesspartnerkindService _BpBusinesspartnerkindService;

        public BpBusinesspartnerkindController(IBpBusinesspartnerkindService BpBusinesspartnerkindService)
        {
            _BpBusinesspartnerkindService = BpBusinesspartnerkindService;
        }

        /// <summary>
        /// 查询BusinessPartnerkind列表
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "bpbusinesspartnerkind:list")]
        public IActionResult QueryBpBusinesspartnerkind([FromQuery] BpBusinesspartnerkindQueryDto parm)
        {
            var response = _BpBusinesspartnerkindService.GetList(parm);
            return SUCCESS(response);
        }


        /// <summary>
        /// 查询BusinessPartnerkind详情
        /// </summary>
        /// <param name="Kindid"></param>
        /// <returns></returns>
        [HttpGet("{Kindid}")]
        [ActionPermissionFilter(Permission = "bpbusinesspartnerkind:query")]
        public IActionResult GetBpBusinesspartnerkind(long Kindid)
        {
            var response = _BpBusinesspartnerkindService.GetInfo(Kindid);
            
            var info = response.Adapt<BpBusinesspartnerkindDto>();
            return SUCCESS(info);
        }

        /// <summary>
        /// 添加BusinessPartnerkind
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ActionPermissionFilter(Permission = "bpbusinesspartnerkind:add")]
        [Log(Title = "BusinessPartnerkind", BusinessType = BusinessType.INSERT)]
        public IActionResult AddBpBusinesspartnerkind([FromBody] BpBusinesspartnerkindDto parm)
        {
            var modal = parm.Adapt<BpBusinesspartnerkind>().ToCreate(HttpContext);

            var response = _BpBusinesspartnerkindService.AddBpBusinesspartnerkind(modal);

            return SUCCESS(response);
        }

        /// <summary>
        /// 更新BusinessPartnerkind
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [ActionPermissionFilter(Permission = "bpbusinesspartnerkind:edit")]
        [Log(Title = "BusinessPartnerkind", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdateBpBusinesspartnerkind([FromBody] BpBusinesspartnerkindDto parm)
        {
            var modal = parm.Adapt<BpBusinesspartnerkind>().ToUpdate(HttpContext);
            var response = _BpBusinesspartnerkindService.UpdateBpBusinesspartnerkind(modal);

            return ToResponse(response);
        }

        /// <summary>
        /// 删除BusinessPartnerkind
        /// </summary>
        /// <returns></returns>
        [HttpPost("delete/{ids}")]
        [ActionPermissionFilter(Permission = "bpbusinesspartnerkind:delete")]
        [Log(Title = "BusinessPartnerkind", BusinessType = BusinessType.DELETE)]
        public IActionResult DeleteBpBusinesspartnerkind([FromRoute]string ids)
        {
            var idArr = Tools.SplitAndConvert<long>(ids);

            return ToResponse(_BpBusinesspartnerkindService.Delete(idArr, "删除BusinessPartnerkind"));
        }

        /// <summary>
        /// 导出BusinessPartnerkind
        /// </summary>
        /// <returns></returns>
        [Log(Title = "BusinessPartnerkind", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        [HttpGet("export")]
        [ActionPermissionFilter(Permission = "bpbusinesspartnerkind:export")]
        public IActionResult Export([FromQuery] BpBusinesspartnerkindQueryDto parm)
        {
            var list = _BpBusinesspartnerkindService.ExportList(parm).Result;
            if (list == null || list.Count <= 0)
            {
                return ToResponse(ResultCode.FAIL, "没有要导出的数据");
            }
            var result = ExportExcelMini(list, "BusinessPartnerkind", "BusinessPartnerkind");
            return ExportExcel(result.Item2, result.Item1);
        }

        /// <summary>
        /// 清空BusinessPartnerkind
        /// </summary>
        /// <returns></returns>
        [Log(Title = "BusinessPartnerkind", BusinessType = BusinessType.CLEAN)]
        [ActionPermissionFilter(Permission = "bpbusinesspartnerkind:delete")]
        [HttpPost("clean")]
        public IActionResult Clear()
        {
            if (!HttpContextExtension.IsAdmin(HttpContext))
            {
                return ToResponse(ResultCode.FAIL, "操作失败");
            }
            return SUCCESS(_BpBusinesspartnerkindService.TruncateBpBusinesspartnerkind());
        }

        /// <summary>
        /// 导入
        /// </summary>
        /// <param name="formFile"></param>
        /// <returns></returns>
        [HttpPost("importData")]
        [Log(Title = "BusinessPartnerkind导入", BusinessType = BusinessType.IMPORT, IsSaveRequestData = false)]
        [ActionPermissionFilter(Permission = "bpbusinesspartnerkind:import")]
        public IActionResult ImportData([FromForm(Name = "file")] IFormFile formFile)
        {
            List<BpBusinesspartnerkindDto> list = new();
            using (var stream = formFile.OpenReadStream())
            {
                list = stream.Query<BpBusinesspartnerkindDto>(startCell: "A1").ToList();
            }

            return SUCCESS(_BpBusinesspartnerkindService.ImportBpBusinesspartnerkind(list.Adapt<List<BpBusinesspartnerkind>>()));
        }

        /// <summary>
        /// BusinessPartnerkind导入模板下载
        /// </summary>
        /// <returns></returns>
        [HttpGet("importTemplate")]
        [Log(Title = "BusinessPartnerkind模板", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        [AllowAnonymous]
        public IActionResult ImportTemplateExcel()
        {
            var result = DownloadImportTemplate(new List<BpBusinesspartnerkindDto>() { }, "BpBusinesspartnerkind");
            return ExportExcel(result.Item2, result.Item1);
        }

    }
}
