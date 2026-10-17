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
    /// Promotion Management Controller - ادارة العروض الترويجية
    /// </summary>
    [Verify]
    [Route("api/marketing/promotions")]
    [ApiExplorerSettings(GroupName = "marketing")]
    public class PromotionController : BaseController
    {
        private readonly IPromotionService _promotionService;

        public PromotionController(IPromotionService promotionService)
        {
            _promotionService = promotionService;
        }

        /// <summary>
        /// Get promotion list
        /// </summary>
        [HttpGet("list")]
        [ActionPermissionFilter("marketing:promotion:list")]
        public async Task<IActionResult> GetPromotionList([FromQuery] PromotionQueryDto dto)
        {
            var predicate = Expressionable.Create<ErpPromotion>();

            predicate = predicate.AndIF(!string.IsNullOrEmpty(dto.PromotionCode),
                it => it.PromotionCode.Contains(dto.PromotionCode));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(dto.PromotionName),
                it => it.PromotionName.Contains(dto.PromotionName) || it.PromotionNameAr.Contains(dto.PromotionName));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(dto.PromotionType),
                it => it.PromotionType == dto.PromotionType);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(dto.Status),
                it => it.Status == dto.Status);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(dto.TargetAudience),
                it => it.TargetAudience == dto.TargetAudience);
            predicate = predicate.AndIF(dto.StartDate.HasValue,
                it => it.StartDate >= dto.StartDate);
            predicate = predicate.AndIF(dto.EndDate.HasValue,
                it => it.EndDate <= dto.EndDate);
            predicate = predicate.AndIF(dto.IsActive.HasValue,
                it => it.IsActive == dto.IsActive);

            var response = await _promotionService.GetPageListAsync(predicate.ToExpression(), dto);
            return SUCCESS(response);
        }

        /// <summary>
        /// Get active promotions
        /// </summary>
        [HttpGet("active")]
        [ActionPermissionFilter("marketing:promotion:list")]
        public async Task<IActionResult> GetActivePromotions()
        {
            var promotions = await _promotionService.GetActivePromotionsAsync();
            return SUCCESS(promotions);
        }

        /// <summary>
        /// Get applicable promotions for member
        /// </summary>
        [HttpGet("applicable")]
        [ActionPermissionFilter("marketing:promotion:list")]
        public async Task<IActionResult> GetApplicablePromotions([FromQuery] long memberId, [FromQuery] string itemType, [FromQuery] decimal amount)
        {
            var promotions = await _promotionService.GetApplicablePromotionsAsync(memberId, itemType, amount);
            return SUCCESS(promotions);
        }

        /// <summary>
        /// Get promotion statistics (aggregate)
        /// </summary>
        [HttpGet("statistics")]
        [ActionPermissionFilter("marketing:promotion:list")]
        public async Task<IActionResult> GetPromotionStatistics()
        {
            var stats = await _promotionService.GetStatisticsAsync();
            return SUCCESS(stats);
        }

        /// <summary>
        /// Get promotion details
        /// </summary>
        [HttpGet("{id}")]
        [ActionPermissionFilter("marketing:promotion:query")]
        public async Task<IActionResult> GetPromotion(long id)
        {
            var promotion = await _promotionService.GetByIdAsync(id);
            if (promotion == null)
            {
                return ToResponse(ResultCode.FAIL, "Promotion not found");
            }
            return SUCCESS(promotion);
        }

        /// <summary>
        /// Get promotion by code
        /// </summary>
        [HttpGet("code/{code}")]
        [ActionPermissionFilter("marketing:promotion:query")]
        public async Task<IActionResult> GetPromotionByCode(string code)
        {
            var promotion = await _promotionService.GetByCodeAsync(code);
            if (promotion == null)
            {
                return ToResponse(ResultCode.FAIL, "Promotion not found");
            }
            return SUCCESS(promotion);
        }

        /// <summary>
        /// Add new promotion
        /// </summary>
        [HttpPost]
        [ActionPermissionFilter("marketing:promotion:add")]
        [Log(Title = "Promotion Management", BusinessType = BusinessType.INSERT)]
        public async Task<IActionResult> AddPromotion([FromBody] PromotionDto dto)
        {
            var promotion = dto.Adapt<ErpPromotion>();
            promotion.Create_by = HttpContext.GetName();
            promotion.Create_time = DateTime.Now;

            var result = await _promotionService.AddAsync(promotion);
            return SUCCESS(result);
        }

        /// <summary>
        /// Update promotion
        /// </summary>
        [HttpPut]
        [ActionPermissionFilter("marketing:promotion:edit")]
        [Log(Title = "Promotion Management", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> UpdatePromotion([FromBody] PromotionDto dto)
        {
            var promotion = dto.Adapt<ErpPromotion>();
            promotion.Update_by = HttpContext.GetName();
            promotion.Update_time = DateTime.Now;

            var result = await _promotionService.UpdateAsync(promotion);
            return SUCCESS(result);
        }

        /// <summary>
        /// Delete promotions
        /// </summary>
        [HttpDelete("{ids}")]
        [ActionPermissionFilter("marketing:promotion:delete")]
        [Log(Title = "Promotion Management", BusinessType = BusinessType.DELETE)]
        public async Task<IActionResult> DeletePromotion(string ids)
        {
            var idArray = Tools.SplitAndConvert<long>(ids);
            var result = await _promotionService.DeleteAsync(idArray);
            return SUCCESS(result);
        }

        /// <summary>
        /// Validate promotion code
        /// </summary>
        [HttpPost("validate")]
        [ActionPermissionFilter("marketing:promotion:query")]
        public async Task<IActionResult> ValidatePromotion([FromBody] PromotionValidateRequest request)
        {
            var result = await _promotionService.ValidatePromotionAsync(request.Code, request.MemberId, request.Amount);
            return SUCCESS(result);
        }

        /// <summary>
        /// Calculate discount
        /// </summary>
        [HttpGet("{id}/calculate")]
        [ActionPermissionFilter("marketing:promotion:query")]
        public async Task<IActionResult> CalculateDiscount(long id, [FromQuery] decimal amount)
        {
            var discount = await _promotionService.CalculateDiscountAsync(id, amount);
            return SUCCESS(new { OriginalAmount = amount, Discount = discount, FinalAmount = amount - discount });
        }

        /// <summary>
        /// Record promotion usage
        /// </summary>
        [HttpPost("record-usage")]
        [ActionPermissionFilter("marketing:promotion:edit")]
        [Log(Title = "Promotion Usage", BusinessType = BusinessType.INSERT)]
        public async Task<IActionResult> RecordUsage([FromBody] PromotionUsageRequest request)
        {
            await _promotionService.RecordUsageAsync(request.PromotionId, request.MemberId, request.InvoiceId, request.DiscountApplied);
            return SUCCESS("Usage recorded successfully");
        }

        /// <summary>
        /// Get promotion analytics
        /// </summary>
        [HttpGet("{id}/analytics")]
        [ActionPermissionFilter("marketing:promotion:query")]
        public async Task<IActionResult> GetPromotionAnalytics(long id)
        {
            var analytics = await _promotionService.GetPromotionAnalyticsAsync(id);
            return SUCCESS(analytics);
        }

        /// <summary>
        /// Update promotion status
        /// </summary>
        [HttpPut("{id}/status")]
        [ActionPermissionFilter("marketing:promotion:edit")]
        [Log(Title = "Promotion Status Update", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> UpdateStatus(long id, [FromQuery] string status)
        {
            var result = await _promotionService.UpdateStatusAsync(id, status);
            return SUCCESS(result);
        }

        /// <summary>
        /// Export promotions
        /// </summary>
        [HttpGet("export")]
        [ActionPermissionFilter("marketing:promotion:export")]
        [Log(Title = "Promotion Management", BusinessType = BusinessType.EXPORT)]
        public async Task<IActionResult> ExportPromotions([FromQuery] PromotionQueryDto dto)
        {
            var predicate = Expressionable.Create<ErpPromotion>();

            predicate = predicate.AndIF(!string.IsNullOrEmpty(dto.PromotionCode),
                it => it.PromotionCode.Contains(dto.PromotionCode));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(dto.Status),
                it => it.Status == dto.Status);

            var list = await _promotionService.GetListAsync(predicate.ToExpression());
            var result = ExportExcelMini(list, "promotions", "Promotion List");
            return ExportExcel(result.Item2, result.Item1);
        }
    }
}

