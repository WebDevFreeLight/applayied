using ZR.Model.HR.EmployeeCore;

using Infrastructure.Converter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Infrastructure.Attribute;
using SqlSugar;
using ZR.Model;
using ZR.Model.HR.Dto;
using ZR.Repository;
using ZR.Service.HR.IService;
using ZR.ServiceCore;
using ZR.Model.HR.Payroll;
using ZR.Model.Finance.GL;
using ZR.Model.Finance.Configuration;
using ZR.Model.Finance.Enums;
using ZR.Model.HR.Finance;
using ZR.Model.HR.Dto.EmployeeCore;

namespace ZR.Service.HR
{

    public class LoanTypeSummaryDto
    {
        public string LoanType { get; set; }
        public int TotalCount { get; set; }
        public decimal TotalAmount { get; set; }
        public int ActiveCount { get; set; }
        public decimal AverageAmount { get; set; }
    }

    public class DefaultedLoanDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long LoanId { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public decimal LoanAmount { get; set; }
        public decimal OutstandingAmount { get; set; }
        public decimal MonthlyDeduction { get; set; }
        public DateTime? LastPaymentDate { get; set; }
        public int DaysOverdue { get; set; }
    }


    /// <summary>
    /// Loan Service Implementation
    /// Provides comprehensive employee loan management including loan applications, approvals, repayments, and analytics
    /// </summary>
    [AppService(ServiceType = typeof(ILoanService), ServiceLifetime = LifeTime.Transient)]
    public class LoanService : BaseService<HrLoan>, ILoanService
    {
        private readonly IEmployeeDataService _employeeDataService;

        public LoanService(IEmployeeDataService employeeDataService)
        {
            _employeeDataService = employeeDataService;
        }
        // Loan Management
        public async Task<PagedInfo<HrLoan>> GetLoansAsync(Expression<Func<HrLoan, bool>> predicate, PagerInfo pager)
        {
            var query = Context.Queryable<HrLoan>()
                .LeftJoin<HrEmployeeCore>((l, e) => l.EmployeeId == e.EmployeeId)
                .Where(predicate)
                .Select((l, e) => new HrLoan
                {
                    LoanId = l.LoanId,
                    EmployeeId = l.EmployeeId,
                    LoanType = l.LoanType,
                    LoanAmount = l.LoanAmount,
                    InterestRate = l.InterestRate,
                    RepaymentMonths = l.RepaymentMonths,
                    MonthlyDeduction = l.MonthlyDeduction,
                    LoanDate = l.LoanDate,
                    StartDeductionDate = l.StartDeductionDate,
                    EndDeductionDate = l.EndDeductionDate,
                    TotalRepaid = l.TotalRepaid,
                    OutstandingAmount = l.OutstandingAmount,
                    Status = l.Status,
                    ApprovedBy = l.ApprovedBy,
                    ApprovalDate = l.ApprovalDate,
                    Purpose = l.Purpose,
                    Remarks = l.Remarks,
                    AttachmentUrl = l.AttachmentUrl,
                    IsActive = l.IsActive,
                    PaidAmount = l.PaidAmount,
                    RemainingAmount = l.RemainingAmount,
                    CompletedDate = l.CompletedDate
                })
                .OrderBy(l => l.Create_time, OrderByType.Desc);

            return await query.ToPageAsync(pager);
        }

        public async Task<List<HrLoan>> GetEmployeeLoansAsync(long employeeId)
        {
            return await Context.Queryable<HrLoan>()
                .Where(l => l.EmployeeId == employeeId && l.IsActive)
                .OrderBy(l => l.Status == "active" ? 0 : 1)
                .OrderBy(l => l.Create_time, OrderByType.Desc)
                .ToListAsync();
        }

        public async Task<List<HrLoan>> GetActiveLoansAsync()
        {
            return await Context.Queryable<HrLoan>()
                .Where(l => l.Status == "active" && l.IsActive)
                .OrderBy(l => l.LoanDate, OrderByType.Desc)
                .ToListAsync();
        }

        public async Task<HrLoan> GetLoanByIdAsync(long loanId)
        {
            return await Context.Queryable<HrLoan>()
                .FirstAsync(l => l.LoanId == loanId);
        }

