 
using Infrastructure.Converter;
using ZR.Model.System;

namespace ZR.Model.Models
{
    /// <summary>
    /// SMS verification code record
    /// </summary>
    [SugarTable("smsCode_log")]
    [Tenant("0")]
    
    [SqlDict(
        dictType: "smscode_log",
        dictName: "Sms Code Log",
        valueColumn: "id",
        labelColumns: "smscode",
        RemarkColumn = "remark",
        OrderBy = "id ASC",
        Description = "Auto-generated from SmsCodeLog table"
    )]
    public class SmsCodeLog : SysBase
    { 
        [SugarColumn(IsPrimaryKey = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long Id { get; set; }
        /// <summary>
        /// SMS verification code
        /// </summary>
        public string SmsCode { get; set; }
        /// <summary>
        /// User ID
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long Userid { get; set; }
        /// <summary>
        /// Phone number
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long PhoneNum { get; set; }
        /// <summary>
        /// SMS content
        /// </summary>
        public string SmsContent { get; set; }
        /// <summary>
        /// Send time
        /// </summary>
        [SugarColumn(InsertServerTime = true)]
        public DateTime AddTime { get; set; }
        /// <summary>
        /// User IP
        /// </summary>
        public string UserIP { get; set; }
        /// <summary>
        /// Location
        /// </summary>
        public string Location { get; set; }
        /// <summary>
        /// 1: Login, 2: Register, 3: Password recovery
        /// </summary>
        public int SendType { get; set; }
    }
}

