using ZR.Model.Operations;
using ZR.Model.Operations.Dto;

namespace ZR.Service.Operations
{
    public interface IOpsPermitTypeService : IBaseService<OpsPermitType>
    {
        PagedInfo<OpsPermitTypeDto> GetList(OpsPermitTypeQueryDto parm);
        OpsPermitType GetInfo(long PermitTypeId);
        OpsPermitType AddOpsPermitType(OpsPermitType model);
        int UpdateOpsPermitType(OpsPermitType model);
        PagedInfo<OpsPermitTypeDto> ExportList(OpsPermitTypeQueryDto parm);
    }
}
