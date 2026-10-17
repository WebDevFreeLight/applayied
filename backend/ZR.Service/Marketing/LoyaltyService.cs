using Infrastructure;
using Infrastructure.Attribute;
using Infrastructure.Extensions;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ZR.Model;
using ZR.Model.Marketing.Another;
using ZR.Model.Marketing.Dto;
using ZR.Repository;
using ZR.Service.Marketing.IMarketingService;
using ZR.ServiceCore;

namespace ZR.Service.Marketing
{
    /// <summary>
    /// Loyalty Service Implementation
    /// </summary>
    [AppService(ServiceType = typeof(ILoyaltyService), ServiceLifetime = LifeTime.Transient)]
    public class LoyaltyService : BaseService<ErpLoyaltyPoints>, ILoyaltyService
    {
        #region Tier Management

        /// <summary>
        /// Get all loyalty tiers
        /// </summary>
        public async Task<List<ErpLoyaltyTier>> GetAllTiersAsync()
        {
            return await Context.Queryable<ErpLoyaltyTier>()
                .Where(t => t.IsActive)
                .OrderBy(t => t.SortOrder)
                .ToListAsync();
        }

        /// <summary>
        /// Get tier by ID
        /// </summary>
        public async Task<ErpLoyaltyTier> GetTierByIdAsync(long tierId)
        {
            return await Context.Queryable<ErpLoyaltyTier>()
                .FirstAsync(t => t.TierId == tierId);
        }

        /// <summary>
        /// Get member's current tier
        /// </summary>
        public async Task<ErpLoyaltyTier> GetMemberTierAsync(long memberId)
        {
            var memberPoints = await GetOrCreateMemberPointsAsync(memberId);
            return await GetTierByIdAsync(memberPoints.CurrentTierId);
        }

        /// <summary>
        /// Add new tier
        /// </summary>
        public async Task<int> AddTierAsync(ErpLoyaltyTier tier)
        {
            try
            {
                if (string.IsNullOrEmpty(tier.TierCode))
                {
                    throw new CustomException("Tier code is required");
                }

                var exists = await Context.Queryable<ErpLoyaltyTier>()
                    .AnyAsync(t => t.TierCode == tier.TierCode);
                if (exists)
                {
                    throw new CustomException($"Tier code {tier.TierCode} already exists");
                }

                tier.Create_time = DateTime.Now;
                var result = await Context.Insertable(tier).ExecuteReturnSnowflakeIdAsync();
                return result > 0 ? 1 : 0;
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException($"Error adding tier: {ex.Message}");
            }
        }

        /// <summary>
        /// Update tier
        /// </summary>
        public async Task<int> UpdateTierAsync(ErpLoyaltyTier tier)
        {
            try
            {
                tier.Update_time = DateTime.Now;
                return await Context.Updateable(tier)
                    .IgnoreColumns(t => new { t.Create_by, t.Create_time })
                    .ExecuteCommandAsync();
            }
            catch (Exception ex)
            {
                throw new CustomException($"Error updating tier: {ex.Message}");
            }
        }

        /// <summary>
        /// Delete tiers
        /// </summary>
        public async Task<int> DeleteTiersAsync(long[] tierIds)
        {
            try
            {
                // Check if any members are using these tiers
                foreach (var tierId in tierIds)
                {
                    var hasMembers = await Context.Queryable<ErpLoyaltyPoints>()
                        .AnyAsync(p => p.CurrentTierId == tierId);
                    if (hasMembers)
                    {
                        throw new CustomException($"Cannot delete tier {tierId}: has members assigned");
                    }
                }

                return await Context.Deleteable<ErpLoyaltyTier>()
                    .In(tierIds)
                    .ExecuteCommandAsync();
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException($"Error deleting tiers: {ex.Message}");
            }
        }

        #endregion

        #region Points Management

