using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MiniExcelLibs.Attributes;
using ZR.Model.Contracts.Enums;

namespace ZR.Model.Contracts.Dto
{
    #region Query DTO

    /// <summary>
    /// Contract Billing Schedule Query DTO - معايير البحث عن جداول الفوترة
    /// Used for filtering and searching billing schedules
    /// </summary>
    public class ErpContractBillingScheduleQueryDto : PagerInfo
    {
        /// <summary>
        /// Schedule ID - معرف الجدول
        /// </summary>
        public Guid? ScheduleId { get; set; }

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
        /// Status - الحالة
        /// </summary>
        public BillingScheduleStatus? Status { get; set; }

        /// <summary>
        /// Multiple Statuses - حالات متعددة
        /// </summary>
        public List<BillingScheduleStatus> Statuses { get; set; }

        /// <summary>
        /// Due Date From - تاريخ الاستحقاق من
        /// </summary>
        public DateTime? DueDateFrom { get; set; }

        /// <summary>
        /// Due Date To - تاريخ الاستحقاق إلى
        /// </summary>
        public DateTime? DueDateTo { get; set; }

        /// <summary>
        /// Period Start From - بداية الفترة من
        /// </summary>
        public DateTime? PeriodStartFrom { get; set; }

        /// <summary>
        /// Period Start To - بداية الفترة إلى
        /// </summary>
        public DateTime? PeriodStartTo { get; set; }

        /// <summary>
        /// Invoice ID - معرف الفاتورة
        /// </summary>
        public Guid? InvoiceId { get; set; }

        /// <summary>
        /// Has Invoice - لديه فاتورة
        /// </summary>
        public bool? HasInvoice { get; set; }

        /// <summary>
        /// Is Overdue - متأخر
        /// </summary>
        public bool? IsOverdue { get; set; }

        /// <summary>
        /// Is Pro-Rated - مقسط نسبياً
        /// </summary>
        public bool? IsProRated { get; set; }

        /// <summary>
        /// Amount From - المبلغ من
        /// </summary>
        public decimal? AmountFrom { get; set; }

        /// <summary>
        /// Amount To - المبلغ إلى
        /// </summary>
        public decimal? AmountTo { get; set; }

        /// <summary>
        /// Currency - العملة
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// Due Within Days - مستحق خلال أيام
        /// Find schedules due within X days
        /// </summary>
        public int? DueWithinDays { get; set; }

        /// <summary>
        /// Company ID - معرف الشركة
        /// </summary>
        public int? CompanyId { get; set; }

        /// <summary>
        /// Branch ID - معرف الفرع
        /// </summary>
        public int? BranchId { get; set; }
    }

    #endregion

    #region List DTO (Full Implementation)

    /// <summary>
    /// Contract Billing Schedule List DTO - قائمة جداول الفوترة
    /// Used for displaying billing schedules in list views
    /// </summary>
    public class ErpContractBillingScheduleFullListDto
    {
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
        [ExcelColumn(Name = "Contract / العقد")]
        public string ContractNumber { get; set; }

        /// <summary>
        /// Customer Name - اسم العميل
        /// </summary>
        [ExcelColumn(Name = "Customer / العميل")]
        public string CustomerName { get; set; }

        /// <summary>
        /// Schedule Number - رقم الجدول
        /// </summary>
        [ExcelColumn(Name = "# / رقم")]
        public int ScheduleNumber { get; set; }

        /// <summary>
        /// Period Start - بداية الفترة
        /// </summary>
        [ExcelColumn(Name = "Period Start / بداية الفترة", Format = "yyyy-MM-dd")]
        public DateTime PeriodStart { get; set; }

        /// <summary>
        /// Period End - نهاية الفترة
        /// </summary>
        [ExcelColumn(Name = "Period End / نهاية الفترة", Format = "yyyy-MM-dd")]
        public DateTime PeriodEnd { get; set; }

        /// <summary>
        /// Period Display - عرض الفترة
        /// </summary>
        public string PeriodDisplay => $"{PeriodStart:MMM yyyy}";

        /// <summary>
        /// Due Date - تاريخ الاستحقاق
        /// </summary>
        [ExcelColumn(Name = "Due Date / تاريخ الاستحقاق", Format = "yyyy-MM-dd")]
        public DateTime DueDate { get; set; }

