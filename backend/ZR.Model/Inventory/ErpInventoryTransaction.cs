using Infrastructure.Converter;
using SqlSugar;
using Newtonsoft.Json;
using ZR.Model.System;

namespace ZR.Model.Inventory
{
    /// <summary>
    /// Inventory Transaction - حركة المخزن
    /// </summary>
    [SugarTable("erp_inventory_transactions")]
    public class ErpInventoryTransaction : SysBase
    {
        /// <summary>
        /// Transaction ID - معرف الحركة
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long TransactionId { get; set; }

        /// <summary>
        /// Transaction Number - رقم الحركة
        /// </summary>
        public string TransactionNumber { get; set; }

        /// <summary>
        /// Transaction Date - تاريخ الحركة
        /// </summary>
        public DateTime TransactionDate { get; set; }

        /// <summary>
        /// Transaction Type - نوع الحركة
        /// opening_balance, direct_addition, direct_return, purchase_order,
        /// purchase_invoice, purchase_return, stock_transfer, custody_issue,
        /// custody_settle, damaged, stock_count, adjustment, sale, sale_return
        /// </summary>
        public string TransactionType { get; set; }

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
        /// Unit Cost - تكلفة الوحدة
        /// </summary>
        public decimal UnitCost { get; set; }

        /// <summary>
        /// Total Cost - إجمالي التكلفة
        /// </summary>
        public decimal TotalCost { get; set; }

        /// <summary>
        /// Balance Before - الرصيد قبل
        /// </summary>
        public decimal BalanceBefore { get; set; }

        /// <summary>
        /// Balance After - الرصيد بعد
        /// </summary>
        public decimal BalanceAfter { get; set; }

        /// <summary>
        /// Reference Type - نوع المرجع
        /// </summary>
        public string ReferenceType { get; set; }

        /// <summary>
        /// Reference ID - معرف المرجع
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? ReferenceId { get; set; }

        /// <summary>
        /// Reference Number - رقم المرجع
        /// </summary>
        public string ReferenceNumber { get; set; }

        /// <summary>
        /// From Warehouse ID - من المخزن (للتحويلات)
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? FromWarehouseId { get; set; }

        /// <summary>
        /// To Warehouse ID - إلى المخزن (للتحويلات)
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? ToWarehouseId { get; set; }

        /// <summary>
        /// Employee ID - معرف الموظف (للعهد)
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? EmployeeId { get; set; }

        /// <summary>
        /// Supplier ID - معرف المورد
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? SupplierId { get; set; }

       
        /// <summary>
        /// Status - الحالة (draft/confirmed/cancelled)
        /// </summary>
        public string Status { get; set; } = "draft";

        /// <summary>
        /// Description - الوصف
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Notes - ملاحظات
        /// </summary>
        public string Notes { get; set; }
    }
}

