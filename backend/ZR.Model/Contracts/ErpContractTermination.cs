using System;
using System.ComponentModel.DataAnnotations;
using SqlSugar;
using ZR.Model.Contracts.Enums;

namespace ZR.Model.Contracts
{
    /// <summary>
    /// Contract Termination - إنهاء العقد
    /// Tracks contract termination details, financial settlements, and related actions
    /// </summary>
    [SugarTable("erp_contract_termination", "Contract Termination")]
    [SugarIndex("ix_termination_contract", nameof(ContractId), OrderByType.Asc)]
    [SugarIndex("ix_termination_number", nameof(TerminationNumber), OrderByType.Asc, IsUnique = true)]
    [SugarIndex("ix_termination_status", nameof(Status), OrderByType.Asc)]
    public class ErpContractTermination : SysBase
    {
        #region Primary Key

        /// <summary>
        /// Termination ID - معرف الإنهاء
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, ColumnName = "termination_id")]
        public Guid TerminationId { get; set; }

        #endregion

        #region Termination Identification - تعريف الإنهاء

        /// <summary>
        /// Termination Number - رقم الإنهاء
        /// Auto-generated: TRM-{ContractNumber}-{Date}
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = false, ColumnName = "termination_number")]
        public string TerminationNumber { get; set; }

        /// <summary>
        /// Contract ID - معرف العقد
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "contract_id")]
        public Guid ContractId { get; set; }

        #endregion

        #region Termination Details - تفاصيل الإنهاء

        /// <summary>
        /// Termination Reason - سبب الإنهاء
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "termination_reason")]
        public TerminationReason TerminationReason { get; set; }

        /// <summary>
        /// Termination Type - نوع الإنهاء
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "termination_type")]
        public TerminationType TerminationType { get; set; }

        /// <summary>
        /// Initiated By - بدأ بواسطة
        /// 0 = Landlord/Company, 1 = Tenant/Customer, 2 = Mutual Agreement, 3 = Court Order, 4 = Force Majeure
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "initiated_by")]
        public int InitiatedBy { get; set; }

        /// <summary>
        /// Status - الحالة
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "status")]
        public TerminationStatus Status { get; set; }

        /// <summary>
        /// Termination Reason Detail - تفاصيل سبب الإنهاء
        /// Detailed explanation of why contract is being terminated
        /// </summary>
        [SugarColumn(Length = 2000, IsNullable = true, ColumnName = "termination_reason_detail")]
        public string TerminationReasonDetail { get; set; }

        #endregion

        #region Dates - التواريخ

        /// <summary>
        /// Notice Date - تاريخ الإشعار
        /// Date when termination notice was given
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "notice_date")]
        public DateTime NoticeDate { get; set; }

        /// <summary>
        /// Effective Date - تاريخ السريان
        /// Date when termination becomes effective
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "effective_date")]
        public DateTime EffectiveDate { get; set; }

        /// <summary>
        /// Original End Date - تاريخ النهاية الأصلي
        /// The original contract end date before termination
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "original_end_date")]
        public DateTime OriginalEndDate { get; set; }

        /// <summary>
        /// Notice Period Days - أيام فترة الإشعار
        /// Required notice period in days
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "notice_period_days")]
        public int NoticePeriodDays { get; set; }

        /// <summary>
        /// Early Termination Days - أيام الإنهاء المبكر
        /// Number of days before original end date
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "early_termination_days")]
        public int EarlyTerminationDays { get; set; }

        /// <summary>
        /// Vacation Date - تاريخ الإخلاء
        /// Date when property/equipment must be vacated/returned
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "vacation_date")]
        public DateTime? VacationDate { get; set; }

        /// <summary>
        /// Actual Vacation Date - تاريخ الإخلاء الفعلي
        /// Date when property/equipment was actually vacated/returned
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "actual_vacation_date")]
        public DateTime? ActualVacationDate { get; set; }

        #endregion

        #region Financial Summary - الملخص المالي

        /// <summary>
        /// Total Contract Value - إجمالي قيمة العقد
        /// Original total contract value
        /// </summary>
        [SugarColumn(DecimalDigits = 2, IsNullable = false, ColumnName = "total_contract_value")]
        public decimal TotalContractValue { get; set; }

        /// <summary>
        /// Amount Billed - المبلغ المفوتر
        /// Total amount already billed
        /// </summary>
        [SugarColumn(DecimalDigits = 2, IsNullable = false, ColumnName = "amount_billed")]
        public decimal AmountBilled { get; set; }

        /// <summary>
        /// Amount Paid - المبلغ المدفوع
        /// Total amount already paid by customer
        /// </summary>
        [SugarColumn(DecimalDigits = 2, IsNullable = false, ColumnName = "amount_paid")]
        public decimal AmountPaid { get; set; }

        /// <summary>
        /// Outstanding Balance - الرصيد المستحق
        /// Amount still owed by customer
        /// </summary>
        [SugarColumn(DecimalDigits = 2, IsNullable = false, ColumnName = "outstanding_balance")]
        public decimal OutstandingBalance { get; set; }

        /// <summary>
        /// Remaining Contract Value - قيمة العقد المتبقية
        /// Value of unfulfilled contract period
        /// </summary>
        [SugarColumn(DecimalDigits = 2, IsNullable = false, ColumnName = "remaining_contract_value")]
        public decimal RemainingContractValue { get; set; }

        #endregion

        #region Penalty and Fees - الغرامات والرسوم

        /// <summary>
        /// Early Termination Penalty Applicable - غرامة الإنهاء المبكر قابلة للتطبيق
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "penalty_applicable")]
        public bool PenaltyApplicable { get; set; }

        /// <summary>
        /// Penalty Calculation Type - نوع حساب الغرامة
        /// 0 = Fixed Amount, 1 = Percentage of Remaining, 2 = Months of Rent, 3 = Custom
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "penalty_calculation_type")]
        public int? PenaltyCalculationType { get; set; }

        /// <summary>
        /// Penalty Percentage - نسبة الغرامة
        /// If percentage-based
        /// </summary>
        [SugarColumn(DecimalDigits = 4, IsNullable = true, ColumnName = "penalty_percentage")]
        public decimal? PenaltyPercentage { get; set; }

        /// <summary>
        /// Penalty Months - أشهر الغرامة
        /// Number of months used for penalty calculation
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "penalty_months")]
        public int? PenaltyMonths { get; set; }

        /// <summary>
        /// Penalty Amount - مبلغ الغرامة
        /// Calculated early termination penalty
        /// </summary>
        [SugarColumn(DecimalDigits = 2, IsNullable = true, ColumnName = "penalty_amount")]
        public decimal? PenaltyAmount { get; set; }

        /// <summary>
        /// Penalty Waived - الغرامة معفاة
        /// True if penalty was waived
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "penalty_waived")]
        public bool PenaltyWaived { get; set; }

        /// <summary>
        /// Penalty Waiver Reason - سبب إعفاء الغرامة
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnName = "penalty_waiver_reason")]
        public string PenaltyWaiverReason { get; set; }

        /// <summary>
        /// Administration Fee - رسوم الإدارة
        /// </summary>
        [SugarColumn(DecimalDigits = 2, IsNullable = true, ColumnName = "administration_fee")]
        public decimal? AdministrationFee { get; set; }

        /// <summary>
        /// Other Fees - رسوم أخرى
        /// Any additional fees
        /// </summary>
        [SugarColumn(DecimalDigits = 2, IsNullable = true, ColumnName = "other_fees")]
        public decimal? OtherFees { get; set; }

        /// <summary>
        /// Other Fees Description - وصف الرسوم الأخرى
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnName = "other_fees_description")]
        public string OtherFeesDescription { get; set; }

        #endregion

        #region Security Deposit Settlement - تسوية الضمان

        /// <summary>
        /// Security Deposit Amount - مبلغ الضمان
        /// Original security deposit held
        /// </summary>
        [SugarColumn(DecimalDigits = 2, IsNullable = true, ColumnName = "security_deposit_amount")]
        public decimal? SecurityDepositAmount { get; set; }

        /// <summary>
        /// Deposit Deductions - خصومات الضمان
        /// Amount deducted from security deposit
        /// </summary>
        [SugarColumn(DecimalDigits = 2, IsNullable = true, ColumnName = "deposit_deductions")]
        public decimal? DepositDeductions { get; set; }

        /// <summary>
        /// Deposit Deduction Details - تفاصيل خصومات الضمان
        /// JSON array of deduction items with reasons and amounts
        /// </summary>
        [SugarColumn(ColumnDataType = "text", IsNullable = true, ColumnName = "deposit_deduction_details")]
        public string DepositDeductionDetailsJson { get; set; }

        /// <summary>
        /// Deposit Refund Amount - مبلغ استرداد الضمان
        /// Amount to be refunded to customer
        /// </summary>
        [SugarColumn(DecimalDigits = 2, IsNullable = true, ColumnName = "deposit_refund_amount")]
        public decimal? DepositRefundAmount { get; set; }

        /// <summary>
        /// Deposit Refund Status - حالة استرداد الضمان
        /// 0 = Pending, 1 = Processing, 2 = Refunded, 3 = Applied to Balance, 4 = Forfeited
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "deposit_refund_status")]
        public int? DepositRefundStatus { get; set; }

        /// <summary>
        /// Deposit Refund Date - تاريخ استرداد الضمان
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "deposit_refund_date")]
        public DateTime? DepositRefundDate { get; set; }

        #endregion

        #region Final Settlement - التسوية النهائية

        /// <summary>
        /// Settlement Amount - مبلغ التسوية
        /// Net amount due (positive = customer owes, negative = refund due)
        /// </summary>
        [SugarColumn(DecimalDigits = 2, IsNullable = false, ColumnName = "settlement_amount")]
        public decimal SettlementAmount { get; set; }

        /// <summary>
        /// Settlement Status - حالة التسوية
        /// 0 = Pending, 1 = Partial, 2 = Completed, 3 = Disputed, 4 = Written Off
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "settlement_status")]
        public int SettlementStatus { get; set; }

        /// <summary>
        /// Settlement Date - تاريخ التسوية
        /// Date when settlement was completed
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "settlement_date")]
        public DateTime? SettlementDate { get; set; }

        /// <summary>
        /// Settlement Notes - ملاحظات التسوية
        /// </summary>
        [SugarColumn(Length = 2000, IsNullable = true, ColumnName = "settlement_notes")]
        public string SettlementNotes { get; set; }

        #endregion

        #region Billing Schedule Impact - تأثير جدول الفوترة

        /// <summary>
        /// Schedules Cancelled - الجداول الملغاة
        /// Number of pending billing schedules cancelled
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "schedules_cancelled")]
        public int? SchedulesCancelled { get; set; }

        /// <summary>
        /// Cancelled Schedules Amount - مبلغ الجداول الملغاة
        /// Total value of cancelled billing schedules
        /// </summary>
        [SugarColumn(DecimalDigits = 2, IsNullable = true, ColumnName = "cancelled_schedules_amount")]
        public decimal? CancelledSchedulesAmount { get; set; }

        /// <summary>
        /// Final Billing Schedule ID - معرف جدول الفوترة النهائي
        /// The last billing schedule before termination (may be prorated)
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "final_billing_schedule_id")]
        public Guid? FinalBillingScheduleId { get; set; }

        /// <summary>
        /// Final Invoice Required - فاتورة نهائية مطلوبة
        /// True if a final prorated invoice is needed
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "final_invoice_required")]
        public bool FinalInvoiceRequired { get; set; }

        /// <summary>
        /// Final Invoice ID - معرف الفاتورة النهائية
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "final_invoice_id")]
        public Guid? FinalInvoiceId { get; set; }

        /// <summary>
        /// Penalty Invoice ID - معرف فاتورة الغرامة
        /// Invoice for early termination penalty
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "penalty_invoice_id")]
        public Guid? PenaltyInvoiceId { get; set; }

        #endregion

        #region Unit Handover - تسليم الوحدات

        /// <summary>
        /// Units to Return - الوحدات للإرجاع
        /// Number of units that need to be returned
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "units_to_return")]
        public int? UnitsToReturn { get; set; }

        /// <summary>
        /// Units Returned - الوحدات المرجعة
        /// Number of units actually returned
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "units_returned")]
        public int? UnitsReturned { get; set; }

        /// <summary>
        /// Handover Inspection Required - فحص التسليم مطلوب
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "handover_inspection_required")]
        public bool HandoverInspectionRequired { get; set; }

        /// <summary>
        /// Handover Inspection Date - تاريخ فحص التسليم
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "handover_inspection_date")]
        public DateTime? HandoverInspectionDate { get; set; }

        /// <summary>
        /// Handover Inspection Notes - ملاحظات فحص التسليم
        /// </summary>
        [SugarColumn(Length = 2000, IsNullable = true, ColumnName = "handover_inspection_notes")]
        public string HandoverInspectionNotes { get; set; }

        /// <summary>
        /// Damages Found - أضرار موجودة
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "damages_found")]
        public bool DamagesFound { get; set; }

        /// <summary>
        /// Damage Assessment Amount - مبلغ تقييم الأضرار
        /// </summary>
        [SugarColumn(DecimalDigits = 2, IsNullable = true, ColumnName = "damage_assessment_amount")]
        public decimal? DamageAssessmentAmount { get; set; }

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

        #region Document References - مراجع المستندات

        /// <summary>
        /// Termination Notice Document ID - معرف مستند إشعار الإنهاء
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "termination_notice_document_id")]
        public Guid? TerminationNoticeDocumentId { get; set; }

        /// <summary>
        /// Settlement Agreement Document ID - معرف مستند اتفاقية التسوية
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "settlement_agreement_document_id")]
        public Guid? SettlementAgreementDocumentId { get; set; }

        /// <summary>
        /// Handover Report Document ID - معرف مستند تقرير التسليم
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "handover_report_document_id")]
        public Guid? HandoverReportDocumentId { get; set; }

        #endregion

        #region Notes - الملاحظات

        /// <summary>
        /// Internal Notes - ملاحظات داخلية
        /// </summary>
        [SugarColumn(Length = 2000, IsNullable = true, ColumnName = "internal_notes")]
        public string InternalNotes { get; set; }

        /// <summary>
        /// Customer Communication Notes - ملاحظات التواصل مع العميل
        /// </summary>
        [SugarColumn(Length = 2000, IsNullable = true, ColumnName = "customer_communication_notes")]
        public string CustomerCommunicationNotes { get; set; }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// Contract - العقد
        /// </summary>
        [Navigate(NavigateType.OneToOne, nameof(ContractId))]
        public ErpContract Contract { get; set; }

        /// <summary>
        /// Financial Settlement - التسوية المالية
        /// Financial details for this termination
        /// </summary>
        [Navigate(NavigateType.OneToOne, nameof(TerminationId))]
        public ErpContractTerminationFinancial FinancialSettlement { get; set; }

        /// <summary>
        /// Deposit Settlement - تسوية الضمان
        /// Deposit handling for this termination
        /// </summary>
        [Navigate(NavigateType.OneToOne, nameof(TerminationId))]
        public ErpContractTerminationDeposit DepositSettlement { get; set; }

        /// <summary>
        /// Inspection Details - تفاصيل الفحص
        /// Unit inspection and handover details
        /// </summary>
        [Navigate(NavigateType.OneToOne, nameof(TerminationId))]
        public ErpContractTerminationInspection Inspection { get; set; }

        #endregion
    }

    /// <summary>
    /// Termination Type - نوع الإنهاء
    /// </summary>
    public enum TerminationType
    {
        /// <summary>
        /// Early Termination - إنهاء مبكر
        /// Contract ended before scheduled end date
        /// </summary>
        Early = 0,

        /// <summary>
        /// Natural Expiry - انتهاء طبيعي
        /// Contract ended at scheduled end date without renewal
        /// </summary>
        NaturalExpiry = 1,

        /// <summary>
        /// Non-Renewal - عدم التجديد
        /// Contract not renewed at end of term
        /// </summary>
        NonRenewal = 2,

        /// <summary>
        /// Breach - خرق
        /// Contract terminated due to breach of terms
        /// </summary>
        Breach = 3,

        /// <summary>
        /// Mutual Agreement - اتفاق متبادل
        /// Both parties agreed to terminate
        /// </summary>
        MutualAgreement = 4,

        /// <summary>
        /// Force Majeure - قوة قاهرة
        /// Terminated due to circumstances beyond control
        /// </summary>
        ForceMajeure = 5
    }

    /// <summary>
    /// Termination Status - حالة الإنهاء
    /// </summary>
    public enum TerminationStatus
    {
        /// <summary>
        /// Draft - مسودة
        /// Termination created but not submitted
        /// </summary>
        Draft = 0,

        /// <summary>
        /// Notice Sent - الإشعار مرسل
        /// Termination notice has been sent
        /// </summary>
        NoticeSent = 1,

        /// <summary>
        /// Pending Approval - في انتظار الموافقة
        /// Awaiting internal approval
        /// </summary>
        PendingApproval = 2,

        /// <summary>
        /// Approved - معتمد
        /// Termination approved, proceeding with process
        /// </summary>
        Approved = 3,

        /// <summary>
        /// In Progress - قيد التنفيذ
        /// Termination process underway
        /// </summary>
        InProgress = 4,

        /// <summary>
        /// Pending Settlement - في انتظار التسوية
        /// Awaiting financial settlement
        /// </summary>
        PendingSettlement = 5,

        /// <summary>
        /// Pending Handover - في انتظار التسليم
        /// Awaiting unit handover/return
        /// </summary>
        PendingHandover = 6,

        /// <summary>
        /// Completed - مكتمل
        /// Termination fully processed
        /// </summary>
        Completed = 7,

        /// <summary>
        /// Cancelled - ملغي
        /// Termination cancelled (contract continues)
        /// </summary>
        Cancelled = 8,

        /// <summary>
        /// Disputed - متنازع عليه
        /// Termination terms are disputed
        /// </summary>
        Disputed = 9
    }
}
