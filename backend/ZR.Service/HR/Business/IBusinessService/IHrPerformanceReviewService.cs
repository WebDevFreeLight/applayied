using ZR.Model.HR.Business.Dto;
using ZR.Model.HR.Performance;

namespace ZR.Service.HR.Business.IBusinessService
{
    /// <summary>
    /// Performance Review Tableservice接口
    /// </summary>
    public interface IHrPerformanceReviewService : IBaseService<HrPerformanceReview>
    {
        PagedInfo<HrPerformanceReviewDto> GetList(HrPerformanceReviewQueryDto parm);

        HrPerformanceReview GetInfo(long Reviewid);


        HrPerformanceReview AddHrPerformanceReview(HrPerformanceReview parm);
        int UpdateHrPerformanceReview(HrPerformanceReview parm);
        
        bool TruncateHrPerformanceReview();

        (string, object, object) ImportHrPerformanceReview(List<HrPerformanceReview> list);

        PagedInfo<HrPerformanceReviewDto> ExportList(HrPerformanceReviewQueryDto parm);
    }
}

