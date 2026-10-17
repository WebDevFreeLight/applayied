using System;
using SqlSugar;
using ZR.Model.Finance.Treasury.Enums;
using ZR.Model.System;

namespace ZR.Model.Finance.Treasury
{
    /// <summary>
    /// Treasury Audit Log - سجل تدقيق الخزينة
    /// For tracking all sensitive treasury operations (requirement #22)
    /// </summary>
    [SugarTable("treasury_audit_log")]
    public class TreasuryAuditLog : SysBase
    {
        /// <summary>
        /// Override BranchID from SysBase - not used
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public new long? BranchID { get; set; }

        /// <summary>
        /// Audit Log ID - معرف سجل التدقيق
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false, ColumnName = "id")]
        public Guid Id { get; set; }

        /// <summary>
        /// Entity Type - نوع الكيان
        /// </summary>
        [SugarColumn(ColumnName = "entity_type")]
        public TreasuryEntityType EntityType { get; set; }

        /// <summary>
        /// Entity ID - معرف الكيان
        /// </summary>
        [SugarColumn(ColumnName = "entity_id")]
        public Guid EntityId { get; set; }

        /// <summary>
        /// Entity Number - رقم الكيان (e.g., payment number, cheque number)
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true, ColumnName = "entity_number")]
        public string EntityNumber { get; set; }

        /// <summary>
        /// Action - الإجراء
        /// </summary>
        [SugarColumn(ColumnName = "action")]
        public TreasuryAuditAction Action { get; set; }

        /// <summary>
        /// Action Description - وصف الإجراء
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnName = "action_description")]
        public string ActionDescription { get; set; }

        /// <summary>
        /// Old Values - القيم القديمة (JSON)
        /// </summary>
        [SugarColumn(ColumnDataType = "text", IsNullable = true, ColumnName = "old_values")]
        public string OldValues { get; set; }

        /// <summary>
        /// New Values - القيم الجديدة (JSON)
        /// </summary>
        [SugarColumn(ColumnDataType = "text", IsNullable = true, ColumnName = "new_values")]
        public string NewValues { get; set; }

        /// <summary>
        /// Changed Fields - الحقول المتغيرة (comma-separated)
        /// </summary>
        [SugarColumn(Length = 1000, IsNullable = true, ColumnName = "changed_fields")]
        public string ChangedFields { get; set; }

        /// <summary>
        /// Amount - المبلغ (if applicable)
        /// </summary>
        [SugarColumn(ColumnName = "amount", ColumnDataType = "decimal(18,4)", IsNullable = true)]
        public decimal? Amount { get; set; }

        /// <summary>
        /// Currency - العملة
        /// </summary>
        [SugarColumn(Length = 3, IsNullable = true, ColumnName = "currency")]
        public string Currency { get; set; }

        /// <summary>
        /// User ID - معرف المستخدم
        /// </summary>
        [SugarColumn(Length = 100, ColumnName = "user_id")]
        public string UserId { get; set; }

        /// <summary>
        /// User Name - اسم المستخدم
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = true, ColumnName = "user_name")]
        public string UserName { get; set; }

        /// <summary>
        /// IP Address - عنوان IP
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnName = "ip_address")]
        public string IpAddress { get; set; }

        /// <summary>
        /// User Agent - وكيل المستخدم
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnName = "user_agent")]
        public string UserAgent { get; set; }

        /// <summary>
        /// Trace ID - معرف التتبع
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true, ColumnName = "trace_id")]
        public string TraceId { get; set; }

        /// <summary>
        /// Request ID - معرف الطلب
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true, ColumnName = "request_id")]
        public string RequestId { get; set; }

        /// <summary>
        /// Timestamp - الطابع الزمني
        /// </summary>
        [SugarColumn(ColumnName = "timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Is Successful - هل نجح الإجراء
        /// </summary>
        [SugarColumn(ColumnName = "is_successful")]
        public bool IsSuccessful { get; set; } = true;

        /// <summary>
        /// Error Message - رسالة الخطأ
        /// </summary>
        [SugarColumn(Length = 1000, IsNullable = true, ColumnName = "error_message")]
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Additional Context - سياق إضافي (JSON)
        /// </summary>
        [SugarColumn(ColumnDataType = "text", IsNullable = true, ColumnName = "additional_context")]
        public string AdditionalContext { get; set; }
    }
}
