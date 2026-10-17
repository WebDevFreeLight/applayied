using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.Attribute;
using Infrastructure.Exceptions;
using Mapster;
using ZR.Model;
using ZR.Model.Finance.Tax.Dto;
using ZR.Model.Finance.Tax;
using ZR.Repository.Finance.Tax;
using ZR.Service.IService.Finance.Tax;

namespace ZR.Service.Finance.Tax
{
    /// <summary>
    /// Tax Rate Service Implementation
    /// تنفيذ خدمة معدلات الضرائب
    /// </summary>
    [AppService(ServiceType = typeof(ITaxRateService), ServiceLifetime = LifeTime.Transient)]
    public class TaxRateService : ITaxRateService
    {
        private readonly ITaxRateRepository _taxRateRepository;
        private readonly ITaxCodeRepository _taxCodeRepository;

        public TaxRateService(ITaxRateRepository taxRateRepository, ITaxCodeRepository taxCodeRepository)
        {
            _taxRateRepository = taxRateRepository;
            _taxCodeRepository = taxCodeRepository;
        }

        #region CRUD Operations

        /// <summary>
        /// Get paginated list of tax rates
        /// </summary>
        public PagedInfo<TaxRateResponseDto> GetList(TaxRateQueryDto query)
        {
            var pager = new PagerInfo
            {
                PageNum = query.PageNum,
                PageSize = query.PageSize
            };

            var filter = new TaxRate
            {
                TaxCodeId = query.TaxCodeId ?? Guid.Empty,
                EffectiveFrom = query.EffectiveDate ?? default,
                IsApproved = query.IsApproved ?? false
            };

            var result = _taxRateRepository.GetList(filter, pager);

            return new PagedInfo<TaxRateResponseDto>
            {
                TotalNum = result.TotalNum,
                PageIndex = result.PageIndex,
                PageSize = result.PageSize,
                TotalPage = result.TotalPage,
                Result = result.Result.Select(x => MapToResponse(x)).ToList()
            };
        }

        /// <summary>
        /// Get tax rate by ID
        /// </summary>
        public TaxRateResponseDto GetById(Guid id)
        {
            var entity = _taxRateRepository.GetById(id);
            return entity == null ? null : MapToResponse(entity);
        }

        /// <summary>
        /// Get tax rate by ID (async)
        /// </summary>
        public async Task<TaxRateResponseDto> GetByIdAsync(Guid id)
        {
            var entity = await _taxRateRepository.GetByIdAsync(id);
            return entity == null ? null : await MapToResponseAsync(entity);
        }

        /// <summary>
        /// Create tax rate
        /// </summary>
        public TaxRateResponseDto Create(CreateTaxRateDto dto)
        {
            // Validate tax code exists
            var taxCode = _taxCodeRepository.GetById(dto.TaxCodeId);
            if (taxCode == null)
            {
                throw new AppException(ExceptionCode.FINANCE_TAX_CODE_NOT_FOUND)
                    .WithParameter("TaxCodeId", dto.TaxCodeId);
            }

            // Validate no overlapping rates
            if (_taxRateRepository.HasOverlappingRates(dto.TaxCodeId, dto.EffectiveFrom, dto.EffectiveTo))
            {
                throw new AppException(ExceptionCode.FINANCE_TAX_RATE_OVERLAP)
                    .WithParameter("TaxCodeId", dto.TaxCodeId);
            }

            // Validate effective_to is after effective_from
            if (dto.EffectiveTo.HasValue && dto.EffectiveTo.Value < dto.EffectiveFrom)
            {
                throw new AppException(ExceptionCode.FINANCE_TAX_RATE_INVALID_DATE_RANGE);
            }

            var entity = dto.Adapt<TaxRate>();
            entity.Id = Guid.NewGuid();
            entity.Create_time = DateTime.Now;

            _taxRateRepository.InsertTaxRate(entity);

            return MapToResponse(entity);
        }

