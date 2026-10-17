using ZR.Model.BusinessPartners.Dto;
using ZR.Model.BusinessPartners;

namespace ZR.Service.BusinessPartners.IService
{
    /// <summary>
    /// BusinessPartnerDocTypeservice接口
    /// </summary>
    public interface IBpBusinesspartnerdoctypeService : IBaseService<BpBusinesspartnerdoctype>
    {
        PagedInfo<BpBusinesspartnerdoctypeDto> GetList(BpBusinesspartnerdoctypeQueryDto parm);

        BpBusinesspartnerdoctype GetInfo(long Id);


        BpBusinesspartnerdoctype AddBpBusinesspartnerdoctype(BpBusinesspartnerdoctype parm);
        int UpdateBpBusinesspartnerdoctype(BpBusinesspartnerdoctype parm);
        
        bool TruncateBpBusinesspartnerdoctype();

        (string, object, object) ImportBpBusinesspartnerdoctype(List<BpBusinesspartnerdoctype> list);

        PagedInfo<BpBusinesspartnerdoctypeDto> ExportList(BpBusinesspartnerdoctypeQueryDto parm);
    }
}

