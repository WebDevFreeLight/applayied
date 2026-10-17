using System;
using Infrastructure.Converter;

namespace ZR.Model.Marketing.Dto
{
    /// <summary>
    /// Marketing Employee DTO
    /// Represents an employee in the marketing team with their details
    /// </summary>
    public class MarketingEmployeeDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }
        public string EmployeeCode { get; set; }
        public DateTime? HireDate { get; set; }
        public string GivenName { get; set; }
        public string FamilyName { get; set; }
        public string FullNameAr { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? PositionId { get; set; }
        public string PositionName { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? ManagerEmployeeId { get; set; }
        public bool IsActive { get; set; }
    }
}
