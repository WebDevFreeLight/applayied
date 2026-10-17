using System;

namespace ZR.Model.HR.Dto.EmployeeCore
{
    /// <summary>
    /// DTO for creating a new address
    /// </summary>
    public class CreateAddressDto
    {
        public long? AddressTypeDictCode { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
        public long? CountryDictCode { get; set; }
        public bool IsPrimary { get; set; }
        public DateTime? EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
    }

    /// <summary>
    /// DTO for updating an existing address
    /// </summary>
    public class UpdateAddressDto
    {
        public long? AddressTypeDictCode { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
        public long? CountryDictCode { get; set; }
        public bool? IsPrimary { get; set; }
        public DateTime? EffectiveTo { get; set; }
    }

    /// <summary>
    /// DTO for simple primary address update
    /// </summary>
    public class UpdatePrimaryAddressDto
    {
        public string Address { get; set; }
    }
}
