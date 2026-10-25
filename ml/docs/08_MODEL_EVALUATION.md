# 08 · Model evaluation

**English** · [العربية](#العربية) · Code: [`evaluate.py`](../src/pmo_ml/evaluate.py)

```bash
ml\.venv\Scripts\python -m pmo_ml evaluate
```

## Metrics used

| Task | Metrics |
|---|---|
| classification | accuracy, precision, recall, **F1** (binary) / **macro-F1** (multiclass), ROC-AUC, confusion matrix |
| regression | **MAE**, RMSE, MAPE (non-zero actuals), R² |
| anomaly | recall and precision on injected outliers, rows flagged |
| clustering | silhouette, segment sizes |

Charts per model in `reports/models/<model>/<version>/`: `comparison.png` (all candidates, baseline in grey),
`confusion_matrix.png`, `roc_curve.png`, `predicted_vs_actual.png`, `feature_importance.png`, `segments_pca.png`.

## Results on the live database (trained 2026-10-25)

| Model | Winner | Evaluation | Score | Baseline | Verdict |
|---|---|---|---|---|---|
| task delay | random forest | 15-row stratified hold-out | F1 **0.82**, ROC-AUC **0.91**, precision 0.88, recall 0.78 | always "late": F1 0.75, AUC 0.50 | useful — ranks risky tasks well |
| risk severity | MLP neural net | 4-fold CV predictions, 24 rows | macro-F1 **0.42** | majority class: 0.26 | weak — suggestion only |
| spend forecast | ridge | latest 21 project-months (from 2026-07) | MAE **93.8k** | last month: 146.6k | useful — 36 % lower error |
| cost anomaly | local outlier factor (20) | injected outliers × 5 | recall **1.00**, precision 0.69 | — | meets criterion |
| segments | KMeans k = 2 | silhouette | **0.35** | ≥ 0.25 | one project stands apart as *At risk* |

Read these honestly:

* For task delay, the cross-validated F1 gap to the baseline is small (0.80 vs 0.78) — on a 63 %-late dataset,
  "always late" is a strong F1 baseline. The real value is the **ranking** (AUC 0.91): the portal sorts open tasks
  by probability, so the riskiest ones come first.
* Spend R² is negative on the hold-out (monthly spend is lumpy), but the absolute error is what managers use, and
  it beats the naive forecast clearly. The API shows the forecast with a ± MAE range.
* Numbers change with each retrain; the live values are always on the portal's **Models & monitoring** tab.

## Explainability

`evaluate.importance` runs **permutation importance** on held-out data: shuffle one feature and measure how much the
score drops. It works for every algorithm. Today's top drivers:

* task delay: project task count, assignee workload, start offset in the project;
* spend forecast: spend 2 and 3 months ago, share of budget already used, planned monthly estimate.

The API returns the top three as `drivers`, translated, and the portal shows them above each table.

---

<div dir="rtl">

## العربية

## تقييم النماذج

**المقاييس:** للتصنيف: الدقة، والإحكام، والاستدعاء، و **F1** (أو **Macro-F1** للفئات المتعددة)، و ROC-AUC، ومصفوفة
الالتباس. للانحدار: **MAE** و RMSE و MAPE و R². للشذوذ: الاستدعاء والإحكام على قيم مزروعة. للتجميع: معامل السيلويت.

## النتائج على قاعدة البيانات الفعلية (2026-10-25)

| النموذج | الفائز | النتيجة | خط الأساس | الحكم |
|---|---|---|---|---|
| تأخر المهام | الغابة العشوائية | F1 ‏**0.82** و ROC-AUC ‏**0.91** | F1 ‏0.75 و AUC ‏0.50 | مفيد — يرتّب المهام الخطرة جيداً |
| شدة الخطر | شبكة عصبية MLP | Macro-F1 ‏**0.42** | 0.26 | ضعيف — اقتراح فقط |
| توقع الإنفاق | Ridge | MAE ‏**93.8 ألف** | 146.6 ألف | مفيد — خطأ أقل بـ 36٪ |
| التكاليف غير الاعتيادية | LOF | استدعاء **1.00** وإحكام 0.69 | — | يحقق المعيار |
| الشرائح | KMeans ‏(k=2) | سيلويت **0.35** | ≥ 0.25 | مشروع واحد يبرز «معرّضاً للخطر» |

**قراءة صادقة:** في تأخر المهام الفرق عن خط الأساس في التحقق المتقاطع صغير لأن 63٪ من المهام متأخرة، لكن القيمة
الحقيقية في **الترتيب** (AUC ‏0.91)، فالبوابة ترتّب المهام حسب الاحتمال. وفي الإنفاق قيمة R² سالبة لأن الإنفاق الشهري
متقطع، لكن الخطأ المطلق أقل بوضوح من التوقع البسيط. الأرقام تتغير مع كل إعادة تدريب وتظهر محدّثة في البوابة.

**قابلية التفسير:** ‏**أهمية التبديل** تخلط خاصية واحدة وتقيس انخفاض النتيجة، وتعمل مع أي خوارزمية. أهم العوامل اليوم:
عدد مهام المشروع وعبء المسند إليه لتأخر المهام، وإنفاق الأشهر السابقة ونسبة الميزانية المستخدمة لتوقع الإنفاق.

</div>
