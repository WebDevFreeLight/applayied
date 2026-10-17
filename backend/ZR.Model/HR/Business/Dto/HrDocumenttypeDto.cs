
using ZR.Model.System;

namespace ZR.Model.HR.Business.Dto
{
    /// <summary>
    /// DocumentType查询对象
    /// </summary>
    public class HrDocumenttypeQueryDto : PagerInfo 
    {
        public string Documenttypename { get; set; }
        public DateTime? BeginCreateTime { get; set; }
        public DateTime? EndCreateTime { get; set; }
        public DateTime? BeginUpdateTime { get; set; }
        public DateTime? EndUpdateTime { get; set; }
    }

    /// <summary>
    /// DocumentType输入输出对象
    /// </summary>
    public class HrDocumenttypeDto : SysBase
    {
        [ExcelColumn(Name = "Organization ID")]
        [ExcelColumnName("Organization ID")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? Documenttypeid { get; set; }

        [Required(ErrorMessage = "DocumentType name不能为空")]
        [ExcelColumn(Name = "DocumentType name")]
        [ExcelColumnName("DocumentType name")]
        public string Documenttypename { get; set; }
         
    }
}
