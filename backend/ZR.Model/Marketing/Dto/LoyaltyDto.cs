using Infrastructure.Converter;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using ZR.Model.System;

namespace ZR.Model.Marketing.Dto
{
    #region Loyalty Tier DTOs

    /// <summary>
    /// Loyalty Tier Query DTO
    /// </summary>
    public class LoyaltyTierQueryDto : PagerInfo
    {
        public string TierCode { get; set; }
        public string TierName { get; set; }
        public bool? IsActive { get; set; }
    }

    /// <summary>
    /// Loyalty Tier Input DTO
    /// </summary>
    public class LoyaltyTierDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long TierId { get; set; }

        [Required(ErrorMessage = "Tier code is required")]
        [StringLength(30, ErrorMessage = "Tier code cannot exceed 30 characters")]
        public string TierCode { get; set; }

        [Required(ErrorMessage = "Tier name is required")]
        [StringLength(100, ErrorMessage = "Tier name cannot exceed 100 characters")]
        public string TierName { get; set; }

        [StringLength(100, ErrorMessage = "Arabic tier name cannot exceed 100 characters")]
        public string TierNameAr { get; set; }

        [Required(ErrorMessage = "Minimum points is required")]
        public int MinPoints { get; set; }

        [Required(ErrorMessage = "Maximum points is required")]
        public int MaxPoints { get; set; }

        [Range(0.1, 10, ErrorMessage = "Points multiplier must be between 0.1 and 10")]
        public decimal PointsMultiplier { get; set; } = 1.0m;

        [Range(0, 100, ErrorMessage = "Discount percent must be between 0 and 100")]
        public decimal DiscountPercent { get; set; }

        public string BenefitsJson { get; set; }
        public string BadgeIcon { get; set; }
        public string BadgeColor { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; } = true;
        public string Remark { get; set; }
    }

    #endregion

    #region Loyalty Points DTOs

    /// <summary>
    /// Loyalty Points Query DTO
    /// </summary>
    public class LoyaltyPointsQueryDto : PagerInfo
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? MemberId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? TierId { get; set; }

        public int? MinBalance { get; set; }
        public int? MaxBalance { get; set; }
    }

    /// <summary>
    /// Member Points Summary DTO
    /// </summary>
    public class MemberPointsSummaryDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long MemberId { get; set; }

        public int TotalEarned { get; set; }
        public int TotalRedeemed { get; set; }
        public int CurrentBalance { get; set; }
        public int PointsToNextTier { get; set; }
        public DateTime? LastActivityDate { get; set; }

        // Current Tier Info
        [JsonConverter(typeof(ValueToStringConverter))]
        public long CurrentTierId { get; set; }
        public string TierCode { get; set; }
        public string TierName { get; set; }
        public string TierNameAr { get; set; }
        public decimal PointsMultiplier { get; set; }
        public decimal TierDiscountPercent { get; set; }
        public string BadgeIcon { get; set; }
        public string BadgeColor { get; set; }

        // Progress to next tier
        public int ProgressPercent { get; set; }
        public string NextTierName { get; set; }
    }

    /// <summary>
    /// Earn Points Request DTO
    /// </summary>
    public class EarnPointsRequest
    {
        [Required(ErrorMessage = "Member ID is required")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long MemberId { get; set; }

        [Required(ErrorMessage = "Points is required")]
        [Range(1, 1000000, ErrorMessage = "Points must be between 1 and 1000000")]
        public int Points { get; set; }

        [Required(ErrorMessage = "Transaction type is required")]
        public string TransactionType { get; set; }

        public string Source { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? SourceId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? RuleId { get; set; }

        public string Description { get; set; }
    }

    /// <summary>
    /// Redeem Points Request DTO
    /// </summary>
    public class RedeemPointsRequest
    {
        [Required(ErrorMessage = "Member ID is required")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long MemberId { get; set; }

        [Required(ErrorMessage = "Points is required")]
        [Range(1, 1000000, ErrorMessage = "Points must be between 1 and 1000000")]
        public int Points { get; set; }

        [Required(ErrorMessage = "Transaction type is required")]
        public string TransactionType { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? SourceId { get; set; }

        public string Description { get; set; }
    }

    /// <summary>
    /// Calculate Points Request DTO
    /// </summary>
    public class CalculatePointsRequest
    {
        [Required(ErrorMessage = "Member ID is required")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long MemberId { get; set; }

        [Required(ErrorMessage = "Amount is required")]
        public decimal Amount { get; set; }

        public string ItemType { get; set; }
    }

    /// <summary>
    /// Calculate Points Result DTO
    /// </summary>
    public class CalculatePointsResult
    {
        public int BasePoints { get; set; }
        public decimal Multiplier { get; set; }
        public int FinalPoints { get; set; }
        public string TierName { get; set; }
    }

    /// <summary>
    /// Loyalty Transaction Query DTO
    /// </summary>
    public class LoyaltyTransactionQueryDto : PagerInfo
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? MemberId { get; set; }

        public string TransactionType { get; set; }
        public string Source { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    /// <summary>
    /// Loyalty Analytics DTO
    /// </summary>
    public class LoyaltyAnalyticsDto
    {
        public int TotalMembers { get; set; }
        public int ActiveMembers { get; set; }
        public long TotalPointsIssued { get; set; }
        public long TotalPointsRedeemed { get; set; }
        public long TotalPointsOutstanding { get; set; }
        public decimal PointsValue { get; set; }
        public Dictionary<string, int> MembersByTier { get; set; }
        public List<TierDistributionDto> TierDistribution { get; set; }
        public List<MonthlyPointsDto> MonthlyTrends { get; set; }
    }

    /// <summary>
    /// Tier Distribution DTO
    /// </summary>
    public class TierDistributionDto
    {
        public string TierName { get; set; }
        public string TierNameAr { get; set; }
        public int MemberCount { get; set; }
        public decimal Percentage { get; set; }
        public string BadgeColor { get; set; }
    }

    /// <summary>
    /// Monthly Points DTO
    /// </summary>
    public class MonthlyPointsDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName { get; set; }
        public long PointsEarned { get; set; }
        public long PointsRedeemed { get; set; }
    }

    /// <summary>
    /// Leaderboard Entry DTO
    /// </summary>
    public class LeaderboardEntryDto
    {
        public int Rank { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long MemberId { get; set; }

        public string MemberName { get; set; }
        public int TotalPoints { get; set; }
        public string TierName { get; set; }
        public string BadgeIcon { get; set; }
        public string BadgeColor { get; set; }
    }

    #endregion

    #region Loyalty Rule DTOs

    /// <summary>
    /// Loyalty Rule Query DTO
    /// </summary>
    public class LoyaltyRuleQueryDto : PagerInfo
    {
        public string RuleCode { get; set; }
        public string RuleType { get; set; }
        public bool? IsActive { get; set; }
    }

    /// <summary>
    /// Loyalty Rule Input DTO
    /// </summary>
    public class LoyaltyRuleDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long RuleId { get; set; }

        [Required(ErrorMessage = "Rule code is required")]
        public string RuleCode { get; set; }

        [Required(ErrorMessage = "Rule name is required")]
        public string RuleName { get; set; }

        public string RuleNameAr { get; set; }

        [Required(ErrorMessage = "Rule type is required")]
        public string RuleType { get; set; }

        public decimal PointsPerSar { get; set; }
        public int FixedPoints { get; set; }
        public string ApplicableTo { get; set; }
        public int? MaxPointsPerTransaction { get; set; }
        public int? DailyLimit { get; set; }
        public bool IsActive { get; set; } = true;
        public string Remark { get; set; }
    }

    #endregion
}

