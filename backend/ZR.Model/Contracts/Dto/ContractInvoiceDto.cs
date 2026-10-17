using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MiniExcelLibs.Attributes;
using ZR.Model.Contracts.Enums;

namespace ZR.Model.Contracts.Dto
{
    #region Invoice Generation DTOs

    /// <summary>
    /// Create Invoice From Schedule Request DTO - طلب إنشاء فاتورة من الجدول
    /// Request to generate an AR Invoice from a billing schedule
    /// </summary>
    public class CreateInvoiceFromScheduleRequestDto
    {
        /// <summary>
        /// Schedule ID - معرف الجدول
        /// </summary>
        [Required(ErrorMessage = "Schedule ID is required / معرف الجدول مطلوب")]
        public Guid ScheduleId { get; set; }

        /// <summary>
        /// Invoice Date - تاريخ الفاتورة
        /// If null, uses the schedule's due date
        /// </summary>
        public DateTime? InvoiceDate { get; set; }

        /// <summary>
        /// Override Due Date - تجاوز تاريخ الاستحقاق
        /// If null, uses schedule's payment due date
        /// </summary>
        public DateTime? OverrideDueDate { get; set; }

        /// <summary>
        /// Auto Post to GL - نشر تلقائي للقيد المحاسبي
        /// If true, posts the invoice to GL immediately
        /// </summary>
        public bool AutoPost { get; set; } = false;

        /// <summary>
        /// Submit to ZATCA - إرسال لزاتكا
        /// If true, submits to ZATCA after creation
        /// </summary>
        public bool SubmitToZatca { get; set; } = false;

        /// <summary>
        /// Send to Customer - إرسال للعميل
        /// If true, sends invoice to customer via email
        /// </summary>
        public bool SendToCustomer { get; set; } = false;

        /// <summary>
        /// Notes - ملاحظات
        /// Additional notes for the invoice
        /// </summary>
        [StringLength(1000)]
        public string Notes { get; set; }
    }

    /// <summary>
    /// Create Invoice From Schedule Result DTO - نتيجة إنشاء فاتورة من الجدول
    /// </summary>
    public class CreateInvoiceFromScheduleResultDto
    {
        /// <summary>
        /// Success - نجاح
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Invoice ID - معرف الفاتورة
        /// </summary>
        public Guid? InvoiceId { get; set; }

        /// <summary>
        /// Invoice Number - رقم الفاتورة
        /// </summary>
        public string InvoiceNumber { get; set; }

        /// <summary>
        /// Schedule ID - معرف الجدول
        /// </summary>
        public Guid ScheduleId { get; set; }

        /// <summary>
        /// Contract ID - معرف العقد
        /// </summary>
        public Guid ContractId { get; set; }

        /// <summary>
        /// Contract Number - رقم العقد
        /// </summary>
        public string ContractNumber { get; set; }

        /// <summary>
        /// Customer ID - معرف العميل
        /// </summary>
        public Guid CustomerId { get; set; }

        /// <summary>
        /// Customer Name - اسم العميل
        /// </summary>
        public string CustomerName { get; set; }

        /// <summary>
        /// Invoice Date - تاريخ الفاتورة
        /// </summary>
        public DateTime? InvoiceDate { get; set; }

        /// <summary>
        /// Due Date - تاريخ الاستحقاق
        /// </summary>
        public DateTime? DueDate { get; set; }

        /// <summary>
        /// Billing Period - فترة الفوترة
        /// </summary>
        public string BillingPeriod { get; set; }

        /// <summary>
        /// Total Amount - المبلغ الإجمالي
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Currency - العملة
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// Is Posted - تم النشر
        /// </summary>
        public bool IsPosted { get; set; }

        /// <summary>
        /// GL Journal ID - معرف القيد المحاسبي
        /// </summary>
        public Guid? GLJournalId { get; set; }

        /// <summary>
        /// ZATCA Status - حالة زاتكا
        /// </summary>
        public string ZatcaStatus { get; set; }

