-- ============================================================
-- Migration: Add full_address, secondary_phone, categories
--            to dm_contact + create dm_category lookup table
-- Date: 2026-02-26
-- ============================================================

-- 1. New columns on dm_contact
ALTER TABLE dm_contact ADD COLUMN IF NOT EXISTS full_address VARCHAR(500) NULL;
ALTER TABLE dm_contact ADD COLUMN IF NOT EXISTS secondary_phone VARCHAR(50) NULL;
ALTER TABLE dm_contact ADD COLUMN IF NOT EXISTS categories VARCHAR(500) NULL;

COMMENT ON COLUMN dm_contact.full_address IS 'Full street address imported from Excel';
COMMENT ON COLUMN dm_contact.secondary_phone IS 'Second phone number when Excel row has two different numbers';
COMMENT ON COLUMN dm_contact.categories IS 'Comma-separated category names (e.g. Coffee,F&B,Restaurant)';

-- 2. Category lookup table (inherits SysBase audit pattern)
CREATE TABLE IF NOT EXISTS dm_category (
    category_id   BIGINT PRIMARY KEY,
    name          VARCHAR(200) NOT NULL,
    name_ar       VARCHAR(200) NULL,
    is_active     BOOLEAN DEFAULT true,
    sort_order    INT DEFAULT 0,
    create_by     VARCHAR(64)  NULL,
    create_time   TIMESTAMP    DEFAULT NOW(),
    update_by     VARCHAR(64)  NULL,
    update_time   TIMESTAMP    NULL
);

COMMENT ON TABLE dm_category IS 'Activity categories for Data Mine contacts';

-- 3. Seed default categories
INSERT INTO dm_category (category_id, name, name_ar, sort_order, create_by, create_time)
VALUES
    -- Food & Beverage
    (1,  'Coffee',                'قهوة',                  1,  'system', NOW()),
    (2,  'Restaurant',            'مطعم',                  2,  'system', NOW()),
    (3,  'Fast Food',             'وجبات سريعة',           3,  'system', NOW()),
    (4,  'Bakery & Pastry',       'مخبوزات وحلويات',       4,  'system', NOW()),
    (5,  'Juice & Smoothie',      'عصائر وسموذي',          5,  'system', NOW()),
    (6,  'Ice Cream & Dessert',   'آيس كريم وحلويات',      6,  'system', NOW()),
    (7,  'Catering',              'تموين وضيافة',          7,  'system', NOW()),
    -- Retail & Shopping
    (8,  'Fashion & Apparel',     'أزياء وملابس',          8,  'system', NOW()),
    (9,  'Jewelry & Watches',     'مجوهرات وساعات',        9,  'system', NOW()),
    (10, 'Perfume & Cosmetics',   'عطور ومستحضرات تجميل',  10, 'system', NOW()),
    (11, 'Electronics',           'إلكترونيات',            11, 'system', NOW()),
    (12, 'Mobile & Accessories',  'جوالات وإكسسوارات',     12, 'system', NOW()),
    (13, 'Home & Furniture',      'أثاث ومفروشات',         13, 'system', NOW()),
    (14, 'Gifts & Novelties',     'هدايا وتحف',            14, 'system', NOW()),
    (15, 'Books & Stationery',    'كتب وقرطاسية',          15, 'system', NOW()),
    (16, 'Sports & Fitness',      'رياضة ولياقة',          16, 'system', NOW()),
    (17, 'Eyewear & Optics',      'نظارات وبصريات',        17, 'system', NOW()),
    (18, 'Toys & Kids',           'ألعاب وأطفال',          18, 'system', NOW()),
    -- Services
    (19, 'Salon & Barbershop',    'صالون وحلاقة',          19, 'system', NOW()),
    (20, 'Spa & Wellness',        'سبا وعناية',            20, 'system', NOW()),
    (21, 'Health & Medical',      'صحة وطب',               21, 'system', NOW()),
    (22, 'Education & Training',  'تعليم وتدريب',          22, 'system', NOW()),
    (23, 'Travel & Tourism',      'سفر وسياحة',            23, 'system', NOW()),
    (24, 'Financial Services',    'خدمات مالية',           24, 'system', NOW()),
    (25, 'Telecom',               'اتصالات',               25, 'system', NOW()),
    (26, 'Laundry & Dry Clean',   'غسيل وتنظيف',           26, 'system', NOW()),
    -- Entertainment & Leisure
    (27, 'Entertainment',         'ترفيه',                 27, 'system', NOW()),
    (28, 'Cinema',                'سينما',                 28, 'system', NOW()),
    (29, 'Gaming & Arcade',       'ألعاب إلكترونية',       29, 'system', NOW()),
    (30, 'Events & Exhibitions',  'فعاليات ومعارض',        30, 'system', NOW()),
    -- Automotive & Other
    (31, 'Automotive',            'سيارات',                31, 'system', NOW()),
    (32, 'Real Estate',           'عقارات',                32, 'system', NOW()),
    (33, 'Grocery & Supermarket', 'بقالة وسوبرماركت',      33, 'system', NOW()),
    (34, 'Pet Shop',              'حيوانات أليفة',         34, 'system', NOW()),
    (35, 'Flower & Plant',        'ورد ونباتات',           35, 'system', NOW()),
    (36, 'Other',                 'أخرى',                  36, 'system', NOW())
ON CONFLICT (category_id) DO NOTHING;
