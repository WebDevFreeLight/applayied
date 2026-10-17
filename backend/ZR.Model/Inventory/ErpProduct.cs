using Infrastructure.Converter;
using SqlSugar;
using Newtonsoft.Json;
using ZR.Model.System;

namespace ZR.Model.Inventory
{
    /// <summary>
    /// Product - المنتج
    /// </summary>
    [SugarTable("erp_products")]
    [SqlDict(
        dictType: "erp_product",
        dictName: "Product - المنتج",
        valueColumn: "productid",
        labelColumns: "productname",
        RemarkColumn = "productnamear",
        OrderBy = "productcode ASC",
        Description = "Auto-generated from ErpProduct table"
    )]
    public class ErpProduct : SysBase
    {
        /// <summary>
        /// Product ID - معرف المنتج
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ProductId { get; set; }

        /// <summary>
        /// Product Code - كود المنتج
        /// </summary>
        public string ProductCode { get; set; }

        /// <summary>
        /// Barcode - الباركود
        /// </summary>
        public string Barcode { get; set; }

        /// <summary>
        /// Product Name - اسم المنتج
        /// </summary>
        public string ProductName { get; set; }

        /// <summary>
        /// Product Name Arabic - اسم المنتج بالعربي
        /// </summary>
        public string ProductNameAr { get; set; }

        /// <summary>
        /// Category ID - معرف الفئة
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? CategoryId { get; set; }

        /// <summary>
        /// Unit of Measure - وحدة القياس
        /// </summary>
        public string UnitOfMeasure { get; set; }

        /// <summary>
        /// Cost Price - سعر التكلفة
        /// </summary>
        public decimal CostPrice { get; set; }

        /// <summary>
        /// Selling Price - سعر البيع
        /// </summary>
        public decimal SellingPrice { get; set; }

        /// <summary>
        /// Wholesale Price - سعر الجملة
        /// </summary>
        public decimal WholesalePrice { get; set; }

        /// <summary>
        /// Minimum Price - أقل سعر
        /// </summary>
        public decimal MinimumPrice { get; set; }

        /// <summary>
        /// VAT Percent - نسبة الضريبة
        /// </summary>
        public decimal VatPercent { get; set; } = 15;

        /// <summary>
        /// Is VAT Inclusive - شامل الضريبة
        /// </summary>
        public bool IsVatInclusive { get; set; }

        /// <summary>
        /// Reorder Level - حد إعادة الطلب
        /// </summary>
        public decimal ReorderLevel { get; set; }

        /// <summary>
        /// Reorder Quantity - كمية إعادة الطلب
        /// </summary>
        public decimal ReorderQuantity { get; set; }

        /// <summary>
        /// Maximum Stock - الحد الأقصى للمخزون
        /// </summary>
        public decimal MaximumStock { get; set; }

        /// <summary>
        /// Is Service - خدمة
        /// </summary>
        public bool IsService { get; set; }

        /// <summary>
        /// Is Serialized - له رقم تسلسلي
        /// </summary>
        public bool IsSerialized { get; set; }

        /// <summary>
        /// Track Inventory - تتبع المخزون
        /// </summary>
        public bool TrackInventory { get; set; } = true;

        /// <summary>
        /// Allow Fractional Qty - يسمح بالكميات الكسرية
        /// </summary>
        public bool AllowFractionalQty { get; set; }

        /// <summary>
        /// Image URL - رابط الصورة
        /// </summary>
        public string ImageUrl { get; set; }

        /// <summary>
        /// Description - الوصف
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Notes - ملاحظات
        /// </summary>
        public string Notes { get; set; }

        /// <summary>
        /// Is Active - نشط
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
}