        public async Task<int> ApplyForLoanAsync(HrLoan loan)
        {
            // Validate loan application data
            if (loan.EmployeeId <= 0)
                throw new ArgumentException("Employee ID is required");

            if (string.IsNullOrEmpty(loan.LoanType))
                throw new ArgumentException("Loan type is required");

            if (loan.LoanAmount <= 0)
                throw new ArgumentException("Loan amount must be greater than zero");

            if (loan.RepaymentMonths <= 0)
                throw new ArgumentException("Repayment months must be greater than zero");

            // Check employee eligibility
            var isEligible = await CheckLoanEligibilityAsync(loan.EmployeeId, loan.LoanType);
            if (!isEligible)
                throw new InvalidOperationException("Employee is not eligible for this loan type");

            // Calculate maximum allowed loan amount
            var maxAmount = await GetMaxLoanAmountAsync(loan.EmployeeId, loan.LoanType);
            if (loan.LoanAmount > maxAmount)
                throw new InvalidOperationException($"Loan amount exceeds maximum allowed amount of {maxAmount:C}");

            // Calculate monthly deduction
            loan.MonthlyDeduction = CalculateMonthlyPayment(loan.LoanAmount, loan.InterestRate, loan.RepaymentMonths);
            
            loan.OutstandingAmount = loan.LoanAmount;
            loan.RemainingAmount = loan.LoanAmount;
            loan.TotalRepaid = 0;
            loan.PaidAmount = 0;
            loan.Status = "pending";
            loan.LoanDate = DateTime.Now;
            loan.IsActive = true;
            loan.Create_time = DateTime.Now;

            return await Context.Insertable(loan).ExecuteReturnIdentityAsync();
        }

        public async Task<int> UpdateLoanAsync(HrLoan loan)
        {
            // Recalculate derived fields if core values changed
            if (loan.LoanAmount > 0 && loan.RepaymentMonths > 0)
            {
                loan.MonthlyDeduction = CalculateMonthlyPayment(loan.LoanAmount, loan.InterestRate, loan.RepaymentMonths);
            }

            loan.Update_time = DateTime.Now;
            return await Context.Updateable(loan)
                .IgnoreColumns(l => new { l.Create_time, l.Create_by, l.LoanDate })
                .ExecuteCommandAsync();
        }

        public async Task<int> CancelLoanAsync(long loanId, string reason)
        {
            // Check if loan can be cancelled
            var loan = await GetLoanByIdAsync(loanId);
            if (loan == null)
                throw new ArgumentException("Loan not found");

            if (loan.Status == "active" && loan.TotalRepaid > 0)
                throw new InvalidOperationException("Cannot cancel loan with existing repayments");

            return await Context.Updateable<HrLoan>()
                .SetColumns(l => new HrLoan
                {
                    Status = "cancelled",
                    Remarks = reason,
                    Update_time = DateTime.Now
                })
                .Where(l => l.LoanId == loanId)
                .ExecuteCommandAsync();
        }

        // Loan Approval
        public async Task<int> ApproveLoanAsync(long loanId, long approvedBy, string comments)
        {
            var loan = await GetLoanByIdAsync(loanId);
            if (loan == null)
                throw new ArgumentException("Loan not found");

            if (loan.Status != "pending")
                throw new InvalidOperationException("Only pending loans can be approved");

            // Validate approver authority (this would typically check user roles/permissions)
            
            // Set deduction start date (typically next month)
            var startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(1);
            var endDate = startDate.AddMonths(loan.RepaymentMonths - 1);

            return await Context.Updateable<HrLoan>()
                .SetColumns(l => new HrLoan
                {
                    Status = "active",
                    ApprovedBy = approvedBy,
                    ApprovalDate = DateTime.Now,
                    StartDeductionDate = startDate,
                    EndDeductionDate = endDate,
                    Remarks = comments,
                    Update_time = DateTime.Now
                })
                .Where(l => l.LoanId == loanId)
                .ExecuteCommandAsync();
        }

        public async Task<int> RejectLoanAsync(long loanId, long rejectedBy, string reason)
        {
            var loan = await GetLoanByIdAsync(loanId);
            if (loan == null)
                throw new ArgumentException("Loan not found");

            if (loan.Status != "pending")
                throw new InvalidOperationException("Only pending loans can be rejected");

            return await Context.Updateable<HrLoan>()
                .SetColumns(l => new HrLoan
                {
                    Status = "rejected",
                    ApprovedBy = rejectedBy,
                    ApprovalDate = DateTime.Now,
                    Remarks = reason,
                    Update_time = DateTime.Now
                })
                .Where(l => l.LoanId == loanId)
                .ExecuteCommandAsync();
        }

