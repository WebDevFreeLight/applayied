
namespace ZR.Model.EventManagement
{
    /// <summary>
    /// Event Status - أنواع حالة الفعاليات
    /// </summary>
    [SugarTable("event_status")]
    
    [SqlDict(
        dictType: "status",
        dictName: "Event Status - حالة الفعاليات",
        valueColumn: "typecode",
        labelColumns: "typecode, ' - ', typename, ' - ', typenamear",
        RemarkColumn = "description",
        OrderBy = "displayorder ASC",
        Description = "Auto-generated from EventStatus table"
    )]
    public class EventStatus :SysBase
    {
        /// <summary>
        /// Status ID - معرف النوع 
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
       //  public long? Typeid { get; set; }
       public long  Typeid { get; set; }

        /// <summary>
        /// Status Code - رمز حالة 
        /// </summary>
       //  public string Typecode { get; set; }
       public string  Typecode { get; set; }

        /// <summary>
        /// Status Name (English) - اسم حالة بالإنجليزية 
        /// </summary>
       public string  Typename { get; set; }

        /// <summary>
        /// Status Name (Arabic) - اسم الحالة بالعربية 
        /// </summary>
       public string  Typenamear { get; set; }

        /// <summary>
        /// Description - الوصف 
        /// </summary>
       public string  Description { get; set; }

        /// <summary>
        /// Icon - الأيقونة 
        /// </summary>
       //  public string Icon { get; set; }
       public string  Icon { get; set; }

        /// <summary>
        /// Color Code - رمز اللون 
        /// </summary>
       public string  Colorcode { get; set; }

        /// <summary>
        /// Display Order - ترتيب العرض 
        /// </summary>
       //  public int? Displayorder { get; set; }
       public int  Displayorder { get; set; }

        /// <summary>
        /// Is Active - نشط 
        /// </summary>
       public string  Isactive { get; set; }

    }
}
