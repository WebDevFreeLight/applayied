"""Step 4 - Exploratory data analysis.

For every problem: statistical summary, missingness, class balance, skew / IQR outliers,
correlation with the label, and charts (histograms, boxplots by label, correlation heatmap,
category counts). Plus two portfolio views: the risk matrix (probability x impact) and the
monthly spend trend. Output: reports/eda/<problem>/*.png, summary.json and EDA.md (en + ar).
"""

from __future__ import annotations

import json
from pathlib import Path

import matplotlib

matplotlib.use("Agg")
import matplotlib.pyplot as plt  # noqa: E402
import numpy as np  # noqa: E402
import pandas as pd  # noqa: E402
import seaborn as sns  # noqa: E402

from .features import BUILDERS, Dataset, monthly_spend  # noqa: E402
from .problems import PROBLEMS  # noqa: E402

sns.set_theme(style="whitegrid", context="notebook")


def _save(fig, path: Path) -> str:
    fig.tight_layout()
    fig.savefig(path, dpi=110)
    plt.close(fig)
    return path.name


def _iqr_outliers(s: pd.Series) -> int:
    s = s.dropna()
    if len(s) < 4:
        return 0
    q1, q3 = s.quantile([0.25, 0.75])
    iqr = q3 - q1
    return int(((s < q1 - 1.5 * iqr) | (s > q3 + 1.5 * iqr)).sum())


def summarize(ds: Dataset, problem: str) -> dict:
    num = ds.X[ds.numeric].apply(pd.to_numeric, errors="coerce") if ds.numeric else pd.DataFrame()
    out = {
        "rows": int(len(ds.X)),
        "features": ds.features,
        "missing": {c: int(n) for c, n in ds.X.isna().sum().items() if n},
        "numeric_summary": json.loads(num.describe().round(3).to_json()) if not num.empty else {},
        "skew": {c: round(float(v), 2) for c, v in num.skew().items() if pd.notna(v)} if not num.empty else {},
        "iqr_outliers": {c: _iqr_outliers(num[c]) for c in num.columns} if not num.empty else {},
    }
    if ds.y is not None:
        if PROBLEMS[problem]["type"] == "classification":
            out["class_balance"] = {str(k): int(v) for k, v in ds.y.value_counts().items()}
            y_num = pd.Series(pd.factorize(ds.y)[0]) if ds.y.dtype == object else ds.y
        else:
            out["target_summary"] = json.loads(ds.y.describe().round(2).to_json())
            y_num = ds.y
        if not num.empty and y_num.nunique() > 1:
            corr = num.apply(lambda c: c.corr(y_num.reset_index(drop=True)))
            out["correlation_with_label"] = {c: round(float(v), 3) for c, v in
                                             corr.dropna().sort_values(key=abs, ascending=False).items()}
    return out


def findings(summary: dict, problem: str) -> list[dict]:
    """Plain-language bullet points (en/ar) derived from the summary."""
    notes = []
    n = summary["rows"]
    notes.append({"en": f"{n} rows available for this problem.", "ar": f"يتوفر {n} صفاً لهذه المشكلة."})
    if n < 100:
        notes.append({"en": "Small sample: rely on cross-validation and treat metrics as indicative.",
                      "ar": "عينة صغيرة: يُعتمد على التحقق المتقاطع وتُعدّ المقاييس استرشادية."})
    bal = summary.get("class_balance")
    if bal:
        top = max(bal.values()) / max(sum(bal.values()), 1)
        if top > 0.6:
            notes.append({"en": f"Imbalanced classes (largest = {top:.0%}): use class weights and F1, not accuracy.",
                          "ar": f"فئات غير متوازنة (الأكبر = {top:.0%}): نستخدم أوزان الفئات ومقياس F1 لا الدقة."})
    corr = summary.get("correlation_with_label") or {}
    if corr:
        k, v = next(iter(corr.items()))
        notes.append({"en": f"Strongest linear signal: '{k}' (r = {v:+.2f}).",
                      "ar": f"أقوى إشارة خطية: «{k}» (r = {v:+.2f})."})
    skewed = [c for c, s in summary.get("skew", {}).items() if abs(s) > 1.5]
    if skewed:
        notes.append({"en": f"Skewed features ({', '.join(skewed[:4])}): scaled and handled by robust/tree models.",
                      "ar": f"خصائص ملتوية ({', '.join(skewed[:4])}): تم تحجيمها ومعالجتها بنماذج الأشجار."})
    if summary.get("missing"):
        notes.append({"en": f"Missing values in {', '.join(summary['missing'])}: imputed inside the pipeline.",
                      "ar": f"قيم مفقودة في {', '.join(summary['missing'])}: تُعوَّض داخل خط المعالجة."})
    return notes


