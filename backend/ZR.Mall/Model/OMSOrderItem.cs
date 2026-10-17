using Infrastructure.Converter;
using ZR.Model.System;

namespace ZR.Mall.Model
{
    /// <summary>
    /// Order Item
    /// </summary>
    [SugarTable("oms_order_item", "Order Item")]
    [Tenant("1")]
    
    [SqlDict(
        dictType: "oms_order_item",
        dictName: "O M S Order Item",
        valueColumn: "ItemId",
        labelColumns: "productname",
        RemarkColumn = "remark",
        OrderBy = "ItemId ASC",
        Description = "Auto-generated from OMSOrderItem table"
    )]
    public class OMSOrderItem:SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ItemId { get; set; }
        /// <summary>
        /// Order ID
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long OrderId { get; set; }
        /// <summary>
        /// Product ID (Snapshot)
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ProductId { get; set; }
        /// <summary>
        /// Product Name (Snapshot)
        /// </summary>
        public string ProductName { get; set; }
        /// <summary>
        /// Product Picture (Snapshot)
        /// </summary>
        public string ProductPic { get; set; }
        /// <summary>
        /// skuId
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long SkuId { get; set; }
        /// <summary>
        /// Product Unit Price Snapshot (Price at Order Time) (Snapshot)
        /// </summary>
        public decimal UnitPrice  { get; set; }
        /// <summary>
        /// Payment Amount (Unit Price * Quantity)
        /// </summary>
        public decimal TotalPrice { get; set; }
        /// <summary>
        /// Quantity
        /// </summary>
        public int Quantity { get; set; }
        /// <summary>
        /// Specification Snapshot (e.g.: Color:Red; Size:XL)
        /// </summary>
        public string SkuSpec { get; set; }
        public DateTime AddTime { get; set; }
    }
}

