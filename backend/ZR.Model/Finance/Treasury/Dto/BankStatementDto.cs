using System;
using System.ComponentModel.DataAnnotations;
using ZR.Model;
using ZR.Model.Finance.Treasury.Enums;

namespace ZR.Model.Finance.Treasury.Dto
{
    /// <summary>
    /// Bank Statement Query DTO - استعلام كشف حساب بنكي
    /// </summary>
    public class BankStatementQueryDto : PagerInfo
    {
        public Guid? Id { get; set; }
        public Guid? BankAccountId { get; set; }
        public DateTime? StatementDateFrom { get; set; }
        public DateTime? StatementDateTo { get; set; }
        public bool? Imported { get; set; }
        public long? TenantId { get; set; }
    }

    /// <summary>
    /// Create Bank Statement DTO - إنشاء كشف حساب بنكي
    /// </summary>
    public class CreateBankStatementDto
    {
        [Required(ErrorMessage = "معرف الحساب البنكي مطلوب")]
        public Guid BankAccountId { get; set; }

        [Required(ErrorMessage = "تاريخ الكشف مطلوب")]
        public DateTime StatementDate { get; set; }

        [Required(ErrorMessage = "مرجع الكشف مطلوب")]
        [StringLength(200)]
        public string StatementReference { get; set; }

        [Required(ErrorMessage = "الرصيد الافتتاحي مطلوب")]
        public decimal OpeningBalance { get; set; }

        [Required(ErrorMessage = "الرصيد الختامي مطلوب")]
        public decimal ClosingBalance { get; set; }

        public BankStatementImportSource ImportSource { get; set; } = BankStatementImportSource.Manual;

        [StringLength(1000)]
        public string Notes { get; set; }

        public long? TenantId { get; set; }
    }

    /// <summary>
    /// Upload Bank Statement DTO - رفع كشف حساب بنكي
    /// </summary>
    public class UploadBankStatementDto
    {
        [Required(ErrorMessage = "معرف الحساب البنكي مطلوب")]
        public Guid BankAccountId { get; set; }

        [Required(ErrorMessage = "اسم الملف مطلوب")]
        public string FileName { get; set; }

        [Required(ErrorMessage = "تنسيق الملف مطلوب")]
        public BankStatementFormat ImportFormat { get; set; }

        /// <summary>
        /// Optional parsing profile ID for bank-specific parsing rules
        /// </summary>
        public Guid? ParsingProfileId { get; set; }

        [StringLength(1000)]
        public string Notes { get; set; }

        public long? TenantId { get; set; }
    }

    /// <summary>
    /// Bank Statement Detail DTO - تفاصيل كشف حساب بنكي
    /// </summary>
    public class BankStatementDetailDto
    {
        public Guid Id { get; set; }
        public Guid BankAccountId { get; set; }
        public string BankAccountName { get; set; }
        public DateTime StatementDate { get; set; }
        public string StatementReference { get; set; }
        public decimal OpeningBalance { get; set; }
        public decimal ClosingBalance { get; set; }
        public bool Imported { get; set; }
        public BankStatementImportSource ImportSource { get; set; }
        public string ImportSourceName => ImportSource switch
        {
            BankStatementImportSource.Manual => "Manual - يدوي",
            BankStatementImportSource.CSV => "CSV",
            BankStatementImportSource.MT940 => "MT940",
            BankStatementImportSource.API => "API - واجهة برمجية",
            _ => ImportSource.ToString()
        };
        public string Notes { get; set; }
        public long? TenantId { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid? CreatedBy { get; set; }
    }

    /// <summary>
    /// Bank Statement Line DTO - بند كشف حساب بنكي
    /// </summary>
    public class BankStatementLineDto
    {
        public Guid Id { get; set; }
        public Guid BankStatementId { get; set; }
        public DateTime LineDate { get; set; }
        public DateTime ValueDate { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public decimal RunningBalance { get; set; }
        public string ReferenceNo { get; set; }
        public bool Matched { get; set; }
        public Guid? MatchedTransactionId { get; set; }
        public string MatchedTransactionNumber { get; set; }
    }

    /// <summary>
    /// Match Bank Statement Line DTO - مطابقة بند كشف حساب
    /// </summary>
    public class MatchBankStatementLineDto
    {
        [Required(ErrorMessage = "معرف بند كشف الحساب مطلوب")]
        public Guid BankStatementLineId { get; set; }

        [Required(ErrorMessage = "معرف المعاملة المطابقة مطلوب")]
        public Guid MatchedTransactionId { get; set; }
    }
}






























