
namespace ZR.Model.EventManagement.Dto
{
    /// <summary>
    /// Event Information - معلومات الفعالية查询对象
    /// </summary>
    public class EventQueryDto : PagerInfo
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? Eventid { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public string Eventcode { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public string Eventname { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public string Eventnamear { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public string Description { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public string Descriptionar { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public string Eventtype { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public string Status { get; set; }
        public DateTime? BeginStartdate { get; set; }
        public DateTime? EndStartdate { get; set; }
        public DateTime? BeginEnddate { get; set; }
        public DateTime? EndEnddate { get; set; }
        public DateTime? BeginRegistrationstartdate { get; set; }
        public DateTime? EndRegistrationstartdate { get; set; }
        public DateTime? BeginRegistrationenddate { get; set; }
        public DateTime? EndRegistrationenddate { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? Locationid { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public string Locationname { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public string Address { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public string City { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public float? Latitude { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public float? Longitude { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public int? Expectedvisitors { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public int? Maxcapacity { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public int? Currentoccupancy { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public string Organizer { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public string Contactemail { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public string Contactphone { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public string Website { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public string Ispublic { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public string Requiresregistration { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public string Ispaidevent { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public float? Ticketprice { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public string Currency { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? Eventmanagerid { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public string Tags { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public string Isactive { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public float? Actualcost { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public float? Budget { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public float? Totalareasqm { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public float? Expectedattendance { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public string Country { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public string Venue { get; set; }
        public DateTime? BeginCreateTime { get; set; }
        public DateTime? EndCreateTime { get; set; }
        public DateTime? BeginUpdateTime { get; set; }
        public DateTime? EndUpdateTime { get; set; }
    }

    /// <summary>
    /// Event Information - معلومات الفعالية输入输出对象
    /// </summary>
    public class EventDto
    {
        [ExcelColumn(Name = "Event ID - معرف الفعالية")]
        [ExcelColumnName("Event ID - معرف الفعالية")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? Eventid { get; set; }
        [Required(ErrorMessage = "Event Code - رمز الفعالية不能为空")]
        [ExcelColumn(Name = "Event Code - رمز الفعالية")]
        [ExcelColumnName("Event Code - رمز الفعالية")]
        public string Eventcode { get; set; }
        [Required(ErrorMessage = "Event Name (English) - اسم الفعالية بالإنجليزية不能为空")]
        [ExcelColumn(Name = "Event Name (English) - اسم الفعالية بالإنجليزية")]
        [ExcelColumnName("Event Name (English) - اسم الفعالية بالإنجليزية")]
        public string Eventname { get; set; }
        [ExcelColumn(Name = "Event Name (Arabic) - اسم الفعالية بالعربية")]
        [ExcelColumnName("Event Name (Arabic) - اسم الفعالية بالعربية")]
        public string Eventnamear { get; set; }
        [ExcelColumn(Name = "Description (English) - الوصف بالإنجليزية")]
        [ExcelColumnName("Description (English) - الوصف بالإنجليزية")]
        public string Description { get; set; }
        [ExcelColumn(Name = "Description (Arabic) - الوصف بالعربية")]
        [ExcelColumnName("Description (Arabic) - الوصف بالعربية")]
        public string Descriptionar { get; set; }
        [ExcelColumn(Name = "Event Type - نوع الفعالية (conference, exhibition, festival, etc.)")]
        [ExcelColumnName("Event Type - نوع الفعالية (conference, exhibition, festival, etc.)")]
        public string Eventtype { get; set; }
        [ExcelColumn(Name = "Event Status - حالة الفعالية (planned, active, completed, cancelled)")]
        [ExcelColumnName("Event Status - حالة الفعالية (planned, active, completed, cancelled)")]
        public string Status { get; set; }
        [ExcelColumn(Name = "Start Date - تاريخ البداية", Format = "yyyy-MM-dd HH:mm:ss", Width = 20)]
        [ExcelColumnName("Start Date - تاريخ البداية")]
        public DateTime? Startdate { get; set; }
        [ExcelColumn(Name = "End Date - تاريخ النهاية", Format = "yyyy-MM-dd HH:mm:ss", Width = 20)]
        [ExcelColumnName("End Date - تاريخ النهاية")]
        public DateTime? Enddate { get; set; }
        [ExcelColumn(Name = "Registration Start Date - تاريخ بداية التسجيل", Format = "yyyy-MM-dd HH:mm:ss", Width = 20)]
        [ExcelColumnName("Registration Start Date - تاريخ بداية التسجيل")]
        public DateTime? Registrationstartdate { get; set; }
        [ExcelColumn(Name = "Registration End Date - تاريخ نهاية التسجيل", Format = "yyyy-MM-dd HH:mm:ss", Width = 20)]
        [ExcelColumnName("Registration End Date - تاريخ نهاية التسجيل")]
        public DateTime? Registrationenddate { get; set; }
        [ExcelColumn(Name = "Location ID - معرف الموقع")]
        [ExcelColumnName("Location ID - معرف الموقع")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? Locationid { get; set; }
        [ExcelColumn(Name = "Location Name - اسم الموقع")]
        [ExcelColumnName("Location Name - اسم الموقع")]
        public string Locationname { get; set; }
        [ExcelColumn(Name = "Address - العنوان")]
        [ExcelColumnName("Address - العنوان")]
        public string Address { get; set; }
        [ExcelColumn(Name = "City - المدينة")]
        [ExcelColumnName("City - المدينة")]
        public string City { get; set; }
        [ExcelColumn(Name = "Latitude - خط العرض")]
        [ExcelColumnName("Latitude - خط العرض")]
        public float Latitude { get; set; }
        [ExcelColumn(Name = "Longitude - خط الطول")]
        [ExcelColumnName("Longitude - خط الطول")]
        public float Longitude { get; set; }
        [ExcelColumn(Name = "Map Image URL - رابط صورة الخريطة")]
        [ExcelColumnName("Map Image URL - رابط صورة الخريطة")]
        public string Mapimageurl { get; set; }
        [ExcelColumn(Name = "Expected Visitors - عدد الزوار المتوقع")]
        [ExcelColumnName("Expected Visitors - عدد الزوار المتوقع")]
        public int? Expectedvisitors { get; set; }
        [ExcelColumn(Name = "Max Capacity - السعة القصوى")]
        [ExcelColumnName("Max Capacity - السعة القصوى")]
        public int? Maxcapacity { get; set; }
        [ExcelColumn(Name = "Current Occupancy - العدد الحالي للحضور")]
        [ExcelColumnName("Current Occupancy - العدد الحالي للحضور")]
        public int? Currentoccupancy { get; set; }
        [ExcelColumn(Name = "Organizer - المنظم")]
        [ExcelColumnName("Organizer - المنظم")]
        public string Organizer { get; set; }
        [ExcelColumn(Name = "Contact Email - البريد الإلكتروني للتواصل")]
        [ExcelColumnName("Contact Email - البريد الإلكتروني للتواصل")]
        public string Contactemail { get; set; }
        [ExcelColumn(Name = "Contact Phone - هاتف التواصل")]
        [ExcelColumnName("Contact Phone - هاتف التواصل")]
        public string Contactphone { get; set; }
        [ExcelColumn(Name = "Website - الموقع الإلكتروني")]
        [ExcelColumnName("Website - الموقع الإلكتروني")]
        public string Website { get; set; }
        [ExcelColumn(Name = "Logo URL - رابط الشعار")]
        [ExcelColumnName("Logo URL - رابط الشعار")]
        public string Logourl { get; set; }
        [ExcelColumn(Name = "Banner Image URL - رابط صورة البانر")]
        [ExcelColumnName("Banner Image URL - رابط صورة البانر")]
        public string Bannerimageurl { get; set; }
        [ExcelColumn(Name = "Is Public Event - هل الحدث عام")]
        [ExcelColumnName("Is Public Event - هل الحدث عام")]
        public string Ispublic { get; set; }
        [ExcelColumn(Name = "Requires Registration - يتطلب التسجيل")]
        [ExcelColumnName("Requires Registration - يتطلب التسجيل")]
        public string Requiresregistration { get; set; }
        [ExcelColumn(Name = "Is Paid Event - حدث مدفوع")]
        [ExcelColumnName("Is Paid Event - حدث مدفوع")]
        public string Ispaidevent { get; set; }
        [ExcelColumn(Name = "Ticket Price - سعر التذكرة")]
        [ExcelColumnName("Ticket Price - سعر التذكرة")]
        public float Ticketprice { get; set; }
        [ExcelColumn(Name = "Currency - العملة")]
        [ExcelColumnName("Currency - العملة")]
        public string Currency { get; set; }
        [ExcelColumn(Name = "Event Manager ID - معرف مدير الفعالية")]
        [ExcelColumnName("Event Manager ID - معرف مدير الفعالية")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? Eventmanagerid { get; set; }
        [ExcelColumn(Name = "Tags - الوسوم")]
        [ExcelColumnName("Tags - الوسوم")]
        public string Tags { get; set; }
        [ExcelColumn(Name = "Is Active - نشط")]
        [ExcelColumnName("Is Active - نشط")]
        public string Isactive { get; set; }
        [ExcelColumn(Name = "Actualcost")]
        [ExcelColumnName("Actualcost")]
        public float Actualcost { get; set; }
        [ExcelColumn(Name = "Budget")]
        [ExcelColumnName("Budget")]
        public float Budget { get; set; }
        [ExcelColumn(Name = "Totalareasqm")]
        [ExcelColumnName("Totalareasqm")]
        public float Totalareasqm { get; set; }
        [ExcelColumn(Name = "Expectedattendance")]
        [ExcelColumnName("Expectedattendance")]
        public float Expectedattendance { get; set; }
        [ExcelColumn(Name = "Country")]
        [ExcelColumnName("Country")]
        public string Country { get; set; }
        [ExcelColumn(Name = "Venue")]
        [ExcelColumnName("Venue")]
        public string Venue { get; set; }
        [ExcelColumn(Name = "Creator")]
        [ExcelColumnName("Creator")]
        public string CreateBy { get; set; }
        [ExcelColumn(Name = "Create time", Format = "yyyy-MM-dd HH:mm:ss", Width = 20)]
        [ExcelColumnName("Create time")]
        public DateTime? CreateTime { get; set; }
        [ExcelColumn(Name = "Updater")]
        [ExcelColumnName("Updater")]
        public string UpdateBy { get; set; }
        [ExcelColumn(Name = "Update time", Format = "yyyy-MM-dd HH:mm:ss", Width = 20)]
        [ExcelColumnName("Update time")]
        public DateTime? UpdateTime { get; set; }
        [ExcelColumn(Name = "Remark")]
        [ExcelColumnName("Remark")]
        public string Remark { get; set; }
        [ExcelColumn(Name = "Companyid")]
        [ExcelColumnName("Companyid")]
        public int? Companyid { get; set; }
        [ExcelColumn(Name = "BranchID")]
        [ExcelColumnName("BranchID")]
        public int? BranchID { get; set; }


        [ExcelColumn(Name = "Event Type - نوع الفعالية (conference, exhibition, festival, etc.)")]
        public string EventtypeLabel { get; set; }
        [ExcelColumn(Name = "Event Status - حالة الفعالية (planned, active, completed, cancelled)")]
        public string StatusLabel { get; set; }
        [ExcelColumn(Name = "Location ID - معرف الموقع")]
        public string LocationidLabel { get; set; }
        [ExcelColumn(Name = "Organizer - المنظم")]
        public string OrganizerLabel { get; set; }
        [ExcelColumn(Name = "Event Manager ID - معرف مدير الفعالية")]
        public string EventmanageridLabel { get; set; }
    }
}
