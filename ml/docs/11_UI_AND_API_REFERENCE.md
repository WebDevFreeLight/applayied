# 11 · UI and API reference

**English** · [العربية](#العربية)

## Portal page — ML Insights (`/ml`)

| Area | Content | Permission |
|---|---|---|
| Header | service status, as-of date, scheduler, project filter, Refresh, **Retrain now** | `pmo:ml:view`; retrain `pmo:ml:train` |
| KPI cards | tasks likely late (of N open), high delay risk, forecast spend next month, unusual expenses, segments | finance cards need `pmo:finance:list` |
| Delay risk | open tasks sorted by delay probability, colour bar, verdict, top drivers | `pmo:ml:view` |
| Spend forecast | history + next-month forecast with ± MAE range (ECharts), table vs plan | `pmo:finance:list` |
| Cost anomalies | most unusual posted costs with reasons ("5.3× the category median") | `pmo:finance:list` |
| Segments | utilization vs late-task share scatter coloured by segment + table | `pmo:ml:view` |
| Risk severity | type a risk → Low / Medium / High with probabilities and confidence | `pmo:ml:view` |
| Models & monitoring | each model's definition, winner, metrics vs baseline, EDA/evaluation charts, drift table, live accuracy, training history | `pmo:ml:view` |

Everything follows the EN ⇄ AR toggle: texts come from `i18n/en.ts` / `ar.ts` (`ml.*`), model names, labels and
drivers come translated from the API, and charts mirror in RTL (`inverse` x-axis, y-axis on the right).

## .NET endpoints (what the portal calls)

| Method | Route | Permission | Forwards to |
|---|---|---|---|
| GET | `/pmo/ml/status` | `pmo:ml:view` | `/health` |
| GET | `/pmo/ml/models` | `pmo:ml:view` | `/models` |
| GET | `/pmo/ml/models/{problem}` | `pmo:ml:view` | `/models/{problem}` |
| GET | `/pmo/ml/task-delay?projectId=` | `pmo:ml:view` | `/predict/task-delay` |
| POST | `/pmo/ml/risk-severity` `{text, kind, projectId}` | `pmo:ml:view` | `/predict/risk-severity` |
| GET | `/pmo/ml/spend-forecast?projectId=` | `pmo:finance:list` | `/forecast/spend` |
| GET | `/pmo/ml/cost-anomalies?projectId=&top=` | `pmo:finance:list` | `/anomalies/costs` |
| GET | `/pmo/ml/segments` | `pmo:ml:view` | `/segments` |
| GET | `/pmo/ml/monitoring` | `pmo:ml:view` | `/monitoring` |
| GET | `/pmo/ml/history` | `pmo:ml:view` | `/training/history` |
| POST | `/pmo/ml/train` `{problems?, eda?}` | `pmo:ml:train` | `/train` |
| GET | `/pmo/ml/chart?path=` | `pmo:ml:view` | `/reports/<path>.png` |

The language is taken from `Accept-Language` and sent as `?lang=en|ar`. JSON is passed through unchanged
(snake_case keys). All ids are strings (64-bit snowflakes lose precision as JavaScript numbers).

## ML service endpoints (direct)

Interactive docs at `http://localhost:8001/docs`. Example:

```bash
curl -s "http://localhost:8001/predict/task-delay?lang=ar"
```

```json
{ "problem": "task_delay", "available": true, "version": "20261025-025527", "model": "random_forest",
  "drivers": ["عدد مهام المشروع", "عبء المسند إليه", "بداية المهمة من بداية المشروع (أيام)"],
  "items": [ { "task_id": "4061", "title": "API development", "due_date": "2026-12-18",
               "probability": 0.918, "risk_band": "High", "risk_label": "مرتفع", "verdict": "مرجح التأخر" } ] }
```

---

<div dir="rtl">

## العربية

## صفحة البوابة — رؤى التعلّم الآلي (`/ml`)

| الجزء | المحتوى | الصلاحية |
|---|---|---|
| الترويسة | حالة الخدمة، التاريخ المرجعي، المجدول، تصفية المشروع، تحديث، **إعادة التدريب الآن** | `pmo:ml:view`، والتدريب `pmo:ml:train` |
| بطاقات المؤشرات | المهام المرجح تأخرها، خطر التأخر المرتفع، الإنفاق المتوقع، المصروفات غير الاعتيادية، الشرائح | بطاقات المالية تتطلب `pmo:finance:list` |
| خطر التأخر | المهام المفتوحة مرتبة حسب احتمال التأخر مع أهم العوامل | `pmo:ml:view` |
| توقع الإنفاق | السجل وتوقع الشهر القادم مع نطاق الخطأ | `pmo:finance:list` |
| التكاليف غير الاعتيادية | أكثر التكاليف شذوذاً مع الأسباب | `pmo:finance:list` |
| الشرائح | مخطط الاستخدام مقابل نسبة المهام المتأخرة مع جدول | `pmo:ml:view` |
| شدة الخطر | اكتب الخطر ← منخفض / متوسط / مرتفع مع الاحتمالات | `pmo:ml:view` |
| النماذج والمراقبة | تعريف كل نموذج، الفائز، المقاييس مقابل خط الأساس، المخططات، الانحراف، سجل التدريب | `pmo:ml:view` |

كل شيء يتبع زر التبديل بين العربية والإنجليزية: النصوص من ملفات الترجمة، وأسماء النماذج والتسميات والعوامل تأتي
مترجمة من الواجهة البرمجية، والمخططات تنعكس من اليمين إلى اليسار.

## نقاط نهاية ‎.NET

جميعها تحت `/pmo/ml/` (انظر الجدول الإنجليزي أعلاه) وتحمي كل منها صلاحية محددة، وتمرر اللغة من `Accept-Language`
وتعيد JSON كما هو. جميع المعرّفات نصوص لأن الأرقام الكبيرة تفقد دقتها في JavaScript.

## نقاط نهاية خدمة التعلّم الآلي

التوثيق التفاعلي على `http://localhost:8001/docs`، ومثال الاستجابة بالعربية موضّح أعلاه.

</div>
