# 01 · Problem definition

**English** · [العربية](#العربية) · Code: [`problems.py`](../src/pmo_ml/problems.py)

Before any data is touched, each model gets a written contract: the business question, the unit of prediction,
the label, the task type and a success criterion that must be met **against a baseline**. These definitions are
code (`PROBLEMS` in `problems.py`), so the API, the portal and these docs always agree.

| Model | Business question | Unit | Label | Type | Selection metric | Success criterion |
|---|---|---|---|---|---|---|
| `task_delay` | Will this open task miss its due date? | task | `late` = not Done & due date passed, or Done after due date | Binary classification | F1 | F1 > majority baseline **and** ROC-AUC ≥ 0.65 (hold-out) |
| `risk_severity` | How severe is this new risk likely to be? | risk / issue | band of probability × impact: Low 1–6, Medium 7–11, High 12–25 | Multiclass classification | macro-F1 | macro-F1 > majority baseline (CV) |
| `spend_forecast` | How much will this project spend next month? | project-month | sum of **posted** costs in the month | Regression (time series) | MAE | MAE < naive "same as last month" on the latest months |
| `cost_anomaly` | Which expenses look unusual? | posted transaction | none (unsupervised) | Outlier detection | recall on injected outliers | ≥ 0.6 |
| `portfolio_segments` | Which projects behave alike? | project | none (unsupervised) | Clustering | silhouette | ≥ 0.25 |

## Why these five

They map one-to-one onto decisions the portal already supports: escalating tasks, rating risks, controlling
spend, reviewing expenses and steering the portfolio. Each covers a different family of ML (classification,
NLP classification, regression/forecasting, anomaly detection, clustering), which makes the project a complete
learning path.

## Choosing the metric

* **F1, not accuracy**, for task delay: 63 % of resolved tasks are late, so "always late" already scores 63 %
  accuracy. F1 balances precision (don't cry wolf) and recall (don't miss late tasks).
* **Macro-F1** for severity: every class counts equally, so the model cannot ignore the rare Medium class.
* **MAE** for spend: errors are in currency units, easy to explain ("typically ± 94k").
* **Injected-outlier recall** for anomalies: there is no ground truth, so we plant obvious outliers and measure
  how many the detector finds.
* **Silhouette** for clusters: how well separated the groups are (−1 … 1).

## Ethics and scope

Predictions support people; they never change data. Users appear only as ids, no personal data is used, and the
risk-severity model is presented as a suggestion with its confidence and a reminder to confirm.

---

<div dir="rtl">

## العربية

## تعريف المشكلة

قبل لمس أي بيانات، يحصل كل نموذج على «عقد» مكتوب: سؤال العمل، ووحدة التنبؤ، والتسمية، ونوع المهمة، ومعيار نجاح
يجب تحقيقه **مقارنةً بخط أساس**. هذه التعريفات مكتوبة كشيفرة (`PROBLEMS` في `problems.py`) فتتطابق دائماً بين
الواجهة البرمجية والبوابة وهذه الدروس.

| النموذج | سؤال العمل | النوع | مقياس الاختيار | معيار النجاح |
|---|---|---|---|---|
| تأخر المهمة | هل ستتجاوز المهمة المفتوحة تاريخ استحقاقها؟ | تصنيف ثنائي | F1 | أعلى من خط الأساس و ROC-AUC ≥ 0.65 |
| شدة الخطر | ما الشدة المتوقعة لهذا الخطر الجديد؟ | تصنيف متعدد | Macro-F1 | أعلى من خط الأساس |
| توقع الإنفاق | كم سينفق المشروع الشهر القادم؟ | انحدار زمني | MAE | أقل من توقع «مثل الشهر الماضي» |
| التكاليف غير الاعتيادية | أي المصروفات تبدو غير طبيعية؟ | كشف القيم الشاذة | الاستدعاء على قيم شاذة مزروعة | ≥ 0.6 |
| تقسيم المحفظة | أي المشاريع تتصرف بشكل متشابه؟ | تجميع | معامل السيلويت | ≥ 0.25 |

## لماذا هذه المقاييس

- **F1 بدل الدقة** لتأخر المهام: 63٪ من المهام المحسومة متأخرة، فالتخمين الدائم «متأخرة» يعطي دقة 63٪. مقياس F1
  يوازن بين عدم إطلاق إنذارات كاذبة وعدم تفويت المهام المتأخرة.
- **Macro-F1** للشدة: كل فئة لها الوزن نفسه، فلا يتجاهل النموذج فئة «متوسط» النادرة.
- **MAE** للإنفاق: الخطأ بوحدة العملة وسهل الشرح.
- **الاستدعاء على قيم مزروعة** للشذوذ: لا توجد حقيقة مرجعية، فنزرع قيماً شاذة واضحة ونقيس كم منها يُكتشف.
- **السيلويت** للعناقيد: مدى انفصال المجموعات (من ‎−1 إلى 1).

## الأخلاقيات والنطاق

التنبؤات تساعد الأشخاص ولا تغيّر البيانات أبداً. المستخدمون يظهرون كأرقام تعريفية فقط، ولا تُستخدم بيانات شخصية،
ويُعرض نموذج شدة الخطر كاقتراح مع درجة الثقة وتذكير بالتأكيد.

</div>
