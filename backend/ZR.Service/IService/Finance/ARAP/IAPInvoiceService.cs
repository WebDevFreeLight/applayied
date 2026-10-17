using System;
using System.Collections.Generic;
using ZR.Model;
using ZR.Model.Finance.ARAP.Dto;
using ZR.Model.Finance.ARAP;

namespace ZR.Service.IService.Finance.ARAP
{
    /// <summary>
    /// AP Invoice Service Interface - واجهة خدمة فواتير الموردين
    /// </summary>
    public interface IAPInvoiceService : IBaseService<APInvoice>
    {
        /// <summary>
        /// Get paginated list of invoices - الحصول على قائمة الفواتير بالترقيم
        /// </summary>
        PagedInfo<APInvoiceListDto> GetList(APInvoiceQueryDto query, PagerInfo pager);

        /// <summary>
        /// Get invoice by ID - الحصول على الفاتورة بالمعرف
        /// </summary>
        APInvoiceDetailDto GetById(Guid id);

        /// <summary>
        /// Create invoice - إنشاء فاتورة
        /// </summary>
        APInvoice Create(CreateAPInvoiceDto dto, string userName);

        /// <summary>
        /// Update invoice - تحديث فاتورة
        /// </summary>
        int Update(UpdateAPInvoiceDto dto, string userName);

        /// <summary>
        /// Delete invoices - حذف فواتير
        /// </summary>
        int Delete(Guid[] ids);

        /// <summary>
        /// Update invoice status - تحديث حالة الفاتورة
        /// </summary>
        void UpdateInvoiceStatus(Guid invoiceId);

        /// <summary>
        /// Prepare GL journal data - إعداد بيانات القيد المحاسبي
        /// </summary>
        object PrepareGLJournalData(Guid invoiceId);

        /// <summary>
        /// Validate invoice for posting - التحقق من صحة الفاتورة للترحيل
        /// </summary>
        InvoiceValidationResultDto ValidateForPosting(Guid invoiceId);

        /// <summary>
        /// Post invoice (create GL journal) - ترحيل الفاتورة (إنشاء قيد اليومية)
        /// </summary>
        InvoicePostResultDto PostInvoice(Guid invoiceId, string userName);

        /// <summary>
        /// Cancel invoice - إلغاء الفاتورة
        /// </summary>
        int CancelInvoice(Guid invoiceId, string reason, string userName);

        /// <summary>
        /// Get unpaid invoices for vendor - الحصول على الفواتير غير المدفوعة للمورد
        /// </summary>
        List<UnpaidInvoiceDto> GetUnpaidInvoices(Guid vendorId);

        /// <summary>
        /// Get next invoice number - الحصول على رقم الفاتورة التالي
        /// </summary>
        string GetNextInvoiceNumber();
    }
}


