"""Step 9 - Inference: score live PMO data with the current models.

Frames are cached for a short time (default 60 s) so a page that calls several endpoints hits
the database once. Every response carries the model version and bilingual labels.
"""

from __future__ import annotations

import threading
import time

import numpy as np
import pandas as pd

from . import registry
from .clean import clean
from .collect import collect
from .config import Settings
from .features import BUILDERS, risk_features_for
from .i18n import feature, label, lang_of
from .monitor import log_predictions


class LiveData:
    """Cached, cleaned PMO tables."""

    def __init__(self, settings: Settings, ttl: float = 60.0):
        self.settings, self.ttl = settings, ttl
        self._frames, self._at, self._lock = None, 0.0, threading.Lock()

    def frames(self, refresh: bool = False):
        with self._lock:
            if refresh or self._frames is None or time.time() - self._at > self.ttl:
                self._frames, _ = clean(collect(self.settings))
                self._at = time.time()
            return self._frames


class Models:
    """Current model per problem, reloaded after (re)training."""

    def __init__(self, settings: Settings):
        self.settings = settings
        self._cache: dict[str, tuple] = {}

    def reload(self):
        self._cache = {p: registry.load(self.settings, p) for p in BUILDERS}

    def get(self, problem: str):
        if problem not in self._cache:
            self._cache[problem] = registry.load(self.settings, problem)
        return self._cache[problem]


def _id(v) -> str:
    """64-bit snowflake ids as strings: JavaScript numbers lose precision above 2^53."""
    return str(int(v))


def _missing(problem: str, lang) -> dict:
    return {"problem": problem, "available": False, "message": label("no_model", lang)}


def _band(p: float) -> str:
    return "High" if p >= 0.66 else "Medium" if p >= 0.4 else "Low"


def _top_drivers(meta: dict, lang, n=3) -> list[str]:
    return [feature(i["feature"], lang) for i in (meta.get("feature_importance") or [])[:n] if i["importance"] > 0]


def task_delay(models: Models, data: LiveData, project_id=None, lang="en", log=True) -> dict:
    model, meta = models.get("task_delay")
    if model is None:
        return _missing("task_delay", lang)
    ds = BUILDERS["task_delay"](data.frames(), data.settings.as_of)
    rows, X = ds.extra["predict_rows"], ds.extra["predict_X"]
    if project_id is not None:
        keep = (rows["project_id"] == int(project_id)).values
        rows, X = rows[keep].reset_index(drop=True), X[keep].reset_index(drop=True)
    proba = model.predict_proba(X)[:, list(model.classes_).index(1)] if len(X) else np.array([])
    items = []
    for r, p in sorted(zip(rows.to_dict("records"), proba), key=lambda x: -x[1]):
        band = _band(float(p))
        items.append({"task_id": _id(r["task_id"]), "project_id": _id(r["project_id"]), "title": r["title"],
                      "due_date": None if pd.isna(r["due_date"]) else r["due_date"].date().isoformat(),
                      "status": r["status"], "priority": r["priority"],
                      "probability": round(float(p), 3), "risk_band": band, "risk_label": label(band, lang),
                      "verdict": label("late" if p >= 0.5 else "on_time", lang)})
    if log:
        log_predictions(data.settings, "task_delay", meta["version"],
                        [{"task_id": int(i["task_id"]), "probability": i["probability"]} for i in items])
    return {"problem": "task_delay", "available": True, "version": meta["version"], "model": meta["best_model"],
            "as_of": str(data.settings.as_of), "drivers": _top_drivers(meta, lang), "items": items}


def risk_severity(models: Models, data: LiveData, text: str, kind: str = "Risk", project_id=None,
                  lang="en") -> dict:
    model, meta = models.get("risk_severity")
    if model is None:
        return _missing("risk_severity", lang)
    X = risk_features_for(text, kind, None if project_id is None else int(project_id),
                          data.frames(), data.settings.as_of)
    proba = model.predict_proba(X)[0]
    classes = [str(c) for c in model.classes_]
    best = classes[int(np.argmax(proba))]
    return {"problem": "risk_severity", "available": True, "version": meta["version"], "model": meta["best_model"],
            "severity": best, "severity_label": label(best, lang), "confidence": round(float(proba.max()), 3),
            "probabilities": {c: round(float(p), 3) for c, p in zip(classes, proba)},
            "labels": {c: label(c, lang) for c in classes},
            "note": {"en": "Suggestion from a small sample - confirm probability and impact yourself.",
                     "ar": "اقتراح مبني على عينة صغيرة - أكّد الاحتمال والأثر بنفسك."}[lang_of(lang)]}


