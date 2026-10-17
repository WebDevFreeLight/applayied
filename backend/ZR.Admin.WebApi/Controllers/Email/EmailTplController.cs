using Microsoft.AspNetCore.Mvc;
using ZR.Model.Dto;
using ZR.Model.Models;

//Created time: 2023-11-12
namespace ZR.Admin.WebApi.Controllers.Email
{
    /// <summary>
    /// Email Template
    /// </summary>
    [Route("system/EmailTpl")]
    [ApiExplorerSettings(GroupName = "sys")]
    public class EmailTplController : BaseController
    {
        /// <summary>
        /// Email template interface
        /// </summary>
        private readonly IEmailTplService _EmailTplService;

        public EmailTplController(IEmailTplService EmailTplService)
        {
            _EmailTplService = EmailTplService;
        }

        /// <summary>
        /// Query email template list
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "tool:emailtpl:list")]
        public IActionResult QueryEmailTpl([FromQuery] EmailTplQueryDto parm)
        {
            var response = _EmailTplService.GetList(parm);
            return SUCCESS(response);
        }

        /// <summary>
        /// Query email template details
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpGet("{Id}")]
        public IActionResult GetEmailTpl(int Id)
        {
            var response = _EmailTplService.GetInfo(Id);

            var info = response.Adapt<EmailTpl>();
            return SUCCESS(info);
        }

        /// <summary>
        /// Add email template
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ActionPermissionFilter(Permission = "tool:emailtpl:add")]
        [Log(Title = "Email Template", BusinessType = BusinessType.INSERT)]
        public IActionResult AddEmailTpl([FromBody] EmailTplDto parm)
        {
            var modal = parm.Adapt<EmailTpl>().ToCreate(HttpContext);

            var response = _EmailTplService.AddEmailTpl(modal);

            return SUCCESS(response);
        }

        /// <summary>
        /// Update email template
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [ActionPermissionFilter(Permission = "tool:emailtpl:edit")]
        [Log(Title = "Email Template", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdateEmailTpl([FromBody] EmailTplDto parm)
        {
            var modal = parm.Adapt<EmailTpl>().ToUpdate(HttpContext);
            var response = _EmailTplService.UpdateEmailTpl(modal);

            return ToResponse(response);
        }

        /// <summary>
        /// Delete email template
        /// </summary>
        /// <returns></returns>
        [HttpDelete("{ids}")]
        [ActionPermissionFilter(Permission = "tool:emailtpl:delete")]
        [Log(Title = "Email Template", BusinessType = BusinessType.DELETE)]
        public IActionResult DeleteEmailTpl(string ids)
        {
            int[] idsArr = Tools.SpitIntArrary(ids);
            if (idsArr.Length <= 0) { return ToResponse(ApiResult.Error($"Delete failed, ID cannot be empty")); }

            var response = _EmailTplService.Delete(idsArr);

            return ToResponse(response);
        }
    }
}
