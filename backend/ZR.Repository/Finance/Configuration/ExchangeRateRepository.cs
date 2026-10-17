using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.Attribute;
using SqlSugar;
using ZR.Model;
using ZR.Model.Finance.Configuration;
using ZR.Model.Finance.GL;

namespace ZR.Repository.Finance.Configuration
{
    /// <summary>
    /// Exchange Rate Repository Implementation
    /// تنفيذ مستودع أسعار الصرف
    /// </summary>
    [AppService(ServiceType = typeof(IExchangeRateRepository), ServiceLifetime = LifeTime.Transient)]
    public class ExchangeRateRepository : BaseRepository<ExchangeRate>, IExchangeRateRepository
    {
        /// <summary>
        /// Get paginated list of exchange rates
        /// </summary>
        public PagedInfo<ExchangeRate> GetList(ExchangeRate searchModel, PagerInfo pager)
        {
            var predicate = Expressionable.Create<ExchangeRate>();
            predicate = predicate.AndIF(searchModel.CurrencyId != Guid.Empty, it => it.CurrencyId == searchModel.CurrencyId);
            predicate = predicate.AndIF(searchModel.RateDate != default, it => it.RateDate == searchModel.RateDate);
            predicate = predicate.AndIF(searchModel.IsOverride != null, it => it.IsOverride == searchModel.IsOverride);
            predicate = predicate.AndIF(searchModel.IsActive != null, it => it.IsActive == searchModel.IsActive);

            return Queryable()
                .Where(predicate.ToExpression())
                .OrderBy(x => x.RateDate, OrderByType.Desc)
                .ToPage(pager);
        }

        /// <summary>
        /// Get exchange rate by ID
        /// </summary>
        public ExchangeRate GetById(Guid id)
        {
            return GetFirst(x => x.Id == id);
        }

        /// <summary>
        /// Get exchange rate by ID (async)
        /// </summary>
        public async Task<ExchangeRate> GetByIdAsync(Guid id)
        {
            return await GetFirstAsync(x => x.Id == id);
        }

        /// <summary>
        /// Get exchange rate for specific currency and date
        /// RULE: Returns the rate for exact date or latest rate before that date
        /// </summary>
        public ExchangeRate GetRateForDate(Guid currencyId, DateTime date)
        {
            return Queryable()
                .Where(x => x.CurrencyId == currencyId && x.RateDate <= date && x.IsActive == true)
                .OrderBy(x => x.RateDate, OrderByType.Desc)
                .First();
        }

        /// <summary>
        /// Get exchange rate for specific currency and date (async)
        /// </summary>
        public async Task<ExchangeRate> GetRateForDateAsync(Guid currencyId, DateTime date)
        {
            return await Context.Queryable<ExchangeRate>()
                .Where(x => x.CurrencyId == currencyId && x.RateDate <= date && x.IsActive == true)
                .OrderBy(x => x.RateDate, OrderByType.Desc)
                .FirstAsync();
        }

        /// <summary>
        /// Get exchange rate by currency code and date
        /// </summary>
        public ExchangeRate GetRateByCodeAndDate(string currencyCode, DateTime date)
        {
            // Join with Currency table to get rate by code
            return Context.Queryable<ExchangeRate, Model.Finance.Configuration.Currency>((rate, curr) => new JoinQueryInfos(
                    JoinType.Inner, rate.CurrencyId == curr.Id
                ))
                .Where((rate, curr) => curr.Code == currencyCode && rate.RateDate <= date && rate.IsActive == true)
                .OrderBy((rate, curr) => rate.RateDate, OrderByType.Desc)
                .Select((rate, curr) => rate)
                .First();
        }

        /// <summary>
        /// Get exchange rate by currency code and date (async)
        /// </summary>
        public async Task<ExchangeRate> GetRateByCodeAndDateAsync(string currencyCode, DateTime date)
        {
            return await Context.Queryable<ExchangeRate, Model.Finance.Configuration.Currency>((rate, curr) => new JoinQueryInfos(
                    JoinType.Inner, rate.CurrencyId == curr.Id
                ))
                .Where((rate, curr) => curr.Code == currencyCode && rate.RateDate <= date && rate.IsActive == true)
                .OrderBy((rate, curr) => rate.RateDate, OrderByType.Desc)
                .Select((rate, curr) => rate)
                .FirstAsync();
        }