def spend_forecast(models: Models, data: LiveData, project_id=None, lang="en") -> dict:
    model, meta = models.get("spend_forecast")
    if model is None:
        return _missing("spend_forecast", lang)
    ds = BUILDERS["spend_forecast"](data.frames(), data.settings.as_of)
    rows, X = ds.extra["predict_rows"], ds.extra["predict_X"]
    if project_id is not None:
        keep = (rows["project_id"] == int(project_id)).values
        rows, X = rows[keep].reset_index(drop=True), X[keep].reset_index(drop=True)
    pred = np.clip(model.predict(X), 0, None) if len(X) else np.array([])
    mae = (meta.get("metrics") or {}).get("mae") or 0
    names = data.frames()["projects"].set_index("project_id")[["code", "name"]]
    items = []
    for r, p in zip(rows.to_dict("records"), pred):
        plan = None if pd.isna(r["planned_estimate"]) else round(float(r["planned_estimate"]), 2)
        items.append({"project_id": _id(r["project_id"]),
                      "code": names["code"].get(r["project_id"]), "name": names["name"].get(r["project_id"]),
                      "month": r["month"].date().isoformat(), "forecast": round(float(p), 2),
                      "low": round(max(float(p) - mae, 0), 2), "high": round(float(p) + mae, 2),
                      "planned_estimate": plan,
                      "last_month": None if pd.isna(r["lag1"]) else round(float(r["lag1"]), 2),
                      "over_plan": bool(plan is not None and p > plan * 1.1)})
    history = []
    g = ds.X.assign(project_id=ds.ids.values, month=ds.extra["months"].values, spend=ds.y.values)
    if project_id is not None:
        g = g[g["project_id"] == int(project_id)]
    for m, s in g.groupby("month")["spend"].sum().items():
        history.append({"month": pd.Timestamp(m).date().isoformat(), "spend": round(float(s), 2)})
    return {"problem": "spend_forecast", "available": True, "version": meta["version"], "model": meta["best_model"],
            "mae": mae, "drivers": _top_drivers(meta, lang), "items": items, "history": history}


def cost_anomalies(models: Models, data: LiveData, project_id=None, top: int = 10, lang="en") -> dict:
    model, meta = models.get("cost_anomaly")
    if model is None:
        return _missing("cost_anomaly", lang)
    ds = BUILDERS["cost_anomaly"](data.frames(), data.settings.as_of)
    rows = ds.extra["rows"]
    score = -model.decision_function(ds.X)          # higher = more unusual
    flag = model.predict(ds.X) == -1
    df = rows.assign(score=score, flag=flag, lag=ds.X["posting_lag_days"].values,
                     vs_cat=ds.X["amount_vs_category"].values)
    if project_id is not None:
        df = df[df["project_id"] == int(project_id)]
    df = df.sort_values("score", ascending=False).head(int(top))
    cats = data.frames()["categories"].set_index("category_id")["name"]
    reasons_l = lang_of(lang)
    items = []
    for r in df.to_dict("records"):
        why = []
        if r["vs_cat"] and r["vs_cat"] > 2.5:
            why.append({"en": f"{r['vs_cat']:.1f}x the category median", "ar": f"{r['vs_cat']:.1f} ضعف وسيط الفئة"}[reasons_l])
        if pd.notna(r["lag"]) and r["lag"] > 30:
            why.append({"en": f"posted {int(r['lag'])} days after the expense", "ar": f"رُحّلت بعد {int(r['lag'])} يوماً من المصروف"}[reasons_l])
        items.append({"transaction_id": _id(r["transaction_id"]), "project_id": _id(r["project_id"]),
                      "category": cats.get(r["category_id"]), "amount": round(float(r["amount"]), 2),
                      "txn_date": r["txn_date"].date().isoformat(), "description": r["description"],
                      "score": round(float(r["score"]), 4), "flagged": bool(r["flag"]),
                      "label": label("anomaly" if r["flag"] else "normal", lang), "reasons": why})
    return {"problem": "cost_anomaly", "available": True, "version": meta["version"], "model": meta["best_model"],
            "flagged_total": int(flag.sum()), "items": items}


def segments(models: Models, data: LiveData, lang="en") -> dict:
    model, meta = models.get("portfolio_segments")
    if model is None:
        return _missing("portfolio_segments", lang)
    ds = BUILDERS["portfolio_segments"](data.frames(), data.settings.as_of)
    labels = model.predict(ds.X)
    seg_meta = meta.get("segments", {})
    rows = ds.extra["rows"]
    items = []
    for (r, x), c in zip(zip(rows.to_dict("records"), ds.X.to_dict("records")), labels):
        key = seg_meta.get(str(int(c)), {}).get("name_key", f"seg_{c}")
        base, _, n = key.partition("#")
        items.append({"project_id": _id(r["project_id"]), "code": r["code"], "name": r["name"],
                      "status": r["status"], "segment": int(c), "segment_key": key,
                      "segment_label": label(base, lang) + (f" {n}" if n else ""),
                      **{k: (None if pd.isna(v) else round(float(v), 3)) for k, v in x.items()}})
    summary = [{"segment": int(k), "segment_key": v["name_key"],
                "segment_label": label(v["name_key"].split("#")[0], lang), "size": v["size"],
                "centroid": v["centroid"]} for k, v in seg_meta.items()]
    return {"problem": "portfolio_segments", "available": True, "version": meta["version"],
            "silhouette": meta["metrics"].get("silhouette"), "segments": summary, "items": items}