        /// <summary>
        /// Create tax rate (async)
        /// </summary>
        public async Task<TaxRateResponseDto> CreateAsync(CreateTaxRateDto dto)
        {
            var taxCode = await _taxCodeRepository.GetByIdAsync(dto.TaxCodeId);
            if (taxCode == null)
            {
                throw new AppException(ExceptionCode.FINANCE_TAX_CODE_NOT_FOUND)
                    .WithParameter("TaxCodeId", dto.TaxCodeId);
            }

            if (await _taxRateRepository.HasOverlappingRatesAsync(dto.TaxCodeId, dto.EffectiveFrom, dto.EffectiveTo))
            {
                throw new AppException(ExceptionCode.FINANCE_TAX_RATE_OVERLAP)
                    .WithParameter("TaxCodeId", dto.TaxCodeId);
            }

            if (dto.EffectiveTo.HasValue && dto.EffectiveTo.Value < dto.EffectiveFrom)
            {
                throw new AppException(ExceptionCode.FINANCE_TAX_RATE_INVALID_DATE_RANGE);
            }

            var entity = dto.Adapt<TaxRate>();
            entity.Id = Guid.NewGuid();
            entity.Create_time = DateTime.Now;

            await _taxRateRepository.InsertTaxRateAsync(entity);

            return await MapToResponseAsync(entity);
        }

        /// <summary>
        /// Update tax rate
        /// </summary>
        public TaxRateResponseDto Update(UpdateTaxRateDto dto)
        {
            var existing = _taxRateRepository.GetById(dto.Id);
            if (existing == null)
            {
                throw new AppException(ExceptionCode.FINANCE_TAX_RATE_NOT_FOUND)
                    .WithParameter("TaxRateId", dto.Id);
            }

            // Validate no overlapping rates (excluding self)
            if (_taxRateRepository.HasOverlappingRates(dto.TaxCodeId, dto.EffectiveFrom, dto.EffectiveTo, dto.Id))
            {
                throw new AppException(ExceptionCode.FINANCE_TAX_RATE_OVERLAP)
                    .WithParameter("TaxCodeId", dto.TaxCodeId);
            }

            if (dto.EffectiveTo.HasValue && dto.EffectiveTo.Value < dto.EffectiveFrom)
            {
                throw new AppException(ExceptionCode.FINANCE_TAX_RATE_INVALID_DATE_RANGE);
            }

            dto.Adapt(existing);
            existing.Update_time = DateTime.Now;

            _taxRateRepository.UpdateTaxRate(existing);

            return MapToResponse(existing);
        }

        /// <summary>
        /// Update tax rate (async)
        /// </summary>
        public async Task<TaxRateResponseDto> UpdateAsync(UpdateTaxRateDto dto)
        {
            var existing = await _taxRateRepository.GetByIdAsync(dto.Id);
            if (existing == null)
            {
                throw new AppException(ExceptionCode.FINANCE_TAX_RATE_NOT_FOUND)
                    .WithParameter("TaxRateId", dto.Id);
            }

            if (await _taxRateRepository.HasOverlappingRatesAsync(dto.TaxCodeId, dto.EffectiveFrom, dto.EffectiveTo, dto.Id))
            {
                throw new AppException(ExceptionCode.FINANCE_TAX_RATE_OVERLAP)
                    .WithParameter("TaxCodeId", dto.TaxCodeId);
            }

            if (dto.EffectiveTo.HasValue && dto.EffectiveTo.Value < dto.EffectiveFrom)
            {
                throw new AppException(ExceptionCode.FINANCE_TAX_RATE_INVALID_DATE_RANGE);
            }

            dto.Adapt(existing);
            existing.Update_time = DateTime.Now;

            await _taxRateRepository.UpdateTaxRateAsync(existing);

            return await MapToResponseAsync(existing);
        }

        /// <summary>
        /// End a tax rate (set effective_to date)
        /// </summary>
        public TaxRateResponseDto EndRate(EndTaxRateDto dto)
        {
            var existing = _taxRateRepository.GetById(dto.Id);
            if (existing == null)
            {
                throw new AppException(ExceptionCode.FINANCE_TAX_RATE_NOT_FOUND)
                    .WithParameter("TaxRateId", dto.Id);
            }

            if (dto.EffectiveTo < existing.EffectiveFrom)
            {
                throw new AppException(ExceptionCode.FINANCE_TAX_RATE_INVALID_DATE_RANGE);
            }

            existing.EffectiveTo = dto.EffectiveTo;
            existing.ChangeReason = dto.Reason;
            existing.Update_time = DateTime.Now;

            _taxRateRepository.UpdateTaxRate(existing);

            return MapToResponse(existing);
        }

