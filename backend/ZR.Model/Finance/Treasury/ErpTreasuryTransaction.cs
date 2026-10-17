using Infrastructure.Converter;
using SqlSugar;
using Newtonsoft.Json;
using ZR.Model.System;

namespace ZR.Model.Finance.Treasury
{
    /// <summary>
    /// Treasury Transaction - حركة الخزينة
    /// </summary>
    [SugarTable("erp_treasury_transactions")]
    public class ErpTreasuryTransaction : SysBase
    {
        /// <summary>
        /// Transaction ID - معرف الحركة
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long TransactionId { get; set; }

        /// <summary>
        /// Treasury ID - معرف الخزينة
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long TreasuryId { get; set; }

        /// <summary>
        /// Transaction Date - تاريخ الحركة
        /// </summary>
        public DateTime TransactionDate { get; set; }

        /// <summary>
        /// Transaction Type - نوع الحركة (receipt/payment/transfer_in/transfer_out/opening)
        /// </summary>
        public string TransactionType { get; set; }

        /// <summary>
        /// Amount - المبلغ
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Balance Before - الرصيد قبل
        /// </summary>
        public decimal BalanceBefore { get; set; }

        /// <summary>
        /// Balance After - الرصيد بعد
        /// </summary>
        public decimal BalanceAfter { get; set; }

        /// <summary>
        /// Currency - العملة
        /// </summary>
        public string Currency { get; set; } = "SAR";

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
        /// Description - الوصف
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Shift ID - معرف الوردية
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? ShiftId { get; set; }

   
        /// <summary>
        /// Journal Entry ID - معرف القيد اليومي
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? JournalEntryId { get; set; }

        /// <summary>
        /// Notes - ملاحظات
        /// </summary>
        public string Notes { get; set; }
    }
}

