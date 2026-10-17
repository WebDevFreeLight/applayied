using System;
using System.Collections.Generic;
using ZR.Model.Contracts.Enums;

namespace ZR.Model.Contracts.Billing.Dto
{
    /// <summary>
    /// Unit Price Calculation Request DTO
    /// </summary>
    public class UnitPriceCalculationRequestDto
    {
        public Guid ContractId { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public List<Guid> UnitIds { get; set; } // Null = all active units
    }

    /// <summary>
    /// Unit Price Result DTO
    /// </summary>
    public class UnitPriceResultDto
    {
        public Guid UnitId { get; set; }
        public string UnitCode { get; set; }
        public string UnitName { get; set; }
        public ContractUnitType UnitType { get; set; }

        /// <summary>Calculation method used</summary>
        public UnitPriceMethod PriceMethod { get; set; }

        /// <summary>Base price (fixed or calculated)</summary>
        public decimal? BasePrice { get; set; }

        /// <summary>Area (if area-based)</summary>
        public decimal? Area { get; set; }

        /// <summary>Price per area unit</summary>
        public decimal? PricePerArea { get; set; }

        /// <summary>Calculated price (Area × PricePerArea)</summary>
        public decimal? CalculatedPrice { get; set; }

        /// <summary>Final effective price</summary>
        public decimal EffectivePrice { get; set; }

        /// <summary>Currency</summary>
        public string Currency { get; set; }

        /// <summary>Calculation details</summary>
        public string CalculationDetails { get; set; }

        /// <summary>Whether unit is active for this period</summary>
        public bool IsActiveForPeriod { get; set; }

        /// <summary>Days active in period (for pro-rating)</summary>
        public int? DaysActiveInPeriod { get; set; }

        /// <summary>Pro-rated price (if partially active)</summary>
        public decimal? ProRatedPrice { get; set; }
    }

    /// <summary>
    /// Unit Price Method Enum
    /// </summary>
    public enum UnitPriceMethod
    {
        FixedPrice = 0,
        AreaBased = 1,
        Calculated = 2,
        Override = 3
    }

    /// <summary>
    /// Aggregate Unit Price Result DTO
    /// </summary>
    public class AggregateUnitPriceResultDto
    {
        public Guid ContractId { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public List<UnitPriceResultDto> UnitPrices { get; set; } = new();
        public decimal TotalBasePrice { get; set; }
        public int ActiveUnitCount { get; set; }
        public decimal TotalArea { get; set; }
        public string Currency { get; set; }
    }
}
