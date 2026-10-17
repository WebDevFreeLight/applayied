using System;
using System.Collections.Generic;
using Infrastructure.Attribute;
using SqlSugar;
using ZR.Model.Finance.ARAP;

namespace ZR.Repository.Finance.ARAP
{
    /// <summary>
    /// AP Invoice Line Repository - مستودع بنود فواتير الموردين
    /// </summary>
    [AppService(ServiceType = typeof(IAPInvoiceLineRepository), ServiceLifetime = LifeTime.Transient)]
    public class APInvoiceLineRepository : BaseRepository<APInvoiceLine>, IAPInvoiceLineRepository
    {
        /// <summary>
        /// Get lines by invoice ID
        /// </summary>
        public List<APInvoiceLine> GetByInvoiceId(Guid invoiceId)
        {
            return Queryable()
                .Where(x => x.InvoiceId == invoiceId)
                .OrderBy(x => x.LineNumber)
                .ToList();
        }

        /// <summary>
        /// Get line by ID
        /// </summary>
        public APInvoiceLine GetById(Guid id)
        {
            return GetFirst(x => x.Id == id);
        }

        /// <summary>
        /// Delete lines by invoice ID
        /// </summary>
        public int DeleteByInvoiceId(Guid invoiceId)
        {
            return Context.Deleteable<APInvoiceLine>()
                .Where(x => x.InvoiceId == invoiceId)
                .ExecuteCommand();
        }

        /// <summary>
        /// Insert multiple lines
        /// </summary>
        public int InsertRange(List<APInvoiceLine> lines)
        {
            return Context.Insertable(lines).ExecuteCommand();
        }
    }
}
