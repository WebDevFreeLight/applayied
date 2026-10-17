using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ZR.Model.Dto;
using ZR.Model.Models;

//Created time: 2023-11-20
namespace ZR.Admin.WebApi.Controllers.Email
{
    /// <summary>
    /// Email Send Log
    /// </summary>
    [Route("system/EmailLog")]
    [ApiExplorerSettings(GroupName = "sys")]
    public class EmailLogController : BaseController
    {
        /// <summary>
        /// Email send log interface
        /// </summary>
        private readonly IEmailLogService _EmailLogService;
        private OptionsSetting OptionsSetting;
        public EmailLogController(
            IEmailLogService EmailLogService,
            IOptions<OptionsSetting> options)
        {
            _EmailLogService = EmailLogService;
            OptionsSetting = options.Value;
        }

        /// <summary>
        /// Query email send log list
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "emaillog:list")]
        public IActionResult QueryEmailLog([FromQuery] EmailLogQueryDto parm)
        {
            var response = _EmailLogService.GetList(parm);
            return SUCCESS(response);
        }

        /// <summary>
        /// Query email send log details
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpGet("{Id}")]
        [ActionPermissionFilter(Permission = "emaillog:query")]
        public IActionResult GetEmailLog(long Id)
        {
            var response = _EmailLogService.GetInfo(Id);

            var info = response.Adapt<EmailLogDto>();
            return SUCCESS(info);
        }

        /// <summary>
        /// Batch send emails (send from log records)
        /// </summary>
        /// <returns></returns>
        [HttpPost("sendEmail")]
        [ActionPermissionFilter(Permission = "tool:email:send")]
        [Log(Title = "Batch Email Send", BusinessType = BusinessType.INSERT)]
        public IActionResult SendEmail([FromBody] EmailLogDto dto)
        {
            if (dto.IdArr.Length <= 0) { return ToResponse(ApiResult.Error($"Send failed, ID cannot be empty")); }
            int count = 0;

            foreach (var item in dto.IdArr)
            {
                var response = _EmailLogService.GetInfo(item);
                MailOptions mailOptions = OptionsSetting.MailOptions.Find(x => x.FromName == response.FromName);
                if (mailOptions == null || string.IsNullOrEmpty(mailOptions.Password))
                {
                    continue;
                }
                MailHelper mailHelper = new(mailOptions);
                string[] toUsers = response.ToEmails.Split(",", StringSplitOptions.RemoveEmptyEntries);

                string result = mailHelper.SendMail(toUsers, response.Subject, "", response.FileUrl, response.EmailContent);
                count += _EmailLogService.Update(x => x.Id == item, x => new EmailLog()
                {
                    IsSend = 1,
                    SendTime = DateTime.Now,
                    SendResult = result
                });
            }
            return SUCCESS(count);
        }

        /// <summary>
        /// Delete email send log
        /// </summary>
        /// <returns></returns>
        [HttpDelete("{ids}")]
        [ActionPermissionFilter(Permission = "emaillog:delete")]
        [Log(Title = "Email Send Log", BusinessType = BusinessType.DELETE)]
        public IActionResult DeleteEmailLog(string ids)
        {
            long[] idsArr = Tools.SpitLongArrary(ids);
            if (idsArr.Length <= 0) { return ToResponse(ApiResult.Error($"Delete failed, ID cannot be empty")); }

            var response = _EmailLogService.Delete(idsArr);

            return ToResponse(response);
        }

        /// <summary>
        /// Send email
        /// </summary>
        /// <param name="sendEmailVo">Request parameter receiving entity</param>
        /// <returns></returns>
        [ActionPermissionFilter(Permission = "tool:email:send")]
        [Log(Title = "Send Email")]
        [HttpPost("/common/SendEmail")]
        public IActionResult SendEmail([FromBody] SendEmailDto sendEmailVo)
        {
            if (sendEmailVo == null)
            {
                return ToResponse(ApiResult.Error($"Request parameters are incomplete"));
            }
            MailOptions mailOptions = OptionsSetting.MailOptions.Find(x => x.FromName == sendEmailVo.FromName);
            if (mailOptions == null || string.IsNullOrEmpty(mailOptions.Password))
            {
                return ToResponse(ApiResult.Error($"Please configure email information"));
            }

            MailHelper mailHelper = new(mailOptions);

            string[] toUsers = sendEmailVo.ToUser.Split(",", StringSplitOptions.RemoveEmptyEntries);
            string result = string.Empty;
            if (sendEmailVo.IsSend)
            {
                result = mailHelper.SendMail(toUsers, sendEmailVo.Subject, sendEmailVo.Content, sendEmailVo.FileUrl, sendEmailVo.HtmlContent);
            }




            sendEmailVo.FromEmail = mailOptions.FromEmail;
            _EmailLogService.AddEmailLog(sendEmailVo, result);
            //logger.Info($"Send email{JsonConvert.SerializeObject(sendEmailVo)}, result{result}");

            return SUCCESS(result);
        }

    }
}
