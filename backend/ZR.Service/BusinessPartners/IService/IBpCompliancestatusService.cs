using ZR.Model.BusinessPartners.Dto;
using ZR.Model.BusinessPartners;

namespace ZR.Service.BusinessPartners.IService
{
    /// <summary>
    /// compliancestatusservice接口
    /// </summary>
    public interface IBpCompliancestatusService : IBaseService<BpCompliancestatus>
    {
        PagedInfo<BpCompliancestatusDto> GetList(BpCompliancestatusQueryDto parm);

        BpCompliancestatus GetInfo(long Id);


        BpCompliancestatus AddBpCompliancestatus(BpCompliancestatus parm);
        int UpdateBpCompliancestatus(BpCompliancestatus parm);
        
        bool TruncateBpCompliancestatus();

        (string, object, object) ImportBpCompliancestatus(List<BpCompliancestatus> list);

        PagedInfo<BpCompliancestatusDto> ExportList(BpCompliancestatusQueryDto parm);
    }
}

