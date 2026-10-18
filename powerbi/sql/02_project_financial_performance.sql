-- Q2 — Project financial performance (finance fact, one row per project)
-- Applies the documented definitions exactly:
--   current approved budget = initial + SUM(approved revisions)   (pending/rejected excluded)
--   actual spend            = SUM(posted cost txns on/before the snapshot)  (drafts excluded)
--   remaining               = approved - actual
--   utilization %           = actual / approved          (NULL when approved = 0)
--   EAC                     = actual + forecast-to-complete
--   projected variance      = approved - EAC
-- Aggregates are pre-grouped per table to AVOID fan-out inflation from joining
-- budget + cost + forecast in one flat join.
WITH budget AS (
    SELECT p.project_id, p.name, p.code, p.dept_id, p.owner_user_id, p.status,
           p.initial_approved_budget
         + COALESCE(SUM(r.amount) FILTER (WHERE r.status = 'Approved'), 0) AS approved_budget
    FROM   pmo_project p
    LEFT   JOIN pmo_budget_revision r ON r.project_id = p.project_id
    GROUP  BY p.project_id, p.name, p.code, p.dept_id, p.owner_user_id, p.status, p.initial_approved_budget
),
spend AS (
    SELECT project_id,
           SUM(amount) FILTER (WHERE status = 'Posted' AND txn_date <= DATE '2026-09-30') AS actual_spend
    FROM   pmo_cost_transaction
    GROUP  BY project_id
),
ftc AS (
    SELECT project_id, SUM(forecast_amount) FILTER (WHERE kind = 'ForecastToComplete') AS forecast_to_complete
    FROM   pmo_financial_forecast
    GROUP  BY project_id
)
SELECT  b.project_id, b.code AS project_code, b.name AS project_name, b.dept_id, b.status,
        b.approved_budget,
        COALESCE(s.actual_spend, 0)                                   AS actual_spend,
        b.approved_budget - COALESCE(s.actual_spend, 0)               AS remaining_budget,
        ROUND(COALESCE(s.actual_spend,0) / NULLIF(b.approved_budget,0) * 100, 1) AS utilization_pct,
        COALESCE(s.actual_spend,0) + COALESCE(f.forecast_to_complete,0)          AS eac,
        b.approved_budget - (COALESCE(s.actual_spend,0) + COALESCE(f.forecast_to_complete,0)) AS projected_variance
FROM    budget b
LEFT    JOIN spend s ON s.project_id = b.project_id
LEFT    JOIN ftc   f ON f.project_id = b.project_id
ORDER BY b.name
