using ZR.Model.Hr.Dto;
using ZR.Model.Hr;

namespace ZR.Service.Hr.IHrService
{
    /// <summary>
    /// insuranceenrollmentservice接口
    /// </summary>
    public interface IHrInsuranceEnrollmentService : IBaseService<HrInsuranceEnrollment>
    {
        PagedInfo<HrInsuranceEnrollmentDto> GetList(HrInsuranceEnrollmentQueryDto parm);

        HrInsuranceEnrollment GetInfo(long Enrollmentid);


        HrInsuranceEnrollment AddHrInsuranceEnrollment(HrInsuranceEnrollment parm);
        int UpdateHrInsuranceEnrollment(HrInsuranceEnrollment parm);
        
        bool TruncateHrInsuranceEnrollment();

        (string, object, object) ImportHrInsuranceEnrollment(List<HrInsuranceEnrollment> list);

        PagedInfo<HrInsuranceEnrollmentDto> ExportList(HrInsuranceEnrollmentQueryDto parm);
    }
}

