# Boulevard Seed Data Documentation

## Overview
This directory contains seed data JSON files for the Boulevard module - a comprehensive retail and shopping complex management system. The Boulevard module is responsible for managing shops, seasons, zones, shop categories, statuses, and customer registrations within a multi-tenant retail environment.

## Files Structure

### 1. boulevard_shop_categories.json
**Purpose:** Defines the different shop categories available in the Boulevard.

**Sample Data:**
- 8 predefined shop categories
- Each with ID, name, remarks, creation time, and status

**Categories Include:**
- ملابس وأزياء (Clothing & Fashion)
- الإلكترونيات (Electronics)
- الأثاث (Furniture)
- المطاعم والكافيهات (Food & Beverage)
- الكتب والمكتبات (Books)
- الجمال والعناية (Beauty & Care)
- الرياضة واللياقة (Sports & Fitness)
- الألعاب والهدايا (Toys & Gifts)

### 2. boulevard_shop_status.json
**Purpose:** Defines the various operational statuses for shops.

**Sample Data:**
- 5 predefined statuses
- Each representing a shop's operational state

**Statuses Include:**
- متاح (Available) - Shop available for rent
- مؤجر (Rented) - Currently rented
- تحت الصيانة (Under Maintenance) - Under maintenance
- مغلق (Closed) - Closed/Not operational
- قيد التطوير (Under Development) - Being developed/prepared

### 3. boulevard_seasons.json
**Purpose:** Defines promotional and operational seasons for the Boulevard.

**Sample Data:**
- 7 predefined seasons
- Each with start date, end date, name, and remarks

**Seasons Include:**
- الموسم الربيعي 2024 (Spring Season 2024) - Mar 21 - Jun 20
- الموسم الصيفي 2024 (Summer Season 2024) - Jun 21 - Sep 22
- الموسم الخريفي 2024 (Fall Season 2024) - Sep 23 - Dec 20
- الموسم الشتوي 2024-2025 (Winter Season 2024-2025) - Dec 21 - Mar 20
- موسم التنزيلات الصيفي (Summer Sales Season) - Jul 1 - Aug 31
- موسم العودة للمدراس (Back to School) - Aug 15 - Sep 15
- موسم الأعياد والاحتفالات (Holiday Season) - Nov 1 - Jan 31

### 4. boulevard_season_zones.json
**Purpose:** Defines geographical zones within the Boulevard for each season.

**Sample Data:**
- 8 predefined zones
- Each with name, image file reference, and remarks

**Zones Include:**
- المنطقة الشمالية (North Zone)
- المنطقة الجنوبية (South Zone)
- المنطقة الشرقية (East Zone)
- المنطقة الغربية (West Zone)
- المنطقة الوسطى (Central Zone)
- منطقة الطعام والشراب (F&B Zone)
- منطقة الترفيه (Entertainment Zone)
- منطقة الإلكترونيات (Tech Zone)

### 5. boulevard_shops.json
**Purpose:** Defines individual shops within the Boulevard with detailed business information.

**Sample Data:**
- 10 comprehensive shop records
- Each with name, zone, status, category, area, pricing, and images

**Shop Information Fields:**
- id: Unique identifier
- name: Shop name (Arabic & English)
- zoneId: Assigned zone
- shopStatus: Current operational status
- shopCategory: Shop category
- shopArea: Square meters (m²)
- meterPrice: Price per square meter
- totalPrice: Total rental price
- imageFiles: Shop image URL
- remark: Additional notes
- createTime: Creation timestamp
- status: Active/Inactive flag

**Sample Shops:**
1. متجر الملابس الفاخرة (Luxury Fashion Store) - 150.5 m², Zone 1, Status: Rented
2. متجر الإلكترونيات الحديثة (Modern Electronics) - 200 m², Zone 8, Status: Rented
3. متجر الأثاث الراقي (Premium Furniture) - 250 m², Zone 2, Status: Rented
4. مطعم النخيل (Al Nakhal Restaurant) - 300 m², Zone 6, Status: Rented
5. مكتبة العلم والمعرفة (Knowledge Library) - 120 m², Zone 3, Status: Rented
6. صالون الجمال الراقي (Luxury Beauty Salon) - 100 m², Zone 4, Status: Rented
7. نادي اللياقة البدنية (Fitness Club) - 400 m², Zone 5, Status: Rented
8. متجر الألعاب والهدايا (Toys & Gifts) - 80 m², Zone 7, Status: Available
9. كافيه الرومانسية (Romance Cafe) - 180 m², Zone 6, Status: Available
10. متجر الملابس الرياضية (Sports Wear) - 140 m², Zone 1, Status: Available

### 6. boulevard_customer_registrations.json
**Purpose:** Stores customer registration and contact information.

**Sample Data:**
- 10 customer registration records
- Each with contact details and preferences

