
namespace ZR.Model.Business.Dto
{
    /// <summary>
    /// Organizations Information Table查询对象
    /// </summary>
    public class OrganizationsQueryDto : PagerInfo 
    {

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? Organizationsid { get; set; }
        public string Name { get; set; }
        public string Organizationtypeid { get; set; }
        public int? DisplayOrder { get; set; }


        [JsonConverter(typeof(ValueToStringConverter))]
        public long ? Parentid { get; set; }
        public string CreateBy { get; set; }
        public DateTime? BeginCreateTime { get; set; }
        public DateTime? EndCreateTime { get; set; }
        public string UpdateBy { get; set; }
        public DateTime? BeginUpdateTime { get; set; }
        public DateTime? EndUpdateTime { get; set; }
        public string Remark { get; set; }
    }

    /// <summary>
    /// Organizations Information Table输入输出对象
    /// </summary>
    public class OrganizationsDto
    {
        [ExcelColumn(Name = "Organization ID")]
        [ExcelColumnName("Organization ID")]

        [JsonConverter(typeof(ValueToStringConverter))]
        public long Organizationsid { get; set; }

        [Required(ErrorMessage = "Organization name不能为空")]
        [ExcelColumn(Name = "Organization name")]
        [ExcelColumnName("Organization name")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Organizationtypeid不能为空")]
        [ExcelColumn(Name = "Organizationtypeid")]
        [ExcelColumnName("Organizationtypeid")]
        public string Organizationtypeid { get; set; }

        [ExcelColumn(Name = "logo_url")]
        [ExcelColumnName("logo_url")]
        public string LogoUrl { get; set; }

        [ExcelColumn(Name = "display_order")]
        [ExcelColumnName("display_order")]
        public int? DisplayOrder { get; set; }

        [ExcelColumn(Name = "Parentid")]
        [ExcelColumnName("Parentid")]

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? Parentid { get; set; }

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

        [ExcelColumn(Name = "BranchID Branch Name")]
        [ExcelColumnName("BranchID Branch Name")]
        public int? BranchID { get; set; }



        [ExcelColumn(Name = "Organizationtypeid")]
        public string OrganizationtypeidLabel { get; set; }
    }
}
