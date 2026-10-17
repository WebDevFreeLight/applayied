using Infrastructure.Attribute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZR.Model.Sales;
using ZR.Model.Sales.Dto;
using ZR.Model.Boulevard;
using ZR.Model.Customers.Leads;
using ZR.Model.HR.EmployeeCore;
using ZR.Model.Contracts.Enums;
using ZR.Repository;
using ZR.Service.Sales.ISalesService;

namespace ZR.Service.Sales
{
    /// <summary>
    /// Sales Deal Unit Service - خدمة وحدات صفقة المبيعات
    /// Manages the link between deals and boulevard shops with per-unit pricing.
    /// </summary>
    [AppService(ServiceType = typeof(ISalesDealUnitService), ServiceLifetime = LifeTime.Transient)]
    public class SalesDealUnitService : BaseService<SalesDealUnit>, ISalesDealUnitService
    {
        private readonly ISalesDealService _dealService;

        public SalesDealUnitService(ISalesDealService dealService)
        {
            _dealService = dealService;
        }

        public async Task<PagedInfo<DealUnitResponseDto>> GetListAsync(DealUnitQueryDto query)
        {
            var now = DateTime.Now;
            // Join order matters: lead must come before emp because emp's join references lead.AssignedTo
            var pageQuery = Context
                .Queryable<SalesDealUnit, BlvBoulevardShops, BlvBoulevardSeasonZones, BlvBoulevardShopCategory, BlvBoulevardSeasons, SalesDeal, BpLead, HrEmployeeCore, HrPersonName>(
                    (du, shop, zone, cat, season, deal, lead, emp, name) => new JoinQueryInfos(
                        JoinType.Left, du.ShopId == shop.Id,
                        JoinType.Left, du.ZoneId == zone.Id,
                        JoinType.Left, du.ShopCategoryId == cat.Id,
                        JoinType.Left, du.SeasonId == season.Id,
                        JoinType.Left, du.DealId == deal.DealId,
                        JoinType.Left, deal.LeadId == lead.LeadId,
                        JoinType.Left, emp.EmployeeId == SqlFunc.IIF(lead.AssignedTo > 0, lead.AssignedTo, deal.AssignedTo),
                        JoinType.Left, emp.PersonId == name.PersonId &&
                                       name.IsPrimary == true &&
                                       name.EffectiveFrom <= now &&
                                       (name.EffectiveTo == null || name.EffectiveTo > now)))
                .WhereIF(query.DealId.HasValue, (du, shop, zone, cat, season, deal, lead, emp, name) => du.DealId == query.DealId.Value)
                .WhereIF(query.ShopId.HasValue, (du, shop, zone, cat, season, deal, lead, emp, name) => du.ShopId == query.ShopId.Value)
                .WhereIF(query.SeasonId.HasValue, (du, shop, zone, cat, season, deal, lead, emp, name) => du.SeasonId == query.SeasonId.Value)
                .WhereIF(query.PricingType.HasValue, (du, shop, zone, cat, season, deal, lead, emp, name) => du.PricingType == query.PricingType.Value)
                .WhereIF(query.IsConverted.HasValue, (du, shop, zone, cat, season, deal, lead, emp, name) => du.IsConverted == query.IsConverted.Value)
                .OrderByDescending((du, shop, zone, cat, season, deal, lead, emp, name) => du.Create_time)
                .Select((du, shop, zone, cat, season, deal, lead, emp, name) => new DealUnitResponseDto
                {
                    DealUnitId = du.DealUnitId,
                    DealId = du.DealId,
                    DealTitle = deal.DealTitle,
                    BrandName = lead.BrandName,
                    ShopId = du.ShopId,
                    ShopName = du.ShopName,
                    ShopArea = du.ShopArea,
                    SeasonId = du.SeasonId,
                    SeasonName = season.Name,
                    ZoneId = du.ZoneId,
                    ZoneName = zone.Name,
                    ShopCategoryId = du.ShopCategoryId,
                    ShopCategoryName = cat.Name,
                    PricingType = du.PricingType,
                    PricingTypeName = du.PricingType.ToString(),
                    FixedRentAmount = du.FixedRentAmount,
                    RevenuePercentage = du.RevenuePercentage,
                    MinimumGuarantee = du.MinimumGuarantee,
                    SettlementFrequency = du.SettlementFrequency,
                    ShopBasePrice = du.ShopBasePrice,
                    DiscountAmount = du.DiscountAmount,
                    DiscountPercentage = du.DiscountPercentage,
                    Notes = du.Notes,
                    IsConverted = du.IsConverted,
                    AssignedToName = name.FullNameAr ?? (name.GivenName + " " + name.FamilyName),

                    Create_by = du.Create_by,
                    Create_time = du.Create_time,
                    Update_by = du.Update_by,
                    Update_time = du.Update_time
                });

            return await pageQuery.ToPageAsync(query);
        }

