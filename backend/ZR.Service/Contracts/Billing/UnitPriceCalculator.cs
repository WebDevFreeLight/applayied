using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SqlSugar;
using ZR.Model.Contracts;
using ZR.Model.Contracts.Billing.Dto;
using ZR.Model.Contracts.Enums;
using ZR.Service.IService.Contracts.Billing;

namespace ZR.Service.Contracts.Billing
{
    /// <summary>
    /// Unit Price Calculator - حاسبة أسعار الوحدات
    /// Calculates prices for contract units with support for area-based, fixed, and override pricing
    /// </summary>
    public class UnitPriceCalculator : IUnitPriceCalculator
    {
        private readonly ISqlSugarClient _db;

        public UnitPriceCalculator(ISqlSugarClient db)
        {
            _db = db;
        }

        #region Main Calculation Methods

        /// <summary>
        /// Calculate prices for all active units in a contract
        /// </summary>
        public async Task<AggregateUnitPriceResultDto> CalculateUnitPricesAsync(UnitPriceCalculationRequestDto request)
        {
            var result = new AggregateUnitPriceResultDto
            {
                ContractId = request.ContractId,
                PeriodStart = request.PeriodStart,
                PeriodEnd = request.PeriodEnd,
                UnitPrices = new List<UnitPriceResultDto>()
            };

            // Get contract with financial extension
            var contract = await _db.Queryable<ErpContract>()
                .Includes(c => c.Financial)
                .FirstAsync(c => c.ContractId == request.ContractId);

            if (contract == null)
            {
                throw new ArgumentException($"Contract {request.ContractId} not found");
            }

            var currency = contract.Financial?.Currency ?? "SAR";
            result.Currency = currency;

            // Get units to process
            List<ErpContractUnit> units;
            if (request.UnitIds != null && request.UnitIds.Any())
            {
                units = await _db.Queryable<ErpContractUnit>()
                    .Where(u => request.UnitIds.Contains(u.UnitId))
                    .ToListAsync();
            }
            else
            {
                units = await _db.Queryable<ErpContractUnit>()
                    .Where(u => u.ContractId == request.ContractId && u.IsActive)
                    .ToListAsync();
            }

            decimal totalBasePrice = 0;
            decimal totalArea = 0;

            // Calculate price for each unit
            foreach (var unit in units)
            {
                var unitResult = await CalculateUnitPriceInternal(
                    unit, contract, request.PeriodStart, request.PeriodEnd, currency);

                if (unitResult.IsActiveForPeriod && unitResult.EffectivePrice > 0)
                {
                    totalBasePrice += unitResult.EffectivePrice;
                    if (unitResult.Area.HasValue)
                    {
                        totalArea += unitResult.Area.Value;
                    }
                }

                result.UnitPrices.Add(unitResult);
            }

            result.TotalBasePrice = totalBasePrice;
            result.ActiveUnitCount = result.UnitPrices.Count(u => u.IsActiveForPeriod);
            result.TotalArea = totalArea;

            return result;
        }

        /// <summary>
        /// Calculate price for a single unit
        /// </summary>
        public async Task<UnitPriceResultDto> CalculateSingleUnitPriceAsync(
            Guid unitId, DateTime periodStart, DateTime periodEnd)
        {
            var unit = await _db.Queryable<ErpContractUnit>()
                .FirstAsync(u => u.UnitId == unitId);

            if (unit == null)
            {
                throw new ArgumentException($"Unit {unitId} not found");
            }

            var contract = await _db.Queryable<ErpContract>()
                .Includes(c => c.Financial)
                .FirstAsync(c => c.ContractId == unit.ContractId);

            var currency = contract?.Financial?.Currency ?? "SAR";

            return await CalculateUnitPriceInternal(unit, contract, periodStart, periodEnd, currency);
        }

