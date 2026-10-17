
namespace ZR.Model.HR.Business.Dto
{
    /// <summary>
    /// Reviewtype Information Table查询对象
    /// </summary>
    public class HrReviewtypeQueryDto : PagerInfo 
    {
        public string Name { get; set; }
        public DateTime? BeginCreateTime { get; set; }
        public DateTime? EndCreateTime { get; set; }
        public DateTime? BeginUpdateTime { get; set; }
        public DateTime? EndUpdateTime { get; set; }
        public string Remark { get; set; }
    }

    /// <summary>
    /// Reviewtype Information Table输入输出对象
    /// </summary>
    public class HrReviewtypeDto
    {
        [ExcelColumn(Name = "Review Type ID")]
        [ExcelColumnName("Review Type ID")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? Reviewtypeid { get; set; }

        [Required(ErrorMessage = "Review  Type name 不能为空")]
        [ExcelColumn(Name = "Review  Type name ")]
        [ExcelColumnName("Review  Type name ")]
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

        [Required(ErrorMessage = "Remark不能为空")]
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
