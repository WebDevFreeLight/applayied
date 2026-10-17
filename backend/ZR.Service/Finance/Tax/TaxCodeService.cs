using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.Attribute;
using Infrastructure.Exceptions;
using Mapster;
using ZR.Model;
using ZR.Model.Finance.Tax.Dto;
using ZR.Model.Finance.Enums;
using ZR.Model.Finance.Tax;
using ZR.Repository.Finance.Tax;
using ZR.Service.IService.Finance.Tax;

namespace ZR.Service.Finance.Tax
{
    /// <summary>
    /// Tax Code Service Implementation
    /// تنفيذ خدمة رموز الضرائب
    /// </summary>
    [AppService(ServiceType = typeof(ITaxCodeService), ServiceLifetime = LifeTime.Transient)]
    public class TaxCodeService : ITaxCodeService
    {
        private readonly ITaxCodeRepository _taxCodeRepository;
        private readonly ITaxRateRepository _taxRateRepository;

        public TaxCodeService(ITaxCodeRepository taxCodeRepository, ITaxRateRepository taxRateRepository)
        {
            _taxCodeRepository = taxCodeRepository;
            _taxRateRepository = taxRateRepository;
        }

        #region CRUD Operations

        /// <summary>
        /// Get paginated list of tax codes
        /// </summary>
        public PagedInfo<TaxCodeResponseDto> GetList(TaxCodeQueryDto query)
        {
            var pager = new PagerInfo
            {
                PageNum = query.PageNum,
                PageSize = query.PageSize
            };

            var filter = new TaxCode
            {
                CompanyID = query.CompanyId,
                Code = query.Code,
                NameEn = query.NameEn,
                IsActive = query.IsActive ?? false
            };

            var result = _taxCodeRepository.GetList(filter, pager);

            return new PagedInfo<TaxCodeResponseDto>
            {
                TotalNum = result.TotalNum,
                PageIndex = result.PageIndex,
                PageSize = result.PageSize,
                Result = result.Result.Select(x => MapToResponse(x)).ToList()
            };
        }

        /// <summary>
        /// Get tax code by ID
        /// </summary>
        public TaxCodeResponseDto GetById(Guid id)
        {
            var entity = _taxCodeRepository.GetById(id);
            return entity == null ? null : MapToResponse(entity);
        }

        /// <summary>
        /// Get tax code by ID (async)
        /// </summary>
        public async Task<TaxCodeResponseDto> GetByIdAsync(Guid id)
        {
            var entity = await _taxCodeRepository.GetByIdAsync(id);
            return entity == null ? null : await MapToResponseAsync(entity);
        }

        /// <summary>
        /// Get tax code by code
        /// </summary>
        public TaxCodeResponseDto GetByCode(string code, long? companyId = null)
        {
            var entity = _taxCodeRepository.GetByCode(code, companyId);
            return entity == null ? null : MapToResponse(entity);
        }

        /// <summary>
        /// Get tax code by code (async)
        /// </summary>
        public async Task<TaxCodeResponseDto> GetByCodeAsync(string code, long? companyId = null)
        {
            var entity = await _taxCodeRepository.GetByCodeAsync(code, companyId);
            return entity == null ? null : await MapToResponseAsync(entity);
        }

        /// <summary>
        /// Create tax code
        /// </summary>
        public TaxCodeResponseDto Create(CreateTaxCodeDto dto)
        {
            // Validate unique code
            if (_taxCodeRepository.CodeExists(dto.Code, dto.CompanyId))
            {
                throw new AppException(ExceptionCode.FINANCE_TAX_CODE_DUPLICATE)
                    .WithParameter("Code", dto.Code);
            }

            var entity = dto.Adapt<TaxCode>();
            entity.Id = Guid.NewGuid();
            entity.Create_time = DateTime.Now;

            _taxCodeRepository.InsertTaxCode(entity);

            // Create initial rate if provided
            if (dto.Rate > 0)
            {
                var rate = new TaxRate
                {
                    Id = Guid.NewGuid(),
                    TaxCodeId = entity.Id,
                    Rate = dto.Rate,
                    EffectiveFrom = DateTime.Today,
                    EffectiveTo = null,
                    IsApproved = true,
                    ApprovedAt = DateTime.Now,
                    Create_time = DateTime.Now
                };
                _taxRateRepository.InsertTaxRate(rate);
            }

            return MapToResponse(entity);
        }

