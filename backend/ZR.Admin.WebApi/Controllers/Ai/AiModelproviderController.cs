using Microsoft.AspNetCore.Mvc;
using ZR.Model.Ai.Dto;
using ZR.Model.Ai;
using ZR.Service.Ai.IAiService;
using MiniExcelLibs;

//创建时间：2025-09-03
namespace ZR.Admin.WebApi.Controllers.Ai
{
    /// <summary>
    /// Model Provider Table
    /// </summary>
    [Route("Ai/AiModelprovider")]
    public class AiModelproviderController : BaseController
    {
        /// <summary>
        /// Model Provider Table接口
        /// </summary>
        private readonly IAiModelproviderService _AiModelproviderService;

        public AiModelproviderController(IAiModelproviderService AiModelproviderService)
        {
            _AiModelproviderService = AiModelproviderService;
        }

        /// <summary>
        /// 查询Model Provider Table列表
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "aimodelprovider:list")]
        public IActionResult QueryAiModelprovider([FromQuery] AiModelproviderQueryDto parm)
        {
            var response = _AiModelproviderService.GetList(parm);
            return SUCCESS(response);
        }


        /// <summary>
        /// 查询Model Provider Table详情
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpGet("{Id}")]
        [ActionPermissionFilter(Permission = "aimodelprovider:query")]
        public IActionResult GetAiModelprovider(long Id)
        {
            var response = _AiModelproviderService.GetInfo(Id);
            
            var info = response.Adapt<AiModelproviderDto>();
            return SUCCESS(info);
        }

        /// <summary>
        /// 添加Model Provider Table
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ActionPermissionFilter(Permission = "aimodelprovider:add")]
        [Log(Title = "Model Provider Table", BusinessType = BusinessType.INSERT)]
        public IActionResult AddAiModelprovider([FromBody] AiModelproviderDto parm)
        {
            var modal = parm.Adapt<AiModelprovider>().ToCreate(HttpContext);

            var response = _AiModelproviderService.AddAiModelprovider(modal);

            return SUCCESS(response);
        }

        /// <summary>
        /// 更新Model Provider Table
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [ActionPermissionFilter(Permission = "aimodelprovider:edit")]
        [Log(Title = "Model Provider Table", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdateAiModelprovider([FromBody] AiModelproviderDto parm)
        {
            var modal = parm.Adapt<AiModelprovider>().ToUpdate(HttpContext);
            var response = _AiModelproviderService.UpdateAiModelprovider(modal);

            return ToResponse(response);
        }

        /// <summary>
        /// 删除Model Provider Table
        /// </summary>
        /// <returns></returns>
        [HttpPost("delete/{ids}")]
        [ActionPermissionFilter(Permission = "aimodelprovider:delete")]
        [Log(Title = "Model Provider Table", BusinessType = BusinessType.DELETE)]
        public IActionResult DeleteAiModelprovider([FromRoute]string ids)
        {
            var idArr = Tools.SplitAndConvert<long>(ids);

            return ToResponse(_AiModelproviderService.Delete(idArr, "删除Model Provider Table"));
        }

        /// <summary>
        /// 导出Model Provider Table
        /// </summary>
        /// <returns></returns>
        [Log(Title = "Model Provider Table", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        [HttpGet("export")]
        [ActionPermissionFilter(Permission = "aimodelprovider:export")]
        public IActionResult Export([FromQuery] AiModelproviderQueryDto parm)
        {
            var list = _AiModelproviderService.ExportList(parm).Result;
            if (list == null || list.Count <= 0)
            {
                return ToResponse(ResultCode.FAIL, "没有要导出的数据");
            }
            var result = ExportExcelMini(list, "Model Provider Table", "Model Provider Table");
            return ExportExcel(result.Item2, result.Item1);
        }

        /// <summary>
        /// 清空Model Provider Table
        /// </summary>
        /// <returns></returns>
        [Log(Title = "Model Provider Table", BusinessType = BusinessType.CLEAN)]
        [ActionPermissionFilter(Permission = "aimodelprovider:delete")]
        [HttpPost("clean")]
        public IActionResult Clear()
        {
            if (!HttpContextExtension.IsAdmin(HttpContext))
            {
                return ToResponse(ResultCode.FAIL, "操作失败");
            }
            return SUCCESS(_AiModelproviderService.TruncateAiModelprovider());
        }

        /// <summary>
        /// 导入
        /// </summary>
        /// <param name="formFile"></param>
        /// <returns></returns>
        [HttpPost("importData")]
        [Log(Title = "Model Provider Table导入", BusinessType = BusinessType.IMPORT, IsSaveRequestData = false)]
        [ActionPermissionFilter(Permission = "aimodelprovider:import")]
        public IActionResult ImportData([FromForm(Name = "file")] IFormFile formFile)
        {
            List<AiModelproviderDto> list = new();
            using (var stream = formFile.OpenReadStream())
            {
                list = stream.Query<AiModelproviderDto>(startCell: "A1").ToList();
            }

            return SUCCESS(_AiModelproviderService.ImportAiModelprovider(list.Adapt<List<AiModelprovider>>()));
        }

        /// <summary>
        /// Model Provider Table导入模板下载
        /// </summary>
        /// <returns></returns>
        [HttpGet("importTemplate")]
        [Log(Title = "Model Provider Table模板", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        [AllowAnonymous]
        public IActionResult ImportTemplateExcel()
        {
            var result = DownloadImportTemplate(new List<AiModelproviderDto>() { }, "AiModelprovider");
            return ExportExcel(result.Item2, result.Item1);
        }

    }
}
