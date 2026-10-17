using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZR.Service.Finance.Treasury
{
    /// <summary>
    /// Treasury Number Sequence Service Interface - واجهة خدمة تسلسل أرقام الخزينة
    /// Implements requirement #25: atomic number sequence generation
    /// </summary>
    public interface ITreasuryNumberSequenceService
    {
        /// <summary>
        /// Get next payment number - الحصول على رقم الدفعة التالي
        /// </summary>
        string GetNextPaymentNumber(long? companyId = null);

        /// <summary>
        /// Get next receipt number - الحصول على رقم الإيصال التالي
        /// </summary>
        string GetNextReceiptNumber(long? companyId = null);

        /// <summary>
        /// Get next transfer number - الحصول على رقم التحويل التالي
        /// </summary>
        string GetNextTransferNumber(long? companyId = null);

        /// <summary>
        /// Get next cheque number - الحصول على رقم الشيك التالي
        /// </summary>
        string GetNextChequeNumber(Guid bankAccountId, long? companyId = null);

        /// <summary>
        /// Get next reconciliation number - الحصول على رقم التسوية التالي
        /// </summary>
        string GetNextReconciliationNumber(long? companyId = null);

        /// <summary>
        /// Initialize sequence if not exists - تهيئة التسلسل إذا لم يكن موجودًا
        /// </summary>
        void InitializeSequence(string sequenceType, long? companyId, string prefix);

        /// <summary>
        /// Get next number atomically - الحصول على الرقم التالي بشكل ذري
        /// </summary>
        string GetNextNumber(string sequenceType, long? companyId = null);

        /// <summary>
        /// Get next voucher number - الحصول على رقم السند التالي
        /// Format: {prefix}-YYYYMM-NNNNN (monthly reset)
        /// </summary>
        /// <param name="prefix">"RQ" for Receipt Voucher, "DS" for Disbursement Voucher</param>
        string GetNextVoucherNumber(string prefix, long? companyId = null);
    }

}
