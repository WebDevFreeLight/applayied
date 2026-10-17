-- ============================================================
-- Seed Data: على خطاه (In His Footsteps - Hijrah Route)
-- Event with 8 Zones, 2 Categories (F&B, Retail), and 34 Units
-- PostgreSQL
-- ============================================================
-- Run this AFTER boulevard_schema_updates.sql
-- ============================================================

DO $$
DECLARE
    -- === ID Variables ===
    -- Event (Season)
    v_event_id      BIGINT;

    -- Zones (8)
    v_z_asfal       BIGINT;  -- أسفل عسفان (Asfal Usfan)
    v_z_ummaabad    BIGINT;  -- أم معبد (Um Maabad)
    v_z_juhfa       BIGINT;  -- الجحفة (Juhfa)
    v_z_qaha        BIGINT;  -- القاحة (Al Qaha)
    v_z_araj        BIGINT;  -- الأعراج (Al-Araj)
    v_z_reem        BIGINT;  -- الريم (Al-Reem)
    v_z_jathjatha   BIGINT;  -- الجثجاثة (Al-Jathjatha)
    v_z_gharthoor   BIGINT;  -- غار ثور (Ghar Thoor)

    -- Existing Categories (from blv_boulevard_shop_category table)
    v_cat_fb        BIGINT := 4;   -- المطاعم والكافيهات (F&B)
    v_cat_retail    BIGINT := 1;   -- ملابس وأزياء (Retail/Fashion - closest match)

    -- Helper
    v_base          BIGINT;
    v_now           TIMESTAMP := NOW();

