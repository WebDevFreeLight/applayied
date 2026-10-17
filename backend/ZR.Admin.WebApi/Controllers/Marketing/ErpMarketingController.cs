using Microsoft.AspNetCore.Mvc;
using ZR.Model.Marketing;
using ZR.Model.Notification;
using ZR.Service.Marketing.IService;
using ZR.Service.Notification.IService;
using ZR.Admin.WebApi.Filters;

namespace ZR.Admin.WebApi.Controllers.Marketing
{
    /// <summary>
    /// Marketing Controller - متحكم التسويق (برومو كود، بطاقات الهدايا، الإشعارات)
    /// </summary>
    [Verify]
    [Route("api/erp/marketing")]
    [ApiController]
    public class ErpMarketingController : BaseController
    {
        private readonly IErpPromoCodeService _promoCodeService;
        private readonly IErpGiftCardService _giftCardService;
        private readonly IErpNotificationService _notificationService;

        public ErpMarketingController(
            IErpPromoCodeService promoCodeService,
            IErpGiftCardService giftCardService,
            IErpNotificationService notificationService)
        {
            _promoCodeService = promoCodeService;
            _giftCardService = giftCardService;
            _notificationService = notificationService;
        }

        #region Promo Codes - برومو كود

        [HttpGet("promo-codes")]
        [ActionPermissionFilter(Permission = "erp:marketing:list")]
        public IActionResult GetPromoCodes([FromQuery] ErpPromoCode parm, [FromQuery] Model.PagerInfo pager) => SUCCESS(_promoCodeService.GetList(parm, pager));

        [HttpGet("promo-codes/{id}")]
        [ActionPermissionFilter(Permission = "erp:marketing:query")]
        public IActionResult GetPromoCode(long id)
        {
            var response = _promoCodeService.GetInfo(id);
            if (response == null) return ToResponse(ResultCode.FAIL, "Promo code not found - البرومو كود غير موجود");
            return SUCCESS(response);
        }

        [HttpPost("promo-codes")]
        [ActionPermissionFilter(Permission = "erp:marketing:add")]
        [Log(Title = "Promo Code", BusinessType = BusinessType.INSERT)]
        public IActionResult CreatePromoCode([FromBody] ErpPromoCode model) => SUCCESS(_promoCodeService.CreatePromoCode(model));

