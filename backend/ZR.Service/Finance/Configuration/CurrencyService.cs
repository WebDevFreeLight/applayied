using System;
using System.Collections.Generic;
using System.Linq;
using Infrastructure.Attribute;
using Infrastructure.Exceptions;
using Mapster;
using SqlSugar;
using ZR.Model;
using ZR.Model.Finance.Configuration;
using ZR.Model.Finance.Configuration.Dto;
using ZR.Repository.Finance.Configuration;
using ZR.Service.IService.Finance.Configuration;

namespace ZR.Service.Finance.Configuration
{
    /// <summary>
    /// Currency Service - خدمة العملات
    /// </summary>
    [AppService(ServiceType = typeof(ICurrencyService), ServiceLifetime = LifeTime.Transient)]
    public class CurrencyService : BaseService<Currency>, ICurrencyService
    {
        private readonly ICurrencyRepository _currencyRepository;

        public CurrencyService(ICurrencyRepository currencyRepository)
        {
            _currencyRepository = currencyRepository;
        }

        /// <summary>
        /// Get paginated list of currencies
        /// </summary>
        public PagedInfo<CurrencyListDto> GetList(CurrencyQueryDto query, PagerInfo pager)
        {
            var predicate = Expressionable.Create<Currency>();

            if (!string.IsNullOrEmpty(query.Code))
            {
                predicate = predicate.And(c => c.Code.Contains(query.Code));
            }
            if (query.IsBaseCurrency.HasValue)
            {
                predicate = predicate.And(c => c.IsBaseCurrency == query.IsBaseCurrency.Value);
            }
            if (query.IsActive.HasValue)
            {
                predicate = predicate.And(c => c.IsActive == query.IsActive.Value);
            }

            int totalCount = 0;
            var result = Queryable()
                .Where(predicate.ToExpression())
                .OrderBy(c => c.SortOrder)
                .OrderBy(c => c.Code)
                .ToPageList(pager.PageNum, pager.PageSize, ref totalCount);

            var dtoList = result.Select(MapToDto).ToList();

            return new PagedInfo<CurrencyListDto>
            {
                TotalNum = totalCount,
                PageIndex = pager.PageNum,
                PageSize = pager.PageSize,
                Result = dtoList
            };
        }

        /// <summary>
        /// Get currency by ID
        /// </summary>
        public CurrencyDetailDto GetById(Guid id)
        {
            var currency = _currencyRepository.GetById(id);
            if (currency == null) return null;

            return MapToDetailDto(currency);
        }

        /// <summary>
        /// Create currency
        /// </summary>
        public Currency Create(CreateCurrencyDto dto, string userName)
        {
            // Validate code uniqueness
            var exists = Queryable().Any(c => c.Code == dto.Code);
            if (exists)
            {
                throw new AppException(ExceptionCode.FINANCE_CONFIG_CURRENCY_CODE_ALREADY_EXISTS)
                    .WithParameter("CurrencyCode", dto.Code);
            }

            var model = dto.Adapt<Currency>();
            model.Id = Guid.NewGuid();
            model.IsActive = dto.IsActive;
            model.Create_by = userName;
            model.Create_time = DateTime.Now;

            Insert(model);
            return model;
        }

        /// <summary>
        /// Update currency
        /// </summary>
        public int Update(UpdateCurrencyDto dto, string userName)
        {
            var entity = _currencyRepository.GetById(dto.Id);
            if (entity == null)
            {
                throw new AppException(ExceptionCode.FINANCE_CONFIG_CURRENCY_NOT_FOUND)
                    .WithParameter("CurrencyId", dto.Id);
            }

            // Update allowed fields
            entity.Name = dto.Name;
            entity.NameAr = dto.NameAr;
            entity.Symbol = dto.Symbol;
            entity.DecimalPlaces = dto.DecimalPlaces;
            entity.IsActive = dto.IsActive;
            entity.RoundingMethod = dto.RoundingMethod;
            entity.DisplayFormat = dto.DisplayFormat;
            entity.Description = dto.Description;
            entity.SortOrder = dto.SortOrder;
            entity.Update_by = userName;
            entity.Update_time = DateTime.Now;

            return Update(entity, it => new
            {
                it.Name,
                it.NameAr,
                it.Symbol,
                it.DecimalPlaces,
                it.IsActive,
                it.RoundingMethod,
                it.DisplayFormat,
                it.Description,
                it.SortOrder,
                it.Update_by,
                it.Update_time
            }, f => f.Id == entity.Id);
        }

