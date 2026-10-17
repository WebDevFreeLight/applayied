namespace ZR.Model.Operations.Dto
{
    /// <summary>
    /// OpsPermitVerification query
    /// </summary>
    public class OpsPermitVerificationQueryDto : PagerInfo
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? PermitId { get; set; }
        public string CustomerId { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? WorkerId { get; set; }
        public string ApplicantName { get; set; }
        public string IdNumber { get; set; }
        public string DocumentType { get; set; }
        public string Status { get; set; }
    }

    /// <summary>
    /// OpsPermitVerification DTO
    /// </summary>
    public class OpsPermitVerificationDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? VerificationId { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? PermitId { get; set; }
        public string CustomerId { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? WorkerId { get; set; }
        public string ApplicantName { get; set; }
        public string IdNumber { get; set; }
        public string DocumentType { get; set; }
        public string DocumentUrl { get; set; }
        public string Status { get; set; }
        public string VerifiedBy { get; set; }
        public DateTime? VerifiedDate { get; set; }
        public string RejectionReason { get; set; }
        public string Notes { get; set; }
        public string CreateBy { get; set; }
        public DateTime? CreateTime { get; set; }
        public string UpdateBy { get; set; }
        public DateTime? UpdateTime { get; set; }
        public string Remark { get; set; }
        // Navigation
        public string PermitNumber { get; set; }
        public string CustomerName { get; set; }
        public string WorkerName { get; set; }
    }

    /// <summary>
    /// Verify document request
    /// </summary>
    public class OpsVerifyDocumentRequest
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long VerificationId { get; set; }
    }

    /// <summary>
    /// Reject document request
    /// </summary>
    public class OpsRejectDocumentRequest
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long VerificationId { get; set; }
        public string Reason { get; set; }
    }
}
