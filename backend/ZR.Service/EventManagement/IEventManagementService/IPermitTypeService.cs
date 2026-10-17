using ZR.Model.EventManagement.Dto;
using ZR.Model.EventManagement;

namespace ZR.Service.EventManagement.IEventManagementService
{
    /// <summary>
    /// Permit Type - أنواع التصاريحservice接口
    /// </summary>
    public interface IPermitTypeService : IBaseService<PermitType>
    {
        PagedInfo<PermitTypeDto> GetList(PermitTypeQueryDto parm);

        PermitType GetInfo(long Typeid);


        PermitType AddPermitType(PermitType parm);
        int UpdatePermitType(PermitType parm);
        
        bool TruncatePermitType();

        (string, object, object) ImportPermitType(List<PermitType> list);

        PagedInfo<PermitTypeDto> ExportList(PermitTypeQueryDto parm);
    }
}

