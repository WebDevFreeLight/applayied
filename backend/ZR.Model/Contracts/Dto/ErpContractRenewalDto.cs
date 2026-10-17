using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MiniExcelLibs.Attributes;
using ZR.Model.Contracts.Enums;

namespace ZR.Model.Contracts.Dto
{
    /// <summary>
    /// Contract Renewal Query DTO - استعلام تجديد العقد
    /// </summary>
    public class ContractRenewalQueryDto : PagerInfo
    {
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
        /// Renewal Type - نوع التجديد
        /// </summary>
        public RenewalType? RenewalType { get; set; }

        /// <summary>
        /// Status - الحالة
        /// </summary>
        public RenewalStatus? Status { get; set; }

        /// <summary>
        /// Is Auto Renewal - تجديد تلقائي
        /// </summary>
        public bool? IsAutoRenewal { get; set; }

        /// <summary>
        /// New Start Date From - من تاريخ البداية الجديد
        /// </summary>
        public DateTime? NewStartDateFrom { get; set; }

        /// <summary>
        /// New Start Date To - إلى تاريخ البداية الجديد
        /// </summary>
        public DateTime? NewStartDateTo { get; set; }

        /// <summary>
        /// Renewal Decision Deadline From - من الموعد النهائي لقرار التجديد
        /// </summary>
        public DateTime? DecisionDeadlineFrom { get; set; }

        /// <summary>
        /// Renewal Decision Deadline To - إلى الموعد النهائي لقرار التجديد
        /// </summary>
        public DateTime? DecisionDeadlineTo { get; set; }

        /// <summary>
        /// Company ID - معرف الشركة
        /// </summary>
        public int? CompanyID { get; set; }

        /// <summary>
        /// Branch ID - معرف الفرع
        /// </summary>
        public int? BranchID { get; set; }
    }

    /// <summary>
    /// Contract Renewal List DTO - قائمة تجديد العقد
    /// </summary>
    public class ErpContractRenewalListDto
    {
        [ExcelColumn(Name = "Renewal ID")]
        public Guid RenewalId { get; set; }

        [ExcelColumn(Name = "Renewal Number")]
        public string RenewalNumber { get; set; }

        public Guid ContractId { get; set; }

        [ExcelColumn(Name = "Contract Number")]
        public string ContractNumber { get; set; }

        [ExcelColumn(Name = "Customer Name")]
        public string CustomerName { get; set; }

        [ExcelColumn(Name = "Renewal Sequence")]
        public int RenewalSequence { get; set; }

        [ExcelColumn(Name = "Renewal Type")]
        public RenewalType RenewalType { get; set; }

        public string RenewalTypeName { get; set; }

        [ExcelColumn(Name = "Status")]
        public RenewalStatus Status { get; set; }

        public string StatusName { get; set; }

        [ExcelColumn(Name = "Is Auto Renewal")]
        public bool IsAutoRenewal { get; set; }

        [ExcelColumn(Name = "Original End Date", Format = "yyyy-MM-dd")]
        public DateTime OriginalEndDate { get; set; }

        [ExcelColumn(Name = "New Start Date", Format = "yyyy-MM-dd")]
        public DateTime NewStartDate { get; set; }

        [ExcelColumn(Name = "New End Date", Format = "yyyy-MM-dd")]
        public DateTime NewEndDate { get; set; }

        [ExcelColumn(Name = "Duration (Months)")]
        public int RenewalDurationMonths { get; set; }

        [ExcelColumn(Name = "Previous Base Amount")]
        public decimal PreviousBaseAmount { get; set; }

        [ExcelColumn(Name = "New Base Amount")]
        public decimal NewBaseAmount { get; set; }

        [ExcelColumn(Name = "New Contract Value")]
        public decimal NewContractValue { get; set; }

        [ExcelColumn(Name = "Decision Deadline", Format = "yyyy-MM-dd")]
        public DateTime? RenewalDecisionDeadline { get; set; }

        [ExcelColumn(Name = "Execution Date", Format = "yyyy-MM-dd")]
        public DateTime? RenewalExecutionDate { get; set; }

        public string Currency { get; set; }
    }