        /// <summary>
        /// Get member points summary
        /// </summary>
        public async Task<MemberPointsSummaryDto> GetMemberPointsAsync(long memberId)
        {
            var memberPoints = await GetOrCreateMemberPointsAsync(memberId);
            var tier = await GetTierByIdAsync(memberPoints.CurrentTierId);
            var tiers = await GetAllTiersAsync();

            var nextTier = tiers
                .Where(t => t.MinPoints > tier.MinPoints)
                .OrderBy(t => t.MinPoints)
                .FirstOrDefault();

            var progressPercent = 0;
            if (nextTier != null)
            {
                var range = nextTier.MinPoints - tier.MinPoints;
                var progress = memberPoints.TotalEarned - tier.MinPoints;
                progressPercent = range > 0 ? Math.Min((int)((progress * 100) / range), 100) : 100;
            }
            else
            {
                progressPercent = 100;
            }

            return new MemberPointsSummaryDto
            {
                MemberId = memberId,
                TotalEarned = memberPoints.TotalEarned,
                TotalRedeemed = memberPoints.TotalRedeemed,
                CurrentBalance = memberPoints.CurrentBalance,
                PointsToNextTier = memberPoints.PointsToNextTier,
                LastActivityDate = memberPoints.LastActivityDate,
                CurrentTierId = memberPoints.CurrentTierId,
                TierCode = tier?.TierCode,
                TierName = tier?.TierName,
                TierNameAr = tier?.TierNameAr,
                PointsMultiplier = tier?.PointsMultiplier ?? 1.0m,
                TierDiscountPercent = tier?.DiscountPercent ?? 0,
                BadgeIcon = tier?.BadgeIcon,
                BadgeColor = tier?.BadgeColor,
                ProgressPercent = progressPercent,
                NextTierName = nextTier?.TierName
            };
        }

        /// <summary>
        /// Get or create member points record
        /// </summary>
        public async Task<ErpLoyaltyPoints> GetOrCreateMemberPointsAsync(long memberId)
        {
            var memberPoints = await Context.Queryable<ErpLoyaltyPoints>()
                .FirstAsync(p => p.MemberId == memberId);

            if (memberPoints == null)
            {
                // Get default tier (lowest tier)
                var defaultTier = await Context.Queryable<ErpLoyaltyTier>()
                    .Where(t => t.IsActive)
                    .OrderBy(t => t.MinPoints)
                    .FirstAsync();

                if (defaultTier == null)
                {
                    throw new CustomException("No loyalty tiers configured");
                }

                memberPoints = new ErpLoyaltyPoints
                {
                    MemberId = memberId,
                    TotalEarned = 0,
                    TotalRedeemed = 0,
                    CurrentBalance = 0,
                    CurrentTierId = defaultTier.TierId,
                    PointsToNextTier = defaultTier.MaxPoints - 0,
                    Create_time = DateTime.Now
                };

                await Context.Insertable(memberPoints).ExecuteReturnSnowflakeIdAsync();
                memberPoints = await Context.Queryable<ErpLoyaltyPoints>()
                    .FirstAsync(p => p.MemberId == memberId);
            }

            return memberPoints;
        }

        /// <summary>
        /// Earn points
        /// </summary>
        public async Task<int> EarnPointsAsync(EarnPointsRequest request)
        {
            try
            {
                var memberPoints = await GetOrCreateMemberPointsAsync(request.MemberId);
                var tier = await GetTierByIdAsync(memberPoints.CurrentTierId);

                // Apply tier multiplier
                var pointsToEarn = (int)(request.Points * (tier?.PointsMultiplier ?? 1.0m));

                // Apply daily limit if rule specified
                if (request.RuleId.HasValue)
                {
                    var rule = await Context.Queryable<ErpLoyaltyRule>()
                        .FirstAsync(r => r.RuleId == request.RuleId.Value);

                    if (rule != null && rule.DailyLimit.HasValue)
                    {
                        var todayEarned = await GetTodayEarnedPointsAsync(request.MemberId, rule.RuleId);
                        var remaining = rule.DailyLimit.Value - todayEarned;
                        pointsToEarn = Math.Min(pointsToEarn, Math.Max(0, remaining));
                    }
                }

                if (pointsToEarn <= 0) return 0;

                var balanceBefore = memberPoints.CurrentBalance;
                memberPoints.TotalEarned += pointsToEarn;
                memberPoints.CurrentBalance += pointsToEarn;
                memberPoints.LastActivityDate = DateTime.Now;
                memberPoints.Update_time = DateTime.Now;

                await Context.Updateable(memberPoints)
                    .ExecuteCommandAsync();

                // Record transaction
                var transaction = new ErpLoyaltyTransaction
                {
                    MemberId = request.MemberId,
                    TransactionType = request.TransactionType,
                    Points = pointsToEarn,
                    BalanceBefore = balanceBefore,
                    BalanceAfter = memberPoints.CurrentBalance,
                    Source = request.Source,
                    SourceId = request.SourceId,
                    Description = request.Description,
                    ExpiryDate = DateTime.Now.AddYears(1),
                    Create_time = DateTime.Now
                };

                await Context.Insertable(transaction).ExecuteReturnSnowflakeIdAsync();

                // Check for tier upgrade
                await UpdateMemberTierAsync(request.MemberId);

                return pointsToEarn;
            }
            catch (Exception ex)
            {
                throw new CustomException($"Error earning points: {ex.Message}");
            }
        }

