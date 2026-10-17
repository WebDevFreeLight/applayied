using System;
using System.Linq;
using Infrastructure.Attribute;
using SqlSugar;
using ZR.Model.Finance.Configuration;
using ZR.Repository.Finance.Configuration;

namespace ZR.Service.Finance.Configuration
{
    /// <summary>
    /// Fiscal Year Helper - مساعد السنة المالية
    /// Centralized service for fiscal year calculations based on ERP Settings
    /// Eliminates hardcoded assumptions about calendar year (January-December)
    /// </summary>
    [AppService(ServiceType = typeof(IFiscalYearHelper), ServiceLifetime = LifeTime.Transient)]
    public class FiscalYearHelper : IFiscalYearHelper
    {
        private readonly IERPSettingsRepository _settingsRepository;
        private ERPSettings _cachedSettings = null;
        private DateTime _cacheTimestamp = DateTime.MinValue;
        private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(5);

        public FiscalYearHelper(IERPSettingsRepository settingsRepository)
        {
            _settingsRepository = settingsRepository;
        }

        /// <summary>
        /// Get ERP Settings with caching (5 minute cache)
        /// </summary>
        private ERPSettings GetERPSettings(long? companyId = null)
        {
            // Check if cache is still valid
            if (_cachedSettings != null && DateTime.Now - _cacheTimestamp < _cacheExpiration)
            {
                return _cachedSettings;
            }

            // Refresh cache
            _cachedSettings = _settingsRepository.GetByCompanyId(companyId);
            _cacheTimestamp = DateTime.Now;

            return _cachedSettings;
        }

        /// <summary>
        /// Get fiscal year start month (1-12) from ERP Settings
        /// Returns 1 (January) if not configured
        /// </summary>
        public int GetFiscalYearStartMonth(long? companyId = null)
        {
            var settings = GetERPSettings(companyId);
            return settings?.FiscalYearStartMonth ?? 1; // Default to January
        }

        /// <summary>
        /// Get number of fiscal periods from ERP Settings
        /// Returns 12 (monthly) if not configured
        /// </summary>
        public int GetNumberOfFiscalPeriods(long? companyId = null)
        {
            var settings = GetERPSettings(companyId);
            return settings?.NumberOfFiscalPeriods ?? 12; // Default to monthly
        }

        /// <summary>
        /// Get fiscal year start date for a given fiscal year number
        /// Example: FY 2025 with April start month → April 1, 2025
        /// </summary>
        public DateTime GetFiscalYearStartDate(int fiscalYear, long? companyId = null)
        {
            int startMonth = GetFiscalYearStartMonth(companyId);
            return new DateTime(fiscalYear, startMonth, 1);
        }

        /// <summary>
        /// Get fiscal year end date for a given fiscal year number
        /// Example: FY 2025 with April start month → March 31, 2026
        /// </summary>
        public DateTime GetFiscalYearEndDate(int fiscalYear, long? companyId = null)
        {
            int startMonth = GetFiscalYearStartMonth(companyId);

            // End date is one day before the next fiscal year starts
            DateTime nextYearStart = new DateTime(fiscalYear + 1, startMonth, 1);
            return nextYearStart.AddDays(-1);
        }

        /// <summary>
        /// Get fiscal year number from a given date
        /// Example: March 15, 2026 with April start month → FY 2025 (not 2026!)
        /// </summary>
        public int GetFiscalYearFromDate(DateTime date, long? companyId = null)
        {
            int startMonth = GetFiscalYearStartMonth(companyId);

            // If date month is before fiscal year start month, it belongs to previous fiscal year
            // Example: March 2026 with April start = FY 2025
            if (date.Month < startMonth)
            {
                return date.Year - 1;
            }
            else
            {
                return date.Year;
            }
        }

        /// <summary>
        /// Get fiscal period number from a given date
        /// Takes into account the fiscal year start month and number of periods
        /// </summary>
        public int GetFiscalPeriodFromDate(DateTime date, long? companyId = null)
        {
            int startMonth = GetFiscalYearStartMonth(companyId);
            int numberOfPeriods = GetNumberOfFiscalPeriods(companyId);

            // Calculate months since fiscal year start
            int monthsSinceFYStart;
            if (date.Month >= startMonth)
            {
                monthsSinceFYStart = date.Month - startMonth;
            }
            else
            {
                monthsSinceFYStart = (12 - startMonth) + date.Month;
            }

            // Convert to period number based on number of periods
            if (numberOfPeriods == 12) // Monthly
            {
                return monthsSinceFYStart + 1; // Period 1-12
            }
            else if (numberOfPeriods == 4) // Quarterly
            {
                return (monthsSinceFYStart / 3) + 1; // Period 1-4
            }
            else if (numberOfPeriods == 52) // Weekly (approximate)
            {
                // Calculate week of fiscal year
                var fiscalYearStart = GetFiscalYearStartDate(GetFiscalYearFromDate(date, companyId), companyId);
                int daysSinceStart = (date - fiscalYearStart).Days;
                return Math.Min((daysSinceStart / 7) + 1, 52); // Period 1-52
            }
            else if (numberOfPeriods == 6) // Bi-monthly
            {
                return (monthsSinceFYStart / 2) + 1; // Period 1-6
            }
            else // Default to monthly for any other configuration
            {
                return monthsSinceFYStart + 1;
            }
        }