        /// <summary>
        /// ZATCA UUID - معرف زاتكا
        /// </summary>
        public string ZatcaUuid { get; set; }

        /// <summary>
        /// Error Message - رسالة الخطأ
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Warnings - التحذيرات
        /// </summary>
        public List<string> Warnings { get; set; }
    }

    /// <summary>
    /// Bulk Create Invoices From Schedules Request DTO - طلب إنشاء فواتير متعددة
    /// </summary>
    public class BulkCreateInvoicesFromSchedulesRequestDto
    {
        /// <summary>
        /// Schedule IDs - معرفات الجداول
        /// </summary>
        [Required(ErrorMessage = "At least one schedule is required / يجب تحديد جدول واحد على الأقل")]
        [MinLength(1)]
        public List<Guid> ScheduleIds { get; set; }

        /// <summary>
        /// Invoice Date - تاريخ الفاتورة
        /// If null, uses each schedule's due date
        /// </summary>
        public DateTime? InvoiceDate { get; set; }

        /// <summary>
        /// Auto Post to GL - نشر تلقائي للقيد المحاسبي
        /// </summary>
        public bool AutoPost { get; set; } = false;

        /// <summary>
        /// Submit to ZATCA - إرسال لزاتكا
        /// </summary>
        public bool SubmitToZatca { get; set; } = false;

        /// <summary>
        /// Send to Customer - إرسال للعميل
        /// </summary>
        public bool SendToCustomer { get; set; } = false;

        /// <summary>
        /// Stop on Error - توقف عند الخطأ
        /// If true, stops processing on first error; if false, continues with others
        /// </summary>
        public bool StopOnError { get; set; } = false;
    }

    /// <summary>
    /// Bulk Create Invoices Result DTO - نتيجة إنشاء فواتير متعددة
    /// </summary>
    public class BulkCreateInvoicesResultDto
    {
        /// <summary>
        /// Total Requested - إجمالي المطلوب
        /// </summary>
        public int TotalRequested { get; set; }

        /// <summary>
        /// Total Success - إجمالي النجاح
        /// </summary>
        public int TotalSuccess { get; set; }

        /// <summary>
        /// Total Failed - إجمالي الفشل
        /// </summary>
        public int TotalFailed { get; set; }

        /// <summary>
        /// Total Amount - إجمالي المبلغ
        /// Sum of all successfully created invoices
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Currency - العملة
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// Results - النتائج
        /// Individual result for each schedule
        /// </summary>
        public List<CreateInvoiceFromScheduleResultDto> Results { get; set; }

        /// <summary>
        /// Errors - الأخطاء
        /// List of error messages for failed invoices
        /// </summary>
        public List<BulkInvoiceErrorDto> Errors { get; set; }
    }

    /// <summary>
    /// Bulk Invoice Error DTO - خطأ إنشاء فاتورة
    /// </summary>
    public class BulkInvoiceErrorDto
    {
        /// <summary>
        /// Schedule ID - معرف الجدول
        /// </summary>
        public Guid ScheduleId { get; set; }

        /// <summary>
        /// Contract Number - رقم العقد
        /// </summary>
        public string ContractNumber { get; set; }

        /// <summary>
        /// Error Message - رسالة الخطأ
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Error Code - رمز الخطأ
        /// </summary>
        public string ErrorCode { get; set; }
    }

    #endregion

    #region Invoice Query DTOs

    /// <summary>
    /// Contract Invoice Query DTO - معايير البحث عن فواتير العقود
    /// Extended query for invoices with contract information
    /// </summary>
    public class ContractInvoiceQueryDto : PagerInfo
    {
        /// <summary>
        /// Invoice ID - معرف الفاتورة
        /// </summary>
        public Guid? InvoiceId { get; set; }

        /// <summary>
        /// Invoice Number - رقم الفاتورة
        /// </summary>
        public string InvoiceNumber { get; set; }

        /// <summary>
        /// Contract ID - معرف العقد
        /// </summary>
        public Guid? ContractId { get; set; }

