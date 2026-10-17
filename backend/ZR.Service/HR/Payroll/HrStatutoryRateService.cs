using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Infrastructure.Attribute;
using SqlSugar;
using ZR.Model.HR.Payroll;
using ZR.Repository;
using ZR.Service.HR.IService;

namespace ZR.Service.HR
{
    [AppService(ServiceType = typeof(IHrStatutoryRateService), ServiceLifetime = LifeTime.Transient)]
    public class HrStatutoryRateService : BaseService<HrStatutoryRate>, IHrStatutoryRateService
    {
        // Default GOSI rates as fallback
        private const decimal DEFAULT_SAUDI_EMPLOYEE_RATE = 0.10m;
        private const decimal DEFAULT_SAUDI_EMPLOYER_RATE = 0.12m;
        private const decimal DEFAULT_NONSAUDI_EMPLOYEE_RATE = 0.02m;
        private const decimal DEFAULT_NONSAUDI_EMPLOYER_RATE = 0.02m;

        public async Task<List<HrStatutoryRate>> GetAllActiveRatesAsync()
        {
            return await Queryable()
                .Where(r => r.IsActive)
                .OrderBy(r => r.RateType)
                .ToListAsync();
        }

        public async Task<HrStatutoryRate> GetRateAsync(long rateId)
        {
            return await GetFirstAsync(r => r.RateId == rateId);
        }

        /// <summary>
        /// Get current GOSI rates with fallback to defaults
        /// </summary>
        public async Task<(decimal employeeRate, decimal employerRate)> GetCurrentGOSIRatesAsync(bool isSaudi)
        {
            var rateType = isSaudi ? "GOSI_SAUDI" : "GOSI_NONSAUDI";
            var now = DateTime.Now;

            var rate = await Queryable()
                .Where(r => r.RateType == rateType
                    && r.IsActive
                    && r.EffectiveFrom <= now
                    && (r.EffectiveTo == null || r.EffectiveTo > now))
                .OrderBy(r => r.EffectiveFrom, OrderByType.Desc)
                .FirstAsync();

            if (rate != null)
            {
                return (rate.EmployeeRate, rate.EmployerRate);
            }

            // Fallback to defaults
            return isSaudi
                ? (DEFAULT_SAUDI_EMPLOYEE_RATE, DEFAULT_SAUDI_EMPLOYER_RATE)
                : (DEFAULT_NONSAUDI_EMPLOYEE_RATE, DEFAULT_NONSAUDI_EMPLOYER_RATE);
        }

        public async Task<long> AddRateAsync(HrStatutoryRate rate)
        {
            rate.RateId = SnowFlakeSingle.Instance.NextId();
            rate.Create_time = DateTime.Now;
            return await InsertReturnSnowflakeIdAsync(rate);
        }

        public async Task<int> UpdateRateAsync(HrStatutoryRate rate)
        {
            rate.Update_time = DateTime.Now;
            return await Context.Updateable(rate)
                .IgnoreColumns(r => new { r.Create_time, r.Create_by })
                .ExecuteCommandAsync();
        }

        public async Task<int> DeleteRateAsync(long rateId)
        {
            // Soft delete by deactivating
            return await Context.Updateable<HrStatutoryRate>()
                .SetColumns(r => r.IsActive == false)
                .SetColumns(r => r.Update_time == DateTime.Now)
                .Where(r => r.RateId == rateId)
                .ExecuteCommandAsync();
        }
    }
}
