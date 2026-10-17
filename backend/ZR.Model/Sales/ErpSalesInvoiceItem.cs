using Infrastructure.Converter;
using SqlSugar;
using Newtonsoft.Json;
using ZR.Model.System;

namespace ZR.Model.Sales
{
    /// <summary>
    /// Sales Invoice Item - بند فاتورة المبيعات
    /// </summary>
    [SugarTable("erp_sales_invoice_items")]
    public class ErpSalesInvoiceItem : SysBase
    {
        /// <summary>
        /// Item ID - معرف البند
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ItemId { get; set; }

        /// <summary>
        /// Invoice ID - معرف الفاتورة
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long InvoiceId { get; set; }

        /// <summary>
        /// Product ID - معرف المنتج
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? ProductId { get; set; }

        /// <summary>
        /// Service ID - معرف الخدمة
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? ServiceId { get; set; }

        /// <summary>
        /// Item Code - كود الصنف
        /// </summary>
        public string ItemCode { get; set; }

        /// <summary>
        /// Item Name - اسم الصنف
        /// </summary>
        public string ItemName { get; set; }

        /// <summary>
        /// Item Name Arabic - اسم الصنف بالعربي
        /// </summary>
        public string ItemNameAr { get; set; }

        /// <summary>
        /// Quantity - الكمية
        /// </summary>
        public decimal Quantity { get; set; }

        /// <summary>
        /// Unit of Measure - وحدة القياس
        /// </summary>
        public string UnitOfMeasure { get; set; }

        /// <summary>
        /// Unit Price - سعر الوحدة
        /// </summary>
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// Discount Amount - مبلغ الخصم
        /// </summary>
        public decimal DiscountAmount { get; set; }

        /// <summary>
        /// Discount Percent - نسبة الخصم
        /// </summary>
        public decimal DiscountPercent { get; set; }

        /// <summary>
        /// Subtotal - المجموع الفرعي
        /// </summary>
        public decimal Subtotal { get; set; }

        /// <summary>
        /// VAT Percent - نسبة الضريبة
        /// </summary>
        public decimal VatPercent { get; set; } = 15;

        /// <summary>
        /// VAT Amount - مبلغ الضريبة
        /// </summary>
        public decimal VatAmount { get; set; }

        /// <summary>
        /// Total - الإجمالي
        /// </summary>
        public decimal Total { get; set; }

        /// <summary>
        /// Cost Price - سعر التكلفة
        /// </summary>
        public decimal CostPrice { get; set; }

        /// <summary>
        /// Profit - الربح
        /// </summary>
        public decimal Profit { get; set; }

        /// <summary>
        /// Warehouse ID - معرف المخزن
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? WarehouseId { get; set; }

        /// <summary>
        /// Line Order - ترتيب السطر
        /// </summary>
        public int LineOrder { get; set; }

        /// <summary>
        /// Notes - ملاحظات
        /// </summary>
        public string Notes { get; set; }
    }
}

