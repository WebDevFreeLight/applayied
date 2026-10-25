"""Bilingual labels for API responses (en / ar). The portal passes ?lang=ar for Arabic."""

from __future__ import annotations

LABELS = {
    "Low": {"en": "Low", "ar": "منخفض"},
    "Medium": {"en": "Medium", "ar": "متوسط"},
    "High": {"en": "High", "ar": "مرتفع"},
    "late": {"en": "Likely late", "ar": "مرجح التأخر"},
    "on_time": {"en": "Likely on time", "ar": "مرجح الالتزام بالموعد"},
    "anomaly": {"en": "Unusual - review", "ar": "غير اعتيادي - للمراجعة"},
    "normal": {"en": "Normal", "ar": "طبيعي"},
    "seg_at_risk": {"en": "At risk", "ar": "معرّض للخطر"},
    "seg_on_track": {"en": "On track", "ar": "ضمن المسار"},
    "seg_early": {"en": "Early stage", "ar": "مرحلة مبكرة"},
    "seg_heavy_spend": {"en": "Heavy spend", "ar": "إنفاق مرتفع"},
    "no_model": {"en": "Model not trained yet - run training first.",
                 "ar": "النموذج غير مدرّب بعد - شغّل التدريب أولاً."},
    "drift": {"en": "Data drift detected", "ar": "تم رصد انحراف في البيانات"},
    "stable": {"en": "Data stable", "ar": "البيانات مستقرة"},
    "retrain_yes": {"en": "Retraining recommended", "ar": "يُنصح بإعادة التدريب"},
    "retrain_no": {"en": "No retraining needed", "ar": "لا حاجة لإعادة التدريب"},
}

FEATURES = {
    "weight": {"en": "Task weight", "ar": "وزن المهمة"},
    "planned_duration_days": {"en": "Planned duration (days)", "ar": "المدة المخططة (أيام)"},
    "start_offset_days": {"en": "Start offset in project (days)", "ar": "بداية المهمة من بداية المشروع (أيام)"},
    "unassigned": {"en": "Unassigned", "ar": "غير مسندة"},
    "assignee_task_count": {"en": "Assignee workload", "ar": "عبء المسند إليه"},
    "project_task_count": {"en": "Project task count", "ar": "عدد مهام المشروع"},
    "budget_log": {"en": "Project budget (log)", "ar": "ميزانية المشروع (لوغاريتمي)"},
    "project_duration_days": {"en": "Project duration (days)", "ar": "مدة المشروع (أيام)"},
    "member_count": {"en": "Team size", "ar": "حجم الفريق"},
    "open_risks": {"en": "Open risks", "ar": "المخاطر المفتوحة"},
    "open_high_risks": {"en": "Open high risks", "ar": "المخاطر المرتفعة المفتوحة"},
    "priority": {"en": "Task priority", "ar": "أولوية المهمة"},
    "project_priority": {"en": "Project priority", "ar": "أولوية المشروع"},
    "project_status": {"en": "Project status", "ar": "حالة المشروع"},
    "text": {"en": "Risk text", "ar": "نص الخطر"},
    "kind": {"en": "Risk or issue", "ar": "خطر أم مشكلة"},
    "lag1": {"en": "Spend last month", "ar": "إنفاق الشهر الماضي"},
    "lag2": {"en": "Spend 2 months ago", "ar": "إنفاق قبل شهرين"},
    "lag3": {"en": "Spend 3 months ago", "ar": "إنفاق قبل 3 أشهر"},
    "roll3": {"en": "3-month average", "ar": "متوسط 3 أشهر"},
    "months_active": {"en": "Months active", "ar": "أشهر النشاط"},
    "budget_used_prev": {"en": "Budget used so far", "ar": "نسبة الميزانية المستخدمة"},
    "month_of_year": {"en": "Month of year", "ar": "شهر السنة"},
    "planned_estimate": {"en": "Planned monthly estimate", "ar": "التقدير الشهري المخطط"},
    "log_amount": {"en": "Amount (log)", "ar": "المبلغ (لوغاريتمي)"},
    "amount_vs_category": {"en": "Amount vs category median", "ar": "المبلغ مقابل وسيط الفئة"},
    "amount_vs_project": {"en": "Amount vs project median", "ar": "المبلغ مقابل وسيط المشروع"},
    "share_of_budget": {"en": "Share of budget", "ar": "حصة من الميزانية"},
    "posting_lag_days": {"en": "Posting lag (days)", "ar": "تأخر الترحيل (أيام)"},
    "day_of_week": {"en": "Day of week", "ar": "يوم الأسبوع"},
    "is_month_end": {"en": "Month end", "ar": "نهاية الشهر"},
    "category": {"en": "Cost category", "ar": "فئة التكلفة"},
    "utilization": {"en": "Budget utilization", "ar": "استخدام الميزانية"},
    "late_task_share": {"en": "Late task share", "ar": "نسبة المهام المتأخرة"},
    "avg_progress": {"en": "Average progress", "ar": "متوسط التقدم"},
    "open_risk_exposure": {"en": "Open risk exposure", "ar": "التعرض للمخاطر المفتوحة"},
    "task_count": {"en": "Task count", "ar": "عدد المهام"},
}


def lang_of(lang: str | None) -> str:
    return "ar" if str(lang or "").lower().startswith("ar") else "en"


def label(key: str, lang: str | None = "en") -> str:
    entry = LABELS.get(key)
    return entry[lang_of(lang)] if entry else key


def feature(key: str, lang: str | None = "en") -> str:
    base = key.split("__")[-1]
    for name, entry in FEATURES.items():
        if base == name or base.startswith(name + "_"):
            return entry[lang_of(lang)] + ("" if base == name else f" ({base[len(name) + 1:]})")
    return key
