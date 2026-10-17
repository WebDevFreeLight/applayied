namespace ZR.Model.Operations.Dto
{
    /// <summary>
    /// OpsGeoTemporalRule query
    /// </summary>
    public class OpsGeoTemporalRuleQueryDto : PagerInfo
    {
        public string RuleName { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? SeasonId { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? ZoneId { get; set; }
        public string RuleType { get; set; }
        public string Status { get; set; }
    }

    /// <summary>
    /// OpsGeoTemporalRule DTO
    /// </summary>
    public class OpsGeoTemporalRuleDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? RuleId { get; set; }
        public string RuleName { get; set; }
        public string RuleNameAr { get; set; }
        public string Description { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? SeasonId { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? ZoneId { get; set; }
        public string RuleType { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public int? RadiusMeters { get; set; }
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidUntil { get; set; }
        public string TimeSlots { get; set; }
        public string PermitTypeIds { get; set; }
        public string Status { get; set; }
        public string Color { get; set; }
        public string CreateBy { get; set; }
        public DateTime? CreateTime { get; set; }
        public string UpdateBy { get; set; }
        public DateTime? UpdateTime { get; set; }
        public string Remark { get; set; }
        // Navigation
        public string SeasonName { get; set; }
        public string ZoneName { get; set; }
    }
}
