# نظام إدارة الفعاليات الداخلي - Internal Event Management System

## 🎯 نظرة عامة - Overview

نظام ERP متكامل لإدارة الفعاليات والأحداث بشكل شامل، يغطي كل جوانب الفعالية من الزوار والبوابات إلى المستأجرين والحوادث.

---

## 📦 الهيكل المعماري - Architecture

### Models Created (النماذج المُنشأة)

تم إنشاء جميع النماذج في المسار:
```
backend/ZR.Model/EventManagement/
```

#### 1. إدارة الفعاليات - Event Management
- **Event.cs** - معلومات الفعالية الأساسية
  - رمز الفعالية، الاسم بالعربي والإنجليزي
  - التواريخ (البداية، النهاية، التسجيل)
  - الموقع والعنوان
  - السعة القصوى والعدد المتوقع
  - معلومات المنظم والتواصل
  - حالة الفعالية

- **EventZone.cs** - المناطق داخل الفعالية
  - رمز واسم المنطقة
  - نوع المنطقة (VIP, عامة، طعام، معارض)
  - السعة القصوى والعدد الحالي
  - المساحة ورقم الطابق
  - إحداثيات الخريطة
  - مستوى الوصول

- **EventGate.cs** - بوابات الدخول والخروج
  - رمز واسم البوابة
  - نوع البوابة (دخول، خروج، كلاهما، VIP، موظفين)
  - السعة القصوى بالساعة
  - عدد المنتظرين ومتوسط وقت الانتظار
  - حالة البوابة (مفتوح، مغلق، صيانة، ممتلئ)
  - أوقات الفتح والإغلاق
  - معلومات الماسح الضوئي

#### 2. إدارة الزوار والحضور - Visitor & Attendance
- **EventVisitor.cs** - بيانات الزوار
  - المعلومات الشخصية (الاسم، البريد، الهاتف)
  - الهوية والجنسية
  - نوع الزائر (عادي، VIP، إعلامي، متحدث)
  - معلومات الشركة والوظيفة
  - حالة الزيارة ووقت الدخول/الخروج
  - رمز QR والبطاقة
  - احتياجات خاصة

- **EventTicket.cs** - تذاكر الدخول
  - رقم التذكرة ونوعها
  - السعر والعملة
  - تواريخ الصلاحية
  - رمز QR والباركود
  - حالة الاستخدام
  - معلومات الدفع
  - إمكانية التحويل

- **EventPermit.cs** - تصاريح الدخول
  - نوع التصريح (موظف، مقاول، VIP، إعلامي، مركبة)
  - معلومات مقدم الطلب
  - تواريخ الصلاحية
  - مستوى الوصول للمناطق والبوابات
  - حالة التصريح (معلق، موافق، مرفوض، نشط)
  - معلومات المركبة (لتصاريح المركبات)
  - المرفقات والصور

#### 3. إدارة المستأجرين والأكشاك - Tenant & Booth Management
- **EventBooth.cs** - الأكشاك/المحلات المؤجرة
  - رقم واسم الكشك
  - نوع الكشك (طعام، بيع، خدمات، معرض)
  - المساحة والموقع
  - سعر الإيجار ونوع الفترة
  - الخدمات (كهرباء، ماء، إنترنت، أثاث)
  - حالة الإيجار
  - تواريخ التجهيز والتفكيك

- **EventTenant.cs** - بيانات المستأجرين
  - معلومات الشركة
  - السجل التجاري والرقم الضريبي
  - معلومات جهة الاتصال
  - المعلومات البنكية
  - التقييم والحالة
  - إجمالي الأكشاك المؤجرة

- **EventContract.cs** - عقود الإيجار
  - رقم العقد وتاريخه
  - الفترة الزمنية
  - المبالغ (الإجمالي، الضريبة، التأمين)
  - شروط وجدول الدفع
  - حالة التوقيع
  - الشروط والأحكام
  - التجديد التلقائي

#### 4. إدارة الحوادث والدعم - Incident & Support
- **EventIncident.cs** - الحوادث والبلاغات
  - نوع الحادث (أمني، طبي، تقني، صيانة، شكوى)
  - الأولوية والخطورة
  - الموقع والمنطقة
  - معلومات المبلغ
  - حالة البلاغ والتكليف
  - وقت الاستجابة والحل
  - التصعيد والمتابعة
  - تقييم رضا العميل

#### 5. النظام المالي - Financial Management
- **EventPayment.cs** - الدفعات
  - نوع الدفعة (تذكرة، إيجار، تأمين، غرامة، مرتجع)
  - المبلغ والعملة والضريبة
  - طريقة الدفع (نقدي، بطاقة، تحويل، أونلاين)
  - معلومات البطاقة والمعاملة
  - بوابة الدفع ورد الدفع
  - الإيصال والمرتجعات

- **EventInvoice.cs** - الفواتير
  - رقم الفاتورة وتاريخها
  - تاريخ الاستحقاق
  - المبالغ (فرعي، ضريبة، خصم، إجمالي)
  - المبلغ المدفوع والرصيد
  - بنود الفاتورة
  - حالة الدفع والتأخير
  - رسوم التأخير

