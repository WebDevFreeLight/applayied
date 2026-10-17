using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZR.Model.Contracts;
using ZR.Model.Contracts.Billing.Dto;
using ZR.Model.Contracts.Enums;
using ZR.Service.IService.Contracts.Billing;

namespace ZR.Service.Contracts.Billing
{
    /// <summary>
    /// Billing Period Generator - مولد فترات الفوترة
    /// Generates billing periods based on contract configuration
    /// </summary>
    public class BillingPeriodGenerator : IBillingPeriodGenerator
    {
        #region Main Generation Method

        /// <summary>
        /// Generate billing periods for a contract
        /// </summary>
        public async Task<PeriodGenerationResultDto> GeneratePeriodsAsync(PeriodGenerationRequestDto request)
        {
            var result = new PeriodGenerationResultDto
            {
                ContractId = request.ContractId,
                Periods = new List<BillingPeriodDto>(),
                Warnings = new List<string>()
            };

            // Validate inputs
            if (request.ContractStartDate == default)
            {
                throw new ArgumentException("Contract start date is required");
            }

            // Normalize billing day
            var billingDay = request.BillingDay;
            if (billingDay < 1 || billingDay > 31)
            {
                billingDay = 1;
                result.Warnings.Add("Invalid billing day, defaulted to 1");
            }

            // Determine effective generation range
            var effectiveStartDate = request.GenerateFromDate ?? request.ContractStartDate;
            var effectiveEndDate = request.GenerateToDate ?? request.ContractEndDate;

            // If no end date, generate up to 12 months ahead
            if (!effectiveEndDate.HasValue)
            {
                effectiveEndDate = DateTime.Today.AddMonths(12);
                result.Warnings.Add("No end date specified, generating 12 months ahead");
            }

            // Generate first period
            var firstPeriodStart = effectiveStartDate;
            var firstPeriodEnd = CalculateFirstPeriodEnd(firstPeriodStart, request.BillingFrequency, billingDay);

            // Ensure first period doesn't exceed contract end
            if (request.ContractEndDate.HasValue && firstPeriodEnd > request.ContractEndDate.Value)
            {
                firstPeriodEnd = request.ContractEndDate.Value;
            }

            var standardPeriodDays = GetStandardPeriodDays(request.BillingFrequency, firstPeriodStart);
            var actualFirstPeriodDays = (int)(firstPeriodEnd - firstPeriodStart).TotalDays + 1;
            var isFirstPeriodProRated = actualFirstPeriodDays < standardPeriodDays && request.ProrateFirstPeriod;

            var firstPeriod = new BillingPeriodDto
            {
                PeriodNumber = 1,
                PeriodStart = firstPeriodStart,
                PeriodEnd = firstPeriodEnd,
                IsProRated = isFirstPeriodProRated,
                ActualDays = actualFirstPeriodDays,
                FullPeriodDays = standardPeriodDays,
                ProRataFactor = isFirstPeriodProRated
                    ? Math.Round((decimal)actualFirstPeriodDays / standardPeriodDays, 6)
                    : 1.0m,
                DueDate = CalculateDueDate(firstPeriodStart, request.BillingFrequency),
                PaymentDueDate = CalculateDueDate(firstPeriodStart, request.BillingFrequency)
                    .AddDays(request.PaymentTermsDays),
                PeriodType = BillingPeriodType.First,
                Description = GeneratePeriodDescription(firstPeriodStart, firstPeriodEnd),
                DescriptionAr = GeneratePeriodDescriptionAr(firstPeriodStart, firstPeriodEnd)
            };

            result.Periods.Add(firstPeriod);
            result.HasProRatedFirst = isFirstPeriodProRated;

            // Generate subsequent periods
            var currentPeriodStart = firstPeriodEnd.AddDays(1);
            var periodNumber = 2;
            var maxPeriods = request.MaxPeriodsToGenerate ?? 1000;

            while (currentPeriodStart <= effectiveEndDate.Value && periodNumber <= maxPeriods)
            {
                var currentPeriodEnd = CalculatePeriodEnd(currentPeriodStart, request.BillingFrequency, billingDay);
                var isLastPeriod = currentPeriodEnd >= effectiveEndDate.Value;

                // Adjust for contract end date
                if (isLastPeriod && request.ContractEndDate.HasValue)
                {
                    currentPeriodEnd = currentPeriodEnd < request.ContractEndDate.Value
                        ? currentPeriodEnd
                        : request.ContractEndDate.Value;
                }

                var actualDays = (int)(currentPeriodEnd - currentPeriodStart).TotalDays + 1;
                var standardDays = GetStandardPeriodDays(request.BillingFrequency, currentPeriodStart);
                var isProRated = actualDays < standardDays && isLastPeriod && request.ProrateLastPeriod;

                var period = new BillingPeriodDto
                {
                    PeriodNumber = periodNumber,
                    PeriodStart = currentPeriodStart,
                    PeriodEnd = currentPeriodEnd,
                    IsProRated = isProRated,
                    ActualDays = actualDays,
                    FullPeriodDays = standardDays,
                    ProRataFactor = isProRated
                        ? Math.Round((decimal)actualDays / standardDays, 6)
                        : 1.0m,
                    DueDate = CalculateDueDate(currentPeriodStart, request.BillingFrequency),
                    PaymentDueDate = CalculateDueDate(currentPeriodStart, request.BillingFrequency)
                        .AddDays(request.PaymentTermsDays),
                    PeriodType = isLastPeriod ? BillingPeriodType.Last : BillingPeriodType.Middle,
                    Description = GeneratePeriodDescription(currentPeriodStart, currentPeriodEnd),
                    DescriptionAr = GeneratePeriodDescriptionAr(currentPeriodStart, currentPeriodEnd)
                };

                result.Periods.Add(period);

                if (isLastPeriod)
                {
                    result.HasProRatedLast = isProRated;
                    break;
                }

                currentPeriodStart = currentPeriodEnd.AddDays(1);
                periodNumber++;
            }

            // Set period types correctly
            if (result.Periods.Count == 1)
            {
                result.Periods[0].PeriodType = BillingPeriodType.Single;
            }

            // Finalize result
            result.TotalPeriodsGenerated = result.Periods.Count;
            result.FirstPeriodStart = result.Periods[0].PeriodStart;
            result.LastPeriodEnd = result.Periods[^1].PeriodEnd;

            return await Task.FromResult(result);
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Calculate the end date for the first billing period
        /// </summary>
        private DateTime CalculateFirstPeriodEnd(DateTime startDate, BillingFrequency frequency, int billingDay)
        {
            switch (frequency)
            {
                case BillingFrequency.Monthly:
                    DateTime targetDate;
                    if (startDate.Day <= billingDay)
                    {
                        // End on billing day of same month
                        var daysInMonth = DateTime.DaysInMonth(startDate.Year, startDate.Month);
                        var effectiveBillingDay = Math.Min(billingDay, daysInMonth);
                        targetDate = new DateTime(startDate.Year, startDate.Month, effectiveBillingDay);
                    }
                    else
                    {
                        // End on billing day of next month
                        var nextMonth = startDate.AddMonths(1);
                        var daysInMonth = DateTime.DaysInMonth(nextMonth.Year, nextMonth.Month);
                        var effectiveBillingDay = Math.Min(billingDay, daysInMonth);
                        targetDate = new DateTime(nextMonth.Year, nextMonth.Month, effectiveBillingDay);
                    }
                    return targetDate.AddDays(-1);

                case BillingFrequency.Quarterly:
                    var quarterEndMonth = ((int)Math.Ceiling(startDate.Month / 3.0)) * 3;
                    var quarterEnd = new DateTime(startDate.Year, quarterEndMonth,
                        DateTime.DaysInMonth(startDate.Year, quarterEndMonth));
                    if (quarterEnd <= startDate)
                    {
                        quarterEnd = quarterEnd.AddMonths(3);
                    }
                    return quarterEnd;

                case BillingFrequency.Yearly:
                    var yearEnd = startDate.AddYears(1).AddDays(-1);
                    return yearEnd;

                case BillingFrequency.SemiAnnual:
                    var halfYearEnd = startDate.AddMonths(6).AddDays(-1);
                    return halfYearEnd;

                case BillingFrequency.Weekly:
                    return startDate.AddDays(6);

                case BillingFrequency.BiWeekly:
                    return startDate.AddDays(13);

                case BillingFrequency.OneTime:
                    // For one-time, the period spans entire contract
                    return startDate; // Will be adjusted to contract end

                default:
                    // Default to monthly
                    return startDate.AddMonths(1).AddDays(-1);
            }
        }

        /// <summary>
        /// Calculate the end date for subsequent periods
        /// </summary>
        private DateTime CalculatePeriodEnd(DateTime periodStart, BillingFrequency frequency, int billingDay)
        {
            switch (frequency)
            {
                case BillingFrequency.Monthly:
                    var nextMonth = periodStart.AddMonths(1);
                    var daysInMonth = DateTime.DaysInMonth(nextMonth.Year, nextMonth.Month);
                    var effectiveBillingDay = Math.Min(billingDay, daysInMonth);
                    return new DateTime(nextMonth.Year, nextMonth.Month, effectiveBillingDay).AddDays(-1);

                case BillingFrequency.Quarterly:
                    return periodStart.AddMonths(3).AddDays(-1);

                case BillingFrequency.Yearly:
                    return periodStart.AddYears(1).AddDays(-1);

                case BillingFrequency.SemiAnnual:
                    return periodStart.AddMonths(6).AddDays(-1);

                case BillingFrequency.Weekly:
                    return periodStart.AddDays(6);

                case BillingFrequency.BiWeekly:
                    return periodStart.AddDays(13);

                case BillingFrequency.OneTime:
                    return periodStart; // Will be adjusted to contract end

                default:
                    return periodStart.AddMonths(1).AddDays(-1);
            }
        }

        /// <summary>
        /// Calculate pro-rata factor for a period
        /// </summary>
        public decimal CalculateProRataFactor(
            DateTime periodStart, DateTime periodEnd,
            DateTime actualStart, DateTime actualEnd,
            BillingFrequency frequency)
        {
            var effectiveStart = actualStart > periodStart ? actualStart : periodStart;
            var effectiveEnd = actualEnd < periodEnd ? actualEnd : periodEnd;

            if (effectiveStart > effectiveEnd)
            {
                return 0m;
            }

            var actualDays = (effectiveEnd - effectiveStart).Days + 1;
            var standardDays = GetStandardPeriodDays(frequency, periodStart);

            if (standardDays == 0) return 0m;

            var factor = (decimal)actualDays / standardDays;
            return Math.Min(Math.Round(factor, 6), 1.0m);
        }

        /// <summary>
        /// Get standard period length in days
        /// </summary>
        public int GetStandardPeriodDays(BillingFrequency frequency, DateTime referenceDate)
        {
            switch (frequency)
            {
                case BillingFrequency.OneTime:
                    return 1; // OneTime billing - single period
                case BillingFrequency.Weekly:
                    return 7;
                case BillingFrequency.BiWeekly:
                    return 14;
                case BillingFrequency.Monthly:
                    return DateTime.DaysInMonth(referenceDate.Year, referenceDate.Month);
                case BillingFrequency.Quarterly:
                    return 91; // Average quarter
                case BillingFrequency.SemiAnnual:
                    return 182; // Half year average
                case BillingFrequency.Yearly:
                    return DateTime.IsLeapYear(referenceDate.Year) ? 366 : 365;
                default:
                    return 30; // Default monthly
            }
        }

        /// <summary>
        /// Calculate next period dates
        /// </summary>
        public (DateTime start, DateTime end) CalculateNextPeriod(
            DateTime currentPeriodEnd, BillingFrequency frequency, int billingDay)
        {
            var nextStart = currentPeriodEnd.AddDays(1);
            var nextEnd = CalculatePeriodEnd(nextStart, frequency, billingDay);
            return (nextStart, nextEnd);
        }

        /// <summary>
        /// Calculate due date for a period
        /// </summary>
        private DateTime CalculateDueDate(DateTime periodStart, BillingFrequency frequency)
        {
            // Default: Invoice at start of period (advance billing)
            return periodStart;
        }

        /// <summary>
        /// Generate period description in English
        /// </summary>
        private string GeneratePeriodDescription(DateTime periodStart, DateTime periodEnd)
        {
            if (periodStart.Month == periodEnd.Month && periodStart.Year == periodEnd.Year)
            {
                return periodStart.ToString("MMMM yyyy");
            }
            else if (periodStart.Year == periodEnd.Year)
            {
                return $"{periodStart:MMM} - {periodEnd:MMM yyyy}";
            }
            else
            {
                return $"{periodStart:MMM yyyy} - {periodEnd:MMM yyyy}";
            }
        }

        /// <summary>
        /// Generate period description in Arabic
        /// </summary>
        private string GeneratePeriodDescriptionAr(DateTime periodStart, DateTime periodEnd)
        {
            var culture = new System.Globalization.CultureInfo("ar-SA");

            if (periodStart.Month == periodEnd.Month && periodStart.Year == periodEnd.Year)
            {
                return periodStart.ToString("MMMM yyyy", culture);
            }
            else if (periodStart.Year == periodEnd.Year)
            {
                return $"{periodStart.ToString("MMM", culture)} - {periodEnd.ToString("MMM yyyy", culture)}";
            }
            else
            {
                return $"{periodStart.ToString("MMM yyyy", culture)} - {periodEnd.ToString("MMM yyyy", culture)}";
            }
        }

        #endregion
    }
}
