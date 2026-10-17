# Event Management Tables Documentation

This document provides comprehensive documentation for all tables in the Event Management module.

## Table of Contents

1. [Core Tables](#core-tables)
2. [Lookup/Reference Tables](#lookupreference-tables)
3. [History/Audit Tables](#historyaudit-tables)
4. [Transaction Tables](#transaction-tables)

---

## Core Tables

### 1. event_events
**Table Name:** `event_events`  
**Class:** `Event`  
**Description:** Event Information - معلومات الفعالية

#### Fields

| Field Name | Type | Description |
|------------|------|-------------|
| Eventid | long (PK) | Event ID - معرف الفعالية |
| Eventcode | string | Event Code - رمز الفعالية |
| Eventname | string | Event Name (English) - اسم الفعالية بالإنجليزية |
| Eventnamear | string | Event Name (Arabic) - اسم الفعالية بالعربية |
| Description | string | Description (English) - الوصف بالإنجليزية |
| Descriptionar | string | Description (Arabic) - الوصف بالعربية |
| Eventtype | string | Event Type - نوع الفعالية |
| Status | string | Event Status - حالة الفعالية |
| Startdate | DateTime | Start Date - تاريخ البداية |
| Enddate | DateTime | End Date - تاريخ النهاية |
| Registrationstartdate | DateTime | Registration Start Date - تاريخ بداية التسجيل |
| Registrationenddate | DateTime | Registration End Date - تاريخ نهاية التسجيل |
| Locationid | long | Location ID - معرف الموقع |
| Locationname | string | Location Name - اسم الموقع |
| Address | string | Address - العنوان |
| City | string | City - المدينة |
| Latitude | decimal | Latitude - خط العرض |
| Longitude | decimal | Longitude - خط الطول |
| Mapimageurl | string | Map Image URL - رابط صورة الخريطة |
| Expectedvisitors | int | Expected Visitors - عدد الزوار المتوقع |
| Maxcapacity | int | Max Capacity - السعة القصوى |
| Currentoccupancy | int | Current Occupancy - العدد الحالي للحضور |
| Organizer | string | Organizer - المنظم |
| Contactemail | string | Contact Email - البريد الإلكتروني للتواصل |
| Contactphone | string | Contact Phone - هاتف التواصل |
| Website | string | Website - الموقع الإلكتروني |
| Logourl | string | Logo URL - رابط الشعار |
| Bannerimageurl | string | Banner Image URL - رابط صورة البانر |
| Ispublic | string | Is Public Event - هل الحدث عام |
| Requiresregistration | string | Requires Registration - يتطلب التسجيل |
| Ispaidevent | string | Is Paid Event - حدث مدفوع |
| Ticketprice | decimal | Ticket Price - سعر التذكرة |
| Currency | string | Currency - العملة |
| Eventmanagerid | long | Event Manager ID - معرف مدير الفعالية |
| Tags | string | Tags - الوسوم |
| Isactive | string | Is Active - نشط |
| Actualcost | decimal | Actual Cost - التكلفة الفعلية |
| Budget | decimal | Budget - الميزانية |
| Totalareasqm | decimal | Total Area (sqm) - إجمالي المساحة بالمتر المربع |
| Expectedattendance | decimal | Expected Attendance - الحضور المتوقع |
| Country | string | Country - الدولة |
| Venue | string | Venue - المكان |

---

### 2. event_booths
**Table Name:** `event_booths`  
**Class:** `EventBooth`  
**Description:** Event Booth/Stall - الأكشاك أو المحلات المؤجرة

#### Fields

| Field Name | Type | Description |
|------------|------|-------------|
| BoothId | long (PK) | Booth ID - معرف الكشك |
| BoothCode | string | Booth Code - رمز الكشك |
| EventId | long | Event ID - معرف الفعالية |
| ZoneId | long? | Zone ID - معرف المنطقة |
| BoothNumber | string | Booth Number - رقم الكشك |
| BoothName | string | Booth Name (English) - اسم الكشك بالإنجليزية |
| BoothNameAr | string | Booth Name (Arabic) - اسم الكشك بالعربية |
| Description | string | Description - الوصف |
| BoothType | string | Booth Type - نوع الكشك |
| BoothCategory | string | Booth Category - فئة الكشك |
| SizeSqm | decimal | Size (Square Meters) - المساحة بالمتر المربع |
| RentalPrice | decimal | Rental Price - سعر الإيجار |
| Currency | string | Currency - العملة |
| RentalPeriodType | string | Rental Period Type - نوع فترة الإيجار |
| TenantId | long? | Tenant ID - معرف المستأجر |
| ContractId | long? | Contract ID - معرف العقد |
| RentalStatus | string | Rental Status - حالة الإيجار |
| HasElectricity | bool | Has Electricity - يحتوي على كهرباء |
| HasWater | bool | Has Water - يحتوي على ماء |
| HasInternet | bool | Has Internet - يحتوي على إنترنت |
| FurnitureIncluded | bool | Furniture Included - يشمل الأثاث |
| FurnitureDescription | string | Furniture Description - وصف الأثاث |
| AdditionalAmenities | string | Additional Amenities - مرافق إضافية |
| SetupDate | DateTime? | Setup Date - تاريخ التجهيز |
| TeardownDate | DateTime? | Teardown Date - تاريخ التفكيك |
| FloorPlanImageUrl | string | Floor Plan Image URL - رابط صورة المخطط |
| DisplayOrder | int? | Display Order - ترتيب العرض |
| IsActive | bool | Is Active - نشط |

---

### 3. event_contracts
**Table Name:** `event_contracts`  
**Class:** `EventContract`  
**Description:** Event Contract - عقد الإيجار

#### Fields

| Field Name | Type | Description |
|------------|------|-------------|
| ContractId | long (PK) | Contract ID - معرف العقد |
| EventId | long | Event ID - معرف الفعالية |
| TenantId | long | Tenant ID - معرف المستأجر |
| BoothId | long | Booth ID - معرف الكشك |
| ContractNumber | string | Contract Number - رقم العقد |
| ContractDate | DateTime | Contract Date - تاريخ العقد |
| StartDate | DateTime | Start Date - تاريخ البداية |
| EndDate | DateTime | End Date - تاريخ النهاية |
| TotalAmount | decimal | Total Amount - المبلغ الإجمالي |
| Currency | string | Currency - العملة |
| VATAmount | decimal? | VAT Amount - قيمة الضريبة |
| VATPercentage | decimal? | VAT Percentage - نسبة الضريبة |
| DepositAmount | decimal? | Deposit Amount - قيمة التأمين |
| PaymentTerms | string | Payment Terms - شروط الدفع |
| PaymentMethod | string | Payment Method - طريقة الدفع |
| Status | string | Status - الحالة |
| SignedByTenant | bool | Signed By Tenant - تم التوقيع من المستأجر |
| SignedByOrganizer | bool | Signed By Organizer - تم التوقيع من المنظم |
| TermsAndConditions | string | Terms and Conditions - الشروط والأحكام |
| SpecialConditions | string | Special Conditions - شروط خاصة |
| Notes | string | Notes - ملاحظات |
| IsActive | bool | Is Active - نشط |
| TenantSignatureDate | DateTime? | Tenant Signature Date - تاريخ توقيع المستأجر |
| TenantSignedBy | long? | Tenant Signed By - تم التوقيع بواسطة (المستأجر) |
| OrganizerSignatureDate | DateTime? | Organizer Signature Date - تاريخ توقيع المنظم |
| OrganizerSignedBy | long? | Organizer Signed By - تم التوقيع بواسطة (المنظم) |
| RentalAmount | decimal | Rental Amount - قيمة الإيجار |
| SpaceArea | decimal | Space Area (m²) - مساحة الموقع |
| ContractType | string | Contract Type - نوع العقد |
| OrganizerCompanyName | string | Organizer Company Name - اسم الشركة المنظمة |
| OrganizerCompanyNameAr | string | Organizer Company Name (Arabic) - اسم الشركة المنظمة بالعربي |
| OrganizerCommercialRegNumber | string | Organizer Commercial Registration Number - السجل التجاري للمنظم |
| OrganizerRepresentativeName | string | Organizer Representative Name - اسم ممثل المنظم |
| OrganizerPhone | string | Organizer Phone - هاتف المنظم |
| OrganizerEmail | string | Organizer Email - بريد المنظم |
| OrganizerAddress | string | Organizer Address - عنوان المنظم |
| TenantCompanyNameAr | string | Tenant Company Name (Arabic) - اسم شركة المستأجر بالعربي |
| TenantCommercialRegNumber | string | Tenant Commercial Registration Number - السجل التجاري للمستأجر |
| TenantRepresentativeName | string | Tenant Representative Name - اسم الممثل القانوني |
| TenantNationalId | string | Tenant National ID - رقم الهوية |
| TenantAddress | string | Tenant Address - عنوان المستأجر |
| TenantAddressAr | string | Tenant Address (Arabic) - عنوان المستأجر بالعربي |
| TenantNationality | string | Tenant Nationality - جنسية المستأجر |
| UnitNumber | string | Unit Number - رقم الوحدة |
| ProjectLocation | string | Project Location - موقع المشروع |
| ProjectLocationAr | string | Project Location (Arabic) - موقع المشروع بالعربي |
| BusinessActivityType | string | Business Activity Type - طبيعة النشاط التجاري |
| BusinessActivityTypeAr | string | Business Activity Type (Arabic) - طبيعة النشاط التجاري بالعربي |
| BrandName | string | Brand Name - العلامة التجارية |
| BrandNameAr | string | Brand Name (Arabic) - العلامة التجارية بالعربي |
| RentalPricePerMeter | decimal? | Rental Price Per Meter - سعر الإيجار للمتر |
| OperationalPeriod | string | Operational Period - الفترة التشغيلية |
| OperationalPeriodAr | string | Operational Period (Arabic) - الفترة التشغيلية بالعربي |
| ContractDuration | string | Contract Duration - مدة الاتفاقية |
| ContractDurationAr | string | Contract Duration (Arabic) - مدة الاتفاقية بالعربي |
| DeliveryDate | DateTime? | Delivery Date - موعد التسليم |
| AdministrativeFees | decimal? | Administrative Fees - الرسوم الإدارية |
| WasteRemovalFees | decimal? | Waste Removal Fees - رسوم إزالة المخلفات |
| ElectricityFees | decimal? | Electricity Fees - قيمة الكهرباء |
| WaterFees | decimal? | Water Fees - قيمة المياه |
| POSServiceFees | decimal? | POS Service Fees - رسوم خدمات أنظمة البيع |
| EngineeringSupervisionFees | decimal? | Engineering Supervision Fees - الإشراف الهندسي |
| TotalContractValueIncludingVAT | decimal? | Total Contract Value Including VAT - القيمة الكلية للعقد شاملة الضريبة |
| EvacuationPeriodDays | int? | Evacuation Period (days) - مدة الإخلاء بالأيام |
| LateEvacuationPenaltyPerDay | decimal? | Late Evacuation Penalty (per day) - غرامة التأخير في الإخلاء لكل يوم |
| CancellationPenaltyPercentage | decimal? | Cancellation Penalty Percentage - نسبة غرامة الإلغاء |
| DisputeResolutionPeriodDays | int? | Dispute Resolution Period (days) - مدة تسوية النزاع بالأيام |
| CompetentCourt | string | Competent Court - المحكمة المختصة |
| CompetentCourtAr | string | Competent Court (Arabic) - المحكمة المختصة بالعربي |
| PreambleText | string | Preamble Text - نص التمهيد |
| PreambleTextAr | string | Preamble Text (Arabic) - نص التمهيد بالعربي |
| ContractTemplate | string | Contract Template Used - القالب المستخدم |
| PDFGenerated | bool | PDF Generated - تم إنشاء ملف PDF |
| PDFGeneratedDate | DateTime? | PDF Generated Date - تاريخ إنشاء ملف PDF |
| DocumentsSubmitted | string | Documents Submitted - المستندات المقدمة (JSON array) |
| AllDocumentsVerified | bool | All Documents Verified - تم التحقق من جميع المستندات |

---

### 4. event_tickets
**Table Name:** `event_tickets`  
**Class:** `EventTicket`  
**Description:** Event Ticket - تذكرة الفعالية

#### Fields

| Field Name | Type | Description |
|------------|------|-------------|
| TicketId | long (PK) | Ticket ID - معرف التذكرة |
| EventId | long | Event ID - معرف الفعالية |
| VisitorId | long | Visitor ID - معرف الزائر |
| TicketNumber | string | Ticket Number - رقم التذكرة |
| TicketType | string | Ticket Type - نوع التذكرة |
| TicketCategory | string | Ticket Category - فئة التذكرة |
| Price | decimal | Price - السعر |
| Currency | string | Currency - العملة |
| PurchaseDate | DateTime | Purchase Date - تاريخ الشراء |
| ValidFrom | DateTime | Valid From - صالح من |
| ValidUntil | DateTime | Valid Until - صالح حتى |
| QRCode | string | QR Code - رمز الاستجابة السريع |
| Barcode | string | Barcode - الباركود |
| Status | string | Status - الحالة |
| IsPaid | bool | Is Paid - تم الدفع |
| PaymentMethod | string | Payment Method - طريقة الدفع |
| PaymentReference | string | Payment Reference - مرجع الدفع |
| UsedDate | DateTime? | Used Date - تاريخ الاستخدام |
| UsedGateId | long? | Used Gate ID - معرف البوابة المستخدمة |
| SeatNumber | string | Seat Number - رقم المقعد |
| ZoneAccess | string | Zone Access - الوصول للمناطق (JSON array) |
| CanBeTransferred | bool | Can be Transferred - قابل للتحويل |
| TransferredToVisitorId | long? | Transferred To Visitor ID - تم التحويل إلى |
| TransferDate | DateTime? | Transfer Date - تاريخ التحويل |
| Notes | string | Notes - ملاحظات |
| IsActive | bool | Is Active - نشط |
| IssueDate | DateTime | Issue Date - تاريخ الإصدار |
| UsedTime | DateTime | Used Time - وقت الاستخدام |
| TransferredAt | DateTime | Transferred At - تاريخ التحويل |
| VisitorName | string | Visitor Name - اسم الزائر |
| phoneNumber | string | Phone Number - رقم الهاتف |
| PaymentStatus | string | Payment Status - حالة الدفع |
| LastCheckinAt | DateTime? | Last Check-in At - آخر وقت دخول |
| LastCheckoutAt | DateTime? | Last Checkout At - آخر وقت خروج |
| IsCheckedIn | bool | Is Checked In - هل تم تسجيل الدخول |
| TotalCheckins | int | Total Check-ins - إجمالي مرات الدخول |
| LastGateId | long? | Last Gate ID - آخر بوابة مستخدمة |
| LastOperatorId | long? | Last Operator ID - آخر مشغل |
| LastDeviceInfo | string | Last Device Info - آخر معلومات جهاز |

---

### 5. event_visitors
**Table Name:** `event_visitors`  
**Class:** `EventVisitor`  
**Description:** Event Visitor - زائر الفعالية

**Note:** This table stores visitor identity and profile information only. Check-in/check-out functionality is handled separately through the `visitor_checkins` table. Visitors are no longer directly tied to specific events; event association is managed through tickets and permits.

#### Fields

| Field Name | Type | Description |
|------------|------|-------------|
| VisitorId | long (PK) | Visitor ID - معرف الزائر |
| VisitorCode | string | Visitor Code - رمز الزائر (auto-generated if not provided) |
| FirstName | string | First Name - الاسم الأول |
| LastName | string | Last Name - الاسم الأخير |
| FirstNameAr | string | First Name (Arabic) - الاسم الأول بالعربية |
| LastNameAr | string | Last Name (Arabic) - الاسم الأخير بالعربية |
| IdentificationType | string | Identification Type - نوع الهوية (national_id, passport, other) |
| NationalId | string | National ID / Iqama - رقم الهوية / الإقامة |
| PassportNumber | string | Passport Number - رقم الجواز |
| IdentificationNumber | string | Identification Number - رقم الهوية (generic) |
| Nationality | string | Nationality - الجنسية |
| NationalityCode | string | Nationality Code - رمز الجنسية (e.g., SA, US) |
| DateOfBirth | DateTime? | Date of Birth - تاريخ الميلاد |
| Gender | string | Gender - الجنس |
| PhoneNumber | string | Phone Number - رقم الهاتف |
| Email | string | Email - البريد الإلكتروني (optional) |
| VerificationStatus | string | Verification Status - حالة التحقق (pending, verified, rejected) |
| PhotoUrl | string | Photo URL - رابط الصورة الشخصية |
| Notes | string | Notes - ملاحظات |
| VisitorType | string | Visitor Type - نوع الزائر |
| CreateTime | DateTime | Create Time - وقت الإنشاء (from SysBase) |
| UpdateTime | DateTime | Update Time - وقت التحديث (from SysBase) |
| CreateBy | string | Create By - منشئ بواسطة (from SysBase) |

**Removed Fields (moved to other tables/logic):**
- `EventId` - Visitors are no longer directly tied to events
- `UserId` - Removed
- `FullNameAr` - Replaced with separate `FirstNameAr` and `LastNameAr`
- `CompanyName`, `JobTitle` - Removed
- `RegistrationDate`, `RegistrationSource` - Removed
- `QRCode` - QR codes are generated from `VisitorCode` when needed
- `BadgeNumber`, `BadgePrinted`, `BadgePrintedDate` - Removed
- `Status` - Replaced with `VerificationStatus`
- `CheckInTime`, `CheckOutTime`, `CheckInGateId`, `CheckOutGateId` - Moved to `visitor_checkins` table
- `LastCheckinAt`, `LastCheckoutAt`, `IsInside`, `TotalVisits`, `LastGateId` - Moved to `visitor_checkins` table logic
- `EmergencyContactName`, `EmergencyContactPhone`, `SpecialNeeds` - Removed
- `IsVIP` - Removed (can be represented via `VisitorType`)
- `IsActive` - Removed
- `Phone` - Renamed to `PhoneNumber`
- `ProfilePictureUrl` - Renamed to `PhotoUrl`

---

### 6. event_zones
**Table Name:** `event_zones`  
**Class:** `EventZone`  
**Description:** Event Zone - المناطق داخل الفعالية

#### Fields

| Field Name | Type | Description |
|------------|------|-------------|
| ZoneId | long (PK) | Zone ID - معرف المنطقة |
| EventId | long | Event ID - معرف الفعالية |
| ZoneCode | string | Zone Code - رمز المنطقة |
| ZoneName | string | Zone Name (English) - اسم المنطقة بالإنجليزية |
| ZoneNameAr | string | Zone Name (Arabic) - اسم المنطقة بالعربية |
| Description | string | Description - الوصف |
| ZoneType | string | Zone Type - نوع المنطقة |
| ParentZoneId | long? | Parent Zone ID - معرف المنطقة الأب |
| MaxCapacity | int? | Max Capacity - السعة القصوى |
| CurrentOccupancy | int | Current Occupancy - العدد الحالي |
| AreaSqm | decimal? | Area (Square Meters) - المساحة بالمتر المربع |
| FloorNumber | int? | Floor Number - رقم الطابق |
| BuildingName | string | Building Name - اسم المبنى |
| MapCoordinates | string | Map Coordinates (JSON) - إحداثيات الخريطة |
| ColorCode | string | Color Code - رمز اللون |
| AccessLevel | string | Access Level - مستوى الوصول |
| ZoneManagerId | long? | Zone Manager ID - معرف مسؤول المنطقة |
| Status | string | Status - الحالة |
| DisplayOrder | int? | Display Order - ترتيب العرض |
| IsActive | bool | Is Active - نشط |

---

### 7. event_gates
**Table Name:** `event_gates`  
**Class:** `EventGate`  
**Description:** Event Gate - بوابات الدخول

#### Fields

| Field Name | Type | Description |
|------------|------|-------------|
| Gateid | long (PK) | Gate ID - معرف البوابة |
| Eventid | long | Event ID - معرف الفعالية |
| Zoneid | long | Zone ID - معرف المنطقة |
| Gatecode | string | Gate Code - رمز البوابة |
| Gatename | string | Gate Name (English) - اسم البوابة بالإنجليزية |
| Gatenamear | string | Gate Name (Arabic) - اسم البوابة بالعربية |
| Description | string | Description - الوصف |
| Gatetype | string | Gate Type - نوع البوابة |
| Location | string | Location - الموقع |
| Latitude | decimal | Latitude - خط العرض |
| Longitude | decimal | Longitude - خط الطول |
| Maxcapacityperhour | int | Max Capacity Per Hour - السعة القصوى في الساعة |
| Status | string | Status - الحالة |
| Openingtime | DateTime | Opening Time - وقت الفتح |
| Closingtime | DateTime | Closing Time - وقت الإغلاق |
| Hasscanner | string | Has Scanner - يحتوي على ماسح ضوئي |
| Scannerdeviceid | string | Scanner Device ID - معرف جهاز المسح |
| Gatemanagerid | long | Gate Manager ID - معرف مسؤول البوابة |
| Displayorder | int | Display Order - ترتيب العرض |
| Isactive | string | Is Active - نشط |
| Laststatusupdate | DateTime | Last Status Update - آخر تحديث للحالة |

---

### 8. event_tenants
**Table Name:** `event_tenants`  
**Class:** `EventTenant`  
**Description:** Event Tenant - مستأجر الكشك

#### Fields

| Field Name | Type | Description |
|------------|------|-------------|
| TenantId | long (PK) | Tenant ID - معرف المستأجر |
| EventId | long | Event ID - معرف الفعالية |
| TenantCode | string | Tenant Code - رمز المستأجر |
| CompanyName | string | Company Name - اسم الشركة |
| CompanyNameAr | string | Company Name (Arabic) - اسم الشركة بالعربية |
| CommercialRegNumber | string | Commercial Registration Number - رقم السجل التجاري |
| TaxNumber | string | Tax Number - الرقم الضريبي |
| BusinessType | string | Business Type - نوع النشاط التجاري |
| ContactPersonName | string | Contact Person Name - اسم جهة الاتصال |
| ContactPosition | string | Contact Position - منصب جهة الاتصال |
| Email | string | Email - البريد الإلكتروني |
| Phone | string | Phone - الهاتف |
| Mobile | string | Mobile - الجوال |
| Address | string | Address - العنوان |
| City | string | City - المدينة |
| Country | string | Country - الدولة |
| Website | string | Website - الموقع الإلكتروني |
| LogoUrl | string | Logo URL - رابط الشعار |
| BankName | string | Bank Name - اسم البنك |
| AccountNumber | string | Account Number - رقم الحساب |
| IBAN | string | IBAN - رقم الآيبان |
| RegistrationDate | DateTime | Registration Date - تاريخ التسجيل |
| Status | string | Status - الحالة |
| Rating | decimal? | Rating - التقييم |
| TotalBoothsRented | int | Total Booths Rented - إجمالي الأكشاك المؤجرة |
| Notes | string | Notes - ملاحظات |
| IsActive | bool | Is Active - نشط |

---

### 9. event_payments
**Table Name:** `event_payments`  
**Class:** `EventPayment`  
**Description:** Event Payment - الدفعات

#### Fields

| Field Name | Type | Description |
|------------|------|-------------|
| PaymentId | long (PK) | Payment ID - معرف الدفعة |
| EventId | long | Event ID - معرف الفعالية |
| ContractId | long? | Contract ID - معرف العقد |
| InvoiceId | long? | Invoice ID - معرف الفاتورة |
| TenantId | long? | Tenant ID - معرف المستأجر |
| VisitorId | long? | Visitor ID - معرف الزائر |
| PaymentNumber | string | Payment Number - رقم الدفعة |
| PaymentType | string | Payment Type - نوع الدفعة |
| PaymentDate | DateTime | Payment Date - تاريخ الدفع |
| Amount | decimal | Amount - المبلغ |
| Currency | string | Currency - العملة |
| VATAmount | decimal? | VAT Amount - قيمة الضريبة |
| TotalAmount | decimal | Total Amount - المبلغ الإجمالي |
| PaymentMethod | string | Payment Method - طريقة الدفع |
| CardType | string | Card Type - نوع البطاقة |
| CardLast4 | string | Card Last 4 Digits - آخر 4 أرقام من البطاقة |
| TransactionReference | string | Transaction Reference - مرجع العملية |
| BankReference | string | Bank Reference - مرجع البنك |
| PaymentGateway | string | Payment Gateway - بوابة الدفع |
| GatewayResponse | string | Gateway Response - رد بوابة الدفع |
| Status | string | Status - الحالة |
| PayerName | string | Payer Name - اسم الدافع |
| PayerEmail | string | Payer Email - بريد الدافع |
| PayerPhone | string | Payer Phone - هاتف الدافع |
| ReceiptNumber | string | Receipt Number - رقم الإيصال |
| ReceiptUrl | string | Receipt URL - رابط الإيصال |
| RefundAmount | decimal? | Refund Amount - مبلغ المرتجع |
| RefundDate | DateTime? | Refund Date - تاريخ المرتجع |
| RefundReason | string | Refund Reason - سبب المرتجع |
| ProcessedBy | string | Processed By - تمت المعالجة بواسطة |
| Notes | string | Notes - ملاحظات |
| IsActive | bool | Is Active - نشط |

---

### 10. event_invoices
**Table Name:** `event_invoices`  
**Class:** `EventInvoice`  
**Description:** Event Invoice - الفواتير

#### Fields

| Field Name | Type | Description |
|------------|------|-------------|
| InvoiceId | long (PK) | Invoice ID - معرف الفاتورة |
| EventId | long | Event ID - معرف الفعالية |
| ContractId | long? | Contract ID - معرف العقد |
| TenantId | long? | Tenant ID - معرف المستأجر |
| InvoiceNumber | string | Invoice Number - رقم الفاتورة |
| InvoiceDate | DateTime | Invoice Date - تاريخ الفاتورة |
| DueDate | DateTime | Due Date - تاريخ الاستحقاق |
| InvoiceType | string | Invoice Type - نوع الفاتورة |
| Description | string | Description - الوصف |
| SubtotalAmount | decimal | Subtotal Amount - المبلغ الفرعي |
| VATPercentage | decimal | VAT Percentage - نسبة الضريبة |
| VATAmount | decimal | VAT Amount - قيمة الضريبة |
| DiscountAmount | decimal? | Discount Amount - مبلغ الخصم |
| TotalAmount | decimal | Total Amount - المبلغ الإجمالي |
| PaidAmount | decimal | Paid Amount - المبلغ المدفوع |
| Balance | decimal | Balance - الرصيد المتبقي |
| Currency | string | Currency - العملة |
| Status | string | Status - الحالة |
| BillingAddress | string | Billing Address - عنوان الفوترة |
| InvoiceItems | string | Invoice Items (JSON) - بنود الفاتورة |
| PaymentTerms | string | Payment Terms - شروط الدفع |
| InvoiceFileUrl | string | Invoice File URL - رابط ملف الفاتورة |
| SentDate | DateTime? | Sent Date - تاريخ الإرسال |
| SentToEmail | string | Sent To Email - تم الإرسال إلى البريد |
| PaymentDate | DateTime? | Payment Date - تاريخ الدفع الكامل |
| LateFeeAmount | decimal? | Late Fee Amount - رسوم التأخير |
| DaysOverdue | int | Days Overdue - أيام التأخير |
| CancelledDate | DateTime? | Cancelled Date - تاريخ الإلغاء |
| CancellationReason | string | Cancellation Reason - سبب الإلغاء |
| Notes | string | Notes - ملاحظات |
| IsActive | bool | Is Active - نشط |
| PaidDate | DateTime | Paid Date - تاريخ الدفع |
| CustomerName | string | Customer Name - اسم العميل |
| CustomerEmail | string | Customer Email - بريد العميل |
| CustomerPhone | string | Customer Phone - هاتف العميل |
| SubTotal | decimal | Sub Total - المجموع الفرعي |
| PaymentStatus | string | Payment Status - حالة الدفع |
| PaymentMethod | string | Payment Method - طريقة الدفع |

---

### 11. event_incidents
**Table Name:** `event_incidents`  
**Class:** `EventIncident`  
**Description:** Event Incident - الحوادث والبلاغات

#### Fields

| Field Name | Type | Description |
|------------|------|-------------|
| IncidentId | long (PK) | Incident ID - معرف الحادث |
| EventId | long | Event ID - معرف الفعالية |
| IncidentNumber | string | Incident Number - رقم البلاغ |
| IncidentType | string | Incident Type - نوع الحادث |
| IncidentCategory | string | Incident Category - فئة الحادث |
| Priority | string | Priority - الأولوية |
| Severity | string | Severity - الخطورة |
| Title | string | Title - العنوان |
| Description | string | Description - الوصف |
| Location | string | Location - الموقع |
| ZoneId | long? | Zone ID - معرف المنطقة |
| BoothId | long? | Booth ID - معرف الكشك |
| ReportedDate | DateTime | Reported Date - تاريخ البلاغ |
| ReportedBy | string | Reported By - تم البلاغ بواسطة |
| ReporterType | string | Reporter Type - نوع المبلغ |
| ReporterContact | string | Reporter Contact - جهة اتصال المبلغ |
| Status | string | Status - الحالة |
| AssignedToId | long? | Assigned To - مكلف به |
| AssignedDate | DateTime? | Assigned Date - تاريخ التكليف |
| AssignedTeam | string | Assigned Team - الفريق المكلف |
| ResponseTimeMinutes | int? | Response Time (Minutes) - وقت الاستجابة بالدقائق |
| ResolutionTimeMinutes | int? | Resolution Time (Minutes) - وقت الحل بالدقائق |
| ResolutionDate | DateTime? | Resolution Date - تاريخ الحل |
| ResolutionNotes | string | Resolution Notes - ملاحظات الحل |
| Escalated | bool | Escalated - تم التصعيد |
| EscalatedTo | string | Escalated To - تم التصعيد إلى |
| EscalationDate | DateTime? | Escalation Date - تاريخ التصعيد |
| EscalationReason | string | Escalation Reason - سبب التصعيد |
| Attachments | string | Attachments (JSON) - المرفقات |
| WitnessInformation | string | Witness Information - معلومات الشهود |
| FollowUpRequired | bool | Follow Up Required - يتطلب متابعة |
| FollowUpDate | DateTime? | Follow Up Date - تاريخ المتابعة |
| ClosedDate | DateTime? | Closed Date - تاريخ الإغلاق |
| ClosedBy | string | Closed By - تم الإغلاق بواسطة |
| SatisfactionRating | int? | Customer Satisfaction Rating - تقييم رضا العميل |
| Notes | string | Notes - ملاحظات |
| IsActive | bool | Is Active - نشط |
| IncidentTime | DateTime | Incident Time - وقت الحادث |
| AssignedTime | DateTime? | Assigned Time - وقت التكليف |
| ResolvedTime | DateTime | Resolved Time - وقت الحل |
| Resolution | string | Resolution - الحل |
| ClosedTime | DateTime | Closed Time - وقت الإغلاق |

---

### 12. event_permits
**Table Name:** `event_permits`  
**Class:** `EventPermit`  
**Description:** Event Permit - تصاريح الدخول

#### Fields

| Field Name | Type | Description |
|------------|------|-------------|
| PermitId | long (PK) | Permit ID - معرف التصريح |
| EventId | long | Event ID - معرف الفعالية |
| EmployeeId | string? | Employee ID - معرف الموظف/العامل |
| VisitorId | string? | Visitor ID - معرف الزائر |
| PermitNumber | string | Permit Number - رقم التصريح |
| PermitType | string | Permit Type - نوع التصريح |
| PermitCategory | string | Permit Category - فئة التصريح |
| ApplicantName | string | Applicant Name - اسم مقدم الطلب |
| ApplicantNameAr | string | Applicant Name (Arabic) - اسم مقدم الطلب بالعربية |
| CompanyName | string | Company Name - اسم الشركة |
| NationalId | string | National ID - رقم الهوية |
| ContactPhone | string | Contact Phone - هاتف التواصل |
| Email | string | Email - البريد الإلكتروني |
| ValidFrom | DateTime | Valid From - صالح من |
| ValidUntil | DateTime | Valid Until - صالح حتى |
| IssueDate | DateTime | Issue Date - تاريخ الإصدار |
| IssuedBy | string | Issued By - صادر من |
| QRCode | string | QR Code - رمز الاستجابة السريع |
| ZoneAccess | string | Zone Access - الوصول للمناطق (JSON array) |
| GateAccess | string | Gate Access - الوصول للبوابات (JSON array) |
| AccessLevel | string | Access Level - مستوى الوصول |
| Purpose | string | Purpose - الغرض |
| VehiclePlateNumber | string | Vehicle Plate Number - رقم لوحة المركبة |
| VehicleType | string | Vehicle Type - نوع المركبة |
| PhotoUrl | string | Photo URL - رابط الصورة |
| AttachmentUrl | string | Attachment URL - رابط المرفق |
| Status | string | Status - الحالة |
| ApprovedBy | long | Approved By - تمت الموافقة من قبل |
| ApprovalDate | DateTime? | Approval Date - تاريخ الموافقة |
| RejectionReason | string | Rejection Reason - سبب الرفض |
| Notes | string | Notes - ملاحظات |
| ApprovedDate | DateTime | Approved Date - تاريخ الموافقة |
| HolderName | string | Holder Name - اسم الحامل |

---

### 13. event_locations
**Table Name:** `event_locations`  
**Class:** `EventLocation`  
**Description:** Event Location Information - معلومات موقع الفعالية

#### Fields

| Field Name | Type | Description |
|------------|------|-------------|
| Locationid | long (PK) | Location ID - معرف الموقع |
| Locationname | string | Location Name - اسم الموقع |
| Venue | string | Venue - المكان/القاعة |
| Address | string | Address - العنوان |
| City | string | City - المدينة |
| Country | string | Country - الدولة |
| Latitude | float | Latitude - خط العرض |
| Longitude | float | Longitude - خط الطول |
| Mapimageurl | string | Map Image URL - رابط صورة الخريطة |
| Maxcapacity | int | Max Capacity - السعة القصوى للموقع |
| Isactive | string | Is Active - نشط |

---

## Lookup/Reference Tables

### 14. event_booth_types
**Table Name:** `event_booth_types`  
**Class:** `BoothType`  
**Description:** Booth Type - أنواع الأكشاك

#### Fields

| Field Name | Type | Description |
|------------|------|-------------|
| Typeid | long (PK) | Type ID - معرف النوع |
| Typecode | string | Type Code - رمز النوع |
| Typename | string | Type Name (English) - اسم النوع بالإنجليزية |
| Typenamear | string | Type Name (Arabic) - اسم النوع بالعربية |
| Description | string | Description - الوصف |
| Icon | string | Icon - الأيقونة |
| Colorcode | string | Color Code - رمز اللون |
| Defaultpricepersqm | float | Default Price Per SQM - السعر الافتراضي للمتر المربع |
| Minsizesqm | float | Min Size SQM - الحد الأدنى للمساحة |
| Maxsizesqm | float | Max Size SQM - الحد الأقصى للمساحة |
| Includeselectricity | string | Includes Electricity - يشمل كهرباء |
| Includesinternet | string | Includes Internet - يشمل إنترنت |
| Includesfurniture | string | Includes Furniture - يشمل أثاث |
| Displayorder | int | Display Order - ترتيب العرض |
| Isactive | string | Is Active - نشط |

---

### 15. event_types
**Table Name:** `event_types`  
**Class:** `EventType`  
**Description:** Event Type - أنواع الفعاليات

#### Fields

| Field Name | Type | Description |
|------------|------|-------------|
| Typeid | long (PK) | Type ID - معرف النوع |
| Typecode | string | Type Code - رمز النوع |
| Typename | string | Type Name (English) - اسم النوع بالإنجليزية |
| Typenamear | string | Type Name (Arabic) - اسم النوع بالعربية |
| Description | string | Description - الوصف |
| Icon | string | Icon - الأيقونة |
| Colorcode | string | Color Code - رمز اللون |
| Displayorder | int | Display Order - ترتيب العرض |
| Isactive | string | Is Active - نشط |

---

### 16. event_status
**Table Name:** `event_status`  
**Class:** `EventStatus`  
**Description:** Event Status - أنواع حالة الفعاليات

#### Fields

| Field Name | Type | Description |
|------------|------|-------------|
| Typeid | long (PK) | Status ID - معرف النوع |
| Typecode | string | Status Code - رمز حالة |
| Typename | string | Status Name (English) - اسم حالة بالإنجليزية |
| Typenamear | string | Status Name (Arabic) - اسم الحالة بالعربية |
| Description | string | Description - الوصف |
| Icon | string | Icon - الأيقونة |
| Colorcode | string | Color Code - رمز اللون |
| Displayorder | int | Display Order - ترتيب العرض |
| Isactive | string | Is Active - نشط |

---

### 17. event_zone_types
**Table Name:** `event_zone_types`  
**Class:** `EventZoneType`  
**Description:** Zone Type - أنواع المناطق

#### Fields

| Field Name | Type | Description |
|------------|------|-------------|
| Typeid | long (PK) | Type ID - معرف النوع |
| Typecode | string | Type Code - رمز النوع |
| Typename | string | Type Name (English) - اسم النوع بالإنجليزية |
| Typenamear | string | Type Name (Arabic) - اسم النوع بالعربية |
| Description | string | Description - الوصف |
| Icon | string | Icon - الأيقونة |
| Colorcode | string | Color Code - رمز اللون |
| Requiresspecialaccess | string | Requires Special Access - يتطلب صلاحية خاصة |
| Displayorder | int | Display Order - ترتيب العرض |
| Isactive | string | Is Active - نشط |

---

### 18. event_gate_types
**Table Name:** `event_gate_types`  
**Class:** `EventGateType`  
**Description:** Gate Type - أنواع البوابات

#### Fields

| Field Name | Type | Description |
|------------|------|-------------|
| Typeid | long (PK) | Type ID - معرف النوع |
| Typecode | string | Type Code - رمز النوع |
| Typename | string | Type Name (English) - اسم النوع بالإنجليزية |
| Typenamear | string | Type Name (Arabic) - اسم النوع بالعربية |
| Description | string | Description - الوصف |
| Icon | string | Icon - الأيقونة |
| Colorcode | string | Color Code - رمز اللون |
| Direction | string | Direction - الاتجاه (entry, exit, both) |
| Displayorder | int | Display Order - ترتيب العرض |
| Isactive | string | Is Active - نشط |

---

### 19. event_ticket_types
**Table Name:** `event_ticket_types`  
**Class:** `TicketType`  
**Description:** Ticket Type - أنواع التذاكر

#### Fields

| Field Name | Type | Description |
|------------|------|-------------|
| Typeid | long (PK) | Type ID - معرف النوع |
| Typecode | string | Type Code - رمز النوع |
| Typename | string | Type Name (English) - اسم النوع بالإنجليزية |
| Typenamear | string | Type Name (Arabic) - اسم النوع بالعربية |
| Description | string | Description - الوصف |
| Defaultprice | float | Default Price - السعر الافتراضي |
| Currency | string | Currency - العملة |
| Icon | string | Icon - الأيقونة |
| Colorcode | string | Color Code - رمز اللون |
| Istransferable | string | Is Transferable - قابل للتحويل |
| Isrefundable | string | Is Refundable - قابل للاسترجاع |
| Maxquantityperperson | int | Max Quantity Per Person - الحد الأقصى للشخص |
| Displayorder | int | Display Order - ترتيب العرض |
| Isactive | string | Is Active - نشط |

---

### 20. event_visitor_types
**Table Name:** `event_visitor_types`  
**Class:** `VisitorType`  
**Description:** Visitor Type - أنواع الزوار

#### Fields

| Field Name | Type | Description |
|------------|------|-------------|
| Typeid | long (PK) | Type ID - معرف النوع |
| Typecode | string | Type Code - رمز النوع |
| Typename | string | Type Name (English) - اسم النوع بالإنجليزية |
| Typenamear | string | Type Name (Arabic) - اسم النوع بالعربية |
| Description | string | Description - الوصف |
| Icon | string | Icon - الأيقونة |
| Colorcode | string | Color Code - رمز اللون |
| Badgecolor | string | Badge Color - لون البطاقة |
| Haspriorityaccess | string | Has Priority Access - له أولوية في الدخول |
| Requiresapproval | string | Requires Approval - يتطلب موافقة |
| Displayorder | int | Display Order - ترتيب العرض |
| Isactive | string | Is Active - نشط |

---

### 21. event_permit_types
**Table Name:** `event_permit_types`  
**Class:** `PermitType`  
**Description:** Permit Type - أنواع التصاريح

#### Fields

| Field Name | Type | Description |
|------------|------|-------------|
| Typeid | long (PK) | Type ID - معرف النوع |
| Typecode | string | Type Code - رمز النوع |
| Typename | string | Type Name (English) - اسم النوع بالإنجليزية |
| Typenamear | string | Type Name (Arabic) - اسم النوع بالعربية |
| Description | string | Description - الوصف |
| Icon | string | Icon - الأيقونة |
| Colorcode | string | Color Code - رمز اللون |
| Requiresapproval | string | Requires Approval - يتطلب موافقة |
| Approvallevel | string | Approval Level - مستوى الموافقة المطلوب |
| Defaultvaliditydays | int | Default Validity Days - عدد أيام الصلاحية الافتراضية |
| Requiresvehicleinfo | string | Requires Vehicle Info - يتطلب معلومات المركبة |
| Requiresphoto | string | Requires Photo - يتطلب صورة |
| Displayorder | int | Display Order - ترتيب العرض |
| Isactive | string | Is Active - نشط |

---

### 22. event_incident_types
**Table Name:** `event_incident_types`  
**Class:** `IncidentType`  
**Description:** Incident Type - أنواع الحوادث

#### Fields

| Field Name | Type | Description |
|------------|------|-------------|
| Typeid | long (PK) | Type ID - معرف النوع |
| Typecode | string | Type Code - رمز النوع |
| Typename | string | Type Name (English) - اسم النوع بالإنجليزية |
| Typenamear | string | Type Name (Arabic) - اسم النوع بالعربية |
| Description | string | Description - الوصف |
| Icon | string | Icon - الأيقونة |
| Colorcode | string | Color Code - رمز اللون |
| Defaultpriority | string | Default Priority - الأولوية الافتراضية |
| Defaultseverity | string | Default Severity - الخطورة الافتراضية |
| Assignedteam | string | Assigned Team - الفريق المسؤول |
| Slaresponseminutes | int | SLA Response Minutes - وقت الاستجابة المتوقع بالدقائق |
| Slaresolutionminutes | int | SLA Resolution Minutes - وقت الحل المتوقع بالدقائق |
| Requiresimmediateaction | string | Requires Immediate Action - يتطلب إجراء فوري |
| Displayorder | int | Display Order - ترتيب العرض |
| Isactive | string | Is Active - نشط |
| Severity | string | Severity - خطورة |

---

### 23. event_payment_methods
**Table Name:** `event_payment_methods`  
**Class:** `PaymentMethod`  
**Description:** Payment Method - طرق الدفع

#### Fields

| Field Name | Type | Description |
|------------|------|-------------|
| Methodid | long (PK) | Method ID - معرف الطريقة |
| Methodcode | string | Method Code - رمز الطريقة |
| Methodname | string | Method Name (English) - اسم الطريقة بالإنجليزية |
| Methodnamear | string | Method Name (Arabic) - اسم الطريقة بالعربية |
| Description | string | Description - الوصف |
| Icon | string | Icon - الأيقونة |
| Colorcode | string | Color Code - رمز اللون |
| Paymentgateway | string | Payment Gateway - بوابة الدفع |
| Gatewayconfig | string | Gateway Config (JSON) - إعدادات البوابة |
| Transactionfeepercentage | float | Transaction Fee Percentage - نسبة رسوم المعاملة |
| Fixedtransactionfee | float | Fixed Transaction Fee - رسوم ثابتة للمعاملة |
| Isonlinepayment | string | Is Online Payment - دفع أونلاين |
| Requiresverification | string | Requires Verification - يتطلب تحقق |
| Displayorder | int | Display Order - ترتيب العرض |
| Isactive | string | Is Active - نشط |

---

## History/Audit Tables

### 24. event_booth_history
**Table Name:** `event_booth_history`  
**Class:** `EventBoothHistory`  
**Description:** Event Booth History - سجل الأكشاك

#### Fields

| Field Name | Type | Description |
|------------|------|-------------|
| HistoryId | long (PK) | History ID - معرف السجل |
| BoothId | long | Booth ID - معرف الكشك |
| BoothCode | string | Booth Code - رمز الكشك |
| BoothName | string | Booth Name - اسم الكشك |
| EventId | long? | Event ID - معرف الفعالية |
| EventName | string | Event Name - اسم الفعالية |
| TenantId | long? | Tenant ID - معرف المستأجر |
| TenantName | string | Tenant Name - اسم المستأجر |
| ActionType | string | Action Type - نوع الإجراء |
| PreviousStatus | string | Previous Status - الحالة السابقة |
| NewStatus | string | New Status - الحالة الجديدة |
| PreviousTenantId | long? | Previous Tenant ID - معرف المستأجر السابق |
| PreviousTenantName | string | Previous Tenant Name - اسم المستأجر السابق |
| RentalPrice | decimal? | Rental Price - سعر الإيجار |
| ActionDate | DateTime | Action Date - تاريخ الإجراء |
| PerformedByUserId | long? | Performed By User ID - معرف المستخدم المنفذ |
| PerformedByUsername | string | Performed By Username - اسم المستخدم المنفذ |
| Notes | string | Notes - ملاحظات |
| IpAddress | string | IP Address - عنوان IP |

---

### 25. event_contract_history
**Table Name:** `event_contract_history`  
**Class:** `EventContractHistory`  
**Description:** Event Contract History - سجل العقود

#### Fields

| Field Name | Type | Description |
|------------|------|-------------|
| HistoryId | long (PK) | History ID - معرف السجل |
| ContractId | long | Contract ID - معرف العقد |
| ContractNumber | string | Contract Number - رقم العقد |
| EventId | long? | Event ID - معرف الفعالية |
| EventName | string | Event Name - اسم الفعالية |
| TenantId | long? | Tenant ID - معرف المستأجر |
| TenantName | string | Tenant Name - اسم المستأجر |
| BoothId | long? | Booth ID - معرف الكشك |
| BoothCode | string | Booth Code - رمز الكشك |
| ActionType | string | Action Type - نوع الإجراء |
| PreviousStatus | string | Previous Status - الحالة السابقة |
| NewStatus | string | New Status - الحالة الجديدة |
| PreviousAmount | decimal? | Previous Amount - المبلغ السابق |
| NewAmount | decimal? | New Amount - المبلغ الجديد |
| PreviousSignedByTenant | bool? | Signed By Tenant Before - موقع من المستأجر سابقاً |
| NewSignedByTenant | bool? | Signed By Tenant After - موقع من المستأجر بعد |
| PreviousSignedByOrganizer | bool? | Signed By Organizer Before - موقع من المنظم سابقاً |
| NewSignedByOrganizer | bool? | Signed By Organizer After - موقع من المنظم بعد |
| ActionDate | DateTime | Action Date - تاريخ الإجراء |
| PerformedByUserId | long? | Performed By User ID - معرف المستخدم الذي قام بالإجراء |
| PerformedByUsername | string | Performed By Username - اسم المستخدم الذي قام بالإجراء |
| IpAddress | string | IP Address - عنوان IP |
| Notes | string | Notes - ملاحظات |
| ChangesJson | string | Changes JSON - التغييرات بصيغة JSON |

---

### 26. event_gate_history
**Table Name:** `event_gate_history`  
**Class:** `EventGateHistory`  
**Description:** Event Gate History - سجل تاريخ البوابات

#### Fields

| Field Name | Type | Description |
|------------|------|-------------|
| HistoryId | long (PK) | History ID - معرف السجل |
| GateId | long | Gate ID - معرف البوابة |
| GateCode | string | Gate Code - رمز البوابة |
| GateName | string | Gate Name - اسم البوابة |
| EventId | long? | Event ID - معرف الفعالية |
| EventName | string | Event Name - اسم الفعالية |
| ActionType | string | Action Type - نوع الإجراء |
| PreviousStatus | string | Previous Status - الحالة السابقة |
| NewStatus | string | New Status - الحالة الجديدة |
| PreviousGateType | string | Previous Gate Type - النوع السابق للبوابة |
| NewGateType | string | New Gate Type - النوع الجديد للبوابة |
| PreviousCapacity | int? | Previous Capacity - السعة السابقة |
| NewCapacity | int? | New Capacity - السعة الجديدة |
| PreviousManagerId | long? | Previous Manager ID - معرف المسؤول السابق |
| PreviousManagerName | string | Previous Manager Name - اسم المسؤول السابق |
| NewManagerId | long? | New Manager ID - معرف المسؤول الجديد |
| NewManagerName | string | New Manager Name - اسم المسؤول الجديد |
| ActionDate | DateTime | Action Date - تاريخ الإجراء |
| PerformedByUserId | long? | Performed By User ID - معرف المستخدم الذي قام بالإجراء |
| PerformedByUsername | string | Performed By Username - اسم المستخدم الذي قام بالإجراء |
| Notes | string | Notes - ملاحظات |
| IpAddress | string | IP Address - عنوان IP |
| VisitorId | long? | Visitor ID - معرف الزائر |
| VisitorName | string | Visitor Name - اسم الزائر |
| VisitorCode | string | Visitor Code - رمز الزائر |
| TicketId | long? | Ticket ID - معرف التذكرة |
| TicketNumber | string | Ticket Number - رقم التذكرة |
| CheckinAction | string | Checkin Action - إجراء الدخول/الخروج |
| DeviceInfo | string | Device Info - معلومات الجهاز |

---

## Transaction Tables

### 27. ticket_checkins
**Table Name:** `ticket_checkins`  
**Class:** `TicketCheckin`  
**Description:** Ticket Check-in/Checkout History - سجل دخول وخروج التذاكر

#### Fields

| Field Name | Type | Description |
|------------|------|-------------|
| CheckinId | long (PK) | Check-in ID - معرف السجل |
| TicketId | long | Ticket ID - معرف التذكرة |
| EventId | long | Event ID - معرف الفعالية |
| VisitorId | long? | Visitor ID - معرف الزائر |
| GateId | long? | Gate ID - معرف البوابة |
| Action | string | Action - الإجراء (checkin, checkout, manual, system) |
| ActionAt | DateTime | Action At - وقت الإجراء |
| OperatorId | long? | Operator ID - معرف المشغل |
| DeviceInfo | string | Device Info - معلومات الجهاز |
| Notes | string | Notes - ملاحظات |
| IpAddress | string | IP Address - عنوان IP |
| Location | string | Location - الموقع |

---

### 28. visitor_checkins
**Table Name:** `visitor_checkins`  
**Class:** `VisitorCheckin`  
**Description:** Visitor Check-in/Checkout History - سجل دخول وخروج الزوار

#### Fields

| Field Name | Type | Description |
|------------|------|-------------|
| CheckinId | long (PK) | Check-in ID - معرف السجل |
| VisitorId | long | Visitor ID - معرف الزائر |
| EventId | long | Event ID - معرف الفعالية |
| GateId | long? | Gate ID - معرف البوابة |
| Action | string | Action - الإجراء (checkin, checkout, manual, system) |
| ActionAt | DateTime | Action At - وقت الإجراء |
| OperatorId | long? | Operator ID - معرف المشغل |
| DeviceInfo | string | Device Info - معلومات الجهاز |
| Notes | string | Notes - ملاحظات |
| IpAddress | string | IP Address - عنوان IP |
| Location | string | Location - الموقع |

---

## Summary

The Event Management module consists of **28 tables** organized into:

- **13 Core Tables**: Main business entities (Events, Booths, Contracts, Tickets, Visitors, Zones, Gates, Tenants, Payments, Invoices, Incidents, Permits, Locations)
- **10 Lookup/Reference Tables**: Configuration and reference data (Booth Types, Event Types, Status, Zone Types, Gate Types, Ticket Types, Visitor Types, Permit Types, Incident Types, Payment Methods)
- **3 History/Audit Tables**: Audit trails for Booths, Contracts, and Gates
- **2 Transaction Tables**: Check-in/checkout tracking for Tickets and Visitors

All tables inherit from `SysBase` which provides common fields like `CreateBy`, `CreateTime`, `UpdateBy`, `UpdateTime`, `Remark`, etc.

---

**Last Updated:** Generated from EventManagement model classes  
**Namespace:** `ZR.Model.EventManagement`

