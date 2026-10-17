using System;
using System.Collections.Generic;
using ZR.Model;
using ZR.Model.Finance.Assets;
using ZR.Model.Finance.Assets.Dto;

namespace ZR.Service.IService.Finance.Assets
{
    /// <summary>
    /// Asset Transaction Service Interface - واجهة خدمة معاملات الأصول
    /// </summary>
    public interface IAssetTransactionService : IBaseService<AssetTransaction>
    {
        /// <summary>
        /// Get transaction list with paging - الحصول على قائمة المعاملات مع ترقيم الصفحات
        /// </summary>
        PagedInfo<AssetTransactionListDto> GetList(AssetTransactionQueryDto parm);

        /// <summary>
        /// Get transaction by ID - الحصول على معاملة بالمعرف
        /// </summary>
        AssetTransactionDetailDto GetDetail(Guid id);

        /// <summary>
        /// Get transactions for an asset - الحصول على معاملات لأصل معين
        /// </summary>
        List<AssetTransactionListDto> GetAssetTransactions(Guid assetId);

        /// <summary>
        /// Record purchase transaction - تسجيل معاملة شراء
        /// </summary>
        AssetTransaction RecordPurchase(Guid assetId, decimal amount, DateTime transactionDate, string userId, string notes = null, string referenceNumber = null);

        /// <summary>
        /// Record depreciation transaction - تسجيل معاملة إهلاك
        /// </summary>
        AssetTransaction RecordDepreciation(Guid assetId, decimal amount, DateTime transactionDate, string userId, Guid? glVoucherId = null);

        /// <summary>
        /// Record sale transaction - تسجيل معاملة بيع
        /// </summary>
        AssetTransaction RecordSale(Guid assetId, decimal salePrice, decimal gainLoss, DateTime transactionDate, string userId, string notes = null, string referenceNumber = null);

        /// <summary>
        /// Record disposal transaction - تسجيل معاملة استبعاد
        /// </summary>
        AssetTransaction RecordDisposal(Guid assetId, decimal bookValue, DateTime transactionDate, string userId, string notes = null, string referenceNumber = null);

        /// <summary>
        /// Record revaluation transaction - تسجيل معاملة إعادة تقييم
        /// </summary>
        AssetTransaction RecordRevaluation(Guid assetId, decimal adjustmentAmount, DateTime transactionDate, string userId, string notes = null, string referenceNumber = null);
    }
}

