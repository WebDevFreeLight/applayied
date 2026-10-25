"""Step 7 - Model training (and the evaluation that decides which model ships).

Supervised problems
  1. split: stratified hold-out (classification) or the latest months (spend forecast)
  2. tune every candidate with GridSearchCV + k-fold CV on the training part
  3. pick the best by cross-validated score; compare it with the Dummy baseline
  4. score the winner on the hold-out; permutation importance on the hold-out
  5. refit the winner on ALL rows and save it (with metadata) to the registry
  Very small tables (< 40 rows) skip the hold-out and report cross-validated predictions.

Unsupervised problems
  cost_anomaly       - candidates scored by recall on deliberately injected outliers
  portfolio_segments - KMeans over a k range, chosen by silhouette
"""

from __future__ import annotations

import hashlib
import json
import platform
import warnings
from datetime import datetime

import numpy as np
import pandas as pd
import sklearn
from sklearn.base import clone
from sklearn.cluster import KMeans
from sklearn.decomposition import PCA
from sklearn.ensemble import IsolationForest
from sklearn.impute import SimpleImputer
from sklearn.metrics import silhouette_score
from sklearn.model_selection import (GridSearchCV, StratifiedKFold, TimeSeriesSplit,
                                     cross_val_predict, train_test_split)
from sklearn.neighbors import LocalOutlierFactor
from sklearn.pipeline import Pipeline
from sklearn.preprocessing import StandardScaler

from . import evaluate as ev
from . import registry
from .config import Settings
from .features import BUILDERS, Dataset
from .i18n import LABELS
from .models import build_pipeline, classification_candidates, preprocessor, regression_candidates
from .monitor import reference_profile
from .problems import PROBLEMS

warnings.filterwarnings("ignore", category=UserWarning)
warnings.filterwarnings("ignore", category=RuntimeWarning)
SMALL_DATA = 40


def _fingerprint(ds: Dataset) -> str:
    h = hashlib.sha256(pd.util.hash_pandas_object(ds.X.astype(str), index=False).values.tobytes())
    if ds.y is not None:
        h.update(pd.util.hash_pandas_object(ds.y.astype(str), index=False).values.tobytes())
    return h.hexdigest()[:16]


def _base_metadata(problem: str, ds: Dataset, settings: Settings, data_info: dict, version: str) -> dict:
    return {
        "problem": problem,
        "version": version,
        "type": PROBLEMS[problem]["type"],
        "trained_at": datetime.now().isoformat(timespec="seconds"),
        "as_of": str(settings.as_of),
        "data": {**data_info, "rows": int(len(ds.X)), "fingerprint": _fingerprint(ds)},
        "features": {"numeric": ds.numeric, "categorical": ds.categorical, "text": ds.text},
        "drift_reference": reference_profile(ds.X, ds.numeric),
        "environment": {"python": platform.python_version(), "sklearn": sklearn.__version__},
    }


def _search(name, est, grid, ds, X, y, cv, scoring):
    pipe = build_pipeline(ds, est)
    try:
        gs = GridSearchCV(pipe, grid or {}, scoring=scoring, cv=cv, n_jobs=1, error_score=np.nan, refit=True)
        gs.fit(X, y)
        i = gs.best_index_
        mean = gs.cv_results_["mean_test_score"][i]
        std = gs.cv_results_["std_test_score"][i]
        return gs.best_estimator_, {
            "name": name, "cv_mean": None if np.isnan(mean) else round(float(mean), 4),
            "cv_std": None if np.isnan(std) else round(float(std), 4),
            "params": {k.replace("model__", ""): v for k, v in gs.best_params_.items()},
        }
    except Exception as e:  # noqa: BLE001 - one failing candidate must not stop training
        return None, {"name": name, "cv_mean": None, "cv_std": None, "error": str(e)[:200]}


