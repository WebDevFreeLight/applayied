
namespace ZR.Model.EventManagement
{
    /// <summary>
    /// Event Gate - بوابات الدخول
    /// </summary>
    [SugarTable("event_gates")]
    
    [SqlDict(
        dictType: "gate",
        dictName: "Event Gate - البوابات",
        valueColumn: "gatecode",
        labelColumns: "gatecode, ' - ', gatename, ' - ', gatenamear",
        RemarkColumn = "description",
        OrderBy = "displayorder ASC",
        Description = "Auto-generated from EventGate table"
    )]


    public class EventGate :SysBase
    {
        /// <summary>
        /// Gate ID - معرف البوابة 
        /// </summary>

        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
       public long  Gateid { get; set; }

        /// <summary>
        /// Event ID - معرف الفعالية 
        /// </summary>
       public long  Eventid { get; set; }

        /// <summary>
        /// Zone ID - معرف المنطقة (اختياري) 
        /// </summary>
       public long  Zoneid { get; set; }

        /// <summary>
        /// Gate Code - رمز البوابة 
        /// </summary>
       public string  Gatecode { get; set; }

        /// <summary>
        /// Gate Name (English) - اسم البوابة بالإنجليزية 
        /// </summary>
       public string  Gatename { get; set; }

        /// <summary>
        /// Gate Name (Arabic) - اسم البوابة بالعربية 
        /// </summary>
       public string  Gatenamear { get; set; }

        /// <summary>
        /// Description - الوصف 
        /// </summary>
       public string  Description { get; set; }

        /// <summary>
        /// Gate Type - نوع البوابة (entry, exit, both, vip, staff) 
        /// </summary>
       public string  Gatetype { get; set; }

        /// <summary>
        /// Location - الموقع 
        /// </summary>
       //  public string Location { get; set; }
       public string  Location { get; set; }

        /// <summary>
        /// Latitude - خط العرض 
        /// </summary>
       public decimal  Latitude { get; set; }

        /// <summary>
        /// Longitude - خط الطول 
        /// </summary>
       public decimal  Longitude { get; set; }

        /// <summary>
        /// Max Capacity Per Hour - السعة القصوى في الساعة 
        /// </summary>
       public int  Maxcapacityperhour { get; set; }

        /// <summary>
        /// Status - الحالة (open, closed, maintenance, full) 
        /// </summary>
       public string  Status { get; set; }

        /// <summary>
        /// Opening Time - وقت الفتح 
        /// </summary>
       public DateTime Openingtime { get; set; }

        /// <summary>
        /// Closing Time - وقت الإغلاق 
        /// </summary>
       public DateTime Closingtime { get; set; }

        /// <summary>
        /// Has Scanner - يحتوي على ماسح ضوئي 
        /// </summary>
       public string  Hasscanner { get; set; }

        /// <summary>
        /// Scanner Device ID - معرف جهاز المسح 
        /// </summary>
       public string  Scannerdeviceid { get; set; }

        /// <summary>
        /// Gate Manager ID - معرف مسؤول البوابة 
        /// </summary>
       public long  Gatemanagerid { get; set; }

        /// <summary>
        /// Display Order - ترتيب العرض 
        /// </summary>
       public int  Displayorder { get; set; }

        /// <summary>
        /// Is Active - نشط 
        /// </summary>
       public string  Isactive { get; set; }

        /// <summary>
        /// Last Status Update - آخر تحديث للحالة 
        /// </summary>
       public DateTime  Laststatusupdate { get; set; }


    }
}
