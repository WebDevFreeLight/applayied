using ZR.Model.BusinessPartners.Dto;
using ZR.Model.BusinessPartners;

namespace ZR.Service.BusinessPartners.IService
{
    /// <summary>
    /// ContactMethodservice接口
    /// </summary>
    public interface IBpContactmethodService : IBaseService<BpContactmethod>
    {
        PagedInfo<BpContactmethodDto> GetList(BpContactmethodQueryDto parm);

        BpContactmethod GetInfo(long Contactmethodenumid);


        BpContactmethod AddBpContactmethod(BpContactmethod parm);
        int UpdateBpContactmethod(BpContactmethod parm);
        
        bool TruncateBpContactmethod();

        (string, object, object) ImportBpContactmethod(List<BpContactmethod> list);

        PagedInfo<BpContactmethodDto> ExportList(BpContactmethodQueryDto parm);
    }
}

