using System;

namespace ZR.Model.HR.Finance.Dto
{
    public class PayrollWithFinanceLinksDto
    {
        public long PayrollId { get; set; }
        public long EmployeeId { get; set; }
        public string PayPeriod { get; set; }
        public decimal NetSalary { get; set; }

        public Guid? APInvoiceId { get; set; }
        public string APInvoiceNumber { get; set; }
        public decimal? APInvoiceTotalAmount { get; set; }
        public string APInvoiceCurrency { get; set; }

        public long? TreasuryTransactionId { get; set; }
        public decimal? TreasuryAmount { get; set; }
        public string TreasuryCurrency { get; set; }
    }
}















