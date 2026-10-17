-- ============================================================
-- Boulevard Schema Updates - Event Management System (PostgreSQL)
-- Add unit_type, rental_type, revenue_percentage to shops
-- Add season_id to zones
-- ============================================================

-- 1. Add new columns to blv_boulevard_shops
ALTER TABLE blv_boulevard_shops ADD COLUMN unit_type INT NOT NULL DEFAULT 2;
ALTER TABLE blv_boulevard_shops ADD COLUMN rental_type INT NOT NULL DEFAULT 1;
ALTER TABLE blv_boulevard_shops ADD COLUMN revenue_percentage FLOAT NULL;

COMMENT ON COLUMN blv_boulevard_shops.unit_type IS 'Unit Type: 1=Kiosk, 2=Shop';
COMMENT ON COLUMN blv_boulevard_shops.rental_type IS 'Rental Type: 1=FullRent, 2=RevenueShare';
COMMENT ON COLUMN blv_boulevard_shops.revenue_percentage IS 'Revenue Percentage (only for RevenueShare rental type)';

-- 2. Add season_id to blv_boulevard_season_zones
ALTER TABLE blv_boulevard_season_zones ADD COLUMN season_id BIGINT NOT NULL DEFAULT 0;
COMMENT ON COLUMN blv_boulevard_season_zones.season_id IS 'FK to blv_boulevard_seasons.id (Event ID)';

-- 3. Add index for season_id lookup
CREATE INDEX idx_season_zones_season_id ON blv_boulevard_season_zones(season_id);

-- 4. Add indexes for new shop columns (for filtering)
CREATE INDEX idx_shops_unit_type ON blv_boulevard_shops(unit_type);
CREATE INDEX idx_shops_rental_type ON blv_boulevard_shops(rental_type);

-- 5. Set all existing units defaults:
--    unit_type    = 2 (Shop)       - all existing are shops
--    rental_type  = 1 (FullRent)   - all existing are full rent
--    revenue_percentage = NULL     - not applicable for FullRent
UPDATE blv_boulevard_shops SET unit_type = 2;
UPDATE blv_boulevard_shops SET rental_type = 1;
UPDATE blv_boulevard_shops SET revenue_percentage = NULL;