    /// <summary>
    /// Contract Renewal Detail DTO - تفاصيل تجديد العقد
    /// </summary>
    public class ErpContractRenewalDetailDto
    {
        // Identification
        public Guid RenewalId { get; set; }
        public string RenewalNumber { get; set; }
        public Guid ContractId { get; set; }
        public string ContractNumber { get; set; }
        public string ContractTitle { get; set; }
        public int RenewalSequence { get; set; }

        // Customer Info
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerCode { get; set; }

        // Original Contract Period
        public DateTime OriginalStartDate { get; set; }
        public DateTime OriginalEndDate { get; set; }
        public decimal OriginalContractValue { get; set; }

        // New Contract Period
        public DateTime NewStartDate { get; set; }
        public DateTime NewEndDate { get; set; }
        public int RenewalDurationMonths { get; set; }
        public decimal NewContractValue { get; set; }

        // Renewal Type and Status
        public RenewalType RenewalType { get; set; }
        public string RenewalTypeName { get; set; }
        public RenewalStatus Status { get; set; }
        public string StatusName { get; set; }
        public bool IsAutoRenewal { get; set; }

        // Price Adjustment
        public int PriceAdjustmentType { get; set; }
        public string PriceAdjustmentTypeName { get; set; }
        public decimal? PriceAdjustmentPercentage { get; set; }
        public decimal? PriceAdjustmentAmount { get; set; }
        public decimal NewBaseAmount { get; set; }
        public decimal PreviousBaseAmount { get; set; }
        public decimal BaseAmountChange { get; set; }
        public decimal BaseAmountChangePercentage { get; set; }

        // Terms Changes
        public bool TermsChanged { get; set; }
        public string TermsChangeSummary { get; set; }
        public int? NewPaymentTerms { get; set; }
        public BillingFrequency? NewBillingFrequency { get; set; }
        public string NewBillingFrequencyName { get; set; }

        // Notification and Dates
        public DateTime? RenewalNoticeDate { get; set; }
        public DateTime? RenewalDecisionDeadline { get; set; }
        public DateTime? CustomerResponseDate { get; set; }
        public DateTime? RenewalExecutionDate { get; set; }

        // Approval
        public bool RequiresApproval { get; set; }
        public long? ApprovedBy { get; set; }
        public string ApprovedByName { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public string ApprovalNotes { get; set; }

        // Billing Schedule Impact
        public int? SchedulesGenerated { get; set; }
        public DateTime? FirstNewScheduleDate { get; set; }
        public DateTime? LastNewScheduleDate { get; set; }
        public decimal? TotalNewSchedulesAmount { get; set; }

        // Documents
        public Guid? RenewalDocumentId { get; set; }
        public string RenewalDocumentNumber { get; set; }
        public Guid? AmendmentId { get; set; }
        public string AmendmentNumber { get; set; }

        // Cancellation
        public bool IsCancelled { get; set; }
        public DateTime? CancellationDate { get; set; }
        public string CancellationReason { get; set; }

        // Notes
        public string InternalNotes { get; set; }
        public string CustomerNotes { get; set; }

        // Currency
        public string Currency { get; set; }

        // Audit
        public string Create_by { get; set; }
        public DateTime? Create_time { get; set; }
        public string Update_by { get; set; }
        public DateTime? Update_time { get; set; }
    }

    /// <summary>
    /// Create Contract Renewal DTO - إنشاء تجديد العقد
    /// </summary>
    public class CreateContractRenewalDto
    {
        /// <summary>
        /// Contract ID - معرف العقد
        /// </summary>
        [Required(ErrorMessage = "Contract ID is required")]
        public Guid ContractId { get; set; }

        /// <summary>
        /// Renewal Type - نوع التجديد
        /// </summary>
        [Required(ErrorMessage = "Renewal type is required")]
        public RenewalType RenewalType { get; set; }

        /// <summary>
        /// Renewal Duration Months - مدة التجديد بالأشهر
        /// </summary>
        [Required(ErrorMessage = "Renewal duration is required")]
        [Range(1, 120, ErrorMessage = "Duration must be between 1 and 120 months")]
        public int RenewalDurationMonths { get; set; }

