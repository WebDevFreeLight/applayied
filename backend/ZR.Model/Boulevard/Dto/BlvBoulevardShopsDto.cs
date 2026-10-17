
namespace ZR.Model.Boulevard.Dto
{
    /// <summary>
    /// shops查询对象
    /// </summary>
    public class BlvBoulevardShopsQueryDto : PagerInfo 
    {
            [JsonConverter(typeof(ValueToStringConverter))]
        public string Name { get; set; }
            [JsonConverter(typeof(ValueToStringConverter))]
        public long? ZoneId { get; set; }
            [JsonConverter(typeof(ValueToStringConverter))]
        public long? ShopStatus { get; set; }
            [JsonConverter(typeof(ValueToStringConverter))]
        public long? ShopCategory { get; set; }
            [JsonConverter(typeof(ValueToStringConverter))]
        public float? ShopArea { get; set; }
            [JsonConverter(typeof(ValueToStringConverter))]
        public float? MeterPrice { get; set; }
            [JsonConverter(typeof(ValueToStringConverter))]
        public float? TotalPrice { get; set; }
        public int? UnitType { get; set; }
        public int? RentalType { get; set; }
        public DateTime? BeginCreateTime { get; set; }
        public DateTime? EndCreateTime { get; set; }
        public DateTime? BeginUpdateTime { get; set; }
        public DateTime? EndUpdateTime { get; set; }
            [JsonConverter(typeof(ValueToStringConverter))]
        public string Remark { get; set; }
    }

    /// <summary>
    /// shops输入输出对象
    /// </summary>
    public class BlvBoulevardShopsDto
    {
        [ExcelColumn(Name = "id")]
        [ExcelColumnName("id")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? Id { get; set; }
        [Required(ErrorMessage = "Name不能为空")]
        [ExcelColumn(Name = "Name")]
        [ExcelColumnName("Name")]
        public string Name { get; set; }
        [Required(ErrorMessage = "ZoneId不能为空")]
        [ExcelColumn(Name = "ZoneId")]
        [ExcelColumnName("ZoneId")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ZoneId { get; set; }
        [Required(ErrorMessage = "Shop Status不能为空")]
        [ExcelColumn(Name = "Shop Status")]
        [ExcelColumnName("Shop Status")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ShopStatus { get; set; }
        [Required(ErrorMessage = "Shop Category不能为空")]
        [ExcelColumn(Name = "Shop Category")]
        [ExcelColumnName("Shop Category")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ShopCategory { get; set; }
        [ExcelColumn(Name = "Shop Area")]
        [ExcelColumnName("Shop Area")]
        public float ShopArea { get; set; }
        [ExcelColumn(Name = "Meter Price")]
        [ExcelColumnName("Meter Price")]
        public float MeterPrice { get; set; }
        [ExcelColumn(Name = "Total Price")]
        [ExcelColumnName("Total Price")]
        public float TotalPrice { get; set; }
        [ExcelColumn(Name = "Image Files")]
        [ExcelColumnName("Image Files")]
        public string ImageFiles { get; set; }
        [ExcelColumn(Name = "Unit Type")]
        [ExcelColumnName("Unit Type")]
        public int UnitType { get; set; }
        [ExcelColumn(Name = "Rental Type")]
        [ExcelColumnName("Rental Type")]
        public int RentalType { get; set; }
        [ExcelColumn(Name = "Revenue Percentage")]
        [ExcelColumnName("Revenue Percentage")]
        public float? RevenuePercentage { get; set; }
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


        [ExcelColumn(Name = "ZoneId")]
        public string ZoneIdLabel { get; set; }
        [ExcelColumn(Name = "Shop Status")]
        public string ShopStatusLabel { get; set; }
        [ExcelColumn(Name = "Shop Category")]
        public string ShopCategoryLabel { get; set; }
    }
}
