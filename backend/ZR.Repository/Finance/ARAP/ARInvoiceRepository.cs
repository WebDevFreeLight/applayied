using System;
using System.Collections.Generic;
using Infrastructure.Attribute;
using SqlSugar;
using ZR.Model;
using ZR.Model.Finance.ARAP;
using ZR.Model.Finance.ARAP.Enums;

namespace ZR.Repository.Finance.ARAP
{
    /// <summary>
    /// AR Invoice Repository - مستودع فواتير العملاء
    /// </summary>
    [AppService(ServiceType = typeof(IARInvoiceRepository), ServiceLifetime = LifeTime.Transient)]
    public class ARInvoiceRepository : BaseRepository<ARInvoice>, IARInvoiceRepository
    {
        /// <summary>
        /// Get paginated list of invoices
        /// </summary>
        public PagedInfo<ARInvoice> GetList(ARInvoice parm, PagerInfo pager)
        {
            var predicate = Expressionable.Create<ARInvoice>();
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.InvoiceNumber), it => it.InvoiceNumber.Contains(parm.InvoiceNumber));
            predicate = predicate.AndIF(parm.CustomerId != Guid.Empty, it => it.CustomerId == parm.CustomerId);
            predicate = predicate.AndIF(parm.CompanyID.HasValue, it => it.CompanyID == parm.CompanyID);

            return Queryable()
                .Where(predicate.ToExpression())
                .OrderByDescending(x => x.InvoiceDate)
                .ToPage(pager);
        }

        /// <summary>
        /// Get invoice by ID
        /// </summary>
        public ARInvoice GetById(Guid id)
        {
            return GetFirst(x => x.Id == id);
        }

        /// <summary>
        /// Get invoice by number
        /// </summary>
        public ARInvoice GetByInvoiceNumber(string invoiceNumber)
        {
            return GetFirst(x => x.InvoiceNumber == invoiceNumber);
        }

        /// <summary>
        /// Get invoices by customer
        /// </summary>
        public List<ARInvoice> GetByCustomerId(Guid customerId)
        {
            return Queryable()
                .Where(x => x.CustomerId == customerId)
                .OrderByDescending(x => x.InvoiceDate)
                .ToList();
        }

        /// <summary>
        /// Get overdue invoices
        /// </summary>
        public List<ARInvoice> GetOverdueInvoices(DateTime asOfDate)
        {
            return Queryable()
                .Where(x => x.DueDate < asOfDate && x.RemainingAmount > 0)
                .OrderBy(x => x.DueDate)
                .ToList();
        }
    }
}


