using Infrastructure.Converter;
using SqlSugar;
using Newtonsoft.Json;
using ZR.Model.System;

namespace ZR.Model.Subscriptions
{
    /// <summary>
    /// Resource - الموارد والغرف
    /// </summary>
    [SugarTable("erp_resources")]
    [SqlDict(
        dictType: "erp_resource",
        dictName: "Resource - المورد/الغرفة",
        valueColumn: "resourceid",
        labelColumns: "resourcename",
        RemarkColumn = "resourcenamear",
        OrderBy = "resourcename ASC",
        Description = "Auto-generated from ErpResource table"
    )]
    public class ErpResource : SysBase
    {
        /// <summary>
        /// Resource ID - معرف المورد
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ResourceId { get; set; }

        /// <summary>
        /// Resource Code - كود المورد
        /// </summary>
        public string ResourceCode { get; set; }

        /// <summary>
        /// Resource Name - اسم المورد
        /// </summary>
        public string ResourceName { get; set; }

        /// <summary>
        /// Resource Name Arabic - اسم المورد بالعربي
        /// </summary>
        public string ResourceNameAr { get; set; }

        /// <summary>
        /// Resource Type - نوع المورد (room/equipment/court/studio)
        /// </summary>
        public string ResourceType { get; set; }

        /// <summary>
        /// Category ID - معرف الفئة
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? CategoryId { get; set; }

        /// <summary>
        /// Capacity - السعة
        /// </summary>
        public int Capacity { get; set; }

        /// <summary>
        /// Hourly Rate - سعر الساعة
        /// </summary>
        public decimal HourlyRate { get; set; }

        /// <summary>
        /// Half Day Rate - سعر نصف اليوم
        /// </summary>
        public decimal HalfDayRate { get; set; }

        /// <summary>
        /// Full Day Rate - سعر اليوم الكامل
        /// </summary>
        public decimal FullDayRate { get; set; }

        /// <summary>
        /// Location - الموقع
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// Floor - الطابق
        /// </summary>
        public string Floor { get; set; }

        /// <summary>
        /// Amenities - المرافق (JSON)
        /// </summary>
        public string Amenities { get; set; }

        /// <summary>
        /// Working Hours - ساعات العمل (JSON)
        /// </summary>
        public string WorkingHours { get; set; }
 

        /// <summary>
        /// Image URL - رابط الصورة
        /// </summary>
        public string ImageUrl { get; set; }

        /// <summary>
        /// Color - اللون
        /// </summary>
        public string Color { get; set; }

        /// <summary>
        /// Is Bookable - قابل للحجز
        /// </summary>
        public bool IsBookable { get; set; } = true;

        /// <summary>
        /// Min Booking Duration - أقل مدة حجز (دقائق)
        /// </summary>
        public int MinBookingDuration { get; set; } = 30;

        /// <summary>
        /// Max Booking Duration - أقصى مدة حجز (دقائق)
        /// </summary>
        public int MaxBookingDuration { get; set; } = 480;

        /// <summary>
        /// Description - الوصف
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Notes - ملاحظات
        /// </summary>
        public string Notes { get; set; }

        /// <summary>
        /// Is Active - نشط
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
}

