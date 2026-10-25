"""Step 3 - Data cleaning & preprocessing (table level).

Row-level fixes happen here (duplicates, inconsistent labels, types, business rules).
Imputation, encoding and scaling are NOT done here: they live inside each model's
scikit-learn Pipeline (see features.py / models.py) so exactly the same transformation
runs at training time and at prediction time.
"""

from __future__ import annotations

import pandas as pd

ID_COLUMNS = {
    "projects": "project_id", "tasks": "task_id", "risks": "risk_id", "costs": "transaction_id",
    "snapshots": "snapshot_id", "forecasts": "forecast_id", "members": "member_id",
    "categories": "category_id", "revisions": "revision_id",
}

# canonical spellings for categorical values (keys are lower-cased, spaces/underscores removed)
CANONICAL = {
    "status": {"todo": "ToDo", "inprogress": "InProgress", "blocked": "Blocked", "done": "Done",
               "active": "Active", "planning": "Planning", "onhold": "OnHold",
               "completed": "Completed", "cancelled": "Cancelled", "open": "Open",
               "mitigating": "Mitigating", "closed": "Closed", "draft": "Draft",
               "posted": "Posted", "pending": "Pending", "approved": "Approved",
               "rejected": "Rejected"},
    "priority": {"low": "Low", "medium": "Medium", "high": "High", "critical": "Critical"},
    "kind": {"risk": "Risk", "issue": "Issue", "monthlyestimate": "MonthlyEstimate",
             "forecasttocomplete": "ForecastToComplete"},
}

NUMERIC = {"initial_approved_budget", "weight", "progress_pct", "probability", "impact", "score",
           "amount", "tasks_total", "tasks_done", "open_risk_count", "forecast_amount"}


def _canon(series: pd.Series, mapping: dict[str, str]) -> pd.Series:
    def fix(v):
        if pd.isna(v):
            return v
        key = str(v).strip().lower().replace(" ", "").replace("_", "").replace("-", "")
        return mapping.get(key, str(v).strip())
    return series.map(fix)


def clean(frames: dict[str, pd.DataFrame]) -> tuple[dict[str, pd.DataFrame], dict]:
    """Return cleaned frames and a report of what changed (for the docs / API)."""
    out: dict[str, pd.DataFrame] = {}
    report: dict[str, dict] = {}
    for name, df in frames.items():
        df = df.copy()
        rep = {"rows_in": int(len(df))}

        key = ID_COLUMNS.get(name)
        if key and key in df.columns:
            before = len(df)
            df = df.dropna(subset=[key]).drop_duplicates(subset=[key], keep="last")
            rep["duplicates_or_missing_ids_dropped"] = int(before - len(df))

        for col, mapping in CANONICAL.items():
            if col in df.columns:
                df[col] = _canon(df[col], mapping)

        for col in NUMERIC.intersection(df.columns):
            df[col] = pd.to_numeric(df[col], errors="coerce")

        for col in df.columns:
            if df[col].dtype == object:
                df[col] = df[col].map(lambda v: v.strip() if isinstance(v, str) else v)

        rep["missing_values"] = {c: int(n) for c, n in df.isna().sum().items() if n}
        out[name] = df
        report[name] = rep

    # Business rules shared with the portal (Documentation/PMO/02_...): only POSTED costs are
    # actual spend; drafts are excluded. Non-positive amounts are invalid and dropped.
    costs = out["costs"]
    before = len(costs)
    out["costs_posted"] = costs[(costs["status"] == "Posted") & (costs["amount"] > 0)].copy()
    report["costs"]["drafts_or_invalid_excluded_from_spend"] = int(before - len(out["costs_posted"]))

    # Tasks without a due date cannot be labelled late/on-time.
    tasks = out["tasks"]
    report["tasks"]["without_due_date"] = int(tasks["due_date"].isna().sum())

    for name, df in out.items():
        report.setdefault(name, {})["rows_out"] = int(len(df))
    return out, report
