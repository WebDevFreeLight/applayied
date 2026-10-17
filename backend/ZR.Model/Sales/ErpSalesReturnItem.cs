using Infrastructure.Converter;
using SqlSugar;
using Newtonsoft.Json;
using ZR.Model.System;

namespace ZR.Model.Sales
{
    /// <summary>
    /// Sales Return Item - بند مرتجع المبيعات
    /// </summary>
    [SugarTable("erp_sales_return_items")]
    public class ErpSalesReturnItem : SysBase
    {
        /// <summary>
        /// Item ID - معرف البند
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ItemId { get; set; }

        /// <summary>
        /// Return ID - معرف المرتجع
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ReturnId { get; set; }

        /// <summary>
        /// Original Invoice Item ID - معرف بند الفاتورة الأصلية
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? OriginalInvoiceItemId { get; set; }

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
        /// Quantity - الكمية
        /// </summary>
        public decimal Quantity { get; set; }

        /// <summary>
        /// Unit Price - سعر الوحدة
        /// </summary>
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// Subtotal - المجموع الفرعي
        /// </summary>
        public decimal Subtotal { get; set; }

        /// <summary>
        /// VAT Percent - نسبة الضريبة
        /// </summary>
        public decimal VatPercent { get; set; }

        /// <summary>
        /// VAT Amount - مبلغ الضريبة
        /// </summary>
        public decimal VatAmount { get; set; }

        /// <summary>
        /// Total - الإجمالي
        /// </summary>
        public decimal Total { get; set; }

        /// <summary>
        /// Return Reason - سبب الإرجاع
        /// </summary>
        public string ReturnReason { get; set; }

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

