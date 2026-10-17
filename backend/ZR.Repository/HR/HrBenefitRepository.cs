using System;
using System.Threading.Tasks;
using Infrastructure.Attribute;
using SqlSugar;
using ZR.Model.Finance.Budget;
using ZR.Model.HR;

namespace ZR.Repository
{
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class HrBenefitRepository : BaseRepository<HrBenefitEnrollment>
    {
        public async Task<bool> ValidateEnrollmentAgainstBudget(long enrollmentId)
        {
            var enrollment = await Context.Queryable<HrBenefitEnrollment>()
                .FirstAsync(e => e.EnrollmentId == enrollmentId);

            if (enrollment == null)
                throw new Exception($"Enrollment not found: {enrollmentId}");

            if (!enrollment.BudgetAllocationId.HasValue)
                return true;

            var available = await Context.Queryable<BudgetLine>()
                .Where(b => b.Id == enrollment.BudgetAllocationId.Value)
                .Select(b =>
                    (SqlFunc.IIF(b.RevisedAmount > 0, b.RevisedAmount, b.BudgetedAmount) - b.ActualAmount - b.CommittedAmount))
                .FirstAsync();

            // Budget is enforced on employer contribution (company liability)
            return available >= enrollment.EmployerContribution;
        }
    }
}















