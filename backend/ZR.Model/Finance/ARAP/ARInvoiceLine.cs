using System;
using SqlSugar;
using ZR.Model.Finance.ARAP;
using ZR.Model.Finance.Zatca.Enums;
using ZR.Model.System;

namespace ZR.Model.Finance.ARAP
{
    /// <summary>
    /// AR Invoice Line - سطر فاتورة العميل
    /// </summary>
    [SugarTable("arap_ar_invoice_line")]
    public class ARInvoiceLine : SysBase
    {
        /// <summary>
        /// Line ID - معرف السطر
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false, ColumnName = "id")]
        public Guid Id { get; set; }

        /// <summary>
        /// Invoice ID - معرف الفاتورة (FK)
        /// </summary>
        [SugarColumn(ColumnName = "invoice_id")]
        public Guid InvoiceId { get; set; }

        /// <summary>
        /// Line Number - رقم السطر
        /// </summary>
        [SugarColumn(ColumnName = "line_number")]
        public int LineNumber { get; set; }

        /// <summary>
        /// Account ID - معرف الحساب (FK to gl_account)
        /// </summary>
        [SugarColumn(ColumnName = "account_id")]
        public Guid AccountId { get; set; }

        /// <summary>
        /// Description - الوصف
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnName = "description")]
        public string Description { get; set; }

        /// <summary>
        /// Quantity - الكمية
        /// </summary>
        [SugarColumn(ColumnName = "quantity", DecimalDigits = 4)]
        public decimal Quantity { get; set; } = 1;

        /// <summary>
        /// Unit Price - سعر الوحدة
        /// </summary>
        [SugarColumn(ColumnName = "unit_price", DecimalDigits = 4)]
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// Amount (Quantity × UnitPrice) in Foreign Currency - المبلغ بالعملة الأجنبية
        /// </summary>
        [SugarColumn(ColumnName = "amount", DecimalDigits = 4)]
        public decimal Amount { get; set; }

        /// <summary>
        /// Tax Code ID - معرف كود الضريبة
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "tax_code_id")]
        public Guid? TaxCodeId { get; set; }

        /// <summary>
        /// Tax Rate - نسبة الضريبة
        /// </summary>
        [SugarColumn(ColumnName = "tax_rate", DecimalDigits = 4)]
        public decimal TaxRate { get; set; } = 0;

        /// <summary>
        /// Tax Amount - مبلغ الضريبة
        /// </summary>
        [SugarColumn(ColumnName = "tax_amount", DecimalDigits = 4)]
        public decimal TaxAmount { get; set; } = 0;

        /// <summary>
        /// Total Amount (Amount + TaxAmount) - إجمالي المبلغ
        /// </summary>
        [SugarColumn(ColumnName = "total_amount", DecimalDigits = 4)]
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Taxable Amount - المبلغ الخاضع للضريبة
        /// Base amount before tax (for tax-inclusive: TotalAmount / (1 + TaxRate))
        /// </summary>
        [SugarColumn(ColumnName = "taxable_amount", DecimalDigits = 4, IsNullable = true)]
        public decimal? TaxableAmount { get; set; }

        /// <summary>
        /// Tax Inclusive Amount - المبلغ شامل الضريبة
        /// Total amount including tax (for tax-inclusive invoices)
        /// </summary>
        [SugarColumn(ColumnName = "tax_inclusive_amount", DecimalDigits = 4, IsNullable = true)]
        public decimal? TaxInclusiveAmount { get; set; }

        /// <summary>
        /// Cost Center ID - معرف مركز التكلفة
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "cost_center_id")]
        public Guid? CostCenterId { get; set; }

        /// <summary>
        /// Department ID - معرف القسم
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "department_id")]
        public Guid? DepartmentId { get; set; }

        /// <summary>
        /// Project ID - معرف المشروع
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "project_id")]
        public Guid? ProjectId { get; set; }

        #region ZATCA E-Invoicing Fields

        /// <summary>
        /// ZATCA Tax Category - فئة الضريبة لزاتكا
        /// S=Standard (15%), Z=Zero Rated, E=Exempt, O=Out of Scope
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "zatca_tax_category")]
        public ZatcaTaxCategory? ZatcaTaxCategory { get; set; }

        /// <summary>
        /// Tax Exemption Reason - سبب الإعفاء الضريبي
        /// Required for Z, E, O tax categories per ZATCA regulations
        /// </summary>
        [SugarColumn(IsNullable = true, Length = 200, ColumnName = "tax_exemption_reason")]
        public string TaxExemptionReason { get; set; }

        /// <summary>
        /// Tax Exemption Reason Code - كود سبب الإعفاء
        /// VATEX-SA-* codes per ZATCA specification
        /// Examples: VATEX-SA-29, VATEX-SA-30, VATEX-SA-31, etc.
        /// </summary>
        [SugarColumn(IsNullable = true, Length = 50, ColumnName = "tax_exemption_reason_code")]
        public string TaxExemptionReasonCode { get; set; }

        #endregion

        /// <summary>
        /// Invoice (Navigation Property) - الفاتورة
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public ARInvoice Invoice { get; set; }
    }
}
