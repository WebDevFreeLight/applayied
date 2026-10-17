using ZR.Model.Hr.Dto;
using ZR.Model.HR.Payroll;

namespace ZR.Service.Hr.IHrService
{
    /// <summary>
    /// Salary Compliance - امتثال الرواتبservice接口
    /// </summary>
    public interface IHrSalaryComplianceService : IBaseService<HrSalaryCompliance>
    {
        PagedInfo<HrSalaryComplianceDto> GetList(HrSalaryComplianceQueryDto parm);

        HrSalaryCompliance GetInfo(long Complianceid);


        HrSalaryCompliance AddHrSalaryCompliance(HrSalaryCompliance parm);
        int UpdateHrSalaryCompliance(HrSalaryCompliance parm);
        
        bool TruncateHrSalaryCompliance();

        (string, object, object) ImportHrSalaryCompliance(List<HrSalaryCompliance> list);

        PagedInfo<HrSalaryComplianceDto> ExportList(HrSalaryComplianceQueryDto parm);
    }
}

