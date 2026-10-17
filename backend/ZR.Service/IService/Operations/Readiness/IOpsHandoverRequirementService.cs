using System.Collections.Generic;
using System.Threading.Tasks;
using ZR.Model.Operations.Readiness;
using ZR.Model.Operations.Readiness.Dto;

namespace ZR.Service.Operations.Readiness
{
    public interface IOpsHandoverRequirementService
    {
        Task<List<OpsHandoverRequirement>> GetActiveRequirementsAsync();
        Task<PagedInfo<OpsHandoverRequirement>> GetRequirementListAsync(HandoverRequirementQueryDto query);
        Task<OpsHandoverRequirement> GetByIdAsync(long requirementId);
        Task<OpsHandoverRequirement> CreateRequirementAsync(HandoverRequirementDto dto, string userName);
        Task<int> UpdateRequirementAsync(HandoverRequirementDto dto, string userName);
        Task<int> SoftDeleteRequirementAsync(long requirementId, string userName);
        Task<RequirementValidationDto> ValidatePercentagesAsync();
    }
}
