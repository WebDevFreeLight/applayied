using System;
using SqlSugar;
using ZR.Model.Contracts.Enums;
using ZR.Model.System;

namespace ZR.Model.Contracts
{
    /// <summary>
    /// Contract Pricing Rule - قاعدة تسعير العقد
    /// Defines pricing logic for contracts: fixed amounts, discounts, escalations, seasonal pricing, etc.
    /// Multiple rules can be combined to calculate the final billing amount.
    /// </summary>
    [SugarTable("erp_contract_pricing_rule")]
    [SugarIndex("IX_pricing_rule_contract", nameof(ContractId), OrderByType.Asc)]
    [SugarIndex("IX_pricing_rule_type", nameof(RuleType), OrderByType.Asc)]
    [SugarIndex("IX_pricing_rule_dates", nameof(StartDate), OrderByType.Asc, nameof(EndDate), OrderByType.Asc)]
    [SugarIndex("IX_pricing_rule_unit", nameof(UnitId), OrderByType.Asc)]
    public class ErpContractPricingRule : SysBase
    {
        /// <summary>
        /// Pricing Rule ID - معرف قاعدة التسعير
        /// Primary key (GUID)
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false, ColumnName = "pricing_rule_id")]
        public Guid PricingRuleId { get; set; }

        /// <summary>
        /// Contract ID - معرف العقد
        /// Foreign key to ErpContract
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "contract_id")]
        public Guid ContractId { get; set; }

        /// <summary>
        /// Unit ID - معرف الوحدة
        /// Optional: If set, rule applies only to this unit; if null, applies to entire contract
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "unit_id")]
        public Guid? UnitId { get; set; }

        /// <summary>
        /// Rule Name - اسم القاعدة
        /// Descriptive name for the rule
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = true, ColumnName = "rule_name")]
        public string RuleName { get; set; }

        /// <summary>
        /// Rule Name (Arabic) - اسم القاعدة بالعربية
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = true, ColumnName = "rule_name_ar")]
        public string RuleNameAr { get; set; }

        /// <summary>
        /// Rule Type - نوع القاعدة
        /// Type of pricing rule (Fixed, Variable, Discount, Seasonal, etc.)
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "rule_type")]
        public PricingRuleType RuleType { get; set; }

        /// <summary>
        /// Value - القيمة
        /// The numeric value for the rule (amount, percentage, rate, etc.)
        /// Interpretation depends on RuleType and ValueType
        /// </summary>
        [SugarColumn(DecimalDigits = 4, IsNullable = false, ColumnName = "value")]
        public decimal Value { get; set; }

        /// <summary>
        /// Value Type - نوع القيمة
        /// How to interpret the value: Amount (fixed), Percentage, PerUnit (rate)
        /// </summary>
        [SugarColumn(Length = 20, IsNullable = false, ColumnName = "value_type")]
        public string ValueType { get; set; } = "Amount"; // Amount, Percentage, PerUnit

        /// <summary>
        /// Minimum Value - الحد الأدنى
        /// For percentage rules: minimum amount to apply
        /// </summary>
        [SugarColumn(DecimalDigits = 4, IsNullable = true, ColumnName = "min_value")]
        public decimal? MinValue { get; set; }

        /// <summary>
        /// Maximum Value - الحد الأقصى
        /// For percentage rules: maximum amount to apply (cap)
        /// </summary>
        [SugarColumn(DecimalDigits = 4, IsNullable = true, ColumnName = "max_value")]
        public decimal? MaxValue { get; set; }

        /// <summary>
        /// Start Date - تاريخ البداية
        /// When the rule becomes effective (null = contract start)
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "start_date")]
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// End Date - تاريخ النهاية
        /// When the rule expires (null = contract end)
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "end_date")]
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Is Active - نشط
        /// Whether the rule is currently active
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "is_active")]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Priority - الأولوية
        /// Order in which rules are applied (lower = first)
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "priority")]
        public int Priority { get; set; } = 100;

        /// <summary>
        /// Is Compounding - تراكمي
        /// If true, rule is applied on top of previous rules; if false, applied on base amount
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "is_compounding")]
        public bool IsCompounding { get; set; } = false;

        /// <summary>
        /// Currency - العملة
        /// Currency for fixed amounts (null = contract currency)
        /// </summary>
        [SugarColumn(Length = 10, IsNullable = true, ColumnName = "currency")]
        public string Currency { get; set; }

        #region Tiered Pricing Fields - حقول التسعير المتدرج

        /// <summary>
        /// Tier From - من المستوى
        /// Lower bound for tiered pricing (quantity, amount, or usage)
        /// </summary>
        [SugarColumn(DecimalDigits = 4, IsNullable = true, ColumnName = "tier_from")]
        public decimal? TierFrom { get; set; }

        /// <summary>
        /// Tier To - إلى المستوى
        /// Upper bound for tiered pricing
        /// </summary>
        [SugarColumn(DecimalDigits = 4, IsNullable = true, ColumnName = "tier_to")]
        public decimal? TierTo { get; set; }

        /// <summary>
        /// Tier Unit - وحدة المستوى
        /// What the tier bounds represent: "Quantity", "Amount", "Usage", "Area"
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnName = "tier_unit")]
        public string TierUnit { get; set; }

        #endregion

        #region Discount Conditions - شروط الخصم

        /// <summary>
        /// Discount Condition - شرط الخصم
        /// Condition for discount application (e.g., "AdvancePayment", "LongTerm", "Loyalty")
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true, ColumnName = "discount_condition")]
        public string DiscountCondition { get; set; }

        /// <summary>
        /// Discount Condition Value - قيمة شرط الخصم
        /// Value for the condition (e.g., number of months for advance payment)
        /// </summary>
        [SugarColumn(DecimalDigits = 2, IsNullable = true, ColumnName = "discount_condition_value")]
        public decimal? DiscountConditionValue { get; set; }

        /// <summary>
        /// Requires Approval - يتطلب موافقة
        /// Whether applying this rule requires approval
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "requires_approval")]
        public bool RequiresApproval { get; set; } = false;

        /// <summary>
        /// Approved By - تمت الموافقة بواسطة
        /// User who approved the rule
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnName = "approved_by")]
        public string ApprovedBy { get; set; }

        /// <summary>
        /// Approved Date - تاريخ الموافقة
        /// Date when rule was approved
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "approved_date")]
        public DateTime? ApprovedDate { get; set; }

        #endregion

        #region Invoice Display - عرض الفاتورة

        /// <summary>
        /// Show On Invoice - إظهار في الفاتورة
        /// Whether this rule appears as a separate line on invoices
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "show_on_invoice")]
        public bool ShowOnInvoice { get; set; } = true;

        /// <summary>
        /// Invoice Line Description - وصف بند الفاتورة
        /// Description to show on invoice line
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = true, ColumnName = "invoice_line_description")]
        public string InvoiceLineDescription { get; set; }

        /// <summary>
        /// Invoice Line Description (Arabic) - وصف بند الفاتورة بالعربية
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = true, ColumnName = "invoice_line_description_ar")]
        public string InvoiceLineDescriptionAr { get; set; }

        #endregion

        /// <summary>
        /// Description - الوصف
        /// Detailed description of the rule
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnName = "description")]
        public string Description { get; set; }

        /// <summary>
        /// Notes - ملاحظات
        /// Internal notes
        /// </summary>
        [SugarColumn(Length = 1000, IsNullable = true, ColumnName = "notes")]
        public string Notes { get; set; }

        /// <summary>
        /// Created From Amendment - تم الإنشاء من تعديل
        /// Reference to amendment that created this rule
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "amendment_id")]
        public Guid? AmendmentId { get; set; }

        #region Navigation Properties - خصائص الملاحة

        /// <summary>
        /// Contract - العقد
        /// Parent contract
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public ErpContract Contract { get; set; }

        /// <summary>
        /// Unit - الوحدة
        /// Associated unit (if rule is unit-specific)
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public ErpContractUnit Unit { get; set; }

        /// <summary>
        /// Escalation Rule - قاعدة التصاعد
        /// Extension table for escalation-type rules (1:1)
        /// </summary>
        [Navigate(NavigateType.OneToOne, nameof(PricingRuleId))]
        public ErpContractEscalationRule EscalationRule { get; set; }

        /// <summary>
        /// Seasonal Rule - قاعدة الموسمية
        /// Extension table for seasonal-type rules (1:1)
        /// </summary>
        [Navigate(NavigateType.OneToOne, nameof(PricingRuleId))]
        public ErpContractSeasonalRule SeasonalRule { get; set; }

        /// <summary>
        /// Unit Code - رمز الوحدة
        /// For display purposes
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public string UnitCode { get; set; }

        /// <summary>
        /// Rule Type Name - اسم نوع القاعدة
        /// For display purposes
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public string RuleTypeName { get; set; }

        /// <summary>
        /// Calculated Amount - المبلغ المحسوب
        /// Calculated amount based on current period (for display)
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public decimal? CalculatedAmount { get; set; }

        #endregion

        #region Computed Properties - الخصائص المحسوبة

        /// <summary>
        /// Is Escalation Type - هل هو نوع تصاعدي
        /// Whether this rule is an escalation type
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public bool IsEscalationType => RuleType == PricingRuleType.Escalation;

        /// <summary>
        /// Is Seasonal Type - هل هو نوع موسمي
        /// Whether this rule is a seasonal type
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public bool IsSeasonalType => RuleType == PricingRuleType.Seasonal;

        #endregion
    }
}
