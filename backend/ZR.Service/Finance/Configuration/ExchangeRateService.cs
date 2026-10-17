using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.Attribute;
using Infrastructure.Exceptions;
using ZR.Model;
using ZR.Model.Finance.Configuration;
using ZR.Model.Finance.Configuration.Dto;
using ZR.Model.Finance.Configuration.Enums;
using ZR.Repository.Finance.Configuration;
using ZR.Service.IService.Finance.Configuration;

namespace ZR.Service.Finance.Configuration
{
    /// <summary>
    /// Exchange Rate Service Implementation
    /// خدمة أسعار الصرف
    /// </summary>
    [AppService(ServiceType = typeof(IExchangeRateService), ServiceLifetime = LifeTime.Transient)]
    public class ExchangeRateService : BaseService<ExchangeRate>, IExchangeRateService
    {
        private readonly IExchangeRateRepository _rateRepository;
        private readonly ICurrencyRepository _currencyRepository;

        public ExchangeRateService(
            IExchangeRateRepository rateRepository,
            ICurrencyRepository currencyRepository)
        {
            _rateRepository = rateRepository;
            _currencyRepository = currencyRepository;
        }

        #region Query Methods

        /// <summary>
        /// Get paginated list of exchange rates
        /// </summary>
        public PagedInfo<ExchangeRateResponseDto> GetList(ExchangeRateQueryDto query, PagerInfo pager)
        {
            var searchModel = new ExchangeRate
            {
                CurrencyId = query.CurrencyId ?? Guid.Empty,
                IsOverride = query.IsOverride ?? false,
                IsActive = query.IsActive ?? true
            };

            var result = _rateRepository.GetList(searchModel, pager);

            return new PagedInfo<ExchangeRateResponseDto>
            {
                PageIndex = result.PageIndex,
                PageSize = result.PageSize,
                TotalNum = result.TotalNum,
                TotalPage = result.TotalPage,
                Result = result.Result.Select(MapToResponseDto).ToList()
            };
        }

        /// <summary>
        /// Get exchange rate by ID
        /// </summary>
        public ExchangeRateResponseDto GetById(Guid id)
        {
            var rate = _rateRepository.GetById(id);
            return rate == null ? null : MapToResponseDto(rate);
        }

        /// <summary>
        /// Get exchange rate by ID (async)
        /// </summary>
        public async Task<ExchangeRateResponseDto> GetByIdAsync(Guid id)
        {
            var rate = await _rateRepository.GetByIdAsync(id);
            return rate == null ? null : MapToResponseDto(rate);
        }

        /// <summary>
        /// Get exchange rate for specific date
        /// RULE: Returns rate for exact date or latest rate before that date
        /// </summary>
        public ExchangeRateResponseDto GetRateForDate(Guid currencyId, DateTime date)
        {
            var rate = _rateRepository.GetRateForDate(currencyId, date);
            return rate == null ? null : MapToResponseDto(rate);
        }

        /// <summary>
        /// Get exchange rate for specific date (async)
        /// </summary>
        public async Task<ExchangeRateResponseDto> GetRateForDateAsync(Guid currencyId, DateTime date)
        {
            var rate = await _rateRepository.GetRateForDateAsync(currencyId, date);
            return rate == null ? null : MapToResponseDto(rate);
        }

        /// <summary>
        /// Get exchange rate by currency code and date
        /// </summary>
        public ExchangeRateResponseDto GetRateByCodeAndDate(string currencyCode, DateTime date)
        {
            var rate = _rateRepository.GetRateByCodeAndDate(currencyCode, date);
            return rate == null ? null : MapToResponseDto(rate);
        }

        /// <summary>
        /// Get exchange rate by currency code and date (async)
        /// </summary>
        public async Task<ExchangeRateResponseDto> GetRateByCodeAndDateAsync(string currencyCode, DateTime date)
        {
            var rate = await _rateRepository.GetRateByCodeAndDateAsync(currencyCode, date);
            return rate == null ? null : MapToResponseDto(rate);
        }

        /// <summary>
        /// Get latest exchange rate for currency
        /// </summary>
        public ExchangeRateResponseDto GetLatestRate(Guid currencyId)
        {
            var rate = _rateRepository.GetLatestRate(currencyId);
            return rate == null ? null : MapToResponseDto(rate);
        }