        public async Task<List<HrLoan>> GetPendingLoansAsync()
        {
            return await Context.Queryable<HrLoan>()
                .LeftJoin<HrEmployeeCore>((l, e) => l.EmployeeId == e.EmployeeId)
                .Where((l, e) => l.Status == "pending" && l.IsActive)
                .OrderBy(l => l.Create_time)
                .Select(l => l)
                .ToListAsync();
        }

        public async Task<List<HrLoan>> GetLoansForApprovalAsync(long approverId)
        {
            // This would typically filter based on approval hierarchy/department
            return await GetPendingLoansAsync();
        }

        // Loan Repayment
        public async Task<List<HrLoanRepayment>> GetLoanRepaymentsAsync(long loanId)
        {
            return await Context.Queryable<HrLoanRepayment>()
                .Where(r => r.LoanId == loanId)
                .OrderBy(r => r.RepaymentDate, OrderByType.Desc)
                .ToListAsync();
        }

        public async Task<List<HrLoanRepayment>> GetEmployeeRepaymentsAsync(long employeeId, string payPeriod)
        {
            var query = Context.Queryable<HrLoanRepayment>()
                .Where(r => r.EmployeeId == employeeId);

            if (!string.IsNullOrEmpty(payPeriod))
                query = query.Where(r => r.PayPeriod == payPeriod);

            return await query.OrderBy(r => r.RepaymentDate, OrderByType.Desc).ToListAsync();
        }

        public async Task<int> ProcessRepaymentAsync(HrLoanRepayment repayment)
        {
            // Validate repayment data
            if (repayment.LoanId <= 0)
                throw new ArgumentException("Loan ID is required");

            if (repayment.RepaymentAmount <= 0)
                throw new ArgumentException("Repayment amount must be greater than zero");

            var loan = await GetLoanByIdAsync(repayment.LoanId);
            if (loan == null)
                throw new ArgumentException("Loan not found");

            if (loan.Status != "active")
                throw new InvalidOperationException("Cannot process repayment for inactive loan");

            // Calculate principal and interest portions
            var monthlyInterest = loan.OutstandingAmount * (loan.InterestRate / 100) / 12;
            repayment.InterestAmount = monthlyInterest;
            repayment.PrincipalAmount = repayment.RepaymentAmount - monthlyInterest;

            // Update outstanding balance
            var newOutstanding = loan.OutstandingAmount - repayment.PrincipalAmount;
            repayment.OutstandingBalance = Math.Max(0, newOutstanding);

            repayment.Status = "paid";
            repayment.Create_time = DateTime.Now;

            // Insert repayment record
            var repaymentId = await Context.Insertable(repayment).ExecuteReturnIdentityAsync();

            // Update loan with new balances
            var newTotalRepaid = loan.TotalRepaid + repayment.RepaymentAmount;
            var newPaidAmount = loan.PaidAmount + repayment.RepaymentAmount;
            var newRemainingAmount = loan.LoanAmount - newPaidAmount;

            var loanUpdateResult = await Context.Updateable<HrLoan>()
                .SetColumns(l => new HrLoan
                {
                    TotalRepaid = newTotalRepaid,
                    PaidAmount = newPaidAmount,
                    RemainingAmount = Math.Max(0, newRemainingAmount),
                    OutstandingAmount = repayment.OutstandingBalance,
                    Update_time = DateTime.Now
                })
                .Where(l => l.LoanId == repayment.LoanId)
                .ExecuteCommandAsync();

            // Check if loan is fully repaid
            if (repayment.OutstandingBalance <= 0.01m) // Allow for small rounding differences
            {
                await Context.Updateable<HrLoan>()
                    .SetColumns(l => new HrLoan
                    {
                        Status = "completed",
                        CompletedDate = DateTime.Now,
                        OutstandingAmount = 0,
                        RemainingAmount = 0,
                        Update_time = DateTime.Now
                    })
                    .Where(l => l.LoanId == repayment.LoanId)
                    .ExecuteCommandAsync();
            }

            return repaymentId;
        }