        /// <summary>
        /// Contract Number - رقم العقد
        /// </summary>
        public string ContractNumber { get; set; }

        /// <summary>
        /// Customer ID - معرف العميل
        /// </summary>
        public Guid? CustomerId { get; set; }

        /// <summary>
        /// Customer Name - اسم العميل
        /// </summary>
        public string CustomerName { get; set; }

        /// <summary>
        /// Contract Type - نوع العقد
        /// </summary>
        public ContractType? ContractType { get; set; }

        /// <summary>
        /// Invoice Status - حالة الفاتورة
        /// </summary>
        public int? InvoiceStatus { get; set; }

        /// <summary>
        /// Multiple Statuses - حالات متعددة
        /// </summary>
        public List<int> InvoiceStatuses { get; set; }

        /// <summary>
        /// Invoice Date From - تاريخ الفاتورة من
        /// </summary>
        public DateTime? InvoiceDateFrom { get; set; }

        /// <summary>
        /// Invoice Date To - تاريخ الفاتورة إلى
        /// </summary>
        public DateTime? InvoiceDateTo { get; set; }

        /// <summary>
        /// Billing Period From - فترة الفوترة من
        /// </summary>
        public DateTime? BillingPeriodFrom { get; set; }

        /// <summary>
        /// Billing Period To - فترة الفوترة إلى
        /// </summary>
        public DateTime? BillingPeriodTo { get; set; }

        /// <summary>
        /// Is Overdue - متأخرة
        /// </summary>
        public bool? IsOverdue { get; set; }

        /// <summary>
        /// Is Recurring Only - متكررة فقط
        /// Only show invoices from contracts
        /// </summary>
        public bool? IsRecurringOnly { get; set; }

        /// <summary>
        /// Company ID - معرف الشركة
        /// </summary>
        public int? CompanyId { get; set; }

        /// <summary>
        /// Branch ID - معرف الفرع
        /// </summary>
        public int? BranchId { get; set; }
    }

    /// <summary>
    /// Contract Invoice List DTO - قائمة فواتير العقود
    /// </summary>
    public class ContractInvoiceListDto
    {
        /// <summary>
        /// Invoice ID - معرف الفاتورة
        /// </summary>
        public Guid InvoiceId { get; set; }

        /// <summary>
        /// Invoice Number - رقم الفاتورة
        /// </summary>
        [ExcelColumn(Name = "Invoice # / رقم الفاتورة")]
        public string InvoiceNumber { get; set; }

        /// <summary>
        /// Contract ID - معرف العقد
        /// </summary>
        public Guid? ContractId { get; set; }

        /// <summary>
        /// Contract Number - رقم العقد
        /// </summary>
        [ExcelColumn(Name = "Contract # / رقم العقد")]
        public string ContractNumber { get; set; }

        /// <summary>
        /// Customer ID - معرف العميل
        /// </summary>
        public Guid CustomerId { get; set; }

        /// <summary>
        /// Customer Name - اسم العميل
        /// </summary>
        [ExcelColumn(Name = "Customer / العميل")]
        public string CustomerName { get; set; }

        /// <summary>
        /// Invoice Date - تاريخ الفاتورة
        /// </summary>
        [ExcelColumn(Name = "Invoice Date / تاريخ الفاتورة", Format = "yyyy-MM-dd")]
        public DateTime InvoiceDate { get; set; }

        /// <summary>
        /// Due Date - تاريخ الاستحقاق
        /// </summary>
        [ExcelColumn(Name = "Due Date / تاريخ الاستحقاق", Format = "yyyy-MM-dd")]
        public DateTime DueDate { get; set; }

        /// <summary>
        /// Billing Period Start - بداية فترة الفوترة
        /// </summary>
        public DateTime? BillingPeriodStart { get; set; }

        /// <summary>
        /// Billing Period End - نهاية فترة الفوترة
        /// </summary>
        public DateTime? BillingPeriodEnd { get; set; }

