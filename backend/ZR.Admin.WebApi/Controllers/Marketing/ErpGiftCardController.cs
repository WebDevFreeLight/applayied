using Microsoft.AspNetCore.Mvc;
using ZR.Model;
using ZR.Model.Marketing;
using ZR.Service.Marketing.IService;
using ZR.Admin.WebApi.Filters;
using ZR.Common;
using Infrastructure.Enums;

namespace ZR.Admin.WebApi.Controllers.Marketing
{
    /// <summary>
    /// Gift Card Management - إدارة بطاقات الهدايا
    /// </summary>
    [Verify]
    [Route("api/erp/gift-cards")]
    [ApiController]
    public class ErpGiftCardController : BaseController
    {
        private readonly IErpGiftCardService _giftCardService;

        public ErpGiftCardController(IErpGiftCardService giftCardService)
        {
            _giftCardService = giftCardService;
        }

        /// <summary>
        /// Get gift card list - قائمة بطاقات الهدايا
        /// </summary>
        [HttpGet("list")]
        [ActionPermissionFilter("marketing:giftcard:list")]
        public IActionResult GetList([FromQuery] ErpGiftCard parm, [FromQuery] PagerInfo pager)
        {
            var response = _giftCardService.GetList(parm, pager);
            return SUCCESS(response);
        }

        /// <summary>
        /// Get gift card by ID - الحصول على بطاقة هدية بالمعرف
        /// </summary>
        [HttpGet("{cardId}")]
        [ActionPermissionFilter("marketing:giftcard:query")]
        public IActionResult GetInfo(long cardId)
        {
            var response = _giftCardService.GetInfo(cardId);
            return SUCCESS(response);
        }

        /// <summary>
        /// Get gift card by code - الحصول على بطاقة هدية بالكود
        /// </summary>
        [HttpGet("by-code/{cardCode}")]
        [ActionPermissionFilter("marketing:giftcard:query")]
        public IActionResult GetByCode(string cardCode)
        {
            var response = _giftCardService.GetByCode(cardCode);
            return SUCCESS(response);
        }

        /// <summary>
        /// Create gift card - إنشاء بطاقة هدية
        /// </summary>
        [HttpPost]
        [ActionPermissionFilter("marketing:giftcard:add")]
        [Log(Title = "Create Gift Card", BusinessType = BusinessType.INSERT)]
        public IActionResult Create([FromBody] ErpGiftCard card)
        {
            card.Create_by = HttpContext.GetName();
            var response = _giftCardService.CreateGiftCard(card);
            return SUCCESS(response);
        }

        /// <summary>
        /// Activate gift card - تفعيل بطاقة هدية
        /// </summary>
        [HttpPost("activate/{cardId}")]
        [ActionPermissionFilter("marketing:giftcard:activate")]
        [Log(Title = "Activate Gift Card", BusinessType = BusinessType.UPDATE)]
        public IActionResult ActivateCard(long cardId)
        {
            var response = _giftCardService.ActivateGiftCard(cardId);
            return ToResponse(response);
        }

        /// <summary>
        /// Deactivate gift card - إلغاء تفعيل بطاقة هدية
        /// </summary>
        [HttpPost("deactivate/{cardId}")]
        [ActionPermissionFilter("marketing:giftcard:deactivate")]
        [Log(Title = "Deactivate Gift Card", BusinessType = BusinessType.UPDATE)]
        public IActionResult DeactivateCard(long cardId)
        {
            var response = _giftCardService.DeactivateGiftCard(cardId);
            return ToResponse(response);
        }

        /// <summary>
        /// Use gift card - استخدام بطاقة هدية
        /// </summary>
        [HttpPost("use")]
        [ActionPermissionFilter("marketing:giftcard:use")]
        [Log(Title = "Use Gift Card", BusinessType = BusinessType.UPDATE)]
        public IActionResult UseGiftCard([FromBody] UseGiftCardDto dto)
        {
            try
            {
                var response = _giftCardService.UseGiftCard(dto.CardCode, dto.Amount, dto.ReferenceId, dto.ReferenceType);
                return SUCCESS(new { CardCode = dto.CardCode, AmountUsed = dto.Amount, RemainingBalance = response });
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.CUSTOM_ERROR, ex.Message);
            }
        }

        /// <summary>
        /// Get balance - الحصول على الرصيد
        /// </summary>
        [HttpGet("balance/{cardCode}")]
        [ActionPermissionFilter("marketing:giftcard:query")]
        public IActionResult GetBalance(string cardCode)
        {
            var response = _giftCardService.GetGiftCardBalance(cardCode);
            return SUCCESS(new { CardCode = cardCode, Balance = response });
        }

        /// <summary>
        /// Get expiring cards - البطاقات التي ستنتهي صلاحيتها
        /// </summary>
        [HttpGet("expiring")]
        [ActionPermissionFilter("marketing:giftcard:report")]
        public IActionResult GetExpiringCards([FromQuery] int days = 30)
        {
            var response = _giftCardService.GetExpiringSoonCards(days);
            return SUCCESS(response);
        }

        /// <summary>
        /// Get gift card report - تقرير بطاقات الهدايا
        /// </summary>
        [HttpGet("report")]
        [ActionPermissionFilter("marketing:giftcard:report")]
        public IActionResult GetGiftCardReport([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
        {
            var response = _giftCardService.GetGiftCardReport(fromDate, toDate);
            return SUCCESS(response);
        }
    }

    public class UseGiftCardDto
    {
        public string CardCode { get; set; }
        public decimal Amount { get; set; }
        public long ReferenceId { get; set; }
        public string ReferenceType { get; set; } = "invoice";
    }
}

