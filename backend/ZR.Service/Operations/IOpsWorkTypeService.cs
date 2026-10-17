using ZR.Model.Operations;
using ZR.Model.Operations.Dto;

namespace ZR.Service.Operations
{
    public interface IOpsWorkTypeService : IBaseService<OpsWorkType>
    {
        PagedInfo<OpsWorkTypeDto> GetList(OpsWorkTypeQueryDto parm);
        OpsWorkType GetInfo(long workTypeId);
        OpsWorkType AddWorkType(OpsWorkType model);
        int UpdateWorkType(OpsWorkType model);
        PagedInfo<OpsWorkTypeDto> ExportList(OpsWorkTypeQueryDto parm);
        List<OpsWorkTypeDto> GetAllActive();
    }
}
