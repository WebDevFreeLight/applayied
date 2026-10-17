
namespace ZR.Model.Boulevard.Dto
{
    /// <summary>
    /// boulevard_seasons查询对象
    /// </summary>
    public class BlvBoulevardSeasonsQueryDto : PagerInfo 
    {
            [JsonConverter(typeof(ValueToStringConverter))]
        public string Name { get; set; }
        public DateTime? BeginStartDate { get; set; }
        public DateTime? EndStartDate { get; set; }
        public DateTime? BeginEndDate { get; set; }
        public DateTime? EndEndDate { get; set; }
        public DateTime? BeginCreateTime { get; set; }
        public DateTime? EndCreateTime { get; set; }
        public DateTime? BeginUpdateTime { get; set; }
        public DateTime? EndUpdateTime { get; set; }
            [JsonConverter(typeof(ValueToStringConverter))]
        public string Remark { get; set; }
    }

    /// <summary>
    /// boulevard_seasons输入输出对象
    /// </summary>
    public class BlvBoulevardSeasonsDto
    {
        [ExcelColumn(Name = "id")]
        [ExcelColumnName("id")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? Id { get; set; }
        [ExcelColumn(Name = "Name")]
        [ExcelColumnName("Name")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Start Date不能为空")]
        [ExcelColumn(Name = "Start Date", Format = "yyyy-MM-dd HH:mm:ss", Width = 20)]
        [ExcelColumnName("Start Date")]
        public DateTime? StartDate { get; set; }
        [Required(ErrorMessage = "End Date不能为空")]
        [ExcelColumn(Name = "End Date", Format = "yyyy-MM-dd HH:mm:ss", Width = 20)]
        [ExcelColumnName("End Date")]
        public DateTime? EndDate { get; set; }
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
        [ExcelColumn(Name = "Image File")]
        [ExcelColumnName("Image File")]
        public string ImageFile { get; set; }
        [ExcelColumn(Name = "Companyid")]
        [ExcelColumnName("Companyid")]
        public int? Companyid { get; set; }
        [ExcelColumn(Name = "BranchID")]
        [ExcelColumnName("BranchID")]
        public int? BranchID { get; set; }


    }
}
