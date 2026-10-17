
namespace ZR.Model.EventManagement
{
    /// <summary>
    /// Booth Type - أنواع الأكشاك
    /// </summary>
    [SugarTable("event_booth_types")]
    
    [SqlDict(
        dictType: "booth_type",
        dictName: "Booth Type - أنواع الأكشاك",
        valueColumn: "typecode",
        labelColumns: "typecode, ' - ', typename, ' - ', typenamear",
        RemarkColumn = "description",
        OrderBy = "displayorder ASC",
        Description = "Auto-generated from BoothType table"
    )]
    public class BoothType :SysBase
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
        /// Default Price Per SQM - السعر الافتراضي للمتر المربع 
        /// </summary>
       //  public float Defaultpricepersqm { get; set; }
       public float  Defaultpricepersqm { get; set; }

        /// <summary>
        /// Min Size SQM - الحد الأدنى للمساحة 
        /// </summary>
       //  public float Minsizesqm { get; set; }
       public float  Minsizesqm { get; set; }

        /// <summary>
        /// Max Size SQM - الحد الأقصى للمساحة 
        /// </summary>
       //  public float Maxsizesqm { get; set; }
       public float  Maxsizesqm { get; set; }

        /// <summary>
        /// Includes Electricity - يشمل كهرباء 
        /// </summary>
       //  public string Includeselectricity { get; set; }
       public string  Includeselectricity { get; set; }

        /// <summary>
        /// Includes Internet - يشمل إنترنت 
        /// </summary>
       //  public string Includesinternet { get; set; }
       public string  Includesinternet { get; set; }

        /// <summary>
        /// Includes Furniture - يشمل أثاث 
        /// </summary>
       //  public string Includesfurniture { get; set; }
       public string  Includesfurniture { get; set; }

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
         
    }
}
