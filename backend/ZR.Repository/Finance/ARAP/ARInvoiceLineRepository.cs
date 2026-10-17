using System;
using System.Collections.Generic;
using Infrastructure.Attribute;
using SqlSugar;
using ZR.Model.Finance.ARAP;

namespace ZR.Repository.Finance.ARAP
{
    /// <summary>
    /// AR Invoice Line Repository - مستودع سطور فواتير العملاء
    /// </summary>
    [AppService(ServiceType = typeof(IARInvoiceLineRepository), ServiceLifetime = LifeTime.Transient)]
    public class ARInvoiceLineRepository : BaseRepository<ARInvoiceLine>, IARInvoiceLineRepository
    {
        /// <summary>
        /// Get invoice lines by invoice ID - الحصول على سطور الفاتورة بمعرف الفاتورة
        /// </summary>
        public List<ARInvoiceLine> GetByInvoiceId(Guid invoiceId)
        {
            return Queryable()
                .Where(x => x.InvoiceId == invoiceId)
                .OrderBy(x => x.LineNumber)
                .ToList();
        }

        /// <summary>
        /// Get invoice line by ID - الحصول على سطر الفاتورة بالمعرف
        /// </summary>
        public ARInvoiceLine GetById(Guid id)
        {
            return GetFirst(x => x.Id == id);
        }

        /// <summary>
        /// Delete lines by invoice ID - حذف السطور بمعرف الفاتورة
        /// </summary>
        public int DeleteByInvoiceId(Guid invoiceId)
        {
            return Context.Deleteable<ARInvoiceLine>()
                .Where(x => x.InvoiceId == invoiceId)
                .ExecuteCommand();
        }

        /// <summary>
        /// Insert multiple lines - إدراج عدة سطور
        /// </summary>
        public int InsertRange(List<ARInvoiceLine> lines)
        {
            return Context.Insertable(lines).ExecuteCommand();
        }
    }
}
