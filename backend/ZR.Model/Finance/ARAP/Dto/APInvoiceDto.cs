using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MiniExcelLibs.Attributes;
using ZR.Model.Finance.ARAP.Enums;

namespace ZR.Model.Finance.ARAP.Dto
{
    /// <summary>
    /// AP Invoice Query DTO - استعلام فاتورة الموردين
    /// </summary>
    public class APInvoiceQueryDto : PagerInfo
    {
        public Guid? Id { get; set; }
        public string InvoiceNumber { get; set; }
        public Guid? VendorId { get; set; }
        public InvoiceStatus? Status { get; set; }
        public DateTime? BeginInvoiceDate { get; set; }
        public DateTime? EndInvoiceDate { get; set; }
        public DateTime? BeginDueDate { get; set; }
        public DateTime? EndDueDate { get; set; }
        public string Currency { get; set; }
        public long? TenantId { get; set; }
    }

    /// <summary>
    /// Create AP Invoice Line DTO - إنشاء سطر فاتورة موردين
    /// </summary>
    public class CreateAPInvoiceLineDto
    {
        [Required(ErrorMessage = "معرف الحساب مطلوب")]
        public Guid AccountId { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [Range(0.0001, double.MaxValue, ErrorMessage = "الكمية يجب أن تكون أكبر من صفر")]
        public decimal Quantity { get; set; } = 1;

        [Range(0.01, double.MaxValue, ErrorMessage = "سعر الوحدة يجب أن يكون أكبر من صفر")]
        public decimal UnitPrice { get; set; }

        public Guid? TaxCodeId { get; set; }

        public Guid? CostCenterId { get; set; }

        public Guid? DepartmentId { get; set; }

        public Guid? ProjectId { get; set; }
    }

    /// <summary>
    /// Update AP Invoice Line DTO - تحديث سطر فاتورة موردين
    /// </summary>
    public class UpdateAPInvoiceLineDto : CreateAPInvoiceLineDto
    {
        public Guid? Id { get; set; }
    }

    /// <summary>
    /// Create AP Invoice DTO - إنشاء فاتورة موردين جديدة
    /// </summary>
    public class CreateAPInvoiceDto
    {
        [Required(ErrorMessage = "رقم الفاتورة مطلوب")]
        [StringLength(50, ErrorMessage = "رقم الفاتورة يجب ألا يتجاوز 50 حرف")]
        public string InvoiceNumber { get; set; }

        [Required(ErrorMessage = "معرف المورد مطلوب")]
        public Guid VendorId { get; set; }

        [Required(ErrorMessage = "تاريخ الفاتورة مطلوب")]
        public DateTime InvoiceDate { get; set; }

        [Required(ErrorMessage = "تاريخ الاستحقاق مطلوب")]
        public DateTime DueDate { get; set; }

        [StringLength(10)]
        public string Currency { get; set; } = "SAR";

        [Range(0.000001, double.MaxValue)]
        public decimal ExchangeRate { get; set; } = 1;

        public Guid? TaxGroupId { get; set; }

        /// <summary>
        /// Is Tax Inclusive - هل الضريبة مشمولة
        /// If true, line amounts include tax; if false, tax is added on top
        /// </summary>
        public bool IsTaxInclusive { get; set; } = false;

        [StringLength(500)]
        public string Description { get; set; }

        [StringLength(100)]
        public string ReferenceNumber { get; set; }

        [StringLength(1000)]
        public string Notes { get; set; }

        /// <summary>
        /// Invoice Lines - بنود الفاتورة
        /// </summary>
        [Required(ErrorMessage = "يجب إضافة سطر واحد على الأقل")]
        [MinLength(1, ErrorMessage = "يجب إضافة سطر واحد على الأقل")]
        public List<CreateAPInvoiceLineDto> Lines { get; set; }

        // Calculated totals from frontend (for validation)
        public decimal Subtotal { get; set; }
        public decimal TotalAmount { get; set; }

        public long? TenantId { get; set; }
    }

    /// <summary>
    /// Update AP Invoice DTO - تحديث فاتورة موردين
    /// </summary>
    public class UpdateAPInvoiceDto
    {
        [Required(ErrorMessage = "معرف الفاتورة مطلوب")]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "رقم الفاتورة مطلوب")]
        [StringLength(50, ErrorMessage = "رقم الفاتورة يجب ألا يتجاوز 50 حرف")]
        public string InvoiceNumber { get; set; }

        [Required(ErrorMessage = "معرف المورد مطلوب")]
        public Guid VendorId { get; set; }

        [Required(ErrorMessage = "تاريخ الفاتورة مطلوب")]
        public DateTime InvoiceDate { get; set; }

        [Required(ErrorMessage = "تاريخ الاستحقاق مطلوب")]
        public DateTime DueDate { get; set; }

        [StringLength(10)]
        public string Currency { get; set; } = "SAR";

        [Range(0.000001, double.MaxValue)]
        public decimal ExchangeRate { get; set; } = 1;

        public Guid? TaxGroupId { get; set; }

        /// <summary>
        /// Is Tax Inclusive - هل الضريبة مشمولة
        /// If true, line amounts include tax; if false, tax is added on top
        /// </summary>
        public bool IsTaxInclusive { get; set; } = false;

        [StringLength(500)]
        public string Description { get; set; }

        [StringLength(100)]
        public string ReferenceNumber { get; set; }

        [StringLength(1000)]
        public string Notes { get; set; }