        /// <summary>
        /// Delete currencies
        /// </summary>
        public int Delete(Guid[] ids)
        {
            // Check if any currency is base currency
            foreach (var id in ids)
            {
                var currency = _currencyRepository.GetById(id);
                if (currency != null && currency.IsBaseCurrency)
                {
                    throw new AppException(ExceptionCode.FINANCE_CONFIG_CURRENCY_CANNOT_DELETE_BASE)
                        .WithParameter("CurrencyId", id);
                }
            }

            return Context.Deleteable<Currency>()
                .Where(it => ids.Contains(it.Id))
                .ExecuteCommand();
        }

        /// <summary>
        /// Set base currency
        /// </summary>
        public int SetBaseCurrency(Guid currencyId, string userName)
        {
            var currency = _currencyRepository.GetById(currencyId);
            if (currency == null)
                throw new AppException(ExceptionCode.FINANCE_CONFIG_CURRENCY_NOT_FOUND)
                    .WithParameter("CurrencyId", currencyId);

            if (!currency.IsActive)
            {
                throw new AppException(ExceptionCode.FINANCE_CONFIG_CURRENCY_BASE_MUST_BE_ACTIVE)
                    .WithParameter("CurrencyId", currencyId);
            }

            // Unset existing base currency
            var existingBase = Queryable().First(c => c.IsBaseCurrency);
            if (existingBase != null && existingBase.Id != currencyId)
            {
                existingBase.IsBaseCurrency = false;
                existingBase.Update_by = userName;
                existingBase.Update_time = DateTime.Now;
                Update(existingBase, it => new { it.IsBaseCurrency, it.Update_by, it.Update_time }, f => f.Id == existingBase.Id);
            }

            // Set new base currency
            currency.IsBaseCurrency = true;
            currency.Update_by = userName;
            currency.Update_time = DateTime.Now;

            return Update(currency, it => new { it.IsBaseCurrency, it.Update_by, it.Update_time }, f => f.Id == currencyId);
        }

        /// <summary>
        /// Get base currency
        /// </summary>
        public CurrencyDetailDto GetBaseCurrency()
        {
            var currency = Queryable().First(c => c.IsBaseCurrency);
            if (currency == null) return null;

            return MapToDetailDto(currency);
        }

        /// <summary>
        /// Get all active currencies
        /// </summary>
        public List<CurrencyListDto> GetActiveCurrencies()
        {
            var currencies = Queryable()
                .Where(c => c.IsActive)
                .OrderBy(c => c.SortOrder)
                .OrderBy(c => c.Code)
                .ToList();
            return currencies.Select(MapToDto).ToList();
        }

        /// <summary>
        /// Check if currency code exists
        /// </summary>
        public bool CodeExists(string currencyCode, Guid? excludeId = null)
        {
            var predicate = Expressionable.Create<Currency>();
            predicate = predicate.And(c => c.Code == currencyCode);

            if (excludeId.HasValue)
            {
                predicate = predicate.And(c => c.Id != excludeId.Value);
            }

            return Queryable().Any(predicate.ToExpression());
        }

        private CurrencyListDto MapToDto(Currency currency)
        {
            return currency.Adapt<CurrencyListDto>();
        }

        private CurrencyDetailDto MapToDetailDto(Currency currency)
        {
            return currency.Adapt<CurrencyDetailDto>();
        }
    }
}
