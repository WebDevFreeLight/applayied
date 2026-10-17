using System;
using SqlSugar;
using ZR.Model.System;

namespace ZR.Model.Finance.Treasury
{
    /// <summary>
    /// Treasury Data Retention Configuration - إعدادات الاحتفاظ بالبيانات
    /// Configuration for audit log archiving and data retention policy (requirement #55)
    /// </summary>
    [SugarTable("treasury_data_retention_config")]
    public class TreasuryDataRetentionConfig : SysBase
    {
        /// <summary>
        /// Override BranchID from SysBase - not used in Treasury tables
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public new long? BranchID { get; set; }

        /// <summary>
        /// Config ID - معرف الإعدادات
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false, ColumnName = "id")]
        public Guid Id { get; set; }

        /// <summary>
        /// Config Name - اسم الإعدادات
        /// </summary>
        [SugarColumn(Length = 100, ColumnName = "config_name")]
        public string ConfigName { get; set; }

        // Note: CompanyID and BranchID are inherited from SysBase - do not duplicate

        #region Audit Log Retention

        /// <summary>
        /// Audit Log Retention Days - أيام الاحتفاظ بسجل التدقيق
        /// Number of days to keep audit logs online before archiving
        /// </summary>
        [SugarColumn(ColumnName = "audit_log_retention_days")]
        public int AuditLogRetentionDays { get; set; } = 365; // 1 year default

        /// <summary>
        /// Audit Log Archive Enabled - تفعيل أرشفة سجل التدقيق
        /// </summary>
        [SugarColumn(ColumnName = "audit_log_archive_enabled")]
        public bool AuditLogArchiveEnabled { get; set; } = true;

        /// <summary>
        /// Audit Log Archive Path - مسار أرشيف سجل التدقيق
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnName = "audit_log_archive_path")]
        public string AuditLogArchivePath { get; set; }

        /// <summary>
        /// Last Audit Archive Date - تاريخ آخر أرشفة
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "last_audit_archive_date")]
        public DateTime? LastAuditArchiveDate { get; set; }

        /// <summary>
        /// Last Audit Archive Record Count - عدد السجلات المؤرشفة
        /// </summary>
        [SugarColumn(ColumnName = "last_audit_archive_count")]
        public long LastAuditArchiveCount { get; set; }

        #endregion

        #region Bank Statement Retention

        /// <summary>
        /// Bank Statement Retention Days - أيام الاحتفاظ بكشوف البنك
        /// </summary>
        [SugarColumn(ColumnName = "bank_statement_retention_days")]
        public int BankStatementRetentionDays { get; set; } = 730; // 2 years default

        /// <summary>
        /// Bank Statement Archive Enabled - تفعيل أرشفة كشوف البنك
        /// </summary>
        [SugarColumn(ColumnName = "bank_statement_archive_enabled")]
        public bool BankStatementArchiveEnabled { get; set; } = false;

        #endregion

        #region Reconciliation Retention

        /// <summary>
        /// Reconciliation Retention Days - أيام الاحتفاظ بالتسويات
        /// </summary>
        [SugarColumn(ColumnName = "reconciliation_retention_days")]
        public int ReconciliationRetentionDays { get; set; } = 2555; // 7 years (IFRS requirement)

        /// <summary>
        /// Keep Reconciliation Attachments - الاحتفاظ بمرفقات التسويات
        /// </summary>
        [SugarColumn(ColumnName = "keep_reconciliation_attachments")]
        public bool KeepReconciliationAttachments { get; set; } = true;

        #endregion

        #region Payment Retention

        /// <summary>
        /// Payment Retention Days - أيام الاحتفاظ بالدفعات
        /// </summary>
        [SugarColumn(ColumnName = "payment_retention_days")]
        public int PaymentRetentionDays { get; set; } = 2555; // 7 years (IFRS requirement)

        /// <summary>
        /// Archive Posted Payments Only - أرشفة الدفعات المسجلة فقط
        /// </summary>
        [SugarColumn(ColumnName = "archive_posted_payments_only")]
        public bool ArchivePostedPaymentsOnly { get; set; } = true;

        #endregion

        #region Cheque Retention

        /// <summary>
        /// Cheque Retention Days - أيام الاحتفاظ بالشيكات
        /// </summary>
        [SugarColumn(ColumnName = "cheque_retention_days")]
        public int ChequeRetentionDays { get; set; } = 2555; // 7 years

        /// <summary>
        /// Archive Cleared Cheques Only - أرشفة الشيكات المسددة فقط
        /// </summary>
        [SugarColumn(ColumnName = "archive_cleared_cheques_only")]
        public bool ArchiveClearedChequesOnly { get; set; } = true;

        #endregion

        #region Attachment Retention

        /// <summary>
        /// Attachment Retention Days - أيام الاحتفاظ بالمرفقات
        /// </summary>
        [SugarColumn(ColumnName = "attachment_retention_days")]
        public int AttachmentRetentionDays { get; set; } = 2555; // 7 years

        /// <summary>
        /// Compress Attachments Before Archive - ضغط المرفقات قبل الأرشفة
        /// </summary>
        [SugarColumn(ColumnName = "compress_attachments")]
        public bool CompressAttachments { get; set; } = true;

        #endregion

        #region Archive Settings

        /// <summary>
        /// Archive Schedule - جدول الأرشفة (Cron expression)
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnName = "archive_schedule")]
        public string ArchiveSchedule { get; set; } = "0 2 * * 0"; // Every Sunday at 2 AM

        /// <summary>
        /// Archive Batch Size - حجم دفعة الأرشفة
        /// </summary>
        [SugarColumn(ColumnName = "archive_batch_size")]
        public int ArchiveBatchSize { get; set; } = 10000;

        /// <summary>
        /// Archive Format - تنسيق الأرشيف
        /// "JSON", "CSV", "PARQUET"
        /// </summary>
        [SugarColumn(Length = 20, IsNullable = true, ColumnName = "archive_format")]
        public string ArchiveFormat { get; set; } = "JSON";

        /// <summary>
        /// Archive Storage Type - نوع تخزين الأرشيف
        /// "LOCAL", "S3", "AZURE_BLOB", "GCS"
        /// </summary>
        [SugarColumn(Length = 20, IsNullable = true, ColumnName = "archive_storage_type")]
        public string ArchiveStorageType { get; set; } = "LOCAL";

        /// <summary>
        /// Archive Storage Connection - اتصال تخزين الأرشيف
        /// Connection string or path
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnName = "archive_storage_connection")]
        public string ArchiveStorageConnection { get; set; }

        #endregion

        #region Compliance Settings

        /// <summary>
        /// ZATCA Compliance - التوافق مع هيئة الزكاة والضريبة والجمارك
        /// Minimum retention period for Saudi Arabia
        /// </summary>
        [SugarColumn(ColumnName = "zatca_compliance")]
        public bool ZATCACompliance { get; set; } = true;

        /// <summary>
        /// IFRS Compliance - التوافق مع معايير التقارير المالية الدولية
        /// </summary>
        [SugarColumn(ColumnName = "ifrs_compliance")]
        public bool IFRSCompliance { get; set; } = true;

        /// <summary>
        /// Legal Hold Enabled - تفعيل الحجز القانوني
        /// Prevents archiving of records under legal hold
        /// </summary>
        [SugarColumn(ColumnName = "legal_hold_enabled")]
        public bool LegalHoldEnabled { get; set; } = false;

        #endregion

        /// <summary>
        /// Is Active - نشط
        /// </summary>
        [SugarColumn(ColumnName = "is_active")]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Notes - ملاحظات
        /// </summary>
        [SugarColumn(Length = 1000, IsNullable = true, ColumnName = "notes")]
        public string Notes { get; set; }
    }
}
