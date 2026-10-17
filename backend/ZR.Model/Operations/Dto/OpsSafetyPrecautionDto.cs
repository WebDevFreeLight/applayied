namespace ZR.Model.Operations.Dto
{
    /// <summary>
    /// OpsSafetyPrecaution query
    /// </summary>
    public class OpsSafetyPrecautionQueryDto : PagerInfo
    {
        public string Code { get; set; }
        public string Description { get; set; }
        public bool? IsActive { get; set; }
    }

    /// <summary>
    /// OpsSafetyPrecaution DTO
    /// </summary>
    public class OpsSafetyPrecautionDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? PrecautionId { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public string DescriptionAr { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
        public string CreateBy { get; set; }
        public DateTime? CreateTime { get; set; }
        public string UpdateBy { get; set; }
        public DateTime? UpdateTime { get; set; }
        public string Remark { get; set; }
    }
}