BEGIN
    -- ──────────────────────────────────────────────
    -- Generate unique IDs from epoch (non-conflicting with SnowFlake)
    -- ──────────────────────────────────────────────
    v_base := (EXTRACT(EPOCH FROM v_now) * 1000)::BIGINT;

    v_event_id      := v_base * 10 + 1;

    v_z_asfal       := v_base * 10 + 101;
    v_z_ummaabad    := v_base * 10 + 102;
    v_z_juhfa       := v_base * 10 + 103;
    v_z_qaha        := v_base * 10 + 104;
    v_z_araj        := v_base * 10 + 105;
    v_z_reem        := v_base * 10 + 106;
    v_z_jathjatha   := v_base * 10 + 107;
    v_z_gharthoor   := v_base * 10 + 108;

    -- ══════════════════════════════════════════════
    -- 1. EVENT (Season)
    -- ══════════════════════════════════════════════
    INSERT INTO blv_boulevard_seasons (id, name, start_date, end_date, create_by, create_time, remark)
    VALUES (
        v_event_id,
        'على خطاه - In His Footsteps',
        '2026-03-01'::TIMESTAMP,   -- ← Update with actual start date
        '2026-05-31'::TIMESTAMP,   -- ← Update with actual end date
        'system',
        v_now,
        'طريق الهجرة - Hijrah Route Event'
    );

    RAISE NOTICE 'Event inserted: ID = %', v_event_id;

    -- ══════════════════════════════════════════════
    -- 2. ZONES (8 zones linked to event)
    -- ══════════════════════════════════════════════
    INSERT INTO blv_boulevard_season_zones (id, name, season_id, create_by, create_time, remark) VALUES
        (v_z_asfal,     'أسفل عسفان - Asfal Usfan',       v_event_id, 'system', v_now, 'Green Hub Zone'),
        (v_z_ummaabad,  'أم معبد - Um Maabad',             v_event_id, 'system', v_now, 'Landmark Zone'),
        (v_z_juhfa,     'الجحفة - Juhfa',                  v_event_id, 'system', v_now, 'Green Hub Zone'),
        (v_z_qaha,      'القاحة - Al Qaha',                v_event_id, 'system', v_now, 'Green Hub Zone'),
        (v_z_araj,      'الأعراج - Al-Araj',               v_event_id, 'system', v_now, 'Orange Hub Zone'),
        (v_z_reem,      'الريم - Al-Reem',                 v_event_id, 'system', v_now, 'Blue Hub Zone'),
        (v_z_jathjatha, 'الجثجاثة - Al-Jathjatha',         v_event_id, 'system', v_now, 'Orange Hub Zone'),
        (v_z_gharthoor, 'غار ثور - Ghar Thoor',            v_event_id, 'system', v_now, 'Ghar Thoor Zone');

    RAISE NOTICE '8 zones inserted';

    -- ══════════════════════════════════════════════
    -- 3. CATEGORIES — Using existing records:
    --    F&B    = 1975173039435943936 (طعام ومشروبات)
    --    Retail = 1975172998679891968 (بيع بالتجزئة)
    -- ══════════════════════════════════════════════

    -- ══════════════════════════════════════════════
    -- 4. UNITS (34 units)
    -- ══════════════════════════════════════════════
    -- Column reference:
    --   shop_status:  1=Available
    --   shop_category: v_cat_fb (F&B) or v_cat_retail (Retail)
    --   unit_type:    1=Kiosk (K in code), 2=Shop/Container (C in code)
    --   rental_type:  1=FullRent (ايجار), 2=RevenueShare (مشاركة 30%-70%)
    --
    -- Revenue Share pricing:
    --   meter_price = 370 SAR/sqm (participation advance)
    --   total_price = area × 370
    --   revenue_percentage = 30
    --
    -- Full Rent pricing:
    --   total_price = contract value (from spreadsheet)
    --   meter_price = total_price / area
    --   revenue_percentage = NULL

    INSERT INTO blv_boulevard_shops (
        id, name, zone_id, shop_status, shop_category, shop_area,
        meter_price, total_price, unit_type, rental_type,
        revenue_percentage, create_by, create_time
    ) VALUES

    -- ─────────────────────────────────────────────
    -- Zone 1: أسفل عسفان (Asfal Usfan) — Green Hub
    -- ─────────────────────────────────────────────
    -- U-K03  | Kiosk | F&B    | 9 sqm    | RevShare 30% | 370/sqm → 3,330
    (v_base*10+1001, 'U-K03',  v_z_asfal, 1, v_cat_fb, 9,    370, 3330,    1, 2, 30, 'system', v_now),
    -- U-K18  | Kiosk | Retail | 16 sqm   | RevShare 30% | 370/sqm → 5,920
    (v_base*10+1002, 'U-K18',  v_z_asfal, 1, v_cat_retail, 16,   370, 5920,    1, 2, 30, 'system', v_now),
    -- U-K23  | Kiosk | F&B    | 9 sqm    | RevShare 30% | 370/sqm → 3,330
    (v_base*10+1003, 'U-K23',  v_z_asfal, 1, v_cat_fb, 9,    370, 3330,    1, 2, 30, 'system', v_now),

    -- ─────────────────────────────────────────────
    -- Zone 2: أم معبد (Um Maabad) — Landmark
    -- ─────────────────────────────────────────────
    -- M-K02  | Kiosk | Retail | 26.6 sqm | RevShare 30% | 370/sqm → 9,842
    (v_base*10+1004, 'M-K02',  v_z_ummaabad, 1, v_cat_retail, 26.6,  370, 9842,    1, 2, 30, 'system', v_now),
    -- M-K04  | Kiosk | Retail | 16.1 sqm | RevShare 30% | 370/sqm → 5,957
    (v_base*10+1005, 'M-K04',  v_z_ummaabad, 1, v_cat_retail, 16.1,  370, 5957,    1, 2, 30, 'system', v_now),

    -- ─────────────────────────────────────────────
    -- Zone 3: الجحفة (Juhfa) — Green Hub
    -- ─────────────────────────────────────────────
    -- J-C03-03 | Shop | Retail | 35.69 sqm | FullRent | 4,000/sqm → 142,760
    (v_base*10+1006, 'J-C03-03', v_z_juhfa, 1, v_cat_retail, 35.69, 4000, 142760,    2, 1, NULL, 'system', v_now),
    -- J-C04-03 | Shop | Retail | 35.69 sqm | FullRent | 4,000/sqm → 142,760
    (v_base*10+1007, 'J-C04-03', v_z_juhfa, 1, v_cat_retail, 35.69, 4000, 142760,    2, 1, NULL, 'system', v_now),
    -- J-K06    | Kiosk | Retail | 16 sqm   | RevShare 30% | 370/sqm → 5,920
    (v_base*10+1008, 'J-K06',   v_z_juhfa, 1, v_cat_retail, 16,    370, 5920,        1, 2, 30, 'system', v_now),
    -- J-K10    | Kiosk | F&B    | 9 sqm    | RevShare 30% | 370/sqm → 3,330
    (v_base*10+1009, 'J-K10',   v_z_juhfa, 1, v_cat_fb, 9,     370, 3330,        1, 2, 30, 'system', v_now),
    -- J-K12    | Kiosk | F&B    | 9 sqm    | RevShare 30% | 370/sqm → 3,330
    (v_base*10+1010, 'J-K12',   v_z_juhfa, 1, v_cat_fb, 9,     370, 3330,        1, 2, 30, 'system', v_now),
    -- J-K19    | Kiosk | Retail | 16 sqm   | RevShare 30% | 370/sqm → 5,920
    (v_base*10+1011, 'J-K19',   v_z_juhfa, 1, v_cat_retail, 16,    370, 5920,        1, 2, 30, 'system', v_now),
    -- J-K30    | Kiosk | F&B    | 9 sqm    | RevShare 30% | 370/sqm → 3,330
    (v_base*10+1012, 'J-K30',   v_z_juhfa, 1, v_cat_fb, 9,     370, 3330,        1, 2, 30, 'system', v_now),

    -- ─────────────────────────────────────────────
    -- Zone 4: القاحة (Al Qaha) — Green Hub
    -- ─────────────────────────────────────────────
    -- Q-C03-02 | Shop  | F&B    | 134.8 sqm  | FullRent | 2,225.52/sqm → 300,000
    (v_base*10+1013, 'Q-C03-02', v_z_qaha, 1, v_cat_fb, 134.8,  2225.52, 300000,   2, 1, NULL, 'system', v_now),
    -- Q-C03-03 | Shop  | Retail | 35.69 sqm  | FullRent | 4,000/sqm → 142,760
    (v_base*10+1014, 'Q-C03-03', v_z_qaha, 1, v_cat_retail, 35.69,  4000, 142760,      2, 1, NULL, 'system', v_now),
    -- Q-C03-04 | Shop  | Retail | 36.57 sqm  | FullRent | 4,000/sqm → 146,280
    (v_base*10+1015, 'Q-C03-04', v_z_qaha, 1, v_cat_retail, 36.57,  4000, 146280,      2, 1, NULL, 'system', v_now),
    -- Q-C04-03 | Shop  | Retail | 35.69 sqm  | FullRent | 4,000/sqm → 142,760
    (v_base*10+1016, 'Q-C04-03', v_z_qaha, 1, v_cat_retail, 35.69,  4000, 142760,      2, 1, NULL, 'system', v_now),
    -- Q-C04-04 | Shop  | Retail | 36.57 sqm  | FullRent | 4,000/sqm → 146,280
    (v_base*10+1017, 'Q-C04-04', v_z_qaha, 1, v_cat_retail, 36.57,  4000, 146280,      2, 1, NULL, 'system', v_now),
    -- Q-C13-01 | Shop  | F&B    | 64.5 sqm   | FullRent | 3,100.78/sqm → 200,000
    (v_base*10+1018, 'Q-C13-01', v_z_qaha, 1, v_cat_fb, 64.5,   3100.78, 200000,   2, 1, NULL, 'system', v_now),
    -- Q-K08    | Kiosk | F&B    | 9 sqm      | RevShare 30% | 370/sqm → 3,330
    (v_base*10+1019, 'Q-K08',   v_z_qaha, 1, v_cat_fb, 9,       370, 3330,         1, 2, 30, 'system', v_now),
    -- Q-K12    | Kiosk | F&B    | 9 sqm      | RevShare 30% | 370/sqm → 3,330
    (v_base*10+1020, 'Q-K12',   v_z_qaha, 1, v_cat_fb, 9,       370, 3330,         1, 2, 30, 'system', v_now),
    -- Q-K22    | Kiosk | F&B    | 9 sqm      | RevShare 30% | 370/sqm → 3,330
    (v_base*10+1021, 'Q-K22',   v_z_qaha, 1, v_cat_fb, 9,       370, 3330,         1, 2, 30, 'system', v_now),
    -- Q-K24    | Kiosk | F&B    | 40 sqm     | FullRent | 4,000/sqm → 160,000
    (v_base*10+1022, 'Q-K24',   v_z_qaha, 1, v_cat_fb, 40,      4000, 160000,      1, 1, NULL, 'system', v_now),
    -- Q-K25    | Kiosk | F&B    | 9 sqm      | RevShare 30% | 370/sqm → 3,330
    (v_base*10+1023, 'Q-K25',   v_z_qaha, 1, v_cat_fb, 9,       370, 3330,         1, 2, 30, 'system', v_now),

    -- ─────────────────────────────────────────────
    -- Zone 5: الأعراج (Al-Araj) — Orange Hub
    -- ─────────────────────────────────────────────
    -- A-C02-02 | Shop  | Retail | 97.1 sqm   | RevShare 30% | 370/sqm → 35,927
    (v_base*10+1024, 'A-C02-02', v_z_araj, 1, v_cat_retail, 97.1,   370, 35927,       2, 2, 30, 'system', v_now),
    -- A-C05    | Shop  | F&B    | 174.35 sqm | RevShare 30% | 370/sqm → 64,509.50
    (v_base*10+1025, 'A-C05',   v_z_araj, 1, v_cat_fb, 174.35,  370, 64509.50,    2, 2, 30, 'system', v_now),
    -- A-K04    | Kiosk | F&B    | 25 sqm     | RevShare 30% | 370/sqm → 9,250
    (v_base*10+1026, 'A-K04',   v_z_araj, 1, v_cat_fb, 25,      370, 9250,        1, 2, 30, 'system', v_now),
    -- A-K07    | Kiosk | F&B    | 25 sqm     | RevShare 30% | 370/sqm → 9,250
    (v_base*10+1027, 'A-K07',   v_z_araj, 1, v_cat_fb, 25,      370, 9250,        1, 2, 30, 'system', v_now),

    -- ─────────────────────────────────────────────
    -- Zone 6: الريم (Al-Reem) — Blue Hub
    -- ─────────────────────────────────────────────
    -- R-C08    | Shop  | Retail | 52.36 sqm  | RevShare 30% | 370/sqm → 19,373.20
    (v_base*10+1028, 'R-C08',   v_z_reem, 1, v_cat_retail, 52.36,    370, 19373.20,    2, 2, 30, 'system', v_now),
    -- R-K02    | Kiosk | F&B    | 22.5 sqm   | RevShare 30% | 370/sqm → 8,325
    (v_base*10+1029, 'R-K02',   v_z_reem, 1, v_cat_fb, 22.5,     370, 8325,        1, 2, 30, 'system', v_now),
    -- R-K05    | Kiosk | F&B    | 22.5 sqm   | RevShare 30% | 370/sqm → 8,325
    (v_base*10+1030, 'R-K05',   v_z_reem, 1, v_cat_fb, 22.5,     370, 8325,        1, 2, 30, 'system', v_now),
    -- R-K11    | Kiosk | F&B    | 16 sqm     | RevShare 30% | 370/sqm → 5,920
    (v_base*10+1031, 'R-K11',   v_z_reem, 1, v_cat_fb, 16,       370, 5920,        1, 2, 30, 'system', v_now),

    -- ─────────────────────────────────────────────
    -- Zone 7: الجثجاثة (Al-Jathjatha) — Orange Hub
    -- ─────────────────────────────────────────────
    -- T-C07-01 | Shop  | F&B    | 75.56 sqm  | RevShare 30% | 370/sqm → 27,957.20
    (v_base*10+1032, 'T-C07-01', v_z_jathjatha, 1, v_cat_fb, 75.56, 370, 27957.20, 2, 2, 30, 'system', v_now),

    -- ─────────────────────────────────────────────
    -- Zone 8: غار ثور (Ghar Thoor) — Ghar Thoor
    -- ─────────────────────────────────────────────
    -- G-K13    | Kiosk | F&B    | 27.45 sqm  | FullRent | 9,000/sqm → 247,050
    (v_base*10+1033, 'G-K13',   v_z_gharthoor, 1, v_cat_fb, 27.45, 9000, 247050, 1, 1, NULL, 'system', v_now);

    -- ══════════════════════════════════════════════
    -- 5. LINK SHOPS TO EVENT (seasons_shops junction)
    -- Without this, the "Available Units" API returns empty!
    -- ══════════════════════════════════════════════
    INSERT INTO blv_boulevard_seasons_shops (id, season_id, shop_id, create_by, create_time) VALUES
        -- Zone 1: Asfal Usfan (3 units)
        (v_base*10+2001, v_event_id, v_base*10+1001, 'system', v_now),
        (v_base*10+2002, v_event_id, v_base*10+1002, 'system', v_now),
        (v_base*10+2003, v_event_id, v_base*10+1003, 'system', v_now),
        -- Zone 2: Um Maabad (2 units)
        (v_base*10+2004, v_event_id, v_base*10+1004, 'system', v_now),
        (v_base*10+2005, v_event_id, v_base*10+1005, 'system', v_now),
        -- Zone 3: Juhfa (7 units)
        (v_base*10+2006, v_event_id, v_base*10+1006, 'system', v_now),
        (v_base*10+2007, v_event_id, v_base*10+1007, 'system', v_now),
        (v_base*10+2008, v_event_id, v_base*10+1008, 'system', v_now),
        (v_base*10+2009, v_event_id, v_base*10+1009, 'system', v_now),
        (v_base*10+2010, v_event_id, v_base*10+1010, 'system', v_now),
        (v_base*10+2011, v_event_id, v_base*10+1011, 'system', v_now),
        (v_base*10+2012, v_event_id, v_base*10+1012, 'system', v_now),
        -- Zone 4: Al Qaha (11 units)
        (v_base*10+2013, v_event_id, v_base*10+1013, 'system', v_now),
        (v_base*10+2014, v_event_id, v_base*10+1014, 'system', v_now),
        (v_base*10+2015, v_event_id, v_base*10+1015, 'system', v_now),
        (v_base*10+2016, v_event_id, v_base*10+1016, 'system', v_now),
        (v_base*10+2017, v_event_id, v_base*10+1017, 'system', v_now),
        (v_base*10+2018, v_event_id, v_base*10+1018, 'system', v_now),
        (v_base*10+2019, v_event_id, v_base*10+1019, 'system', v_now),
        (v_base*10+2020, v_event_id, v_base*10+1020, 'system', v_now),
        (v_base*10+2021, v_event_id, v_base*10+1021, 'system', v_now),
        (v_base*10+2022, v_event_id, v_base*10+1022, 'system', v_now),
        (v_base*10+2023, v_event_id, v_base*10+1023, 'system', v_now),
        -- Zone 5: Al-Araj (4 units)
        (v_base*10+2024, v_event_id, v_base*10+1024, 'system', v_now),
        (v_base*10+2025, v_event_id, v_base*10+1025, 'system', v_now),
        (v_base*10+2026, v_event_id, v_base*10+1026, 'system', v_now),
        (v_base*10+2027, v_event_id, v_base*10+1027, 'system', v_now),
        -- Zone 6: Al-Reem (4 units)
        (v_base*10+2028, v_event_id, v_base*10+1028, 'system', v_now),
        (v_base*10+2029, v_event_id, v_base*10+1029, 'system', v_now),
        (v_base*10+2030, v_event_id, v_base*10+1030, 'system', v_now),
        (v_base*10+2031, v_event_id, v_base*10+1031, 'system', v_now),
        -- Zone 7: Al-Jathjatha (1 unit)
        (v_base*10+2032, v_event_id, v_base*10+1032, 'system', v_now),
        -- Zone 8: Ghar Thoor (1 unit)
        (v_base*10+2033, v_event_id, v_base*10+1033, 'system', v_now);

    RAISE NOTICE '34 season-shop links inserted';

    RAISE NOTICE '──────────────────────────────────────';
    RAISE NOTICE 'Seed completed successfully!';
    RAISE NOTICE '──────────────────────────────────────';
    RAISE NOTICE 'Event ID:    %', v_event_id;
    RAISE NOTICE 'Zones:       8';
    RAISE NOTICE 'Categories:  Using existing (F&B=%s, Retail=%s)', v_cat_fb, v_cat_retail;
    RAISE NOTICE 'Units:       34 (24 RevShare + 10 FullRent)';
    RAISE NOTICE 'Season-Shop Links: 34';
    RAISE NOTICE '──────────────────────────────────────';

