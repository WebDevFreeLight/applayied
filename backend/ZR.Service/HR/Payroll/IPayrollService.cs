
using Infrastructure.Converter;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ZR.Model;
using ZR.Model.HR.Dto;
using ZR.Model.HR.Payroll;

namespace ZR.Service.HR.IService
{
    /// <summary>
    /// Payroll Service Interface
    /// </summary>
    public interface IPayrollService 
    {
        Task<PagedInfo<HrPayroll>> GetPageListAsync(Expression<Func<HrPayroll, bool>> predicate, PayrollQueryDto pager);
        Task<HrPayroll> GetByIdAsync(long payrollId);


        Task<HrPayroll> GetEmployeePayrollAsync(long employeeId, string payPeriod);
        Task<object> GetPayrollListAsync(PayrollQueryDto query);
        Task<object> CalculatePayrollAsync(PayrollCalculationDto calculation);
        Task<int> CalculatePayrollAsync(long employeeId, string payPeriod);
        Task<int> CalculateBulkPayrollAsync(string payPeriod, long? departmentId = null);
        Task<object> ProcessBulkPayrollAsync(BulkPayrollDto bulk);
        Task<int> ProcessPayrollAsync(string payPeriod);
        Task<bool> ApprovePayrollAsync(long payrollId, string approvedBy);
        Task<int> ApprovePayrollAsync(long payrollId, string approvedBy, bool returnInt);
        Task<int> RejectPayrollAsync(long payrollId, string reason, string rejectedBy);
        Task<bool> ProcessPaymentAsync(long payrollId, string paymentMethod, string bankReference);
        Task<PayrollCalculationResult> CalculateSalaryComponentsAsync(long employeeId, string payPeriod);
        Task<decimal> CalculateGOSIAsync(decimal basicSalary, bool isEmployeeContribution);
        Task<decimal> CalculateTaxAsync(decimal taxableIncome);
        Task<object> GetPayrollStatisticsAsync(string payPeriod);
        Task<object> GetPayrollSummaryAsync(string payPeriod, long? departmentId = null);
        Task<byte[]> GeneratePayslipAsync(long payrollId);
        Task<PayslipDto> GetPayslipDetailsAsync(long payrollId);
        Task<int> GenerateBankFileAsync(string payPeriod);
        Task<object> GenerateBankTransferFileAsync(string payPeriod, string bankCode);
        Task<object> GetGOSIComplianceReportAsync(string payPeriod);
        Task<object> GetComplianceReportAsync(string payPeriod);
        Task<List<PayrollValidationError>> ValidatePayrollAsync(string payPeriod);
        Task<int> RecalculatePayrollAsync(long payrollId);
        Task<object> GetYearlyPayrollSummaryAsync(long employeeId, int year);

        // End of Service Benefit
        Task<ESBResultDto> CalculateESBAsync(long employeeId, string terminationType);

        // WPS Bank File
        Task<byte[]> GenerateWPSFileAsync(string payPeriod, string bankCode);

        // HR-Finance Integration
        Task<long> GeneratePayroll(long employeeId);
    }

    public class PayrollCalculationResult
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }
        public string PayPeriod { get; set; }
        public decimal BasicSalary { get; set; }
        public decimal HousingAllowance { get; set; }
        public decimal TransportAllowance { get; set; }
        public decimal OtherAllowances { get; set; }
        public decimal OvertimePay { get; set; }
        public decimal GrossTotal { get; set; }
        public decimal Deductions { get; set; }
        public decimal GOSIEmployee { get; set; }
        public decimal GOSIEmployer { get; set; }
        public decimal Tax { get; set; }
        public decimal NetSalary { get; set; }
        public int WorkingDays { get; set; }
        public int ActualDays { get; set; }
        public decimal LeaveDeduction { get; set; }
        public decimal LateDeduction { get; set; }
        public List<PayrollDeductionDetail> DeductionDetails { get; set; }
        public List<PayrollAllowanceDetail> AllowanceDetails { get; set; }
    }

    public class PayrollDeductionDetail
    {
        public string DeductionType { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
    }

    public class PayrollAllowanceDetail
    {
        public string AllowanceType { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
    }

    public class PayrollValidationError
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string ErrorType { get; set; }
        public string ErrorMessage { get; set; }
        public string Severity { get; set; } // Error, Warning, Info
    }
}

