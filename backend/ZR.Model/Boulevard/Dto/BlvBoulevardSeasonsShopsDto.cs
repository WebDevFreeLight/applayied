
namespace ZR.Model.Boulevard.Dto
{
    /// <summary>
    /// boulevard_seasons_Shops查询对象
    /// </summary>
    public class BlvBoulevardSeasonsShopsQueryDto : PagerInfo 
    {
            [JsonConverter(typeof(ValueToStringConverter))]
        public long? SeasonId { get; set; }
            [JsonConverter(typeof(ValueToStringConverter))]
        public long? ShopId { get; set; }
    }

    /// <summary>
    /// boulevard_seasons_Shops输入输出对象
    /// </summary>
    public class BlvBoulevardSeasonsShopsDto
    {
        [ExcelColumn(Name = "id")]
        [ExcelColumnName("id")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? Id { get; set; }
        [Required(ErrorMessage = "SeasonId不能为空")]
        [ExcelColumn(Name = "SeasonId")]
        [ExcelColumnName("SeasonId")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long SeasonId { get; set; }
        [Required(ErrorMessage = "ShopId不能为空")]
        [ExcelColumn(Name = "ShopId")]
        [ExcelColumnName("ShopId")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ShopId { get; set; }
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


        [ExcelColumn(Name = "SeasonId")]
        public string SeasonIdLabel { get; set; }
        [ExcelColumn(Name = "ShopId")]
        public string ShopIdLabel { get; set; }
    }
}