#### 6. لوحة التحكم والتقارير - Dashboard & Reports
- **EventDashboardDto.cs** - لوحة التحكم الرئيسية
  - معلومات الفعالية الأساسية
  - إحصائيات الزوار الحالية
  - حالة البوابات ووقت الانتظار
  - إشغال المناطق
  - إحصائيات الأكشاك
  - الملخص المالي
  - ملخص الحوادث
  - الموظفون المناوبون

---

## 🗄️ قاعدة البيانات - Database Schema

### الجداول المُنشأة:
```sql
- event_events           -- الفعاليات
- event_zones            -- المناطق
- event_gates            -- البوابات
- event_visitors         -- الزوار
- event_tickets          -- التذاكر
- event_permits          -- التصاريح
- event_booths           -- الأكشاك
- event_tenants          -- المستأجرين
- event_contracts        -- العقود
- event_incidents        -- الحوادث
- event_payments         -- الدفعات
- event_invoices         -- الفواتير
```

### العلاقات الرئيسية:
```
Event (الفعالية)
  ├── EventZones (المناطق)
  │     └── EventBooths (الأكشاك)
  ├── EventGates (البوابات)
  ├── EventVisitors (الزوار)
  │     └── EventTickets (التذاكر)
  ├── EventTenants (المستأجرين)
  │     ├── EventBooths (الأكشاك المؤجرة)
  │     ├── EventContracts (العقود)
  │     ├── EventInvoices (الفواتير)
  │     └── EventPayments (الدفعات)
  └── EventIncidents (الحوادث)
```

---

## 🚀 الخطوات التالية - Next Steps

### المرحلة الحالية ✅
- [x] تصميم قاعدة البيانات
- [x] إنشاء جميع Models/Entities
- [x] إنشاء DTOs للوحة التحكم

### المراحل القادمة:

#### 1. Repositories Layer
```bash
# سيتم إنشاء:
- IEventRepository + EventRepository
- IEventVisitorRepository + EventVisitorRepository
- IEventGateRepository + EventGateRepository
- IEventBoothRepository + EventBoothRepository
- IEventTenantRepository + EventTenantRepository
- IEventIncidentRepository + EventIncidentRepository
- IEventPaymentRepository + EventPaymentRepository
- IEventInvoiceRepository + EventInvoiceRepository
```

#### 2. Services Layer
```bash
# سيتم إنشاء:
- IEventService + EventService
- IEventVisitorService + EventVisitorService
- IEventGateService + EventGateService
- IEventBoothService + EventBoothService
- IEventTenantService + EventTenantService
- IEventIncidentService + EventIncidentService
- IEventPaymentService + EventPaymentService
- IEventDashboardService + EventDashboardService
```

#### 3. Controllers (API Endpoints)
```bash
# سيتم إنشاء:
- EventController
- EventVisitorController
- EventGateController
- EventBoothController
- EventTenantController
- EventIncidentController
- EventPaymentController
- EventDashboardController
```

