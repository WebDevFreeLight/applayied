
namespace ZR.Model.BusinessPartners.Dto
{
    /// <summary>
    /// businesspartner查询对象
    /// </summary>
    public class BpBusinesspartnerQueryDto : PagerInfo 
    {
        public long? Type { get; set; }
        public long? Status { get; set; }
        public long? Kind { get; set; }
        public long? Assigneddepartment { get; set; }
        public long? Assignedto { get; set; }
        public string Contactfirstname { get; set; }
        public string Contactlastname { get; set; }
        public string Businessphone { get; set; }
        public string Contactphone { get; set; }
        public string Businessemail { get; set; }
        public string Contactemail { get; set; }
        public string Mobile { get; set; }
        public string Buildingnumber { get; set; }
        public string Streetaddress { get; set; }
        public string Unitnumber { get; set; }
        public string City { get; set; }
        public string Province { get; set; }
        public string Postalcode { get; set; }
        public string Country { get; set; }
        public string Compliancestatus { get; set; }
        public long? Preferredcontactmethod { get; set; }
    }

    /// <summary>
    /// businesspartner输入输出对象
    /// </summary>
    public class BpBusinesspartnerDto_Res
    {
        [ExcelColumn(Name = "Partnerid")]
        [ExcelColumnName("Partnerid")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? Partnerid { get; set; }

        [Required(ErrorMessage = "Type不能为空")]
        [ExcelColumn(Name = "Type")]
        [ExcelColumnName("Type")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long Type { get; set; }

        [Required(ErrorMessage = "Status不能为空")]
        [ExcelColumn(Name = "Status")]
        [ExcelColumnName("Status")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long Status { get; set; }

        [Required(ErrorMessage = "Kind不能为空")]
        [ExcelColumn(Name = "Kind")]
        [ExcelColumnName("Kind")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long Kind { get; set; }

        [Required(ErrorMessage = "Assigneddepartment不能为空")]
        [ExcelColumn(Name = "Assigneddepartment")]
        [ExcelColumnName("Assigneddepartment")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long Assigneddepartment { get; set; }

        [Required(ErrorMessage = "Assignedto不能为空")]
        [ExcelColumn(Name = "Assignedto")]
        [ExcelColumnName("Assignedto")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long Assignedto { get; set; }

        [ExcelColumn(Name = "Entityid")]
        [ExcelColumnName("Entityid")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? Entityid { get; set; }

        [ExcelColumn(Name = "Legacyid")]
        [ExcelColumnName("Legacyid")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? Legacyid { get; set; }

        [ExcelColumn(Name = "Partnerstage")]
        [ExcelColumnName("Partnerstage")]
        public string Partnerstage { get; set; }

        [ExcelColumn(Name = "Commercialname")]
        [ExcelColumnName("Commercialname")]
        public string Commercialname { get; set; }

        [ExcelColumn(Name = "Legalname")]
        [ExcelColumnName("Legalname")]
        public string Legalname { get; set; }

        [ExcelColumn(Name = "Vatid")]
        [ExcelColumnName("Vatid")]
        public string Vatid { get; set; }

        [ExcelColumn(Name = "Crnumber")]
        [ExcelColumnName("Crnumber")]
        public string Crnumber { get; set; }

        [ExcelColumn(Name = "Industry")]
        [ExcelColumnName("Industry")]
        public string Industry { get; set; }

        [ExcelColumn(Name = "Subindustry")]
        [ExcelColumnName("Subindustry")]
        public string Subindustry { get; set; }

        [ExcelColumn(Name = "Contactfirstname")]
        [ExcelColumnName("Contactfirstname")]
        public string Contactfirstname { get; set; }

        [ExcelColumn(Name = "Contactlastname")]
        [ExcelColumnName("Contactlastname")]
        public string Contactlastname { get; set; }

        [ExcelColumn(Name = "Businessphone")]
        [ExcelColumnName("Businessphone")]
        public string Businessphone { get; set; }

        [ExcelColumn(Name = "Contactphone")]
        [ExcelColumnName("Contactphone")]
        public string Contactphone { get; set; }

        [ExcelColumn(Name = "Businessemail")]
        [ExcelColumnName("Businessemail")]
        public string Businessemail { get; set; }

        [ExcelColumn(Name = "Contactemail")]
        [ExcelColumnName("Contactemail")]
        public string Contactemail { get; set; }

        [ExcelColumn(Name = "Mobile")]
        [ExcelColumnName("Mobile")]
        public string Mobile { get; set; }

        [ExcelColumn(Name = "Buildingnumber")]
        [ExcelColumnName("Buildingnumber")]
        public string Buildingnumber { get; set; }

        [ExcelColumn(Name = "Streetaddress")]
        [ExcelColumnName("Streetaddress")]
        public string Streetaddress { get; set; }

        [ExcelColumn(Name = "Unitnumber")]
        [ExcelColumnName("Unitnumber")]
        public string Unitnumber { get; set; }

        [ExcelColumn(Name = "City")]
        [ExcelColumnName("City")]
        public string City { get; set; }

        [ExcelColumn(Name = "Province")]
        [ExcelColumnName("Province")]
        public string Province { get; set; }

        [ExcelColumn(Name = "Postalcode")]
        [ExcelColumnName("Postalcode")]
        public string Postalcode { get; set; }

        [ExcelColumn(Name = "Country")]
        [ExcelColumnName("Country")]
        public string Country { get; set; }

        [ExcelColumn(Name = "ExtraAttributes")]
        [ExcelColumnName("ExtraAttributes")]
        public string ExtraAttributes { get; set; }

        [Required(ErrorMessage = "Compliancestatus不能为空")]
        [ExcelColumn(Name = "Compliancestatus")]
        [ExcelColumnName("Compliancestatus")]
        public string Compliancestatus { get; set; }

        [Required(ErrorMessage = "Preferredcontactmethod不能为空")]
        [ExcelColumn(Name = "Preferredcontactmethod")]
        [ExcelColumnName("Preferredcontactmethod")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long Preferredcontactmethod { get; set; }

        [ExcelColumn(Name = "BudgetRange")]
        [ExcelColumnName("BudgetRange")]
        public string BudgetRange { get; set; }

        [ExcelColumn(Name = "Image Files")]
        public string ImageFiles { get; set; }


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
