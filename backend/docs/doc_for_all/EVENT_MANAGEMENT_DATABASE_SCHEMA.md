# 🗄️ نظام إدارة الفعاليات - Database Schema Documentation
# Event Management System - Complete Database Schema

---

## 📊 Database Overview - نظرة عامة على قاعدة البيانات

### Total Tables: 21 جدول
- **8 جداول أنواع/فئات (Lookup Tables)** - للبيانات المرجعية القابلة للتخصيص
- **12 جدول رئيسي (Main Tables)** - لبيانات النظام الأساسية
- **1 جدول DTO** - للوحة التحكم والتقارير

---

## 🎯 Section 1: Lookup/Reference Tables - جداول الأنواع والفئات

هذه الجداول تحتوي على البيانات المرجعية القابلة للتخصيص والإدارة من لوحة التحكم.

### 1.1 Event Types - أنواع الفعاليات
**Table:** `event_types`

| Column | Type | Description (EN) | الوصف (AR) |
|--------|------|------------------|-----------|
| TypeId | long (PK) | Type ID | معرف النوع |
| TypeCode | string(50) | Unique type code | رمز النوع الفريد |
| TypeName | string(100) | Type name (English) | اسم النوع بالإنجليزية |
| TypeNameAr | string(100) | Type name (Arabic) | اسم النوع بالعربية |
| Description | string(500) | Description | الوصف |
| Icon | string(100) | Icon/emoji | الأيقونة |
| ColorCode | string(20) | Color code (hex) | رمز اللون |
| DisplayOrder | int | Display order | ترتيب العرض |
| IsActive | bool | Active status | حالة التفعيل |

**Sample Data / بيانات نموذجية:**
```sql
INSERT INTO event_types VALUES
('conference', 'Conference', 'مؤتمر', 'Professional conferences', '🎤', '#3498db', 1, true),
('exhibition', 'Exhibition', 'معرض', 'Trade exhibitions', '🏪', '#2ecc71', 2, true),
('festival', 'Festival', 'مهرجان', 'Festivals and celebrations', '🎉', '#e74c3c', 3, true),
('concert', 'Concert', 'حفل موسيقي', 'Musical concerts', '🎵', '#9b59b6', 4, true),
('sports', 'Sports Event', 'حدث رياضي', 'Sports events', '⚽', '#f39c12', 5, true),
('workshop', 'Workshop', 'ورشة عمل', 'Training workshops', '📚', '#1abc9c', 6, true);
```

---

### 1.2 Zone Types - أنواع المناطق
**Table:** `event_zone_types`

| Column | Type | Description (EN) | الوصف (AR) |
|--------|------|------------------|-----------|
| TypeId | long (PK) | Type ID | معرف النوع |
| TypeCode | string(50) | Unique type code | رمز النوع |
| TypeName | string(100) | Type name (English) | اسم النوع بالإنجليزية |
| TypeNameAr | string(100) | Type name (Arabic) | اسم النوع بالعربية |
| Description | string(500) | Description | الوصف |
| Icon | string(100) | Icon | الأيقونة |
| ColorCode | string(20) | Color code | رمز اللون |
| RequiresSpecialAccess | bool | Requires special access | يتطلب صلاحية خاصة |
| DisplayOrder | int | Display order | ترتيب العرض |
| IsActive | bool | Active status | حالة التفعيل |

**Sample Data / بيانات نموذجية:**
```sql
INSERT INTO event_zone_types VALUES
('vip', 'VIP Area', 'منطقة VIP', 'VIP exclusive area', '👑', '#FFD700', true, 1, true),
('general', 'General Area', 'منطقة عامة', 'Public general area', '👥', '#3498db', false, 2, true),
('food', 'Food & Beverage', 'منطقة الطعام', 'Food court area', '🍽️', '#e67e22', false, 3, true),
('exhibition', 'Exhibition Area', 'منطقة المعارض', 'Exhibition halls', '🏛️', '#9b59b6', false, 4, true),
('parking', 'Parking Area', 'منطقة الانتظار', 'Vehicle parking', '🚗', '#95a5a6', false, 5, true),
('stage', 'Main Stage', 'المسرح الرئيسي', 'Main event stage', '🎭', '#e74c3c', true, 6, true);
```

---

### 1.3 Gate Types - أنواع البوابات
**Table:** `event_gate_types`

| Column | Type | Description (EN) | الوصف (AR) |
|--------|------|------------------|-----------|
| TypeId | long (PK) | Type ID | معرف النوع |
| TypeCode | string(50) | Unique type code | رمز النوع |
| TypeName | string(100) | Type name (English) | اسم النوع بالإنجليزية |
| TypeNameAr | string(100) | Type name (Arabic) | اسم النوع بالعربية |
| Description | string(500) | Description | الوصف |
| Icon | string(100) | Icon | الأيقونة |
| ColorCode | string(20) | Color code | رمز اللون |
| Direction | string(20) | Direction type | نوع الاتجاه |
| DisplayOrder | int | Display order | ترتيب العرض |
| IsActive | bool | Active status | حالة التفعيل |

**Sample Data / بيانات نموذجية:**
```sql
INSERT INTO event_gate_types VALUES
('entry', 'Entry Gate', 'بوابة دخول', 'Entry only gate', '🚪➡️', '#2ecc71', 'entry', 1, true),
('exit', 'Exit Gate', 'بوابة خروج', 'Exit only gate', '🚪⬅️', '#e74c3c', 'exit', 2, true),
('both', 'Two-Way Gate', 'بوابة ثنائية', 'Entry and exit', '🚪↔️', '#3498db', 'both', 3, true),
('vip', 'VIP Gate', 'بوابة VIP', 'VIP exclusive', '👑🚪', '#FFD700', 'both', 4, true),
('staff', 'Staff Gate', 'بوابة الموظفين', 'Staff only', '👔🚪', '#95a5a6', 'both', 5, true),
('emergency', 'Emergency Exit', 'مخرج طوارئ', 'Emergency use only', '🚨🚪', '#c0392b', 'exit', 6, true);
```

