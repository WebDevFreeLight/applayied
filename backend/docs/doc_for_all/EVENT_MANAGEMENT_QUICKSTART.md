# 🚀 بداية سريعة - نظام إدارة الفعاليات

## ✅ ما تم إنجازه

تم إنشاء **12 نموذج (Model)** كاملة لنظام إدارة الفعاليات:

### الملفات المُنشأة:
```
backend/ZR.Model/EventManagement/
├── Event.cs              # معلومات الفعاليات
├── EventZone.cs          # المناطق
├── EventGate.cs          # البوابات
├── EventVisitor.cs       # الزوار
├── EventTicket.cs        # التذاكر
├── EventPermit.cs        # التصاريح
├── EventBooth.cs         # الأكشاك
├── EventTenant.cs        # المستأجرين
├── EventContract.cs      # العقود
├── EventIncident.cs      # الحوادث
├── EventPayment.cs       # الدفعات
├── EventInvoice.cs       # الفواتير
└── Dto/
    └── EventDashboardDto.cs  # لوحة التحكم
```

## 📋 الخطوات التالية

### الخطوة 1: تفعيل إنشاء الجداول تلقائياً

افتح ملف: `backend/ZR.Admin.WebApi/Program.cs`

ابحث عن السطر الذي يحتوي على تعليقات حول Database Initialization (حوالي السطر 200)

أضف الكود التالي:

```csharp
// Initialize Event Management tables
using ZR.Model.EventManagement;

app.Environment.InitDbWithTypes(
    typeof(Event),
    typeof(EventZone),
    typeof(EventGate),
    typeof(EventVisitor),
    typeof(EventTicket),
    typeof(EventPermit),
    typeof(EventBooth),
    typeof(EventTenant),
    typeof(EventContract),
    typeof(EventIncident),
    typeof(EventPayment),
    typeof(EventInvoice)
);
```

### الخطوة 2: تشغيل النظام لإنشاء الجداول

```bash
cd backend/ZR.Admin.WebApi
dotnet run
```

سيتم إنشاء جميع الجداول تلقائياً في قاعدة البيانات PostgreSQL.

### الخطوة 3: التحقق من الجداول

```bash
# افتح قاعدة البيانات وتحقق من وجود الجداول:
event_events
event_zones
event_gates
event_visitors
event_tickets
event_permits
event_booths
event_tenants
event_contracts
event_incidents
event_payments
event_invoices
```

## 🔧 إنشاء Repositories & Services & Controllers

### خيار 1: استخدام Code Generator المدمج ⚡ (الأسرع)

1. شغل النظام
2. افتح المتصفح على: `http://localhost:8887`
3. اذهب إلى قسم **Code Generator**
4. اختر الجدول (مثل: `event_events`)
5. اضغط **Generate Code**
6. سيتم توليد:
   - Repository
   - Service
   - Controller
   - Frontend Components

### خيار 2: الإنشاء اليدوي 🛠️

إذا أردت إنشاء الطبقات يدوياً، اتبع هذا النمط:

#### Repository Example:

```csharp
// backend/ZR.Repository/EventManagement/EventRepository.cs
public interface IEventRepository : IBaseRepository<Event>
{
    Task<List<Event>> GetActiveEventsAsync();
}

public class EventRepository : BaseRepository<Event>, IEventRepository
{
    public async Task<List<Event>> GetActiveEventsAsync()
    {
        return await GetListAsync(x => x.IsActive == true);
    }
}
```

#### Service Example:

```csharp
// backend/ZR.Service/EventManagement/EventService.cs
public interface IEventService
{
    Task<List<Event>> GetActiveEventsAsync();
    Task<Event> CreateEventAsync(Event eventData);
}

public class EventService : IEventService
{
    private readonly IEventRepository _repository;

    public EventService(IEventRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<Event>> GetActiveEventsAsync()
    {
        return await _repository.GetActiveEventsAsync();
    }

    public async Task<Event> CreateEventAsync(Event eventData)
    {
        return await _repository.InsertAsync(eventData);
    }
}
```

#### Controller Example:

```csharp
// backend/ZR.Admin.WebApi/Controllers/EventManagement/EventController.cs
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class EventController : ControllerBase
{
    private readonly IEventService _eventService;

    public EventController(IEventService eventService)
    {
        _eventService = eventService;
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActiveEvents()
    {
        var events = await _eventService.GetActiveEventsAsync();
        return Ok(ApiResult.Success(events));
    }

    [HttpPost]
    public async Task<IActionResult> CreateEvent([FromBody] Event eventData)
    {
        var result = await _eventService.CreateEventAsync(eventData);
        return Ok(ApiResult.Success(result));
    }
}
```

