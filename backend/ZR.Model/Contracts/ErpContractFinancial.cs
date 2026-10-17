using System;
using SqlSugar;
using ZR.Model.System;

namespace ZR.Model.Contracts
{
    /// <summary>
    /// Contract Financial Settings - الإعدادات المالية للعقد
    /// Stores financial configuration for a contract (1:1 relationship with ErpContract)
    /// </summary>
    [SugarTable("erp_contract_financial")]
    public class ErpContractFinancial : SysBase
    {
        #region Primary Key

        /// <summary>
        /// Contract ID - معرف العقد
        /// Primary key and foreign key to ErpContract (1:1 relationship)
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, ColumnName = "contract_id")]
        public Guid ContractId { get; set; }

        #endregion

        #region Financial Fields - الحقول المالية

        /// <summary>
        /// Base Amount - المبلغ الأساسي
        /// Base contract amount per billing period (before adjustments)
        /// </summary>
        [SugarColumn(DecimalDigits = 4, IsNullable = true, ColumnName = "base_amount")]
        public decimal? BaseAmount { get; set; }

        /// <summary>
        /// Total Amount - القيمة الإجمالية للعقد
        /// Total value over the contract duration (nullable for variable/indefinite)
        /// </summary>
        [SugarColumn(DecimalDigits = 4, IsNullable = true, ColumnName = "total_amount")]
        public decimal? TotalAmount { get; set; }

        /// <summary>
        /// Currency - العملة
        /// Contract currency code (default SAR)
        /// </summary>
        [SugarColumn(Length = 10, IsNullable = false, ColumnName = "currency")]
        public string Currency { get; set; } = "SAR";

        /// <summary>
        /// Tax Group ID - معرف مجموعة الضريبة
        /// Foreign key to Tax Group for tax calculation
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "tax_group_id")]
        public Guid? TaxGroupId { get; set; }

        /// <summary>
        /// Is Tax Inclusive - هل الضريبة مشمولة
        /// Whether contract amounts include tax
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "is_tax_inclusive")]
        public bool IsTaxInclusive { get; set; } = false;

        #endregion

        #region Security Deposit - التأمين

        /// <summary>
        /// Security Deposit - التأمين
        /// Security deposit amount (if applicable)
        /// </summary>
        [SugarColumn(DecimalDigits = 4, IsNullable = true, ColumnName = "security_deposit")]
        public decimal? SecurityDeposit { get; set; }

        /// <summary>
        /// Security Deposit Status - حالة التأمين
        /// Status of security deposit (Pending, Received, Returned, Forfeited, PartialReturn)
        /// </summary>
        [SugarColumn(Length = 20, IsNullable = true, ColumnName = "security_deposit_status")]
        public string SecurityDepositStatus { get; set; }

        #endregion

        #region Retention - الاحتفاظ

        /// <summary>
        /// Retention Percentage - نسبة الاحتفاظ
        /// Percentage of payment retained until contract completion
        /// </summary>
        [SugarColumn(DecimalDigits = 2, IsNullable = true, ColumnName = "retention_percentage")]
        public decimal? RetentionPercentage { get; set; }

        #endregion

        #region Payment Terms - شروط الدفع

        /// <summary>
        /// Payment Terms - شروط الدفع
        /// Payment terms description (e.g., "Net 30", "Due on Receipt")
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true, ColumnName = "payment_terms")]
        public string PaymentTerms { get; set; }

        /// <summary>
        /// Payment Due Days - أيام استحقاق الدفع
        /// Number of days after invoice date for payment
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "payment_due_days")]
        public int? PaymentDueDays { get; set; } = 30;

        #endregion

        #region Navigation Properties - خصائص الملاحة

        /// <summary>
        /// Contract - العقد
        /// Parent contract (navigation property)
        /// </summary>
        [Navigate(NavigateType.OneToOne, nameof(ContractId))]
        public ErpContract Contract { get; set; }

        /// <summary>
        /// Tax Group Name - اسم مجموعة الضريبة
        /// For display purposes
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public string TaxGroupName { get; set; }

        #endregion
    }
}
