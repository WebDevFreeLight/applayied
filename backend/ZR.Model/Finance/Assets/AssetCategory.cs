using System;
using SqlSugar;
using ZR.Model.Finance.Assets.Enums;
using ZR.Model.System;

namespace ZR.Model.Finance.Assets
{
    /// <summary>
    /// Asset Category - فئة الأصول
    /// </summary>
    [SugarTable("asset_category")]
    public class AssetCategory : SysBase
    {
        /// <summary>
        /// Category ID - معرف الفئة
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false, ColumnName = "id")]
        public Guid Id { get; set; }

        /// <summary>
        /// Category Code - رمز الفئة
        /// </summary>
        [SugarColumn(Length = 50, ColumnName = "category_code")]
        public string CategoryCode { get; set; }

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
        /// Depreciation Method - طريقة الإهلاك
        /// </summary>
        [SugarColumn(ColumnName = "depreciation_method")]
        public DepreciationMethod DepreciationMethod { get; set; }

        /// <summary>
        /// Useful Life in Months - العمر الإنتاجي بالأشهر
        /// </summary>
        [SugarColumn(ColumnName = "useful_life_months")]
        public int UsefulLifeMonths { get; set; }

        /// <summary>
        /// Default Asset Account ID - حساب الأصل الافتراضي
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "default_asset_account_id")]
        public Guid? DefaultAssetAccountId { get; set; }

        /// <summary>
        /// Default Depreciation Expense Account ID - حساب مصروف الإهلاك الافتراضي
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "default_depreciation_expense_account_id")]
        public Guid? DefaultDepreciationExpenseAccountId { get; set; }

        /// <summary>
        /// Default Accumulated Depreciation Account ID - حساب مجمع الإهلاك الافتراضي
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "default_accumulated_depreciation_account_id")]
        public Guid? DefaultAccumulatedDepreciationAccountId { get; set; }

        /// <summary>
        /// Default Gain/Loss Account ID - حساب الأرباح والخسائر الافتراضي
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "default_gain_loss_account_id")]
        public Guid? DefaultGainLossAccountId { get; set; }

        /// <summary>
        /// Is Active - نشط
        /// </summary>
        [SugarColumn(ColumnName = "is_active")]
        public bool IsActive { get; set; } = true;
    }
}

