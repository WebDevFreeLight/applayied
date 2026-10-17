using System;

namespace ZR.Model.HR.Finance.Dto
{
    public class LoanWithGLJournalDto
    {
        public long LoanId { get; set; }
        public long EmployeeId { get; set; }
        public decimal LoanAmount { get; set; }
        public string Status { get; set; }
        public Guid? GLJournalId { get; set; }

        public long? RepaymentId { get; set; }
        public decimal? RepaymentAmount { get; set; }
        public DateTime? RepaymentDate { get; set; }
        public Guid? RepaymentGLJournalId { get; set; }
    }
}