        /// <summary>
        /// Create tax code (async)
        /// </summary>
        public async Task<TaxCodeResponseDto> CreateAsync(CreateTaxCodeDto dto)
        {
            // Validate unique code
            if (await _taxCodeRepository.CodeExistsAsync(dto.Code, dto.CompanyId))
            {
                throw new AppException(ExceptionCode.FINANCE_TAX_CODE_DUPLICATE)
                    .WithParameter("Code", dto.Code);
            }

            var entity = dto.Adapt<TaxCode>();
            entity.Id = Guid.NewGuid();
            entity.Create_time = DateTime.Now;

            await _taxCodeRepository.InsertTaxCodeAsync(entity);

            // Create initial rate if provided
            if (dto.Rate > 0)
            {
                var rate = new TaxRate
                {
                    Id = Guid.NewGuid(),
                    TaxCodeId = entity.Id,
                    Rate = dto.Rate,
                    EffectiveFrom = DateTime.Today,
                    EffectiveTo = null,
                    IsApproved = true,
                    ApprovedAt = DateTime.Now,
                    Create_time = DateTime.Now
                };
                await _taxRateRepository.InsertTaxRateAsync(rate);
            }

            return await MapToResponseAsync(entity);
        }

        /// <summary>
        /// Update tax code
        /// </summary>
        public TaxCodeResponseDto Update(UpdateTaxCodeDto dto)
        {
            var existing = _taxCodeRepository.GetById(dto.Id);
            if (existing == null)
            {
                throw new AppException(ExceptionCode.FINANCE_TAX_CODE_NOT_FOUND)
                    .WithParameter("TaxCodeId", dto.Id);
            }

            // Validate unique code
            if (_taxCodeRepository.CodeExists(dto.Code, dto.CompanyId, dto.Id))
            {
                throw new AppException(ExceptionCode.FINANCE_TAX_CODE_DUPLICATE)
                    .WithParameter("Code", dto.Code);
            }

            dto.Adapt(existing);
            existing.Update_time = DateTime.Now;

            _taxCodeRepository.UpdateTaxCode(existing);

            return MapToResponse(existing);
        }

        /// <summary>
        /// Update tax code (async)
        /// </summary>
        public async Task<TaxCodeResponseDto> UpdateAsync(UpdateTaxCodeDto dto)
        {
            var existing = await _taxCodeRepository.GetByIdAsync(dto.Id);
            if (existing == null)
            {
                throw new AppException(ExceptionCode.FINANCE_TAX_CODE_NOT_FOUND)
                    .WithParameter("TaxCodeId", dto.Id);
            }

            // Validate unique code
            if (await _taxCodeRepository.CodeExistsAsync(dto.Code, dto.CompanyId, dto.Id))
            {
                throw new AppException(ExceptionCode.FINANCE_TAX_CODE_DUPLICATE)
                    .WithParameter("Code", dto.Code);
            }

            dto.Adapt(existing);
            existing.Update_time = DateTime.Now;

            await _taxCodeRepository.UpdateTaxCodeAsync(existing);

            return await MapToResponseAsync(existing);
        }

        /// <summary>
        /// Delete tax codes
        /// </summary>
        public int Delete(Guid[] ids)
        {
            // Validate each can be deleted
            foreach (var id in ids)
            {
                if (!CanDelete(id))
                {
                    throw new AppException(ExceptionCode.FINANCE_TAX_CODE_IN_USE)
                        .WithParameter("TaxCodeId", id);
                }
            }

            return _taxCodeRepository.DeleteTaxCodes(ids);
        }

        /// <summary>
        /// Delete tax codes (async)
        /// </summary>
        public async Task<int> DeleteAsync(Guid[] ids)
        {
            foreach (var id in ids)
            {
                if (!await CanDeleteAsync(id))
                {
                    throw new AppException(ExceptionCode.FINANCE_TAX_CODE_IN_USE)
                        .WithParameter("TaxCodeId", id);
                }
            }

            return await _taxCodeRepository.DeleteTaxCodesAsync(ids);
        }

        #endregion

        #region Query Operations

        /// <summary>
        /// Get all active tax codes for dropdown
        /// </summary>
        public List<TaxCodeListItemDto> GetActiveTaxCodes(long? companyId = null)
        {
            var codes = _taxCodeRepository.GetActiveTaxCodes(companyId);
            return codes.Select(x => new TaxCodeListItemDto
            {
                Id = x.Id,
                Code = x.Code,
                NameEn = x.NameEn,
                NameAr = x.NameAr,
                Rate = GetEffectiveRate(x.Id, DateTime.Today),
                TaxType = x.TaxType,
                IsActive = x.IsActive
            }).ToList();
        }

        /// <summary>
        /// Get all active tax codes for dropdown (async)
        /// </summary>
        public async Task<List<TaxCodeListItemDto>> GetActiveTaxCodesAsync(long? companyId = null)
        {
            var codes = await _taxCodeRepository.GetActiveTaxCodesAsync(companyId);
            var result = new List<TaxCodeListItemDto>();

            foreach (var x in codes)
            {
                result.Add(new TaxCodeListItemDto
                {
                    Id = x.Id,
                    Code = x.Code,
                    NameEn = x.NameEn,
                    NameAr = x.NameAr,
                    Rate = await GetEffectiveRateAsync(x.Id, DateTime.Today),
                    TaxType = x.TaxType,
                    IsActive = x.IsActive
                });
            }

            return result;
        }

