using System;
using System.Threading.Tasks;
using Infrastructure.Attribute;
using SqlSugar;
using ZR.Model.Finance.Zatca;
using ZR.Model.Finance.Zatca.Enums;
using ZR.Model.HR.Finance;
using ZR.Model.HR.Payroll;
using ZR.Service.HR.IService;

namespace ZR.Service.HR
{
    [AppService(ServiceType = typeof(IHRComplianceService), ServiceLifetime = LifeTime.Transient)]
    public class HRComplianceService : BaseService<HrPayrollZatca>, IHRComplianceService
    {
        public async Task<ZatcaSubmissionStatus> CheckZatcaCompliance(long payrollId)
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
                    throw new Exception($"No ZATCA hash found for AP invoice {payroll.APInvoiceId.Value}.");

                var existing = await Context.Queryable<HrPayrollZatca>()
                    .FirstAsync(x => x.PayrollId == payrollId);

                if (existing == null)
                {
                    existing = new HrPayrollZatca
                    {
                        Id = Guid.NewGuid(),
                        PayrollId = payrollId,
                        ZatcaHashId = hash.Id,
                        SubmissionStatus = hash.SubmissionStatus,
                        Create_time = DateTime.Now
                    };
                    await Context.Insertable(existing).ExecuteCommandAsync();
                }
                else
                {
                    existing.ZatcaHashId = hash.Id;
                    existing.SubmissionStatus = hash.SubmissionStatus;
                    existing.Update_time = DateTime.Now;
                    await Context.Updateable(existing).ExecuteCommandAsync();
                }

                // Audit log
                var audit = new HrFinancialAuditLog
                {
                    LogId = Guid.NewGuid(),
                    Action = "CheckZatcaCompliance",
                    UserId = 0,
                    Timestamp = DateTime.Now,
                    Changes = $"{{\"payrollId\":{payrollId},\"apInvoiceId\":\"{payroll.APInvoiceId.Value}\",\"zatcaHashId\":\"{hash.Id}\",\"status\":{(int)hash.SubmissionStatus}}}",
                    Create_time = DateTime.Now
                };
                await Context.Insertable(audit).ExecuteCommandAsync();

                return hash.SubmissionStatus;
            });

            if (!result.IsSuccess)
                throw result.ErrorException;

            return (ZatcaSubmissionStatus)result.Data;
        }
    }
}


