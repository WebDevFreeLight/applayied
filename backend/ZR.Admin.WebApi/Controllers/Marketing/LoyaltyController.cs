using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Infrastructure;
using Infrastructure.Attribute;
using Infrastructure.Enums;
using Infrastructure.Model;
using Mapster;
using SqlSugar;
using ZR.Admin.WebApi.Extensions;
using ZR.Admin.WebApi.Filters;
using ZR.Common;
using ZR.Model.Marketing.Dto;
using ZR.Service.Marketing.IMarketingService;
using ZR.Model.Marketing.Another;

namespace ZR.Admin.WebApi.Controllers.Marketing
{
    /// <summary>
    /// Loyalty Program Management Controller - ادارة برنامج الولاء
    /// </summary>
    [Verify]
    [Route("api/marketing/loyalty")]
    [ApiExplorerSettings(GroupName = "marketing")]
    public class LoyaltyController : BaseController
    {
        private readonly ILoyaltyService _loyaltyService;

        public LoyaltyController(ILoyaltyService loyaltyService)
        {
            _loyaltyService = loyaltyService;
        }

        #region Points Management

        /// <summary>
        /// Get member points summary
        /// </summary>
        [HttpGet("points/{memberId}")]
        [ActionPermissionFilter("marketing:loyalty:query")]
        public async Task<IActionResult> GetMemberPoints(long memberId)
        {
            var points = await _loyaltyService.GetMemberPointsAsync(memberId);
            return SUCCESS(points);
        }

        /// <summary>
        /// Get points balance
        /// </summary>
        [HttpGet("balance/{memberId}")]
        [ActionPermissionFilter("marketing:loyalty:query")]
        public async Task<IActionResult> GetPointsBalance(long memberId)
        {
            var balance = await _loyaltyService.GetPointsBalanceAsync(memberId);
            return SUCCESS(new { MemberId = memberId, Balance = balance });
        }

        /// <summary>
        /// Earn points
        /// </summary>
        [HttpPost("earn")]
        [ActionPermissionFilter("marketing:loyalty:earn")]
        [Log(Title = "Loyalty Points Earn", BusinessType = BusinessType.INSERT)]
        public async Task<IActionResult> EarnPoints([FromBody] EarnPointsRequest request)
        {
            var pointsEarned = await _loyaltyService.EarnPointsAsync(request);
            return SUCCESS(new { PointsEarned = pointsEarned, Message = $"Earned {pointsEarned} points" });
        }

