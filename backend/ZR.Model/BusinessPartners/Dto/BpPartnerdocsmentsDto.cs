
namespace ZR.Model.BusinessPartners.Dto
{
    /// <summary>
    /// Bp_PartnerDocsments查询对象
    /// </summary>
    public class BpPartnerdocsmentsQueryDto : PagerInfo 
    {
            [JsonConverter(typeof(ValueToStringConverter))]
        public long? Doctype { get; set; }
            [JsonConverter(typeof(ValueToStringConverter))]
        public long? Partner { get; set; }
        public DateTime? BeginCreateTime { get; set; }
        public DateTime? EndCreateTime { get; set; }
        public DateTime? BeginUpdateTime { get; set; }
        public DateTime? EndUpdateTime { get; set; }
            [JsonConverter(typeof(ValueToStringConverter))]
        public string Remark { get; set; }
    }

    /// <summary>
    /// Bp_PartnerDocsments输入输出对象
    /// </summary>
    public class BpPartnerdocsmentsDto
    {
        [ExcelColumn(Name = "ID")]
        [ExcelColumnName("ID")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? Id { get; set; }
        [Required(ErrorMessage = "document type不能为空")]
        [ExcelColumn(Name = "document type")]
        [ExcelColumnName("document type")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long Doctype { get; set; }
        [Required(ErrorMessage = "Partner不能为空")]
        [ExcelColumn(Name = "Partner")]
        [ExcelColumnName("Partner")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long Partner { get; set; }
        [Required(ErrorMessage = "FileUrl不能为空")]
        [ExcelColumn(Name = "FileUrl")]
        [ExcelColumnName("FileUrl")]
        public string FileUrl { get; set; }
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


        [ExcelColumn(Name = "document type")]
        public string DoctypeLabel { get; set; }
        [ExcelColumn(Name = "Partner")]
        public string PartnerLabel { get; set; }
    }
}
