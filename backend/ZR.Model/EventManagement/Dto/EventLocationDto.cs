
namespace ZR.Model.EventManagement.Dto
{
    /// <summary>
    /// Event Location Information - معلومات موقع الفعالية查询对象
    /// </summary>
    public class EventLocationQueryDto : PagerInfo 
    {
            [JsonConverter(typeof(ValueToStringConverter))]
        public long? Locationid { get; set; }
            [JsonConverter(typeof(ValueToStringConverter))]
        public string Locationname { get; set; }
            [JsonConverter(typeof(ValueToStringConverter))]
        public string Venue { get; set; }
            [JsonConverter(typeof(ValueToStringConverter))]
        public string Address { get; set; }
            [JsonConverter(typeof(ValueToStringConverter))]
        public string City { get; set; }
            [JsonConverter(typeof(ValueToStringConverter))]
        public string Country { get; set; }
            [JsonConverter(typeof(ValueToStringConverter))]
        public float? Latitude { get; set; }
            [JsonConverter(typeof(ValueToStringConverter))]
        public float? Longitude { get; set; }
            [JsonConverter(typeof(ValueToStringConverter))]
        public int? Maxcapacity { get; set; }
            [JsonConverter(typeof(ValueToStringConverter))]
        public string Isactive { get; set; }
        public DateTime? BeginCreateTime { get; set; }
        public DateTime? EndCreateTime { get; set; }
        public DateTime? BeginUpdateTime { get; set; }
        public DateTime? EndUpdateTime { get; set; }
            [JsonConverter(typeof(ValueToStringConverter))]
        public string Remark { get; set; }
    }

    /// <summary>
    /// Event Location Information - معلومات موقع الفعالية输入输出对象
    /// </summary>
    public class EventLocationDto
    {
        [ExcelColumn(Name = "Location ID - معرف الموقع")]
        [ExcelColumnName("Location ID - معرف الموقع")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? Locationid { get; set; }
        [Required(ErrorMessage = "Location Name - اسم الموقع不能为空")]
        [ExcelColumn(Name = "Location Name - اسم الموقع")]
        [ExcelColumnName("Location Name - اسم الموقع")]
        public string Locationname { get; set; }
        [ExcelColumn(Name = "Venue - المكان/القاعة")]
        [ExcelColumnName("Venue - المكان/القاعة")]
        public string Venue { get; set; }
        [Required(ErrorMessage = "Address - العنوان不能为空")]
        [ExcelColumn(Name = "Address - العنوان")]
        [ExcelColumnName("Address - العنوان")]
        public string Address { get; set; }
        [ExcelColumn(Name = "City - المدينة")]
        [ExcelColumnName("City - المدينة")]
        public string City { get; set; }
        [ExcelColumn(Name = "Country - الدولة")]
        [ExcelColumnName("Country - الدولة")]
        public string Country { get; set; }
        [ExcelColumn(Name = "Latitude - خط العرض")]
        [ExcelColumnName("Latitude - خط العرض")]
        public float Latitude { get; set; }
        [ExcelColumn(Name = "Longitude - خط الطول")]
        [ExcelColumnName("Longitude - خط الطول")]
        public float Longitude { get; set; }
        [ExcelColumn(Name = "Map Image URL - رابط صورة الخريطة")]
        [ExcelColumnName("Map Image URL - رابط صورة الخريطة")]
        public string Mapimageurl { get; set; }
        [ExcelColumn(Name = "Max Capacity - السعة القصوى للموقع")]
        [ExcelColumnName("Max Capacity - السعة القصوى للموقع")]
        public int? Maxcapacity { get; set; }
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


        [ExcelColumn(Name = "Is Active - نشط")]
        public string IsactiveLabel { get; set; }
    }
}
