
namespace ZR.Model.EventManagement
{
    /// <summary>
    /// Zone Type - أنواع المناطق
    /// </summary>
    [SugarTable("event_zone_types")]
    [SqlDict(
        dictType: "zone_type",
        dictName: "أنواع المناطق - Zone Types",
        valueColumn: "typecode",
        labelColumns: "typecode, ' - ', typename, ' - ', typenamear",
        RemarkColumn = "remark",
        OrderBy = "typecode ASC",
        Description = "Generated from EventZoneType table"
    )]
    public class EventZoneType : SysBase
    {
        /// <summary>
        /// Type ID - معرف النوع 
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        //  public long? Typeid { get; set; }
        public long Typeid { get; set; }

        /// <summary>
        /// Type Code - رمز النوع 
        /// </summary>
        //  public string Typecode { get; set; }
        public string Typecode { get; set; }

        /// <summary>
        /// Type Name (English) - اسم النوع بالإنجليزية 
        /// </summary>
        //  public string Typename { get; set; }
        public string Typename { get; set; }

        /// <summary>
        /// Type Name (Arabic) - اسم النوع بالعربية 
        /// </summary>
        //  public string Typenamear { get; set; }
        public string Typenamear { get; set; }

        /// <summary>
        /// Description - الوصف 
        /// </summary>
        //  public string Description { get; set; }
        public string Description { get; set; }

        /// <summary>
        /// Icon - الأيقونة 
        /// </summary>
        //  public string Icon { get; set; }
        public string Icon { get; set; }

        /// <summary>
        /// Color Code - رمز اللون 
        /// </summary>
        //  public string Colorcode { get; set; }
        public string Colorcode { get; set; }

        /// <summary>
        /// Requires Special Access - يتطلب صلاحية خاصة 
        /// </summary>
        //  public string Requiresspecialaccess { get; set; }
        public string Requiresspecialaccess { get; set; }

        /// <summary>
        /// Display Order - ترتيب العرض 
        /// </summary>
        //  public int? Displayorder { get; set; }
        public int Displayorder { get; set; }

        /// <summary>
        /// Is Active - نشط 
        /// </summary>
        //  public string Isactive { get; set; }
        public string Isactive { get; set; }


    }
}