---

### 1.4 Visitor Types - أنواع الزوار
**Table:** `event_visitor_types`

| Column | Type | Description (EN) | الوصف (AR) |
|--------|------|------------------|-----------|
| TypeId | long (PK) | Type ID | معرف النوع |
| TypeCode | string(50) | Unique type code | رمز النوع |
| TypeName | string(100) | Type name (English) | اسم النوع بالإنجليزية |
| TypeNameAr | string(100) | Type name (Arabic) | اسم النوع بالعربية |
| Description | string(500) | Description | الوصف |
| Icon | string(100) | Icon | الأيقونة |
| ColorCode | string(20) | Color code | رمز اللون |
| BadgeColor | string(20) | Badge color | لون البطاقة |
| HasPriorityAccess | bool | Priority access | أولوية الدخول |
| RequiresApproval | bool | Requires approval | يتطلب موافقة |
| DisplayOrder | int | Display order | ترتيب العرض |
| IsActive | bool | Active status | حالة التفعيل |

**Sample Data / بيانات نموذجية:**
```sql
INSERT INTO event_visitor_types VALUES
('regular', 'Regular Visitor', 'زائر عادي', 'Standard visitor', '👤', '#3498db', '#FFFFFF', false, false, 1, true),
('vip', 'VIP Guest', 'ضيف VIP', 'VIP guest', '⭐', '#FFD700', '#FFD700', true, false, 2, true),
('speaker', 'Speaker', 'متحدث', 'Event speaker', '🎤', '#e74c3c', '#FF6B6B', true, true, 3, true),
('media', 'Media/Press', 'إعلامي', 'Media representative', '📰', '#9b59b6', '#CE93D8', true, true, 4, true),
('exhibitor', 'Exhibitor', 'عارض', 'Booth exhibitor', '🏪', '#f39c12', '#FFB74D', true, false, 5, true),
('sponsor', 'Sponsor', 'راعي', 'Event sponsor', '💎', '#1abc9c', '#4DB6AC', true, true, 6, true),
('student', 'Student', 'طالب', 'Student visitor', '🎓', '#16a085', '#81C784', false, false, 7, true);
```

---

### 1.5 Ticket Types - أنواع التذاكر
**Table:** `event_ticket_types`

| Column | Type | Description (EN) | الوصف (AR) |
|--------|------|------------------|-----------|
| TypeId | long (PK) | Type ID | معرف النوع |
| TypeCode | string(50) | Unique type code | رمز النوع |
| TypeName | string(100) | Type name (English) | اسم النوع بالإنجليزية |
| TypeNameAr | string(100) | Type name (Arabic) | اسم النوع بالعربية |
| Description | string(500) | Description | الوصف |
| DefaultPrice | decimal | Default price | السعر الافتراضي |
| Currency | string(10) | Currency | العملة |
| Icon | string(100) | Icon | الأيقونة |
| ColorCode | string(20) | Color code | رمز اللون |
| IsTransferable | bool | Can be transferred | قابل للتحويل |
| IsRefundable | bool | Can be refunded | قابل للاسترجاع |
| MaxQuantityPerPerson | int | Max per person | الحد الأقصى للشخص |
| DisplayOrder | int | Display order | ترتيب العرض |
| IsActive | bool | Active status | حالة التفعيل |

**Sample Data / بيانات نموذجية:**
```sql
INSERT INTO event_ticket_types VALUES
('free', 'Free Entry', 'دخول مجاني', 'Free admission', 0.00, 'SAR', '🎫', '#2ecc71', false, false, 1, 1, true),
('general', 'General Admission', 'دخول عام', 'Standard ticket', 100.00, 'SAR', '🎟️', '#3498db', true, true, 5, 2, true),
('vip', 'VIP Ticket', 'تذكرة VIP', 'VIP access', 500.00, 'SAR', '⭐', '#FFD700', false, false, 2, 3, true),
('premium', 'Premium Pass', 'تذكرة بريميوم', 'Premium features', 300.00, 'SAR', '💎', '#9b59b6', true, true, 3, 4, true),
('early_bird', 'Early Bird', 'الحجز المبكر', 'Discount price', 80.00, 'SAR', '🐦', '#f39c12', true, true, 10, 5, true),
('group', 'Group Ticket', 'تذكرة مجموعة', 'Group discount', 400.00, 'SAR', '👥', '#1abc9c', true, true, 10, 6, true);
```

---

### 1.6 Permit Types - أنواع التصاريح
**Table:** `event_permit_types`

| Column | Type | Description (EN) | الوصف (AR) |
|--------|------|------------------|-----------|
| TypeId | long (PK) | Type ID | معرف النوع |
| TypeCode | string(50) | Unique type code | رمز النوع |
| TypeName | string(100) | Type name (English) | اسم النوع بالإنجليزية |
| TypeNameAr | string(100) | Type name (Arabic) | اسم النوع بالعربية |
| Description | string(500) | Description | الوصف |
| Icon | string(100) | Icon | الأيقونة |
| ColorCode | string(20) | Color code | رمز اللون |
| RequiresApproval | bool | Needs approval | يتطلب موافقة |
| ApprovalLevel | string(50) | Approval level | مستوى الموافقة |
| DefaultValidityDays | int | Default validity | أيام الصلاحية |
| RequiresVehicleInfo | bool | Vehicle info needed | يتطلب معلومات مركبة |
| RequiresPhoto | bool | Photo required | يتطلب صورة |
| DisplayOrder | int | Display order | ترتيب العرض |
| IsActive | bool | Active status | حالة التفعيل |