        /// <summary>
        /// Redeem points
        /// </summary>
        [HttpPost("redeem")]
        [ActionPermissionFilter("marketing:loyalty:redeem")]
        [Log(Title = "Loyalty Points Redeem", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> RedeemPoints([FromBody] RedeemPointsRequest request)
        {
            var pointsRedeemed = await _loyaltyService.RedeemPointsAsync(request);
            return SUCCESS(new { PointsRedeemed = pointsRedeemed, Message = $"Redeemed {pointsRedeemed} points" });
        }

        /// <summary>
        /// Calculate points for purchase
        /// </summary>
        [HttpPost("calculate")]
        [ActionPermissionFilter("marketing:loyalty:query")]
        public async Task<IActionResult> CalculatePoints([FromBody] CalculatePointsRequest request)
        {
            var result = await _loyaltyService.CalculatePointsForPurchaseAsync(request.MemberId, request.Amount);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get transaction history
        /// </summary>
        [HttpGet("history/{memberId}")]
        [ActionPermissionFilter("marketing:loyalty:query")]
        public async Task<IActionResult> GetTransactionHistory(long memberId, [FromQuery] LoyaltyTransactionQueryDto dto)
        {
            dto.MemberId = memberId;
            var history = await _loyaltyService.GetTransactionHistoryAsync(memberId, dto);
            return SUCCESS(history);
        }

        /// <summary>
        /// Get recent transactions
        /// </summary>
        [HttpGet("recent/{memberId}")]
        [ActionPermissionFilter("marketing:loyalty:query")]
        public async Task<IActionResult> GetRecentTransactions(long memberId, [FromQuery] int limit = 10)
        {
            var transactions = await _loyaltyService.GetRecentTransactionsAsync(memberId, limit);
            return SUCCESS(transactions);
        }

        #endregion

        #region Tier Management

        /// <summary>
        /// Get all tiers
        /// </summary>
        [HttpGet("tiers")]
        [ActionPermissionFilter("marketing:loyalty:tiers")]
        public async Task<IActionResult> GetAllTiers()
        {
            var tiers = await _loyaltyService.GetAllTiersAsync();
            return SUCCESS(tiers);
        }

        /// <summary>
        /// Get tier by ID
        /// </summary>
        [HttpGet("tiers/{id}")]
        [ActionPermissionFilter("marketing:loyalty:tiers")]
        public async Task<IActionResult> GetTier(long id)
        {
            var tier = await _loyaltyService.GetTierByIdAsync(id);
            if (tier == null)
            {
                return ToResponse(ResultCode.FAIL, "Tier not found");
            }
            return SUCCESS(tier);
        }

        /// <summary>
        /// Get member tier
        /// </summary>
        [HttpGet("tier/{memberId}")]
        [ActionPermissionFilter("marketing:loyalty:query")]
        public async Task<IActionResult> GetMemberTier(long memberId)
        {
            var tier = await _loyaltyService.GetMemberTierAsync(memberId);
            return SUCCESS(tier);
        }

        /// <summary>
        /// Add new tier
        /// </summary>
        [HttpPost("tiers")]
        [ActionPermissionFilter("marketing:loyalty:tiers:add")]
        [Log(Title = "Loyalty Tier Management", BusinessType = BusinessType.INSERT)]
        public async Task<IActionResult> AddTier([FromBody] LoyaltyTierDto dto)
        {
            var tier = dto.Adapt<ErpLoyaltyTier>();
            tier.Create_by = HttpContext.GetName();
            tier.Create_time = DateTime.Now;

            var result = await _loyaltyService.AddTierAsync(tier);
            return SUCCESS(result);
        }

        /// <summary>
        /// Update tier
        /// </summary>
        [HttpPut("tiers")]
        [ActionPermissionFilter("marketing:loyalty:tiers:edit")]
        [Log(Title = "Loyalty Tier Management", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> UpdateTier([FromBody] LoyaltyTierDto dto)
        {
            var tier = dto.Adapt<ErpLoyaltyTier>();
            tier.Update_by = HttpContext.GetName();
            tier.Update_time = DateTime.Now;

            var result = await _loyaltyService.UpdateTierAsync(tier);
            return SUCCESS(result);
        }

        /// <summary>
        /// Delete tiers
        /// </summary>
        [HttpDelete("tiers/{ids}")]
        [ActionPermissionFilter("marketing:loyalty:tiers:delete")]
        [Log(Title = "Loyalty Tier Management", BusinessType = BusinessType.DELETE)]
        public async Task<IActionResult> DeleteTiers(string ids)
        {
            var idArray = Tools.SplitAndConvert<long>(ids);
            var result = await _loyaltyService.DeleteTiersAsync(idArray);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get tier distribution
        /// </summary>
        [HttpGet("tier-distribution")]
        [ActionPermissionFilter("marketing:loyalty:analytics")]
        public async Task<IActionResult> GetTierDistribution()
        {
            var distribution = await _loyaltyService.GetTierDistributionAsync();
            return SUCCESS(distribution);
        }

        #endregion

        #region Rules Management

        /// <summary>
        /// Get all rules
        /// </summary>
        [HttpGet("rules")]
        [ActionPermissionFilter("marketing:loyalty:rules")]
        public async Task<IActionResult> GetAllRules()
        {
            var rules = await _loyaltyService.GetAllRulesAsync();
            return SUCCESS(rules);
        }

        /// <summary>
        /// Get active rule by type
        /// </summary>
        [HttpGet("rules/active/{ruleType}")]
        [ActionPermissionFilter("marketing:loyalty:rules")]
        public async Task<IActionResult> GetActiveRule(string ruleType)
        {
            var rule = await _loyaltyService.GetActiveRuleAsync(ruleType);
            return SUCCESS(rule);
        }

        /// <summary>
        /// Add new rule
        /// </summary>
        [HttpPost("rules")]
        [ActionPermissionFilter("marketing:loyalty:rules:add")]
        [Log(Title = "Loyalty Rule Management", BusinessType = BusinessType.INSERT)]
        public async Task<IActionResult> AddRule([FromBody] LoyaltyRuleDto dto)
        {
            var rule = dto.Adapt<ErpLoyaltyRule>();
            rule.Create_by = HttpContext.GetName();
            rule.Create_time = DateTime.Now;

            var result = await _loyaltyService.AddRuleAsync(rule);
            return SUCCESS(result);
        }

        /// <summary>
        /// Update rule
        /// </summary>
        [HttpPut("rules")]
        [ActionPermissionFilter("marketing:loyalty:rules:edit")]
        [Log(Title = "Loyalty Rule Management", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> UpdateRule([FromBody] LoyaltyRuleDto dto)
        {
            var rule = dto.Adapt<ErpLoyaltyRule>();
            rule.Update_by = HttpContext.GetName();
            rule.Update_time = DateTime.Now;

            var result = await _loyaltyService.UpdateRuleAsync(rule);
            return SUCCESS(result);
        }

        /// <summary>
        /// Delete rules
        /// </summary>
        [HttpDelete("rules/{ids}")]
        [ActionPermissionFilter("marketing:loyalty:rules:delete")]
        [Log(Title = "Loyalty Rule Management", BusinessType = BusinessType.DELETE)]
        public async Task<IActionResult> DeleteRules(string ids)
        {
            var idArray = Tools.SplitAndConvert<long>(ids);
            var result = await _loyaltyService.DeleteRulesAsync(idArray);
            return SUCCESS(result);
        }

        #endregion

        #region Analytics & Leaderboard

        /// <summary>
        /// Get loyalty analytics
        /// </summary>
        [HttpGet("analytics")]
        [ActionPermissionFilter("marketing:loyalty:analytics")]
        public async Task<IActionResult> GetAnalytics([FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            var fromDate = from ?? DateTime.Now.AddMonths(-1);
            var toDate = to ?? DateTime.Now;

            var analytics = await _loyaltyService.GetAnalyticsAsync(fromDate, toDate);
            return SUCCESS(analytics);
        }

        /// <summary>
        /// Get leaderboard
        /// </summary>
        [HttpGet("leaderboard")]
        [ActionPermissionFilter("marketing:loyalty:query")]
        public async Task<IActionResult> GetLeaderboard([FromQuery] int limit = 10)
        {
            var leaderboard = await _loyaltyService.GetLeaderboardAsync(limit);
            return SUCCESS(leaderboard);
        }

        #endregion
    }
}

