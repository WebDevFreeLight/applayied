using Infrastructure.Converter;
using SqlSugar;
using Newtonsoft.Json;
using ZR.Model.System;

namespace ZR.Model.Sales
{
    /// <summary>
    /// Sales Return - مرتجع المبيعات
    /// </summary>
    [SugarTable("erp_sales_returns")]
    [SqlDict(
        dictType: "erp_sales_return",
        dictName: "Sales Return - مرتجع المبيعات",
        valueColumn: "returnid",
        labelColumns: "returnnumber",
        RemarkColumn = "description",
        OrderBy = "returndate DESC",
        Description = "Auto-generated from ErpSalesReturn table"
    )]
    public class ErpSalesReturn : SysBase
    {
        /// <summary>
        /// Return ID - معرف المرتجع
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ReturnId { get; set; }

        /// <summary>
        /// Return Number - رقم المرتجع
        /// </summary>
        public string ReturnNumber { get; set; }

        /// <summary>
        /// Return Date - تاريخ المرتجع
        /// </summary>
        public DateTime ReturnDate { get; set; }

        /// <summary>
        /// Return Type - نوع المرتجع (sale/expense/advance)
        /// </summary>
        public string ReturnType { get; set; } = "sale";

        /// <summary>
        /// Original Invoice ID - معرف الفاتورة الأصلية
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? OriginalInvoiceId { get; set; }

        /// <summary>
        /// Original Invoice Number - رقم الفاتورة الأصلية
        /// </summary>
        public string OriginalInvoiceNumber { get; set; }

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
        /// Subtotal - المجموع الفرعي
        /// </summary>
        public decimal Subtotal { get; set; }

        /// <summary>
        /// VAT Amount - مبلغ الضريبة
        /// </summary>
        public decimal VatAmount { get; set; }

        /// <summary>
        /// Total Amount - المبلغ الإجمالي
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Refund Method - طريقة الاسترداد (cash/credit/original)
        /// </summary>
        public string RefundMethod { get; set; }

        /// <summary>
        /// Refunded Amount - المبلغ المسترد
        /// </summary>
        public decimal RefundedAmount { get; set; }

        /// <summary>
        /// Credit Note Amount - مبلغ الإشعار الدائن
        /// </summary>
        public decimal CreditNoteAmount { get; set; }

        /// <summary>
        /// Treasury ID - معرف الخزينة
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? TreasuryId { get; set; }

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
        /// Reason - السبب
        /// </summary>
        public string Reason { get; set; }

        /// <summary>
        /// Description - الوصف
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Journal Entry ID - معرف القيد اليومي
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? JournalEntryId { get; set; }

        /// <summary>
        /// Status - الحالة (draft/confirmed/cancelled)
        /// </summary>
        public string Status { get; set; } = "draft";

        /// <summary>
        /// Notes - ملاحظات
        /// </summary>
        public string Notes { get; set; }

        /// <summary>
        /// Is Active - نشط
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Navigation - Return Items
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public List<ErpSalesReturnItem> Items { get; set; }
    }
}

