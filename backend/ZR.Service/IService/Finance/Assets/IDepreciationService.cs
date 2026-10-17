using System;
using System.Collections.Generic;
using ZR.Model;
using ZR.Model.Finance.Assets;
using ZR.Model.Finance.Assets.Dto;

namespace ZR.Service.IService.Finance.Assets
{
    /// <summary>
    /// Depreciation Service Interface - واجهة خدمة الإهلاك
    /// </summary>
    public interface IDepreciationService : IBaseService<AssetDepreciation>
    {
        /// <summary>
        /// Get depreciation records with paging - الحصول على سجلات الإهلاك مع ترقيم الصفحات
        /// </summary>
        PagedInfo<DepreciationRecordListDto> GetList(AssetDepreciationQueryDto parm);

        /// <summary>
        /// Get depreciation records for an asset - الحصول على سجلات الإهلاك لأصل معين
        /// </summary>
        List<DepreciationRecordListDto> GetAssetDepreciationHistory(Guid assetId);

        /// <summary>
        /// Generate monthly depreciation for all active assets - توليد الإهلاك الشهري لجميع الأصول النشطة
        /// </summary>
        DepreciationSummaryDto GenerateMonthlyDepreciation(GenerateMonthlyDepreciationDto dto, string userId);

        /// <summary>
        /// Generate depreciation for a single asset - توليد الإهلاك لأصل واحد
        /// </summary>
        AssetDepreciation DepreciateAsset(CreateDepreciationRecordDto dto, string userId);

        /// <summary>
        /// Check if depreciation already exists for period - التحقق من وجود إهلاك للفترة
        /// </summary>
        bool DepreciationExistsForPeriod(Guid assetId, int fiscalYear, int fiscalPeriod);

        /// <summary>
        /// Get total depreciation for an asset - الحصول على إجمالي الإهلاك لأصل
        /// </summary>
        decimal GetTotalDepreciation(Guid assetId);

        /// <summary>
        /// Get depreciation summary by period - الحصول على ملخص الإهلاك بالفترة
        /// </summary>
        DepreciationSummaryDto GetPeriodSummary(int fiscalYear, int fiscalPeriod, long? companyId = null);
    }
}

