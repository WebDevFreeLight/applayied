using Infrastructure.Converter;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ZR.Model.HR.Dto.EmployeeCore
{
    #region Person DTOs

    /// <summary>
    /// Create Person DTO
    /// </summary>
    public class CreatePersonDto
    {
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// Update Person DTO
    /// </summary>
    public class UpdatePersonDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long PersonId { get; set; }

        public bool IsActive { get; set; }
    }

    #endregion

    #region Person Name DTOs

    /// <summary>
    /// Create Person Name DTO
    /// </summary>
    public class CreatePersonNameDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long PersonId { get; set; }

        public string GivenName { get; set; }
        public string MiddleName { get; set; }
        public string FamilyName { get; set; }
        public string FullNameAr { get; set; }

        public DateTime EffectiveFrom { get; set; } = DateTime.Now;
        public DateTime? EffectiveTo { get; set; }
        public bool IsPrimary { get; set; } = true;
    }

    /// <summary>
    /// Update Person Name DTO
    /// </summary>
    public class UpdatePersonNameDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long PersonNameId { get; set; }

        /// <summary>
        /// PersonId - Required when PersonNameId = 0 (CREATE scenario)
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? PersonId { get; set; }

        public string GivenName { get; set; }
        public string MiddleName { get; set; }
        public string FamilyName { get; set; }
        public string FullNameAr { get; set; }

        public DateTime? EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public bool IsPrimary { get; set; }
    }

    #endregion

    #region Person Demographic DTOs

    /// <summary>
    /// Create Person Demographic DTO
    /// </summary>
    public class CreatePersonDemographicDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long PersonId { get; set; }

        public DateTime DateOfBirth { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? GenderDictCode { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? MaritalStatusDictCode { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? NationalityDictCode { get; set; }
    }

    /// <summary>
    /// Update Person Demographic DTO
    /// </summary>
    public class UpdatePersonDemographicDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long PersonId { get; set; }

        public DateTime DateOfBirth { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? GenderDictCode { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? MaritalStatusDictCode { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? NationalityDictCode { get; set; }
    }

    #endregion

    #region Person Identifier DTOs

    /// <summary>
    /// Create Person Identifier DTO (Iqama, Passport, etc.)
    /// </summary>
    public class CreatePersonIdentifierDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long PersonId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long IdentifierTypeDictCode { get; set; }

        public string IdentifierNumber { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? IssuingCountryDictCode { get; set; }

        public DateTime? IssueDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public bool IsPrimary { get; set; } = false;
        public bool IsVerified { get; set; } = false;
    }

    /// <summary>
    /// Update Person Identifier DTO
    /// </summary>
    public class UpdatePersonIdentifierDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long PersonIdentifierId { get; set; }

        /// <summary>
        /// PersonId - Required when PersonIdentifierId = 0 (CREATE scenario)
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? PersonId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long IdentifierTypeDictCode { get; set; }

        public string IdentifierNumber { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? IssuingCountryDictCode { get; set; }

        public DateTime? IssueDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsVerified { get; set; }
    }

    #endregion

    #region Person Contact DTOs

    /// <summary>
    /// Create Person Contact DTO (Email, Phone, etc.)
    /// </summary>
    public class CreatePersonContactDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long PersonId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long ContactTypeDictCode { get; set; }

        public string ContactValue { get; set; }

        public DateTime EffectiveFrom { get; set; } = DateTime.Now;
        public DateTime? EffectiveTo { get; set; }
        public bool IsPrimary { get; set; } = false;
        public bool IsVerified { get; set; } = false;
    }

    /// <summary>
    /// Update Person Contact DTO
    /// </summary>
    public class UpdatePersonContactDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long PersonContactId { get; set; }

        /// <summary>
        /// PersonId - Required when PersonContactId = 0 (CREATE scenario)
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? PersonId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long ContactTypeDictCode { get; set; }

        public string ContactValue { get; set; }

        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsVerified { get; set; }
    }

    #endregion

    #region Person Address DTOs

    /// <summary>
    /// Create Person Address DTO
    /// </summary>
    public class CreatePersonAddressDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long PersonId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long AddressTypeDictCode { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? CountryDictCode { get; set; }

        public string City { get; set; }
        public string PostalCode { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }

        public DateTime EffectiveFrom { get; set; } = DateTime.Now;
        public DateTime? EffectiveTo { get; set; }
        public bool IsPrimary { get; set; } = false;
    }

    /// <summary>
    /// Update Person Address DTO
    /// </summary>
    public class UpdatePersonAddressDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long PersonAddressId { get; set; }

        /// <summary>
        /// PersonId - Required when PersonAddressId = 0 (CREATE scenario)
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? PersonId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long AddressTypeDictCode { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? CountryDictCode { get; set; }

        public string City { get; set; }
        public string PostalCode { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }

        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public bool IsPrimary { get; set; }
    }

    #endregion

    #region Complete Person DTO

    /// <summary>
    /// Create Complete Person DTO - Create person with all related entities
    /// </summary>
    public class CreatePersonCompleteDto
    {
        public CreatePersonDto Person { get; set; }
        public List<CreatePersonNameDto> Names { get; set; }
        public CreatePersonDemographicDto Demographic { get; set; }
        public List<CreatePersonIdentifierDto> Identifiers { get; set; }
        public List<CreatePersonContactDto> Contacts { get; set; }
        public List<CreatePersonAddressDto> Addresses { get; set; }
    }

    #endregion
}
