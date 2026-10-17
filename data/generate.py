#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
PMO Portal — reproducible synthetic data generator.

Produces deterministic CSV files (fixed random seed) for every pmo_* table plus the
identity rows (departments + demo users) needed for role-based sign-in. CSV headers are
the C# *entity property names* so the backend seeder (PmoDataSeeder) can import them via
SqlSugar — the ORM owns the Postgres column mapping, so this file never guesses column
casing.

Design rules that mirror the backend's financial definitions (so the web dashboard,
SQL and Power BI reconcile to the same numbers at the snapshot date):

  current approved budget = initial_approved_budget + Σ(Approved budget revisions)
  actual spend            = Σ(Posted cost transactions with TxnDate <= snapshot)
  remaining budget        = current approved budget − actual spend
  utilization             = actual spend / current approved budget   (0-budget → None)
  EAC                     = actual spend + Σ(ForecastToComplete)  (no forecast → EAC = actual, flagged)
  projected variance      = current approved budget − EAC

Baseline reporting snapshot: 30 September 2026 (production passes "today"/user date instead).

Usage:  python generate.py        # writes ./csv/*.csv, data_dictionary.md, reconciliation.md
"""

import csv
import os
import random
from datetime import date, timedelta

# --------------------------------------------------------------------------------------
# Configuration
# --------------------------------------------------------------------------------------
SEED = 20260930                       # fixed seed → identical output every run
SNAPSHOT = date(2026, 9, 30)          # baseline reporting cutoff
CURRENCY = "AED"
HERE = os.path.dirname(os.path.abspath(__file__))
CSV_DIR = os.path.join(HERE, "csv")
SEED_BY = "generator"

random.seed(SEED)
os.makedirs(CSV_DIR, exist_ok=True)


def iso(d):
    """date/datetime -> ISO string; None -> '' (empty cell = SQL NULL on import)."""
    return "" if d is None else d.isoformat()


def write_csv(name, headers, rows):
    path = os.path.join(CSV_DIR, name)
    with open(path, "w", newline="", encoding="utf-8-sig") as f:
        w = csv.writer(f)
        w.writerow(headers)
        for r in rows:
            w.writerow(["" if v is None else v for v in r])
    print(f"  {name:32s} {len(rows):4d} rows")


def d(y, m, day):
    return date(y, m, day)


# --------------------------------------------------------------------------------------
# 1. Departments  (>= 3 required)  — seeded into sys_dept by the backend importer
# --------------------------------------------------------------------------------------
# DeptNameAr is carried for the bilingual UI; the backend stores English DeptName and the
# frontend resolves the Arabic label from a lookup, keeping FK joins simple.
DEPARTMENTS = [
    # DeptId, DeptName,                 DeptNameAr,                 Leader
    (101, "IT Delivery",            "تسليم تقنية المعلومات",      "Hamdan Al Zaabi"),
    (102, "Infrastructure",         "البنية التحتية",             "Sara Al Hashimi"),
    (103, "Digital Transformation", "التحول الرقمي",              "Yousef Al Marri"),
    (104, "Project Management Office", "مكتب إدارة المشاريع",      "Noura Al Suwaidi"),
]

# --------------------------------------------------------------------------------------
# 2. Demo users  (>= 15, distributed across the 3 PMO roles) — seeded into sys_user
# --------------------------------------------------------------------------------------
# RoleKey drives sys_user_role; password for every demo account is "123456"
# (stored uppercase-MD5 by the backend importer).  2 admins, 5 PMs, 8 contributors.
USERS = [
    # UserId, UserName,   NickName,            NickNameAr,            DeptId, RoleKey,          Sex
    (1001, "pmo.admin",   "Omar Al Blooshi",   "عمر البلوشي",         104, "pmo_admin",       0),
    (1002, "pmo.admin2",  "Aisha Al Nuaimi",   "عائشة النعيمي",       104, "pmo_admin",       1),
    (1003, "pm.hamdan",   "Hamdan Al Zaabi",   "حمدان الزعابي",       101, "pmo_pm",          0),
    (1004, "pm.sara",     "Sara Al Hashimi",   "سارة الهاشمي",        102, "pmo_pm",          1),
    (1005, "pm.yousef",   "Yousef Al Marri",   "يوسف المري",          103, "pmo_pm",          0),
    (1006, "pm.fatima",   "Fatima Al Qubaisi", "فاطمة القبيسي",       101, "pmo_pm",          1),
    (1007, "pm.khalid",   "Khalid Al Shamsi",  "خالد الشامسي",        102, "pmo_pm",          0),
    (1008, "con.mariam",  "Mariam Al Ali",     "مريم العلي",          101, "pmo_contributor", 1),
    (1009, "con.rashid",  "Rashid Al Falasi",  "راشد الفلاسي",        101, "pmo_contributor", 0),
    (1010, "con.layla",   "Layla Al Mazrouei", "ليلى المزروعي",       102, "pmo_contributor", 1),
    (1011, "con.saeed",   "Saeed Al Ketbi",    "سعيد الكتبي",         102, "pmo_contributor", 0),
    (1012, "con.huda",    "Huda Al Dhaheri",   "هدى الظاهري",         103, "pmo_contributor", 1),
    (1013, "con.ali",     "Ali Al Hammadi",    "علي الحمادي",         103, "pmo_contributor", 0),
    (1014, "con.amna",    "Amna Al Rumaithi",  "آمنة الرميثي",        104, "pmo_contributor", 1),
    (1015, "con.tariq",   "Tariq Al Jaberi",   "طارق الجابري",        104, "pmo_contributor", 0),
]
PM_IDS = [u[0] for u in USERS if u[5] == "pmo_pm"]
CON_IDS = [u[0] for u in USERS if u[5] == "pmo_contributor"]
ADMIN_IDS = [u[0] for u in USERS if u[5] == "pmo_admin"]

# --------------------------------------------------------------------------------------
# 3. Cost categories  (>= 4 required)
# --------------------------------------------------------------------------------------
CATEGORIES = [
    (8001, "LAB", "Labour"),
    (8002, "HW",  "Hardware"),
    (8003, "SW",  "Software"),
    (8004, "SVC", "Services"),
]
CAT_IDS = [c[0] for c in CATEGORIES]

# --------------------------------------------------------------------------------------
# 4. Projects  (>= 8)  — edge cases placed explicitly (see comments)
# --------------------------------------------------------------------------------------
# cols: ProjectId, Code, Name, DeptId, OwnerUserId, Status, Priority,
#       StartDate, PlannedEndDate, ActualEndDate, Description, InitialApprovedBudget
PROJECTS = [
    (2001, "PRJ-CBM",  "Core Banking Modernization", 101, 1003, "Active",    "Critical",
     d(2025, 10, 1), d(2026, 12, 31), None,
     "Replace the legacy core banking platform with a modern service-based system.", 5000000),
    # Nearing budget: actual spend ~95% of approved budget
    (2002, "PRJ-NET",  "Network Backbone Upgrade",   102, 1004, "Active",    "High",
     d(2025, 11, 15), d(2026, 10, 31), None,
     "Upgrade the inter-site network backbone to 100G and add redundancy.", 2000000),
    # Forecast to exceed budget: EAC > current approved budget (projected overrun)
    (2003, "PRJ-AII",  "AI Customer Insights",       103, 1005, "Active",    "High",
     d(2026, 1, 10), d(2026, 11, 30), None,
     "Build an AI platform for customer behaviour insights and churn prediction.", 1500000),
    (2004, "PRJ-HRP",  "HR Self-Service Portal",     104, 1006, "Active",    "Medium",
     d(2026, 2, 1), d(2026, 10, 15), None,
     "Employee self-service portal for leave, payslips and HR requests.", 800000),
    # No forecast data (treatment: EAC falls back to actual spend, flagged "no forecast")
    (2005, "PRJ-DLP",  "Data Lake Platform",         103, 1005, "OnHold",    "Medium",
     d(2025, 9, 1), d(2027, 3, 31), None,
     "Central data lake consolidating enterprise sources for analytics.", 3000000),
    # Has an Approved and an unapproved (Pending/Rejected) budget revision
    (2006, "PRJ-SEC",  "Cybersecurity Hardening",    102, 1007, "Active",    "Critical",
     d(2025, 12, 1), d(2026, 12, 15), None,
     "Enterprise-wide security hardening and zero-trust rollout.", 1200000),
    # No POSTED spend (only draft costs) — demonstrates draft/posted distinction
    (2007, "PRJ-MOB",  "Mobile App Revamp",          101, 1003, "Planning",  "Low",
     d(2026, 9, 1), d(2027, 1, 31), None,
     "Redesign and rebuild the customer mobile application.", 600000),
    # Completed project — actual_end_date set, status Completed
    (2008, "PRJ-LEG",  "Legacy Decommissioning",     101, 1006, "Completed", "Medium",
     d(2025, 6, 1), d(2026, 6, 30), d(2026, 6, 25),
     "Decommission retired legacy applications and migrate residual data.", 900000),
]
PROJECT_IDS = [p[0] for p in PROJECTS]

# --------------------------------------------------------------------------------------
# 5. Project members  (drives Contributor scoped access)
# --------------------------------------------------------------------------------------
members = []
mid = 3001
member_map = {}  # project_id -> [user_ids]
for p in PROJECTS:
    pid, dept = p[0], p[3]
    # contributors in the same department become members; owner is implicitly a member
    dept_cons = [u[0] for u in USERS if u[4] == dept and u[5] == "pmo_contributor"]
    picks = dept_cons[:] or random.sample(CON_IDS, 2)
    # add one cross-department contributor to enable cross-dept filtering scenarios
    extra = random.choice([c for c in CON_IDS if c not in picks])
    picks = picks + [extra]
    member_map[pid] = picks
    for uid in picks:
        role_in = random.choice(["Developer", "Analyst", "QA", "Lead"])
        members.append((mid, pid, uid, role_in))
        mid += 1

# --------------------------------------------------------------------------------------
# 6. Tasks  (>= 80)  with required edge cases
# --------------------------------------------------------------------------------------
TASK_STATUSES = ["ToDo", "InProgress", "Blocked", "Done"]
PRIORITIES = ["Low", "Medium", "High", "Critical"]
TASK_TITLES = [
    "Requirements workshop", "Solution design", "API development", "Database schema",
    "UI implementation", "Integration testing", "Security review", "Performance tuning",
    "Data migration", "User acceptance testing", "Deployment runbook", "Documentation",
    "Stakeholder sign-off", "Vendor coordination", "Training materials", "Go-live support",
]

tasks = []
tid = 4001


def add_task(pid, title, assignee, status, priority, start, due, weight, progress, version=1):
    global tid
    tasks.append((tid, pid, title, assignee, status, priority,
                  iso(start), iso(due), weight, progress, version))
    tid += 1


# --- explicit edge-case tasks -------------------------------------------------------
# 5+ overdue incomplete tasks (DueDate < snapshot and status != Done)
for i in range(6):
    pid = PROJECT_IDS[i % 6]
    add_task(pid, f"{random.choice(TASK_TITLES)} (overdue)",
             random.choice(member_map[pid]),
             random.choice(["ToDo", "InProgress", "Blocked"]),
             random.choice(["High", "Critical"]),
             SNAPSHOT - timedelta(days=60), SNAPSHOT - timedelta(days=10 + i),
             weight=2, progress=random.choice([0, 20, 40, 60]))
# 1 task due exactly on the reporting/snapshot date
add_task(2001, "Milestone gate review (due on snapshot)", 1008, "InProgress",
         "High", SNAPSHOT - timedelta(days=20), SNAPSHOT, weight=3, progress=70)
# 1 unassigned task (AssigneeUserId = NULL)
add_task(2001, "Backlog grooming (unassigned)", None, "ToDo",
         "Low", None, SNAPSHOT + timedelta(days=15), weight=1, progress=0)
# a Blocked task to feed the "blocked work" KPI
add_task(2003, "Model retraining (blocked on data)", 1012, "Blocked",
         "High", SNAPSHOT - timedelta(days=30), SNAPSHOT + timedelta(days=5),
         weight=2, progress=30)

# --- bulk tasks to reach >= 80, deterministic ---------------------------------------
while len(tasks) < 84:
    pid = random.choice(PROJECT_IDS)
    proj = next(p for p in PROJECTS if p[0] == pid)
    completed_project = proj[5] == "Completed"
    status = "Done" if completed_project else random.choice(TASK_STATUSES)
    progress = 100 if status == "Done" else random.choice([0, 10, 25, 50, 75, 90])
    start = proj[7] + timedelta(days=random.randint(0, 120)) if proj[7] else None
    # mix of past and future due dates to enable timeframe filtering and overdue logic
    due = SNAPSHOT + timedelta(days=random.randint(-90, 120))
    assignee = random.choice(member_map[pid])
    add_task(pid, random.choice(TASK_TITLES), assignee, status,
             random.choice(PRIORITIES), start, due,
             weight=round(random.uniform(1, 4), 1), progress=progress)

# --------------------------------------------------------------------------------------
# 7. Milestones  (>= 24)
# --------------------------------------------------------------------------------------
MILESTONE_NAMES = ["Kickoff", "Design Complete", "Build Complete", "UAT Sign-off",
                   "Go-Live", "Hypercare Exit", "Phase 1", "Phase 2"]
milestones = []
msid = 5001
for pid in PROJECT_IDS:
    proj = next(p for p in PROJECTS if p[0] == pid)
    n = random.randint(3, 4)
    for j in range(n):
        base = proj[7] or d(2026, 1, 1)
        due = base + timedelta(days=60 * (j + 1))
        if proj[5] == "Completed":
            status = "Achieved"
        elif due < SNAPSHOT:
            status = random.choice(["Achieved", "Missed"])
        else:
            status = "Pending"
        milestones.append((msid, pid, f"{MILESTONE_NAMES[j % len(MILESTONE_NAMES)]}",
                           iso(due), status))
        msid += 1
# ensure at least one milestone due within 14 days of snapshot (dashboard KPI)
milestones.append((msid, 2002, "Cutover readiness", iso(SNAPSHOT + timedelta(days=7)), "Pending"))
msid += 1

# --------------------------------------------------------------------------------------
# 8. Risks / issues  (>= 20)  — score = probability * impact
# --------------------------------------------------------------------------------------
RISK_TITLES = ["Vendor delay", "Scope creep", "Key resource attrition", "Integration failure",
               "Budget pressure", "Security vulnerability", "Data quality gap",
               "Regulatory change", "Infrastructure capacity", "Third-party API change"]
risks = []
rid = 6001


def add_risk(pid, kind, title, owner, prob, impact, mitigation, due, status):
    global rid
    risks.append((rid, pid, kind, title, owner, prob, impact, prob * impact,
                  mitigation, iso(due), status))
    rid += 1


# explicit: one OPEN high-exposure risk (counts as open high exposure)
add_risk(2003, "Risk", "AI model accuracy below target", 1005, 5, 5,
         "Add labelled data, expand validation set, weekly review.",
         SNAPSHOT + timedelta(days=20), "Open")
# explicit: one CLOSED high-exposure risk (must NOT count as open)
add_risk(2001, "Risk", "Core migration data loss", 1003, 5, 4,
         "Dual-write + reconciliation completed; verified.",
         SNAPSHOT - timedelta(days=15), "Closed")
# an Issue (kind = Issue) that is open and high exposure
add_risk(2006, "Issue", "Unpatched critical CVE in gateway", 1007, 4, 5,
         "Emergency patch window scheduled.", SNAPSHOT + timedelta(days=3), "Open")

while len(risks) < 22:
    pid = random.choice(PROJECT_IDS)
    prob = random.randint(1, 5)
    impact = random.randint(1, 5)
    status = random.choice(["Open", "Open", "Mitigating", "Closed"])
    add_risk(pid, random.choice(["Risk", "Issue"]), random.choice(RISK_TITLES),
             random.choice(PM_IDS + CON_IDS), prob, impact,
             "Documented mitigation and owner assigned.",
             SNAPSHOT + timedelta(days=random.randint(-40, 60)), status)

# --------------------------------------------------------------------------------------
# 9. Change requests  (>= 15)  + change history (append-only transitions)
# --------------------------------------------------------------------------------------
CR_TITLES = ["Add reporting module", "Extend timeline 4 weeks", "Increase scope: SSO",
             "Switch vendor", "Add mobile support", "Change hosting region",
             "Add audit logging", "Upgrade database tier"]
# Valid workflow: Draft -> Submitted -> UnderReview -> (Approved | Rejected | Returned)
change_requests = []
change_history = []
crid = 7001
chid = 7501


def add_cr(pid, title, requester, final_status, est_impact, decision_by=None,
           reason=None, decided_at=None):
    """Create a CR at final_status and the matching append-only history trail."""
    global crid, chid
    version = 1
    change_requests.append((crid, pid, title,
                            f"Change request: {title}.", requester, final_status,
                            decision_by, reason, iso(decided_at), est_impact, version))
    # build the transition chain that leads to final_status
    chain = ["Draft", "Submitted", "UnderReview"]
    if final_status in ("Approved", "Rejected", "Returned"):
        chain.append(final_status)
    elif final_status not in chain:
        chain.append(final_status)
    acted = (decided_at or SNAPSHOT) - timedelta(days=len(chain))
    for k in range(1, len(chain)):
        actor = requester if chain[k] in ("Submitted",) else (decision_by or requester)
        r = reason if chain[k] == final_status else None
        change_history.append((chid, crid, chain[k - 1], chain[k], actor,
                               r, iso(acted + timedelta(days=k))))
        chid += 1
    crid += 1


# explicit: at least one Returned and one Rejected change request
add_cr(2001, "Extend timeline 4 weeks", 1008, "Returned", 0, 1003,
       "Need updated impact analysis before decision.", SNAPSHOT - timedelta(days=12))
add_cr(2003, "Switch vendor", 1012, "Rejected", 120000, 1005,
       "Not justified; current vendor meets SLAs.", SNAPSHOT - timedelta(days=8))
add_cr(2006, "Increase scope: SSO", 1011, "Approved", 90000, 1007,
       "Aligns with zero-trust objectives.", SNAPSHOT - timedelta(days=5))
# a couple still pending decision (Submitted / UnderReview) for the dashboard KPI
add_cr(2002, "Add reporting module", 1010, "Submitted", 40000)
add_cr(2004, "Add mobile support", 1014, "UnderReview", 60000)

while len(change_requests) < 15:
    pid = random.choice(PROJECT_IDS)
    requester = random.choice(member_map[pid])
    final = random.choice(["Approved", "Rejected", "Returned", "Submitted", "UnderReview", "Draft"])
    pm = next(p for p in PROJECTS if p[0] == pid)[4]
    if final in ("Approved", "Rejected", "Returned"):
        add_cr(pid, random.choice(CR_TITLES), requester, final,
               random.choice([0, 25000, 50000, 75000]), pm,
               "Decision recorded.", SNAPSHOT - timedelta(days=random.randint(1, 30)))
    else:
        add_cr(pid, random.choice(CR_TITLES), requester, final,
               random.choice([0, 15000, 30000]))

# --------------------------------------------------------------------------------------
# 10. Budget revisions  (several; only Approved affect current approved budget)
# --------------------------------------------------------------------------------------
budget_revisions = []
brid = 8101
# P2006: one Approved (+150k) and one Pending (not counted)
budget_revisions.append((brid, 2006, 150000, "Additional pen-test scope approved by steering committee.",
                         "Approved", 1007, 1001, iso(SNAPSHOT - timedelta(days=20)))); brid += 1
budget_revisions.append((brid, 2006, 100000, "Requested extra tooling budget (awaiting approval).",
                         "Pending", 1007, None, "")); brid += 1
# P2001: approved uplift
budget_revisions.append((brid, 2001, 300000, "Scope addition: fraud analytics module.",
                         "Approved", 1003, 1001, iso(SNAPSHOT - timedelta(days=40)))); brid += 1
# P2003: a Rejected revision (no effect)
budget_revisions.append((brid, 2003, 200000, "Extra GPU budget — rejected, use cloud credits.",
                         "Rejected", 1005, 1002, iso(SNAPSHOT - timedelta(days=10)))); brid += 1
# P2002: small approved revision
budget_revisions.append((brid, 2002, 50000, "Redundant links added.",
                         "Approved", 1004, 1001, iso(SNAPSHOT - timedelta(days=25)))); brid += 1

# effective current approved budget per project (initial + approved revisions)
approved_rev = {}
for r in budget_revisions:
    if r[4] == "Approved":
        approved_rev[r[1]] = approved_rev.get(r[1], 0) + r[2]
current_budget = {p[0]: p[11] + approved_rev.get(p[0], 0) for p in PROJECTS}

# --------------------------------------------------------------------------------------
# 11. Cost transactions  (>= 80)  — Posted counts as actual spend; Draft excluded
# --------------------------------------------------------------------------------------
# Target posted-spend ratios per project to realise the financial edge cases:
#   2002 nearing budget (~95%), 2003 moderate (EAC pushed over via forecast),
#   2007 NO posted spend (draft only), 2008 completed (fully/mostly spent).
cost_transactions = []
ctid = 9001
POSTED_TARGET = {
    2001: 0.55, 2002: 0.95, 2003: 0.60, 2004: 0.45,
    2005: 0.20, 2006: 0.50, 2007: 0.00, 2008: 0.88,
}


def add_cost(pid, cat, amount, txn_date, desc, status, posted_by=None, posted_at=None):
    global ctid
    cost_transactions.append((ctid, pid, cat, round(amount, 2), iso(txn_date), desc,
                              status, posted_by, iso(posted_at)))
    ctid += 1


for p in PROJECTS:
    pid = p[0]
    budget = current_budget[pid]
    target_posted = budget * POSTED_TARGET[pid]
    start = p[7] or d(2026, 1, 1)
    pm = p[4]
    posted_so_far = 0.0
    # spread ~8-12 posted transactions across months up to the snapshot
    n_posted = 0 if POSTED_TARGET[pid] == 0 else random.randint(8, 12)
    for _ in range(n_posted):
        months_in = random.randint(0, max(1, (SNAPSHOT.year - start.year) * 12 + SNAPSHOT.month - start.month))
        txn_date = min(SNAPSHOT - timedelta(days=random.randint(1, 20)),
                       start + timedelta(days=30 * months_in + random.randint(0, 27)))
        if txn_date < start:
            txn_date = start + timedelta(days=random.randint(0, 20))
        amt = target_posted / n_posted * random.uniform(0.6, 1.4)
        posted_so_far += amt
        add_cost(pid, random.choice(CAT_IDS), amt, txn_date,
                 "Posted expense", "Posted", pm, txn_date + timedelta(days=1))
    # a few DRAFT costs (excluded from actual spend) on every project
    for _ in range(random.randint(2, 4)):
        add_cost(pid, random.choice(CAT_IDS), random.uniform(5000, 40000),
                 SNAPSHOT - timedelta(days=random.randint(1, 15)),
                 "Draft expense awaiting posting", "Draft")

# explicit: at least one POSTED cost dated AFTER the snapshot cutoff
# (must be EXCLUDED from actual spend at the 2026-09-30 baseline, included later)
add_cost(2001, 8002, 75000, SNAPSHOT + timedelta(days=15),
         "Hardware delivered after cutoff", "Posted", 1003, SNAPSHOT + timedelta(days=16))

# --------------------------------------------------------------------------------------
# 12. Financial forecasts  (monthly; ForecastToComplete drives EAC)
# --------------------------------------------------------------------------------------
# P2005 intentionally gets NO forecast rows (edge case: EAC falls back to actual spend).
forecasts = []
fid = 10001
for p in PROJECTS:
    pid = p[0]
    if pid == 2005:
        continue  # no forecast data
    budget = current_budget[pid]
    # forecast-to-complete: for P2003 make EAC exceed budget (projected overrun)
    if pid == 2003:
        ftc = budget * 0.75   # actual ~60% + FTC 75% => EAC ~135% of budget
    elif pid == 2002:
        ftc = budget * 0.08   # nearly done
    elif pid == 2008:
        ftc = 0               # completed
    else:
        ftc = budget * random.uniform(0.2, 0.4)
    # one ForecastToComplete row + a few forward MonthlyEstimate rows
    forecasts.append((fid, pid, iso(d(2026, 10, 1)), round(ftc, 2), "ForecastToComplete")); fid += 1
    for mo in range(3):
        period = d(2026, 10 + mo, 1) if 10 + mo <= 12 else d(2027, 10 + mo - 12, 1)
        forecasts.append((fid, pid, iso(period), round(ftc / 3, 2), "MonthlyEstimate")); fid += 1

# --------------------------------------------------------------------------------------
# 13. Progress snapshots  (several per project, historical trend)
# --------------------------------------------------------------------------------------
snapshots = []
snid = 11001
for pid in PROJECT_IDS:
    ptasks = [t for t in tasks if t[1] == pid]
    total = len(ptasks)
    for k in range(4):  # 4 monthly historical points leading to the snapshot
        snap_date = SNAPSHOT - timedelta(days=30 * (3 - k))
        done = min(total, int(total * (0.2 + 0.2 * k)))
        pct = int(100 * done / total) if total else 0
        open_risks = len([r for r in risks if r[1] == pid and r[10] in ("Open", "Mitigating")])
        snapshots.append((snid, pid, iso(snap_date), pct, total, done, open_risks))
        snid += 1

# --------------------------------------------------------------------------------------
# 14. Automation runs + outbox notifications + audit events (workflow evidence)
# --------------------------------------------------------------------------------------
automation_runs = []
arid = 12001
for k in range(6):
    run_date = SNAPSHOT - timedelta(days=k * 2)
    rule = random.choice(["overdue_task_escalation", "budget_overrun_alert", "milestone_due_soon"])
    status = random.choice(["Success", "Success", "Skipped", "Failed"])
    automation_runs.append((arid, rule, iso(run_date),
                            status, f"{rule} executed with status {status}.",
                            f"{rule}:{run_date.isoformat()}"))
    arid += 1

outbox = []
obid = 12501
for k in range(5):
    created = SNAPSHOT - timedelta(days=k)
    outbox.append((obid, "OverdueEscalation", "Overdue task requires attention",
                   "You have an overdue task that needs an update.",
                   random.choice(PM_IDS), iso(created),
                   iso(created) if k % 2 == 0 else "",
                   "Sent" if k % 2 == 0 else "Pending",
                   f"overdue:{obid}"))
    obid += 1

audit_events = []
auid = 13001
for cr in change_requests:
    if cr[5] in ("Approved", "Rejected", "Returned"):
        audit_events.append((auid, "ChangeRequest", cr[0], f"Decision:{cr[5]}",
                             cr[6] or 1001, f"CR {cr[0]} decided {cr[5]}.",
                             cr[8] or iso(SNAPSHOT)))
        auid += 1
for ct in cost_transactions:
    if ct[6] == "Posted" and ct[7]:
        audit_events.append((auid, "CostTransaction", ct[0], "Posted",
                             ct[7], f"Cost {ct[0]} posted ({ct[3]} {CURRENCY}).", ct[8]))
        auid += 1

# --------------------------------------------------------------------------------------
# Write all CSVs (headers == C# entity property names)
# --------------------------------------------------------------------------------------
print("Writing CSV files to", CSV_DIR)
write_csv("departments.csv", ["DeptId", "DeptName", "DeptNameAr", "Leader"], DEPARTMENTS)
write_csv("users.csv",
          ["UserId", "UserName", "NickName", "NickNameAr", "DeptId", "RoleKey", "Sex"], USERS)
write_csv("cost_categories.csv", ["CategoryId", "Code", "Name"], CATEGORIES)
write_csv("projects.csv",
          ["ProjectId", "Code", "Name", "DeptId", "OwnerUserId", "Status", "Priority",
           "StartDate", "PlannedEndDate", "ActualEndDate", "Description",
           "InitialApprovedBudget"],
          [(p[0], p[1], p[2], p[3], p[4], p[5], p[6], iso(p[7]), iso(p[8]), iso(p[9]),
            p[10], p[11]) for p in PROJECTS])
write_csv("project_members.csv",
          ["MemberId", "ProjectId", "UserId", "RoleInProject"], members)
write_csv("tasks.csv",
          ["TaskId", "ProjectId", "Title", "AssigneeUserId", "Status", "Priority",
           "StartDate", "DueDate", "Weight", "ProgressPct", "Version"], tasks)
write_csv("milestones.csv",
          ["MilestoneId", "ProjectId", "Name", "DueDate", "Status"], milestones)
write_csv("risks.csv",
          ["RiskId", "ProjectId", "Kind", "Title", "OwnerUserId", "Probability",
           "Impact", "Score", "Mitigation", "DueDate", "Status"], risks)
write_csv("change_requests.csv",
          ["ChangeRequestId", "ProjectId", "Title", "Description", "RequestedByUserId",
           "Status", "DecisionByUserId", "DecisionReason", "DecisionAt",
           "EstimatedCostImpact", "Version"], change_requests)
write_csv("change_history.csv",
          ["HistoryId", "ChangeRequestId", "FromStatus", "ToStatus", "ActorUserId",
           "Reason", "ActedAt"], change_history)
write_csv("budget_revisions.csv",
          ["RevisionId", "ProjectId", "Amount", "Reason", "Status",
           "RequestedByUserId", "ApprovedByUserId", "ApprovedAt"], budget_revisions)
write_csv("cost_transactions.csv",
          ["TransactionId", "ProjectId", "CategoryId", "Amount", "TxnDate",
           "Description", "Status", "PostedByUserId", "PostedAt"], cost_transactions)
write_csv("financial_forecasts.csv",
          ["ForecastId", "ProjectId", "PeriodMonth", "ForecastAmount", "Kind"], forecasts)
write_csv("progress_snapshots.csv",
          ["SnapshotId", "ProjectId", "SnapshotDate", "ProgressPct", "TasksTotal",
           "TasksDone", "OpenRiskCount"], snapshots)
write_csv("automation_runs.csv",
          ["RunId", "RuleKey", "TriggeredAt", "Status", "Detail", "DedupeKey"],
          automation_runs)
write_csv("outbox_notifications.csv",
          ["NotificationId", "Type", "Subject", "Body", "TargetUserId", "CreatedAt",
           "SentAt", "Status", "DedupeKey"], outbox)
write_csv("audit_events.csv",
          ["AuditId", "EntityType", "EntityId", "Action", "ActorUserId", "Detail",
           "ActedAt"], audit_events)

# --------------------------------------------------------------------------------------
# Reconciliation report — financial KPIs at the snapshot, for cross-system checking
# --------------------------------------------------------------------------------------
def actual_spend(pid):
    return sum(c[3] for c in cost_transactions
               if c[1] == pid and c[6] == "Posted"
               and date.fromisoformat(c[4]) <= SNAPSHOT)


def ftc(pid):
    rows = [f for f in forecasts if f[1] == pid and f[4] == "ForecastToComplete"]
    return sum(f[3] for f in rows) if rows else None


lines = [
    "# PMO — KPI reconciliation (baseline snapshot " + SNAPSHOT.isoformat() + ")",
    "",
    "Numbers computed by the generator from source-of-truth rows using the backend's",
    "financial definitions. The web dashboard, raw SQL and Power BI must reproduce these",
    "for the same snapshot/filter scope.",
    "",
    "| Project | Approved budget | Actual spend | Remaining | Utilization | FTC | EAC | Proj. variance | Flag |",
    "|---------|----------------:|-------------:|----------:|------------:|----:|----:|---------------:|------|",
]
tot_budget = tot_spend = tot_eac = 0
for p in PROJECTS:
    pid = p[0]
    budget = current_budget[pid]
    spend = actual_spend(pid)
    rem = budget - spend
    util = ("%.1f%%" % (100 * spend / budget)) if budget else "n/a"
    f = ftc(pid)
    eac = spend + (f if f is not None else 0)
    flag = "no forecast → EAC=actual" if f is None else ("OVERRUN" if (budget - eac) < 0 else "")
    tot_budget += budget; tot_spend += spend; tot_eac += eac
    lines.append("| %s | %0.0f | %0.0f | %0.0f | %s | %s | %0.0f | %0.0f | %s |" % (
        p[1], budget, spend, rem, util,
        ("%0.0f" % f) if f is not None else "—", eac, budget - eac, flag))
lines.append("| **TOTAL** | %0.0f | %0.0f | %0.0f | %.1f%% | | %0.0f | %0.0f | |" % (
    tot_budget, tot_spend, tot_budget - tot_spend,
    100 * tot_spend / tot_budget, tot_eac, tot_budget - tot_eac))

overdue = len([t for t in tasks if t[7] and date.fromisoformat(t[7]) < SNAPSHOT and t[4] != "Done"])
due_soon_ms = len([m for m in milestones if m[3] and
                   SNAPSHOT <= date.fromisoformat(m[3]) <= SNAPSHOT + timedelta(days=14)
                   and m[4] == "Pending"])
# Matches the dashboard KPI definition exactly: Status == "Open" AND score >= 12 (Mitigating excluded).
open_high = len([r for r in risks if r[10] == "Open" and r[7] >= 12])
pending_cr = len([c for c in change_requests if c[5] in ("Submitted", "UnderReview")])
lines += [
    "",
    "## Delivery & governance KPIs at snapshot",
    f"- Total projects: {len(PROJECTS)}  |  Active: {len([p for p in PROJECTS if p[5]=='Active'])}",
    f"- Overdue incomplete tasks: {overdue}",
    f"- Milestones due within 14 days: {due_soon_ms}",
    f"- Open high-exposure risks/issues (score >= 12): {open_high}",
    f"- Pending change requests (Submitted/UnderReview): {pending_cr}",
    f"- Posted cost transactions: {len([c for c in cost_transactions if c[6]=='Posted'])}"
    f"  |  Draft: {len([c for c in cost_transactions if c[6]=='Draft'])}",
]
with open(os.path.join(HERE, "reconciliation.md"), "w", encoding="utf-8") as f:
    f.write("\n".join(lines) + "\n")
print("Wrote reconciliation.md")
print("\nDone. Demo login password for every user: 123456")
