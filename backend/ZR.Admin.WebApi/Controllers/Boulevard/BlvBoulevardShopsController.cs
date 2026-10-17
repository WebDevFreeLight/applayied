using Microsoft.AspNetCore.Mvc;
using ZR.Model.Boulevard.Dto;
using ZR.Model.Boulevard;
using ZR.Service.Boulevard.IBoulevardService;
using MiniExcelLibs;

//创建时间：2025-09-16
namespace ZR.Admin.WebApi.Controllers.Boulevard
{
    /// <summary>
    /// shops
    /// </summary>
    [Route("boulevard/BlvBoulevardShops")]
    [ApiExplorerSettings(GroupName = "boulevard")]

    public class BlvBoulevardShopsController : BaseController
    {
        /// <summary>
        /// shops接口
        /// </summary>
        private readonly IBlvBoulevardShopsService _BlvBoulevardShopsService;

        public BlvBoulevardShopsController(IBlvBoulevardShopsService BlvBoulevardShopsService)
        {
            _BlvBoulevardShopsService = BlvBoulevardShopsService;
        }

        /// <summary>
        /// 查询shops列表
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "blvboulevardshops:list")]
        public IActionResult QueryBlvBoulevardShops([FromQuery] BlvBoulevardShopsQueryDto parm)
        {
            var response = _BlvBoulevardShopsService.GetList(parm);
            return SUCCESS(response);
        }


        /// <summary>
        /// 查询shops详情
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpGet("{Id}")]
        [ActionPermissionFilter(Permission = "blvboulevardshops:query")]
        public IActionResult GetBlvBoulevardShops(long Id)
        {
            var response = _BlvBoulevardShopsService.GetInfo(Id);
            
            var info = response.Adapt<BlvBoulevardShopsDto>();
            return SUCCESS(info);
        }

        /// <summary>
        /// 添加shops
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ActionPermissionFilter(Permission = "blvboulevardshops:add")]
        [Log(Title = "shops", BusinessType = BusinessType.INSERT)]
        public IActionResult AddBlvBoulevardShops([FromBody] BlvBoulevardShopsDto parm)
        {
            var modal = parm.Adapt<BlvBoulevardShops>().ToCreate(HttpContext);

            var response = _BlvBoulevardShopsService.AddBlvBoulevardShops(modal);

            return SUCCESS(response);
        }

        /// <summary>
        /// 更新shops
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [ActionPermissionFilter(Permission = "blvboulevardshops:edit")]
        [Log(Title = "shops", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdateBlvBoulevardShops([FromBody] BlvBoulevardShopsDto parm)
        {
            var modal = parm.Adapt<BlvBoulevardShops>().ToUpdate(HttpContext);
            var response = _BlvBoulevardShopsService.UpdateBlvBoulevardShops(modal);

            return ToResponse(response);
        }

        /// <summary>
        /// 删除shops
        /// </summary>
        /// <returns></returns>
        [HttpPost("delete/{ids}")]
        [ActionPermissionFilter(Permission = "blvboulevardshops:delete")]
        [Log(Title = "shops", BusinessType = BusinessType.DELETE)]
        public IActionResult DeleteBlvBoulevardShops([FromRoute]string ids)
        {
            var idArr = Tools.SplitAndConvert<long>(ids);

            return ToResponse(_BlvBoulevardShopsService.Delete(idArr, "删除shops"));
        }

        /// <summary>
        /// 导出shops
        /// </summary>
        /// <returns></returns>
        [Log(Title = "shops", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        [HttpGet("export")]
        [ActionPermissionFilter(Permission = "blvboulevardshops:export")]
        public IActionResult Export([FromQuery] BlvBoulevardShopsQueryDto parm)
        {
            var list = _BlvBoulevardShopsService.ExportList(parm).Result;
            if (list == null || list.Count <= 0)
            {
                return ToResponse(ResultCode.FAIL, "没有要导出的数据");
            }
            var result = ExportExcelMini(list, "shops", "shops");
            return ExportExcel(result.Item2, result.Item1);
        }

        /// <summary>
        /// 清空shops
        /// </summary>
        /// <returns></returns>
        [Log(Title = "shops", BusinessType = BusinessType.CLEAN)]
        [ActionPermissionFilter(Permission = "blvboulevardshops:delete")]
        [HttpPost("clean")]
        public IActionResult Clear()
        {
            if (!HttpContextExtension.IsAdmin(HttpContext))
            {
                return ToResponse(ResultCode.FAIL, "操作失败");
            }
            return SUCCESS(_BlvBoulevardShopsService.TruncateBlvBoulevardShops());
        }

        /// <summary>
        /// 导入
        /// </summary>
        /// <param name="formFile"></param>
        /// <returns></returns>
        [HttpPost("importData")]
        [Log(Title = "shops导入", BusinessType = BusinessType.IMPORT, IsSaveRequestData = false)]
        [ActionPermissionFilter(Permission = "blvboulevardshops:import")]
        public IActionResult ImportData([FromForm(Name = "file")] IFormFile formFile)
        {
            List<BlvBoulevardShopsDto> list = new();
            using (var stream = formFile.OpenReadStream())
            {
                list = stream.Query<BlvBoulevardShopsDto>(startCell: "A1").ToList();
            }

            return SUCCESS(_BlvBoulevardShopsService.ImportBlvBoulevardShops(list.Adapt<List<BlvBoulevardShops>>()));
        }

        /// <summary>
        /// shops导入模板下载
        /// </summary>
        /// <returns></returns>
        [HttpGet("importTemplate")]
        [Log(Title = "shops模板", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        [AllowAnonymous]
        public IActionResult ImportTemplateExcel()
        {
            var result = DownloadImportTemplate(new List<BlvBoulevardShopsDto>() { }, "BlvBoulevardShops");
            return ExportExcel(result.Item2, result.Item1);
        }

    }
}
