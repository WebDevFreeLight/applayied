using Infrastructure;
using Infrastructure.Attribute;
using Infrastructure.Extensions;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;
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
    /// Promotion Service Implementation
    /// </summary>
    [AppService(ServiceType = typeof(IPromotionService), ServiceLifetime = LifeTime.Transient)]
    public class PromotionService : BaseService<ErpPromotion>, IPromotionService
    {
        #region Promotion CRUD

        /// <summary>
        /// Get promotion paged list
        /// </summary>
        public async Task<PagedInfo<ErpPromotion>> GetPageListAsync(Expression<Func<ErpPromotion, bool>> predicate, PromotionQueryDto pager)
        {
            try
            {
                var query = Queryable()
                    .Where(predicate)
                    .OrderByDescending(e => e.Create_time);

                return await query.ToPageAsync(pager);
            }
            catch (Exception ex)
            {
                throw new CustomException($"Error retrieving promotion list: {ex.Message}");
            }
        }

        /// <summary>
        /// Get promotion list
        /// </summary>
        public new async Task<List<ErpPromotion>> GetListAsync(Expression<Func<ErpPromotion, bool>> predicate)
        {
            try
            {
                return await Queryable()
                    .Where(predicate)
                    .OrderByDescending(e => e.Create_time)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new CustomException($"Error retrieving promotions: {ex.Message}");
            }
        }

        /// <summary>
        /// Get promotion by ID
        /// </summary>
        public new async Task<ErpPromotion> GetByIdAsync(long promotionId)
        {
            try
            {
                if (promotionId <= 0)
                {
                    throw new CustomException("Invalid promotion ID");
                }

                var promotion = await GetFirstAsync(e => e.PromotionId == promotionId);
                return promotion;
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException($"Error retrieving promotion: {ex.Message}");
            }
        }

        /// <summary>
        /// Get promotion by code
        /// </summary>
        public async Task<ErpPromotion> GetByCodeAsync(string promotionCode)
        {
            if (string.IsNullOrWhiteSpace(promotionCode))
                return null;

            return await GetFirstAsync(e => e.PromotionCode == promotionCode);
        }

        /// <summary>
        /// Add new promotion
        /// </summary>
        public async Task<int> AddAsync(ErpPromotion promotion)
        {
            try
            {
                // Validate required fields
                if (string.IsNullOrEmpty(promotion.PromotionCode))
                {
                    throw new CustomException("Promotion code is required");
                }

                if (string.IsNullOrEmpty(promotion.PromotionName))
                {
                    throw new CustomException("Promotion name is required");
                }

                // Check for duplicate code
                var exists = await ExistsAsync(e => e.PromotionCode == promotion.PromotionCode);
                if (exists)
                {
                    throw new CustomException($"Promotion code {promotion.PromotionCode} already exists");
                }

                // Validate dates
                if (promotion.EndDate <= promotion.StartDate)
                {
                    throw new CustomException("End date must be after start date");
                }

                promotion.Create_time = DateTime.Now;
                promotion.UsageCount = 0;

                var result = await Insertable(promotion).ExecuteReturnSnowflakeIdAsync();
                return result > 0 ? 1 : 0;
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException($"Error adding promotion: {ex.Message}");
            }
        }

        /// <summary>
        /// Update promotion
        /// </summary>
        public async Task<int> UpdateAsync(ErpPromotion promotion)
        {
            try
            {
                if (promotion.PromotionId <= 0)
                {
                    throw new CustomException("Invalid promotion ID");
                }

                // Check if exists
                var existing = await GetByIdAsync(promotion.PromotionId);
                if (existing == null)
                {
                    throw new CustomException("Promotion not found");
                }

                // Check for duplicate code if changed
                if (promotion.PromotionCode != existing.PromotionCode)
                {
                    var codeExists = await ExistsAsync(e =>
                        e.PromotionCode == promotion.PromotionCode &&
                        e.PromotionId != promotion.PromotionId);

                    if (codeExists)
                    {
                        throw new CustomException($"Promotion code {promotion.PromotionCode} already exists");
                    }
                }

                promotion.Update_time = DateTime.Now;

                return await Context.Updateable(promotion)
                    .IgnoreColumns(e => new { e.Create_by, e.Create_time, e.UsageCount })
                    .ExecuteCommandAsync();
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException($"Error updating promotion: {ex.Message}");
            }
        }

        /// <summary>
        /// Delete promotions
        /// </summary>
        public async Task<int> DeleteAsync(long[] promotionIds)
        {
            try
            {
                if (promotionIds == null || promotionIds.Length == 0)
                {
                    throw new CustomException("No promotion IDs provided");
                }

                // Check for usage before deletion
                foreach (var id in promotionIds)
                {
                    var hasUsage = await Context.Queryable<ErpPromotionUsage>()
                        .AnyAsync(u => u.PromotionId == id);

                    if (hasUsage)
                    {
                        throw new CustomException($"Cannot delete promotion {id}: has usage records");
                    }
                }

                return await Context.Deleteable<ErpPromotion>()
                    .In(promotionIds)
                    .ExecuteCommandAsync();
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException($"Error deleting promotions: {ex.Message}");
            }
        }

        /// <summary>
        /// Check if promotion exists
        /// </summary>
        public new async Task<bool> ExistsAsync(Expression<Func<ErpPromotion, bool>> predicate)
        {
            return await Context.Queryable<ErpPromotion>().AnyAsync(predicate);
        }

        #endregion

        #region Promotion Business Logic

        /// <summary>
        /// Get all active promotions
        /// </summary>
        public async Task<List<ErpPromotion>> GetActivePromotionsAsync()
        {
            var now = DateTime.Now;
            return await GetListAsync(p =>
                p.IsActive &&
                p.Status == "active" &&
                p.StartDate <= now &&
                p.EndDate >= now);
        }

        /// <summary>
        /// Get applicable promotions for a member and amount
        /// </summary>
        public async Task<List<ErpPromotion>> GetApplicablePromotionsAsync(long memberId, string itemType, decimal amount)
        {
            var activePromotions = await GetActivePromotionsAsync();

            var applicablePromotions = new List<ErpPromotion>();

            foreach (var promotion in activePromotions)
            {
                // Check usage limit
                if (promotion.UsageLimit.HasValue && promotion.UsageCount >= promotion.UsageLimit)
                    continue;

                // Check minimum purchase amount
                if (promotion.MinPurchaseAmount.HasValue && amount < promotion.MinPurchaseAmount)
                    continue;

                // Check applicable items
                if (!string.IsNullOrEmpty(promotion.ApplicableTo) &&
                    promotion.ApplicableTo != "all" &&
                    promotion.ApplicableTo != itemType)
                    continue;

                // Check member usage limit
                if (promotion.UsageLimitPerMember.HasValue)
                {
                    var memberUsage = await GetMemberUsageCountAsync(promotion.PromotionId, memberId);
                    if (memberUsage >= promotion.UsageLimitPerMember)
                        continue;
                }

                applicablePromotions.Add(promotion);
            }

            return applicablePromotions.OrderByDescending(p => p.Priority).ToList();
        }

        /// <summary>
        /// Validate a promotion
        /// </summary>
        public async Task<PromotionValidationResult> ValidatePromotionAsync(string code, long memberId, decimal amount)
        {
            var promotion = await GetByCodeAsync(code);

            if (promotion == null)
            {
                return new PromotionValidationResult
                {
                    IsValid = false,
                    Message = "Invalid promotion code",
                    MessageAr = "كود العرض غير صالح"
                };
            }

            if (!promotion.IsActive || promotion.Status != "active")
            {
                return new PromotionValidationResult
                {
                    IsValid = false,
                    Message = "Promotion is not active",
                    MessageAr = "العرض غير نشط"
                };
            }

            var now = DateTime.Now;
            if (now < promotion.StartDate || now > promotion.EndDate)
            {
                return new PromotionValidationResult
                {
                    IsValid = false,
                    Message = "Promotion has expired or not yet started",
                    MessageAr = "العرض منتهي أو لم يبدأ بعد"
                };
            }

            if (promotion.UsageLimit.HasValue && promotion.UsageCount >= promotion.UsageLimit)
            {
                return new PromotionValidationResult
                {
                    IsValid = false,
                    Message = "Promotion usage limit reached",
                    MessageAr = "تم الوصول للحد الأقصى للاستخدام"
                };
            }

            if (promotion.MinPurchaseAmount.HasValue && amount < promotion.MinPurchaseAmount)
            {
                return new PromotionValidationResult
                {
                    IsValid = false,
                    Message = $"Minimum purchase of {promotion.MinPurchaseAmount} SAR required",
                    MessageAr = $"الحد الأدنى للشراء {promotion.MinPurchaseAmount} ريال"
                };
            }

            if (promotion.UsageLimitPerMember.HasValue)
            {
                var memberUsageCount = await GetMemberUsageCountAsync(promotion.PromotionId, memberId);
                if (memberUsageCount >= promotion.UsageLimitPerMember)
                {
                    return new PromotionValidationResult
                    {
                        IsValid = false,
                        Message = "You have reached the usage limit for this promotion",
                        MessageAr = "لقد وصلت للحد الأقصى لاستخدام هذا العرض"
                    };
                }
            }

            var discount = await CalculateDiscountAsync(promotion.PromotionId, amount);

            return new PromotionValidationResult
            {
                IsValid = true,
                Promotion = promotion,
                DiscountAmount = discount,
                FinalAmount = amount - discount,
                Message = "Promotion applied successfully",
                MessageAr = "تم تطبيق العرض بنجاح"
            };
        }

        /// <summary>
        /// Calculate discount for a promotion
        /// </summary>
        public async Task<decimal> CalculateDiscountAsync(long promotionId, decimal originalAmount)
        {
            var promotion = await GetByIdAsync(promotionId);
            if (promotion == null) return 0;

            decimal discount = promotion.PromotionType switch
            {
                "percentage" => originalAmount * (promotion.DiscountValue / 100),
                "fixed_amount" => promotion.DiscountValue,
                _ => 0
            };

            // Apply max discount cap
            if (promotion.MaxDiscountAmount.HasValue && discount > promotion.MaxDiscountAmount)
            {
                discount = promotion.MaxDiscountAmount.Value;
            }

            // Ensure discount doesn't exceed original amount
            if (discount > originalAmount)
            {
                discount = originalAmount;
            }

            return Math.Round(discount, 2);
        }

        /// <summary>
        /// Record promotion usage
        /// </summary>
        public async Task RecordUsageAsync(long promotionId, long memberId, long? invoiceId, decimal discountApplied)
        {
            try
            {
                var usage = new ErpPromotionUsage
                {
                    PromotionId = promotionId,
                    MemberId = memberId,
                    InvoiceId = invoiceId,
                    DiscountApplied = discountApplied,
                    UsedAt = DateTime.Now,
                    Create_time = DateTime.Now
                };

                await Context.Insertable(usage).ExecuteReturnSnowflakeIdAsync();

                // Increment usage count
                await Context.Updateable<ErpPromotion>()
                    .SetColumns(p => p.UsageCount == p.UsageCount + 1)
                    .Where(p => p.PromotionId == promotionId)
                    .ExecuteCommandAsync();
            }
            catch (Exception ex)
            {
                throw new CustomException($"Error recording promotion usage: {ex.Message}");
            }
        }

        /// <summary>
        /// Get member usage count for a promotion
        /// </summary>
        public async Task<int> GetMemberUsageCountAsync(long promotionId, long memberId)
        {
            return await Context.Queryable<ErpPromotionUsage>()
                .Where(u => u.PromotionId == promotionId && u.MemberId == memberId)
                .CountAsync();
        }

        /// <summary>
        /// Get promotion analytics
        /// </summary>
        public async Task<PromotionAnalyticsDto> GetPromotionAnalyticsAsync(long promotionId)
        {
            var promotion = await GetByIdAsync(promotionId);
            if (promotion == null)
            {
                throw new CustomException("Promotion not found");
            }

            var usages = await Context.Queryable<ErpPromotionUsage>()
                .Where(u => u.PromotionId == promotionId)
                .ToListAsync();

            var dailyUsage = usages
                .GroupBy(u => u.UsedAt.Date)
                .Select(g => new DailyUsageDto
                {
                    Date = g.Key,
                    UsageCount = g.Count(),
                    DiscountAmount = g.Sum(x => x.DiscountApplied)
                })
                .OrderBy(d => d.Date)
                .ToList();

            return new PromotionAnalyticsDto
            {
                PromotionId = promotionId,
                PromotionCode = promotion.PromotionCode,
                PromotionName = promotion.PromotionName,
                TotalUsageCount = usages.Count,
                UniqueMembers = usages.Select(u => u.MemberId).Distinct().Count(),
                TotalDiscountGiven = usages.Sum(u => u.DiscountApplied),
                AverageDiscountPerUse = usages.Count > 0 ? usages.Average(u => u.DiscountApplied) : 0,
                DailyUsage = dailyUsage
            };
        }

        /// <summary>
        /// Update promotion status
        /// </summary>
        public async Task<int> UpdateStatusAsync(long promotionId, string status)
        {
            var validStatuses = new[] { "draft", "active", "paused", "expired" };
            if (!validStatuses.Contains(status.ToLower()))
            {
                throw new CustomException($"Invalid status. Must be one of: {string.Join(", ", validStatuses)}");
            }

            return await Context.Updateable<ErpPromotion>()
                .SetColumns(p => p.Status == status)
                .SetColumns(p => p.Update_time == DateTime.Now)
                .Where(p => p.PromotionId == promotionId)
                .ExecuteCommandAsync();
        }

        /// <summary>
        /// Get promotion statistics (aggregate)
        /// </summary>
        public async Task<object> GetStatisticsAsync()
        {
            try
            {
                var now = DateTime.Now;
                var allPromotions = await Queryable().ToListAsync();

                if (allPromotions == null || allPromotions.Count == 0)
                {
                    return new
                    {
                        totalPromotions = 0,
                        activePromotions = 0,
                        scheduledPromotions = 0,
                        expiredPromotions = 0,
                        totalRedemptions = 0,
                        totalDiscountGiven = 0m,
                        averageDiscountPerRedemption = 0m,
                        topPromotions = new List<object>()
                    };
                }

                var activeCount = allPromotions.Count(p => p.IsActive && p.Status == "active" && p.StartDate <= now && p.EndDate >= now);
                var scheduledCount = allPromotions.Count(p => p.Status == "active" && p.StartDate > now);
                var expiredCount = allPromotions.Count(p => p.EndDate < now || p.Status == "expired");

                // Get usage data
                var usages = new List<ErpPromotionUsage>();
                try
                {
                    usages = await Context.Queryable<ErpPromotionUsage>().ToListAsync();
                }
                catch { /* table may not exist yet */ }

                var totalRedemptions = usages.Count;
                var totalDiscountGiven = usages.Sum(u => u.DiscountApplied);
                var avgDiscount = totalRedemptions > 0 ? totalDiscountGiven / totalRedemptions : 0;

                // Top promotions by usage
                var topPromotions = usages
                    .GroupBy(u => u.PromotionId)
                    .Select(g =>
                    {
                        var promo = allPromotions.FirstOrDefault(p => p.PromotionId == g.Key);
                        return new
                        {
                            promotionId = g.Key,
                            promotionCode = promo?.PromotionCode ?? "",
                            nameEn = promo?.PromotionName ?? "",
                            totalUsed = g.Count(),
                            totalDiscountGiven = g.Sum(x => x.DiscountApplied)
                        };
                    })
                    .OrderByDescending(x => x.totalUsed)
                    .Take(5)
                    .ToList();

                return new
                {
                    totalPromotions = allPromotions.Count,
                    activePromotions = activeCount,
                    scheduledPromotions = scheduledCount,
                    expiredPromotions = expiredCount,
                    totalRedemptions,
                    totalDiscountGiven,
                    averageDiscountPerRedemption = Math.Round(avgDiscount, 2),
                    topPromotions
                };
            }
            catch (Exception)
            {
                return new
                {
                    totalPromotions = 0,
                    activePromotions = 0,
                    scheduledPromotions = 0,
                    expiredPromotions = 0,
                    totalRedemptions = 0,
                    totalDiscountGiven = 0m,
                    averageDiscountPerRedemption = 0m,
                    topPromotions = new List<object>()
                };
            }
        }

        #endregion
    }
}

