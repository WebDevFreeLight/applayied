"""Step 10 - Monitoring & maintenance.

- Data drift: Population Stability Index (PSI) of each numeric feature, today vs the
  training reference stored in the model metadata.  PSI < 0.1 stable, 0.1-0.25 watch,
  > 0.25 drift.
- Data growth: how many new rows a problem has since its model was trained.
- Live accuracy: task-delay predictions are logged (reports/monitoring/predictions.jsonl);
  once those tasks are resolved we score the old predictions against what really happened.
- Decision: retrain when there is drift, enough new rows, or live accuracy fell.
"""

from __future__ import annotations

import json
from datetime import datetime

import numpy as np
import pandas as pd

from . import registry
from .config import Settings
from .features import BUILDERS, task_outcomes

BINS = 10


def reference_profile(X: pd.DataFrame, numeric: list[str]) -> dict:
    """Decile edges + share per bin for each numeric feature (stored with the model)."""
    prof = {}
    for c in numeric:
        s = pd.to_numeric(X[c], errors="coerce").dropna()
        if s.nunique() < 2:
            continue
        edges = np.unique(np.quantile(s, np.linspace(0, 1, BINS + 1)))
        if len(edges) < 3:
            continue
        counts, _ = np.histogram(s.clip(edges[0], edges[-1]), bins=edges)
        prof[c] = {"edges": [float(e) for e in edges], "share": (counts / counts.sum()).round(6).tolist()}
    return prof


def psi(reference: dict, values: pd.Series) -> float | None:
    s = pd.to_numeric(values, errors="coerce").dropna()
    if not len(s):
        return None
    edges = np.array(reference["edges"])
    counts, _ = np.histogram(s.clip(edges[0], edges[-1]), bins=edges)
    actual = counts / counts.sum()
    expected = np.array(reference["share"])
    eps = 1e-4
    a, e = np.clip(actual, eps, None), np.clip(expected, eps, None)
    return round(float(np.sum((a - e) * np.log(a / e))), 4)


def drift_report(frames, settings: Settings, problem: str) -> dict:
    meta = registry.load_metadata(settings, problem)
    if not meta:
        return {"problem": problem, "status": "no_model"}
    ds = BUILDERS[problem](frames, settings.as_of)
    threshold = float(settings.section("monitoring").get("drift_psi_threshold", 0.25))
    feats = {c: psi(ref, ds.X[c]) for c, ref in meta.get("drift_reference", {}).items() if c in ds.X}
    drifted = sorted([c for c, v in feats.items() if v is not None and v > threshold])
    new_rows = int(len(ds.X)) - int(meta["data"]["rows"])
    return {"problem": problem, "version": meta["version"], "trained_at": meta["trained_at"],
            "rows_at_training": meta["data"]["rows"], "rows_now": int(len(ds.X)), "new_rows": new_rows,
            "psi": feats, "drifted_features": drifted, "drift": bool(drifted)}


# ------------------------------------------------------------------ prediction log
def _log_path(settings: Settings):
    p = settings.path("reports_dir") / "monitoring"
    p.mkdir(parents=True, exist_ok=True)
    return p / "predictions.jsonl"


def log_predictions(settings: Settings, problem: str, version: str, rows: list[dict]) -> None:
    if not rows:
        return
    stamp = datetime.now().isoformat(timespec="seconds")
    with open(_log_path(settings), "a", encoding="utf-8") as f:
        for r in rows:
            f.write(json.dumps({"problem": problem, "version": version, "at": stamp, **r}, default=str) + "\n")


def live_accuracy(frames, settings: Settings) -> dict:
    """Score logged task-delay predictions whose tasks are now resolved."""
    path = _log_path(settings)
    if not path.exists():
        return {"evaluated": 0}
    logs = pd.read_json(path, lines=True)
    logs = logs[logs["problem"] == "task_delay"]
    if logs.empty:
        return {"evaluated": 0}
    logs = logs.sort_values("at").drop_duplicates("task_id", keep="first")   # first prediction per task
    outcome = task_outcomes(frames["tasks"].dropna(subset=["due_date"]), settings.as_of)
    resolved = outcome[outcome["resolved"]].set_index("task_id")["late"]
    hit = logs[logs["task_id"].isin(resolved.index)]
    if hit.empty:
        return {"evaluated": 0, "pending": int(len(logs))}
    actual = hit["task_id"].map(resolved).astype(int)
    predicted = (hit["probability"] >= 0.5).astype(int)
    return {"evaluated": int(len(hit)), "pending": int(len(logs) - len(hit)),
            "accuracy": round(float((actual == predicted).mean()), 4)}


def monitoring_report(frames, settings: Settings, problems: list[str] | None = None) -> dict:
    mon = settings.section("monitoring")
    min_new = int(mon.get("min_new_rows_for_retrain", 5))
    problems = problems or list(BUILDERS)
    per = {p: drift_report(frames, settings, p) for p in problems}
    live = live_accuracy(frames, settings)
    reasons = []
    for p, r in per.items():
        if r.get("status") == "no_model":
            reasons.append({"problem": p, "reason": "no_model"})
        elif r["drift"]:
            reasons.append({"problem": p, "reason": "drift", "features": r["drifted_features"]})
        elif r["new_rows"] >= min_new:
            reasons.append({"problem": p, "reason": "new_data", "new_rows": r["new_rows"]})
    meta = registry.load_metadata(settings, "task_delay")
    if live.get("evaluated", 0) >= 5 and meta and meta["metrics"].get("accuracy") is not None:
        if live["accuracy"] < meta["metrics"]["accuracy"] - 0.15:
            reasons.append({"problem": "task_delay", "reason": "live_accuracy_drop", "live": live["accuracy"]})
    return {"checked_at": datetime.now().isoformat(timespec="seconds"), "as_of": str(settings.as_of),
            "problems": per, "live_task_delay": live,
            "retrain_recommended": bool(reasons), "reasons": reasons}
