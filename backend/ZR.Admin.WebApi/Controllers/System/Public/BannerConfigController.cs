using Microsoft.AspNetCore.Mvc;
using ZR.Model.System.Public;
using ZR.Model.System.Public.Dto;
using ZR.Service.Public.IPublicService;

//Created: 2024-05-11
namespace ZR.Admin.WebApi.Controllers.System.Public
{
    /// <summary>
    /// Banner Management
    /// </summary>
    [Route("public/BannerConfig")]
    public class BannerConfigController : BaseController
    {
        /// <summary>
        /// Banner Management Interface
        /// </summary>
        private readonly IBannerConfigService _BannerConfigService;

        public BannerConfigController(IBannerConfigService BannerConfigService)
        {
            _BannerConfigService = BannerConfigService;
        }

        /// <summary>
        /// Query banner management list
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "bannerconfig:list")]
        public IActionResult QueryBannerConfig([FromQuery] BannerConfigQueryDto parm)
        {
            var response = _BannerConfigService.GetList(parm);
            return SUCCESS(response);
        }

        /// <summary>
        /// Query banner management details
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpGet("{Id}")]
        [ActionPermissionFilter(Permission = "bannerconfig:query")]
        public IActionResult GetBannerConfig(int Id)
        {
            var response = _BannerConfigService.GetInfo(Id);

            var info = response.Adapt<BannerConfigDto>();
            return SUCCESS(info);
        }

        /// <summary>
        /// Add banner management
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ActionPermissionFilter(Permission = "bannerconfig:add")]
        [Log(Title = "Banner Management", BusinessType = BusinessType.INSERT)]
        public IActionResult AddBannerConfig([FromBody] BannerConfigDto parm)
        {
            var modal = parm.Adapt<BannerConfig>().ToCreate(HttpContext);

            var response = _BannerConfigService.AddBannerConfig(modal);

            return SUCCESS(response);
        }

        /// <summary>
        /// Update banner management
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [ActionPermissionFilter(Permission = "bannerconfig:edit")]
        [Log(Title = "Banner Management", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdateBannerConfig([FromBody] BannerConfigDto parm)
        {
            var modal = parm.Adapt<BannerConfig>();
            var response = _BannerConfigService.UpdateBannerConfig(modal);

            return ToResponse(response);
        }

        /// <summary>
        /// Delete banner management
        /// </summary>
        /// <returns></returns>
        [HttpDelete("delete/{ids}")]
        [ActionPermissionFilter(Permission = "bannerconfig:delete")]
        [Log(Title = "Banner Management", BusinessType = BusinessType.DELETE)]
        public IActionResult DeleteBannerConfig([FromRoute] string ids)
        {
            var idArr = Tools.SplitAndConvert<int>(ids);

            return ToResponse(_BannerConfigService.Delete(idArr, "Delete Banner Management"));
        }

        /// <summary>
        /// Export banner management
        /// </summary>
        /// <returns></returns>
        [Log(Title = "Banner Management", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        [HttpGet("export")]
        [ActionPermissionFilter(Permission = "bannerconfig:export")]
        public IActionResult Export([FromQuery] BannerConfigQueryDto parm)
        {
            parm.PageNum = 1;
            parm.PageSize = 100000;
            var list = _BannerConfigService.ExportList(parm).Result;
            if (list == null || list.Count <= 0)
            {
                return ToResponse(ResultCode.FAIL, "No data to export");
            }
            var result = ExportExcelMini(list, "Banner Management", "Banner Management");
            return ExportExcel(result.Item2, result.Item1);
        }

        /// <summary>
        /// Save sort order
        /// </summary>
        /// <param name="id">Primary key</param>
        /// <param name="value">Sort value</param>
        /// <returns></returns>
        [ActionPermissionFilter(Permission = "bannerconfig:edit")]
        [HttpGet("ChangeSort")]
        [Log(Title = "Save Sort", BusinessType = BusinessType.UPDATE)]
        public IActionResult ChangeSort(int id = 0, int value = 0)
        {
            if (id <= 0) { return ToResponse(ApiResult.Error(101, "Request parameters are invalid")); }
            var response = _BannerConfigService.Update(w => w.Id == id, it => new BannerConfig()
            {
                SortId = value,
            });

            return ToResponse(response);
        }

        /// <summary>
        /// Query banner management list
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        [HttpGet("bannerList")]
        [AllowAnonymous]
        public IActionResult QueryBannerList([FromQuery] BannerConfigQueryDto parm)
        {
            var response = _BannerConfigService.GetBannerList(parm);
            return SUCCESS(new { list = response });
        }
    }
}
