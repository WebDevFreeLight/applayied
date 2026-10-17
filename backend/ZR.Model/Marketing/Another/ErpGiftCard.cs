using Infrastructure.Converter;
using System;
using SqlSugar;
using Newtonsoft.Json;
using ZR.Model.System;

namespace ZR.Model.Marketing.Another
{
    /// <summary>
    /// Gift Card Entity - بطاقات الهدايا
    /// </summary>
    [SugarTable("erp_gift_card")]
    public class ErpGiftCard : SysBase
    {
        /// <summary>
        /// Gift Card ID
        /// </summary>
        [SugarColumn(IsPrimaryKey = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long GiftCardId { get; set; }

        /// <summary>
        /// Card Number (16-digit)
        /// </summary>
        [SugarColumn(Length = 20)]
        public string CardNumber { get; set; }

        /// <summary>
        /// PIN (4-digit)
        /// </summary>
        [SugarColumn(Length = 10)]
        public string Pin { get; set; }

        /// <summary>
        /// Initial Balance
        /// </summary>
        [SugarColumn(DecimalDigits = 2)]
        public decimal InitialBalance { get; set; }

        /// <summary>
        /// Current Balance
        /// </summary>
        [SugarColumn(DecimalDigits = 2)]
        public decimal CurrentBalance { get; set; }

        /// <summary>
        /// Currency (default SAR)
        /// </summary>
        [SugarColumn(Length = 10)]
        public string Currency { get; set; } = "SAR";

        /// <summary>
        /// Issue Date
        /// </summary>
        public DateTime IssueDate { get; set; }

        /// <summary>
        /// Expiry Date
        /// </summary>
        public DateTime ExpiryDate { get; set; }

        /// <summary>
        /// Purchased By Member ID
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsNullable = true)]
        public long? PurchasedByMemberId { get; set; }

        /// <summary>
        /// Assigned To Member ID
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsNullable = true)]
        public long? AssignedToMemberId { get; set; }

        /// <summary>
        /// Recipient Name
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = true)]
        public string RecipientName { get; set; }

        /// <summary>
        /// Recipient Email
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = true)]
        public string RecipientEmail { get; set; }

        /// <summary>
        /// Recipient Phone
        /// </summary>
        [SugarColumn(Length = 20, IsNullable = true)]
        public string RecipientPhone { get; set; }

        /// <summary>
        /// Personal Message
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true)]
        public string PersonalMessage { get; set; }

        /// <summary>
        /// Card Type: physical, digital
        /// </summary>
        [SugarColumn(Length = 20)]
        public string CardType { get; set; }

        /// <summary>
        /// Card Design template name
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true)]
        public string CardDesign { get; set; }

        /// <summary>
        /// Status: active, used, expired, cancelled
        /// </summary>
        [SugarColumn(Length = 20)]
        public string Status { get; set; } = "active";

        /// <summary>
        /// Is Activated flag
        /// </summary>
        public bool IsActivated { get; set; } = false;

        /// <summary>
        /// Activation Date/Time
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public DateTime? ActivatedAt { get; set; }
    }

    /// <summary>
    /// Gift Card Transaction Entity - سجل معاملات البطاقة
    /// </summary>
    [SugarTable("erp_gift_card_transaction")]
    public class ErpGiftCardTransaction : SysBase
    {
        /// <summary>
        /// Transaction ID
        /// </summary>
        [SugarColumn(IsPrimaryKey = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long TransactionId { get; set; }

        /// <summary>
        /// Gift Card ID
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long GiftCardId { get; set; }

        /// <summary>
        /// Transaction Type: purchase, redemption, refund, adjustment
        /// </summary>
        [SugarColumn(Length = 30)]
        public string TransactionType { get; set; }

        /// <summary>
        /// Amount (positive for credit, negative for debit)
        /// </summary>
        [SugarColumn(DecimalDigits = 2)]
        public decimal Amount { get; set; }

        /// <summary>
        /// Balance Before Transaction
        /// </summary>
        [SugarColumn(DecimalDigits = 2)]
        public decimal BalanceBefore { get; set; }

        /// <summary>
        /// Balance After Transaction
        /// </summary>
        [SugarColumn(DecimalDigits = 2)]
        public decimal BalanceAfter { get; set; }

        /// <summary>
        /// Invoice ID (if applicable)
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsNullable = true)]
        public long? InvoiceId { get; set; }

        /// <summary>
        /// Processed By User ID
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsNullable = true)]
        public long? ProcessedBy { get; set; }

        /// <summary>
        /// Transaction Notes
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true)]
        public string Notes { get; set; }

        /// <summary>
        /// Transaction Date/Time
        /// </summary>
        public DateTime TransactionDate { get; set; }
    }
}

