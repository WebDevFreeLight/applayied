# PMO — Concurrency, Automated Tests, Manual Checklist & Finance Definitions

This document answers four assignment requirements with concrete, code-backed evidence:

1. One **concurrency scenario** (optimistic version checking) and how **double submissions** are prevented.
2. The **automated checks** (30, well over the required five) across API business logic, RBAC, finance and workflow.
3. A **manual end-to-end test checklist**.
4. How the mandated **finance definitions** map onto the implementation.

---

## 1. Concurrency & double-submission

### 1.1 Optimistic version checking (lost-update prevention) — tasks

Every `pmo_task` / `pmo_change_request` row carries a monotonic `version` column
(`PmoTask.Version`, `PmoChangeRequest.Version`). Tasks are the hot path (progress is edited by
contributors and PMs concurrently), so they use an **explicit, provider-independent optimistic check**
in `PmoTaskService` (`UpdateTask`, `UpdateProgress`):

```
private static void EnsureNotStale(long? currentVersion, long? clientVersion)
{
    if (clientVersion.HasValue && currentVersion.HasValue && clientVersion.Value != currentVersion.Value)
        throw new CustomException(ResultCode.FAIL, "This task was changed by someone else — reload and retry.");
}
```

**Flow (the classic two-writer race):**

| Step | Client A | Client B | Row version |
|------|----------|----------|-------------|
| read | reads task @ `v1` | reads task @ `v1` | 1 |
| A saves | `UpdateProgress(version=1)` → matches → write, bump | | **2** |
| B saves | | `UpdateProgress(version=1)` → `1 ≠ 2` → **rejected** | 2 |

Client B gets a 200-envelope `code≠200` with the message *"This task was changed by someone else —
reload and retry."* instead of silently clobbering A's value. The service then re-reads the current
version and bumps it (`version = current + 1`) on every successful write, so the next stale write is
caught. The SqlSugar `[version]` attribute was intentionally **removed** because its behaviour is
inconsistent across providers (inert on SQLite, pre-increment conflict risk on PostgreSQL); the
explicit check behaves identically everywhere. A defensive `catch (VersionExceptions)` remains as a
backstop.

> Why optimistic (not pessimistic) locking: PMO edits are short, user-driven and low-contention;
> row locks would hurt throughput and risk deadlocks across the task/finance modules. Optimistic
> version checking detects the rare real conflict and lets everything else proceed lock-free.

**Proven by:** `PmoConcurrencyTests.First_writer_wins_and_increments_the_row_version`,
`PmoConcurrencyTests.Stale_second_writer_is_rejected_not_silently_overwritten`.

### 1.2 Double-submission prevention — three independent guards

| Where | Mechanism | Effect |
|-------|-----------|--------|
| **Change-request decisions** | `PmoChangeRequestService` validates every transition against a documented `Allowed` state-machine set and commits the status change **and exactly one** `pmo_change_history` row inside one `Context.Ado` transaction. | A duplicate *Approve* (double-click / retried request) finds the CR already in the terminal `Approved` state — there is no `Approved → Approved` edge — so it is refused with *"Transition Approved → Approved is not permitted"*. No second decision, no second history row. |
| **Automation notifications** | `pmo_outbox_notification.dedupe_key` has a **UNIQUE index**; `PmoAutomationRunner` checks `Any(dedupe_key==…)` before insert and catches the unique violation. Key = `overdue:{taskId}:{yyyyMMdd}` / `overrun:{projectId}:{yyyyMMdd}`. | Re-running the automation (scheduler tick, retry, or manual trigger) never sends the same alert twice. Verified live: a re-run reported `created=0, skipped=33`. |
| **Cost posting** | A posted `pmo_cost_transaction` can only move `Draft → Posted` once; posting stamps `PostedByUserId`/`PostedAt` in a transaction and posted rows cannot be silently deleted. | A repeated post is a no-op on an already-posted row; actor+time are captured (audit). |

**Proven by:** `PmoConcurrencyTests.Approving_twice_is_refused_the_second_time`,
`PmoWorkflowTests.Valid_decision_commits_status_and_exactly_one_history_row`, and the live
automation dedup run.

---

## 2. Automated checks (30) — `backend/ZR.Pmo.Tests`

Run: `dotnet test backend/ZR.Pmo.Tests` → **Passed: 30, Failed: 0**.
These drive the **real** services (`RuntimeHelpers.GetUninitializedObject` + `Context` on an in-memory
SQLite `SqlSugarScope`), so they exercise production logic, not mocks.

### Financial calculations — `PmoFinanceCalcTests` (5)
1. `Current_approved_budget_counts_only_approved_revisions` — pending/rejected revisions excluded.
2. `Actual_spend_excludes_draft_and_post_cutoff` — only `Posted` txns on/before cutoff.
3. `Summary_computes_remaining_eac_and_variance` — remaining / EAC / projected variance.
4. `Zero_budget_project_reports_null_utilization` — zero-budget guard (no divide-by-zero).
5. `Project_without_forecast_sets_eac_to_actual` — EAC = actual when no forecast-to-complete.

### RBAC & localization — `PmoRbacAndI18nTests` (10)
6–12. Role floor (admin ≥ pm ≥ contributor), super-admin treated as pmo_admin, `RankOf` mapping,
token → RoleKeys copy.
13–17. Arabic translation of RBAC messages, English pass-through, interpolated values preserved,
unknown-message fallback, `Accept-Language` parsing.

