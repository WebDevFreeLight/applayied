using ZR.Model.BusinessPartners.Dto;
using ZR.Model.BusinessPartners;

namespace ZR.Service.BusinessPartners.IService
{
    /// <summary>
    /// Bp_PartnerDocsmentsservice接口
    /// </summary>
    public interface IBpPartnerdocsmentsService : IBaseService<BpPartnerdocsments>
    {
        PagedInfo<BpPartnerdocsmentsDto> GetList(BpPartnerdocsmentsQueryDto parm);

        BpPartnerdocsments GetInfo(long Id);


        BpPartnerdocsments AddBpPartnerdocsments(BpPartnerdocsments parm);
        int UpdateBpPartnerdocsments(BpPartnerdocsments parm);
        
        bool TruncateBpPartnerdocsments();

        (string, object, object) ImportBpPartnerdocsments(List<BpPartnerdocsments> list);

        PagedInfo<BpPartnerdocsmentsDto> ExportList(BpPartnerdocsmentsQueryDto parm);
    }
}

