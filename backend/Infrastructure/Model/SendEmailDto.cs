using System;
using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Model
{
    public class SendEmailDto
    {
        /// <summary>
        /// ????
        /// </summary>
        public string FileUrl { get; set; } = "";
        /// <summary>
        /// ??
        /// </summary>
        [Required(ErrorMessage = "Subject cannot be empty")]
        public string Subject { get; set; }
        [Required(ErrorMessage = "Recipient cannot be empty")]
        public string ToUser { get; set; }
        public string Content { get; set; } = "";
        public string HtmlContent { get; set; }
        /// <summary>
        /// ???????
        /// </summary>
        public bool SendMe { get; set; }
        public DateTime AddTime { get; set; }
        /// <summary>
        /// ????
        /// </summary>
        public bool IsSend { get; set; }
        /// <summary>
        /// ????
        /// </summary>
        public string FromName { get; set; }
        public string FromEmail { get; set; }
    }
}

