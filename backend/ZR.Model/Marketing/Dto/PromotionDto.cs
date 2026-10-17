using Infrastructure.Converter;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using ZR.Model.System;
using ZR.Model.Marketing.Another;

namespace ZR.Model.Marketing.Dto
{
    /// <summary>
    /// Promotion Query DTO
    /// </summary>
    public class PromotionQueryDto : PagerInfo
    {
        public string PromotionCode { get; set; }
        public string PromotionName { get; set; }
        public string PromotionType { get; set; }
        public string Status { get; set; }
        public string TargetAudience { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool? IsActive { get; set; }
    }

    /// <summary>
    /// Promotion Input DTO
    /// </summary>
    public class PromotionDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long PromotionId { get; set; }

        [Required(ErrorMessage = "Promotion code is required")]
        [StringLength(50, ErrorMessage = "Promotion code cannot exceed 50 characters")]
        public string PromotionCode { get; set; }

        [Required(ErrorMessage = "Promotion name is required")]
        [StringLength(200, ErrorMessage = "Promotion name cannot exceed 200 characters")]
        public string PromotionName { get; set; }

        [StringLength(200, ErrorMessage = "Arabic promotion name cannot exceed 200 characters")]
        public string PromotionNameAr { get; set; }

        [Required(ErrorMessage = "Promotion type is required")]
        public string PromotionType { get; set; }

        [Required(ErrorMessage = "Discount value is required")]
        [Range(0, 100000, ErrorMessage = "Discount value must be between 0 and 100000")]
        public decimal DiscountValue { get; set; }

        public decimal? MaxDiscountAmount { get; set; }
        public decimal? MinPurchaseAmount { get; set; }

        [Required(ErrorMessage = "Start date is required")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "End date is required")]
        public DateTime EndDate { get; set; }

        public string ApplicableTo { get; set; }
        public string ApplicableItemsJson { get; set; }
        public int? UsageLimit { get; set; }
        public int? UsageLimitPerMember { get; set; }
        public string TargetAudience { get; set; }
        public string TargetTiersJson { get; set; }
        public bool RequiresPromoCode { get; set; }
        public bool IsStackable { get; set; }
        public int Priority { get; set; }
        public bool IsActive { get; set; } = true;
        public string Status { get; set; } = "draft";
        public string Remark { get; set; }
    }

    /// <summary>
    /// Promotion Validation Request
    /// </summary>
    public class PromotionValidateRequest
    {
        [Required(ErrorMessage = "Promotion code is required")]
        public string Code { get; set; }

        [Required(ErrorMessage = "Member ID is required")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long MemberId { get; set; }

        [Required(ErrorMessage = "Amount is required")]
        public decimal Amount { get; set; }

        public string ItemType { get; set; }
    }

    /// <summary>
    /// Promotion Validation Result
    /// </summary>
    public class PromotionValidationResult
    {
        public bool IsValid { get; set; }
        public string Message { get; set; }
        public string MessageAr { get; set; }
        public ErpPromotion Promotion { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalAmount { get; set; }
    }

    /// <summary>
    /// Promotion Usage Record Request
    /// </summary>
    public class PromotionUsageRequest
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long PromotionId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long MemberId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? InvoiceId { get; set; }

        public decimal DiscountApplied { get; set; }
    }

    /// <summary>
    /// Promotion Analytics DTO
    /// </summary>
    public class PromotionAnalyticsDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long PromotionId { get; set; }
        public string PromotionCode { get; set; }
        public string PromotionName { get; set; }
        public int TotalUsageCount { get; set; }
        public int UniqueMembers { get; set; }
        public decimal TotalDiscountGiven { get; set; }
        public decimal AverageDiscountPerUse { get; set; }
        public decimal RevenueGenerated { get; set; }
        public List<DailyUsageDto> DailyUsage { get; set; }
    }

    /// <summary>
    /// Daily Usage DTO
    /// </summary>
    public class DailyUsageDto
    {
        public DateTime Date { get; set; }
        public int UsageCount { get; set; }
        public decimal DiscountAmount { get; set; }
    }
}

