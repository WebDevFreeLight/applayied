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
    /// ZATCA Submission Log Repository Implementation
    /// تنفيذ مستودع سجل إرسال زاتكا
    /// </summary>
    [AppService(ServiceType = typeof(IZatcaSubmissionLogRepository), ServiceLifetime = LifeTime.Transient)]
    public class ZatcaSubmissionLogRepository : BaseRepository<ZatcaSubmissionLog>, IZatcaSubmissionLogRepository
    {
        public PagedInfo<ZatcaSubmissionLog> GetList(ZatcaSubmissionLog parm, PagerInfo pager)
        {
            var predicate = Expressionable.Create<ZatcaSubmissionLog>();

            predicate = predicate.AndIF(parm.CompanyID.HasValue, it => it.CompanyID == parm.CompanyID);
            predicate = predicate.AndIF(parm.InvoiceHashId != Guid.Empty, it => it.InvoiceHashId == parm.InvoiceHashId);
            predicate = predicate.AndIF(parm.DeviceId != Guid.Empty, it => it.DeviceId == parm.DeviceId);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.OperationType),
                it => it.OperationType == parm.OperationType);

            return Queryable()
                .Where(predicate.ToExpression())
                .OrderByDescending(x => x.SubmissionDate)
                .ToPage(pager);
        }

        public ZatcaSubmissionLog GetById(Guid id)
        {
            return GetFirst(x => x.Id == id);
        }

        public async Task<ZatcaSubmissionLog> GetByIdAsync(Guid id)
        {
            return await GetFirstAsync(x => x.Id == id);
        }

        public List<ZatcaSubmissionLog> GetByInvoiceHashId(Guid invoiceHashId)
        {
            return Queryable()
                .Where(x => x.InvoiceHashId == invoiceHashId)
                .OrderByDescending(x => x.SubmissionDate)
                .ToList();
        }

        public async Task<List<ZatcaSubmissionLog>> GetByInvoiceHashIdAsync(Guid invoiceHashId)
        {
            return await Context.Queryable<ZatcaSubmissionLog>()
                .Where(x => x.InvoiceHashId == invoiceHashId)
                .OrderByDescending(x => x.SubmissionDate)
                .ToListAsync();
        }

        public int InsertLog(ZatcaSubmissionLog model)
        {
            return Insert(model);
        }

        public async Task<int> InsertLogAsync(ZatcaSubmissionLog model)
        {
            var result = await InsertAsync(model);
            return result ? 1 : 0;
        }

        public List<ZatcaSubmissionLog> GetByDateRange(DateTime startDate, DateTime endDate, long? companyId = null)
        {
            var query = Queryable()
                .Where(x => x.SubmissionDate >= startDate && x.SubmissionDate <= endDate);
            if (companyId.HasValue)
            {
                query = query.Where(x => x.CompanyID == companyId);
            }
            return query.OrderByDescending(x => x.SubmissionDate).ToList();
        }

        public List<ZatcaSubmissionLog> GetFailedLogs(DateTime? since = null, long? companyId = null)
        {
            var query = Queryable().Where(x => !x.IsSuccess);
            if (since.HasValue)
            {
                query = query.Where(x => x.SubmissionDate >= since.Value);
            }
            if (companyId.HasValue)
            {
                query = query.Where(x => x.CompanyID == companyId);
            }
            return query.OrderByDescending(x => x.SubmissionDate).ToList();
        }

        public (int total, int success, int failed, decimal avgResponseTime) GetStatistics(DateTime? since = null, long? companyId = null)
        {
            var query = Queryable();
            if (since.HasValue)
            {
                query = query.Where(x => x.SubmissionDate >= since.Value);
            }
            if (companyId.HasValue)
            {
                query = query.Where(x => x.CompanyID == companyId);
            }

            var logs = query.ToList();
            var total = logs.Count;
            var success = logs.Count(x => x.IsSuccess);
            var failed = total - success;
            var avgResponseTime = logs.Where(x => x.ResponseTimeMs.HasValue).Average(x => x.ResponseTimeMs ?? 0);

            return (total, success, failed, (decimal)avgResponseTime);
        }

        public int GetTodaySubmissionCount(long? companyId = null)
        {
            var today = DateTime.Today;
            var query = Queryable().Where(x => x.SubmissionDate >= today);
            if (companyId.HasValue)
            {
                query = query.Where(x => x.CompanyID == companyId);
            }
            return query.Count();
        }

        public decimal GetSuccessRate(DateTime? since = null, long? companyId = null)
        {
            var (total, success, _, _) = GetStatistics(since, companyId);
            if (total == 0) return 100m;
            return Math.Round((decimal)success / total * 100, 2);
        }
    }
}
