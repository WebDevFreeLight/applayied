using System;
using System.Collections.Generic;
using System.Linq;
using Infrastructure.Attribute;
using SqlSugar;
using ZR.Model.Finance.Treasury;
using ZR.Model.Finance.Treasury.Enums;

namespace ZR.Repository.Finance.Treasury
{
    /// <summary>
    /// Cheque Status History Repository - مستودع سجل تغييرات حالة الشيك
    /// Provides data access for cheque status history tracking
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class ChequeStatusHistoryRepository : BaseRepository<ChequeStatusHistory>
    {
        /// <summary>
        /// Get history for a specific cheque - الحصول على سجل شيك معين
        /// </summary>
        /// <param name="chequeId">Cheque ID</param>
        /// <returns>List of history entries ordered by date descending</returns>
        public List<ChequeStatusHistory> GetByChequeId(Guid chequeId)
        {
            return Context.Queryable<ChequeStatusHistory>()
                .Where(x => x.ChequeId == chequeId)
                .OrderByDescending(x => x.ChangedAt)
                .ToList();
        }

        /// <summary>
        /// Get latest status change for a cheque - الحصول على آخر تغيير حالة
        /// </summary>
        /// <param name="chequeId">Cheque ID</param>
        /// <returns>Latest history entry or null</returns>
        public ChequeStatusHistory GetLatestByChequeId(Guid chequeId)
        {
            return Context.Queryable<ChequeStatusHistory>()
                .Where(x => x.ChequeId == chequeId)
                .OrderByDescending(x => x.ChangedAt)
                .First();
        }

        /// <summary>
        /// Get all history entries for specific status - الحصول على جميع السجلات لحالة معينة
        /// </summary>
        /// <param name="status">Cheque status to filter by</param>
        /// <returns>List of history entries</returns>
        public List<ChequeStatusHistory> GetByStatus(ChequeStatus status)
        {
            return Context.Queryable<ChequeStatusHistory>()
                .Where(x => x.NewStatus == status)
                .OrderByDescending(x => x.ChangedAt)
                .ToList();
        }

        /// <summary>
        /// Get history in date range - الحصول على السجلات في فترة زمنية
        /// </summary>
        /// <param name="from">Start date</param>
        /// <param name="to">End date</param>
        /// <returns>List of history entries</returns>
        public List<ChequeStatusHistory> GetByDateRange(DateTime from, DateTime to)
        {
            return Context.Queryable<ChequeStatusHistory>()
                .Where(x => x.ChangedAt >= from && x.ChangedAt <= to)
                .OrderByDescending(x => x.ChangedAt)
                .ToList();
        }

        /// <summary>
        /// Get bounce history for a cheque - الحصول على سجل الارتجاعات
        /// </summary>
        /// <param name="chequeId">Cheque ID</param>
        /// <returns>List of bounce entries</returns>
        public List<ChequeStatusHistory> GetBounceHistory(Guid chequeId)
        {
            return Context.Queryable<ChequeStatusHistory>()
                .Where(x => x.ChequeId == chequeId && x.NewStatus == ChequeStatus.Bounced)
                .OrderByDescending(x => x.ChangedAt)
                .ToList();
        }

        /// <summary>
        /// Get history by user - الحصول على السجلات حسب المستخدم
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="from">Optional start date</param>
        /// <param name="to">Optional end date</param>
        /// <returns>List of history entries</returns>
        public List<ChequeStatusHistory> GetByUser(string userId, DateTime? from = null, DateTime? to = null)
        {
            var query = Context.Queryable<ChequeStatusHistory>()
                .Where(x => x.ChangedBy == userId);

            if (from.HasValue)
                query = query.Where(x => x.ChangedAt >= from.Value);

            if (to.HasValue)
                query = query.Where(x => x.ChangedAt <= to.Value);

            return query.OrderByDescending(x => x.ChangedAt).ToList();
        }

        /// <summary>
        /// Count status changes for a cheque - عدد تغييرات الحالة
        /// </summary>
        /// <param name="chequeId">Cheque ID</param>
        /// <returns>Number of status changes</returns>
        public int CountByChequeId(Guid chequeId)
        {
            return Context.Queryable<ChequeStatusHistory>()
                .Where(x => x.ChequeId == chequeId)
                .Count();
        }

        /// <summary>
        /// Delete all history for a cheque - حذف كل السجلات لشيك معين
        /// Used when deleting a cheque
        /// </summary>
        /// <param name="chequeId">Cheque ID</param>
        /// <returns>Number of deleted records</returns>
        public int DeleteByChequeId(Guid chequeId)
        {
            return Context.Deleteable<ChequeStatusHistory>()
                .Where(x => x.ChequeId == chequeId)
                .ExecuteCommand();
        }
    }
}
