using System;
using System.ComponentModel.DataAnnotations;
using SqlSugar;
using ZR.Model.Finance.Treasury.Enums;

namespace ZR.Model.Finance.Treasury
{
    /// <summary>
    /// Cheque Status History - سجل تغييرات حالة الشيك
    /// Tracks all status transitions for audit and timeline display
    /// </summary>
    [SugarTable("cheque_status_history", "Cheque Status History")]
    public class ChequeStatusHistory
    {
        /// <summary>
        /// Primary Key
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, ColumnName = "id")]
        public Guid Id { get; set; }

        /// <summary>
        /// Reference to Cheque - مرجع الشيك
        /// </summary>
        [Required]
        [SugarColumn(ColumnName = "cheque_id")]
        public Guid ChequeId { get; set; }

        /// <summary>
        /// Previous Status - الحالة السابقة
        /// </summary>
        [SugarColumn(ColumnName = "previous_status")]
        public ChequeStatus PreviousStatus { get; set; }

        /// <summary>
        /// New Status - الحالة الجديدة
        /// </summary>
        [SugarColumn(ColumnName = "new_status")]
        public ChequeStatus NewStatus { get; set; }

        /// <summary>
        /// When the change occurred - وقت التغيير
        /// </summary>
        [SugarColumn(ColumnName = "changed_at")]
        public DateTime ChangedAt { get; set; }

        /// <summary>
        /// User who made the change - المستخدم
        /// </summary>
        [Required]
        [StringLength(100)]
        [SugarColumn(ColumnName = "changed_by")]
        public string ChangedBy { get; set; }

        /// <summary>
        /// Reason for change (bounce/cancel) - سبب التغيير
        /// </summary>
        [StringLength(500)]
        [SugarColumn(ColumnName = "reason")]
        public string Reason { get; set; }

        /// <summary>
        /// Additional notes - ملاحظات إضافية
        /// </summary>
        [StringLength(1000)]
        [SugarColumn(ColumnName = "notes")]
        public string Notes { get; set; }

        /// <summary>
        /// Bank fees if applicable - رسوم البنك
        /// </summary>
        [SugarColumn(ColumnName = "bank_fees")]
        public decimal? BankFees { get; set; }

        /// <summary>
        /// Bank reference number - مرجع البنك
        /// </summary>
        [StringLength(100)]
        [SugarColumn(ColumnName = "bank_reference")]
        public string BankReference { get; set; }

        /// <summary>
        /// GL Journal ID if created - معرف القيد المحاسبي
        /// </summary>
        [SugarColumn(ColumnName = "gl_journal_id")]
        public Guid? GLJournalId { get; set; }

        /// <summary>
        /// IP Address of user - عنوان IP
        /// </summary>
        [StringLength(50)]
        [SugarColumn(ColumnName = "ip_address")]
        public string IPAddress { get; set; }

        /// <summary>
        /// Created timestamp - وقت الإنشاء
        /// </summary>
        [SugarColumn(ColumnName = "created_at")]
        public DateTime CreatedAt { get; set; }

        // Navigation property (not mapped)
        [SugarColumn(IsIgnore = true)]
        public Cheque Cheque { get; set; }
    }
}