        /// <summary>
        /// Is Auto Renewal - هل هو تجديد تلقائي
        /// </summary>
        public bool IsAutoRenewal { get; set; }

        /// <summary>
        /// Price Adjustment Type - نوع تعديل السعر
        /// 0 = No Change, 1 = Fixed Increase, 2 = Percentage Increase, 3 = New Price, 4 = Negotiated
        /// </summary>
        public int PriceAdjustmentType { get; set; }

        /// <summary>
        /// Price Adjustment Percentage - نسبة تعديل السعر
        /// Required if PriceAdjustmentType = 2
        /// </summary>
        [Range(-100, 1000, ErrorMessage = "Percentage must be between -100 and 1000")]
        public decimal? PriceAdjustmentPercentage { get; set; }

        /// <summary>
        /// Price Adjustment Amount - مبلغ تعديل السعر
        /// Required if PriceAdjustmentType = 1
        /// </summary>
        public decimal? PriceAdjustmentAmount { get; set; }

        /// <summary>
        /// New Base Amount - المبلغ الأساسي الجديد
        /// Required if PriceAdjustmentType = 3 or 4
        /// </summary>
        public decimal? NewBaseAmount { get; set; }

        /// <summary>
        /// Terms Changed - هل تغيرت الشروط
        /// </summary>
        public bool TermsChanged { get; set; }

        /// <summary>
        /// Terms Change Summary - ملخص تغيير الشروط
        /// </summary>
        [StringLength(1000)]
        public string TermsChangeSummary { get; set; }

        /// <summary>
        /// New Payment Terms - شروط الدفع الجديدة
        /// </summary>
        [Range(0, 365)]
        public int? NewPaymentTerms { get; set; }

        /// <summary>
        /// New Billing Frequency - تكرار الفوترة الجديد
        /// </summary>
        public BillingFrequency? NewBillingFrequency { get; set; }

        /// <summary>
        /// Renewal Notice Date - تاريخ إشعار التجديد
        /// </summary>
        public DateTime? RenewalNoticeDate { get; set; }

        /// <summary>
        /// Renewal Decision Deadline - الموعد النهائي لقرار التجديد
        /// </summary>
        public DateTime? RenewalDecisionDeadline { get; set; }

        /// <summary>
        /// Requires Approval - يتطلب موافقة
        /// </summary>
        public bool RequiresApproval { get; set; }

        /// <summary>
        /// Renewal Document Number - رقم مستند التجديد
        /// </summary>
        [StringLength(100)]
        public string RenewalDocumentNumber { get; set; }

        /// <summary>
        /// Internal Notes - ملاحظات داخلية
        /// </summary>
        [StringLength(2000)]
        public string InternalNotes { get; set; }

        /// <summary>
        /// Customer Notes - ملاحظات العميل
        /// </summary>
        [StringLength(2000)]
        public string CustomerNotes { get; set; }
    }

    /// <summary>
    /// Create Auto Renewal DTO - إنشاء تجديد تلقائي
    /// Used by scheduled job for auto-renewals
    /// </summary>
    public class CreateAutoRenewalDto
    {
        /// <summary>
        /// Contract ID - معرف العقد
        /// </summary>
        [Required]
        public Guid ContractId { get; set; }

        /// <summary>
        /// Use Contract Default Duration - استخدام مدة العقد الافتراضية
        /// If true, uses the contract's renewal terms
        /// </summary>
        public bool UseContractDefaults { get; set; } = true;

        /// <summary>
        /// Override Duration Months - تجاوز مدة الأشهر
        /// If UseContractDefaults is false
        /// </summary>
        public int? OverrideDurationMonths { get; set; }

        /// <summary>
        /// Override Price Adjustment Percentage - تجاوز نسبة تعديل السعر
        /// </summary>
        public decimal? OverridePriceAdjustmentPercentage { get; set; }
    }

    /// <summary>
    /// Update Contract Renewal DTO - تحديث تجديد العقد
    /// </summary>
    public class UpdateContractRenewalDto
    {
        /// <summary>
        /// Renewal Duration Months - مدة التجديد بالأشهر
        /// </summary>
        [Range(1, 120)]
        public int? RenewalDurationMonths { get; set; }

