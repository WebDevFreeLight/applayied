 
using Infrastructure.Converter;
using ZR.Model.System;

namespace ZR.Model.Models
{
    /// <summary>
    /// Email sending record
    /// </summary>
    [SugarTable("email_log")]
    [Tenant("0")]
    
    [SqlDict(
        dictType: "email_log",
        dictName: "Email Log",
        valueColumn: "id",
        labelColumns: "fromname",
        RemarkColumn = "remark",
        OrderBy = "id ASC",
        Description = "Auto-generated from EmailLog table"
    )]
    public class EmailLog : SysBase
    { 
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long Id { get; set; }
        /// <summary>
        /// Sender name
        /// </summary>
        public string FromName { get; set; }
        /// <summary>
        /// Sender email
        /// </summary>
        public string FromEmail { get; set; }
        /// <summary>
        /// Email subject
        /// </summary>
        public string Subject { get; set; }
        /// <summary>
        /// Recipient emails
        /// </summary>
        [SugarColumn(ColumnDataType = StaticConfig.CodeFirst_BigString)]
        public string ToEmails{ get; set; }
        /// <summary>
        /// Email content
        /// </summary>
        [SugarColumn(ColumnDataType = StaticConfig.CodeFirst_BigString)]
        public string EmailContent { get; set; }
        /// <summary>
        /// Send time
        /// </summary>
        public DateTime AddTime { get; set; } = DateTime.Now;
        /// <summary>
        /// Is sent
        /// </summary>
        public int IsSend { get; set; }
        /// <summary>
        /// Send result
        /// </summary>
        public string SendResult { get; set; }
        /// <summary>
        /// Attachment URL
        /// </summary>
        public string FileUrl { get; set; }
        /// <summary>
        /// Send time
        /// </summary>
        public DateTime SendTime { get; set; }
    }
}

