
using Infrastructure.Converter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.Attribute;
using SqlSugar;
using ZR.Repository;
using ZR.Service.HR.IHrService;
using ZR.Service.HR.IService;
using ZR.Model.HR.Attendance;
using ZR.Model.HR.Payroll;
using ZR.Model.HR.EmployeeCore;

namespace ZR.Service.HR
{
    /// <summary>
    /// HR Payroll Service Implementation
    /// </summary>
    [AppService(ServiceType = typeof(IHrPayrollService), ServiceLifetime = LifeTime.Transient)]
    public class HrPayrollService : BaseService<HrPayroll>, IHrPayrollService
    {
        private readonly ISqlSugarClient _db;
        private readonly IEmployeeDataService _employeeDataService;

        public HrPayrollService(IEmployeeDataService employeeDataService)
        {
            _db = Context;
            _employeeDataService = employeeDataService;
        }

        #region Payroll Processing

        /// <summary>
        /// Calculate payroll for an employee
        /// </summary>
        public async Task<HrPayrollDetail> CalculatePayrollAsync(long employeeId, DateTime payPeriodStart, DateTime payPeriodEnd)
        {
            var employee = await _employeeDataService.GetEmployeeFullDataAsync(employeeId);
            if (employee == null) return null;

            var workingDays = GetWorkingDays(payPeriodStart, payPeriodEnd);
            var actualDays = await GetActualWorkingDaysAsync(employeeId, payPeriodStart, payPeriodEnd);

            var detail = new HrPayrollDetail
            {
                EmployeeId = employeeId,
                PayPeriodStart = payPeriodStart,
                PayPeriodEnd = payPeriodEnd,
                BasicSalary = await CalculateBasicSalaryAsync(employeeId, workingDays, actualDays),
                Allowances = await CalculateAllowancesAsync(employeeId),
                Bonus = await CalculateBonusAsync(employeeId, payPeriodStart),
                OvertimePay = await CalculateOvertimeAsync(employeeId, await GetOvertimeHoursAsync(employeeId, payPeriodStart, payPeriodEnd)),
                Deductions = await CalculateDeductionsAsync(employeeId),
                Status = "Calculated",
                CreatedDate = DateTime.Now
            };

            detail.GOSIEmployee = await CalculateGOSIAsync(detail.BasicSalary, employee.IsSaudi);
            detail.GOSIEmployer = detail.GOSIEmployee * 1.2m; // Employer contribution
            detail.Tax = await CalculateTaxAsync(detail.GrossSalary);
            detail.LoanDeduction = await CalculateLoanDeductionAsync(employeeId);
            
            detail.GrossSalary = detail.BasicSalary + detail.Allowances + detail.Bonus + detail.OvertimePay;
            detail.NetSalary = detail.GrossSalary - detail.Deductions - detail.GOSIEmployee - detail.Tax - detail.LoanDeduction;

            return detail;
        }

        /// <summary>
        /// Process batch payroll
        /// </summary>
        public async Task<List<HrPayrollDetail>> ProcessBatchPayrollAsync(DateTime payPeriodStart, DateTime payPeriodEnd)
        {
            var activeEmployees = await _employeeDataService.GetAllActiveEmployeesAsync();

            var payrollDetails = new List<HrPayrollDetail>();

            foreach (var employee in activeEmployees)
            {
                var detail = await CalculatePayrollAsync(employee.EmployeeId, payPeriodStart, payPeriodEnd);
                if (detail != null)
                {
                    payrollDetails.Add(detail);
                }
            }

            return payrollDetails;
        }

        /// <summary>
        /// Approve payroll
        /// </summary>
        public async Task<bool> ApprovePayrollAsync(long payrollId, long approvedBy)
        {
            var payroll = await GetFirstAsync(p => p.PayrollId == payrollId);
            if (payroll == null) return false;

            payroll.Status = "Approved";
            payroll.ApprovedBy = approvedBy;
            payroll.ApprovedDate = DateTime.Now;
            payroll.Update_time = DateTime.Now;

            return await UpdateAsync(payroll);
        }

