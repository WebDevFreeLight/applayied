using Microsoft.AspNetCore.Mvc;
using ZR.Model.Boulevard.Dto;
using ZR.Model.Boulevard;
using ZR.Service.Boulevard.IBoulevardService;
using MiniExcelLibs;

//创建时间：2025-09-16
namespace ZR.Admin.WebApi.Controllers.Boulevard
{
    /// <summary>
    /// shop_status
    /// </summary>
    [Route("boulevard/BlvBoulevardShopStatus")]
    [ApiExplorerSettings(GroupName = "boulevard")]

    public class BlvBoulevardShopStatusController : BaseController
    {
        /// <summary>
        /// shop_status接口
        /// </summary>
        private readonly IBlvBoulevardShopStatusService _BlvBoulevardShopStatusService;

        public BlvBoulevardShopStatusController(IBlvBoulevardShopStatusService BlvBoulevardShopStatusService)
        {
            _BlvBoulevardShopStatusService = BlvBoulevardShopStatusService;
        }

        /// <summary>
        /// 查询shop_status列表
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "blvboulevardshopstatus:list")]
        public IActionResult QueryBlvBoulevardShopStatus([FromQuery] BlvBoulevardShopStatusQueryDto parm)
        {
            var response = _BlvBoulevardShopStatusService.GetList(parm);
            return SUCCESS(response);
        }


        /// <summary>
        /// 查询shop_status详情
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpGet("{Id}")]
        [ActionPermissionFilter(Permission = "blvboulevardshopstatus:query")]
        public IActionResult GetBlvBoulevardShopStatus(long Id)
        {
            var response = _BlvBoulevardShopStatusService.GetInfo(Id);
            
            var info = response.Adapt<BlvBoulevardShopStatusDto>();
            return SUCCESS(info);
        }

        /// <summary>
        /// 添加shop_status
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ActionPermissionFilter(Permission = "blvboulevardshopstatus:add")]
        [Log(Title = "shop_status", BusinessType = BusinessType.INSERT)]
        public IActionResult AddBlvBoulevardShopStatus([FromBody] BlvBoulevardShopStatusDto parm)
        {
            var modal = parm.Adapt<BlvBoulevardShopStatus>().ToCreate(HttpContext);

            var response = _BlvBoulevardShopStatusService.AddBlvBoulevardShopStatus(modal);

            return SUCCESS(response);
        }

        /// <summary>
        /// 更新shop_status
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [ActionPermissionFilter(Permission = "blvboulevardshopstatus:edit")]
        [Log(Title = "shop_status", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdateBlvBoulevardShopStatus([FromBody] BlvBoulevardShopStatusDto parm)
        {
            var modal = parm.Adapt<BlvBoulevardShopStatus>().ToUpdate(HttpContext);
            var response = _BlvBoulevardShopStatusService.UpdateBlvBoulevardShopStatus(modal);

            return ToResponse(response);
        }

        /// <summary>
        /// 删除shop_status
        /// </summary>
        /// <returns></returns>
        [HttpPost("delete/{ids}")]
        [ActionPermissionFilter(Permission = "blvboulevardshopstatus:delete")]
        [Log(Title = "shop_status", BusinessType = BusinessType.DELETE)]
        public IActionResult DeleteBlvBoulevardShopStatus([FromRoute]string ids)
        {
            var idArr = Tools.SplitAndConvert<long>(ids);

            return ToResponse(_BlvBoulevardShopStatusService.Delete(idArr, "删除shop_status"));
        }

        /// <summary>
        /// 导出shop_status
        /// </summary>
        /// <returns></returns>
        [Log(Title = "shop_status", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        [HttpGet("export")]
        [ActionPermissionFilter(Permission = "blvboulevardshopstatus:export")]
        public IActionResult Export([FromQuery] BlvBoulevardShopStatusQueryDto parm)
        {
            var list = _BlvBoulevardShopStatusService.ExportList(parm).Result;
            if (list == null || list.Count <= 0)
            {
                return ToResponse(ResultCode.FAIL, "没有要导出的数据");
            }
            var result = ExportExcelMini(list, "shop_status", "shop_status");
            return ExportExcel(result.Item2, result.Item1);
        }

        /// <summary>
        /// 清空shop_status
        /// </summary>
        /// <returns></returns>
        [Log(Title = "shop_status", BusinessType = BusinessType.CLEAN)]
        [ActionPermissionFilter(Permission = "blvboulevardshopstatus:delete")]
        [HttpPost("clean")]
        public IActionResult Clear()
        {
            if (!HttpContextExtension.IsAdmin(HttpContext))
            {
                return ToResponse(ResultCode.FAIL, "操作失败");
            }
            return SUCCESS(_BlvBoulevardShopStatusService.TruncateBlvBoulevardShopStatus());
        }

        /// <summary>
        /// 导入
        /// </summary>
        /// <param name="formFile"></param>
        /// <returns></returns>
        [HttpPost("importData")]
        [Log(Title = "shop_status导入", BusinessType = BusinessType.IMPORT, IsSaveRequestData = false)]
        [ActionPermissionFilter(Permission = "blvboulevardshopstatus:import")]
        public IActionResult ImportData([FromForm(Name = "file")] IFormFile formFile)
        {
            List<BlvBoulevardShopStatusDto> list = new();
            using (var stream = formFile.OpenReadStream())
            {
                list = stream.Query<BlvBoulevardShopStatusDto>(startCell: "A1").ToList();
            }

            return SUCCESS(_BlvBoulevardShopStatusService.ImportBlvBoulevardShopStatus(list.Adapt<List<BlvBoulevardShopStatus>>()));
        }

        /// <summary>
        /// shop_status导入模板下载
        /// </summary>
        /// <returns></returns>
        [HttpGet("importTemplate")]
        [Log(Title = "shop_status模板", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        [AllowAnonymous]
        public IActionResult ImportTemplateExcel()
        {
            var result = DownloadImportTemplate(new List<BlvBoulevardShopStatusDto>() { }, "BlvBoulevardShopStatus");
            return ExportExcel(result.Item2, result.Item1);
        }

    }
}