        /// <summary>
        /// Get exchange rates for date range
        /// </summary>
        public List<ExchangeRate> GetRatesForDateRange(Guid currencyId, DateTime startDate, DateTime endDate)
        {
            return Queryable()
                .Where(x => x.CurrencyId == currencyId && x.RateDate >= startDate && x.RateDate <= endDate)
                .OrderBy(x => x.RateDate)
                .ToList();
        }

        /// <summary>
        /// Get exchange rates for date range (async)
        /// </summary>
        public async Task<List<ExchangeRate>> GetRatesForDateRangeAsync(Guid currencyId, DateTime startDate, DateTime endDate)
        {
            return await Context.Queryable<ExchangeRate>()
                .Where(x => x.CurrencyId == currencyId && x.RateDate >= startDate && x.RateDate <= endDate)
                .OrderBy(x => x.RateDate)
                .ToListAsync();
        }

        /// <summary>
        /// Get latest exchange rate for currency
        /// </summary>
        public ExchangeRate GetLatestRate(Guid currencyId)
        {
            return Queryable()
                .Where(x => x.CurrencyId == currencyId && x.IsActive == true)
                .OrderBy(x => x.RateDate, OrderByType.Desc)
                .First();
        }

        /// <summary>
        /// Get latest exchange rate for currency (async)
        /// </summary>
        public async Task<ExchangeRate> GetLatestRateAsync(Guid currencyId)
        {
            return await Context.Queryable<ExchangeRate>()
                .Where(x => x.CurrencyId == currencyId && x.IsActive == true)
                .OrderBy(x => x.RateDate, OrderByType.Desc)
                .FirstAsync();
        }

        /// <summary>
        /// Get all rates for a specific date
        /// </summary>
        public List<ExchangeRate> GetRatesForDate(DateTime date)
        {
            return Queryable()
                .Where(x => x.RateDate == date)
                .ToList();
        }

        /// <summary>
        /// Get all rates for a specific date (async)
        /// </summary>
        public async Task<List<ExchangeRate>> GetRatesForDateAsync(DateTime date)
        {
            return await Context.Queryable<ExchangeRate>()
                .Where(x => x.RateDate == date)
                .ToListAsync();
        }

        /// <summary>
        /// Check if rate exists for currency and date
        /// </summary>
        public bool RateExistsForDate(Guid currencyId, DateTime date)
        {
            return Any(x => x.CurrencyId == currencyId && x.RateDate == date);
        }

        /// <summary>
        /// Check if rate exists for currency and date (async)
        /// </summary>
        public async Task<bool> RateExistsForDateAsync(Guid currencyId, DateTime date)
        {
            return await Context.Queryable<ExchangeRate>()
                .AnyAsync(x => x.CurrencyId == currencyId && x.RateDate == date);
        }

        /// <summary>
        /// Get rate history for currency
        /// </summary>
        public List<ExchangeRate> GetRateHistory(Guid currencyId, int days = 30)
        {
            var startDate = DateTime.Now.Date.AddDays(-days);
            return Queryable()
                .Where(x => x.CurrencyId == currencyId && x.RateDate >= startDate)
                .OrderBy(x => x.RateDate, OrderByType.Desc)
                .ToList();
        }

        /// <summary>
        /// Get rate history for currency (async)
        /// </summary>
        public async Task<List<ExchangeRate>> GetRateHistoryAsync(Guid currencyId, int days = 30)
        {
            var startDate = DateTime.Now.Date.AddDays(-days);
            return await Context.Queryable<ExchangeRate>()
                .Where(x => x.CurrencyId == currencyId && x.RateDate >= startDate)
                .OrderBy(x => x.RateDate, OrderByType.Desc)
                .ToListAsync();
        }

        /// <summary>
        /// Create exchange rate
        /// </summary>
        public int CreateRate(ExchangeRate rate)
        {
            return Insert(rate);
        }

        /// <summary>
        /// Create exchange rate (async)
        /// </summary>
        public async Task<int> CreateRateAsync(ExchangeRate rate)
        {
            var result = await InsertAsync(rate);
            return result ? 1 : 0;
        }

        /// <summary>
        /// Create multiple exchange rates (bulk insert)
        /// </summary>
        public int CreateRates(List<ExchangeRate> rates)
        {
            return Insert(rates);
        }

