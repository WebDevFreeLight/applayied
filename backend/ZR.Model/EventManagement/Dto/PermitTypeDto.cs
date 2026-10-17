
namespace ZR.Model.EventManagement.Dto
{
    /// <summary>
    /// Permit Type - أنواع التصاريح查询对象
    /// </summary>
    public class PermitTypeQueryDto : PagerInfo 
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
        public string Requiresapproval { get; set; }
            [JsonConverter(typeof(ValueToStringConverter))]
        public string Approvallevel { get; set; }
            [JsonConverter(typeof(ValueToStringConverter))]
        public int? Defaultvaliditydays { get; set; }
            [JsonConverter(typeof(ValueToStringConverter))]
        public string Requiresvehicleinfo { get; set; }
            [JsonConverter(typeof(ValueToStringConverter))]
        public string Requiresphoto { get; set; }
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
    /// Permit Type - أنواع التصاريح输入输出对象
    /// </summary>
    public class PermitTypeDto
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
        [ExcelColumn(Name = "Requires Approval - يتطلب موافقة")]
        [ExcelColumnName("Requires Approval - يتطلب موافقة")]
        public string Requiresapproval { get; set; }
        [ExcelColumn(Name = "Approval Level - مستوى الموافقة المطلوب")]
        [ExcelColumnName("Approval Level - مستوى الموافقة المطلوب")]
        public string Approvallevel { get; set; }
        [ExcelColumn(Name = "Default Validity Days - عدد أيام الصلاحية الافتراضية")]
        [ExcelColumnName("Default Validity Days - عدد أيام الصلاحية الافتراضية")]
        public int? Defaultvaliditydays { get; set; }
        [ExcelColumn(Name = "Requires Vehicle Info - يتطلب معلومات المركبة")]
        [ExcelColumnName("Requires Vehicle Info - يتطلب معلومات المركبة")]
        public string Requiresvehicleinfo { get; set; }
        [ExcelColumn(Name = "Requires Photo - يتطلب صورة")]
        [ExcelColumnName("Requires Photo - يتطلب صورة")]
        public string Requiresphoto { get; set; }
        [ExcelColumn(Name = "Display Order - ترتيب العرض")]
        [ExcelColumnName("Display Order - ترتيب العرض")]
        public int? Displayorder { get; set; }
        [ExcelColumn(Name = "Is Active - نشط")]
        [ExcelColumnName("Is Active - نشط")]
        public string Isactive { get; set; }
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


        [ExcelColumn(Name = "Requires Approval - يتطلب موافقة")]
        public string RequiresapprovalLabel { get; set; }
    }
}
