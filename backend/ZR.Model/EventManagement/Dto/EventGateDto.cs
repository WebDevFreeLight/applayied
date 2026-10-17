
namespace ZR.Model.EventManagement.Dto
{
    /// <summary>
    /// Event Gate - بوابات الدخول查询对象
    /// </summary>
    public class EventGateQueryDto : PagerInfo 
    {
            [JsonConverter(typeof(ValueToStringConverter))]
        public long? Eventid { get; set; }
            [JsonConverter(typeof(ValueToStringConverter))]
        public long? Zoneid { get; set; }
            [JsonConverter(typeof(ValueToStringConverter))]
        public string Gatecode { get; set; }
            [JsonConverter(typeof(ValueToStringConverter))]
        public string Gatename { get; set; }
            [JsonConverter(typeof(ValueToStringConverter))]
        public string Gatenamear { get; set; }
            [JsonConverter(typeof(ValueToStringConverter))]
        public string Description { get; set; }
            [JsonConverter(typeof(ValueToStringConverter))]
        public string Gatetype { get; set; }
            [JsonConverter(typeof(ValueToStringConverter))]
        public string Location { get; set; }
            [JsonConverter(typeof(ValueToStringConverter))]
        public decimal? Latitude { get; set; }
            [JsonConverter(typeof(ValueToStringConverter))]
        public decimal? Longitude { get; set; }
            [JsonConverter(typeof(ValueToStringConverter))]
        public int? Maxcapacityperhour { get; set; }
            [JsonConverter(typeof(ValueToStringConverter))]
        public string Status { get; set; }
        public DateTime? BeginOpeningtime { get; set; }
        public DateTime? EndOpeningtime { get; set; }
        public DateTime? BeginClosingtime { get; set; }
        public DateTime? EndClosingtime { get; set; }
            [JsonConverter(typeof(ValueToStringConverter))]
        public string Hasscanner { get; set; }
            [JsonConverter(typeof(ValueToStringConverter))]
        public string Scannerdeviceid { get; set; }
            [JsonConverter(typeof(ValueToStringConverter))]
        public long? Gatemanagerid { get; set; }
            [JsonConverter(typeof(ValueToStringConverter))]
        public int? Displayorder { get; set; }
            [JsonConverter(typeof(ValueToStringConverter))]
        public string Isactive { get; set; }
        public DateTime? BeginLaststatusupdate { get; set; }
        public DateTime? EndLaststatusupdate { get; set; }
        public DateTime? BeginCreateTime { get; set; }
        public DateTime? EndCreateTime { get; set; }
        public DateTime? BeginUpdateTime { get; set; }
        public DateTime? EndUpdateTime { get; set; }
    }