def charts(ds: Dataset, problem: str, out: Path) -> list[str]:
    made = []
    num = ds.X[ds.numeric].apply(pd.to_numeric, errors="coerce") if ds.numeric else pd.DataFrame()
    if not num.empty:
        cols = num.columns[:9]
        fig, axes = plt.subplots(int(np.ceil(len(cols) / 3)), 3, figsize=(12, 3.2 * np.ceil(len(cols) / 3)))
        for ax, c in zip(np.ravel(axes), cols):
            sns.histplot(num[c].dropna(), ax=ax, kde=len(num[c].dropna()) > 5, color="#3b6ea5")
            ax.set_title(c, fontsize=10)
        for ax in np.ravel(axes)[len(cols):]:
            ax.axis("off")
        made.append(_save(fig, out / "histograms.png"))

        if num.shape[1] > 1:
            data = num.copy()
            if ds.y is not None and PROBLEMS[problem]["type"] != "classification":
                data["TARGET"] = ds.y.values
            elif ds.y is not None and ds.y.nunique() == 2:
                data["TARGET"] = ds.y.astype(float).values
            fig, ax = plt.subplots(figsize=(9, 7))
            sns.heatmap(data.corr(), annot=True, fmt=".2f", cmap="vlag", center=0, ax=ax, cbar=False,
                        annot_kws={"size": 7})
            ax.set_title("Correlation heatmap")
            made.append(_save(fig, out / "correlation_heatmap.png"))

    if ds.y is not None and PROBLEMS[problem]["type"] == "classification":
        fig, ax = plt.subplots(figsize=(5, 3.2))
        ds.y.astype(str).value_counts().plot.bar(ax=ax, color="#3b6ea5")
        ax.set_title("Class balance")
        made.append(_save(fig, out / "class_balance.png"))
        if not num.empty:
            cols = num.columns[:6]
            fig, axes = plt.subplots(2, 3, figsize=(12, 6.5))
            for ax, c in zip(np.ravel(axes), cols):
                sns.boxplot(x=ds.y.astype(str), y=num[c], ax=ax, color="#9db8d9")
                ax.set_title(c, fontsize=10)
                ax.set_xlabel("")
            for ax in np.ravel(axes)[len(cols):]:
                ax.axis("off")
            made.append(_save(fig, out / "boxplots_by_label.png"))

    for c in ds.categorical[:3]:
        fig, ax = plt.subplots(figsize=(5, 3))
        ds.X[c].astype(str).value_counts().plot.barh(ax=ax, color="#6b8f71")
        ax.set_title(f"{c} counts")
        made.append(_save(fig, out / f"counts_{c}.png"))
    return made


def portfolio_charts(frames, as_of, out: Path) -> list[str]:
    made = []
    r = frames["risks"].dropna(subset=["probability", "impact"])
    if len(r):
        mat = pd.crosstab(r["impact"], r["probability"]).reindex(index=range(5, 0, -1), columns=range(1, 6), fill_value=0)
        fig, ax = plt.subplots(figsize=(5.5, 4.5))
        sns.heatmap(mat, annot=True, fmt="d", cmap="Reds", cbar=False, ax=ax)
        ax.set_xlabel("Probability")
        ax.set_ylabel("Impact")
        ax.set_title("Risk matrix (count)")
        made.append(_save(fig, out / "risk_matrix.png"))
    g = monthly_spend(frames, as_of)
    if len(g):
        fig, ax = plt.subplots(figsize=(10, 4))
        g.groupby("month")["spend"].sum().plot(ax=ax, marker="o", color="#3b6ea5")
        ax.set_title("Monthly posted spend - portfolio")
        ax.set_ylabel("Spend")
        made.append(_save(fig, out / "spend_trend.png"))
    return made


def run_eda(frames, settings, problems: list[str] | None = None) -> dict:
    root = settings.path("reports_dir") / "eda"
    root.mkdir(parents=True, exist_ok=True)
    result = {"portfolio": portfolio_charts(frames, settings.as_of, root)}
    md = ["# EDA report / تقرير تحليل البيانات الاستكشافي", "",
          f"As of / بتاريخ: **{settings.as_of}**", ""]
    for problem in problems or list(BUILDERS):
        ds = BUILDERS[problem](frames, settings.as_of)
        out = root / problem
        out.mkdir(exist_ok=True)
        summary = summarize(ds, problem)
        summary["findings"] = findings(summary, problem)
        summary["charts"] = charts(ds, problem, out)
        (out / "summary.json").write_text(json.dumps(summary, indent=2, ensure_ascii=False), encoding="utf-8")
        result[problem] = summary
        name = PROBLEMS[problem]["name"]
        md += [f"## {name['en']} / {name['ar']}", ""]
        md += [f"- {f['en']}" for f in summary["findings"]]
        md += ["", '<div dir="rtl">', ""] + [f"- {f['ar']}" for f in summary["findings"]] + ["", "</div>", ""]
        md += [f"![{c}]({problem}/{c})" for c in summary["charts"]] + [""]
    md += ["## Portfolio / المحفظة", ""] + [f"![{c}]({c})" for c in result["portfolio"]]
    (root / "EDA.md").write_text("\n".join(md), encoding="utf-8")
    return result
