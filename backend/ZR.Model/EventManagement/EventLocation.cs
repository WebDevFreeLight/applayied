
namespace ZR.Model.EventManagement
{
    /// <summary>
    /// Event Location Information - معلومات موقع الفعالية
    /// </summary>
    [SugarTable("event_locations")]
    
    [SqlDict(
        dictType: "location",
        dictName: "Event Location - المواقع",
        valueColumn: "locationid",
        labelColumns: "locationname",
        RemarkColumn = "remark",
        OrderBy = "locationid ASC",
        Description = "Auto-generated from EventLocation table"
    )]
    public class EventLocation : SysBase
    {
        /// <summary>
        /// Location ID - معرف الموقع 
        /// </summary>

        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        public long Locationid { get; set; }
        /// <summary>
        /// Location Name - اسم الموقع 
        /// </summary>
        public string Locationname { get; set; }

        /// <summary>
        /// Venue - المكان/القاعة 
        /// </summary>
        public string Venue { get; set; }

        /// <summary>
        /// Address - العنوان 
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// City - المدينة 
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// Country - الدولة 
        /// </summary>
        public string Country { get; set; }

        /// <summary>
        /// Latitude - خط العرض 
        /// </summary>
        public float Latitude { get; set; }

        /// <summary>
        /// Longitude - خط الطول 
        /// </summary>
        public float Longitude { get; set; }

        /// <summary>
        /// Map Image URL - رابط صورة الخريطة 
        /// </summary>
        public string Mapimageurl { get; set; }

        /// <summary>
        /// Max Capacity - السعة القصوى للموقع 
        /// </summary>
        public int Maxcapacity { get; set; }

        /// <summary>
        /// Is Active - نشط 
        /// </summary>
        public string Isactive { get; set; }


    }
}
