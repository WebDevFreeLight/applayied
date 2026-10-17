namespace ZR.Model.Operations.Dto
{
    /// <summary>
    /// OpsWorkCategory query
    /// </summary>
    public class OpsWorkCategoryQueryDto : PagerInfo
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public bool? IsActive { get; set; }
    }

    /// <summary>
    /// OpsWorkCategory DTO
    /// </summary>
    public class OpsWorkCategoryDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? WorkCategoryId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string NameAr { get; set; }
        public int? DisplayOrder { get; set; }
        public bool IsActive { get; set; }
        public string CreateBy { get; set; }
        public DateTime? CreateTime { get; set; }
        public string UpdateBy { get; set; }
        public DateTime? UpdateTime { get; set; }
        public string Remark { get; set; }
    }
}