        /// <summary>
        /// Reject payroll
        /// </summary>
        public async Task<bool> RejectPayrollAsync(long payrollId, long rejectedBy, string reason)
        {
            var payroll = await GetFirstAsync(p => p.PayrollId == payrollId);
            if (payroll == null) return false;

            payroll.Status = "Rejected";
            payroll.ApprovedBy = rejectedBy;
            payroll.ApprovedDate = DateTime.Now;
            payroll.Remark = reason;
            payroll.Update_time = DateTime.Now;

            return await UpdateAsync(payroll);
        }

        /// <summary>
        /// Finalize payroll
        /// </summary>
        public async Task<bool> FinalizePayrollAsync(long payrollId)
        {
            var payroll = await GetFirstAsync(p => p.PayrollId == payrollId);
            if (payroll == null || payroll.Status != "Approved") return false;

            payroll.Status = "Finalized";
            payroll.FinalizedDate = DateTime.Now;
            payroll.Update_time = DateTime.Now;

            return await UpdateAsync(payroll);
        }

        /// <summary>
        /// Reverse payroll
        /// </summary>
        public async Task<bool> ReversePayrollAsync(long payrollId, string reason)
        {
            var payroll = await GetFirstAsync(p => p.PayrollId == payrollId);
            if (payroll == null) return false;

            payroll.Status = "Reversed";
            payroll.Remark = reason;
            payroll.Update_time = DateTime.Now;

            return await UpdateAsync(payroll);
        }

        #endregion

        #region Payroll Details

        /// <summary>
        /// Get payroll detail
        /// </summary>
        public async Task<HrPayrollDetail> GetPayrollDetailAsync(long payrollId)
        {
            return await _db.Queryable<HrPayrollDetail>()
                .FirstAsync(pd => pd.PayrollId == payrollId);
        }

        /// <summary>
        /// Get employee payroll history
        /// </summary>
        public async Task<List<HrPayrollDetail>> GetEmployeePayrollHistoryAsync(long employeeId, int year)
        {
            return await _db.Queryable<HrPayrollDetail>()
                .Where(pd => pd.EmployeeId == employeeId && pd.PayPeriodStart.Year == year)
                .OrderBy(pd => pd.PayPeriodStart, OrderByType.Desc)
                .ToListAsync();
        }

        /// <summary>
        /// Get payroll by period
        /// </summary>
        public async Task<List<HrPayrollDetail>> GetPayrollByPeriodAsync(DateTime startDate, DateTime endDate)
        {
            return await _db.Queryable<HrPayrollDetail>()
                .Where(pd => pd.PayPeriodStart >= startDate && pd.PayPeriodEnd <= endDate)
                .ToListAsync();
        }

        /// <summary>
        /// Update payroll detail
        /// </summary>
        public async Task<bool> UpdatePayrollDetailAsync(HrPayrollDetail detail)
        {
            detail.Update_time= DateTime.Now;
            return await _db.Updateable(detail).ExecuteCommandAsync() > 0;
        }

        #endregion

        #region Salary Components

        private async Task<HrPayroll> GetLatestPayrollSnapshotAsync(long employeeId)
        {
            return await _db.Queryable<HrPayroll>()
                .Where(p => p.EmployeeId == employeeId)
                .OrderBy(p => p.PayPeriod, OrderByType.Desc)
                .FirstAsync();
        }

        /// <summary>
        /// Calculate basic salary
        /// </summary>
        public async Task<decimal> CalculateBasicSalaryAsync(long employeeId, int workingDays, int actualDays)
        {
            var payroll = await GetLatestPayrollSnapshotAsync(employeeId);
            if (payroll == null || workingDays <= 0) return 0;

            var dailyRate = payroll.BasicSalary / workingDays;
            return dailyRate * actualDays;
        }

