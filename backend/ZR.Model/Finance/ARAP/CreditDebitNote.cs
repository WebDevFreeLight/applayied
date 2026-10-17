using System;
using SqlSugar;
using ZR.Model.Finance.ARAP.Enums;
using ZR.Model.Finance.Zatca.Enums;
using ZR.Model.System;

namespace ZR.Model.Finance.ARAP
{
    /// <summary>
    /// Credit/Debit Note - إشعار دائن/مدين
    /// </summary>
    [SugarTable("arap_credit_debit_note")]
    public class CreditDebitNote : SysBase
    {
        /// <summary>
        /// Note ID - معرف الإشعار
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        public Guid Id { get; set; }

        /// <summary>
        /// Note Number - رقم الإشعار
        /// </summary>
        [SugarColumn(Length = 50)]
        public string NoteNumber { get; set; }

        /// <summary>
        /// Note Type - نوع الإشعار
        /// </summary>
        public NoteType NoteType { get; set; }

        /// <summary>
        /// Customer ID (nullable for AR adjustments) - معرف العميل (للتعديلات على العملاء)
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public Guid? CustomerId { get; set; }

        /// <summary>
        /// Vendor ID (nullable for AP adjustments) - معرف المورد (للتعديلات على الموردين)
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public Guid? VendorId { get; set; }

        /// <summary>
        /// Amount - المبلغ
        /// </summary>
        [SugarColumn(DecimalDigits = 2)]
        public decimal Amount { get; set; }

        /// <summary>
        /// Note Date - تاريخ الإشعار
        /// </summary>
        public DateTime NoteDate { get; set; }

        /// <summary>
        /// Reason - السبب
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true)]
        public string Reason { get; set; }

        /// <summary>
        /// GL Journal ID - معرف القيد المحاسبي
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "gl_journal_id")]
        public Guid? GLJournalId { get; set; }

        /// <summary>
        /// Original Invoice ID - معرف الفاتورة الأصلية
        /// Reference to the invoice being adjusted (required by ZATCA)
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "original_invoice_id")]
        public Guid? OriginalInvoiceId { get; set; }

        /// <summary>
        /// Original Invoice Number - رقم الفاتورة الأصلية
        /// </summary>
        [SugarColumn(IsNullable = true, Length = 50, ColumnName = "original_invoice_number")]
        public string OriginalInvoiceNumber { get; set; }

        #region ZATCA E-Invoicing Fields - حقول الفوترة الإلكترونية زاتكا

        /// <summary>
        /// ZATCA UUID - المعرف الفريد العالمي لزاتكا
        /// </summary>
        [SugarColumn(IsNullable = true, Length = 36, ColumnName = "zatca_uuid")]
        public string ZatcaUuid { get; set; }

        /// <summary>
        /// ZATCA Invoice Type - نوع فاتورة زاتكا
        /// Credit Note (381) or Debit Note (383)
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "zatca_invoice_type")]
        public ZatcaInvoiceType? ZatcaInvoiceType { get; set; }

        /// <summary>
        /// ZATCA Invoice Sub Type - النوع الفرعي للفاتورة
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "zatca_invoice_sub_type")]
        public ZatcaInvoiceSubType? ZatcaInvoiceSubType { get; set; }

        /// <summary>
        /// ZATCA Submission Status - حالة الإرسال لزاتكا
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "zatca_status")]
        public ZatcaSubmissionStatus? ZatcaStatus { get; set; }

        /// <summary>
        /// ZATCA Invoice Hash ID - معرف هاش الفاتورة
        /// FK to zatca_invoice_hash table
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "zatca_hash_id")]
        public Guid? ZatcaHashId { get; set; }

        /// <summary>
        /// Invoice Counter Value (ICV) - قيمة عداد الفاتورة
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "zatca_counter")]
        public long? ZatcaCounter { get; set; }

        /// <summary>
        /// QR Code - رمز QR
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDataType = StaticConfig.CodeFirst_BigString, ColumnName = "zatca_qr_code")]
        public string ZatcaQrCode { get; set; }

        /// <summary>
        /// Invoice Hash - هاش الفاتورة
        /// </summary>
        [SugarColumn(IsNullable = true, Length = 100, ColumnName = "zatca_hash")]
        public string ZatcaHash { get; set; }

        /// <summary>
        /// ZATCA Submitted At - تاريخ الإرسال لزاتكا
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "zatca_submitted_at")]
        public DateTime? ZatcaSubmittedAt { get; set; }

        /// <summary>
        /// ZATCA Cleared/Reported At - تاريخ المقاصة/الإبلاغ
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "zatca_cleared_at")]
        public DateTime? ZatcaClearedAt { get; set; }

        /// <summary>
        /// ZATCA Instruction Note Reason Code - كود سبب الإشعار
        /// Required by ZATCA for credit/debit notes
        /// </summary>
        [SugarColumn(IsNullable = true, Length = 10, ColumnName = "zatca_instruction_note_reason")]
        public string ZatcaInstructionNoteReason { get; set; }

        #endregion
    }
}