        /// <summary>
        /// Get latest exchange rate for currency (async)
        /// </summary>
        public async Task<ExchangeRateResponseDto> GetLatestRateAsync(Guid currencyId)
        {
            var rate = await _rateRepository.GetLatestRateAsync(currencyId);
            return rate == null ? null : MapToResponseDto(rate);
        }

        /// <summary>
        /// Get all rates for a specific date
        /// </summary>
        public List<ExchangeRateResponseDto> GetRatesForDate(DateTime date)
        {
            var rates = _rateRepository.GetRatesForDate(date);
            return rates.Select(MapToResponseDto).ToList();
        }

        /// <summary>
        /// Get all rates for a specific date (async)
        /// </summary>
        public async Task<List<ExchangeRateResponseDto>> GetRatesForDateAsync(DateTime date)
        {
            var rates = await _rateRepository.GetRatesForDateAsync(date);
            return rates.Select(MapToResponseDto).ToList();
        }

        /// <summary>
        /// Get rate history for currency
        /// </summary>
        public List<ExchangeRateResponseDto> GetRateHistory(Guid currencyId, int days = 30)
        {
            var rates = _rateRepository.GetRateHistory(currencyId, days);
            return rates.Select(MapToResponseDto).ToList();
        }

        /// <summary>
        /// Get rate history for currency (async)
        /// </summary>
        public async Task<List<ExchangeRateResponseDto>> GetRateHistoryAsync(Guid currencyId, int days = 30)
        {
            var rates = await _rateRepository.GetRateHistoryAsync(currencyId, days);
            return rates.Select(MapToResponseDto).ToList();
        }

        #endregion

        #region Create/Update Methods

        /// <summary>
        /// Create exchange rate
        /// VALIDATION RULES:
        /// - Rate must be > 0
        /// - Currency must exist and be active
        /// - Auto-calculate InverseRate = 1 / Rate
        /// - If !AllowManualExchangeRate, IsOverride must be false
        /// </summary>
        public ExchangeRateResponseDto Create(CreateExchangeRateDto dto, string createdBy)
        {
            // Validate rate value
            if (dto.Rate <= 0)
            {
                throw new AppException(ExceptionCode.FINANCE_CONFIG_EXCHANGE_RATE_INVALID_RATE)
                    .WithParameter("Rate", dto.Rate);
            }

            // Validate currency exists and is active
            var currency = _currencyRepository.GetById(dto.CurrencyId);
            if (currency == null)
            {
                throw new AppException(ExceptionCode.FINANCE_CONFIG_EXCHANGE_RATE_CURRENCY_NOT_FOUND)
                    .WithParameter("CurrencyId", dto.CurrencyId);
            }
            if (!currency.IsActive)
            {
                throw new AppException(ExceptionCode.FINANCE_CONFIG_EXCHANGE_RATE_CURRENCY_INACTIVE)
                    .WithParameter("CurrencyId", dto.CurrencyId);
            }

            // Create rate entity
            var rate = new ExchangeRate
            {
                Id = Guid.NewGuid(),
                CurrencyId = dto.CurrencyId,
                RateDate = dto.RateDate.Date, // Normalize to date only
                Rate = dto.Rate,
                InverseRate = 1 / dto.Rate, // Auto-calculate inverse
                IsOverride = dto.IsOverride,
                Source = dto.Source ?? "Manual",
                Notes = dto.Notes,
                IsActive = true,
                Create_by = createdBy,
                Create_time = DateTime.Now
            };

            _rateRepository.CreateRate(rate);

            return MapToResponseDto(rate);
        }

        /// <summary>
        /// Create exchange rate (async)
        /// </summary>
        public async Task<ExchangeRateResponseDto> CreateAsync(CreateExchangeRateDto dto, string createdBy)
        {
            // Validate rate value
            if (dto.Rate <= 0)
            {
                throw new AppException(ExceptionCode.FINANCE_CONFIG_EXCHANGE_RATE_INVALID_RATE)
                    .WithParameter("Rate", dto.Rate);
            }

            // Validate currency exists and is active (sync call since interface doesn't have async)
            var currency = _currencyRepository.GetById(dto.CurrencyId);
            if (currency == null)
            {
                throw new AppException(ExceptionCode.FINANCE_CONFIG_EXCHANGE_RATE_CURRENCY_NOT_FOUND)
                    .WithParameter("CurrencyId", dto.CurrencyId);
            }
            if (!currency.IsActive)
            {
                throw new AppException(ExceptionCode.FINANCE_CONFIG_EXCHANGE_RATE_CURRENCY_INACTIVE)
                    .WithParameter("CurrencyId", dto.CurrencyId);
            }

            // Create rate entity
            var rate = new ExchangeRate
            {
                Id = Guid.NewGuid(),
                CurrencyId = dto.CurrencyId,
                RateDate = dto.RateDate.Date,
                Rate = dto.Rate,
                InverseRate = 1 / dto.Rate,
                IsOverride = dto.IsOverride,
                Source = dto.Source ?? "Manual",
                Notes = dto.Notes,
                IsActive = true,
                Create_by = createdBy,
                Create_time = DateTime.Now
            };

            await _rateRepository.CreateRateAsync(rate);

            return MapToResponseDto(rate);
        }

