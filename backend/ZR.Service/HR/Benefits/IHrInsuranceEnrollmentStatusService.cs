using ZR.Model.Hr.Dto;
using ZR.Model.Hr;

namespace ZR.Service.Hr.IHrService
{
    /// <summary>
    /// hrinsuranceenrollmentstatusservice接口
    /// </summary>
    public interface IHrInsuranceEnrollmentStatusService : IBaseService<HrInsuranceEnrollmentStatus>
    {
        PagedInfo<HrInsuranceEnrollmentStatusDto> GetList(HrInsuranceEnrollmentStatusQueryDto parm);

        HrInsuranceEnrollmentStatus GetInfo(long Id);


        HrInsuranceEnrollmentStatus AddHrInsuranceEnrollmentStatus(HrInsuranceEnrollmentStatus parm);
        int UpdateHrInsuranceEnrollmentStatus(HrInsuranceEnrollmentStatus parm);
        
        bool TruncateHrInsuranceEnrollmentStatus();

        (string, object, object) ImportHrInsuranceEnrollmentStatus(List<HrInsuranceEnrollmentStatus> list);

        PagedInfo<HrInsuranceEnrollmentStatusDto> ExportList(HrInsuranceEnrollmentStatusQueryDto parm);
    }
}

