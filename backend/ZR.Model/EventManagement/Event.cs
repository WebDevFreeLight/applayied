
namespace ZR.Model.EventManagement
{
    /// <summary>
    /// Event Information - معلومات الفعالية
    /// </summary>
    [SugarTable("event_events")]
    
    [SqlDict(
        dictType: "event",
        dictName: "Event - الفعاليات",
        valueColumn: "eventcode",
        labelColumns: "eventcode, ' - ', eventname, ' - ', eventnamear",
        RemarkColumn = "description",
        OrderBy = "eventcode ASC",
        Description = "Auto-generated from Event table"
    )]
    public class Event : SysBase
    {
        /// <summary>
        /// Event ID - معرف الفعالية 
        /// </summary>


        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
       public long  Eventid { get; set; }

        /// <summary>
        /// Event Code - رمز الفعالية 
        /// </summary>
       public string  Eventcode { get; set; }

        /// <summary>
        /// Event Name (English) - اسم الفعالية بالإنجليزية 
        /// </summary>
       public string  Eventname { get; set; }

        /// <summary>
        /// Event Name (Arabic) - اسم الفعالية بالعربية 
        /// </summary>
       public string  Eventnamear { get; set; }

        /// <summary>
        /// Description (English) - الوصف بالإنجليزية 
        /// </summary>
       public string  Description { get; set; }

        /// <summary>
        /// Description (Arabic) - الوصف بالعربية 
        /// </summary>
       public string  Descriptionar { get; set; }

        /// <summary>
        /// Event Type - نوع الفعالية (conference, exhibition, festival, etc.) 
        /// </summary>
       public string  Eventtype { get; set; }

        /// <summary>
        /// Event Status - حالة الفعالية (planned, active, completed, cancelled) 
        /// </summary>
       public string  Status { get; set; }

        /// <summary>
        /// Start Date - تاريخ البداية 
        /// </summary>
       public DateTime  Startdate { get; set; }

        /// <summary>
        /// End Date - تاريخ النهاية 
        /// </summary>
       public DateTime  Enddate { get; set; }

        /// <summary>
        /// Registration Start Date - تاريخ بداية التسجيل 
        /// </summary>
       public DateTime  Registrationstartdate { get; set; }

        /// <summary>
        /// Registration End Date - تاريخ نهاية التسجيل 
        /// </summary>
       public DateTime  Registrationenddate { get; set; }

        /// <summary>
        /// Location ID - معرف الموقع 
        /// </summary>
       public long  Locationid { get; set; }

        /// <summary>
        /// Location Name - اسم الموقع 
        /// </summary>
       public string  Locationname { get; set; }

        /// <summary>
        /// Address - العنوان 
        /// </summary>
       public string  Address { get; set; }

        /// <summary>
        /// City - المدينة 
        /// </summary>
       public string  City { get; set; }

        /// <summary>
        /// Latitude - خط العرض 
        /// </summary>
       public decimal  Latitude { get; set; }

        /// <summary>
        /// Longitude - خط الطول 
        /// </summary>
       public decimal  Longitude { get; set; }

        /// <summary>
        /// Map Image URL - رابط صورة الخريطة 
        /// </summary>
       public string  Mapimageurl { get; set; }

        /// <summary>
        /// Expected Visitors - عدد الزوار المتوقع 
        /// </summary>
       public int  Expectedvisitors { get; set; }

        /// <summary>
        /// Max Capacity - السعة القصوى 
        /// </summary>
       public int  Maxcapacity { get; set; }

        /// <summary>
        /// Current Occupancy - العدد الحالي للحضور 
        /// </summary>
       public int  Currentoccupancy { get; set; }

        /// <summary>
        /// Organizer - المنظم 
        /// </summary>
       public string  Organizer { get; set; }

        /// <summary>
        /// Contact Email - البريد الإلكتروني للتواصل 
        /// </summary>
       public string  Contactemail { get; set; }

        /// <summary>
        /// Contact Phone - هاتف التواصل 
        /// </summary>
       public string  Contactphone { get; set; }

        /// <summary>
        /// Website - الموقع الإلكتروني 
        /// </summary>
       public string  Website { get; set; }

        /// <summary>
        /// Logo URL - رابط الشعار 
        /// </summary>
       public string  Logourl { get; set; }

        /// <summary>
        /// Banner Image URL - رابط صورة البانر 
        /// </summary>
       public string  Bannerimageurl { get; set; }

        /// <summary>
        /// Is Public Event - هل الحدث عام 
        /// </summary>
       public string  Ispublic { get; set; }

        /// <summary>
        /// Requires Registration - يتطلب التسجيل 
        /// </summary>
       public string  Requiresregistration { get; set; }

        /// <summary>
        /// Is Paid Event - حدث مدفوع 
        /// </summary>
       public string  Ispaidevent { get; set; }

        /// <summary>
        /// Ticket Price - سعر التذكرة 
        /// </summary>
       public decimal  Ticketprice { get; set; }

        /// <summary>
        /// Currency - العملة 
        /// </summary>
       public string  Currency { get; set; }

        /// <summary>
        /// Event Manager ID - معرف مدير الفعالية 
        /// </summary>
       public long  Eventmanagerid { get; set; }

        /// <summary>
        /// Tags - الوسوم 
        /// </summary>
       public string  Tags { get; set; }

        /// <summary>
        /// Is Active - نشط 
        /// </summary>
       public string  Isactive { get; set; }

        /// <summary>
        /// Actualcost 
        /// </summary>
       public decimal  Actualcost { get; set; }

        /// <summary>
        /// Budget 
        /// </summary>
       public decimal  Budget { get; set; }

        /// <summary>
        /// Totalareasqm 
        /// </summary>
       public decimal  Totalareasqm { get; set; }

        /// <summary>
        /// Expectedattendance 
        /// </summary>
       public decimal  Expectedattendance { get; set; }

        /// <summary>
        /// Country 
        /// </summary>
       public string  Country { get; set; }

        /// <summary>
        /// Venue 
        /// </summary>
       public string  Venue { get; set; } 

    }
}
