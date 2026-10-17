namespace ZR.Model.Operations.Dto
{
    /// <summary>
    /// OpsPermit query
    /// </summary>
    public class OpsPermitQueryDto : PagerInfo
    {
        public string PermitNumber { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? SeasonId { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? ZoneId { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? ShopId { get; set; }
        public string CustomerId { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? WorkerId { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? PermitTypeId { get; set; }
        public string Status { get; set; }
        public string ApplicantName { get; set; }
        public string Direction { get; set; }
        public string WorkCondition { get; set; }
        public DateTime? BeginValidFrom { get; set; }
        public DateTime? EndValidFrom { get; set; }
        public DateTime? BeginValidUntil { get; set; }
        public DateTime? EndValidUntil { get; set; }
        public DateTime? BeginOpeningDate { get; set; }
        public DateTime? EndOpeningDate { get; set; }
    }

    /// <summary>
    /// OpsPermit DTO
    /// </summary>
    public class OpsPermitDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? PermitId { get; set; }
        public string PermitNumber { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long SeasonId { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? ZoneId { get; set; }
        public string CustomerId { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? WorkerId { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long PermitTypeId { get; set; }
        public string ApplicantName { get; set; }
        public string ApplicantNameAr { get; set; }
        public string NationalId { get; set; }
        public string ContactPhone { get; set; }
        public string Email { get; set; }
        public string CompanyName { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidUntil { get; set; }
        public DateTime IssueDate { get; set; }
        public string IssuedBy { get; set; }
        public string Purpose { get; set; }
        public string AccessLevel { get; set; }
        public string VehiclePlateNumber { get; set; }
        public string VehicleType { get; set; }
        public string PhotoUrl { get; set; }
        public string AttachmentUrl { get; set; }
        public string QRCode { get; set; }
        public string Status { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? ApprovedBy { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public string RejectionReason { get; set; }
        public string Notes { get; set; }
        // Sela fields
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? ShopId { get; set; }
        public string GateNumber { get; set; }
        public string Direction { get; set; }
        public DateTime? OpeningDate { get; set; }
        public DateTime? ClosingDate { get; set; }
        public string WorkTypeIds { get; set; }
        public string WorkCategoryIds { get; set; }
        public string WorkCondition { get; set; }
        public string WorkDescription { get; set; }
        // Audit
        public string CreateBy { get; set; }
        public DateTime? CreateTime { get; set; }
        public string UpdateBy { get; set; }
        public DateTime? UpdateTime { get; set; }
        public string Remark { get; set; }
        // Navigation
        public string SeasonName { get; set; }
        public string ZoneName { get; set; }
        public string CustomerName { get; set; }
        public string WorkerName { get; set; }
        public string PermitTypeName { get; set; }
        public string ShopName { get; set; }
        public List<string> WorkTypeNames { get; set; }
        public List<string> WorkCategoryNames { get; set; }
        public List<OpsPermitWorkerDto> Workers { get; set; }
        public List<OpsPermitSafetyCheckDto> SafetyChecks { get; set; }
    }

    /// <summary>
    /// Permit stats for dashboard
    /// </summary>
    public class OpsPermitStatsDto
    {
        public int Total { get; set; }
        public int Pending { get; set; }
        public int Approved { get; set; }
        public int Active { get; set; }
        public int Rejected { get; set; }
        public int Expired { get; set; }
        public int Cancelled { get; set; }
        public int TodayCount { get; set; }
    }

    /// <summary>
    /// Approve permit request
    /// </summary>
    public class OpsPermitApproveRequest
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long PermitId { get; set; }
    }

    /// <summary>
    /// Reject permit request
    /// </summary>
    public class OpsPermitRejectRequest
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long PermitId { get; set; }
        public string Reason { get; set; }
    }

    /// <summary>
    /// Monthly report item
    /// </summary>
    public class OpsPermitMonthlyReportDto
    {
        public int Month { get; set; }
        public string MonthName { get; set; }
        public int Total { get; set; }
        public int Approved { get; set; }
        public int Rejected { get; set; }
        public int Pending { get; set; }
    }

    /// <summary>
    /// Permit type distribution item
    /// </summary>
    public class OpsPermitDistributionDto
    {
        public string PermitTypeName { get; set; }
        public int Count { get; set; }
        public decimal Percentage { get; set; }
    }

    /// <summary>
    /// Permit worker DTO (for detail view)
    /// </summary>
    public class OpsPermitWorkerDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long Id { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long PermitId { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long WorkerId { get; set; }
        public string WorkerName { get; set; }
        public string WorkerNameAr { get; set; }
        public string NationalId { get; set; }
        public string Phone { get; set; }
        public string IqamaImageUrl { get; set; }
    }

    /// <summary>
    /// Permit safety check DTO (for detail view)
    /// </summary>
    public class OpsPermitSafetyCheckDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long Id { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long PermitId { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long PrecautionId { get; set; }
        public bool IsChecked { get; set; }
        public string Notes { get; set; }
        public string PrecautionDescription { get; set; }
        public string PrecautionDescriptionAr { get; set; }
        public string PrecautionCode { get; set; }
    }

    /// <summary>
    /// Safety check entry for create request
    /// </summary>
    public class OpsPermitSafetyCheckEntry
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long PrecautionId { get; set; }
        public bool IsChecked { get; set; }
        public string Notes { get; set; }
    }

    /// <summary>
    /// Full permit creation request (single API call)
    /// </summary>
    public class OpsPermitCreateRequest
    {
        // Permit fields
        [JsonConverter(typeof(ValueToStringConverter))]
        public long SeasonId { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? ZoneId { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? ShopId { get; set; }
        public string CustomerId { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long PermitTypeId { get; set; }
        public string GateNumber { get; set; }
        public string Direction { get; set; }
        public DateTime? OpeningDate { get; set; }
        public DateTime? ClosingDate { get; set; }
        public string WorkTypeIds { get; set; }
        public string WorkCategoryIds { get; set; }
        public string WorkCondition { get; set; }
        public string WorkDescription { get; set; }
        public string VehiclePlateNumber { get; set; }
        public string VehicleType { get; set; }
        public string Notes { get; set; }
        // Workers
        public List<long> WorkerIds { get; set; }
        // Safety checks
        public List<OpsPermitSafetyCheckEntry> SafetyChecks { get; set; }
    }
}
