# PMO — Project Portfolio & Financial Control Portal

An end-to-end business application: **React + Vite** frontend → **.NET (SqlSugar)** API →
**PostgreSQL**, with server-side RBAC, a live SQL-backed executive dashboard, a finance module
with defensible calculations, a controlled change-request workflow, and bilingual (English /
Arabic, RTL) UI and API responses.

```
applayied/
├── backend/     .NET solution (ZRAdmin fork). PMO module under ZR.*/PMO, Controllers/PMO.
├── frontend/    React + Vite + TypeScript SPA (EN/AR, RTL, ECharts dashboard).
├── data/        Reproducible Python data generator + CSVs + data dictionary.
└── docs/        API route list and supporting docs.
```

## Architecture at a glance

- **Source-of-truth vs derived.** Transactions, tasks, revisions… are stored. Every KPI (actual
  spend, remaining budget, utilization, EAC, completion %) is **computed at query time** from SQL
  aggregations — the dashboard recomputes after a task update or a posted expense, no client-side
  KPI patching. See `backend/Documentation/PMO/01_SCHEMA_AND_DESIGN.md`.
- **Server-enforced RBAC.** `PmoAuthorizeAttribute` reads the JWT and enforces a role floor on
  every PMO route (a Contributor calling the approval/posting API directly gets 403 regardless of
  the UI), plus in-service rules (project scope, no self-approval, draft→posted control).
- **Atomic mutations.** Change-request decisions commit the decision **and** its history in one
  transaction; posting a cost commits the post **and** its audit entry together.
- **Bilingual.** `Accept-Language: ar|en` (or `?lang=ar`) localizes API error/validation/RBAC
  messages; the SPA switches language and RTL direction live.

## Prerequisites
- .NET SDK 8, Node.js 18+, PostgreSQL 14+ (local or Docker), Python 3.8+.

## 1 — Database & backend
1. Create a PostgreSQL database and point `backend/ZR.Admin.WebApi/appsettings.json` →
   `dbConfigs[0].Conn` at it (`DbType: 4`). Secrets stay out of source control via appsettings /
   environment overrides.
2. First run only: set `"InitDb": true` so SqlSugar creates every `pmo_*` table, then set it back
   to `false`.
3. Generate and import the demo data (see `data/README.md`): either set `"PmoSeed": true` once, or
   call `POST /pmo/admin/seed` as an administrator. This seeds 4 departments, 3 PMO roles, 15 demo
   users and all `pmo_*` rows (idempotent & transactional).
4. Run the API: `dotnet run --project backend/ZR.Admin.WebApi`.

## 2 — Frontend
```bash
cd frontend
npm install
npm run dev        # http://localhost:5173, proxies /api -> backend (VITE_API_TARGET)
```
`cp .env.example .env` to change the API target/base.

## 2b — Machine learning (optional)
Five scikit-learn models trained on this same PostgreSQL — task delay risk, risk severity, monthly spend
forecast, cost anomalies and portfolio segments — served to the **ML Insights** page through `/pmo/ml/*`.
```bash
powershell -ExecutionPolicy Bypass -File ml\scripts\setup.ps1      # once
powershell -ExecutionPolicy Bypass -File ml\scripts\run_pipeline.ps1
ml\.venv\Scripts\python -m pmo_ml serve                            # http://localhost:8001/docs
```
Full bilingual (EN/AR) tutorial, one file per ML lifecycle step: [`ml/README.md`](ml/README.md).

## 3 — Demo logins (password `123456`)
| Username | Role | Can do |
|----------|------|--------|
| `pmo.admin` | Administrator | everything incl. approve budget revisions, seed, config |
| `pm.hamdan` | Project Manager | manage projects/tasks/risks, post costs, decide change requests |
| `con.mariam` | Contributor | update own task progress, submit change requests (no posting/approval) |

## Reporting snapshot
Baseline demos use the fixed cutoff **30 September 2026**; the same endpoints accept "today" or a
user-selected date (the cutoff is a query parameter). `data/reconciliation.md` lists the KPI values
at that snapshot for cross-checking SQL ↔ dashboard ↔ Power BI.

## API
See `docs/API_ROUTES.md` for the full route list (methods, role floor, purpose).

## Status / remaining deliverables
Implemented: schema + ORM models, RBAC + action filter, data generator + importer, bilingual API
& UI, all module CRUD, finance module, change-request workflow, executive dashboard (7 visuals, 6
filters, 2 drill-downs). Not yet built: automated test suite, automation worker/outbox wiring,
Power BI `.pbix`, Access/VBA utility, AI development log and slides.
