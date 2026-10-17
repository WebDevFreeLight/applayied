
namespace ZR.Model.EventManagement.Dto
{
    /// <summary>
    /// Event Status - أنواع حالة الفعاليات查询对象
    /// </summary>
    public class EventStatusQueryDto : PagerInfo 
    {
            [JsonConverter(typeof(ValueToStringConverter))]
        public string Typecode { get; set; }
            [JsonConverter(typeof(ValueToStringConverter))]
        public string Typename { get; set; }
            [JsonConverter(typeof(ValueToStringConverter))]
        public string Typenamear { get; set; }
            [JsonConverter(typeof(ValueToStringConverter))]
        public string Description { get; set; }
            [JsonConverter(typeof(ValueToStringConverter))]
        public string Colorcode { get; set; }
            [JsonConverter(typeof(ValueToStringConverter))]
        public int? Displayorder { get; set; }
            [JsonConverter(typeof(ValueToStringConverter))]
        public string Isactive { get; set; }
        public DateTime? BeginCreateTime { get; set; }
        public DateTime? EndCreateTime { get; set; }
        public DateTime? BeginUpdateTime { get; set; }
        public DateTime? EndUpdateTime { get; set; }
    }

    /// <summary>
    /// Event Status - أنواع حالة الفعاليات输入输出对象
    /// </summary>
    public class EventStatusDto
    {
        [ExcelColumn(Name = "Status ID - معرف النوع")]
        [ExcelColumnName("Status ID - معرف النوع")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? Typeid { get; set; }
        [Required(ErrorMessage = "Status Code - رمز حالة不能为空")]
        [ExcelColumn(Name = "Status Code - رمز حالة")]
        [ExcelColumnName("Status Code - رمز حالة")]
        public string Typecode { get; set; }
        [Required(ErrorMessage = "Status Name (English) - اسم حالة بالإنجليزية不能为空")]
        [ExcelColumn(Name = "Status Name (English) - اسم حالة بالإنجليزية")]
        [ExcelColumnName("Status Name (English) - اسم حالة بالإنجليزية")]
        public string Typename { get; set; }
        [ExcelColumn(Name = "Status Name (Arabic) - اسم الحالة بالعربية")]
        [ExcelColumnName("Status Name (Arabic) - اسم الحالة بالعربية")]
        public string Typenamear { get; set; }
        [ExcelColumn(Name = "Description - الوصف")]
        [ExcelColumnName("Description - الوصف")]
        public string Description { get; set; }
        [ExcelColumn(Name = "Icon - الأيقونة")]
        [ExcelColumnName("Icon - الأيقونة")]
        public string Icon { get; set; }
        [ExcelColumn(Name = "Color Code - رمز اللون")]
        [ExcelColumnName("Color Code - رمز اللون")]
        public string Colorcode { get; set; }
        [ExcelColumn(Name = "Display Order - ترتيب العرض")]
        [ExcelColumnName("Display Order - ترتيب العرض")]
        public int? Displayorder { get; set; }
        [ExcelColumn(Name = "Is Active - نشط")]
        [ExcelColumnName("Is Active - نشط")]
        public string Isactive { get; set; }
        [ExcelColumn(Name = "BranchID")]
        [ExcelColumnName("BranchID")]
        public int? BranchID { get; set; }
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


        [ExcelColumn(Name = "Is Active - نشط")]
        public string IsactiveLabel { get; set; }
    }
}