        /// <summary>
        /// Redeem points
        /// </summary>
        public async Task<int> RedeemPointsAsync(RedeemPointsRequest request)
        {
            try
            {
                var memberPoints = await GetOrCreateMemberPointsAsync(request.MemberId);

                if (memberPoints.CurrentBalance < request.Points)
                {
                    throw new CustomException($"Insufficient points. Available: {memberPoints.CurrentBalance}");
                }

                var balanceBefore = memberPoints.CurrentBalance;
                memberPoints.TotalRedeemed += request.Points;
                memberPoints.CurrentBalance -= request.Points;
                memberPoints.LastActivityDate = DateTime.Now;
                memberPoints.Update_time = DateTime.Now;

                await Context.Updateable(memberPoints)
                    .ExecuteCommandAsync();

                // Record transaction
                var transaction = new ErpLoyaltyTransaction
                {
                    MemberId = request.MemberId,
                    TransactionType = request.TransactionType,
                    Points = -request.Points,
                    BalanceBefore = balanceBefore,
                    BalanceAfter = memberPoints.CurrentBalance,
                    Source = "redemption",
                    SourceId = request.SourceId,
                    Description = request.Description,
                    Create_time = DateTime.Now
                };

                await Context.Insertable(transaction).ExecuteReturnSnowflakeIdAsync();

                return request.Points;
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException($"Error redeeming points: {ex.Message}");
            }
        }

        /// <summary>
        /// Calculate points for purchase
        /// </summary>
        public async Task<CalculatePointsResult> CalculatePointsForPurchaseAsync(long memberId, decimal amount)
        {
            var rule = await GetActiveRuleAsync("purchase_points");
            if (rule == null)
            {
                return new CalculatePointsResult { BasePoints = 0, Multiplier = 1, FinalPoints = 0 };
            }

            var memberPoints = await GetOrCreateMemberPointsAsync(memberId);
            var tier = await GetTierByIdAsync(memberPoints.CurrentTierId);

            var basePoints = (int)(amount * rule.PointsPerSar);

            if (rule.MaxPointsPerTransaction.HasValue)
            {
                basePoints = Math.Min(basePoints, rule.MaxPointsPerTransaction.Value);
            }

            var multiplier = tier?.PointsMultiplier ?? 1.0m;
            var finalPoints = (int)(basePoints * multiplier);

            return new CalculatePointsResult
            {
                BasePoints = basePoints,
                Multiplier = multiplier,
                FinalPoints = finalPoints,
                TierName = tier?.TierName
            };
        }

        /// <summary>
        /// Update member tier based on points
        /// </summary>
        public async Task UpdateMemberTierAsync(long memberId)
        {
            var memberPoints = await GetOrCreateMemberPointsAsync(memberId);
            var tiers = await GetAllTiersAsync();

            var newTier = tiers
                .Where(t => memberPoints.TotalEarned >= t.MinPoints)
                .OrderByDescending(t => t.MinPoints)
                .FirstOrDefault();

            if (newTier != null && newTier.TierId != memberPoints.CurrentTierId)
            {
                memberPoints.CurrentTierId = newTier.TierId;
                memberPoints.TierUpdatedAt = DateTime.Now;

                // Calculate points to next tier
                var nextTier = tiers
                    .Where(t => t.MinPoints > newTier.MinPoints)
                    .OrderBy(t => t.MinPoints)
                    .FirstOrDefault();

                memberPoints.PointsToNextTier = nextTier != null
                    ? nextTier.MinPoints - memberPoints.TotalEarned
                    : 0;

                await Context.Updateable(memberPoints)
                    .ExecuteCommandAsync();
            }
        }

