using System;
using System.Threading.Tasks;
using ZR.Model.Contracts.Billing.Dto;

namespace ZR.Service.IService.Contracts.Billing
{
    /// <summary>
    /// IUnitPriceCalculator - Unit Price Calculation Interface
    /// حاسبة أسعار الوحدات
    /// </summary>
    public interface IUnitPriceCalculator
    {
        /// <summary>
        /// Calculate prices for all active units in a contract
        /// </summary>
        Task<AggregateUnitPriceResultDto> CalculateUnitPricesAsync(UnitPriceCalculationRequestDto request);

        /// <summary>
        /// Calculate price for a single unit
        /// </summary>
        Task<UnitPriceResultDto> CalculateSingleUnitPriceAsync(
            Guid unitId, DateTime periodStart, DateTime periodEnd);

        /// <summary>
        /// Calculate area-based price
        /// </summary>
        decimal CalculateAreaBasedPrice(decimal area, decimal pricePerArea);
    }
}
