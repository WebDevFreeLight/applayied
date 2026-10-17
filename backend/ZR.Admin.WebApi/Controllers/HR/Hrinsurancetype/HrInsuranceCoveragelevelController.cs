using Microsoft.AspNetCore.Mvc;
using ZR.Model.HR.Hrinsurancetype.Dto;
using ZR.Model.HR.Hrinsurancetype;
using ZR.Service.HR.Hrinsurancetype.IHrinsurancetypeService;
using MiniExcelLibs;

//创建时间：2025-09-02
namespace ZR.Admin.WebApi.Controllers.HR.Hrinsurancetype
{
    /// <summary>
    /// insurancetype
    /// </summary>
    [Route("hrinsurancetype/HrInsuranceCoveragelevel")]
    public class HrInsuranceCoveragelevelController : BaseController
    {
        /// <summary>
        /// insurancetype接口
        /// </summary>
        private readonly IHrInsuranceCoveragelevelService _HrInsuranceCoveragelevelService;

        public HrInsuranceCoveragelevelController(IHrInsuranceCoveragelevelService HrInsuranceCoveragelevelService)
        {
            _HrInsuranceCoveragelevelService = HrInsuranceCoveragelevelService;
        }

        /// <summary>
        /// 查询insurancetype列表
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "hrinsurancecoveragelevel:list")]
        public IActionResult QueryHrInsuranceCoveragelevel([FromQuery] HrInsuranceCoveragelevelQueryDto parm)
        {
            var response = _HrInsuranceCoveragelevelService.GetList(parm);
            return SUCCESS(response);
        }


        /// <summary>
        /// 查询insurancetype详情
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpGet("{Id}")]
        [ActionPermissionFilter(Permission = "hrinsurancecoveragelevel:query")]
        public IActionResult GetHrInsuranceCoveragelevel(long Id)
        {
            var response = _HrInsuranceCoveragelevelService.GetInfo(Id);
            
            var info = response.Adapt<HrInsuranceCoveragelevelDto>();
            return SUCCESS(info);
        }

        /// <summary>
        /// 添加insurancetype
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ActionPermissionFilter(Permission = "hrinsurancecoveragelevel:add")]
        [Log(Title = "insurancetype", BusinessType = BusinessType.INSERT)]
        public IActionResult AddHrInsuranceCoveragelevel([FromBody] HrInsuranceCoveragelevelDto parm)
        {
            var modal = parm.Adapt<HrInsuranceCoveragelevel>().ToCreate(HttpContext);

            var response = _HrInsuranceCoveragelevelService.AddHrInsuranceCoveragelevel(modal);

            return SUCCESS(response);
        }

        /// <summary>
        /// 更新insurancetype
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [ActionPermissionFilter(Permission = "hrinsurancecoveragelevel:edit")]
        [Log(Title = "insurancetype", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdateHrInsuranceCoveragelevel([FromBody] HrInsuranceCoveragelevelDto parm)
        {
            var modal = parm.Adapt<HrInsuranceCoveragelevel>().ToUpdate(HttpContext);
            var response = _HrInsuranceCoveragelevelService.UpdateHrInsuranceCoveragelevel(modal);

            return ToResponse(response);
        }

        /// <summary>
        /// 删除insurancetype
        /// </summary>
        /// <returns></returns>
        [HttpPost("delete/{ids}")]
        [ActionPermissionFilter(Permission = "hrinsurancecoveragelevel:delete")]
        [Log(Title = "insurancetype", BusinessType = BusinessType.DELETE)]
        public IActionResult DeleteHrInsuranceCoveragelevel([FromRoute]string ids)
        {
            var idArr = Tools.SplitAndConvert<long>(ids);

            return ToResponse(_HrInsuranceCoveragelevelService.Delete(idArr, "删除insurancetype"));
        }

        /// <summary>
        /// 导出insurancetype
        /// </summary>
        /// <returns></returns>
        [Log(Title = "insurancetype", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        [HttpGet("export")]
        [ActionPermissionFilter(Permission = "hrinsurancecoveragelevel:export")]
        public IActionResult Export([FromQuery] HrInsuranceCoveragelevelQueryDto parm)
        {
            var list = _HrInsuranceCoveragelevelService.ExportList(parm).Result;
            if (list == null || list.Count <= 0)
            {
                return ToResponse(ResultCode.FAIL, "没有要导出的数据");
            }
            var result = ExportExcelMini(list, "insurancetype", "insurancetype");
            return ExportExcel(result.Item2, result.Item1);
        }

        /// <summary>
        /// 清空insurancetype
        /// </summary>
        /// <returns></returns>
        [Log(Title = "insurancetype", BusinessType = BusinessType.CLEAN)]
        [ActionPermissionFilter(Permission = "hrinsurancecoveragelevel:delete")]
        [HttpPost("clean")]
        public IActionResult Clear()
        {
            if (!HttpContextExtension.IsAdmin(HttpContext))
            {
                return ToResponse(ResultCode.FAIL, "操作失败");
            }
            return SUCCESS(_HrInsuranceCoveragelevelService.TruncateHrInsuranceCoveragelevel());
        }

        /// <summary>
        /// 导入
        /// </summary>
        /// <param name="formFile"></param>
        /// <returns></returns>
        [HttpPost("importData")]
        [Log(Title = "insurancetype导入", BusinessType = BusinessType.IMPORT, IsSaveRequestData = false)]
        [ActionPermissionFilter(Permission = "hrinsurancecoveragelevel:import")]
        public IActionResult ImportData([FromForm(Name = "file")] IFormFile formFile)
        {
            List<HrInsuranceCoveragelevelDto> list = new();
            using (var stream = formFile.OpenReadStream())
            {
                list = stream.Query<HrInsuranceCoveragelevelDto>(startCell: "A1").ToList();
            }

            return SUCCESS(_HrInsuranceCoveragelevelService.ImportHrInsuranceCoveragelevel(list.Adapt<List<HrInsuranceCoveragelevel>>()));
        }

        /// <summary>
        /// insurancetype导入模板下载
        /// </summary>
        /// <returns></returns>
        [HttpGet("importTemplate")]
        [Log(Title = "insurancetype模板", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        [AllowAnonymous]
        public IActionResult ImportTemplateExcel()
        {
            var result = DownloadImportTemplate(new List<HrInsuranceCoveragelevelDto>() { }, "HrInsuranceCoveragelevel");
            return ExportExcel(result.Item2, result.Item1);
        }

    }
}
