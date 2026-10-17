using ZR.Model.BusinessPartners.Dto;
using ZR.Model.BusinessPartners;

namespace ZR.Service.BusinessPartners.IService
{
    /// <summary>
    /// BusinessPartnerTypeservice接口
    /// </summary>
    public interface IBpBusinesspartnertypeService : IBaseService<BpBusinesspartnertype>
    {
        PagedInfo<BpBusinesspartnertypeDto> GetList(BpBusinesspartnertypeQueryDto parm);

        BpBusinesspartnertype GetInfo(long Typeid);


        BpBusinesspartnertype AddBpBusinesspartnertype(BpBusinesspartnertype parm);
        int UpdateBpBusinesspartnertype(BpBusinesspartnertype parm);
        int DeleteBpBusinesspartnertype(long Typeid);

        bool TruncateBpBusinesspartnertype();

        (string, object, object) ImportBpBusinesspartnertype(List<BpBusinesspartnertype> list);

        PagedInfo<BpBusinesspartnertypeDto> ExportList(BpBusinesspartnertypeQueryDto parm);
    }
}

