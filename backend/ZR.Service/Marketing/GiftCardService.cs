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
    /// Gift Card Service Implementation
    /// </summary>
    [AppService(ServiceType = typeof(IGiftCardService), ServiceLifetime = LifeTime.Transient)]
    public class GiftCardService : BaseService<ErpGiftCard>, IGiftCardService
    {
        #region Gift Card CRUD

        /// <summary>
        /// Get gift card paged list
        /// </summary>
        public async Task<PagedInfo<ErpGiftCard>> GetPageListAsync(Expression<Func<ErpGiftCard, bool>> predicate, GiftCardQueryDto pager)
        {
            try
            {
                var query = Queryable()
                    .Where(predicate)
                    .OrderByDescending(g => g.Create_time);

                return await query.ToPageAsync(pager);
            }
            catch (Exception ex)
            {
                throw new CustomException($"Error retrieving gift card list: {ex.Message}");
            }
        }

        /// <summary>
        /// Get gift card list
        /// </summary>
        public new async Task<List<ErpGiftCard>> GetListAsync(Expression<Func<ErpGiftCard, bool>> predicate)
        {
            return await Queryable()
                .Where(predicate)
                .OrderByDescending(g => g.Create_time)
                .ToListAsync();
        }

        /// <summary>
        /// Get gift card by ID
        /// </summary>
        public new async Task<ErpGiftCard> GetByIdAsync(long giftCardId)
        {
            return await GetFirstAsync(g => g.GiftCardId == giftCardId);
        }

        /// <summary>
        /// Get gift card by card number
        /// </summary>
        public async Task<ErpGiftCard> GetByCardNumberAsync(string cardNumber)
        {
            if (string.IsNullOrWhiteSpace(cardNumber))
                return null;

            return await GetFirstAsync(g => g.CardNumber == cardNumber);
        }

        /// <summary>
        /// Delete gift cards
        /// </summary>
        public async Task<int> DeleteAsync(long[] giftCardIds)
        {
            try
            {
                // Check for transactions before deletion
                foreach (var id in giftCardIds)
                {
                    var hasTransactions = await Context.Queryable<ErpGiftCardTransaction>()
                        .AnyAsync(t => t.GiftCardId == id);

                    if (hasTransactions)
                    {
                        throw new CustomException($"Cannot delete gift card {id}: has transaction records");
                    }
                }

                return await Context.Deleteable<ErpGiftCard>()
                    .In(giftCardIds)
                    .ExecuteCommandAsync();
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException($"Error deleting gift cards: {ex.Message}");
            }
        }

        /// <summary>
        /// Check if gift card exists
        /// </summary>
        public new async Task<bool> ExistsAsync(Expression<Func<ErpGiftCard, bool>> predicate)
        {
            return await Context.Queryable<ErpGiftCard>().AnyAsync(predicate);
        }

        #endregion

        #region Gift Card Operations

        /// <summary>
        /// Create new gift card
        /// </summary>
        public async Task<ErpGiftCard> CreateGiftCardAsync(GiftCardCreateDto dto)
        {
            try
            {
                var card = new ErpGiftCard
                {
                    CardNumber = GenerateCardNumber(),
                    Pin = GeneratePin(),
                    InitialBalance = dto.Amount,
                    CurrentBalance = dto.Amount,
                    Currency = dto.Currency ?? "SAR",
                    IssueDate = DateTime.Now,
                    ExpiryDate = DateTime.Now.AddMonths(dto.ValidityMonths ?? 12),
                    PurchasedByMemberId = dto.PurchasedByMemberId,
                    RecipientName = dto.RecipientName,
                    RecipientEmail = dto.RecipientEmail,
                    RecipientPhone = dto.RecipientPhone,
                    PersonalMessage = dto.PersonalMessage,
                    CardType = dto.CardType,
                    CardDesign = dto.CardDesign ?? "default",
                    Status = "active",
                    IsActivated = dto.CardType == "digital",
                    ActivatedAt = dto.CardType == "digital" ? DateTime.Now : null,
                    Remark = dto.Remark,
                    Create_time = DateTime.Now
                };

                var cardId = await Context.Insertable(card).ExecuteReturnSnowflakeIdAsync();
                card.GiftCardId = cardId;

                // Record purchase transaction
                await RecordTransactionAsync(cardId, "purchase", dto.Amount, 0, dto.Amount, null, "Gift card purchased");

                // Send digital card if applicable
                if (dto.CardType == "digital")
                {
                    await SendDigitalCardAsync(cardId);
                }

                return card;
            }
            catch (Exception ex)
            {
                throw new CustomException($"Error creating gift card: {ex.Message}");
            }
        }

        /// <summary>
        /// Activate gift card
        /// </summary>
        public async Task<ErpGiftCard> ActivateCardAsync(string cardNumber, string pin)
        {
            var card = await GetByCardNumberAsync(cardNumber);

            if (card == null || card.Pin != pin)
            {
                throw new CustomException("Invalid card number or PIN");
            }

            if (card.Status != "active")
            {
                throw new CustomException("Gift card is not active");
            }

            if (card.IsActivated)
            {
                throw new CustomException("Gift card is already activated");
            }

            if (card.ExpiryDate < DateTime.Now)
            {
                throw new CustomException("Gift card has expired");
            }

            card.IsActivated = true;
            card.ActivatedAt = DateTime.Now;
            card.Update_time = DateTime.Now;

            await Context.Updateable(card)
                .UpdateColumns(c => new { c.IsActivated, c.ActivatedAt, c.Update_time })
                .ExecuteCommandAsync();

            return card;
        }

        /// <summary>
        /// Check gift card balance
        /// </summary>
        public async Task<GiftCardBalanceResult> CheckBalanceAsync(string cardNumber, string pin)
        {
            var card = await GetByCardNumberAsync(cardNumber);

            if (card == null || card.Pin != pin)
            {
                return new GiftCardBalanceResult
                {
                    IsValid = false,
                    Message = "Invalid card number or PIN",
                    MessageAr = "رقم البطاقة أو الرقم السري غير صحيح"
                };
            }

            var isExpired = card.ExpiryDate < DateTime.Now;

            return new GiftCardBalanceResult
            {
                IsValid = true,
                Message = isExpired ? "Gift card has expired" : "Balance retrieved successfully",
                MessageAr = isExpired ? "البطاقة منتهية الصلاحية" : "تم استرجاع الرصيد بنجاح",
                CardNumber = MaskCardNumber(card.CardNumber),
                CurrentBalance = card.CurrentBalance,
                Currency = card.Currency,
                ExpiryDate = card.ExpiryDate,
                Status = card.Status,
                IsExpired = isExpired
            };
        }

        /// <summary>
        /// Redeem gift card
        /// </summary>
        public async Task<GiftCardRedeemResult> RedeemAsync(string cardNumber, string pin, decimal amount, long? invoiceId, string notes = null)
        {
            var card = await GetByCardNumberAsync(cardNumber);

            if (card == null || card.Pin != pin)
            {
                return new GiftCardRedeemResult
                {
                    Success = false,
                    Message = "Invalid card number or PIN",
                    MessageAr = "رقم البطاقة أو الرقم السري غير صحيح"
                };
            }

            if (card.Status != "active")
            {
                return new GiftCardRedeemResult
                {
                    Success = false,
                    Message = "Gift card is not active",
                    MessageAr = "البطاقة غير نشطة"
                };
            }

            if (card.ExpiryDate < DateTime.Now)
            {
                return new GiftCardRedeemResult
                {
                    Success = false,
                    Message = "Gift card has expired",
                    MessageAr = "البطاقة منتهية الصلاحية"
                };
            }

            if (!card.IsActivated)
            {
                return new GiftCardRedeemResult
                {
                    Success = false,
                    Message = "Gift card has not been activated",
                    MessageAr = "البطاقة لم يتم تفعيلها"
                };
            }

            if (card.CurrentBalance < amount)
            {
                return new GiftCardRedeemResult
                {
                    Success = false,
                    Message = $"Insufficient balance. Available: {card.CurrentBalance} {card.Currency}",
                    MessageAr = $"الرصيد غير كافي. المتوفر: {card.CurrentBalance} {card.Currency}"
                };
            }

            var balanceBefore = card.CurrentBalance;
            card.CurrentBalance -= amount;

            if (card.CurrentBalance == 0)
            {
                card.Status = "used";
            }

            card.Update_time = DateTime.Now;

            await Context.Updateable(card)
                .UpdateColumns(c => new { c.CurrentBalance, c.Status, c.Update_time })
                .ExecuteCommandAsync();

            // Record transaction
            var transaction = await RecordTransactionAsync(
                card.GiftCardId, "redemption", -amount,
                balanceBefore, card.CurrentBalance, invoiceId, notes);

            return new GiftCardRedeemResult
            {
                Success = true,
                Message = "Redemption successful",
                MessageAr = "تم الاسترداد بنجاح",
                AmountRedeemed = amount,
                RemainingBalance = card.CurrentBalance,
                TransactionId = transaction.TransactionId.ToString()
            };
        }

        /// <summary>
        /// Refund to gift card
        /// </summary>
        public async Task<ErpGiftCard> RefundToCardAsync(long giftCardId, decimal amount, string reason)
        {
            var card = await GetByIdAsync(giftCardId);

            if (card == null)
            {
                throw new CustomException("Gift card not found");
            }

            var balanceBefore = card.CurrentBalance;
            card.CurrentBalance += amount;

            if (card.Status == "used")
            {
                card.Status = "active";
            }

            card.Update_time = DateTime.Now;

            await Context.Updateable(card)
                .UpdateColumns(c => new { c.CurrentBalance, c.Status, c.Update_time })
                .ExecuteCommandAsync();

            // Record refund transaction
            await RecordTransactionAsync(
                card.GiftCardId, "refund", amount,
                balanceBefore, card.CurrentBalance, null, reason);

            return card;
        }

        /// <summary>
        /// Send digital gift card
        /// </summary>
        public async Task SendDigitalCardAsync(long giftCardId)
        {
            var card = await GetByIdAsync(giftCardId);
            if (card == null)
            {
                throw new CustomException("Gift card not found");
            }

            // This would integrate with notification service to send email/SMS
            // For now, this is a placeholder
            await Task.CompletedTask;
        }

        /// <summary>
        /// Cancel gift card
        /// </summary>
        public async Task<int> CancelCardAsync(long giftCardId, string reason)
        {
            var card = await GetByIdAsync(giftCardId);
            if (card == null)
            {
                throw new CustomException("Gift card not found");
            }

            card.Status = "cancelled";
            card.Remark = $"{card.Remark} | Cancelled: {reason}";
            card.Update_time = DateTime.Now;

            return await Context.Updateable(card)
                .UpdateColumns(c => new { c.Status, c.Remark, c.Update_time })
                .ExecuteCommandAsync();
        }

        /// <summary>
        /// Update gift card status
        /// </summary>
        public async Task<int> UpdateStatusAsync(long giftCardId, string status)
        {
            var validStatuses = new[] { "active", "used", "expired", "cancelled" };
            if (!validStatuses.Contains(status.ToLower()))
            {
                throw new CustomException($"Invalid status. Must be one of: {string.Join(", ", validStatuses)}");
            }

            return await Context.Updateable<ErpGiftCard>()
                .SetColumns(g => g.Status == status)
                .SetColumns(g => g.Update_time == DateTime.Now)
                .Where(g => g.GiftCardId == giftCardId)
                .ExecuteCommandAsync();
        }

        #endregion

        #region Transaction History

        /// <summary>
        /// Get transaction history for a gift card
        /// </summary>
        public async Task<List<ErpGiftCardTransaction>> GetTransactionHistoryAsync(long giftCardId)
        {
            return await Context.Queryable<ErpGiftCardTransaction>()
                .Where(t => t.GiftCardId == giftCardId)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();
        }

        /// <summary>
        /// Get paged transaction history
        /// </summary>
        public async Task<PagedInfo<GiftCardTransactionDto>> GetPagedTransactionHistoryAsync(GiftCardTransactionQueryDto query)
        {
            var q = Context.Queryable<ErpGiftCardTransaction>()
                .LeftJoin<ErpGiftCard>((t, g) => t.GiftCardId == g.GiftCardId)
                .WhereIF(query.GiftCardId.HasValue, (t, g) => t.GiftCardId == query.GiftCardId)
                .WhereIF(!string.IsNullOrEmpty(query.TransactionType), (t, g) => t.TransactionType == query.TransactionType)
                .WhereIF(query.StartDate.HasValue, (t, g) => t.TransactionDate >= query.StartDate)
                .WhereIF(query.EndDate.HasValue, (t, g) => t.TransactionDate <= query.EndDate)
                .OrderByDescending((t, g) => t.TransactionDate)
                .Select((t, g) => new GiftCardTransactionDto
                {
                    TransactionId = t.TransactionId,
                    GiftCardId = t.GiftCardId,
                    CardNumber = g.CardNumber,
                    TransactionType = t.TransactionType,
                    Amount = t.Amount,
                    BalanceBefore = t.BalanceBefore,
                    BalanceAfter = t.BalanceAfter,
                    InvoiceId = t.InvoiceId,
                    ProcessedBy = t.ProcessedBy,
                    Notes = t.Notes,
                    TransactionDate = t.TransactionDate
                });

            return await q.ToPageAsync(query);
        }

        private async Task<ErpGiftCardTransaction> RecordTransactionAsync(
            long giftCardId, string transactionType, decimal amount,
            decimal balanceBefore, decimal balanceAfter, long? invoiceId, string notes)
        {
            var transaction = new ErpGiftCardTransaction
            {
                GiftCardId = giftCardId,
                TransactionType = transactionType,
                Amount = amount,
                BalanceBefore = balanceBefore,
                BalanceAfter = balanceAfter,
                InvoiceId = invoiceId,
                Notes = notes,
                TransactionDate = DateTime.Now,
                Create_time = DateTime.Now
            };

            var transactionId = await Context.Insertable(transaction).ExecuteReturnSnowflakeIdAsync();
            transaction.TransactionId = transactionId;

            return transaction;
        }

        #endregion

        #region Statistics & Reporting

        /// <summary>
        /// Get gift card statistics
        /// </summary>
        public async Task<GiftCardStatsDto> GetStatisticsAsync()
        {
            List<ErpGiftCard> allCards;
            try
            {
                allCards = await Context.Queryable<ErpGiftCard>().ToListAsync();
            }
            catch
            {
                // Table may not exist yet — return empty stats
                allCards = new List<ErpGiftCard>();
            }

            if (allCards.Count == 0)
            {
                return new GiftCardStatsDto
                {
                    TotalCardsIssued = 0, ActiveCards = 0, UsedCards = 0, ExpiredCards = 0,
                    TotalValueIssued = 0, TotalValueRedeemed = 0, TotalValueOutstanding = 0, AverageCardValue = 0,
                    CardsByType = new Dictionary<string, int>(),
                    CardsByStatus = new Dictionary<string, int>()
                };
            }

            return new GiftCardStatsDto
            {
                TotalCardsIssued = allCards.Count,
                ActiveCards = allCards.Count(c => c.Status == "active"),
                UsedCards = allCards.Count(c => c.Status == "used"),
                ExpiredCards = allCards.Count(c => c.Status == "expired" || c.ExpiryDate < DateTime.Now),
                TotalValueIssued = allCards.Sum(c => c.InitialBalance),
                TotalValueRedeemed = allCards.Sum(c => c.InitialBalance - c.CurrentBalance),
                TotalValueOutstanding = allCards.Where(c => c.Status == "active").Sum(c => c.CurrentBalance),
                AverageCardValue = allCards.Average(c => c.InitialBalance),
                CardsByType = allCards.GroupBy(c => c.CardType ?? "unknown").ToDictionary(g => g.Key, g => g.Count()),
                CardsByStatus = allCards.GroupBy(c => c.Status ?? "unknown").ToDictionary(g => g.Key, g => g.Count())
            };
        }

        /// <summary>
        /// Get expiring gift cards
        /// </summary>
        public async Task<List<ErpGiftCard>> GetExpiringCardsAsync(int daysBeforeExpiry)
        {
            var expiryDate = DateTime.Now.AddDays(daysBeforeExpiry);

            return await Context.Queryable<ErpGiftCard>()
                .Where(g => g.Status == "active" &&
                            g.CurrentBalance > 0 &&
                            g.ExpiryDate <= expiryDate &&
                            g.ExpiryDate > DateTime.Now)
                .ToListAsync();
        }

        #endregion

        #region Helper Methods

        private string GenerateCardNumber()
        {
            // Format: 16 digits in groups of 4
            var random = new Random();
            return string.Join("", Enumerable.Range(0, 16)
                .Select(_ => random.Next(0, 10).ToString()));
        }

        private string GeneratePin()
        {
            return new Random().Next(1000, 9999).ToString();
        }

        private string MaskCardNumber(string cardNumber)
        {
            if (string.IsNullOrEmpty(cardNumber) || cardNumber.Length < 4)
                return cardNumber;

            return $"****-****-****-{cardNumber.Substring(cardNumber.Length - 4)}";
        }

        #endregion
    }
}