        /// <summary>
        /// End a tax rate (async)
        /// </summary>
        public async Task<TaxRateResponseDto> EndRateAsync(EndTaxRateDto dto)
        {
            var existing = await _taxRateRepository.GetByIdAsync(dto.Id);
            if (existing == null)
            {
                throw new AppException(ExceptionCode.FINANCE_TAX_RATE_NOT_FOUND)
                    .WithParameter("TaxRateId", dto.Id);
            }

            if (dto.EffectiveTo < existing.EffectiveFrom)
            {
                throw new AppException(ExceptionCode.FINANCE_TAX_RATE_INVALID_DATE_RANGE);
            }

            existing.EffectiveTo = dto.EffectiveTo;
            existing.ChangeReason = dto.Reason;
            existing.Update_time = DateTime.Now;

            await _taxRateRepository.UpdateTaxRateAsync(existing);

            return await MapToResponseAsync(existing);
        }

        /// <summary>
        /// Delete tax rates
        /// </summary>
        public int Delete(Guid[] ids)
        {
            foreach (var id in ids)
            {
                if (!CanDelete(id))
                {
                    throw new AppException(ExceptionCode.FINANCE_TAX_RATE_IN_USE)
                        .WithParameter("TaxRateId", id);
                }
            }

            return _taxRateRepository.DeleteTaxRates(ids);
        }

        /// <summary>
        /// Delete tax rates (async)
        /// </summary>
        public async Task<int> DeleteAsync(Guid[] ids)
        {
            foreach (var id in ids)
            {
                if (!await CanDeleteAsync(id))
                {
                    throw new AppException(ExceptionCode.FINANCE_TAX_RATE_IN_USE)
                        .WithParameter("TaxRateId", id);
                }
            }

            return await _taxRateRepository.DeleteTaxRatesAsync(ids);
        }

        #endregion

        #region Query Operations

        /// <summary>
        /// Get all rates for a tax code
        /// </summary>
        public List<TaxRateResponseDto> GetRatesByTaxCode(Guid taxCodeId)
        {
            var rates = _taxRateRepository.GetRatesByTaxCode(taxCodeId);
            return rates.Select(x => MapToResponse(x)).ToList();
        }

        /// <summary>
        /// Get all rates for a tax code (async)
        /// </summary>
        public async Task<List<TaxRateResponseDto>> GetRatesByTaxCodeAsync(Guid taxCodeId)
        {
            var rates = await _taxRateRepository.GetRatesByTaxCodeAsync(taxCodeId);
            var result = new List<TaxRateResponseDto>();
            foreach (var rate in rates)
            {
                result.Add(await MapToResponseAsync(rate));
            }
            return result;
        }

        /// <summary>
        /// Get effective rate for a specific date
        /// </summary>
        public GetRateForDateResponse GetRateForDate(GetRateForDateRequest request)
        {
            var taxCode = _taxCodeRepository.GetById(request.TaxCodeId);
            var rate = _taxRateRepository.GetEffectiveRate(request.TaxCodeId, request.Date);

            return new GetRateForDateResponse
            {
                TaxCodeId = request.TaxCodeId,
                TaxCode = taxCode?.Code,
                Date = request.Date,
                Rate = rate?.Rate,
                RateId = rate?.Id,
                EffectiveFrom = rate?.EffectiveFrom,
                EffectiveTo = rate?.EffectiveTo,
                RateFound = rate != null,
                ErrorMessage = rate == null ? "No effective rate found for the specified date - لا يوجد معدل ساري للتاريخ المحدد" : null
            };
        }

        /// <summary>
        /// Get effective rate for a specific date (async)
        /// </summary>
        public async Task<GetRateForDateResponse> GetRateForDateAsync(GetRateForDateRequest request)
        {
            var taxCode = await _taxCodeRepository.GetByIdAsync(request.TaxCodeId);
            var rate = await _taxRateRepository.GetEffectiveRateAsync(request.TaxCodeId, request.Date);

            return new GetRateForDateResponse
            {
                TaxCodeId = request.TaxCodeId,
                TaxCode = taxCode?.Code,
                Date = request.Date,
                Rate = rate?.Rate,
                RateId = rate?.Id,
                EffectiveFrom = rate?.EffectiveFrom,
                EffectiveTo = rate?.EffectiveTo,
                RateFound = rate != null,
                ErrorMessage = rate == null ? "No effective rate found for the specified date - لا يوجد معدل ساري للتاريخ المحدد" : null
            };
        }

