# 05 · Feature engineering & selection

**English** · [العربية](#العربية) · Code: [`features.py`](../src/pmo_ml/features.py)

Each model has one builder in `BUILDERS`. A builder returns a `Dataset`: feature table `X`, label `y`, row ids, and
the lists of numeric / categorical / text columns that `models.py` uses to build the preprocessing.

## The no-leakage rule

> A feature must be known **before** the outcome it predicts.

A task's `status`, `progress_pct` or "days overdue" are *consequences* of being late. A model trained with them
scores ~100 % in testing and is useless in production. They are therefore excluded, and
`tests/test_features.py::test_no_leaky_features` fails the build if anyone adds them back. Likewise
`probability`, `impact` and `score` are excluded from risk severity because they define its label.

## Shared project context (`project_context`)

`budget_log` (log of the initial budget), project priority/status, planned duration, team size, open risks, open
high risks (score ≥ 12), approved budget (initial + approved revisions), posted spend to date and utilization.

## Features per model

| Model | Engineered features |
|---|---|
| task delay | priority, weight, **planned duration** (due − start), **start offset** in the project, unassigned flag, **assignee workload** (tasks per assignee), project task count + project context |
| risk severity | **TF-IDF of title + mitigation** (1–2 grams), kind (Risk/Issue), project priority/status, budget |
| spend forecast | **lags** of monthly spend (1, 2, 3 months), **3-month rolling mean**, months active, share of budget already used, month of year, **planned monthly estimate** |
| cost anomaly | log amount, **amount ÷ category median**, **amount ÷ project median**, share of budget, **posting lag** (posted − expense date), weekday, month-end flag, category |
| segments | budget, utilization, **late-task share**, average progress, open-risk exposure, task count |

Bold = engineered (not a raw column).

## Time awareness (spend forecast)

`monthly_spend` builds a complete project × month grid up to the as-of date (missing months = 0). Lags only look
**backwards** (`shift(1)` before every rolling window), the current, incomplete month is never used for
training, and the next month's features are computed from history alone.

## Selection

Features are kept small and meaningful on purpose (tens of rows cannot support hundreds of features).
Relevance is then measured, not assumed: **permutation importance** on held-out data (step 8) shows which features
the chosen model actually relies on; features with no importance across retrains are candidates for removal.

---

<div dir="rtl">

## العربية

## هندسة الخصائص واختيارها

لكل نموذج مُنشئ واحد في `BUILDERS` يعيد جدول الخصائص والتسمية ومعرّفات الصفوف وقوائم الأعمدة الرقمية والفئوية والنصية.

## قاعدة منع التسرّب

> يجب أن تكون الخاصية معروفة **قبل** النتيجة التي تتنبأ بها.

حالة المهمة ونسبة تقدمها وعدد أيام التأخر هي *نتائج* للتأخر، فالنموذج المدرّب عليها يحصل على ~100٪ في الاختبار ولا
فائدة منه فعلياً. لذلك استُبعدت، ويفشل الاختبار `test_no_leaky_features` إذا أعادها أحد. وكذلك يُستبعد الاحتمال
والأثر والدرجة من نموذج شدة الخطر لأنها تحدد التسمية.

## الخصائص لكل نموذج

| النموذج | الخصائص المصمَّمة |
|---|---|
| تأخر المهام | الأولوية، الوزن، **المدة المخططة**، **موعد البدء داخل المشروع**، غير مسندة، **عبء المسند إليه**، عدد مهام المشروع وسياق المشروع |
| شدة الخطر | **TF-IDF لعنوان الخطر وخطة التخفيف**، النوع، أولوية وحالة المشروع، الميزانية |
| توقع الإنفاق | **إنفاق الأشهر السابقة** (1، 2، 3)، **المتوسط المتحرك لثلاثة أشهر**، أشهر النشاط، نسبة الميزانية المستخدمة، شهر السنة، **التقدير الشهري المخطط** |
| التكاليف غير الاعتيادية | لوغاريتم المبلغ، **المبلغ ÷ وسيط الفئة**، **المبلغ ÷ وسيط المشروع**، حصة الميزانية، **تأخر الترحيل**، يوم الأسبوع، نهاية الشهر، الفئة |
| الشرائح | الميزانية، الاستخدام، **نسبة المهام المتأخرة**، متوسط التقدم، التعرض للمخاطر، عدد المهام |

## الوعي الزمني

في توقع الإنفاق تنظر الخصائص **إلى الماضي فقط**، ولا يُستخدم الشهر الحالي غير المكتمل في التدريب.

## الاختيار

الخصائص قليلة ومعبّرة عن قصد لأن عشرات الصفوف لا تحتمل مئات الخصائص، ثم تُقاس أهميتها فعلياً بطريقة
**أهمية التبديل** في الخطوة 8.

</div>
