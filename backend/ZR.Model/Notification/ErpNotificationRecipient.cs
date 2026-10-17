using Infrastructure.Converter;
using SqlSugar;
using Newtonsoft.Json;

namespace ZR.Model.Notification
{
    /// <summary>
    /// Notification recipient (one row per user per notification) - مستلم الإشعار
    /// </summary>
    [SugarTable("erp_notification_recipient")]
    public class ErpNotificationRecipient
    {
        /// <summary>
        /// Recipient ID - معرف المستلم
        /// </summary>
        [SugarColumn(ColumnName = "recipientid", IsPrimaryKey = true, IsIdentity = false)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long RecipientId { get; set; }

        /// <summary>
        /// Notification ID - معرف الإشعار
        /// </summary>
        [SugarColumn(ColumnName = "notificationid")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long NotificationId { get; set; }

        /// <summary>
        /// User ID - معرف المستخدم
        /// </summary>
        [SugarColumn(ColumnName = "userid")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long UserId { get; set; }

        /// <summary>
        /// Is Read - مقروء
        /// </summary>
        [SugarColumn(ColumnName = "isread", IsNullable = true)]
        public bool IsRead { get; set; }

        /// <summary>
        /// Read At - وقت القراءة
        /// </summary>
        [SugarColumn(ColumnName = "readat", IsNullable = true)]
        public DateTime? ReadAt { get; set; }

        /// <summary>
        /// Is Deleted - محذوف
        /// </summary>
        [SugarColumn(ColumnName = "isdeleted", IsNullable = true)]
        public bool IsDeleted { get; set; }

        /// <summary>
        /// Deleted At - وقت الحذف
        /// </summary>
        [SugarColumn(ColumnName = "deletedat", IsNullable = true)]
        public DateTime? DeletedAt { get; set; }

        /// <summary>
        /// Create Time - وقت الإنشاء
        /// </summary>
        [SugarColumn(ColumnName = "createtime", IsNullable = true)]
        public DateTime CreateTime { get; set; }
    }
}
