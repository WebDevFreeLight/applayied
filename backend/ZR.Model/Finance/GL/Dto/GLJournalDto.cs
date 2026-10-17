using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MiniExcelLibs.Attributes;
using ZR.Model.Finance.Enums;

namespace ZR.Model.Finance.GL.Dto
{
    /// <summary>
    /// Journal Query DTO - استعلام دفتر اليومية
    /// </summary>
    public class GLJournalQueryDto : PagerInfo
    {
        public Guid? Id { get; set; }
        public string JournalNumber { get; set; }
        public JournalStatus? Status { get; set; }
        public string Source { get; set; }
        public DateTime? BeginJournalDate { get; set; }
        public DateTime? EndJournalDate { get; set; }
        public DateTime? BeginCreateTime { get; set; }
        public DateTime? EndCreateTime { get; set; }
        public int? FiscalYear { get; set; }
        public int? FiscalPeriod { get; set; }
        public string SourceDocumentType { get; set; }
        public Guid? SourceDocumentId { get; set; }
    }

    /// <summary>
    /// Create Journal DTO - إنشاء قيد يومية جديد
    /// </summary>
    public class CreateJournalDto
    {
        [StringLength(500)]
        public string DescriptionAr { get; set; }

        [StringLength(500)]
        public string DescriptionEn { get; set; }

        [Required(ErrorMessage = "تاريخ القيد مطلوب")]
        public DateTime JournalDate { get; set; }

        [StringLength(50)]
        public string Source { get; set; }

        [StringLength(50)]
        public string SourceDocumentType { get; set; }

        public Guid? SourceDocumentId { get; set; }

        [StringLength(10)]
        public string Currency { get; set; } = "SAR";

        [StringLength(100)]
        public string ReferenceNumber { get; set; }

        public int? FiscalYear { get; set; }
        public int? FiscalPeriod { get; set; }

        public long? TenantId { get; set; }
        public long? CompanyId { get; set; }

        /// <summary>
        /// Journal Entries - قيود دفتر اليومية
        /// </summary>
        public List<CreateJournalEntryDto> Entries { get; set; }
    }

    /// <summary>
    /// Update Journal DTO - تحديث قيد يومية
    /// </summary>
    public class UpdateJournalDto
    {
        [Required(ErrorMessage = "معرف القيد مطلوب")]
        public Guid Id { get; set; }

        [StringLength(500)]
        public string DescriptionAr { get; set; }

        [StringLength(500)]
        public string DescriptionEn { get; set; }

        [Required(ErrorMessage = "تاريخ القيد مطلوب")]
        public DateTime JournalDate { get; set; }

        [StringLength(50)]
        public string Source { get; set; }

        [StringLength(50)]
        public string SourceDocumentType { get; set; }

        public Guid? SourceDocumentId { get; set; }

        [StringLength(10)]
        public string Currency { get; set; }

        [StringLength(100)]
        public string ReferenceNumber { get; set; }

        public int? FiscalYear { get; set; }
        public int? FiscalPeriod { get; set; }

        public long? TenantId { get; set; }
        public long? CompanyId { get; set; }

        /// <summary>
        /// Journal Entries - قيود دفتر اليومية
        /// </summary>
        public List<UpdateJournalEntryDto> Entries { get; set; }
    }

    /// <summary>
    /// Post Journal DTO - ترحيل قيد يومية
    /// </summary>
    public class PostJournalDto
    {
        [Required(ErrorMessage = "معرف القيد مطلوب")]
        public Guid Id { get; set; }
    }

    /// <summary>
    /// Reverse Journal DTO - عكس قيد يومية
    /// </summary>
    public class ReverseJournalDto
    {
        [Required(ErrorMessage = "معرف القيد مطلوب")]
        public Guid Id { get; set; }

        public DateTime? ReversalDate { get; set; }

        [StringLength(500)]
        public string ReversalReason { get; set; }
    }

    /// <summary>
    /// Journal Response DTO - استجابة دفتر اليومية
    /// </summary>
    public class JournalResponseDto
    {
        [ExcelColumn(Name = "Journal ID")]
        public Guid Id { get; set; }

        [ExcelColumn(Name = "Journal Number")]
        public string JournalNumber { get; set; }

        [ExcelColumn(Name = "Description (Arabic)")]
        public string DescriptionAr { get; set; }

        [ExcelColumn(Name = "Description (English)")]
        public string DescriptionEn { get; set; }

        [ExcelColumn(Name = "Journal Date", Format = "yyyy-MM-dd")]
        public DateTime JournalDate { get; set; }

        [ExcelColumn(Name = "Source")]
        public string Source { get; set; }

        [ExcelColumn(Name = "Source Document Type")]
        public string SourceDocumentType { get; set; }

        public Guid? SourceDocumentId { get; set; }

        [ExcelColumn(Name = "Status")]
        public JournalStatus Status { get; set; }

        [ExcelColumn(Name = "Currency")]
        public string Currency { get; set; }

        [ExcelColumn(Name = "Reference Number")]
        public string ReferenceNumber { get; set; }

        /// <summary>
        /// Total Debit - إجمالي المدين
        /// </summary>
        [ExcelColumn(Name = "Total Debit")]
        public decimal TotalDebit { get; set; }

        /// <summary>
        /// Total Credit - إجمالي الدائن
        /// </summary>
        [ExcelColumn(Name = "Total Credit")]
        public decimal TotalCredit { get; set; }

        [ExcelColumn(Name = "Is Balanced")]
        public bool IsBalanced { get; set; }

        [ExcelColumn(Name = "Fiscal Year")]
        public int? FiscalYear { get; set; }

        [ExcelColumn(Name = "Fiscal Period")]
        public int? FiscalPeriod { get; set; }

        [ExcelColumn(Name = "Posted Date", Format = "yyyy-MM-dd HH:mm")]
        public DateTime? PostedDate { get; set; }

        public Guid? PostedBy { get; set; }

        [ExcelColumn(Name = "Approved Date", Format = "yyyy-MM-dd HH:mm")]
        public DateTime? ApprovedDate { get; set; }

        public Guid? ApprovedBy { get; set; }

        public Guid? ReversalJournalId { get; set; }
        public Guid? OriginalJournalId { get; set; }

        public DateTime? Create_time { get; set; }
        public DateTime? Update_time { get; set; }
        public string Create_by { get; set; }
        public string Update_by { get; set; }
        public long? CompanyID { get; set; }

        /// <summary>
        /// Journal Entries - قيود دفتر اليومية
        /// </summary>
        public List<JournalEntryResponseDto> Entries { get; set; }
    }

    /// <summary>
    /// Journal Validation Result - نتيجة تحقق القيد
    /// </summary>
    public class JournalValidationResult
    {
        public bool IsValid { get; set; }
        public string Message { get; set; }
        public decimal TotalDebit { get; set; }
        public decimal TotalCredit { get; set; }
        public decimal Difference { get; set; }
    }

    /// <summary>
    /// Journal Totals - مجاميع القيد
    /// </summary>
    public class JournalTotals
    {
        public decimal TotalDebit { get; set; }
        public decimal TotalCredit { get; set; }
        public bool IsBalanced { get; set; }
    }
}
