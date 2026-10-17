# PMO Portal — API route list

Base URL: the backend origin (dev: `http://localhost:5000`). The frontend calls these through
the Vite proxy at `/api/*`. All responses use the envelope `{ code, msg, data }` (200 = success).
Errors are localized to the request language via `Accept-Language: ar|en` (or `?lang=ar`).

Every PMO route is guarded server-side by `[PmoAuthorize(minRole)]` (role floor) plus in-service
business rules (project scope, no self-approval, draft→posted control). IDs are 64-bit, serialized
as JSON strings.

## Auth
| Method | Route | Min role | Purpose |
|--------|-------|----------|---------|
| POST | `/login` | anonymous | `{username,password}` → `{token, user, roles, permissions}` |

## Lookups (`/pmo/lookup`, `/pmo/cost-category`)
| Method | Route | Min role | Purpose |
|--------|-------|----------|---------|
| GET | `/pmo/lookup/departments` | Contributor | department options |
| GET | `/pmo/lookup/users` | Contributor | user options |
| GET | `/pmo/lookup/managers` | Contributor | PM/owner options |
| GET | `/pmo/lookup/projects` | Contributor | project options (scoped) |
| GET | `/pmo/cost-category/list` | Contributor | cost categories |
| POST/PUT | `/pmo/cost-category` | Admin | create/update category |

## Projects (`/pmo/project`)
| Method | Route | Min role | Purpose |
|--------|-------|----------|---------|
| GET | `/pmo/project/list` | Contributor | search/filter/paged (keyword, deptId, ownerUserId, status, dates) |
| GET | `/pmo/project/{projectId}` | Contributor | details (scoped) |
| POST | `/pmo/project` | PM | create |
| PUT | `/pmo/project` | PM | update (owner/admin only) |

## Tasks (`/pmo/task`)
| Method | Route | Min role | Purpose |
|--------|-------|----------|---------|
| GET | `/pmo/task/list` | Contributor | filter by project/status/assignee |
| GET | `/pmo/task/{taskId}` | Contributor | details |
| POST / PUT | `/pmo/task` | PM | create / update |
| PUT | `/pmo/task/{taskId}/progress` | Contributor | update own task progress (optimistic version) |

## Milestones / Risks
| Method | Route | Min role | Purpose |
|--------|-------|----------|---------|
| GET | `/pmo/milestone/list`, `/pmo/milestone/{id}` | Contributor | list / details |
| POST / PUT | `/pmo/milestone` | PM | create / update |
| GET | `/pmo/risk/list`, `/pmo/risk/{id}` | Contributor | list (highExposureOnly filter) / details |
| POST / PUT | `/pmo/risk` | PM | create / update (score = probability × impact) |

## Change requests (`/pmo/change-request`)
| Method | Route | Min role | Purpose |
|--------|-------|----------|---------|
| GET | `/pmo/change-request/list` | Contributor | list |
| GET | `/pmo/change-request/{id}` | Contributor | details |
| GET | `/pmo/change-request/{id}/history` | Contributor | append-only transition history |
| POST | `/pmo/change-request` | Contributor | create (Draft) |
| POST | `/pmo/change-request/{id}/submit` | Contributor | submit (requester only) |
| POST | `/pmo/change-request/decision` | PM | approve/reject/return (never own request; atomic decision+history) |

## Finance (`/pmo/finance`)
| Method | Route | Min role | Purpose |
|--------|-------|----------|---------|
| GET | `/pmo/finance/summary/{projectId}?cutoff=` | Contributor | budget/spend/EAC/variance at cutoff |
| GET | `/pmo/finance/transactions` | Contributor | list cost transactions |
| POST / PUT | `/pmo/finance/transactions` | PM | add / edit draft cost |
| POST | `/pmo/finance/transactions/{id}/post` | PM | post cost (atomic post + audit) |
| GET | `/pmo/finance/revisions` | Contributor | list budget revisions |
| POST | `/pmo/finance/revisions` | PM | request revision |
| POST | `/pmo/finance/revisions/decision` | Admin | approve/reject revision |
| GET | `/pmo/finance/forecasts/{projectId}` | Contributor | forecasts |
| POST | `/pmo/finance/forecasts` | PM | add forecast |
| GET | `/pmo/finance/drilldown` | Contributor | KPI → underlying transactions |

## Dashboard (`/pmo/dashboard`) — management (PM/Admin)
| Method | Route | Purpose |
|--------|-------|---------|
| GET | `/pmo/dashboard/kpis` | delivery/risk/finance KPI cards (filter: deptId, projectId, managerId, status, categoryId, cutoff) |
| GET | `/pmo/dashboard/analytics` | 7 visuals: project health, budget-vs-actual-vs-EAC, monthly trend, spend by category, risk matrix, overdue by owner, milestone roadmap |
| GET | `/pmo/dashboard/drilldown/tasks?mode=all\|overdue\|blocked` | delivery drill-down → tasks |
| GET | `/pmo/dashboard/drilldown/risks?highExposureOnly=` | delivery drill-down → risks |

## Admin (`/pmo/admin`)
| Method | Route | Min role | Purpose |
|--------|-------|----------|---------|
| POST | `/pmo/admin/seed` | Admin | import the synthetic data package (idempotent, transactional) |