**Customer Fields:**
- id: Unique identifier
- name: Customer name (Arabic & English)
- phone: Contact phone number
- email: Email address
- notes: Customer preferences/interests
- remark: Classification (VIP, Regular, etc.)
- createTime: Registration timestamp
- status: Active/Inactive

**Sample Customers:**
- Arabic and English names for bilingual support
- Saudi Arabian phone numbers (+966)
- Various customer categories (VIP, Regular, New, Special Offers)
- Customer interests aligned with shop categories

## Integration with SeedDataService

### Methods Added to SeedDataService.cs

1. **InitBlvShopCategoryData(List<BlvBoulevardShopCategory> data)**
   - Initializes shop categories
   - Supports insert and update operations
   - Handles upsert logic

2. **InitBlvShopStatusData(List<BlvBoulevardShopStatus> data)**
   - Initializes shop statuses
   - Supports insert and update operations

3. **InitBlvSeasonZonesData(List<BlvBoulevardSeasonZones> data)**
   - Initializes season zones
   - Supports insert and update operations

4. **InitBlvSeasonsData(List<BlvBoulevardSeasons> data)**
   - Initializes seasons
   - Supports insert and update operations

5. **InitBlvShopsData(List<BlvBoulevardShops> data)**
   - Initializes individual shops
   - Supports insert and update operations

6. **InitBlvCustomerRegistrationData(List<BlvCustomerregistrationform> data)**
   - Initializes customer registrations
   - Supports insert and update operations

7. **InitBoulevardSeedData(string path, string WebRootPath, bool clean)**
   - Main orchestration method
   - Loads data from Excel or JSON files
   - Handles exceptions gracefully
   - Falls back to JSON if Excel sheet is missing
   - Automatically saves to JSON format

## Usage

### Automatic Loading
The Boulevard seed data is automatically loaded when `InitSeedData()` is called:

```csharp
public List<string> InitSeedData(string path, string WebRootPath, bool clean)
{
    // ... other seed data initialization ...

    // Initialize Boulevard Seed Data
    var boulevardData = InitBoulevardSeedData(path, WebRootPath, clean);
    if (boulevardData is not null && boulevardData.Count > 0)
    {
        result.AddRange(boulevardData);
    }

    return result;
}
```

### Excel to JSON Conversion
The system automatically:
1. Reads data from Excel sheets (if available)
2. Saves the data to JSON files for backup
3. Falls back to JSON files if Excel sheets are missing

**Sheet Names Expected:**
- boulevard_shop_categories
- boulevard_shop_status
- boulevard_seasons
- boulevard_season_zones
- boulevard_shops
- boulevard_customer_registrations

### Data Loading Order
1. Shop Categories (parent data)
2. Shop Status (reference data)
3. Seasons
4. Season Zones
5. Shops (depends on categories, status, zones)
6. Customer Registrations

## Data Integrity

### Foreign Key Relationships
```
Shop ├─> ShopCategory (shopCategory field)
     ├─> ShopStatus (shopStatus field)
     └─> SeasonZone (zoneId field)

CustomerRegistration └─> No dependencies (standalone)
```

### Delete Order (when clean = true)
Data is deleted in reverse order to maintain referential integrity:
1. Boulevard Shops (highest dependency)
2. Boulevard Season Zones
3. Boulevard Seasons
4. Boulevard Shop Categories
5. Boulevard Shop Status
6. Boulevard Customer Registrations (no dependencies)

## Bilingual Support
- All data supports both Arabic and English names
- Sample data uses bilingual entries: "Arabic - English"
- Suitable for Arabic-speaking regions

## Remarks/Comments
Each entity includes a remark field for:
- Internal notes about the entity
- Purpose or special characteristics
- Classification information
- Audit trail information

## Status Field
All entities include a status field (typically "0" for active):
- "0" = Active
- "1" or other values = Inactive/Deleted

## Path Configuration
- **JSON Base Path:** `{WebRootPath}/Database/SeedData/Boulevard/`
- **File Naming:** `boulevard_{entity_name}.json`
- **Example:** `F:\mahamBackendNET\backend\ZR.Admin.WebApi\Database\SeedData\Boulevard\boulevard_shops.json`

## Error Handling
The system includes robust error handling:
- Gracefully handles missing Excel sheets
- Falls back to JSON files automatically
- Reports errors in the output message
- Continues processing even if one sheet fails

## Performance Considerations
- JSON files allow offline initialization
- No need for Excel file presence after first initialization
- Cached JSON files speed up subsequent operations
- Batch insert/update operations for efficiency

## Future Extensions
The Boulevard module can be extended with:
- Shop rental rates and pricing history
- Tenant information and contracts
- Seasonal promotions and discounts
- Shop inspection and maintenance logs
- Customer loyalty programs
- Sales and occupancy analytics

---
**Last Updated:** 2024-10-21
**Version:** 1.0
**Status:** Production Ready
