# Partnertype Module Seed Data Documentation

## Overview
This directory contains comprehensive seed data for the Partnertype module, which manages partner classification, status tracking, document requirements, and compliance management. The Partnertype module provides the foundational framework for categorizing and managing all external business partners.

## Files Structure

### 1. partner_types.json
**Purpose:** Defines the main types/categories of business partners in the system.

**Records:** 7 partner types

**Data Categories:**
- Vendor classification
- Contractor types
- Service provider categories
- Sub-contractor classification
- Consultant types
- Distributor classification
- Strategic partner designation

**Schema:**
```json
{
  "partnertypeid": 1,
  "typename": "الموردون - Vendors",
  "remark": "Description of partner type",
  "createTime": "2024-01-15T10:00:00",
  "status": "0"
}
```

**Sample Partner Types:**
1. **Vendors (الموردون)** - Companies providing goods and materials
2. **Contractors (المقاولون)** - Service providers and project contractors
3. **Service Providers (موفري الخدمات)** - Professional service companies
4. **Sub-contractors (الموردين الفرعيين)** - Secondary level contractors
5. **Consultants (الاستشاريون)** - Expert advisors and consultants
6. **Distributors (الموزعون)** - Distribution and logistics partners
7. **Strategic Partners (الشركاء الاستراتيجيين)** - Long-term strategic relationships

---

### 2. partner_status.json
**Purpose:** Defines the operational status of business partnerships.

**Records:** 5 status types

**Status Types:**
- Active partnerships
- Suspended relationships
- Under review status
- Inactive partnerships
- Negotiation phase

**Schema:**
```json
{
  "statusid": 1,
  "statusname": "نشط - Active",
  "statuscode": "ACTIVE",
  "description": "Partner is actively working with us",
  "remark": "Full business relationship in progress",
  "createTime": "2024-01-15T10:00:00",
  "status": "0"
}
```

**Status Definitions:**

| Status | Code | Description | Action |
|--------|------|-------------|--------|
| Active (نشط) | ACTIVE | Full working relationship | Proceed with business |
| Suspended (معلق) | SUSPENDED | Temporarily halted | Under review |
| Under Review (قيد المراجعة) | REVIEW | Being evaluated | Awaiting action |
| Inactive (غير نشط) | INACTIVE | Relationship ended | Archive |
| Negotiating (قيد التفاوض) | NEGOTIATING | Initial discussions | Complete terms |

---

### 3. partner_kind.json
**Purpose:** Classifies partners by organizational type and scope.

**Records:** 8 partner kind categories

**Classification Types:**
- Local vs. International scope
- Government entities
- Private company classification
- Individual service providers
- Non-profit organizations
- Small and medium enterprises (SMEs)
- Large enterprise partnerships

**Schema:**
```json
{
  "kindid": 1,
  "kindname": "شركات محلية - Local Companies",
  "kindcode": "LOCAL",
  "description": "Companies operating within Saudi Arabia",
  "remark": "Domestic business partners",
  "createTime": "2024-01-15T10:00:00",
  "status": "0"
}
```

**Partner Kinds:**
1. **Local Companies (شركات محلية)** - Saudi Arabia based
2. **International Companies (شركات دولية)** - From other countries
3. **Government Entities (منشآت حكومية)** - Public sector
4. **Private Companies (شركات خاصة)** - Private sector
5. **Individuals (أفراد)** - Self-employed professionals
6. **Non-Profit Organizations (منظمات غير ربحية)** - NGOs
7. **SMEs (مؤسسات صغيرة ومتوسطة)** - Small/medium enterprises
8. **Large Enterprises (مؤسسات كبرى)** - Multinational corporations

---

### 4. contact_methods.json
**Purpose:** Specifies available methods for communicating with partners.

**Records:** 6 contact method types

**Contact Method Categories:**
- Direct phone communication
- Email communication
- In-person meetings
- Postal mail
- Online chat/messaging
- Secure partner portal

**Schema:**
```json
{
  "methodid": 1,
  "methodname": "الهاتف - Phone",
  "methodcode": "PHONE",
  "description": "Contact via telephone",
  "displayorder": 1,
  "remark": "Direct phone call communication",
  "createTime": "2024-01-15T10:00:00",
  "status": "0"
}
```

**Available Contact Methods:**
1. **Phone (الهاتف)** - Direct telephone calls
2. **Email (البريد الإلكتروني)** - Electronic mail
3. **In-Person (الاجتماع الشخصي)** - Face-to-face meetings
4. **Letter (البريد)** - Postal correspondence
5. **Online Chat (المحادثة عبر الإنترنت)** - Instant messaging
6. **Online Portal (البوابة الإلكترونية)** - Web-based platform

