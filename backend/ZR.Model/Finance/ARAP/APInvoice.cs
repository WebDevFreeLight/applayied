using System;
using System.Collections.Generic;
using SqlSugar;
using ZR.Model.Finance.ARAP.Enums;
using ZR.Model.System;

namespace ZR.Model.Finance.ARAP
{
    /// <summary>
    /// AP Invoice - فاتورة الموردين
    /// </summary>
    [SugarTable("arap_ap_invoice")]
    public class APInvoice : SysBase
    {
        /// <summary>
        /// Invoice ID - معرف الفاتورة
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false, ColumnName = "id")]
        public Guid Id { get; set; }

        /// <summary>
        /// Invoice Number - رقم الفاتورة
        /// </summary>
        [SugarColumn(Length = 50, ColumnName = "invoice_number")]
        public string InvoiceNumber { get; set; }

        /// <summary>
        /// Vendor ID - معرف المورد
        /// </summary>
        [SugarColumn(ColumnName = "vendor_id")]
        public Guid VendorId { get; set; }

        /// <summary>
        /// Invoice Date - تاريخ الفاتورة
        /// </summary>
        [SugarColumn(ColumnName = "invoice_date")]
        public DateTime InvoiceDate { get; set; }

        /// <summary>
        /// Due Date - تاريخ الاستحقاق
        /// </summary>
        [SugarColumn(ColumnName = "due_date")]
        public DateTime DueDate { get; set; }

        /// <summary>
        /// Currency - العملة
        /// </summary>
        [SugarColumn(Length = 10, ColumnName = "currency")]
        public string Currency { get; set; } = "SAR";

        /// <summary>
        /// Exchange Rate - سعر الصرف
        /// </summary>
        [SugarColumn(ColumnName = "exchange_rate", DecimalDigits = 6)]
        public decimal ExchangeRate { get; set; } = 1;

        /// <summary>
        /// Subtotal (Sum of line amounts before tax) - المجموع الفرعي
        /// </summary>
        [SugarColumn(ColumnName = "subtotal", DecimalDigits = 4)]
        public decimal Subtotal { get; set; }

        /// <summary>
        /// Tax Amount - مبلغ الضريبة
        /// </summary>
        [SugarColumn(ColumnName = "tax_amount", DecimalDigits = 4)]
        public decimal TaxAmount { get; set; } = 0;

        /// <summary>
        /// Total Amount - المبلغ الإجمالي
        /// </summary>
        [SugarColumn(ColumnName = "total_amount", DecimalDigits = 4)]
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Paid Amount - المبلغ المدفوع
        /// </summary>
        [SugarColumn(ColumnName = "paid_amount", DecimalDigits = 4)]
        public decimal PaidAmount { get; set; } = 0;

        /// <summary>
        /// Remaining Amount - المبلغ المتبقي
        /// </summary>
        [SugarColumn(ColumnName = "remaining_amount", DecimalDigits = 4)]
        public decimal RemainingAmount { get; set; }

        /// <summary>
        /// Status - الحالة
        /// </summary>
        [SugarColumn(ColumnName = "status")]
        public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;

        /// <summary>
        /// Tax Group ID - معرف مجموعة الضريبة
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "tax_group_id")]
        public Guid? TaxGroupId { get; set; }

        /// <summary>
        /// Is Tax Inclusive - هل الضريبة مشمولة
        /// If true, line amounts include tax; if false, tax is added on top
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "is_tax_inclusive")]
        public bool IsTaxInclusive { get; set; } = false;

        /// <summary>
        /// Description - الوصف
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnName = "description")]
        public string Description { get; set; }

        /// <summary>
        /// Reference Number - رقم المرجع
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true, ColumnName = "reference_number")]
        public string ReferenceNumber { get; set; }

        /// <summary>
        /// GL Journal ID - معرف القيد المحاسبي
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "gl_journal_id")]
        public Guid? GLJournalId { get; set; }

        /// <summary>
        /// Fiscal Year ID - معرف السنة المالية
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "fiscal_year_id")]
        public Guid? FiscalYearId { get; set; }

        /// <summary>
        /// Fiscal Period ID - معرف الفترة المالية
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "fiscal_period_id")]
        public Guid? FiscalPeriodId { get; set; }

        /// <summary>
        /// Notes - ملاحظات
        /// </summary>
        [SugarColumn(Length = 1000, IsNullable = true, ColumnName = "notes")]
        public string Notes { get; set; }

        /// <summary>
        /// Invoice Lines (Navigation Property) - بنود الفاتورة
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public List<APInvoiceLine> Lines { get; set; }

        /// <summary>
        /// Vendor (Navigation Property) - المورد
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public Vendor Vendor { get; set; }
    }
}


