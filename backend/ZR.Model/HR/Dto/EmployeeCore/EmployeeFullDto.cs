using System;

namespace ZR.Model.HR.Dto.EmployeeCore
{
    /// <summary>
    /// Complete employee DTO - SINGLE SOURCE OF TRUTH
    /// Maps to ACTUAL normalized model properties (with dict codes)
    /// </summary>
    public class EmployeeFullDto
    {
        // Core Identity
        public long EmployeeId { get; set; }
        public long PersonId { get; set; }
        public long? UserId { get; set; }
        public string EmployeeCode { get; set; }
        public DateTime? HireDate { get; set; }
        public bool IsActive { get; set; }

        // Person Name (from HrPersonName - ACTUAL properties)
        public string GivenName { get; set; }        // NOT FirstName!
        public string FamilyName { get; set; }       // NOT LastName!
        public string MiddleName { get; set; }
        public string FullNameAr { get; set; }

        // Convenience properties for backward compatibility
        public string FirstName => GivenName;       // Alias
        public string LastName => FamilyName;        // Alias
        public string FullName => $"{GivenName} {FamilyName}".Trim();
        public string NameAr => FullNameAr;

        // Person Contact (from HrPersonContact - using dict codes)
        // Note: Actual model stores these as ContactTypeDictCode + ContactValue
        // We'll leave these empty for now and populate in Phase 2
        public string Email { get; set; }
        public string Phone { get; set; }
        public string MobileNumber { get; set; }

        // Person Demographics (from HrPersonDemographic - DICT CODES!)
        public long? GenderDictCode { get; set; }
        public long? MaritalStatusDictCode { get; set; }
        public long? NationalityDictCode { get; set; }
        public DateTime? DateOfBirth { get; set; }

        // Convenience string properties (empty for now, can be filled with dict lookups later)
        public string Gender { get; set; }
        public string MaritalStatus { get; set; }
        public string Nationality { get; set; }

        // Employee Assignment (from HrEmployeeAssignment)
        public long? DepartmentId { get; set; }
        public long? PositionId { get; set; }
        public long? ManagerEmployeeId { get; set; }
        public long? WorkLocationId { get; set; }

        // Employee Status (from HrEmployeeStatus - DICT CODE!)
        public long? EmploymentStatusDictCode { get; set; }

        // Convenience string property
        public string Status { get; set; }
        public string CurrentStatus => Status;

        // Employee Contract (from HrEmployeeContract)
        public long EmployeeContractId { get; set; }
        public long? ContractTypeDictCode { get; set; }  // DICT CODE!
        public DateTime? ContractStartDate { get; set; }
        public DateTime? ContractEndDate { get; set; }

        // Convenience string property
        public string ContractType { get; set; }

        // Bank Account (from HrEmployeeBankAccount - ENCRYPTED!)
        public string BankName { get; set; }
        public string BankAccountEncrypted { get; set; }
        public string IbanEncrypted { get; set; }
        public string AccountNumberEncrypted { get; set; }

        // Decrypted properties (for display - will be populated if needed)
        public string BankAccountNumber { get; set; }
        public string IBAN { get; set; }

        // Statutory (from HrEmployeeStatutory)
        public string GosiNumber { get; set; }
        public bool IsSaudi { get; set; }
        public string MedicalInsuranceNumber { get; set; }

        // Convenience properties
        public long? ManagerId => ManagerEmployeeId;  // Alias for old code
        public string Name => FullName;                // Alias for old code

        // Calculated Properties
        public int? Age => DateOfBirth.HasValue ?
            (DateTime.Now.Year - DateOfBirth.Value.Year -
            (DateTime.Now.DayOfYear < DateOfBirth.Value.DayOfYear ? 1 : 0)) : null;

        public int? TenureYears => HireDate.HasValue ?
            (DateTime.Now.Year - HireDate.Value.Year -
            (DateTime.Now.DayOfYear < HireDate.Value.DayOfYear ? 1 : 0)) : null;
    }
}
