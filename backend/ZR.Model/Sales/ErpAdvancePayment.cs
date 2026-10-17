using Infrastructure.Converter;
using SqlSugar;
using Newtonsoft.Json;
using ZR.Model.System;

namespace ZR.Model.Sales
{
    /// <summary>
    /// Advance Payment - دفعات مقدمة
    /// </summary>
    [SugarTable("erp_advance_payments")]
    [SqlDict(
        dictType: "erp_advance_payment",
        dictName: "Advance Payment - دفعات مقدمة",
        valueColumn: "advanceid",
        labelColumns: "advancenumber",
        RemarkColumn = "notes",
        OrderBy = "paymentdate DESC",
        Description = "Auto-generated from ErpAdvancePayment table"
    )]
    public class ErpAdvancePayment : SysBase
    {
        /// <summary>
        /// Advance ID - معرف الدفعة المقدمة
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long AdvanceId { get; set; }

        /// <summary>
        /// Advance Number - رقم الدفعة المقدمة
        /// </summary>
        public string AdvanceNumber { get; set; }

        /// <summary>
        /// Customer ID - معرف العميل
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long CustomerId { get; set; }

        /// <summary>
        /// Payment Date - تاريخ الدفع
        /// </summary>
        public DateTime PaymentDate { get; set; }

        /// <summary>
        /// Amount - المبلغ
        /// </summary>
        public decimal Amount { get; set; }

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
        /// Used Amount - المبلغ المستخدم
        /// </summary>
        public decimal UsedAmount { get; set; }

        /// <summary>
        /// Remaining Amount - المبلغ المتبقي
        /// </summary>
        public decimal RemainingAmount { get; set; }

        /// <summary>
        /// Currency - العملة
        /// </summary>
        public string Currency { get; set; } = "SAR";

        /// <summary>
        /// Status - الحالة (pending/confirmed/used/cancelled)
        /// </summary>
        public string Status { get; set; } = "pending";
         

        /// <summary>
        /// Receipt ID - معرف سند القبض
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? ReceiptId { get; set; }

        /// <summary>
        /// Journal Entry ID - معرف القيد اليومي
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? JournalEntryId { get; set; }

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