        /// <summary>
        /// Calculate overtime
        /// </summary>
        public async Task<decimal> CalculateOvertimeAsync(long employeeId, decimal overtimeHours)
        {
            var payroll = await GetLatestPayrollSnapshotAsync(employeeId);
            if (payroll == null) return 0;

            var hourlyRate = payroll.BasicSalary / 240m; // Assuming 240 working hours per month
            return hourlyRate * overtimeHours * 1.5m; // 1.5x for overtime
        }

        /// <summary>
        /// Calculate allowances
        /// </summary>
        public async Task<decimal> CalculateAllowancesAsync(long employeeId)
        {
            var payroll = await GetLatestPayrollSnapshotAsync(employeeId);
            if (payroll == null) return 0;

            return payroll.HousingAllowance + payroll.TransportAllowance + payroll.OtherAllowances;
        }

        /// <summary>
        /// Calculate bonus
        /// </summary>
        public async Task<decimal> CalculateBonusAsync(long employeeId, DateTime period)
        {
            // Check for any bonuses in the period
            var bonus = await _db.Queryable<HrBonus>()
                .Where(b => b.EmployeeId == employeeId && 
                           b.BonusDate >= period && 
                           b.BonusDate <= period.AddMonths(1))
                .SumAsync(b => b.Amount);

            return bonus;
        }

        /// <summary>
        /// Calculate deductions
        /// </summary>
        public async Task<decimal> CalculateDeductionsAsync(long employeeId)
        {
            // Calculate deductions based on attendance and other factors
            var deductions = await _db.Queryable<HrDeduction>()
                .Where(d => d.EmployeeId == employeeId && d.IsActive)
                .SumAsync(d => d.Amount);

            return deductions;
        }

        /// <summary>
        /// Calculate GOSI
        /// </summary>
        public async Task<decimal> CalculateGOSIAsync(decimal salary, bool isSaudi)
        {
            if (isSaudi)
            {
                return salary * 0.10m; // 10% for Saudi nationals
            }
            else
            {
                return salary * 0.02m; // 2% for expatriates
            }
        }

        /// <summary>
        /// Calculate tax - Saudi Arabia has no personal income tax
        /// </summary>
        public async Task<decimal> CalculateTaxAsync(decimal grossSalary)
        {
            return await Task.FromResult(0m);
        }

        #endregion

        #region Loan Management

        /// <summary>
        /// Get employee loans
        /// </summary>
        public async Task<List<HrLoan>> GetEmployeeLoansAsync(long employeeId)
        {
            return await _db.Queryable<HrLoan>()
                .Where(l => l.EmployeeId == employeeId)
                .ToListAsync();
        }

        /// <summary>
        /// Add loan
        /// </summary>
        public async Task<int> AddLoanAsync(HrLoan loan)
        {
            loan.Status = "Active";
            loan.Create_time = DateTime.Now;
            return await _db.Insertable(loan).ExecuteCommandAsync();
        }

        /// <summary>
        /// Update loan
        /// </summary>
        public async Task<bool> UpdateLoanAsync(HrLoan loan)
        {
            loan.Update_time = DateTime.Now;
            return await _db.Updateable(loan).ExecuteCommandAsync() > 0;
        }

        /// <summary>
        /// Calculate loan deduction
        /// </summary>
        public async Task<decimal> CalculateLoanDeductionAsync(long employeeId)
        {
            var activeLoans = await _db.Queryable<HrLoan>()
                .Where(l => l.EmployeeId == employeeId && l.Status == "Active")
                .ToListAsync();

            return activeLoans.Sum(l => l.MonthlyDeduction);
        }

        /// <summary>
        /// Process loan payment
        /// </summary>
        public async Task<bool> ProcessLoanPaymentAsync(long loanId, decimal amount)
        {
            var loan = await _db.Queryable<HrLoan>()
                .FirstAsync(l => l.LoanId == loanId);
            
            if (loan == null) return false;

            loan.PaidAmount += amount;
            loan.RemainingAmount -= amount;
            
            if (loan.RemainingAmount <= 0)
            {
                loan.Status = "Completed";
                loan.CompletedDate = DateTime.Now;
            }

            loan.Update_time = DateTime.Now;
            return await _db.Updateable(loan).ExecuteCommandAsync() > 0;
        }

