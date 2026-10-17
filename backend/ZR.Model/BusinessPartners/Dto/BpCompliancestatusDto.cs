
namespace ZR.Model.BusinessPartners.Dto
{
    /// <summary>
    /// compliancestatus查询对象
    /// </summary>
    public class BpCompliancestatusQueryDto : PagerInfo 
    {
        public string Name { get; set; }
        public string Remark { get; set; }
    }

    /// <summary>
    /// compliancestatus输入输出对象
    /// </summary>
    public class BpCompliancestatusDto
    {
        [ExcelColumn(Name = "ComplianceStatus  ID")]
        [ExcelColumnName("ComplianceStatus  ID")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? Id { get; set; }

        [Required(ErrorMessage = "Type name不能为空")]
        [ExcelColumn(Name = "Type name")]
        [ExcelColumnName("Type name")]
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
