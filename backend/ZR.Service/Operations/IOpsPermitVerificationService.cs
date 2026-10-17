using ZR.Model.Operations;
using ZR.Model.Operations.Dto;

namespace ZR.Service.Operations
{
    public interface IOpsPermitVerificationService : IBaseService<OpsPermitVerification>
    {
        PagedInfo<OpsPermitVerificationDto> GetList(OpsPermitVerificationQueryDto parm);
        OpsPermitVerification GetInfo(long VerificationId);
        OpsPermitVerification AddOpsPermitVerification(OpsPermitVerification model);
        int UpdateOpsPermitVerification(OpsPermitVerification model);
        PagedInfo<OpsPermitVerificationDto> ExportList(OpsPermitVerificationQueryDto parm);
        int VerifyDocument(long verificationId, string verifiedBy);
        int RejectDocument(long verificationId, string reason);
    }
}
