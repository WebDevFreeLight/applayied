using System;
using System.ComponentModel.DataAnnotations;
using MiniExcelLibs.Attributes;
using ZR.Model.Finance.ARAP.Enums;

namespace ZR.Model.Finance.ARAP.Dto
{
    /// <summary>
    /// Credit/Debit Note Query DTO - استعلام الإشعار
    /// </summary>
    public class CreditDebitNoteQueryDto : PagerInfo
    {
        public Guid? Id { get; set; }
        public string NoteNumber { get; set; }
        public NoteType? NoteType { get; set; }
        public Guid? CustomerId { get; set; }
        public Guid? VendorId { get; set; }
        public DateTime? BeginNoteDate { get; set; }
        public DateTime? EndNoteDate { get; set; }
        public long? TenantId { get; set; }

        /// <summary>
        /// Company ID for filtering
        /// معرف الشركة
        /// </summary>
        public long? CompanyId { get; set; }
    }

    /// <summary>
    /// Create Credit/Debit Note DTO - إنشاء إشعار جديد
    /// </summary>
    public class CreateCreditDebitNoteDto
    {
        [Required(ErrorMessage = "رقم الإشعار مطلوب")]
        [StringLength(50, ErrorMessage = "رقم الإشعار يجب ألا يتجاوز 50 حرف")]
        public string NoteNumber { get; set; }

        [Required(ErrorMessage = "نوع الإشعار مطلوب")]
        public NoteType NoteType { get; set; }

        public Guid? CustomerId { get; set; }

        public Guid? VendorId { get; set; }

        [Required(ErrorMessage = "المبلغ مطلوب")]
        [Range(0.01, double.MaxValue, ErrorMessage = "المبلغ يجب أن يكون أكبر من صفر")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "تاريخ الإشعار مطلوب")]
        public DateTime NoteDate { get; set; }

        [StringLength(500)]
        public string Reason { get; set; }

        public Guid? GLJournalId { get; set; }

        public long? TenantId { get; set; }
    }

    /// <summary>
    /// Credit/Debit Note List DTO - قائمة الإشعارات
    /// </summary>
    public class CreditDebitNoteListDto
    {
        [ExcelColumn(Name = "Note ID")]
        public Guid Id { get; set; }

        [ExcelColumn(Name = "Note Number")]
        public string NoteNumber { get; set; }

        [ExcelColumn(Name = "Note Type")]
        public NoteType NoteType { get; set; }

        [ExcelColumn(Name = "Customer ID")]
        public Guid? CustomerId { get; set; }

        [ExcelColumn(Name = "Customer Name")]
        public string CustomerName { get; set; }

        [ExcelColumn(Name = "Vendor ID")]
        public Guid? VendorId { get; set; }

        [ExcelColumn(Name = "Vendor Name")]
        public string VendorName { get; set; }

        [ExcelColumn(Name = "Amount")]
        public decimal Amount { get; set; }

        [ExcelColumn(Name = "Note Date")]
        public DateTime NoteDate { get; set; }

        [ExcelColumn(Name = "Reason")]
        public string Reason { get; set; }

        public Guid? GLJournalId { get; set; }
        public long? TenantId { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// Credit/Debit Note Detail DTO - تفاصيل الإشعار
    /// </summary>
    public class CreditDebitNoteDetailDto : CreditDebitNoteListDto
    {
        public string CreatedBy { get; set; }
    }
}


