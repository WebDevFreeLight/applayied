using Infrastructure.Converter;
using SqlSugar;
using Newtonsoft.Json;
using ZR.Model.System;

namespace ZR.Model.Subscriptions
{
    /// <summary>
    /// Activity - النشاط
    /// </summary>
    [SugarTable("erp_activities")]
    [SqlDict(
        dictType: "erp_activity",
        dictName: "Activity - النشاط",
        valueColumn: "activityid",
        labelColumns: "activityname",
        RemarkColumn = "activitynamear",
        OrderBy = "sortorder ASC",
        Description = "Auto-generated from ErpActivity table"
    )]
    public class ErpActivity : SysBase
    {
        /// <summary>
        /// Activity ID - معرف النشاط
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ActivityId { get; set; }

        /// <summary>
        /// Activity Code - كود النشاط
        /// </summary>
        public string ActivityCode { get; set; }

        /// <summary>
        /// Activity Name - اسم النشاط
        /// </summary>
        public string ActivityName { get; set; }

        /// <summary>
        /// Activity Name Arabic - اسم النشاط بالعربي
        /// </summary>
        public string ActivityNameAr { get; set; }

        /// <summary>
        /// Activity Type - نوع النشاط (class/session/course)
        /// </summary>
        public string ActivityType { get; set; }

        /// <summary>
        /// Category ID - معرف الفئة
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? CategoryId { get; set; }

        /// <summary>
        /// Duration Minutes - المدة بالدقائق
        /// </summary>
        public int DurationMinutes { get; set; }

        /// <summary>
        /// Max Capacity - السعة القصوى
        /// </summary>
        public int MaxCapacity { get; set; }

        /// <summary>
        /// Is Paid - نشاط مدفوع
        /// </summary>
        public bool IsPaid { get; set; }

        /// <summary>
        /// Price - السعر
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Resource ID - معرف المورد/الغرفة
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? ResourceId { get; set; }

        /// <summary>
        /// Default Trainer ID - المدرب الافتراضي
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? DefaultTrainerId { get; set; }
 

        /// <summary>
        /// Color - اللون
        /// </summary>
        public string Color { get; set; }

        /// <summary>
        /// Image URL - رابط الصورة
        /// </summary>
        public string ImageUrl { get; set; }

        /// <summary>
        /// Description - الوصف
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Requirements - المتطلبات
        /// </summary>
        public string Requirements { get; set; }

        /// <summary>
        /// Sort Order - ترتيب العرض
        /// </summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// Is Active - نشط
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
}