        /// <summary>
        /// Invoice Lines - بنود الفاتورة
        /// </summary>
        [Required(ErrorMessage = "يجب إضافة سطر واحد على الأقل")]
        [MinLength(1, ErrorMessage = "يجب إضافة سطر واحد على الأقل")]
        public List<UpdateAPInvoiceLineDto> Lines { get; set; }

        // Calculated totals from frontend (for validation)
        public decimal Subtotal { get; set; }
        public decimal TotalAmount { get; set; }

        public long? TenantId { get; set; }
    }

    /// <summary>
    /// AP Invoice Line Response DTO - استجابة سطر فاتورة الموردين
    /// </summary>
    public class APInvoiceLineDto
    {
        public Guid Id { get; set; }
        public Guid InvoiceId { get; set; }
        public int LineNumber { get; set; }
        public Guid AccountId { get; set; }
        public string AccountCode { get; set; }
        public string AccountName { get; set; }
        public string Description { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Amount { get; set; }
        public Guid? TaxCodeId { get; set; }
        public string TaxCodeCode { get; set; }
        public decimal TaxRate { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal? TaxableAmount { get; set; }
        public decimal? TaxInclusiveAmount { get; set; }
        public Guid? CostCenterId { get; set; }
        public string CostCenterName { get; set; }
        public Guid? DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public Guid? ProjectId { get; set; }
        public string ProjectName { get; set; }
    }

    /// <summary>
    /// AP Invoice List DTO - قائمة فواتير الموردين
    /// </summary>
    public class APInvoiceListDto
    {
        [ExcelColumn(Name = "Invoice ID")]
        public Guid Id { get; set; }

        [ExcelColumn(Name = "Invoice Number")]
        public string InvoiceNumber { get; set; }

        [ExcelColumn(Name = "Vendor ID")]
        public Guid VendorId { get; set; }

        [ExcelColumn(Name = "Vendor Code")]
        public string VendorCode { get; set; }

        [ExcelColumn(Name = "Vendor Name")]
        public string VendorName { get; set; }

        [ExcelColumn(Name = "Invoice Date")]
        public DateTime InvoiceDate { get; set; }

        [ExcelColumn(Name = "Due Date")]
        public DateTime DueDate { get; set; }

        [ExcelColumn(Name = "Currency")]
        public string Currency { get; set; }

        [ExcelColumn(Name = "Exchange Rate")]
        public decimal ExchangeRate { get; set; }

        [ExcelColumn(Name = "Subtotal")]
        public decimal Subtotal { get; set; }

        [ExcelColumn(Name = "Tax Amount")]
        public decimal TaxAmount { get; set; }

        [ExcelColumn(Name = "Total Amount")]
        public decimal TotalAmount { get; set; }

        [ExcelColumn(Name = "Paid Amount")]
        public decimal PaidAmount { get; set; }

        [ExcelColumn(Name = "Remaining Amount")]
        public decimal RemainingAmount { get; set; }

        [ExcelColumn(Name = "Status")]
        public InvoiceStatus Status { get; set; }

        public Guid? TaxGroupId { get; set; }
        public string TaxGroupCode { get; set; }
        public bool IsTaxInclusive { get; set; }

        public string Description { get; set; }
        public string ReferenceNumber { get; set; }

        public Guid? GLJournalId { get; set; }
        public Guid? FiscalYearId { get; set; }
        public Guid? FiscalPeriodId { get; set; }

        public DateTime? Create_time { get; set; }
        public string Create_by { get; set; }
    }

    /// <summary>
    /// AP Invoice Detail DTO - تفاصيل فاتورة الموردين
    /// </summary>
    public class APInvoiceDetailDto : APInvoiceListDto
    {
        public string Notes { get; set; }
        public DateTime? Update_time { get; set; }
        public string Update_by { get; set; }

        /// <summary>
        /// Invoice Lines - بنود الفاتورة
        /// </summary>
        public List<APInvoiceLineDto> Lines { get; set; }
    }

    /// <summary>
    /// Invoice Validation Result DTO - نتيجة التحقق من الفاتورة
    /// </summary>
    public class InvoiceValidationResultDto
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public List<string> Warnings { get; set; } = new List<string>();
        public PeriodValidationDto PeriodValidation { get; set; }
        public BalanceCheckDto BalanceCheck { get; set; }
    }

    /// <summary>
    /// Period Validation DTO - نتيجة التحقق من الفترة
    /// </summary>
    public class PeriodValidationDto
    {
        public bool IsValid { get; set; }
        public bool IsClosed { get; set; }
        public bool IsLocked { get; set; }
        public string PeriodName { get; set; }
        public string Message { get; set; }
    }

    /// <summary>
    /// Balance Check DTO - نتيجة التحقق من التوازن
    /// </summary>
    public class BalanceCheckDto
    {
        public bool IsBalanced { get; set; }
        public decimal TotalDebit { get; set; }
        public decimal TotalCredit { get; set; }
        public decimal Difference { get; set; }
    }

    /// <summary>
    /// Invoice Post Result DTO - نتيجة ترحيل الفاتورة
    /// </summary>
    public class InvoicePostResultDto
    {
        public Guid InvoiceId { get; set; }
        public Guid JournalId { get; set; }
        public string JournalNumber { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
    }

    /// <summary>
    /// Unpaid Invoice DTO - الفواتير غير المدفوعة
    /// </summary>
    public class UnpaidInvoiceDto
    {
        public Guid Id { get; set; }
        public string InvoiceNumber { get; set; }
        public DateTime InvoiceDate { get; set; }
        public DateTime DueDate { get; set; }
        public string Currency { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public int DaysOverdue { get; set; }
    }
}


