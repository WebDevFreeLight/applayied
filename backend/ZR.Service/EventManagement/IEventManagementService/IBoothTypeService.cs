using ZR.Model.EventManagement.Dto;
using ZR.Model.EventManagement;

namespace ZR.Service.EventManagement.IEventManagementService
{
    /// <summary>
    /// Booth Type - أنواع الأكشاكservice接口
    /// </summary>
    public interface IBoothTypeService : IBaseService<BoothType>
    {
        PagedInfo<BoothTypeDto> GetList(BoothTypeQueryDto parm);

        BoothType GetInfo(long Typeid);


        BoothType AddBoothType(BoothType parm);
        int UpdateBoothType(BoothType parm);
        
        bool TruncateBoothType();

        (string, object, object) ImportBoothType(List<BoothType> list);

        PagedInfo<BoothTypeDto> ExportList(BoothTypeQueryDto parm);
    }
}