## 📊 اختبار النظام

### 1. باستخدام Swagger
افتح: `http://localhost:8887/swagger`

### 2. باستخدام Scalar (الأفضل)
افتح: `http://localhost:8887/scalar`

### 3. اختبار إنشاء فعالية جديدة:

```json
POST /api/Event
{
  "eventCode": "EVT001",
  "eventName": "Tech Conference 2025",
  "eventNameAr": "مؤتمر التقنية 2025",
  "eventType": "conference",
  "status": "planned",
  "startDate": "2025-03-01",
  "endDate": "2025-03-03",
  "maxCapacity": 5000,
  "city": "Riyadh",
  "isActive": true
}
```

## 🎯 نصائح التطوير

### استخدام الصلاحيات:
```csharp
[Authorize(Roles = "EventManager")]  // للمدراء فقط
[Authorize]  // لأي مستخدم مسجل
[AllowAnonymous]  // بدون مصادقة
```

### معالجة الأخطاء:
```csharp
try
{
    var result = await _service.CreateAsync(data);
    return Ok(ApiResult.Success(result));
}
catch (Exception ex)
{
    return BadRequest(ApiResult.Error(ex.Message));
}
```

### التصفية والبحث:
```csharp
var events = await _repository.GetListAsync(x =>
    x.IsActive &&
    x.StartDate >= DateTime.Now &&
    x.City == "Riyadh"
);
```

## 📱 ربط Frontend (Vue.js)

### إنشاء API Service:

```javascript
// frontend/src/api/event.js
import request from '@/utils/request'

export function getEvents() {
  return request({
    url: '/api/Event/active',
    method: 'get'
  })
}

export function createEvent(data) {
  return request({
    url: '/api/Event',
    method: 'post',
    data
  })
}
```

### استخدام في Component:

```vue
<script setup>
import { ref, onMounted } from 'vue'
import { getEvents } from '@/api/event'

const events = ref([])

onMounted(async () => {
  const response = await getEvents()
  events.value = response.data
})
</script>

<template>
  <div>
    <h1>الفعاليات النشطة</h1>
    <div v-for="event in events" :key="event.eventId">
      <h3>{{ event.eventName }}</h3>
      <p>{{ event.description }}</p>
    </div>
  </div>
</template>
```

## 🔍 التوثيق الكامل

للتفاصيل الشاملة، راجع:
- **EVENT_MANAGEMENT_SYSTEM.md** - التوثيق الكامل والشامل
- **CLAUDE.md** - دليل تطوير المشروع الأساسي

## 💡 أمثلة متقدمة

### لوحة التحكم Dashboard:

```csharp
[HttpGet("dashboard")]
public async Task<IActionResult> GetDashboard([FromQuery] long eventId)
{
    var dashboard = new EventDashboardDto
    {
        EventInfo = await GetEventInfo(eventId),
        Visitors = await GetVisitorStats(eventId),
        Gates = await GetGateStatus(eventId),
        Zones = await GetZoneOccupancy(eventId),
        Booths = await GetBoothStats(eventId),
        Financial = await GetFinancialSummary(eventId),
        Incidents = await GetIncidentSummary(eventId),
        Staff = await GetStaffSummary(eventId),
        LastUpdated = DateTime.Now
    };

    return Ok(ApiResult.Success(dashboard));
}
```

### تسجيل دخول زائر:

```csharp
[HttpPost("visitor/checkin")]
public async Task<IActionResult> CheckInVisitor([FromBody] CheckInRequest request)
{
    var visitor = await _visitorService.GetByQRCodeAsync(request.QRCode);

    if (visitor == null)
        return NotFound(ApiResult.Error("Visitor not found"));

    if (visitor.Status == "checked-in")
        return BadRequest(ApiResult.Error("Already checked in"));

    visitor.Status = "checked-in";
    visitor.CheckInTime = DateTime.Now;
    visitor.CheckInGateId = request.GateId;

    await _visitorService.UpdateAsync(visitor);

    // Update gate statistics
    await _gateService.IncrementCheckInCount(request.GateId);

    return Ok(ApiResult.Success("Check-in successful"));
}
```

## 🎉 جاهز للانطلاق!

لديك الآن نظام إدارة فعاليات كامل جاهز للتطوير والاستخدام!

الخطوة التالية: اختر أحد الخيارات أعلاه لإنشاء Repositories والـ Services والـ Controllers.

---

**للأسئلة أو المساعدة، راجع التوثيق الكامل في EVENT_MANAGEMENT_SYSTEM.md**
