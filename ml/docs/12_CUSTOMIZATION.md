# 12 · Customization & advanced features

**English** · [العربية](#العربية)

## Change behaviour without code (`config/config.yaml`)

| Want to… | Setting |
|---|---|
| train on the CSV seed | `data.source: csv` or `PMO_ML_SOURCE=csv` |
| freeze "today" for a demo | `data.as_of_date: 2026-09-30` or `PMO_ML_AS_OF` |
| more / fewer CV folds | `training.cv_folds` |
| select models by another metric | `training.scoring.task_delay: roc_auc` (any scikit-learn scorer) |
| train only some models | `training.problems: [task_delay, spend_forecast]` |
| flag more expenses | `anomaly.contamination: 0.1` |
| try more segments | `segments.k_max: 6` |
| be stricter about drift | `monitoring.drift_psi_threshold: 0.15` |
| retrain every 6 hours | `api.auto_retrain_hours: 6` |

## Tutorial — add an algorithm

In `models.py › classification_candidates`, add one entry:

```python
from sklearn.ensemble import ExtraTreesClassifier
...
"extra_trees": (ExtraTreesClassifier(n_estimators=300, class_weight="balanced", random_state=rs),
                {"model__max_depth": [3, None]}),
```

Run `python -m pmo_ml train --problem task_delay`. It is cross-validated next to the others, appears in
`comparison.png`, and ships only if it wins.

## Tutorial — add a feature

In `features.py › task_delay_dataset`:

```python
feats["due_in_quarter_end"] = t["due_date"].dt.is_quarter_end.astype(int)
numeric = [..., "due_in_quarter_end"]
```

Add a translation in `i18n.py › FEATURES` so the portal can name it as a driver. Retrain; check its permutation
importance in `feature_importance.png`.

## Tutorial — add a new model (e.g. milestone slip)

1. **Define** it in `problems.py › PROBLEMS` (type, bilingual name, objective, label, metric, success).
2. **Build** a `milestone_slip_dataset(frames, as_of)` in `features.py` returning a `Dataset`, and register it in
   `BUILDERS`. If you need a new table, add it to `collect.TABLES`.
3. **Train**: nothing to do — `train_all` picks the trainer from the problem's `type`.
4. **Predict**: add a function in `predict.py` and an endpoint in `api.py`.
5. **Expose**: add an action in `PmoMlController.cs`, a call in `frontend/src/api/ml.ts`, a tab in `MlInsights.tsx`
   and the `ml.*` strings in `en.ts` / `ar.ts`.
6. **Test**: `test_every_builder_runs_on_csv` covers the builder automatically; add a leakage check if needed.
7. **Log it** in `00_MODIFICATIONS.md`.

## Advanced ideas

* Calibrate probabilities (`CalibratedClassifierCV`) before showing them as percentages.
* Explain single predictions with SHAP values next to the global permutation importance.
* Forecast with prediction intervals (`GradientBoostingRegressor(loss="quantile")`).
* Store models in a remote registry (MLflow) instead of the `models/` folder.
* Use the delay probability inside the .NET automation runner to escalate tasks before they are overdue.

---

<div dir="rtl">

## العربية

## التخصيص والميزات المتقدمة

**دون كتابة شيفرة:** كل السلوك قابل للتعديل من `config/config.yaml`: مصدر البيانات، تثبيت التاريخ المرجعي، عدد طيّات
التحقق، مقياس الاختيار، النماذج المراد تدريبها، نسبة التكاليف غير الاعتيادية، عدد الشرائح، عتبة الانحراف، وتكرار
إعادة التدريب التلقائي.

**درس — إضافة خوارزمية:** أضف سطراً واحداً في `classification_candidates` (مثل ExtraTrees) ثم أعد التدريب؛ ستُقارن مع
الباقي بالتحقق المتقاطع ولن تُعتمد إلا إذا فازت.

**درس — إضافة خاصية:** أضف العمود في مُنشئ الخصائص وفي قائمة `numeric`، وأضف ترجمتها في `i18n.py`، ثم أعد التدريب
وراجع أهميتها.

**درس — إضافة نموذج جديد (مثل تأخر المعالم):**

1. عرّفه في `problems.py`.
2. أنشئ مُنشئ البيانات في `features.py` وسجّله في `BUILDERS`.
3. التدريب تلقائي حسب نوع المشكلة.
4. أضف دالة التنبؤ في `predict.py` ونقطة النهاية في `api.py`.
5. أضف الإجراء في `PmoMlController.cs` والاستدعاء في `ml.ts` والتبويب في `MlInsights.tsx` والنصوص بالعربية والإنجليزية.
6. الاختبارات تغطي المُنشئ تلقائياً.
7. سجّل التعديل في `00_MODIFICATIONS.md`.

**أفكار متقدمة:** معايرة الاحتمالات، وتفسير التنبؤ الفردي بقيم SHAP، وفترات تنبؤ للإنفاق، وسجل نماذج بعيد (MLflow)،
واستخدام احتمال التأخر في أتمتة ‎.NET للتصعيد قبل حدوث التأخر.

</div>