---

### 5. compliance_status.json
**Purpose:** Tracks the compliance and approval status of partners.

**Records:** 4 compliance status types

**Compliance Status Types:**
- Approved and compliant
- Pending review status
- Rejected non-compliance
- Suspended pending remediation

**Schema:**
```json
{
  "compliancestatusid": 1,
  "statusname": "موافق عليه - Approved",
  "statuscode": "APPROVED",
  "description": "Partner fully compliant with all requirements",
  "requirements": "All documentation verified and approved",
  "actionrequired": "None - can proceed with business",
  "displayorder": 1,
  "remark": "Full compliance achieved",
  "createTime": "2024-01-15T10:00:00",
  "status": "0"
}
```

**Compliance Status Lifecycle:**

```
PENDING → APPROVED/REJECTED → SUSPENDED (optional)
  ↓          ↓                    ↓
Review    Compliant         Remediation
```

| Status | Code | Requirements | Action |
|--------|------|--------------|--------|
| Approved (موافق عليه) | APPROVED | All docs verified | Proceed with business |
| Pending (قيد المراجعة) | PENDING | Under review | Submit required docs |
| Rejected (مرفوض) | REJECTED | Failed compliance | Cannot do business |
| Suspended (معلق) | SUSPENDED | Issue identified | Remediate issues |

---

### 6. partner_doc_types.json
**Purpose:** Defines types of documents required from business partners.

**Records:** 8 document type categories

**Required Document Types:**
- Tax certificates
- Commercial licenses
- Insurance documentation
- Service agreements
- Registration certificates
- Financial statements
- Compliance certificates
- Quality certifications

**Schema:**
```json
{
  "doctypeid": 1,
  "doctypename": "شهادة الضريبة - Tax Certificate",
  "doccode": "TAX_CERT",
  "description": "Tax registration and compliance certificate",
  "ismandatory": true,
  "renewalperioddays": 365,
  "remark": "Required for all business partners",
  "createTime": "2024-01-15T10:00:00",
  "status": "0"
}
```

**Document Types:**

| Document | Code | Mandatory | Renewal | Purpose |
|----------|------|-----------|---------|---------|
| Tax Certificate | TAX_CERT | Yes | Yearly | Tax compliance proof |
| Commercial License | LICENSE | Yes | Yearly | Business authorization |
| Insurance Certificate | INSURANCE | Yes | Yearly | Coverage verification |
| Contract | CONTRACT | Yes | 2 years | Service agreement |
| Registration Certificate | REGISTRATION | Yes | Yearly | Official registration |
| Financial Statements | FINANCIAL | No | Yearly | Financial assessment |
| Compliance Certificate | COMPLIANCE | No | Yearly | Compliance proof |
| Quality Certificate | QUALITY | No | 2 years | Quality standards |

---

### 7. partner_documents.json
**Purpose:** Stores actual partner documents with references to document types.

**Records:** 10 sample partner documents

**Data Categories:**
- Document identification and linking
- Issue and expiry date tracking
- Issuing authority information
- Document file references
- Compliance status tracking

**Schema:**
```json
{
  "documentid": 1,
  "partnerid": 1,
  "doctypeid": 1,
  "documentname": "شهادة ضريبة الدخل - Tax Compliance Certificate",
  "documentpath": "/partner_documents/tax_cert_alnoor_2024.pdf",
  "issuedate": "2024-01-10T00:00:00",
  "expirydate": "2025-01-10T00:00:00",
  "issuedby": "الهيئة العامة للزكاة والدخل",
  "documentnumber": "300123456700003",
  "status": "0",
  "remark": "Current tax certificate",
  "createTime": "2024-01-15T10:00:00"
}
```

**Sample Documents:**
- 3 documents for Al-Noor Distribution (Tax, License, Insurance)
- 2 documents for Modern Tech (Contract, Tax)
- 2 documents for Quality Manufacturing (Registration, Tax)
- 2 documents for Global Consultancy (ISO Certification, Financial)
- 1 document for Accounting Services (License, Compliance)

---

## Integration with SeedDataService

### Methods Added

#### Entity-Level Methods (7 total)

1. **InitBpBusinesspartnertypeData()**
   - Initializes partner types
   - WhereColumns: Partnertypeid

2. **InitBpBusinesspartnerstatusData()**
   - Initializes partner statuses
   - WhereColumns: Statusid

3. **InitBpBusinesspartnerkindData()**
   - Initializes partner kinds
   - WhereColumns: Kindid

