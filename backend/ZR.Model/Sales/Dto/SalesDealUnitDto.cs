using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using ZR.Model.Contracts.Enums;

namespace ZR.Model.Sales.Dto
{
    /// <summary>
    /// Query parameters for deal units
    /// </summary>
    public class DealUnitQueryDto : PagerInfo
    {
        public long? DealId { get; set; }
        public long? ShopId { get; set; }
        public long? SeasonId { get; set; }
        public PricingType? PricingType { get; set; }
        public bool? IsConverted { get; set; }
    }

    /// <summary>
    /// Create a single deal unit (link shop to deal with pricing)
    /// </summary>
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public class CreateDealUnitDto
    {
        public long DealId { get; set; }
        public long ShopId { get; set; }
        public long SeasonId { get; set; }
        public PricingType PricingType { get; set; } = PricingType.Fixed;
        public decimal? FixedRentAmount { get; set; }
        public decimal? RevenuePercentage { get; set; }
        public decimal? MinimumGuarantee { get; set; }
        public SettlementFrequency? SettlementFrequency { get; set; }
        public string Notes { get; set; }
    }

    /// <summary>
    /// Update a deal unit (only allowed before contract conversion)
    /// </summary>
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public class UpdateDealUnitDto
    {
        public long DealUnitId { get; set; }

        /// <summary>
        /// Allow changing the linked shop (before conversion only)
        /// </summary>
        public long? ShopId { get; set; }

        public PricingType? PricingType { get; set; }
        public decimal? FixedRentAmount { get; set; }
        public decimal? RevenuePercentage { get; set; }
        public decimal? MinimumGuarantee { get; set; }
        public SettlementFrequency? SettlementFrequency { get; set; }
        public string Notes { get; set; }
    }

    /// <summary>
    /// Response DTO for deal unit with all joined display fields
    /// </summary>
    public class DealUnitResponseDto
    {
        public long DealUnitId { get; set; }
        public long DealId { get; set; }
        public string DealTitle { get; set; }
        public string BrandName { get; set; }
        public long ShopId { get; set; }
        public string ShopName { get; set; }
        public decimal? ShopArea { get; set; }
        public long SeasonId { get; set; }
        public string SeasonName { get; set; }
        public long? ZoneId { get; set; }
        public string ZoneName { get; set; }
        public long? ShopCategoryId { get; set; }
        public string ShopCategoryName { get; set; }

        // Pricing
        public PricingType PricingType { get; set; }
        public string PricingTypeName { get; set; }
        public decimal? FixedRentAmount { get; set; }
        public decimal? RevenuePercentage { get; set; }
        public decimal? MinimumGuarantee { get; set; }
        public SettlementFrequency? SettlementFrequency { get; set; }

        // Discount analytics
        public decimal? ShopBasePrice { get; set; }
        public decimal? DiscountAmount { get; set; }
        public decimal? DiscountPercentage { get; set; }

        public string Notes { get; set; }
        public bool IsConverted { get; set; }

        // Employee who handled the deal
        public string AssignedToName { get; set; }

        public string Create_by { get; set; }
        public DateTime Create_time { get; set; }
        public string Update_by { get; set; }
        public DateTime? Update_time { get; set; }
    }

    /// <summary>
    /// Bulk add multiple shops to a deal
    /// </summary>
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public class BulkCreateDealUnitsDto
    {
        public long DealId { get; set; }
        public long SeasonId { get; set; }
        public List<DealUnitItemDto> Units { get; set; } = new();
    }

    /// <summary>
    /// Single unit item inside a bulk create request
    /// </summary>
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public class DealUnitItemDto
    {
        public long ShopId { get; set; }
        public PricingType PricingType { get; set; } = PricingType.Fixed;
        public decimal? FixedRentAmount { get; set; }
        public decimal? RevenuePercentage { get; set; }
        public decimal? MinimumGuarantee { get; set; }
        public SettlementFrequency? SettlementFrequency { get; set; }
        public string Notes { get; set; }
    }

    /// <summary>
    /// Deal unit availability check result (used when changing deal status to Won)
    /// </summary>
    public class DealUnitAvailabilityDto
    {
        public long DealUnitId { get; set; }
        public long ShopId { get; set; }
        public string ShopName { get; set; }
        public string ZoneName { get; set; }
        public long SeasonId { get; set; }
        public string PricingTypeName { get; set; }
        public bool IsAvailable { get; set; }
        public string ReservedByDealTitle { get; set; }
    }

    /// <summary>
    /// Available shop for deal unit selection
    /// </summary>
    public class AvailableShopDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public long ZoneId { get; set; }
        public string ZoneName { get; set; }
        public long ShopCategory { get; set; }
        public string CategoryName { get; set; }
        public float ShopArea { get; set; }
        public float MeterPrice { get; set; }
        public float TotalPrice { get; set; }
        public long ShopStatusId { get; set; }
        public string ShopStatusName { get; set; }
        public string ImageFiles { get; set; }
        public int UnitType { get; set; }
        public int RentalType { get; set; }
        public float? RevenuePercentage { get; set; }
    }
}