    /// <summary>
    /// Event Gate - بوابات الدخول输入输出对象
    /// </summary>
    public class EventGateDto
    {

        
        [ExcelColumn(Name = "Gate ID - معرف البوابة")]
        [ExcelColumnName("Gate ID - معرف البوابة")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? Gateid { get; set; }
        [Required(ErrorMessage = "Event ID - معرف الفعالية不能为空")]
        [ExcelColumn(Name = "Event ID - معرف الفعالية")]
        [ExcelColumnName("Event ID - معرف الفعالية")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long Eventid { get; set; }
        [Required(ErrorMessage = "Zone ID - معرف المنطقة (اختياري)不能为空")]
        [ExcelColumn(Name = "Zone ID - معرف المنطقة (اختياري)")]
        [ExcelColumnName("Zone ID - معرف المنطقة (اختياري)")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long Zoneid { get; set; }
        [Required(ErrorMessage = "Gate Code - رمز البوابة不能为空")]
        [ExcelColumn(Name = "Gate Code - رمز البوابة")]
        [ExcelColumnName("Gate Code - رمز البوابة")]
        public string Gatecode { get; set; }
        [Required(ErrorMessage = "Gate Name (English) - اسم البوابة بالإنجليزية不能为空")]
        [ExcelColumn(Name = "Gate Name (English) - اسم البوابة بالإنجليزية")]
        [ExcelColumnName("Gate Name (English) - اسم البوابة بالإنجليزية")]
        public string Gatename { get; set; }
        [ExcelColumn(Name = "Gate Name (Arabic) - اسم البوابة بالعربية")]
        [ExcelColumnName("Gate Name (Arabic) - اسم البوابة بالعربية")]
        public string Gatenamear { get; set; }
        [ExcelColumn(Name = "Description - الوصف")]
        [ExcelColumnName("Description - الوصف")]
        public string Description { get; set; }
        [ExcelColumn(Name = "Gate Type - نوع البوابة (entry, exit, both, vip, staff)")]
        [ExcelColumnName("Gate Type - نوع البوابة (entry, exit, both, vip, staff)")]
        public string Gatetype { get; set; }
        [ExcelColumn(Name = "Location - الموقع")]
        [ExcelColumnName("Location - الموقع")]
        public string Location { get; set; }
        [ExcelColumn(Name = "Latitude - خط العرض")]
        [ExcelColumnName("Latitude - خط العرض")]
        public decimal Latitude { get; set; }
        [ExcelColumn(Name = "Longitude - خط الطول")]
        [ExcelColumnName("Longitude - خط الطول")]
        public decimal Longitude { get; set; }
        [ExcelColumn(Name = "Max Capacity Per Hour - السعة القصوى في الساعة")]
        [ExcelColumnName("Max Capacity Per Hour - السعة القصوى في الساعة")]
        public int? Maxcapacityperhour { get; set; }
        [ExcelColumn(Name = "Status - الحالة (open, closed, maintenance, full)")]
        [ExcelColumnName("Status - الحالة (open, closed, maintenance, full)")]
        public string Status { get; set; }
        [ExcelColumn(Name = "Opening Time - وقت الفتح")]
        [ExcelColumnName("Opening Time - وقت الفتح")]
        public string Openingtime { get; set; }
        [ExcelColumn(Name = "Closing Time - وقت الإغلاق")]
        [ExcelColumnName("Closing Time - وقت الإغلاق")]
        public string Closingtime { get; set; }
        [ExcelColumn(Name = "Has Scanner - يحتوي على ماسح ضوئي")]
        [ExcelColumnName("Has Scanner - يحتوي على ماسح ضوئي")]
        public string Hasscanner { get; set; }
        [ExcelColumn(Name = "Scanner Device ID - معرف جهاز المسح")]
        [ExcelColumnName("Scanner Device ID - معرف جهاز المسح")]
        public string Scannerdeviceid { get; set; }
        [ExcelColumn(Name = "Gate Manager ID - معرف مسؤول البوابة")]
        [ExcelColumnName("Gate Manager ID - معرف مسؤول البوابة")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? Gatemanagerid { get; set; }
        [ExcelColumn(Name = "Display Order - ترتيب العرض")]
        [ExcelColumnName("Display Order - ترتيب العرض")]
        public int? Displayorder { get; set; }
        [ExcelColumn(Name = "Is Active - نشط")]
        [ExcelColumnName("Is Active - نشط")]
        public string Isactive { get; set; }
        [ExcelColumn(Name = "Last Status Update - آخر تحديث للحالة", Format = "yyyy-MM-dd HH:mm:ss", Width = 20)]
        [ExcelColumnName("Last Status Update - آخر تحديث للحالة")]
        public DateTime? Laststatusupdate { get; set; }
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


        [ExcelColumn(Name = "Event ID - معرف الفعالية")]
        public string EventidLabel { get; set; }
        [ExcelColumn(Name = "Zone ID - معرف المنطقة (اختياري)")]
        public string ZoneidLabel { get; set; }
        [ExcelColumn(Name = "Gate Type - نوع البوابة (entry, exit, both, vip, staff)")]
        public string GatetypeLabel { get; set; }
        [ExcelColumn(Name = "Location - الموقع")]
        public string LocationLabel { get; set; }
        [ExcelColumn(Name = "Status - الحالة (open, closed, maintenance, full)")]
        public string StatusLabel { get; set; }
        [ExcelColumn(Name = "Has Scanner - يحتوي على ماسح ضوئي")]
        public string HasscannerLabel { get; set; }
        [ExcelColumn(Name = "Gate Manager ID - معرف مسؤول البوابة")]
        public string GatemanageridLabel { get; set; }


















    }
     

    /// <summary>
    /// Update Gate Status Request DTO
    /// </summary>
    public class UpdateGateStatusDto
    {
        [Required(ErrorMessage = "Gate ID is required")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long GateId { get; set; }

        [Required(ErrorMessage = "Status is required")]
        public string Status { get; set; }
    }

    /// <summary>
    /// Update Queue Count Request DTO
    /// </summary>
    public class UpdateQueueCountDto
    {
        [Required(ErrorMessage = "Gate ID is required")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long GateId { get; set; }

        [Required(ErrorMessage = "Queue count is required")]
        public int QueueCount { get; set; }
    }

}
