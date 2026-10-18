# Power BI — DAX Measures (PMO Portal)

Create a **measures table** `_Measures` (Home → Enter Data → empty table) and add the measures below.
`[Snapshot Date]` comes from a **What-if / single-value parameter** named `Snapshot` (default
`DATE(2026,9,30)`); production points it at `TODAY()` or a user-selected date. All amounts are AED.

```DAX
-- 0) Snapshot the whole model reconciles to (disconnected parameter value).
Snapshot Date = SELECTEDVALUE ( 'Snapshot'[Snapshot], DATE ( 2026, 9, 30 ) )
```

### 1) Overdue Tasks (count)  — delivery
```DAX
Overdue Tasks =
CALCULATE (
    COUNTROWS ( pmo_task ),
    pmo_task[status] <> "Done",
    pmo_task[due_date] < [Snapshot Date]
)
```

### 2) Task Completion %  (weighted portfolio progress) — delivery
```DAX
Task Completion % =
DIVIDE (
    SUMX ( pmo_task, pmo_task[weight] * pmo_task[progress_pct] ),
    SUMX ( pmo_task, pmo_task[weight] )
)
```

### 3) Approved Budget  (initial + approved revisions) — finance
```DAX
Approved Budget =
SUM ( pmo_project[initial_approved_budget] )
    + CALCULATE (
        SUM ( pmo_budget_revision[amount] ),
        pmo_budget_revision[status] = "Approved"      -- pending/rejected excluded
    )
```

### 4) Actual Spend  (posted ≤ snapshot) — finance
```DAX
Actual Spend =
CALCULATE (
    SUM ( pmo_cost_transaction[amount] ),
    pmo_cost_transaction[status] = "Posted",          -- drafts excluded
    pmo_cost_transaction[txn_date] <= [Snapshot Date]
)
```

### 5) Budget Utilization %  — finance
```DAX
Budget Utilization % = DIVIDE ( [Actual Spend], [Approved Budget] )   -- 0 when budget = 0
```

### 6) Forecast Variance  (bonus — EAC vs budget) — finance
```DAX
Forecast To Complete =
CALCULATE ( SUM ( pmo_financial_forecast[forecast_amount] ),
            pmo_financial_forecast[kind] = "ForecastToComplete" )

EAC = [Actual Spend] + [Forecast To Complete]

Projected Variance = [Approved Budget] - [EAC]        -- negative = projected overrun
```

### Supporting KPI measures (used on the page)
```DAX
Remaining Budget   = [Approved Budget] - [Actual Spend]
Total Projects     = DISTINCTCOUNT ( pmo_project[project_id] )
Active Projects    = CALCULATE ( [Total Projects], pmo_project[status] = "Active" )
Open High Risks    = CALCULATE ( COUNTROWS ( pmo_risk ),
                        pmo_risk[status] = "Open", pmo_risk[score] >= 12 )
Projected Overruns = COUNTROWS ( FILTER ( VALUES ( pmo_project[project_id] ), [Projected Variance] < 0 ) )
```

**Expected values @ Snapshot = 2026-09-30 (whole portfolio):**
Total Projects **8** · Active **5** · Overdue Tasks **23** · Open High Risks **6** ·
Approved Budget **15,501,500** · Actual Spend **8,095,836.53** · Utilization **52.2%**.