        /// <summary>
        /// Calculate area-based price
        /// </summary>
        public decimal CalculateAreaBasedPrice(decimal area, decimal pricePerArea)
        {
            if (area <= 0 || pricePerArea <= 0)
            {
                return 0;
            }
            return Math.Round(area * pricePerArea, 4);
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Internal method to calculate price for a unit
        /// </summary>
        private async Task<UnitPriceResultDto> CalculateUnitPriceInternal(
            ErpContractUnit unit,
            ErpContract contract,
            DateTime periodStart,
            DateTime periodEnd,
            string currency)
        {
            var result = new UnitPriceResultDto
            {
                UnitId = unit.UnitId,
                UnitCode = unit.UnitCode,
                UnitName = unit.UnitName,
                UnitType = unit.UnitType,
                Currency = currency
            };

            // Determine unit's effective dates
            var unitEffectiveStart = unit.StartDate ?? contract.StartDate;
            var unitEffectiveEnd = unit.EndDate ?? contract.EndDate;

            // Check if unit is active for this period
            if (unitEffectiveStart > periodEnd ||
                (unitEffectiveEnd.HasValue && unitEffectiveEnd.Value < periodStart))
            {
                result.IsActiveForPeriod = false;
                result.EffectivePrice = 0;
                result.CalculationDetails = "Unit not active in this period";
                return result;
            }

            result.IsActiveForPeriod = true;

            // Determine pricing method and calculate base price
            if (unit.CalculatedPrice.HasValue && unit.CalculatedPrice.Value > 0)
            {
                // Use pre-calculated/override price
                result.PriceMethod = UnitPriceMethod.Override;
                result.CalculatedPrice = unit.CalculatedPrice;
                result.EffectivePrice = unit.CalculatedPrice.Value;
                result.CalculationDetails = "Using override calculated price";
            }
            else if (unit.Area.HasValue && unit.Area.Value > 0 &&
                     unit.PricePerArea.HasValue && unit.PricePerArea.Value > 0)
            {
                // Area-based calculation
                result.PriceMethod = UnitPriceMethod.AreaBased;
                result.Area = unit.Area;
                result.PricePerArea = unit.PricePerArea;
                result.CalculatedPrice = CalculateAreaBasedPrice(unit.Area.Value, unit.PricePerArea.Value);
                result.EffectivePrice = result.CalculatedPrice.Value;
                result.CalculationDetails = $"Area: {unit.Area:N2} × Rate: {unit.PricePerArea:N4} = {result.CalculatedPrice:N4}";
            }
            else if (unit.BasePrice.HasValue && unit.BasePrice.Value > 0)
            {
                // Fixed base price
                result.PriceMethod = UnitPriceMethod.FixedPrice;
                result.BasePrice = unit.BasePrice;
                result.EffectivePrice = unit.BasePrice.Value;
                result.CalculationDetails = "Using fixed base price";
            }
            else
            {
                // No price defined
                result.PriceMethod = UnitPriceMethod.FixedPrice;
                result.EffectivePrice = 0;
                result.CalculationDetails = "WARNING: No price defined for unit";
            }

            // Calculate pro-rating if unit is only partially active in period
            var activeStart = unitEffectiveStart > periodStart ? unitEffectiveStart : periodStart;
            var activeEnd = unitEffectiveEnd.HasValue && unitEffectiveEnd.Value < periodEnd
                ? unitEffectiveEnd.Value
                : periodEnd;

            var periodDays = (periodEnd - periodStart).Days + 1;
            var activeDays = (activeEnd - activeStart).Days + 1;

            if (activeDays < periodDays && activeDays > 0)
            {
                // Unit is only partially active - pro-rate
                var proRataFactor = (decimal)activeDays / periodDays;
                result.DaysActiveInPeriod = activeDays;
                result.ProRatedPrice = Math.Round(result.EffectivePrice * proRataFactor, 4);
                result.CalculationDetails += $" | Pro-rated: {activeDays} days of {periodDays} = {result.ProRatedPrice:N4}";
                result.EffectivePrice = result.ProRatedPrice.Value;
            }
            else
            {
                result.DaysActiveInPeriod = periodDays;
            }

            return await Task.FromResult(result);
        }

        #endregion
    }
}
