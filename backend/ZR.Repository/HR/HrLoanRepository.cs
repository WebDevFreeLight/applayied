using System.Collections.Generic;
using System.Threading.Tasks;
using Infrastructure.Attribute;
using SqlSugar;
using ZR.Model.HR.Finance.Dto;
using ZR.Model.HR.Payroll;

namespace ZR.Repository
{
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class HrLoanRepository : BaseRepository<HrLoan>
    {
        public async Task<List<LoanWithGLJournalDto>> GetLoansWithGLJournals(long employeeId)
        {
            return await Context.Queryable<HrLoan>()
                .LeftJoin<HrLoanRepayment>((l, r) => l.LoanId == r.LoanId)
                .Where((l, r) => l.EmployeeId == employeeId)
                .Select((l, r) => new LoanWithGLJournalDto
                {
                    LoanId = l.LoanId,
                    EmployeeId = l.EmployeeId,
                    LoanAmount = l.LoanAmount,
                    Status = l.Status,
                    GLJournalId = l.GLJournalId,
                    RepaymentId = SqlFunc.IIF(r.RepaymentId == 0, (long?)null, r.RepaymentId),
                    RepaymentAmount = SqlFunc.IIF(r.RepaymentId == 0, (decimal?)null, r.RepaymentAmount),
                    RepaymentDate = SqlFunc.IIF(r.RepaymentId == 0, (System.DateTime?)null, r.RepaymentDate),
                    RepaymentGLJournalId = r.GLJournalId
                })
                .ToListAsync();
        }
    }
}