        public async Task<List<DealUnitResponseDto>> GetByDealIdAsync(long dealId)
        {
            return await Context
                .Queryable<SalesDealUnit, BlvBoulevardShops, BlvBoulevardSeasonZones, BlvBoulevardShopCategory, BlvBoulevardSeasons>(
                    (du, shop, zone, cat, season) => new JoinQueryInfos(
                        JoinType.Left, du.ShopId == shop.Id,
                        JoinType.Left, du.ZoneId == zone.Id,
                        JoinType.Left, du.ShopCategoryId == cat.Id,
                        JoinType.Left, du.SeasonId == season.Id))
                .Where((du, shop, zone, cat, season) => du.DealId == dealId)
                .OrderBy((du, shop, zone, cat, season) => du.Create_time)
                .Select((du, shop, zone, cat, season) => new DealUnitResponseDto
                {
                    DealUnitId = du.DealUnitId,
                    DealId = du.DealId,
                    ShopId = du.ShopId,
                    ShopName = du.ShopName,
                    ShopArea = du.ShopArea,
                    SeasonId = du.SeasonId,
                    SeasonName = season.Name,
                    ZoneId = du.ZoneId,
                    ZoneName = zone.Name,
                    ShopCategoryId = du.ShopCategoryId,
                    ShopCategoryName = cat.Name,
                    PricingType = du.PricingType,
                    PricingTypeName = du.PricingType.ToString(),
                    FixedRentAmount = du.FixedRentAmount,
                    RevenuePercentage = du.RevenuePercentage,
                    MinimumGuarantee = du.MinimumGuarantee,
                    SettlementFrequency = du.SettlementFrequency,
                    ShopBasePrice = du.ShopBasePrice,
                    DiscountAmount = du.DiscountAmount,
                    DiscountPercentage = du.DiscountPercentage,
                    Notes = du.Notes,
                    IsConverted = du.IsConverted,
                    Create_by = du.Create_by,
                    Create_time = du.Create_time,
                    Update_by = du.Update_by,
                    Update_time = du.Update_time
                })
                .ToListAsync();
        }

        public async Task<SalesDealUnit> GetByIdAsync(long dealUnitId)
        {
            return await GetFirstAsync(du => du.DealUnitId == dealUnitId);
        }

