using Infrastructure.Converter;
using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using ZR.Model.System;
using ZR.Model.Marketing.Another;

namespace ZR.Model.Marketing.Dto
{
    /// <summary>
    /// Promo Code Query DTO
    /// </summary>
    public class PromoCodeQueryDto : PagerInfo
    {
        public string Code { get; set; }
        public string CodeType { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? PromotionId { get; set; }
    }

    /// <summary>
    /// Promo Code Input DTO
    /// </summary>
    public class PromoCodeDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long PromoCodeId { get; set; }

        [Required(ErrorMessage = "Code is required")]
        [StringLength(50, ErrorMessage = "Code cannot exceed 50 characters")]
        public string Code { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? PromotionId { get; set; }

        [Required(ErrorMessage = "Code type is required")]
        public string CodeType { get; set; }

        [Range(0, 100, ErrorMessage = "Discount percent must be between 0 and 100")]
        public decimal DiscountPercent { get; set; }

        public decimal? DiscountFixed { get; set; }
        public decimal? MaxDiscount { get; set; }

        [Required(ErrorMessage = "Valid from date is required")]
        public DateTime ValidFrom { get; set; }

        [Required(ErrorMessage = "Valid to date is required")]
        public DateTime ValidTo { get; set; }

        public int? MaxRedemptions { get; set; }
        public string ApplicableProducts { get; set; }
        public string SpecificItemIds { get; set; }
        public bool IsActive { get; set; } = true;

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? CreatedForMemberId { get; set; }

        public string Remark { get; set; }
    }

    /// <summary>
    /// Bulk Promo Code Generation Request
    /// </summary>
    public class BulkPromoCodeRequest
    {
        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, 10000, ErrorMessage = "Quantity must be between 1 and 10000")]
        public int Quantity { get; set; }

        public string Prefix { get; set; }

        [Range(4, 16, ErrorMessage = "Code length must be between 4 and 16")]
        public int CodeLength { get; set; } = 8;

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? PromotionId { get; set; }

        public string CodeType { get; set; } = "single_use";

        [Range(0, 100, ErrorMessage = "Discount percent must be between 0 and 100")]
        public decimal DiscountPercent { get; set; }

        public decimal? DiscountFixed { get; set; }
        public decimal? MaxDiscount { get; set; }

        [Required(ErrorMessage = "Valid from date is required")]
        public DateTime ValidFrom { get; set; }

        [Required(ErrorMessage = "Valid to date is required")]
        public DateTime ValidTo { get; set; }

        public int? MaxRedemptions { get; set; }
        public string ApplicableProducts { get; set; }
    }

    /// <summary>
    /// Promo Code Validation Request
    /// </summary>
    public class PromoCodeValidateRequest
    {
        [Required(ErrorMessage = "Code is required")]
        public string Code { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? MemberId { get; set; }

        [Required(ErrorMessage = "Amount is required")]
        public decimal Amount { get; set; }
    }

    /// <summary>
    /// Promo Code Validation Result
    /// </summary>
    public class PromoCodeValidationResult
    {
        public bool IsValid { get; set; }
        public string Message { get; set; }
        public string MessageAr { get; set; }
        public ErpPromoCode PromoCode { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalAmount { get; set; }
    }

    /// <summary>
    /// Promo Code Redeem Request
    /// </summary>
    public class PromoCodeRedeemRequest
    {
        [Required(ErrorMessage = "Code is required")]
        public string Code { get; set; }

        [Required(ErrorMessage = "Member ID is required")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long MemberId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? InvoiceId { get; set; }

        [Required(ErrorMessage = "Original amount is required")]
        public decimal OriginalAmount { get; set; }
    }

    /// <summary>
    /// Promo Code Statistics DTO
    /// </summary>
    public class PromoCodeStatsDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long PromoCodeId { get; set; }
        public string Code { get; set; }
        public int TotalRedemptions { get; set; }
        public int RemainingRedemptions { get; set; }
        public decimal TotalDiscountGiven { get; set; }
        public decimal AverageDiscountPerUse { get; set; }
        public int UniqueMembers { get; set; }
    }
}

