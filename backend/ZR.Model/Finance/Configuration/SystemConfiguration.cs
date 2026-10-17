using System;
using SqlSugar;
using ZR.Model.System;

namespace ZR.Model.Finance.Configuration
{
    /// <summary>
    /// System Configuration - إعدادات النظام
    /// </summary>
    [SugarTable("system_configuration")]
    public class SystemConfiguration : SysBase
    {
        /// <summary>
        /// Configuration ID - معرف الإعداد
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false, ColumnName = "id")]
        public Guid Id { get; set; }

        /// <summary>
        /// Configuration Key - مفتاح الإعداد (e.g., finance.currency.main)
        /// </summary>
        [SugarColumn(Length = 200, ColumnName = "key")]
        public string Key { get; set; }

        /// <summary>
        /// Configuration Value - قيمة الإعداد
        /// </summary>
        [SugarColumn(Length = 2000, ColumnName = "value")]
        public string Value { get; set; }

        /// <summary>
        /// Value Type - نوع القيمة (string, int, bool, json)
        /// </summary>
        [SugarColumn(Length = 50, ColumnName = "value_type")]
        public string ValueType { get; set; } = "string";

        /// <summary>
        /// Description (Arabic) - الوصف بالعربي
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnName = "description_ar")]
        public string DescriptionAr { get; set; }

        /// <summary>
        /// Description (English) - الوصف بالإنجليزي
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnName = "description_en")]
        public string DescriptionEn { get; set; }
    }
}

