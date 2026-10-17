using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MiniExcelLibs.Attributes;
using ZR.Model.Contracts.Enums;

namespace ZR.Model.Contracts.Dto
{
    /// <summary>
    /// Contract Termination Query DTO - استعلام إنهاء العقد
    /// </summary>
    public class ContractTerminationQueryDto : PagerInfo
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
        /// Termination Reason - سبب الإنهاء
        /// </summary>
        public TerminationReason? TerminationReason { get; set; }

        /// <summary>
        /// Termination Type - نوع الإنهاء
        /// </summary>
        public TerminationType? TerminationType { get; set; }

        /// <summary>
        /// Status - الحالة
        /// </summary>
        public TerminationStatus? Status { get; set; }

        /// <summary>
        /// Effective Date From - من تاريخ السريان
        /// </summary>
        public DateTime? EffectiveDateFrom { get; set; }

        /// <summary>
        /// Effective Date To - إلى تاريخ السريان
        /// </summary>
        public DateTime? EffectiveDateTo { get; set; }

        /// <summary>
        /// Settlement Status - حالة التسوية
        /// </summary>
        public int? SettlementStatus { get; set; }

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
    /// Contract Termination List DTO - قائمة إنهاء العقد
    /// </summary>
    public class ErpContractTerminationListDto
    {
        [ExcelColumn(Name = "Termination ID")]
        public Guid TerminationId { get; set; }

        [ExcelColumn(Name = "Termination Number")]
        public string TerminationNumber { get; set; }

        public Guid ContractId { get; set; }

        [ExcelColumn(Name = "Contract Number")]
        public string ContractNumber { get; set; }

        [ExcelColumn(Name = "Customer Name")]
        public string CustomerName { get; set; }

        [ExcelColumn(Name = "Termination Reason")]
        public TerminationReason TerminationReason { get; set; }

        public string TerminationReasonName { get; set; }

        [ExcelColumn(Name = "Termination Type")]
        public TerminationType TerminationType { get; set; }

        public string TerminationTypeName { get; set; }

        [ExcelColumn(Name = "Status")]
        public TerminationStatus Status { get; set; }

        public string StatusName { get; set; }

        [ExcelColumn(Name = "Notice Date", Format = "yyyy-MM-dd")]
        public DateTime NoticeDate { get; set; }

        [ExcelColumn(Name = "Effective Date", Format = "yyyy-MM-dd")]
        public DateTime EffectiveDate { get; set; }

        [ExcelColumn(Name = "Original End Date", Format = "yyyy-MM-dd")]
        public DateTime OriginalEndDate { get; set; }

        [ExcelColumn(Name = "Early Termination Days")]
        public int EarlyTerminationDays { get; set; }

        [ExcelColumn(Name = "Outstanding Balance")]
        public decimal OutstandingBalance { get; set; }

        [ExcelColumn(Name = "Penalty Amount")]
        public decimal? PenaltyAmount { get; set; }

        [ExcelColumn(Name = "Settlement Amount")]
        public decimal SettlementAmount { get; set; }

        [ExcelColumn(Name = "Settlement Status")]
        public int SettlementStatus { get; set; }

        public string SettlementStatusName { get; set; }

        public string Currency { get; set; }
    }

    /// <summary>
    /// Contract Termination Detail DTO - تفاصيل إنهاء العقد
    /// </summary>
    public class ErpContractTerminationDetailDto
    {
        // Identification
        public Guid TerminationId { get; set; }
        public string TerminationNumber { get; set; }
        public Guid ContractId { get; set; }
        public string ContractNumber { get; set; }
        public string ContractTitle { get; set; }

        // Customer Info
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerCode { get; set; }

        // Termination Details
        public TerminationReason TerminationReason { get; set; }
        public string TerminationReasonName { get; set; }
        public TerminationType TerminationType { get; set; }
        public string TerminationTypeName { get; set; }
        public int InitiatedBy { get; set; }
        public string InitiatedByName { get; set; }
        public TerminationStatus Status { get; set; }
        public string StatusName { get; set; }
        public string TerminationReasonDetail { get; set; }

        // Dates
        public DateTime NoticeDate { get; set; }
        public DateTime EffectiveDate { get; set; }
        public DateTime OriginalEndDate { get; set; }
        public int NoticePeriodDays { get; set; }
        public int EarlyTerminationDays { get; set; }
        public DateTime? VacationDate { get; set; }
        public DateTime? ActualVacationDate { get; set; }

