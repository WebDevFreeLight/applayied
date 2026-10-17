using System;
using SqlSugar;
using ZR.Model.System;

namespace ZR.Model.Contracts
{
    /// <summary>
    /// Contract Termination Financial - التسوية المالية للإنهاء
    /// Financial settlement details for contract termination (1:1 with ErpContractTermination)
    /// </summary>
    [SugarTable("erp_contract_termination_financial")]
    [SugarIndex("ix_termination_financial_status", nameof(SettlementStatus), OrderByType.Asc)]
    public class ErpContractTerminationFinancial : SysBase
    {
        #region Primary Key

        /// <summary>
        /// Termination ID - معرف الإنهاء
        /// Primary key and foreign key to ErpContractTermination (1:1 relationship)
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, ColumnName = "termination_id")]
        public Guid TerminationId { get; set; }

        #endregion

        #region Contract Value Summary - ملخص قيمة العقد

        /// <summary>
        /// Total Contract Value - إجمالي قيمة العقد
        /// Original total contract value
        /// </summary>
        [SugarColumn(DecimalDigits = 2, IsNullable = false, ColumnName = "total_contract_value")]
        public decimal TotalContractValue { get; set; } = 0;

        /// <summary>
        /// Amount Billed - المبلغ المفوتر
        /// Total amount already billed
        /// </summary>
        [SugarColumn(DecimalDigits = 2, IsNullable = false, ColumnName = "amount_billed")]
        public decimal AmountBilled { get; set; } = 0;

        /// <summary>
        /// Amount Paid - المبلغ المدفوع
        /// Total amount already paid by customer
        /// </summary>
        [SugarColumn(DecimalDigits = 2, IsNullable = false, ColumnName = "amount_paid")]
        public decimal AmountPaid { get; set; } = 0;

        /// <summary>
        /// Outstanding Balance - الرصيد المستحق
        /// Amount still owed by customer
        /// </summary>
        [SugarColumn(DecimalDigits = 2, IsNullable = false, ColumnName = "outstanding_balance")]
        public decimal OutstandingBalance { get; set; } = 0;

        /// <summary>
        /// Remaining Contract Value - قيمة العقد المتبقية
        /// Value of unfulfilled contract period
        /// </summary>
        [SugarColumn(DecimalDigits = 2, IsNullable = false, ColumnName = "remaining_contract_value")]
        public decimal RemainingContractValue { get; set; } = 0;

        #endregion

        #region Penalty Calculation - حساب الغرامة

        /// <summary>
        /// Penalty Applicable - غرامة الإنهاء المبكر قابلة للتطبيق
        /// Whether early termination penalty applies
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "penalty_applicable")]
        public bool PenaltyApplicable { get; set; } = false;

        /// <summary>
        /// Penalty Calculation Type - نوع حساب الغرامة
        /// 0 = Fixed Amount, 1 = Percentage of Remaining, 2 = Months of Rent, 3 = Custom
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "penalty_calculation_type")]
        public int? PenaltyCalculationType { get; set; }

        /// <summary>
        /// Penalty Percentage - نسبة الغرامة
        /// Percentage used for penalty calculation (if percentage-based)
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
        public bool PenaltyWaived { get; set; } = false;

        /// <summary>
        /// Penalty Waiver Reason - سبب إعفاء الغرامة
        /// Reason for waiving the penalty
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnName = "penalty_waiver_reason")]
        public string PenaltyWaiverReason { get; set; }

        /// <summary>
        /// Penalty Waiver Approved By - تمت الموافقة على الإعفاء بواسطة
        /// User who approved the penalty waiver
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnName = "penalty_waiver_approved_by")]
        public string PenaltyWaiverApprovedBy { get; set; }

        #endregion

        #region Fees - الرسوم

        /// <summary>
        /// Administration Fee - رسوم الإدارة
        /// Administrative fee for termination processing
        /// </summary>
        [SugarColumn(DecimalDigits = 2, IsNullable = true, ColumnName = "administration_fee")]
        public decimal? AdministrationFee { get; set; } = 0;

        /// <summary>
        /// Legal Fee - الرسوم القانونية
        /// Legal fees if applicable
        /// </summary>
        [SugarColumn(DecimalDigits = 2, IsNullable = true, ColumnName = "legal_fee")]
        public decimal? LegalFee { get; set; } = 0;

        /// <summary>
        /// Other Fees - رسوم أخرى
        /// Any additional fees
        /// </summary>
        [SugarColumn(DecimalDigits = 2, IsNullable = true, ColumnName = "other_fees")]
        public decimal? OtherFees { get; set; } = 0;

        /// <summary>
        /// Other Fees Description - وصف الرسوم الأخرى
        /// Description of other fees
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnName = "other_fees_description")]
        public string OtherFeesDescription { get; set; }

        #endregion

        #region Final Settlement - التسوية النهائية

        /// <summary>
        /// Settlement Amount - مبلغ التسوية
        /// Net amount due (positive = customer owes, negative = refund due)
        /// </summary>
        [SugarColumn(DecimalDigits = 2, IsNullable = false, ColumnName = "settlement_amount")]
        public decimal SettlementAmount { get; set; } = 0;

        /// <summary>
        /// Settlement Status - حالة التسوية
        /// 0 = Pending, 1 = Partial, 2 = Completed, 3 = Disputed, 4 = WrittenOff
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "settlement_status")]
        public int SettlementStatus { get; set; } = 0;

        /// <summary>
        /// Settlement Date - تاريخ التسوية
        /// Date when settlement was completed
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "settlement_date")]
        public DateTime? SettlementDate { get; set; }

        /// <summary>
        /// Settlement Notes - ملاحظات التسوية
        /// Notes about the settlement
        /// </summary>
        [SugarColumn(Length = 2000, IsNullable = true, ColumnName = "settlement_notes")]
        public string SettlementNotes { get; set; }

        #endregion

        #region Currency - العملة

        /// <summary>
        /// Currency - العملة
        /// Currency for all amounts
        /// </summary>
        [SugarColumn(Length = 10, IsNullable = false, ColumnName = "currency")]
        public string Currency { get; set; } = "SAR";

        #endregion

        #region Navigation Properties - خصائص الملاحة

        /// <summary>
        /// Termination - الإنهاء
        /// Parent termination record (navigation property)
        /// </summary>
        [Navigate(NavigateType.OneToOne, nameof(TerminationId))]
        public ErpContractTermination Termination { get; set; }

        #endregion

        #region Computed Properties - الخصائص المحسوبة

        /// <summary>
        /// Total Fees - إجمالي الرسوم
        /// Sum of all fees
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public decimal TotalFees => (AdministrationFee ?? 0) + (LegalFee ?? 0) + (OtherFees ?? 0);

        /// <summary>
        /// Settlement Status Name - اسم حالة التسوية
        /// Display name for settlement status
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public string SettlementStatusName => SettlementStatus switch
        {
            0 => "Pending",
            1 => "Partial",
            2 => "Completed",
            3 => "Disputed",
            4 => "WrittenOff",
            _ => "Unknown"
        };

        #endregion
    }
}
