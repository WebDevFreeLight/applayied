using System;
using ZR.Model.Finance.ARAP;
using ZR.Model.Finance.GL;
using ZR.Model.Finance.GL.Dto;

namespace ZR.Service.IService.Finance.ARAP
{
    /// <summary>
    /// ARAP GL Integration Service Interface - واجهة خدمة تكامل الذمم المدينة والدائنة مع دفتر الأستاذ
    /// Handles GL journal creation for AR/AP invoices
    /// </summary>
    public interface IARAPGLIntegrationService
    {
        #region AR Invoice GL Integration

        /// <summary>
        /// Create GL Journal for AR Invoice posting - إنشاء قيد دفتر الأستاذ لترحيل فاتورة العملاء
        /// Debit: Accounts Receivable (Customer GL Account)
        /// Credit: Revenue Account(s) from invoice lines
        /// Credit: VAT Output (if applicable)
        /// </summary>
        GLJournal CreateARInvoiceJournal(ARInvoice invoice, string userName);

        /// <summary>
        /// Preview GL Journal for AR Invoice (without creating) - معاينة قيد دفتر الأستاذ لفاتورة العملاء
        /// </summary>
        CreateJournalDto PreviewARInvoiceJournal(ARInvoice invoice);

        /// <summary>
        /// Reverse AR Invoice GL Journal - عكس قيد فاتورة العملاء
        /// </summary>
        GLJournal ReverseARInvoiceJournal(ARInvoice invoice, string userName, string reason);

        #endregion

        #region AP Invoice GL Integration

        /// <summary>
        /// Create GL Journal for AP Invoice posting - إنشاء قيد دفتر الأستاذ لترحيل فاتورة الموردين
        /// Debit: Expense Account(s) from invoice lines
        /// Debit: VAT Input (if applicable)
        /// Credit: Accounts Payable (Vendor GL Account)
        /// </summary>
        GLJournal CreateAPInvoiceJournal(APInvoice invoice, string userName);

        /// <summary>
        /// Preview GL Journal for AP Invoice (without creating) - معاينة قيد دفتر الأستاذ لفاتورة الموردين
        /// </summary>
        CreateJournalDto PreviewAPInvoiceJournal(APInvoice invoice);

        /// <summary>
        /// Reverse AP Invoice GL Journal - عكس قيد فاتورة الموردين
        /// </summary>
        GLJournal ReverseAPInvoiceJournal(APInvoice invoice, string userName, string reason);

        #endregion
    }
}