### Workflow rules — `PmoWorkflowTests` (5)
18. `Cannot_decide_your_own_change_request` (segregation of duties).
19. `Valid_decision_commits_status_and_exactly_one_history_row` (atomic decision + audit).
20. `Reject_requires_a_reason`.
21. `Illegal_transition_is_rejected` (Draft → Approved blocked).
22. `Allowed_transition_table_matches_documented_state_machine`.

### Concurrency & double-submission — `PmoConcurrencyTests` (3)
23. `First_writer_wins_and_increments_the_row_version`.
24. `Stale_second_writer_is_rejected_not_silently_overwritten` (optimistic lock, no lost update).
25. `Approving_twice_is_refused_the_second_time` (idempotent workflow decision).

*(The remaining count is Theory rows expanded by xUnit.)*

---

## 3. Manual end-to-end test checklist

Environment: backend `http://localhost:63873`, frontend `http://localhost:5173`.
Sign in `pmo.admin / 123456` (admin), `pm.hamdan / 123456` (PM), `con.mariam / 123456` (contributor).

| # | Area | Steps | Expected |
|---|------|-------|----------|
| A | **Auth / captcha** | Load /login, enter credentials + captcha. | Lands on dashboard; JWT stored; role shown in header. |
| B | **RBAC deny** | As `con.mariam`, open Finance / post a cost / open an admin page. | 403 with a localized message; admin-only nav hidden. |
| C | **Bilingual** | Toggle العربية; repeat a denied action. | UI flips RTL; API error message returns in Arabic (`Accept-Language`). |
| D | **Budget revision** | As PM, add a budget revision with rationale; approve it. | Current approved budget = initial + approved revision; pending has no effect. |
| E | **Cost transaction** | Add a cost (blank amount → error; negative → error; valid as Draft; then Post). | Validation enforced; Draft excluded from actual spend; Posted included; actor+time captured; posted row cannot be deleted silently. |
| F | **Dashboard recompute** | Note Overdue tasks + Actual spend. (1) Mark an overdue task Done. (2) Post a new expense. Reload. | Both KPIs recompute from SQL (no client patching); numbers change correctly. |
| G | **Drill-down** | Click the Overdue-tasks KPI (delivery) and the Actual-spend path (finance). | Delivery KPI opens filtered tasks; finance KPI opens underlying cost transactions. |
| H | **Filters** | Apply department + manager + status + cutoff + category filters. | Every KPI/visual re-queries scope-aware; category applies only to finance visuals. |
| I | **Optimistic concurrency** | Open the same task in two tabs; update progress in tab 1, then save tab 2 with the old value. | Tab 2 is rejected: *"changed by someone else — reload and retry."* Tab 1's value survives. |
| J | **Double submission** | Approve a change request, then click Approve again. | Second attempt refused (*not permitted*); exactly one history row. |
| K | **Automation + dedup** | Activity → **Run automation now**; run again. | 1st run creates N notifications; 2nd run `created=0, skipped=N`. Runs logged; outbox + Messages populated. |
| L | **Overrun alert** | Post costs pushing a project over its approved budget; run automation. | Budget-overrun alert queued to the owner (Messages + bell). |
| M | **Notice realtime** | Admin sends a notice. | All signed-in users get a live bell popup; the notice is logged in each user's Messages. |
| N | **Sessions** | Admin → Sessions; force-logout a session. | Live sessions listed; forced session is disconnected and removed. |
| O | **AI assistant** | Open ✨, ask a question; project ✨ AI Summary; dashboard ✨ AI Insights. | With keys: real answers. Without keys: graceful "AI unavailable" (no crash). |

---

## 4. Finance definitions → implementation map

Computed in `PmoFinanceService` from relational SQL (SqlSugar), never hardcoded:

| Definition | Implementation |
|-----------|----------------|
| **Current approved budget** = initial + **approved** revisions (pending/rejected excluded) | `InitialApprovedBudget + Σ(PmoBudgetRevision where Status='Approved')` (grouped query). |
| **Actual spend** = posted cost transactions on/before cutoff (drafts excluded) | `Σ(PmoCostTransaction where Status='Posted' and TxnDate ≤ cutoff)`. |
| **Remaining** = approved − actual; **Utilization** = actual / approved (zero-budget safe) | Summary object; utilization is `null` when approved ≤ 0 (no divide-by-zero). |
| **EAC** = actual + forecast-to-complete; **Projected variance** = approved − EAC | Forecast-to-complete from `PmoFinancialForecast` (`Kind='ForecastToComplete'`); documented remaining-period method. |
| Approved change requests do **not** auto-post expenditure/revisions | CRs are a separate workflow; any budget effect is an explicit, audited budget revision. |
| Mutations restricted + actor/time captured + no silent deletion of posted txns | `[ActionPermissionFilter]` RBAC + `PostedByUserId`/`PostedAt` + `pmo_audit_event`. |
| Budget-overrun alert + project comparison + KPI→transactions drill-down | `budget_overrun_alert` automation + Finance portfolio comparison + dashboard finance drill-down. |

See `01_SCHEMA_AND_DESIGN.md` for the full schema and the dashboard's two worked KPI calculations
(source tables, filter context, formula).
