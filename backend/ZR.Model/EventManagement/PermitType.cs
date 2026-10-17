
namespace ZR.Model.EventManagement
{
    /// <summary>
    /// Permit Type - أنواع التصاريح
    /// </summary>
    [SugarTable("event_permit_types")]
    
    [SqlDict(
        dictType: "permit_type",
        dictName: "Permit Type - أنواع التصاريح",
        valueColumn: "typecode",
        labelColumns: "typecode, ' - ', typename, ' - ', typenamear",
        RemarkColumn = "description",
        OrderBy = "displayorder ASC",
        Description = "Auto-generated from PermitType table"
    )]
    public class PermitType :SysBase
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
        /// Requires Approval - يتطلب موافقة 
        /// </summary>
       //  public string Requiresapproval { get; set; }
       public string  Requiresapproval { get; set; }

        /// <summary>
        /// Approval Level - مستوى الموافقة المطلوب 
        /// </summary>
       //  public string Approvallevel { get; set; }
       public string  Approvallevel { get; set; }

        /// <summary>
        /// Default Validity Days - عدد أيام الصلاحية الافتراضية 
        /// </summary>
       //  public int? Defaultvaliditydays { get; set; }
       public int  Defaultvaliditydays { get; set; }

        /// <summary>
        /// Requires Vehicle Info - يتطلب معلومات المركبة 
        /// </summary>
       //  public string Requiresvehicleinfo { get; set; }
       public string  Requiresvehicleinfo { get; set; }

        /// <summary>
        /// Requires Photo - يتطلب صورة 
        /// </summary>
       //  public string Requiresphoto { get; set; }
       public string  Requiresphoto { get; set; }

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