        /// <summary>
        /// Billing Period Display - عرض فترة الفوترة
        /// </summary>
        [ExcelColumn(Name = "Billing Period / فترة الفوترة")]
        public string BillingPeriodDisplay => BillingPeriodStart.HasValue
            ? $"{BillingPeriodStart:MMM yyyy}"
            : "-";

        /// <summary>
        /// Subtotal - المجموع الفرعي
        /// </summary>
        public decimal Subtotal { get; set; }

        /// <summary>
        /// Tax Amount - مبلغ الضريبة
        /// </summary>
        [ExcelColumn(Name = "Tax / الضريبة", Format = "#,##0.00")]
        public decimal TaxAmount { get; set; }

        /// <summary>
        /// Total Amount - المبلغ الإجمالي
        /// </summary>
        [ExcelColumn(Name = "Total / الإجمالي", Format = "#,##0.00")]
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Paid Amount - المبلغ المدفوع
        /// </summary>
        [ExcelColumn(Name = "Paid / المدفوع", Format = "#,##0.00")]
        public decimal PaidAmount { get; set; }

        /// <summary>
        /// Remaining Amount - المبلغ المتبقي
        /// </summary>
        [ExcelColumn(Name = "Remaining / المتبقي", Format = "#,##0.00")]
        public decimal RemainingAmount { get; set; }

        /// <summary>
        /// Currency - العملة
        /// </summary>
        [ExcelColumn(Name = "Currency / العملة")]
        public string Currency { get; set; }

        /// <summary>
        /// Status - الحالة
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// Status Name - اسم الحالة
        /// </summary>
        [ExcelColumn(Name = "Status / الحالة")]
        public string StatusName { get; set; }

        /// <summary>
        /// Is Recurring - متكررة
        /// </summary>
        public bool IsRecurring { get; set; }

        /// <summary>
        /// Is Overdue - متأخرة
        /// </summary>
        public bool IsOverdue => RemainingAmount > 0 && DueDate < DateTime.Today;

        /// <summary>
        /// Days Overdue - أيام التأخير
        /// </summary>
        public int? DaysOverdue => IsOverdue ? (int)(DateTime.Today - DueDate).TotalDays : null;

        /// <summary>
        /// ZATCA Status - حالة زاتكا
        /// </summary>
        public string ZatcaStatus { get; set; }

        /// <summary>
        /// Created Time - وقت الإنشاء
        /// </summary>
        public DateTime? Create_time { get; set; }
    }

    #endregion

    #region Invoice Line DTOs

    /// <summary>
    /// Contract Invoice Line DTO - بند فاتورة العقد
    /// Extended invoice line with contract information
    /// </summary>
    public class ContractInvoiceLineDto
    {
        /// <summary>
        /// Line ID - معرف البند
        /// </summary>
        public Guid LineId { get; set; }

        /// <summary>
        /// Line Number - رقم البند
        /// </summary>
        public int LineNumber { get; set; }

        /// <summary>
        /// Description - الوصف
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Contract Unit ID - معرف وحدة العقد
        /// </summary>
        public Guid? ContractUnitId { get; set; }

        /// <summary>
        /// Contract Unit Code - رمز وحدة العقد
        /// </summary>
        public string ContractUnitCode { get; set; }

        /// <summary>
        /// Contract Pricing Rule ID - معرف قاعدة التسعير
        /// </summary>
        public Guid? ContractPricingRuleId { get; set; }

        /// <summary>
        /// Pricing Rule Name - اسم قاعدة التسعير
        /// </summary>
        public string PricingRuleName { get; set; }

        /// <summary>
        /// Quantity - الكمية
        /// </summary>
        public decimal Quantity { get; set; }

        /// <summary>
        /// Unit Price - سعر الوحدة
        /// </summary>
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// Amount - المبلغ
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Tax Rate - نسبة الضريبة
        /// </summary>
        public decimal TaxRate { get; set; }

