using Infrastructure.Converter;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using ZR.Model;

namespace ZR.Model.HR.Dto.EmployeeCore
{
    /// <summary>
    /// Employee API DTO - Simplified flat structure for frontend consumption
    /// Maps to normalized EmployeeCore backend structure
    /// </summary>
    public class EmployeeApiDto
    {
        // Core Identity
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long PersonId { get; set; }

        public string EmployeeCode { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? UserId { get; set; }

        // Name Information (legacy)
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName => $"{FirstName} {LastName}".Trim();
        public string NameAr { get; set; }

        // Normalized person name fields (preferred)
        public string GivenName { get; set; }
        public string FamilyName { get; set; }
        public string MiddleName { get; set; }
        public string FullNameAr { get; set; }

        // Contact Information
        public string Email { get; set; }
        public string Phone { get; set; }
        public string PersonalEmail { get; set; }
        public string MobileNumber { get; set; }

        // Personal Information
        public DateTime? DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string GenderLabel { get; set; }
        public string MaritalStatus { get; set; }
        public string MaritalStatusLabel { get; set; }
        public string Nationality { get; set; }
        public string NationalityLabel { get; set; }
        public string IdNumber { get; set; }
        public DateTime? IdExpiryDate { get; set; }
        public string IqamaNumber { get; set; }
        public DateTime? IqamaExpiryDate { get; set; }
        public string PassportNumber { get; set; }
        public DateTime? PassportExpiryDate { get; set; }
        public string Address { get; set; }

        // Employment Information
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? DepartmentId { get; set; }
        public string DepartmentName { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? PositionId { get; set; }
        public string PositionName { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? ManagerId { get; set; }
        public string ManagerName { get; set; }

        public DateTime? HireDate { get; set; }
        public DateTime? TerminationDate { get; set; }
        public string Status { get; set; }
        public string EmploymentType { get; set; }
        public string EmploymentTypeLabel { get; set; }
        public bool IsActive { get; set; }
        public string WorkLocation { get; set; }

        // Contract Information
        public DateTime? ContractStartDate { get; set; }
        public DateTime? ContractEndDate { get; set; }

        // Statutory Information
        public string GosiNumber { get; set; }
        public string MedicalInsuranceNumber { get; set; }
        public bool IsSaudi { get; set; }

        // Bank Information (masked for security)
        public string BankName { get; set; }
        public string AccountNumber { get; set; } // Masked: ****1234
        public string Iban { get; set; } // Masked: SA**************1234

        // Emergency Contact
        public string EmergencyContactName { get; set; }
        public string EmergencyContactPhone { get; set; }

        // Audit Fields
        public string CreateBy { get; set; }
        public DateTime? CreateTime { get; set; }
        public string UpdateBy { get; set; }
        public DateTime? UpdateTime { get; set; }
        public string Remark { get; set; }
    }

    /// <summary>
    /// Employee List DTO - Lightweight for list views
    /// </summary>
    public class EmployeeListDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }

        public string EmployeeCode { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName => $"{FirstName} {LastName}".Trim();
        public string Email { get; set; }
        public string Phone { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? DepartmentId { get; set; }
        public string DepartmentName { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? PositionId { get; set; }
        public string PositionName { get; set; }

        public string Status { get; set; }
        public bool IsActive { get; set; }
        public string EmploymentType { get; set; }
        public DateTime? HireDate { get; set; }
    }

    /// <summary>
    /// Create Employee Request - Input DTO for creating new employee
    /// </summary>
    public class CreateEmployeeRequest
    {
        // Required Name Information
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string NameAr { get; set; }

        // Contact Information
        public string Email { get; set; }
        public string Phone { get; set; }
        public string PersonalEmail { get; set; }
        public string MobileNumber { get; set; }

        // Personal Information
        public DateTime? DateOfBirth { get; set; }
        public string Gender { get; set; } // Dict code: "0"=Male, "1"=Female
        public string MaritalStatus { get; set; } // Dict code: "0"=Single, "1"=Married, etc.
        public string Nationality { get; set; }
        public string IdNumber { get; set; }
        public string PassportNumber { get; set; }
        public DateTime? PassportExpiryDate { get; set; }
        public string Address { get; set; }

        // Employment Information
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? DepartmentId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? PositionId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? ManagerId { get; set; }

        public DateTime HireDate { get; set; } = DateTime.Now;
        public string EmploymentType { get; set; } // Dict code
        public string WorkLocation { get; set; }

        // Contract Information
        public DateTime? ContractStartDate { get; set; }
        public DateTime? ContractEndDate { get; set; }

        // Statutory Information
        public string GosiNumber { get; set; }
        public string MedicalInsuranceNumber { get; set; }
        public bool IsSaudi { get; set; }

        // Bank Information
        public string BankName { get; set; }
        public string AccountNumber { get; set; }
        public string Iban { get; set; }

        // Emergency Contact
        public string EmergencyContactName { get; set; }
        public string EmergencyContactPhone { get; set; }

        public string Remark { get; set; }
    }

    /// <summary>
    /// Update Employee Request - Input DTO for updating employee
    /// All fields optional except EmployeeId
    /// </summary>
    public class UpdateEmployeeRequest
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }

        // Name Information (support both naming conventions)
        public string FirstName { get; set; }  // Alias for GivenName
        public string LastName { get; set; }   // Alias for FamilyName
        public string NameAr { get; set; }     // Alias for FullNameAr

        // Normalized person name fields (preferred)
        public string GivenName { get; set; }
        public string FamilyName { get; set; }
        public string MiddleName { get; set; }
        public string FullNameAr { get; set; }

        // Contact Information
        public string Email { get; set; }
        public string Phone { get; set; }
        public string PersonalEmail { get; set; }
        public string MobileNumber { get; set; }

        // Personal Information
        public DateTime? DateOfBirth { get; set; }
        public string Gender { get; set; }  // String value for legacy support
        public string MaritalStatus { get; set; }  // String value for legacy support
        public string Nationality { get; set; }

        // Personal Information (Dict Codes - preferred)
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? GenderDictCode { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? MaritalStatusDictCode { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? NationalityDictCode { get; set; }
        public string IdNumber { get; set; }
        public DateTime? IdExpiryDate { get; set; }
        public string IqamaNumber { get; set; }
        public DateTime? IqamaExpiryDate { get; set; }
        public string PassportNumber { get; set; }
        public DateTime? PassportExpiryDate { get; set; }
        public string Address { get; set; }

        // Employment Information
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? DepartmentId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? PositionId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? ManagerId { get; set; }

        public DateTime? HireDate { get; set; }
        public string Status { get; set; } // "active", "inactive", "terminated"
        public string EmploymentType { get; set; }
        public string WorkLocation { get; set; }

        // Contract Information
        public DateTime? ContractStartDate { get; set; }
        public DateTime? ContractEndDate { get; set; }

        // Statutory Information
        public string GosiNumber { get; set; }
        public string MedicalInsuranceNumber { get; set; }

        // Bank Information
        public string BankName { get; set; }
        public string AccountNumber { get; set; }
        public string Iban { get; set; }

        // Emergency Contact
        public string EmergencyContactName { get; set; }
        public string EmergencyContactPhone { get; set; }

        public string Remark { get; set; }
    }

    /// <summary>
    /// Employee Query Request - For filtering and searching
    /// </summary>
    public class EmployeeQueryRequest : PagerInfo
    {
        public string EmployeeCode { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? DepartmentId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? PositionId { get; set; }

        public string Status { get; set; }
        public bool? IsActive { get; set; }
        public string EmploymentType { get; set; }
        public DateTime? HireDateFrom { get; set; }
        public DateTime? HireDateTo { get; set; }
    }

    /// <summary>
    /// Employee Statistics Response
    /// </summary>
    public class EmployeeStatisticsResponse
    {
        public int TotalEmployees { get; set; }
        public int ActiveEmployees { get; set; }
        public int InactiveEmployees { get; set; }
        public int TerminatedEmployees { get; set; }

        public Dictionary<string, int> ByDepartment { get; set; }
        public Dictionary<string, int> ByEmploymentType { get; set; }
        public Dictionary<string, int> ByNationality { get; set; }

        public int NewHiresThisMonth { get; set; }
        public int TerminationsThisMonth { get; set; }
    }
}