        public async Task<int> ProcessRepayment(long repaymentId)
        {
            var result = await Context.Ado.UseTranAsync(async () =>
            {
                var repayment = await Context.Queryable<HrLoanRepayment>()
                    .FirstAsync(r => r.RepaymentId == repaymentId);

                if (repayment == null)
                    throw new ArgumentException($"Repayment not found: {repaymentId}");

                if (repayment.GLJournalId.HasValue)
                    return 1;

                var baseCurrency = await Context.Queryable<Currency>()
                    .FirstAsync(c => c.IsBaseCurrency && c.IsActive);

                var journal = new GLJournal
                {
                    Id = Guid.NewGuid(),
                    JournalNumber = $"HRLOAN-{repaymentId}",
                    JournalDate = DateTime.Now,
                    Status = JournalStatus.Draft,
                    Source = "hr_loan_repayment",
                    SourceDocumentType = "hr_loan_repayment",
                    SourceDocumentId = null,
                    TotalDebit = repayment.RepaymentAmount,
                    TotalCredit = repayment.RepaymentAmount,
                    Currency = baseCurrency?.Code ?? "SAR",
                    CurrencyId = baseCurrency?.Id,
                    ExchangeRate = 1.0m,
                    ReferenceNumber = $"HR-LOAN-RP-{repaymentId}",
                    Create_time = DateTime.Now
                };

                await Context.Insertable(journal).ExecuteCommandAsync();

                repayment.GLJournalId = journal.Id;
                repayment.Update_time = DateTime.Now;
                await Context.Updateable(repayment)
                    .UpdateColumns(r => new { r.GLJournalId, r.Update_time })
                    .Where(r => r.RepaymentId == repaymentId)
                    .ExecuteCommandAsync();

                var audit = new HrFinancialAuditLog
                {
                    LogId = Guid.NewGuid(),
                    Action = "ProcessLoanRepayment",
                    UserId = 0,
                    Timestamp = DateTime.Now,
                    Changes = $"{{\"repaymentId\":{repaymentId},\"loanId\":{repayment.LoanId},\"amount\":{repayment.RepaymentAmount},\"glJournalId\":\"{journal.Id}\"}}",
                    Create_time = DateTime.Now
                };
                await Context.Insertable(audit).ExecuteCommandAsync();

                return 1;
            });

            if (!result.IsSuccess)
                throw result.ErrorException;

            return (int)result.Data;
        }

        public async Task<int> ProcessMonthlyRepaymentAsync(string payPeriod)
        {
            var processedCount = 0;
            var payPeriodDate = DateTime.ParseExact(payPeriod, "yyyy-MM", null);
            
            // Get all active loans that should have deductions this period
            var activeLoans = await Context.Queryable<HrLoan>()
                .Where(l => l.Status == "active" && 
                           l.StartDeductionDate <= payPeriodDate &&
                           (l.EndDeductionDate == null || l.EndDeductionDate >= payPeriodDate) &&
                           l.OutstandingAmount > 0)
                .ToListAsync();

            foreach (var loan in activeLoans)
            {
                // Check if repayment already processed for this period
                var existingRepayment = await Context.Queryable<HrLoanRepayment>()
                    .AnyAsync(r => r.LoanId == loan.LoanId && r.PayPeriod == payPeriod);

                if (!existingRepayment)
                {
                    var repaymentAmount = Math.Min(loan.MonthlyDeduction, loan.OutstandingAmount);
                    
                    var repayment = new HrLoanRepayment
                    {
                        LoanId = loan.LoanId,
                        EmployeeId = loan.EmployeeId,
                        PayPeriod = payPeriod,
                        RepaymentAmount = repaymentAmount,
                        RepaymentDate = DateTime.Now,
                        PaymentMethod = "salary_deduction"
                    };

                    await ProcessRepaymentAsync(repayment);
                    processedCount++;
                }
            }

            return processedCount;
        }

        public async Task<decimal> CalculateMonthlyRepaymentAsync(long loanId)
        {
            var loan = await GetLoanByIdAsync(loanId);
            if (loan == null)
                return 0;

            return CalculateMonthlyPayment(loan.OutstandingAmount, loan.InterestRate, 
                GetRemainingMonths(loan.StartDeductionDate, loan.EndDeductionDate ?? DateTime.Now));
        }

        public async Task<decimal> GetOutstandingBalanceAsync(long loanId)
        {
            var loan = await GetLoanByIdAsync(loanId);
            return loan?.OutstandingAmount ?? 0;
        }

