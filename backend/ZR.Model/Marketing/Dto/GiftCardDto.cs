using Infrastructure.Converter;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using ZR.Model.System;

namespace ZR.Model.Marketing.Dto
{
    /// <summary>
    /// Gift Card Query DTO
    /// </summary>
    public class GiftCardQueryDto : PagerInfo
    {
        public string CardNumber { get; set; }
        public string Status { get; set; }
        public string CardType { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? PurchasedByMemberId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? AssignedToMemberId { get; set; }

        public DateTime? IssueDateFrom { get; set; }
        public DateTime? IssueDateTo { get; set; }
        public bool? IsActivated { get; set; }
    }

    /// <summary>
    /// Gift Card Create DTO
    /// </summary>
    public class GiftCardCreateDto
    {
        [Required(ErrorMessage = "Amount is required")]
        [Range(10, 100000, ErrorMessage = "Amount must be between 10 and 100000")]
        public decimal Amount { get; set; }

        public string Currency { get; set; } = "SAR";

        [Required(ErrorMessage = "Card type is required")]
        public string CardType { get; set; }

        public string CardDesign { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? PurchasedByMemberId { get; set; }

        public string RecipientName { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string RecipientEmail { get; set; }

        [Phone(ErrorMessage = "Invalid phone format")]
        public string RecipientPhone { get; set; }

        [StringLength(500, ErrorMessage = "Personal message cannot exceed 500 characters")]
        public string PersonalMessage { get; set; }

        public int? ValidityMonths { get; set; } = 12;

        public string Remark { get; set; }
    }

    /// <summary>
    /// Gift Card Update DTO
    /// </summary>
    public class GiftCardDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long GiftCardId { get; set; }

        public string CardNumber { get; set; }
        public decimal InitialBalance { get; set; }
        public decimal CurrentBalance { get; set; }
        public string Currency { get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime ExpiryDate { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? PurchasedByMemberId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? AssignedToMemberId { get; set; }

        public string RecipientName { get; set; }
        public string RecipientEmail { get; set; }
        public string RecipientPhone { get; set; }
        public string PersonalMessage { get; set; }
        public string CardType { get; set; }
        public string CardDesign { get; set; }
        public string Status { get; set; }
        public bool IsActivated { get; set; }
        public DateTime? ActivatedAt { get; set; }
        public string Remark { get; set; }
    }

    /// <summary>
    /// Gift Card Activate Request
    /// </summary>
    public class GiftCardActivateRequest
    {
        [Required(ErrorMessage = "Card number is required")]
        public string CardNumber { get; set; }

        [Required(ErrorMessage = "PIN is required")]
        [StringLength(10, MinimumLength = 4, ErrorMessage = "PIN must be between 4 and 10 characters")]
        public string Pin { get; set; }
    }

    /// <summary>
    /// Gift Card Balance Check Request
    /// </summary>
    public class GiftCardBalanceRequest
    {
        [Required(ErrorMessage = "Card number is required")]
        public string CardNumber { get; set; }

        [Required(ErrorMessage = "PIN is required")]
        public string Pin { get; set; }
    }

    /// <summary>
    /// Gift Card Balance Result
    /// </summary>
    public class GiftCardBalanceResult
    {
        public bool IsValid { get; set; }
        public string Message { get; set; }
        public string MessageAr { get; set; }
        public string CardNumber { get; set; }
        public decimal CurrentBalance { get; set; }
        public string Currency { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string Status { get; set; }
        public bool IsExpired { get; set; }
    }

    /// <summary>
    /// Gift Card Redeem Request
    /// </summary>
    public class GiftCardRedeemRequest
    {
        [Required(ErrorMessage = "Card number is required")]
        public string CardNumber { get; set; }

        [Required(ErrorMessage = "PIN is required")]
        public string Pin { get; set; }

        [Required(ErrorMessage = "Amount is required")]
        [Range(0.01, 100000, ErrorMessage = "Amount must be between 0.01 and 100000")]
        public decimal Amount { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? InvoiceId { get; set; }

        public string Notes { get; set; }
    }

    /// <summary>
    /// Gift Card Redeem Result
    /// </summary>
    public class GiftCardRedeemResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string MessageAr { get; set; }
        public decimal AmountRedeemed { get; set; }
        public decimal RemainingBalance { get; set; }
        public string TransactionId { get; set; }
    }

    /// <summary>
    /// Gift Card Refund Request
    /// </summary>
    public class GiftCardRefundRequest
    {
        [Required(ErrorMessage = "Gift card ID is required")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long GiftCardId { get; set; }

        [Required(ErrorMessage = "Amount is required")]
        [Range(0.01, 100000, ErrorMessage = "Amount must be between 0.01 and 100000")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Reason is required")]
        public string Reason { get; set; }
    }

    /// <summary>
    /// Gift Card Transaction Query DTO
    /// </summary>
    public class GiftCardTransactionQueryDto : PagerInfo
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? GiftCardId { get; set; }

        public string TransactionType { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    /// <summary>
    /// Gift Card Transaction DTO
    /// </summary>
    public class GiftCardTransactionDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long TransactionId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long GiftCardId { get; set; }

        public string CardNumber { get; set; }
        public string TransactionType { get; set; }
        public decimal Amount { get; set; }
        public decimal BalanceBefore { get; set; }
        public decimal BalanceAfter { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? InvoiceId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? ProcessedBy { get; set; }

        public string ProcessedByName { get; set; }
        public string Notes { get; set; }
        public DateTime TransactionDate { get; set; }
    }

    /// <summary>
    /// Gift Card Statistics DTO
    /// </summary>
    public class GiftCardStatsDto
    {
        public int TotalCardsIssued { get; set; }
        public int ActiveCards { get; set; }
        public int UsedCards { get; set; }
        public int ExpiredCards { get; set; }
        public decimal TotalValueIssued { get; set; }
        public decimal TotalValueRedeemed { get; set; }
        public decimal TotalValueOutstanding { get; set; }
        public decimal AverageCardValue { get; set; }
        public Dictionary<string, int> CardsByType { get; set; }
        public Dictionary<string, int> CardsByStatus { get; set; }
    }
}

