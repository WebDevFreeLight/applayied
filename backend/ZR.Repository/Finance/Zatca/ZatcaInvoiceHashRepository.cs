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
    /// ZATCA Invoice Hash Repository Implementation
    /// تنفيذ مستودع هاش فواتير زاتكا
    /// </summary>
    [AppService(ServiceType = typeof(IZatcaInvoiceHashRepository), ServiceLifetime = LifeTime.Transient)]
    public class ZatcaInvoiceHashRepository : BaseRepository<ZatcaInvoiceHash>, IZatcaInvoiceHashRepository
    {
        #region CRUD Operations

        public PagedInfo<ZatcaInvoiceHash> GetList(ZatcaInvoiceHash parm, PagerInfo pager)
        {
            var predicate = Expressionable.Create<ZatcaInvoiceHash>();

            predicate = predicate.AndIF(parm.CompanyID.HasValue, it => it.CompanyID == parm.CompanyID);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.InvoiceNumber),
                it => it.InvoiceNumber.Contains(parm.InvoiceNumber));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.InvoiceUuid),
                it => it.InvoiceUuid == parm.InvoiceUuid);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.InvoiceSourceType),
                it => it.InvoiceSourceType == parm.InvoiceSourceType);

            return Queryable()
                .Where(predicate.ToExpression())
                .OrderByDescending(x => x.InvoiceCounterValue)
                .ToPage(pager);
        }

        public ZatcaInvoiceHash GetById(Guid id)
        {
            return GetFirst(x => x.Id == id);
        }

        public async Task<ZatcaInvoiceHash> GetByIdAsync(Guid id)
        {
            return await GetFirstAsync(x => x.Id == id);
        }

        public ZatcaInvoiceHash GetByInvoiceId(Guid invoiceId)
        {
            return GetFirst(x => x.InvoiceId == invoiceId);
        }

        public async Task<ZatcaInvoiceHash> GetByInvoiceIdAsync(Guid invoiceId)
        {
            return await GetFirstAsync(x => x.InvoiceId == invoiceId);
        }

        public ZatcaInvoiceHash GetByUuid(string uuid)
        {
            return GetFirst(x => x.InvoiceUuid == uuid);
        }

        public int InsertInvoiceHash(ZatcaInvoiceHash model)
        {
            return Insert(model);
        }

        public async Task<int> InsertInvoiceHashAsync(ZatcaInvoiceHash model)
        {
            var result = await InsertAsync(model);
            return result ? 1 : 0;
        }

        public int UpdateInvoiceHash(ZatcaInvoiceHash model)
        {
            return Context.Updateable(model)
                .IgnoreColumns(it => new { it.Create_by, it.Create_time })
                .ExecuteCommand();
        }

        public async Task<int> UpdateInvoiceHashAsync(ZatcaInvoiceHash model)
        {
            return await Context.Updateable(model)
                .IgnoreColumns(it => new { it.Create_by, it.Create_time })
                .ExecuteCommandAsync();
        }

        #endregion

        #region Counter Operations

        public long GetNextCounterValue(long? companyId)
        {
            var maxCounter = Queryable()
                .Where(x => x.CompanyID == companyId)
                .Max(x => (long?)x.InvoiceCounterValue) ?? 0;
            return maxCounter + 1;
        }

        public async Task<long> GetNextCounterValueAsync(long? companyId)
        {
            var maxCounter = await Context.Queryable<ZatcaInvoiceHash>()
                .Where(x => x.CompanyID == companyId)
                .MaxAsync(x => (long?)x.InvoiceCounterValue) ?? 0;
            return maxCounter + 1;
        }

        public ZatcaInvoiceHash GetLatestInvoiceHash(long? companyId)
        {
            return Queryable()
                .Where(x => x.CompanyID == companyId)
                .OrderByDescending(x => x.InvoiceCounterValue)
                .First();
        }

        public async Task<ZatcaInvoiceHash> GetLatestInvoiceHashAsync(long? companyId)
        {
            return await Context.Queryable<ZatcaInvoiceHash>()
                .Where(x => x.CompanyID == companyId)
                .OrderByDescending(x => x.InvoiceCounterValue)
                .FirstAsync();
        }

        public string GetPreviousInvoiceHash(long? companyId)
        {
            var latest = GetLatestInvoiceHash(companyId);
            return latest?.InvoiceHash ?? ZatcaInvoiceHash.InitialPreviousInvoiceHash;
        }

        #endregion

        #region Query Operations

        public List<ZatcaInvoiceHash> GetByStatus(ZatcaSubmissionStatus status, long? companyId = null)
        {
            var query = Queryable().Where(x => x.SubmissionStatus == status);
            if (companyId.HasValue)
            {
                query = query.Where(x => x.CompanyID == companyId);
            }
            return query.OrderByDescending(x => x.IssueDateTime).ToList();
        }

        public async Task<List<ZatcaInvoiceHash>> GetByStatusAsync(ZatcaSubmissionStatus status, long? companyId = null)
        {
            var query = Context.Queryable<ZatcaInvoiceHash>().Where(x => x.SubmissionStatus == status);
            if (companyId.HasValue)
            {
                query = query.Where(x => x.CompanyID == companyId);
            }
            return await query.OrderByDescending(x => x.IssueDateTime).ToListAsync();
        }

        public List<ZatcaInvoiceHash> GetPendingInvoices(long? companyId = null, int maxRetries = 3)
        {
            var query = Queryable()
                .Where(x => (x.SubmissionStatus == ZatcaSubmissionStatus.NotSubmitted ||
                            x.SubmissionStatus == ZatcaSubmissionStatus.Pending) &&
                            x.RetryCount < maxRetries);
            if (companyId.HasValue)
            {
                query = query.Where(x => x.CompanyID == companyId);
            }
            return query.OrderBy(x => x.InvoiceCounterValue).ToList();
        }

        public List<ZatcaInvoiceHash> GetInvoicesNeedingRetry(int maxRetries = 3, long? companyId = null)
        {
            var query = Queryable()
                .Where(x => (x.SubmissionStatus == ZatcaSubmissionStatus.Failed ||
                            x.SubmissionStatus == ZatcaSubmissionStatus.Rejected) &&
                            x.RetryCount < maxRetries);
            if (companyId.HasValue)
            {
                query = query.Where(x => x.CompanyID == companyId);
            }
            return query.OrderBy(x => x.InvoiceCounterValue).ToList();
        }

        public List<ZatcaInvoiceHash> GetByDateRange(DateTime startDate, DateTime endDate, long? companyId = null)
        {
            var query = Queryable()
                .Where(x => x.IssueDateTime >= startDate && x.IssueDateTime <= endDate);
            if (companyId.HasValue)
            {
                query = query.Where(x => x.CompanyID == companyId);
            }
            return query.OrderByDescending(x => x.IssueDateTime).ToList();
        }

        #endregion

        #region Status Operations

        public int UpdateSubmissionStatus(Guid id, ZatcaSubmissionStatus status, string responseCode, string clearanceStatus, string reportingStatus)
        {
            var submittedAt = status == ZatcaSubmissionStatus.Pending ? DateTime.Now : (DateTime?)null;

            return Context.Updateable<ZatcaInvoiceHash>()
                .SetColumns(x => new ZatcaInvoiceHash
                {
                    SubmissionStatus = status,
                    ZatcaResponseCode = responseCode,
                    ClearanceStatus = clearanceStatus,
                    ReportingStatus = reportingStatus,
                    SubmittedAt = submittedAt ?? x.SubmittedAt,
                    Update_time = DateTime.Now
                })
                .Where(x => x.Id == id)
                .ExecuteCommand();
        }

        public async Task<int> UpdateSubmissionStatusAsync(Guid id, ZatcaSubmissionStatus status, string responseCode, string clearanceStatus, string reportingStatus)
        {
            return await Context.Updateable<ZatcaInvoiceHash>()
                .SetColumns(x => new ZatcaInvoiceHash
                {
                    SubmissionStatus = status,
                    ZatcaResponseCode = responseCode,
                    ClearanceStatus = clearanceStatus,
                    ReportingStatus = reportingStatus,
                    Update_time = DateTime.Now
                })
                .Where(x => x.Id == id)
                .ExecuteCommandAsync();
        }

        public int UpdateValidationResults(Guid id, string validationResults, string errorMessages, int warningCount, int errorCount)
        {
            return Context.Updateable<ZatcaInvoiceHash>()
                .SetColumns(x => new ZatcaInvoiceHash
                {
                    ValidationResults = validationResults,
                    ErrorMessages = errorMessages,
                    WarningCount = warningCount,
                    ErrorCount = errorCount,
                    Update_time = DateTime.Now
                })
                .Where(x => x.Id == id)
                .ExecuteCommand();
        }

        public int IncrementRetryCount(Guid id)
        {
            var hash = GetById(id);
            if (hash == null) return 0;

            return Context.Updateable<ZatcaInvoiceHash>()
                .SetColumns(x => new ZatcaInvoiceHash
                {
                    RetryCount = hash.RetryCount + 1,
                    LastRetryAt = DateTime.Now,
                    Update_time = DateTime.Now
                })
                .Where(x => x.Id == id)
                .ExecuteCommand();
        }

        public int UpdateQrCode(Guid id, string qrCode)
        {
            return Context.Updateable<ZatcaInvoiceHash>()
                .SetColumns(x => new ZatcaInvoiceHash
                {
                    QrCode = qrCode,
                    Update_time = DateTime.Now
                })
                .Where(x => x.Id == id)
                .ExecuteCommand();
        }

        public int UpdateSignedXml(Guid id, string signedXml, string hash, string cryptographicStamp)
        {
            return Context.Updateable<ZatcaInvoiceHash>()
                .SetColumns(x => new ZatcaInvoiceHash
                {
                    SignedXml = signedXml,
                    InvoiceHash = hash,
                    CryptographicStamp = cryptographicStamp,
                    Update_time = DateTime.Now
                })
                .Where(x => x.Id == id)
                .ExecuteCommand();
        }

        #endregion
    }
}