        /// <summary>
        /// Get currently effective rates
        /// </summary>
        public List<TaxRateResponseDto> GetCurrentlyEffectiveRates()
        {
            var rates = _taxRateRepository.GetCurrentlyEffectiveRates();
            return rates.Select(x => MapToResponse(x)).ToList();
        }

        /// <summary>
        /// Get future rates
        /// </summary>
        public List<TaxRateResponseDto> GetFutureRates()
        {
            var rates = _taxRateRepository.GetFutureRates();
            return rates.Select(x => MapToResponse(x)).ToList();
        }

        /// <summary>
        /// Get expired rates
        /// </summary>
        public List<TaxRateResponseDto> GetExpiredRates()
        {
            var rates = _taxRateRepository.GetExpiredRates();
            return rates.Select(x => MapToResponse(x)).ToList();
        }

        #endregion

        #region Validation Operations

        /// <summary>
        /// Check for overlapping rates
        /// </summary>
        public bool HasOverlappingRates(Guid taxCodeId, DateTime effectiveFrom, DateTime? effectiveTo, Guid? excludeId = null)
        {
            return _taxRateRepository.HasOverlappingRates(taxCodeId, effectiveFrom, effectiveTo, excludeId);
        }

        /// <summary>
        /// Check for overlapping rates (async)
        /// </summary>
        public async Task<bool> HasOverlappingRatesAsync(Guid taxCodeId, DateTime effectiveFrom, DateTime? effectiveTo, Guid? excludeId = null)
        {
            return await _taxRateRepository.HasOverlappingRatesAsync(taxCodeId, effectiveFrom, effectiveTo, excludeId);
        }

        /// <summary>
        /// Check if rate can be deleted
        /// </summary>
        public bool CanDelete(Guid id)
        {
            return _taxRateRepository.CanDelete(id);
        }

        /// <summary>
        /// Check if rate can be deleted (async)
        /// </summary>
        public async Task<bool> CanDeleteAsync(Guid id)
        {
            return await _taxRateRepository.CanDeleteAsync(id);
        }

        #endregion

        #region Approval Operations

        /// <summary>
        /// Approve a tax rate
        /// </summary>
        public TaxRateResponseDto Approve(Guid id, string approvedBy)
        {
            _taxRateRepository.ApproveRate(id, approvedBy);
            return GetById(id);
        }

        /// <summary>
        /// Approve a tax rate (async)
        /// </summary>
        public async Task<TaxRateResponseDto> ApproveAsync(Guid id, string approvedBy)
        {
            await _taxRateRepository.ApproveRateAsync(id, approvedBy);
            return await GetByIdAsync(id);
        }

        #endregion

        #region Mapping Helpers

        private TaxRateResponseDto MapToResponse(TaxRate entity)
        {
            var taxCode = _taxCodeRepository.GetById(entity.TaxCodeId);
            var today = DateTime.Today;

            var response = entity.Adapt<TaxRateResponseDto>();
            response.TaxCode = taxCode?.Code;
            response.TaxCodeNameEn = taxCode?.NameEn;
            response.TaxCodeNameAr = taxCode?.NameAr;
            response.RateDisplay = $"{entity.Rate:0.##}%";
            response.IsCurrentlyEffective = entity.EffectiveFrom <= today && (entity.EffectiveTo == null || entity.EffectiveTo >= today);
            response.IsExpired = entity.EffectiveTo.HasValue && entity.EffectiveTo.Value < today;
            response.IsFuture = entity.EffectiveFrom > today;

            return response;
        }

        private async Task<TaxRateResponseDto> MapToResponseAsync(TaxRate entity)
        {
            var taxCode = await _taxCodeRepository.GetByIdAsync(entity.TaxCodeId);
            var today = DateTime.Today;

            var response = entity.Adapt<TaxRateResponseDto>();
            response.TaxCode = taxCode?.Code;
            response.TaxCodeNameEn = taxCode?.NameEn;
            response.TaxCodeNameAr = taxCode?.NameAr;
            response.RateDisplay = $"{entity.Rate:0.##}%";
            response.IsCurrentlyEffective = entity.EffectiveFrom <= today && (entity.EffectiveTo == null || entity.EffectiveTo >= today);
            response.IsExpired = entity.EffectiveTo.HasValue && entity.EffectiveTo.Value < today;
            response.IsFuture = entity.EffectiveFrom > today;

            return response;
        }

        #endregion
    }
}
