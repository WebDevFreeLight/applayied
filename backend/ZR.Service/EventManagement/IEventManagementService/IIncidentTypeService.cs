using ZR.Model.EventManagement.Dto;
using ZR.Model.EventManagement;

namespace ZR.Service.EventManagement.IEventManagementService
{
    /// <summary>
    /// Incident Type - أنواع الحوادثservice接口
    /// </summary>
    public interface IIncidentTypeService : IBaseService<IncidentType>
    {
        PagedInfo<IncidentTypeDto> GetList(IncidentTypeQueryDto parm);

        IncidentType GetInfo(long Typeid);


        IncidentType AddIncidentType(IncidentType parm);
        int UpdateIncidentType(IncidentType parm);
        
        bool TruncateIncidentType();

        (string, object, object) ImportIncidentType(List<IncidentType> list);

        PagedInfo<IncidentTypeDto> ExportList(IncidentTypeQueryDto parm);
    }
}

