using ZR.Model.BusinessPartners.Dto;
using ZR.Model.BusinessPartners;

namespace ZR.Service.BusinessPartners.IService
{
    /// <summary>
    /// BusinessPartnerkindservice接口
    /// </summary>
    public interface IBpBusinesspartnerkindService : IBaseService<BpBusinesspartnerkind>
    {
        PagedInfo<BpBusinesspartnerkindDto> GetList(BpBusinesspartnerkindQueryDto parm);

        BpBusinesspartnerkind GetInfo(long Kindid);


        BpBusinesspartnerkind AddBpBusinesspartnerkind(BpBusinesspartnerkind parm);
        int UpdateBpBusinesspartnerkind(BpBusinesspartnerkind parm);
        
        bool TruncateBpBusinesspartnerkind();

        (string, object, object) ImportBpBusinesspartnerkind(List<BpBusinesspartnerkind> list);

        PagedInfo<BpBusinesspartnerkindDto> ExportList(BpBusinesspartnerkindQueryDto parm);
    }
}

