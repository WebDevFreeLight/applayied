using System;
using SqlSugar;
using ZR.Model.Finance.Assets.Enums;
using ZR.Model.System;

namespace ZR.Model.Finance.Assets
{
    /// <summary>
    /// Current Asset - الأصل المتداول
    /// </summary>
    [SugarTable("current_asset")]
    public class CurrentAsset : SysBase
    {
        /// <summary>
        /// Asset ID - معرف الأصل
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false, ColumnName = "id")]
        public Guid Id { get; set; }

        /// <summary>
        /// Item Code - رمز الصنف
        /// </summary>
        [SugarColumn(Length = 50, ColumnName = "item_code")]
        public string ItemCode { get; set; }

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
        /// Cost - التكلفة
        /// </summary>
        [SugarColumn(ColumnName = "cost", DecimalDigits = 4)]
        public decimal Cost { get; set; }

        /// <summary>
        /// Current Value - القيمة الحالية
        /// </summary>
        [SugarColumn(ColumnName = "current_value", DecimalDigits = 4)]
        public decimal CurrentValue { get; set; }

        /// <summary>
        /// GL Account ID - حساب الأستاذ العام
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "gl_account_id")]
        public Guid? GLAccountId { get; set; }

        /// <summary>
        /// Status - الحالة
        /// </summary>
        [SugarColumn(ColumnName = "status")]
        public CurrentAssetStatus Status { get; set; } = CurrentAssetStatus.Active;

        /// <summary>
        /// Category - الفئة
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true, ColumnName = "category")]
        public string Category { get; set; }

        /// <summary>
        /// Notes - ملاحظات
        /// </summary>
        [SugarColumn(Length = 1000, IsNullable = true, ColumnName = "notes")]
        public string Notes { get; set; }
    }
}

