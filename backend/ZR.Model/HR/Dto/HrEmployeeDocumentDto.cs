
using ZR.Model.System;

namespace ZR.Model.HR.Dto
{
    /// <summary>
    /// Employee Documents Table查询对象
    /// </summary>
    public class HrEmployeeDocumentQueryDto : PagerInfo 
    {
        public long? Employeeid { get; set; }
        public long? Doctypeid { get; set; }
        public string Documentname { get; set; }
        public string Documentnumber { get; set; }
        public DateTime? BeginIssuedate { get; set; }
        public DateTime? EndIssuedate { get; set; }
        public DateTime? BeginExpirydate { get; set; }
        public DateTime? EndExpirydate { get; set; }
        public string Issuingauthority { get; set; }
        public string Filepath { get; set; }
        public long? Filesize { get; set; }
        public string Status { get; set; }
        public string Isverified { get; set; }
        public string Verifiedby { get; set; }
        public DateTime? BeginVerifieddate { get; set; }
        public DateTime? EndVerifieddate { get; set; }
    }

    /// <summary>
    /// Employee Documents Table输入输出对象
    /// </summary>
    public class HrEmployeeDocumentDto 
    {
        [ExcelColumn(Name = "Documentid")]
        [ExcelColumnName("Documentid")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? Documentid { get; set; }

        [Required(ErrorMessage = "Employeeid不能为空")]
        [ExcelColumn(Name = "Employeeid")]
        [ExcelColumnName("Employeeid")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long Employeeid { get; set; }

        [Required(ErrorMessage = "Doctypeid不能为空")]
        [ExcelColumn(Name = "Doctypeid")]
        [ExcelColumnName("Doctypeid")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long Doctypeid { get; set; }

        [Required(ErrorMessage = "Documentname不能为空")]
        [ExcelColumn(Name = "Documentname")]
        [ExcelColumnName("Documentname")]
        public string Documentname { get; set; }

        [ExcelColumn(Name = "Documentnumber")]
        [ExcelColumnName("Documentnumber")]
        public string Documentnumber { get; set; }

        [ExcelColumn(Name = "Issuedate", Format = "yyyy-MM-dd HH:mm:ss", Width = 20)]
        [ExcelColumnName("Issuedate")]
        public DateTime? Issuedate { get; set; }

        [ExcelColumn(Name = "Expirydate", Format = "yyyy-MM-dd HH:mm:ss", Width = 20)]
        [ExcelColumnName("Expirydate")]
        public DateTime? Expirydate { get; set; }

        [ExcelColumn(Name = "Issuingauthority")]
        [ExcelColumnName("Issuingauthority")]
        public string Issuingauthority { get; set; }

        [ExcelColumn(Name = "Filepath")]
        [ExcelColumnName("Filepath")]
        public string Filepath { get; set; }

        [ExcelColumn(Name = "Filesize")]
        [ExcelColumnName("Filesize")]
        public long? Filesize { get; set; }

        [ExcelColumn(Name = "Status")]
        [ExcelColumnName("Status")]
        public string Status { get; set; }

        [ExcelColumn(Name = "Isverified")]
        [ExcelColumnName("Isverified")]
        public string Isverified { get; set; }

        [ExcelColumn(Name = "Verifiedby")]
        [ExcelColumnName("Verifiedby")]
        public string Verifiedby { get; set; }

        [ExcelColumn(Name = "Verifieddate", Format = "yyyy-MM-dd HH:mm:ss", Width = 20)]
        [ExcelColumnName("Verifieddate")]
        public DateTime? Verifieddate { get; set; }

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
        public long? Companyid { get; set; }

        [ExcelColumn(Name = "BranchID Branch Name")]
        [ExcelColumnName("BranchID Branch Name")]
        public long? BranchID { get; set; }



        [ExcelColumn(Name = "Employeeid")]
        public string EmployeeidLabel { get; set; }
        [ExcelColumn(Name = "Doctypeid")]
        public string DoctypeidLabel { get; set; }
        [ExcelColumn(Name = "Issuingauthority")]
        public string IssuingauthorityLabel { get; set; }
    }
}
