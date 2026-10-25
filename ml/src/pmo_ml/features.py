"""Step 5 - Feature engineering.

One builder per problem. Each returns a `Dataset`: the feature table, the label (if
supervised), the row ids, and which columns are numeric / categorical / text so models.py
can build the matching preprocessing pipeline.

Leakage rule: a feature must be known *before* the outcome it predicts. For example the
task-delay model never sees a task's status, progress or days-overdue - those are results of
being late, not causes. See docs/05_FEATURE_ENGINEERING.md.
"""

from __future__ import annotations

from dataclasses import dataclass, field

import numpy as np
import pandas as pd

HIGH_RISK_SCORE = 12        # the portal's "high exposure" threshold (probability x impact)
SEVERITY_BANDS = [(0, 6, "Low"), (7, 11, "Medium"), (12, 25, "High")]


@dataclass
class Dataset:
    X: pd.DataFrame
    y: pd.Series | None
    ids: pd.Series
    numeric: list[str]
    categorical: list[str]
    text: str | None = None
    extra: dict = field(default_factory=dict)       # e.g. rows to predict, group keys

    @property
    def features(self) -> list[str]:
        return self.numeric + self.categorical + ([self.text] if self.text else [])


def _ts(as_of) -> pd.Timestamp:
    return pd.Timestamp(as_of).normalize()


# ---------------------------------------------------------------- shared project context
def project_context(frames: dict[str, pd.DataFrame], as_of) -> pd.DataFrame:
    """One row per project with context features used by several models."""
    as_of = _ts(as_of)
    p = frames["projects"].copy()
    ctx = pd.DataFrame({"project_id": p["project_id"]})
    ctx["budget_log"] = np.log1p(p["initial_approved_budget"].fillna(0).clip(lower=0))
    ctx["project_priority"] = p["priority"]
    ctx["project_status"] = p["status"]
    ctx["dept_id"] = p["dept_id"].astype("Int64").astype(str)
    ctx["project_duration_days"] = (p["planned_end_date"] - p["start_date"]).dt.days

    members = frames["members"].groupby("project_id").size().rename("member_count")
    ctx = ctx.merge(members, on="project_id", how="left")

    r = frames["risks"]
    open_r = r[r["status"] != "Closed"]
    ctx = ctx.merge(open_r.groupby("project_id").size().rename("open_risks"), on="project_id", how="left")
    ctx = ctx.merge(open_r[open_r["score"] >= HIGH_RISK_SCORE].groupby("project_id").size()
                    .rename("open_high_risks"), on="project_id", how="left")

    # approved budget = initial + approved revisions; spend = posted costs <= as_of
    rev = frames["revisions"]
    approved_rev = rev[rev["status"] == "Approved"].groupby("project_id")["amount"].sum()
    spend = frames["costs_posted"]
    spend = spend[spend["txn_date"] <= as_of].groupby("project_id")["amount"].sum()
    budget = p.set_index("project_id")["initial_approved_budget"].fillna(0).add(approved_rev, fill_value=0)
    ctx["approved_budget"] = ctx["project_id"].map(budget)
    ctx["actual_spend"] = ctx["project_id"].map(spend).fillna(0)
    ctx["utilization"] = np.where(ctx["approved_budget"] > 0, ctx["actual_spend"] / ctx["approved_budget"], np.nan)

    for col in ["member_count", "open_risks", "open_high_risks"]:
        ctx[col] = ctx[col].fillna(0)
    return ctx


# ---------------------------------------------------------------- 1. task delay
def task_outcomes(tasks: pd.DataFrame, as_of) -> pd.DataFrame:
    """Label each task: resolved?, late?

    late = not Done and due date passed, OR Done but completed (update_time) after due date.
    A Done task without a completion timestamp is treated as on time (documented assumption).
    Open tasks whose due date is still in the future are unresolved -> prediction targets.
    """
    as_of = _ts(as_of)
    t = tasks.copy()
    due = t["due_date"].dt.normalize()
    done = t["status"] == "Done"
    finished_late = done & t["update_time"].notna() & (t["update_time"].dt.normalize() > due)
    open_overdue = ~done & (due < as_of)
    t["resolved"] = done | (due < as_of)
    t["late"] = (finished_late | open_overdue).astype(int)
    return t