        public async Task<int> RecalculateRepaymentScheduleAsync(long loanId)
        {
            var loan = await GetLoanByIdAsync(loanId);
            if (loan == null)
                throw new ArgumentException("Loan not found");

            // Get all processed repayments
            var repayments = await GetLoanRepaymentsAsync(loanId);
            var totalPaid = repayments.Where(r => r.Status == "paid").Sum(r => r.RepaymentAmount);
            
            // Recalculate outstanding balance
            var newOutstanding = loan.LoanAmount - totalPaid;
            
            // Recalculate monthly payment for remaining months
            var remainingMonths = GetRemainingMonths(DateTime.Now, loan.EndDeductionDate ?? DateTime.Now.AddMonths(loan.RepaymentMonths));
            var newMonthlyPayment = remainingMonths > 0 ? CalculateMonthlyPayment(newOutstanding, loan.InterestRate, remainingMonths) : 0;

            return await Context.Updateable<HrLoan>()
                .SetColumns(l => new HrLoan
                {
                    OutstandingAmount = Math.Max(0, newOutstanding),
                    RemainingAmount = Math.Max(0, newOutstanding),
                    TotalRepaid = totalPaid,
                    PaidAmount = totalPaid,
                    MonthlyDeduction = newMonthlyPayment,
                    Update_time = DateTime.Now
                })
                .Where(l => l.LoanId == loanId)
                .ExecuteCommandAsync();
        }

        // Early Settlement
        public async Task<decimal> CalculateEarlySettlementAmountAsync(long loanId)
        {
            var loan = await GetLoanByIdAsync(loanId);
            if (loan == null)
                return 0;

            // For early settlement, typically only the principal amount is required
            // Some institutions may add a penalty or discount
            var settlementAmount = loan.OutstandingAmount;
            
            // Apply early settlement discount if applicable (e.g., waive future interest)
            // This would be based on business rules
            
            return settlementAmount;
        }

        public async Task<int> ProcessEarlySettlementAsync(long loanId, decimal amount, string paymentMethod)
        {
            var loan = await GetLoanByIdAsync(loanId);
            if (loan == null)
                throw new ArgumentException("Loan not found");

            if (loan.Status != "active")
                throw new InvalidOperationException("Only active loans can be settled early");

            var expectedAmount = await CalculateEarlySettlementAmountAsync(loanId);
            if (amount < expectedAmount)
                throw new ArgumentException($"Settlement amount is insufficient. Expected: {expectedAmount:C}");

            // Create final repayment record
            var repayment = new HrLoanRepayment
            {
                LoanId = loanId,
                EmployeeId = loan.EmployeeId,
                PayPeriod = DateTime.Now.ToString("yyyy-MM"),
                RepaymentAmount = amount,
                RepaymentDate = DateTime.Now,
                PaymentMethod = paymentMethod,
                PrincipalAmount = amount,
                InterestAmount = 0,
                OutstandingBalance = 0,
                Status = "paid"
            };

            await ProcessRepaymentAsync(repayment);

            // Mark loan as completed
            return await Context.Updateable<HrLoan>()
                .SetColumns(l => new HrLoan
                {
                    Status = "completed",
                    CompletedDate = DateTime.Now,
                    OutstandingAmount = 0,
                    RemainingAmount = 0,
                    Remarks = "Early settlement completed",
                    Update_time = DateTime.Now
                })
                .Where(l => l.LoanId == loanId)
                .ExecuteCommandAsync();
        }

