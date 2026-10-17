using System;
using System.Collections.Generic;
using Infrastructure.Attribute;
using SqlSugar;
using ZR.Model;
using ZR.Model.Finance.ARAP;

namespace ZR.Repository.Finance.ARAP
{
    /// <summary>
    /// AP Invoice Repository - مستودع فواتير الموردين
    /// </summary>
    [AppService(ServiceType = typeof(IAPInvoiceRepository), ServiceLifetime = LifeTime.Transient)]
    public class APInvoiceRepository : BaseRepository<APInvoice>, IAPInvoiceRepository
    {
        /// <summary>
        /// Get paginated list of invoices
        /// </summary>
        public PagedInfo<APInvoice> GetList(APInvoice parm, PagerInfo pager)
        {
            var predicate = Expressionable.Create<APInvoice>();
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.InvoiceNumber), it => it.InvoiceNumber.Contains(parm.InvoiceNumber));
            predicate = predicate.AndIF(parm.VendorId != Guid.Empty, it => it.VendorId == parm.VendorId);
            predicate = predicate.AndIF(parm.CompanyID.HasValue, it => it.CompanyID == parm.CompanyID);

            return Queryable()
                .Where(predicate.ToExpression())
                .OrderByDescending(x => x.InvoiceDate)
                .ToPage(pager);
        }

        /// <summary>
        /// Get invoice by ID
        /// </summary>
        public APInvoice GetById(Guid id)
        {
            return GetFirst(x => x.Id == id);
        }

        /// <summary>
        /// Get invoice by number
        /// </summary>
        public APInvoice GetByInvoiceNumber(string invoiceNumber)
        {
            return GetFirst(x => x.InvoiceNumber == invoiceNumber);
        }

        /// <summary>
        /// Get invoices by vendor
        /// </summary>
        public List<APInvoice> GetByVendorId(Guid vendorId)
        {
            return Queryable()
                .Where(x => x.VendorId == vendorId)
                .OrderByDescending(x => x.InvoiceDate)
                .ToList();
        }

        /// <summary>
        /// Get overdue invoices
        /// </summary>
        public List<APInvoice> GetOverdueInvoices(DateTime asOfDate)
        {
            return Queryable()
                .Where(x => x.DueDate < asOfDate && x.RemainingAmount > 0)
                .OrderBy(x => x.DueDate)
                .ToList();
        }
    }
}