def _pick(results: list[tuple]) -> tuple:
    """Best non-baseline candidate by CV score (baselines are kept only for comparison)."""
    scored = [(m, r) for m, r in results if m is not None and r["cv_mean"] is not None]
    pool = [x for x in scored if not x[1]["name"].startswith("baseline")] or scored
    return max(pool, key=lambda x: x[1]["cv_mean"])


def _best_baseline(results):
    base = [r for m, r in results if r["name"].startswith("baseline") and r["cv_mean"] is not None]
    return max(base, key=lambda r: r["cv_mean"]) if base else None


# ------------------------------------------------------------------ classification
def train_classification(problem, ds: Dataset, settings: Settings, out_dir, version, data_info) -> tuple:
    tr = settings.section("training")
    rs = settings.random_state
    scoring = tr.get("scoring", {}).get(problem, "f1_macro")
    X, y = ds.X, ds.y.astype(int) if ds.y.dtype != object else ds.y.astype(str)
    classes = sorted(y.unique().tolist())
    if len(classes) < 2:
        raise ValueError("only one class present - nothing to learn")
    if scoring == "f1" and len(classes) > 2:
        scoring = "f1_macro"
    small = len(X) < SMALL_DATA
    if small:
        X_tr, y_tr, X_te, y_te = X, y, None, None
    else:
        X_tr, X_te, y_tr, y_te = train_test_split(X, y, test_size=tr.get("test_size", 0.25),
                                                  stratify=y, random_state=rs)
    folds = int(max(2, min(tr.get("cv_folds", 5), y_tr.value_counts().min())))
    cv = StratifiedKFold(n_splits=folds, shuffle=True, random_state=rs)

    results = [_search(n, e, g, ds, X_tr, y_tr, cv, scoring)
               for n, (e, g) in classification_candidates(rs).items()]
    best_model, best = _pick(results)
    baseline = _best_baseline(results)
    base_model = next(m for m, r in results if baseline and r["name"] == baseline["name"])

    charts = {"comparison": ev.plot_comparison([r for _, r in results], scoring, out_dir / "comparison.png")}
    if small:
        strategy = f"{folds}-fold cross-validated predictions on all {len(X)} rows"
        pred = cross_val_predict(best_model, X, y, cv=cv)
        proba = cross_val_predict(best_model, X, y, cv=cv, method="predict_proba")
        test = ev.classification_metrics(y, pred, proba, classes)
        base_pred = cross_val_predict(base_model, X, y, cv=cv)
        base_test = ev.classification_metrics(y, base_pred, None, classes)
        imp_X, imp_y = X, y
        roc_y, roc_p = y, proba
    else:
        strategy = f"{len(X_te)}-row stratified hold-out ({folds}-fold CV for selection)"
        proba = best_model.predict_proba(X_te)
        test = ev.classification_metrics(y_te, best_model.predict(X_te), proba, classes)
        base_test = ev.classification_metrics(y_te, base_model.predict(X_te), None, classes)
        imp_X, imp_y = X_te, y_te
        roc_y, roc_p = y_te, proba
    charts["confusion"] = ev.plot_confusion(test["confusion_matrix"], out_dir / "confusion_matrix.png")
    if len(classes) == 2:
        charts["roc"] = ev.plot_roc(roc_y, roc_p, out_dir / "roc_curve.png")
    imp = ev.importance(best_model, imp_X, imp_y, scoring, rs)
    charts["importance"] = ev.plot_importance(imp, out_dir / "feature_importance.png")

    final = build_pipeline(ds, clone(best_model.named_steps["model"])).fit(X, y)   # refit on all rows
    key = "f1" if len(classes) == 2 else "f1_macro"
    meta = _base_metadata(problem, ds, settings, data_info, version)
    meta.update({
        "label_distribution": {str(k): int(v) for k, v in y.value_counts().items()},
        "classes": [str(c) for c in classes],
        "scoring": scoring, "evaluation_strategy": strategy,
        "candidates": [r for _, r in results],
        "best_model": best["name"], "best_params": best["params"],
        "metrics": test, "baseline": {"name": baseline["name"] if baseline else None, "metrics": base_test},
        "beats_baseline": bool(test.get(key) is not None and test[key] > (base_test.get(key) or 0)),
        "feature_importance": imp, "charts": {k: v for k, v in charts.items() if v},
    })
    return final, meta


