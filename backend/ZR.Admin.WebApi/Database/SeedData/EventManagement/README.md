# Event Management Seed Data

This directory contains JSON seed data files for the Event Management System.

## Overview

The seed data system supports both Excel and JSON formats:
- **Primary**: Excel file with multiple sheets
- **Fallback**: JSON files (used when Excel sheet is missing)
- **Auto-export**: When Excel data is successfully loaded, it's automatically saved to JSON

## File Structure

### Lookup Tables (Type/Reference Data)
- `event_types.json` - Event types (conference, exhibition, festival, etc.)
- `zone_types.json` - Zone types (VIP, general, food, exhibition, etc.)
- `gate_types.json` - Gate types (entry, exit, VIP, emergency, etc.)
- `visitor_types.json` - Visitor types (regular, VIP, speaker, media, etc.)
- `ticket_types.json` - Ticket types (free, general, VIP, premium, etc.)
- `permit_types.json` - Permit types (staff, vendor, contractor, etc.)
- `booth_types.json` - Booth types (standard, premium, food, retail, etc.)
- `incident_types.json` - Incident types (security, medical, technical, etc.)
- `payment_methods.json` - Payment methods (cash, card, Mada, STC Pay, etc.)

### Main Entity Tables
- `events.json` - Event main records
- `event_zones.json` - Event zones/areas
- `event_gates.json` - Event gates
- `event_visitors.json` - Event visitors
- `event_tickets.json` - Event tickets
- `event_permits.json` - Event permits
- `event_booths.json` - Event booths/stalls
- `event_tenants.json` - Booth tenants
- `event_contracts.json` - Rental contracts
- `event_incidents.json` - Incidents/issues
- `event_payments.json` - Payment transactions
- `event_invoices.json` - Invoices

## Usage

### Using Excel File
```csharp
var service = new SeedDataService();
var results = service.InitEventManagementSeedData("path/to/excel.xlsx", clean: true);
```

### Using JSON Files (Automatic Fallback)
If Excel sheets are missing, the system will automatically load from JSON files in this directory.

### Generating JSON from Excel
When you successfully load data from Excel, JSON files are automatically created/updated in this directory as backups.

## Data Format

All JSON files follow the same structure:
```json
[
  {
    "property1": "value1",
    "property2": "value2",
    ...
  }
]
```

Properties use camelCase naming convention and support:
- Arabic text (UTF-8 encoding)
- Emojis and special characters
- Null values for optional fields
- Boolean, number, and string types

## Sample Data

All files in this directory contain sample/demo data based on the EVENT_MANAGEMENT_DATABASE_SCHEMA.md documentation. You can modify these files to match your specific needs.

## Notes

- JSON files are automatically created when Excel data is loaded
- JSON fallback ensures system works even without Excel files
- All files use UTF-8 encoding with Arabic support
- Boolean fields: `true` / `false` (lowercase)
- Decimal numbers: use `.` (dot) as decimal separator
- Dates: ISO 8601 format recommended: `"2025-03-01T00:00:00"`

## Customization

To customize seed data:
1. Edit the JSON files directly, or
2. Prepare Excel file with appropriate sheets, or
3. Use both (Excel takes priority, JSON as fallback)

---

**للمطورين العرب:**
هذه الملفات تحتوي على بيانات أولية لنظام إدارة الفعاليات. يمكنك تعديلها حسب احتياجاتك، ويدعم النظام اللغة العربية بالكامل.
