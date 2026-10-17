using System;
using SqlSugar;

namespace ZR.Model.Finance.ARAP
{
    /// <summary>
    /// Invoice Number Sequence - تسلسل أرقام الفواتير
    /// Atomic counter for thread-safe invoice number generation
    /// عداد ذري لتوليد أرقام الفواتير بأمان للخيوط
    /// </summary>
    [SugarTable("arap_invoice_number_sequence")]
    [SugarIndex("uk_invoice_seq", nameof(SequenceType), OrderByType.Asc, nameof(CompanyId), OrderByType.Asc, nameof(FiscalYear), OrderByType.Asc, true)]
    public class InvoiceNumberSequence
    {
        /// <summary>
        /// Sequence ID - معرف التسلسل
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false, ColumnName = "id")]
        public Guid Id { get; set; }

        /// <summary>
        /// Sequence Type - نوع التسلسل
        /// AR = Accounts Receivable, AP = Accounts Payable
        /// </summary>
        [SugarColumn(Length = 10, IsNullable = false, ColumnName = "sequence_type")]
        public string SequenceType { get; set; }

        /// <summary>
        /// Company ID - معرف الشركة
        /// For multi-tenant support
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "company_id")]
        public long? CompanyId { get; set; }

        /// <summary>
        /// Fiscal Year - السنة المالية
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "fiscal_year")]
        public int FiscalYear { get; set; }

        /// <summary>
        /// Current Number - الرقم الحالي
        /// Last used number in the sequence
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "current_number")]
        public long CurrentNumber { get; set; } = 0;

        /// <summary>
        /// Minimum Digits - الحد الأدنى للأرقام
        /// For zero-padding (e.g., 5 = 00001)
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "min_digits")]
        public int MinDigits { get; set; } = 5;

        /// <summary>
        /// Row Version - إصدار السجل
        /// For optimistic concurrency control
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "row_version")]
        public int RowVersion { get; set; } = 1;

        /// <summary>
        /// Create Time - وقت الإنشاء
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "create_time")]
        public DateTime? CreateTime { get; set; }

        /// <summary>
        /// Update Time - وقت التحديث
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "update_time")]
        public DateTime? UpdateTime { get; set; }

        #region Helper Methods

        /// <summary>
        /// Format the current number with the specified minimum digits
        /// تنسيق الرقم الحالي بالحد الأدنى من الأرقام المحددة
        /// </summary>
        /// <returns>Formatted number string (e.g., "00001")</returns>
        public string FormatCurrentNumber()
        {
            return CurrentNumber.ToString($"D{MinDigits}");
        }

        /// <summary>
        /// Generate invoice number prefix based on sequence type and fiscal year
        /// توليد بادئة رقم الفاتورة بناءً على نوع التسلسل والسنة المالية
        /// </summary>
        /// <returns>Invoice number (e.g., "AR-INV-2025-00001")</returns>
        public string GenerateInvoiceNumber()
        {
            return $"{SequenceType}-INV-{FiscalYear}-{FormatCurrentNumber()}";
        }

        /// <summary>
        /// Create a new sequence for the given parameters
        /// إنشاء تسلسل جديد للمعلمات المحددة
        /// </summary>
        public static InvoiceNumberSequence Create(string sequenceType, int fiscalYear, long? companyId = null)
        {
            return new InvoiceNumberSequence
            {
                Id = Guid.NewGuid(),
                SequenceType = sequenceType,
                FiscalYear = fiscalYear,
                CompanyId = companyId,
                CurrentNumber = 0,
                MinDigits = 5,
                RowVersion = 1,
                CreateTime = DateTime.Now
            };
        }

        #endregion
    }

    /// <summary>
    /// Sequence Type Constants - ثوابت نوع التسلسل
    /// </summary>
    public static class InvoiceSequenceType
    {
        public const string AR = "AR";
        public const string AP = "AP";
        public const string CreditNote = "CN";
        public const string DebitNote = "DN";
    }
}