**Sample Data / بيانات نموذجية:**
```sql
INSERT INTO event_permit_types VALUES
('staff', 'Staff Permit', 'تصريح موظف', 'Event staff', '👔', '#3498db', true, 'supervisor', 30, false, true, 1, true),
('vendor', 'Vendor Permit', 'تصريح مورد', 'Service vendor', '🛠️', '#f39c12', true, 'manager', 7, false, true, 2, true),
('contractor', 'Contractor Pass', 'تصريح مقاول', 'Construction contractor', '⚒️', '#e67e22', true, 'manager', 15, false, true, 3, true),
('vehicle', 'Vehicle Permit', 'تصريح مركبة', 'Vehicle access', '🚗', '#95a5a6', true, 'security', 7, true, false, 4, true),
('media', 'Media Pass', 'تصريح إعلامي', 'Press/media', '📰', '#9b59b6', true, 'pr_manager', 3, false, true, 5, true),
('vip', 'VIP Access', 'تصريح VIP', 'VIP backstage', '⭐', '#FFD700', true, 'director', 30, false, true, 6, true),
('emergency', 'Emergency Services', 'خدمات طوارئ', 'Emergency personnel', '🚑', '#e74c3c', false, 'auto', 90, true, true, 7, true);
```

---

### 1.7 Booth Types - أنواع الأكشاك
**Table:** `event_booth_types`

| Column | Type | Description (EN) | الوصف (AR) |
|--------|------|------------------|-----------|
| TypeId | long (PK) | Type ID | معرف النوع |
| TypeCode | string(50) | Unique type code | رمز النوع |
| TypeName | string(100) | Type name (English) | اسم النوع بالإنجليزية |
| TypeNameAr | string(100) | Type name (Arabic) | اسم النوع بالعربية |
| Description | string(500) | Description | الوصف |
| Icon | string(100) | Icon | الأيقونة |
| ColorCode | string(20) | Color code | رمز اللون |
| DefaultPricePerSqm | decimal | Price per sqm | السعر للمتر المربع |
| MinSizeSqm | decimal | Min size | الحد الأدنى للمساحة |
| MaxSizeSqm | decimal | Max size | الحد الأقصى للمساحة |
| IncludesElectricity | bool | Has electricity | يشمل كهرباء |
| IncludesInternet | bool | Has internet | يشمل إنترنت |
| IncludesFurniture | bool | Has furniture | يشمل أثاث |
| DisplayOrder | int | Display order | ترتيب العرض |
| IsActive | bool | Active status | حالة التفعيل |

**Sample Data / بيانات نموذجية:**
```sql
INSERT INTO event_booth_types VALUES
('standard', 'Standard Booth', 'كشك قياسي', 'Basic booth', '🏪', '#3498db', 500.00, 9.0, 20.0, true, false, false, 1, true),
('premium', 'Premium Booth', 'كشك بريميوم', 'Premium features', '⭐', '#FFD700', 800.00, 15.0, 40.0, true, true, true, 2, true),
('food', 'Food Stall', 'كشك طعام', 'Food service', '🍽️', '#e67e22', 600.00, 12.0, 30.0, true, false, true, 3, true),
('retail', 'Retail Shop', 'محل بيع', 'Retail store', '🛍️', '#9b59b6', 700.00, 15.0, 50.0, true, true, true, 4, true),
('exhibition', 'Exhibition Stand', 'منصة عرض', 'Display stand', '🖼️', '#2ecc71', 1000.00, 20.0, 100.0, true, true, false, 5, true),
('kiosk', 'Small Kiosk', 'كشك صغير', 'Compact kiosk', '📦', '#1abc9c', 300.00, 4.0, 9.0, true, false, false, 6, true);
```

---

### 1.8 Incident Types - أنواع الحوادث
**Table:** `event_incident_types`

| Column | Type | Description (EN) | الوصف (AR) |
|--------|------|------------------|-----------|
| TypeId | long (PK) | Type ID | معرف النوع |
| TypeCode | string(50) | Unique type code | رمز النوع |
| TypeName | string(100) | Type name (English) | اسم النوع بالإنجليزية |
| TypeNameAr | string(100) | Type name (Arabic) | اسم النوع بالعربية |
| Description | string(500) | Description | الوصف |
| Icon | string(100) | Icon | الأيقونة |
| ColorCode | string(20) | Color code | رمز اللون |
| DefaultPriority | string(20) | Default priority | الأولوية الافتراضية |
| DefaultSeverity | string(20) | Default severity | الخطورة الافتراضية |
| AssignedTeam | string(100) | Responsible team | الفريق المسؤول |
| SLAResponseMinutes | int | Response SLA | وقت الاستجابة |
| SLAResolutionMinutes | int | Resolution SLA | وقت الحل |
| RequiresImmediateAction | bool | Immediate action | إجراء فوري |
| DisplayOrder | int | Display order | ترتيب العرض |
| IsActive | bool | Active status | حالة التفعيل |

**Sample Data / بيانات نموذجية:**
```sql
INSERT INTO event_incident_types VALUES
('security', 'Security Issue', 'مشكلة أمنية', 'Security incident', '🔒', '#e74c3c', 'high', 'major', 'Security Team', 5, 30, true, 1, true),
('medical', 'Medical Emergency', 'طوارئ طبية', 'Medical issue', '🏥', '#c0392b', 'critical', 'critical', 'Medical Team', 2, 10, true, 2, true),
('technical', 'Technical Issue', 'مشكلة تقنية', 'Tech problem', '⚙️', '#3498db', 'medium', 'moderate', 'IT Support', 15, 60, false, 3, true),
('maintenance', 'Maintenance Request', 'طلب صيانة', 'Maintenance needed', '🔧', '#f39c12', 'low', 'minor', 'Maintenance', 30, 120, false, 4, true),
('complaint', 'Visitor Complaint', 'شكوى زائر', 'Customer complaint', '📝', '#95a5a6', 'medium', 'moderate', 'Customer Service', 10, 120, false, 5, true),
('fire', 'Fire Hazard', 'خطر حريق', 'Fire emergency', '🔥', '#e74c3c', 'critical', 'critical', 'Fire Safety', 1, 5, true, 6, true),
('crowd', 'Crowd Control', 'إدارة الحشود', 'Crowd issue', '👥', '#9b59b6', 'high', 'major', 'Security Team', 5, 15, true, 7, true),
('lost_found', 'Lost & Found', 'مفقودات', 'Lost item', '🔍', '#1abc9c', 'low', 'minor', 'Information Desk', 30, 180, false, 8, true);
```