        /// <summary>
        /// Payment Due Date - تاريخ استحقاق الدفع
        /// </summary>
        [ExcelColumn(Name = "Payment Due / استحقاق الدفع", Format = "yyyy-MM-dd")]
        public DateTime PaymentDueDate { get; set; }

        /// <summary>
        /// Base Amount - المبلغ الأساسي
        /// </summary>
        public decimal BaseAmount { get; set; }

        /// <summary>
        /// Adjustment Amount - مبلغ التعديلات
        /// </summary>
        public decimal AdjustmentAmount { get; set; }

        /// <summary>
        /// Subtotal - المجموع الفرعي
        /// </summary>
        [ExcelColumn(Name = "Subtotal / المجموع الفرعي", Format = "#,##0.00")]
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
        /// Currency - العملة
        /// </summary>
        [ExcelColumn(Name = "Currency / العملة")]
        public string Currency { get; set; }

        /// <summary>
        /// Status - الحالة
        /// </summary>
        [ExcelColumn(Name = "Status / الحالة")]
        public BillingScheduleStatus Status { get; set; }

        /// <summary>
        /// Status Name - اسم الحالة
        /// </summary>
        public string StatusName { get; set; }

        /// <summary>
        /// Invoice ID - معرف الفاتورة
        /// </summary>
        public Guid? InvoiceId { get; set; }

        /// <summary>
        /// Invoice Number - رقم الفاتورة
        /// </summary>
        [ExcelColumn(Name = "Invoice / الفاتورة")]
        public string InvoiceNumber { get; set; }

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
        /// Is Overdue - متأخر
        /// </summary>
        [ExcelColumn(Name = "Overdue / متأخر")]
        public bool IsOverdue { get; set; }

        /// <summary>
        /// Days Overdue - أيام التأخير
        /// </summary>
        public int? DaysOverdue { get; set; }

        /// <summary>
        /// Is Pro-Rated - مقسط نسبياً
        /// </summary>
        public bool IsProRated { get; set; }

        /// <summary>
        /// Can Generate Invoice - يمكن إنشاء فاتورة
        /// </summary>
        public bool CanGenerateInvoice => Status == BillingScheduleStatus.Pending || Status == BillingScheduleStatus.Due;

        /// <summary>
        /// Created Time - وقت الإنشاء
        /// </summary>
        public DateTime? Create_time { get; set; }
    }

    #endregion

    #region Detail DTO

    /// <summary>
    /// Contract Billing Schedule Detail DTO - تفاصيل جدول الفوترة
    /// Used for displaying full schedule details
    /// </summary>
    public class ErpContractBillingScheduleDetailDto : ErpContractBillingScheduleFullListDto
    {
        /// <summary>
        /// Description - الوصف
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Description (Arabic) - الوصف بالعربية
        /// </summary>
        public string DescriptionAr { get; set; }

        /// <summary>
        /// Invoiced Date - تاريخ الفوترة
        /// </summary>
        public DateTime? InvoicedDate { get; set; }

        /// <summary>
        /// Invoiced By - تمت الفوترة بواسطة
        /// </summary>
        public string InvoicedBy { get; set; }

        /// <summary>
        /// Last Payment Date - تاريخ آخر دفعة
        /// </summary>
        public DateTime? LastPaymentDate { get; set; }

        /// <summary>
        /// Tax Group ID - معرف مجموعة الضريبة
        /// </summary>
        public Guid? TaxGroupId { get; set; }

        /// <summary>
        /// Tax Group Name - اسم مجموعة الضريبة
        /// </summary>
        public string TaxGroupName { get; set; }

        /// <summary>
        /// Tax Rate - نسبة الضريبة
        /// </summary>
        public decimal? TaxRate { get; set; }

        /// <summary>
        /// Is Tax Inclusive - هل الضريبة مشمولة
        /// </summary>
        public bool IsTaxInclusive { get; set; }

        /// <summary>
        /// Pro-Rata Days - أيام التقسيط
        /// </summary>
        public int? ProRataDays { get; set; }

        /// <summary>
        /// Full Period Days - أيام الفترة الكاملة
        /// </summary>
        public int? FullPeriodDays { get; set; }

        #region Cancellation/Skip Fields

        /// <summary>
        /// Cancelled Date - تاريخ الإلغاء
        /// </summary>
        public DateTime? CancelledDate { get; set; }

        /// <summary>
        /// Cancelled By - تم الإلغاء بواسطة
        /// </summary>
        public string CancelledBy { get; set; }