        /// <summary>
        /// Create multiple exchange rates (bulk insert, async)
        /// </summary>
        public async Task<int> CreateRatesAsync(List<ExchangeRate> rates)
        {
            var result = await InsertRangeAsync(rates);
            return result ? rates.Count : 0;
        }

        /// <summary>
        /// Update exchange rate
        /// </summary>
        public int UpdateRate(ExchangeRate rate)
        {
            return Update(rate);
        }

        /// <summary>
        /// Update exchange rate (async)
        /// </summary>
        public async Task<int> UpdateRateAsync(ExchangeRate rate)
        {
            var result = await UpdateAsync(rate);
            return result ? 1 : 0;
        }

        /// <summary>
        /// Approve exchange rate
        /// </summary>
        public int ApproveRate(Guid id, Guid approvedBy)
        {
            return Context.Updateable<ExchangeRate>()
                .SetColumns(x => x.ApprovedBy == approvedBy)
                .SetColumns(x => x.ApprovedAt == DateTime.Now)
                .Where(x => x.Id == id)
                .ExecuteCommand();
        }

        /// <summary>
        /// Approve exchange rate (async)
        /// </summary>
        public async Task<int> ApproveRateAsync(Guid id, Guid approvedBy)
        {
            return await Context.Updateable<ExchangeRate>()
                .SetColumns(x => x.ApprovedBy == approvedBy)
                .SetColumns(x => x.ApprovedAt == DateTime.Now)
                .Where(x => x.Id == id)
                .ExecuteCommandAsync();
        }

        /// <summary>
        /// Delete exchange rate
        /// RULE: Cannot delete if used in journals
        /// </summary>
        public int DeleteRate(Guid id)
        {
            // Check if used in journals first
            if (IsRateUsedInJournals(id))
            {
                throw new InvalidOperationException("Cannot delete exchange rate that is used in journals");
            }

            return Delete(id);
        }

        /// <summary>
        /// Delete exchange rate (async)
        /// </summary>
        public async Task<int> DeleteRateAsync(Guid id)
        {
            // Check if used in journals first
            if (await IsRateUsedInJournalsAsync(id))
            {
                throw new InvalidOperationException("Cannot delete exchange rate that is used in journals");
            }

            var result = await DeleteByIdAsync(id);
            return result ? 1 : 0;
        }

        /// <summary>
        /// Check if rate is used in journals
        /// </summary>
        public bool IsRateUsedInJournals(Guid rateId)
        {
            // Get the rate to find its date and currency
            var rate = GetById(rateId);
            if (rate == null) return false;

            // Check if any journal uses this currency and date
            return Context.Queryable<GLJournal>()
                .Any(j => j.CurrencyId == rate.CurrencyId && j.JournalDate == rate.RateDate);
        }

        /// <summary>
        /// Check if rate is used in journals (async)
        /// </summary>
        public async Task<bool> IsRateUsedInJournalsAsync(Guid rateId)
        {
            // Get the rate to find its date and currency
            var rate = await GetByIdAsync(rateId);
            if (rate == null) return false;

            // Check if any journal uses this currency and date
            return await Context.Queryable<GLJournal>()
                .AnyAsync(j => j.CurrencyId == rate.CurrencyId && j.JournalDate == rate.RateDate);
        }

        /// <summary>
        /// Find gaps in rate history
        /// Returns list of dates where rate is missing
        /// </summary>
        public List<DateTime> FindRateGaps(Guid currencyId, DateTime startDate, DateTime endDate)
        {
            // Get all existing rates in the range
            var existingRates = GetRatesForDateRange(currencyId, startDate, endDate)
                .Select(r => r.RateDate.Date)
                .ToHashSet();

            // Generate all dates in range
            var allDates = new List<DateTime>();
            for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
            {
                if (!existingRates.Contains(date))
                {
                    allDates.Add(date);
                }
            }

            return allDates;
        }

        /// <summary>
        /// Find gaps in rate history (async)
        /// </summary>
        public async Task<List<DateTime>> FindRateGapsAsync(Guid currencyId, DateTime startDate, DateTime endDate)
        {
            // Get all existing rates in the range
            var rates = await GetRatesForDateRangeAsync(currencyId, startDate, endDate);
            var existingRates = rates.Select(r => r.RateDate.Date).ToHashSet();

            // Generate all dates in range
            var allDates = new List<DateTime>();
            for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
            {
                if (!existingRates.Contains(date))
                {
                    allDates.Add(date);
                }
            }

            return allDates;
        }
    }
}
