using System;

namespace ZR.Model.HR.Dto.EmployeeCore
{
    /// <summary>
    /// DTO for creating a new emergency contact
    /// </summary>
    public class CreateEmergencyContactDto
    {
        public string ContactName { get; set; }
        public long? RelationshipDictCode { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public bool IsPrimary { get; set; }
    }

    /// <summary>
    /// DTO for updating an existing emergency contact
    /// </summary>
    public class UpdateEmergencyContactDto
    {
        public string ContactName { get; set; }
        public long? RelationshipDictCode { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public bool? IsPrimary { get; set; }
    }

    /// <summary>
    /// DTO for simple primary emergency contact update
    /// </summary>
    public class UpdatePrimaryEmergencyContactDto
    {
        public string EmergencyContactName { get; set; }
        public string EmergencyContactPhone { get; set; }
    }
}
