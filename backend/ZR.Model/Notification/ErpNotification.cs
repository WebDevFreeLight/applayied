using Infrastructure.Converter;
using SqlSugar;
using Newtonsoft.Json;
using ZR.Model.System;

namespace ZR.Model.Notification
{
    /// <summary>
    /// Notification content (one row per notification) - محتوى الإشعار
    /// </summary>
    [SugarTable("erp_notification")]
    public class ErpNotification : SysBase
    {
        /// <summary>
        /// Notification ID - معرف الإشعار
        /// </summary>
        [SugarColumn(ColumnName = "notificationid", IsPrimaryKey = true, IsIdentity = false)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long NotificationId { get; set; }

        /// <summary>
        /// Notification Type - نوع الإشعار (system/sms/email/push/whatsapp)
        /// </summary>
        [SugarColumn(ColumnName = "notificationtype", Length = 50, IsNullable = true)]
        public string NotificationType { get; set; }

        /// <summary>
        /// Title - العنوان
        /// </summary>
        [SugarColumn(ColumnName = "title", Length = 500, IsNullable = true)]
        public string Title { get; set; }

        /// <summary>
        /// Title Arabic - العنوان بالعربي
        /// </summary>
        [SugarColumn(ColumnName = "titlear", Length = 500, IsNullable = true)]
        public string TitleAr { get; set; }

        /// <summary>
        /// Message - الرسالة
        /// </summary>
        [SugarColumn(ColumnName = "message", ColumnDataType = "text", IsNullable = true)]
        public string Message { get; set; }

        /// <summary>
        /// Message Arabic - الرسالة بالعربي
        /// </summary>
        [SugarColumn(ColumnName = "messagear", ColumnDataType = "text", IsNullable = true)]
        public string MessageAr { get; set; }

        /// <summary>
        /// Reference Type - نوع المرجع
        /// </summary>
        [SugarColumn(ColumnName = "referencetype", Length = 100, IsNullable = true)]
        public string ReferenceType { get; set; }

        /// <summary>
        /// Reference ID - معرف المرجع
        /// </summary>
        [SugarColumn(ColumnName = "referenceid", IsNullable = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? ReferenceId { get; set; }

        /// <summary>
        /// Severity - مستوى الأهمية (info/warning/error/success)
        /// </summary>
        [SugarColumn(ColumnName = "severity", Length = 20, IsNullable = true)]
        public string Severity { get; set; } = "info";

        /// <summary>
        /// Module - الوحدة المصدرية (e.g. "leads", "hr", "sales")
        /// </summary>
        [SugarColumn(ColumnName = "module", Length = 100, IsNullable = true)]
        public string Module { get; set; }

        /// <summary>
        /// Action URL - رابط الإجراء
        /// </summary>
        [SugarColumn(ColumnName = "actionurl", Length = 500, IsNullable = true)]
        public string ActionUrl { get; set; }

        /// <summary>
        /// Target Roles - الأدوار المستهدفة (JSON audit trail)
        /// </summary>
        [SugarColumn(ColumnName = "targetroles", ColumnDataType = "text", IsNullable = true)]
        public string TargetRoles { get; set; }

        /// <summary>
        /// Target Scope - نطاق الاستهداف (targeted/broadcast)
        /// </summary>
        [SugarColumn(ColumnName = "targetscope", Length = 50, IsNullable = true)]
        public string TargetScope { get; set; } = "targeted";

        /// <summary>
        /// Sender User ID - معرف المرسل
        /// </summary>
        [SugarColumn(ColumnName = "senderuserid", IsNullable = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? SenderUserId { get; set; }

        /// <summary>
        /// Sender Name - اسم المرسل
        /// </summary>
        [SugarColumn(ColumnName = "sendername", Length = 200, IsNullable = true)]
        public string SenderName { get; set; }

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
    }
}