        public async Task<SalesDealUnit> CreateAsync(CreateDealUnitDto dto, string createdBy)
        {
            // Validate deal exists and is not converted
            var deal = await Context.Queryable<SalesDeal>()
                .Where(d => d.DealId == dto.DealId)
                .FirstAsync();
            if (deal == null)
                throw new Exception("Deal not found");
            if (deal.DealStatus == "Won" || deal.DealStatus == "Lost")
                throw new Exception("Cannot add units to a closed deal");

            // Validate shop exists
            var shop = await Context.Queryable<BlvBoulevardShops>()
                .Where(s => s.Id == dto.ShopId)
                .FirstAsync();
            if (shop == null)
                throw new Exception("Shop not found");

            // Check shop not already linked to another active deal
            if (await IsShopLinkedToActiveDealAsync(dto.ShopId, dto.DealId))
                throw new Exception($"Shop {shop.Name} is already linked to another active deal");

            // Check shop not already in this deal
            var existsInDeal = await Context.Queryable<SalesDealUnit>()
                .Where(du => du.DealId == dto.DealId && du.ShopId == dto.ShopId)
                .AnyAsync();
            if (existsInDeal)
                throw new Exception($"Shop {shop.Name} is already added to this deal");

            // Validate pricing integrity
            ValidatePricing(dto.PricingType, dto.FixedRentAmount, dto.RevenuePercentage);

            // Build entity with snapshot data from shop
            var shopBasePrice = (decimal)shop.TotalPrice;
            var entity = new SalesDealUnit
            {
                DealUnitId = SnowFlakeSingle.Instance.NextId(),
                DealId = dto.DealId,
                ShopId = dto.ShopId,
                SeasonId = dto.SeasonId,
                PricingType = dto.PricingType,
                FixedRentAmount = dto.FixedRentAmount,
                RevenuePercentage = dto.RevenuePercentage,
                MinimumGuarantee = dto.MinimumGuarantee,
                SettlementFrequency = dto.SettlementFrequency ?? SettlementFrequency.Monthly,
                Notes = dto.Notes,
                // Snapshot from shop master data
                ShopName = shop.Name,
                ShopArea = (decimal)shop.ShopArea,
                ZoneId = shop.ZoneId,
                ShopCategoryId = shop.ShopCategory,
                ShopBasePrice = shopBasePrice,
                IsConverted = false,
                Create_by = createdBy,
                Create_time = DateTime.Now
            };

            // Calculate discount (analytics only, does NOT block)
            if (dto.PricingType == PricingType.Fixed && dto.FixedRentAmount.HasValue && shopBasePrice > 0)
            {
                entity.DiscountAmount = shopBasePrice - dto.FixedRentAmount.Value;
                entity.DiscountPercentage = Math.Round((shopBasePrice - dto.FixedRentAmount.Value) / shopBasePrice * 100, 2);
            }

            await Context.Insertable(entity).ExecuteCommandAsync();

            // Recalculate deal summary from units
            await _dealService.RecalculateDealSummaryAsync(dto.DealId);

            return entity;
        }

        public async Task<List<SalesDealUnit>> BulkCreateAsync(BulkCreateDealUnitsDto dto, string createdBy)
        {
            var results = new List<SalesDealUnit>();
            foreach (var item in dto.Units)
            {
                var createDto = new CreateDealUnitDto
                {
                    DealId = dto.DealId,
                    SeasonId = dto.SeasonId,
                    ShopId = item.ShopId,
                    PricingType = item.PricingType,
                    FixedRentAmount = item.FixedRentAmount,
                    RevenuePercentage = item.RevenuePercentage,
                    MinimumGuarantee = item.MinimumGuarantee,
                    SettlementFrequency = item.SettlementFrequency,
                    Notes = item.Notes
                };
                // CreateAsync already calls RecalculateDealSummaryAsync individually
                results.Add(await CreateAsync(createDto, createdBy));
            }
            return results;
        }

        public async Task<int> UpdateAsync(UpdateDealUnitDto dto, string updatedBy)
        {
            var entity = await GetFirstAsync(du => du.DealUnitId == dto.DealUnitId);
            if (entity == null)
                throw new Exception("Deal unit not found");
            if (entity.IsConverted)
                throw new Exception("Cannot modify a deal unit that has already been converted to a contract");

            // If changing shop, validate new shop
            if (dto.ShopId.HasValue && dto.ShopId.Value != entity.ShopId)
            {
                var newShop = await Context.Queryable<BlvBoulevardShops>()
                    .Where(s => s.Id == dto.ShopId.Value)
                    .FirstAsync();
                if (newShop == null)
                    throw new Exception("New shop not found");

                if (await IsShopLinkedToActiveDealAsync(dto.ShopId.Value, entity.DealId))
                    throw new Exception($"Shop {newShop.Name} is already linked to another active deal");

                entity.ShopId = dto.ShopId.Value;
                entity.ShopName = newShop.Name;
                entity.ShopArea = (decimal)newShop.ShopArea;
                entity.ZoneId = newShop.ZoneId;
                entity.ShopCategoryId = newShop.ShopCategory;
                entity.ShopBasePrice = (decimal)newShop.TotalPrice;
            }

            if (dto.PricingType.HasValue) entity.PricingType = dto.PricingType.Value;
            if (dto.FixedRentAmount.HasValue) entity.FixedRentAmount = dto.FixedRentAmount;
            if (dto.RevenuePercentage.HasValue) entity.RevenuePercentage = dto.RevenuePercentage;
            if (dto.MinimumGuarantee.HasValue) entity.MinimumGuarantee = dto.MinimumGuarantee;
            if (dto.SettlementFrequency.HasValue) entity.SettlementFrequency = dto.SettlementFrequency;
            if (dto.Notes != null) entity.Notes = dto.Notes;

            // Validate pricing integrity
            ValidatePricing(entity.PricingType, entity.FixedRentAmount, entity.RevenuePercentage);

            // Recalculate discount
            if (entity.PricingType == PricingType.Fixed && entity.FixedRentAmount.HasValue && entity.ShopBasePrice.HasValue && entity.ShopBasePrice.Value > 0)
            {
                entity.DiscountAmount = entity.ShopBasePrice.Value - entity.FixedRentAmount.Value;
                entity.DiscountPercentage = Math.Round((entity.ShopBasePrice.Value - entity.FixedRentAmount.Value) / entity.ShopBasePrice.Value * 100, 2);
            }
            else
            {
                entity.DiscountAmount = null;
                entity.DiscountPercentage = null;
            }

            entity.Update_by = updatedBy;
            entity.Update_time = DateTime.Now;

            var result = await UpdateAsync(entity) ? 1 : 0;

            // Recalculate deal summary from units
            await _dealService.RecalculateDealSummaryAsync(entity.DealId);

            return result;
        }

