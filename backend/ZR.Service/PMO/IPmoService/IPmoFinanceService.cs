using System;
using System.Collections.Generic;
using ZR.Model;
using ZR.Model.PMO;
using ZR.Model.PMO.Dto;

namespace ZR.Service.PMO.IPmoService
{
    public interface IPmoFinanceService
    {
        // Derived summary + building blocks (all from SQL)
        PmoFinancialSummaryDto GetProjectSummary(long projectId, DateTime? cutoff, PmoActor actor);
        decimal GetCurrentApprovedBudget(long projectId);
        decimal GetActualSpend(long projectId, DateTime cutoff);

        // Cost transactions
        PagedInfo<PmoCostTransaction> GetTransactions(PmoCostTransactionQueryDto parm, PmoActor actor);
        PmoCostTransaction AddTransaction(PmoCostTransactionInputDto dto, PmoActor actor);
        int UpdateTransaction(PmoCostTransactionInputDto dto, PmoActor actor);
        PmoCostTransaction PostTransaction(long transactionId, PmoActor actor);

        // Budget revisions
        PagedInfo<PmoBudgetRevision> GetRevisions(PmoBudgetRevisionQueryDto parm, PmoActor actor);
        PmoBudgetRevision AddRevision(PmoBudgetRevisionInputDto dto, PmoActor actor);
        PmoBudgetRevision DecideRevision(PmoBudgetRevisionDecisionDto dto, PmoActor actor);

        // Forecasts
        List<PmoFinancialForecast> GetForecasts(long projectId);
        PmoFinancialForecast UpsertForecast(PmoForecastInputDto dto, PmoActor actor);

        // Drill-down (finance KPI -> underlying posted transactions)
        List<PmoCostTransaction> GetTransactionDrilldown(long? projectId, long? categoryId, string period, DateTime? cutoff);
    }
}
