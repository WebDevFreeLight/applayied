 
using Infrastructure.Converter;
using ZR.Model.System;
using ZR.Model.System.Enums;

namespace ZR.Model
{
    /// <summary>
    /// User system message
    /// </summary>
    [SugarTable("sys_user_msg")]
    [Tenant(0)]
    public class SysUserMsg : SysBase
    {
        /// <summary>
        /// Message ID
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long MsgId { get; set; }

        /// <summary>
        /// User ID
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? UserId { get; set; }

        /// <summary>
        /// Message content
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Is read
        /// </summary>
        public int IsRead { get; set; }

        /// <summary>
        /// Add time
        /// </summary>
        [SugarColumn(InsertServerTime = true)]
        public DateTime? AddTime { get; set; }

        /// <summary>
        /// Target ID
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? TargetId { get; set; }

        /// <summary>
        /// Message type
        /// </summary>
        public UserMsgType MsgType { get; set; }

        /// <summary>
        /// Is deleted
        /// </summary>
        public int IsDelete { get; set; }
        /// <summary>
        /// Source user
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long FromUserid { get; set; }
        /// <summary>
        /// Associated user
        /// </summary>
        [Navigate(NavigateType.OneToOne, nameof(FromUserid), nameof(SysUser.UserId))]//变量名不要等类名 
        public SysUser User { get; set; }
    }
}

