using Microsoft.AspNetCore.Mvc;
using ZR.Model.Boulevard.Dto;
using ZR.Model.Boulevard;
using ZR.Service.Boulevard.IBoulevardService;
using MiniExcelLibs;

//创建时间：2025-09-16
namespace ZR.Admin.WebApi.Controllers.Boulevard
{
    /// <summary>
    /// boulevard_seasons
    /// </summary>
    [Route("boulevard/BlvBoulevardSeasons")]
    //[AllowAnonymous]

    [ApiExplorerSettings(GroupName = "boulevard")]
    public class BlvBoulevardSeasonsController : BaseController
    {
        /// <summary>
        /// boulevard_seasons接口
        /// </summary>
        private readonly IBlvBoulevardSeasonsService _BlvBoulevardSeasonsService;

        public BlvBoulevardSeasonsController(IBlvBoulevardSeasonsService BlvBoulevardSeasonsService)
        {
            _BlvBoulevardSeasonsService = BlvBoulevardSeasonsService;
        }

        /// <summary>
        /// 查询boulevard_seasons列表
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "blvboulevardseasons:list")]

        public IActionResult QueryBlvBoulevardSeasons([FromQuery] BlvBoulevardSeasonsQueryDto parm)
        {
            var response = _BlvBoulevardSeasonsService.GetList(parm);
            return SUCCESS(response);
        }


        /// <summary>
        /// 查询boulevard_seasons详情
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpGet("{Id}")]
        [ActionPermissionFilter(Permission = "blvboulevardseasons:query")]
        public IActionResult GetBlvBoulevardSeasons(long Id)
        {
            var response = _BlvBoulevardSeasonsService.GetInfo(Id);
            
            var info = response.Adapt<BlvBoulevardSeasonsDto>();
            return SUCCESS(info);
        }

        /// <summary>
        /// 添加boulevard_seasons
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ActionPermissionFilter(Permission = "blvboulevardseasons:add")]
        [Log(Title = "boulevard_seasons", BusinessType = BusinessType.INSERT)]
        public IActionResult AddBlvBoulevardSeasons([FromBody] BlvBoulevardSeasonsDto parm)
        {
            var modal = parm.Adapt<BlvBoulevardSeasons>().ToCreate(HttpContext);

            var response = _BlvBoulevardSeasonsService.AddBlvBoulevardSeasons(modal);

            return SUCCESS(response);
        }

        /// <summary>
        /// 更新boulevard_seasons
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [ActionPermissionFilter(Permission = "blvboulevardseasons:edit")]
        [Log(Title = "boulevard_seasons", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdateBlvBoulevardSeasons([FromBody] BlvBoulevardSeasonsDto parm)
        {
            var modal = parm.Adapt<BlvBoulevardSeasons>().ToUpdate(HttpContext);
            var response = _BlvBoulevardSeasonsService.UpdateBlvBoulevardSeasons(modal);

            return ToResponse(response);
        }
        /// <summary>
        /// 删除boulevard_seasons
        /// </summary>
        /// <returns></returns>
        [HttpPost("delete/{ids}")]
        [ActionPermissionFilter(Permission = "blvboulevardseasons:delete")]
        [Log(Title = "boulevard_seasons", BusinessType = BusinessType.DELETE)]
        public IActionResult DeleteBlvBoulevardSeasons([FromRoute]string ids)
        {
            var idArr = Tools.SplitAndConvert<long>(ids);

            return ToResponse(_BlvBoulevardSeasonsService.Delete(idArr, "删除boulevard_seasons"));
        }

        /// <summary>
        /// 导出boulevard_seasons
        /// </summary>
        /// <returns></returns>
        [Log(Title = "boulevard_seasons", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        [HttpGet("export")]
        [ActionPermissionFilter(Permission = "blvboulevardseasons:export")]
        public IActionResult Export([FromQuery] BlvBoulevardSeasonsQueryDto parm)
        {
            var list = _BlvBoulevardSeasonsService.ExportList(parm).Result;
            if (list == null || list.Count <= 0)
            {
                return ToResponse(ResultCode.FAIL, "没有要导出的数据");
            }
            var result = ExportExcelMini(list, "boulevard_seasons", "boulevard_seasons");
            return ExportExcel(result.Item2, result.Item1);
        }

        /// <summary>
        /// 清空boulevard_seasons
        /// </summary>
        /// <returns></returns>
        [Log(Title = "boulevard_seasons", BusinessType = BusinessType.CLEAN)]
        [ActionPermissionFilter(Permission = "blvboulevardseasons:delete")]
        [HttpPost("clean")]
        public IActionResult Clear()
        {
            if (!HttpContextExtension.IsAdmin(HttpContext))
            {
                return ToResponse(ResultCode.FAIL, "操作失败");
            }
            return SUCCESS(_BlvBoulevardSeasonsService.TruncateBlvBoulevardSeasons());
        }

        /// <summary>
        /// 导入
        /// </summary>
        /// <param name="formFile"></param>
        /// <returns></returns>
        [HttpPost("importData")]
        [Log(Title = "boulevard_seasons导入", BusinessType = BusinessType.IMPORT, IsSaveRequestData = false)]
        [ActionPermissionFilter(Permission = "blvboulevardseasons:import")]

        public IActionResult ImportData([FromForm(Name = "file")] IFormFile formFile)
        {
            List<BlvBoulevardSeasonsDto> list = new();
            using (var stream = formFile.OpenReadStream())
            {
                list = stream.Query<BlvBoulevardSeasonsDto>(startCell: "A1").ToList();
            }

            return SUCCESS(_BlvBoulevardSeasonsService.ImportBlvBoulevardSeasons(list.Adapt<List<BlvBoulevardSeasons>>()));
        }

        /// <summary>
        /// boulevard_seasons导入模板下载
        /// </summary>
        /// <returns></returns>
        [HttpGet("importTemplate")]
        [Log(Title = "boulevard_seasons模板", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        [AllowAnonymous]
        public IActionResult ImportTemplateExcel()
        {
            var result = DownloadImportTemplate(new List<BlvBoulevardSeasonsDto>() { }, "BlvBoulevardSeasons");
            return ExportExcel(result.Item2, result.Item1);
        }

    }
}
