using System;
using System.Collections.Generic;
using ZR.Model.Finance.ARAP;

namespace ZR.Repository.Finance.ARAP
{
    /// <summary>
    /// AP Invoice Line Repository Interface - واجهة مستودع بنود فواتير الموردين
    /// </summary>
    public interface IAPInvoiceLineRepository : IBaseRepository<APInvoiceLine>
    {
        /// <summary>
        /// Get lines by invoice ID - الحصول على بنود الفاتورة بمعرف الفاتورة
        /// </summary>
        List<APInvoiceLine> GetByInvoiceId(Guid invoiceId);

        /// <summary>
        /// Get line by ID - الحصول على السطر بالمعرف
        /// </summary>
        APInvoiceLine GetById(Guid id);

        /// <summary>
        /// Delete lines by invoice ID - حذف بنود الفاتورة بمعرف الفاتورة
        /// </summary>
        int DeleteByInvoiceId(Guid invoiceId);

        /// <summary>
        /// Insert multiple lines - إدراج عدة بنود
        /// </summary>
        int InsertRange(List<APInvoiceLine> lines);
    }
}