        [HttpPut("promo-codes")]
        [ActionPermissionFilter(Permission = "erp:marketing:edit")]
        [Log(Title = "Promo Code", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdatePromoCode([FromBody] ErpPromoCode model) => ToResponse(_promoCodeService.UpdatePromoCode(model));

        [HttpDelete("promo-codes/{ids}")]
        [ActionPermissionFilter(Permission = "erp:marketing:delete")]
        [Log(Title = "Promo Code", BusinessType = BusinessType.DELETE)]
        public IActionResult DeletePromoCodes(string ids)
        {
            long[] idsArr = Tools.SpitLongArrary(ids);
            if (idsArr.Length <= 0) return ToResponse(ResultCode.FAIL);
            return ToResponse(_promoCodeService.Delete(idsArr));
        }

        [HttpPost("promo-codes/validate")]
        [ActionPermissionFilter(Permission = "erp:marketing:query")]
        public IActionResult ValidatePromoCode([FromBody] ValidatePromoRequest request)
        {
            var isValid = _promoCodeService.ValidatePromoCode(request.Code, request.CustomerId, request.Amount);
            var discount = isValid ? _promoCodeService.ApplyPromoCode(request.Code, request.Amount) : 0;
            return SUCCESS(new { IsValid = isValid, DiscountAmount = discount });
        }

        [HttpGet("promo-codes/active")]
        [ActionPermissionFilter(Permission = "erp:marketing:list")]
        public IActionResult GetActivePromoCodes() => SUCCESS(_promoCodeService.GetActivePromoCodes());

        [HttpGet("promo-codes/code/{code}")]
        [ActionPermissionFilter(Permission = "erp:marketing:query")]
        public IActionResult GetPromoCodeByCode(string code)
        {
            var response = _promoCodeService.GetByCode(code);
            if (response == null) return ToResponse(ResultCode.FAIL, "Promo code not found - البرومو كود غير موجود");
            return SUCCESS(response);
        }

        #endregion

        #region Gift Cards - بطاقات الهدايا

        [HttpGet("gift-cards")]
        [ActionPermissionFilter(Permission = "erp:marketing:list")]
        public IActionResult GetGiftCards([FromQuery] ErpGiftCard parm, [FromQuery] Model.PagerInfo pager) => SUCCESS(_giftCardService.GetList(parm, pager));

        [HttpGet("gift-cards/{id}")]
        [ActionPermissionFilter(Permission = "erp:marketing:query")]
        public IActionResult GetGiftCard(long id)
        {
            var response = _giftCardService.GetInfo(id);
            if (response == null) return ToResponse(ResultCode.FAIL, "Gift card not found - بطاقة الهدية غير موجودة");
            return SUCCESS(response);
        }

        [HttpPost("gift-cards")]
        [ActionPermissionFilter(Permission = "erp:marketing:add")]
        [Log(Title = "Gift Card", BusinessType = BusinessType.INSERT)]
        public IActionResult CreateGiftCard([FromBody] ErpGiftCard model) => SUCCESS(_giftCardService.CreateGiftCard(model));

        [HttpPost("gift-cards/{id}/activate")]
        [ActionPermissionFilter(Permission = "erp:marketing:activate")]
        [Log(Title = "Activate Gift Card", BusinessType = BusinessType.UPDATE)]
        public IActionResult ActivateGiftCard(long id) => ToResponse(_giftCardService.ActivateGiftCard(id));

        [HttpPost("gift-cards/{id}/deactivate")]
        [ActionPermissionFilter(Permission = "erp:marketing:deactivate")]
        [Log(Title = "Deactivate Gift Card", BusinessType = BusinessType.UPDATE)]
        public IActionResult DeactivateGiftCard(long id) => ToResponse(_giftCardService.DeactivateGiftCard(id));

        [HttpPost("gift-cards/redeem")]
        [ActionPermissionFilter(Permission = "erp:marketing:redeem")]
        [Log(Title = "Redeem Gift Card", BusinessType = BusinessType.OTHER)]
        public IActionResult RedeemGiftCard([FromBody] RedeemGiftCardRequest request)
        {
            var remaining = _giftCardService.UseGiftCard(request.CardNumber, request.Amount, request.ReferenceId, request.ReferenceType);
            return SUCCESS(new { RemainingBalance = remaining });
        }

        [HttpGet("gift-cards/code/{code}")]
        [ActionPermissionFilter(Permission = "erp:marketing:query")]
        public IActionResult GetGiftCardByCode(string code)
        {
            var response = _giftCardService.GetByCode(code);
            if (response == null) return ToResponse(ResultCode.FAIL, "Gift card not found - بطاقة الهدية غير موجودة");
            return SUCCESS(response);
        }

        [HttpGet("gift-cards/code/{code}/balance")]
        [ActionPermissionFilter(Permission = "erp:marketing:query")]
        public IActionResult GetGiftCardBalance(string code)
        {
            var balance = _giftCardService.GetGiftCardBalance(code);
            return SUCCESS(new { CardCode = code, Balance = balance });
        }

        [HttpGet("gift-cards/expiring")]
        [ActionPermissionFilter(Permission = "erp:marketing:list")]
        public IActionResult GetExpiringGiftCards([FromQuery] int days = 30) => SUCCESS(_giftCardService.GetExpiringSoonCards(days));

        #endregion

        #region Notifications - الإشعارات

        [HttpGet("notifications/{id}")]
        [ActionPermissionFilter(Permission = "erp:marketing:query")]
        public IActionResult GetNotification(long id)
        {
            var response = _notificationService.GetInfo(id);
            if (response == null) return ToResponse(ResultCode.FAIL, "Notification not found - الإشعار غير موجود");
            return SUCCESS(response);
        }

        [HttpPost("notifications")]
        [ActionPermissionFilter(Permission = "erp:marketing:add")]
        [Log(Title = "Notification", BusinessType = BusinessType.INSERT)]
        public IActionResult CreateNotification([FromBody] ErpNotification model) => SUCCESS(_notificationService.CreateNotification(model));

        [HttpGet("notifications/user/{userId}")]
        [ActionPermissionFilter(Permission = "erp:marketing:list")]
        public IActionResult GetUserNotifications(long userId, [FromQuery] bool unreadOnly = false) => SUCCESS(_notificationService.GetUserNotifications(userId, unreadOnly));

        [HttpGet("notifications/user/{userId}/unread-count")]
        [ActionPermissionFilter(Permission = "erp:marketing:query")]
        public IActionResult GetUnreadCount(long userId)
        {
            var count = _notificationService.GetUnreadCount(userId);
            return SUCCESS(new { UserId = userId, UnreadCount = count });
        }

        #endregion

        [HttpGet("export/promo-codes")]
        [ActionPermissionFilter(Permission = "erp:marketing:export")]
        public IActionResult ExportPromoCodes([FromQuery] ErpPromoCode parm, [FromQuery] Model.PagerInfo pager)
        {
            pager.PageSize = 10000;
            var list = _promoCodeService.GetList(parm, pager).Result;
            var result = ExportExcelMini(list, "PromoCodes", "Promo Codes - برومو كود");
            return ExportExcel(result.Item2, result.Item1);
        }

        [HttpGet("export/gift-cards")]
        [ActionPermissionFilter(Permission = "erp:marketing:export")]
        public IActionResult ExportGiftCards([FromQuery] ErpGiftCard parm, [FromQuery] Model.PagerInfo pager)
        {
            pager.PageSize = 10000;
            var list = _giftCardService.GetList(parm, pager).Result;
            var result = ExportExcelMini(list, "GiftCards", "Gift Cards - بطاقات الهدايا");
            return ExportExcel(result.Item2, result.Item1);
        }
    }

    public class ValidatePromoRequest { public string Code { get; set; } public long? CustomerId { get; set; } public decimal Amount { get; set; } }
    public class RedeemGiftCardRequest { public string CardNumber { get; set; } public decimal Amount { get; set; } public long ReferenceId { get; set; } public string ReferenceType { get; set; } }
}

