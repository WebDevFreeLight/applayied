using System;
using System.Threading.Tasks;
using ZR.Model.Contracts.Billing.Dto;
using ZR.Model.Contracts.Enums;

namespace ZR.Service.IService.Contracts.Billing
{
    /// <summary>
    /// IBillingPeriodGenerator - Period Generation Interface
    /// مولد فترات الفوترة
    /// </summary>
    public interface IBillingPeriodGenerator
    {
        /// <summary>
        /// Generate billing periods for a contract
        /// </summary>
        Task<PeriodGenerationResultDto> GeneratePeriodsAsync(PeriodGenerationRequestDto request);

        /// <summary>
        /// Calculate pro-rata factor for a period
        /// </summary>
        decimal CalculateProRataFactor(
            DateTime periodStart, DateTime periodEnd,
            DateTime actualStart, DateTime actualEnd,
            BillingFrequency frequency);

        /// <summary>
        /// Get standard period length in days
        /// </summary>
        int GetStandardPeriodDays(BillingFrequency frequency, DateTime referenceDate);

        /// <summary>
        /// Calculate next period dates
        /// </summary>
        (DateTime start, DateTime end) CalculateNextPeriod(
            DateTime currentPeriodEnd, BillingFrequency frequency, int billingDay);
    }
}