---

### 1.9 Payment Methods - طرق الدفع
**Table:** `event_payment_methods`

| Column | Type | Description (EN) | الوصف (AR) |
|--------|------|------------------|-----------|
| MethodId | long (PK) | Method ID | معرف الطريقة |
| MethodCode | string(50) | Unique method code | رمز الطريقة |
| MethodName | string(100) | Method name (English) | اسم الطريقة بالإنجليزية |
| MethodNameAr | string(100) | Method name (Arabic) | اسم الطريقة بالعربية |
| Description | string(500) | Description | الوصف |
| Icon | string(100) | Icon | الأيقونة |
| ColorCode | string(20) | Color code | رمز اللون |
| PaymentGateway | string(50) | Gateway name | اسم البوابة |
| GatewayConfig | string(2000) | Gateway config (JSON) | إعدادات البوابة |
| TransactionFeePercentage | decimal | Fee percentage | نسبة الرسوم |
| FixedTransactionFee | decimal | Fixed fee | رسوم ثابتة |
| IsOnlinePayment | bool | Is online | دفع أونلاين |
| RequiresVerification | bool | Needs verification | يتطلب تحقق |
| DisplayOrder | int | Display order | ترتيب العرض |
| IsActive | bool | Active status | حالة التفعيل |

**Sample Data / بيانات نموذجية:**
```sql
INSERT INTO event_payment_methods VALUES
('cash', 'Cash', 'نقدي', 'Cash payment', '💵', '#2ecc71', null, null, 0.00, 0.00, false, false, 1, true),
('card', 'Credit/Debit Card', 'بطاقة', 'Card payment', '💳', '#3498db', 'Paymob', '{}', 2.75, 0.00, true, true, 2, true),
('mada', 'Mada', 'مدى', 'Mada card', '💳', '#00A651', 'Paymob', '{}', 2.00, 0.00, true, true, 3, true),
('bank_transfer', 'Bank Transfer', 'تحويل بنكي', 'Bank transfer', '🏦', '#9b59b6', null, null, 0.00, 5.00, false, true, 4, true),
('apple_pay', 'Apple Pay', 'آبل باي', 'Apple Pay', '🍎', '#000000', 'Paymob', '{}', 2.75, 0.00, true, true, 5, true),
('stc_pay', 'STC Pay', 'اس تي سي باي', 'STC mobile payment', '📱', '#6e1a5d', 'STC', '{}', 2.50, 0.00, true, true, 6, true);
```

---

## 📋 Section 2: Main Entity Tables - الجداول الرئيسية

### 2.1 Events - الفعاليات
**Table:** `event_events`

**Purpose:** Store main event information / تخزين معلومات الفعاليات الرئيسية

| Column | Type | Nullable | Description |
|--------|------|----------|-------------|
| EventId | long (PK) | No | Unique event ID |
| EventCode | string(50) | No | Event code |
| EventName | string(200) | No | Event name (English) |
| EventNameAr | string(200) | Yes | Event name (Arabic) |
| Description | string(2000) | Yes | Description (English) |
| DescriptionAr | string(2000) | Yes | Description (Arabic) |
| EventType | string(50) | Yes | Event type (from event_types) |
| Status | string(20) | No | Status (planned, active, completed, cancelled) |
| StartDate | DateTime | No | Start date |
| EndDate | DateTime | No | End date |
| RegistrationStartDate | DateTime | Yes | Registration start |
| RegistrationEndDate | DateTime | Yes | Registration end |
| LocationId | long | Yes | Location reference |
| LocationName | string(200) | Yes | Location name |
| Address | string(500) | Yes | Full address |
| City | string(100) | Yes | City |
| Latitude | decimal | Yes | GPS latitude |
| Longitude | decimal | Yes | GPS longitude |
| MapImageUrl | string(500) | Yes | Map image URL |
| ExpectedVisitors | int | Yes | Expected visitor count |
| MaxCapacity | int | Yes | Maximum capacity |
| CurrentOccupancy | int | No | Current occupancy (computed) |
| Organizer | string(200) | Yes | Organizer name |
| ContactEmail | string(100) | Yes | Contact email |
| ContactPhone | string(20) | Yes | Contact phone |
| Website | string(200) | Yes | Website URL |
| LogoUrl | string(500) | Yes | Logo URL |
| BannerImageUrl | string(500) | Yes | Banner image URL |
| IsPublic | bool | No | Public event |
| RequiresRegistration | bool | No | Registration required |
| IsPaidEvent | bool | No | Paid event |
| TicketPrice | decimal | Yes | Ticket price |
| Currency | string(10) | Yes | Currency code |
| EventManagerId | long | Yes | Event manager ID |
| Tags | string(500) | Yes | Tags (comma-separated) |
| IsActive | bool | No | Active status |
| + SysBase fields | - | - | Audit fields |

---

### 2.2 Event Zones - المناطق
**Table:** `event_zones`

**Purpose:** Define zones/areas within an event / تحديد المناطق داخل الفعالية