        // Financial Summary
        public decimal TotalContractValue { get; set; }
        public decimal AmountBilled { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal OutstandingBalance { get; set; }
        public decimal RemainingContractValue { get; set; }

        // Penalty and Fees
        public bool PenaltyApplicable { get; set; }
        public int? PenaltyCalculationType { get; set; }
        public string PenaltyCalculationTypeName { get; set; }
        public decimal? PenaltyPercentage { get; set; }
        public int? PenaltyMonths { get; set; }
        public decimal? PenaltyAmount { get; set; }
        public bool PenaltyWaived { get; set; }
        public string PenaltyWaiverReason { get; set; }
        public decimal? AdministrationFee { get; set; }
        public decimal? OtherFees { get; set; }
        public string OtherFeesDescription { get; set; }

        // Security Deposit Settlement
        public decimal? SecurityDepositAmount { get; set; }
        public decimal? DepositDeductions { get; set; }
        public List<DepositDeductionItemDto> DepositDeductionItems { get; set; }
        public decimal? DepositRefundAmount { get; set; }
        public int? DepositRefundStatus { get; set; }
        public string DepositRefundStatusName { get; set; }
        public DateTime? DepositRefundDate { get; set; }

        // Final Settlement
        public decimal SettlementAmount { get; set; }
        public int SettlementStatus { get; set; }
        public string SettlementStatusName { get; set; }
        public DateTime? SettlementDate { get; set; }
        public string SettlementNotes { get; set; }

        // Billing Schedule Impact
        public int? SchedulesCancelled { get; set; }
        public decimal? CancelledSchedulesAmount { get; set; }
        public Guid? FinalBillingScheduleId { get; set; }
        public bool FinalInvoiceRequired { get; set; }
        public Guid? FinalInvoiceId { get; set; }
        public string FinalInvoiceNumber { get; set; }
        public Guid? PenaltyInvoiceId { get; set; }
        public string PenaltyInvoiceNumber { get; set; }

        // Unit Handover
        public int? UnitsToReturn { get; set; }
        public int? UnitsReturned { get; set; }
        public bool HandoverInspectionRequired { get; set; }
        public DateTime? HandoverInspectionDate { get; set; }
        public string HandoverInspectionNotes { get; set; }
        public bool DamagesFound { get; set; }
        public decimal? DamageAssessmentAmount { get; set; }

        // Approval
        public bool RequiresApproval { get; set; }
        public long? ApprovedBy { get; set; }
        public string ApprovedByName { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public string ApprovalNotes { get; set; }

        // Documents
        public Guid? TerminationNoticeDocumentId { get; set; }
        public Guid? SettlementAgreementDocumentId { get; set; }
        public Guid? HandoverReportDocumentId { get; set; }

        // Notes
        public string InternalNotes { get; set; }
        public string CustomerCommunicationNotes { get; set; }

        // Currency
        public string Currency { get; set; }

        // Audit
        public string Create_by { get; set; }
        public DateTime? Create_time { get; set; }
        public string Update_by { get; set; }
        public DateTime? Update_time { get; set; }
    }

    /// <summary>
    /// Deposit Deduction Item DTO - عنصر خصم الضمان
    /// </summary>
    public class DepositDeductionItemDto
    {
        public string Description { get; set; }
        public string DescriptionAr { get; set; }
        public decimal Amount { get; set; }
        public string Category { get; set; } // Damage, Cleaning, Unpaid Rent, Other
        public Guid? UnitId { get; set; }
        public string UnitCode { get; set; }
    }

    /// <summary>
    /// Create Contract Termination DTO - إنشاء إنهاء العقد
    /// </summary>
    public class CreateContractTerminationDto
    {
        /// <summary>
        /// Contract ID - معرف العقد
        /// </summary>
        [Required(ErrorMessage = "Contract ID is required")]
        public Guid ContractId { get; set; }

        /// <summary>
        /// Termination Reason - سبب الإنهاء
        /// </summary>
        [Required(ErrorMessage = "Termination reason is required")]
        public TerminationReason TerminationReason { get; set; }

        /// <summary>
        /// Termination Type - نوع الإنهاء
        /// </summary>
        [Required(ErrorMessage = "Termination type is required")]
        public TerminationType TerminationType { get; set; }

        /// <summary>
        /// Initiated By - بدأ بواسطة
        /// 0 = Landlord/Company, 1 = Tenant/Customer, 2 = Mutual Agreement, 3 = Court Order, 4 = Force Majeure
        /// </summary>
        [Required(ErrorMessage = "Initiated by is required")]
        public int InitiatedBy { get; set; }

        /// <summary>
        /// Termination Reason Detail - تفاصيل سبب الإنهاء
        /// </summary>
        [StringLength(2000)]
        public string TerminationReasonDetail { get; set; }

        /// <summary>
        /// Notice Date - تاريخ الإشعار
        /// </summary>
        [Required(ErrorMessage = "Notice date is required")]
        public DateTime NoticeDate { get; set; }

        /// <summary>
        /// Effective Date - تاريخ السريان
        /// </summary>
        [Required(ErrorMessage = "Effective date is required")]
        public DateTime EffectiveDate { get; set; }

        /// <summary>
        /// Vacation Date - تاريخ الإخلاء
        /// </summary>
        public DateTime? VacationDate { get; set; }

        /// <summary>
        /// Apply Early Termination Penalty - تطبيق غرامة الإنهاء المبكر
        /// </summary>
        public bool ApplyPenalty { get; set; }

        /// <summary>
        /// Penalty Calculation Type - نوع حساب الغرامة
        /// 0 = Fixed Amount, 1 = Percentage of Remaining, 2 = Months of Rent, 3 = Custom
        /// </summary>
        public int? PenaltyCalculationType { get; set; }

        /// <summary>
        /// Penalty Percentage - نسبة الغرامة
        /// </summary>
        [Range(0, 100)]
        public decimal? PenaltyPercentage { get; set; }

        /// <summary>
        /// Penalty Months - أشهر الغرامة
        /// </summary>
        [Range(0, 24)]
        public int? PenaltyMonths { get; set; }

        /// <summary>
        /// Custom Penalty Amount - مبلغ غرامة مخصص
        /// </summary>
        public decimal? CustomPenaltyAmount { get; set; }

        /// <summary>
        /// Waive Penalty - إعفاء من الغرامة
        /// </summary>
        public bool WaivePenalty { get; set; }

        /// <summary>
        /// Penalty Waiver Reason - سبب إعفاء الغرامة
        /// </summary>
        [StringLength(500)]
        public string PenaltyWaiverReason { get; set; }

        /// <summary>
        /// Administration Fee - رسوم الإدارة
        /// </summary>
        public decimal? AdministrationFee { get; set; }

        /// <summary>
        /// Other Fees - رسوم أخرى
        /// </summary>
        public decimal? OtherFees { get; set; }

        /// <summary>
        /// Other Fees Description - وصف الرسوم الأخرى
        /// </summary>
        [StringLength(500)]
        public string OtherFeesDescription { get; set; }

        /// <summary>
        /// Handover Inspection Required - فحص التسليم مطلوب
        /// </summary>
        public bool HandoverInspectionRequired { get; set; }

        /// <summary>
        /// Requires Approval - يتطلب موافقة
        /// </summary>
        public bool RequiresApproval { get; set; }

        /// <summary>
        /// Internal Notes - ملاحظات داخلية
        /// </summary>
        [StringLength(2000)]
        public string InternalNotes { get; set; }

        /// <summary>
        /// Customer Communication Notes - ملاحظات التواصل مع العميل
        /// </summary>
        [StringLength(2000)]
        public string CustomerCommunicationNotes { get; set; }
    }

    /// <summary>
    /// Update Contract Termination DTO - تحديث إنهاء العقد
    /// </summary>
    public class UpdateContractTerminationDto
    {
        /// <summary>
        /// Termination Reason Detail - تفاصيل سبب الإنهاء
        /// </summary>
        [StringLength(2000)]
        public string TerminationReasonDetail { get; set; }

        /// <summary>
        /// Effective Date - تاريخ السريان
        /// </summary>
        public DateTime? EffectiveDate { get; set; }

        /// <summary>
        /// Vacation Date - تاريخ الإخلاء
        /// </summary>
        public DateTime? VacationDate { get; set; }

        /// <summary>
        /// Penalty Calculation Type - نوع حساب الغرامة
        /// </summary>
        public int? PenaltyCalculationType { get; set; }

        /// <summary>
        /// Penalty Percentage - نسبة الغرامة
        /// </summary>
        [Range(0, 100)]
        public decimal? PenaltyPercentage { get; set; }

        /// <summary>
        /// Penalty Months - أشهر الغرامة
        /// </summary>
        [Range(0, 24)]
        public int? PenaltyMonths { get; set; }

        /// <summary>
        /// Custom Penalty Amount - مبلغ غرامة مخصص
        /// </summary>
        public decimal? CustomPenaltyAmount { get; set; }

        /// <summary>
        /// Waive Penalty - إعفاء من الغرامة
        /// </summary>
        public bool? WaivePenalty { get; set; }

        /// <summary>
        /// Penalty Waiver Reason - سبب إعفاء الغرامة
        /// </summary>
        [StringLength(500)]
        public string PenaltyWaiverReason { get; set; }

        /// <summary>
        /// Administration Fee - رسوم الإدارة
        /// </summary>
        public decimal? AdministrationFee { get; set; }

        /// <summary>
        /// Other Fees - رسوم أخرى
        /// </summary>
        public decimal? OtherFees { get; set; }

        /// <summary>
        /// Other Fees Description - وصف الرسوم الأخرى
        /// </summary>
        [StringLength(500)]
        public string OtherFeesDescription { get; set; }

        /// <summary>
        /// Handover Inspection Required - فحص التسليم مطلوب
        /// </summary>
        public bool? HandoverInspectionRequired { get; set; }

        /// <summary>
        /// Internal Notes - ملاحظات داخلية
        /// </summary>
        [StringLength(2000)]
        public string InternalNotes { get; set; }

        /// <summary>
        /// Customer Communication Notes - ملاحظات التواصل مع العميل
        /// </summary>
        [StringLength(2000)]
        public string CustomerCommunicationNotes { get; set; }
    }

    /// <summary>
    /// Process Security Deposit DTO - معالجة الضمان
    /// </summary>
    public class ProcessSecurityDepositDto
    {
        /// <summary>
        /// Termination ID - معرف الإنهاء
        /// </summary>
        [Required]
        public Guid TerminationId { get; set; }

        /// <summary>
        /// Deposit Deductions - خصومات الضمان
        /// </summary>
        public List<DepositDeductionItemDto> Deductions { get; set; } = new();

        /// <summary>
        /// Apply to Outstanding Balance - تطبيق على الرصيد المستحق
        /// If true, deposit is applied to outstanding balance first
        /// </summary>
        public bool ApplyToOutstandingBalance { get; set; }

        /// <summary>
        /// Refund Method - طريقة الاسترداد
        /// 0 = Bank Transfer, 1 = Check, 2 = Cash, 3 = Credit to Account
        /// </summary>
        public int RefundMethod { get; set; }

        /// <summary>
        /// Processing Notes - ملاحظات المعالجة
        /// </summary>
        [StringLength(1000)]
        public string ProcessingNotes { get; set; }
    }

    /// <summary>
    /// Record Handover Inspection DTO - تسجيل فحص التسليم
    /// </summary>
    public class RecordHandoverInspectionDto
    {
        /// <summary>
        /// Termination ID - معرف الإنهاء
        /// </summary>
        [Required]
        public Guid TerminationId { get; set; }

        /// <summary>
        /// Inspection Date - تاريخ الفحص
        /// </summary>
        [Required]
        public DateTime InspectionDate { get; set; }

        /// <summary>
        /// Units Returned - الوحدات المرجعة
        /// </summary>
        [Required]
        public int UnitsReturned { get; set; }

        /// <summary>
        /// Damages Found - أضرار موجودة
        /// </summary>
        public bool DamagesFound { get; set; }

        /// <summary>
        /// Damage Items - عناصر الضرر
        /// </summary>
        public List<DamageItemDto> DamageItems { get; set; } = new();

        /// <summary>
        /// Inspection Notes - ملاحظات الفحص
        /// </summary>
        [StringLength(2000)]
        public string InspectionNotes { get; set; }
    }

    /// <summary>
    /// Damage Item DTO - عنصر الضرر
    /// </summary>
    public class DamageItemDto
    {
        public Guid? UnitId { get; set; }
        public string UnitCode { get; set; }
        public string Description { get; set; }
        public string DescriptionAr { get; set; }
        public decimal EstimatedCost { get; set; }
        public string Severity { get; set; } // Minor, Moderate, Major
    }

    /// <summary>
    /// Complete Settlement DTO - إكمال التسوية
    /// </summary>
    public class CompleteSettlementDto
    {
        /// <summary>
        /// Termination ID - معرف الإنهاء
        /// </summary>
        [Required]
        public Guid TerminationId { get; set; }

        /// <summary>
        /// Settlement Notes - ملاحظات التسوية
        /// </summary>
        [StringLength(2000)]
        public string SettlementNotes { get; set; }

        /// <summary>
        /// Write Off Amount - مبلغ الشطب
        /// Amount to write off if not collectible
        /// </summary>
        public decimal? WriteOffAmount { get; set; }

        /// <summary>
        /// Write Off Reason - سبب الشطب
        /// </summary>
        [StringLength(500)]
        public string WriteOffReason { get; set; }
    }

    /// <summary>
    /// Approve Termination DTO - اعتماد الإنهاء
    /// </summary>
    public class ApproveTerminationDto
    {
        /// <summary>
        /// Approval Notes - ملاحظات الموافقة
        /// </summary>
        [StringLength(1000)]
        public string ApprovalNotes { get; set; }

        /// <summary>
        /// Auto Process - معالجة تلقائية
        /// If true, immediately starts processing the termination
        /// </summary>
        public bool AutoProcess { get; set; }
    }

    /// <summary>
    /// Cancel Termination DTO - إلغاء الإنهاء
    /// </summary>
    public class CancelTerminationDto
    {
        /// <summary>
        /// Cancellation Reason - سبب الإلغاء
        /// </summary>
        [Required(ErrorMessage = "Cancellation reason is required")]
        [StringLength(500)]
        public string CancellationReason { get; set; }

        /// <summary>
        /// Reactivate Contract - إعادة تنشيط العقد
        /// If true, sets contract back to Active status
        /// </summary>
        public bool ReactivateContract { get; set; } = true;
    }

    /// <summary>
    /// Termination Preview DTO - معاينة الإنهاء
    /// Shows financial impact before executing termination
    /// </summary>
    public class TerminationPreviewDto
    {
        // Contract Info
        public Guid ContractId { get; set; }
        public string ContractNumber { get; set; }
        public DateTime ContractStartDate { get; set; }
        public DateTime OriginalEndDate { get; set; }
        public DateTime EffectiveDate { get; set; }
        public int EarlyTerminationDays { get; set; }

        // Financial Summary
        public decimal TotalContractValue { get; set; }
        public decimal AmountBilled { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal OutstandingBalance { get; set; }
        public decimal RemainingContractValue { get; set; }

        // Penalty Calculation
        public bool PenaltyApplicable { get; set; }
        public decimal CalculatedPenalty { get; set; }
        public string PenaltyCalculationBreakdown { get; set; }

        // Security Deposit
        public decimal SecurityDepositHeld { get; set; }
        public decimal EstimatedDepositRefund { get; set; }

        // Fees
        public decimal AdministrationFee { get; set; }
        public decimal OtherFees { get; set; }

        // Net Settlement
        public decimal EstimatedSettlement { get; set; }
        public bool CustomerOwes { get; set; }

        // Billing Impact
        public int SchedulesToCancel { get; set; }
        public decimal CancelledSchedulesAmount { get; set; }
        public bool FinalInvoiceRequired { get; set; }
        public decimal FinalInvoiceAmount { get; set; }

        // Unit Impact
        public int UnitsToReturn { get; set; }
        public List<UnitReturnPreviewDto> UnitReturnDetails { get; set; } = new();

        // Warnings
        public List<string> Warnings { get; set; } = new();
    }

    /// <summary>
    /// Unit Return Preview DTO - معاينة إرجاع الوحدة
    /// </summary>
    public class UnitReturnPreviewDto
    {
        public Guid UnitId { get; set; }
        public string UnitCode { get; set; }
        public string UnitName { get; set; }
        public int UnitType { get; set; }
        public string UnitTypeName { get; set; }
        public DateTime? ExpectedReturnDate { get; set; }
    }

    /// <summary>
    /// Termination Execution Result DTO - نتيجة تنفيذ الإنهاء
    /// </summary>
    public class TerminationExecutionResultDto
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public Guid TerminationId { get; set; }
        public string TerminationNumber { get; set; }
        public TerminationStatus NewStatus { get; set; }
        public int SchedulesCancelled { get; set; }
        public decimal CancelledAmount { get; set; }
        public Guid? FinalInvoiceId { get; set; }
        public string FinalInvoiceNumber { get; set; }
        public Guid? PenaltyInvoiceId { get; set; }
        public string PenaltyInvoiceNumber { get; set; }
        public decimal SettlementAmount { get; set; }
        public DateTime ProcessedAt { get; set; }
    }

    /// <summary>
    /// Termination Financial Summary DTO - ملخص مالي للإنهاء
    /// </summary>
    public class TerminationFinancialSummaryDto
    {
        // Receivables
        public decimal OutstandingInvoices { get; set; }
        public decimal PenaltyAmount { get; set; }
        public decimal AdministrationFee { get; set; }
        public decimal DamageCharges { get; set; }
        public decimal OtherCharges { get; set; }
        public decimal TotalReceivables { get; set; }

        // Payables
        public decimal SecurityDepositRefund { get; set; }
        public decimal PrepaidAmountRefund { get; set; }
        public decimal OtherRefunds { get; set; }
        public decimal TotalPayables { get; set; }

        // Net
        public decimal NetSettlement { get; set; }
        public bool IsRefundDue { get; set; }

        public string Currency { get; set; }
    }
}
