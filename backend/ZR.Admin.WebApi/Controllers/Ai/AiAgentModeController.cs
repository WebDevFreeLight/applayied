using Microsoft.AspNetCore.Mvc;
using ZR.Model.Ai.Dto;
using ZR.Model.Ai;
using ZR.Service.Ai.IAiService;
using MiniExcelLibs;

//创建时间：2025-09-03
namespace ZR.Admin.WebApi.Controllers.Ai
{
    /// <summary>
    /// Agent Mode Table
    /// </summary>
    [Route("Ai/AiAgentMode")]
    public class AiAgentModeController : BaseController
    {
        /// <summary>
        /// Agent Mode Table接口
        /// </summary>
        private readonly IAiAgentModeService _AiAgentModeService;

        public AiAgentModeController(IAiAgentModeService AiAgentModeService)
        {
            _AiAgentModeService = AiAgentModeService;
        }

        /// <summary>
        /// 查询Agent Mode Table列表
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "aiagentmode:list")]
        public IActionResult QueryAiAgentMode([FromQuery] AiAgentModeQueryDto parm)
        {
            var response = _AiAgentModeService.GetList(parm);
            return SUCCESS(response);
        }


        /// <summary>
        /// 查询Agent Mode Table详情
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpGet("{Id}")]
        [ActionPermissionFilter(Permission = "aiagentmode:query")]
        public IActionResult GetAiAgentMode(long Id)
        {
            var response = _AiAgentModeService.GetInfo(Id);
            
            var info = response.Adapt<AiAgentModeDto>();
            return SUCCESS(info);
        }

        /// <summary>
        /// 添加Agent Mode Table
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ActionPermissionFilter(Permission = "aiagentmode:add")]
        [Log(Title = "Agent Mode Table", BusinessType = BusinessType.INSERT)]
        public IActionResult AddAiAgentMode([FromBody] AiAgentModeDto parm)
        {
            var modal = parm.Adapt<AiAgentMode>().ToCreate(HttpContext);

            var response = _AiAgentModeService.AddAiAgentMode(modal);

            return SUCCESS(response);
        }

        /// <summary>
        /// 更新Agent Mode Table
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [ActionPermissionFilter(Permission = "aiagentmode:edit")]
        [Log(Title = "Agent Mode Table", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdateAiAgentMode([FromBody] AiAgentModeDto parm)
        {
            var modal = parm.Adapt<AiAgentMode>().ToUpdate(HttpContext);
            var response = _AiAgentModeService.UpdateAiAgentMode(modal);

            return ToResponse(response);
        }

        /// <summary>
        /// 删除Agent Mode Table
        /// </summary>
        /// <returns></returns>
        [HttpPost("delete/{ids}")]
        [ActionPermissionFilter(Permission = "aiagentmode:delete")]
        [Log(Title = "Agent Mode Table", BusinessType = BusinessType.DELETE)]
        public IActionResult DeleteAiAgentMode([FromRoute]string ids)
        {
            var idArr = Tools.SplitAndConvert<long>(ids);

            return ToResponse(_AiAgentModeService.Delete(idArr, "删除Agent Mode Table"));
        }

        /// <summary>
        /// 导出Agent Mode Table
        /// </summary>
        /// <returns></returns>
        [Log(Title = "Agent Mode Table", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        [HttpGet("export")]
        [ActionPermissionFilter(Permission = "aiagentmode:export")]
        public IActionResult Export([FromQuery] AiAgentModeQueryDto parm)
        {
            var list = _AiAgentModeService.ExportList(parm).Result;
            if (list == null || list.Count <= 0)
            {
                return ToResponse(ResultCode.FAIL, "没有要导出的数据");
            }
            var result = ExportExcelMini(list, "Agent Mode Table", "Agent Mode Table");
            return ExportExcel(result.Item2, result.Item1);
        }

        /// <summary>
        /// 清空Agent Mode Table
        /// </summary>
        /// <returns></returns>
        [Log(Title = "Agent Mode Table", BusinessType = BusinessType.CLEAN)]
        [ActionPermissionFilter(Permission = "aiagentmode:delete")]
        [HttpPost("clean")]
        public IActionResult Clear()
        {
            if (!HttpContextExtension.IsAdmin(HttpContext))
            {
                return ToResponse(ResultCode.FAIL, "操作失败");
            }
            return SUCCESS(_AiAgentModeService.TruncateAiAgentMode());
        }

        /// <summary>
        /// 导入
        /// </summary>
        /// <param name="formFile"></param>
        /// <returns></returns>
        [HttpPost("importData")]
        [Log(Title = "Agent Mode Table导入", BusinessType = BusinessType.IMPORT, IsSaveRequestData = false)]
        [ActionPermissionFilter(Permission = "aiagentmode:import")]
        public IActionResult ImportData([FromForm(Name = "file")] IFormFile formFile)
        {
            List<AiAgentModeDto> list = new();
            using (var stream = formFile.OpenReadStream())
            {
                list = stream.Query<AiAgentModeDto>(startCell: "A1").ToList();
            }

            return SUCCESS(_AiAgentModeService.ImportAiAgentMode(list.Adapt<List<AiAgentMode>>()));
        }

        /// <summary>
        /// Agent Mode Table导入模板下载
        /// </summary>
        /// <returns></returns>
        [HttpGet("importTemplate")]
        [Log(Title = "Agent Mode Table模板", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        [AllowAnonymous]
        public IActionResult ImportTemplateExcel()
        {
            var result = DownloadImportTemplate(new List<AiAgentModeDto>() { }, "AiAgentMode");
            return ExportExcel(result.Item2, result.Item1);
        }

    }
}