| Column | Type | Description |
|--------|------|-------------|
| ZoneId | long (PK) | Zone ID |
| EventId | long (FK) | Event reference |
| ZoneCode | string(50) | Zone code |
| ZoneName | string(200) | Zone name (English) |
| ZoneNameAr | string(200) | Zone name (Arabic) |
| Description | string(1000) | Description |
| ZoneType | string(50) | Zone type (from zone_types) |
| ParentZoneId | long | Parent zone (for sub-zones) |
| MaxCapacity | int | Maximum capacity |
| CurrentOccupancy | int | Current occupancy |
| AreaSqm | decimal | Area in square meters |
| FloorNumber | int | Floor number |
| BuildingName | string(100) | Building name |
| MapCoordinates | string(2000) | Map coordinates (JSON) |
| ColorCode | string(20) | Display color |
| AccessLevel | string(20) | Access level (public, restricted, vip) |
| ZoneManagerId | long | Zone manager ID |
| Status | string(20) | Status |
| DisplayOrder | int | Display order |
| IsActive | bool | Active status |
| + SysBase fields | - | Audit fields |

---

### 2.3 Event Gates - البوابات
**Table:** `event_gates`

**Purpose:** Manage entry/exit gates / إدارة بوابات الدخول والخروج

| Column | Type | Description |
|--------|------|-------------|
| GateId | long (PK) | Gate ID |
| EventId | long (FK) | Event reference |
| ZoneId | long (FK) | Zone reference (optional) |
| GateCode | string(50) | Gate code |
| GateName | string(200) | Gate name (English) |
| GateNameAr | string(200) | Gate name (Arabic) |
| Description | string(1000) | Description |
| GateType | string(20) | Gate type (from gate_types) |
| Location | string(200) | Location description |
| Latitude | decimal | GPS latitude |
| Longitude | decimal | GPS longitude |
| MaxCapacityPerHour | int | Hourly capacity |
| CurrentQueueCount | int | Current queue |
| AvgWaitTimeMinutes | int | Average wait time |
| Status | string(20) | Status (open, closed, maintenance, full) |
| OpeningTime | TimeSpan | Opening time |
| ClosingTime | TimeSpan | Closing time |
| HasScanner | bool | Has QR scanner |
| ScannerDeviceId | string(100) | Scanner device ID |
| GateManagerId | long | Gate manager ID |
| DisplayOrder | int | Display order |
| IsActive | bool | Active status |
| LastStatusUpdate | DateTime | Last status update |
| + SysBase fields | - | Audit fields |

---

### 2.4 Event Visitors - الزوار
**Table:** `event_visitors`

**Purpose:** Store visitor information / تخزين معلومات الزوار

| Column | Type | Description |
|--------|------|-------------|
| VisitorId | long (PK) | Visitor ID |
| EventId | long (FK) | Event reference |
| UserId | long | User account ID (optional) |
| VisitorCode | string(50) | Visitor code |
| FirstName | string(100) | First name |
| LastName | string(100) | Last name |
| FullNameAr | string(200) | Full name (Arabic) |
| Email | string(100) | Email |
| Phone | string(20) | Phone |
| NationalId | string(50) | National ID / Iqama |
| PassportNumber | string(50) | Passport number |
| Nationality | string(50) | Nationality |
| Gender | string(10) | Gender |
| DateOfBirth | DateTime | Date of birth |
| CompanyName | string(200) | Company name |
| JobTitle | string(100) | Job title |
| VisitorType | string(20) | Visitor type (from visitor_types) |
| RegistrationDate | DateTime | Registration date |
| RegistrationSource | string(20) | Registration source |
| QRCode | string(200) | QR code |
| BadgeNumber | string(50) | Badge number |
| BadgePrinted | bool | Badge printed |
| BadgePrintedDate | DateTime | Badge print date |
| ProfilePictureUrl | string(500) | Profile picture URL |
| Status | string(20) | Status (registered, checked-in, checked-out, cancelled) |
| CheckInTime | DateTime | Check-in time |
| CheckOutTime | DateTime | Check-out time |
| CheckInGateId | long | Check-in gate ID |
| EmergencyContactName | string(100) | Emergency contact |
| EmergencyContactPhone | string(20) | Emergency phone |
| SpecialNeeds | string(500) | Special needs |
| Notes | string(1000) | Notes |
| IsVIP | bool | VIP status |
| IsActive | bool | Active status |
| + SysBase fields | - | Audit fields |

---

### 2.5 Event Tickets - التذاكر
**Table:** `event_tickets`

**Purpose:** Manage event tickets / إدارة تذاكر الفعاليات

| Column | Type | Description |
|--------|------|-------------|
| TicketId | long (PK) | Ticket ID |
| EventId | long (FK) | Event reference |
| VisitorId | long (FK) | Visitor reference |
| TicketNumber | string(50) | Ticket number |
| TicketType | string(50) | Ticket type (from ticket_types) |
| TicketCategory | string(100) | Ticket category |
| Price | decimal | Price |
| Currency | string(10) | Currency |
| PurchaseDate | DateTime | Purchase date |
| ValidFrom | DateTime | Valid from |
| ValidUntil | DateTime | Valid until |
| QRCode | string(200) | QR code |
| Barcode | string(100) | Barcode |
| Status | string(20) | Status (active, used, cancelled, expired) |
| IsPaid | bool | Payment status |
| PaymentMethod | string(50) | Payment method |
| PaymentReference | string(100) | Payment reference |
| UsedDate | DateTime | Used date |
| UsedGateId | long | Gate used at |
| SeatNumber | string(50) | Seat number |
| ZoneAccess | string(500) | Zone access (JSON) |
| CanBeTransferred | bool | Transferable |
| TransferredToVisitorId | long | Transferred to |
| TransferDate | DateTime | Transfer date |
| Notes | string(1000) | Notes |
| IsActive | bool | Active status |
| + SysBase fields | - | Audit fields |

