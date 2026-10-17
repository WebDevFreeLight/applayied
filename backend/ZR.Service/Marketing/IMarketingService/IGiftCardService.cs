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
    /// Gift Card Service Interface
    /// </summary>
    public interface IGiftCardService
    {
        #region Gift Card CRUD

        /// <summary>
        /// Get gift card paged list
        /// </summary>
        Task<PagedInfo<ErpGiftCard>> GetPageListAsync(Expression<Func<ErpGiftCard, bool>> predicate, GiftCardQueryDto pager);

        /// <summary>
        /// Get gift card list
        /// </summary>
        Task<List<ErpGiftCard>> GetListAsync(Expression<Func<ErpGiftCard, bool>> predicate);

        /// <summary>
        /// Get gift card by ID
        /// </summary>
        Task<ErpGiftCard> GetByIdAsync(long giftCardId);

        /// <summary>
        /// Get gift card by card number
        /// </summary>
        Task<ErpGiftCard> GetByCardNumberAsync(string cardNumber);

        /// <summary>
        /// Delete gift cards
        /// </summary>
        Task<int> DeleteAsync(long[] giftCardIds);

        /// <summary>
        /// Check if gift card exists
        /// </summary>
        Task<bool> ExistsAsync(Expression<Func<ErpGiftCard, bool>> predicate);

        #endregion

        #region Gift Card Operations

        /// <summary>
        /// Create new gift card
        /// </summary>
        Task<ErpGiftCard> CreateGiftCardAsync(GiftCardCreateDto dto);

        /// <summary>
        /// Activate gift card
        /// </summary>
        Task<ErpGiftCard> ActivateCardAsync(string cardNumber, string pin);

        /// <summary>
        /// Check gift card balance
        /// </summary>
        Task<GiftCardBalanceResult> CheckBalanceAsync(string cardNumber, string pin);

        /// <summary>
        /// Redeem gift card
        /// </summary>
        Task<GiftCardRedeemResult> RedeemAsync(string cardNumber, string pin, decimal amount, long? invoiceId, string notes = null);

        /// <summary>
        /// Refund to gift card
        /// </summary>
        Task<ErpGiftCard> RefundToCardAsync(long giftCardId, decimal amount, string reason);

        /// <summary>
        /// Send digital gift card
        /// </summary>
        Task SendDigitalCardAsync(long giftCardId);

        /// <summary>
        /// Cancel gift card
        /// </summary>
        Task<int> CancelCardAsync(long giftCardId, string reason);

        /// <summary>
        /// Update gift card status
        /// </summary>
        Task<int> UpdateStatusAsync(long giftCardId, string status);

        #endregion

        #region Transaction History

        /// <summary>
        /// Get transaction history for a gift card
        /// </summary>
        Task<List<ErpGiftCardTransaction>> GetTransactionHistoryAsync(long giftCardId);

        /// <summary>
        /// Get paged transaction history
        /// </summary>
        Task<PagedInfo<GiftCardTransactionDto>> GetPagedTransactionHistoryAsync(GiftCardTransactionQueryDto query);

        #endregion

        #region Statistics & Reporting

        /// <summary>
        /// Get gift card statistics
        /// </summary>
        Task<GiftCardStatsDto> GetStatisticsAsync();

        /// <summary>
        /// Get expiring gift cards
        /// </summary>
        Task<List<ErpGiftCard>> GetExpiringCardsAsync(int daysBeforeExpiry);

        #endregion
    }
}

