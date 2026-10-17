using System.Threading.Tasks;
using ZR.Model.Finance.Zatca.Enums;

namespace ZR.Service.HR.IService
{
    public interface IHRComplianceService
    {
        Task<ZatcaSubmissionStatus> CheckZatcaCompliance(long payrollId);
    }
}