        public async Task<int> DeleteAsync(long dealUnitId)
        {
            var entity = await GetFirstAsync(du => du.DealUnitId == dealUnitId);
            if (entity == null)
                throw new Exception("Deal unit not found");
            if (entity.IsConverted)
                throw new Exception("Cannot delete a deal unit that has already been converted to a contract");

            var dealId = entity.DealId;
            var result = await Context.Deleteable<SalesDealUnit>()
                .Where(du => du.DealUnitId == dealUnitId)
                .ExecuteCommandAsync();

            // Recalculate deal summary from remaining units
            await _dealService.RecalculateDealSummaryAsync(dealId);

            return result;
        }

        public async Task<bool> IsShopLinkedToActiveDealAsync(long shopId, long? excludeDealId = null)
        {
            var query = Context.Queryable<SalesDealUnit, SalesDeal>(
                (du, deal) => new JoinQueryInfos(JoinType.Inner, du.DealId == deal.DealId))
                .Where((du, deal) => du.ShopId == shopId)
                .Where((du, deal) => deal.DealStatus != "Lost" && deal.DealStatus != "Won");

            if (excludeDealId.HasValue)
            {
                query = query.Where((du, deal) => du.DealId != excludeDealId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<List<AvailableShopDto>> GetAvailableShopsAsync(long seasonId, long? zoneId = null)
        {
            // Get shops in this season that are not linked to active deals
            var linkedShopIds = await Context.Queryable<SalesDealUnit, SalesDeal>(
                (du, deal) => new JoinQueryInfos(JoinType.Inner, du.DealId == deal.DealId))
                .Where((du, deal) => du.SeasonId == seasonId)
                .Where((du, deal) => deal.DealStatus != "Lost")
                .Select((du, deal) => du.ShopId)
                .ToListAsync();

            var query = Context.Queryable<BlvBoulevardShops, BlvBoulevardSeasonsShops, BlvBoulevardSeasonZones, BlvBoulevardShopCategory, BlvBoulevardShopStatus>(
                (shop, ss, zone, cat, status) => new JoinQueryInfos(
                    JoinType.Inner, shop.Id == ss.ShopId,
                    JoinType.Left, shop.ZoneId == zone.Id,
                    JoinType.Left, shop.ShopCategory == cat.Id,
                    JoinType.Left, shop.ShopStatus == status.Id))
                .Where((shop, ss, zone, cat, status) => ss.SeasonId == seasonId);

            if (zoneId.HasValue)
            {
                query = query.Where((shop, ss, zone, cat, status) => shop.ZoneId == zoneId.Value);
            }

            if (linkedShopIds.Any())
            {
                query = query.Where((shop, ss, zone, cat, status) => !linkedShopIds.Contains(shop.Id));
            }

            return await query
                .Select((shop, ss, zone, cat, status) => new AvailableShopDto
                {
                    Id = shop.Id,
                    Name = shop.Name,
                    ZoneId = shop.ZoneId,
                    ZoneName = zone.Name,
                    ShopCategory = shop.ShopCategory,
                    CategoryName = cat.Name,
                    ShopArea = shop.ShopArea,
                    MeterPrice = shop.MeterPrice,
                    TotalPrice = shop.TotalPrice,
                    ShopStatusId = shop.ShopStatus,
                    ShopStatusName = status.Name,
                    ImageFiles = shop.ImageFiles,
                    UnitType = shop.UnitType,
                    RentalType = shop.RentalType,
                    RevenuePercentage = shop.RevenuePercentage
                })
                .ToListAsync();
        }

        public async Task<List<DealUnitAvailabilityDto>> CheckDealUnitsAvailabilityAsync(long dealId)
        {
            // Get all units for this deal
            var units = await Context.Queryable<SalesDealUnit>()
                .Where(du => du.DealId == dealId)
                .ToListAsync();

            var result = new List<DealUnitAvailabilityDto>();

            foreach (var unit in units)
            {
                var dto = new DealUnitAvailabilityDto
                {
                    DealUnitId = unit.DealUnitId,
                    ShopId = unit.ShopId,
                    ShopName = unit.ShopName,
                    ZoneName = null,
                    SeasonId = unit.SeasonId,
                    PricingTypeName = unit.PricingType.ToString(),
                    IsAvailable = true,
                    ReservedByDealTitle = null
                };

                // Check if shop is reserved (ShopStatus == 2) by looking at the shop master
                var shop = await Context.Queryable<BlvBoulevardShops>()
                    .Where(s => s.Id == unit.ShopId)
                    .FirstAsync();

                if (shop != null && shop.ShopStatus == 2)
                {
                    // Shop is reserved — find which deal reserved it
                    var reservingDeal = await Context.Queryable<SalesDealUnit, SalesDeal>(
                        (du, deal) => new JoinQueryInfos(JoinType.Inner, du.DealId == deal.DealId))
                        .Where((du, deal) => du.ShopId == unit.ShopId && du.IsConverted == true && du.DealId != dealId)
                        .Select((du, deal) => deal.DealTitle)
                        .FirstAsync();

                    dto.IsAvailable = false;
                    dto.ReservedByDealTitle = reservingDeal ?? "Another deal";
                }

                // Also check if shop is linked to another active (Open) deal
                if (dto.IsAvailable && await IsShopLinkedToActiveDealAsync(unit.ShopId, dealId))
                {
                    var linkedDealTitle = await Context.Queryable<SalesDealUnit, SalesDeal>(
                        (du, deal) => new JoinQueryInfos(JoinType.Inner, du.DealId == deal.DealId))
                        .Where((du, deal) => du.ShopId == unit.ShopId && deal.DealStatus != "Lost" && deal.DealStatus != "Won" && du.DealId != dealId)
                        .Select((du, deal) => deal.DealTitle)
                        .FirstAsync();

                    dto.IsAvailable = false;
                    dto.ReservedByDealTitle = linkedDealTitle ?? "Another active deal";
                }

                // Get zone name from join
                if (unit.ZoneId.HasValue)
                {
                    var zone = await Context.Queryable<BlvBoulevardSeasonZones>()
                        .Where(z => z.Id == unit.ZoneId.Value)
                        .FirstAsync();
                    dto.ZoneName = zone?.Name;
                }

                result.Add(dto);
            }

            return result;
        }

        #region Private Helpers

        private static void ValidatePricing(PricingType pricingType, decimal? fixedRentAmount, decimal? revenuePercentage)
        {
            switch (pricingType)
            {
                case PricingType.Fixed:
                    if (!fixedRentAmount.HasValue || fixedRentAmount.Value <= 0)
                        throw new Exception("Fixed rent amount is required and must be greater than 0 for Fixed pricing type");
                    break;
                case PricingType.RevenueShare:
                    if (!revenuePercentage.HasValue || revenuePercentage.Value <= 0 || revenuePercentage.Value > 100)
                        throw new Exception("Revenue percentage must be between 0 and 100 for RevenueShare pricing type");
                    break;
            }
        }

        #endregion
    }
}