        /// <summary>
        /// Get points balance
        /// </summary>
        public async Task<int> GetPointsBalanceAsync(long memberId)
        {
            var memberPoints = await GetOrCreateMemberPointsAsync(memberId);
            return memberPoints.CurrentBalance;
        }

        private async Task<int> GetTodayEarnedPointsAsync(long memberId, long ruleId)
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            var todayPoints = await Context.Queryable<ErpLoyaltyTransaction>()
                .Where(t => t.MemberId == memberId &&
                            t.Points > 0 &&
                            t.Create_time >= today &&
                            t.Create_time < tomorrow)
                .SumAsync(t => t.Points);

            return todayPoints;
        }

        #endregion

        #region Transaction History

        /// <summary>
        /// Get transaction history
        /// </summary>
        public async Task<PagedInfo<ErpLoyaltyTransaction>> GetTransactionHistoryAsync(long memberId, LoyaltyTransactionQueryDto pager)
        {
            var query = Context.Queryable<ErpLoyaltyTransaction>()
                .Where(t => t.MemberId == memberId)
                .WhereIF(!string.IsNullOrEmpty(pager.TransactionType), t => t.TransactionType == pager.TransactionType)
                .WhereIF(!string.IsNullOrEmpty(pager.Source), t => t.Source == pager.Source)
                .WhereIF(pager.StartDate.HasValue, t => t.Create_time >= pager.StartDate)
                .WhereIF(pager.EndDate.HasValue, t => t.Create_time <= pager.EndDate)
                .OrderByDescending(t => t.Create_time);

            return await query.ToPageAsync(pager);
        }

        /// <summary>
        /// Get recent transactions
        /// </summary>
        public async Task<List<ErpLoyaltyTransaction>> GetRecentTransactionsAsync(long memberId, int limit = 10)
        {
            return await Context.Queryable<ErpLoyaltyTransaction>()
                .Where(t => t.MemberId == memberId)
                .OrderByDescending(t => t.Create_time)
                .Take(limit)
                .ToListAsync();
        }

        #endregion

        #region Rules Management

        /// <summary>
        /// Get all loyalty rules
        /// </summary>
        public async Task<List<ErpLoyaltyRule>> GetAllRulesAsync()
        {
            return await Context.Queryable<ErpLoyaltyRule>()
                .OrderBy(r => r.RuleCode)
                .ToListAsync();
        }

        /// <summary>
        /// Get active rule by type
        /// </summary>
        public async Task<ErpLoyaltyRule> GetActiveRuleAsync(string ruleType)
        {
            return await Context.Queryable<ErpLoyaltyRule>()
                .FirstAsync(r => r.RuleType == ruleType && r.IsActive);
        }

        /// <summary>
        /// Add new rule
        /// </summary>
        public async Task<int> AddRuleAsync(ErpLoyaltyRule rule)
        {
            rule.Create_time = DateTime.Now;
            var result = await Context.Insertable(rule).ExecuteReturnSnowflakeIdAsync();
            return result > 0 ? 1 : 0;
        }

        /// <summary>
        /// Update rule
        /// </summary>
        public async Task<int> UpdateRuleAsync(ErpLoyaltyRule rule)
        {
            rule.Update_time = DateTime.Now;
            return await Context.Updateable(rule)
                .IgnoreColumns(r => new { r.Create_by, r.Create_time })
                .ExecuteCommandAsync();
        }

        /// <summary>
        /// Delete rules
        /// </summary>
        public async Task<int> DeleteRulesAsync(long[] ruleIds)
        {
            return await Context.Deleteable<ErpLoyaltyRule>()
                .In(ruleIds)
                .ExecuteCommandAsync();
        }

        #endregion

        #region Analytics & Reporting

        /// <summary>
        /// Get loyalty analytics
        /// </summary>
        public async Task<LoyaltyAnalyticsDto> GetAnalyticsAsync(DateTime from, DateTime to)
        {
            var allPoints = await Context.Queryable<ErpLoyaltyPoints>().ToListAsync();
            var transactions = await Context.Queryable<ErpLoyaltyTransaction>()
                .Where(t => t.Create_time >= from && t.Create_time <= to)
                .ToListAsync();

            var tierDistribution = await GetTierDistributionAsync();

            return new LoyaltyAnalyticsDto
            {
                TotalMembers = allPoints.Count,
                ActiveMembers = allPoints.Count(p => p.LastActivityDate >= from),
                TotalPointsIssued = transactions.Where(t => t.Points > 0).Sum(t => t.Points),
                TotalPointsRedeemed = Math.Abs(transactions.Where(t => t.Points < 0).Sum(t => t.Points)),
                TotalPointsOutstanding = allPoints.Sum(p => p.CurrentBalance),
                TierDistribution = tierDistribution,
                MembersByTier = tierDistribution.ToDictionary(t => t.TierName, t => t.MemberCount)
            };
        }