---

### 2.6 Event Permits - التصاريح
**Table:** `event_permits`

**Purpose:** Manage access permits / إدارة تصاريح الدخول

| Column | Type | Description |
|--------|------|-------------|
| PermitId | long (PK) | Permit ID |
| EventId | long (FK) | Event reference |
| EmployeeId | long | Employee reference |
| VisitorId | long | Visitor reference |
| PermitNumber | string(50) | Permit number |
| PermitType | string(50) | Permit type (from permit_types) |
| PermitCategory | string(100) | Permit category |
| ApplicantName | string(200) | Applicant name (English) |
| ApplicantNameAr | string(200) | Applicant name (Arabic) |
| CompanyName | string(200) | Company name |
| NationalId | string(50) | National ID |
| ContactPhone | string(20) | Contact phone |
| Email | string(100) | Email |
| ValidFrom | DateTime | Valid from |
| ValidUntil | DateTime | Valid until |
| IssueDate | DateTime | Issue date |
| IssuedBy | string(100) | Issued by |
| QRCode | string(200) | QR code |
| ZoneAccess | string(1000) | Zone access (JSON) |
| GateAccess | string(1000) | Gate access (JSON) |
| AccessLevel | string(20) | Access level |
| Purpose | string(500) | Purpose |
| VehiclePlateNumber | string(20) | Vehicle plate |
| VehicleType | string(50) | Vehicle type |
| PhotoUrl | string(500) | Photo URL |
| AttachmentUrl | string(500) | Attachment URL |
| Status | string(20) | Status (pending, approved, rejected, active, expired, cancelled) |
| ApprovedBy | string(100) | Approved by |
| ApprovalDate | DateTime | Approval date |
| RejectionReason | string(500) | Rejection reason |
| Notes | string(1000) | Notes |
| IsActive | bool | Active status |
| + SysBase fields | - | Audit fields |

---

### 2.7 Event Booths - الأكشاك
**Table:** `event_booths`

**Purpose:** Manage booth/stall rentals / إدارة الأكشاك المؤجرة

| Column | Type | Description |
|--------|------|-------------|
| BoothId | long (PK) | Booth ID |
| EventId | long (FK) | Event reference |
| ZoneId | long (FK) | Zone reference |
| BoothNumber | string(50) | Booth number |
| BoothName | string(200) | Booth name (English) |
| BoothNameAr | string(200) | Booth name (Arabic) |
| Description | string(1000) | Description |
| BoothType | string(50) | Booth type (from booth_types) |
| BoothCategory | string(100) | Booth category |
| SizeSqm | decimal | Size in sqm |
| LocationDescription | string(200) | Location |
| FloorNumber | int | Floor number |
| MapCoordinates | string(500) | Map coordinates |
| RentalPrice | decimal | Rental price |
| Currency | string(10) | Currency |
| RentalPeriodType | string(20) | Period type |
| TenantId | long | Tenant ID |
| ContractId | long | Contract ID |
| RentalStatus | string(20) | Status (available, rented, reserved, maintenance) |
| HasElectricity | bool | Has electricity |
| HasWater | bool | Has water |
| HasInternet | bool | Has internet |
| FurnitureIncluded | bool | Includes furniture |
| FurnitureDescription | string(500) | Furniture details |
| AdditionalAmenities | string(1000) | Additional amenities |
| SetupDate | DateTime | Setup date |
| TeardownDate | DateTime | Teardown date |
| FloorPlanImageUrl | string(500) | Floor plan image |
| Notes | string(1000) | Notes |
| DisplayOrder | int | Display order |
| IsActive | bool | Active status |
| + SysBase fields | - | Audit fields |

---

### 2.8 Event Tenants - المستأجرون
**Table:** `event_tenants`

**Purpose:** Manage booth tenants / إدارة مستأجري الأكشاك

| Column | Type | Description |
|--------|------|-------------|
| TenantId | long (PK) | Tenant ID |
| EventId | long (FK) | Event reference |
| TenantCode | string(50) | Tenant code |
| CompanyName | string(200) | Company name (English) |
| CompanyNameAr | string(200) | Company name (Arabic) |
| CommercialRegNumber | string(50) | Commercial registration |
| TaxNumber | string(50) | Tax number |
| BusinessType | string(100) | Business type |
| ContactPersonName | string(200) | Contact person |
| ContactPosition | string(100) | Contact position |
| Email | string(100) | Email |
| Phone | string(20) | Phone |
| Mobile | string(20) | Mobile |
| Address | string(500) | Address |
| City | string(100) | City |
| Country | string(100) | Country |
| Website | string(200) | Website |
| LogoUrl | string(500) | Logo URL |
| BankName | string(100) | Bank name |
| AccountNumber | string(50) | Account number |
| IBAN | string(50) | IBAN |
| RegistrationDate | DateTime | Registration date |
| Status | string(20) | Status (active, inactive, blacklisted, pending) |
| Rating | decimal | Rating |
| TotalBoothsRented | int | Total booths (computed) |
| Notes | string(1000) | Notes |
| IsActive | bool | Active status |
| + SysBase fields | - | Audit fields |

---

### 2.9 Event Contracts - العقود
**Table:** `event_contracts`

**Purpose:** Manage rental contracts / إدارة عقود الإيجار