        /// <summary>
        /// Price Adjustment Type - نوع تعديل السعر
        /// </summary>
        public int? PriceAdjustmentType { get; set; }

        /// <summary>
        /// Price Adjustment Percentage - نسبة تعديل السعر
        /// </summary>
        [Range(-100, 1000)]
        public decimal? PriceAdjustmentPercentage { get; set; }

        /// <summary>
        /// Price Adjustment Amount - مبلغ تعديل السعر
        /// </summary>
        public decimal? PriceAdjustmentAmount { get; set; }

        /// <summary>
        /// New Base Amount - المبلغ الأساسي الجديد
        /// </summary>
        public decimal? NewBaseAmount { get; set; }

        /// <summary>
        /// Terms Changed - هل تغيرت الشروط
        /// </summary>
        public bool? TermsChanged { get; set; }

        /// <summary>
        /// Terms Change Summary - ملخص تغيير الشروط
        /// </summary>
        [StringLength(1000)]
        public string TermsChangeSummary { get; set; }

        /// <summary>
        /// New Payment Terms - شروط الدفع الجديدة
        /// </summary>
        [Range(0, 365)]
        public int? NewPaymentTerms { get; set; }

        /// <summary>
        /// New Billing Frequency - تكرار الفوترة الجديد
        /// </summary>
        public BillingFrequency? NewBillingFrequency { get; set; }

        /// <summary>
        /// Renewal Decision Deadline - الموعد النهائي لقرار التجديد
        /// </summary>
        public DateTime? RenewalDecisionDeadline { get; set; }

        /// <summary>
        /// Internal Notes - ملاحظات داخلية
        /// </summary>
        [StringLength(2000)]
        public string InternalNotes { get; set; }

        /// <summary>
        /// Customer Notes - ملاحظات العميل
        /// </summary>
        [StringLength(2000)]
        public string CustomerNotes { get; set; }
    }

    /// <summary>
    /// Execute Renewal DTO - تنفيذ التجديد
    /// </summary>
    public class ExecuteRenewalDto
    {
        /// <summary>
        /// Renewal ID - معرف التجديد
        /// </summary>
        [Required]
        public Guid RenewalId { get; set; }

        /// <summary>
        /// Generate Billing Schedules - توليد جداول الفوترة
        /// </summary>
        public bool GenerateBillingSchedules { get; set; } = true;

        /// <summary>
        /// Execution Notes - ملاحظات التنفيذ
        /// </summary>
        [StringLength(1000)]
        public string ExecutionNotes { get; set; }
    }

    /// <summary>
    /// Approve Renewal DTO - اعتماد التجديد
    /// </summary>
    public class ApproveRenewalDto
    {
        /// <summary>
        /// Approval Notes - ملاحظات الموافقة
        /// </summary>
        [StringLength(1000)]
        public string ApprovalNotes { get; set; }

        /// <summary>
        /// Auto Execute After Approval - تنفيذ تلقائي بعد الموافقة
        /// </summary>
        public bool AutoExecute { get; set; }
    }

    /// <summary>
    /// Decline Renewal DTO - رفض التجديد
    /// </summary>
    public class DeclineRenewalDto
    {
        /// <summary>
        /// Decline Reason - سبب الرفض
        /// </summary>
        [Required(ErrorMessage = "Decline reason is required")]
        [StringLength(1000)]
        public string DeclineReason { get; set; }

        /// <summary>
        /// Create Termination - إنشاء إنهاء
        /// If true, creates a termination record for non-renewal
        /// </summary>
        public bool CreateTermination { get; set; }
    }

    /// <summary>
    /// Cancel Renewal DTO - إلغاء التجديد
    /// </summary>
    public class CancelRenewalDto
    {
        /// <summary>
        /// Cancellation Reason - سبب الإلغاء
        /// </summary>
        [Required(ErrorMessage = "Cancellation reason is required")]
        [StringLength(500)]
        public string CancellationReason { get; set; }
    }

