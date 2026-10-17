# PMO — Project Portfolio & Financial Control Portal · Schema & Design

This module is added **inside** the existing ZRAdmin.NET solution (fork/extend), reusing its
SqlSugar ORM, JWT auth, `sys_user` / `sys_role` identity, DI scanning and audit-log infrastructure.
It targets **PostgreSQL** (`DbType = 4`).

## 1. Design principles

- **Source-of-truth vs derived.** Transactional records (`pmo_cost_transaction`, `pmo_budget_revision`,
  `pmo_task`, `pmo_change_request`…) are stored. All KPIs (actual spend, remaining budget, utilization,
  EAC, completion %) are **derived at query time** from SQL aggregations — never stored as denormalised
  totals. This is what lets the dashboard "recompute from SQL" after a task update or a posted expense.
- **One connected graph.** Everything hangs off `pmo_project.project_id`. Users come from the existing
  `sys_user`; departments from `sys_dept`; roles from `sys_role`. No parallel user list.
- **Avoid fan-out double counting.** Tasks, risks and costs are each 1-to-many from project. We never
  JOIN two of those one-to-many children in a single query (that would multiply rows and inflate
  spend/budget). Aggregates are computed per child table and then composed by `project_id`.

## 2. Identity & access (reused tables)

| Table | Role here |
|-------|-----------|
| `sys_user` | People. Owners, assignees, risk owners, requesters, approvers. Login via existing JWT (password = lowercase MD5). |
| `sys_dept` | Departments (≥3). `pmo_project.dept_id` → `sys_dept`. Dashboard "department" filter. |
| `sys_role` | The three PMO roles, by `role_key`: `pmo_admin`, `pmo_pm`, `pmo_contributor`. Carried in the JWT as `RoleKeys`. |

RBAC is enforced server-side by a PMO action filter (role check) **plus** in-service business rules
(project scope, self-approval prevention, draft→posted control). See `02_RBAC_AND_SECURITY.md`.

## 3. Entities (new `pmo_*` tables)

All inherit `SysBase` → `Create_by`, `Create_time`, `Update_by`, `Update_time`, `Remark`.
PKs are `long` (snowflake, `IsIdentity=false`). FKs are indexed for join/filter.

### People / portfolio
- **pmo_project** — `project_id` PK, `code` (unique), `name`, `dept_id`→sys_dept, `owner_user_id`→sys_user,
  `status` {Planning, Active, OnHold, Completed, Cancelled}, `priority` {Low, Medium, High, Critical},
  `start_date`, `planned_end_date`, `actual_end_date` (null until closed), `description`,
  `initial_approved_budget` decimal(18,2), `currency` (AED). **Index:** dept_id, owner_user_id, status.
- **pmo_project_member** — `member_id` PK, `project_id`→project, `user_id`→sys_user,
  `role_in_project`. Unique(project_id, user_id). Drives Contributor scoped access.

### Delivery
- **pmo_task** — `task_id` PK, `project_id`→project, `title`, `assignee_user_id`→sys_user (nullable →
  the required *unassigned* edge case), `status` {ToDo, InProgress, Blocked, Done}, `priority`,
  `start_date`, `due_date`, `weight` decimal(6,2) (relative size), `progress_pct` int 0–100,
  `version` (optimistic concurrency). **Index:** project_id, assignee_user_id, status, due_date.
- **pmo_milestone** — `milestone_id` PK, `project_id`→project, `name`, `due_date`,
  `status` {Pending, Achieved, Missed}. **Index:** project_id, due_date.
- **pmo_progress_snapshot** — `snapshot_id` PK, `project_id`→project, `snapshot_date`,
  `progress_pct`, `tasks_total`, `tasks_done`, `open_risk_count`. Historical trend (derived-at-capture).

### Governance
- **pmo_risk** — `risk_id` PK, `project_id`→project, `kind` {Risk, Issue}, `title`, `owner_user_id`,
  `probability` 1–5, `impact` 1–5, `score` = probability × impact (set by service),
  `mitigation`, `due_date`, `status` {Open, Mitigating, Closed}. **Index:** project_id, status, score.
- **pmo_change_request** — `change_request_id` PK, `project_id`→project, `title`, `description`,
  `requested_by_user_id`, `status` {Draft, Submitted, UnderReview, Approved, Rejected, Returned},
  `decision_by_user_id`, `decision_reason`, `decision_at`, `estimated_cost_impact` decimal(18,2),
  `version`. **Index:** project_id, status.
- **pmo_change_history** — `history_id` PK, `change_request_id`→change_request, `from_status`,
  `to_status`, `actor_user_id`, `reason`, `acted_at`. Durable append-only audit of every transition.

### Finance
- **pmo_cost_category** — `category_id` PK, `code` (unique), `name`. (≥4: Labour, Hardware, Software,
  Services.)
- **pmo_budget_revision** — `revision_id` PK, `project_id`→project, `amount` decimal(18,2) (signed delta),
  `reason`, `status` {Pending, Approved, Rejected}, `requested_by_user_id`, `approved_by_user_id`,
  `approved_at`. Only **Approved** rows change the current approved budget.
- **pmo_cost_transaction** — `transaction_id` PK, `project_id`→project, `category_id`→cost_category,
  `amount` decimal(18,2) (>0), `txn_date`, `description`, `status` {Draft, Posted},
  `posted_by_user_id`, `posted_at`. Only **Posted** rows count as actual spend. **Index:** project_id,
  category_id, status, txn_date.
- **pmo_financial_forecast** — `forecast_id` PK, `project_id`→project, `period_month` (1st of month),
  `forecast_amount` decimal(18,2), `kind` {MonthlyEstimate, ForecastToComplete}. Unique(project_id,
  period_month, kind).

### Automation / logging
- **pmo_automation_run** — `run_id` PK, `rule_key`, `triggered_at`, `status` {Success, Failed, Skipped},
  `detail`, `dedupe_key` (**unique** → idempotency / no double-run).
- **pmo_outbox_notification** — `notification_id` PK, `type`, `subject`, `body`, `target_user_id`,
  `created_at`, `sent_at`, `status` {Pending, Sent, Failed}, `dedupe_key` (**unique** → no duplicate send).

## 4. Financial definitions (applied consistently everywhere)

- **Current approved budget** = `initial_approved_budget` + Σ(approved budget revisions). Pending/rejected = 0 effect.
- **Actual spend** = Σ(posted cost transactions with `txn_date` ≤ reporting cutoff). Draft excluded.
- **Remaining budget** = current approved budget − actual spend.
- **Utilization** = actual spend ÷ current approved budget (0 when budget = 0 → reported as `null`/0, never ÷0).
- **EAC** = actual spend + forecast-to-complete (Σ of `ForecastToComplete` rows, or Σ future `MonthlyEstimate`
  when no FTC exists; a project with no forecast → EAC = actual spend, flagged "no forecast").
- **Projected variance** = current approved budget − EAC. Negative ⇒ projected overrun.
- Approved change requests do **not** auto-post spend or auto-create revisions — any link is explicit & audited.

## 5. Reporting snapshot

Baseline demos use a fixed cutoff **2026-09-30**. Production passes "today" or a user-selected date to the
same aggregation endpoints (the cutoff is a parameter, not a constant).
