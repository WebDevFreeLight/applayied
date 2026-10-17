using System;
using SqlSugar;
using ZR.Model.System;

namespace ZR.Model.Finance.Configuration
{
    /// <summary>
    /// Currency - العملة
    /// ISO 4217 compliant currency management
    /// </summary>
    [SugarTable("currencies")]
    public class Currency : SysBase
    {
        /// <summary>
        /// Currency ID - معرف العملة
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false, ColumnName = "id")]
        public Guid Id { get; set; }

        /// <summary>
        /// ISO 4217 Currency Code (e.g., USD, EUR, SAR)
        /// رمز العملة حسب المعيار الدولي
        /// </summary>
        [SugarColumn(Length = 3, IsNullable = false, ColumnName = "code")]
        public string Code { get; set; }

        /// <summary>
        /// Currency Name (e.g., US Dollar)
        /// اسم العملة بالإنجليزية
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = false, ColumnName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Currency Name in Arabic
        /// اسم العملة بالعربية
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true, ColumnName = "name_ar")]
        public string NameAr { get; set; }

        /// <summary>
        /// Currency Symbol (e.g., $, €, ﷼)
        /// رمز العملة
        /// </summary>
        [SugarColumn(Length = 10, IsNullable = true, ColumnName = "symbol")]
        public string Symbol { get; set; }

        /// <summary>
        /// Is this the base (functional) currency?
        /// هل هذه العملة الأساسية؟
        /// RULE: Only ONE currency can be base currency
        /// </summary>
        [SugarColumn(ColumnName = "is_base_currency")]
        public bool IsBaseCurrency { get; set; }

        /// <summary>
        /// Decimal Places (0-6, typically 2)
        /// عدد الخانات العشرية
        /// RULE: Must match ISO standard if possible
        /// </summary>
        [SugarColumn(ColumnName = "decimal_places")]
        public int DecimalPlaces { get; set; } = 2;

        /// <summary>
        /// Is currency active?
        /// هل العملة نشطة؟
        /// RULE: Base currency cannot be deactivated
        /// </summary>
        [SugarColumn(ColumnName = "is_active")]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Rounding Method (0=Standard, 1=Up, 2=Down)
        /// طريقة التقريب
        /// </summary>
        [SugarColumn(ColumnName = "rounding_method")]
        public int RoundingMethod { get; set; } = 0;

        /// <summary>
        /// Display Format (e.g., "#,##0.00")
        /// تنسيق العرض
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnName = "display_format")]
        public string DisplayFormat { get; set; }

        /// <summary>
        /// Description / Notes
        /// ملاحظات
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnName = "description")]
        public string Description { get; set; }

        /// <summary>
        /// Sort Order
        /// ترتيب العرض
        /// </summary>
        [SugarColumn(ColumnName = "sort_order")]
        public int SortOrder { get; set; }
    }
}
