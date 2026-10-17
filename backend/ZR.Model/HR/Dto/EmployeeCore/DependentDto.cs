using Infrastructure.Converter;
using Newtonsoft.Json;
using System;

namespace ZR.Model.HR.Dto.EmployeeCore
{
    /// <summary>
    /// Employee dependent (display DTO)
    /// </summary>
    public class DependentDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeDependentLinkId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long DependentPersonId { get; set; }

        public string Relationship { get; set; }
        public string RelationshipAr { get; set; }

        // Person details
        public string FullName { get; set; }
        public string FullNameAr { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public int? Age { get; set; }
        public string Gender { get; set; }
        public string GenderAr { get; set; }
        public string Nationality { get; set; }
        public string NationalityAr { get; set; }
        public string IdNumber { get; set; }

        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// Create dependent DTO
    /// </summary>
    public class CreateDependentDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? RelationshipDictCode { get; set; }

        public DateTime EffectiveFrom { get; set; }

        // Person information
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullNameAr { get; set; }
        public DateTime? DateOfBirth { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? GenderDictCode { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? NationalityDictCode { get; set; }

        public string IdNumber { get; set; }
    }

    /// <summary>
    /// Update dependent DTO
    /// </summary>
    public class UpdateDependentDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeDependentLinkId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? RelationshipDictCode { get; set; }

        public DateTime? EffectiveTo { get; set; }
        public bool IsActive { get; set; }

        // Person information updates
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullNameAr { get; set; }
        public DateTime? DateOfBirth { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? GenderDictCode { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? NationalityDictCode { get; set; }

        public string IdNumber { get; set; }
    }
}
