using System;
using System.Collections.Generic;
using ZR.Model;
using ZR.Model.Finance.GL.Dto;
using ZR.Model.Finance.Treasury;
using ZR.Model.Finance.Treasury.Dto;
using ZR.Model.Finance.Treasury.Enums;

namespace ZR.Service.IService.Finance.Treasury
{
    /// <summary>
    /// Cheque Service Interface - واجهة خدمة الشيك
    /// </summary>
    public interface IChequeService : IBaseService<Cheque>
    {
        /// <summary>
        /// Get cheque list with paging - الحصول على قائمة الشيكات مع ترقيم الصفحات
        /// </summary>
        PagedInfo<ChequeListDto> GetList(ChequeQueryDto parm);

        /// <summary>
        /// Get cheque by ID - الحصول على شيك بالمعرف
        /// </summary>
        ChequeDetailDto GetDetail(Guid id);

        /// <summary>
        /// Create cheque - إنشاء شيك
        /// </summary>
        Cheque Create(CreateChequeDto dto, string userId);

        /// <summary>
        /// Update cheque - تحديث شيك
        /// </summary>
        int Update(UpdateChequeDto dto, string userId);

        /// <summary>
        /// Delete cheques - حذف شيكات
        /// </summary>
        int Delete(Guid[] ids);

        /// <summary>
        /// Issue cheque - إصدار شيك
        /// </summary>
        Cheque IssueCheque(Guid paymentId, CreateChequeDto dto, string userId);

        /// <summary>
        /// Present cheque for clearing (UnderCollection) - تقديم الشيك للتحصيل
        /// This reserves the available balance to prevent overdraft
        /// </summary>
        int Present(Guid chequeId, string userId);

        /// <summary>
        /// Mark cheque as presented to bank - تعليم الشيك كمقدم للبنك
        /// Transitions from UnderCollection to Presented
        /// </summary>
        int MarkPresented(Guid chequeId, string userId);

        /// <summary>
        /// Clear cheque - تصفية الشيك
        /// For outgoing cheques: reduces CurrentBalance
        /// For incoming cheques: increases CurrentBalance
        /// </summary>
        int Clear(Guid chequeId, string userId);

        /// <summary>
        /// Bounce cheque - ارتداد شيك
        /// </summary>
        int Bounce(Guid chequeId, string userId, string reason);

        /// <summary>
        /// Bounce cheque with bank fees - إرجاع الشيك مع رسوم البنك
        /// Restores available balance and posts bank fees to GL
        /// </summary>
        int BounceWithFees(Guid chequeId, string userId, string reason, decimal bankFees);

        /// <summary>
        /// Cancel cheque - إلغاء شيك
        /// </summary>
        int Cancel(Guid chequeId, string userId, string reason);

        /// <summary>
        /// Get pending cheques - الحصول على الشيكات المعلقة
        /// </summary>
        List<ChequeListDto> GetPendingCheques(Guid? bankAccountId = null, long? companyId = null);

        /// <summary>
        /// Get cheques due in period - الحصول على الشيكات المستحقة في الفترة
        /// </summary>
        List<ChequeListDto> GetChequesDueInPeriod(DateTime startDate, DateTime endDate, Guid? bankAccountId = null, long? companyId = null);

        /// <summary>
        /// Preview GL Journal for cheque operation - معاينة القيد المحاسبي
        /// </summary>
        CreateJournalDto PreviewGLJournal(Guid chequeId, ChequeStatus targetStatus);

        /// <summary>
        /// Get cheque status history - الحصول على سجل تغييرات الحالة
        /// </summary>
        /// <param name="chequeId">Cheque ID</param>
        /// <returns>List of status history entries</returns>
        List<ChequeStatusHistoryDto> GetChequeHistory(Guid chequeId);

        /// <summary>
        /// Get cheque timeline for UI display - الحصول على الجدول الزمني للعرض
        /// </summary>
        /// <param name="chequeId">Cheque ID</param>
        /// <returns>Formatted timeline items</returns>
        List<ChequeTimelineItemDto> GetChequeTimeline(Guid chequeId);

        /// <summary>
        /// Get cheque statistics - الحصول على إحصائيات الشيكات
        /// </summary>
        /// <param name="bankAccountId">Optional bank account filter</param>
        /// <param name="companyId">Optional company filter</param>
        /// <returns>Statistics grouped by status</returns>
        ChequeStatisticsDto GetStatistics(Guid? bankAccountId = null, long? companyId = null);

        /// <summary>
        /// Generate next cheque number - توليد رقم الشيك التالي
        /// Format: CHQ-{BankCode}-{YYYYMMDD}-{Sequence}
        /// </summary>
        /// <param name="bankAccountId">Bank account ID for the cheque</param>
        /// <param name="chequeType">Type of cheque (Outgoing/Incoming)</param>
        /// <returns>Generated cheque number</returns>
        string GenerateNextChequeNumber(Guid bankAccountId, ChequeType chequeType);
    }
}






























