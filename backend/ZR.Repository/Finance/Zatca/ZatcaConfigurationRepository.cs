using System;
using System.Threading.Tasks;
using Infrastructure.Attribute;
using SqlSugar;
using ZR.Model.Finance.Zatca;
using ZR.Model.Finance.Zatca.Enums;

namespace ZR.Repository.Finance.Zatca
{
    /// <summary>
    /// ZATCA Configuration Repository Implementation
    /// تنفيذ مستودع إعدادات زاتكا
    /// </summary>
    [AppService(ServiceType = typeof(IZatcaConfigurationRepository), ServiceLifetime = LifeTime.Transient)]
    public class ZatcaConfigurationRepository : BaseRepository<ZatcaConfiguration>, IZatcaConfigurationRepository
    {
        public ZatcaConfiguration GetByCompanyId(long? companyId)
        {
            return Queryable()
                .Where(x => x.CompanyID == companyId)
                .First();
        }

        public async Task<ZatcaConfiguration> GetByCompanyIdAsync(long? companyId)
        {
            return await Context.Queryable<ZatcaConfiguration>()
                .Where(x => x.CompanyID == companyId)
                .FirstAsync();
        }

        public ZatcaConfiguration GetById(Guid id)
        {
            return GetFirst(x => x.Id == id);
        }

        public async Task<ZatcaConfiguration> GetByIdAsync(Guid id)
        {
            return await GetFirstAsync(x => x.Id == id);
        }

        public int InsertConfiguration(ZatcaConfiguration model)
        {
            return Insert(model);
        }

        public async Task<int> InsertConfigurationAsync(ZatcaConfiguration model)
        {
            var result = await InsertAsync(model);
            return result ? 1 : 0;
        }

        public int UpdateConfiguration(ZatcaConfiguration model)
        {
            return Context.Updateable(model)
                .IgnoreColumns(it => new { it.Create_by, it.Create_time })
                .ExecuteCommand();
        }

        public async Task<int> UpdateConfigurationAsync(ZatcaConfiguration model)
        {
            return await Context.Updateable(model)
                .IgnoreColumns(it => new { it.Create_by, it.Create_time })
                .ExecuteCommandAsync();
        }

        public bool ConfigurationExists(long? companyId)
        {
            return Queryable().Where(x => x.CompanyID == companyId).Any();
        }

        public async Task<bool> ConfigurationExistsAsync(long? companyId)
        {
            return await Context.Queryable<ZatcaConfiguration>()
                .Where(x => x.CompanyID == companyId)
                .AnyAsync();
        }

        public int UpdateActiveDevice(long? companyId, Guid? deviceId)
        {
            return Context.Updateable<ZatcaConfiguration>()
                .SetColumns(x => new ZatcaConfiguration
                {
                    ActiveDeviceId = deviceId,
                    Update_time = DateTime.Now
                })
                .Where(x => x.CompanyID == companyId)
                .ExecuteCommand();
        }

        public int UpdateEnvironment(long? companyId, ZatcaEnvironment environment)
        {
            return Context.Updateable<ZatcaConfiguration>()
                .SetColumns(x => new ZatcaConfiguration
                {
                    Environment = environment,
                    Update_time = DateTime.Now
                })
                .Where(x => x.CompanyID == companyId)
                .ExecuteCommand();
        }
    }
}
