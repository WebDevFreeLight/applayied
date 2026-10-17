using System;
using SqlSugar;
using ZR.Model.CustomerAssets.Enums;
using ZR.Model.System;

namespace ZR.Model.CustomerAssets
{
    /// <summary>
    /// Customer Asset (Inventory) - أصل العميل (المخزون)
    /// </summary>
    [SugarTable("customer_asset")]
    public class CustomerAsset : SysBase
    {
        /// <summary>
        /// Asset ID - معرف الأصل
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        public Guid Id { get; set; }

        /// <summary>
        /// Asset Type ID - معرف نوع الأصل
        /// </summary>
        public Guid AssetTypeId { get; set; }

        /// <summary>
        /// Asset Code (CA-YYYY-NNNN) - رمز الأصل
        /// </summary>
        [SugarColumn(Length = 20)]
        public string AssetCode { get; set; }

        /// <summary>
        /// Serial Number - الرقم التسلسلي
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true)]
        public string SerialNumber { get; set; }

        /// <summary>
        /// Name (English) - الاسم بالإنجليزي
        /// </summary>
        [SugarColumn(Length = 200)]
        public string NameEn { get; set; }

        /// <summary>
        /// Name (Arabic) - الاسم بالعربي
        /// </summary>
        [SugarColumn(Length = 200)]
        public string NameAr { get; set; }

        /// <summary>
        /// Current Condition - الحالة الحالية
        /// </summary>
        public AssetCondition CurrentCondition { get; set; } = AssetCondition.New;

        /// <summary>
        /// Current Status - الوضع الحالي
        /// </summary>
        public InventoryAssetStatus CurrentStatus { get; set; } = InventoryAssetStatus.Available;

        /// <summary>
        /// Purchase Date - تاريخ الشراء
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public DateTime? PurchaseDate { get; set; }

        /// <summary>
        /// Purchase Cost - تكلفة الشراء
        /// </summary>
        [SugarColumn(DecimalDigits = 2, IsNullable = true)]
        public decimal? PurchaseCost { get; set; }

        /// <summary>
        /// Warranty Expiry - انتهاء الضمان
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public DateTime? WarrantyExpiry { get; set; }

        /// <summary>
        /// Notes - ملاحظات
        /// </summary>
        [SugarColumn(Length = 1000, IsNullable = true)]
        public string Notes { get; set; }

        #region Navigation Properties

        [SugarColumn(IsIgnore = true)]
        public AssetType AssetType { get; set; }

        #endregion
    }
}
