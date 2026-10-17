# BusinessPartnerData Module Seed Data Documentation

## Overview
This directory contains comprehensive seed data for the BusinessPartnerData module, which manages all external business partner relationships, including vendors, contractors, suppliers, consultants, and service providers. The BusinessPartnerData module provides complete partner lifecycle management from lead qualification through active partnership.

## Files Structure

### business_partners.json
**Purpose:** Stores comprehensive business partner information and relationship data.

**Records:** 5 sample business partners across different industries

**Data Categories:**
- Partner identification and classification
- Contact and location information
- Business details (VAT, CR numbers)
- Financial information (budget ranges)
- Compliance and status tracking

## Sample Partners

### 1. Al-Noor Distribution Co.
- **Status:** Active (Rented)
- **Stage:** Active
- **Type:** Logistics & Distribution
- **Industry:** Commercial Distribution
- **Contact:** Ahmed Al-Shammari (+966501234567)
- **Budget Range:** 100,000 - 500,000 SAR
- **City:** Riyadh
- **Compliance:** Approved ✓

### 2. Modern Tech Co.
- **Status:** Active (Rented)
- **Stage:** Active
- **Type:** Information Technology
- **Industry:** Software Development
- **Contact:** Mohammad Al-Otaibi (+966502345678)
- **Budget Range:** 250,000 - 1,000,000 SAR
- **City:** Jeddah
- **Compliance:** Approved ✓

### 3. Quality Manufacturing
- **Status:** Qualified
- **Stage:** Lead
- **Type:** Food Manufacturing
- **Industry:** Food Production
- **Contact:** Khaled Al-Subai (+966503456789)
- **Budget Range:** 50,000 - 200,000 SAR
- **City:** Dammam
- **Compliance:** Pending ⏳

### 4. Global Consultancy
- **Status:** Active (Qualified)
- **Stage:** Qualified
- **Type:** Consulting & Training
- **Industry:** HR Consulting
- **Contact:** Fatima Al-Zahrani (+966504567890)
- **Budget Range:** 75,000 - 300,000 SAR
- **City:** Riyadh
- **Compliance:** Approved ✓

### 5. Accounting Services
- **Status:** Active (Rented)
- **Stage:** Active
- **Type:** Financial Services
- **Industry:** Accounting & Audit
- **Contact:** Omar Al-Qahtani (+966505678901)
- **Budget Range:** 150,000 - 500,000 SAR
- **City:** Riyadh
- **Compliance:** Approved ✓

## Data Schema

```json
{
  "partnerid": 1,
  "type": 1,                                    // Partner type ID
  "status": 2,                                  // Current status
  "kind": 1,                                    // Partner kind/category
  "assigneddepartment": 3,                      // Department ID
  "assignedto": 201,                            // User/Manager ID
  "entityid": null,
  "legacyid": null,
  "partnerstage": "active",                     // Lead/Qualified/Active
  "commercialname": "Company Name",             // Business trading name
  "legalname": "Legal Entity Name",             // Official registered name
  "vatid": "300123456700003",                   // VAT registration number
  "crnumber": "1010123456",                     // Commercial registration
  "industry": "Industry Type",
  "subindustry": "Sub-Industry Type",
  "contactfirstname": "Contact First Name",
  "contactlastname": "Contact Last Name",
  "businessphone": "+966112345678",             // Main business phone
  "contactphone": "+966501234567",              // Contact person phone
  "businessemail": "info@company.sa",           // Business email
  "contactemail": "contact@company.sa",         // Contact email
  "mobile": "+966501234567",                    // Mobile number
  "buildingnumber": "123",
  "streetaddress": "Street Name",
  "unitnumber": "A",
  "city": "City Name",
  "province": "Province Name",
  "postalcode": "11411",
  "country": "Saudi Arabia",
  "extraAttributes": "{}",                      // JSON for extended data
  "compliancestatus": "approved",               // approved/pending/rejected
  "preferredcontactmethod": 1,                  // Contact method ID
  "budgetRange": "{\"min\": 100000, \"max\": 500000, \"currency\": \"SAR\"}",
  "imageFiles": "/images/partners/logo.jpg",
  "remark": "Partner notes",
  "createTime": "2024-01-15T10:00:00",
  "status": "0"                                 // 0=Active, 1=Inactive
}
```

## Key Fields Explained

### Partner Identification
- **partnerid:** Unique partner identifier (Primary Key)
- **commercialname:** Trading/business name (Arabic & English)
- **legalname:** Official registered business name
- **remark:** Internal notes about the partner

### Classification
- **type:** Partner type (1=Vendor, 2=Supplier, 3=Contractor, etc.)
- **kind:** Partner category/classification
- **stage:** Partnership stage (lead/qualified/active)
- **status:** Current status code

### Legal & Compliance
- **vatid:** VAT registration number (Saudi Arabia format)
- **crnumber:** Commercial registration number
- **compliancestatus:** Compliance approval state
  - "approved" - Fully compliant
  - "pending" - Under review
  - "rejected" - Does not meet criteria

