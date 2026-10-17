using System;

namespace ZR.Model.HR.Dto.EmployeeCore
{
    /// <summary>
    /// Employee Excel Export DTO - Flat structure with resolved labels for all dict codes
    /// Mapped from EmployeeApiDto (which already resolves everything)
    /// </summary>
    public class EmployeeExportDto
    {
        [ExcelColumn(Name = "Employee Code", Width = 15)]
        public string EmployeeCode { get; set; }

        [ExcelColumn(Name = "Full Name (EN)", Width = 25)]
        public string FullName { get; set; }

        [ExcelColumn(Name = "Full Name (AR)", Width = 25)]
        public string FullNameAr { get; set; }

        [ExcelColumn(Name = "Gender", Width = 10)]
        public string Gender { get; set; }

        [ExcelColumn(Name = "Date of Birth", Width = 15, Format = "yyyy-MM-dd")]
        public DateTime? DateOfBirth { get; set; }

        [ExcelColumn(Name = "Nationality", Width = 15)]
        public string Nationality { get; set; }

        [ExcelColumn(Name = "Marital Status", Width = 15)]
        public string MaritalStatus { get; set; }

        [ExcelColumn(Name = "Work Email", Width = 30)]
        public string Email { get; set; }

        [ExcelColumn(Name = "Work Phone", Width = 18)]
        public string Phone { get; set; }

        [ExcelColumn(Name = "Mobile", Width = 18)]
        public string MobileNumber { get; set; }

        [ExcelColumn(Name = "Personal Email", Width = 30)]
        public string PersonalEmail { get; set; }

        [ExcelColumn(Name = "National ID", Width = 18)]
        public string IdNumber { get; set; }

        [ExcelColumn(Name = "ID Expiry Date", Width = 15, Format = "yyyy-MM-dd")]
        public DateTime? IdExpiryDate { get; set; }

        [ExcelColumn(Name = "Iqama Number", Width = 18)]
        public string IqamaNumber { get; set; }

        [ExcelColumn(Name = "Iqama Expiry Date", Width = 15, Format = "yyyy-MM-dd")]
        public DateTime? IqamaExpiryDate { get; set; }

        [ExcelColumn(Name = "Passport Number", Width = 18)]
        public string PassportNumber { get; set; }

        [ExcelColumn(Name = "Passport Expiry Date", Width = 15, Format = "yyyy-MM-dd")]
        public DateTime? PassportExpiryDate { get; set; }

        [ExcelColumn(Name = "Address", Width = 35)]
        public string Address { get; set; }

        [ExcelColumn(Name = "Department", Width = 20)]
        public string DepartmentName { get; set; }

        [ExcelColumn(Name = "Position", Width = 20)]
        public string PositionName { get; set; }

        [ExcelColumn(Name = "Manager", Width = 25)]
        public string ManagerName { get; set; }

        [ExcelColumn(Name = "Hire Date", Width = 15, Format = "yyyy-MM-dd")]
        public DateTime? HireDate { get; set; }

        [ExcelColumn(Name = "Employment Type", Width = 15)]
        public string EmploymentType { get; set; }

        [ExcelColumn(Name = "Status", Width = 12)]
        public string Status { get; set; }

        [ExcelColumn(Name = "Active", Width = 8)]
        public string IsActive { get; set; }

        [ExcelColumn(Name = "Work Location", Width = 18)]
        public string WorkLocation { get; set; }

        [ExcelColumn(Name = "Contract Start", Width = 15, Format = "yyyy-MM-dd")]
        public DateTime? ContractStartDate { get; set; }

        [ExcelColumn(Name = "Contract End", Width = 15, Format = "yyyy-MM-dd")]
        public DateTime? ContractEndDate { get; set; }

        [ExcelColumn(Name = "GOSI Number", Width = 18)]
        public string GosiNumber { get; set; }

        [ExcelColumn(Name = "Medical Insurance", Width = 18)]
        public string MedicalInsuranceNumber { get; set; }

        [ExcelColumn(Name = "Saudi National", Width = 12)]
        public string IsSaudi { get; set; }

        [ExcelColumn(Name = "Bank Name", Width = 20)]
        public string BankName { get; set; }

        [ExcelColumn(Name = "IBAN (Masked)", Width = 30)]
        public string Iban { get; set; }

        [ExcelColumn(Name = "Account (Masked)", Width = 20)]
        public string AccountNumber { get; set; }

        [ExcelColumn(Name = "Emergency Contact", Width = 25)]
        public string EmergencyContactName { get; set; }

        [ExcelColumn(Name = "Emergency Phone", Width = 18)]
        public string EmergencyContactPhone { get; set; }
    }
}
