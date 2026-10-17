using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MiniExcelLibs.Attributes;

namespace ZR.Model.Finance.GL.Dto
{
    /// <summary>
    /// Journal Entry Query DTO - استعلام قيد اليومية
    /// </summary>
    public class GLJournalEntryQueryDto : PagerInfo
    {
        public Guid? Id { get; set; }
        public Guid? JournalId { get; set; }
        public Guid? AccountId { get; set; }
        public decimal? MinDebit { get; set; }
        public decimal? MaxDebit { get; set; }
        public decimal? MinCredit { get; set; }
        public decimal? MaxCredit { get; set; }
        public Guid? CostCenterId { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? ProjectId { get; set; }
        public string Currency { get; set; }
        public DateTime? BeginCreateTime { get; set; }
        public DateTime? EndCreateTime { get; set; }
    }

    /// <summary>
    /// Create Journal Entry DTO - إنشاء قيد يومية جديد
    /// </summary>
    public class CreateJournalEntryDto
    {
        public Guid? JournalId { get; set; }

        [Required(ErrorMessage = "معرف الحساب مطلوب")]
        public Guid AccountId { get; set; }

        public int LineNumber { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "المبلغ المدين يجب أن يكون قيمة موجبة")]
        public decimal Debit { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "المبلغ الدائن يجب أن يكون قيمة موجبة")]
        public decimal Credit { get; set; }

        [StringLength(500)]
        public string DescriptionAr { get; set; }

        [StringLength(500)]
        public string DescriptionEn { get; set; }

        public Guid? CostCenterId { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? ProjectId { get; set; }

        [StringLength(100)]
        public string ReferenceNumber { get; set; }

        [StringLength(10)]
        public string Currency { get; set; } = "SAR";

        [Range(0.000001, double.MaxValue, ErrorMessage = "سعر الصرف يجب أن يكون قيمة موجبة")]
        public decimal ExchangeRate { get; set; } = 1;

        public long? TenantId { get; set; }
        public long? CompanyId { get; set; }
    }

    /// <summary>
    /// Update Journal Entry DTO - تحديث قيد يومية
    /// </summary>
    public class UpdateJournalEntryDto
    {
        public Guid? Id { get; set; }

        public Guid? JournalId { get; set; }

        [Required(ErrorMessage = "معرف الحساب مطلوب")]
        public Guid AccountId { get; set; }

        public int LineNumber { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "المبلغ المدين يجب أن يكون قيمة موجبة")]
        public decimal Debit { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "المبلغ الدائن يجب أن يكون قيمة موجبة")]
        public decimal Credit { get; set; }

        [StringLength(500)]
        public string DescriptionAr { get; set; }

        [StringLength(500)]
        public string DescriptionEn { get; set; }

        public Guid? CostCenterId { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? ProjectId { get; set; }

        [StringLength(100)]
        public string ReferenceNumber { get; set; }

        [StringLength(10)]
        public string Currency { get; set; }

        [Range(0.000001, double.MaxValue, ErrorMessage = "سعر الصرف يجب أن يكون قيمة موجبة")]
        public decimal ExchangeRate { get; set; } = 1;

        public long? TenantId { get; set; }
        public long? CompanyId { get; set; }
    }

    /// <summary>
    /// Journal Entry Response DTO - استجابة قيد اليومية
    /// </summary>
    public class JournalEntryResponseDto
    {
        [ExcelColumn(Name = "Entry ID")]
        public Guid Id { get; set; }

        [ExcelColumn(Name = "Journal ID")]
        public Guid JournalId { get; set; }

        [ExcelColumn(Name = "Account ID")]
        public Guid AccountId { get; set; }

        [ExcelColumn(Name = "Line Number")]
        public int LineNumber { get; set; }

        [ExcelColumn(Name = "Debit")]
        public decimal Debit { get; set; }

        [ExcelColumn(Name = "Credit")]
        public decimal Credit { get; set; }

        [ExcelColumn(Name = "Description (Arabic)")]
        public string DescriptionAr { get; set; }

        [ExcelColumn(Name = "Description (English)")]
        public string DescriptionEn { get; set; }

        [ExcelColumn(Name = "Cost Center ID")]
        public Guid? CostCenterId { get; set; }

        [ExcelColumn(Name = "Department ID")]
        public Guid? DepartmentId { get; set; }

        [ExcelColumn(Name = "Project ID")]
        public Guid? ProjectId { get; set; }

        [ExcelColumn(Name = "Reference Number")]
        public string ReferenceNumber { get; set; }

        [ExcelColumn(Name = "Currency")]
        public string Currency { get; set; }

        [ExcelColumn(Name = "Exchange Rate")]
        public decimal ExchangeRate { get; set; }

        [ExcelColumn(Name = "Base Debit")]
        public decimal BaseDebit { get; set; }

        [ExcelColumn(Name = "Base Credit")]
        public decimal BaseCredit { get; set; }

        public DateTime? Create_time { get; set; }
        public DateTime? Update_time { get; set; }
        public string Create_by { get; set; }
        public string Update_by { get; set; }
        public long? CompanyID { get; set; }

        /// <summary>
        /// Account Name (Arabic) - اسم الحساب بالعربي
        /// </summary>
        [ExcelColumn(Name = "Account Name (Arabic)")]
        public string AccountNameAr { get; set; }

        /// <summary>
        /// Account Name (English) - اسم الحساب بالإنجليزي
        /// </summary>
        [ExcelColumn(Name = "Account Name (English)")]
        public string AccountNameEn { get; set; }

        /// <summary>
        /// Account Code - رمز الحساب
        /// </summary>
        [ExcelColumn(Name = "Account Code")]
        public string AccountCode { get; set; }

        /// <summary>
        /// Journal Number - رقم دفتر اليومية
        /// </summary>
        [ExcelColumn(Name = "Journal Number")]
        public string JournalNumber { get; set; }
    }
}
