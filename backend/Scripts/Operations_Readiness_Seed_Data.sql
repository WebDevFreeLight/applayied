-- ====================================================
-- Seed Data for Operations Readiness Module
-- ====================================================
-- Populates: ops_readiness_category, ops_readiness_checklist,
--            ops_readiness_checklist_item
-- Generated: 2026-02-24
-- ====================================================
-- Safe to re-run: uses ON CONFLICT DO NOTHING
-- Uses fixed Snowflake-like IDs (700xxx range)
-- ====================================================
-- NOTE: SysBase columns (create_by, create_time, etc.)
-- are included. CompanyID/BranchID left NULL for global data.
-- ====================================================

-- ====================================================
-- SECTION 1: READINESS CATEGORIES
-- ====================================================
-- 6 categories covering all store readiness aspects

INSERT INTO ops_readiness_category (
    category_id, category_code, name_en, name_ar, description, icon, color,
    weight, sort_order, is_required, is_active, create_by, create_time
) VALUES
(700001, 'CLEAN', 'Cleanliness & Hygiene', 'النظافة والصحة',
 'Store cleanliness, sanitation, and hygiene standards',
 'sparkles', '#10B981', 1.2, 1, true, true, 'admin', NOW()),

(700002, 'EQUIP', 'Equipment & Systems', 'المعدات والأنظمة',
 'All equipment, machinery, POS systems, and technical infrastructure',
 'wrench', '#3B82F6', 1.5, 2, true, true, 'admin', NOW()),

(700003, 'SAFETY', 'Safety & Compliance', 'السلامة والامتثال',
 'Fire safety, emergency exits, first aid, regulatory compliance',
 'shield-check', '#EF4444', 2.0, 3, true, true, 'admin', NOW()),

(700004, 'BRAND', 'Branding & Visual', 'العلامة التجارية والمظهر',
 'Signage, branding elements, visual merchandising, display standards',
 'palette', '#8B5CF6', 1.0, 4, true, true, 'admin', NOW()),

(700005, 'STAFF', 'Staff Readiness', 'جاهزية الموظفين',
 'Staff training, uniforms, scheduling, and operational readiness',
 'users', '#F59E0B', 1.3, 5, true, true, 'admin', NOW()),

(700006, 'DOCS', 'Documentation & Permits', 'التوثيق والتراخيص',
 'Business licenses, permits, insurance, and required documentation',
 'file-text', '#6366F1', 1.0, 6, true, true, 'admin', NOW())

ON CONFLICT (category_id) DO NOTHING;

-- ====================================================
-- SECTION 2: READINESS CHECKLISTS (1 per category)
-- ====================================================

INSERT INTO ops_readiness_checklist (
    checklist_id, category_id, checklist_code, name_en, name_ar, description,
    applies_to_type, version, is_active, create_by, create_time
) VALUES
(710001, 700001, 'CL-CLEAN-01', 'Daily Cleanliness Checklist', 'قائمة النظافة اليومية',
 'Standard daily cleanliness inspection checklist for all store types',
 NULL, 1, true, 'admin', NOW()),

(710002, 700002, 'CL-EQUIP-01', 'Equipment Readiness Checklist', 'قائمة جاهزية المعدات',
 'Equipment operational status and maintenance verification',
 NULL, 1, true, 'admin', NOW()),

(710003, 700003, 'CL-SAFETY-01', 'Safety Compliance Checklist', 'قائمة الامتثال للسلامة',
 'Fire safety, emergency procedures, and regulatory compliance',
 NULL, 1, true, 'admin', NOW()),

(710004, 700004, 'CL-BRAND-01', 'Brand Standards Checklist', 'قائمة معايير العلامة التجارية',
 'Signage, displays, and visual branding standards',
 NULL, 1, true, 'admin', NOW()),

(710005, 700005, 'CL-STAFF-01', 'Staff Readiness Checklist', 'قائمة جاهزية الموظفين',
 'Staff training, uniforms, and scheduling verification',
 NULL, 1, true, 'admin', NOW()),

(710006, 700006, 'CL-DOCS-01', 'Documentation Checklist', 'قائمة التوثيق',
 'Licenses, permits, and required documentation verification',
 NULL, 1, true, 'admin', NOW())

ON CONFLICT (checklist_id) DO NOTHING;

-- ====================================================
-- SECTION 3: CHECKLIST ITEMS
-- ====================================================

