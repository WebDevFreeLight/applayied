
using ZR.Mall.Enum; 
using Infrastructure.Converter;
using ZR.Model.System;

namespace ZR.Mall.Model
{
    /// <summary>
    /// Order Management
    /// </summary>
    [SugarTable("oms_order", "Order Table")]
    [SugarIndex("index_orderNo", nameof(OrderNo), OrderByType.Asc, true)]
    [Tenant("1")]
    
    [SqlDict(
        dictType: "oms_order",
        dictName: "O M S Order",
        valueColumn: "Id",
        labelColumns: "orderno",
        RemarkColumn = "ordernote",
        OrderBy = "Id ASC",
        Description = "Auto-generated from OMSOrder table"
    )]
    public class OMSOrder : SysBase
    {
        /// <summary>
        /// Id 
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long Id { get; set; }

        /// <summary>
        /// Order Number 
        /// </summary>
        public string OrderNo { get; set; }

        /// <summary>
        /// User ID 
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long UserId { get; set; }

        /// <summary>
        /// Gift To User ID
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? ToUserId { get; set; }

        /// <summary>
        /// Total Amount 
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Payment Amount 
        /// </summary>
        public decimal PayAmount { get; set; }

        /// <summary>
        /// Order Status: 0-Pending Payment 1-Pending Shipment 2-Shipped 3-Completed 4-Cancelled 5-Refunded
        /// </summary>
        public OrderStatusEnum OrderStatus { get; set; }
        /// <summary>
        /// Delivery Status: 0->Not Shipped; 1->Shipped; 2->Received;
        /// </summary>
        [SugarColumn(DefaultValue = "0")]
        public DeliveryStatusEnum DeliveryStatus { get; set; }

        /// <summary>
        /// Refund Status: 0->No Refund; 1->Refunding; 2->Refunded; 3->Returning; 4->Returned
        /// </summary>
        [SugarColumn(DefaultValue = "0")]
        public RefundStatusEnum RefundStatus { get; set; }
         

        /// <summary>
        /// Payment Time 
        /// </summary>
        public DateTime? PayTime { get; set; }

        /// <summary>
        /// Cancel Time 
        /// </summary>
        public DateTime? CancelTime { get; set; }

        /// <summary>
        /// Shipping Time
        /// </summary>
        public DateTime? ShipTime { get; set; }

        /// <summary>
        /// Confirm Receipt Time
        /// </summary>
        public DateTime? ConfirmTime { get; set; }

        /// <summary>
        /// Order Note (User)
        /// </summary>
        public string OrderNote { get; set; }

        /// <summary>
        /// Merchant Note 
        /// </summary>
        public string MerchantNote { get; set; }

        /// <summary>
        /// Shipping Address 
        /// </summary>
        [SugarColumn(IsJson = true)]
        public AddressSnapshot AddressSnapshot { get; set; }

        /// <summary>
        /// Logistics Company 
        /// </summary>
        public string DeliveryCompany { get; set; }

        /// <summary>
        /// Tracking Number 
        /// </summary>
        public string DeliveryNo { get; set; }

        /// <summary>
        /// Is Deleted (Soft Delete) 1.Deleted 0.Not Deleted
        /// </summary>
        [SugarColumn(DefaultValue = "0")]
        public int IsDelete { get; set; }

        /// <summary>
        /// Order Items
        /// </summary>
        [Navigate(NavigateType.OneToMany, nameof(OMSOrderItem.OrderId))]
        public List<OMSOrderItem> Items { get; set; }
    }

    public class AddressSnapshot
    {
        public string UserName { get; set; }
        public string Phone { get; set; }
        public string Province { get; set; }
        public string City { get; set; }
        public string District { get; set; }
        public string DetailAddress { get; set; }
    }
}