### Contact Information
- **contactfirstname/contactlastname:** Primary contact person
- **businessphone:** Main office number
- **contactphone:** Contact person's phone
- **businessemail:** Company email
- **contactemail:** Contact person's email
- **mobile:** Cell phone number
- **preferredcontactmethod:** Preferred communication method ID

### Location & Address
- **city:** City/Municipality
- **province:** Region/Province
- **buildingnumber:** Building number
- **streetaddress:** Street name/address
- **unitnumber:** Unit/Suite number
- **postalcode:** ZIP/postal code
- **country:** Country (default: Saudi Arabia)

### Business Details
- **industry:** Primary industry
- **subindustry:** Specific industry segment
- **budgetRange:** JSON with min/max amounts in SAR
- **assigneddepartment:** Internal department ID
- **assignedto:** Assigned manager/user ID

### Media & Attributes
- **imageFiles:** Company logo or profile image URL
- **extraAttributes:** Additional JSON data fields

## Partner Stage Lifecycle

```
LEAD → QUALIFIED → ACTIVE → INACTIVE
  ↓        ↓          ↓         ↓
Prospect  Evaluated  Working   Terminated
```

### Stage Definitions
1. **Lead** (محتمل)
   - Initial contact/prospect
   - Not yet evaluated
   - Example: Quality Manufacturing

2. **Qualified** (مؤهل)
   - Passed initial screening
   - Under evaluation
   - Example: Global Consultancy

3. **Active** (نشط)
   - Approved and engaged
   - Regular business relationship
   - Example: All other partners

4. **Inactive** (غير نشط)
   - Relationship ended
   - No longer doing business

## Compliance Status States

| Status | Description | Action Required |
|--------|-------------|-----------------|
| approved | Partner is compliant | Can proceed with business |
| pending | Under compliance review | Awaiting documentation |
| rejected | Does not meet requirements | Cannot do business |

## Integration with SeedDataService

### Method: InitBpBusinesspartnerData()
```csharp
public (string, object, object) InitBpBusinesspartnerData(List<BpBusinesspartner> data)
{
    var db = DbScoped.SugarScope;
    var x = db.Storageable(data)
        .WhereColumns(it => it.Partnerid)
        .ToStorage();
    x.AsInsertable.ExecuteCommand();
    x.AsUpdateable.ExecuteCommand();

    string msg = $"[Business Partners] Inserted {x.InsertList.Count} Updated {x.UpdateList.Count}";
    return (msg, x.ErrorList, x.IgnoreList);
}
```

### Method: InitBusinessPartnerDataSeedData()
- Main orchestration method
- Loads from Excel or JSON
- Automatic conversion to JSON backup
- Fallback mechanism if Excel fails

## Usage Example

```csharp
var seedService = serviceProvider.GetRequiredService<SeedDataService>();
var results = seedService.InitBusinessPartnerDataSeedData(
    path: "/path/to/businesspartnerdata.xlsx",
    WebRootPath: webRootPath,
    clean: true  // Clear existing data
);
```

## Data Relationships

### Hierarchical Structure
```
BusinessPartner
├── Department (assigneddepartment)
├── Manager/User (assignedto)
├── Compliance Status
├── Contact Information
└── Financial Data (budgetRange)
```

## Saudi Arabia Specific Data

### VAT ID Format
- 15-digit number
- Format: 300XXXXXXXXXXX
- Sample: 300123456700003

### CR Number Format
- Commercial registration number
- Format: 1010XXXXXX
- Issued by Ministry of Commerce

### Phone Number Format
- International: +966 (country code)
- Mobile: +966 50X XXXXXX
- Landline: +966 11 XXXXXXX

### Currency
- Default: SAR (Saudi Arabian Riyal)
- Used for budget ranges and financial data

## Bilingual Support
- All text fields support Arabic and English
- Format: "Arabic Text - English Text"
- Regional and international requirements met

## Export/Import

### Excel Format
Expected sheet name:
- business_partners

Column mapping from JSON properties

### JSON Format
File: business_partners.json
- Automatic backup after Excel import
- Fallback for re-initialization
- Can be edited and reloaded manually

## Customization

### Adding New Partners
1. Add record to business_partners.json
2. Set appropriate type, kind, and stage
3. Complete all contact information
4. Set compliance status and budget range
5. Reload seed data

### Partner Type Categories
- Type 1: Vendor/Supplier
- Type 2: Contractor
- Type 3: Service Provider
- Type 4: Consultant
- Type 5: Other

### Partner Kind Categories
- Kind 1: Logistics/Distribution
- Kind 2: Technology
- Kind 3: Manufacturing
- Kind 4: Consulting
- Kind 5: Financial Services

## Status Codes
- "0" = Active (نشط)
- "1" = Inactive (غير نشط)

## Error Handling
- Graceful Excel import failures
- Automatic JSON fallback
- Detailed error messages
- Transaction support

## Performance Metrics
- 5 sample partners
- Multiple industry types
- Realistic contact information
- Complete compliance tracking

## Future Enhancements
- Partner performance tracking
- Historical data maintenance
- Relationship scoring
- Automated compliance reminders
- Partner portal integration
- Financial transaction history
- Service level agreements (SLAs)
- Contract management

---

**Last Updated:** 2024-10-21
**Version:** 1.0
**Status:** Production Ready