-- ------------------------------------------------
-- 3A: Cleanliness Items (CL-CLEAN-01 → 710001)
-- ------------------------------------------------
INSERT INTO ops_readiness_checklist_item (
    item_id, checklist_id, item_code, description_en, description_ar,
    severity_weight, photo_required, is_mandatory, is_blocking, sort_order, help_text, is_active,
    create_by, create_time
) VALUES
(720001, 710001, 'CLEAN-001', 'Floor is clean and free of debris', 'الأرضية نظيفة وخالية من الأوساخ',
 2, false, true, false, 1, 'Check all floor areas including corners and under shelves', true, 'admin', NOW()),

(720002, 710001, 'CLEAN-002', 'Restrooms are clean and stocked', 'دورات المياه نظيفة ومجهزة',
 3, true, true, true, 2, 'Verify soap, paper towels, and toilet paper are available', true, 'admin', NOW()),

(720003, 710001, 'CLEAN-003', 'Counters and surfaces are sanitized', 'الأسطح والطاولات معقمة',
 2, false, true, false, 3, 'All customer-facing surfaces must be wiped with sanitizer', true, 'admin', NOW()),

(720004, 710001, 'CLEAN-004', 'Trash bins emptied and liners replaced', 'سلات المهملات فارغة والأكياس مستبدلة',
 1, false, true, false, 4, 'All bins should be less than 75% full at inspection time', true, 'admin', NOW()),

-- ------------------------------------------------
-- 3B: Equipment Items (CL-EQUIP-01 → 710002)
-- ------------------------------------------------
(720011, 710002, 'EQUIP-001', 'POS system is operational', 'نظام نقاط البيع يعمل',
 4, false, true, true, 1, 'Test at least one transaction on each POS terminal', true, 'admin', NOW()),

(720012, 710002, 'EQUIP-002', 'Air conditioning is functioning properly', 'التكييف يعمل بشكل صحيح',
 3, false, true, true, 2, 'Temperature should be between 20-24°C', true, 'admin', NOW()),

(720013, 710002, 'EQUIP-003', 'Lighting is fully operational', 'الإضاءة تعمل بالكامل',
 2, true, true, false, 3, 'Check all zones - no burned out or flickering lights', true, 'admin', NOW()),

(720014, 710002, 'EQUIP-004', 'Internet/Wi-Fi connectivity is stable', 'الاتصال بالإنترنت/الواي فاي مستقر',
 3, false, true, true, 4, 'Verify stable connection for POS and operations', true, 'admin', NOW()),

-- ------------------------------------------------
-- 3C: Safety Items (CL-SAFETY-01 → 710003)
-- ------------------------------------------------
(720021, 710003, 'SAFE-001', 'Fire extinguishers present and not expired', 'طفايات الحريق موجودة وغير منتهية الصلاحية',
 4, true, true, true, 1, 'Check expiry date and pressure gauge on each extinguisher', true, 'admin', NOW()),

(720022, 710003, 'SAFE-002', 'Emergency exits are clear and marked', 'مخارج الطوارئ واضحة ومحددة',
 4, true, true, true, 2, 'Exit signs must be illuminated, paths unobstructed', true, 'admin', NOW()),

(720023, 710003, 'SAFE-003', 'First aid kit is stocked and accessible', 'صندوق الإسعافات الأولية مجهز ويمكن الوصول إليه',
 3, false, true, false, 3, 'Verify contents against required checklist inside kit', true, 'admin', NOW()),

(720024, 710003, 'SAFE-004', 'CCTV cameras are operational', 'كاميرات المراقبة تعمل',
 3, false, true, false, 4, 'Verify all cameras are recording and accessible remotely', true, 'admin', NOW()),

-- ------------------------------------------------
-- 3D: Branding Items (CL-BRAND-01 → 710004)
-- ------------------------------------------------
(720031, 710004, 'BRAND-001', 'External signage is clean and illuminated', 'اللافتة الخارجية نظيفة ومضاءة',
 2, true, true, false, 1, 'Check all external brand signs for damage or burnt lights', true, 'admin', NOW()),

(720032, 710004, 'BRAND-002', 'Product displays match planogram', 'عرض المنتجات يطابق المخطط',
 2, false, true, false, 2, 'Compare current display with latest approved planogram', true, 'admin', NOW()),

