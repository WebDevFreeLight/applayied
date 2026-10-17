using System;
using ZR.Model;
using ZR.Model.Finance.Treasury;
using ZR.Model.Finance.Treasury.Dto;

namespace ZR.Service.IService.Finance.Treasury
{
    /// <summary>
    /// Bank Reconciliation Service Interface - واجهة خدمة التسوية البنكية
    /// </summary>
    public interface IBankReconciliationService : IBaseService<BankReconciliation>
    {
        /// <summary>
        /// Get reconciliation list with paging - الحصول على قائمة التسويات مع ترقيم الصفحات
        /// </summary>
        PagedInfo<ReconciliationDetailDto> GetList(BankReconciliationQueryDto parm);

        /// <summary>
        /// Get reconciliation by ID - الحصول على تسوية بالمعرف
        /// </summary>
        ReconciliationDetailDto GetDetail(Guid id);

        /// <summary>
        /// Start reconciliation - بدء تسوية
        /// </summary>
        BankReconciliation StartReconciliation(CreateReconciliationDto dto, string userId);

        /// <summary>
        /// Auto-match reconciliation items - مطابقة تلقائية لعناصر التسوية
        /// </summary>
        int AutoMatch(Guid reconciliationId);

        /// <summary>
        /// Match reconciliation line - مطابقة بند تسوية
        /// </summary>
        int MatchLine(MatchReconciliationLineDto dto, string userId);

        /// <summary>
        /// Complete reconciliation - إكمال التسوية
        /// </summary>
        ReconciliationResultDto Complete(CompleteReconciliationDto dto, string userId);

        /// <summary>
        /// Get reconciliation report - الحصول على تقرير التسوية
        /// </summary>
        ReconciliationResultDto GetReport(Guid reconciliationId);
    }
}






























