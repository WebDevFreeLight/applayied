# 04 · Exploratory data analysis (EDA)

**English** · [العربية](#العربية) · Code: [`eda.py`](../src/pmo_ml/eda.py)

```bash
ml\.venv\Scripts\python -m pmo_ml eda
```

writes `reports/eda/EDA.md` plus, for every model, `reports/eda/<model>/`:

| Output | What to look for |
|---|---|
| `summary.json` | rows, missing values, `describe()`, skew, IQR outlier counts, correlation with the label |
| `histograms.png` | shape of each numeric feature (skew → scaling / tree models) |
| `correlation_heatmap.png` | redundant features (|r| ≈ 1) and the strongest linear signals |
| `boxplots_by_label.png` | features that separate late from on-time tasks, or the severity bands |
| `class_balance.png` | imbalance → class weights and F1 instead of accuracy |
| `counts_<column>.png` | rare categories |
| `../risk_matrix.png`, `../spend_trend.png` | portfolio views: probability × impact grid, monthly posted spend |

The same charts are served by the API (`/reports/...`) and appear on the portal's **Models & monitoring** tab.

## Findings on the live database (2026-10-25)

| Model | Rows | Finding → decision |
|---|---|---|
| task delay | 59 resolved tasks | 63 % late → class weights, F1/ROC-AUC. Strongest signal `project_task_count` (r = +0.41) |
| risk severity | 24 risks | 62 % High, only 4 Medium → macro-F1, treat as suggestion |
| spend forecast | 64 project-months | `lag2` correlates best (r = +0.57); lags are collinear → regularised models |
| cost anomaly | 94 posted costs | amounts heavily skewed → log amount and ratios to medians |
| segments | 10 projects | tiny sample → k between 2 and 4 only |

The findings are generated automatically (`eda.findings`), in English and Arabic, every time the pipeline runs.

## Reading the charts — three habits

1. **Check the label first** (class balance / target summary): it decides the metric.
2. **Look for leakage**: a feature that separates the classes perfectly in a boxplot is suspicious.
3. **Note skew and outliers**: they decide between scaling, log transforms and tree models.

---

<div dir="rtl">

## العربية

## التحليل الاستكشافي للبيانات

الأمر `python -m pmo_ml eda` يكتب التقرير `reports/eda/EDA.md` ومجلداً لكل نموذج يحتوي: ملخصاً إحصائياً
(`summary.json`)، ومدرجات تكرارية، وخريطة حرارية للارتباط، ومخططات صندوقية حسب التسمية، وتوازن الفئات، وعدد
القيم الفئوية، إضافةً إلى مصفوفة المخاطر (الاحتمال × الأثر) واتجاه الإنفاق الشهري. وتظهر المخططات نفسها في تبويب
**النماذج والمراقبة** في البوابة.

## نتائج على قاعدة البيانات الفعلية (2026-10-25)

| النموذج | الصفوف | النتيجة ← القرار |
|---|---|---|
| تأخر المهام | 59 مهمة محسومة | 63٪ متأخرة ← أوزان للفئات ومقياس F1 و ROC-AUC |
| شدة الخطر | 24 خطراً | 62٪ مرتفع و 4 فقط متوسط ← Macro-F1 ويُعرض كاقتراح |
| توقع الإنفاق | 64 شهراً-مشروعاً | إنفاق قبل شهرين هو الأقوى ارتباطاً؛ والفترات متداخلة ← نماذج منتظمة (Ridge) |
| التكاليف غير الاعتيادية | 94 تكلفة | المبالغ ملتوية بشدة ← لوغاريتم المبلغ ونسب إلى الوسيط |
| الشرائح | 10 مشاريع | عينة صغيرة ← عدد العناقيد بين 2 و 4 فقط |

تُولَّد هذه النتائج تلقائياً بالعربية والإنجليزية في كل تشغيل.

## ثلاث عادات عند قراءة المخططات

1. **ابدأ بالتسمية** (توازن الفئات): فهي تحدد المقياس.
2. **ابحث عن التسرّب**: خاصية تفصل الفئات تماماً أمر مريب.
3. **لاحظ الالتواء والقيم الشاذة**: فهي تحدد الاختيار بين التحجيم واللوغاريتم ونماذج الأشجار.

</div>
