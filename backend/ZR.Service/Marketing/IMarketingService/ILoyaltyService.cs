using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ZR.Model;
using ZR.Model.Marketing.Another;
using ZR.Model.Marketing.Dto;

namespace ZR.Service.Marketing.IMarketingService
{
    /// <summary>
    /// Loyalty Service Interface
    /// </summary>
    public interface ILoyaltyService
    {
        #region Tier Management

        /// <summary>
        /// Get all loyalty tiers
        /// </summary>
        Task<List<ErpLoyaltyTier>> GetAllTiersAsync();

        /// <summary>
        /// Get tier by ID
        /// </summary>
        Task<ErpLoyaltyTier> GetTierByIdAsync(long tierId);

        /// <summary>
        /// Get member's current tier
        /// </summary>
        Task<ErpLoyaltyTier> GetMemberTierAsync(long memberId);

        /// <summary>
        /// Add new tier
        /// </summary>
        Task<int> AddTierAsync(ErpLoyaltyTier tier);

        /// <summary>
        /// Update tier
        /// </summary>
        Task<int> UpdateTierAsync(ErpLoyaltyTier tier);

        /// <summary>
        /// Delete tiers
        /// </summary>
        Task<int> DeleteTiersAsync(long[] tierIds);

        #endregion

        #region Points Management

        /// <summary>
        /// Get member points summary
        /// </summary>
        Task<MemberPointsSummaryDto> GetMemberPointsAsync(long memberId);

        /// <summary>
        /// Get or create member points record
        /// </summary>
        Task<ErpLoyaltyPoints> GetOrCreateMemberPointsAsync(long memberId);

        /// <summary>
        /// Earn points
        /// </summary>
        Task<int> EarnPointsAsync(EarnPointsRequest request);

        /// <summary>
        /// Redeem points
        /// </summary>
        Task<int> RedeemPointsAsync(RedeemPointsRequest request);

        /// <summary>
        /// Calculate points for purchase
        /// </summary>
        Task<CalculatePointsResult> CalculatePointsForPurchaseAsync(long memberId, decimal amount);

        /// <summary>
        /// Update member tier based on points
        /// </summary>
        Task UpdateMemberTierAsync(long memberId);

        /// <summary>
        /// Get points balance
        /// </summary>
        Task<int> GetPointsBalanceAsync(long memberId);

        #endregion

        #region Transaction History

        /// <summary>
        /// Get transaction history
        /// </summary>
        Task<PagedInfo<ErpLoyaltyTransaction>> GetTransactionHistoryAsync(long memberId, LoyaltyTransactionQueryDto pager);

        /// <summary>
        /// Get recent transactions
        /// </summary>
        Task<List<ErpLoyaltyTransaction>> GetRecentTransactionsAsync(long memberId, int limit = 10);

        #endregion

        #region Rules Management

        /// <summary>
        /// Get all loyalty rules
        /// </summary>
        Task<List<ErpLoyaltyRule>> GetAllRulesAsync();

        /// <summary>
        /// Get active rule by type
        /// </summary>
        Task<ErpLoyaltyRule> GetActiveRuleAsync(string ruleType);

        /// <summary>
        /// Add new rule
        /// </summary>
        Task<int> AddRuleAsync(ErpLoyaltyRule rule);

        /// <summary>
        /// Update rule
        /// </summary>
        Task<int> UpdateRuleAsync(ErpLoyaltyRule rule);

        /// <summary>
        /// Delete rules
        /// </summary>
        Task<int> DeleteRulesAsync(long[] ruleIds);

        #endregion

        #region Analytics & Reporting

        /// <summary>
        /// Get loyalty analytics
        /// </summary>
        Task<LoyaltyAnalyticsDto> GetAnalyticsAsync(DateTime from, DateTime to);

        /// <summary>
        /// Get leaderboard
        /// </summary>
        Task<List<LeaderboardEntryDto>> GetLeaderboardAsync(int limit = 10);

        /// <summary>
        /// Get tier distribution
        /// </summary>
        Task<List<TierDistributionDto>> GetTierDistributionAsync();

        #endregion

        #region Scheduled Jobs

        /// <summary>
        /// Process birthday bonuses
        /// </summary>
        Task ProcessBirthdayBonusAsync();

        /// <summary>
        /// Process points expiry
        /// </summary>
        Task ProcessPointsExpiryAsync();

        #endregion
    }
}

