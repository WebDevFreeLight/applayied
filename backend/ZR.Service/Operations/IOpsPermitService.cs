using ZR.Model.Operations;
using ZR.Model.Operations.Dto;

namespace ZR.Service.Operations
{
    public interface IOpsPermitService : IBaseService<OpsPermit>
    {
        PagedInfo<OpsPermitDto> GetList(OpsPermitQueryDto parm);
        OpsPermit GetInfo(long PermitId);
        OpsPermit AddOpsPermit(OpsPermit model);
        int UpdateOpsPermit(OpsPermit model);
        PagedInfo<OpsPermitDto> ExportList(OpsPermitQueryDto parm);
        List<OpsPermitDto> GetByCustomerId(string customerId);
        List<OpsPermitDto> GetBySeasonId(long seasonId);
        List<OpsPermitDto> GetByWorkerId(long workerId);
        OpsPermitStatsDto GetStats();
        PagedInfo<OpsPermitDto> GetDigitalPermits(OpsPermitQueryDto parm);
        int ApprovePermit(long permitId, long approvedBy);
        int RejectPermit(long permitId, string reason);
        List<OpsPermitMonthlyReportDto> GetMonthlyReport(int year);
        List<OpsPermitDistributionDto> GetPermitTypeDistribution();
        OpsPermitDto AddPermitFull(OpsPermitCreateRequest request, Microsoft.AspNetCore.Http.HttpContext httpContext);
        OpsPermitDto GetPermitDetail(long permitId);
    }
}
