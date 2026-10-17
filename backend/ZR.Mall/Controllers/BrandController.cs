using Microsoft.AspNetCore.Mvc;
using ZR.Common;
using ZR.Mall.Model;
using ZR.Mall.Model.Dto;
using ZR.Mall.Service.IService;

//创建时间：2025-05-29
namespace ZR.Mall.Controllers
{
    /// <summary>
    /// Brand management
    /// </summary>
    [Route("shopping/brand")]
    [ApiExplorerSettings(GroupName = "shopping")]
    public class BrandController : BaseController
    {
        /// <summary>
        /// Brand table interface
        /// </summary>
        private readonly IBrandService _ShopBrandService;

        public BrandController(IBrandService ShopBrandService)
        {
            _ShopBrandService = ShopBrandService;
        }

        /// <summary>
        /// Query brand table list
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "shop:brand:list")]
        public IActionResult QueryShopBrand([FromQuery] ShopBrandQueryDto parm)
        {
            var response = _ShopBrandService.GetList(parm);
            return SUCCESS(response);
        }

        /// <summary>
        /// Query brand table details
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpGet("{Id}")]
        [ActionPermissionFilter(Permission = "shop:brand:query")]
        public IActionResult GetShopBrand(long Id)
        {
            var response = _ShopBrandService.GetInfo(Id);
            
            var info = response.Adapt<BrandDto>();
            return SUCCESS(info);
        }

        /// <summary>
        /// Add brand table
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ActionPermissionFilter(Permission = "shop:brand:add")]
        [Log(Title = "Brand table", BusinessType = BusinessType.INSERT)]
        public IActionResult AddShopBrand([FromBody] BrandDto parm)
        {
            var modal = parm.Adapt<Brand>().ToCreate(HttpContext);

            var response = _ShopBrandService.AddShopBrand(modal);

            return SUCCESS(response);
        }

        /// <summary>
        /// Update brand table
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [ActionPermissionFilter(Permission = "shop:brand:edit")]
        [Log(Title = "Brand table", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdateShopBrand([FromBody] BrandDto parm)
        {
            var modal = parm.Adapt<Brand>().ToUpdate(HttpContext);
            var response = _ShopBrandService.UpdateShopBrand(modal);

            return ToResponse(response);
        }

        /// <summary>
        /// Delete brand table
        /// </summary>
        /// <returns></returns>
        [HttpPost("delete/{ids}")]
        [ActionPermissionFilter(Permission = "shop:brand:delete")]
        [Log(Title = "Brand table", BusinessType = BusinessType.DELETE)]
        public IActionResult DeleteShopBrand([FromRoute]string ids)
        {
            var idArr = Tools.SplitAndConvert<long>(ids);

            return ToResponse(_ShopBrandService.Delete(idArr, "Delete brand table"));
        }

        /// <summary>
        /// Export brand table
        /// </summary>
        /// <returns></returns>
        [Log(Title = "Brand table", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        [HttpGet("export")]
        [ActionPermissionFilter(Permission = "shop:brand:export")]
        public IActionResult Export([FromQuery] ShopBrandQueryDto parm)
        {
            parm.PageNum = 1;
            parm.PageSize = 100000;
            var list = _ShopBrandService.ExportList(parm).Result;
            if (list == null || list.Count <= 0)
            {
                return ToResponse(ResultCode.FAIL, "No data to export");
            }
            var result = ExportExcelMini(list, "Brand table", "Brand table");
            return ExportExcel(result.Item2, result.Item1);
        }
    }
}
