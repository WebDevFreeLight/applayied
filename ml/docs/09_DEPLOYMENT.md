# 09 · Deployment

**English** · [العربية](#العربية) · Code: [`api.py`](../src/pmo_ml/api.py), [`PmoMlService.cs`](../../backend/ZR.Service/PMO/PmoMlService.cs), [`PmoMlController.cs`](../../backend/ZR.Admin.WebApi/Controllers/PMO/PmoMlController.cs)

## Architecture

```mermaid
sequenceDiagram
    participant UI as React (MlInsights.tsx)
    participant NET as .NET /pmo/ml/*
    participant ML as PMO ML (FastAPI :8001)
    participant DB as PostgreSQL
    UI->>NET: GET /pmo/ml/task-delay (JWT, Accept-Language: ar)
    NET->>NET: ActionPermissionFilter pmo:ml:view
    NET->>ML: GET /predict/task-delay?lang=ar (X-ML-Key)
    ML->>DB: read PMO tables (cached 60 s)
    ML-->>NET: JSON {items, drivers, version}
    NET-->>UI: {code:200, data: same JSON}
```

The browser never talks to the ML service: the .NET API keeps authentication and RBAC in one place. If the ML
service is down, the bridge returns `{available:false, message}` and the page shows a friendly notice — the rest of
the portal is unaffected.

## Run locally

```bash
ml\.venv\Scripts\python -m pmo_ml serve
```

Swagger UI: <http://localhost:8001/docs>. The .NET API finds it through `appsettings.json`:

```json
"ML": { "BaseUrl": "http://localhost:8001", "ApiKey": "", "TimeoutSeconds": 30, "TrainTimeoutSeconds": 300 }
```

### Optional shared secret

Set the same value in both places (never commit it):

* ML service: env `PMO_ML_API_KEY` (or `api.api_key` in `config.yaml`)
* .NET: env `ML__ApiKey`

The service then rejects calls without the `X-ML-Key` header (except `/health`).

## Docker

```bash
docker build -t pmo-ml ml
docker run -p 8001:8001 -e DATABASE_URL=postgres://user:pass@host:5432/mahamdb pmo-ml
```

In a container there is no `appsettings.json`, so the database comes from `DATABASE_URL` or `PMO_DB_CONN`. Models
are trained automatically on first start.

## Render (cloud)

1. New → **Web Service** → this repo, **Root directory** `ml`, **Runtime** Docker.
2. Environment: `DATABASE_URL` = the same value as the API service; `PMO_ML_API_KEY` = a long random string.
3. On the API service (`applayied`): `ML__BaseUrl` = the ML service's URL (its `https://<name>.onrender.com`
   address, or the private-network address if your plan offers one) and `ML__ApiKey` = the same random string.
   With a public URL the shared key is required, not optional.
4. Free instances sleep and their disk is ephemeral: models are retrained on wake-up (~1 minute). For a permanent
   registry attach a disk at `/app/models`.

## Permissions

| Permission | Granted to | Opens |
|---|---|---|
| `pmo:ml:view` | admin, PM, contributor | ML Insights page, task delay, segments, severity, models, monitoring |
| `pmo:finance:list` | admin, PM | spend forecast and cost anomalies (finance data) |
| `pmo:ml:train` | admin | "Retrain now" |

The seeder creates `pmo:ml:view` and `pmo:ml:train` on the API's next start (when `PmoSeed` is true).

---

<div dir="rtl">

## العربية

## النشر

**البنية:** الواجهة الأمامية ← واجهة ‎.NET ‏(`/pmo/ml/*` مع التحقق من الصلاحيات) ← خدمة التعلّم الآلي FastAPI على المنفذ
8001 ← قاعدة PostgreSQL. المتصفح لا يتصل بخدمة التعلّم الآلي مباشرة، فتبقى المصادقة والصلاحيات في مكان واحد. وإذا
توقفت الخدمة تعيد واجهة ‎.NET رسالة ودية `{available:false}` دون التأثير على بقية البوابة.

**التشغيل المحلي:** ‏`python -m pmo_ml serve` ثم افتح `http://localhost:8001/docs`. تجد واجهة ‎.NET الخدمة عبر القسم
`ML` في `appsettings.json`.

**المفتاح المشترك (اختياري):** ضع القيمة نفسها في `PMO_ML_API_KEY` للخدمة و `ML__ApiKey` لواجهة ‎.NET ولا تحفظها في Git.

**Docker:** ‏`docker build -t pmo-ml ml` ثم التشغيل مع `DATABASE_URL`. تُدرَّب النماذج تلقائياً عند أول تشغيل.

**Render:** أنشئ خدمة ويب بمجلد الجذر `ml` ونوع Docker، واضبط `DATABASE_URL` و `PMO_ML_API_KEY`، ثم اضبط في خدمة
الواجهة `ML__BaseUrl` و `ML__ApiKey`. القرص في الخطة المجانية مؤقت، فتُعاد تدريب النماذج عند الاستيقاظ.

**الصلاحيات:** ‏`pmo:ml:view` لجميع الأدوار، و `pmo:finance:list` لتبويبي الإنفاق والتكاليف، و `pmo:ml:train` للمسؤول
لزر «إعادة التدريب الآن».

</div>
