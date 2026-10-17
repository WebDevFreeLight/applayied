using System;
using System.Collections.Generic;
using ZR.Model;
using ZR.Model.Finance.Treasury;
using ZR.Model.Finance.Treasury.Dto;

namespace ZR.Service.IService.Finance.Treasury
{
    /// <summary>
    /// Bank Statement Service Interface - واجهة خدمة كشف الحساب البنكي
    /// </summary>
    public interface IBankStatementService : IBaseService<BankStatement>
    {
        /// <summary>
        /// Get bank statement list with paging - الحصول على قائمة كشوف الحساب مع ترقيم الصفحات
        /// </summary>
        PagedInfo<BankStatementDetailDto> GetList(BankStatementQueryDto parm);

        /// <summary>
        /// Get bank statement by ID - الحصول على كشف حساب بالمعرف
        /// </summary>
        BankStatementDetailDto GetDetail(Guid id);

        /// <summary>
        /// Create bank statement - إنشاء كشف حساب
        /// </summary>
        BankStatement Create(CreateBankStatementDto dto, string userId);

        /// <summary>
        /// Import bank statement - استيراد كشف حساب
        /// </summary>
        BankStatement ImportBankStatement(UploadBankStatementDto dto, byte[] fileContent, string userId);

        /// <summary>
        /// Get statement lines - الحصول على بنود الكشف
        /// </summary>
        List<BankStatementLineDto> GetStatementLines(Guid statementId);

        /// <summary>
        /// Get unmatched lines - الحصول على البنود غير المطابقة
        /// </summary>
        List<BankStatementLineDto> GetUnmatchedLines(Guid bankAccountId, long? companyId = null);

        /// <summary>
        /// Auto-match lines - مطابقة تلقائية للبنود
        /// </summary>
        int AutoMatchLines(Guid statementId);

        /// <summary>
        /// Match line manually - مطابقة بند يدوياً
        /// </summary>
        int MatchLine(MatchBankStatementLineDto dto, string userId);
    }
}






























