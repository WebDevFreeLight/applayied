using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Infrastructure.Attribute;
using SqlSugar;
using ZR.Model.Finance.ARAP;
using ZR.Model.HR.Finance;
using ZR.Model.HR.Finance.Enums;
using ZR.Model.HR.Payroll;

namespace ZR.Repository
{
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class HrFinanceReconciliationRepository : BaseRepository<HrFinanceReconciliation>
    {
        public async Task<int> ReconcileAllPending()
        {
            var result = await Context.Ado.UseTranAsync(async () =>
            {
                var pending = await Context.Queryable<HrFinanceReconciliation, HrPayroll, APInvoice>((r, p, ap) =>
                        new JoinQueryInfos(
                            JoinType.Inner, r.PayrollId == p.PayrollId,
                            JoinType.Inner, r.APInvoiceId == ap.Id))
                    .Where((r, p, ap) => r.Status == HrFinanceReconciliationStatus.Pending)
                    .Select((r, p, ap) => new
                    {
                        r.ReconciliationId,
                        PayrollAmount = p.NetSalary,
                        InvoiceAmount = ap.TotalAmount
                    })
                    .ToListAsync();

                var updated = 0;
                const decimal tolerance = 0.01m;

                foreach (var row in pending)
                {
                    var delta = row.InvoiceAmount - row.PayrollAmount;
                    var status = Math.Abs(delta) <= tolerance
                        ? HrFinanceReconciliationStatus.Reconciled
                        : HrFinanceReconciliationStatus.Discrepancy;

                    var insight = status == HrFinanceReconciliationStatus.Reconciled
                        ? null
                        : $"{{\"expectedPayroll\":{row.PayrollAmount},\"invoice\":{row.InvoiceAmount},\"delta\":{delta}}}";

                    updated += await Context.Updateable<HrFinanceReconciliation>()
                        .SetColumns(r => new HrFinanceReconciliation
                        {
                            Status = status,
                            AIInsight = insight,
                            ReconciledAmount = row.InvoiceAmount,
                            Update_time = DateTime.Now
                        })
                        .Where(r => r.ReconciliationId == row.ReconciliationId)
                        .ExecuteCommandAsync();
                }

                return updated;
            });

            if (!result.IsSuccess)
                throw result.ErrorException;

            return (int)result.Data;
        }
    }
}















