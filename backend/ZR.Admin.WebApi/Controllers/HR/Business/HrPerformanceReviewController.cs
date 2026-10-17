using Microsoft.AspNetCore.Mvc;
using ZR.Model.HR.Business.Dto;
using ZR.Service.HR.Business.IBusinessService;
using MiniExcelLibs;
using ZR.Model.HR.Performance;

//创建时间：2025-08-30
namespace ZR.Admin.WebApi.Controllers.HR.Business
{
    /// <summary>
    /// Performance Review Table
    /// </summary>
    [Route("business/HrPerformanceReview")]
    public class HrPerformanceReviewController : BaseController
    {
        /// <summary>
        /// Performance Review Table接口
        /// </summary>
        private readonly IHrPerformanceReviewService _HrPerformanceReviewService;

        public HrPerformanceReviewController(IHrPerformanceReviewService HrPerformanceReviewService)
        {
            _HrPerformanceReviewService = HrPerformanceReviewService;
        }

        /// <summary>
        /// 查询Performance Review Table列表
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "hrperformancereview:list")]
        public IActionResult QueryHrPerformanceReview([FromQuery] HrPerformanceReviewQueryDto parm)
        {
            var response = _HrPerformanceReviewService.GetList(parm);
            return SUCCESS(response);
        }


        /// <summary>
        /// 查询Performance Review Table详情
        /// </summary>
        /// <param name="Reviewid"></param>
        /// <returns></returns>
        [HttpGet("{Reviewid}")]
        [ActionPermissionFilter(Permission = "hrperformancereview:query")]
        public IActionResult GetHrPerformanceReview(long Reviewid)
        {
            var response = _HrPerformanceReviewService.GetInfo(Reviewid);
            
            var info = response.Adapt<HrPerformanceReviewDto>();
            return SUCCESS(info);
        }

        /// <summary>
        /// 添加Performance Review Table
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ActionPermissionFilter(Permission = "hrperformancereview:add")]
        [Log(Title = "Performance Review Table", BusinessType = BusinessType.INSERT)]
        public IActionResult AddHrPerformanceReview([FromBody] HrPerformanceReviewDto parm)
        {
            var modal = parm.Adapt<HrPerformanceReview>().ToCreate(HttpContext);

            var response = _HrPerformanceReviewService.AddHrPerformanceReview(modal);

            return SUCCESS(response);
        }

        /// <summary>
        /// 更新Performance Review Table
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [ActionPermissionFilter(Permission = "hrperformancereview:edit")]
        [Log(Title = "Performance Review Table", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdateHrPerformanceReview([FromBody] HrPerformanceReviewDto parm)
        {
            var modal = parm.Adapt<HrPerformanceReview>().ToUpdate(HttpContext);
            var response = _HrPerformanceReviewService.UpdateHrPerformanceReview(modal);

            return ToResponse(response);
        }

        /// <summary>
        /// 删除Performance Review Table
        /// </summary>
        /// <returns></returns>
        [HttpPost("delete/{ids}")]
        [ActionPermissionFilter(Permission = "hrperformancereview:delete")]
        [Log(Title = "Performance Review Table", BusinessType = BusinessType.DELETE)]
        public IActionResult DeleteHrPerformanceReview([FromRoute]string ids)
        {
            var idArr = Tools.SplitAndConvert<long>(ids);

            return ToResponse(_HrPerformanceReviewService.Delete(idArr, "删除Performance Review Table"));
        }

        /// <summary>
        /// 导出Performance Review Table
        /// </summary>
        /// <returns></returns>
        [Log(Title = "Performance Review Table", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        [HttpGet("export")]
        [ActionPermissionFilter(Permission = "hrperformancereview:export")]
        public IActionResult Export([FromQuery] HrPerformanceReviewQueryDto parm)
        {
            var list = _HrPerformanceReviewService.ExportList(parm).Result;
            if (list == null || list.Count <= 0)
            {
                return ToResponse(ResultCode.FAIL, "没有要导出的数据");
            }
            var result = ExportExcelMini(list, "Performance Review Table", "Performance Review Table");
            return ExportExcel(result.Item2, result.Item1);
        }

        /// <summary>
        /// 清空Performance Review Table
        /// </summary>
        /// <returns></returns>
        [Log(Title = "Performance Review Table", BusinessType = BusinessType.CLEAN)]
        [ActionPermissionFilter(Permission = "hrperformancereview:delete")]
        [HttpPost("clean")]
        public IActionResult Clear()
        {
            if (!HttpContextExtension.IsAdmin(HttpContext))
            {
                return ToResponse(ResultCode.FAIL, "操作失败");
            }
            return SUCCESS(_HrPerformanceReviewService.TruncateHrPerformanceReview());
        }

        /// <summary>
        /// 导入
        /// </summary>
        /// <param name="formFile"></param>
        /// <returns></returns>
        [HttpPost("importData")]
        [Log(Title = "Performance Review Table导入", BusinessType = BusinessType.IMPORT, IsSaveRequestData = false)]
        [ActionPermissionFilter(Permission = "hrperformancereview:import")]
        public IActionResult ImportData([FromForm(Name = "file")] IFormFile formFile)
        {
            List<HrPerformanceReviewDto> list = new();
            using (var stream = formFile.OpenReadStream())
            {
                list = stream.Query<HrPerformanceReviewDto>(startCell: "A1").ToList();
            }

            return SUCCESS(_HrPerformanceReviewService.ImportHrPerformanceReview(list.Adapt<List<HrPerformanceReview>>()));
        }

        /// <summary>
        /// Performance Review Table导入模板下载
        /// </summary>
        /// <returns></returns>
        [HttpGet("importTemplate")]
        [Log(Title = "Performance Review Table模板", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        [AllowAnonymous]
        public IActionResult ImportTemplateExcel()
        {
            var result = DownloadImportTemplate(new List<HrPerformanceReviewDto>() { }, "HrPerformanceReview");
            return ExportExcel(result.Item2, result.Item1);
        }

    }
}
