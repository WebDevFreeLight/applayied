using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZR.Model;
using ZR.Model.Finance.Zatca;
using ZR.Model.Finance.Zatca.Enums;

namespace ZR.Repository.Finance.Zatca
{
    /// <summary>
    /// ZATCA Submission Log Repository Interface
    /// واجهة مستودع سجل إرسال زاتكا
    /// </summary>
    public interface IZatcaSubmissionLogRepository : IBaseRepository<ZatcaSubmissionLog>
    {
        /// <summary>
        /// Get paginated list of submission logs
        /// </summary>
        PagedInfo<ZatcaSubmissionLog> GetList(ZatcaSubmissionLog parm, PagerInfo pager);

        /// <summary>
        /// Get submission log by ID
        /// </summary>
        ZatcaSubmissionLog GetById(Guid id);

        /// <summary>
        /// Get submission log by ID (async)
        /// </summary>
        Task<ZatcaSubmissionLog> GetByIdAsync(Guid id);

        /// <summary>
        /// Get logs by invoice hash ID
        /// </summary>
        List<ZatcaSubmissionLog> GetByInvoiceHashId(Guid invoiceHashId);

        /// <summary>
        /// Get logs by invoice hash ID (async)
        /// </summary>
        Task<List<ZatcaSubmissionLog>> GetByInvoiceHashIdAsync(Guid invoiceHashId);

        /// <summary>
        /// Insert submission log
        /// </summary>
        int InsertLog(ZatcaSubmissionLog model);

        /// <summary>
        /// Insert submission log (async)
        /// </summary>
        Task<int> InsertLogAsync(ZatcaSubmissionLog model);

        /// <summary>
        /// Get logs by date range
        /// </summary>
        List<ZatcaSubmissionLog> GetByDateRange(DateTime startDate, DateTime endDate, long? companyId = null);

        /// <summary>
        /// Get failed logs
        /// </summary>
        List<ZatcaSubmissionLog> GetFailedLogs(DateTime? since = null, long? companyId = null);

        /// <summary>
        /// Get statistics for dashboard
        /// </summary>
        (int total, int success, int failed, decimal avgResponseTime) GetStatistics(DateTime? since = null, long? companyId = null);

        /// <summary>
        /// Get today's submission count
        /// </summary>
        int GetTodaySubmissionCount(long? companyId = null);

        /// <summary>
        /// Get success rate
        /// </summary>
        decimal GetSuccessRate(DateTime? since = null, long? companyId = null);
    }
}