        /// <summary>
        /// Get leaderboard
        /// </summary>
        public async Task<List<LeaderboardEntryDto>> GetLeaderboardAsync(int limit = 10)
        {
            var topMembers = await Context.Queryable<ErpLoyaltyPoints>()
                .OrderByDescending(p => p.TotalEarned)
                .Take(limit)
                .ToListAsync();

            var tiers = await GetAllTiersAsync();
            var result = new List<LeaderboardEntryDto>();

            int rank = 1;
            foreach (var member in topMembers)
            {
                var tier = tiers.FirstOrDefault(t => t.TierId == member.CurrentTierId);
                result.Add(new LeaderboardEntryDto
                {
                    Rank = rank++,
                    MemberId = member.MemberId,
                    TotalPoints = member.TotalEarned,
                    TierName = tier?.TierName,
                    BadgeIcon = tier?.BadgeIcon,
                    BadgeColor = tier?.BadgeColor
                });
            }

            return result;
        }

        /// <summary>
        /// Get tier distribution
        /// </summary>
        public async Task<List<TierDistributionDto>> GetTierDistributionAsync()
        {
            var tiers = await GetAllTiersAsync();
            var allPoints = await Context.Queryable<ErpLoyaltyPoints>().ToListAsync();
            var totalMembers = allPoints.Count;

            var distribution = new List<TierDistributionDto>();

            foreach (var tier in tiers)
            {
                var memberCount = allPoints.Count(p => p.CurrentTierId == tier.TierId);
                distribution.Add(new TierDistributionDto
                {
                    TierName = tier.TierName,
                    TierNameAr = tier.TierNameAr,
                    MemberCount = memberCount,
                    Percentage = totalMembers > 0 ? Math.Round((decimal)memberCount / totalMembers * 100, 2) : 0,
                    BadgeColor = tier.BadgeColor
                });
            }

            return distribution;
        }

        #endregion

        #region Scheduled Jobs

        /// <summary>
        /// Process birthday bonuses
        /// </summary>
        public async Task ProcessBirthdayBonusAsync()
        {
            // This would integrate with member service to get members with birthdays today
            // For now, this is a placeholder
            await Task.CompletedTask;
        }

        /// <summary>
        /// Process points expiry
        /// </summary>
        public async Task ProcessPointsExpiryAsync()
        {
            var now = DateTime.Now;

            // Get expired transactions
            var expiredTransactions = await Context.Queryable<ErpLoyaltyTransaction>()
                .Where(t => t.ExpiryDate <= now && t.Points > 0)
                .ToListAsync();

            foreach (var transaction in expiredTransactions)
            {
                var memberPoints = await GetOrCreateMemberPointsAsync(transaction.MemberId);

                // Calculate remaining points from this transaction
                var pointsToExpire = Math.Min(transaction.Points, memberPoints.CurrentBalance);

                if (pointsToExpire > 0)
                {
                    // Create expiry transaction
                    var expiryTransaction = new ErpLoyaltyTransaction
                    {
                        MemberId = transaction.MemberId,
                        TransactionType = "expire",
                        Points = -pointsToExpire,
                        BalanceBefore = memberPoints.CurrentBalance,
                        BalanceAfter = memberPoints.CurrentBalance - pointsToExpire,
                        Source = "system",
                        Description = $"Points expired from transaction {transaction.TransactionId}",
                        Create_time = DateTime.Now
                    };

                    await Context.Insertable(expiryTransaction).ExecuteReturnSnowflakeIdAsync();

                    // Update member balance
                    memberPoints.CurrentBalance -= pointsToExpire;
                    memberPoints.Update_time = DateTime.Now;

                    await Context.Updateable(memberPoints)
                        .ExecuteCommandAsync();
                }
            }
        }

        #endregion
    }
}

