using System;
using SqlSugar;
using ZR.Model.System;

namespace ZR.Model.Finance.ARAP
{
    /// <summary>
    /// AP Invoice Line - سطر فاتورة المورد
    /// </summary>
    [SugarTable("arap_ap_invoice_line")]
    public class APInvoiceLine : SysBase
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

        /// <summary>
        /// Invoice (Navigation Property) - الفاتورة
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public APInvoice Invoice { get; set; }
    }
}
