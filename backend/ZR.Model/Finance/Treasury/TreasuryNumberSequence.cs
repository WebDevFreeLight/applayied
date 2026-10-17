using System;
using SqlSugar;
using ZR.Model.System;

namespace ZR.Model.Finance.Treasury
{
    /// <summary>
    /// Treasury Number Sequence - تسلسل أرقام الخزينة
    /// For atomic generation of payment/cheque/transfer numbers (requirement #25)
    /// </summary>
    [SugarTable("treasury_number_sequence")]
    public class TreasuryNumberSequence : SysBase
    {
        /// <summary>
        /// Override BranchID from SysBase - not used
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public new long? BranchID { get; set; }

        /// <summary>
        /// Sequence ID - معرف التسلسل
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false, ColumnName = "id")]
        public Guid Id { get; set; }

        /// <summary>
        /// Sequence Type - نوع التسلسل (e.g., "PAY", "RCP", "TRF", "CHQ")
        /// </summary>
        [SugarColumn(Length = 20, ColumnName = "sequence_type")]
        public string SequenceType { get; set; }

        // Note: CompanyID and BranchID are inherited from SysBase - do not duplicate

        /// <summary>
        /// Fiscal Year - السنة المالية
        /// </summary>
        [SugarColumn(ColumnName = "fiscal_year")]
        public int FiscalYear { get; set; }

        /// <summary>
        /// Prefix - البادئة
        /// </summary>
        [SugarColumn(Length = 20, ColumnName = "prefix")]
        public string Prefix { get; set; }

        /// <summary>
        /// Suffix - اللاحقة
        /// </summary>
        [SugarColumn(Length = 20, IsNullable = true, ColumnName = "suffix")]
        public string Suffix { get; set; }

        /// <summary>
        /// Current Number - الرقم الحالي
        /// </summary>
        [SugarColumn(ColumnName = "current_number")]
        public long CurrentNumber { get; set; }

        /// <summary>
        /// Increment By - مقدار الزيادة
        /// </summary>
        [SugarColumn(ColumnName = "increment_by")]
        public int IncrementBy { get; set; } = 1;

        /// <summary>
        /// Minimum Digits - أقل عدد من الأرقام
        /// </summary>
        [SugarColumn(ColumnName = "min_digits")]
        public int MinDigits { get; set; } = 6;

        /// <summary>
        /// Reset On New Year - إعادة تعيين في سنة جديدة
        /// </summary>
        [SugarColumn(ColumnName = "reset_on_new_year")]
        public bool ResetOnNewYear { get; set; } = true;

        /// <summary>
        /// Last Used At - آخر استخدام
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "last_used_at")]
        public DateTime? LastUsedAt { get; set; }

        /// <summary>
        /// Is Active - نشط
        /// </summary>
        [SugarColumn(ColumnName = "is_active")]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Row Version - نسخة الصف (للتزامن المتفائل)
        /// </summary>
        [SugarColumn(ColumnName = "row_version", IsEnableUpdateVersionValidation = true)]
        public int RowVersion { get; set; }
    }
}
