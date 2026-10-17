using System;
using System.Collections.Generic;
using SqlSugar;
using ZR.Model.Contracts.Enums;
using ZR.Model.System;

namespace ZR.Model.Contracts
{
    /// <summary>
    /// Contract Billing Schedule - جدول فوترة العقد
    /// Represents a scheduled billing entry for a contract.
    /// Generated automatically based on contract billing frequency and pricing rules.
    /// Each entry becomes an AR Invoice when processed.
    /// </summary>
    [SugarTable("erp_contract_billing_schedule")]
    [SugarIndex("IX_billing_schedule_contract", nameof(ContractId), OrderByType.Asc)]
    [SugarIndex("IX_billing_schedule_due_date", nameof(DueDate), OrderByType.Asc)]
    [SugarIndex("IX_billing_schedule_status", nameof(Status), OrderByType.Asc)]
    [SugarIndex("IX_billing_schedule_invoice", nameof(InvoiceId), OrderByType.Asc)]
    [SugarIndex("UX_billing_schedule_period", nameof(ContractId), OrderByType.Asc, nameof(PeriodStart), OrderByType.Asc, nameof(PeriodEnd), OrderByType.Asc, true)]
    public class ErpContractBillingSchedule : SysBase
    {
        /// <summary>
        /// Schedule ID - معرف الجدول
        /// Primary key (GUID)
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false, ColumnName = "schedule_id")]
        public Guid ScheduleId { get; set; }

        /// <summary>
        /// Contract ID - معرف العقد
        /// Foreign key to ErpContract
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "contract_id")]
        public Guid ContractId { get; set; }

        /// <summary>
        /// Schedule Number - رقم الجدول
        /// Sequential number within the contract (1, 2, 3...)
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "schedule_number")]
        public int ScheduleNumber { get; set; }

        /// <summary>
        /// Period Start - بداية الفترة
        /// Start date of the billing period
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "period_start")]
        public DateTime PeriodStart { get; set; }

        /// <summary>
        /// Period End - نهاية الفترة
        /// End date of the billing period
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "period_end")]
        public DateTime PeriodEnd { get; set; }

        /// <summary>
        /// Due Date - تاريخ الاستحقاق
        /// Date when the invoice should be generated/sent
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "due_date")]
        public DateTime DueDate { get; set; }

        /// <summary>
        /// Payment Due Date - تاريخ استحقاق الدفع
        /// Date when payment is expected (DueDate + PaymentDueDays)
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "payment_due_date")]
        public DateTime PaymentDueDate { get; set; }

        /// <summary>
        /// Base Amount - المبلغ الأساسي
        /// Sum of unit prices for the period
        /// </summary>
        [SugarColumn(DecimalDigits = 4, IsNullable = false, ColumnName = "base_amount")]
        public decimal BaseAmount { get; set; }

        /// <summary>
        /// Adjustment Amount - مبلغ التعديلات
        /// Sum of all pricing rule adjustments (positive or negative)
        /// </summary>
        [SugarColumn(DecimalDigits = 4, IsNullable = false, ColumnName = "adjustment_amount")]
        public decimal AdjustmentAmount { get; set; } = 0;

        /// <summary>
        /// Subtotal - المجموع الفرعي
        /// BaseAmount + AdjustmentAmount (before tax)
        /// </summary>
        [SugarColumn(DecimalDigits = 4, IsNullable = false, ColumnName = "subtotal")]
        public decimal Subtotal { get; set; }

        /// <summary>
        /// Tax Amount - مبلغ الضريبة
        /// Calculated tax amount
        /// </summary>
        [SugarColumn(DecimalDigits = 4, IsNullable = false, ColumnName = "tax_amount")]
        public decimal TaxAmount { get; set; } = 0;

        /// <summary>
        /// Total Amount - المبلغ الإجمالي
        /// Final amount including tax (Subtotal + TaxAmount)
        /// </summary>
        [SugarColumn(DecimalDigits = 4, IsNullable = false, ColumnName = "total_amount")]
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Currency - العملة
        /// Currency code
        /// </summary>
        [SugarColumn(Length = 10, IsNullable = false, ColumnName = "currency")]
        public string Currency { get; set; } = "SAR";

        /// <summary>
        /// Status - الحالة
        /// Current status of the billing schedule entry
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "status")]
        public BillingScheduleStatus Status { get; set; } = BillingScheduleStatus.Pending;

        /// <summary>
        /// Invoice ID - معرف الفاتورة
        /// Foreign key to ARInvoice (set when invoice is generated)
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "invoice_id")]
        public Guid? InvoiceId { get; set; }

        /// <summary>
        /// Invoice Number - رقم الفاتورة
        /// Stored for quick reference
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnName = "invoice_number")]
        public string InvoiceNumber { get; set; }

        /// <summary>
        /// Invoiced Date - تاريخ الفوترة
        /// Date when invoice was generated
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "invoiced_date")]
        public DateTime? InvoicedDate { get; set; }

        /// <summary>
        /// Invoiced By - تمت الفوترة بواسطة
        /// User who generated the invoice
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnName = "invoiced_by")]
        public string InvoicedBy { get; set; }

        #region Cancellation/Skip Fields - حقول الإلغاء/التخطي

        /// <summary>
        /// Cancelled Date - تاريخ الإلغاء
        /// Date when this schedule was cancelled
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "cancelled_date")]
        public DateTime? CancelledDate { get; set; }

        /// <summary>
        /// Cancelled By - تم الإلغاء بواسطة
        /// User who cancelled this schedule
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnName = "cancelled_by")]
        public string CancelledBy { get; set; }

        /// <summary>
        /// Cancellation Reason - سبب الإلغاء
        /// Reason for cancellation
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnName = "cancellation_reason")]
        public string CancellationReason { get; set; }

        /// <summary>
        /// Skipped Date - تاريخ التخطي
        /// Date when this schedule was skipped
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "skipped_date")]
        public DateTime? SkippedDate { get; set; }

        /// <summary>
        /// Skipped By - تم التخطي بواسطة
        /// User who skipped this schedule
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnName = "skipped_by")]
        public string SkippedBy { get; set; }

        /// <summary>
        /// Skip Reason - سبب التخطي
        /// Reason for skipping (e.g., contract suspended)
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnName = "skip_reason")]
        public string SkipReason { get; set; }

        #endregion

        #region Calculation Details - تفاصيل الحساب

        /// <summary>
        /// Calculation Breakdown (JSON) - تفصيل الحساب
        /// JSON string containing detailed calculation breakdown
        /// </summary>
        [SugarColumn(ColumnDataType = StaticConfig.CodeFirst_BigString, IsNullable = true, ColumnName = "calculation_breakdown")]
        public string CalculationBreakdown { get; set; }

        /// <summary>
        /// Applied Rules (JSON) - القواعد المطبقة
        /// JSON array of pricing rules applied to this schedule
        /// </summary>
        [SugarColumn(ColumnDataType = StaticConfig.CodeFirst_BigString, IsNullable = true, ColumnName = "applied_rules")]
        public string AppliedRules { get; set; }

        /// <summary>
        /// Is Pro-Rated - مقسط نسبياً
        /// Whether this is a pro-rated period (partial month)
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "is_pro_rated")]
        public bool IsProRated { get; set; } = false;

        /// <summary>
        /// Pro-Rata Days - أيام التقسيط
        /// Number of days for pro-rated calculation
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "pro_rata_days")]
        public int? ProRataDays { get; set; }

        /// <summary>
        /// Full Period Days - أيام الفترة الكاملة
        /// Total days in the full billing period
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "full_period_days")]
        public int? FullPeriodDays { get; set; }

        #endregion

        #region Reference Fields - حقول المرجع

        /// <summary>
        /// Amendment ID - معرف التعديل
        /// If this schedule was created/modified by an amendment
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "amendment_id")]
        public Guid? AmendmentId { get; set; }

        /// <summary>
        /// Regenerated From - أعيد إنشاؤه من
        /// Previous schedule ID if this was regenerated
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "regenerated_from")]
        public Guid? RegeneratedFrom { get; set; }

        /// <summary>
        /// Generation Batch ID - معرف دفعة الإنشاء
        /// Batch ID when multiple schedules are generated together
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "generation_batch_id")]
        public Guid? GenerationBatchId { get; set; }

        /// <summary>
        /// Description - الوصف
        /// Auto-generated description for the billing period
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnName = "description")]
        public string Description { get; set; }

        /// <summary>
        /// Description (Arabic) - الوصف بالعربية
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnName = "description_ar")]
        public string DescriptionAr { get; set; }

        /// <summary>
        /// Notes - ملاحظات
        /// Internal notes
        /// </summary>
        [SugarColumn(Length = 1000, IsNullable = true, ColumnName = "notes")]
        public string Notes { get; set; }

        #endregion

        #region Notification Fields - حقول الإشعارات

        /// <summary>
        /// Reminder Sent - تم إرسال التذكير
        /// Whether a reminder has been sent for this schedule
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "reminder_sent")]
        public bool ReminderSent { get; set; } = false;

        /// <summary>
        /// Reminder Sent Date - تاريخ إرسال التذكير
        /// Date when reminder was sent
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "reminder_sent_date")]
        public DateTime? ReminderSentDate { get; set; }

        /// <summary>
        /// Overdue Notice Sent - تم إرسال إشعار التأخير
        /// Whether an overdue notice has been sent
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "overdue_notice_sent")]
        public bool OverdueNoticeSent { get; set; } = false;

        /// <summary>
        /// Overdue Notice Date - تاريخ إشعار التأخير
        /// Date when overdue notice was sent
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "overdue_notice_date")]
        public DateTime? OverdueNoticeDate { get; set; }

        #endregion

        #region Navigation Properties - خصائص الملاحة

        /// <summary>
        /// Contract - العقد
        /// Parent contract
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public ErpContract Contract { get; set; }

        /// <summary>
        /// Payment Status - حالة الدفع
        /// Payment tracking extension table (1:1)
        /// </summary>
        [Navigate(NavigateType.OneToOne, nameof(ScheduleId))]
        public ErpBillingPaymentStatus PaymentStatus { get; set; }

        /// <summary>
        /// Schedule Line Items - بنود الجدول
        /// Detailed line items for the schedule
        /// </summary>
        [Navigate(NavigateType.OneToMany, nameof(ErpContractBillingScheduleLine.ScheduleId))]
        public List<ErpContractBillingScheduleLine> Lines { get; set; }

        /// <summary>
        /// Contract Number - رقم العقد
        /// For display purposes
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public string ContractNumber { get; set; }

        /// <summary>
        /// Customer Name - اسم العميل
        /// For display purposes
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public string CustomerName { get; set; }

        /// <summary>
        /// Status Name - اسم الحالة
        /// For display purposes
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public string StatusName { get; set; }

        #endregion

        #region Computed Properties - الخصائص المحسوبة

        /// <summary>
        /// Paid Amount - المبلغ المدفوع
        /// Amount paid from payment status extension
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public decimal PaidAmount => PaymentStatus?.PaidAmount ?? 0;

        /// <summary>
        /// Remaining Amount - المبلغ المتبقي
        /// Remaining amount from payment status extension
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public decimal RemainingAmount => PaymentStatus?.RemainingAmount ?? TotalAmount;

        /// <summary>
        /// Is Overdue - متأخر
        /// Whether this schedule is overdue
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public bool IsOverdue => PaymentStatus?.IsOverdue ?? false;

        /// <summary>
        /// Days Overdue - أيام التأخير
        /// Number of days past payment due date
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public int? DaysOverdue => PaymentStatus?.DaysOverdue;

        /// <summary>
        /// Is Fully Paid - مدفوع بالكامل
        /// Whether this schedule is fully paid
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public bool IsFullyPaid => PaymentStatus?.IsFullyPaid ?? false;

        #endregion
    }

  
}
