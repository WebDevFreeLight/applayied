# PMO Portal — Power BI Reporting Track

An **independent** Power BI report connected to the **PostgreSQL** database (not the CSVs). It reuses
the same SQL database and business definitions as the web dashboard but has its own model, measures
and visuals. Build time in Power BI Desktop: ~15 minutes using the assets in this folder.

```
powerbi/
├─ README.md                 ← this file (model, connection, pages, reconciliation, build steps)
├─ DAX_measures.md           ← 6 required + supporting DAX measures
└─ sql/                      ← 5 validated PostgreSQL queries (all run as-is)
   ├─ 01_overdue_tasks.sql
   ├─ 02_project_financial_performance.sql
   ├─ 03_high_exposure_open_risks.sql
   ├─ 04_monthly_spend_and_forecast.sql
   └─ 05_milestone_status.sql
```

---

## 1. Connect to PostgreSQL (uses the `appsettings.json` connection)

`backend/ZR.Admin.WebApi/appsettings.json → DbConnection.Conn`:
```
Host=localhost;Port=5432;Database=MahamDB;Username=abdulqader;Password=BaObk42oig2y
```

In **Power BI Desktop**:
1. **Home → Get Data → PostgreSQL database** (if prompted, let it install the Npgsql connector).
2. **Server** `localhost` (or `localhost:5432`) · **Database** `MahamDB`.
3. **Data Connectivity mode: Import**.
4. Credentials tab **Database**: User `abdulqader`, Password `BaObk42oig2y`. (For a remote host swap the
   Server for the host in the alternate `Conn` string in appsettings.json.)
5. Either pick the base tables in the Navigator **or** paste each file in `sql/` via
   **Get Data → PostgreSQL → Advanced → SQL statement** (recommended — the queries pre-shape the data).

> Snapshot parameter: **Modeling → New parameter → Numeric/Date (What-if)** named `Snapshot`,
> default `2026-09-30`. The SQL files hard-code `DATE '2026-09-30'` for the baseline; to make them
> dynamic, replace that literal with a Power Query parameter `@snapshot` and bind it to `Snapshot`.

---

## 2. Relational model & filter direction

Import these base tables (or the shaped queries): `pmo_project`, `pmo_task`, `pmo_risk`,
`pmo_milestone`, `pmo_change_request`, `pmo_cost_transaction`, `pmo_budget_revision`,
`pmo_financial_forecast`, `pmo_cost_category`, `sys_user`, `sys_dept`, plus a **Calendar** table.

**Calendar (date dimension)** — New table:
```DAX
Calendar =
ADDCOLUMNS (
    CALENDAR ( DATE (2025,1,1), DATE (2027,12,31) ),
    "Year", YEAR ([Date]), "Month", FORMAT ([Date],"YYYY-MM"),
    "MonthName", FORMAT ([Date],"MMM yyyy"), "Quarter", "Q" & FORMAT ([Date],"Q")
)
```
Mark it as a **Date table** (Table tools → Mark as date table → `Date`).

**Relationships (all single-direction, from dimension → fact — a star schema):**

| From (one) | To (many) | Key | Active | Notes |
|---|---|---|---|---|
| `pmo_project[project_id]` | `pmo_task[project_id]` | project | ✔ | |
| `pmo_project[project_id]` | `pmo_risk[project_id]` | project | ✔ | |
| `pmo_project[project_id]` | `pmo_milestone[project_id]` | project | ✔ | |
| `pmo_project[project_id]` | `pmo_cost_transaction[project_id]` | project | ✔ | |
| `pmo_project[project_id]` | `pmo_budget_revision[project_id]` | project | ✔ | |
| `pmo_project[project_id]` | `pmo_financial_forecast[project_id]` | project | ✔ | |
| `pmo_cost_category[category_id]` | `pmo_cost_transaction[category_id]` | category | ✔ | drives the category slicer on finance visuals only |
| `sys_dept[deptid]` | `pmo_project[dept_id]` | dept | ✔ | department slicer |
| `sys_user[userid]` | `pmo_project[owner_user_id]` | manager | ✔ | manager slicer |
| `Calendar[Date]` | `pmo_cost_transaction[txn_date]` | date | ✔ | the ACTIVE date relationship (spend trend) |
| `Calendar[Date]` | `pmo_task[due_date]` | date | ✖ inactive | role-playing; activate with `USERELATIONSHIP` if you add a due-date time visual |

