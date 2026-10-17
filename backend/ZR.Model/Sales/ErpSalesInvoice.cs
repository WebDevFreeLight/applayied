using Infrastructure.Converter;
using SqlSugar;
using Newtonsoft.Json;
using ZR.Model.System;

namespace ZR.Model.Sales
{
    /// <summary>
    /// Sales Invoice - فاتورة المبيعات
    /// </summary>
    [SugarTable("erp_sales_invoices")]
    [SqlDict(
        dictType: "erp_sales_invoice",
        dictName: "Sales Invoice - فاتورة المبيعات",
        valueColumn: "invoiceid",
        labelColumns: "invoicenumber",
        RemarkColumn = "description",
        OrderBy = "invoicedate DESC",
        Description = "Auto-generated from ErpSalesInvoice table"
    )]
    public class ErpSalesInvoice : SysBase
    {
        /// <summary>
        /// Invoice ID - معرف الفاتورة
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long InvoiceId { get; set; }

        /// <summary>
        /// Invoice Number - رقم الفاتورة
        /// </summary>
        public string InvoiceNumber { get; set; }

        /// <summary>
        /// Invoice Date - تاريخ الفاتورة
        /// </summary>
        public DateTime InvoiceDate { get; set; }

        /// <summary>
        /// Invoice Type - نوع الفاتورة (simplified/standard)
        /// </summary>
        public string InvoiceType { get; set; } = "simplified";

        /// <summary>
        /// Customer ID - معرف العميل
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? CustomerId { get; set; }

        /// <summary>
        /// Customer Name - اسم العميل
        /// </summary>
        public string CustomerName { get; set; }

      

        /// <summary>
        /// Warehouse ID - معرف المخزن
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? WarehouseId { get; set; }

        /// <summary>
        /// Shift ID - معرف الوردية
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? ShiftId { get; set; }

        /// <summary>
        /// Sales Order ID - معرف أمر البيع
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? SalesOrderId { get; set; }

        /// <summary>
        /// Subtotal - المجموع الفرعي
        /// </summary>
        public decimal Subtotal { get; set; }

        /// <summary>
        /// Discount Amount - مبلغ الخصم
        /// </summary>
        public decimal DiscountAmount { get; set; }

        /// <summary>
        /// Discount Percent - نسبة الخصم
        /// </summary>
        public decimal DiscountPercent { get; set; }

        /// <summary>
        /// Taxable Amount - المبلغ الخاضع للضريبة
        /// </summary>
        public decimal TaxableAmount { get; set; }

        /// <summary>
        /// VAT Amount - مبلغ الضريبة
        /// </summary>
        public decimal VatAmount { get; set; }

        /// <summary>
        /// Total Amount - المبلغ الإجمالي
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Paid Amount - المبلغ المدفوع
        /// </summary>
        public decimal PaidAmount { get; set; }

        /// <summary>
        /// Remaining Amount - المبلغ المتبقي
        /// </summary>
        public decimal RemainingAmount { get; set; }

        /// <summary>
        /// Payment Method ID - طريقة الدفع
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? PaymentMethodId { get; set; }

        /// <summary>
        /// Treasury ID - معرف الخزينة
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? TreasuryId { get; set; }

        /// <summary>
        /// Salesperson ID - معرف البائع
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? SalespersonId { get; set; }

        /// <summary>
        /// Cost Center ID - معرف مركز التكلفة
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? CostCenterId { get; set; }

        /// <summary>
        /// Currency - العملة
        /// </summary>
        public string Currency { get; set; } = "SAR";

        /// <summary>
        /// Exchange Rate - سعر الصرف
        /// </summary>
        public decimal ExchangeRate { get; set; } = 1;

        /// <summary>
        /// Status - الحالة (draft/confirmed/paid/cancelled)
        /// </summary>
        public string Status { get; set; } = "draft";

        /// <summary>
        /// Is Posted - مرحّلة
        /// </summary>
        public bool IsPosted { get; set; }

        /// <summary>
        /// Journal Entry ID - معرف القيد اليومي
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? JournalEntryId { get; set; }

        /// <summary>
        /// ZATCA Submitted - مرسلة للزكاة
        /// </summary>
        public bool ZatcaSubmitted { get; set; }

        /// <summary>
        /// ZATCA UUID - معرف الزكاة
        /// </summary>
        public string ZatcaUuid { get; set; }

        /// <summary>
        /// QR Code - كود QR
        /// </summary>
        public string QrCode { get; set; }

        /// <summary>
        /// Description - الوصف
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Notes - ملاحظات
        /// </summary>
        public string Notes { get; set; }

        /// <summary>
        /// Is Active - نشطة
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Navigation - Invoice Items
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public List<ErpSalesInvoiceItem> Items { get; set; }
    }
}

