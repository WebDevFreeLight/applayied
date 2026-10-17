using ZR.Model.BusinessPartners.Dto;
using ZR.Model.BusinessPartners;

namespace ZR.Service.BusinessPartners.IService
{
    /// <summary>
    /// BusinessPartnerStatusservice接口
    /// </summary>
    public interface IBpBusinesspartnerstatusService : IBaseService<BpBusinesspartnerstatus>
    {
        PagedInfo<BpBusinesspartnerstatusDto> GetList(BpBusinesspartnerstatusQueryDto parm);

        BpBusinesspartnerstatus GetInfo(long Statusid);


        BpBusinesspartnerstatus AddBpBusinesspartnerstatus(BpBusinesspartnerstatus parm);
        int UpdateBpBusinesspartnerstatus(BpBusinesspartnerstatus parm);
        
        bool TruncateBpBusinesspartnerstatus();

        (string, object, object) ImportBpBusinesspartnerstatus(List<BpBusinesspartnerstatus> list);

        PagedInfo<BpBusinesspartnerstatusDto> ExportList(BpBusinesspartnerstatusQueryDto parm);
    }
}