**Filter direction = single (one-way).** Dimensions (`pmo_project`, `pmo_cost_category`, `sys_dept`,
`sys_user`, `Calendar`) filter the facts; facts never filter back. This prevents the classic
fan-out inflation — e.g. a project with many tasks **and** many cost rows must not multiply spend by
task count. Each fact is aggregated on its own grain (SQL `WITH` CTEs in Q2 do the same server-side).

---

## 3. SQL queries (the four required + one bonus)

| File | Purpose | Rows @ snapshot |
|---|---|---|
| `01_overdue_tasks.sql` | Overdue incomplete tasks (delivery) | **23** |
| `02_project_financial_performance.sql` | Approved / actual / remaining / util / EAC / variance per project | **8** |
| `03_high_exposure_open_risks.sql` | Open risks with score ≥ 12 | **6** |
| `04_monthly_spend_and_forecast.sql` | Monthly actual spend + forecast series (period spend/forecast) | **55** |
| `05_milestone_status.sql` | Milestone roadmap buckets (Achieved/Overdue/Upcoming) | **27** |

All five are validated against the live DB and return the counts above.

---

## 4. Management page (design)

One polished page, AED/% units, legends, consistent `Snapshot` period:
- **KPI card row:** Total Projects, Active, Overdue Tasks, Open High Risks, Approved Budget,
  Actual Spend, Utilization %, Projected Variance (all DAX measures).
- **Visuals (≥5):** (1) *Budget vs Actual vs EAC* clustered column by project (Q2 measures);
  (2) *Monthly cost trend* line with Actual/Forecast legend (Q4); (3) *Spend by category* donut
  (`pmo_cost_transaction[amount]` by `pmo_cost_category[name]`); (4) *Overdue tasks by owner* bar (Q1);
  (5) *Risk matrix* matrix of probability × impact colored by count (Q3); (6) *Milestone roadmap* table (Q5).
- **Slicers (≥4, coordinated):** Department (`sys_dept[deptname]`), Project (`pmo_project[name]`),
  Manager (`sys_user[nickname]`), Lifecycle status (`pmo_project[status]`), Calendar (timeframe), and
  **Cost category** — apply the category slicer only to the finance visuals (Format → Edit interactions).
- **Drill-through:** a *Transactions* detail page filtered by `project_id` (and optional category/month);
  right-click a finance KPI/bar → Drill through → underlying posted transactions. A delivery drill-through
  page shows filtered tasks for a selected project/owner. Set loading/empty states via visual headers.

---

## 5. KPI reconciliation — SQL vs Web dashboard vs Power BI  (snapshot 2026-09-30, whole portfolio)

| KPI | SQL query | Web dashboard (`/pmo/dashboard/kpis`) | Power BI (DAX) | Match |
|---|---|---|---|---|
| Total projects | 8 | 8 | `Total Projects` = 8 | ✅ |
| Overdue tasks | 23 | 23 | `Overdue Tasks` = 23 | ✅ |
| Open high-exposure risks | 6 | 6 | `Open High Risks` = 6 | ✅ |
| Approved budget | 15,501,500 | 15,501,500 | `Approved Budget` = 15,501,500 | ✅ |
| Actual spend | 8,095,836.53 | 8,095,836.53 | `Actual Spend` = 8,095,836.53 | ✅ |

All identical because the three layers share one database and the **same definitions** (approved =
initial + approved revisions; actual = posted ≤ snapshot). **Expected/explained discrepancies:** if a
reader filters Power BI to a different `Snapshot` or scope, numbers will differ from an unfiltered web
view — reconcile only under identical snapshot + filter scope. A project **due exactly on 2026-09-30**
is *not* overdue (strict `<`), which is why overdue = 23, not 24.

---

## 6. Build checklist
1. Get Data → PostgreSQL → `localhost` / `MahamDB` (Import) → auth `abdulqader`.
2. Load the 5 `sql/` statements (+ base dimensions) ; add the `Calendar` table and mark it as a date table.
3. Create the relationships in the table above (single direction).
4. Add the measures from `DAX_measures.md` and the `Snapshot` parameter.
5. Build the KPI row, 6 visuals and 4+ slicers; wire the two drill-through pages.
6. Confirm the 5 KPIs equal the reconciliation table, then save as `PMO_Portal.pbix`.