        /// <summary>
        /// Cancellation Reason - سبب الإلغاء
        /// </summary>
        public string CancellationReason { get; set; }

        /// <summary>
        /// Skipped Date - تاريخ التخطي
        /// </summary>
        public DateTime? SkippedDate { get; set; }

        /// <summary>
        /// Skipped By - تم التخطي بواسطة
        /// </summary>
        public string SkippedBy { get; set; }

        /// <summary>
        /// Skip Reason - سبب التخطي
        /// </summary>
        public string SkipReason { get; set; }

        #endregion

        #region Notification Fields

        /// <summary>
        /// Reminder Sent - تم إرسال التذكير
        /// </summary>
        public bool ReminderSent { get; set; }

        /// <summary>
        /// Reminder Sent Date - تاريخ إرسال التذكير
        /// </summary>
        public DateTime? ReminderSentDate { get; set; }

        /// <summary>
        /// Overdue Notice Sent - تم إرسال إشعار التأخير
        /// </summary>
        public bool OverdueNoticeSent { get; set; }

        /// <summary>
        /// Overdue Notice Date - تاريخ إشعار التأخير
        /// </summary>
        public DateTime? OverdueNoticeDate { get; set; }

        #endregion

        /// <summary>
        /// Notes - ملاحظات
        /// </summary>
        public string Notes { get; set; }

        /// <summary>
        /// Amendment ID - معرف التعديل
        /// </summary>
        public Guid? AmendmentId { get; set; }

        /// <summary>
        /// Lines - البنود
        /// Detailed line items
        /// </summary>
        public List<BillingScheduleLineDto> Lines { get; set; }

        #region Audit Fields

        /// <summary>
        /// Created By - أنشئ بواسطة
        /// </summary>
        public string Create_by { get; set; }

        /// <summary>
        /// Updated By - تم التحديث بواسطة
        /// </summary>
        public string Update_by { get; set; }

        /// <summary>
        /// Updated Time - وقت التحديث
        /// </summary>
        public DateTime? Update_time { get; set; }

        #endregion
    }

    /// <summary>
    /// Billing Schedule Line DTO - بند جدول الفوترة
    /// </summary>
    public class BillingScheduleLineDto
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
        /// Line Type - نوع البند
        /// </summary>
        public string LineType { get; set; }

        /// <summary>
        /// Unit ID - معرف الوحدة
        /// </summary>
        public Guid? UnitId { get; set; }

        /// <summary>
        /// Unit Code - رمز الوحدة
        /// </summary>
        public string UnitCode { get; set; }

        /// <summary>
        /// Pricing Rule ID - معرف قاعدة التسعير
        /// </summary>
        public Guid? PricingRuleId { get; set; }

        /// <summary>
        /// Pricing Rule Name - اسم قاعدة التسعير
        /// </summary>
        public string PricingRuleName { get; set; }

        /// <summary>
        /// Description - الوصف
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Description (Arabic) - الوصف بالعربية
        /// </summary>
        public string DescriptionAr { get; set; }

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
        public decimal? TaxRate { get; set; }

        /// <summary>
        /// Tax Amount - مبلغ الضريبة
        /// </summary>
        public decimal TaxAmount { get; set; }

        /// <summary>
        /// Total Amount - المبلغ الإجمالي
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Is Pro-Rated - مقسط نسبياً
        /// </summary>
        public bool IsProRated { get; set; }

        /// <summary>
        /// Original Amount - المبلغ الأصلي
        /// </summary>
        public decimal? OriginalAmount { get; set; }

