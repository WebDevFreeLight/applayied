
namespace ZR.Model.EventManagement.Dto
{
    /// <summary>
    /// Incident Type - أنواع الحوادث查询对象
    /// </summary>
    public class IncidentTypeQueryDto : PagerInfo 
    {
            [JsonConverter(typeof(ValueToStringConverter))]
        public long? Typeid { get; set; }
            [JsonConverter(typeof(ValueToStringConverter))]
        public string Typecode { get; set; }
            [JsonConverter(typeof(ValueToStringConverter))]
        public string Typename { get; set; }
            [JsonConverter(typeof(ValueToStringConverter))]
        public string Typenamear { get; set; }
            [JsonConverter(typeof(ValueToStringConverter))]
        public string Description { get; set; }
            [JsonConverter(typeof(ValueToStringConverter))]
        public string Defaultpriority { get; set; }
            [JsonConverter(typeof(ValueToStringConverter))]
        public string Defaultseverity { get; set; }
            [JsonConverter(typeof(ValueToStringConverter))]
        public string Assignedteam { get; set; }
            [JsonConverter(typeof(ValueToStringConverter))]
        public int? Slaresponseminutes { get; set; }
            [JsonConverter(typeof(ValueToStringConverter))]
        public int? Slaresolutionminutes { get; set; }
            [JsonConverter(typeof(ValueToStringConverter))]
        public string Requiresimmediateaction { get; set; }
            [JsonConverter(typeof(ValueToStringConverter))]
        public int? Displayorder { get; set; }
            [JsonConverter(typeof(ValueToStringConverter))]
        public string Isactive { get; set; }
            [JsonConverter(typeof(ValueToStringConverter))]
        public string Severity { get; set; }
        public DateTime? BeginCreateTime { get; set; }
        public DateTime? EndCreateTime { get; set; }
        public DateTime? BeginUpdateTime { get; set; }
        public DateTime? EndUpdateTime { get; set; }
    }

    /// <summary>
    /// Incident Type - أنواع الحوادث输入输出对象
    /// </summary>
    public class IncidentTypeDto
    {
        [ExcelColumn(Name = "Type ID - معرف النوع")]
        [ExcelColumnName("Type ID - معرف النوع")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? Typeid { get; set; }
        [Required(ErrorMessage = "Type Code - رمز النوع不能为空")]
        [ExcelColumn(Name = "Type Code - رمز النوع")]
        [ExcelColumnName("Type Code - رمز النوع")]
        public string Typecode { get; set; }
        [Required(ErrorMessage = "Type Name (English) - اسم النوع بالإنجليزية不能为空")]
        [ExcelColumn(Name = "Type Name (English) - اسم النوع بالإنجليزية")]
        [ExcelColumnName("Type Name (English) - اسم النوع بالإنجليزية")]
        public string Typename { get; set; }
        [ExcelColumn(Name = "Type Name (Arabic) - اسم النوع بالعربية")]
        [ExcelColumnName("Type Name (Arabic) - اسم النوع بالعربية")]
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
        [Required(ErrorMessage = "Default Priority - الأولوية الافتراضية不能为空")]
        [ExcelColumn(Name = "Default Priority - الأولوية الافتراضية")]
        [ExcelColumnName("Default Priority - الأولوية الافتراضية")]
        public string Defaultpriority { get; set; }
        [Required(ErrorMessage = "Default Severity - الخطورة الافتراضية不能为空")]
        [ExcelColumn(Name = "Default Severity - الخطورة الافتراضية")]
        [ExcelColumnName("Default Severity - الخطورة الافتراضية")]
        public string Defaultseverity { get; set; }
        [ExcelColumn(Name = "Assigned Team - الفريق المسؤول")]
        [ExcelColumnName("Assigned Team - الفريق المسؤول")]
        public string Assignedteam { get; set; }
        [ExcelColumn(Name = "SLA Response Minutes - وقت الاستجابة المتوقع بالدقائق")]
        [ExcelColumnName("SLA Response Minutes - وقت الاستجابة المتوقع بالدقائق")]
        public int? Slaresponseminutes { get; set; }
        [ExcelColumn(Name = "SLA Resolution Minutes - وقت الحل المتوقع بالدقائق")]
        [ExcelColumnName("SLA Resolution Minutes - وقت الحل المتوقع بالدقائق")]
        public int? Slaresolutionminutes { get; set; }
        [ExcelColumn(Name = "Requires Immediate Action - يتطلب إجراء فوري")]
        [ExcelColumnName("Requires Immediate Action - يتطلب إجراء فوري")]
        public string Requiresimmediateaction { get; set; }
        [ExcelColumn(Name = "Display Order - ترتيب العرض")]
        [ExcelColumnName("Display Order - ترتيب العرض")]
        public int? Displayorder { get; set; }
        [ExcelColumn(Name = "Is Active - نشط")]
        [ExcelColumnName("Is Active - نشط")]
        public string Isactive { get; set; }
        [ExcelColumn(Name = "Severity خطورة")]
        [ExcelColumnName("Severity خطورة")]
        public string Severity { get; set; }
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
        [ExcelColumn(Name = "BranchID")]
        [ExcelColumnName("BranchID")]
        public int? BranchID { get; set; }


        [ExcelColumn(Name = "Requires Immediate Action - يتطلب إجراء فوري")]
        public string RequiresimmediateactionLabel { get; set; }
    }
}
