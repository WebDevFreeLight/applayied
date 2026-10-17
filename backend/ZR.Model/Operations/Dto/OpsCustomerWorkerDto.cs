namespace ZR.Model.Operations.Dto
{
    /// <summary>
    /// OpsCustomerWorker query
    /// </summary>
    public class OpsCustomerWorkerQueryDto : PagerInfo
    {
        public string CustomerId { get; set; }
        public string FullName { get; set; }
        public string NationalId { get; set; }
        public string DocumentType { get; set; }
        public string Phone { get; set; }
        public string Nationality { get; set; }
        public bool? IsActive { get; set; }
    }

    /// <summary>
    /// OpsCustomerWorker DTO
    /// </summary>
    public class OpsCustomerWorkerDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? WorkerId { get; set; }
        public string CustomerId { get; set; }
        public string WorkerCode { get; set; }
        public string FullName { get; set; }
        public string FullNameAr { get; set; }
        public string NationalId { get; set; }
        public string PassportNumber { get; set; }
        public string DocumentType { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Nationality { get; set; }
        public string JobTitle { get; set; }
        public string PhotoUrl { get; set; }
        public string IqamaImageUrl { get; set; }
        public bool IsActive { get; set; }
        public string Notes { get; set; }
        public string CreateBy { get; set; }
        public DateTime? CreateTime { get; set; }
        public string UpdateBy { get; set; }
        public DateTime? UpdateTime { get; set; }
        public string Remark { get; set; }
        public string CustomerName { get; set; }
    }
}
