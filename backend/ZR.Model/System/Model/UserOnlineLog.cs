
using ZR.Model.System;

namespace ZR.Model.Models
{
    /// <summary>
    /// User online duration
    /// </summary>
    [SugarTable("userOnlineLog", TableDescription = "User online duration")]
    [Tenant("0")]
    
    [SqlDict(
        dictType: "useronlinelog",
        dictName: "User Online Log",
        valueColumn: "id",
        labelColumns: "location",
        RemarkColumn = "remark",
        OrderBy = "id ASC",
        Description = "Auto-generated from UserOnlineLog table"
    )]
    public class UserOnlineLog: SysBase
    {
        /// <summary>
        /// ID
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long Id { get; set; }

        /// <summary>
        /// User ID
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long UserId { get; set; }

        /// <summary>
        /// Online duration (minutes)
        /// </summary>
        public double OnlineTime { get; set; }
        /// <summary>
        /// Today's online duration
        /// </summary>
        public double TodayOnlineTime { get; set; }

        /// <summary>
        /// End time
        /// </summary>
        public DateTime? AddTime { get; set; }

        /// <summary>
        /// Address location
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// User IP
        /// </summary>
        public string UserIP { get; set; }
        public DateTime LoginTime { get; set; }
        /// <summary>
        /// Remark
        /// </summary>
        public string Remark { get; set; }
        /// <summary>
        /// Login platform
        /// </summary>
        public string Platform { get; set; }
    }
}

