-- Lead Qualification Fields Migration
-- Run this on the ONLINE PostgreSQL database

-- 1. Add brand image column
ALTER TABLE bp_lead ADD COLUMN IF NOT EXISTS brandimage VARCHAR(500);

-- 2. Make owneremail nullable (fixes "email is required" error)
ALTER TABLE bp_lead ALTER COLUMN owneremail DROP NOT NULL;

-- 3. Add qualification columns
ALTER TABLE bp_lead ADD COLUMN IF NOT EXISTS companyname VARCHAR(200);
ALTER TABLE bp_lead ADD COLUMN IF NOT EXISTS activitytype VARCHAR(200);
ALTER TABLE bp_lead ADD COLUMN IF NOT EXISTS interesting TEXT;
ALTER TABLE bp_lead ADD COLUMN IF NOT EXISTS locationinterested TEXT;
ALTER TABLE bp_lead ADD COLUMN IF NOT EXISTS contracttypeinterested VARCHAR(50);

-- 4. Add lead_id, brand_name, brand_image columns to arap_customer if not exist
ALTER TABLE arap_customer ADD COLUMN IF NOT EXISTS lead_id BIGINT;
ALTER TABLE arap_customer ADD COLUMN IF NOT EXISTS brand_name VARCHAR(200);
ALTER TABLE arap_customer ADD COLUMN IF NOT EXISTS brand_image VARCHAR(500);

-- 5. Back-fill: Create customer records for existing Confirmed leads that have no customer
INSERT INTO arap_customer (
    "Id", "CustomerCode", "NameAr", "NameEn", "ContactPerson", "Email", "Phone",
    "CreditLimit", "IsActive", credit_days, lead_id, brand_name, brand_image,
    "Create_by", "Create_time"
)
SELECT
    gen_random_uuid(),
    'CUS-' || EXTRACT(YEAR FROM NOW())::TEXT || '-' || LPAD((ROW_NUMBER() OVER (ORDER BY l.leadid) + COALESCE(
        (SELECT MAX(SUBSTRING("CustomerCode" FROM 'CUS-\d{4}-(\d+)')::INT) FROM arap_customer WHERE "CustomerCode" LIKE 'CUS-' || EXTRACT(YEAR FROM NOW())::TEXT || '-%'),
        0
    ))::TEXT, 5, '0'),
    COALESCE(l.brandname, l.ownername),
    COALESCE(l.brandname, l.ownername),
    l.ownername,
    l.owneremail,
    l.ownerphone,
    0,
    true,
    30,
    l.leadid,
    l.brandname,
    l.brandimage,
    'System',
    NOW()
FROM bp_lead l
WHERE l.leadstatus = 'Confirmed'
  AND NOT EXISTS (
    SELECT 1 FROM arap_customer c WHERE c.lead_id = l.leadid
  );
