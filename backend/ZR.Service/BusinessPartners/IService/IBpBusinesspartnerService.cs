using ZR.Model.BusinessPartners.Dto;
using ZR.Model.BusinessPartners;

namespace ZR.Service.BusinessPartners.IService
{
    /// <summary>
    /// businesspartnerservice接口
    /// </summary>
    public interface IBpBusinesspartnerService : IBaseService<BpBusinesspartner>
    {
        PagedInfo<BpBusinesspartnerDto_Res> GetList(BpBusinesspartnerQueryDto parm);

        BpBusinesspartner GetInfo(long Partnerid);


        BpBusinesspartner AddBpBusinesspartner(BpBusinesspartner parm);
        int UpdateBpBusinesspartner(BpBusinesspartner parm);
        
        bool TruncateBpBusinesspartner();

        (string, object, object) ImportBpBusinesspartner(List<BpBusinesspartner> list);

        PagedInfo<BpBusinesspartnerDto_Res> ExportList(BpBusinesspartnerQueryDto parm);
    }
}