| Column | Type | Description |
|--------|------|-------------|
| ContractId | long (PK) | Contract ID |
| EventId | long (FK) | Event reference |
| TenantId | long (FK) | Tenant reference |
| BoothId | long (FK) | Booth reference |
| ContractNumber | string(50) | Contract number |
| ContractDate | DateTime | Contract date |
| StartDate | DateTime | Start date |
| EndDate | DateTime | End date |
| TotalAmount | decimal | Total amount |
| Currency | string(10) | Currency |
| VATAmount | decimal | VAT amount |
| VATPercentage | decimal | VAT percentage |
| DepositAmount | decimal | Deposit amount |
| PaymentTerms | string(500) | Payment terms |
| PaymentMethod | string(50) | Payment method |
| PaymentSchedule | string(2000) | Payment schedule (JSON) |
| Status | string(20) | Status (draft, pending, active, completed, cancelled, terminated) |
| SignedDate | DateTime | Signed date |
| SignedByTenant | bool | Tenant signature |
| SignedByOrganizer | bool | Organizer signature |
| ContractFileUrl | string(500) | Contract file URL |
| TermsAndConditions | string(4000) | Terms and conditions |
| SpecialConditions | string(2000) | Special conditions |
| AutoRenewal | bool | Auto renewal |
| RenewalDate | DateTime | Renewal date |
| CancellationReason | string(500) | Cancellation reason |
| CancelledBy | string(100) | Cancelled by |
| CancellationDate | DateTime | Cancellation date |
| Notes | string(1000) | Notes |
| IsActive | bool | Active status |
| + SysBase fields | - | Audit fields |

---

### 2.10 Event Incidents - الحوادث
**Table:** `event_incidents`

**Purpose:** Manage incidents and support tickets / إدارة الحوادث والبلاغات

| Column | Type | Description |
|--------|------|-------------|
| IncidentId | long (PK) | Incident ID |
| EventId | long (FK) | Event reference |
| IncidentNumber | string(50) | Incident number |
| IncidentType | string(50) | Incident type (from incident_types) |
| IncidentCategory | string(100) | Incident category |
| Priority | string(20) | Priority (low, medium, high, critical) |
| Severity | string(20) | Severity (minor, moderate, major, critical) |
| Title | string(200) | Title |
| Description | string(2000) | Description |
| Location | string(200) | Location |
| ZoneId | long | Zone reference |
| BoothId | long | Booth reference |
| ReportedDate | DateTime | Reported date |
| ReportedBy | string(100) | Reported by |
| ReporterType | string(50) | Reporter type |
| ReporterContact | string(100) | Reporter contact |
| Status | string(20) | Status (reported, assigned, in-progress, resolved, closed, escalated) |
| AssignedToId | long | Assigned to |
| AssignedDate | DateTime | Assigned date |
| AssignedTeam | string(100) | Assigned team |
| ResponseTimeMinutes | int | Response time |
| ResolutionTimeMinutes | int | Resolution time |
| ResolutionDate | DateTime | Resolution date |
| ResolutionNotes | string(2000) | Resolution notes |
| Escalated | bool | Escalated |
| EscalatedTo | string(100) | Escalated to |
| EscalationDate | DateTime | Escalation date |
| EscalationReason | string(500) | Escalation reason |
| Attachments | string(2000) | Attachments (JSON) |
| WitnessInformation | string(1000) | Witness info |
| FollowUpRequired | bool | Follow-up required |
| FollowUpDate | DateTime | Follow-up date |
| ClosedDate | DateTime | Closed date |
| ClosedBy | string(100) | Closed by |
| SatisfactionRating | int | Customer satisfaction (1-5) |
| Notes | string(2000) | Notes |
| IsActive | bool | Active status |
| + SysBase fields | - | Audit fields |

---

### 2.11 Event Payments - الدفعات
**Table:** `event_payments`

**Purpose:** Track all payments / تتبع جميع الدفعات

| Column | Type | Description |
|--------|------|-------------|
| PaymentId | long (PK) | Payment ID |
| EventId | long (FK) | Event reference |
| ContractId | long | Contract reference |
| InvoiceId | long | Invoice reference |
| TenantId | long | Tenant reference |
| VisitorId | long | Visitor reference |
| PaymentNumber | string(50) | Payment number |
| PaymentType | string(50) | Payment type |
| PaymentDate | DateTime | Payment date |
| Amount | decimal | Amount |
| Currency | string(10) | Currency |
| VATAmount | decimal | VAT amount |
| TotalAmount | decimal | Total amount |
| PaymentMethod | string(50) | Payment method (from payment_methods) |
| CardType | string(50) | Card type |
| CardLast4 | string(4) | Last 4 digits |
| TransactionReference | string(100) | Transaction reference |
| BankReference | string(100) | Bank reference |
| PaymentGateway | string(50) | Payment gateway |
| GatewayResponse | string(2000) | Gateway response |
| Status | string(20) | Status (pending, completed, failed, refunded, cancelled) |
| PayerName | string(200) | Payer name |
| PayerEmail | string(100) | Payer email |
| PayerPhone | string(20) | Payer phone |
| ReceiptNumber | string(50) | Receipt number |
| ReceiptUrl | string(500) | Receipt URL |
| RefundAmount | decimal | Refund amount |
| RefundDate | DateTime | Refund date |
| RefundReason | string(500) | Refund reason |
| ProcessedBy | string(100) | Processed by |
| Notes | string(1000) | Notes |
| IsActive | bool | Active status |
| + SysBase fields | - | Audit fields |

---

### 2.12 Event Invoices - الفواتير
**Table:** `event_invoices`

**Purpose:** Manage invoices / إدارة الفواتير

