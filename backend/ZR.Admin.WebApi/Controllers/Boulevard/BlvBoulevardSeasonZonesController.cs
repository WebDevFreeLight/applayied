using Microsoft.AspNetCore.Mvc;
using ZR.Model.Boulevard.Dto;
using ZR.Model.Boulevard;
using ZR.Service.Boulevard.IBoulevardService;
using MiniExcelLibs;

//创建时间：2025-09-16
namespace ZR.Admin.WebApi.Controllers.Boulevard
{
    /// <summary>
    /// boulevard_season_zones
    /// </summary>
    [Route("boulevard/BlvBoulevardSeasonZones")]
    [ApiExplorerSettings(GroupName = "boulevard")]

    public class BlvBoulevardSeasonZonesController : BaseController
    {
        /// <summary>
        /// boulevard_season_zones接口
        /// </summary>
        private readonly IBlvBoulevardSeasonZonesService _BlvBoulevardSeasonZonesService;

        public BlvBoulevardSeasonZonesController(IBlvBoulevardSeasonZonesService BlvBoulevardSeasonZonesService)
        {
            _BlvBoulevardSeasonZonesService = BlvBoulevardSeasonZonesService;
        }

        /// <summary>
        /// 查询boulevard_season_zones列表
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "blvboulevardseasonzones:list")]
        public IActionResult QueryBlvBoulevardSeasonZones([FromQuery] BlvBoulevardSeasonZonesQueryDto parm)
        {
            var response = _BlvBoulevardSeasonZonesService.GetList(parm);
            return SUCCESS(response);
        }



        /// <summary>
        /// 查询boulevard_season_zones列表
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        [HttpGet("list_all")]
        [ActionPermissionFilter(Permission = "blvboulevardseasonzones:list")]
        public IActionResult QueryBlvBoulevardSeasonZones_all([FromQuery] BlvBoulevardSeasonZonesQueryDto parm)
        {
            var response = _BlvBoulevardSeasonZonesService.GetListAll(parm);
            return SUCCESS(response);
        }


        /// <summary>
        /// 查询boulevard_season_zones详情
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpGet("{Id}")]
        [ActionPermissionFilter(Permission = "blvboulevardseasonzones:query")]
        public IActionResult GetBlvBoulevardSeasonZones(long Id)
        {
            var response = _BlvBoulevardSeasonZonesService.GetInfo(Id);
            
            var info = response.Adapt<BlvBoulevardSeasonZonesDto>();
            return SUCCESS(info);
        }

        /// <summary>
        /// 添加boulevard_season_zones
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ActionPermissionFilter(Permission = "blvboulevardseasonzones:add")]
        [Log(Title = "boulevard_season_zones", BusinessType = BusinessType.INSERT)]
        public IActionResult AddBlvBoulevardSeasonZones([FromBody] BlvBoulevardSeasonZonesDto parm)
        {
            var modal = parm.Adapt<BlvBoulevardSeasonZones>().ToCreate(HttpContext);

            var response = _BlvBoulevardSeasonZonesService.AddBlvBoulevardSeasonZones(modal);

            return SUCCESS(response);
        }

        /// <summary>
        /// 更新boulevard_season_zones
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [ActionPermissionFilter(Permission = "blvboulevardseasonzones:edit")]
        [Log(Title = "boulevard_season_zones", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdateBlvBoulevardSeasonZones([FromBody] BlvBoulevardSeasonZonesDto parm)
        {
            var modal = parm.Adapt<BlvBoulevardSeasonZones>().ToUpdate(HttpContext);
            var response = _BlvBoulevardSeasonZonesService.UpdateBlvBoulevardSeasonZones(modal);

            return ToResponse(response);
        }

        /// <summary>
        /// 删除boulevard_season_zones
        /// </summary>
        /// <returns></returns>
        [HttpPost("delete/{ids}")]
        [ActionPermissionFilter(Permission = "blvboulevardseasonzones:delete")]
        [Log(Title = "boulevard_season_zones", BusinessType = BusinessType.DELETE)]
        public IActionResult DeleteBlvBoulevardSeasonZones([FromRoute]string ids)
        {
            var idArr = Tools.SplitAndConvert<long>(ids);

            return ToResponse(_BlvBoulevardSeasonZonesService.Delete(idArr, "删除boulevard_season_zones"));
        }

        /// <summary>
        /// 导出boulevard_season_zones
        /// </summary>
        /// <returns></returns>
        [Log(Title = "boulevard_season_zones", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        [HttpGet("export")]
        [ActionPermissionFilter(Permission = "blvboulevardseasonzones:export")]
        public IActionResult Export([FromQuery] BlvBoulevardSeasonZonesQueryDto parm)
        {
            var list = _BlvBoulevardSeasonZonesService.ExportList(parm).Result;
            if (list == null || list.Count <= 0)
            {
                return ToResponse(ResultCode.FAIL, "没有要导出的数据");
            }
            var result = ExportExcelMini(list, "boulevard_season_zones", "boulevard_season_zones");
            return ExportExcel(result.Item2, result.Item1);
        }

        /// <summary>
        /// 清空boulevard_season_zones
        /// </summary>
        /// <returns></returns>
        [Log(Title = "boulevard_season_zones", BusinessType = BusinessType.CLEAN)]
        [ActionPermissionFilter(Permission = "blvboulevardseasonzones:delete")]
        [HttpPost("clean")]
        public IActionResult Clear()
        {
            if (!HttpContextExtension.IsAdmin(HttpContext))
            {
                return ToResponse(ResultCode.FAIL, "操作失败");
            }
            return SUCCESS(_BlvBoulevardSeasonZonesService.TruncateBlvBoulevardSeasonZones());
        }

        /// <summary>
        /// 导入
        /// </summary>
        /// <param name="formFile"></param>
        /// <returns></returns>
        [HttpPost("importData")]
        [Log(Title = "boulevard_season_zones导入", BusinessType = BusinessType.IMPORT, IsSaveRequestData = false)]
        [ActionPermissionFilter(Permission = "blvboulevardseasonzones:import")]
        public IActionResult ImportData([FromForm(Name = "file")] IFormFile formFile)
        {
            List<BlvBoulevardSeasonZonesDto> list = new();
            using (var stream = formFile.OpenReadStream())
            {
                list = stream.Query<BlvBoulevardSeasonZonesDto>(startCell: "A1").ToList();
            }

            return SUCCESS(_BlvBoulevardSeasonZonesService.ImportBlvBoulevardSeasonZones(list.Adapt<List<BlvBoulevardSeasonZones>>()));
        }

        /// <summary>
        /// boulevard_season_zones导入模板下载
        /// </summary>
        /// <returns></returns>
        [HttpGet("importTemplate")]
        [Log(Title = "boulevard_season_zones模板", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        [AllowAnonymous]
        public IActionResult ImportTemplateExcel()
        {
            var result = DownloadImportTemplate(new List<BlvBoulevardSeasonZonesDto>() { }, "BlvBoulevardSeasonZones");
            return ExportExcel(result.Item2, result.Item1);
        }

    }
}
