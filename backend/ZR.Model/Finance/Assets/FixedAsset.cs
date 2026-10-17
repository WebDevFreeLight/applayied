using System;
using SqlSugar;
using ZR.Model.Finance.Assets.Enums;
using ZR.Model.System;

namespace ZR.Model.Finance.Assets
{
    /// <summary>
    /// Fixed Asset - الأصل الثابت
    /// </summary>
    [SugarTable("fixed_asset")]
    public class FixedAsset : SysBase
    {
        /// <summary>
        /// Asset ID - معرف الأصل
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false, ColumnName = "id")]
        public Guid Id { get; set; }

        /// <summary>
        /// Asset Code - رمز الأصل
        /// </summary>
        [SugarColumn(Length = 50, ColumnName = "asset_code")]
        public string AssetCode { get; set; }

        /// <summary>
        /// Name (Arabic) - الاسم بالعربي
        /// </summary>
        [SugarColumn(Length = 200, ColumnName = "name_ar")]
        public string NameAr { get; set; }

        /// <summary>
        /// Name (English) - الاسم بالإنجليزي
        /// </summary>
        [SugarColumn(Length = 200, ColumnName = "name_en")]
        public string NameEn { get; set; }

        /// <summary>
        /// Purchase Date - تاريخ الشراء
        /// </summary>
        [SugarColumn(ColumnName = "purchase_date")]
        public DateTime PurchaseDate { get; set; }

        /// <summary>
        /// Purchase Cost - تكلفة الشراء
        /// </summary>
        [SugarColumn(ColumnName = "purchase_cost", DecimalDigits = 4)]
        public decimal PurchaseCost { get; set; }

        /// <summary>
        /// Salvage Value - القيمة التخريدية
        /// </summary>
        [SugarColumn(ColumnName = "salvage_value", DecimalDigits = 4)]
        public decimal SalvageValue { get; set; }

        /// <summary>
        /// Category ID - معرف الفئة
        /// </summary>
        [SugarColumn(ColumnName = "category_id")]
        public Guid CategoryId { get; set; }

        /// <summary>
        /// GL Asset Account ID - حساب الأصل في الأستاذ العام
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "gl_asset_account_id")]
        public Guid? GLAssetAccountId { get; set; }

        /// <summary>
        /// GL Accumulated Depreciation Account ID - حساب مجمع الإهلاك في الأستاذ العام
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "gl_accumulated_depreciation_account_id")]
        public Guid? GLAccumulatedDepreciationAccountId { get; set; }

        /// <summary>
        /// GL Depreciation Expense Account ID - حساب مصروف الإهلاك في الأستاذ العام
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "gl_depreciation_expense_account_id")]
        public Guid? GLDepreciationExpenseAccountId { get; set; }

        /// <summary>
        /// Current Book Value - القيمة الدفترية الحالية
        /// </summary>
        [SugarColumn(ColumnName = "current_book_value", DecimalDigits = 4)]
        public decimal CurrentBookValue { get; set; }

        /// <summary>
        /// Accumulated Depreciation - مجمع الإهلاك
        /// </summary>
        [SugarColumn(ColumnName = "accumulated_depreciation", DecimalDigits = 4)]
        public decimal AccumulatedDepreciation { get; set; }

        /// <summary>
        /// Last Depreciation Date - تاريخ آخر إهلاك
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "last_depreciation_date")]
        public DateTime? LastDepreciationDate { get; set; }

        /// <summary>
        /// Status - حالة الأصل
        /// </summary>
        [SugarColumn(ColumnName = "status")]
        public AssetStatus Status { get; set; } = AssetStatus.Active;

        /// <summary>
        /// Notes - ملاحظات
        /// </summary>
        [SugarColumn(Length = 1000, IsNullable = true, ColumnName = "notes")]
        public string Notes { get; set; }

        /// <summary>
        /// Navigation - Category
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public AssetCategory Category { get; set; }
    }
}

