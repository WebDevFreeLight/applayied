using System;
using System.Collections.Generic;
using ZR.Model;
using ZR.Model.Finance.Assets;
using ZR.Model.Finance.Assets.Dto;

namespace ZR.Service.IService.Finance.Assets
{
    /// <summary>
    /// Fixed Asset Service Interface - واجهة خدمة الأصول الثابتة
    /// </summary>
    public interface IFixedAssetService : IBaseService<FixedAsset>
    {
        /// <summary>
        /// Get asset list with paging - الحصول على قائمة الأصول مع ترقيم الصفحات
        /// </summary>
        PagedInfo<FixedAssetListDto> GetList(FixedAssetQueryDto parm);

        /// <summary>
        /// Get asset by ID - الحصول على أصل بالمعرف
        /// </summary>
        FixedAssetDetailDto GetDetail(Guid id);

        /// <summary>
        /// Create asset - إنشاء أصل
        /// </summary>
        FixedAsset Create(CreateFixedAssetDto dto, string userId);

        /// <summary>
        /// Update asset - تحديث أصل
        /// </summary>
        int Update(UpdateFixedAssetDto dto, string userId);

        /// <summary>
        /// Delete assets - حذف أصول
        /// </summary>
        int Delete(Guid[] ids);

        /// <summary>
        /// Check if asset code exists - التحقق من وجود رمز الأصل
        /// </summary>
        bool CodeExists(string code, Guid? excludeId = null, long? companyId = null);

        /// <summary>
        /// Calculate depreciation amount for an asset - حساب مبلغ الإهلاك للأصل
        /// </summary>
        decimal CalculateDepreciationAmount(Guid assetId);

        /// <summary>
        /// Check if asset can be deprecated - التحقق من إمكانية إهلاك الأصل
        /// </summary>
        bool CanDepreciate(Guid assetId);

        /// <summary>
        /// Get depreciable assets - الحصول على الأصول القابلة للإهلاك
        /// </summary>
        List<FixedAsset> GetDepreciableAssets(long? companyId = null);

        /// <summary>
        /// Update asset book value - تحديث القيمة الدفترية للأصل
        /// </summary>
        int UpdateBookValue(Guid assetId, decimal depreciationAmount);

        /// <summary>
        /// Sell asset - بيع الأصل
        /// </summary>
        AssetTransaction SellAsset(AssetSaleDto dto, string userId);

        /// <summary>
        /// Dispose asset - استبعاد الأصل
        /// </summary>
        AssetTransaction DisposeAsset(AssetDisposalDto dto, string userId);

        /// <summary>
        /// Revalue asset - إعادة تقييم الأصل
        /// </summary>
        AssetTransaction RevalueAsset(AssetRevaluationDto dto, string userId);

        /// <summary>
        /// Get next asset code - الحصول على رمز الأصل التالي
        /// Format: FA-YYYY-NNNN (e.g., FA-2025-0001)
        /// </summary>
        string GetNextAssetCode(long? companyId = null);
    }
}

