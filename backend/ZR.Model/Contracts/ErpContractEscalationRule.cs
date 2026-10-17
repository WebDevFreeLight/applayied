using System;
using SqlSugar;
using ZR.Model.System;

namespace ZR.Model.Contracts
{
    /// <summary>
    /// Contract Escalation Rule - قاعدة تصاعد العقد
    /// Extension table for pricing rules of type Escalation (1:1 with ErpContractPricingRule)
    /// </summary>
    [SugarTable("erp_contract_escalation_rule")]
    [SugarIndex("ix_escalation_rule_next", nameof(NextEscalationDate), OrderByType.Asc)]
    public class ErpContractEscalationRule : SysBase
    {
        #region Primary Key

        /// <summary>
        /// Pricing Rule ID - معرف قاعدة التسعير
        /// Primary key and foreign key to ErpContractPricingRule (1:1 relationship)
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, ColumnName = "pricing_rule_id")]
        public Guid PricingRuleId { get; set; }

        #endregion

        #region Escalation Configuration - إعدادات التصاعد

        /// <summary>
        /// Escalation Frequency - تكرار التصاعد
        /// How often escalation is applied (Monthly, Quarterly, SemiAnnual, Yearly)
        /// </summary>
        [SugarColumn(Length = 20, IsNullable = false, ColumnName = "escalation_frequency")]
        public string EscalationFrequency { get; set; } = "Yearly";

        /// <summary>
        /// Escalation Base Date - تاريخ أساس التصاعد
        /// Reference date for calculating escalation periods
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "escalation_base_date")]
        public DateTime? EscalationBaseDate { get; set; }

        /// <summary>
        /// Escalation Cap Percentage - سقف نسبة التصاعد
        /// Maximum total escalation percentage over contract life
        /// </summary>
        [SugarColumn(DecimalDigits = 2, IsNullable = true, ColumnName = "escalation_cap_percentage")]
        public decimal? EscalationCapPercentage { get; set; }

        /// <summary>
        /// Escalation Method - طريقة التصاعد
        /// How escalation is calculated (Simple or Compound)
        /// </summary>
        [SugarColumn(Length = 20, IsNullable = false, ColumnName = "escalation_method")]
        public string EscalationMethod { get; set; } = "Simple";

        #endregion

        #region Tracking - التتبع

        /// <summary>
        /// Escalation Applied Count - عدد مرات التصاعد المطبقة
        /// Number of times escalation has been applied
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "escalation_applied_count")]
        public int? EscalationAppliedCount { get; set; } = 0;

        /// <summary>
        /// Last Escalation Date - تاريخ آخر تصاعد
        /// Date of last escalation application
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "last_escalation_date")]
        public DateTime? LastEscalationDate { get; set; }

        /// <summary>
        /// Next Escalation Date - تاريخ التصاعد القادم
        /// Date of next scheduled escalation
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "next_escalation_date")]
        public DateTime? NextEscalationDate { get; set; }

        /// <summary>
        /// Cumulative Percentage - النسبة التراكمية
        /// Total percentage escalation applied so far
        /// </summary>
        [SugarColumn(DecimalDigits = 4, IsNullable = true, ColumnName = "cumulative_percentage")]
        public decimal? CumulativePercentage { get; set; } = 0;

        #endregion

        #region Navigation Properties - خصائص الملاحة

        /// <summary>
        /// Pricing Rule - قاعدة التسعير
        /// Parent pricing rule (navigation property)
        /// </summary>
        [Navigate(NavigateType.OneToOne, nameof(PricingRuleId))]
        public ErpContractPricingRule PricingRule { get; set; }

        #endregion
    }
}