# ------------------------------------------------------------------ regression (spend forecast)
def train_regression(problem, ds: Dataset, settings: Settings, out_dir, version, data_info) -> tuple:
    tr = settings.section("training")
    rs = settings.random_state
    scoring = tr.get("scoring", {}).get(problem, "neg_mean_absolute_error")
    months = ds.extra["months"]
    order = np.argsort(months.values, kind="stable")
    X, y, months = ds.X.iloc[order].reset_index(drop=True), ds.y.iloc[order].reset_index(drop=True), \
        months.iloc[order].reset_index(drop=True)
    # time-based hold-out: the latest months are the test set
    uniq = np.sort(months.unique())
    n_test_months = max(1, int(round(len(uniq) * tr.get("test_size", 0.25))))
    cut = uniq[-n_test_months]
    tr_mask = (months < cut).values
    X_tr, y_tr, X_te, y_te = X[tr_mask], y[tr_mask], X[~tr_mask], y[~tr_mask]
    cv = TimeSeriesSplit(n_splits=int(max(2, min(tr.get("cv_folds", 5), len(X_tr) // 6))))

    lag1 = ds.numeric.index("lag1")
    results = [_search(n, e, g, ds, X_tr, y_tr, cv, scoring)
               for n, (e, g) in regression_candidates(rs, lag1).items()]
    best_model, best = _pick(results)
    naive = next((m for m, r in results if r["name"] == "baseline_last_month" and m is not None), None)

    pred = best_model.predict(X_te)
    test = ev.regression_metrics(y_te, pred)
    base_test = ev.regression_metrics(y_te, naive.predict(X_te)) if naive is not None else {}
    imp = ev.importance(best_model, X_te, y_te, scoring, rs)
    charts = {
        "comparison": ev.plot_comparison([r for _, r in results], "neg MAE", out_dir / "comparison.png"),
        "regression": ev.plot_regression(y_te, pred, out_dir / "predicted_vs_actual.png"),
        "importance": ev.plot_importance(imp, out_dir / "feature_importance.png"),
    }
    final = build_pipeline(ds, clone(best_model.named_steps["model"])).fit(X, y)
    meta = _base_metadata(problem, ds, settings, data_info, version)
    meta.update({
        "target_summary": {"mean": round(float(y.mean()), 2), "std": round(float(y.std()), 2)},
        "scoring": scoring,
        "evaluation_strategy": f"time-based hold-out: {len(X_te)} rows from {pd.Timestamp(cut).date()} on "
                               f"({cv.n_splits}-split TimeSeriesSplit for selection)",
        "candidates": [r for _, r in results],
        "best_model": best["name"], "best_params": best["params"],
        "metrics": test, "baseline": {"name": "baseline_last_month", "metrics": base_test},
        "beats_baseline": bool(base_test and test["mae"] is not None and test["mae"] < base_test["mae"]),
        "feature_importance": imp, "charts": {k: v for k, v in charts.items() if v},
    })
    return final, meta


# ------------------------------------------------------------------ anomalies
def _inject(X: pd.DataFrame, rng, share=0.1) -> pd.DataFrame:
    k = max(3, int(len(X) * share))
    fake = X.sample(k, random_state=int(rng.integers(1e9)), replace=len(X) < k).copy()
    fake["log_amount"] = fake["log_amount"] + np.log(8)
    for c in ["amount_vs_category", "amount_vs_project", "share_of_budget"]:
        fake[c] = fake[c] * 8
    fake["posting_lag_days"] = fake["posting_lag_days"].fillna(0) + 90
    return fake


def train_anomaly(problem, ds: Dataset, settings: Settings, out_dir, version, data_info) -> tuple:
    rs = settings.random_state
    c = float(settings.section("anomaly").get("contamination", 0.06))
    rng = np.random.default_rng(rs)
    candidates = {
        "isolation_forest_100": IsolationForest(n_estimators=100, contamination=c, random_state=rs),
        "isolation_forest_300": IsolationForest(n_estimators=300, contamination=c, random_state=rs),
        "local_outlier_factor_10": LocalOutlierFactor(n_neighbors=10, contamination=c, novelty=True),
        "local_outlier_factor_20": LocalOutlierFactor(n_neighbors=20, contamination=c, novelty=True),
    }
    results = []
    for name, est in candidates.items():
        recalls, precisions = [], []
        for _ in range(5):
            fake = _inject(ds.X, rng)
            pipe = build_pipeline(ds, est).fit(ds.X)
            flag_real = pipe.predict(ds.X) == -1
            flag_fake = pipe.predict(fake) == -1
            recalls.append(flag_fake.mean())
            tp, fp = flag_fake.sum(), flag_real.sum()
            precisions.append(tp / max(tp + fp, 1))
        results.append({"name": name, "cv_mean": round(float(np.mean(recalls)), 4),
                        "cv_std": round(float(np.std(recalls)), 4),
                        "precision_on_injected": round(float(np.mean(precisions)), 4)})
    best = max(results, key=lambda r: (r["cv_mean"], r["precision_on_injected"]))
    final = build_pipeline(ds, clone(candidates[best["name"]])).fit(ds.X)
    flagged = int((final.predict(ds.X) == -1).sum())
    charts = {"comparison": ev.plot_comparison(results, "recall on injected outliers", out_dir / "comparison.png")}
    meta = _base_metadata(problem, ds, settings, data_info, version)
    meta.update({
        "scoring": "recall on injected outliers (5 repeats)", "evaluation_strategy":
            "outlier injection: copies of real rows made 8x larger and posted 90 days late",
        "candidates": results, "best_model": best["name"], "best_params": {"contamination": c},
        "metrics": {"injected_recall": best["cv_mean"], "precision_on_injected": best["precision_on_injected"],
                    "flagged_in_training_data": flagged, "rows": int(len(ds.X))},
        "beats_baseline": best["cv_mean"] >= 0.6, "charts": charts,
    })
    return final, meta


# ------------------------------------------------------------------ clustering
def segment_names(centers: pd.DataFrame) -> dict[int, str]:
    """Readable name per cluster from its centroid (original units)."""
    names = {}
    for k, row in centers.iterrows():
        if (row.get("late_task_share", 0) or 0) >= 0.5 or (row.get("utilization", 0) or 0) >= 0.9:
            names[k] = "seg_at_risk"
        elif (row.get("avg_progress", 0) or 0) < 25:
            names[k] = "seg_early"
        elif (row.get("utilization", 0) or 0) >= 0.6:
            names[k] = "seg_heavy_spend"
        else:
            names[k] = "seg_on_track"
    return names


def train_clustering(problem, ds: Dataset, settings: Settings, out_dir, version, data_info) -> tuple:
    rs = settings.random_state
    seg = settings.section("segments")
    n = len(ds.X)
    k_range = range(int(seg.get("k_min", 2)), int(min(seg.get("k_max", 4), n - 1)) + 1)
    results, models = [], {}
    for k in k_range:
        pipe = Pipeline([("impute", SimpleImputer(strategy="median")), ("scale", StandardScaler()),
                         ("model", KMeans(n_clusters=k, n_init=20, random_state=rs))]).fit(ds.X)
        Z = pipe[:-1].transform(ds.X)
        labels = pipe.named_steps["model"].labels_
        score = silhouette_score(Z, labels) if len(set(labels)) > 1 else np.nan
        results.append({"name": f"kmeans_k{k}", "cv_mean": round(float(score), 4), "cv_std": 0.0,
                        "inertia": round(float(pipe.named_steps["model"].inertia_), 3),
                        "smallest_segment": int(np.bincount(labels).min())})
        models[f"kmeans_k{k}"] = pipe
    # a one-project "segment" is just an outlier: prefer k whose segments all hold >= 2 projects
    usable = [r for r in results if r["smallest_segment"] >= 2] or results
    best = max(usable, key=lambda r: r["cv_mean"])
    final = models[best["name"]]
    labels = final.named_steps["model"].labels_
    centers = ds.X.assign(_c=labels).groupby("_c")[ds.numeric].mean()
    names = segment_names(centers)
    seen: dict[str, int] = {}
    for k, v in list(names.items()):                     # keep names unique
        seen[v] = seen.get(v, 0) + 1
        if seen[v] > 1:
            names[k] = f"{v}#{seen[v]}"

    Z = final[:-1].transform(ds.X)
    xy = PCA(n_components=2, random_state=rs).fit_transform(Z) if Z.shape[1] >= 2 else np.c_[Z, Z]
    import matplotlib.pyplot as plt
    fig, ax = plt.subplots(figsize=(6, 4.5))
    for c in sorted(set(labels)):
        m = labels == c
        base = names[c].split("#")[0]
        ax.scatter(xy[m, 0], xy[m, 1], s=80, label=LABELS.get(base, {}).get("en", base))
    for (px, py), code in zip(xy, ds.extra["rows"]["code"]):
        ax.annotate(str(code), (px, py), fontsize=8, xytext=(4, 4), textcoords="offset points")
    ax.set_title("Project segments (PCA projection)")
    ax.legend()
    fig.tight_layout()
    fig.savefig(out_dir / "segments_pca.png", dpi=110)
    plt.close(fig)

    meta = _base_metadata(problem, ds, settings, data_info, version)
    meta.update({
        "scoring": "silhouette", "evaluation_strategy": f"KMeans for k in {list(k_range)}; best silhouette",
        "candidates": results, "best_model": best["name"], "best_params": {"k": int(best["name"].split("k")[-1])},
        "metrics": {"silhouette": best["cv_mean"], "rows": n},
        "segments": {str(k): {"name_key": v, "size": int((labels == k).sum()),
                              "centroid": {c: (None if pd.isna(x) else round(float(x), 3))
                                           for c, x in centers.loc[k].items()}}
                     for k, v in names.items()},
        "beats_baseline": best["cv_mean"] >= 0.25,
        "charts": {"comparison": ev.plot_comparison(results, "silhouette", out_dir / "comparison.png"),
                   "segments": "segments_pca.png"},
    })
    return final, meta


TRAINERS = {"classification": train_classification, "regression": train_regression,
            "anomaly": train_anomaly, "clustering": train_clustering}


def train_all(frames, settings: Settings, problems: list[str] | None = None, data_info: dict | None = None) -> dict:
    """Train each problem; returns {problem: summary}. A failure in one problem is reported, not raised."""
    problems = problems or settings.section("training").get("problems") or list(BUILDERS)
    summary = {}
    for problem in problems:
        version = registry.new_version()
        out_dir = settings.path("reports_dir") / "models" / problem / version
        out_dir.mkdir(parents=True, exist_ok=True)
        try:
            ds = BUILDERS[problem](frames, settings.as_of)
            model, meta = TRAINERS[PROBLEMS[problem]["type"]](problem, ds, settings, out_dir, version,
                                                             data_info or {})
            meta["charts_dir"] = str(out_dir.relative_to(settings.path("reports_dir"))).replace("\\", "/")
            registry.save(settings, problem, model, meta, version)
            (out_dir / "metadata.json").write_text(json.dumps(meta, indent=2, ensure_ascii=False, default=str),
                                                   encoding="utf-8")
            summary[problem] = {"status": "trained", "version": version, "best_model": meta["best_model"],
                                "metrics": {k: v for k, v in meta["metrics"].items() if k != "confusion_matrix"},
                                "beats_baseline": meta["beats_baseline"], "rows": meta["data"]["rows"]}
        except Exception as e:  # noqa: BLE001
            summary[problem] = {"status": "failed", "error": f"{type(e).__name__}: {e}"[:300]}
    return summary
