using Microsoft.AspNetCore.Mvc;
using ZR.Model.Ai.Dto;
using ZR.Model.Ai;
using ZR.Service.Ai.IAiService;
using MiniExcelLibs;

//创建时间：2025-09-03
namespace ZR.Admin.WebApi.Controllers.Ai
{
    /// <summary>
    /// Tool Type Table
    /// </summary>
    [Route("Ai/AiTooltype")]
    public class AiTooltypeController : BaseController
    {
        /// <summary>
        /// Tool Type Table接口
        /// </summary>
        private readonly IAiTooltypeService _AiTooltypeService;

        public AiTooltypeController(IAiTooltypeService AiTooltypeService)
        {
            _AiTooltypeService = AiTooltypeService;
        }

        /// <summary>
        /// 查询Tool Type Table列表
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "aitooltype:list")]
        public IActionResult QueryAiTooltype([FromQuery] AiTooltypeQueryDto parm)
        {
            var response = _AiTooltypeService.GetList(parm);
            return SUCCESS(response);
        }


        /// <summary>
        /// 查询Tool Type Table详情
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpGet("{Id}")]
        [ActionPermissionFilter(Permission = "aitooltype:query")]
        public IActionResult GetAiTooltype(long Id)
        {
            var response = _AiTooltypeService.GetInfo(Id);
            
            var info = response.Adapt<AiTooltypeDto>();
            return SUCCESS(info);
        }

        /// <summary>
        /// 添加Tool Type Table
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ActionPermissionFilter(Permission = "aitooltype:add")]
        [Log(Title = "Tool Type Table", BusinessType = BusinessType.INSERT)]
        public IActionResult AddAiTooltype([FromBody] AiTooltypeDto parm)
        {
            var modal = parm.Adapt<AiTooltype>().ToCreate(HttpContext);

            var response = _AiTooltypeService.AddAiTooltype(modal);

            return SUCCESS(response);
        }

        /// <summary>
        /// 更新Tool Type Table
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [ActionPermissionFilter(Permission = "aitooltype:edit")]
        [Log(Title = "Tool Type Table", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdateAiTooltype([FromBody] AiTooltypeDto parm)
        {
            var modal = parm.Adapt<AiTooltype>().ToUpdate(HttpContext);
            var response = _AiTooltypeService.UpdateAiTooltype(modal);

            return ToResponse(response);
        }

        /// <summary>
        /// 删除Tool Type Table
        /// </summary>
        /// <returns></returns>
        [HttpPost("delete/{ids}")]
        [ActionPermissionFilter(Permission = "aitooltype:delete")]
        [Log(Title = "Tool Type Table", BusinessType = BusinessType.DELETE)]
        public IActionResult DeleteAiTooltype([FromRoute]string ids)
        {
            var idArr = Tools.SplitAndConvert<long>(ids);

            return ToResponse(_AiTooltypeService.Delete(idArr, "删除Tool Type Table"));
        }

        /// <summary>
        /// 导出Tool Type Table
        /// </summary>
        /// <returns></returns>
        [Log(Title = "Tool Type Table", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        [HttpGet("export")]
        [ActionPermissionFilter(Permission = "aitooltype:export")]
        public IActionResult Export([FromQuery] AiTooltypeQueryDto parm)
        {
            var list = _AiTooltypeService.ExportList(parm).Result;
            if (list == null || list.Count <= 0)
            {
                return ToResponse(ResultCode.FAIL, "没有要导出的数据");
            }
            var result = ExportExcelMini(list, "Tool Type Table", "Tool Type Table");
            return ExportExcel(result.Item2, result.Item1);
        }

        /// <summary>
        /// 清空Tool Type Table
        /// </summary>
        /// <returns></returns>
        [Log(Title = "Tool Type Table", BusinessType = BusinessType.CLEAN)]
        [ActionPermissionFilter(Permission = "aitooltype:delete")]
        [HttpPost("clean")]
        public IActionResult Clear()
        {
            if (!HttpContextExtension.IsAdmin(HttpContext))
            {
                return ToResponse(ResultCode.FAIL, "操作失败");
            }
            return SUCCESS(_AiTooltypeService.TruncateAiTooltype());
        }

        /// <summary>
        /// 导入
        /// </summary>
        /// <param name="formFile"></param>
        /// <returns></returns>
        [HttpPost("importData")]
        [Log(Title = "Tool Type Table导入", BusinessType = BusinessType.IMPORT, IsSaveRequestData = false)]
        [ActionPermissionFilter(Permission = "aitooltype:import")]
        public IActionResult ImportData([FromForm(Name = "file")] IFormFile formFile)
        {
            List<AiTooltypeDto> list = new();
            using (var stream = formFile.OpenReadStream())
            {
                list = stream.Query<AiTooltypeDto>(startCell: "A1").ToList();
            }

            return SUCCESS(_AiTooltypeService.ImportAiTooltype(list.Adapt<List<AiTooltype>>()));
        }

        /// <summary>
        /// Tool Type Table导入模板下载
        /// </summary>
        /// <returns></returns>
        [HttpGet("importTemplate")]
        [Log(Title = "Tool Type Table模板", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        [AllowAnonymous]
        public IActionResult ImportTemplateExcel()
        {
            var result = DownloadImportTemplate(new List<AiTooltypeDto>() { }, "AiTooltype");
            return ExportExcel(result.Item2, result.Item1);
        }

    }
}
