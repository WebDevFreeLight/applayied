using System;
using System.ComponentModel.DataAnnotations;
using SqlSugar;
using ZR.Model.Contracts.Enums;

namespace ZR.Model.Contracts
{
    /// <summary>
    /// Contract Renewal - تجديد العقد
    /// Tracks contract renewal history and manages the renewal process
    /// </summary>
    [SugarTable("erp_contract_renewal", "Contract Renewal")]
    [SugarIndex("ix_renewal_contract", nameof(ContractId), OrderByType.Asc)]
    [SugarIndex("ix_renewal_number", nameof(RenewalNumber), OrderByType.Asc, IsUnique = true)]
    [SugarIndex("ix_renewal_status", nameof(Status), OrderByType.Asc)]
    public class ErpContractRenewal : SysBase
    {
        #region Primary Key

        /// <summary>
        /// Renewal ID - معرف التجديد
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, ColumnName = "renewal_id")]
        public Guid RenewalId { get; set; }

        #endregion

        #region Renewal Identification - تعريف التجديد

        /// <summary>
        /// Renewal Number - رقم التجديد
        /// Auto-generated: REN-{ContractNumber}-{Sequence}
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = false, ColumnName = "renewal_number")]
        public string RenewalNumber { get; set; }

        /// <summary>
        /// Contract ID - معرف العقد
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "contract_id")]
        public Guid ContractId { get; set; }

        /// <summary>
        /// Renewal Sequence - تسلسل التجديد
        /// 1 = First renewal, 2 = Second renewal, etc.
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "renewal_sequence")]
        public int RenewalSequence { get; set; }

        #endregion

        #region Original Contract Period - فترة العقد الأصلية

        /// <summary>
        /// Original Start Date - تاريخ البداية الأصلي
        /// The contract start date before this renewal
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "original_start_date")]
        public DateTime OriginalStartDate { get; set; }

        /// <summary>
        /// Original End Date - تاريخ النهاية الأصلي
        /// The contract end date before this renewal
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "original_end_date")]
        public DateTime OriginalEndDate { get; set; }

        /// <summary>
        /// Original Contract Value - قيمة العقد الأصلية
        /// Total contract value before renewal
        /// </summary>
        [SugarColumn(DecimalDigits = 2, IsNullable = false, ColumnName = "original_contract_value")]
        public decimal OriginalContractValue { get; set; }

        #endregion

        #region New Contract Period - فترة العقد الجديدة

        /// <summary>
        /// New Start Date - تاريخ البداية الجديد
        /// Typically = OriginalEndDate + 1 day
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "new_start_date")]
        public DateTime NewStartDate { get; set; }

        /// <summary>
        /// New End Date - تاريخ النهاية الجديد
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "new_end_date")]
        public DateTime NewEndDate { get; set; }

        /// <summary>
        /// Renewal Duration Months - مدة التجديد بالأشهر
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "renewal_duration_months")]
        public int RenewalDurationMonths { get; set; }

        /// <summary>
        /// New Contract Value - قيمة العقد الجديدة
        /// Total contract value after renewal
        /// </summary>
        [SugarColumn(DecimalDigits = 2, IsNullable = false, ColumnName = "new_contract_value")]
        public decimal NewContractValue { get; set; }

        #endregion

        #region Renewal Type and Status - نوع وحالة التجديد

        /// <summary>
        /// Renewal Type - نوع التجديد
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "renewal_type")]
        public RenewalType RenewalType { get; set; }

        /// <summary>
        /// Status - الحالة
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "status")]
        public RenewalStatus Status { get; set; }

        /// <summary>
        /// Is Auto Renewal - هل هو تجديد تلقائي
        /// True if this renewal was triggered automatically
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "is_auto_renewal")]
        public bool IsAutoRenewal { get; set; }

        #endregion

        #region Extension Flag - علم التمديد

        /// <summary>
        /// Is Extension - هل هو تمديد
        /// True if this is a simple term extension without creating a new contract.
        /// False if this creates a separate renewal contract.
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "is_extension")]
        public bool IsExtension { get; set; } = false;

        #endregion

        #region Terms Changes - تغييرات الشروط

        /// <summary>
        /// Terms Changed - هل تغيرت الشروط
        /// True if contract terms were modified during renewal
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "terms_changed")]
        public bool TermsChanged { get; set; }

        /// <summary>
        /// Terms Change Summary - ملخص تغيير الشروط
        /// Brief description of what terms changed
        /// </summary>
        [SugarColumn(Length = 1000, IsNullable = true, ColumnName = "terms_change_summary")]
        public string TermsChangeSummary { get; set; }

        /// <summary>
        /// New Payment Terms Days - أيام شروط الدفع الجديدة
        /// If payment terms changed
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "new_payment_terms")]
        public int? NewPaymentTerms { get; set; }

        /// <summary>
        /// New Billing Frequency - تكرار الفوترة الجديد
        /// If billing frequency changed
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "new_billing_frequency")]
        public BillingFrequency? NewBillingFrequency { get; set; }

        #endregion

        #region Notification and Dates - الإشعارات والتواريخ

        /// <summary>
        /// Renewal Notice Date - تاريخ إشعار التجديد
        /// Date when renewal notice was sent/received
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "renewal_notice_date")]
        public DateTime? RenewalNoticeDate { get; set; }

        /// <summary>
        /// Renewal Decision Deadline - الموعد النهائي لقرار التجديد
        /// Last date to decide on renewal
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "renewal_decision_deadline")]
        public DateTime? RenewalDecisionDeadline { get; set; }

        /// <summary>
        /// Customer Response Date - تاريخ رد العميل
        /// Date when customer responded to renewal offer
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "customer_response_date")]
        public DateTime? CustomerResponseDate { get; set; }

        /// <summary>
        /// Renewal Execution Date - تاريخ تنفيذ التجديد
        /// Date when renewal was executed/applied
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "renewal_execution_date")]
        public DateTime? RenewalExecutionDate { get; set; }

        #endregion

        #region Approval Workflow - سير عمل الموافقة

        /// <summary>
        /// Requires Approval - يتطلب موافقة
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "requires_approval")]
        public bool RequiresApproval { get; set; }

        /// <summary>
        /// Approved By - المعتمد من قبل
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "approved_by")]
        public long? ApprovedBy { get; set; }

        /// <summary>
        /// Approved Date - تاريخ الاعتماد
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "approved_date")]
        public DateTime? ApprovedDate { get; set; }

        /// <summary>
        /// Approval Notes - ملاحظات الموافقة
        /// </summary>
        [SugarColumn(Length = 1000, IsNullable = true, ColumnName = "approval_notes")]
        public string ApprovalNotes { get; set; }

        #endregion

        #region Billing Schedule Impact - تأثير جدول الفوترة

        /// <summary>
        /// Schedules Generated - الجداول المولدة
        /// Number of new billing schedules created
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "schedules_generated")]
        public int? SchedulesGenerated { get; set; }

        /// <summary>
        /// First New Schedule Date - تاريخ أول جدول جديد
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "first_new_schedule_date")]
        public DateTime? FirstNewScheduleDate { get; set; }

        /// <summary>
        /// Last New Schedule Date - تاريخ آخر جدول جديد
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "last_new_schedule_date")]
        public DateTime? LastNewScheduleDate { get; set; }

        /// <summary>
        /// Total New Schedules Amount - إجمالي مبلغ الجداول الجديدة
        /// </summary>
        [SugarColumn(DecimalDigits = 2, IsNullable = true, ColumnName = "total_new_schedules_amount")]
        public decimal? TotalNewSchedulesAmount { get; set; }

        #endregion

        #region Document Reference - مرجع المستند

        /// <summary>
        /// Renewal Document ID - معرف مستند التجديد
        /// Reference to the signed renewal document
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "renewal_document_id")]
        public Guid? RenewalDocumentId { get; set; }

        /// <summary>
        /// Renewal Document Number - رقم مستند التجديد
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true, ColumnName = "renewal_document_number")]
        public string RenewalDocumentNumber { get; set; }

        /// <summary>
        /// Amendment ID - معرف التعديل
        /// If renewal created an amendment record
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "amendment_id")]
        public Guid? AmendmentId { get; set; }

        #endregion

        #region Cancellation - الإلغاء

        /// <summary>
        /// Is Cancelled - ملغي
        /// True if renewal was cancelled before execution
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "is_cancelled")]
        public bool IsCancelled { get; set; }

        /// <summary>
        /// Cancellation Date - تاريخ الإلغاء
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "cancellation_date")]
        public DateTime? CancellationDate { get; set; }

        /// <summary>
        /// Cancellation Reason - سبب الإلغاء
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnName = "cancellation_reason")]
        public string CancellationReason { get; set; }

        /// <summary>
        /// Cancelled By - ملغي من قبل
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "cancelled_by")]
        public long? CancelledBy { get; set; }

        #endregion

        #region Notes - الملاحظات

        /// <summary>
        /// Internal Notes - ملاحظات داخلية
        /// </summary>
        [SugarColumn(Length = 2000, IsNullable = true, ColumnName = "internal_notes")]
        public string InternalNotes { get; set; }

        /// <summary>
        /// Customer Notes - ملاحظات العميل
        /// Notes from customer regarding the renewal
        /// </summary>
        [SugarColumn(Length = 2000, IsNullable = true, ColumnName = "customer_notes")]
        public string CustomerNotes { get; set; }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// Contract - العقد
        /// </summary>
        [Navigate(NavigateType.OneToOne, nameof(ContractId))]
        public ErpContract Contract { get; set; }

        /// <summary>
        /// Amendment - التعديل
        /// </summary>
        [Navigate(NavigateType.OneToOne, nameof(AmendmentId))]
        public ErpContractAmendment Amendment { get; set; }

        #endregion
    }

    /// <summary>
    /// Renewal Status - حالة التجديد
    /// </summary>
    public enum RenewalStatus
    {
        /// <summary>
        /// Pending - معلق
        /// Renewal created, awaiting decision
        /// </summary>
        Pending = 0,

        /// <summary>
        /// Offered - معروض
        /// Renewal offer sent to customer
        /// </summary>
        Offered = 1,

        /// <summary>
        /// Under Review - قيد المراجعة
        /// Customer reviewing the renewal terms
        /// </summary>
        UnderReview = 2,

        /// <summary>
        /// Negotiating - قيد التفاوض
        /// Terms being negotiated
        /// </summary>
        Negotiating = 3,

        /// <summary>
        /// Approved - معتمد
        /// Renewal approved, pending execution
        /// </summary>
        Approved = 4,

        /// <summary>
        /// Executed - منفذ
        /// Renewal has been applied to the contract
        /// </summary>
        Executed = 5,

        /// <summary>
        /// Declined - مرفوض
        /// Customer declined renewal
        /// </summary>
        Declined = 6,

        /// <summary>
        /// Cancelled - ملغي
        /// Renewal cancelled before execution
        /// </summary>
        Cancelled = 7,

        /// <summary>
        /// Expired - منتهي
        /// Renewal offer expired without decision
        /// </summary>
        Expired = 8
    }
}
