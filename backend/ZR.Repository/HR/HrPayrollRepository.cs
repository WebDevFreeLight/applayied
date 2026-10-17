using System;
using System.Threading.Tasks;
using Infrastructure.Attribute;
using SqlSugar;
using ZR.Model.Finance.ARAP;
using ZR.Model.Finance.Treasury;
using ZR.Model.HR.Finance;
using ZR.Model.HR.Finance.Enums;
using ZR.Model.HR.Finance.Dto;
using ZR.Model.HR.Payroll;
using ZR.Common.Cache;

namespace ZR.Repository
{
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class HrPayrollRepository : BaseRepository<HrPayroll>
    {
        public async Task<PayrollWithFinanceLinksDto> GetPayrollWithFinanceLinks(long payrollId)
        {
            var cacheKey = $"hr:payroll:finance:{payrollId}";
            if (RedisServer.Cache != null && RedisServer.Cache.Exists(cacheKey))
            {
                var cached = RedisServer.Cache.Get<PayrollWithFinanceLinksDto>(cacheKey);
                if (cached != null) return cached;
            }

            var dto = await Context.Queryable<HrPayroll>()
                .LeftJoin<APInvoice>((p, ap) => p.APInvoiceId == ap.Id)
                .LeftJoin<ErpTreasuryTransaction>((p, ap, tt) => p.TreasuryTransactionId == tt.TransactionId)
                .Where((p, ap, tt) => p.PayrollId == payrollId)
                .Select((p, ap, tt) => new PayrollWithFinanceLinksDto
                {
                    PayrollId = p.PayrollId,
                    EmployeeId = p.EmployeeId,
                    PayPeriod = p.PayPeriod,
                    NetSalary = p.NetSalary,
                    APInvoiceId = p.APInvoiceId,
                    APInvoiceNumber = ap.InvoiceNumber,
                    APInvoiceTotalAmount = SqlFunc.IIF(ap.Id == Guid.Empty, (decimal?)null, ap.TotalAmount),
                    APInvoiceCurrency = ap.Currency,
                    TreasuryTransactionId = p.TreasuryTransactionId,
                    TreasuryAmount = SqlFunc.IIF(tt.TransactionId == 0, (decimal?)null, tt.Amount),
                    TreasuryCurrency = tt.Currency
                })
                .FirstAsync();

            if (dto != null)
            {
                // short TTL to reduce repeated joins while staying fresh
                RedisServer.Cache?.Set(cacheKey, dto, 60);
            }

            return dto;
        }

        public async Task<Guid> ReconcilePayroll(long payrollId)
        {
            var result = await Context.Ado.UseTranAsync(async () =>
            {
                var payroll = await Context.Queryable<HrPayroll>()
                    .FirstAsync(p => p.PayrollId == payrollId);

                if (payroll == null)
                    throw new Exception($"Payroll not found: {payrollId}");
                if (!payroll.APInvoiceId.HasValue)
                    throw new Exception($"Payroll {payrollId} has no APInvoiceId.");

                var recon = new HrFinanceReconciliation
                {
                    ReconciliationId = Guid.NewGuid(),
                    PayrollId = payrollId,
                    APInvoiceId = payroll.APInvoiceId.Value,
                    ReconciledAmount = payroll.NetSalary,
                    Status = HrFinanceReconciliationStatus.Pending,
                    AIInsight = null,
                    Create_time = DateTime.Now
                };

                await Context.Insertable(recon).ExecuteCommandAsync();

                // invalidate cached join
                var cacheKey = $"hr:payroll:finance:{payrollId}";
                RedisServer.Cache?.Del(cacheKey);
                return recon.ReconciliationId;
            });

            if (!result.IsSuccess)
                throw result.ErrorException;

            return (Guid)result.Data;
        }
    }
}


