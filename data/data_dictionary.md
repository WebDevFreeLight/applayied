# PMO Portal — Data Dictionary

Every CSV maps to one database table. `*_id` columns are 64-bit keys. Empty cell = SQL NULL.
Dates are ISO-8601 (`YYYY-MM-DD`). Amounts are decimal AED. Derived KPIs are **not** stored —
they are computed at query time from these source-of-truth rows.

## Identity (seeded into reused `sys_*` tables)

### departments.csv → `sys_dept`
| Column | Type | Notes |
|--------|------|-------|
| DeptId | long PK | 101–104 |
| DeptName | string | English name |
| DeptNameAr | string | Arabic name (frontend bilingual label; not a DB column on sys_dept) |
| Leader | string | Department leader (display only) |

### users.csv → `sys_user` + `sys_user_role`
| Column | Type | Notes |
|--------|------|-------|
| UserId | long PK | 1001–1015 |
| UserName | string, unique | login account |
| NickName / NickNameAr | string | display name (EN / AR) |
| DeptId | long FK → sys_dept | |
| RoleKey | string | `pmo_admin` \| `pmo_pm` \| `pmo_contributor` → sys_user_role |
| Sex | int | 0 male, 1 female |

## Portfolio

### projects.csv → `pmo_project`
| Column | Type | Notes |
|--------|------|-------|
| ProjectId | long PK | |
| Code | string, unique | e.g. `PRJ-CBM` |
| Name | string | |
| DeptId | long FK → sys_dept | owning department |
| OwnerUserId | long FK → sys_user | project owner (a PM) |
| Status | enum | Planning, Active, OnHold, Completed, Cancelled |
| Priority | enum | Low, Medium, High, Critical |
| StartDate / PlannedEndDate | date | |
| ActualEndDate | date, nullable | set only when the project is closed |
| Description | string | |
| InitialApprovedBudget | decimal | baseline approved budget (revisions apply on top) |

### project_members.csv → `pmo_project_member`
| Column | Type | Notes |
|--------|------|-------|
| MemberId | long PK | |
| ProjectId | long FK | |
| UserId | long FK → sys_user | unique per (ProjectId, UserId) |
| RoleInProject | string | Developer, Analyst, QA, Lead — drives Contributor scoped access |

## Delivery

### tasks.csv → `pmo_task`
| Column | Type | Notes |
|--------|------|-------|
| TaskId | long PK | |
| ProjectId | long FK | |
| Title | string | |
| AssigneeUserId | long FK, **nullable** | NULL = unassigned (edge case present) |
| Status | enum | ToDo, InProgress, Blocked, Done |
| Priority | enum | Low…Critical |
| StartDate / DueDate | date, nullable | overdue = DueDate < cutoff AND Status ≠ Done |
| Weight | decimal | relative size for weighted progress |
| ProgressPct | int 0–100 | |
| Version | long | optimistic-concurrency token |

### milestones.csv → `pmo_milestone`
| Column | Type | Notes |
|--------|------|-------|
| MilestoneId | long PK | ProjectId FK; Name; DueDate; Status = Pending/Achieved/Missed |

## Governance

### risks.csv → `pmo_risk`
| Column | Type | Notes |
|--------|------|-------|
| RiskId | long PK | |
| ProjectId | long FK | |
| Kind | enum | Risk \| Issue |
| OwnerUserId | long FK | |
| Probability / Impact | int 1–5 | |
| Score | int | = Probability × Impact (high exposure ≥ 12) |
| Status | enum | Open, Mitigating, Closed (Closed never counts as open) |

### change_requests.csv → `pmo_change_request`
| Column | Type | Notes |
|--------|------|-------|
| ChangeRequestId | long PK | |
| ProjectId FK; Title; Description; RequestedByUserId FK | | |
| Status | enum | Draft, Submitted, UnderReview, Approved, Rejected, Returned |
| DecisionByUserId | long FK, nullable | never equals RequestedByUserId (no self-approval) |
| DecisionReason / DecisionAt | string / datetime | |
| EstimatedCostImpact | decimal, nullable | informational — does **not** auto-post to budget |
| Version | long | optimistic concurrency |

### change_history.csv → `pmo_change_history`
Append-only audit of every transition: `FromStatus`, `ToStatus`, `ActorUserId`, `Reason`, `ActedAt`.

## Finance

### cost_categories.csv → `pmo_cost_category`
`CategoryId` PK, `Code` unique, `Name` (Labour, Hardware, Software, Services).

### budget_revisions.csv → `pmo_budget_revision`
| Column | Notes |
|--------|-------|
| RevisionId PK, ProjectId FK, Amount (signed delta), Reason | |
| Status | Pending, Approved, Rejected — **only Approved changes the current budget** |
| RequestedByUserId, ApprovedByUserId, ApprovedAt | |

### cost_transactions.csv → `pmo_cost_transaction`
| Column | Notes |
|--------|-------|
| TransactionId PK, ProjectId FK, CategoryId FK, Amount (>0), TxnDate, Description | |
| Status | Draft, Posted — **only Posted on/before cutoff = actual spend** |
| PostedByUserId, PostedAt | audit of who posted and when |

### financial_forecasts.csv → `pmo_financial_forecast`
`ForecastId` PK, `ProjectId` FK, `PeriodMonth` (1st of month), `ForecastAmount`,
`Kind` = `ForecastToComplete` | `MonthlyEstimate`. Unique (ProjectId, PeriodMonth, Kind).

## Automation / logging

| File → table | Key columns |
|--------------|-------------|
| progress_snapshots.csv → `pmo_progress_snapshot` | SnapshotDate, ProgressPct, TasksTotal, TasksDone, OpenRiskCount |
| automation_runs.csv → `pmo_automation_run` | RuleKey, TriggeredAt, Status, Detail, **DedupeKey (unique → no double-run)** |
| outbox_notifications.csv → `pmo_outbox_notification` | Type, Subject, Body, TargetUserId, Status, **DedupeKey (unique → no duplicate send)** |
| audit_events.csv → `pmo_audit_event` | EntityType, EntityId, Action, ActorUserId, Detail, ActedAt |

## Intentional edge cases (for grading)

| Edge case | Where |
|-----------|-------|
| Project with no posted spend | `PRJ-MOB` (draft costs only) |
| Project nearing its budget | `PRJ-NET` (~92% utilization) |
| Project forecast to exceed budget | `PRJ-AII` (EAC > approved budget) |
| Completed project | `PRJ-LEG` (ActualEndDate set) |
| Project without forecast data | `PRJ-DLP` (EAC falls back to actual, flagged) |
| ≥5 overdue incomplete tasks | 24 present |
| Task due exactly on cutoff | "Milestone gate review (due on snapshot)" |
| Unassigned task | "Backlog grooming (unassigned)" |
| Closed high-exposure risk (not open) | "Core migration data loss" (score 20, Closed) |
| Open high-exposure risk | "AI model accuracy below target" (score 25, Open) |
| Returned / Rejected change request | "Extend timeline 4 weeks" (Returned), "Switch vendor" (Rejected) |
| Approved + unapproved budget revision | `PRJ-SEC` (Approved +150k, Pending +100k); `PRJ-AII` (Rejected) |
| Posted + draft costs | every project has both |
| Posted cost after the cutoff | `PRJ-CBM` hardware on 2026-10-15 (excluded at baseline) |
| Multiple departments & periods | 4 departments, dates across 2025–2027 |