def task_delay_dataset(frames: dict[str, pd.DataFrame], as_of) -> Dataset:
    t = task_outcomes(frames["tasks"].dropna(subset=["due_date"]), as_of)
    ctx = project_context(frames, as_of)
    t = t.merge(ctx, on="project_id", how="left")
    p_start = frames["projects"].set_index("project_id")["start_date"]

    feats = pd.DataFrame(index=t.index)
    feats["priority"] = t["priority"]
    feats["weight"] = t["weight"]
    feats["planned_duration_days"] = (t["due_date"] - t["start_date"]).dt.days
    feats["start_offset_days"] = (t["start_date"] - t["project_id"].map(p_start)).dt.days
    feats["unassigned"] = t["assignee_user_id"].isna().astype(int)
    load = frames["tasks"].groupby("assignee_user_id").size()
    feats["assignee_task_count"] = t["assignee_user_id"].map(load).fillna(0)
    proj_load = frames["tasks"].groupby("project_id").size()
    feats["project_task_count"] = t["project_id"].map(proj_load)
    for col in ["budget_log", "project_priority", "project_status", "project_duration_days",
                "member_count", "open_risks", "open_high_risks"]:
        feats[col] = t[col]

    numeric = ["weight", "planned_duration_days", "start_offset_days", "unassigned",
               "assignee_task_count", "project_task_count", "budget_log", "project_duration_days",
               "member_count", "open_risks", "open_high_risks"]
    categorical = ["priority", "project_priority", "project_status"]

    train_mask = t["resolved"].values
    return Dataset(
        X=feats[train_mask].reset_index(drop=True),
        y=t.loc[train_mask, "late"].reset_index(drop=True),
        ids=t.loc[train_mask, "task_id"].reset_index(drop=True),
        numeric=numeric, categorical=categorical,
        extra={
            "predict_X": feats[~train_mask].reset_index(drop=True),
            "predict_rows": t.loc[~train_mask, ["task_id", "project_id", "title", "due_date", "status",
                                                 "priority", "assignee_user_id"]].reset_index(drop=True),
        },
    )


# ---------------------------------------------------------------- 2. risk severity
def severity_band(score) -> str:
    for lo, hi, name in SEVERITY_BANDS:
        if lo <= score <= hi:
            return name
    return "High"


def risk_text(title, mitigation) -> str:
    return f"{title or ''} . {mitigation or ''}".strip()


def risk_severity_dataset(frames: dict[str, pd.DataFrame], as_of) -> Dataset:
    r = frames["risks"].dropna(subset=["score"]).copy()
    ctx = project_context(frames, as_of)
    r = r.merge(ctx[["project_id", "budget_log", "project_priority", "project_status"]], on="project_id", how="left")
    X = pd.DataFrame({
        "text": [risk_text(a, b) for a, b in zip(r["title"], r["mitigation"])],
        "kind": r["kind"],
        "project_priority": r["project_priority"],
        "project_status": r["project_status"],
        "budget_log": r["budget_log"],
    })
    # probability / impact / score are NOT features: they define the label.
    return Dataset(X=X.reset_index(drop=True), y=r["score"].map(severity_band).reset_index(drop=True),
                   ids=r["risk_id"].reset_index(drop=True), numeric=["budget_log"],
                   categorical=["kind", "project_priority", "project_status"], text="text")


def risk_features_for(text: str, kind: str, project_id, frames, as_of) -> pd.DataFrame:
    """Feature row for a risk that is being typed in the UI (not saved yet)."""
    ctx = project_context(frames, as_of).set_index("project_id")
    row = ctx.loc[project_id] if project_id in ctx.index else None
    return pd.DataFrame([{
        "text": text, "kind": kind or "Risk",
        "project_priority": None if row is None else row["project_priority"],
        "project_status": None if row is None else row["project_status"],
        "budget_log": np.nan if row is None else row["budget_log"],
    }])


# ---------------------------------------------------------------- 3. spend forecast
def monthly_spend(frames: dict[str, pd.DataFrame], as_of) -> pd.DataFrame:
    """Complete project x month grid of posted spend up to the as-of month (missing months = 0)."""
    as_of = _ts(as_of)
    c = frames["costs_posted"]
    c = c[c["txn_date"] <= as_of].copy()
    c["month"] = c["txn_date"].dt.to_period("M").dt.to_timestamp()
    agg = c.groupby(["project_id", "month"])["amount"].sum()
    last = as_of.to_period("M").to_timestamp()
    rows = []
    for pid, first in c.groupby("project_id")["month"].min().items():
        for m in pd.date_range(first, last, freq="MS"):
            rows.append((pid, m, float(agg.get((pid, m), 0.0))))
    return pd.DataFrame(rows, columns=["project_id", "month", "spend"])


def _spend_features(grid: pd.DataFrame, frames, as_of) -> pd.DataFrame:
    g = grid.sort_values(["project_id", "month"]).copy()
    by = g.groupby("project_id")["spend"]
    g["lag1"] = by.shift(1)
    g["lag2"] = by.shift(2)
    g["lag3"] = by.shift(3)
    g["roll3"] = by.transform(lambda s: s.shift(1).rolling(3, min_periods=1).mean())
    g["cum_prev"] = by.transform(lambda s: s.shift(1).cumsum())
    g["months_active"] = g.groupby("project_id").cumcount()
    ctx = project_context(frames, as_of).set_index("project_id")
    g["budget_log"] = g["project_id"].map(ctx["budget_log"])
    g["budget_used_prev"] = g["cum_prev"] / g["project_id"].map(ctx["approved_budget"]).replace(0, np.nan)
    g["month_of_year"] = g["month"].dt.month
    est = frames["forecasts"]
    est = est[est["kind"] == "MonthlyEstimate"].copy()
    est["month"] = est["period_month"].dt.to_period("M").dt.to_timestamp()
    est = est.groupby(["project_id", "month"])["forecast_amount"].sum()
    g["planned_estimate"] = [est.get((p, m), np.nan) for p, m in zip(g["project_id"], g["month"])]
    return g


