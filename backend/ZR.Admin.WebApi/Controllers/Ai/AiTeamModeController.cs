using Microsoft.AspNetCore.Mvc;
using ZR.Model.AI.Dto;
using ZR.Model.AI;
using ZR.Service.AI.IAIService;
using MiniExcelLibs;

//创建时间：2025-09-03
namespace ZR.Admin.WebApi.Controllers.AI
{
    /// <summary>
    /// Team Mode Table
    /// </summary>
    [Route("AI/AiTeamMode")]
    public class AiTeamModeController : BaseController
    {
        /// <summary>
        /// Team Mode Table接口
        /// </summary>
        private readonly IAiTeamModeService _AiTeamModeService;

        public AiTeamModeController(IAiTeamModeService AiTeamModeService)
        {
            _AiTeamModeService = AiTeamModeService;
        }

        /// <summary>
        /// 查询Team Mode Table列表
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "aiteammode:list")]
        public IActionResult QueryAiTeamMode([FromQuery] AiTeamModeQueryDto parm)
        {
            var response = _AiTeamModeService.GetList(parm);
            return SUCCESS(response);
        }


        /// <summary>
        /// 查询Team Mode Table详情
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpGet("{Id}")]
        [ActionPermissionFilter(Permission = "aiteammode:query")]
        public IActionResult GetAiTeamMode(long Id)
        {
            var response = _AiTeamModeService.GetInfo(Id);
            
            var info = response.Adapt<AiTeamModeDto>();
            return SUCCESS(info);
        }

        /// <summary>
        /// 添加Team Mode Table
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ActionPermissionFilter(Permission = "aiteammode:add")]
        [Log(Title = "Team Mode Table", BusinessType = BusinessType.INSERT)]
        public IActionResult AddAiTeamMode([FromBody] AiTeamModeDto parm)
        {
            var modal = parm.Adapt<AiTeamMode>().ToCreate(HttpContext);

            var response = _AiTeamModeService.AddAiTeamMode(modal);

            return SUCCESS(response);
        }

        /// <summary>
        /// 更新Team Mode Table
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [ActionPermissionFilter(Permission = "aiteammode:edit")]
        [Log(Title = "Team Mode Table", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdateAiTeamMode([FromBody] AiTeamModeDto parm)
        {
            var modal = parm.Adapt<AiTeamMode>().ToUpdate(HttpContext);
            var response = _AiTeamModeService.UpdateAiTeamMode(modal);

            return ToResponse(response);
        }

        /// <summary>
        /// 删除Team Mode Table
        /// </summary>
        /// <returns></returns>
        [HttpPost("delete/{ids}")]
        [ActionPermissionFilter(Permission = "aiteammode:delete")]
        [Log(Title = "Team Mode Table", BusinessType = BusinessType.DELETE)]
        public IActionResult DeleteAiTeamMode([FromRoute]string ids)
        {
            var idArr = Tools.SplitAndConvert<long>(ids);

            return ToResponse(_AiTeamModeService.Delete(idArr, "删除Team Mode Table"));
        }

        /// <summary>
        /// 导出Team Mode Table
        /// </summary>
        /// <returns></returns>
        [Log(Title = "Team Mode Table", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        [HttpGet("export")]
        [ActionPermissionFilter(Permission = "aiteammode:export")]
        public IActionResult Export([FromQuery] AiTeamModeQueryDto parm)
        {
            var list = _AiTeamModeService.ExportList(parm).Result;
            if (list == null || list.Count <= 0)
            {
                return ToResponse(ResultCode.FAIL, "没有要导出的数据");
            }
            var result = ExportExcelMini(list, "Team Mode Table", "Team Mode Table");
            return ExportExcel(result.Item2, result.Item1);
        }

        /// <summary>
        /// 清空Team Mode Table
        /// </summary>
        /// <returns></returns>
        [Log(Title = "Team Mode Table", BusinessType = BusinessType.CLEAN)]
        [ActionPermissionFilter(Permission = "aiteammode:delete")]
        [HttpPost("clean")]
        public IActionResult Clear()
        {
            if (!HttpContextExtension.IsAdmin(HttpContext))
            {
                return ToResponse(ResultCode.FAIL, "操作失败");
            }
            return SUCCESS(_AiTeamModeService.TruncateAiTeamMode());
        }

        /// <summary>
        /// 导入
        /// </summary>
        /// <param name="formFile"></param>
        /// <returns></returns>
        [HttpPost("importData")]
        [Log(Title = "Team Mode Table导入", BusinessType = BusinessType.IMPORT, IsSaveRequestData = false)]
        [ActionPermissionFilter(Permission = "aiteammode:import")]
        public IActionResult ImportData([FromForm(Name = "file")] IFormFile formFile)
        {
            List<AiTeamModeDto> list = new();
            using (var stream = formFile.OpenReadStream())
            {
                list = stream.Query<AiTeamModeDto>(startCell: "A1").ToList();
            }

            return SUCCESS(_AiTeamModeService.ImportAiTeamMode(list.Adapt<List<AiTeamMode>>()));
        }

        /// <summary>
        /// Team Mode Table导入模板下载
        /// </summary>
        /// <returns></returns>
        [HttpGet("importTemplate")]
        [Log(Title = "Team Mode Table模板", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        [AllowAnonymous]
        public IActionResult ImportTemplateExcel()
        {
            var result = DownloadImportTemplate(new List<AiTeamModeDto>() { }, "AiTeamMode");
            return ExportExcel(result.Item2, result.Item1);
        }

    }
}