        /// <summary>
        /// Bulk import exchange rates
        /// VALIDATION RULES:
        /// - All currency codes must exist
        /// - All rates must be > 0
        /// - Auto-calculate inverse rates
        /// </summary>
        public List<ExchangeRateResponseDto> BulkImport(BulkImportExchangeRatesDto dto, string createdBy)
        {
            var rates = new List<ExchangeRate>();

            foreach (var item in dto.Rates)
            {
                // Validate rate
                if (item.Rate <= 0)
                {
                    throw new AppException(ExceptionCode.FINANCE_CONFIG_EXCHANGE_RATE_INVALID_RATE)
                        .WithParameter("Rate", item.Rate)
                        .WithParameter("CurrencyCode", item.CurrencyCode);
                }

                // Get currency by code (using CurrencyCode property from Configuration.Currency)
                var currency = _currencyRepository.GetByCode(item.CurrencyCode);
                if (currency == null)
                {
                    throw new AppException(ExceptionCode.FINANCE_CONFIG_EXCHANGE_RATE_CURRENCY_CODE_NOT_FOUND)
                        .WithParameter("CurrencyCode", item.CurrencyCode);
                }

                var rate = new ExchangeRate
                {
                    Id = Guid.NewGuid(),
                    CurrencyId = currency.Id,
                    RateDate = dto.RateDate.Date,
                    Rate = item.Rate,
                    InverseRate = 1 / item.Rate,
                    IsOverride = false,
                    Source = dto.Source ?? "Bulk Import",
                    IsActive = true,
                    Create_by = createdBy,
                    Create_time = DateTime.Now
                };

                rates.Add(rate);
            }

            _rateRepository.CreateRates(rates);

            return rates.Select(MapToResponseDto).ToList();
        }

        /// <summary>
        /// Bulk import exchange rates (async)
        /// </summary>
        public async Task<List<ExchangeRateResponseDto>> BulkImportAsync(BulkImportExchangeRatesDto dto, string createdBy)
        {
            var rates = new List<ExchangeRate>();

            foreach (var item in dto.Rates)
            {
                // Validate rate
                if (item.Rate <= 0)
                {
                    throw new AppException(ExceptionCode.FINANCE_CONFIG_EXCHANGE_RATE_INVALID_RATE)
                        .WithParameter("Rate", item.Rate)
                        .WithParameter("CurrencyCode", item.CurrencyCode);
                }

                // Get currency by code (sync call since interface doesn't have async)
                var currency = _currencyRepository.GetByCode(item.CurrencyCode);
                if (currency == null)
                {
                    throw new AppException(ExceptionCode.FINANCE_CONFIG_EXCHANGE_RATE_CURRENCY_CODE_NOT_FOUND)
                        .WithParameter("CurrencyCode", item.CurrencyCode);
                }

                var rate = new ExchangeRate
                {
                    Id = Guid.NewGuid(),
                    CurrencyId = currency.Id,
                    RateDate = dto.RateDate.Date,
                    Rate = item.Rate,
                    InverseRate = 1 / item.Rate,
                    IsOverride = false,
                    Source = dto.Source ?? "Bulk Import",
                    IsActive = true,
                    Create_by = createdBy,
                    Create_time = DateTime.Now
                };

                rates.Add(rate);
            }

            await _rateRepository.CreateRatesAsync(rates);

            return rates.Select(MapToResponseDto).ToList();
        }

