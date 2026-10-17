using System;
using SqlSugar;
using ZR.Model.System;

namespace ZR.Model.Contracts
{
    /// <summary>
    /// Contract Termination Deposit - تسوية الضمان للإنهاء
    /// Deposit/security handling for contract termination (1:1 with ErpContractTermination)
    /// </summary>
    [SugarTable("erp_contract_termination_deposit")]
    [SugarIndex("ix_termination_deposit_status", nameof(DepositRefundStatus), OrderByType.Asc)]
    public class ErpContractTerminationDeposit : SysBase
    {
        #region Primary Key

        /// <summary>
        /// Termination ID - معرف الإنهاء
        /// Primary key and foreign key to ErpContractTermination (1:1 relationship)
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, ColumnName = "termination_id")]
        public Guid TerminationId { get; set; }

        #endregion

        #region Original Deposit - الضمان الأصلي

        /// <summary>
        /// Security Deposit Amount - مبلغ الضمان
        /// Original security deposit held
        /// </summary>
        [SugarColumn(DecimalDigits = 2, IsNullable = true, ColumnName = "security_deposit_amount")]
        public decimal? SecurityDepositAmount { get; set; }

        /// <summary>
        /// Deposit Receipt Date - تاريخ استلام الضمان
        /// Date when deposit was received
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "deposit_receipt_date")]
        public DateTime? DepositReceiptDate { get; set; }

        /// <summary>
        /// Deposit Receipt Reference - مرجع استلام الضمان
        /// Reference number for deposit receipt
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true, ColumnName = "deposit_receipt_reference")]
        public string DepositReceiptReference { get; set; }

        #endregion

        #region Deductions - الخصومات

        /// <summary>
        /// Deposit Deductions - خصومات الضمان
        /// Total amount deducted from security deposit
        /// </summary>
        [SugarColumn(DecimalDigits = 2, IsNullable = true, ColumnName = "deposit_deductions")]
        public decimal? DepositDeductions { get; set; } = 0;

        /// <summary>
        /// Deposit Deduction Details - تفاصيل خصومات الضمان
        /// JSON array of deduction items with reasons and amounts
        /// Example: [{"reason": "Damage repair", "amount": 500}, {"reason": "Cleaning", "amount": 200}]
        /// </summary>
        [SugarColumn(ColumnDataType = "text", IsNullable = true, ColumnName = "deposit_deduction_details")]
        public string DepositDeductionDetails { get; set; }

        #endregion

        #region Refund - الاسترداد

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
        public int? DepositRefundStatus { get; set; } = 0;

        /// <summary>
        /// Deposit Refund Date - تاريخ استرداد الضمان
        /// Date when refund was processed
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "deposit_refund_date")]
        public DateTime? DepositRefundDate { get; set; }

        /// <summary>
        /// Deposit Refund Reference - مرجع استرداد الضمان
        /// Reference number for refund transaction
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true, ColumnName = "deposit_refund_reference")]
        public string DepositRefundReference { get; set; }

        /// <summary>
        /// Deposit Refund Method - طريقة استرداد الضمان
        /// Method of refund (BankTransfer, Cheque, Cash, etc.)
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnName = "deposit_refund_method")]
        public string DepositRefundMethod { get; set; }

        #endregion

        #region Applied to Balance - تطبيق على الرصيد

        /// <summary>
        /// Deposit Applied to Balance - الضمان المطبق على الرصيد
        /// Amount of deposit applied to outstanding balance
        /// </summary>
        [SugarColumn(DecimalDigits = 2, IsNullable = true, ColumnName = "deposit_applied_to_balance")]
        public decimal? DepositAppliedToBalance { get; set; } = 0;

        #endregion

        #region Forfeiture - المصادرة

        /// <summary>
        /// Deposit Forfeited Amount - مبلغ الضمان المصادر
        /// Amount of deposit forfeited
        /// </summary>
        [SugarColumn(DecimalDigits = 2, IsNullable = true, ColumnName = "deposit_forfeited_amount")]
        public decimal? DepositForfeitedAmount { get; set; } = 0;

        /// <summary>
        /// Forfeiture Reason - سبب المصادرة
        /// Reason for deposit forfeiture
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnName = "forfeiture_reason")]
        public string ForfeitureReason { get; set; }

        #endregion

        #region Currency - العملة

        /// <summary>
        /// Currency - العملة
        /// Currency for all amounts
        /// </summary>
        [SugarColumn(Length = 10, IsNullable = false, ColumnName = "currency")]
        public string Currency { get; set; } = "SAR";

        #endregion

        #region Notes - الملاحظات

        /// <summary>
        /// Deposit Notes - ملاحظات الضمان
        /// Additional notes about deposit handling
        /// </summary>
        [SugarColumn(Length = 1000, IsNullable = true, ColumnName = "deposit_notes")]
        public string DepositNotes { get; set; }

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
        /// Deposit Refund Status Name - اسم حالة استرداد الضمان
        /// Display name for refund status
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public string DepositRefundStatusName => DepositRefundStatus switch
        {
            0 => "Pending",
            1 => "Processing",
            2 => "Refunded",
            3 => "Applied to Balance",
            4 => "Forfeited",
            _ => "Unknown"
        };

        /// <summary>
        /// Net Deposit Disposition - صافي التصرف في الضمان
        /// Breakdown of how deposit was disposed
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public decimal NetDepositDisposition =>
            (DepositRefundAmount ?? 0) +
            (DepositAppliedToBalance ?? 0) +
            (DepositDeductions ?? 0) +
            (DepositForfeitedAmount ?? 0);

        #endregion
    }
}
