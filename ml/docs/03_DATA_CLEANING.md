# 03 · Data cleaning & preprocessing

**English** · [العربية](#العربية) · Code: [`clean.py`](../src/pmo_ml/clean.py), [`models.py › preprocessor`](../src/pmo_ml/models.py)

Cleaning happens at two levels, on purpose.

## Level 1 — table cleaning (`clean.py`)

Applied once, right after collection:

| Problem | Fix |
|---|---|
| Duplicate or missing ids | drop rows without an id; keep the last row per id |
| Inconsistent labels (`in progress`, `IN_PROGRESS`) | map to canonical values (`InProgress`) via `CANONICAL` |
| Wrong types | numeric columns coerced with `pd.to_numeric`; dates parsed in `collect.py` |
| Stray whitespace | stripped from every text column |
| Business rule: actual spend | only **Posted** costs with amount > 0 count (drafts excluded) → `costs_posted` |
| Tasks without due date | reported; they cannot be labelled and are excluded from task delay |

`clean()` returns a report (rows in/out, duplicates dropped, missing values per column) that is stored with every
training run.

## Level 2 — model preprocessing (inside the Pipeline)

Imputation, encoding and scaling live **inside each scikit-learn Pipeline**, never as a separate step:

| Column kind | Transformation |
|---|---|
| numeric | `SimpleImputer(median)` → `StandardScaler` |
| categorical | `SimpleImputer(most_frequent)` → `OneHotEncoder(handle_unknown="ignore")` |
| text (risk) | `TfidfVectorizer(ngram_range=(1, 2), sublinear_tf=True)` |

Why inside the Pipeline: the imputer's median and the scaler's mean are learned **only from the training folds**
during cross-validation (no information from the test fold leaks in), and the exact same transformation is saved
with the model and reused by the API. `handle_unknown="ignore"` means a new category in production (a new
priority value, say) does not crash prediction.

## Try it

```bash
ml\.venv\Scripts\python -c "from pmo_ml.config import load_settings; from pmo_ml.collect import collect; from pmo_ml.clean import clean; import json; print(json.dumps(clean(collect(load_settings()))[1]['costs'], indent=1))"
```

---

<div dir="rtl">

## العربية

## تنظيف البيانات ومعالجتها

يتم التنظيف على مستويين عن قصد.

**المستوى الأول — تنظيف الجداول (`clean.py`):** حذف الصفوف بلا معرّف والتكرارات، توحيد القيم غير المتسقة
(مثل `in progress` إلى `InProgress`)، تحويل الأنواع الرقمية والتواريخ، إزالة المسافات الزائدة، وتطبيق قاعدة العمل:
الإنفاق الفعلي = التكاليف **المرحّلة** فقط ذات المبلغ الموجب (تُستبعد المسودات). المهام بلا تاريخ استحقاق لا يمكن
تسميتها فتُستبعد من نموذج التأخر. يعيد `clean()` تقريراً يُحفظ مع كل عملية تدريب.

**المستوى الثاني — المعالجة داخل خط النموذج:** تعويض القيم المفقودة والترميز والتحجيم تتم **داخل Pipeline** لكل
نموذج: الأعمدة الرقمية بالوسيط ثم التحجيم المعياري، والفئوية بالقيمة الأكثر تكراراً ثم One-Hot، والنص بـ TF-IDF.

**لماذا داخل الخط؟** لأن قيم الوسيط والمتوسط تُتعلّم من بيانات التدريب فقط أثناء التحقق المتقاطع (فلا يتسرب شيء من
بيانات الاختبار)، وتُحفظ المعالجة نفسها مع النموذج وتستخدمها الواجهة البرمجية. ويضمن `handle_unknown="ignore"` ألا
يتعطل التنبؤ عند ظهور فئة جديدة.

</div>
