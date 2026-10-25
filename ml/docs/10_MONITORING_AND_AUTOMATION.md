# 10 · Monitoring, maintenance & automation

**English** · [العربية](#العربية) · Code: [`monitor.py`](../src/pmo_ml/monitor.py), [`api.py › _auto_loop`](../src/pmo_ml/api.py), [`scripts/`](../scripts)

```bash
ml\.venv\Scripts\python -m pmo_ml monitor     # exit code 3 = retraining recommended
```

## What is monitored

| Signal | How | Triggers retraining when |
|---|---|---|
| **Data drift** | PSI of every numeric feature: today's data vs the decile profile saved at training | any feature PSI > 0.25 (`monitoring.drift_psi_threshold`) |
| **Data growth** | rows available now vs rows at training | ≥ 5 new rows (`monitoring.min_new_rows_for_retrain`) |
| **Live accuracy** | task-delay predictions are logged to `reports/monitoring/predictions.jsonl`; when those tasks resolve, the old predictions are scored against reality | live accuracy < hold-out accuracy − 0.15 (after ≥ 5 resolved predictions) |
| **No model** | registry has no version | always |

PSI guide: < 0.1 stable · 0.1–0.25 watch · > 0.25 drift.

## Automation — built in

When the API service runs, a background thread checks every `api.auto_retrain_hours` (default 24, env
`PMO_ML_AUTO_RETRAIN_HOURS`, 0 = off). If any reason above fires, it retrains **only the affected models**, then
hot-reloads them — no restart, no downtime. On first start without models it trains everything
(`api.train_on_startup_if_missing`). A lock guarantees one training run at a time.

Every run (manual, API, scheduled) is appended to `reports/training_history.jsonl`, shown on the portal's
**Models & monitoring** tab with its trigger (`cli`, `api`, `auto:drift,new_data`, …).

## Automation — without the API

```bash
powershell -ExecutionPolicy Bypass -File ml\scripts\run_pipeline.ps1 -IfNeeded
```

runs the monitor and trains only when needed. To run it nightly:

```bash
powershell -ExecutionPolicy Bypass -File ml\scripts\register_retrain_task.ps1 -At 02:30
```

(registers a Windows scheduled task; remove it with `Unregister-ScheduledTask -TaskName "PMO ML retrain"`).

## Rollback

Each version stays on disk. If a new model misbehaves:

```bash
ml\.venv\Scripts\python -m pmo_ml promote task_delay 20261025-025527
```

or `POST /models/task_delay/promote/<version>`; the API serves it immediately.

---

<div dir="rtl">

## العربية

## المراقبة والصيانة والأتمتة

الأمر `python -m pmo_ml monitor` يعرض تقرير المراقبة، ويعيد الرمز 3 عندما يُنصح بإعادة التدريب.

**ما الذي يُراقَب:**

| الإشارة | الطريقة | متى تُعاد عملية التدريب |
|---|---|---|
| **انحراف البيانات** | مؤشر PSI لكل خاصية رقمية مقارنةً بملف التوزيع المحفوظ عند التدريب | إذا تجاوز أي مؤشر 0.25 |
| **نمو البيانات** | عدد الصفوف الآن مقارنةً بوقت التدريب | 5 صفوف جديدة أو أكثر |
| **الدقة الفعلية** | تُسجَّل تنبؤات تأخر المهام، وعند حسم المهام تُقارن التنبؤات القديمة بالواقع | إذا انخفضت الدقة 0.15 عن دقة الاختبار |
| **لا يوجد نموذج** | السجل فارغ | دائماً |

**الأتمتة المدمجة:** عند تشغيل خدمة الواجهة البرمجية يفحص مسار خلفي كل 24 ساعة (قابلة للتعديل)، ويعيد تدريب
**النماذج المتأثرة فقط** ثم يعيد تحميلها دون إعادة تشغيل. وعند أول تشغيل بلا نماذج يدرّب الكل. ويُسجَّل كل تشغيل في
سجل التدريب الظاهر في البوابة مع سببه.

**الأتمتة بدون الخدمة:** السكربت `run_pipeline.ps1 -IfNeeded` يدرّب عند الحاجة فقط، و `register_retrain_task.ps1`
يسجّل مهمة ويندوز مجدولة ليلية.

**التراجع:** كل إصدار يبقى محفوظاً؛ الأمر `python -m pmo_ml promote <النموذج> <الإصدار>` يعيد إصداراً سابقاً فوراً.

</div>
