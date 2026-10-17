using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZR.Model.Contracts
{
    /// <summary>
    /// Contract Billing Schedule Line - بند جدول الفوترة
    /// Detailed line item for each unit/adjustment in a billing schedule
    /// </summary>
    [SugarTable("erp_contract_billing_schedule_line")]
    [SugarIndex("IX_schedule_line_schedule", nameof(ScheduleId), OrderByType.Asc)]
    [SugarIndex("IX_schedule_line_unit", nameof(UnitId), OrderByType.Asc)]
    public class ErpContractBillingScheduleLine : SysBase
    {
        /// <summary>
        /// Line ID - معرف البند
        /// Primary key (GUID)
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false, ColumnName = "line_id")]
        public Guid LineId { get; set; }

        /// <summary>
        /// Schedule ID - معرف الجدول
        /// Foreign key to ErpContractBillingSchedule
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "schedule_id")]
        public Guid ScheduleId { get; set; }

        /// <summary>
        /// Line Number - رقم البند
        /// Sequential line number
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "line_number")]
        public int LineNumber { get; set; }

        /// <summary>
        /// Line Type - نوع البند
        /// "Unit", "Adjustment", "Tax", "Discount"
        /// </summary>
        [SugarColumn(Length = 20, IsNullable = false, ColumnName = "line_type")]
        public string LineType { get; set; }

        /// <summary>
        /// Unit ID - معرف الوحدة
        /// Foreign key to ErpContractUnit (if line type is "Unit")
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "unit_id")]
        public Guid? UnitId { get; set; }

        /// <summary>
        /// Pricing Rule ID - معرف قاعدة التسعير
        /// Foreign key to ErpContractPricingRule (if line type is "Adjustment")
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "pricing_rule_id")]
        public Guid? PricingRuleId { get; set; }

        /// <summary>
        /// Description - الوصف
        /// Line description
        /// </summary>
        [SugarColumn(Length = 300, IsNullable = false, ColumnName = "description")]
        public string Description { get; set; }

        /// <summary>
        /// Description (Arabic) - الوصف بالعربية
        /// </summary>
        [SugarColumn(Length = 300, IsNullable = true, ColumnName = "description_ar")]
        public string DescriptionAr { get; set; }

        /// <summary>
        /// Quantity - الكمية
        /// Quantity (usually 1 for rentals, can vary for usage-based)
        /// </summary>
        [SugarColumn(DecimalDigits = 4, IsNullable = false, ColumnName = "quantity")]
        public decimal Quantity { get; set; } = 1;

        /// <summary>
        /// Unit Price - سعر الوحدة
        /// Price per unit
        /// </summary>
        [SugarColumn(DecimalDigits = 4, IsNullable = false, ColumnName = "unit_price")]
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// Amount - المبلغ
        /// Quantity × UnitPrice (can be negative for discounts)
        /// </summary>
        [SugarColumn(DecimalDigits = 4, IsNullable = false, ColumnName = "amount")]
        public decimal Amount { get; set; }

        /// <summary>
        /// Tax Group ID - معرف مجموعة الضريبة
        /// Tax group for this line
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "tax_group_id")]
        public Guid? TaxGroupId { get; set; }

        /// <summary>
        /// Tax Rate - نسبة الضريبة
        /// Tax rate for this line
        /// </summary>
        [SugarColumn(DecimalDigits = 2, IsNullable = true, ColumnName = "tax_rate")]
        public decimal? TaxRate { get; set; }

        /// <summary>
        /// Tax Amount - مبلغ الضريبة
        /// Tax amount for this line
        /// </summary>
        [SugarColumn(DecimalDigits = 4, IsNullable = false, ColumnName = "tax_amount")]
        public decimal TaxAmount { get; set; } = 0;

        /// <summary>
        /// Total Amount - المبلغ الإجمالي
        /// Amount + TaxAmount
        /// </summary>
        [SugarColumn(DecimalDigits = 4, IsNullable = false, ColumnName = "total_amount")]
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Revenue Account ID - معرف حساب الإيرادات
        /// GL account for this line
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "revenue_account_id")]
        public Guid? RevenueAccountId { get; set; }

        /// <summary>
        /// Cost Center ID - معرف مركز التكلفة
        /// Cost center for this line
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "cost_center_id")]
        public Guid? CostCenterId { get; set; }

        /// <summary>
        /// Is Pro-Rated - مقسط نسبياً
        /// Whether this line is pro-rated
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "is_pro_rated")]
        public bool IsProRated { get; set; } = false;

        /// <summary>
        /// Original Amount - المبلغ الأصلي
        /// Original amount before pro-rating
        /// </summary>
        [SugarColumn(DecimalDigits = 4, IsNullable = true, ColumnName = "original_amount")]
        public decimal? OriginalAmount { get; set; }

        #region Navigation Properties

        /// <summary>
        /// Unit Code - رمز الوحدة
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public string UnitCode { get; set; }

        /// <summary>
        /// Pricing Rule Name - اسم قاعدة التسعير
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public string PricingRuleName { get; set; }

        /// <summary>
        /// Revenue Account Name - اسم حساب الإيرادات
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public string RevenueAccountName { get; set; }

        #endregion
    }
}