        #endregion

        #region Bank Transfer

        /// <summary>
        /// Generate bank file
        /// </summary>
        public async Task<bool> GenerateBankFileAsync(List<long> payrollIds)
        {
            // This would generate a bank transfer file in the required format
            // Implementation depends on bank requirements
            return await Task.FromResult(true);
        }

        /// <summary>
        /// Mark as transferred
        /// </summary>
        public async Task<bool> MarkAsTransferredAsync(List<long> payrollIds, string referenceNumber)
        {
            var payrolls = await GetListAsync(p => payrollIds.Contains(p.PayrollId));
            
            foreach (var payroll in payrolls)
            {
                payroll.Status = "Transferred";
                payroll.TransferReference = referenceNumber;
                payroll.TransferDate = DateTime.Now;
                payroll.Update_time = DateTime.Now;
            }

            return await UpdateRangeAsync(payrolls) > 0;
        }

        #endregion

        #region Reports

        /// <summary>
        /// Generate payslip
        /// </summary>
        public async Task<byte[]> GeneratePayslipAsync(long payrollId)
        {
            // Generate PDF payslip
            return await Task.FromResult(new byte[0]);
        }

        /// <summary>
        /// Generate salary report
        /// </summary>
        public async Task<byte[]> GenerateSalaryReportAsync(DateTime period)
        {
            // Generate salary report for the period
            return await Task.FromResult(new byte[0]);
        }

        /// <summary>
        /// Generate GOSI report
        /// </summary>
        public async Task<byte[]> GenerateGOSIReportAsync(DateTime period)
        {
            // Generate GOSI report
            return await Task.FromResult(new byte[0]);
        }

        /// <summary>
        /// Generate tax report
        /// </summary>
        public async Task<byte[]> GenerateTaxReportAsync(int year)
        {
            // Generate annual tax report
            return await Task.FromResult(new byte[0]);
        }

        #endregion

        #region Helper Methods

        private int GetWorkingDays(DateTime startDate, DateTime endDate)
        {
            var days = 0;
            var current = startDate;
            
            while (current <= endDate)
            {
                if (current.DayOfWeek != DayOfWeek.Friday && current.DayOfWeek != DayOfWeek.Saturday)
                {
                    days++;
                }
                current = current.AddDays(1);
            }
            
            return days;
        }

        private async Task<int> GetActualWorkingDaysAsync(long employeeId, DateTime startDate, DateTime endDate)
        {
            var attendance = await _db.Queryable<ZR.Model.HR.Attendance.AttendanceProcessing.HrAttendanceDay>()
                .Where(a => a.EmployeeId == employeeId &&
                           a.AttendanceDate >= startDate &&
                           a.AttendanceDate <= endDate &&
                           !a.IsAbsent)
                .CountAsync();

            return attendance;
        }

        private async Task<decimal> GetOvertimeHoursAsync(long employeeId, DateTime startDate, DateTime endDate)
        {
            var overtimeHours = await _db.Queryable<ZR.Model.HR.Attendance.AttendanceProcessing.HrAttendanceDay>()
                .Where(a => a.EmployeeId == employeeId &&
                           a.AttendanceDate >= startDate &&
                           a.AttendanceDate <= endDate)
                .SumAsync(a => a.OvertimeHours);
            
            return overtimeHours;
        }

        #endregion
    }

    // Supporting classes
 

    public class HrBonus
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long BonusId { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }
        public decimal Amount { get; set; }
        public DateTime BonusDate { get; set; }
        public string Reason { get; set; }
    }


    public class HrDeduction
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long DeductionId { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }
        public decimal Amount { get; set; }
        public string Reason { get; set; }
        public bool IsActive { get; set; }
    }
}

