using Infrastructure.Attribute;
using Infrastructure.Exceptions;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ZR.Model;
using ZR.Model.HR.EmployeeCore;
using ZR.Model.HR.Dto.EmployeeCore;
using ZR.Service.HR.IService;

namespace ZR.Service.HR.EmployeeCore
{
    /// <summary>
    /// Enhanced Person Name Service with business logic (Partial Class Extension)
    /// </summary>
    public partial class HrPersonNameService
    {
        #region Create/Update with Business Logic

        /// <summary>
        /// Create person name with validation and primary flag management
        /// </summary>
        public async Task<long> CreateAsync(CreatePersonNameDto dto)
        {
            // Validation
            ValidatePersonNameDto(dto);

            // Ensure person exists
            var personExists = await Context.Queryable<HrPerson>()
                .AnyAsync(p => p.PersonId == dto.PersonId);
            if (!personExists)
            {
                throw new AppException(ExceptionCode.NOT_FOUND, "Person not found")
                    .WithParameter("PersonId", dto.PersonId);
            }

            // Validate effective date range
            ValidateEffectiveDateRange(dto.EffectiveFrom, dto.EffectiveTo);

            // If this is being set as primary, unset other primaries
            if (dto.IsPrimary)
            {
                await UnsetOtherPrimariesAsync(dto.PersonId, dto.EffectiveFrom);
            }

            var entity = new HrPersonName
            {
                PersonId = dto.PersonId,
                GivenName = dto.GivenName,
                MiddleName = dto.MiddleName,
                FamilyName = dto.FamilyName,
                FullNameAr = dto.FullNameAr,
                EffectiveFrom = dto.EffectiveFrom,
                EffectiveTo = dto.EffectiveTo,
                IsPrimary = dto.IsPrimary,
                Create_time = DateTime.Now
            };

            var result = await Context.Insertable(entity)
                .ExecuteReturnSnowflakeIdAsync();

            return result;
        }

        /// <summary>
        /// Update person name with validation and primary flag management
        /// </summary>
        public async Task<int> UpdateAsync(UpdatePersonNameDto dto)
        {
            // Validation
            ValidatePersonNameDto(dto);

            // Ensure entity exists
            var existing = await GetByIdAsync(dto.PersonNameId);
            if (existing == null)
            {
                throw new AppException(ExceptionCode.NOT_FOUND, "Person name not found")
                    .WithParameter("PersonNameId", dto.PersonNameId);
            }

            // Validate effective date range
            var effectiveFrom = dto.EffectiveFrom ?? existing.EffectiveFrom;
            ValidateEffectiveDateRange(effectiveFrom, dto.EffectiveTo);

            // If this is being set as primary, unset other primaries (except this one)
            if (dto.IsPrimary && !existing.IsPrimary)
            {
                await UnsetOtherPrimariesAsync(existing.PersonId, effectiveFrom, dto.PersonNameId);
            }

            existing.GivenName = dto.GivenName;
            existing.MiddleName = dto.MiddleName;
            existing.FamilyName = dto.FamilyName;
            existing.FullNameAr = dto.FullNameAr;
            existing.EffectiveFrom = effectiveFrom;
            existing.EffectiveTo = dto.EffectiveTo;
            existing.IsPrimary = dto.IsPrimary;
            existing.Update_time = DateTime.Now;

            return await Context.Updateable(existing).ExecuteCommandAsync();
        }

        #endregion

        #region Business Logic Helpers

        /// <summary>
        /// Get current effective name for a person
        /// </summary>
        public async Task<HrPersonName> GetCurrentEffectiveNameAsync(long personId)
        {
            var now = DateTime.Now;
            return await Context.Queryable<HrPersonName>()
                .Where(n => n.PersonId == personId)
                .Where(n => n.EffectiveFrom <= now)
                .Where(n => n.EffectiveTo == null || n.EffectiveTo > now)
                .Where(n => n.IsPrimary == true)
                .OrderByDescending(n => n.EffectiveFrom)
                .FirstAsync();
        }

        /// <summary>
        /// Get primary name for a person at a specific date
        /// </summary>
        public async Task<HrPersonName> GetPrimaryNameAtDateAsync(long personId, DateTime asOfDate)
        {
            return await Context.Queryable<HrPersonName>()
                .Where(n => n.PersonId == personId)
                .Where(n => n.EffectiveFrom <= asOfDate)
                .Where(n => n.EffectiveTo == null || n.EffectiveTo > asOfDate)
                .Where(n => n.IsPrimary == true)
                .OrderByDescending(n => n.EffectiveFrom)
                .FirstAsync();
        }

        /// <summary>
        /// Unset other primaries when setting a new primary
        /// </summary>
        private async Task UnsetOtherPrimariesAsync(long personId, DateTime effectiveFrom, long? excludePersonNameId = null)
        {
            var query = Context.Queryable<HrPersonName>()
                .Where(n => n.PersonId == personId && n.IsPrimary == true);

            if (excludePersonNameId.HasValue)
            {
                query = query.Where(n => n.PersonNameId != excludePersonNameId.Value);
            }

            var otherPrimaries = await query.ToListAsync();

            foreach (var name in otherPrimaries)
            {
                // Set EffectiveTo to the day before new primary becomes effective
                name.EffectiveTo = effectiveFrom.AddDays(-1);
                name.IsPrimary = false;
                name.Update_time = DateTime.Now;
            }

            if (otherPrimaries.Any())
            {
                await Context.Updateable(otherPrimaries).ExecuteCommandAsync();
            }
        }

        /// <summary>
        /// Validate effective date range
        /// </summary>
        private void ValidateEffectiveDateRange(DateTime effectiveFrom, DateTime? effectiveTo)
        {
            if (effectiveTo.HasValue && effectiveTo.Value <= effectiveFrom)
            {
                throw new AppException(ExceptionCode.INVALID_PARAMETER, "Effective date range invalid: EffectiveTo must be greater than EffectiveFrom")
                    .WithParameter("EffectiveFrom", effectiveFrom)
                    .WithParameter("EffectiveTo", effectiveTo);
            }
        }

        /// <summary>
        /// Validate person name DTO
        /// </summary>
        private void ValidatePersonNameDto(object dto)
        {
            if (dto is CreatePersonNameDto createDto)
            {
                if (createDto.PersonId <= 0)
                {
                    throw new AppException(ExceptionCode.INVALID_PARAMETER, "PersonId is required");
                }

                if (string.IsNullOrWhiteSpace(createDto.GivenName) &&
                    string.IsNullOrWhiteSpace(createDto.FamilyName) &&
                    string.IsNullOrWhiteSpace(createDto.FullNameAr))
                {
                    throw new AppException(ExceptionCode.INVALID_PARAMETER, "Person name is required");
                }
            }
            else if (dto is UpdatePersonNameDto updateDto)
            {
                if (updateDto.PersonNameId <= 0)
                {
                    throw new AppException(ExceptionCode.INVALID_PARAMETER, "PersonNameId is required");
                }

                if (string.IsNullOrWhiteSpace(updateDto.GivenName) &&
                    string.IsNullOrWhiteSpace(updateDto.FamilyName) &&
                    string.IsNullOrWhiteSpace(updateDto.FullNameAr))
                {
                    throw new AppException(ExceptionCode.INVALID_PARAMETER, "Person name is required");
                }
            }
        }

        #endregion
    }
}
