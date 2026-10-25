# 07 · Model training

**English** · [العربية](#العربية) · Code: [`train.py`](../src/pmo_ml/train.py), [`pipeline.py`](../src/pmo_ml/pipeline.py)

```bash
ml\.venv\Scripts\python -m pmo_ml train                      # all five models
ml\.venv\Scripts\python -m pmo_ml train --problem task_delay  # one model
```

## Supervised models, step by step

1. **Split**
   * classification: stratified hold-out (`test_size` 0.25) keeps the late/on-time ratio in both parts;
   * spend forecast: **time-based** — the latest 25 % of months are the test set, so the model is always tested on
     the future;
   * fewer than 40 rows (risk severity): no hold-out; evaluation uses cross-validated predictions on all rows
     (more reliable than a 6-row test set).
2. **Cross-validate & tune** — `GridSearchCV` for every candidate on the training part only:
   `StratifiedKFold` (folds reduced automatically when a class has few rows) or `TimeSeriesSplit` for spend.
3. **Select** — the best non-baseline candidate by mean CV score (`training.scoring` in the config).
4. **Evaluate** — score the winner and the baseline on the hold-out (step 8).
5. **Refit** — `clone()` the winning estimator and fit it on **all** rows; that model is saved.
6. **Register** — `models/<problem>/<version>/model.joblib` + `metadata.json`, and `current.json` now points to it.

A failing candidate (for example k-NN with more neighbours than rows) is recorded with its error and skipped; it
never stops the run.

## Unsupervised models

* **cost anomaly** — each candidate is fitted on the real transactions, then scored on deliberately injected
  outliers (copies of real rows, 8× larger, posted 90 days late), 5 repeats. Best recall wins (precision breaks ties).
* **segments** — KMeans for each k; the best silhouette among the k whose segments all hold ≥ 2 projects.

## What is saved with every model

Version, timestamp, as-of date, data source + snapshot path + fingerprint, rows, label distribution, every
candidate's CV score and parameters, the winner, hold-out and baseline metrics, `beats_baseline`, permutation
importance, chart file names, the drift reference profile, and Python / scikit-learn versions.

## Reproducibility

`training.random_state` (42) seeds every split and estimator; the data fingerprint tells you whether two runs saw
the same data.

---

<div dir="rtl">

## العربية

## تدريب النماذج

الأمر `python -m pmo_ml train` يدرّب النماذج الخمسة، ويمكن تحديد نموذج واحد بالخيار `--problem`.

**النماذج الموجّهة خطوة بخطوة:**

1. **التقسيم:** للتصنيف تقسيم طبقي يحافظ على نسبة المتأخر/غير المتأخر؛ ولتوقع الإنفاق تقسيم **زمني** (أحدث 25٪ من
   الأشهر للاختبار) فيُختبر النموذج دائماً على المستقبل؛ وعند أقل من 40 صفاً (شدة الخطر) يُستخدم التحقق المتقاطع
   على كل الصفوف بدل بيانات اختبار صغيرة جداً.
2. **التحقق المتقاطع والضبط:** ‏`GridSearchCV` لكل خوارزمية على بيانات التدريب فقط.
3. **الاختيار:** أفضل خوارزمية (غير خط الأساس) حسب متوسط نتيجة التحقق.
4. **التقييم:** على البيانات المحجوزة مقارنةً بخط الأساس.
5. **إعادة التدريب:** على **كل** الصفوف ثم الحفظ.
6. **التسجيل:** في `models/<المشكلة>/<الإصدار>/` مع تحديث `current.json`.

الخوارزمية التي تفشل تُسجَّل مع الخطأ وتُتجاوز دون إيقاف التدريب.

**النماذج غير الموجّهة:** للشذوذ نزرع قيماً شاذة واضحة ونقيس نسبة اكتشافها؛ وللشرائح نختار عدد العناقيد بأفضل معامل
سيلويت بشرط ألا تقل أي شريحة عن مشروعين.

**ما يُحفظ مع كل نموذج:** الإصدار، والتاريخ، ومصدر البيانات ولقطتها وبصمتها، ونتائج كل الخوارزميات، والفائز،
والمقاييس مقارنةً بخط الأساس، وأهمية الخصائص، وملف مرجعي لاكتشاف الانحراف، وإصدارات المكتبات.

**قابلية التكرار:** ‏`random_state = 42` يثبّت كل تقسيم وخوارزمية.

</div>
