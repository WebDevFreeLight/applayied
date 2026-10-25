"""Step 8 - Model evaluation: metrics, comparison charts and explainability.

Classification: accuracy, precision, recall, F1 (macro for multiclass), ROC-AUC, confusion matrix.
Regression: MAE, RMSE, MAPE (non-zero actuals), R^2, predicted-vs-actual and residuals.
Explainability: permutation importance on held-out data (model-agnostic).
"""

from __future__ import annotations

from pathlib import Path

import matplotlib

matplotlib.use("Agg")
import matplotlib.pyplot as plt  # noqa: E402
import numpy as np  # noqa: E402
import pandas as pd  # noqa: E402
import seaborn as sns  # noqa: E402
from sklearn.inspection import permutation_importance  # noqa: E402
from sklearn.metrics import (accuracy_score, confusion_matrix, f1_score, mean_absolute_error,  # noqa: E402
                             mean_squared_error, precision_score, r2_score, recall_score,
                             roc_auc_score, roc_curve)


def _r(v) -> float | None:
    return None if v is None or (isinstance(v, float) and np.isnan(v)) else round(float(v), 4)


def classification_metrics(y_true, y_pred, proba=None, classes=None) -> dict:
    y_true, y_pred = np.asarray(y_true), np.asarray(y_pred)
    binary = classes is not None and len(classes) == 2
    avg = "binary" if binary else "macro"
    m = {
        "accuracy": _r(accuracy_score(y_true, y_pred)),
        "precision": _r(precision_score(y_true, y_pred, average=avg, zero_division=0)),
        "recall": _r(recall_score(y_true, y_pred, average=avg, zero_division=0)),
        "f1": _r(f1_score(y_true, y_pred, average=avg, zero_division=0)),
        "f1_macro": _r(f1_score(y_true, y_pred, average="macro", zero_division=0)),
        "support": int(len(y_true)),
    }
    if proba is not None and len(np.unique(y_true)) > 1:
        try:
            m["roc_auc"] = _r(roc_auc_score(y_true, proba[:, 1]) if binary
                              else roc_auc_score(y_true, proba, multi_class="ovr", labels=classes))
        except ValueError:
            m["roc_auc"] = None
    labels = list(classes) if classes is not None else sorted(np.unique(np.r_[y_true, y_pred]))
    m["confusion_matrix"] = {"labels": [str(x) for x in labels],
                             "matrix": confusion_matrix(y_true, y_pred, labels=labels).tolist()}
    return m


def regression_metrics(y_true, y_pred) -> dict:
    y_true, y_pred = np.asarray(y_true, float), np.asarray(y_pred, float)
    nz = y_true != 0
    return {
        "mae": _r(mean_absolute_error(y_true, y_pred)),
        "rmse": _r(np.sqrt(mean_squared_error(y_true, y_pred))),
        "mape": _r(np.mean(np.abs((y_true[nz] - y_pred[nz]) / y_true[nz]))) if nz.any() else None,
        "r2": _r(r2_score(y_true, y_pred)) if len(y_true) > 1 else None,
        "support": int(len(y_true)),
    }


def importance(model, X: pd.DataFrame, y, scoring, rs: int, top: int = 12) -> list[dict]:
    try:
        res = permutation_importance(model, X, y, scoring=scoring, n_repeats=10, random_state=rs)
    except Exception:  # noqa: BLE001 - importance is best-effort
        return []
    order = np.argsort(res.importances_mean)[::-1][:top]
    return [{"feature": X.columns[i], "importance": _r(res.importances_mean[i]),
             "std": _r(res.importances_std[i])} for i in order]


# ------------------------------------------------------------------ plots
def plot_comparison(candidates: list[dict], metric: str, path: Path) -> str:
    rows = [c for c in candidates if c.get("cv_mean") is not None]
    if not rows:
        return ""
    df = pd.DataFrame(rows).sort_values("cv_mean")
    fig, ax = plt.subplots(figsize=(7, 0.45 * len(df) + 1.2))
    colors = ["#c8c8c8" if n.startswith("baseline") else "#3b6ea5" for n in df["name"]]
    ax.barh(df["name"], df["cv_mean"], xerr=df["cv_std"].fillna(0), color=colors)
    ax.set_xlabel(f"cross-validated {metric}")
    ax.set_title("Model comparison (grey = baseline)")
    fig.tight_layout()
    fig.savefig(path, dpi=110)
    plt.close(fig)
    return path.name


def plot_confusion(cm: dict, path: Path) -> str:
    fig, ax = plt.subplots(figsize=(4.2, 3.6))
    sns.heatmap(np.array(cm["matrix"]), annot=True, fmt="d", cmap="Blues", cbar=False,
                xticklabels=cm["labels"], yticklabels=cm["labels"], ax=ax)
    ax.set_xlabel("Predicted")
    ax.set_ylabel("Actual")
    fig.tight_layout()
    fig.savefig(path, dpi=110)
    plt.close(fig)
    return path.name


def plot_roc(y_true, proba, path: Path) -> str:
    if len(np.unique(y_true)) < 2:
        return ""
    fpr, tpr, _ = roc_curve(y_true, proba[:, 1])
    fig, ax = plt.subplots(figsize=(4.2, 3.8))
    ax.plot(fpr, tpr, color="#3b6ea5", label=f"AUC = {roc_auc_score(y_true, proba[:, 1]):.2f}")
    ax.plot([0, 1], [0, 1], "--", color="grey")
    ax.set_xlabel("False positive rate")
    ax.set_ylabel("True positive rate")
    ax.legend(loc="lower right")
    fig.tight_layout()
    fig.savefig(path, dpi=110)
    plt.close(fig)
    return path.name


def plot_regression(y_true, y_pred, path: Path) -> str:
    y_true, y_pred = np.asarray(y_true, float), np.asarray(y_pred, float)
    fig, axes = plt.subplots(1, 2, figsize=(9, 3.8))
    axes[0].scatter(y_true, y_pred, color="#3b6ea5")
    lim = [0, max(y_true.max(initial=0), y_pred.max(initial=0)) * 1.05 or 1]
    axes[0].plot(lim, lim, "--", color="grey")
    axes[0].set_xlabel("Actual")
    axes[0].set_ylabel("Predicted")
    axes[0].set_title("Predicted vs actual")
    axes[1].scatter(y_pred, y_true - y_pred, color="#b5543c")
    axes[1].axhline(0, color="grey", ls="--")
    axes[1].set_xlabel("Predicted")
    axes[1].set_ylabel("Residual")
    axes[1].set_title("Residuals")
    fig.tight_layout()
    fig.savefig(path, dpi=110)
    plt.close(fig)
    return path.name


def plot_importance(items: list[dict], path: Path) -> str:
    if not items:
        return ""
    df = pd.DataFrame(items).iloc[::-1]
    fig, ax = plt.subplots(figsize=(6.5, 0.4 * len(df) + 1.2))
    ax.barh(df["feature"], df["importance"], xerr=df["std"], color="#6b8f71")
    ax.set_title("Permutation importance")
    fig.tight_layout()
    fig.savefig(path, dpi=110)
    plt.close(fig)
    return path.name
