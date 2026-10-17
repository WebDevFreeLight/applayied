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
    /// Gift Card Management Controller - ادارة بطاقات الهدايا
    /// </summary>
    [Verify]
    [Route("api/marketing/gift-cards")]
    [ApiExplorerSettings(GroupName = "marketing")]
    public class GiftCardController : BaseController
    {
        private readonly IGiftCardService _giftCardService;

        public GiftCardController(IGiftCardService giftCardService)
        {
            _giftCardService = giftCardService;
        }

        #region Gift Card CRUD

        /// <summary>
        /// Get gift card list
        /// </summary>
        [HttpGet("list")]
        [ActionPermissionFilter("marketing:giftcard:list")]
        public async Task<IActionResult> GetGiftCardList([FromQuery] GiftCardQueryDto dto)
        {
            var predicate = Expressionable.Create<ErpGiftCard>();

            predicate = predicate.AndIF(!string.IsNullOrEmpty(dto.CardNumber),
                it => it.CardNumber.Contains(dto.CardNumber));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(dto.Status),
                it => it.Status == dto.Status);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(dto.CardType),
                it => it.CardType == dto.CardType);
            predicate = predicate.AndIF(dto.PurchasedByMemberId.HasValue,
                it => it.PurchasedByMemberId == dto.PurchasedByMemberId);
            predicate = predicate.AndIF(dto.AssignedToMemberId.HasValue,
                it => it.AssignedToMemberId == dto.AssignedToMemberId);
            predicate = predicate.AndIF(dto.IssueDateFrom.HasValue,
                it => it.IssueDate >= dto.IssueDateFrom);
            predicate = predicate.AndIF(dto.IssueDateTo.HasValue,
                it => it.IssueDate <= dto.IssueDateTo);
            predicate = predicate.AndIF(dto.IsActivated.HasValue,
                it => it.IsActivated == dto.IsActivated);

            var response = await _giftCardService.GetPageListAsync(predicate.ToExpression(), dto);
            return SUCCESS(response);
        }

        /// <summary>
        /// Get gift card details
        /// </summary>
        [HttpGet("{id}")]
        [ActionPermissionFilter("marketing:giftcard:query")]
        public async Task<IActionResult> GetGiftCard(long id)
        {
            var giftCard = await _giftCardService.GetByIdAsync(id);
            if (giftCard == null)
            {
                return ToResponse(ResultCode.FAIL, "Gift card not found");
            }
            return SUCCESS(giftCard);
        }

        /// <summary>
        /// Create new gift card
        /// </summary>
        [HttpPost]
        [ActionPermissionFilter("marketing:giftcard:add")]
        [Log(Title = "Gift Card Management", BusinessType = BusinessType.INSERT)]
        public async Task<IActionResult> CreateGiftCard([FromBody] GiftCardCreateDto dto)
        {
            var giftCard = await _giftCardService.CreateGiftCardAsync(dto);
            return SUCCESS(giftCard);
        }

        /// <summary>
        /// Delete gift cards
        /// </summary>
        [HttpDelete("{ids}")]
        [ActionPermissionFilter("marketing:giftcard:delete")]
        [Log(Title = "Gift Card Management", BusinessType = BusinessType.DELETE)]
        public async Task<IActionResult> DeleteGiftCard(string ids)
        {
            var idArray = Tools.SplitAndConvert<long>(ids);
            var result = await _giftCardService.DeleteAsync(idArray);
            return SUCCESS(result);
        }

        #endregion

        #region Gift Card Operations

        /// <summary>
        /// Activate gift card
        /// </summary>
        [HttpPost("activate")]
        [ActionPermissionFilter("marketing:giftcard:activate")]
        [Log(Title = "Gift Card Activation", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> ActivateCard([FromBody] GiftCardActivateRequest request)
        {
            var card = await _giftCardService.ActivateCardAsync(request.CardNumber, request.Pin);
            return SUCCESS(new { Message = "Gift card activated successfully", CardNumber = card.CardNumber });
        }

        /// <summary>
        /// Check gift card balance
        /// </summary>
        [HttpPost("check-balance")]
        [ActionPermissionFilter("marketing:giftcard:query")]
        public async Task<IActionResult> CheckBalance([FromBody] GiftCardBalanceRequest request)
        {
            var result = await _giftCardService.CheckBalanceAsync(request.CardNumber, request.Pin);
            return SUCCESS(result);
        }

        /// <summary>
        /// Redeem gift card
        /// </summary>
        [HttpPost("redeem")]
        [ActionPermissionFilter("marketing:giftcard:redeem")]
        [Log(Title = "Gift Card Redemption", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> RedeemCard([FromBody] GiftCardRedeemRequest request)
        {
            var result = await _giftCardService.RedeemAsync(
                request.CardNumber, request.Pin, request.Amount, request.InvoiceId, request.Notes);
            return SUCCESS(result);
        }

        /// <summary>
        /// Refund to gift card
        /// </summary>
        [HttpPost("refund")]
        [ActionPermissionFilter("marketing:giftcard:refund")]
        [Log(Title = "Gift Card Refund", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> RefundToCard([FromBody] GiftCardRefundRequest request)
        {
            var card = await _giftCardService.RefundToCardAsync(request.GiftCardId, request.Amount, request.Reason);
            return SUCCESS(new { Message = "Refund successful", NewBalance = card.CurrentBalance });
        }

        /// <summary>
        /// Cancel gift card
        /// </summary>
        [HttpPost("{id}/cancel")]
        [ActionPermissionFilter("marketing:giftcard:cancel")]
        [Log(Title = "Gift Card Cancellation", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> CancelCard(long id, [FromQuery] string reason)
        {
            var result = await _giftCardService.CancelCardAsync(id, reason);
            return SUCCESS(new { Message = "Gift card cancelled successfully" });
        }

        /// <summary>
        /// Update gift card status
        /// </summary>
        [HttpPut("{id}/status")]
        [ActionPermissionFilter("marketing:giftcard:edit")]
        [Log(Title = "Gift Card Status Update", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> UpdateStatus(long id, [FromQuery] string status)
        {
            var result = await _giftCardService.UpdateStatusAsync(id, status);
            return SUCCESS(result);
        }

        /// <summary>
        /// Send digital gift card
        /// </summary>
        [HttpPost("{id}/send")]
        [ActionPermissionFilter("marketing:giftcard:send")]
        [Log(Title = "Send Digital Gift Card", BusinessType = BusinessType.OTHER)]
        public async Task<IActionResult> SendDigitalCard(long id)
        {
            await _giftCardService.SendDigitalCardAsync(id);
            return SUCCESS("Digital gift card sent successfully");
        }

        #endregion

        #region Transaction History

        /// <summary>
        /// Get transaction history for a gift card
        /// </summary>
        [HttpGet("{id}/history")]
        [ActionPermissionFilter("marketing:giftcard:query")]
        public async Task<IActionResult> GetTransactionHistory(long id)
        {
            var transactions = await _giftCardService.GetTransactionHistoryAsync(id);
            return SUCCESS(transactions);
        }

        /// <summary>
        /// Get paged transaction history
        /// </summary>
        [HttpGet("transactions")]
        [ActionPermissionFilter("marketing:giftcard:query")]
        public async Task<IActionResult> GetPagedTransactionHistory([FromQuery] GiftCardTransactionQueryDto query)
        {
            var transactions = await _giftCardService.GetPagedTransactionHistoryAsync(query);
            return SUCCESS(transactions);
        }

        #endregion

        #region Statistics & Reporting

        /// <summary>
        /// Get gift card statistics
        /// </summary>
        [HttpGet("statistics")]
        [ActionPermissionFilter("marketing:giftcard:statistics")]
        public async Task<IActionResult> GetStatistics()
        {
            var stats = await _giftCardService.GetStatisticsAsync();
            return SUCCESS(stats);
        }

        /// <summary>
        /// Get expiring gift cards
        /// </summary>
        [HttpGet("expiring")]
        [ActionPermissionFilter("marketing:giftcard:list")]
        public async Task<IActionResult> GetExpiringCards([FromQuery] int days = 30)
        {
            var cards = await _giftCardService.GetExpiringCardsAsync(days);
            return SUCCESS(cards);
        }

        /// <summary>
        /// Export gift cards
        /// </summary>
        [HttpGet("export")]
        [ActionPermissionFilter("marketing:giftcard:export")]
        [Log(Title = "Gift Card Management", BusinessType = BusinessType.EXPORT)]
        public async Task<IActionResult> ExportGiftCards([FromQuery] GiftCardQueryDto dto)
        {
            var predicate = Expressionable.Create<ErpGiftCard>();

            predicate = predicate.AndIF(!string.IsNullOrEmpty(dto.Status),
                it => it.Status == dto.Status);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(dto.CardType),
                it => it.CardType == dto.CardType);

            var list = await _giftCardService.GetListAsync(predicate.ToExpression());
            var result = ExportExcelMini(list, "gift_cards", "Gift Card List");
            return ExportExcel(result.Item2, result.Item1);
        }

        #endregion
    }
}

