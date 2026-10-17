using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.Attribute;
using SqlSugar;
using ZR.Model;
using ZR.Model.Finance.Configuration;

namespace ZR.Repository.Finance.Configuration
{
    /// <summary>
    /// System Configuration Repository - مستودع إعدادات النظام
    /// </summary>
    [AppService(ServiceType = typeof(ISystemConfigurationRepository), ServiceLifetime = LifeTime.Transient)]
    public class SystemConfigurationRepository : BaseRepository<SystemConfiguration>, ISystemConfigurationRepository
    {
        /// <summary>
        /// Get paginated list of configurations
        /// </summary>
        public PagedInfo<SystemConfiguration> GetList(SystemConfiguration parm, PagerInfo pager)
        {
            var predicate = Expressionable.Create<SystemConfiguration>();
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Key), it => it.Key.Contains(parm.Key));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.ValueType), it => it.ValueType == parm.ValueType);
            predicate = predicate.AndIF(parm.CompanyID.HasValue, it => it.CompanyID == parm.CompanyID);

            return Queryable()
                .Where(predicate.ToExpression())
                .OrderBy(x => x.Key)
                .ToPage(pager);
        }

        /// <summary>
        /// Get configuration by ID
        /// </summary>
        public SystemConfiguration GetById(Guid id)
        {
            return GetFirst(x => x.Id == id);
        }

        /// <summary>
        /// Get configuration by ID async
        /// </summary>
        public async Task<SystemConfiguration> GetByIdAsync(Guid id)
        {
            return await Queryable().FirstAsync(x => x.Id == id);
        }

        /// <summary>
        /// Get configuration by key
        /// </summary>
        public SystemConfiguration GetByKey(string key, long? companyId = null)
        {
            var predicate = Expressionable.Create<SystemConfiguration>();
            predicate = predicate.And(it => it.Key == key);
            predicate = predicate.AndIF(companyId.HasValue, it => it.CompanyID == companyId);

            return GetFirst(predicate.ToExpression());
        }

        /// <summary>
        /// Get configuration by key async
        /// </summary>
        public async Task<SystemConfiguration> GetByKeyAsync(string key, long? companyId = null)
        {
            var predicate = Expressionable.Create<SystemConfiguration>();
            predicate = predicate.And(it => it.Key == key);
            predicate = predicate.AndIF(companyId.HasValue, it => it.CompanyID == companyId);

            return await Queryable().FirstAsync(predicate.ToExpression());
        }

        /// <summary>
        /// Get all configurations
        /// </summary>
        public List<SystemConfiguration> ListAll(long? companyId = null)
        {
            var predicate = Expressionable.Create<SystemConfiguration>();
            predicate = predicate.AndIF(companyId.HasValue, it => it.CompanyID == companyId);

            return Queryable()
                .Where(predicate.ToExpression())
                .OrderBy(x => x.Key)
                .ToList();
        }

        /// <summary>
        /// Get all configurations async
        /// </summary>
        public async Task<List<SystemConfiguration>> ListAllAsync(long? companyId = null)
        {
            var predicate = Expressionable.Create<SystemConfiguration>();
            predicate = predicate.AndIF(companyId.HasValue, it => it.CompanyID == companyId);

            return await Queryable()
                .Where(predicate.ToExpression())
                .OrderBy(x => x.Key)
                .ToListAsync();
        }

        /// <summary>
        /// Create configuration
        /// </summary>
        public int CreateConfiguration(SystemConfiguration model)
        {
            return Insert(model);
        }

        /// <summary>
        /// Create configuration async
        /// </summary>
        public async Task<int> CreateConfigurationAsync(SystemConfiguration model)
        {
            return await Context.Insertable(model).ExecuteCommandAsync();
        }

        /// <summary>
        /// Update configuration
        /// </summary>
        public int UpdateConfiguration(SystemConfiguration model)
        {
            return Update(model, it => new
            {
                it.Key,
                it.Value,
                it.ValueType,
                it.DescriptionAr,
                it.DescriptionEn,
                it.Update_by,
                it.Update_time
            }, f => f.Id == model.Id);
        }

        /// <summary>
        /// Update configuration async
        /// </summary>
        public async Task<int> UpdateConfigurationAsync(SystemConfiguration model)
        {
            return await Context.Updateable(model)
                .UpdateColumns(it => new
                {
                    it.Key,
                    it.Value,
                    it.ValueType,
                    it.DescriptionAr,
                    it.DescriptionEn,
                    it.Update_by,
                    it.Update_time
                })
                .Where(f => f.Id == model.Id)
                .ExecuteCommandAsync();
        }

        /// <summary>
        /// Delete configuration
        /// </summary>
        public int DeleteConfiguration(Guid id)
        {
            return Context.Deleteable<SystemConfiguration>()
                .Where(it => it.Id == id)
                .ExecuteCommand();
        }

        /// <summary>
        /// Delete configuration async
        /// </summary>
        public async Task<int> DeleteConfigurationAsync(Guid id)
        {
            return await Context.Deleteable<SystemConfiguration>()
                .Where(it => it.Id == id)
                .ExecuteCommandAsync();
        }

        /// <summary>
        /// Check if key exists
        /// </summary>
        public bool KeyExists(string key, long? companyId = null, Guid? excludeId = null)
        {
            var predicate = Expressionable.Create<SystemConfiguration>();
            predicate = predicate.And(it => it.Key == key);
            predicate = predicate.AndIF(companyId.HasValue, it => it.CompanyID == companyId);
            predicate = predicate.AndIF(excludeId.HasValue, it => it.Id != excludeId.Value);

            return Any(predicate.ToExpression());
        }

        /// <summary>
        /// Check if key exists async
        /// </summary>
        public async Task<bool> KeyExistsAsync(string key, long? companyId = null, Guid? excludeId = null)
        {
            var predicate = Expressionable.Create<SystemConfiguration>();
            predicate = predicate.And(it => it.Key == key);
            predicate = predicate.AndIF(companyId.HasValue, it => it.CompanyID == companyId);
            predicate = predicate.AndIF(excludeId.HasValue, it => it.Id != excludeId.Value);

            return await Queryable().AnyAsync(predicate.ToExpression());
        }
    }
}

