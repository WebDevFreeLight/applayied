
namespace ZR.Model.EventManagement
{
    /// <summary>
    /// Visitor Type - أنواع الزوار
    /// </summary>
    [SugarTable("event_visitor_types")]
    [SqlDict(
        dictType: "visitor_type",
        dictName: "أنواع الزوار - Visitor Types",
        valueColumn: "typecode",
        labelColumns: "typecode, ' - ', typename, ' - ', typenamear",
        RemarkColumn = "remark",
        OrderBy = "typecode ASC",
        Description = "Generated from VisitorType table"
    )]
    public class VisitorType :SysBase
    {
        /// <summary>
        /// Type ID - معرف النوع 
        /// </summary>


        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
       public long  Typeid { get; set; }

        /// <summary>
        /// Type Code - رمز النوع 
        /// </summary>
       public string  Typecode { get; set; }

        /// <summary>
        /// Type Name (English) - اسم النوع بالإنجليزية 
        /// </summary>
       public string  Typename { get; set; }

        /// <summary>
        /// Type Name (Arabic) - اسم النوع بالعربية 
        /// </summary>
       public string  Typenamear { get; set; }

        /// <summary>
        /// Description - الوصف 
        /// </summary>
       public string  Description { get; set; }

        /// <summary>
        /// Icon - الأيقونة 
        /// </summary>
       public string  Icon { get; set; }

        /// <summary>
        /// Color Code - رمز اللون 
        /// </summary>
       public string  Colorcode { get; set; }

        /// <summary>
        /// Badge Color - لون البطاقة 
        /// </summary>
       //  public string Badgecolor { get; set; }
       public string  Badgecolor { get; set; }

        /// <summary>
        /// Has Priority Access - له أولوية في الدخول 
        /// </summary>
       public string  Haspriorityaccess { get; set; }

        /// <summary>
        /// Requires Approval - يتطلب موافقة 
        /// </summary>
       public string  Requiresapproval { get; set; }

        /// <summary>
        /// Display Order - ترتيب العرض 
        /// </summary>
       public int  Displayorder { get; set; }

        /// <summary>
        /// Is Active - نشط 
        /// </summary>
       public string  Isactive { get; set; }
         

    }
}
