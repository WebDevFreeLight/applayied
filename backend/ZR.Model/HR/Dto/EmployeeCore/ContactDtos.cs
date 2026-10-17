using System;

namespace ZR.Model.HR.Dto.EmployeeCore
{
    /// <summary>
    /// DTO for creating a new contact
    /// </summary>
    public class CreateContactDto
    {
        public long ContactTypeDictCode { get; set; }
        public string ContactValue { get; set; }
        public bool IsPrimary { get; set; }
        public bool? IsVerified { get; set; }
        public DateTime? EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
    }

    /// <summary>
    /// DTO for updating an existing contact
    /// </summary>
    public class UpdateContactDto
    {
        public long? ContactTypeDictCode { get; set; }
        public string ContactValue { get; set; }
        public bool? IsPrimary { get; set; }
        public bool? IsVerified { get; set; }
        public DateTime? EffectiveTo { get; set; }
    }

    /// <summary>
    /// DTO for batch updating contacts (work email, personal email, phone, mobile)
    /// </summary>
    public class UpdateContactsBatchDto
    {
        public string Email { get; set; }
        public string PersonalEmail { get; set; }
        public string Phone { get; set; }
        public string MobileNumber { get; set; }
    }
}