| Column | Type | Description |
|--------|------|-------------|
| InvoiceId | long (PK) | Invoice ID |
| EventId | long (FK) | Event reference |
| ContractId | long | Contract reference |
| TenantId | long | Tenant reference |
| InvoiceNumber | string(50) | Invoice number |
| InvoiceDate | DateTime | Invoice date |
| DueDate | DateTime | Due date |
| InvoiceType | string(50) | Invoice type |
| Description | string(500) | Description |
| SubtotalAmount | decimal | Subtotal |
| VATPercentage | decimal | VAT percentage |
| VATAmount | decimal | VAT amount |
| DiscountAmount | decimal | Discount |
| TotalAmount | decimal | Total amount |
| PaidAmount | decimal | Paid amount |
| Balance | decimal | Balance |
| Currency | string(10) | Currency |
| Status | string(20) | Status (draft, issued, partially_paid, paid, overdue, cancelled) |
| BillingAddress | string(500) | Billing address |
| InvoiceItems | string(4000) | Invoice items (JSON) |
| PaymentTerms | string(500) | Payment terms |
| InvoiceFileUrl | string(500) | Invoice file URL |
| SentDate | DateTime | Sent date |
| SentToEmail | string(100) | Sent to email |
| PaymentDate | DateTime | Payment date |
| LateFeeAmount | decimal | Late fee |
| DaysOverdue | int | Days overdue (computed) |
| CancelledDate | DateTime | Cancelled date |
| CancellationReason | string(500) | Cancellation reason |
| Notes | string(1000) | Notes |
| IsActive | bool | Active status |
| + SysBase fields | - | Audit fields |

---

## 🔗 Entity Relationships - العلاقات بين الجداول

```
Event (1) ──┬─── (M) EventZones
            ├─── (M) EventGates
            ├─── (M) EventVisitors ──── (M) EventTickets
            ├─── (M) EventTenants ──┬─── (M) EventBooths
            │                        ├─── (M) EventContracts
            │                        ├─── (M) EventInvoices
            │                        └─── (M) EventPayments
            ├─── (M) EventIncidents
            └─── (M) EventPermits

EventZone (1) ──┬─── (M) EventBooths
                └─── (M) EventGates

EventTypes ──── (M) Events
ZoneTypes ──── (M) EventZones
GateTypes ──── (M) EventGates
VisitorTypes ──── (M) EventVisitors
TicketTypes ──── (M) EventTickets
PermitTypes ──── (M) EventPermits
BoothTypes ──── (M) EventBooths
IncidentTypes ──── (M) EventIncidents
PaymentMethods ──── (M) EventPayments
```

---

## 📝 Section 3: Database Initialization

### تهيئة قاعدة البيانات

لإنشاء جميع الجداول تلقائياً، أضف الكود التالي في `Program.cs`:

```csharp
using ZR.Model.EventManagement;

// Initialize Event Management System tables
app.Environment.InitDbWithTypes(
    // Main Tables
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
    typeof(EventInvoice),

    // Lookup Tables
    typeof(EventType),
    typeof(ZoneType),
    typeof(GateType),
    typeof(VisitorType),
    typeof(TicketType),
    typeof(PermitType),
    typeof(BoothType),
    typeof(IncidentType),
    typeof(PaymentMethod)
);
```

---

## 🎯 Section 4: Sample Data Scripts

### نصوص البيانات النموذجية

راجع القسم الأول من هذا الملف لجميع نصوص INSERT لتعبئة جداول الأنواع.

---

## 🔒 Section 5: Indexes & Performance

### الفهارس الموصى بها

```sql
-- Events
CREATE INDEX idx_event_status ON event_events(Status);
CREATE INDEX idx_event_dates ON event_events(StartDate, EndDate);
CREATE INDEX idx_event_type ON event_events(EventType);

-- Visitors
CREATE INDEX idx_visitor_event ON event_visitors(EventId);
CREATE INDEX idx_visitor_status ON event_visitors(Status);
CREATE INDEX idx_visitor_qr ON event_visitors(QRCode);
CREATE INDEX idx_visitor_email ON event_visitors(Email);

-- Gates
CREATE INDEX idx_gate_event ON event_gates(EventId);
CREATE INDEX idx_gate_status ON event_gates(Status);

-- Incidents
CREATE INDEX idx_incident_event ON event_incidents(EventId);
CREATE INDEX idx_incident_status ON event_incidents(Status);
CREATE INDEX idx_incident_priority ON event_incidents(Priority);

-- Payments
CREATE INDEX idx_payment_event ON event_payments(EventId);
CREATE INDEX idx_payment_tenant ON event_payments(TenantId);
CREATE INDEX idx_payment_status ON event_payments(Status);

-- Invoices
CREATE INDEX idx_invoice_tenant ON event_invoices(TenantId);
CREATE INDEX idx_invoice_status ON event_invoices(Status);
CREATE INDEX idx_invoice_due ON event_invoices(DueDate);
```

---

## 📈 Section 6: Database Statistics

### إحصائيات قاعدة البيانات

| Category | Count |
|----------|-------|
| Total Tables | 21 |
| Lookup Tables | 8 |
| Main Entity Tables | 12 |
| Dashboard DTOs | 1 |
| Total Columns | ~600+ |
| Foreign Keys | ~30 |
| Indexes (Recommended) | ~20 |

---

## ✨ Key Features - المزايا الرئيسية

1. ✅ **Fully Normalized** - قاعدة بيانات منظمة بالكامل
2. ✅ **Bilingual Support** - دعم العربية والإنجليزية
3. ✅ **Flexible Type System** - نظام أنواع مرن وقابل للتخصيص
4. ✅ **Audit Trail** - تتبع كامل للتعديلات
5. ✅ **Multi-tenancy Ready** - جاهز لدعم متعدد المستأجرين
6. ✅ **Scalable Design** - تصميم قابل للتوسع
7. ✅ **Rich Metadata** - بيانات وصفية غنية
8. ✅ **Performance Optimized** - محسّن للأداء

---

## 📞 Support

للمزيد من المعلومات:
- **EVENT_MANAGEMENT_SYSTEM.md** - التوثيق الشامل
- **EVENT_MANAGEMENT_QUICKSTART.md** - دليل البداية السريعة

---

**تم التوثيق بحمد الله** ✨
**Database Schema Documentation Complete** 🎉
