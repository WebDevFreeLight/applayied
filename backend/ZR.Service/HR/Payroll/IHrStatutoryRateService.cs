using System.Collections.Generic;
using System.Threading.Tasks;
using ZR.Model.HR.Payroll;

namespace ZR.Service.HR.IService
{
    public interface IHrStatutoryRateService
    {
        Task<List<HrStatutoryRate>> GetAllActiveRatesAsync();
        Task<HrStatutoryRate> GetRateAsync(long rateId);
        Task<(decimal employeeRate, decimal employerRate)> GetCurrentGOSIRatesAsync(bool isSaudi);
        Task<long> AddRateAsync(HrStatutoryRate rate);
        Task<int> UpdateRateAsync(HrStatutoryRate rate);
        Task<int> DeleteRateAsync(long rateId);
    }
}
