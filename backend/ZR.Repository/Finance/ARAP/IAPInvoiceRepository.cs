using System;
using System.Collections.Generic;
using ZR.Model;
using ZR.Model.Finance.ARAP;

namespace ZR.Repository.Finance.ARAP
{
    /// <summary>
    /// AP Invoice Repository Interface - واجهة مستودع فواتير الموردين
    /// </summary>
    public interface IAPInvoiceRepository : IBaseRepository<APInvoice>
    {
        /// <summary>
        /// Get paginated list of invoices - الحصول على قائمة الفواتير بالترقيم
        /// </summary>
        PagedInfo<APInvoice> GetList(APInvoice parm, PagerInfo pager);

        /// <summary>
        /// Get invoice by ID - الحصول على الفاتورة بالمعرف
        /// </summary>
        APInvoice GetById(Guid id);

        /// <summary>
        /// Get invoice by number - الحصول على الفاتورة بالرقم
        /// </summary>
        APInvoice GetByInvoiceNumber(string invoiceNumber);

        /// <summary>
        /// Get invoices by vendor - الحصول على فواتير المورد
        /// </summary>
        List<APInvoice> GetByVendorId(Guid vendorId);

        /// <summary>
        /// Get overdue invoices - الحصول على الفواتير المتأخرة
        /// </summary>
        List<APInvoice> GetOverdueInvoices(DateTime asOfDate);
    }
}


