using Microsoft.AspNetCore.Mvc;
using ZR.Admin.WebApi.Filters;
using ZR.Model.Sales.Dto;
using ZR.Service.Sales.ISalesService;
using Infrastructure;
using Infrastructure.Model;

namespace ZR.Admin.WebApi.Controllers.Sales
{
    /// <summary>
    /// Sales Deal Units - وحدات صفقة المبيعات
    /// Manages the link between deals and boulevard shops with per-unit pricing.
    /// </summary>
    [Verify]
    [Route("api/sales/deal-units")]
    public class SalesDealUnitController : BaseController
    {
        private readonly ISalesDealUnitService _dealUnitService;

        public SalesDealUnitController(ISalesDealUnitService dealUnitService)
        {
            _dealUnitService = dealUnitService;
        }

        private TokenModel GetCurrentUser()
        {
            return JwtUtil.GetLoginUser(HttpContext);
        }

        /// <summary>
        /// Get deal units with pagination and filtering
        /// </summary>
        [HttpGet("list")]
        [ActionPermissionFilter("sales:deals:list")]
        public async Task<IActionResult> GetList([FromQuery] DealUnitQueryDto dto)
        {
            try
            {
                var result = await _dealUnitService.GetListAsync(dto);
                return SUCCESS(result);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Get all units for a specific deal
        /// </summary>
        [HttpGet("deal/{dealId}")]
        [ActionPermissionFilter("sales:deals:view")]
        public async Task<IActionResult> GetByDealId(long dealId)
        {
            return SUCCESS(await _dealUnitService.GetByDealIdAsync(dealId));
        }

        /// <summary>
        /// Get single deal unit by ID
        /// </summary>
        [HttpGet("{dealUnitId}")]
        [ActionPermissionFilter("sales:deals:view")]
        public async Task<IActionResult> GetById(long dealUnitId)
        {
            return SUCCESS(await _dealUnitService.GetByIdAsync(dealUnitId));
        }

        /// <summary>
        /// Add a shop to a deal with pricing terms
        /// </summary>
        [HttpPost("add")]
        [ActionPermissionFilter("sales:deals:edit")]
        public async Task<IActionResult> Create([FromBody] CreateDealUnitDto dto)
        {
            try
            {
                if (dto == null || dto.DealId == 0 || dto.ShopId == 0)
                    return ToResponse(ResultCode.FAIL, "Invalid request: dealId and shopId are required");
                var currentUser = GetCurrentUser();
                var result = await _dealUnitService.CreateAsync(dto, currentUser?.UserName);
                return SUCCESS(result);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Bulk add multiple shops to a deal
        /// </summary>
        [HttpPost("bulk-add")]
        [ActionPermissionFilter("sales:deals:edit")]
        public async Task<IActionResult> BulkCreate([FromBody] BulkCreateDealUnitsDto dto)
        {
            try
            {
                var currentUser = GetCurrentUser();
                var result = await _dealUnitService.BulkCreateAsync(dto, currentUser?.UserName);
                return SUCCESS(result, $"{result.Count} shops added to deal successfully");
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Update a deal unit (pricing, shop change — only before contract conversion)
        /// </summary>
        [HttpPost("update")]
        [ActionPermissionFilter("sales:deals:edit")]
        public async Task<IActionResult> Update([FromBody] UpdateDealUnitDto dto)
        {
            try
            {
                var currentUser = GetCurrentUser();
                var result = await _dealUnitService.UpdateAsync(dto, currentUser?.UserName);
                return SUCCESS(result, "Deal unit updated successfully");
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Remove a shop from a deal (only before contract conversion)
        /// </summary>
        [HttpPost("delete/{dealUnitId}")]
        [ActionPermissionFilter("sales:deals:edit")]
        public async Task<IActionResult> Delete(long dealUnitId)
        {
            try
            {
                var result = await _dealUnitService.DeleteAsync(dealUnitId);
                return SUCCESS(result, "Deal unit removed successfully");
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Check if a shop is already linked to an active deal
        /// </summary>
        [HttpGet("shop-available/{shopId}")]
        [ActionPermissionFilter("sales:deals:list")]
        public async Task<IActionResult> IsShopAvailable(long shopId)
        {
            var isLinked = await _dealUnitService.IsShopLinkedToActiveDealAsync(shopId);
            return SUCCESS(new { shopId, isAvailable = !isLinked });
        }

        /// <summary>
        /// Check availability of all units in a deal (for Won status validation)
        /// Returns each unit with isAvailable flag and which deal reserved it
        /// </summary>
        [HttpGet("check-availability/{dealId}")]
        [ActionPermissionFilter("sales:deals:list")]
        public async Task<IActionResult> CheckDealUnitsAvailability(long dealId)
        {
            var result = await _dealUnitService.CheckDealUnitsAvailabilityAsync(dealId);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get available shops for a season (excluding already-linked shops)
        /// </summary>
        [HttpGet("available-shops/{seasonId}")]
        [ActionPermissionFilter("sales:deals:list")]
        public async Task<IActionResult> GetAvailableShops(long seasonId, [FromQuery] long? zoneId = null)
        {
            return SUCCESS(await _dealUnitService.GetAvailableShopsAsync(seasonId, zoneId));
        }
    }
}
