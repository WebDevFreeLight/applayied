using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.Attribute;
using SqlSugar;
using ZR.Model;
using ZR.Model.Finance.Zatca;
using ZR.Model.Finance.Zatca.Enums;

namespace ZR.Repository.Finance.Zatca
{
    /// <summary>
    /// ZATCA Device Repository Implementation
    /// تنفيذ مستودع أجهزة زاتكا
    /// </summary>
    [AppService(ServiceType = typeof(IZatcaDeviceRepository), ServiceLifetime = LifeTime.Transient)]
    public class ZatcaDeviceRepository : BaseRepository<ZatcaDevice>, IZatcaDeviceRepository
    {
        #region CRUD Operations

        public PagedInfo<ZatcaDevice> GetList(ZatcaDevice parm, PagerInfo pager)
        {
            var predicate = Expressionable.Create<ZatcaDevice>();

            predicate = predicate.AndIF(parm.CompanyID.HasValue, it => it.CompanyID == parm.CompanyID);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.DeviceSerialNumber),
                it => it.DeviceSerialNumber.Contains(parm.DeviceSerialNumber));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.DeviceName),
                it => it.DeviceName.Contains(parm.DeviceName));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.OrganizationIdentifier),
                it => it.OrganizationIdentifier.Contains(parm.OrganizationIdentifier));

            return Queryable()
                .Where(predicate.ToExpression())
                .OrderByDescending(x => x.Create_time)
                .ToPage(pager);
        }

        public ZatcaDevice GetById(Guid id)
        {
            return GetFirst(x => x.Id == id);
        }

        public async Task<ZatcaDevice> GetByIdAsync(Guid id)
        {
            return await GetFirstAsync(x => x.Id == id);
        }

        public ZatcaDevice GetBySerialNumber(string serialNumber, long? companyId = null)
        {
            var query = Queryable().Where(x => x.DeviceSerialNumber == serialNumber);
            if (companyId.HasValue)
            {
                query = query.Where(x => x.CompanyID == companyId);
            }
            return query.First();
        }

        public int InsertDevice(ZatcaDevice model)
        {
            return Insert(model);
        }

        public async Task<int> InsertDeviceAsync(ZatcaDevice model)
        {
            var result = await InsertAsync(model);
            return result ? 1 : 0;
        }

        public int UpdateDevice(ZatcaDevice model)
        {
            return Context.Updateable(model)
                .IgnoreColumns(it => new { it.Create_by, it.Create_time })
                .ExecuteCommand();
        }

        public async Task<int> UpdateDeviceAsync(ZatcaDevice model)
        {
            return await Context.Updateable(model)
                .IgnoreColumns(it => new { it.Create_by, it.Create_time })
                .ExecuteCommandAsync();
        }

        public int DeleteDevices(Guid[] ids)
        {
            return Context.Deleteable<ZatcaDevice>()
                .Where(it => ids.Contains(it.Id))
                .ExecuteCommand();
        }

        #endregion

        #region Query Operations

        public List<ZatcaDevice> GetActiveDevices(long? companyId = null)
        {
            var query = Queryable().Where(x => x.IsActive);
            if (companyId.HasValue)
            {
                query = query.Where(x => x.CompanyID == companyId);
            }
            return query.OrderBy(x => x.DeviceName).ToList();
        }

        public async Task<List<ZatcaDevice>> GetActiveDevicesAsync(long? companyId = null)
        {
            var query = Context.Queryable<ZatcaDevice>().Where(x => x.IsActive);
            if (companyId.HasValue)
            {
                query = query.Where(x => x.CompanyID == companyId);
            }
            return await query.OrderBy(x => x.DeviceName).ToListAsync();
        }

        public List<ZatcaDevice> GetProductionReadyDevices(long? companyId = null)
        {
            var query = Queryable().Where(x => x.IsActive && x.IsProductionReady);
            if (companyId.HasValue)
            {
                query = query.Where(x => x.CompanyID == companyId);
            }
            return query.OrderBy(x => x.DeviceName).ToList();
        }

        public List<ZatcaDevice> GetByEnvironment(ZatcaEnvironment environment, long? companyId = null)
        {
            var query = Queryable().Where(x => x.Environment == environment && x.IsActive);
            if (companyId.HasValue)
            {
                query = query.Where(x => x.CompanyID == companyId);
            }
            return query.OrderBy(x => x.DeviceName).ToList();
        }

        public List<ZatcaDevice> GetDevicesWithExpiringCertificates(int daysBeforeExpiry = 30, long? companyId = null)
        {
            var expiryThreshold = DateTime.Now.AddDays(daysBeforeExpiry);
            var query = Queryable()
                .Where(x => x.IsActive && x.CertificateExpiryDate.HasValue && x.CertificateExpiryDate <= expiryThreshold);
            if (companyId.HasValue)
            {
                query = query.Where(x => x.CompanyID == companyId);
            }
            return query.OrderBy(x => x.CertificateExpiryDate).ToList();
        }

        #endregion

        #region Validation Operations

        public bool SerialNumberExists(string serialNumber, long? companyId, Guid? excludeId = null)
        {
            var query = Queryable().Where(x => x.DeviceSerialNumber == serialNumber);
            if (companyId.HasValue)
            {
                query = query.Where(x => x.CompanyID == companyId);
            }
            if (excludeId.HasValue)
            {
                query = query.Where(x => x.Id != excludeId.Value);
            }
            return query.Any();
        }

        public async Task<bool> SerialNumberExistsAsync(string serialNumber, long? companyId, Guid? excludeId = null)
        {
            var query = Context.Queryable<ZatcaDevice>().Where(x => x.DeviceSerialNumber == serialNumber);
            if (companyId.HasValue)
            {
                query = query.Where(x => x.CompanyID == companyId);
            }
            if (excludeId.HasValue)
            {
                query = query.Where(x => x.Id != excludeId.Value);
            }
            return await query.AnyAsync();
        }

        #endregion

        #region Certificate Operations

        public int UpdateCsr(Guid deviceId, string csr, string privateKey)
        {
            return Context.Updateable<ZatcaDevice>()
                .SetColumns(x => new ZatcaDevice
                {
                    Csr = csr,
                    PrivateKey = privateKey,
                    Update_time = DateTime.Now
                })
                .Where(x => x.Id == deviceId)
                .ExecuteCommand();
        }

        public int UpdateComplianceCertificate(Guid deviceId, string csid, string secret, string certificate, DateTime? expiryDate)
        {
            return Context.Updateable<ZatcaDevice>()
                .SetColumns(x => new ZatcaDevice
                {
                    ComplianceCsid = csid,
                    ComplianceCsidSecret = secret,
                    ComplianceCertificate = certificate,
                    CertificateExpiryDate = expiryDate,
                    IsComplianceCompleted = true,
                    ComplianceCompletedAt = DateTime.Now,
                    Update_time = DateTime.Now
                })
                .Where(x => x.Id == deviceId)
                .ExecuteCommand();
        }

        public int UpdateProductionCertificate(Guid deviceId, string csid, string secret, string certificate, DateTime? expiryDate)
        {
            return Context.Updateable<ZatcaDevice>()
                .SetColumns(x => new ZatcaDevice
                {
                    ProductionCsid = csid,
                    ProductionCsidSecret = secret,
                    ProductionCertificate = certificate,
                    CertificateExpiryDate = expiryDate,
                    IsProductionReady = true,
                    ProductionActivatedAt = DateTime.Now,
                    Update_time = DateTime.Now
                })
                .Where(x => x.Id == deviceId)
                .ExecuteCommand();
        }

        public int UpdateLastUsed(Guid deviceId)
        {
            return Context.Updateable<ZatcaDevice>()
                .SetColumns(x => new ZatcaDevice
                {
                    LastUsedAt = DateTime.Now
                })
                .Where(x => x.Id == deviceId)
                .ExecuteCommand();
        }

        #endregion
    }
}
