# Business Module Seed Data Documentation

## Overview
This directory contains comprehensive seed data for the Business module, which handles organizational structures, document types, insurance plans, review types, and performance reviews. The Business module provides essential HR and organizational management capabilities.

## Files Structure

### 1. organization_types.json
**Purpose:** Defines organizational entity types within the system.

**Records:** 5 organization types

**Data Categories:**
- Private Companies
- Public Companies
- Government Agencies
- Non-Profit Organizations
- Contractors & Suppliers

**Schema:**
```json
{
  "organizationtypeid": 1,
  "organizationtypename": "شركة خاصة - Private Company",
  "remark": "Description",
  "createTime": "2024-01-01T00:00:00",
  "status": "0"
}
```

### 2. organizations.json
**Purpose:** Defines organizational departments and hierarchies.

**Records:** 6 organizations with parent-child relationships

**Sample Organizations:**
1. **Al-Ain Modern Co.** (Parent) - Main Company
   - HR Department (Child)
   - Sales & Marketing (Child)
   - IT Department (Child)
   - Operations (Child)
   - Finance (Child)

**Key Features:**
- Hierarchical structure (parentid relationships)
- Display order for UI sorting
- Logo URLs for branding
- Department specialization

**Schema:**
```json
{
  "organizationsid": 1,
  "name": "شركة العين الحديثة - Al-Ain Modern Co.",
  "organizationtypeid": "1",
  "logoUrl": "/images/organizations/logo.png",
  "displayOrder": 1,
  "parentid": 0,
  "remark": "Main organization",
  "createTime": "2024-01-01T00:00:00",
  "status": "0"
}
```

### 3. document_types.json
**Purpose:** Defines document types used throughout the HR system.

**Records:** 8 document types

**Document Types:**
1. Graduation Certificate - University degrees
2. Experience Certificate - Work experience
3. License & Qualifications - Professional licenses
4. Insurance Document - Health insurance docs
5. Employment Contract - Work agreements
6. Medical Report - Health reports
7. Identity Document - ID and residency
8. Salary Certificate - Income verification

**Schema:**
```json
{
  "documenttypeid": 1,
  "documenttypename": "شهادة تخرج - Graduation Certificate",
  "remark": "University degrees and certificates",
  "createTime": "2024-01-01T00:00:00",
  "status": "0"
}
```

### 4. insurance_plans.json
**Purpose:** Defines employee insurance plans with coverage details and premium information.

**Records:** 5 comprehensive insurance plans

**Insurance Plans:**
1. **Basic Plan**
   - Monthly Premium: 450 SAR
   - Employer Contribution: 300 SAR
   - Employee Contribution: 150 SAR
   - Coverage: Comprehensive health insurance

2. **Premium Plan**
   - Monthly Premium: 750 SAR
   - Employer Contribution: 500 SAR
   - Employee Contribution: 250 SAR
   - Coverage: Advanced with specialized treatments

3. **Family Plan**
   - Monthly Premium: 1,200 SAR
   - Employer Contribution: 800 SAR
   - Employee Contribution: 400 SAR
   - Coverage: Employee + Family

4. **Accident Insurance**
   - Monthly Premium: 100 SAR
   - Protection against workplace accidents

5. **Life Insurance**
   - Monthly Premium: 200 SAR
   - Financial protection for family

**Key Fields:**
- Plan name, provider, type
- Coverage level and details
- Monthly premium breakdown
- Employer/employee contributions

**Schema:**
```json
{
  "planid": 1,
  "planname": "الخطة الأساسية - Basic Plan",
  "provider": "Provider Name",
  "plantype": "Health Insurance",
  "coveragelevel": "Comprehensive",
  "coveragedetails": "Full coverage description",
  "monthlypremium": 450.0,
  "employercontribution": 300.0,
  "employeecontribution": 150.0,
  "remark": "Plan description",
  "createTime": "2024-01-01T00:00:00",
  "status": "0"
}
```

### 5. review_types.json
**Purpose:** Defines types of performance reviews conducted.

**Records:** 6 review types

**Review Types:**
1. Annual Review - Yearly performance assessment
2. Quarterly Review - Every 3 months
3. Mid-Year Review - 6-month checkpoint
4. Probation Review - New employee evaluation
5. Career Development Review - Promotion/advancement
6. 360 Degree Review - Multi-perspective evaluation

**Schema:**
```json
{
  "reviewtypeid": 1,
  "name": "تقييم سنوي - Annual Review",
  "remark": "Yearly performance assessment",
  "createTime": "2024-01-01T00:00:00",
  "status": "0"
}
```

### 6. performance_reviews.json
**Purpose:** Stores individual performance review records with detailed ratings and feedback.

**Records:** 3 sample performance reviews

**Sample Reviews:**
1. **Annual Review (Excellent)** - 4.2/5.0 overall rating
2. **Mid-Year Review (Good)** - 3.8/5.0 overall rating
3. **Probation Review (Developing)** - 3.3/5.0 overall rating

