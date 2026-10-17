using Infrastructure.Converter;
using SqlSugar;
using Newtonsoft.Json;
using ZR.Model.System;

namespace ZR.Model.Inventory
{
    /// <summary>
    /// Warehouse Stock - رصيد المخزن
    /// </summary>
    [SugarTable("erp_warehouse_stock")]
    public class ErpWarehouseStock : SysBase
    {
        /// <summary>
        /// Stock ID - معرف الرصيد
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long StockId { get; set; }

        /// <summary>
        /// Warehouse ID - معرف المخزن
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long WarehouseId { get; set; }

        /// <summary>
        /// Product ID - معرف المنتج
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ProductId { get; set; }

        /// <summary>
        /// Quantity - الكمية
        /// </summary>
        public decimal Quantity { get; set; }

        /// <summary>
        /// Reserved Quantity - الكمية المحجوزة
        /// </summary>
        public decimal ReservedQuantity { get; set; }

        /// <summary>
        /// Available Quantity - الكمية المتاحة
        /// </summary>
        public decimal AvailableQuantity { get; set; }

        /// <summary>
        /// Average Cost - متوسط التكلفة
        /// </summary>
        public decimal AverageCost { get; set; }

        /// <summary>
        /// Last Receipt Date - تاريخ آخر استلام
        /// </summary>
        public DateTime? LastReceiptDate { get; set; }

        /// <summary>
        /// Last Issue Date - تاريخ آخر صرف
        /// </summary>
        public DateTime? LastIssueDate { get; set; }

        /// <summary>
        /// Last Stock Count Date - تاريخ آخر جرد
        /// </summary>
        public DateTime? LastStockCountDate { get; set; }
    }
}