END $$;

-- ============================================================
-- VERIFICATION QUERIES (run after INSERT to confirm)
-- ============================================================

-- Check event
SELECT id, name, start_date, end_date FROM blv_boulevard_seasons
WHERE name LIKE '%على خطاه%';

-- Check zones
SELECT z.id, z.name, z.season_id, s.name AS event_name
FROM blv_boulevard_season_zones z
JOIN blv_boulevard_seasons s ON z.season_id = s.id
WHERE s.name LIKE '%على خطاه%'
ORDER BY z.id;

-- Check categories
SELECT id, name FROM blv_boulevard_shop_category
ORDER BY id;

-- Check units summary by zone
SELECT z.name AS zone_name,
       COUNT(*) AS total_units,
       SUM(CASE WHEN sh.rental_type = 1 THEN 1 ELSE 0 END) AS full_rent,
       SUM(CASE WHEN sh.rental_type = 2 THEN 1 ELSE 0 END) AS rev_share,
       SUM(CASE WHEN sh.unit_type = 1 THEN 1 ELSE 0 END) AS kiosks,
       SUM(CASE WHEN sh.unit_type = 2 THEN 1 ELSE 0 END) AS shops,
       ROUND(SUM(sh.total_price)::NUMERIC, 2) AS total_value
FROM blv_boulevard_shops sh
JOIN blv_boulevard_season_zones z ON sh.zone_id = z.id
JOIN blv_boulevard_seasons s ON z.season_id = s.id
WHERE s.name LIKE '%على خطاه%'
GROUP BY z.name
ORDER BY z.name;

-- Check full unit listing
SELECT sh.name AS unit_code,
       z.name AS zone,
       CASE sh.unit_type WHEN 1 THEN 'Kiosk' WHEN 2 THEN 'Shop' END AS unit_type,
       cat.name AS category,
       sh.shop_area AS area_sqm,
       sh.meter_price,
       sh.total_price,
       CASE sh.rental_type WHEN 1 THEN 'FullRent' WHEN 2 THEN 'RevShare' END AS rental,
       sh.revenue_percentage
FROM blv_boulevard_shops sh
JOIN blv_boulevard_season_zones z ON sh.zone_id = z.id
JOIN blv_boulevard_seasons s ON z.season_id = s.id
LEFT JOIN blv_boulevard_shop_category cat ON sh.shop_category = cat.id
WHERE s.name LIKE '%على خطاه%'
ORDER BY z.name, sh.name;
