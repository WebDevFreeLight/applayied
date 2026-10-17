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
    /// Tax Group Service Implementation
    /// تنفيذ خدمة مجموعات الضرائب
    /// </summary>
    [AppService(ServiceType = typeof(ITaxGroupService), ServiceLifetime = LifeTime.Transient)]
    public class TaxGroupService : ITaxGroupService
    {
        private readonly ITaxGroupRepository _taxGroupRepository;
        private readonly ITaxCodeRepository _taxCodeRepository;
        private readonly ITaxRateRepository _taxRateRepository;

        public TaxGroupService(
            ITaxGroupRepository taxGroupRepository,
            ITaxCodeRepository taxCodeRepository,
            ITaxRateRepository taxRateRepository)
        {
            _taxGroupRepository = taxGroupRepository;
            _taxCodeRepository = taxCodeRepository;
            _taxRateRepository = taxRateRepository;
        }

        #region CRUD Operations

        /// <summary>
        /// Get paginated list of tax groups
        /// </summary>
        public PagedInfo<TaxGroupResponseDto> GetList(TaxGroupQueryDto query)
        {
            var pager = new PagerInfo
            {
                PageNum = query.PageNum,
                PageSize = query.PageSize
            };

            var filter = new TaxGroup
            {
                CompanyID = query.CompanyId,
                Code = query.Code,
                NameEn = query.NameEn,
                IsActive = query.IsActive ?? false
            };

            var result = _taxGroupRepository.GetList(filter, pager);

            return new PagedInfo<TaxGroupResponseDto>
            {
                TotalNum = result.TotalNum,
                PageIndex = result.PageIndex,
                PageSize = result.PageSize,
                TotalPage = result.TotalPage,
                Result = result.Result.Select(x => MapToResponse(x)).ToList()
            };
        }

        /// <summary>
        /// Get tax group by ID
        /// </summary>
        public TaxGroupResponseDto GetById(Guid id)
        {
            var entity = _taxGroupRepository.GetById(id, true);
            return entity == null ? null : MapToResponse(entity);
        }

        /// <summary>
        /// Get tax group by ID (async)
        /// </summary>
        public async Task<TaxGroupResponseDto> GetByIdAsync(Guid id)
        {
            var entity = await _taxGroupRepository.GetByIdAsync(id, true);
            return entity == null ? null : await MapToResponseAsync(entity);
        }

        /// <summary>
        /// Get tax group by code
        /// </summary>
        public TaxGroupResponseDto GetByCode(string code, long? companyId = null)
        {
            var entity = _taxGroupRepository.GetByCode(code, companyId);
            return entity == null ? null : MapToResponse(entity);
        }

        /// <summary>
        /// Get tax group by code (async)
        /// </summary>
        public async Task<TaxGroupResponseDto> GetByCodeAsync(string code, long? companyId = null)
        {
            var entity = await _taxGroupRepository.GetByCodeAsync(code, companyId);
            return entity == null ? null : await MapToResponseAsync(entity);
        }

        /// <summary>
        /// Create tax group
        /// </summary>
        public TaxGroupResponseDto Create(CreateTaxGroupDto dto)
        {
            // Validate unique code
            if (_taxGroupRepository.CodeExists(dto.Code, dto.CompanyId))
            {
                throw new AppException(ExceptionCode.FINANCE_TAX_GROUP_CODE_DUPLICATE)
                    .WithParameter("Code", dto.Code);
            }

            // Validate all tax codes exist (if lines provided)
            if (dto.Lines != null && dto.Lines.Count > 0)
            {
                foreach (var line in dto.Lines)
                {
                    var taxCode = _taxCodeRepository.GetById(line.TaxCodeId);
                    if (taxCode == null)
                    {
                        throw new AppException(ExceptionCode.FINANCE_TAX_CODE_NOT_FOUND)
                            .WithParameter("TaxCodeId", line.TaxCodeId);
                    }
                }
            }

            var entity = dto.Adapt<TaxGroup>();
            entity.Id = Guid.NewGuid();
            entity.Create_time = DateTime.Now;

            // Map lines (handle null/empty)
            entity.Lines = dto.Lines?.Select(l => new TaxGroupLine
            {
                Id = Guid.NewGuid(),
                TaxGroupId = entity.Id,
                TaxCodeId = l.TaxCodeId,
                Sequence = l.Sequence,
                OverrideRate = l.OverrideRate,
                IsActive = l.IsActive
            }).ToList() ?? new List<TaxGroupLine>();

            _taxGroupRepository.InsertTaxGroup(entity);

            return MapToResponse(entity);
        }

        /// <summary>
        /// Create tax group (async)
        /// </summary>
        public async Task<TaxGroupResponseDto> CreateAsync(CreateTaxGroupDto dto)
        {
            if (await _taxGroupRepository.CodeExistsAsync(dto.Code, dto.CompanyId))
            {
                throw new AppException(ExceptionCode.FINANCE_TAX_GROUP_CODE_DUPLICATE)
                    .WithParameter("Code", dto.Code);
            }

            // Validate all tax codes exist (if lines provided)
            if (dto.Lines != null && dto.Lines.Count > 0)
            {
                foreach (var line in dto.Lines)
                {
                    var taxCode = await _taxCodeRepository.GetByIdAsync(line.TaxCodeId);
                    if (taxCode == null)
                    {
                        throw new AppException(ExceptionCode.FINANCE_TAX_CODE_NOT_FOUND)
                            .WithParameter("TaxCodeId", line.TaxCodeId);
                    }
                }
            }

            var entity = dto.Adapt<TaxGroup>();
            entity.Id = Guid.NewGuid();
            entity.Create_time = DateTime.Now;

            // Map lines (handle null/empty)
            entity.Lines = dto.Lines?.Select(l => new TaxGroupLine
            {
                Id = Guid.NewGuid(),
                TaxGroupId = entity.Id,
                TaxCodeId = l.TaxCodeId,
                Sequence = l.Sequence,
                OverrideRate = l.OverrideRate,
                IsActive = l.IsActive
            }).ToList() ?? new List<TaxGroupLine>();

            await _taxGroupRepository.InsertTaxGroupAsync(entity);

            return await MapToResponseAsync(entity);
        }

        /// <summary>
        /// Update tax group
        /// </summary>
        public TaxGroupResponseDto Update(UpdateTaxGroupDto dto)
        {
            var existing = _taxGroupRepository.GetById(dto.Id, false);
            if (existing == null)
            {
                throw new AppException(ExceptionCode.FINANCE_TAX_GROUP_NOT_FOUND)
                    .WithParameter("TaxGroupId", dto.Id);
            }

            if (_taxGroupRepository.CodeExists(dto.Code, dto.CompanyId, dto.Id))
            {
                throw new AppException(ExceptionCode.FINANCE_TAX_GROUP_CODE_DUPLICATE)
                    .WithParameter("Code", dto.Code);
            }

            // Validate all tax codes exist (if lines provided)
            if (dto.Lines != null && dto.Lines.Count > 0)
            {
                foreach (var line in dto.Lines)
                {
                    var taxCode = _taxCodeRepository.GetById(line.TaxCodeId);
                    if (taxCode == null)
                    {
                        throw new AppException(ExceptionCode.FINANCE_TAX_CODE_NOT_FOUND)
                            .WithParameter("TaxCodeId", line.TaxCodeId);
                    }
                }
            }

            dto.Adapt(existing);
            existing.Update_time = DateTime.Now;

            // Map lines (handle null/empty)
            existing.Lines = dto.Lines?.Select(l => new TaxGroupLine
            {
                Id = l.Id ?? Guid.NewGuid(),
                TaxGroupId = existing.Id,
                TaxCodeId = l.TaxCodeId,
                Sequence = l.Sequence,
                OverrideRate = l.OverrideRate,
                IsActive = l.IsActive
            }).ToList() ?? new List<TaxGroupLine>();

            _taxGroupRepository.UpdateTaxGroup(existing);

            return MapToResponse(existing);
        }

        /// <summary>
        /// Update tax group (async)
        /// </summary>
        public async Task<TaxGroupResponseDto> UpdateAsync(UpdateTaxGroupDto dto)
        {
            var existing = await _taxGroupRepository.GetByIdAsync(dto.Id, false);
            if (existing == null)
            {
                throw new AppException(ExceptionCode.FINANCE_TAX_GROUP_NOT_FOUND)
                    .WithParameter("TaxGroupId", dto.Id);
            }

            if (await _taxGroupRepository.CodeExistsAsync(dto.Code, dto.CompanyId, dto.Id))
            {
                throw new AppException(ExceptionCode.FINANCE_TAX_GROUP_CODE_DUPLICATE)
                    .WithParameter("Code", dto.Code);
            }

            // Validate all tax codes exist (if lines provided)
            if (dto.Lines != null && dto.Lines.Count > 0)
            {
                foreach (var line in dto.Lines)
                {
                    var taxCode = await _taxCodeRepository.GetByIdAsync(line.TaxCodeId);
                    if (taxCode == null)
                    {
                        throw new AppException(ExceptionCode.FINANCE_TAX_CODE_NOT_FOUND)
                            .WithParameter("TaxCodeId", line.TaxCodeId);
                    }
                }
            }

            dto.Adapt(existing);
            existing.Update_time = DateTime.Now;

            // Map lines (handle null/empty)
            existing.Lines = dto.Lines?.Select(l => new TaxGroupLine
            {
                Id = l.Id ?? Guid.NewGuid(),
                TaxGroupId = existing.Id,
                TaxCodeId = l.TaxCodeId,
                Sequence = l.Sequence,
                OverrideRate = l.OverrideRate,
                IsActive = l.IsActive
            }).ToList() ?? new List<TaxGroupLine>();

            await _taxGroupRepository.UpdateTaxGroupAsync(existing);

            return await MapToResponseAsync(existing);
        }

        /// <summary>
        /// Delete tax groups
        /// </summary>
        public int Delete(Guid[] ids)
        {
            foreach (var id in ids)
            {
                if (!CanDelete(id))
                {
                    throw new AppException(ExceptionCode.FINANCE_TAX_GROUP_IN_USE)
                        .WithParameter("TaxGroupId", id);
                }
            }

            return _taxGroupRepository.DeleteTaxGroups(ids);
        }

        /// <summary>
        /// Delete tax groups (async)
        /// </summary>
        public async Task<int> DeleteAsync(Guid[] ids)
        {
            foreach (var id in ids)
            {
                if (!await CanDeleteAsync(id))
                {
                    throw new AppException(ExceptionCode.FINANCE_TAX_GROUP_IN_USE)
                        .WithParameter("TaxGroupId", id);
                }
            }

            return await _taxGroupRepository.DeleteTaxGroupsAsync(ids);
        }

        #endregion

        #region Query Operations

        /// <summary>
        /// Get all active tax groups for dropdown
        /// </summary>
        public List<TaxGroupListItemDto> GetActiveTaxGroups(long? companyId = null)
        {
            var groups = _taxGroupRepository.GetActiveTaxGroups(companyId);
            return groups.Select(x => new TaxGroupListItemDto
            {
                Id = x.Id,
                Code = x.Code,
                NameEn = x.NameEn,
                NameAr = x.NameAr,
                CombinedRate = _taxGroupRepository.CalculateCombinedRate(x.Id),
                IsCompound = x.IsCompound,
                IsActive = x.IsActive,
                TaxCodeCount = x.Lines?.Count ?? 0
            }).ToList();
        }

        /// <summary>
        /// Get all active tax groups for dropdown (async)
        /// </summary>
        public async Task<List<TaxGroupListItemDto>> GetActiveTaxGroupsAsync(long? companyId = null)
        {
            var groups = await _taxGroupRepository.GetActiveTaxGroupsAsync(companyId);
            var result = new List<TaxGroupListItemDto>();

            foreach (var x in groups)
            {
                result.Add(new TaxGroupListItemDto
                {
                    Id = x.Id,
                    Code = x.Code,
                    NameEn = x.NameEn,
                    NameAr = x.NameAr,
                    CombinedRate = await _taxGroupRepository.CalculateCombinedRateAsync(x.Id),
                    IsCompound = x.IsCompound,
                    IsActive = x.IsActive,
                    TaxCodeCount = x.Lines?.Count ?? 0
                });
            }

            return result;
        }

        /// <summary>
        /// Get tax groups containing a tax code
        /// </summary>
        public List<TaxGroupListItemDto> GetGroupsByTaxCode(Guid taxCodeId)
        {
            var groups = _taxGroupRepository.GetGroupsByTaxCode(taxCodeId);
            return groups.Select(x => new TaxGroupListItemDto
            {
                Id = x.Id,
                Code = x.Code,
                NameEn = x.NameEn,
                NameAr = x.NameAr,
                CombinedRate = _taxGroupRepository.CalculateCombinedRate(x.Id),
                IsCompound = x.IsCompound,
                IsActive = x.IsActive,
                TaxCodeCount = x.Lines?.Count ?? 0
            }).ToList();
        }

        #endregion

        #region Line Operations

        /// <summary>
        /// Add a tax code to a group
        /// </summary>
        public TaxGroupResponseDto AddTaxCode(Guid groupId, TaxGroupLineDto lineDto)
        {
            var group = _taxGroupRepository.GetById(groupId, true);
            if (group == null)
            {
                throw new AppException(ExceptionCode.FINANCE_TAX_GROUP_NOT_FOUND)
                    .WithParameter("TaxGroupId", groupId);
            }

            var taxCode = _taxCodeRepository.GetById(lineDto.TaxCodeId);
            if (taxCode == null)
            {
                throw new AppException(ExceptionCode.FINANCE_TAX_CODE_NOT_FOUND)
                    .WithParameter("TaxCodeId", lineDto.TaxCodeId);
            }

            var line = new TaxGroupLine
            {
                Id = Guid.NewGuid(),
                TaxGroupId = groupId,
                TaxCodeId = lineDto.TaxCodeId,
                Sequence = lineDto.Sequence,
                OverrideRate = lineDto.OverrideRate,
                IsActive = lineDto.IsActive
            };

            _taxGroupRepository.AddGroupLine(line);

            return GetById(groupId);
        }

        /// <summary>
        /// Add a tax code to a group (async)
        /// </summary>
        public async Task<TaxGroupResponseDto> AddTaxCodeAsync(Guid groupId, TaxGroupLineDto lineDto)
        {
            var group = await _taxGroupRepository.GetByIdAsync(groupId, true);
            if (group == null)
            {
                throw new AppException(ExceptionCode.FINANCE_TAX_GROUP_NOT_FOUND)
                    .WithParameter("TaxGroupId", groupId);
            }

            var taxCode = await _taxCodeRepository.GetByIdAsync(lineDto.TaxCodeId);
            if (taxCode == null)
            {
                throw new AppException(ExceptionCode.FINANCE_TAX_CODE_NOT_FOUND)
                    .WithParameter("TaxCodeId", lineDto.TaxCodeId);
            }

            var line = new TaxGroupLine
            {
                Id = Guid.NewGuid(),
                TaxGroupId = groupId,
                TaxCodeId = lineDto.TaxCodeId,
                Sequence = lineDto.Sequence,
                OverrideRate = lineDto.OverrideRate,
                IsActive = lineDto.IsActive
            };

            await _taxGroupRepository.AddGroupLineAsync(line);

            return await GetByIdAsync(groupId);
        }

        /// <summary>
        /// Remove a tax code from a group
        /// </summary>
        public TaxGroupResponseDto RemoveTaxCode(Guid groupId, Guid lineId)
        {
            _taxGroupRepository.RemoveGroupLine(lineId);
            return GetById(groupId);
        }

        /// <summary>
        /// Remove a tax code from a group (async)
        /// </summary>
        public async Task<TaxGroupResponseDto> RemoveTaxCodeAsync(Guid groupId, Guid lineId)
        {
            await _taxGroupRepository.RemoveGroupLineAsync(lineId);
            return await GetByIdAsync(groupId);
        }

        /// <summary>
        /// Update line sequence/override in a group
        /// </summary>
        public TaxGroupResponseDto UpdateLine(Guid groupId, TaxGroupLineDto lineDto)
        {
            if (!lineDto.Id.HasValue)
            {
                throw new AppException(ExceptionCode.FINANCE_TAX_GROUP_LINE_ID_REQUIRED);
            }

            var line = new TaxGroupLine
            {
                Id = lineDto.Id.Value,
                TaxGroupId = groupId,
                TaxCodeId = lineDto.TaxCodeId,
                Sequence = lineDto.Sequence,
                OverrideRate = lineDto.OverrideRate,
                IsActive = lineDto.IsActive
            };

            _taxGroupRepository.UpdateGroupLine(line);

            return GetById(groupId);
        }

        /// <summary>
        /// Update line sequence/override in a group (async)
        /// </summary>
        public async Task<TaxGroupResponseDto> UpdateLineAsync(Guid groupId, TaxGroupLineDto lineDto)
        {
            if (!lineDto.Id.HasValue)
            {
                throw new AppException(ExceptionCode.FINANCE_TAX_GROUP_LINE_ID_REQUIRED);
            }

            var line = new TaxGroupLine
            {
                Id = lineDto.Id.Value,
                TaxGroupId = groupId,
                TaxCodeId = lineDto.TaxCodeId,
                Sequence = lineDto.Sequence,
                OverrideRate = lineDto.OverrideRate,
                IsActive = lineDto.IsActive
            };

            await _taxGroupRepository.UpdateGroupLineAsync(line);

            return await GetByIdAsync(groupId);
        }

        #endregion

        #region Calculation Operations

        /// <summary>
        /// Calculate combined rate for a group
        /// </summary>
        public decimal CalculateCombinedRate(Guid groupId, DateTime? effectiveDate = null)
        {
            return _taxGroupRepository.CalculateCombinedRate(groupId, effectiveDate);
        }

        /// <summary>
        /// Calculate combined rate for a group (async)
        /// </summary>
        public async Task<decimal> CalculateCombinedRateAsync(Guid groupId, DateTime? effectiveDate = null)
        {
            return await _taxGroupRepository.CalculateCombinedRateAsync(groupId, effectiveDate);
        }

        /// <summary>
        /// Calculate combined rate by code
        /// </summary>
        public decimal CalculateCombinedRateByCode(string code, DateTime? effectiveDate = null, long? companyId = null)
        {
            var group = _taxGroupRepository.GetByCode(code, companyId);
            if (group == null) return 0;
            return CalculateCombinedRate(group.Id, effectiveDate);
        }

        /// <summary>
        /// Calculate combined rate by code (async)
        /// </summary>
        public async Task<decimal> CalculateCombinedRateByCodeAsync(string code, DateTime? effectiveDate = null, long? companyId = null)
        {
            var group = await _taxGroupRepository.GetByCodeAsync(code, companyId);
            if (group == null) return 0;
            return await CalculateCombinedRateAsync(group.Id, effectiveDate);
        }

        #endregion

        #region Validation Operations

        /// <summary>
        /// Check if code exists
        /// </summary>
        public bool CodeExists(string code, long? companyId, Guid? excludeId = null)
        {
            return _taxGroupRepository.CodeExists(code, companyId, excludeId);
        }

        /// <summary>
        /// Check if code exists (async)
        /// </summary>
        public async Task<bool> CodeExistsAsync(string code, long? companyId, Guid? excludeId = null)
        {
            return await _taxGroupRepository.CodeExistsAsync(code, companyId, excludeId);
        }

        /// <summary>
        /// Check if tax group can be deleted
        /// </summary>
        public bool CanDelete(Guid id)
        {
            return !_taxGroupRepository.IsUsedInJournals(id);
        }

        /// <summary>
        /// Check if tax group can be deleted (async)
        /// </summary>
        public async Task<bool> CanDeleteAsync(Guid id)
        {
            return !await _taxGroupRepository.IsUsedInJournalsAsync(id);
        }

        #endregion

        #region Mapping Helpers

        private TaxGroupResponseDto MapToResponse(TaxGroup entity)
        {
            var response = entity.Adapt<TaxGroupResponseDto>();
            response.CombinedRate = _taxGroupRepository.CalculateCombinedRate(entity.Id);
            response.CombinedRateDisplay = $"{response.CombinedRate:0.##}%";

            response.Lines = entity.Lines?.Select(l =>
            {
                var taxCode = _taxCodeRepository.GetById(l.TaxCodeId);
                var effectiveRate = taxCode != null
                    ? _taxRateRepository.GetEffectiveRate(taxCode.Id, DateTime.Today)?.Rate ?? 0
                    : 0;

                return new TaxGroupLineResponseDto
                {
                    Id = l.Id,
                    TaxGroupId = l.TaxGroupId,
                    TaxCodeId = l.TaxCodeId,
                    TaxCode = taxCode?.Code,
                    TaxCodeNameEn = taxCode?.NameEn,
                    TaxCodeNameAr = taxCode?.NameAr,
                    Sequence = l.Sequence,
                    OverrideRate = l.OverrideRate,
                    EffectiveRate = l.OverrideRate ?? effectiveRate,
                    EffectiveRateDisplay = $"{(l.OverrideRate ?? effectiveRate):0.##}%",
                    IsActive = l.IsActive
                };
            }).ToList() ?? new List<TaxGroupLineResponseDto>();

            return response;
        }

        private async Task<TaxGroupResponseDto> MapToResponseAsync(TaxGroup entity)
        {
            var response = entity.Adapt<TaxGroupResponseDto>();
            response.CombinedRate = await _taxGroupRepository.CalculateCombinedRateAsync(entity.Id);
            response.CombinedRateDisplay = $"{response.CombinedRate:0.##}%";

            var lines = new List<TaxGroupLineResponseDto>();
            if (entity.Lines != null)
            {
                foreach (var l in entity.Lines)
                {
                    var taxCode = await _taxCodeRepository.GetByIdAsync(l.TaxCodeId);
                    var effectiveRate = taxCode != null
                        ? (await _taxRateRepository.GetEffectiveRateAsync(taxCode.Id, DateTime.Today))?.Rate ?? 0
                        : 0;

                    lines.Add(new TaxGroupLineResponseDto
                    {
                        Id = l.Id,
                        TaxGroupId = l.TaxGroupId,
                        TaxCodeId = l.TaxCodeId,
                        TaxCode = taxCode?.Code,
                        TaxCodeNameEn = taxCode?.NameEn,
                        TaxCodeNameAr = taxCode?.NameAr,
                        Sequence = l.Sequence,
                        OverrideRate = l.OverrideRate,
                        EffectiveRate = l.OverrideRate ?? effectiveRate,
                        EffectiveRateDisplay = $"{(l.OverrideRate ?? effectiveRate):0.##}%",
                        IsActive = l.IsActive
                    });
                }
            }

            response.Lines = lines;
            return response;
        }

        #endregion
    }
}
