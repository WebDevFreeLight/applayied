# 00 · Modifications log

**English** · [العربية](#العربية)

Every file added or changed in the repository to build the ML feature (branch `feat/pmo-ml`). Add a row here for
each later change so the history of the ML work stays readable in one place.

## 2026-10-25 — v1.0.0: initial ML project

### Added — Python project `ml/`

| File | What it is |
|---|---|
| `ml/README.md` | Project overview (EN/AR) |
| `ml/requirements.txt`, `ml/pyproject.toml` | Pinned dependencies; `pmo-ml` package and CLI |
| `ml/config/config.yaml` | All settings, each overridable by an environment variable |
| `ml/src/pmo_ml/config.py` | Loads the YAML + env overrides |
| `ml/src/pmo_ml/db.py` | Reads `dbConfigs[0].Conn` from `appsettings.json` (comment-tolerant), or `PMO_DB_CONN` / `DATABASE_URL` |
| `ml/src/pmo_ml/collect.py` | Step 2 — SQL extraction, CSV fallback, lineage snapshots |
| `ml/src/pmo_ml/clean.py` | Step 3 — cleaning and business rules |
| `ml/src/pmo_ml/eda.py` | Step 4 — EDA charts + bilingual `EDA.md` |
| `ml/src/pmo_ml/features.py` | Step 5 — feature builders (leakage-checked) |
| `ml/src/pmo_ml/models.py` | Step 6 — pipelines and candidate algorithms |
| `ml/src/pmo_ml/train.py` | Step 7 — CV, tuning, selection, refit |
| `ml/src/pmo_ml/evaluate.py` | Step 8 — metrics, plots, permutation importance |
| `ml/src/pmo_ml/registry.py` | Versioned model store |
| `ml/src/pmo_ml/predict.py`, `api.py` | Step 9 — live inference and FastAPI service |
| `ml/src/pmo_ml/monitor.py` | Step 10 — PSI drift, live accuracy, retrain decision |
| `ml/src/pmo_ml/pipeline.py`, `cli.py`, `__main__.py` | Orchestration and commands |
| `ml/src/pmo_ml/problems.py`, `i18n.py` | Bilingual problem definitions and labels |
| `ml/tests/*` | 23 pytest tests |
| `ml/scripts/*.ps1` | Setup, pipeline run, optional Windows scheduled task |
| `ml/Dockerfile`, `ml/.dockerignore`, `ml/.gitignore` | Container image; generated folders ignored |
| `ml/docs/00…12_*.md` | These tutorials |

### Added — .NET

| File | What it is |
|---|---|
| `backend/ZR.Service/PMO/IPmoService/IPmoMlService.cs` | Bridge contract |
| `backend/ZR.Service/PMO/PmoMlService.cs` | HttpClient bridge: timeouts, `X-ML-Key`, JSON pass-through, graceful `{available:false}` |
| `backend/ZR.Admin.WebApi/Controllers/PMO/PmoMlController.cs` | `/pmo/ml/*` endpoints with RBAC and language pass-through |
| `backend/ZR.Pmo.Tests/PmoMlServiceTests.cs` | 6 xUnit tests with a fake HTTP handler |

### Changed — .NET

| File | Change |
|---|---|
| `backend/ZR.Admin.WebApi/Program.cs` | Registers `IPmoMlService` as a typed HttpClient |
| `backend/ZR.Admin.WebApi/appsettings.json` | New `ML` section (`BaseUrl`, `ApiKey`, timeouts) |
| `backend/ZR.Service/PMO/PmoDataSeeder.cs` | Permissions `pmo:ml:view` (all roles) and `pmo:ml:train` (admin) |

### Added / changed — frontend

| File | Change |
|---|---|
| `frontend/src/api/ml.ts` | Typed client for `/pmo/ml/*` |
| `frontend/src/pages/MlInsights.tsx` | ML Insights page: KPI cards + 6 tabs |
| `frontend/src/App.tsx` | Route `/ml` gated by `pmo:ml:view` |
| `frontend/src/components/Layout.tsx` | Sidebar link "ML Insights" |
| `frontend/src/i18n/en.ts`, `ar.ts` | `nav.ml` and the `ml.*` strings in English and Arabic |

### Database

No schema change. On first start of the updated API the seeder adds two permission rows (`pmo:ml:view`,
`pmo:ml:train`) under the PMO menu and grants them by tier. The ML service only **reads** the PMO tables.

## Template for the next change

```markdown
## YYYY-MM-DD — vX.Y.Z: <short title>
| File | Change |
|---|---|
| path/to/file | what changed and why |
```

---

<div dir="rtl">

## العربية

## سجل التعديلات

يسرد هذا الملف كل ملف أُضيف أو عُدّل في المستودع لبناء ميزة التعلّم الآلي (الفرع `feat/pmo-ml`). أضف صفاً هنا مع كل
تعديل لاحق ليبقى تاريخ العمل واضحاً في مكان واحد.

### الإضافات — مشروع Python ‏`ml/`

- ملفات الإعداد والمكتبات: `requirements.txt` و `pyproject.toml` و `config/config.yaml`.
- وحدة لكل خطوة من خطوات دورة الحياة: `collect` (جمع البيانات)، `clean` (التنظيف)، `eda` (التحليل الاستكشافي)،
  `features` (هندسة الخصائص)، `models` (اختيار النموذج)، `train` (التدريب)، `evaluate` (التقييم)،
  `api` و `predict` (النشر)، `monitor` (المراقبة).
- سجل النماذج `registry.py`، والأوامر `cli.py`، والنصوص ثنائية اللغة `problems.py` و `i18n.py`.
- 23 اختباراً آلياً، وسكربتات PowerShell، وملف Docker، وهذه الدروس.

### الإضافات والتعديلات — ‎.NET

- خدمة الربط `PmoMlService` ووحدة التحكم `PmoMlController` على المسار `/pmo/ml/*` مع الصلاحيات.
- تسجيل الخدمة في `Program.cs`، وقسم `ML` جديد في `appsettings.json`.
- صلاحيتان جديدتان في المُهيِّئ: `pmo:ml:view` لجميع الأدوار و `pmo:ml:train` للمسؤول فقط.
- 6 اختبارات xUnit للخدمة.

### الواجهة الأمامية

- صفحة **رؤى التعلّم الآلي** (`MlInsights.tsx`) مع بطاقات المؤشرات وستة تبويبات، ومسار `/ml`، ورابط في القائمة،
  وكل النصوص بالإنجليزية والعربية.

### قاعدة البيانات

لا تغيير في البنية. عند أول تشغيل للواجهة المحدّثة يضيف المُهيِّئ صفّي صلاحيات فقط. خدمة التعلّم الآلي **تقرأ**
جداول PMO ولا تكتب فيها.

</div>
