# 02 · Data collection

**English** · [العربية](#العربية) · Code: [`db.py`](../src/pmo_ml/db.py), [`collect.py`](../src/pmo_ml/collect.py)

## Source: the portal's own PostgreSQL

The connection string is **not** stored in `ml/`. At runtime `db.py` opens
`backend/ZR.Admin.WebApi/appsettings.json`, strips its `//` comments, reads `dbConfigs[0].Conn`
(Npgsql format `Host=…;Port=…;Database=…;Username=…;Password=…`) and turns it into a SQLAlchemy URL.

| Priority | Setting | Use when |
|---|---|---|
| 1 | `PMO_DB_CONN` (Npgsql style) | another database on the same machine |
| 2 | `DATABASE_URL` (`postgres://user:pass@host:5432/db`) | Render, Neon, Docker |
| 3 | `appsettings.json → dbConfigs[0].Conn` | default — same DB as the portal |

`/health` and the logs show `host:port/db as user`, never the password.

## What is read

Only the columns the models need (`TABLES` in `collect.py`):

| Logical name | Table | Used by |
|---|---|---|
| projects | `pmo_project` | all (context) |
| tasks | `pmo_task` | task delay, segments |
| risks | `pmo_risk` | risk severity, context |
| costs | `pmo_cost_transaction` | spend forecast, anomalies |
| forecasts | `pmo_financial_forecast` | spend forecast (planned estimate) |
| members, revisions, categories, snapshots | `pmo_project_member`, `pmo_budget_revision`, `pmo_cost_category`, `pmo_progress_snapshot` | context features |

```bash
ml\.venv\Scripts\python -m pmo_ml collect
```

prints the row counts and writes a snapshot to `data/raw/<timestamp>/`. Every training run stores its snapshot path
in the model metadata (**lineage**: any model can be traced to the exact data it saw).

## Fallback: the CSV seed

`--source csv` reads `data/csv/*.csv` (the synthetic PMO package). Column names there are PascalCase
(`ProjectId`); `collect.snake()` converts them, so the rest of the pipeline is identical. Tests always use this
source.

## Legal and ethical compliance

* Read-only access; the ML service never writes to PMO tables.
* No names, e-mails or free-text user data are read — users are opaque ids.
* Data stays inside your infrastructure; no external ML API is called.
* Snapshots are git-ignored (`ml/.gitignore`) so data never lands in the repository.

---

<div dir="rtl">

## العربية

## جمع البيانات

**المصدر:** قاعدة PostgreSQL الخاصة بالبوابة نفسها. لا يُخزَّن نص الاتصال داخل `ml/`؛ بل يفتح `db.py` أثناء التشغيل
الملف `appsettings.json`، ويزيل التعليقات، ويقرأ `dbConfigs[0].Conn` ويحوّله إلى عنوان SQLAlchemy.

**الأولوية:** ‏`PMO_DB_CONN` ثم `DATABASE_URL` ثم ملف `appsettings.json` (الافتراضي). تعرض نقطة `/health` والسجلات
الخادم وقاعدة البيانات واسم المستخدم فقط، ولا تعرض كلمة المرور أبداً.

**ما الذي يُقرأ:** الأعمدة التي تحتاجها النماذج فقط من جداول المشاريع، والمهام، والمخاطر، والتكاليف، والتوقعات
المالية، والأعضاء، ومراجعات الميزانية، وفئات التكلفة.

الأمر `python -m pmo_ml collect` يطبع عدد الصفوف ويحفظ لقطة في `data/raw/<التاريخ>/`. كل عملية تدريب تحفظ مسار اللقطة
في بيانات النموذج الوصفية، فيمكن دائماً معرفة البيانات الدقيقة التي تدرّب عليها أي نموذج.

**البديل:** ‏`--source csv` يقرأ البيانات التجريبية من `data/csv`، وتُحوَّل أسماء الأعمدة تلقائياً.

**الامتثال القانوني والأخلاقي:** وصول للقراءة فقط، دون أسماء أو بريد إلكتروني، والبيانات لا تغادر بنيتك التحتية،
واللقطات مستثناة من Git.

</div>
