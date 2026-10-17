using Infrastructure.Attribute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZR.Model.Operations.Readiness;
using ZR.Model.Operations.Readiness.Dto;
using ZR.Repository;

namespace ZR.Service.Operations.Readiness
{
    [AppService(ServiceType = typeof(IOpsHandoverRequirementService), ServiceLifetime = LifeTime.Transient)]
    public class OpsHandoverRequirementService : BaseService<OpsHandoverRequirement>, IOpsHandoverRequirementService
    {
        public async Task<List<OpsHandoverRequirement>> GetActiveRequirementsAsync()
        {
            return await Context.Queryable<OpsHandoverRequirement>()
                .Where(r => r.IsActive)
                .OrderBy(r => r.SortOrder)
                .ToListAsync();
        }

        public async Task<PagedInfo<OpsHandoverRequirement>> GetRequirementListAsync(HandoverRequirementQueryDto query)
        {
            var pageQuery = Context.Queryable<OpsHandoverRequirement>()
                .WhereIF(query.IsActive.HasValue, r => r.IsActive == query.IsActive.Value)
                .WhereIF(!string.IsNullOrEmpty(query.Code), r => r.Code.Contains(query.Code))
                .OrderBy(r => r.SortOrder);

            return await pageQuery.ToPageAsync(query);
        }

        public new async Task<OpsHandoverRequirement> GetByIdAsync(long requirementId)
        {
            return await Context.Queryable<OpsHandoverRequirement>()
                .Where(r => r.RequirementId == requirementId)
                .FirstAsync();
        }

        public async Task<OpsHandoverRequirement> CreateRequirementAsync(HandoverRequirementDto dto, string userName)
        {
            var requirement = new OpsHandoverRequirement
            {
                RequirementId = SnowFlakeSingle.Instance.NextId(),
                Code = dto.Code,
                NameEn = dto.NameEn,
                NameAr = dto.NameAr,
                DescriptionEn = dto.DescriptionEn,
                DescriptionAr = dto.DescriptionAr,
                Percentage = dto.Percentage,
                PhotoRequired = dto.PhotoRequired,
                IsBlocking = dto.IsBlocking,
                SortOrder = dto.SortOrder,
                IsActive = dto.IsActive,
                Create_by = userName,
                Create_time = DateTime.Now
            };

            await Context.Insertable(requirement).ExecuteCommandAsync();
            return requirement;
        }

        public async Task<int> UpdateRequirementAsync(HandoverRequirementDto dto, string userName)
        {
            if (!dto.RequirementId.HasValue) return 0;

            var requirement = await GetByIdAsync(dto.RequirementId.Value);
            if (requirement == null) return 0;

            requirement.Code = dto.Code;
            requirement.NameEn = dto.NameEn;
            requirement.NameAr = dto.NameAr;
            requirement.DescriptionEn = dto.DescriptionEn;
            requirement.DescriptionAr = dto.DescriptionAr;
            requirement.Percentage = dto.Percentage;
            requirement.PhotoRequired = dto.PhotoRequired;
            requirement.IsBlocking = dto.IsBlocking;
            requirement.SortOrder = dto.SortOrder;
            requirement.IsActive = dto.IsActive;
            requirement.Update_by = userName;
            requirement.Update_time = DateTime.Now;

            return await Context.Updateable(requirement).ExecuteCommandAsync();
        }

        public async Task<int> SoftDeleteRequirementAsync(long requirementId, string userName)
        {
            var requirement = await GetByIdAsync(requirementId);
            if (requirement == null) return 0;

            requirement.IsActive = false;
            requirement.Update_by = userName;
            requirement.Update_time = DateTime.Now;

            return await Context.Updateable(requirement).ExecuteCommandAsync();
        }

        public async Task<RequirementValidationDto> ValidatePercentagesAsync()
        {
            var activeRequirements = await GetActiveRequirementsAsync();
            var total = activeRequirements.Sum(r => r.Percentage);

            return new RequirementValidationDto
            {
                TotalPercentage = total,
                IsValid = total == 100.0m,
                ActiveCount = activeRequirements.Count
            };
        }
    }
}
