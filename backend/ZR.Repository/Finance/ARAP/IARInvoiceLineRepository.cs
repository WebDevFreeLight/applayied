using System;
using System.Collections.Generic;
using ZR.Model.Finance.ARAP;

namespace ZR.Repository.Finance.ARAP
{
    /// <summary>
    /// AR Invoice Line Repository Interface - واجهة مستودع سطور فواتير العملاء
    /// </summary>
    public interface IARInvoiceLineRepository
    {
        /// <summary>
        /// Get invoice lines by invoice ID - الحصول على سطور الفاتورة بمعرف الفاتورة
        /// </summary>
        List<ARInvoiceLine> GetByInvoiceId(Guid invoiceId);

        /// <summary>
        /// Get invoice line by ID - الحصول على سطر الفاتورة بالمعرف
        /// </summary>
        ARInvoiceLine GetById(Guid id);

        /// <summary>
        /// Delete lines by invoice ID - حذف السطور بمعرف الفاتورة
        /// </summary>
        int DeleteByInvoiceId(Guid invoiceId);

        /// <summary>
        /// Insert multiple lines - إدراج عدة سطور
        /// </summary>
        int InsertRange(List<ARInvoiceLine> lines);
    }
}
