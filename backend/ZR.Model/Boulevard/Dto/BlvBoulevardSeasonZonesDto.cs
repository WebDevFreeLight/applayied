
namespace ZR.Model.Boulevard.Dto
{
    /// <summary>
    /// boulevard_season_zones查询对象
    /// </summary>
    public class BlvBoulevardSeasonZonesQueryDto : PagerInfo 
    {
            [JsonConverter(typeof(ValueToStringConverter))]
        public string Name { get; set; }
        public DateTime? BeginCreateTime { get; set; }
        public DateTime? EndCreateTime { get; set; }
        public DateTime? BeginUpdateTime { get; set; }
        public DateTime? EndUpdateTime { get; set; }
            [JsonConverter(typeof(ValueToStringConverter))]
        public string Remark { get; set; }
            [JsonConverter(typeof(ValueToStringConverter))]
        public long? SeasonId { get; set; }
    }

    /// <summary>
    /// boulevard_season_zones输入输出对象
    /// </summary>
    public class BlvBoulevardSeasonZonesDto
    {
        [ExcelColumn(Name = "id")]
        [ExcelColumnName("id")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? Id { get; set; }
        [Required(ErrorMessage = "Name不能为空")]
        [ExcelColumn(Name = "Name")]
        [ExcelColumnName("Name")]
        public string Name { get; set; }
        [ExcelColumn(Name = "ImageFile")]
        [ExcelColumnName("ImageFile")]
        public string ImageFile { get; set; }
        [Required(ErrorMessage = "SeasonId is required")]
        [ExcelColumn(Name = "Season Id")]
        [ExcelColumnName("Season Id")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long SeasonId { get; set; }
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


    }
}
