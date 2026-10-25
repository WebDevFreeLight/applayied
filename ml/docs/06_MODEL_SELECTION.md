# 06 · Model selection

**English** · [العربية](#العربية) · Code: [`models.py`](../src/pmo_ml/models.py)

Every candidate is wrapped in the same `Pipeline(preprocess → estimator)`, so they are compared fairly on identical
features. Each list deliberately mixes algorithm families and always includes a **baseline**.

## Classification (task delay, risk severity)

| Candidate | Family | Why it is here | Trade-off |
|---|---|---|---|
| `baseline_majority` | Dummy | the bar to beat | no skill |
| `logistic_regression` | linear | interpretable coefficients, robust on small data | linear boundaries only |
| `decision_tree` | tree | readable rules | overfits easily |
| `random_forest` | bagged trees | strong default, handles mixed features | less interpretable |
| `gradient_boosting` | boosted trees | often most accurate on tabular data | needs tuning |
| `svm_rbf` | kernel | non-linear boundaries on scaled data | slow on big data, needs probability calibration |
| `knn` | neighbours | simple, local patterns | sensitive to scaling and noise |
| `neural_net_mlp` | neural network | non-linear, works well with TF-IDF text | data-hungry, less stable |

`class_weight="balanced"` is used where supported, because the classes are imbalanced.

## Regression (spend forecast)

`baseline_mean`, **`baseline_last_month`** (naive forecast: next month = last month — the business bar),
`linear_regression`, `ridge`, `random_forest`, `gradient_boosting`, `svr_rbf`, `knn`, `neural_net_mlp`.

Lesson from the live data: plain `linear_regression` produced a cross-validated MAE of ~2.1 million because the
lag features are strongly collinear; `ridge` (the same model with L2 regularisation) is stable and won.

## Unsupervised

| Model | Candidates |
|---|---|
| cost anomaly | `IsolationForest` (100 / 300 trees), `LocalOutlierFactor` (10 / 20 neighbours, `novelty=True`) |
| segments | `KMeans` for k = 2…4 (k ≤ rows − 1) |

## Constraints considered

* **Interpretability**: permutation importance is reported for every winner, whatever its family.
* **Performance**: chosen by cross-validated score, then confirmed on a hold-out.
* **Compute**: grids are small (2–4 values per parameter); the full pipeline trains all five models in under a
  minute on a laptop.

---

<div dir="rtl">

## العربية

## اختيار النموذج

كل خوارزمية مرشحة توضع في خط المعالجة نفسه (`Pipeline`) فتُقارن بعدل على الخصائص ذاتها، وتتضمن كل قائمة عائلات
مختلفة من الخوارزميات و**خط أساس** دائماً.

**التصنيف:** خط الأساس (الفئة الأكثر تكراراً)، الانحدار اللوجستي (قابل للتفسير)، شجرة القرار (قواعد مقروءة)، الغابة
العشوائية (خيار افتراضي قوي)، التعزيز التدريجي (غالباً الأدق في البيانات الجدولية)، SVM بنواة RBF (حدود غير خطية)،
أقرب الجيران، والشبكة العصبية MLP (جيدة مع نص TF-IDF). تُستخدم أوزان متوازنة للفئات بسبب عدم التوازن.

**الانحدار:** المتوسط، و«مثل الشهر الماضي» (المعيار العملي)، والانحدار الخطي، و Ridge، والغابة العشوائية، والتعزيز
التدريجي، و SVR، وأقرب الجيران، و MLP. درس من البيانات الفعلية: الانحدار الخطي العادي أعطى خطأً يقارب 2.1 مليون بسبب
التداخل الشديد بين خصائص الأشهر السابقة، بينما Ridge (النموذج نفسه مع تنظيم L2) مستقر وفاز.

**غير الموجّه:** ‏IsolationForest و LocalOutlierFactor للشذوذ، و KMeans لعدد عناقيد من 2 إلى 4.

**القيود:** قابلية التفسير (أهمية الخصائص لكل نموذج فائز)، والأداء (التحقق المتقاطع ثم بيانات محجوزة)، والحوسبة
(شبكات بحث صغيرة؛ يُدرَّب كل شيء في أقل من دقيقة).

</div>
