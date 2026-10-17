using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using ZR.Admin.WebApi.Filters;
using ZR.Model.Boulevard.Dto;
using ZR.Service.Boulevard.IBoulevardService;

namespace ZR.Admin.WebApi.Controllers.Dashboard
{
    /// <summary>
    /// User Dashboard - aggregated endpoints for authenticated users
    /// No specific module permissions required, just authentication
    /// </summary>
    [Verify]
    [Route("api/dashboard")]
    [ApiExplorerSettings(GroupName = "dashboard")]
    [ApiController]
    public class UserDashboardController : BaseController
    {
        private readonly IBlvBoulevardSeasonsService _seasonsService;
        private readonly IBlvBoulevardShopsService _shopsService;
        private readonly IBlvBoulevardSeasonsShopsService _seasonShopsService;

        public UserDashboardController(
            IBlvBoulevardSeasonsService seasonsService,
            IBlvBoulevardShopsService shopsService,
            IBlvBoulevardSeasonsShopsService seasonShopsService)
        {
            _seasonsService = seasonsService;
            _shopsService = shopsService;
            _seasonShopsService = seasonShopsService;
        }

        /// <summary>
        /// Get rental progress for all active seasons
        /// Returns aggregated data (no specific boulevard permissions needed)
        /// </summary>
        [HttpGet("rental-progress")]
        public IActionResult GetRentalProgress()
        {
            try
            {
                // 1. Get all seasons
                var allSeasons = _seasonsService.GetList(new BlvBoulevardSeasonsQueryDto
                {
                    PageNum = 1,
                    PageSize = 100
                });

                // 2. Filter active seasons (now between StartDate and EndDate)
                var now = DateTime.Now;
                var activeSeasons = allSeasons.Result
                    .Where(s => s.StartDate.HasValue && s.EndDate.HasValue
                        && now >= s.StartDate.Value && now <= s.EndDate.Value)
                    .ToList();

                if (!activeSeasons.Any())
                {
                    return SUCCESS(new { seasons = Array.Empty<object>(), overallTotal = 0, overallRented = 0 });
                }

                // 3. Get all season-shop assignments (unpaged)
                var allSeasonShops = _seasonShopsService.GetListAll(new BlvBoulevardSeasonsShopsQueryDto());

                // 4. Get all shops with status
                var allShops = _shopsService.GetList(new BlvBoulevardShopsQueryDto
                {
                    PageNum = 1,
                    PageSize = 9999
                });

                // Build shop lookup
                var shopMap = allShops.Result.ToDictionary(s => s.Id ?? 0, s => s);

                // 5. Aggregate per active season
                var seasonResults = activeSeasons.Select(season =>
                {
                    var seasonShopIds = allSeasonShops
                        .Where(ss => ss.SeasonId == (season.Id ?? 0))
                        .Select(ss => ss.ShopId)
                        .ToList();

                    var totalShops = seasonShopIds.Count;

                    // Status ID 2 = Rented
                    var rentedShops = seasonShopIds
                        .Where(shopId => shopMap.ContainsKey(shopId) && shopMap[shopId].ShopStatus == 2)
                        .Count();

                    return new
                    {
                        seasonId = season.Id,
                        name = season.Name ?? $"Season {season.Id}",
                        startDate = season.StartDate,
                        endDate = season.EndDate,
                        totalShops,
                        rentedShops,
                        percentage = totalShops > 0 ? Math.Round((double)rentedShops / totalShops * 100) : 0
                    };
                }).ToList();

                var overallTotal = seasonResults.Sum(s => s.totalShops);
                var overallRented = seasonResults.Sum(s => s.rentedShops);

                return SUCCESS(new
                {
                    seasons = seasonResults,
                    overallTotal,
                    overallRented,
                    overallPercentage = overallTotal > 0 ? Math.Round((double)overallRented / overallTotal * 100) : 0
                });
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.CUSTOM_ERROR, $"Failed to load rental progress: {ex.Message}");
            }
        }
    }
}
