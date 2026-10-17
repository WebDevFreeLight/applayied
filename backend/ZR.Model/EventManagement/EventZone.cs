using Infrastructure.Converter;
using SqlSugar;
using Newtonsoft.Json;
using ZR.Model.System;

namespace ZR.Model.EventManagement
{

    /// <summary>
    /// Event Zone - المناطق داخل الفعالية
    /// </summary>
    [SugarTable("event_zones")]
    
    [SqlDict(
        dictType: "zone",
        dictName: "Event Zone - مناطق الفعاليات",
        valueColumn: "zonecode",
        labelColumns: "zonecode, ' - ', zonename, ' - ', zonenamear",
        RemarkColumn = "description",
        OrderBy = "displayorder ASC",
        Description = "Auto-generated from EventZone table"
    )]
    public class EventZone : SysBase
    {
        /// <summary>
        /// Zone ID - معرف المنطقة
        /// </summary>
                [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ZoneId { get; set; }

        /// <summary>
        /// Event ID - معرف الفعالية
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EventId { get; set; }

        /// <summary>
        /// Zone Code - رمز المنطقة
        /// </summary>
        public string ZoneCode { get; set; }

        /// <summary>
        /// Zone Name (English) - اسم المنطقة بالإنجليزية
        /// </summary>
        public string ZoneName { get; set; }

        /// <summary>
        /// Zone Name (Arabic) - اسم المنطقة بالعربية
        /// </summary>
        public string ZoneNameAr { get; set; }

        /// <summary>
        /// Description - الوصف
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Zone Type - نوع المنطقة (vip, general, food, exhibition, etc.)
        /// </summary>
        public string ZoneType { get; set; }

        /// <summary>
        /// Parent Zone ID - معرف المنطقة الأب (للمناطق الفرعية)
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? ParentZoneId { get; set; }

        /// <summary>
        /// Max Capacity - السعة القصوى
        /// </summary>
        public int? MaxCapacity { get; set; }

        /// <summary>
        /// Current Occupancy - العدد الحالي
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public int CurrentOccupancy { get; set; }

        /// <summary>
        /// Area (Square Meters) - المساحة بالمتر المربع
        /// </summary>
        public decimal? AreaSqm { get; set; }

        /// <summary>
        /// Floor Number - رقم الطابق
        /// </summary>
        public int? FloorNumber { get; set; }

        /// <summary>
        /// Building Name - اسم المبنى
        /// </summary>
        public string BuildingName { get; set; }

        /// <summary>
        /// Map Coordinates (JSON) - إحداثيات الخريطة
        /// </summary>
        public string MapCoordinates { get; set; }

        /// <summary>
        /// Color Code - رمز اللون (للتمييز في الخريطة)
        /// </summary>
        public string ColorCode { get; set; }

        /// <summary>
        /// Access Level - مستوى الوصول (public, restricted, vip)
        /// </summary>
        public string AccessLevel { get; set; }

        /// <summary>
        /// Zone Manager ID - معرف مسؤول المنطقة
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? ZoneManagerId { get; set; }

        /// <summary>
        /// Status - الحالة (active, inactive, closed)
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Display Order - ترتيب العرض
        /// </summary>
        public int? DisplayOrder { get; set; }

        /// <summary>
        /// Is Active - نشط
        /// </summary>
        public bool IsActive { get; set; }
    }
}

