using ZR.Model.HR.Business.Dto;
using ZR.Model.HR.Business;

namespace ZR.Service.HR.Business.IBusinessService
{
    /// <summary>
    /// Reviewtype Information Tableservice接口
    /// </summary>
    public interface IHrReviewtypeService : IBaseService<HrReviewtype>
    {
        PagedInfo<HrReviewtypeDto> GetList(HrReviewtypeQueryDto parm);

        HrReviewtype GetInfo(long Reviewtypeid);


        HrReviewtype AddHrReviewtype(HrReviewtype parm);
        int UpdateHrReviewtype(HrReviewtype parm);
        
        bool TruncateHrReviewtype();

        (string, object, object) ImportHrReviewtype(List<HrReviewtype> list);

        PagedInfo<HrReviewtypeDto> ExportList(HrReviewtypeQueryDto parm);
    }
}

