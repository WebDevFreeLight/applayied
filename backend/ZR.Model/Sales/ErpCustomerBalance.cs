using Infrastructure.Converter;
using SqlSugar;
using Newtonsoft.Json;
using ZR.Model.System;

namespace ZR.Model.Sales
{
    /// <summary>
    /// Customer Balance - أرصدة العملاء
    /// </summary>
    [SugarTable("erp_customer_balances")]
    public class ErpCustomerBalance : SysBase
    {
        /// <summary>
        /// Balance ID - معرف الرصيد
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long BalanceId { get; set; }

        /// <summary>
        /// Customer ID - معرف العميل
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long CustomerId { get; set; }

        /// <summary>
        /// Transaction Date - تاريخ الحركة
        /// </summary>
        public DateTime TransactionDate { get; set; }

        /// <summary>
        /// Transaction Type - نوع الحركة (invoice/payment/return/adjustment)
        /// </summary>
        public string TransactionType { get; set; }

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
        /// Debit Amount - المبلغ المدين
        /// </summary>
        public decimal DebitAmount { get; set; }

        /// <summary>
        /// Credit Amount - المبلغ الدائن
        /// </summary>
        public decimal CreditAmount { get; set; }

        /// <summary>
        /// Balance - الرصيد
        /// </summary>
        public decimal Balance { get; set; }

        /// <summary>
        /// Currency - العملة
        /// </summary>
        public string Currency { get; set; } = "SAR";
         

        /// <summary>
        /// Notes - ملاحظات
        /// </summary>
        public string Notes { get; set; }
    }
}

