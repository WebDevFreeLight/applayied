using System;
using System.Threading.Tasks;
using Infrastructure.Attribute;
using SqlSugar;
using ZR.Model.Finance.Zatca;
using ZR.Model.HR.Finance;
using ZR.Model.HR.Payroll;

namespace ZR.Repository
{
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class HrPayrollZatcaRepository : BaseRepository<HrPayrollZatca>
    {
        public async Task<Guid> SubmitToZatca(long payrollId)
        {
            var result = await Context.Ado.UseTranAsync(async () =>
            {
                var payroll = await Context.Queryable<HrPayroll>()
                    .FirstAsync(p => p.PayrollId == payrollId);

                if (payroll == null)
                    throw new Exception($"Payroll not found: {payrollId}");
                if (!payroll.APInvoiceId.HasValue)
                    throw new Exception($"Payroll {payrollId} has no APInvoiceId.");

                var hash = await Context.Queryable<ZatcaInvoiceHash>()
                    .FirstAsync(h => h.InvoiceId == payroll.APInvoiceId.Value);

                if (hash == null)
                    throw new Exception($"No ZATCA hash found for AP invoice {payroll.APInvoiceId.Value} (payroll {payrollId}).");

                var existing = await Context.Queryable<HrPayrollZatca>()
                    .FirstAsync(x => x.PayrollId == payrollId);

                if (existing != null)
                {
                    existing.ZatcaHashId = hash.Id;
                    existing.SubmissionStatus = hash.SubmissionStatus;
                    existing.Update_time = DateTime.Now;
                    await Context.Updateable(existing).ExecuteCommandAsync();
                    return existing.Id;
                }

                var row = new HrPayrollZatca
                {
                    Id = Guid.NewGuid(),
                    PayrollId = payrollId,
                    ZatcaHashId = hash.Id,
                    SubmissionStatus = hash.SubmissionStatus,
                    Create_time = DateTime.Now
                };

                await Context.Insertable(row).ExecuteCommandAsync();
                return row.Id;
            });

            if (!result.IsSuccess)
                throw result.ErrorException;

            return (Guid)result.Data;
        }
    }
}