    /// <summary>
    /// Renewal Preview DTO - معاينة التجديد
    /// Shows what will happen when renewal is executed
    /// </summary>
    public class RenewalPreviewDto
    {
        // Contract Changes
        public DateTime CurrentEndDate { get; set; }
        public DateTime NewEndDate { get; set; }
        public int AdditionalMonths { get; set; }

        // Financial Changes
        public decimal CurrentBaseAmount { get; set; }
        public decimal NewBaseAmount { get; set; }
        public decimal BaseAmountChange { get; set; }
        public decimal BaseAmountChangePercentage { get; set; }

        public decimal CurrentContractValue { get; set; }
        public decimal NewContractValue { get; set; }
        public decimal ContractValueChange { get; set; }

        // Billing Schedule Preview
        public int SchedulesToGenerate { get; set; }
        public DateTime FirstSchedulePeriodStart { get; set; }
        public DateTime LastSchedulePeriodEnd { get; set; }
        public decimal TotalSchedulesAmount { get; set; }
        public List<RenewalSchedulePreviewDto> SchedulePreviews { get; set; } = new();

        // Terms Changes
        public bool HasTermsChanges { get; set; }
        public List<string> TermsChangeDescriptions { get; set; } = new();

        // Warnings
        public List<string> Warnings { get; set; } = new();
    }

    /// <summary>
    /// Renewal Schedule Preview DTO - معاينة جدول التجديد
    /// </summary>
    public class RenewalSchedulePreviewDto
    {
        public int ScheduleNumber { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public DateTime DueDate { get; set; }
        public decimal BaseAmount { get; set; }
        public decimal AdjustmentAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
    }

    /// <summary>
    /// Contracts Due for Renewal Query DTO - استعلام العقود المستحقة للتجديد
    /// </summary>
    public class ContractsDueForRenewalQueryDto : PagerInfo
    {
        /// <summary>
        /// Days Until Expiry - أيام حتى الانتهاء
        /// Find contracts expiring within this many days
        /// </summary>
        public int DaysUntilExpiry { get; set; } = 90;

        /// <summary>
        /// Include Auto Renew Only - تضمين التجديد التلقائي فقط
        /// </summary>
        public bool? AutoRenewOnly { get; set; }

        /// <summary>
        /// Customer ID - معرف العميل
        /// </summary>
        public Guid? CustomerId { get; set; }

        /// <summary>
        /// Contract Type - نوع العقد
        /// </summary>
        public ContractType? ContractType { get; set; }

        /// <summary>
        /// Company ID - معرف الشركة
        /// </summary>
        public int? CompanyID { get; set; }

        /// <summary>
        /// Branch ID - معرف الفرع
        /// </summary>
        public int? BranchID { get; set; }
    }

    /// <summary>
    /// Contract Due for Renewal DTO - عقد مستحق للتجديد
    /// </summary>
    public class ContractDueForRenewalDto
    {
        public Guid ContractId { get; set; }
        public string ContractNumber { get; set; }
        public string ContractTitle { get; set; }
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; }
        public ContractType ContractType { get; set; }
        public string ContractTypeName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int DaysUntilExpiry { get; set; }
        public decimal BaseAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public bool AutoRenew { get; set; }
        public RenewalType? RenewalType { get; set; }
        public string RenewalTypeName { get; set; }
        public int? DefaultRenewalMonths { get; set; }
        public decimal? DefaultEscalationPercentage { get; set; }
        public int TotalRenewals { get; set; }
        public DateTime? LastRenewalDate { get; set; }
        public bool HasPendingRenewal { get; set; }
        public string Currency { get; set; }
    }

    /// <summary>
    /// Renewal Execution Result DTO - نتيجة تنفيذ التجديد
    /// </summary>
    public class RenewalExecutionResultDto
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public Guid RenewalId { get; set; }
        public string RenewalNumber { get; set; }
        public DateTime NewContractEndDate { get; set; }
        public decimal NewContractValue { get; set; }
        public int SchedulesGenerated { get; set; }
        public decimal TotalSchedulesAmount { get; set; }
        public Guid? AmendmentId { get; set; }
        public DateTime ExecutedAt { get; set; }
    }
}
