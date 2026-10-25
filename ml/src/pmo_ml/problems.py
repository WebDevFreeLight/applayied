"""Step 1 - Problem definition: objective, task type, label and success metric per model.

This is the single source of truth used by training, the API (/models) and the docs.
Every text is bilingual (en / ar).
"""

from __future__ import annotations

PROBLEMS: dict[str, dict] = {
    "task_delay": {
        "type": "classification",
        "name": {"en": "Task delay risk", "ar": "خطر تأخر المهمة"},
        "objective": {
            "en": "Predict whether a task will miss its due date, so managers act before it slips.",
            "ar": "التنبؤ بما إذا كانت المهمة ستتجاوز تاريخ استحقاقها، ليتدخل المدير قبل التأخر.",
        },
        "unit": {"en": "one task", "ar": "مهمة واحدة"},
        "label": {
            "en": "late = not Done and due date passed, or Done after the due date",
            "ar": "متأخرة = غير منجزة وتجاوزت تاريخ الاستحقاق، أو أُنجزت بعده",
        },
        "metric": "f1",
        "success": {
            "en": "F1 above the majority-class baseline and ROC-AUC >= 0.65 on the hold-out set",
            "ar": "قيمة F1 أعلى من خط الأساس (الفئة الأكثر تكراراً) و ROC-AUC لا تقل عن 0.65 على بيانات الاختبار",
        },
        "use": {"en": "Delay-probability badge on open tasks", "ar": "شارة احتمال التأخر على المهام المفتوحة"},
    },
    "risk_severity": {
        "type": "classification",
        "name": {"en": "Risk severity suggestion", "ar": "اقتراح شدة الخطر"},
        "objective": {
            "en": "Suggest Low / Medium / High severity from a risk's text and project context while it is being logged.",
            "ar": "اقتراح شدة منخفضة / متوسطة / مرتفعة من نص الخطر وسياق المشروع أثناء تسجيله.",
        },
        "unit": {"en": "one risk or issue", "ar": "خطر أو مشكلة واحدة"},
        "label": {
            "en": "severity band of probability x impact: Low 1-6, Medium 7-11, High 12-25",
            "ar": "فئة الشدة من الاحتمال × الأثر: منخفض 1-6، متوسط 7-11، مرتفع 12-25",
        },
        "metric": "f1_macro",
        "success": {
            "en": "Macro-F1 above the majority-class baseline (cross-validated)",
            "ar": "قيمة Macro-F1 أعلى من خط الأساس (بالتحقق المتقاطع)",
        },
        "use": {"en": "'Suggest severity' on the risk form", "ar": "زر «اقترح الشدة» في نموذج الخطر"},
    },
    "spend_forecast": {
        "type": "regression",
        "name": {"en": "Monthly spend forecast", "ar": "توقع الإنفاق الشهري"},
        "objective": {
            "en": "Forecast next month's posted spend per project to spot overruns early.",
            "ar": "توقع الإنفاق المرحّل للشهر القادم لكل مشروع لاكتشاف التجاوزات مبكراً.",
        },
        "unit": {"en": "one project-month", "ar": "شهر واحد لمشروع"},
        "label": {"en": "sum of posted cost transactions in the month", "ar": "مجموع تكاليف الشهر المرحّلة"},
        "metric": "neg_mean_absolute_error",
        "success": {
            "en": "MAE lower than the naive 'same as last month' forecast on the latest months",
            "ar": "متوسط الخطأ المطلق أقل من التوقع البسيط «مثل الشهر الماضي» على أحدث الأشهر",
        },
        "use": {"en": "Forecast vs plan chart per project", "ar": "مخطط التوقع مقابل الخطة لكل مشروع"},
    },
    "cost_anomaly": {
        "type": "anomaly",
        "name": {"en": "Cost anomaly detection", "ar": "كشف التكاليف غير الاعتيادية"},
        "objective": {
            "en": "Flag unusual expenses (size, timing, posting lag) for a finance review.",
            "ar": "تمييز المصروفات غير الاعتيادية (الحجم، التوقيت، تأخر الترحيل) لمراجعتها مالياً.",
        },
        "unit": {"en": "one posted cost transaction", "ar": "معاملة تكلفة مرحّلة واحدة"},
        "label": {"en": "unsupervised - no label", "ar": "غير موجّه - بدون تسمية"},
        "metric": "injected_recall",
        "success": {
            "en": "Recovers most deliberately injected outliers (recall@flagged >= 0.6)",
            "ar": "يكتشف معظم القيم الشاذة المُدخلة عمداً (الاستدعاء >= 0.6)",
        },
        "use": {"en": "'Review these' list in Finance", "ar": "قائمة «راجِع هذه» في المالية"},
    },
    "portfolio_segments": {
        "type": "clustering",
        "name": {"en": "Portfolio segmentation", "ar": "تقسيم المحفظة"},
        "objective": {
            "en": "Group similar projects (spend, delay, risk, progress) to manage them as cohorts.",
            "ar": "تجميع المشاريع المتشابهة (الإنفاق، التأخر، المخاطر، التقدم) لإدارتها كمجموعات.",
        },
        "unit": {"en": "one project", "ar": "مشروع واحد"},
        "label": {"en": "unsupervised - clusters", "ar": "غير موجّه - عناقيد"},
        "metric": "silhouette",
        "success": {"en": "Silhouette score >= 0.25", "ar": "معامل السيلويت لا يقل عن 0.25"},
        "use": {"en": "Segment tags on the portfolio", "ar": "وسوم الشرائح في المحفظة"},
    },
}


def localized(problem: str, lang: str = "en") -> dict:
    lang = "ar" if str(lang).lower().startswith("ar") else "en"
    p = PROBLEMS[problem]
    return {"key": problem, "type": p["type"], "metric": p["metric"],
            **{k: v[lang] for k, v in p.items() if isinstance(v, dict) and lang in v}}
