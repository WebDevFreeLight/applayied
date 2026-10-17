using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.Converter;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
namespace ZR.Model.EventManagement.Dto
{
    // ==================== Event Permit ====================
    public class EventPermitQueryDto : PagerInfo
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? EventId { get; set; }
        public string PermitNumber { get; set; }
        public string PermitType { get; set; }
        public string HolderName { get; set; }
        public string Status { get; set; }
        public DateTime? BeginIssueDate { get; set; }
        public DateTime? EndIssueDate { get; set; }
    }

    public class EventPermitDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long PermitId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        [Required(ErrorMessage = "Event ID is required")]
        public long EventId { get; set; }

        [Required(ErrorMessage = "Permit number is required")]
        public string PermitNumber { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]

        public string PermitType { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]


        public string PermitCategory { get; set; }

        [Required(ErrorMessage = "Applicant name is required")]
        public string ApplicantName { get; set; }

        public string ApplicantNameAr { get; set; }

        public string CompanyName { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]


        public string NationalId { get; set; }

        public string ContactPhone { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }



        [JsonConverter(typeof(ValueToStringConverter))]
        public string? EmployeeId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public string? VisitorId { get; set; }

        public DateTime IssueDate { get; set; }

        public DateTime ValidFrom { get; set; }

        public DateTime ValidUntil { get; set; }

        public string IssuedBy { get; set; }

        public string QRCode { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]


        public string ZoneAccess { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]


        public string GateAccess { get; set; }

        public string AccessLevel { get; set; }

        public string Purpose { get; set; }

        public string VehiclePlateNumber { get; set; }

        public string VehicleType { get; set; }

        public string PhotoUrl { get; set; }

        public string AttachmentUrl { get; set; }

        public string Status { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long ApprovedBy { get; set; }

        public DateTime? ApprovalDate { get; set; }

        public DateTime ApprovedDate { get; set; }

        public string RejectionReason { get; set; }

        public string Notes { get; set; }

        //public bool IsActive { get; set; }

        public string HolderName { get; set; }
    }

    public class ApproveEventPermitRequest
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        [Required(ErrorMessage = "Permit ID is required")]
        public long PermitId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        [Required(ErrorMessage = "Approved by is required")]
        public long ApprovedBy { get; set; }
    }

    public class RejectEventPermitRequest
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        [Required(ErrorMessage = "Permit ID is required")]
        public long PermitId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        [Required(ErrorMessage = "Rejected by is required")]
        public long RejectedBy { get; set; }

        [Required(ErrorMessage = "Rejection reason is required")]
        public string Reason { get; set; }
    }
}

