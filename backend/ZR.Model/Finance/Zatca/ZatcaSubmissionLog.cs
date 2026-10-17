using System;
using SqlSugar;
using ZR.Model.Finance.Zatca.Enums;
using ZR.Model.System;

namespace ZR.Model.Finance.Zatca
{
    /// <summary>
    /// ZATCA Submission Log - سجل إرسال زاتكا
    /// Audit trail for all API calls to ZATCA
    /// </summary>
    [SugarTable("zatca_submission_log")]
    [SugarIndex("IX_zatca_log_invoice_hash", nameof(InvoiceHashId), OrderByType.Asc)]
    [SugarIndex("IX_zatca_log_date", nameof(SubmissionDate), OrderByType.Desc)]
    public class ZatcaSubmissionLog : SysBase
    {
        /// <summary>
        /// Log ID - معرف السجل
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false, ColumnName = "id")]
        public Guid Id { get; set; }

        /// <summary>
        /// Invoice Hash ID - معرف هاش الفاتورة (FK)
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "invoice_hash_id")]
        public Guid InvoiceHashId { get; set; }

        /// <summary>
        /// Device ID - معرف الجهاز
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "device_id")]
        public Guid DeviceId { get; set; }

        /// <summary>
        /// API Endpoint - نقطة نهاية API
        /// </summary>
        [SugarColumn(IsNullable = false, Length = 500, ColumnName = "api_endpoint")]
        public string ApiEndpoint { get; set; }

        /// <summary>
        /// API Operation Type - نوع العملية
        /// compliance-csid, production-csid, reporting, clearance, etc.
        /// </summary>
        [SugarColumn(IsNullable = false, Length = 50, ColumnName = "operation_type")]
        public string OperationType { get; set; }

        /// <summary>
        /// Environment - البيئة
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "environment")]
        public ZatcaEnvironment Environment { get; set; }

        /// <summary>
        /// Submission Date - تاريخ الإرسال
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "submission_date")]
        public DateTime SubmissionDate { get; set; }

        /// <summary>
        /// Request Headers (JSON) - رؤوس الطلب
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDataType = StaticConfig.CodeFirst_BigString, ColumnName = "request_headers")]
        public string RequestHeaders { get; set; }

        /// <summary>
        /// Request Body - جسم الطلب
        /// Invoice XML or request payload
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDataType = StaticConfig.CodeFirst_BigString, ColumnName = "request_body")]
        public string RequestBody { get; set; }

        /// <summary>
        /// HTTP Status Code - كود حالة HTTP
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "http_status_code")]
        public int? HttpStatusCode { get; set; }

        /// <summary>
        /// Response Headers (JSON) - رؤوس الاستجابة
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDataType = StaticConfig.CodeFirst_BigString, ColumnName = "response_headers")]
        public string ResponseHeaders { get; set; }

        /// <summary>
        /// Response Body - جسم الاستجابة
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDataType = StaticConfig.CodeFirst_BigString, ColumnName = "response_body")]
        public string ResponseBody { get; set; }

        /// <summary>
        /// ZATCA Disposition Status - حالة التصرف من زاتكا
        /// REPORTED, CLEARED, REJECTED, etc.
        /// </summary>
        [SugarColumn(IsNullable = true, Length = 50, ColumnName = "disposition_status")]
        public string DispositionStatus { get; set; }

        /// <summary>
        /// Is Success - ناجح
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "is_success")]
        public bool IsSuccess { get; set; } = false;

        /// <summary>
        /// Error Code - كود الخطأ
        /// </summary>
        [SugarColumn(IsNullable = true, Length = 50, ColumnName = "error_code")]
        public string ErrorCode { get; set; }

        /// <summary>
        /// Error Message - رسالة الخطأ
        /// </summary>
        [SugarColumn(IsNullable = true, Length = 2000, ColumnName = "error_message")]
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Validation Steps (JSON) - خطوات التحقق
        /// Array of validation results from ZATCA
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDataType = StaticConfig.CodeFirst_BigString, ColumnName = "validation_steps")]
        public string ValidationSteps { get; set; }

        /// <summary>
        /// Response Time (ms) - زمن الاستجابة بالمللي ثانية
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "response_time_ms")]
        public long? ResponseTimeMs { get; set; }

        /// <summary>
        /// Retry Attempt Number - رقم محاولة إعادة المحاولة
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "retry_attempt")]
        public int RetryAttempt { get; set; } = 0;

        /// <summary>
        /// Client IP Address - عنوان IP للعميل
        /// </summary>
        [SugarColumn(IsNullable = true, Length = 50, ColumnName = "client_ip")]
        public string ClientIp { get; set; }

        /// <summary>
        /// User Agent - وكيل المستخدم
        /// </summary>
        [SugarColumn(IsNullable = true, Length = 500, ColumnName = "user_agent")]
        public string UserAgent { get; set; }

        /// <summary>
        /// Invoice Hash (Navigation) - هاش الفاتورة
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public ZatcaInvoiceHash InvoiceHash { get; set; }
    }
}
