using Infrastructure.Attribute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZR.Model.Finance.ZATCA;
using ZR.Service.IService.Finance.ZATCA;

namespace ZR.Service.Finance.ZATCA
{



    [AppService(ServiceType = typeof(IErpZatcaSettingsService), ServiceLifetime = LifeTime.Transient)]
    public class ErpZatcaSettingsService : BaseService<ErpZatcaSettings>, IErpZatcaSettingsService
    {


        public ErpZatcaSettings GetSettings(long? BranchID = null) => GetFirst(x => (BranchID == null && x.BranchID == null) || x.BranchID == BranchID);

        public int SaveSettings(ErpZatcaSettings model)
        {
            var existing = GetSettings(model.BranchID);
            if (existing != null)
            {
                model.SettingsId = existing.SettingsId;
                return Context.Updateable(model).ExecuteCommand();
            }
            model.SettingsId = SnowFlakeSingle.Instance.NextId();
            return Context.Insertable(model).ExecuteCommand();
        }

        public async Task<bool> ValidateCertificateAsync(string certificatePath) { await Task.CompletedTask; return System.IO.File.Exists(certificatePath); }
        public async Task<bool> TestConnectionAsync() { await Task.CompletedTask; return true; }
    }
}

