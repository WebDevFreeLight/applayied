"""Step 2 - Data collection: read the PMO tables from PostgreSQL (or the CSV seed).

Only the columns the models need are selected, and no personal data (names, e-mails) is
read: users appear as opaque ids. Each run can be snapshotted to data/raw/<timestamp>/ so
a trained model can always be traced back to the exact data it saw.
"""

from __future__ import annotations

import re
from datetime import datetime
from pathlib import Path

import pandas as pd
from sqlalchemy import text

from .config import Settings, resolve
from .db import get_engine

# logical name -> (db table, csv file, columns)
TABLES: dict[str, tuple[str, str, list[str]]] = {
    "projects": ("pmo_project", "projects.csv",
                 ["project_id", "code", "name", "dept_id", "owner_user_id", "status", "priority",
                  "start_date", "planned_end_date", "actual_end_date", "initial_approved_budget"]),
    "tasks": ("pmo_task", "tasks.csv",
              ["task_id", "project_id", "title", "assignee_user_id", "status", "priority",
               "start_date", "due_date", "weight", "progress_pct", "update_time"]),
    "risks": ("pmo_risk", "risks.csv",
              ["risk_id", "project_id", "kind", "title", "owner_user_id", "probability", "impact",
               "score", "mitigation", "due_date", "status"]),
    "costs": ("pmo_cost_transaction", "cost_transactions.csv",
              ["transaction_id", "project_id", "category_id", "amount", "txn_date", "description",
               "status", "posted_by_user_id", "posted_at"]),
    "snapshots": ("pmo_progress_snapshot", "progress_snapshots.csv",
                  ["snapshot_id", "project_id", "snapshot_date", "progress_pct", "tasks_total",
                   "tasks_done", "open_risk_count"]),
    "forecasts": ("pmo_financial_forecast", "financial_forecasts.csv",
                  ["forecast_id", "project_id", "period_month", "forecast_amount", "kind"]),
    "members": ("pmo_project_member", "project_members.csv",
                ["member_id", "project_id", "user_id", "role_in_project"]),
    "categories": ("pmo_cost_category", "cost_categories.csv", ["category_id", "code", "name"]),
    "revisions": ("pmo_budget_revision", "budget_revisions.csv",
                  ["revision_id", "project_id", "amount", "status"]),
}

DATE_COLUMNS = {"start_date", "planned_end_date", "actual_end_date", "due_date", "update_time",
                "txn_date", "posted_at", "snapshot_date", "period_month"}


def snake(name: str) -> str:
    return re.sub(r"(?<=[a-z0-9])(?=[A-Z])", "_", name.strip().lstrip("﻿")).lower()


def _normalize(df: pd.DataFrame, columns: list[str]) -> pd.DataFrame:
    df = df.rename(columns={c: snake(c) for c in df.columns})
    for col in columns:                       # a CSV may lack e.g. update_time
        if col not in df.columns:
            df[col] = pd.NA
    df = df[columns].copy()
    for col in DATE_COLUMNS.intersection(df.columns):
        df[col] = pd.to_datetime(df[col], errors="coerce")
    return df


def collect(settings: Settings, source: str | None = None) -> dict[str, pd.DataFrame]:
    """Return {logical_name: DataFrame} with snake_case columns and parsed dates."""
    source = (source or settings.section("data").get("source", "db")).lower()
    frames: dict[str, pd.DataFrame] = {}
    if source == "db":
        engine = get_engine(settings)
        with engine.connect() as conn:
            for name, (table, _, cols) in TABLES.items():
                sql = text(f'SELECT {", ".join(cols)} FROM "{table}"')
                frames[name] = _normalize(pd.read_sql(sql, conn), cols)
        engine.dispose()
    elif source == "csv":
        csv_dir = resolve(settings.section("data")["csv_dir"])
        if not (csv_dir / "tasks.csv").exists():
            raise FileNotFoundError(f"CSV seed not found in {csv_dir} (set data.csv_dir, or use source: db)")
        for name, (_, file, cols) in TABLES.items():
            path = csv_dir / file
            frames[name] = _normalize(pd.read_csv(path) if path.exists() else pd.DataFrame(), cols)
    else:
        raise ValueError(f"Unknown data source '{source}' (use db or csv)")
    return frames


def snapshot(frames: dict[str, pd.DataFrame], settings: Settings) -> Path:
    """Write the collected tables to data/raw/<timestamp>/ for lineage."""
    out = settings.path("raw_dir") / datetime.now().strftime("%Y%m%d-%H%M%S")
    out.mkdir(parents=True, exist_ok=True)
    for name, df in frames.items():
        df.to_csv(out / f"{name}.csv", index=False)
    return out