#### 4. Database Initialization
```csharp
// في Program.cs:
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

---

## 📊 مميزات النظام - System Features

### ✅ إدارة الزوار
- تسجيل الزوار (أونلاين/في الموقع)
- طباعة البطاقات مع QR Code
- متابعة الدخول والخروج
- إدارة التذاكر والتصاريح
- تصنيف الزوار (عادي، VIP، إعلامي، إلخ)

### ✅ إدارة الحشود
- متابعة العدد الحالي في كل منطقة
- عرض حالة البوابات ومتوسط وقت الانتظار
- تنبيهات عند اقتراب السعة القصوى
- تحليل تدفق الزوار

### ✅ إدارة المستأجرين
- قاعدة بيانات المستأجرين
- إدارة الأكشاك والمحلات
- العقود الإلكترونية
- الفوترة والدفعات
- متابعة المستحقات

### ✅ إدارة الحوادث
- استقبال البلاغات
- تصنيف حسب الأولوية والخطورة
- التكليف التلقائي
- متابعة الحالة
- التصعيد عند الحاجة
- تقارير الأداء

### ✅ النظام المالي
- الفوترة الإلكترونية
- إدارة الدفعات
- ربط بوابات الدفع (Paymob)
- متابعة المستحقات
- التقارير المالية

### ✅ لوحة التحكم
- عرض لحظي لكل المؤشرات
- إحصائيات الزوار والبوابات
- نسب الإشغال
- الحالة المالية
- ملخص الحوادث
- حالة الموظفين

---

## 🛠️ التقنيات المستخدمة - Technologies

### Backend:
- **.NET 8** - الإطار الأساسي
- **ASP.NET Core Web API** - واجهات API
- **SqlSugar ORM** - إدارة قاعدة البيانات
- **PostgreSQL** - قاعدة البيانات الرئيسية
- **JWT Authentication** - المصادقة
- **SignalR** - التحديثات اللحظية

### Frontend (القادم):
- **Vue.js 3** - الواجهة الأمامية
- **Element Plus** - مكتبة المكونات
- **Chart.js** - الرسوم البيانية
- **QR Code Generator** - توليد رموز QR

---

## 📱 صلاحيات المستخدمين - User Roles

### 1. مدير النظام - System Admin
- الوصول الكامل لجميع الوظائف
- إدارة المستخدمين والصلاحيات
- التقارير الشاملة

### 2. مدير الفعالية - Event Manager
- إنشاء وإدارة الفعاليات
- متابعة الزوار والبوابات
- إدارة المستأجرين
- التقارير التفصيلية

### 3. موظف التسجيل - Registration Staff
- تسجيل الزوار
- طباعة البطاقات
- إصدار التصاريح

### 4. موظف البوابة - Gate Staff
- مسح QR Code
- تسجيل الدخول/الخروج
- متابعة الحشود

### 5. مسؤول الأكشاك - Booth Manager
- إدارة الأكشاك
- متابعة العقود
- إصدار الفواتير

### 6. فريق الدعم - Support Team
- استقبال البلاغات
- متابعة الحوادث
- إدارة الحالات

### 7. المستأجر - Tenant
- عرض العقود الخاصة
- متابعة الفواتير
- تقديم طلبات الصيانة

---

## 🔐 الأمان - Security

- مصادقة JWT
- تشفير البيانات الحساسة
- صلاحيات دقيقة لكل دور
- سجلات التدقيق (Audit Logs)
- حماية من SQL Injection
- معدلات الطلبات (Rate Limiting)

---

## 📈 التقارير المتاحة - Available Reports

### تقارير الزوار:
- إجمالي الزوار حسب الفترة
- توزيع أنواع الزوار
- تقرير الدخول والخروج
- تقرير VIP

### تقارير البوابات:
- أداء البوابات
- أوقات الذروة
- متوسط أوقات الانتظار

### تقارير المستأجرين:
- الإيرادات من الأكشاك
- معدل الإشغال
- المستحقات والمتأخرات
- أداء المستأجرين

### تقارير الحوادث:
- الحوادث حسب النوع
- أوقات الاستجابة والحل
- أداء فرق الدعم
- تحليل الاتجاهات

### التقارير المالية:
- الإيرادات التفصيلية
- الفواتير والمدفوعات
- المتأخرات
- التوقعات المالية

---

## 🎨 واجهة المستخدم - UI Features

### لوحة التحكم الرئيسية:
- بطاقات المؤشرات الرئيسية (KPI Cards)
- رسوم بيانية تفاعلية
- خريطة الموقع مع حالة المناطق
- قائمة البوابات ووقت الانتظار
- آخر الحوادث
- تنبيهات وإشعارات

### الخصائص:
- تصميم متجاوب (Responsive)
- دعم اللغة العربية والإنجليزية
- وضع مظلم/فاتح
- تحديثات لحظية (Real-time)
- طباعة التقارير
- تصدير البيانات (Excel, PDF)

---

## 📞 الدعم والتوثيق - Support & Documentation

للمساعدة في التطوير والاستخدام:

1. **CLAUDE.md** - دليل التطوير الأساسي
2. **EVENT_MANAGEMENT_SYSTEM.md** - هذا الملف (التوثيق الشامل)
3. **API Documentation** - متاح عبر Swagger/Scalar

---

## 📝 ملاحظات التطوير - Development Notes

### التسميات:
- جميع الجداول تبدأ بـ `event_`
- جميع الـ Models في namespace: `ZR.Model.EventManagement`
- جميع الـ DTOs في namespace: `ZR.Model.EventManagement.Dto`

### الاتفاقيات:
- استخدام `SugarTable` و `SugarColumn` attributes
- جميع الـ IDs من نوع `long` مع Snowflake ID
- Soft Delete عبر `IsActive` field
- Audit fields من `SysBase` (Create_by, Create_time, Update_by, Update_time)
- Multi-language support (English/Arabic)

### Code Generation:
يمكن استخدام Code Generator المدمج لإنشاء:
- Controllers
- Services
- Repositories
- Frontend Components

---

## ✨ المزايا التنافسية - Competitive Advantages

1. **شامل ومتكامل** - يغطي كل جوانب الفعالية
2. **في الوقت الفعلي** - تحديثات لحظية عبر SignalR
3. **مرن وقابل للتخصيص** - يناسب أنواع مختلفة من الفعاليات
4. **متعدد اللغات** - دعم العربية والإنجليزية
5. **أمان عالي** - مصادقة وصلاحيات دقيقة
6. **تقارير شاملة** - تحليلات وإحصائيات تفصيلية
7. **سهل الاستخدام** - واجهة بديهية
8. **قابل للتوسع** - بنية معمارية قابلة للنمو

---

**تم بحمد الله إنشاء البنية الأساسية الكاملة لنظام إدارة الفعاليات الداخلي! 🎉**

الخطوة القادمة: إنشاء الـ Repositories والـ Services والـ Controllers.
