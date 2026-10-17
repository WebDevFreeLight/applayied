using System;
using System.Collections.Generic;
using ZR.Model;
using ZR.Model.Finance.ARAP;
using ZR.Model.Finance.ARAP.Enums;

namespace ZR.Repository.Finance.ARAP
{
    /// <summary>
    /// AR Invoice Repository Interface - واجهة مستودع فواتير العملاء
    /// </summary>
    public interface IARInvoiceRepository : IBaseRepository<ARInvoice>
    {
        /// <summary>
        /// Get paginated list of invoices - الحصول على قائمة الفواتير بالترقيم
        /// </summary>
        PagedInfo<ARInvoice> GetList(ARInvoice parm, PagerInfo pager);

        /// <summary>
        /// Get invoice by ID - الحصول على الفاتورة بالمعرف
        /// </summary>
        ARInvoice GetById(Guid id);

        /// <summary>
        /// Get invoice by number - الحصول على الفاتورة بالرقم
        /// </summary>
        ARInvoice GetByInvoiceNumber(string invoiceNumber);

        /// <summary>
        /// Get invoices by customer - الحصول على فواتير العميل
        /// </summary>
        List<ARInvoice> GetByCustomerId(Guid customerId);

        /// <summary>
        /// Get overdue invoices - الحصول على الفواتير المتأخرة
        /// </summary>
        List<ARInvoice> GetOverdueInvoices(DateTime asOfDate);
    }
}


