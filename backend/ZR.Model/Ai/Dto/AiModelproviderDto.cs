
namespace ZR.Model.Ai.Dto
{
    /// <summary>
    /// Model Provider Table查询对象
    /// </summary>
    public class AiModelproviderQueryDto : PagerInfo 
    {
            [JsonConverter(typeof(ValueToStringConverter))]
        public long? Id { get; set; }
            [JsonConverter(typeof(ValueToStringConverter))]
        public string Name { get; set; }
        public DateTime? BeginCreateTime { get; set; }
        public DateTime? EndCreateTime { get; set; }
        public DateTime? BeginUpdateTime { get; set; }
        public DateTime? EndUpdateTime { get; set; }
            [JsonConverter(typeof(ValueToStringConverter))]
        public string Remark { get; set; }
    }

    /// <summary>
    /// Model Provider Table输入输出对象
    /// </summary>
    public class AiModelproviderDto
    {
        [ExcelColumn(Name = "Id")]
        [ExcelColumnName("Id")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? Id { get; set; }
        [Required(ErrorMessage = "name不能为空")]
        [ExcelColumn(Name = "name")]
        [ExcelColumnName("name")]
        public string Name { get; set; }
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