        /// <summary>
        /// Tax Amount - مبلغ الضريبة
        /// </summary>
        public decimal TaxAmount { get; set; }

        /// <summary>
        /// Total Amount - المبلغ الإجمالي
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Account ID - معرف الحساب
        /// </summary>
        public Guid AccountId { get; set; }

        /// <summary>
        /// Account Name - اسم الحساب
        /// </summary>
        public string AccountName { get; set; }

        /// <summary>
        /// Cost Center ID - معرف مركز التكلفة
        /// </summary>
        public Guid? CostCenterId { get; set; }

        /// <summary>
        /// Cost Center Name - اسم مركز التكلفة
        /// </summary>
        public string CostCenterName { get; set; }
    }

    #endregion

    #region Summary DTOs

    /// <summary>
    /// Contract Revenue Summary DTO - ملخص إيرادات العقود
    /// </summary>
    public class ContractRevenueSummaryDto
    {
        /// <summary>
        /// Period Start - بداية الفترة
        /// </summary>
        public DateTime PeriodStart { get; set; }

        /// <summary>
        /// Period End - نهاية الفترة
        /// </summary>
        public DateTime PeriodEnd { get; set; }

        /// <summary>
        /// Total Contracts - إجمالي العقود
        /// </summary>
        public int TotalContracts { get; set; }

        /// <summary>
        /// Total Invoices - إجمالي الفواتير
        /// </summary>
        public int TotalInvoices { get; set; }

        /// <summary>
        /// Total Invoiced Amount - إجمالي المبالغ المفوترة
        /// </summary>
        public decimal TotalInvoicedAmount { get; set; }

        /// <summary>
        /// Total Collected Amount - إجمالي المبالغ المحصلة
        /// </summary>
        public decimal TotalCollectedAmount { get; set; }

        /// <summary>
        /// Outstanding Amount - المبالغ المستحقة
        /// </summary>
        public decimal OutstandingAmount { get; set; }

        /// <summary>
        /// Overdue Amount - المبالغ المتأخرة
        /// </summary>
        public decimal OverdueAmount { get; set; }

        /// <summary>
        /// Collection Rate - نسبة التحصيل
        /// </summary>
        public decimal CollectionRate => TotalInvoicedAmount > 0
            ? Math.Round((TotalCollectedAmount / TotalInvoicedAmount) * 100, 2)
            : 0;

        /// <summary>
        /// Currency - العملة
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// By Contract Type - حسب نوع العقد
        /// </summary>
        public List<RevenueByContractTypeDto> ByContractType { get; set; }

        /// <summary>
        /// By Month - حسب الشهر
        /// </summary>
        public List<MonthlyRevenueDto> ByMonth { get; set; }
    }

    /// <summary>
    /// Revenue By Contract Type DTO - الإيرادات حسب نوع العقد
    /// </summary>
    public class RevenueByContractTypeDto
    {
        /// <summary>
        /// Contract Type - نوع العقد
        /// </summary>
        public ContractType ContractType { get; set; }

        /// <summary>
        /// Contract Type Name - اسم نوع العقد
        /// </summary>
        public string ContractTypeName { get; set; }

        /// <summary>
        /// Contract Count - عدد العقود
        /// </summary>
        public int ContractCount { get; set; }

        /// <summary>
        /// Invoice Count - عدد الفواتير
        /// </summary>
        public int InvoiceCount { get; set; }

        /// <summary>
        /// Invoiced Amount - المبلغ المفوتر
        /// </summary>
        public decimal InvoicedAmount { get; set; }

        /// <summary>
        /// Collected Amount - المبلغ المحصل
        /// </summary>
        public decimal CollectedAmount { get; set; }

        /// <summary>
        /// Percentage of Total - نسبة من الإجمالي
        /// </summary>
        public decimal PercentageOfTotal { get; set; }
    }

    /// <summary>
    /// Monthly Revenue DTO - الإيرادات الشهرية
    /// </summary>
    public class MonthlyRevenueDto
    {
        /// <summary>
        /// Month - الشهر
        /// </summary>
        public int Month { get; set; }