        /// <summary>
        /// Check if a date falls within a specific fiscal year
        /// </summary>
        public bool IsDateInFiscalYear(DateTime date, int fiscalYear, long? companyId = null)
        {
            var startDate = GetFiscalYearStartDate(fiscalYear, companyId);
            var endDate = GetFiscalYearEndDate(fiscalYear, companyId);

            return date >= startDate && date <= endDate;
        }

        /// <summary>
        /// Get fiscal year date range
        /// </summary>
        public (DateTime StartDate, DateTime EndDate) GetFiscalYearDateRange(int fiscalYear, long? companyId = null)
        {
            return (
                GetFiscalYearStartDate(fiscalYear, companyId),
                GetFiscalYearEndDate(fiscalYear, companyId)
            );
        }

        /// <summary>
        /// Get period start date for a given fiscal year and period number
        /// </summary>
        public DateTime GetPeriodStartDate(int fiscalYear, int periodNumber, long? companyId = null)
        {
            int numberOfPeriods = GetNumberOfFiscalPeriods(companyId);
            var fiscalYearStart = GetFiscalYearStartDate(fiscalYear, companyId);

            if (numberOfPeriods == 12) // Monthly
            {
                return fiscalYearStart.AddMonths(periodNumber - 1);
            }
            else if (numberOfPeriods == 4) // Quarterly
            {
                return fiscalYearStart.AddMonths((periodNumber - 1) * 3);
            }
            else if (numberOfPeriods == 52) // Weekly
            {
                return fiscalYearStart.AddDays((periodNumber - 1) * 7);
            }
            else if (numberOfPeriods == 6) // Bi-monthly
            {
                return fiscalYearStart.AddMonths((periodNumber - 1) * 2);
            }
            else // Default to monthly
            {
                return fiscalYearStart.AddMonths(periodNumber - 1);
            }
        }

        /// <summary>
        /// Get period end date for a given fiscal year and period number
        /// </summary>
        public DateTime GetPeriodEndDate(int fiscalYear, int periodNumber, long? companyId = null)
        {
            int numberOfPeriods = GetNumberOfFiscalPeriods(companyId);

            // If this is the last period, return fiscal year end
            if (periodNumber == numberOfPeriods)
            {
                return GetFiscalYearEndDate(fiscalYear, companyId);
            }

            // Otherwise, return one day before next period starts
            var nextPeriodStart = GetPeriodStartDate(fiscalYear, periodNumber + 1, companyId);
            return nextPeriodStart.AddDays(-1);
        }

        /// <summary>
        /// Get period date range
        /// </summary>
        public (DateTime StartDate, DateTime EndDate) GetPeriodDateRange(int fiscalYear, int periodNumber, long? companyId = null)
        {
            return (
                GetPeriodStartDate(fiscalYear, periodNumber, companyId),
                GetPeriodEndDate(fiscalYear, periodNumber, companyId)
            );
        }

        /// <summary>
        /// Get current fiscal year based on today's date
        /// </summary>
        public int GetCurrentFiscalYear(long? companyId = null)
        {
            return GetFiscalYearFromDate(DateTime.Today, companyId);
        }

        /// <summary>
        /// Get current fiscal period based on today's date
        /// </summary>
        public int GetCurrentFiscalPeriod(long? companyId = null)
        {
            return GetFiscalPeriodFromDate(DateTime.Today, companyId);
        }

        /// <summary>
        /// Validate if period number is valid for configured number of periods
        /// </summary>
        public bool IsValidPeriodNumber(int periodNumber, long? companyId = null)
        {
            int numberOfPeriods = GetNumberOfFiscalPeriods(companyId);
            return periodNumber >= 1 && periodNumber <= numberOfPeriods;
        }

        /// <summary>
        /// Clear cached settings (useful after updating ERP Settings)
        /// </summary>
        public void ClearCache()
        {
            _cachedSettings = null;
            _cacheTimestamp = DateTime.MinValue;
        }
    }

    /// <summary>
    /// Interface for Fiscal Year Helper
    /// </summary>
    public interface IFiscalYearHelper
    {
        int GetFiscalYearStartMonth(long? companyId = null);
        int GetNumberOfFiscalPeriods(long? companyId = null);
        DateTime GetFiscalYearStartDate(int fiscalYear, long? companyId = null);
        DateTime GetFiscalYearEndDate(int fiscalYear, long? companyId = null);
        int GetFiscalYearFromDate(DateTime date, long? companyId = null);
        int GetFiscalPeriodFromDate(DateTime date, long? companyId = null);
        bool IsDateInFiscalYear(DateTime date, int fiscalYear, long? companyId = null);
        (DateTime StartDate, DateTime EndDate) GetFiscalYearDateRange(int fiscalYear, long? companyId = null);
        DateTime GetPeriodStartDate(int fiscalYear, int periodNumber, long? companyId = null);
        DateTime GetPeriodEndDate(int fiscalYear, int periodNumber, long? companyId = null);
        (DateTime StartDate, DateTime EndDate) GetPeriodDateRange(int fiscalYear, int periodNumber, long? companyId = null);
        int GetCurrentFiscalYear(long? companyId = null);
        int GetCurrentFiscalPeriod(long? companyId = null);
        bool IsValidPeriodNumber(int periodNumber, long? companyId = null);
        void ClearCache();
    }
}
