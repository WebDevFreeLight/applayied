using ZR.Model.EventManagement.Dto;
using ZR.Model.EventManagement;

namespace ZR.Service.EventManagement.IEventManagementService
{
    /// <summary>
    /// Visitor Type - أنواع الزوارservice接口
    /// </summary>
    public interface IVisitorTypeService : IBaseService<VisitorType>
    {
        PagedInfo<VisitorTypeDto> GetList(VisitorTypeQueryDto parm);

        VisitorType GetInfo(long Typeid);


        VisitorType AddVisitorType(VisitorType parm);
        int UpdateVisitorType(VisitorType parm);
        
        bool TruncateVisitorType();

        (string, object, object) ImportVisitorType(List<VisitorType> list);

        PagedInfo<VisitorTypeDto> ExportList(VisitorTypeQueryDto parm);
    }
}

