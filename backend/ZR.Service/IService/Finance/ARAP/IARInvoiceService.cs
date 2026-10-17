using System;
using System.Collections.Generic;
using ZR.Model;
using ZR.Model.Finance.ARAP;
using ZR.Model.Finance.ARAP.Dto;
using ZR.Model.Finance.GL.Dto;

namespace ZR.Service.IService.Finance.ARAP
{
    /// <summary>
    /// AR Invoice Service Interface - واجهة خدمة فواتير العملاء
    /// </summary>
    public interface IARInvoiceService : IBaseService<ARInvoice>
    {
        /// <summary>
        /// Get paginated list of invoices - الحصول على قائمة الفواتير بالترقيم
        /// </summary>
        PagedInfo<ARInvoiceListDto> GetList(ARInvoiceQueryDto query, PagerInfo pager);

        /// <summary>
        /// Get invoice by ID - الحصول على الفاتورة بالمعرف
        /// </summary>
        ARInvoiceDetailDto GetById(Guid id);

        /// <summary>
        /// Create invoice - إنشاء فاتورة
        /// </summary>
        ARInvoice Create(CreateARInvoiceDto dto, string userName);

        /// <summary>
        /// Update invoice - تحديث فاتورة
        /// </summary>
        int Update(UpdateARInvoiceDto dto, string userName);

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
        /// Get unpaid invoices for customer - الحصول على الفواتير غير المدفوعة للعميل
        /// </summary>
        List<UnpaidInvoiceDto> GetUnpaidInvoices(Guid customerId);

        /// <summary>
        /// Get next invoice number (thread-safe) - الحصول على رقم الفاتورة التالي (آمن للخيوط)
        /// Uses optimistic concurrency with InvoiceNumberSequence table
        /// </summary>
        /// <param name="companyId">Optional company ID for multi-tenant support</param>
        /// <returns>Next invoice number in format: AR-INV-{Year}-{Sequence}</returns>
        string GetNextInvoiceNumber(long? companyId = null);

        /// <summary>
        /// Check if invoice number already exists
        /// التحقق إذا كان رقم الفاتورة موجود مسبقاً
        /// </summary>
        /// <param name="invoiceNumber">Invoice number to check</param>
        /// <param name="excludeId">Optional invoice ID to exclude (for updates)</param>
        /// <returns>Response DTO with exists flag: { "code": 200, "data": { "exists": true|false }, "msg": null }</returns>
        CheckCodeResponseDto CheckInvoiceNumberExists(string invoiceNumber, Guid? excludeId = null);

        /// <summary>
        /// Apply payment allocation to invoice - تطبيق تخصيص الدفعة على الفاتورة
        /// Updates PaidAmount, RemainingAmount, and Status
        /// </summary>
        /// <param name="invoiceId">Invoice ID</param>
        /// <param name="allocatedAmount">Amount allocated from payment</param>
        /// <param name="userName">User making the allocation</param>
        void ApplyPaymentAllocation(Guid invoiceId, decimal allocatedAmount, string userName);
    }
}


