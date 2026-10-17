-- Seed dm_category with clear Saudi market categories
-- Run this once after the table is created (forceRecreate: false ensures safe init)

INSERT INTO dm_category (category_id, name, name_ar, is_active, sort_order, create_by, create_time, update_by, update_time, company_id, branch_id)
VALUES
-- Food & Drink
(1, 'Restaurants', 'مطاعم', true, 1, 'admin', NOW(), 'admin', NOW(), 0, 0),
(2, 'Cafés & Coffee Shops', 'مقاهي وكوفي شوب', true, 2, 'admin', NOW(), 'admin', NOW(), 0, 0),
(3, 'Bakeries & Pastries', 'مخابز وحلويات', true, 3, 'admin', NOW(), 'admin', NOW(), 0, 0),
(4, 'Fast Food & Street Food', 'وجبات سريعة', true, 4, 'admin', NOW(), 'admin', NOW(), 0, 0),
(5, 'Ice Cream & Desserts', 'آيس كريم وحلويات', true, 5, 'admin', NOW(), 'admin', NOW(), 0, 0),
(6, 'Juice & Smoothie Bars', 'عصائر وسموذي', true, 6, 'admin', NOW(), 'admin', NOW(), 0, 0),

-- Fashion & Retail
(7, 'Fashion & Clothing', 'أزياء وملابس', true, 7, 'admin', NOW(), 'admin', NOW(), 0, 0),
(8, 'Shoes & Bags', 'أحذية وحقائب', true, 8, 'admin', NOW(), 'admin', NOW(), 0, 0),
(9, 'Jewelry & Watches', 'مجوهرات وساعات', true, 9, 'admin', NOW(), 'admin', NOW(), 0, 0),
(10, 'Perfumes & Fragrances', 'عطور', true, 10, 'admin', NOW(), 'admin', NOW(), 0, 0),
(11, 'Eyewear & Optics', 'نظارات وبصريات', true, 11, 'admin', NOW(), 'admin', NOW(), 0, 0),
(12, 'Kids & Baby Stores', 'متاجر أطفال ورضع', true, 12, 'admin', NOW(), 'admin', NOW(), 0, 0),

-- Health & Beauty
(13, 'Pharmacies', 'صيدليات', true, 13, 'admin', NOW(), 'admin', NOW(), 0, 0),
(14, 'Beauty Salons & Spas', 'صالونات تجميل وسبا', true, 14, 'admin', NOW(), 'admin', NOW(), 0, 0),
(15, 'Gyms & Fitness Centers', 'أندية رياضية ولياقة', true, 15, 'admin', NOW(), 'admin', NOW(), 0, 0),
(16, 'Health & Wellness', 'صحة وعافية', true, 16, 'admin', NOW(), 'admin', NOW(), 0, 0),

-- Electronics & Tech
(17, 'Electronics & Mobile', 'إلكترونيات وجوالات', true, 17, 'admin', NOW(), 'admin', NOW(), 0, 0),
(18, 'Gaming & Entertainment', 'ألعاب وترفيه', true, 18, 'admin', NOW(), 'admin', NOW(), 0, 0),

-- Home & Living
(19, 'Furniture & Home Décor', 'أثاث وديكور', true, 19, 'admin', NOW(), 'admin', NOW(), 0, 0),
(20, 'Supermarkets & Grocery', 'سوبرماركت وبقالة', true, 20, 'admin', NOW(), 'admin', NOW(), 0, 0),
(21, 'Flowers & Gifts', 'زهور وهدايا', true, 21, 'admin', NOW(), 'admin', NOW(), 0, 0),

-- Services
(22, 'Education & Training', 'تعليم وتدريب', true, 22, 'admin', NOW(), 'admin', NOW(), 0, 0),
(23, 'Medical Clinics', 'عيادات طبية', true, 23, 'admin', NOW(), 'admin', NOW(), 0, 0),
(24, 'Dental Clinics', 'عيادات أسنان', true, 24, 'admin', NOW(), 'admin', NOW(), 0, 0),
(25, 'Auto Services & Car Wash', 'خدمات سيارات وغسيل', true, 25, 'admin', NOW(), 'admin', NOW(), 0, 0),
(26, 'Real Estate', 'عقارات', true, 26, 'admin', NOW(), 'admin', NOW(), 0, 0),
(27, 'Travel & Tourism', 'سياحة وسفر', true, 27, 'admin', NOW(), 'admin', NOW(), 0, 0),
(28, 'Banks & Financial Services', 'بنوك وخدمات مالية', true, 28, 'admin', NOW(), 'admin', NOW(), 0, 0),
(29, 'Laundry & Dry Cleaning', 'مغاسل وتنظيف', true, 29, 'admin', NOW(), 'admin', NOW(), 0, 0),
(30, 'Pet Shops & Veterinary', 'محلات حيوانات وبيطرة', true, 30, 'admin', NOW(), 'admin', NOW(), 0, 0),

-- Other
(31, 'Events & Wedding Planning', 'تنظيم فعاليات وأعراس', true, 31, 'admin', NOW(), 'admin', NOW(), 0, 0),
(32, 'Photography & Media', 'تصوير وإعلام', true, 32, 'admin', NOW(), 'admin', NOW(), 0, 0),
(33, 'Printing & Stationery', 'طباعة وقرطاسية', true, 33, 'admin', NOW(), 'admin', NOW(), 0, 0),
(34, 'Other', 'أخرى', true, 34, 'admin', NOW(), 'admin', NOW(), 0, 0)

ON CONFLICT (category_id) DO NOTHING;
