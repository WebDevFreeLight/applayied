-- Q4 — Period spend & forecast (finance, monthly series for a cost-trend visual)
-- "Actual" rows = posted cost transactions bucketed by calendar month (on/before snapshot).
-- "Forecast" rows = forecast-to-complete amounts by their period month.
-- Union gives one tidy table (series column) that a line/column chart can split by legend.
SELECT  to_char(date_trunc('month', t.txn_date), 'YYYY-MM') AS period_month,
        t.project_id,
        'Actual'            AS series,
        SUM(t.amount)       AS amount
FROM    pmo_cost_transaction t
WHERE   t.status = 'Posted'
  AND   t.txn_date <= DATE '2026-09-30'
GROUP BY 1, 2
UNION ALL
SELECT  to_char(date_trunc('month', f.period_month), 'YYYY-MM') AS period_month,
        f.project_id,
        'Forecast'          AS series,
        SUM(f.forecast_amount) AS amount
FROM    pmo_financial_forecast f
WHERE   f.kind = 'ForecastToComplete'
GROUP BY 1, 2
ORDER BY period_month, series
