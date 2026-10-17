using Microsoft.AspNetCore.Mvc;
using ZR.Model.Boulevard.Dto;
using ZR.Model.Boulevard;
using ZR.Service.Boulevard.IBoulevardService;
using MiniExcelLibs;

//创建时间：2025-10-04
namespace ZR.Admin.WebApi.Controllers.Boulevard
{
    /// <summary>
    /// CustomerRegistrationForm
    /// </summary>
    [Route("Boulevard/BlvCustomerregistrationform")]
    [ApiExplorerSettings(GroupName = "boulevard")]

    public class BlvCustomerregistrationformController : BaseController
    {
        /// <summary>
        /// CustomerRegistrationForm接口
        /// </summary>
        private readonly IBlvCustomerregistrationformService _BlvCustomerregistrationformService;

        public BlvCustomerregistrationformController(IBlvCustomerregistrationformService BlvCustomerregistrationformService)
        {
            _BlvCustomerregistrationformService = BlvCustomerregistrationformService;
        }

        /// <summary>
        /// 查询CustomerRegistrationForm列表
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "blvcustomerregistrationform:list")]
        public IActionResult QueryBlvCustomerregistrationform([FromQuery] BlvCustomerregistrationformQueryDto parm)
        {
            var response = _BlvCustomerregistrationformService.GetList(parm);
            return SUCCESS(response);
        }


        /// <summary>
        /// 查询CustomerRegistrationForm详情
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpGet("{Id}")]
        [ActionPermissionFilter(Permission = "blvcustomerregistrationform:query")]
        public IActionResult GetBlvCustomerregistrationform(long Id)
        {
            var response = _BlvCustomerregistrationformService.GetInfo(Id);
            
            var info = response.Adapt<BlvCustomerregistrationformDto>();
            return SUCCESS(info);
        }

        /// <summary>
        ///  CustomerRegistrationForm add
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost]
        //[ActionPermissionFilter(Permission = "blvcustomerregistrationform:add")]
        [Log(Title = "CustomerRegistrationForm", BusinessType = BusinessType.INSERT)]
        public IActionResult AddBlvCustomerregistrationform([FromBody] BlvCustomerregistrationformDto parm)
        {
            if (parm==null)
            {

            }
            var modal = parm.Adapt<BlvCustomerregistrationform>().ToCreate(HttpContext);

            var response = _BlvCustomerregistrationformService.AddBlvCustomerregistrationform(modal);

            return SUCCESS(response);
        }

        /// <summary>
        /// 更新CustomerRegistrationForm
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [ActionPermissionFilter(Permission = "blvcustomerregistrationform:edit")]
        [Log(Title = "CustomerRegistrationForm", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdateBlvCustomerregistrationform([FromBody] BlvCustomerregistrationformDto parm)
        {
            var modal = parm.Adapt<BlvCustomerregistrationform>().ToUpdate(HttpContext);
            var response = _BlvCustomerregistrationformService.UpdateBlvCustomerregistrationform(modal);

            return ToResponse(response);
        }

        /// <summary>
        /// 删除CustomerRegistrationForm
        /// </summary>
        /// <returns></returns>
        [HttpPost("delete/{ids}")]
        [ActionPermissionFilter(Permission = "blvcustomerregistrationform:delete")]
        [Log(Title = "CustomerRegistrationForm", BusinessType = BusinessType.DELETE)]
        public IActionResult DeleteBlvCustomerregistrationform([FromRoute]string ids)
        {
            var idArr = Tools.SplitAndConvert<long>(ids);

            return ToResponse(_BlvCustomerregistrationformService.Delete(idArr, "删除CustomerRegistrationForm"));
        }

        /// <summary>
        /// 导出CustomerRegistrationForm
        /// </summary>
        /// <returns></returns>
        [Log(Title = "CustomerRegistrationForm", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        [HttpGet("export")]
        [ActionPermissionFilter(Permission = "blvcustomerregistrationform:export")]
        public IActionResult Export([FromQuery] BlvCustomerregistrationformQueryDto parm)
        {
            var list = _BlvCustomerregistrationformService.ExportList(parm).Result;
            if (list == null || list.Count <= 0)
            {
                return ToResponse(ResultCode.FAIL, "没有要导出的数据");
            }
            var result = ExportExcelMini(list, "CustomerRegistrationForm", "CustomerRegistrationForm");
            return ExportExcel(result.Item2, result.Item1);
        }

        /// <summary>
        /// 清空CustomerRegistrationForm
        /// </summary>
        /// <returns></returns>
        [Log(Title = "CustomerRegistrationForm", BusinessType = BusinessType.CLEAN)]
        [ActionPermissionFilter(Permission = "blvcustomerregistrationform:delete")]
        [HttpPost("clean")]
        public IActionResult Clear()
        {
            if (!HttpContextExtension.IsAdmin(HttpContext))
            {
                return ToResponse(ResultCode.FAIL, "操作失败");
            }
            return SUCCESS(_BlvCustomerregistrationformService.TruncateBlvCustomerregistrationform());
        }

        /// <summary>
        /// 导入
        /// </summary>
        /// <param name="formFile"></param>
        /// <returns></returns>
        [HttpPost("importData")]
        [Log(Title = "CustomerRegistrationForm导入", BusinessType = BusinessType.IMPORT, IsSaveRequestData = false)]
        [ActionPermissionFilter(Permission = "blvcustomerregistrationform:import")]
        public IActionResult ImportData([FromForm(Name = "file")] IFormFile formFile)
        {
            List<BlvCustomerregistrationformDto> list = new();
            using (var stream = formFile.OpenReadStream())
            {
                list = stream.Query<BlvCustomerregistrationformDto>(startCell: "A1").ToList();
            }

            return SUCCESS(_BlvCustomerregistrationformService.ImportBlvCustomerregistrationform(list.Adapt<List<BlvCustomerregistrationform>>()));
        }

        /// <summary>
        /// CustomerRegistrationForm导入模板下载
        /// </summary>
        /// <returns></returns>
        [HttpGet("importTemplate")]
        [Log(Title = "CustomerRegistrationForm模板", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        [AllowAnonymous]
        public IActionResult ImportTemplateExcel()
        {
            var result = DownloadImportTemplate(new List<BlvCustomerregistrationformDto>() { }, "BlvCustomerregistrationform");
            return ExportExcel(result.Item2, result.Item1);
        }

    }
}
