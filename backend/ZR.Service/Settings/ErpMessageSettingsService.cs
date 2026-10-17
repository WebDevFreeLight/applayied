using Infrastructure.Attribute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZR.Model.Settings;
using ZR.Service.Settings.IService;

namespace ZR.Service.Settings
{

    [AppService(ServiceType = typeof(IErpMessageSettingsService), ServiceLifetime = LifeTime.Transient)]
    public class ErpMessageSettingsService : BaseService<ErpMessageSettings>, IErpMessageSettingsService
    {
     

        public ErpMessageSettings GetSettings(string eventType = null) => GetFirst(x => eventType == null || x.EventType == eventType);

        public int SaveSettings(ErpMessageSettings model)
        {
            var existing = GetFirst(x => x.SettingId == model.SettingId);
            if (existing != null) { return Context.Updateable(model).ExecuteCommand(); }
            model.SettingId = SnowFlakeSingle.Instance.NextId();
            return Context.Insertable(model).ExecuteCommand();
        }

        public async Task<bool> TestSmsAsync(string phone) { await Task.CompletedTask; return true; }
        public async Task<bool> TestEmailAsync(string email) { await Task.CompletedTask; return true; }
    }
}

