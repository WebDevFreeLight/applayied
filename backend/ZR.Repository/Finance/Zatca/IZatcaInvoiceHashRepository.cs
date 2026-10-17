using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZR.Model;
using ZR.Model.Finance.Zatca;
using ZR.Model.Finance.Zatca.Enums;

namespace ZR.Repository.Finance.Zatca
{
    /// <summary>
    /// ZATCA Invoice Hash Repository Interface
    /// واجهة مستودع هاش فواتير زاتكا
    /// </summary>
    public interface IZatcaInvoiceHashRepository : IBaseRepository<ZatcaInvoiceHash>
    {
        #region CRUD Operations

        /// <summary>
        /// Get paginated list of invoice hashes
        /// </summary>
        PagedInfo<ZatcaInvoiceHash> GetList(ZatcaInvoiceHash parm, PagerInfo pager);

        /// <summary>
        /// Get invoice hash by ID
        /// </summary>
        ZatcaInvoiceHash GetById(Guid id);

        /// <summary>
        /// Get invoice hash by ID (async)
        /// </summary>
        Task<ZatcaInvoiceHash> GetByIdAsync(Guid id);

        /// <summary>
        /// Get invoice hash by invoice ID
        /// </summary>
        ZatcaInvoiceHash GetByInvoiceId(Guid invoiceId);

        /// <summary>
        /// Get invoice hash by invoice ID (async)
        /// </summary>
        Task<ZatcaInvoiceHash> GetByInvoiceIdAsync(Guid invoiceId);

        /// <summary>
        /// Get invoice hash by UUID
        /// </summary>
        ZatcaInvoiceHash GetByUuid(string uuid);

        /// <summary>
        /// Insert invoice hash
        /// </summary>
        int InsertInvoiceHash(ZatcaInvoiceHash model);

        /// <summary>
        /// Insert invoice hash (async)
        /// </summary>
        Task<int> InsertInvoiceHashAsync(ZatcaInvoiceHash model);

        /// <summary>
        /// Update invoice hash
        /// </summary>
        int UpdateInvoiceHash(ZatcaInvoiceHash model);

        /// <summary>
        /// Update invoice hash (async)
        /// </summary>
        Task<int> UpdateInvoiceHashAsync(ZatcaInvoiceHash model);

        #endregion

        #region Counter Operations

        /// <summary>
        /// Get next invoice counter value
        /// </summary>
        long GetNextCounterValue(long? companyId);

        /// <summary>
        /// Get next invoice counter value (async)
        /// </summary>
        Task<long> GetNextCounterValueAsync(long? companyId);

        /// <summary>
        /// Get latest invoice hash (PIH)
        /// </summary>
        ZatcaInvoiceHash GetLatestInvoiceHash(long? companyId);

        /// <summary>
        /// Get latest invoice hash (PIH) (async)
        /// </summary>
        Task<ZatcaInvoiceHash> GetLatestInvoiceHashAsync(long? companyId);

        /// <summary>
        /// Get previous invoice hash for chain integrity
        /// </summary>
        string GetPreviousInvoiceHash(long? companyId);

        #endregion

        #region Query Operations

        /// <summary>
        /// Get invoice hashes by status
        /// </summary>
        List<ZatcaInvoiceHash> GetByStatus(ZatcaSubmissionStatus status, long? companyId = null);

        /// <summary>
        /// Get invoice hashes by status (async)
        /// </summary>
        Task<List<ZatcaInvoiceHash>> GetByStatusAsync(ZatcaSubmissionStatus status, long? companyId = null);

        /// <summary>
        /// Get pending invoices (not submitted or failed)
        /// </summary>
        List<ZatcaInvoiceHash> GetPendingInvoices(long? companyId = null, int maxRetries = 3);

        /// <summary>
        /// Get invoices needing retry
        /// </summary>
        List<ZatcaInvoiceHash> GetInvoicesNeedingRetry(int maxRetries = 3, long? companyId = null);

        /// <summary>
        /// Get invoice hashes by date range
        /// </summary>
        List<ZatcaInvoiceHash> GetByDateRange(DateTime startDate, DateTime endDate, long? companyId = null);

        #endregion

        #region Status Operations

        /// <summary>
        /// Update submission status
        /// </summary>
        int UpdateSubmissionStatus(Guid id, ZatcaSubmissionStatus status, string responseCode, string clearanceStatus, string reportingStatus);

        /// <summary>
        /// Update submission status (async)
        /// </summary>
        Task<int> UpdateSubmissionStatusAsync(Guid id, ZatcaSubmissionStatus status, string responseCode, string clearanceStatus, string reportingStatus);

        /// <summary>
        /// Update validation results
        /// </summary>
        int UpdateValidationResults(Guid id, string validationResults, string errorMessages, int warningCount, int errorCount);

        /// <summary>
        /// Increment retry count
        /// </summary>
        int IncrementRetryCount(Guid id);

        /// <summary>
        /// Update QR code
        /// </summary>
        int UpdateQrCode(Guid id, string qrCode);

        /// <summary>
        /// Update signed XML
        /// </summary>
        int UpdateSignedXml(Guid id, string signedXml, string hash, string cryptographicStamp);

        #endregion
    }
}
