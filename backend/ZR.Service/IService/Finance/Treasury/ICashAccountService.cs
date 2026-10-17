using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZR.Model;
using ZR.Model.Finance.Treasury;
using ZR.Model.Finance.Treasury.Dto;
using ZR.Model.Finance.GL;

namespace ZR.Service.IService.Finance.Treasury
{
    /// <summary>
    /// Cash Account Service Interface - واجهة خدمة الحساب النقدي
    /// </summary>
    public interface ICashAccountService : IBaseService<CashAccount>
    {
        /// <summary>
        /// Get cash account list with paging - الحصول على قائمة الحسابات النقدية مع ترقيم الصفحات
        /// </summary>
        PagedInfo<CashAccountListDto> GetList(CashAccountQueryDto parm);

        /// <summary>
        /// Get cash account by ID - الحصول على حساب نقدي بالمعرف
        /// </summary>
        CashAccountDetailDto GetDetail(Guid id);

        /// <summary>
        /// Create cash account - إنشاء حساب نقدي
        /// </summary>
        CashAccount Create(CreateCashAccountDto dto, string userId);

        /// <summary>
        /// Update cash account - تحديث حساب نقدي
        /// </summary>
        int Update(UpdateCashAccountDto dto, string userId);

        /// <summary>
        /// Delete cash accounts - حذف حسابات نقدية
        /// </summary>
        int Delete(Guid[] ids);

        /// <summary>
        /// Soft delete cash account with transaction check - حذف ناعم للحساب النقدي مع التحقق من المعاملات
        /// Blocks deletion if account has related payments
        /// </summary>
        int SoftDelete(Guid id, string userId);

        /// <summary>
        /// Soft delete multiple cash accounts with transaction check
        /// </summary>
        int SoftDelete(Guid[] ids, string userId);

        /// <summary>
        /// Check if account has related transactions - التحقق من وجود معاملات مرتبطة
        /// </summary>
        bool HasRelatedTransactions(Guid accountId);

        /// <summary>
        /// Check if code exists - التحقق من وجود الرمز
        /// </summary>
        bool CodeExists(string code, Guid? excludeId = null, long? companyId = null);

        /// <summary>
        /// Get balance - الحصول على الرصيد
        /// </summary>
        decimal GetBalance(Guid accountId);

        /// <summary>
        /// Adjust balance - تعديل الرصيد
        /// </summary>
        int AdjustBalance(Guid accountId, decimal adjustmentAmount, string reason, string userId);

        /// <summary>
        /// Create cash account asynchronously with GL auto-creation support
        /// إنشاء حساب نقدي بشكل غير متزامن مع دعم إنشاء حساب دفتر الأستاذ تلقائياً
        /// </summary>
        Task<CashAccount> CreateAsync(CreateCashAccountDto dto, string userId);

        /// <summary>
        /// Gets parent GL accounts suitable for cash account placement
        /// الحصول على حسابات الأصل المناسبة لوضع الحسابات النقدية
        /// </summary>
        Task<List<GLAccount>> GetCashParentAccountOptionsAsync(long? companyId);
    }
}






























