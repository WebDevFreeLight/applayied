using Microsoft.AspNetCore.Mvc;
using ZR.Model.BusinessPartners.Dto;
using ZR.Model.BusinessPartners;
using ZR.Service.BusinessPartners.IService;
using MiniExcelLibs;

//创建时间：2025-08-31
namespace ZR.Admin.WebApi.Controllers.BusinessPartners
{
    /// <summary>
    /// businesspartner
    /// </summary>
    [Route("businesspartnerdata/BpBusinesspartner")]
    [ApiExplorerSettings(GroupName = "boulevard")]

    public class BpBusinesspartnerController : BaseController
    {
        /// <summary>
        /// businesspartner接口
        /// </summary>
        private readonly IBpBusinesspartnerService _BpBusinesspartnerService;

        public BpBusinesspartnerController(IBpBusinesspartnerService BpBusinesspartnerService)
        {
            _BpBusinesspartnerService = BpBusinesspartnerService;
        }

        /// <summary>
        /// 查询businesspartner列表
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "bpbusinesspartner:list")]
        public IActionResult QueryBpBusinesspartner([FromQuery] BpBusinesspartnerQueryDto parm)
        {
            var response = _BpBusinesspartnerService.GetList(parm);
            return SUCCESS(response);
        }


        /// <summary>
        /// 查询businesspartner详情
        /// </summary>
        /// <param name="Partnerid"></param>
        /// <returns></returns>
        [HttpGet("{Partnerid}")]
        [ActionPermissionFilter(Permission = "bpbusinesspartner:query")]
        public IActionResult GetBpBusinesspartner(long Partnerid)
        {
            var response = _BpBusinesspartnerService.GetInfo(Partnerid);
            
            var info = response.Adapt<BpBusinesspartnerDto_Res>();
            return SUCCESS(info);
        }

        /// <summary>
        /// 添加businesspartner
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ActionPermissionFilter(Permission = "bpbusinesspartner:add")]
        [Log(Title = "businesspartner", BusinessType = BusinessType.INSERT)]
        public IActionResult AddBpBusinesspartner([FromBody] BpBusinesspartnerDto_Res parm)
        {
            var modal = parm.Adapt<BpBusinesspartner>().ToCreate(HttpContext);// sd  

            var response = _BpBusinesspartnerService.AddBpBusinesspartner(modal);

            return SUCCESS(response);
        }

        /// <summary>
        /// 更新businesspartner
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [ActionPermissionFilter(Permission = "bpbusinesspartner:edit")]
        [Log(Title = "businesspartner", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdateBpBusinesspartner([FromBody] BpBusinesspartnerDto_Res parm)
        {
            var modal = parm.Adapt<BpBusinesspartner>().ToUpdate(HttpContext);
            var response = _BpBusinesspartnerService.UpdateBpBusinesspartner(modal);

            return ToResponse(response);
        }

        /// <summary>
        /// 删除businesspartner
        /// </summary>
        /// <returns></returns>
        [HttpPost("delete/{ids}")]
        [ActionPermissionFilter(Permission = "bpbusinesspartner:delete")]
        [Log(Title = "businesspartner", BusinessType = BusinessType.DELETE)]
        public IActionResult DeleteBpBusinesspartner([FromRoute]string ids)
        {
            var idArr = Tools.SplitAndConvert<long>(ids);

            return ToResponse(_BpBusinesspartnerService.Delete(idArr, "删除businesspartner"));
        }

        /// <summary>
        /// 导出businesspartner
        /// </summary>
        /// <returns></returns>
        [Log(Title = "businesspartner", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        [HttpGet("export")]
        [ActionPermissionFilter(Permission = "bpbusinesspartner:export")]
        public IActionResult Export([FromQuery] BpBusinesspartnerQueryDto parm)
        {
            var list = _BpBusinesspartnerService.ExportList(parm).Result;
            if (list == null || list.Count <= 0)
            {
                return ToResponse(ResultCode.FAIL, "没有要导出的数据");
            }
            var result = ExportExcelMini(list, "businesspartner", "businesspartner");
            return ExportExcel(result.Item2, result.Item1);
        }

        /// <summary>
        /// 清空businesspartner
        /// </summary>
        /// <returns></returns>
        [Log(Title = "businesspartner", BusinessType = BusinessType.CLEAN)]
        [ActionPermissionFilter(Permission = "bpbusinesspartner:delete")]
        [HttpPost("clean")]
        public IActionResult Clear()
        {
            if (!HttpContextExtension.IsAdmin(HttpContext))
            {
                return ToResponse(ResultCode.FAIL, "操作失败");
            }
            return SUCCESS(_BpBusinesspartnerService.TruncateBpBusinesspartner());
        }

        /// <summary>
        /// 导入
        /// </summary>
        /// <param name="formFile"></param>
        /// <returns></returns>
        [HttpPost("importData")]
        [Log(Title = "businesspartner导入", BusinessType = BusinessType.IMPORT, IsSaveRequestData = false)]
        [ActionPermissionFilter(Permission = "bpbusinesspartner:import")]
        public IActionResult ImportData([FromForm(Name = "file")] IFormFile formFile)
        {
            List<BpBusinesspartnerDto_Res> list = new();
            using (var stream = formFile.OpenReadStream())
            {
                list = stream.Query<BpBusinesspartnerDto_Res>(startCell: "A1").ToList();
            }

            return SUCCESS(_BpBusinesspartnerService.ImportBpBusinesspartner(list.Adapt<List<BpBusinesspartner>>()));
        }

        /// <summary>
        /// businesspartner导入模板下载
        /// </summary>
        /// <returns></returns>
        [HttpGet("importTemplate")]
        [Log(Title = "businesspartner模板", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        [AllowAnonymous]
        public IActionResult ImportTemplateExcel()
        {
            var result = DownloadImportTemplate(new List<BpBusinesspartnerDto_Res>() { }, "BpBusinesspartner");
            return ExportExcel(result.Item2, result.Item1);
        }

    }
}