        // Loan Analytics
        public async Task<object> GetLoanStatisticsAsync(DateTime startDate, DateTime endDate)
        {
            var loans = await Context.Queryable<HrLoan>()
                .Where(l => l.LoanDate >= startDate && l.LoanDate <= endDate)
                .ToListAsync();

            var repayments = await Context.Queryable<HrLoanRepayment>()
                .Where(r => r.RepaymentDate >= startDate && r.RepaymentDate <= endDate && r.Status == "paid")
                .ToListAsync();

            return new
            {
                Period = new { StartDate = startDate, EndDate = endDate },
                TotalLoans = loans.Count,
                TotalLoanAmount = loans.Sum(l => l.LoanAmount),
                ActiveLoans = loans.Count(l => l.Status == "active"),
                CompletedLoans = loans.Count(l => l.Status == "completed"),
                PendingLoans = loans.Count(l => l.Status == "pending"),
                RejectedLoans = loans.Count(l => l.Status == "rejected"),
                TotalRepayments = repayments.Sum(r => r.RepaymentAmount),
                AverageLoanAmount = loans.Any() ? loans.Average(l => l.LoanAmount) : 0,
                LoansByType = loans.GroupBy(l => l.LoanType).Select(g => new
                {
                    Type = g.Key,
                    Count = g.Count(),
                    TotalAmount = g.Sum(l => l.LoanAmount)
                }).ToList(),
                MonthlyTrends = loans.GroupBy(l => new { l.LoanDate.Year, l.LoanDate.Month }).Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Count = g.Count(),
                    Amount = g.Sum(l => l.LoanAmount)
                }).OrderBy(t => t.Year).ThenBy(t => t.Month).ToList()
            };
        }

        public async Task<object> GetEmployeeLoanSummaryAsync(long employeeId)
        {
            var loans = await GetEmployeeLoansAsync(employeeId);
            var repayments = await Context.Queryable<HrLoanRepayment>()
                .Where(r => r.EmployeeId == employeeId && r.Status == "paid")
                .ToListAsync();

            var activeLoans = loans.Where(l => l.Status == "active").ToList();

            return new
            {
                EmployeeId = employeeId,
                TotalLoans = loans.Count,
                ActiveLoans = activeLoans.Count,
                CompletedLoans = loans.Count(l => l.Status == "completed"),
                TotalLoanAmount = loans.Sum(l => l.LoanAmount),
                TotalRepaid = repayments.Sum(r => r.RepaymentAmount),
                CurrentOutstanding = activeLoans.Sum(l => l.OutstandingAmount),
                MonthlyDeductions = activeLoans.Sum(l => l.MonthlyDeduction),
                LoansHistory = loans.OrderBy(l => l.LoanDate
                
                //, OrderByType.Desc
                
                ).Take(10).ToList()
            };
        }

        public async Task<decimal> GetTotalOutstandingLoansAsync()
        {
            return await Context.Queryable<HrLoan>()
                .Where(l => l.Status == "active" && l.IsActive)
                .SumAsync(l => l.OutstandingAmount);
        }



        
        public async Task<List<LoanTypeSummaryDto>> GetLoansByTypeAsync()
        {

            var data = await Context.Queryable<HrLoan>()
                .Where(l => l.IsActive)
                .GroupBy(l => l.LoanType)
                .Select(g => new LoanTypeSummaryDto()
                {
                    LoanType = g.LoanType,
                    TotalCount = SqlFunc.AggregateCount(g.LoanId),
                    TotalAmount = SqlFunc.AggregateSum(g.LoanAmount),
                    ActiveCount = SqlFunc.AggregateCount(SqlFunc.IIF(g.Status == "active", g.LoanId, 0)),
                    AverageAmount = SqlFunc.AggregateAvg(g.LoanAmount)
                })
                .ToListAsync ();



            return data;
        }


        public async Task<List<DefaultedLoanDto>> GetDefaultedLoansAsync()
        {
            // Loans that are overdue by more than 30 days
            var overdueDate = DateTime.Now.AddDays(-30);

            // Get active loans with outstanding amounts
            var activeLoans = await Context.Queryable<HrLoan>()
                .Where(l => l.Status == "active" && l.OutstandingAmount > 0)
                .ToListAsync();

            if (!activeLoans.Any())
                return new List<DefaultedLoanDto>();

            // Get repayments for these loans
            var loanIds = activeLoans.Select(l => l.LoanId).ToList();
            var repayments = await Context.Queryable<HrLoanRepayment>()
                .Where(r => loanIds.Contains(r.LoanId))
                .ToListAsync();

            // Group repayments by loan ID and get last payment date
            var lastPaymentByLoan = repayments
                .GroupBy(r => r.LoanId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Max(r => r.RepaymentDate)
                );

            // Filter loans that are overdue
            var overdueLoans = activeLoans.Where(l =>
            {
                if (!lastPaymentByLoan.ContainsKey(l.LoanId))
                    return l.StartDeductionDate < overdueDate; // No payment ever made

                return lastPaymentByLoan[l.LoanId] < overdueDate;
            }).ToList();

            if (!overdueLoans.Any())
                return new List<DefaultedLoanDto>();

            // Batch load employee data
            var employeeIds = overdueLoans.Select(l => l.EmployeeId).Distinct().ToList();
            var employees = await _employeeDataService.GetEmployeesFullDataAsync(employeeIds);
            var employeeLookup = employees.ToDictionary(e => e.EmployeeId);

            // Build result
            var result = overdueLoans.Select(l =>
            {
                var lastPayment = lastPaymentByLoan.ContainsKey(l.LoanId)
                    ? lastPaymentByLoan[l.LoanId]
                    : (DateTime?)null;

                // Get reference date - use last payment if available, else start deduction date
                DateTime referenceDate = lastPayment ?? l.StartDeductionDate;
                var daysOverdue = (DateTime.Now - referenceDate).Days;

                return new DefaultedLoanDto
                {
                    LoanId = l.LoanId,
                    EmployeeId = l.EmployeeId,
                    EmployeeName = employeeLookup.ContainsKey(l.EmployeeId)
                        ? (employeeLookup[l.EmployeeId].NameAr ?? employeeLookup[l.EmployeeId].FullName)
                        : "Unknown",
                    LoanAmount = l.LoanAmount,
                    OutstandingAmount = l.OutstandingAmount,
                    MonthlyDeduction = l.MonthlyDeduction,
                    LastPaymentDate = lastPayment,
                    DaysOverdue = daysOverdue
                };
            }).ToList();

            return result;
        }

        public async Task<object> GetRepaymentAnalyticsAsync(int year)
        {
            var repayments = await Context.Queryable<HrLoanRepayment>()
                .Where(r => r.RepaymentDate.Year == year && r.Status == "paid")
                .ToListAsync();

            return new
            {
                Year = year,
                TotalRepayments = repayments.Sum(r => r.RepaymentAmount),
                MonthlyBreakdown = repayments.GroupBy(r => r.RepaymentDate.Month).Select(g => new
                {
                    Month = g.Key,
                    Amount = g.Sum(r => r.RepaymentAmount),
                    Count = g.Count(),
                    PrincipalAmount = g.Sum(r => r.PrincipalAmount),
                    InterestAmount = g.Sum(r => r.InterestAmount)
                }).OrderBy(m => m.Month).ToList(),
                PaymentMethods = repayments.GroupBy(r => r.PaymentMethod).Select(g => new
                {
                    Method = g.Key,
                    Count = g.Count(),
                    Amount = g.Sum(r => r.RepaymentAmount)
                }).ToList()
            };
        }


        



        // Loan Eligibility
        public async Task<bool> CheckLoanEligibilityAsync(long employeeId, string loanType)
        {
            // Get employee details using EmployeeDataService
            var employee = await _employeeDataService.GetEmployeeFullDataAsync(employeeId);

            if (employee == null || employee.Status != "active")
                return false;

            // Check tenure requirement (minimum 6 months)
            if (!employee.HireDate.HasValue)
                return false;

            var tenure = DateTime.Now - employee.HireDate.Value;
            if (tenure.Days < 180) // 6 months
                return false;

            // Check existing active loans limit
            var activeLoansCount = await Context.Queryable<HrLoan>()
                .CountAsync(l => l.EmployeeId == employeeId && l.Status == "active");

            if (activeLoansCount >= 2) // Max 2 active loans
                return false;

            // Check loan type specific eligibility
            switch (loanType?.ToLower())
            {
                case "housing":
                    // Housing loans typically require longer tenure
                    return tenure.Days >= 365; // 1 year

                case "vehicle":
                    // Vehicle loans may require specific salary level
                    return true; // Add salary check if needed

                case "emergency":
                    // Emergency loans have relaxed criteria
                    return tenure.Days >= 90; // 3 months

                default:
                    return true;
            }
        }

        public async Task<decimal> GetMaxLoanAmountAsync(long employeeId, string loanType)
        {
            // Get employee's current salary
            var latestSalary = await Context.Queryable<HrPayrollDetail>()
                .Where(p => p.EmployeeId == employeeId)
                .OrderBy(p => p.PayPeriodEnd, OrderByType.Desc)
                .Select(p => p.BasicSalary)
                .FirstAsync();

            if (latestSalary <= 0)
                return 0;

            // Calculate max loan based on salary multiples
            decimal maxMultiplier = loanType?.ToLower() switch
            {
                "housing" => 48, // 4 years salary
                "vehicle" => 24, // 2 years salary  
                "education" => 12, // 1 year salary
                "emergency" => 3, // 3 months salary
                "personal" => 6, // 6 months salary
                _ => 6 // Default to 6 months
            };

            var maxAmount = latestSalary * maxMultiplier;

            // Also check against existing loan obligations
            var existingMonthlyDeductions = await Context.Queryable<HrLoan>()
                .Where(l => l.EmployeeId == employeeId && l.Status == "active")
                .SumAsync(l => l.MonthlyDeduction);

            // Ensure total deductions don't exceed 50% of salary
            var maxMonthlyDeduction = latestSalary * 0.5m - existingMonthlyDeductions;
            if (maxMonthlyDeduction <= 0)
                return 0;

            // Calculate max loan amount based on repayment capacity
            var maxRepaymentMonths = loanType?.ToLower() switch
            {
                "housing" => 60, // 5 years
                "vehicle" => 48, // 4 years
                _ => 36 // 3 years default
            };

            var maxBasedOnCapacity = maxMonthlyDeduction * maxRepaymentMonths;

            return Math.Min(maxAmount, maxBasedOnCapacity);
        }

        public async Task<object> GetEligibilityCriteriaAsync(string loanType)
        {
            return loanType?.ToLower() switch
            {
                "housing" => new
                {
                    MinTenure = 365,
                    MaxAmount = "48x Monthly Salary",
                    MaxTerm = 60,
                    InterestRate = 0,
                    RequiredDocuments = new[] { "Salary Certificate", "Property Documents", "No Objection Certificate" }
                },
                "vehicle" => new
                {
                    MinTenure = 180,
                    MaxAmount = "24x Monthly Salary",
                    MaxTerm = 48,
                    InterestRate = 0,
                    RequiredDocuments = new[] { "Salary Certificate", "Vehicle Quote", "Insurance Quote" }
                },
                "emergency" => new
                {
                    MinTenure = 90,
                    MaxAmount = "3x Monthly Salary",
                    MaxTerm = 12,
                    InterestRate = 0,
                    RequiredDocuments = new[] { "Emergency Justification", "Supporting Documents" }
                },
                _ => new
                {
                    MinTenure = 180,
                    MaxAmount = "6x Monthly Salary", 
                    MaxTerm = 36,
                    InterestRate = 0,
                    RequiredDocuments = new[] { "Salary Certificate", "Loan Application Form" }
                }
            };
        }

        // Loan Documents
        public async Task<int> AttachLoanDocumentAsync(long loanId, string documentPath)
        {
            var loan = await GetLoanByIdAsync(loanId);
            if (loan == null)
                throw new ArgumentException("Loan not found");

            var documents = string.IsNullOrEmpty(loan.AttachmentUrl) 
                ? new List<string>() 
                : loan.AttachmentUrl.Split(';').ToList();

            documents.Add(documentPath);

            return await Context.Updateable<HrLoan>()
                .SetColumns(l => new HrLoan
                {
                    AttachmentUrl = string.Join(";", documents),
                    Update_time = DateTime.Now
                })
                .Where(l => l.LoanId == loanId)
                .ExecuteCommandAsync();
        }

        public async Task<List<string>> GetLoanDocumentsAsync(long loanId)
        {
            var loan = await GetLoanByIdAsync(loanId);
            if (loan == null || string.IsNullOrEmpty(loan.AttachmentUrl))
                return new List<string>();

            return loan.AttachmentUrl.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList();
        }

        // Reports
        public async Task<byte[]> GenerateLoanStatementAsync(long loanId)
        {
            // This would integrate with a reporting engine like Crystal Reports, DevExpress, etc.
            await Task.CompletedTask;
            return new byte[0];
        }

        public async Task<byte[]> GenerateRepaymentScheduleAsync(long loanId)
        {
            // This would generate a detailed repayment schedule document
            await Task.CompletedTask;
            return new byte[0];
        }

        public async Task<byte[]> ExportLoansReportAsync(Expression<Func<HrLoan, bool>> predicate, string format)
        {
            // This would export loan data in various formats (Excel, PDF, CSV)
            await Task.CompletedTask;
            return new byte[0];
        }

        // Bulk Operations
        public async Task<int> ProcessBulkRepaymentAsync(List<HrLoanRepayment> repayments)
        {
            var processedCount = 0;
            
            foreach (var repayment in repayments)
            {
                try
                {
                    await ProcessRepaymentAsync(repayment);
                    processedCount++;
                }
                catch (Exception)
                {
                    // Log error and continue with next repayment
                    continue;
                }
            }

            return processedCount;
        }

        public async Task<int> UpdateMultipleLoansStatusAsync(List<long> loanIds, string status)
        {
            return await Context.Updateable<HrLoan>()
                .SetColumns(l => new HrLoan
                {
                    Status = status,
                    Update_time = DateTime.Now
                })
                .Where(l => loanIds.Contains(l.LoanId))
                .ExecuteCommandAsync();
        }

        // Helper Methods
        private decimal CalculateMonthlyPayment(decimal principal, decimal annualRate, int months)
        {
            if (annualRate <= 0)
            {
                // If no interest, simple division
                return principal / months;
            }

            var monthlyRate = annualRate / 100 / 12;
            var denominator = Math.Pow(1 + (double)monthlyRate, months) - 1;
            
            if (denominator <= 0)
                return principal / months;

            return principal * monthlyRate * (decimal)Math.Pow(1 + (double)monthlyRate, months) / (decimal)denominator;
        }

        private int GetRemainingMonths(DateTime startDate, DateTime endDate)
        {
            if (endDate <= startDate)
                return 0;

            var months = 0;
            var current = startDate;
            
            while (current < endDate)
            {
                current = current.AddMonths(1);
                months++;
            }

            return months;
        }
    }
}

