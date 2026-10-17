
namespace ZR.Model.EventManagement
{
    /// <summary>
    /// Ticket Type - أنواع التذاكر
    /// </summary>
    [SugarTable("event_ticket_types")]
    
    [SqlDict(
        dictType: "ticket_type",
        dictName: "Ticket Type - أنواع التذاكر",
        valueColumn: "typecode",
        labelColumns: "typecode, ' - ', typename, ' - ', typenamear",
        RemarkColumn = "description",
        OrderBy = "displayorder ASC",
        Description = "Auto-generated from TicketType table"
    )]
    public class TicketType :SysBase
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
        /// Default Price - السعر الافتراضي 
        /// </summary>
       //  public float Defaultprice { get; set; }
       public float  Defaultprice { get; set; }

        /// <summary>
        /// Currency - العملة 
        /// </summary>
       //  public string Currency { get; set; }
       public string  Currency { get; set; }

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
        /// Is Transferable - قابل للتحويل 
        /// </summary>
       //  public string Istransferable { get; set; }
       public string  Istransferable { get; set; }

        /// <summary>
        /// Is Refundable - قابل للاسترجاع 
        /// </summary>
       //  public string Isrefundable { get; set; }
       public string  Isrefundable { get; set; }

        /// <summary>
        /// Max Quantity Per Person - الحد الأقصى للشخص 
        /// </summary>
       //  public int? Maxquantityperperson { get; set; }
       public int  Maxquantityperperson { get; set; }

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
