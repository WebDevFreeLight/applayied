using Infrastructure.Converter;
using SqlSugar;
using Newtonsoft.Json;
using ZR.Model.System;

namespace ZR.Model.EventManagement
{
    /// <summary>
    /// Event Booth/Stall - الأكشاك أو المحلات المؤجرة
    /// </summary>
    [SugarTable("event_booths")]
    
    [SqlDict(
        dictType: "booth",
        dictName: "Event Booth - الأكشاك",
        valueColumn: "boothcode",
        labelColumns: "boothcode, ' - ', boothname, ' - ', boothnamear",
        RemarkColumn = "description",
        OrderBy = "displayorder ASC",
        Description = "Auto-generated from EventBooth table"
    )]
    public class EventBooth : SysBase
    {
        /// <summary>
        /// Booth ID - معرف الكشك
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long BoothId { get; set; }


        public string BoothCode { get; set; }

        /// <summary>
        /// Event ID - معرف الفعالية
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EventId { get; set; }

        /// <summary>
        /// Zone ID - معرف المنطقة
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? ZoneId { get; set; }

        /// <summary>
        /// Booth Number - رقم الكشك
        /// </summary>
        public string BoothNumber { get; set; }

        /// <summary>
        /// Booth Name (English) - اسم الكشك بالإنجليزية
        /// </summary>
        public string BoothName { get; set; }

        /// <summary>
        /// Booth Name (Arabic) - اسم الكشك بالعربية
        /// </summary>
        public string BoothNameAr { get; set; }

        /// <summary>
        /// Description - الوصف
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Booth Type - نوع الكشك (food, retail, services, exhibition)
        /// </summary>
        public string BoothType { get; set; }

        /// <summary>
        /// Booth Category - فئة الكشك
        /// </summary>
        public string BoothCategory { get; set; }

        /// <summary>
        /// Size (Square Meters) - المساحة بالمتر المربع
        /// </summary>
        public decimal SizeSqm { get; set; }

        /// <summary>
        /// Rental Price - سعر الإيجار
        /// </summary>
        public decimal RentalPrice { get; set; }

        /// <summary>
        /// Currency - العملة
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// Rental Period Type - نوع فترة الإيجار (daily, weekly, full-event)
        /// </summary>
        public string RentalPeriodType { get; set; }

        /// <summary>
        /// Tenant ID - معرف المستأجر
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? TenantId { get; set; }

        /// <summary>
        /// Contract ID - معرف العقد
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? ContractId { get; set; }

        /// <summary>
        /// Rental Status - حالة الإيجار (available, rented, reserved, maintenance)
        /// </summary>
        public string RentalStatus { get; set; }

        /// <summary>
        /// Has Electricity - يحتوي على كهرباء
        /// </summary>
        public bool HasElectricity { get; set; }

        /// <summary>
        /// Has Water - يحتوي على ماء
        /// </summary>
        public bool HasWater { get; set; }

        /// <summary>
        /// Has Internet - يحتوي على إنترنت
        /// </summary>
        public bool HasInternet { get; set; }

        /// <summary>
        /// Furniture Included - يشمل الأثاث
        /// </summary>
        public bool FurnitureIncluded { get; set; }

        /// <summary>
        /// Furniture Description - وصف الأثاث
        /// </summary>
        public string FurnitureDescription { get; set; }

        /// <summary>
        /// Additional Amenities - مرافق إضافية
        /// </summary>
        public string AdditionalAmenities { get; set; }

        /// <summary>
        /// Setup Date - تاريخ التجهيز
        /// </summary>
        public DateTime? SetupDate { get; set; }

        /// <summary>
        /// Teardown Date - تاريخ التفكيك
        /// </summary>
        public DateTime? TeardownDate { get; set; }

        /// <summary>
        /// Floor Plan Image URL - رابط صورة المخطط
        /// </summary>
        public string FloorPlanImageUrl { get; set; }

 

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

