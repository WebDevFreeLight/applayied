using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MiniExcelLibs.Attributes;
using ZR.Model.Finance.ARAP.Enums;

namespace ZR.Model.Finance.ARAP.Dto
{
    /// <summary>
    /// AR Invoice Query DTO - استعلام فاتورة العملاء
    /// </summary>
    public class ARInvoiceQueryDto : PagerInfo
    {
        public Guid? Id { get; set; }
        public string InvoiceNumber { get; set; }
        public Guid? CustomerId { get; set; }
        public InvoiceStatus? Status { get; set; }
        public DateTime? BeginInvoiceDate { get; set; }
        public DateTime? EndInvoiceDate { get; set; }
        public DateTime? BeginDueDate { get; set; }
        public DateTime? EndDueDate { get; set; }
        public string Currency { get; set; }
        public long? TenantId { get; set; }
    }

    /// <summary>
    /// Create AR Invoice Line DTO - إنشاء سطر فاتورة عملاء
    /// </summary>
    public class CreateARInvoiceLineDto
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
    /// Update AR Invoice Line DTO - تحديث سطر فاتورة عملاء
    /// </summary>
    public class UpdateARInvoiceLineDto : CreateARInvoiceLineDto
    {
        public Guid? Id { get; set; }
    }

    /// <summary>
    /// Create AR Invoice DTO - إنشاء فاتورة عملاء جديدة
    /// </summary>
    public class CreateARInvoiceDto
    {
        [Required(ErrorMessage = "رقم الفاتورة مطلوب")]
        [StringLength(50, ErrorMessage = "رقم الفاتورة يجب ألا يتجاوز 50 حرف")]
        public string InvoiceNumber { get; set; }

        [Required(ErrorMessage = "معرف العميل مطلوب")]
        public Guid CustomerId { get; set; }

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
        public List<CreateARInvoiceLineDto> Lines { get; set; }

        // Calculated totals from frontend (for validation)
        public decimal Subtotal { get; set; }
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Skip credit limit validation (for contract-based invoices where the contract itself is the credit approval)
        /// </summary>
        public bool SkipCreditLimitCheck { get; set; } = false;

        public long? TenantId { get; set; }
    }

    /// <summary>
    /// Update AR Invoice DTO - تحديث فاتورة عملاء
    /// </summary>
    public class UpdateARInvoiceDto
    {
        [Required(ErrorMessage = "معرف الفاتورة مطلوب")]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "رقم الفاتورة مطلوب")]
        [StringLength(50, ErrorMessage = "رقم الفاتورة يجب ألا يتجاوز 50 حرف")]
        public string InvoiceNumber { get; set; }

        [Required(ErrorMessage = "معرف العميل مطلوب")]
        public Guid CustomerId { get; set; }

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
        public List<UpdateARInvoiceLineDto> Lines { get; set; }

        // Calculated totals from frontend (for validation)
        public decimal Subtotal { get; set; }
        public decimal TotalAmount { get; set; }

        public long? TenantId { get; set; }
    }

    /// <summary>
    /// AR Invoice Line Response DTO - استجابة سطر فاتورة العملاء
    /// </summary>
    public class ARInvoiceLineDto
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
    /// AR Invoice List DTO - قائمة فواتير العملاء
    /// </summary>
    public class ARInvoiceListDto
    {
        [ExcelColumn(Name = "Invoice ID")]
        public Guid Id { get; set; }

        [ExcelColumn(Name = "Invoice Number")]
        public string InvoiceNumber { get; set; }

        [ExcelColumn(Name = "Customer ID")]
        public Guid CustomerId { get; set; }

        [ExcelColumn(Name = "Customer Code")]
        public string CustomerCode { get; set; }

        [ExcelColumn(Name = "Customer Name")]
        public string CustomerName { get; set; }

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
    /// AR Invoice Detail DTO - تفاصيل فاتورة العملاء
    /// </summary>
    public class ARInvoiceDetailDto : ARInvoiceListDto
    {
        public string Notes { get; set; }
        public DateTime? Update_time { get; set; }
        public string Update_by { get; set; }

        /// <summary>
        /// Invoice Lines - بنود الفاتورة
        /// </summary>
        public List<ARInvoiceLineDto> Lines { get; set; }
    }

    /// <summary>
    /// Credit Limit Validation Result DTO - نتيجة التحقق من الحد الائتماني
    /// </summary>
    public class CreditLimitValidationDto
    {
        public bool IsValid { get; set; }
        public decimal CreditLimit { get; set; }
        public decimal CurrentBalance { get; set; }
        public decimal AvailableCredit { get; set; }
        public decimal InvoiceAmount { get; set; }
        public string Message { get; set; }
    }
}