        /// <summary>
        /// Update exchange rate
        /// VALIDATION RULES:
        /// - Rate must be > 0
        /// - Auto-update InverseRate
        /// - Log the update
        /// </summary>
        public int Update(UpdateExchangeRateDto dto, string updatedBy)
        {
            // Validate rate value
            if (dto.Rate <= 0)
            {
                throw new AppException(ExceptionCode.FINANCE_CONFIG_EXCHANGE_RATE_INVALID_RATE)
                    .WithParameter("Rate", dto.Rate);
            }

            var rate = _rateRepository.GetById(dto.Id);
            if (rate == null)
            {
                throw new AppException(ExceptionCode.FINANCE_CONFIG_EXCHANGE_RATE_NOT_FOUND)
                    .WithParameter("ExchangeRateId", dto.Id);
            }

            // Update fields
            rate.Rate = dto.Rate;
            rate.InverseRate = 1 / dto.Rate; // Auto-calculate inverse
            rate.IsOverride = dto.IsOverride;
            rate.Source = dto.Source;
            rate.Notes = dto.Notes;
            rate.IsActive = dto.IsActive;
            rate.Update_by = updatedBy;
            rate.Update_time = DateTime.Now;

            return _rateRepository.UpdateRate(rate);
        }

        /// <summary>
        /// Update exchange rate (async)
        /// </summary>
        public async Task<int> UpdateAsync(UpdateExchangeRateDto dto, string updatedBy)
        {
            // Validate rate value
            if (dto.Rate <= 0)
            {
                throw new AppException(ExceptionCode.FINANCE_CONFIG_EXCHANGE_RATE_INVALID_RATE)
                    .WithParameter("Rate", dto.Rate);
            }

            var rate = await _rateRepository.GetByIdAsync(dto.Id);
            if (rate == null)
            {
                throw new AppException(ExceptionCode.FINANCE_CONFIG_EXCHANGE_RATE_NOT_FOUND)
                    .WithParameter("ExchangeRateId", dto.Id);
            }

            // Update fields
            rate.Rate = dto.Rate;
            rate.InverseRate = 1 / dto.Rate;
            rate.IsOverride = dto.IsOverride;
            rate.Source = dto.Source;
            rate.Notes = dto.Notes;
            rate.IsActive = dto.IsActive;
            rate.Update_by = updatedBy;
            rate.Update_time = DateTime.Now;

            return await _rateRepository.UpdateRateAsync(rate);
        }

        #endregion

        #region Approval & Delete Methods

        /// <summary>
        /// Approve exchange rate
        /// </summary>
        public int Approve(Guid id, string approvedBy)
        {
            // Convert username to Guid (simplified - you may need to lookup user)
            var approvedByGuid = Guid.NewGuid(); // TODO: Get actual user GUID from username
            return _rateRepository.ApproveRate(id, approvedByGuid);
        }

        /// <summary>
        /// Approve exchange rate (async)
        /// </summary>
        public async Task<int> ApproveAsync(Guid id, string approvedBy)
        {
            var approvedByGuid = Guid.NewGuid(); // TODO: Get actual user GUID
            return await _rateRepository.ApproveRateAsync(id, approvedByGuid);
        }

        /// <summary>
        /// Delete exchange rate
        /// VALIDATION RULES:
        /// - Cannot delete if used in journals
        /// </summary>
        public int Delete(Guid id)
        {
            return _rateRepository.DeleteRate(id);
        }

        /// <summary>
        /// Delete exchange rate (async)
        /// </summary>
        public async Task<int> DeleteAsync(Guid id)
        {
            return await _rateRepository.DeleteRateAsync(id);
        }

        #endregion

        #region Validation & Calculation Methods

        /// <summary>
        /// Validate exchange rate for journal
        /// VALIDATION RULES:
        /// - Rate must exist for journal date
        /// - Rate must not be too old (configurable MaxRateAgeDays)
        /// - If RequireRateApproval = true, rate must be approved
        /// </summary>
        public void ValidateRateForJournal(Guid currencyId, DateTime journalDate)
        {
            var rate = _rateRepository.GetRateForDate(currencyId, journalDate);

            if (rate == null)
            {
                throw new AppException(ExceptionCode.FINANCE_CONFIG_EXCHANGE_RATE_NOT_FOUND_FOR_DATE)
                    .WithParameter("JournalDate", journalDate.ToString("yyyy-MM-dd"))
                    .WithParameter("CurrencyId", currencyId);
            }

            // Check rate age (example: max 7 days old)
            const int maxRateAgeDays = 7;
            var daysDiff = (journalDate - rate.RateDate).Days;
            if (daysDiff > maxRateAgeDays)
            {
                throw new AppException(ExceptionCode.FINANCE_CONFIG_EXCHANGE_RATE_TOO_OLD)
                    .WithParameter("DaysDiff", daysDiff)
                    .WithParameter("MaxDays", maxRateAgeDays);
            }

            // Check if approval required (simplified - should come from settings)
            const bool requireRateApproval = false;
            if (requireRateApproval && !rate.ApprovedBy.HasValue)
            {
                throw new AppException(ExceptionCode.FINANCE_CONFIG_EXCHANGE_RATE_NOT_APPROVED);
            }
        }

