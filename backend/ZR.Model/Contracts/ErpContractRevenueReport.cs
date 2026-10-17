using System;
using SqlSugar;
using ZR.Model.Contracts.Enums;
using ZR.Model.System;

namespace ZR.Model.Contracts
{
    /// <summary>
    /// Contract Revenue Report - تقرير إيرادات العقد
    /// Used for revenue share calculation. Each record represents one reporting period for one unit.
    /// Calculation: If Fixed → fixed_rent_amount. If RevenueShare → MAX(reported_revenue × percentage, minimum_guarantee).
    /// </summary>
    [SugarTable("erp_contract_revenue_report")]
    [SugarIndex("IX_revenue_report_contract", nameof(ContractId), OrderByType.Asc)]
    [SugarIndex("IX_revenue_report_unit", nameof(UnitId), OrderByType.Asc)]
    [SugarIndex("IX_revenue_report_period", nameof(PeriodStart), OrderByType.Asc, nameof(PeriodEnd), OrderByType.Asc)]
    public class ErpContractRevenueReport : SysBase
    {
        /// <summary>
        /// Report ID - معرف التقرير
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false, ColumnName = "report_id")]
        public Guid ReportId { get; set; }

        /// <summary>
        /// Contract ID - معرف العقد
        /// FK to erp_contract
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "contract_id")]
        public Guid ContractId { get; set; }

        /// <summary>
        /// Unit ID - معرف الوحدة
        /// FK to erp_contract_unit. The specific unit this report covers.
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "unit_id")]
        public Guid UnitId { get; set; }

        /// <summary>
        /// Period Start - بداية الفترة
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "period_start")]
        public DateTime PeriodStart { get; set; }

        /// <summary>
        /// Period End - نهاية الفترة
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "period_end")]
        public DateTime PeriodEnd { get; set; }

        /// <summary>
        /// Reported Revenue - الإيرادات المبلغ عنها
        /// Actual revenue reported by tenant for this period.
        /// </summary>
        [SugarColumn(DecimalDigits = 2, IsNullable = false, ColumnName = "reported_revenue")]
        public decimal ReportedRevenue { get; set; }

        #region Pricing Snapshot (from contract unit at time of calculation)

        /// <summary>
        /// Pricing Type - نوع التسعير
        /// Snapshot of unit's pricing type used for this calculation.
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "pricing_type")]
        public PricingType PricingType { get; set; }

        /// <summary>
        /// Fixed Rent Amount - مبلغ الإيجار الثابت
        /// Snapshot from contract unit.
        /// </summary>
        [SugarColumn(DecimalDigits = 2, IsNullable = true, ColumnName = "fixed_rent_amount")]
        public decimal? FixedRentAmount { get; set; }

        /// <summary>
        /// Revenue Percentage - نسبة الإيرادات
        /// Snapshot from contract unit.
        /// </summary>
        [SugarColumn(DecimalDigits = 2, IsNullable = true, ColumnName = "revenue_percentage")]
        public decimal? RevenuePercentage { get; set; }

        /// <summary>
        /// Minimum Guarantee - الحد الأدنى المضمون
        /// Snapshot from contract unit.
        /// </summary>
        [SugarColumn(DecimalDigits = 2, IsNullable = true, ColumnName = "minimum_guarantee")]
        public decimal? MinimumGuarantee { get; set; }

        #endregion

        #region Calculated Fields

        /// <summary>
        /// Revenue Based Amount - المبلغ المبني على الإيرادات
        /// = reported_revenue × (revenue_percentage / 100)
        /// Null for Fixed pricing.
        /// </summary>
        [SugarColumn(DecimalDigits = 2, IsNullable = true, ColumnName = "revenue_based_amount")]
        public decimal? RevenueBasedAmount { get; set; }

        /// <summary>
        /// Calculated Invoice Amount - مبلغ الفاتورة المحسوب
        /// Final amount to invoice:
        /// Fixed → fixed_rent_amount
        /// RevenueShare → MAX(revenue_based_amount, minimum_guarantee ?? 0)
        /// </summary>
        [SugarColumn(DecimalDigits = 2, IsNullable = false, ColumnName = "calculated_invoice_amount")]
        public decimal CalculatedInvoiceAmount { get; set; }

        /// <summary>
        /// Minimum Guarantee Applied - هل تم تطبيق الحد الأدنى
        /// True if minimum_guarantee was used instead of calculated revenue share.
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "minimum_guarantee_applied")]
        public bool MinimumGuaranteeApplied { get; set; } = false;

        #endregion

        /// <summary>
        /// Currency - العملة
        /// </summary>
        [SugarColumn(Length = 10, IsNullable = false, ColumnName = "currency")]
        public string Currency { get; set; } = "SAR";

        /// <summary>
        /// Status - الحالة
        /// Draft → Submitted → Invoiced
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "status")]
        public RevenueReportStatus Status { get; set; } = RevenueReportStatus.Draft;

        /// <summary>
        /// Billing Schedule ID - معرف جدول الفوترة
        /// FK to erp_contract_billing_schedule if linked to a billing entry.
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "billing_schedule_id")]
        public Guid? BillingScheduleId { get; set; }

        /// <summary>
        /// Notes - ملاحظات
        /// </summary>
        [SugarColumn(Length = 1000, IsNullable = true, ColumnName = "notes")]
        public string Notes { get; set; }

        #region Navigation Properties

        [SugarColumn(IsIgnore = true)]
        public string ContractNumber { get; set; }

        [SugarColumn(IsIgnore = true)]
        public string UnitCode { get; set; }

        [SugarColumn(IsIgnore = true)]
        public string UnitName { get; set; }

        [SugarColumn(IsIgnore = true)]
        public string CustomerName { get; set; }

        [SugarColumn(IsIgnore = true)]
        public string StatusName => Status.ToString();

        [SugarColumn(IsIgnore = true)]
        public string PricingTypeName => PricingType.ToString();

        #endregion
    }
}
