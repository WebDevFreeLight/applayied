using Microsoft.AspNetCore.Mvc;
using ZR.Model.Boulevard.Dto;
using ZR.Model.Boulevard;
using ZR.Service.Boulevard.IBoulevardService;
using MiniExcelLibs;

//创建时间：2025-09-27
namespace ZR.Admin.WebApi.Controllers.Boulevard
{
    /// <summary>
    /// boulevard_seasons_Shops
    /// </summary>
    [Route("boulevard/BlvBoulevardSeasonsShops")]
    [ApiExplorerSettings(GroupName = "boulevard")]

    public class BlvBoulevardSeasonsShopsController : BaseController
    {
        /// <summary>
        /// boulevard_seasons_Shops接口
        /// </summary>
        private readonly IBlvBoulevardSeasonsShopsService _BlvBoulevardSeasonsShopsService;

        public BlvBoulevardSeasonsShopsController(IBlvBoulevardSeasonsShopsService BlvBoulevardSeasonsShopsService)
        {
            _BlvBoulevardSeasonsShopsService = BlvBoulevardSeasonsShopsService;
        }

        /// <summary>
        /// 查询boulevard_seasons_Shops列表
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "blvboulevardseasonsshops:list")]
        public IActionResult QueryBlvBoulevardSeasonsShops([FromQuery] BlvBoulevardSeasonsShopsQueryDto parm)
        {
            var response = _BlvBoulevardSeasonsShopsService.GetList(parm);
            return SUCCESS(response);
        }


        /// <summary>
        /// 查询boulevard_seasons_Shops列表
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        [HttpGet("list_all")]
        [ActionPermissionFilter(Permission = "blvboulevardseasonsshops:list_all")]
        public IActionResult QueryBlvBoulevardSeasonsShopsAll([FromQuery] BlvBoulevardSeasonsShopsQueryDto parm)
        {
            var response = _BlvBoulevardSeasonsShopsService.GetListAll(parm);
            return SUCCESS(response);
        }

        /// <summary>
        /// 查询boulevard_seasons_Shops详情 ds
        /// </summary> 
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpGet("{Id}")]
        [ActionPermissionFilter(Permission = "blvboulevardseasonsshops:query")]
        public IActionResult GetBlvBoulevardSeasonsShops(long Id)
        {
            var response = _BlvBoulevardSeasonsShopsService.GetInfo(Id);
            
            var info = response.Adapt<BlvBoulevardSeasonsShopsDto>();
            return SUCCESS(info);
        }

        /// <summary>
        /// 添加boulevard_seasons_Shops
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ActionPermissionFilter(Permission = "blvboulevardseasonsshops:add")]
        [Log(Title = "boulevard_seasons_Shops", BusinessType = BusinessType.INSERT)]
        public IActionResult AddBlvBoulevardSeasonsShops([FromBody] BlvBoulevardSeasonsShopsDto parm)
        {
            var modal = parm.Adapt<BlvBoulevardSeasonsShops>().ToCreate(HttpContext);

            var response = _BlvBoulevardSeasonsShopsService.AddBlvBoulevardSeasonsShops(modal);

            return SUCCESS(response);
        }

        /// <summary>
        /// 更新boulevard_seasons_Shops
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [ActionPermissionFilter(Permission = "blvboulevardseasonsshops:edit")]
        [Log(Title = "boulevard_seasons_Shops", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdateBlvBoulevardSeasonsShops([FromBody] BlvBoulevardSeasonsShopsDto parm)
        {
            var modal = parm.Adapt<BlvBoulevardSeasonsShops>().ToUpdate(HttpContext);
            var response = _BlvBoulevardSeasonsShopsService.UpdateBlvBoulevardSeasonsShops(modal);

            return ToResponse(response);
        }

        /// <summary>
        /// 删除boulevard_seasons_Shops
        /// </summary>
        /// <returns></returns>
        [HttpPost("delete/{ids}")]
        [ActionPermissionFilter(Permission = "blvboulevardseasonsshops:delete")]
        [Log(Title = "boulevard_seasons_Shops", BusinessType = BusinessType.DELETE)]
        public IActionResult DeleteBlvBoulevardSeasonsShops([FromRoute]string ids)
        {
            var idArr = Tools.SplitAndConvert<long>(ids);

            return ToResponse(_BlvBoulevardSeasonsShopsService.Delete(idArr, "删除boulevard_seasons_Shops"));
        }

        /// <summary>
        /// 导出boulevard_seasons_Shops
        /// </summary>
        /// <returns></returns>
        [Log(Title = "boulevard_seasons_Shops", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        [HttpGet("export")]
        [ActionPermissionFilter(Permission = "blvboulevardseasonsshops:export")]
        public IActionResult Export([FromQuery] BlvBoulevardSeasonsShopsQueryDto parm)
        {
            var list = _BlvBoulevardSeasonsShopsService.ExportList(parm).Result;
            if (list == null || list.Count <= 0)
            {
                return ToResponse(ResultCode.FAIL, "没有要导出的数据");
            }
            var result = ExportExcelMini(list, "boulevard_seasons_Shops", "boulevard_seasons_Shops");
            return ExportExcel(result.Item2, result.Item1);
        }

        /// <summary>
        /// 清空boulevard_seasons_Shops
        /// </summary>
        /// <returns></returns>
        [Log(Title = "boulevard_seasons_Shops", BusinessType = BusinessType.CLEAN)]
        [ActionPermissionFilter(Permission = "blvboulevardseasonsshops:delete")]
        [HttpPost("clean")]
        public IActionResult Clear()
        {
            if (!HttpContextExtension.IsAdmin(HttpContext))
            {
                return ToResponse(ResultCode.FAIL, "操作失败");
            }
            return SUCCESS(_BlvBoulevardSeasonsShopsService.TruncateBlvBoulevardSeasonsShops());
        }

        /// <summary>
        /// 导入
        /// </summary>
        /// <param name="formFile"></param>
        /// <returns></returns>
        [HttpPost("importData")]
        [Log(Title = "boulevard_seasons_Shops导入", BusinessType = BusinessType.IMPORT, IsSaveRequestData = false)]
        [ActionPermissionFilter(Permission = "blvboulevardseasonsshops:import")]
        public IActionResult ImportData([FromForm(Name = "file")] IFormFile formFile)
        {
            List<BlvBoulevardSeasonsShopsDto> list = new();
            using (var stream = formFile.OpenReadStream())
            {
                list = stream.Query<BlvBoulevardSeasonsShopsDto>(startCell: "A1").ToList();
            }

            return SUCCESS(_BlvBoulevardSeasonsShopsService.ImportBlvBoulevardSeasonsShops(list.Adapt<List<BlvBoulevardSeasonsShops>>()));
        }

        /// <summary>
        /// boulevard_seasons_Shops导入模板下载
        /// </summary>
        /// <returns></returns>
        [HttpGet("importTemplate")]
        [Log(Title = "boulevard_seasons_Shops模板", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        [AllowAnonymous]
        public IActionResult ImportTemplateExcel()
        {
            var result = DownloadImportTemplate(new List<BlvBoulevardSeasonsShopsDto>() { }, "BlvBoulevardSeasonsShops");
            return ExportExcel(result.Item2, result.Item1);
        }

    }
}