        /// <summary>
        /// Revenue Account Name - اسم حساب الإيرادات
        /// </summary>
        public string RevenueAccountName { get; set; }
    }

    #endregion

    #region Generation DTOs

    /// <summary>
    /// Generate Billing Schedule Request DTO - طلب إنشاء جدول الفوترة
    /// </summary>
    public class GenerateBillingScheduleRequestDto
    {
        /// <summary>
        /// Contract ID - معرف العقد
        /// </summary>
        [Required(ErrorMessage = "Contract ID is required / معرف العقد مطلوب")]
        public Guid ContractId { get; set; }

        /// <summary>
        /// Generate From Date - إنشاء من تاريخ
        /// If null, uses contract start date or last billing date
        /// </summary>
        public DateTime? GenerateFromDate { get; set; }

        /// <summary>
        /// Generate To Date - إنشاء إلى تاريخ
        /// If null, generates until contract end date or 12 months ahead
        /// </summary>
        public DateTime? GenerateToDate { get; set; }

        /// <summary>
        /// Number of Periods - عدد الفترات
        /// Alternative to GenerateToDate - generate N periods
        /// </summary>
        public int? NumberOfPeriods { get; set; }

        /// <summary>
        /// Regenerate Existing - إعادة إنشاء الموجود
        /// If true, regenerates pending schedules (cancels old ones)
        /// </summary>
        public bool RegenerateExisting { get; set; } = false;

        /// <summary>
        /// Include Pro-Rata - تضمين التقسيط النسبي
        /// Whether to create pro-rated entry for partial first period
        /// </summary>
        public bool IncludeProRata { get; set; } = true;
    }

    /// <summary>
    /// Generate Billing Schedule Result DTO - نتيجة إنشاء جدول الفوترة
    /// </summary>
    public class GenerateBillingScheduleResultDto
    {
        /// <summary>
        /// Contract ID - معرف العقد
        /// </summary>
        public Guid ContractId { get; set; }

        /// <summary>
        /// Contract Number - رقم العقد
        /// </summary>
        public string ContractNumber { get; set; }

        /// <summary>
        /// Generation Batch ID - معرف دفعة الإنشاء
        /// </summary>
        public Guid GenerationBatchId { get; set; }

        /// <summary>
        /// Total Schedules Created - إجمالي الجداول المنشأة
        /// </summary>
        public int TotalSchedulesCreated { get; set; }

        /// <summary>
        /// Total Schedules Cancelled - إجمالي الجداول الملغاة
        /// </summary>
        public int TotalSchedulesCancelled { get; set; }

        /// <summary>
        /// First Period Start - بداية أول فترة
        /// </summary>
        public DateTime? FirstPeriodStart { get; set; }

        /// <summary>
        /// Last Period End - نهاية آخر فترة
        /// </summary>
        public DateTime? LastPeriodEnd { get; set; }

        /// <summary>
        /// Total Amount - المبلغ الإجمالي
        /// Sum of all generated schedules
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Currency - العملة
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// Has Pro-Rata Period - يوجد فترة تقسيط
        /// </summary>
        public bool HasProRataPeriod { get; set; }

        /// <summary>
        /// Generated Schedules - الجداول المنشأة
        /// List of created schedule summaries
        /// </summary>
        public List<GeneratedScheduleSummaryDto> GeneratedSchedules { get; set; }

        /// <summary>
        /// Warnings - التحذيرات
        /// Any warnings during generation
        /// </summary>
        public List<string> Warnings { get; set; }
    }

    /// <summary>
    /// Generated Schedule Summary DTO - ملخص الجدول المنشأ
    /// </summary>
    public class GeneratedScheduleSummaryDto
    {
        /// <summary>
        /// Schedule ID - معرف الجدول
        /// </summary>
        public Guid ScheduleId { get; set; }

        /// <summary>
        /// Schedule Number - رقم الجدول
        /// </summary>
        public int ScheduleNumber { get; set; }

        /// <summary>
        /// Period Start - بداية الفترة
        /// </summary>
        public DateTime PeriodStart { get; set; }

        /// <summary>
        /// Period End - نهاية الفترة
        /// </summary>
        public DateTime PeriodEnd { get; set; }

        /// <summary>
        /// Due Date - تاريخ الاستحقاق
        /// </summary>
        public DateTime DueDate { get; set; }

        /// <summary>
        /// Total Amount - المبلغ الإجمالي
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Is Pro-Rated - مقسط نسبياً
        /// </summary>
        public bool IsProRated { get; set; }
    }

    /// <summary>
    /// Bulk Generate Billing Schedules DTO - إنشاء جداول فوترة متعددة
    /// Generate schedules for multiple contracts
    /// </summary>
    public class BulkGenerateBillingSchedulesDto
    {
        /// <summary>
        /// Contract IDs - معرفات العقود
        /// </summary>
        public List<Guid> ContractIds { get; set; }

        /// <summary>
        /// Generate To Date - إنشاء إلى تاريخ
        /// </summary>
        public DateTime? GenerateToDate { get; set; }

        /// <summary>
        /// Number of Periods - عدد الفترات
        /// </summary>
        public int? NumberOfPeriods { get; set; }

        /// <summary>
        /// Regenerate Existing - إعادة إنشاء الموجود
        /// </summary>
        public bool RegenerateExisting { get; set; } = false;

        /// <summary>
        /// Filter By Status - تصفية حسب الحالة
        /// Only generate for contracts with these statuses
        /// </summary>
        public List<ContractStatus> FilterByStatus { get; set; }
    }

    #endregion

    #region Action DTOs

    /// <summary>
    /// Cancel Billing Schedule DTO - إلغاء جدول الفوترة
    /// </summary>
    public class CancelBillingScheduleDto
    {
        /// <summary>
        /// Schedule ID - معرف الجدول
        /// </summary>
        [Required(ErrorMessage = "Schedule ID is required / معرف الجدول مطلوب")]
        public Guid ScheduleId { get; set; }

        /// <summary>
        /// Cancellation Reason - سبب الإلغاء
        /// </summary>
        [Required(ErrorMessage = "Cancellation reason is required / سبب الإلغاء مطلوب")]
        [StringLength(500)]
        public string CancellationReason { get; set; }
    }

    /// <summary>
    /// Skip Billing Schedule DTO - تخطي جدول الفوترة
    /// </summary>
    public class SkipBillingScheduleDto
    {
        /// <summary>
        /// Schedule ID - معرف الجدول
        /// </summary>
        [Required(ErrorMessage = "Schedule ID is required / معرف الجدول مطلوب")]
        public Guid ScheduleId { get; set; }

        /// <summary>
        /// Skip Reason - سبب التخطي
        /// </summary>
        [Required(ErrorMessage = "Skip reason is required / سبب التخطي مطلوب")]
        [StringLength(500)]
        public string SkipReason { get; set; }
    }

    /// <summary>
    /// Mark Schedule Due DTO - تحديد الجدول كمستحق
    /// Manually mark a schedule as due for invoicing
    /// </summary>
    public class MarkScheduleDueDto
    {
        /// <summary>
        /// Schedule ID - معرف الجدول
        /// </summary>
        [Required(ErrorMessage = "Schedule ID is required / معرف الجدول مطلوب")]
        public Guid ScheduleId { get; set; }

        /// <summary>
        /// Override Due Date - تجاوز تاريخ الاستحقاق
        /// </summary>
        public DateTime? OverrideDueDate { get; set; }
    }

    /// <summary>
    /// Recalculate Schedule DTO - إعادة حساب الجدول
    /// Recalculate a pending schedule with current pricing rules
    /// </summary>
    public class RecalculateScheduleDto
    {
        /// <summary>
        /// Schedule ID - معرف الجدول
        /// </summary>
        [Required(ErrorMessage = "Schedule ID is required / معرف الجدول مطلوب")]
        public Guid ScheduleId { get; set; }

        /// <summary>
        /// Recalculation Reason - سبب إعادة الحساب
        /// </summary>
        [StringLength(500)]
        public string RecalculationReason { get; set; }
    }

    /// <summary>
    /// Bulk Invoice Generation DTO - إنشاء فواتير متعددة
    /// Generate invoices for multiple due schedules
    /// </summary>
    public class BulkInvoiceGenerationDto
    {
        /// <summary>
        /// Schedule IDs - معرفات الجداول
        /// </summary>
        [Required(ErrorMessage = "At least one schedule is required / يجب تحديد جدول واحد على الأقل")]
        [MinLength(1)]
        public List<Guid> ScheduleIds { get; set; }

        /// <summary>
        /// Invoice Date - تاريخ الفاتورة
        /// </summary>
        public DateTime? InvoiceDate { get; set; }

        /// <summary>
        /// Auto Post - نشر تلقائي
        /// Whether to automatically post invoices to GL
        /// </summary>
        public bool AutoPost { get; set; } = false;

        /// <summary>
        /// Send to Customer - إرسال للعميل
        /// Whether to send invoices to customers
        /// </summary>
        public bool SendToCustomer { get; set; } = false;
    }

    #endregion

    #region Summary DTOs

    /// <summary>
    /// Billing Schedule Summary DTO - ملخص جداول الفوترة
    /// Aggregated summary for a contract or period
    /// </summary>
    public class BillingScheduleSummaryDto
    {
        /// <summary>
        /// Contract ID - معرف العقد
        /// </summary>
        public Guid? ContractId { get; set; }

        /// <summary>
        /// Total Schedules - إجمالي الجداول
        /// </summary>
        public int TotalSchedules { get; set; }

        /// <summary>
        /// Pending Count - عدد المعلقة
        /// </summary>
        public int PendingCount { get; set; }

        /// <summary>
        /// Due Count - عدد المستحقة
        /// </summary>
        public int DueCount { get; set; }

        /// <summary>
        /// Invoiced Count - عدد المفوترة
        /// </summary>
        public int InvoicedCount { get; set; }

        /// <summary>
        /// Paid Count - عدد المدفوعة
        /// </summary>
        public int PaidCount { get; set; }

        /// <summary>
        /// Cancelled Count - عدد الملغاة
        /// </summary>
        public int CancelledCount { get; set; }

        /// <summary>
        /// Overdue Count - عدد المتأخرة
        /// </summary>
        public int OverdueCount { get; set; }

        /// <summary>
        /// Total Amount - المبلغ الإجمالي
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Pending Amount - المبلغ المعلق
        /// </summary>
        public decimal PendingAmount { get; set; }

        /// <summary>
        /// Invoiced Amount - المبلغ المفوتر
        /// </summary>
        public decimal InvoicedAmount { get; set; }

        /// <summary>
        /// Paid Amount - المبلغ المدفوع
        /// </summary>
        public decimal PaidAmount { get; set; }

        /// <summary>
        /// Outstanding Amount - المبلغ المستحق
        /// </summary>
        public decimal OutstandingAmount { get; set; }

        /// <summary>
        /// Overdue Amount - المبلغ المتأخر
        /// </summary>
        public decimal OverdueAmount { get; set; }

        /// <summary>
        /// Currency - العملة
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// Next Due Date - تاريخ الاستحقاق القادم
        /// </summary>
        public DateTime? NextDueDate { get; set; }

        /// <summary>
        /// Next Due Amount - المبلغ المستحق القادم
        /// </summary>
        public decimal? NextDueAmount { get; set; }
    }

    /// <summary>
    /// Upcoming Billing Report DTO - تقرير الفوترة القادمة
    /// </summary>
    public class UpcomingBillingReportDto
    {
        /// <summary>
        /// Report Date - تاريخ التقرير
        /// </summary>
        public DateTime ReportDate { get; set; }

        /// <summary>
        /// Report Period Start - بداية فترة التقرير
        /// </summary>
        public DateTime PeriodStart { get; set; }

        /// <summary>
        /// Report Period End - نهاية فترة التقرير
        /// </summary>
        public DateTime PeriodEnd { get; set; }

        /// <summary>
        /// Total Expected Amount - إجمالي المبلغ المتوقع
        /// </summary>
        public decimal TotalExpectedAmount { get; set; }

        /// <summary>
        /// Currency - العملة
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// By Period - حسب الفترة
        /// </summary>
        public List<PeriodBillingSummaryDto> ByPeriod { get; set; }

        /// <summary>
        /// By Customer - حسب العميل
        /// </summary>
        public List<CustomerBillingSummaryDto> ByCustomer { get; set; }
    }

    /// <summary>
    /// Period Billing Summary DTO - ملخص فوترة الفترة
    /// </summary>
    public class PeriodBillingSummaryDto
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
        /// Period Label - تسمية الفترة
        /// </summary>
        public string PeriodLabel { get; set; }

        /// <summary>
        /// Schedule Count - عدد الجداول
        /// </summary>
        public int ScheduleCount { get; set; }

        /// <summary>
        /// Total Amount - المبلغ الإجمالي
        /// </summary>
        public decimal TotalAmount { get; set; }
    }

    /// <summary>
    /// Customer Billing Summary DTO - ملخص فوترة العميل
    /// </summary>
    public class CustomerBillingSummaryDto
    {
        /// <summary>
        /// Customer ID - معرف العميل
        /// </summary>
        public Guid CustomerId { get; set; }

        /// <summary>
        /// Customer Name - اسم العميل
        /// </summary>
        public string CustomerName { get; set; }

        /// <summary>
        /// Contract Count - عدد العقود
        /// </summary>
        public int ContractCount { get; set; }

        /// <summary>
        /// Schedule Count - عدد الجداول
        /// </summary>
        public int ScheduleCount { get; set; }

        /// <summary>
        /// Total Amount - المبلغ الإجمالي
        /// </summary>
        public decimal TotalAmount { get; set; }
    }

    #endregion
}
