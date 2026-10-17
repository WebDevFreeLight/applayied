
namespace ZR.Model.Business.Dto
{
    /// <summary>
    /// Organizations Information Table查询对象
    /// </summary>
    public class OrganizationtypeQueryDto : PagerInfo 
    {
        [JsonConverter(typeof( ValueToStringConverter))]
        public long? Organizationtypeid { get; set; }
        public string Organizationtypename { get; set; }
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
    public class OrganizationtypeDto
    {
        [ExcelIgnore]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? Organizationtypeid { get; set; }

        [Required(ErrorMessage = "OrganizationType name不能为空")]
        [ExcelColumn(Name = "OrganizationType name")]
        [ExcelColumnName("OrganizationType name")]
        public string Organizationtypename { get; set; }

        [ExcelIgnore]
        public string CreateBy { get; set; }

        [ExcelIgnore]
        public DateTime? CreateTime { get; set; }

        [ExcelIgnore]
        public string UpdateBy { get; set; }

        [ExcelIgnore]
        public DateTime? UpdateTime { get; set; }

        [ExcelColumn(Name = "Remark")]
        [ExcelColumnName("Remark")]
        public string Remark { get; set; }

        [ExcelIgnore]
        public int? Companyid { get; set; }

        [ExcelIgnore]
        public int? BranchID { get; set; }



    }
}