        /// <summary>
        /// Get default tax code
        /// </summary>
        public TaxCodeResponseDto GetDefaultTaxCode(long? companyId = null)
        {
            var entity = _taxCodeRepository.GetDefaultTaxCode(companyId);
            return entity == null ? null : MapToResponse(entity);
        }

        /// <summary>
        /// Get default tax code (async)
        /// </summary>
        public async Task<TaxCodeResponseDto> GetDefaultTaxCodeAsync(long? companyId = null)
        {
            var entity = await _taxCodeRepository.GetDefaultTaxCodeAsync(companyId);
            return entity == null ? null : await MapToResponseAsync(entity);
        }

        /// <summary>
        /// Get tax codes by type
        /// </summary>
        public List<TaxCodeListItemDto> GetByTaxType(TaxType taxType, long? companyId = null)
        {
            var codes = _taxCodeRepository.GetByTaxType(taxType, companyId);
            return codes.Select(x => new TaxCodeListItemDto
            {
                Id = x.Id,
                Code = x.Code,
                NameEn = x.NameEn,
                NameAr = x.NameAr,
                Rate = GetEffectiveRate(x.Id, DateTime.Today),
                TaxType = x.TaxType,
                IsActive = x.IsActive
            }).ToList();
        }

        #endregion

        #region Rate Operations

        /// <summary>
        /// Get effective rate for tax code on a date
        /// </summary>
        public decimal GetEffectiveRate(Guid taxCodeId, DateTime date)
        {
            var rate = _taxRateRepository.GetEffectiveRate(taxCodeId, date);
            return rate?.Rate ?? 0;
        }

        /// <summary>
        /// Get effective rate for tax code on a date (async)
        /// </summary>
        public async Task<decimal> GetEffectiveRateAsync(Guid taxCodeId, DateTime date)
        {
            var rate = await _taxRateRepository.GetEffectiveRateAsync(taxCodeId, date);
            return rate?.Rate ?? 0;
        }

        /// <summary>
        /// Get effective rate by code on a date
        /// </summary>
        public decimal GetEffectiveRateByCode(string code, DateTime date, long? companyId = null)
        {
            var taxCode = _taxCodeRepository.GetByCode(code, companyId);
            if (taxCode == null) return 0;

            return GetEffectiveRate(taxCode.Id, date);
        }

        /// <summary>
        /// Get effective rate by code on a date (async)
        /// </summary>
        public async Task<decimal> GetEffectiveRateByCodeAsync(string code, DateTime date, long? companyId = null)
        {
            var taxCode = await _taxCodeRepository.GetByCodeAsync(code, companyId);
            if (taxCode == null) return 0;

            return await GetEffectiveRateAsync(taxCode.Id, date);
        }

        #endregion

        #region Validation Operations

        /// <summary>
        /// Check if code exists
        /// </summary>
        public bool CodeExists(string code, long? companyId, Guid? excludeId = null)
        {
            return _taxCodeRepository.CodeExists(code, companyId, excludeId);
        }

        /// <summary>
        /// Check if code exists (async)
        /// </summary>
        public async Task<bool> CodeExistsAsync(string code, long? companyId, Guid? excludeId = null)
        {
            return await _taxCodeRepository.CodeExistsAsync(code, companyId, excludeId);
        }

        /// <summary>
        /// Check if tax code can be deleted
        /// </summary>
        public bool CanDelete(Guid id)
        {
            return !_taxCodeRepository.IsUsedInJournals(id);
        }

        /// <summary>
        /// Check if tax code can be deleted (async)
        /// </summary>
        public async Task<bool> CanDeleteAsync(Guid id)
        {
            return !await _taxCodeRepository.IsUsedInJournalsAsync(id);
        }

        #endregion

        #region Mapping Helpers

        private TaxCodeResponseDto MapToResponse(TaxCode entity)
        {
            var response = entity.Adapt<TaxCodeResponseDto>();
            response.EffectiveRate = GetEffectiveRate(entity.Id, DateTime.Today);
            response.EffectiveRateDisplay = $"{response.EffectiveRate:0.##}%";
            return response;
        }

        private async Task<TaxCodeResponseDto> MapToResponseAsync(TaxCode entity)
        {
            var response = entity.Adapt<TaxCodeResponseDto>();
            response.EffectiveRate = await GetEffectiveRateAsync(entity.Id, DateTime.Today);
            response.EffectiveRateDisplay = $"{response.EffectiveRate:0.##}%";
            return response;
        }

        #endregion
    }
}
