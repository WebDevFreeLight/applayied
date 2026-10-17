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
    /// Promotion Service Interface
    /// </summary>
    public interface IPromotionService
    {
        #region Promotion CRUD

        /// <summary>
        /// Get promotion paged list
        /// </summary>
        Task<PagedInfo<ErpPromotion>> GetPageListAsync(Expression<Func<ErpPromotion, bool>> predicate, PromotionQueryDto pager);

        /// <summary>
        /// Get promotion list
        /// </summary>
        Task<List<ErpPromotion>> GetListAsync(Expression<Func<ErpPromotion, bool>> predicate);

        /// <summary>
        /// Get promotion by ID
        /// </summary>
        Task<ErpPromotion> GetByIdAsync(long promotionId);

        /// <summary>
        /// Get promotion by code
        /// </summary>
        Task<ErpPromotion> GetByCodeAsync(string promotionCode);

        /// <summary>
        /// Add new promotion
        /// </summary>
        Task<int> AddAsync(ErpPromotion promotion);

        /// <summary>
        /// Update promotion
        /// </summary>
        Task<int> UpdateAsync(ErpPromotion promotion);

        /// <summary>
        /// Delete promotions
        /// </summary>
        Task<int> DeleteAsync(long[] promotionIds);

        /// <summary>
        /// Check if promotion exists
        /// </summary>
        Task<bool> ExistsAsync(Expression<Func<ErpPromotion, bool>> predicate);

        #endregion

        #region Promotion Business Logic

        /// <summary>
        /// Get all active promotions
        /// </summary>
        Task<List<ErpPromotion>> GetActivePromotionsAsync();

        /// <summary>
        /// Get applicable promotions for a member and amount
        /// </summary>
        Task<List<ErpPromotion>> GetApplicablePromotionsAsync(long memberId, string itemType, decimal amount);

        /// <summary>
        /// Validate a promotion
        /// </summary>
        Task<PromotionValidationResult> ValidatePromotionAsync(string code, long memberId, decimal amount);

        /// <summary>
        /// Calculate discount for a promotion
        /// </summary>
        Task<decimal> CalculateDiscountAsync(long promotionId, decimal originalAmount);

        /// <summary>
        /// Record promotion usage
        /// </summary>
        Task RecordUsageAsync(long promotionId, long memberId, long? invoiceId, decimal discountApplied);

        /// <summary>
        /// Get member usage count for a promotion
        /// </summary>
        Task<int> GetMemberUsageCountAsync(long promotionId, long memberId);

        /// <summary>
        /// Get promotion analytics
        /// </summary>
        Task<PromotionAnalyticsDto> GetPromotionAnalyticsAsync(long promotionId);

        /// <summary>
        /// Update promotion status
        /// </summary>
        Task<int> UpdateStatusAsync(long promotionId, string status);

        /// <summary>
        /// Get promotion statistics (aggregate)
        /// </summary>
        Task<object> GetStatisticsAsync();

        #endregion
    }
}

