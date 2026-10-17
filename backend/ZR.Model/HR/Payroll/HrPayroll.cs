
using Infrastructure.Converter;
using SqlSugar;
using System;
using ZR.Model.System;
using Newtonsoft.Json;

namespace ZR.Model.HR.Payroll
{
    /// <summary>
    /// Payroll Records Table
    /// </summary>
    [SugarTable("hr_payroll")]
    
    [SqlDict(
        dictType: "payroll",
        dictName: "Hr Payroll",
        valueColumn: "PayrollId",
        labelColumns: "payperiod",
        RemarkColumn = "remark",
        OrderBy = "PayrollId ASC",
        Description = "Auto-generated from HrPayroll table"
    )]
    public class HrPayroll : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long PayrollId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }

        [SugarColumn(Length = 20)]
        public string PayPeriod { get; set; } // YYYY-MM

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

        [SugarColumn(Length = 20)]
        public string PaymentMethod { get; set; } // bank_transfer, cash, check

        [SugarColumn(Length = 100)]
        public string BankReference { get; set; }

        [SugarColumn(Length = 20)]
        public string Status { get; set; } // pending, processed, paid

        public int WorkingDays { get; set; }

        public int ActualDays { get; set; }

        public decimal LeaveDeduction { get; set; }

        public decimal LateDeduction { get; set; }

        public decimal Bonus { get; set; }

        public decimal Commission { get; set; }

       
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ApprovedBy { get; set; }
        public DateTime ApprovedDate { get; set; }
  
        public DateTime FinalizedDate { get; set; }
        public string TransferReference { get; set; }
        public DateTime TransferDate { get; set; }

        /// <summary>
        /// Finance AP Invoice ID (FK -> arap_ap_invoice.id)
        /// </summary>
        public Guid? APInvoiceId { get; set; }

        /// <summary>
        /// Treasury Transaction ID (FK -> erp_treasury_transactions.transactionid)
        /// NOTE: treasury module uses BIGINT IDs in current schema.
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? TreasuryTransactionId { get; set; }

        /// <summary>
        /// Finance Currency ID (FK -> currencies.id)
        /// </summary>
        public Guid? CurrencyId { get; set; }

        /// <summary>
        /// Exchange Rate
        /// </summary>
        [SugarColumn(Length = 18, DecimalDigits = 10)]
        public decimal ExchangeRate { get; set; } = 1.0m;
    }

    /// <summary>
    /// Payroll Details Table
    /// </summary>
    [SugarTable("hr_payroll_detail")]
    public class HrPayrollDetail : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long DetailId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long PayrollId { get; set; }

        [SugarColumn(Length = 50)]
        public string ComponentType { get; set; } // earning, deduction

        [SugarColumn(Length = 100)]
        public string ComponentName { get; set; }

        public decimal Amount { get; set; }

        [SugarColumn(Length = 500)]
        public string Description { get; set; }

        public bool IsTaxable { get; set; }

        public DateTime Create_time { get; set; }

        /// <summary>
        /// Gross Pay
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public decimal GrossPay => BasicSalary + OvertimePay + Bonus;

        public DateTime PayPeriodStart { get; set; }
        public decimal OvertimePay { get; set; }
        public DateTime PayPeriodEnd { get; set; }
        public decimal NetPay { get; set; }
        public decimal BasicSalary { get; set; }
        public decimal Bonus { get; set; }
        public decimal TotalDeductions { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }
        public decimal IncomeTax { get; set; }


        public decimal Allowances { get; set; }
        public decimal GrossSalary { get; set; }
        public decimal Deductions { get; set; }
        public decimal GOSIEmployee { get; set; }
        public decimal GOSIEmployer { get; set; }
        public decimal Tax { get; set; }
        public decimal LoanDeduction { get; set; }
        public decimal NetSalary { get; set; }
        public string Status { get; set; }
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Tax Code ID (FK -> tax_code.id)
        /// </summary>
        public Guid? TaxCodeId { get; set; }

        /// <summary>
        /// ZATCA Invoice Hash ID (FK -> zatca_invoice_hash.id)
        /// </summary>
        public Guid? ZatcaHashId { get; set; }

        /// <summary>
        /// Is Tax Inclusive?
        /// </summary>
        public bool IsTaxInclusive { get; set; } = false;
    }
}