        /// <summary>
        /// Year - السنة
        /// </summary>
        public int Year { get; set; }

        /// <summary>
        /// Month Name - اسم الشهر
        /// </summary>
        public string MonthName { get; set; }

        /// <summary>
        /// Invoice Count - عدد الفواتير
        /// </summary>
        public int InvoiceCount { get; set; }

        /// <summary>
        /// Invoiced Amount - المبلغ المفوتر
        /// </summary>
        public decimal InvoicedAmount { get; set; }

        /// <summary>
        /// Collected Amount - المبلغ المحصل
        /// </summary>
        public decimal CollectedAmount { get; set; }
    }

    #endregion

    #region Void/Credit Note DTOs

    /// <summary>
    /// Void Contract Invoice Request DTO - طلب إلغاء فاتورة عقد
    /// </summary>
    public class VoidContractInvoiceRequestDto
    {
        /// <summary>
        /// Invoice ID - معرف الفاتورة
        /// </summary>
        [Required(ErrorMessage = "Invoice ID is required / معرف الفاتورة مطلوب")]
        public Guid InvoiceId { get; set; }

        /// <summary>
        /// Void Reason - سبب الإلغاء
        /// </summary>
        [Required(ErrorMessage = "Void reason is required / سبب الإلغاء مطلوب")]
        [StringLength(500)]
        public string VoidReason { get; set; }

        /// <summary>
        /// Reopen Schedule - إعادة فتح الجدول
        /// If true, sets the billing schedule back to Pending
        /// </summary>
        public bool ReopenSchedule { get; set; } = true;

        /// <summary>
        /// Create Credit Note - إنشاء إشعار دائن
        /// If true, creates a credit note for the voided invoice
        /// </summary>
        public bool CreateCreditNote { get; set; } = false;
    }

    /// <summary>
    /// Create Credit Note From Contract Request DTO - طلب إنشاء إشعار دائن من العقد
    /// </summary>
    public class CreateCreditNoteFromContractRequestDto
    {
        /// <summary>
        /// Original Invoice ID - معرف الفاتورة الأصلية
        /// </summary>
        [Required(ErrorMessage = "Original invoice ID is required / معرف الفاتورة الأصلية مطلوب")]
        public Guid OriginalInvoiceId { get; set; }

        /// <summary>
        /// Credit Note Reason - سبب الإشعار الدائن
        /// </summary>
        [Required(ErrorMessage = "Credit note reason is required / سبب الإشعار الدائن مطلوب")]
        [StringLength(500)]
        public string CreditNoteReason { get; set; }

        /// <summary>
        /// Is Full Credit - إشعار دائن كامل
        /// If true, credits the full invoice amount
        /// </summary>
        public bool IsFullCredit { get; set; } = true;

        /// <summary>
        /// Lines to Credit - البنود للإشعار
        /// If not full credit, specify lines to credit
        /// </summary>
        public List<CreditNoteLineDto> LinesToCredit { get; set; }

        /// <summary>
        /// Credit Note Date - تاريخ الإشعار الدائن
        /// </summary>
        public DateTime? CreditNoteDate { get; set; }

        /// <summary>
        /// Adjust Schedule - تعديل الجدول
        /// If true, adjusts the related billing schedule
        /// </summary>
        public bool AdjustSchedule { get; set; } = false;
    }

    /// <summary>
    /// Credit Note Line DTO - بند الإشعار الدائن
    /// </summary>
    public class CreditNoteLineDto
    {
        /// <summary>
        /// Original Line ID - معرف البند الأصلي
        /// </summary>
        [Required]
        public Guid OriginalLineId { get; set; }

        /// <summary>
        /// Credit Amount - مبلغ الإشعار
        /// </summary>
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal CreditAmount { get; set; }

        /// <summary>
        /// Credit Reason - سبب الإشعار
        /// </summary>
        [StringLength(200)]
        public string CreditReason { get; set; }
    }

    #endregion
}