4. **InitBpContactmethodData()**
   - Initializes contact methods
   - WhereColumns: Methodid

5. **InitBpCompliancestatusData()**
   - Initializes compliance statuses
   - WhereColumns: Compliancestatusid

6. **InitBpBusinesspartnerdoctypeData()**
   - Initializes document types
   - WhereColumns: Doctypeid

7. **InitBpPartnerdocsementsData()**
   - Initializes partner documents
   - WhereColumns: Documentid

#### Orchestration Method (1)

8. **InitPartnertypeSeedData()**
   - Main orchestration method
   - Coordinates all Partnertype data loading
   - Supports Excel to JSON conversion
   - Automatic fallback mechanism
   - Proper deletion order for clean operations

### Deletion Order (when clean = true)

1. BpPartnerdocsments (documents first)
2. BpBusinesspartnerdoctype
3. BpCompliancestatus
4. BpContactmethod
5. BpBusinesspartnerkind
6. BpBusinesspartnerstatus
7. BpBusinesspartnertype

---

## Data Relationships

```
BusinessPartner (external table)
├── Partner Type (BpBusinesspartnertype)
├── Partner Status (BpBusinesspartnerstatus)
├── Partner Kind (BpBusinesspartnerkind)
├── Contact Method (BpContactmethod)
├── Compliance Status (BpCompliancestatus)
└── Partner Documents (BpPartnerdocsments)
    └── Document Type (BpBusinesspartnerdoctype)
```

---

## Usage Example

```csharp
var seedService = serviceProvider.GetRequiredService<SeedDataService>();
var results = seedService.InitPartnertypeSeedData(
    path: "/path/to/partnertype_seedata.xlsx",
    WebRootPath: webRootPath,
    clean: true  // Clear existing data first
);

foreach (var message in results)
{
    logger.LogInformation(message);
}
```

---

## Bilingual Support
- All text is provided in both Arabic and English
- Format: "Arabic Name - English Name"
- Supports regional requirements
- Saudi Arabia specific formats used throughout

---

## Saudi Arabia Specific Data

### VAT ID Format
- 15-digit number
- Format: 300XXXXXXXXXXX
- Example: 300123456700003

### Commercial Registration Number
- Format: 1010XXXXXX
- Issued by Ministry of Commerce
- Example: 1010123456

### Phone Number Format
- International: +966 (country code)
- Mobile: +966 50X XXXXXX
- Landline: +966 11 XXXXXXX

### Currency
- Default: SAR (Saudi Arabian Riyal)
- Used for financial tracking

---

## Excel Import Format

### Expected Sheet Names
- partner_types
- partner_status
- partner_kind
- contact_methods
- compliance_status
- partner_doc_types
- partner_documents

### Column Mapping
Automatic mapping from:
- JSON property names → Excel column headers
- PascalCase → snake_case conversion
- Type conversions handled automatically

---

## Export/Import

### Excel Format
Expected sheet names for Excel import (as listed above)

### JSON Format
Automatic JSON backup files created:
- partner_types.json
- partner_status.json
- partner_kind.json
- contact_methods.json
- compliance_status.json
- partner_doc_types.json
- partner_documents.json

---

## Customization

### Adding New Partner Types
1. Add to partner_types.json
2. Set unique partnertypeid
3. Update remarks as needed
4. Reload seed data

### Adding New Compliance Status
1. Add to compliance_status.json
2. Define requirements and actions
3. Set displayorder for UI sorting
4. Reload seed data

### Adding Partner Documents
1. Add to partner_documents.json
2. Link to partner and document type
3. Set expiry date for renewal tracking
4. Update document path
5. Reload seed data

---

## Status Codes
- "0" = Active (نشط)
- "1" = Inactive (غير نشط)

---

## Performance Metrics
- 7 entity types with comprehensive data
- 40+ total records across all types
- Hierarchical relationships supported
- Efficient bulk operations

---

## Error Handling
The system includes:
- Graceful Excel import failure handling
- JSON fallback when Excel unavailable
- Detailed error messaging
- Transaction support for data integrity

---

## Future Enhancements
- Partner performance scoring
- Historical document tracking
- Automated compliance reminders
- Document expiry notifications
- SLA management
- Relationship scoring algorithms
- Financial metrics tracking
- Service level monitoring

---

## Maintenance Tasks

### Regular Review
- Quarterly compliance review
- Annual renewal verification
- Inactive partner cleanup
- Document expiry monitoring

### Common Updates
- New partner types
- Status changes
- Document additions
- Compliance updates
- Contact method changes

---

---

**Last Updated:** 2024-10-21
**Version:** 1.0
**Status:** Production Ready

