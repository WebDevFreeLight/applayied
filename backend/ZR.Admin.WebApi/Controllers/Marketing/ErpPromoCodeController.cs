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
    /// Promo Code Management - إدارة أكواد الخصم
    /// </summary>
    [Verify]
    [Route("api/erp/promo-codes")]
    [ApiController]
    public class ErpPromoCodeController : BaseController
    {
        private readonly IErpPromoCodeService _promoCodeService;

        public ErpPromoCodeController(IErpPromoCodeService promoCodeService)
        {
            _promoCodeService = promoCodeService;
        }

        /// <summary>
        /// Get promo code list - قائمة أكواد الخصم
        /// </summary>
        [HttpGet("list")]
        [ActionPermissionFilter("marketing:promocode:list")]
        public IActionResult GetList([FromQuery] ErpPromoCode parm, [FromQuery] PagerInfo pager)
        {
            var response = _promoCodeService.GetList(parm, pager);
            return SUCCESS(response);
        }

        /// <summary>
        /// Get promo code by ID - الحصول على كود خصم بالمعرف
        /// </summary>
        [HttpGet("{promoCodeId}")]
        [ActionPermissionFilter("marketing:promocode:query")]
        public IActionResult GetInfo(long promoCodeId)
        {
            var response = _promoCodeService.GetInfo(promoCodeId);
            return SUCCESS(response);
        }

        /// <summary>
        /// Get promo code by code - الحصول على كود خصم بالكود
        /// </summary>
        [HttpGet("by-code/{code}")]
        [ActionPermissionFilter("marketing:promocode:query")]
        public IActionResult GetByCode(string code)
        {
            var response = _promoCodeService.GetByCode(code);
            return SUCCESS(response);
        }

        /// <summary>
        /// Create promo code - إنشاء كود خصم
        /// </summary>
        [HttpPost]
        [ActionPermissionFilter("marketing:promocode:add")]
        [Log(Title = "Create Promo Code", BusinessType = BusinessType.INSERT)]
        public IActionResult Create([FromBody] ErpPromoCode promoCode)
        {
            promoCode.Create_by = HttpContext.GetName();
            var response = _promoCodeService.CreatePromoCode(promoCode);
            return SUCCESS(response);
        }

        /// <summary>
        /// Update promo code - تحديث كود خصم
        /// </summary>
        [HttpPut]
        [ActionPermissionFilter("marketing:promocode:edit")]
        [Log(Title = "Update Promo Code", BusinessType = BusinessType.UPDATE)]
        public IActionResult Update([FromBody] ErpPromoCode promoCode)
        {
            promoCode.Update_by = HttpContext.GetName();
            promoCode.Update_time = DateTime.Now;
            var response = _promoCodeService.UpdatePromoCode(promoCode);
            return ToResponse(response);
        }

        /// <summary>
        /// Delete promo codes - حذف أكواد الخصم
        /// </summary>
        [HttpDelete("{ids}")]
        [ActionPermissionFilter("marketing:promocode:delete")]
        [Log(Title = "Delete Promo Codes", BusinessType = BusinessType.DELETE)]
        public IActionResult Delete(string ids)
        {
            long[] idArr = Tools.SpitLongArrary(ids);
            var response = _promoCodeService.Delete(idArr);
            return ToResponse(response);
        }

        /// <summary>
        /// Validate promo code - التحقق من صلاحية كود الخصم
        /// </summary>
        [HttpGet("validate/{code}")]
        [ActionPermissionFilter("marketing:promocode:validate")]
        public IActionResult ValidatePromoCode(string code, [FromQuery] long? customerId, [FromQuery] decimal orderAmount)
        {
            var isValid = _promoCodeService.ValidatePromoCode(code, customerId, orderAmount);
            decimal discount = 0;
            if (isValid)
            {
                discount = _promoCodeService.ApplyPromoCode(code, orderAmount);
            }
            return SUCCESS(new { Code = code, IsValid = isValid, DiscountAmount = discount, FinalAmount = orderAmount - discount });
        }

        /// <summary>
        /// Apply promo code - تطبيق كود الخصم
        /// </summary>
        [HttpPost("apply")]
        [ActionPermissionFilter("marketing:promocode:apply")]
        public IActionResult ApplyPromoCode([FromBody] ApplyPromoCodeDto dto)
        {
            var isValid = _promoCodeService.ValidatePromoCode(dto.Code, dto.CustomerId, dto.OrderAmount);
            if (!isValid)
            {
                return ToResponse(ResultCode.CUSTOM_ERROR, "كود الخصم غير صالح - Invalid promo code");
            }
            var discount = _promoCodeService.ApplyPromoCode(dto.Code, dto.OrderAmount);
            return SUCCESS(new { Code = dto.Code, DiscountAmount = discount, FinalAmount = dto.OrderAmount - discount });
        }

        /// <summary>
        /// Record promo code usage - تسجيل استخدام كود الخصم
        /// </summary>
        [HttpPost("record-usage")]
        [ActionPermissionFilter("marketing:promocode:usage")]
        [Log(Title = "Record Promo Code Usage", BusinessType = BusinessType.INSERT)]
        public IActionResult RecordUsage([FromBody] RecordUsageDto dto)
        {
            var response = _promoCodeService.RecordUsage(dto.PromoCodeId, dto.CustomerId, dto.ReferenceId);
            return ToResponse(response);
        }

        /// <summary>
        /// Get active promo codes - أكواد الخصم النشطة
        /// </summary>
        [HttpGet("active")]
        [ActionPermissionFilter("marketing:promocode:list")]
        public IActionResult GetActivePromoCodes()
        {
            var response = _promoCodeService.GetActivePromoCodes();
            return SUCCESS(response);
        }

        /// <summary>
        /// Get promo code usage report - تقرير استخدام أكواد الخصم
        /// </summary>
        [HttpGet("usage-report")]
        [ActionPermissionFilter("marketing:promocode:report")]
        public IActionResult GetUsageReport([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
        {
            var response = _promoCodeService.GetPromoCodeUsageReport(fromDate, toDate);
            return SUCCESS(response);
        }
    }

    public class ApplyPromoCodeDto
    {
        public string Code { get; set; }
        public long? CustomerId { get; set; }
        public decimal OrderAmount { get; set; }
    }

    public class RecordUsageDto
    {
        public long PromoCodeId { get; set; }
        public long CustomerId { get; set; }
        public long ReferenceId { get; set; }
    }
}