(720033, 710004, 'BRAND-003', 'Price tags are accurate and visible', 'بطاقات الأسعار دقيقة ومرئية',
 2, false, true, false, 3, 'Spot check at least 10 random items for price accuracy', true, 'admin', NOW()),

-- ------------------------------------------------
-- 3E: Staff Items (CL-STAFF-01 → 710005)
-- ------------------------------------------------
(720041, 710005, 'STAFF-001', 'All staff in proper uniform', 'جميع الموظفين بالزي الموحد',
 2, false, true, false, 1, 'Name badges visible, clean uniform, proper grooming', true, 'admin', NOW()),

(720042, 710005, 'STAFF-002', 'Minimum staffing level met', 'مستوى التوظيف الأدنى متحقق',
 3, false, true, true, 2, 'Verify against scheduled headcount for this shift', true, 'admin', NOW()),

(720043, 710005, 'STAFF-003', 'Staff trained on emergency procedures', 'الموظفون مدربون على إجراءات الطوارئ',
 3, false, true, false, 3, 'Verify training records are up to date (within 6 months)', true, 'admin', NOW()),

-- ------------------------------------------------
-- 3F: Documentation Items (CL-DOCS-01 → 710006)
-- ------------------------------------------------
(720051, 710006, 'DOCS-001', 'Business license displayed and valid', 'الرخصة التجارية معروضة وسارية',
 4, true, true, true, 1, 'Must be displayed in customer-visible area, not expired', true, 'admin', NOW()),

(720052, 710006, 'DOCS-002', 'Municipality permit is current', 'تصريح البلدية ساري المفعول',
 4, true, true, true, 2, 'Check expiry date - must have > 30 days remaining', true, 'admin', NOW()),

(720053, 710006, 'DOCS-003', 'Insurance certificate on file', 'شهادة التأمين محفوظة',
 3, false, true, false, 3, 'Verify liability and property insurance are current', true, 'admin', NOW())

ON CONFLICT (item_id) DO NOTHING;

-- ====================================================
-- VERIFICATION
-- ====================================================
SELECT 'Categories' as entity, COUNT(*) as count FROM ops_readiness_category WHERE category_id BETWEEN 700001 AND 700099
UNION ALL
SELECT 'Checklists', COUNT(*) FROM ops_readiness_checklist WHERE checklist_id BETWEEN 710001 AND 710099
UNION ALL
SELECT 'Checklist Items', COUNT(*) FROM ops_readiness_checklist_item WHERE item_id BETWEEN 720001 AND 720099;

-- Detailed view
SELECT c.name_en as category, cl.name_en as checklist, COUNT(i.item_id) as items
FROM ops_readiness_category c
JOIN ops_readiness_checklist cl ON cl.category_id = c.category_id
LEFT JOIN ops_readiness_checklist_item i ON i.checklist_id = cl.checklist_id
WHERE c.category_id BETWEEN 700001 AND 700099
GROUP BY c.name_en, cl.name_en, c.sort_order
ORDER BY c.sort_order;

-- ====================================================
-- DONE! Summary:
-- ====================================================
--   6 Categories:
--     700001 CLEAN  - Cleanliness & Hygiene (weight: 1.2)
--     700002 EQUIP  - Equipment & Systems (weight: 1.5)
--     700003 SAFETY - Safety & Compliance (weight: 2.0)
--     700004 BRAND  - Branding & Visual (weight: 1.0)
--     700005 STAFF  - Staff Readiness (weight: 1.3)
--     700006 DOCS   - Documentation & Permits (weight: 1.0)
--
--   6 Checklists (1 per category):
--     710001-710006
--
--   23 Checklist Items:
--     720001-720004  Cleanliness (4 items)
--     720011-720014  Equipment (4 items)
--     720021-720024  Safety (4 items)
--     720031-720033  Branding (3 items)
--     720041-720043  Staff (3 items)
--     720051-720053  Documentation (3 items)
--
--   Blocking items (prevent handover if failed):
--     CLEAN-002 Restrooms
--     EQUIP-001 POS System
--     EQUIP-002 Air Conditioning
--     EQUIP-004 Internet
--     SAFE-001  Fire Extinguishers
--     SAFE-002  Emergency Exits
--     STAFF-002 Minimum Staffing
--     DOCS-001  Business License
--     DOCS-002  Municipality Permit
-- ====================================================
