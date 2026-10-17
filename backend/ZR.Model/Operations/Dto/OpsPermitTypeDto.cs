namespace ZR.Model.Operations.Dto
{
    /// <summary>
    /// OpsPermitType query
    /// </summary>
    public class OpsPermitTypeQueryDto : PagerInfo
    {
        public string TypeCode { get; set; }
        public string TypeName { get; set; }
        public bool? IsActive { get; set; }
        public bool? RequiresApproval { get; set; }
    }

    /// <summary>
    /// OpsPermitType DTO
    /// </summary>
    public class OpsPermitTypeDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? PermitTypeId { get; set; }
        public string TypeCode { get; set; }
        public string TypeName { get; set; }
        public string TypeNameAr { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
        public string ColorCode { get; set; }
        public bool RequiresApproval { get; set; }
        public int ApprovalLevel { get; set; }
        public int? DefaultValidityDays { get; set; }
        public bool RequiresVehicleInfo { get; set; }
        public bool RequiresPhoto { get; set; }
        public int? MaxQuantity { get; set; }
        public decimal? Price { get; set; }
        public int? DisplayOrder { get; set; }
        public bool IsActive { get; set; }
        public string CreateBy { get; set; }
        public DateTime? CreateTime { get; set; }
        public string UpdateBy { get; set; }
        public DateTime? UpdateTime { get; set; }
        public string Remark { get; set; }
    }
}
