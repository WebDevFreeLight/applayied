
using Infrastructure.Converter;
using SqlSugar;
using System;
using ZR.Model.System;
using Newtonsoft.Json;

namespace ZR.Model.HR.Payroll
{
    /// <summary>
    /// Employee Loan Table
    /// </summary>
    [SugarTable("hr_loan")]
    
    [SqlDict(
        dictType: "loan",
        dictName: "Hr Loan",
        valueColumn: "LoanId",
        labelColumns: "loantype",
        RemarkColumn = "remarks",
        OrderBy = "LoanId ASC",
        Description = "Auto-generated from HrLoan table"
    )]
    public class HrLoan : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long LoanId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }

        [SugarColumn(Length = 50)]
        public string LoanType { get; set; } // personal, housing, vehicle, education, emergency

        public decimal LoanAmount { get; set; }

        public decimal InterestRate { get; set; } = 0;

        public int RepaymentMonths { get; set; }

        public decimal MonthlyDeduction { get; set; }

        public DateTime LoanDate { get; set; }

        public DateTime ApplicationDate { get; set; }

        public DateTime StartDeductionDate { get; set; }

        public DateTime? EndDeductionDate { get; set; }

        public decimal TotalRepaid { get; set; } = 0;

        public decimal OutstandingAmount { get; set; }

        [SugarColumn(Length = 20)]
        public string Status { get; set; } // pending, approved, active, completed, rejected

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? ApprovedBy { get; set; }

        public DateTime? ApprovalDate { get; set; }

        [SugarColumn(Length = 500)]
        public string Purpose { get; set; }

        [SugarColumn(Length = 500)]
        public string Remarks { get; set; }

        [SugarColumn(Length = 500)]
        public string AttachmentUrl { get; set; }

        public bool IsActive { get; set; } = true;

    
        public decimal PaidAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public DateTime CompletedDate { get; set; }

        /// <summary>
        /// Finance GL Journal ID (FK -> gl_journal.id)
        /// </summary>
        public Guid? GLJournalId { get; set; }

        /// <summary>
        /// Payment Hub Transaction ID (FK -> payment_hub_transaction.id)
        /// </summary>
        public Guid? PaymentHubTransactionId { get; set; }
    }

    /// <summary>
    /// Loan Repayment Table
    /// </summary>
    [SugarTable("hr_loan_repayment")]
    public class HrLoanRepayment : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long RepaymentId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long LoanId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }

        [SugarColumn(Length = 20)]
        public string PayPeriod { get; set; } // YYYY-MM

        public decimal RepaymentAmount { get; set; }

        public DateTime RepaymentDate { get; set; }

        public DateTime PaymentDate { get; set; }

        [SugarColumn(Length = 20)]
        public string PaymentMethod { get; set; } // salary_deduction, bank_transfer, cash

        public decimal PrincipalAmount { get; set; }

        public decimal InterestAmount { get; set; }

        public decimal OutstandingBalance { get; set; }

        [SugarColumn(Length = 20)]
        public string Status { get; set; } // scheduled, paid, overdue, cancelled

        /// <summary>
        /// Finance GL Journal ID (FK -> gl_journal.id)
        /// </summary>
        public Guid? GLJournalId { get; set; }

        /// <summary>
        /// Payment Hub Transaction ID (FK -> payment_hub_transaction.id)
        /// </summary>
        public Guid? PaymentHubTransactionId { get; set; }
 
    }
}

