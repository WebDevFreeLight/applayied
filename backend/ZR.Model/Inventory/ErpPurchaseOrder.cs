using SqlSugar;
using ZR.Model.System.Model;

namespace ZR.Model.Inventory
{
    /// <summary>
    /// Purchase Order - أمر الشراء
    /// </summary>
    [SugarTable("erp_purchase_order")]
    public class ErpPurchaseOrder : SysBase
    {
        /// <summary>
        /// Purchase Order ID - معرف أمر الشراء
        /// </summary>
        [SugarColumn(IsPrimaryKey = true)]
        public long PurchaseOrderId { get; set; }

        /// <summary>
        /// Purchase Order Number - رقم أمر الشراء
        /// </summary>
        public string PurchaseOrderNumber { get; set; }

        /// <summary>
        /// Supplier ID - معرف المورد
        /// </summary>
        public long SupplierId { get; set; }

        /// <summary>
        /// Warehouse ID - معرف المستودع
        /// </summary>
        public long WarehouseId { get; set; }

        /// <summary>
        /// Order Date - تاريخ الطلب
        /// </summary>
        public DateTime OrderDate { get; set; }

        /// <summary>
        /// Expected Delivery Date - تاريخ التسليم المتوقع
        /// </summary>
        public DateTime? ExpectedDeliveryDate { get; set; }

        /// <summary>
        /// Actual Delivery Date - تاريخ التسليم الفعلي
        /// </summary>
        public DateTime? ActualDeliveryDate { get; set; }

        /// <summary>
        /// Status - الحالة (draft, pending, approved, received, cancelled)
        /// </summary>
        public string Status { get; set; } = "draft";

        /// <summary>
        /// Subtotal - المجموع الفرعي
        /// </summary>
        public decimal Subtotal { get; set; }

        /// <summary>
        /// Discount Amount - مبلغ الخصم
        /// </summary>
        public decimal DiscountAmount { get; set; }

        /// <summary>
        /// Tax Amount - مبلغ الضريبة
        /// </summary>
        public decimal TaxAmount { get; set; }

        /// <summary>
        /// Total Amount - المبلغ الإجمالي
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Paid Amount - المبلغ المدفوع
        /// </summary>
        public decimal PaidAmount { get; set; }

        /// <summary>
        /// Notes - ملاحظات
        /// </summary>
        public string Notes { get; set; }

        /// <summary>
        /// Approved By - تمت الموافقة بواسطة
        /// </summary>
        public string ApprovedBy { get; set; }

        /// <summary>
        /// Approved Date - تاريخ الموافقة
        /// </summary>
        public DateTime? ApprovedDate { get; set; }

        /// <summary>
        /// Is Active - نشط
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Purchase Order Items - أصناف أمر الشراء
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public List<ErpPurchaseOrderItem> Items { get; set; }
    }

    /// <summary>
    /// Purchase Order Item - صنف أمر الشراء
    /// </summary>
    [SugarTable("erp_purchase_order_item")]
    public class ErpPurchaseOrderItem : SysBase
    {
        /// <summary>
        /// Item ID - معرف الصنف
        /// </summary>
        [SugarColumn(IsPrimaryKey = true)]
        public long ItemId { get; set; }

        /// <summary>
        /// Purchase Order ID - معرف أمر الشراء
        /// </summary>
        public long PurchaseOrderId { get; set; }

        /// <summary>
        /// Product ID - معرف المنتج
        /// </summary>
        public long ProductId { get; set; }

        /// <summary>
        /// Product Name - اسم المنتج
        /// </summary>
        public string ProductName { get; set; }

        /// <summary>
        /// Unit - الوحدة
        /// </summary>
        public string Unit { get; set; }

        /// <summary>
        /// Ordered Quantity - الكمية المطلوبة
        /// </summary>
        public decimal OrderedQuantity { get; set; }

        /// <summary>
        /// Received Quantity - الكمية المستلمة
        /// </summary>
        public decimal ReceivedQuantity { get; set; }

        /// <summary>
        /// Unit Price - سعر الوحدة
        /// </summary>
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// Discount Percentage - نسبة الخصم
        /// </summary>
        public decimal DiscountPercentage { get; set; }

        /// <summary>
        /// Tax Percentage - نسبة الضريبة
        /// </summary>
        public decimal TaxPercentage { get; set; }

        /// <summary>
        /// Total Price - السعر الإجمالي
        /// </summary>
        public decimal TotalPrice { get; set; }

        /// <summary>
        /// Notes - ملاحظات
        /// </summary>
        public string Notes { get; set; }
    }
}

