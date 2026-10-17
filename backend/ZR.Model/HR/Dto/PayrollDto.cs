 
using Infrastructure.Converter;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ZR.Model.System;

namespace ZR.Model.HR.Dto
{
    /// <summary>
    /// Payroll Query DTO
    /// </summary>
    public class PayrollQueryDto : PagerInfo
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? EmployeeId { get; set; }
        public string PayPeriod { get; set; }
        public string Status { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? DepartmentId { get; set; }
        public DateTime? PaymentDateFrom { get; set; }
        public DateTime? PaymentDateTo { get; set; }
    }

    /// <summary>
    /// Payroll DTO
    /// </summary>
    public class PayrollDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long PayrollId { get; set; }
        
        [Required(ErrorMessage = "Employee is required")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }
        
        [Required(ErrorMessage = "Pay period is required")]
        public string PayPeriod { get; set; }
        
        public decimal BasicSalary { get; set; }
        public decimal HousingAllowance { get; set; }
        public decimal TransportAllowance { get; set; }
        public decimal OtherAllowances { get; set; }
        public decimal OvertimePay { get; set; }
        public decimal Deductions { get; set; }
        public decimal GosiEmployee { get; set; }
        public decimal GosiEmployer { get; set; }
        public decimal Tax { get; set; }
        public decimal NetSalary { get; set; }
        public DateTime? PaymentDate { get; set; }
        public string PaymentMethod { get; set; }
        public string BankReference { get; set; }
        public string Status { get; set; }
        public int WorkingDays { get; set; }
        public int ActualDays { get; set; }
        public decimal LeaveDeduction { get; set; }
        public decimal LateDeduction { get; set; }
        public decimal Bonus { get; set; }
        public decimal Commission { get; set; }
        public string Remarks { get; set; }
    }

    /// <summary>
    /// Payroll Calculation DTO
    /// </summary>
    public class PayrollCalculationDto
    {
        [Required(ErrorMessage = "Employee is required")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }
        
        [Required(ErrorMessage = "Pay period is required")]
        public string PayPeriod { get; set; }
        
        public bool IncludeOvertime { get; set; } = true;
        public bool ApplyDeductions { get; set; } = true;
        public bool CalculateGOSI { get; set; } = true;
        public bool CalculateTax { get; set; } = true;


        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<long> EmployeeIds { get; set; }
    }

    /// <summary>
    /// Bulk Payroll Process DTO
    /// </summary>
    public class BulkPayrollDto
    {
        [Required(ErrorMessage = "Pay period is required")]
        public string PayPeriod { get; set; }
        
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? DepartmentId { get; set; }
        public List<long> EmployeeIds { get; set; }
        public bool AutoApprove { get; set; } = false;
    }

    /// <summary>
    /// Payroll Summary DTO
    /// </summary>
    public class PayrollSummaryDto
    {
        public string PayPeriod { get; set; }
        public int TotalEmployees { get; set; }
        public int ProcessedCount { get; set; }
        public int PendingCount { get; set; }
        public int PaidCount { get; set; }
        public decimal TotalGrossSalary { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal TotalNetSalary { get; set; }
        public decimal TotalGOSI { get; set; }
        public decimal TotalTax { get; set; }
        public List<DepartmentPayrollSummary> DepartmentSummaries { get; set; }
    }

    public class DepartmentPayrollSummary
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public int EmployeeCount { get; set; }
        public decimal TotalGross { get; set; }
        public decimal TotalNet { get; set; }
    }

    /// <summary>
    /// Payslip DTO
    /// </summary>
    public class PayslipDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long PayrollId { get; set; }
        public string EmployeeCode { get; set; }
        public string EmployeeName { get; set; }
        public string Department { get; set; }
        public string Position { get; set; }
        public string PayPeriod { get; set; }
        public DateTime PaymentDate { get; set; }
        
        // Earnings
        public decimal BasicSalary { get; set; }
        public decimal HousingAllowance { get; set; }
        public decimal TransportAllowance { get; set; }
        public decimal OtherAllowances { get; set; }
        public decimal OvertimePay { get; set; }
        public decimal Bonus { get; set; }
        public decimal Commission { get; set; }
        public decimal TotalEarnings { get; set; }
        
        // Deductions
        public decimal GOSIEmployee { get; set; }
        public decimal Tax { get; set; }
        public decimal LeaveDeduction { get; set; }
        public decimal LateDeduction { get; set; }
        public decimal OtherDeductions { get; set; }
        public decimal TotalDeductions { get; set; }
        
        // Summary
        public decimal NetSalary { get; set; }
        public int WorkingDays { get; set; }
        public int ActualDays { get; set; }
        public int LeaveDays { get; set; }
        public int LateDays { get; set; }
        
        // Bank Details
        public string BankName { get; set; }
        public string AccountNumber { get; set; }
        public string IBAN { get; set; }
    }

    /// <summary>
    /// GOSI Compliance Report DTO
    /// </summary>
    public class GOSIComplianceDto
    {
        public string PayPeriod { get; set; }
        public int TotalEmployees { get; set; }
        public int SaudiEmployees { get; set; }
        public int NonSaudiEmployees { get; set; }
        public decimal TotalWages { get; set; }
        public decimal TotalEmployeeContribution { get; set; }
        public decimal TotalEmployerContribution { get; set; }
        public decimal TotalGOSI { get; set; }
        public List<EmployeeGOSIDetail> EmployeeDetails { get; set; }
    }

    public class EmployeeGOSIDetail
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }
        public string EmployeeCode { get; set; }
        public string EmployeeName { get; set; }
        public string GOSINumber { get; set; }
        public string Nationality { get; set; }
        public decimal BasicSalary { get; set; }
        public decimal EmployeeContribution { get; set; }
        public decimal EmployerContribution { get; set; }
        public decimal Total { get; set; }
    }

    /// <summary>
    /// End of Service Benefit (ESB) DTO
    /// </summary>
    public class ESBResultDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }
        public string EmployeeCode { get; set; }
        public string EmployeeName { get; set; }
        public DateTime? HireDate { get; set; }
        public DateTime CalculationDate { get; set; }
        public string TerminationType { get; set; }
        public decimal BasicSalary { get; set; }
        public decimal TotalServiceYears { get; set; }
        public decimal First5YearsAmount { get; set; }
        public decimal After5YearsAmount { get; set; }
        public decimal FullESBAmount { get; set; }
        public decimal EligibilityFactor { get; set; }
        public decimal FinalESBAmount { get; set; }
        public string EligibilityNote { get; set; }
    }

    /// <summary>
    /// Bank Transfer File DTO
    /// </summary>
    public class BankTransferDto
    {
        public string PayPeriod { get; set; }
        public string BankCode { get; set; }
        public DateTime ValueDate { get; set; }
        public List<BankTransferEntry> Entries { get; set; }
        public decimal TotalAmount { get; set; }
        public int TotalCount { get; set; }
    }

    public class BankTransferEntry
    {
        public string EmployeeCode { get; set; }
        public string EmployeeName { get; set; }
        public string IBAN { get; set; }
        public decimal Amount { get; set; }
        public string Reference { get; set; }
        public string Description { get; set; }
    }
}

