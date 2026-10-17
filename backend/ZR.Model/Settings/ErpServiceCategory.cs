using Infrastructure.Converter;
using SqlSugar;
using Newtonsoft.Json;
using ZR.Model.System;

namespace ZR.Model.Settings
{
    /// <summary>
    /// Service Category - فئة الخدمات
    /// </summary>
    [SugarTable("erp_service_categories")]
    [SqlDict(
        dictType: "erp_service_category",
        dictName: "Service Category - فئة الخدمات",
        valueColumn: "categoryid",
        labelColumns: "categoryname",
        RemarkColumn = "categorynamear",
        OrderBy = "sortorder ASC",
        Description = "Auto-generated from ErpServiceCategory table"
    )]
    public class ErpServiceCategory : SysBase
    {
        /// <summary>
        /// Category ID - معرف الفئة
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long CategoryId { get; set; }

        /// <summary>
        /// Category Code - كود الفئة
        /// </summary>
        public string CategoryCode { get; set; }

        /// <summary>
        /// Category Name - اسم الفئة
        /// </summary>
        public string CategoryName { get; set; }

        /// <summary>
        /// Category Name Arabic - اسم الفئة بالعربي
        /// </summary>
        public string CategoryNameAr { get; set; }

        /// <summary>
        /// Parent Category ID - معرف الفئة الأب
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? ParentCategoryId { get; set; }

        /// <summary>
        /// Image URL - رابط الصورة
        /// </summary>
        public string ImageUrl { get; set; }

        /// <summary>
        /// Color - اللون
        /// </summary>
        public string Color { get; set; }

        /// <summary>
        /// Sort Order - ترتيب العرض
        /// </summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// Description - الوصف
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Is Active - نشط
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
}