SPEND_NUMERIC = ["lag1", "lag2", "lag3", "roll3", "months_active", "budget_log",
                 "budget_used_prev", "month_of_year", "planned_estimate"]


def spend_forecast_dataset(frames: dict[str, pd.DataFrame], as_of) -> Dataset:
    as_of = _ts(as_of)
    g = _spend_features(monthly_spend(frames, as_of), frames, as_of)
    current = as_of.to_period("M").to_timestamp()
    # train on complete months with at least one month of history; the as-of month is partial
    train = g[(g["month"] < current) & g["lag1"].notna()].sort_values("month").reset_index(drop=True)

    # next month per project: append an empty row and compute its features from history
    nxt_month = current + pd.offsets.MonthBegin(1) if as_of.day > 25 else current
    hist = g[g["month"] < nxt_month][["project_id", "month", "spend"]]
    future = pd.DataFrame({"project_id": hist["project_id"].unique(), "month": nxt_month, "spend": np.nan})
    fg = _spend_features(pd.concat([hist, future], ignore_index=True), frames, as_of)
    fg = fg[fg["month"] == nxt_month].reset_index(drop=True)

    return Dataset(X=train[SPEND_NUMERIC], y=train["spend"], ids=train["project_id"],
                   numeric=SPEND_NUMERIC, categorical=[],
                   extra={"months": train["month"], "predict_X": fg[SPEND_NUMERIC],
                          "predict_rows": fg[["project_id", "month", "planned_estimate", "lag1"]]})


# ---------------------------------------------------------------- 4. cost anomalies
ANOMALY_NUMERIC = ["log_amount", "amount_vs_category", "amount_vs_project", "share_of_budget",
                   "posting_lag_days", "day_of_week", "is_month_end"]


def cost_anomaly_dataset(frames: dict[str, pd.DataFrame], as_of) -> Dataset:
    c = frames["costs_posted"].copy()
    ctx = project_context(frames, as_of).set_index("project_id")
    cat_median = c.groupby("category_id")["amount"].transform("median")
    proj_median = c.groupby("project_id")["amount"].transform("median")
    X = pd.DataFrame({
        "log_amount": np.log1p(c["amount"]),
        "amount_vs_category": c["amount"] / cat_median.replace(0, np.nan),
        "amount_vs_project": c["amount"] / proj_median.replace(0, np.nan),
        "share_of_budget": c["amount"] / c["project_id"].map(ctx["approved_budget"]).replace(0, np.nan),
        "posting_lag_days": (c["posted_at"] - c["txn_date"]).dt.days,
        "day_of_week": c["txn_date"].dt.dayofweek,
        "is_month_end": c["txn_date"].dt.is_month_end.astype(int),
        "category": c["category_id"].astype("Int64").astype(str),
    })
    return Dataset(X=X.reset_index(drop=True), y=None, ids=c["transaction_id"].reset_index(drop=True),
                   numeric=ANOMALY_NUMERIC, categorical=["category"],
                   extra={"rows": c[["transaction_id", "project_id", "category_id", "amount", "txn_date",
                                     "description"]].reset_index(drop=True)})


# ---------------------------------------------------------------- 5. portfolio segments
SEGMENT_NUMERIC = ["budget_log", "utilization", "late_task_share", "avg_progress", "open_risk_exposure",
                   "task_count"]


def portfolio_dataset(frames: dict[str, pd.DataFrame], as_of) -> Dataset:
    ctx = project_context(frames, as_of)
    t = task_outcomes(frames["tasks"].dropna(subset=["due_date"]), as_of)
    late_share = t.groupby("project_id")["late"].mean()
    progress = frames["tasks"].groupby("project_id")["progress_pct"].mean()
    r = frames["risks"]
    exposure = r[r["status"] != "Closed"].groupby("project_id")["score"].sum()
    count = frames["tasks"].groupby("project_id").size()
    X = pd.DataFrame({
        "budget_log": ctx["budget_log"],
        "utilization": ctx["utilization"],
        "late_task_share": ctx["project_id"].map(late_share),
        "avg_progress": ctx["project_id"].map(progress),
        "open_risk_exposure": ctx["project_id"].map(exposure).fillna(0),
        "task_count": ctx["project_id"].map(count).fillna(0),
    })
    names = frames["projects"].set_index("project_id")[["code", "name", "status"]]
    return Dataset(X=X, y=None, ids=ctx["project_id"], numeric=SEGMENT_NUMERIC, categorical=[],
                   extra={"rows": names.reindex(ctx["project_id"]).reset_index()})


BUILDERS = {
    "task_delay": task_delay_dataset,
    "risk_severity": risk_severity_dataset,
    "spend_forecast": spend_forecast_dataset,
    "cost_anomaly": cost_anomaly_dataset,
    "portfolio_segments": portfolio_dataset,
}
