using ZR.Model.HR.Business.Dto;
using ZR.Model.HR.Business;

namespace ZR.Service.HR.Business.IBusinessService
{
    /// <summary>
    /// DocumentTypeservice接口
    /// </summary>
    public interface IHrDocumenttypeService : IBaseService<HrDocumenttype>
    {
        PagedInfo<HrDocumenttypeDto> GetList(HrDocumenttypeQueryDto parm);

        HrDocumenttype GetInfo(long Documenttypeid);


        HrDocumenttype AddHrDocumenttype(HrDocumenttype parm);
        int UpdateHrDocumenttype(HrDocumenttype parm);
        
        bool TruncateHrDocumenttype();

        (string, object, object) ImportHrDocumenttype(List<HrDocumenttype> list);

        PagedInfo<HrDocumenttypeDto> ExportList(HrDocumenttypeQueryDto parm);
    }
}

