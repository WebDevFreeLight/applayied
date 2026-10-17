using Microsoft.AspNetCore.Mvc;
using ZR.Model.Boulevard.Dto;
using ZR.Model.Boulevard;
using ZR.Service.Boulevard.IBoulevardService;
using MiniExcelLibs;

//创建时间：2025-09-16
namespace ZR.Admin.WebApi.Controllers.Boulevard
{
    /// <summary>
    /// shop_category
    /// </summary>
    [Route("boulevard/BlvBoulevardShopCategory")]
    [ApiExplorerSettings(GroupName = "boulevard")]

    public class BlvBoulevardShopCategoryController : BaseController
    {
        /// <summary>
        /// shop_category接口
        /// </summary>
        private readonly IBlvBoulevardShopCategoryService _BlvBoulevardShopCategoryService;

        public BlvBoulevardShopCategoryController(IBlvBoulevardShopCategoryService BlvBoulevardShopCategoryService)
        {
            _BlvBoulevardShopCategoryService = BlvBoulevardShopCategoryService;
        }

        /// <summary>
        /// 查询shop_category列表
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "blvboulevardshopcategory:list")]
        public IActionResult QueryBlvBoulevardShopCategory([FromQuery] BlvBoulevardShopCategoryQueryDto parm)
        {
            var response = _BlvBoulevardShopCategoryService.GetList(parm);
            return SUCCESS(response);
        }


        /// <summary>
        /// 查询shop_category详情
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpGet("{Id}")]
        [ActionPermissionFilter(Permission = "blvboulevardshopcategory:query")]
        public IActionResult GetBlvBoulevardShopCategory(long Id)
        {
            var response = _BlvBoulevardShopCategoryService.GetInfo(Id);
            
            var info = response.Adapt<BlvBoulevardShopCategoryDto>();
            return SUCCESS(info);
        }

        /// <summary>
        /// 添加shop_category
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ActionPermissionFilter(Permission = "blvboulevardshopcategory:add")]
        [Log(Title = "shop_category", BusinessType = BusinessType.INSERT)]
        public IActionResult AddBlvBoulevardShopCategory([FromBody] BlvBoulevardShopCategoryDto parm)
        {
            var modal = parm.Adapt<BlvBoulevardShopCategory>().ToCreate(HttpContext);

            var response = _BlvBoulevardShopCategoryService.AddBlvBoulevardShopCategory(modal);

            return SUCCESS(response);
        }

        /// <summary>
        /// 更新shop_category
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [ActionPermissionFilter(Permission = "blvboulevardshopcategory:edit")]
        [Log(Title = "shop_category", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdateBlvBoulevardShopCategory([FromBody] BlvBoulevardShopCategoryDto parm)
        {
            var modal = parm.Adapt<BlvBoulevardShopCategory>().ToUpdate(HttpContext);
            var response = _BlvBoulevardShopCategoryService.UpdateBlvBoulevardShopCategory(modal);

            return ToResponse(response);
        }

        /// <summary>
        /// 删除shop_category
        /// </summary>
        /// <returns></returns>
        [HttpPost("delete/{ids}")]
        [ActionPermissionFilter(Permission = "blvboulevardshopcategory:delete")]
        [Log(Title = "shop_category", BusinessType = BusinessType.DELETE)]
        public IActionResult DeleteBlvBoulevardShopCategory([FromRoute]string ids)
        {
            var idArr = Tools.SplitAndConvert<long>(ids);

            return ToResponse(_BlvBoulevardShopCategoryService.Delete(idArr, "删除shop_category"));
        }

        /// <summary>
        /// 导出shop_category
        /// </summary>
        /// <returns></returns>
        [Log(Title = "shop_category", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        [HttpGet("export")]
        [ActionPermissionFilter(Permission = "blvboulevardshopcategory:export")]
        public IActionResult Export([FromQuery] BlvBoulevardShopCategoryQueryDto parm)
        {
            var list = _BlvBoulevardShopCategoryService.ExportList(parm).Result;
            if (list == null || list.Count <= 0)
            {
                return ToResponse(ResultCode.FAIL, "没有要导出的数据");
            }
            var result = ExportExcelMini(list, "shop_category", "shop_category");
            return ExportExcel(result.Item2, result.Item1);
        }

        /// <summary>
        /// 清空shop_category
        /// </summary>
        /// <returns></returns>
        [Log(Title = "shop_category", BusinessType = BusinessType.CLEAN)]
        [ActionPermissionFilter(Permission = "blvboulevardshopcategory:delete")]
        [HttpPost("clean")]
        public IActionResult Clear()
        {
            if (!HttpContextExtension.IsAdmin(HttpContext))
            {
                return ToResponse(ResultCode.FAIL, "操作失败");
            }
            return SUCCESS(_BlvBoulevardShopCategoryService.TruncateBlvBoulevardShopCategory());
        }

        /// <summary>
        /// 导入
        /// </summary>
        /// <param name="formFile"></param>
        /// <returns></returns>
        [HttpPost("importData")]
        [Log(Title = "shop_category导入", BusinessType = BusinessType.IMPORT, IsSaveRequestData = false)]
        [ActionPermissionFilter(Permission = "blvboulevardshopcategory:import")]
        public IActionResult ImportData([FromForm(Name = "file")] IFormFile formFile)
        {
            List<BlvBoulevardShopCategoryDto> list = new();
            using (var stream = formFile.OpenReadStream())
            {
                list = stream.Query<BlvBoulevardShopCategoryDto>(startCell: "A1").ToList();
            }

            return SUCCESS(_BlvBoulevardShopCategoryService.ImportBlvBoulevardShopCategory(list.Adapt<List<BlvBoulevardShopCategory>>()));
        }

        /// <summary>
        /// shop_category导入模板下载
        /// </summary>
        /// <returns></returns>
        [HttpGet("importTemplate")]
        [Log(Title = "shop_category模板", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        [AllowAnonymous]
        public IActionResult ImportTemplateExcel()
        {
            var result = DownloadImportTemplate(new List<BlvBoulevardShopCategoryDto>() { }, "BlvBoulevardShopCategory");
            return ExportExcel(result.Item2, result.Item1);
        }

    }
}
