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
    // ==================== Event Visitor ====================
    public class EventVisitorQueryDto : PagerInfo
    {
        public string VisitorCode { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FirstNameAr { get; set; }
        public string LastNameAr { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string NationalId { get; set; }
        public string PassportNumber { get; set; }
        public string IdentificationNumber { get; set; }
        public string IdentificationType { get; set; }
        public string Nationality { get; set; }
        public string NationalityCode { get; set; }
        public string Gender { get; set; }
        public string VisitorType { get; set; }
        public string VerificationStatus { get; set; }
    }

    public class EventVisitorDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long VisitorId { get; set; }

        public string VisitorCode { get; set; }

        [Required(ErrorMessage = "First name is required")]
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string FirstNameAr { get; set; }

        public string LastNameAr { get; set; }

        public string IdentificationType { get; set; }

        public string NationalId { get; set; }

        public string PassportNumber { get; set; }

        public string IdentificationNumber { get; set; }

        public string Nationality { get; set; }

        public string NationalityCode { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public string Gender { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        public string PhoneNumber { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }

        public string VerificationStatus { get; set; }

        public string PhotoUrl { get; set; }

        public string Notes { get; set; }

        public string VisitorType { get; set; }
    }

    /// <summary>
    /// Check-In Visitor Request DTO
    /// </summary>
    public class CheckInVisitorDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        [Required(ErrorMessage = "Visitor ID is required")]
        public long VisitorId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        [Required(ErrorMessage = "Gate ID is required")]
        public long GateId { get; set; }
    }

    /// <summary>
    /// Check-Out Visitor Request DTO
    /// </summary>
    public class CheckOutVisitorDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        [Required(ErrorMessage = "Visitor ID is required")]
        public long VisitorId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        [Required(ErrorMessage = "Gate ID is required")]
        public long GateId { get; set; }
    }

}

