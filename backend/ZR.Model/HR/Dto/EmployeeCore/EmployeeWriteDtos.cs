using Infrastructure.Converter;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ZR.Model.HR.Dto.EmployeeCore
{
    #region Employee Core DTOs

    /// <summary>
    /// Create Employee Core DTO
    /// </summary>
    public class CreateEmployeeCoreDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long PersonId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? UserId { get; set; }

        public string EmployeeCode { get; set; }
        public DateTime? HireDate { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? EmploymentTypeDictCode { get; set; }

        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// Update Employee Core DTO
    /// </summary>
    public class UpdateEmployeeCoreDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }

        public string EmployeeCode { get; set; }
        public DateTime? HireDate { get; set; }
        public DateTime? TerminationDate { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? EmploymentTypeDictCode { get; set; }

        public bool IsActive { get; set; }
    }

    #endregion

    #region Employee Assignment DTOs

    /// <summary>
    /// Create Employee Assignment DTO
    /// </summary>
    public class CreateEmployeeAssignmentDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? DepartmentId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? PositionId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? ManagerEmployeeId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? WorkLocationId { get; set; }

        public DateTime EffectiveFrom { get; set; } = DateTime.Now;
        public DateTime? EffectiveTo { get; set; }
        public bool IsPrimary { get; set; } = true;
    }

    /// <summary>
    /// Update Employee Assignment DTO
    /// </summary>
    public class UpdateEmployeeAssignmentDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeAssignmentId { get; set; }

        /// <summary>
        /// EmployeeId - Required when EmployeeAssignmentId = 0 (CREATE scenario)
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? EmployeeId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? DepartmentId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? PositionId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? ManagerEmployeeId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? WorkLocationId { get; set; }

        public DateTime? EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public bool IsPrimary { get; set; }
    }

    #endregion

    #region Employee Status DTOs

    /// <summary>
    /// Create Employee Status DTO
    /// </summary>
    public class CreateEmployeeStatusDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmploymentStatusDictCode { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? StatusReasonDictCode { get; set; }

        public DateTime EffectiveFrom { get; set; } = DateTime.Now;
        public DateTime? EffectiveTo { get; set; }
    }

    /// <summary>
    /// Update Employee Status DTO
    /// </summary>
    public class UpdateEmployeeStatusDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeStatusId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmploymentStatusDictCode { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? StatusReasonDictCode { get; set; }

        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
    }

    #endregion

    #region Employee Contract DTOs

    /// <summary>
    /// Create Employee Contract DTO
    /// </summary>
    public class CreateEmployeeContractDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? ContractTypeDictCode { get; set; }

        public DateTime? ContractStartDate { get; set; }
        public DateTime? ContractEndDate { get; set; }
        public DateTime? ProbationEndDate { get; set; }
        public bool IsCurrent { get; set; } = true;

        public DateTime EffectiveFrom { get; set; } = DateTime.Now;
        public DateTime? EffectiveTo { get; set; }
    }

    /// <summary>
    /// Update Employee Contract DTO
    /// </summary>
    public class UpdateEmployeeContractDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeContractId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? ContractTypeDictCode { get; set; }

        public DateTime? ContractStartDate { get; set; }
        public DateTime? ContractEndDate { get; set; }
        public DateTime? ProbationEndDate { get; set; }
        public bool IsCurrent { get; set; }

        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
    }

    #endregion

    #region Employee Statutory DTOs

    /// <summary>
    /// Create Employee Statutory DTO
    /// </summary>
    public class CreateEmployeeStatutoryDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? CountryDictCode { get; set; }

        public bool HasGosi { get; set; }
        public string GosiNumber { get; set; }
        public bool IsSaudi { get; set; }
        public string MedicalInsuranceNumber { get; set; }

        public DateTime EffectiveFrom { get; set; } = DateTime.Now;
        public DateTime? EffectiveTo { get; set; }
    }

    /// <summary>
    /// Update Employee Statutory DTO
    /// </summary>
    public class UpdateEmployeeStatutoryDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeStatutoryId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? CountryDictCode { get; set; }

        public bool HasGosi { get; set; }
        public string GosiNumber { get; set; }
        public bool IsSaudi { get; set; }
        public string MedicalInsuranceNumber { get; set; }

        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
    }

    #endregion

    #region Employee Bank Account DTOs

    /// <summary>
    /// Create Employee Bank Account DTO
    /// </summary>
    public class CreateEmployeeBankAccountDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }

        public string BankName { get; set; }

        /// <summary>
        /// Plain IBAN (will be encrypted before storage)
        /// </summary>
        public string Iban { get; set; }

        /// <summary>
        /// Plain Account Number (will be encrypted before storage)
        /// </summary>
        public string AccountNumber { get; set; }

        /// <summary>
        /// Plain Bank Account (will be encrypted before storage)
        /// </summary>
        public string BankAccount { get; set; }

        public Guid? CurrencyId { get; set; }

        public DateTime EffectiveFrom { get; set; } = DateTime.Now;
        public DateTime? EffectiveTo { get; set; }
        public bool IsPrimary { get; set; } = false;
    }

    /// <summary>
    /// Update Employee Bank Account DTO
    /// </summary>
    public class UpdateEmployeeBankAccountDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeBankAccountId { get; set; }

        /// <summary>
        /// EmployeeId - Required when EmployeeBankAccountId = 0 (CREATE scenario)
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? EmployeeId { get; set; }

        public string BankName { get; set; }

        /// <summary>
        /// Plain IBAN (will be encrypted before storage)
        /// </summary>
        public string Iban { get; set; }

        /// <summary>
        /// Plain Account Number (will be encrypted before storage)
        /// </summary>
        public string AccountNumber { get; set; }

        /// <summary>
        /// Plain Bank Account (will be encrypted before storage)
        /// </summary>
        public string BankAccount { get; set; }

        public Guid? CurrencyId { get; set; }

        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public bool IsPrimary { get; set; }
    }

    #endregion

    #region Employee Compensation DTOs

    /// <summary>
    /// Create Employee Compensation Package DTO
    /// </summary>
    public class CreateEmployeeCompensationPackageDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }

        public Guid? CurrencyId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? CompPackageStatusDictCode { get; set; }

        public DateTime EffectiveFrom { get; set; } = DateTime.Now;
        public DateTime? EffectiveTo { get; set; }

        /// <summary>
        /// Compensation components within this package
        /// </summary>
        public List<CreateEmployeeCompensationComponentDto> Components { get; set; }
    }

    /// <summary>
    /// Update Employee Compensation Package DTO
    /// </summary>
    public class UpdateEmployeeCompensationPackageDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long CompPackageId { get; set; }

        public Guid? CurrencyId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? CompPackageStatusDictCode { get; set; }

        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
    }

    /// <summary>
    /// Create Employee Compensation Component DTO
    /// </summary>
    public class CreateEmployeeCompensationComponentDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? CompPackageId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long ComponentTypeDictCode { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long ComponentCodeDictCode { get; set; }

        public decimal Amount { get; set; }
    }

    /// <summary>
    /// Update Employee Compensation Component DTO
    /// </summary>
    public class UpdateEmployeeCompensationComponentDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long CompComponentId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long ComponentTypeDictCode { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long ComponentCodeDictCode { get; set; }

        public decimal Amount { get; set; }
    }

    #endregion

    #region Employee Emergency Contact DTOs

    /// <summary>
    /// Create Employee Emergency Contact DTO
    /// </summary>
    public class CreateEmployeeEmergencyContactDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }

        public string ContactName { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? RelationshipDictCode { get; set; }

        public string Phone { get; set; }
        public string AlternatePhone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public bool IsPrimary { get; set; } = false;
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// Update Employee Emergency Contact DTO
    /// </summary>
    public class UpdateEmployeeEmergencyContactDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmergencyContactId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? EmployeeId { get; set; } // Required for CREATE operations (when EmergencyContactId = 0)

        public string ContactName { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? RelationshipDictCode { get; set; }

        public string Phone { get; set; }
        public string AlternatePhone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsActive { get; set; }
    }

    #endregion

    #region Employee Dependent Link DTOs

    /// <summary>
    /// Create Employee Dependent Link DTO
    /// </summary>
    public class CreateEmployeeDependentLinkDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long DependentPersonId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long RelationshipDictCode { get; set; }

        public DateTime EffectiveFrom { get; set; } = DateTime.Now;
        public DateTime? EffectiveTo { get; set; }
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// Update Employee Dependent Link DTO
    /// </summary>
    public class UpdateEmployeeDependentLinkDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeDependentLinkId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long RelationshipDictCode { get; set; }

        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public bool IsActive { get; set; }
    }

    #endregion

    #region Complete Employee DTO

    /// <summary>
    /// Create Complete Employee DTO - Create employee with all related entities in one transaction
    /// </summary>
    public class CreateEmployeeCompleteDto
    {
        // Person Information
        public CreatePersonCompleteDto Person { get; set; }

        // Employee Core Information
        public CreateEmployeeCoreDto Employee { get; set; }

        // Employment Details
        public List<CreateEmployeeAssignmentDto> Assignments { get; set; }
        public List<CreateEmployeeStatusDto> Statuses { get; set; }
        public List<CreateEmployeeContractDto> Contracts { get; set; }
        public List<CreateEmployeeStatutoryDto> Statutory { get; set; }

        // Compensation & Banking
        public List<CreateEmployeeCompensationPackageDto> CompensationPackages { get; set; }
        public List<CreateEmployeeBankAccountDto> BankAccounts { get; set; }

        // Family & Emergency
        public List<CreateEmployeeEmergencyContactDto> EmergencyContacts { get; set; }
        public List<CreateEmployeeDependentLinkDto> Dependents { get; set; }
    }

    /// <summary>
    /// Update Complete Employee DTO - Update entire employee hierarchy
    /// </summary>
    public class UpdateEmployeeCompleteDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long PersonId { get; set; }

        // Employee Core Information
        public UpdateEmployeeCoreDto Employee { get; set; }

        // Person Information (optional - only if changing)
        public UpdatePersonDto Person { get; set; }
        public List<UpdatePersonNameDto> Names { get; set; }
        public UpdatePersonDemographicDto Demographic { get; set; }
        public List<UpdatePersonIdentifierDto> Identifiers { get; set; }
        public List<UpdatePersonContactDto> Contacts { get; set; }
        public List<UpdatePersonAddressDto> Addresses { get; set; }

        // Employment Details (optional - only if changing)
        public List<UpdateEmployeeAssignmentDto> Assignments { get; set; }
        public List<UpdateEmployeeStatusDto> Statuses { get; set; }
        public List<UpdateEmployeeContractDto> Contracts { get; set; }
        public List<UpdateEmployeeStatutoryDto> Statutory { get; set; }

        // Compensation & Banking (optional - only if changing)
        public List<UpdateEmployeeCompensationPackageDto> CompensationPackages { get; set; }
        public List<UpdateEmployeeBankAccountDto> BankAccounts { get; set; }

        // Family & Emergency (optional - only if changing)
        public List<UpdateEmployeeEmergencyContactDto> EmergencyContacts { get; set; }
        public List<UpdateEmployeeDependentLinkDto> Dependents { get; set; }
    }

    #endregion

    #region Orchestration DTOs

    /// <summary>
    /// Terminate Employee DTO
    /// </summary>
    public class TerminateEmployeeDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }

        public DateTime TerminationDate { get; set; }

        public string Reason { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? TerminationReasonDictCode { get; set; }
    }

    /// <summary>
    /// Transfer Employee DTO
    /// </summary>
    public class TransferEmployeeDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long NewDepartmentId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? NewPositionId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? NewManagerEmployeeId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? NewWorkLocationId { get; set; }

        public DateTime EffectiveDate { get; set; }

        public string Reason { get; set; }
    }

    /// <summary>
    /// Promote Employee DTO
    /// </summary>
    public class PromoteEmployeeDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long NewPositionId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? NewDepartmentId { get; set; }

        public decimal? NewBaseSalary { get; set; }

        public DateTime EffectiveDate { get; set; }

        public string Reason { get; set; }
    }

    /// <summary>
    /// Update Employee Status DTO (orchestration)
    /// </summary>
    public class UpdateEmployeeStatusOrchestrationDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long StatusDictCode { get; set; }

        public DateTime EffectiveDate { get; set; }

        public string Reason { get; set; }
    }

    #endregion
}