**Performance Rating Categories:**
- Knowledge Rating
- Productivity Rating
- Quality Rating
- Attendance Rating
- Teamwork Rating
- Leadership Rating
- Communication Rating
- Initiative Rating

**Feedback Fields:**
- Strengths (نقاط القوة)
- Improvements (المجالات المراد تحسينها)
- Goals (الأهداف)
- Comments
- Manager Comments
- Employee Comments
- Career Aspirations
- Training Needs
- Development Areas
- Goals Achieved

**Key Fields:**
```json
{
  "reviewid": 1,
  "duedate": "2024-12-31T00:00:00",
  "reviewyear": 2024,
  "reviewdate": "2024-12-15T10:00:00",
  "reviewquarter": 4,
  "reviewtype": "Annual Review",
  "employeeid": 101,
  "reviewerid": 201,
  "overallrating": 4.2,
  "productivityrating": 4.3,
  "qualityrating": 4.5,
  "attendancerating": 4.8,
  "teamworkrating": 4.1,
  "leadershiprating": 4.0,
  "communicationrating": 4.2,
  "initiativerating": 4.4,
  "status": "Completed",
  "approveddate": "2024-12-20T14:30:00",
  "approvedby": "Executive Manager",
  "createTime": "2024-12-15T10:00:00",
  "status": "0"
}
```

## Integration with SeedDataService

### Methods Added

1. **InitOrganizationtypeData()**
   - Initializes organization types
   - WhereColumns: Organizationtypeid

2. **InitOrganizationsData()**
   - Initializes organizations and departments
   - WhereColumns: Organizationsid
   - Supports hierarchical structure

3. **InitHrDocumenttypeData()**
   - Initializes document types
   - WhereColumns: Documenttypeid

4. **InitHrInsurancePlanData()**
   - Initializes insurance plans
   - WhereColumns: Planid

5. **InitHrReviewtypeData()**
   - Initializes review types
   - WhereColumns: Reviewtypeid

6. **InitHrPerformanceReviewData()**
   - Initializes performance reviews
   - WhereColumns: Reviewid

7. **InitBusinessSeedData()**
   - Main orchestration method
   - Coordinates all Business data loading
   - Supports Excel to JSON conversion

## Data Relationships

```
Organizations
├── Organizationtype (foreign key)
└── Hierarchical Structure (self-referencing)

Performance Reviews
├── Review Types
├── Organizations (via employee assignment)
└── Documents (referenced)

Insurance Plans
└── Organizations
```

## Deletion Order (when clean = true)

1. HrPerformanceReview (highest dependency)
2. HrReviewtype
3. HrInsurancePlan
4. HrDocumenttype
5. Organizations
6. Organizationtype (lowest dependency)

## Usage Example

```csharp
var seedService = serviceProvider.GetRequiredService<SeedDataService>();
var results = seedService.InitBusinessSeedData(
    path: "/path/to/business_seedata.xlsx",
    WebRootPath: webRootPath,
    clean: true  // Clear existing data first
);
```

## Bilingual Support
- All text is provided in both Arabic and English
- Format: "Arabic Name - English Name"
- Supports regional requirements

## Rating Scale
- Performance ratings use 0-5 scale
- 5.0 = Outstanding (ممتاز)
- 4.0 = Excellent (جيد جداً)
- 3.0 = Good (جيد)
- 2.0 = Needs Improvement (يحتاج تحسين)
- 1.0 = Unsatisfactory (غير مرضي)

## Currency & Amounts
- All amounts in SAR (Saudi Arabian Riyal)
- Insurance premiums in SAR/month
- Easy to customize for other currencies

## Export/Import

### Excel Format
Expected sheet names for Excel import:
- organization_types
- organizations
- document_types
- insurance_plans
- review_types
- performance_reviews

### JSON Format
Automatic JSON backup files created:
- organization_types.json
- organizations.json
- document_types.json
- insurance_plans.json
- review_types.json
- performance_reviews.json

## Customization

### Adding New Organization Types
1. Add to organization_types.json
2. Create new organizations with that type
3. Update parent-child relationships as needed

### Adding New Insurance Plans
1. Add to insurance_plans.json
2. Specify coverage levels and contributions
3. Update employee assignments

### Adding Performance Reviews
1. Add to performance_reviews.json
2. Link to employees and reviewers
3. Set review dates and types

## Status Codes
- "0" = Active (نشط)
- "1" = Inactive (غير نشط)
- "2" = Archived (مؤرشف)

## Performance Metrics
- 6 entity types with comprehensive data
- 20+ sample records
- Realistic rating distributions
- Complete organizational hierarchy

## Error Handling
The system includes:
- Graceful Excel import failure handling
- JSON fallback when Excel unavailable
- Detailed error messaging
- Transaction support for data integrity

---

**Last Updated:** 2024-10-21
**Version:** 1.0
**Status:** Production Ready
