
namespace ZR.Model.EventManagement
{
    /// <summary>
    /// Incident Type - أنواع الحوادث
    /// </summary>
    [SugarTable("event_incident_types")]
    
    [SqlDict(
        dictType: "incident_type",
        dictName: "Incident Type - أنواع الحوادث",
        valueColumn: "typecode",
        labelColumns: "typecode, ' - ', typename, ' - ', typenamear",
        RemarkColumn = "description",
        OrderBy = "displayorder ASC",
        Description = "Auto-generated from IncidentType table"
    )]
    public class IncidentType :SysBase
    {
        /// <summary>
        /// Type ID - معرف النوع 
        /// </summary>


        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
       //  public long? Typeid { get; set; }
       public long  Typeid { get; set; }

        /// <summary>
        /// Type Code - رمز النوع 
        /// </summary>
       //  public string Typecode { get; set; }
       public string  Typecode { get; set; }

        /// <summary>
        /// Type Name (English) - اسم النوع بالإنجليزية 
        /// </summary>
       //  public string Typename { get; set; }
       public string  Typename { get; set; }

        /// <summary>
        /// Type Name (Arabic) - اسم النوع بالعربية 
        /// </summary>
       //  public string Typenamear { get; set; }
       public string  Typenamear { get; set; }

        /// <summary>
        /// Description - الوصف 
        /// </summary>
       //  public string Description { get; set; }
       public string  Description { get; set; }

        /// <summary>
        /// Icon - الأيقونة 
        /// </summary>
       //  public string Icon { get; set; }
       public string  Icon { get; set; }

        /// <summary>
        /// Color Code - رمز اللون 
        /// </summary>
       //  public string Colorcode { get; set; }
       public string  Colorcode { get; set; }

        /// <summary>
        /// Default Priority - الأولوية الافتراضية 
        /// </summary>
       //  public string Defaultpriority { get; set; }
       public string  Defaultpriority { get; set; }

        /// <summary>
        /// Default Severity - الخطورة الافتراضية 
        /// </summary>
       //  public string Defaultseverity { get; set; }
       public string  Defaultseverity { get; set; }

        /// <summary>
        /// Assigned Team - الفريق المسؤول 
        /// </summary>
       //  public string Assignedteam { get; set; }
       public string  Assignedteam { get; set; }

        /// <summary>
        /// SLA Response Minutes - وقت الاستجابة المتوقع بالدقائق 
        /// </summary>
       //  public int? Slaresponseminutes { get; set; }
       public int  Slaresponseminutes { get; set; }

        /// <summary>
        /// SLA Resolution Minutes - وقت الحل المتوقع بالدقائق 
        /// </summary>
       //  public int? Slaresolutionminutes { get; set; }
       public int  Slaresolutionminutes { get; set; }

        /// <summary>
        /// Requires Immediate Action - يتطلب إجراء فوري 
        /// </summary>
       //  public string Requiresimmediateaction { get; set; }
       public string  Requiresimmediateaction { get; set; }

        /// <summary>
        /// Display Order - ترتيب العرض 
        /// </summary>
       //  public int? Displayorder { get; set; }
       public int  Displayorder { get; set; }

        /// <summary>
        /// Is Active - نشط 
        /// </summary>
       //  public string Isactive { get; set; }
       public string  Isactive { get; set; }

        /// <summary>
        /// Severity خطورة 
        /// </summary>
       //  public string Severity { get; set; }
       public string  Severity { get; set; }


    }
}