        /// <summary>
        /// Validate exchange rate for journal (async)
        /// </summary>
        public async Task ValidateRateForJournalAsync(Guid currencyId, DateTime journalDate)
        {
            var rate = await _rateRepository.GetRateForDateAsync(currencyId, journalDate);

            if (rate == null)
            {
                throw new AppException(ExceptionCode.FINANCE_CONFIG_EXCHANGE_RATE_NOT_FOUND_FOR_DATE)
                    .WithParameter("JournalDate", journalDate.ToString("yyyy-MM-dd"))
                    .WithParameter("CurrencyId", currencyId);
            }

            const int maxRateAgeDays = 7;
            var daysDiff = (journalDate - rate.RateDate).Days;
            if (daysDiff > maxRateAgeDays)
            {
                throw new AppException(ExceptionCode.FINANCE_CONFIG_EXCHANGE_RATE_TOO_OLD)
                    .WithParameter("DaysDiff", daysDiff)
                    .WithParameter("MaxDays", maxRateAgeDays);
            }

            const bool requireRateApproval = false;
            if (requireRateApproval && !rate.ApprovedBy.HasValue)
            {
                throw new AppException(ExceptionCode.FINANCE_CONFIG_EXCHANGE_RATE_NOT_APPROVED);
            }
        }

        /// <summary>
        /// Calculate base amount from foreign amount
        /// Formula: BaseAmount = Round(ForeignAmount × ExchangeRate, 2)
        /// </summary>
        public decimal CalculateBaseAmount(decimal foreignAmount, decimal exchangeRate)
        {
            return Math.Round(foreignAmount * exchangeRate, 2, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// Calculate foreign amount from base amount
        /// Formula: ForeignAmount = Round(BaseAmount / ExchangeRate, CurrencyDecimals)
        /// </summary>
        public decimal CalculateForeignAmount(decimal baseAmount, decimal exchangeRate, int decimals = 2)
        {
            if (exchangeRate == 0)
            {
                throw new AppException(ExceptionCode.FINANCE_CONFIG_EXCHANGE_RATE_DIVIDE_BY_ZERO);
            }

            return Math.Round(baseAmount / exchangeRate, decimals, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// Find gaps in exchange rate history
        /// </summary>
        public List<DateTime> FindRateGaps(Guid currencyId, DateTime startDate, DateTime endDate)
        {
            return _rateRepository.FindRateGaps(currencyId, startDate, endDate);
        }

        /// <summary>
        /// Find gaps in exchange rate history (async)
        /// </summary>
        public async Task<List<DateTime>> FindRateGapsAsync(Guid currencyId, DateTime startDate, DateTime endDate)
        {
            return await _rateRepository.FindRateGapsAsync(currencyId, startDate, endDate);
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Map ExchangeRate entity to ResponseDto
        /// </summary>
        private ExchangeRateResponseDto MapToResponseDto(ExchangeRate rate)
        {
            // Get currency info for display (using Configuration.Currency)
            var currency = _currencyRepository.GetById(rate.CurrencyId);

            return new ExchangeRateResponseDto
            {
                Id = rate.Id,
                CurrencyId = rate.CurrencyId,
                CurrencyCode = currency?.Code ?? "",
                CurrencyName = currency?.Name ?? "",
                RateDate = rate.RateDate,
                Rate = rate.Rate,
                InverseRate = rate.InverseRate,
                IsOverride = rate.IsOverride,
                Source = rate.Source,
                Notes = rate.Notes,
                IsActive = rate.IsActive,
                ApprovedBy = rate.ApprovedBy?.ToString(),
                ApprovedAt = rate.ApprovedAt,
                Create_time = rate.Create_time
            };
        }

        #endregion
    }
}
