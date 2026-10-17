using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZR.Model;
using ZR.Model.Finance.Zatca;
using ZR.Model.Finance.Zatca.Enums;

namespace ZR.Repository.Finance.Zatca
{
    /// <summary>
    /// ZATCA Device Repository Interface
    /// واجهة مستودع أجهزة زاتكا
    /// </summary>
    public interface IZatcaDeviceRepository : IBaseRepository<ZatcaDevice>
    {
        #region CRUD Operations

        /// <summary>
        /// Get paginated list of devices
        /// </summary>
        PagedInfo<ZatcaDevice> GetList(ZatcaDevice parm, PagerInfo pager);

        /// <summary>
        /// Get device by ID
        /// </summary>
        ZatcaDevice GetById(Guid id);

        /// <summary>
        /// Get device by ID (async)
        /// </summary>
        Task<ZatcaDevice> GetByIdAsync(Guid id);

        /// <summary>
        /// Get device by serial number
        /// </summary>
        ZatcaDevice GetBySerialNumber(string serialNumber, long? companyId = null);

        /// <summary>
        /// Insert device
        /// </summary>
        int InsertDevice(ZatcaDevice model);

        /// <summary>
        /// Insert device (async)
        /// </summary>
        Task<int> InsertDeviceAsync(ZatcaDevice model);

        /// <summary>
        /// Update device
        /// </summary>
        int UpdateDevice(ZatcaDevice model);

        /// <summary>
        /// Update device (async)
        /// </summary>
        Task<int> UpdateDeviceAsync(ZatcaDevice model);

        /// <summary>
        /// Delete devices by IDs
        /// </summary>
        int DeleteDevices(Guid[] ids);

        #endregion

        #region Query Operations

        /// <summary>
        /// Get all active devices for a company
        /// </summary>
        List<ZatcaDevice> GetActiveDevices(long? companyId = null);

        /// <summary>
        /// Get all active devices for a company (async)
        /// </summary>
        Task<List<ZatcaDevice>> GetActiveDevicesAsync(long? companyId = null);

        /// <summary>
        /// Get production-ready devices
        /// </summary>
        List<ZatcaDevice> GetProductionReadyDevices(long? companyId = null);

        /// <summary>
        /// Get devices by environment
        /// </summary>
        List<ZatcaDevice> GetByEnvironment(ZatcaEnvironment environment, long? companyId = null);

        /// <summary>
        /// Get devices with expiring certificates
        /// </summary>
        List<ZatcaDevice> GetDevicesWithExpiringCertificates(int daysBeforeExpiry = 30, long? companyId = null);

        #endregion

        #region Validation Operations

        /// <summary>
        /// Check if serial number exists
        /// </summary>
        bool SerialNumberExists(string serialNumber, long? companyId, Guid? excludeId = null);

        /// <summary>
        /// Check if serial number exists (async)
        /// </summary>
        Task<bool> SerialNumberExistsAsync(string serialNumber, long? companyId, Guid? excludeId = null);

        #endregion

        #region Certificate Operations

        /// <summary>
        /// Update device CSR
        /// </summary>
        int UpdateCsr(Guid deviceId, string csr, string privateKey);

        /// <summary>
        /// Update compliance certificate
        /// </summary>
        int UpdateComplianceCertificate(Guid deviceId, string csid, string secret, string certificate, DateTime? expiryDate);

        /// <summary>
        /// Update production certificate
        /// </summary>
        int UpdateProductionCertificate(Guid deviceId, string csid, string secret, string certificate, DateTime? expiryDate);

        /// <summary>
        /// Update last used timestamp
        /// </summary>
        int UpdateLastUsed(Guid deviceId);

        #endregion
    }
}
