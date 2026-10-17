using Microsoft.AspNetCore.Mvc;
using ZR.Model.HR.Business.Dto;
using ZR.Model.HR.Business;
using ZR.Service.HR.Business.IBusinessService;
using MiniExcelLibs;

//创建时间：2025-08-30
namespace ZR.Admin.WebApi.Controllers.HR.Business
{
    /// <summary>
    /// Reviewtype Information Table
    /// </summary>
    [Route("business/HrReviewtype")]
    public class HrReviewtypeController : BaseController
    {
        /// <summary>
        /// Reviewtype Information Table接口
        /// </summary>
        private readonly IHrReviewtypeService _HrReviewtypeService;

        public HrReviewtypeController(IHrReviewtypeService HrReviewtypeService)
        {
            _HrReviewtypeService = HrReviewtypeService;
        }

        /// <summary>
        /// 查询Reviewtype Information Table列表
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "hrreviewtype:list")]
        public IActionResult QueryHrReviewtype([FromQuery] HrReviewtypeQueryDto parm)
        {
            var response = _HrReviewtypeService.GetList(parm);
            return SUCCESS(response);
        }


        /// <summary>
        /// 查询Reviewtype Information Table详情
        /// </summary>
        /// <param name="Reviewtypeid"></param>
        /// <returns></returns>
        [HttpGet("{Reviewtypeid}")]
        [ActionPermissionFilter(Permission = "hrreviewtype:query")]
        public IActionResult GetHrReviewtype(long Reviewtypeid)
        {
            var response = _HrReviewtypeService.GetInfo(Reviewtypeid);
            
            var info = response.Adapt<HrReviewtypeDto>();
            return SUCCESS(info);
        }

        /// <summary>
        /// 添加Reviewtype Information Table
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ActionPermissionFilter(Permission = "hrreviewtype:add")]
        [Log(Title = "Reviewtype Information Table", BusinessType = BusinessType.INSERT)]
        public IActionResult AddHrReviewtype([FromBody] HrReviewtypeDto parm)
        {
            var modal = parm.Adapt<HrReviewtype>().ToCreate(HttpContext);

            var response = _HrReviewtypeService.AddHrReviewtype(modal);

            return SUCCESS(response);
        }

        /// <summary>
        /// 更新Reviewtype Information Table
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [ActionPermissionFilter(Permission = "hrreviewtype:edit")]
        [Log(Title = "Reviewtype Information Table", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdateHrReviewtype([FromBody] HrReviewtypeDto parm)
        {
            var modal = parm.Adapt<HrReviewtype>().ToUpdate(HttpContext);
            var response = _HrReviewtypeService.UpdateHrReviewtype(modal);

            return ToResponse(response);
        }

        /// <summary>
        /// 删除Reviewtype Information Table
        /// </summary>
        /// <returns></returns>
        [HttpPost("delete/{ids}")]
        [ActionPermissionFilter(Permission = "hrreviewtype:delete")]
        [Log(Title = "Reviewtype Information Table", BusinessType = BusinessType.DELETE)]
        public IActionResult DeleteHrReviewtype([FromRoute]string ids)
        {
            var idArr = Tools.SplitAndConvert<long>(ids);

            return ToResponse(_HrReviewtypeService.Delete(idArr, "删除Reviewtype Information Table"));
        }

        /// <summary>
        /// 导出Reviewtype Information Table
        /// </summary>
        /// <returns></returns>
        [Log(Title = "Reviewtype Information Table", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        [HttpGet("export")]
        [ActionPermissionFilter(Permission = "hrreviewtype:export")]
        public IActionResult Export([FromQuery] HrReviewtypeQueryDto parm)
        {
            var list = _HrReviewtypeService.ExportList(parm).Result;
            if (list == null || list.Count <= 0)
            {
                return ToResponse(ResultCode.FAIL, "没有要导出的数据");
            }
            var result = ExportExcelMini(list, "Reviewtype Information Table", "Reviewtype Information Table");
            return ExportExcel(result.Item2, result.Item1);
        }

        /// <summary>
        /// 清空Reviewtype Information Table
        /// </summary>
        /// <returns></returns>
        [Log(Title = "Reviewtype Information Table", BusinessType = BusinessType.CLEAN)]
        [ActionPermissionFilter(Permission = "hrreviewtype:delete")]
        [HttpPost("clean")]
        public IActionResult Clear()
        {
            if (!HttpContextExtension.IsAdmin(HttpContext))
            {
                return ToResponse(ResultCode.FAIL, "操作失败");
            }
            return SUCCESS(_HrReviewtypeService.TruncateHrReviewtype());
        }

        /// <summary>
        /// 导入
        /// </summary>
        /// <param name="formFile"></param>
        /// <returns></returns>
        [HttpPost("importData")]
        [Log(Title = "Reviewtype Information Table导入", BusinessType = BusinessType.IMPORT, IsSaveRequestData = false)]
        [ActionPermissionFilter(Permission = "hrreviewtype:import")]
        public IActionResult ImportData([FromForm(Name = "file")] IFormFile formFile)
        {
            List<HrReviewtypeDto> list = new();
            using (var stream = formFile.OpenReadStream())
            {
                list = stream.Query<HrReviewtypeDto>(startCell: "A1").ToList();
            }

            return SUCCESS(_HrReviewtypeService.ImportHrReviewtype(list.Adapt<List<HrReviewtype>>()));
        }

        /// <summary>
        /// Reviewtype Information Table导入模板下载
        /// </summary>
        /// <returns></returns>
        [HttpGet("importTemplate")]
        [Log(Title = "Reviewtype Information Table模板", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        [AllowAnonymous]
        public IActionResult ImportTemplateExcel()
        {
            var result = DownloadImportTemplate(new List<HrReviewtypeDto>() { }, "HrReviewtype");
            return ExportExcel(result.Item2, result.Item1);
        }

    }
}
