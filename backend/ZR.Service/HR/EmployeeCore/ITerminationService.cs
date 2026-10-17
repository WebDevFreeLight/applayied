using System.Threading.Tasks;
using ZR.Model;
using ZR.Model.HR.Dto;
using ZR.Model.HR.EmployeeCore;

namespace ZR.Service.HR.IService
{
    public interface ITerminationService
    {
        Task<PagedInfo<HrTermination>> GetListAsync(TerminationQueryDto query);
        Task<HrTermination> GetByIdAsync(long terminationId);
        Task<HrTermination> InitiateTerminationAsync(TerminationRequestDto request, string initiatedBy);
        Task<bool> ApproveTerminationAsync(TerminationApprovalDto approval, long approverId);
        Task<TerminationSettlementDto> CalculateSettlementAsync(long terminationId);
        Task<bool> UpdateChecklistAsync(TerminationChecklistDto checklist);
        Task<bool> CompleteTerminationAsync(long terminationId);
        Task<bool> CancelTerminationAsync(long terminationId, string reason);
    }
}
