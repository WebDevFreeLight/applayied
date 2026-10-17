using System;

namespace ZR.Model.Operations
{
    /// <summary>
    /// Geographic/Temporal Access Rules - قواعد الوصول الجغرافية/الزمنية
    /// </summary>
    [SugarTable("ops_geo_temporal_rule")]
    public class OpsGeoTemporalRule : SysBase
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(ColumnName = "rule_id", IsPrimaryKey = true, IsIdentity = false)]
        public long RuleId { get; set; }

        /// <summary>
        /// Rule name (EN)
        /// </summary>
        [SugarColumn(ColumnName = "rule_name", Length = 200)]
        public string RuleName { get; set; }

        /// <summary>
        /// Rule name (AR)
        /// </summary>
        [SugarColumn(ColumnName = "rule_name_ar", Length = 200, IsNullable = true)]
        public string RuleNameAr { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        [SugarColumn(ColumnName = "description", Length = 500, IsNullable = true)]
        public string Description { get; set; }

        /// <summary>
        /// FK to blv_boulevard_seasons
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(ColumnName = "season_id", IsNullable = true)]
        public long? SeasonId { get; set; }

        /// <summary>
        /// FK to blv_boulevard_season_zones
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(ColumnName = "zone_id", IsNullable = true)]
        public long? ZoneId { get; set; }

        /// <summary>
        /// Rule type: zone, building, floor, parking, entrance
        /// </summary>
        [SugarColumn(ColumnName = "rule_type", Length = 30)]
        public string RuleType { get; set; } = "zone";

        /// <summary>
        /// GPS latitude
        /// </summary>
        [SugarColumn(ColumnName = "latitude", IsNullable = true, DecimalDigits = 8)]
        public decimal? Latitude { get; set; }

        /// <summary>
        /// GPS longitude
        /// </summary>
        [SugarColumn(ColumnName = "longitude", IsNullable = true, DecimalDigits = 8)]
        public decimal? Longitude { get; set; }

        /// <summary>
        /// Geo-fence radius in meters
        /// </summary>
        [SugarColumn(ColumnName = "radius_meters", IsNullable = true)]
        public int? RadiusMeters { get; set; }

        /// <summary>
        /// Rule start
        /// </summary>
        [SugarColumn(ColumnName = "valid_from", IsNullable = true)]
        public DateTime? ValidFrom { get; set; }

        /// <summary>
        /// Rule end
        /// </summary>
        [SugarColumn(ColumnName = "valid_until", IsNullable = true)]
        public DateTime? ValidUntil { get; set; }

        /// <summary>
        /// JSON: [{"day":"Sunday","startTime":"08:00","endTime":"18:00"}]
        /// </summary>
        [SugarColumn(ColumnName = "time_slots", ColumnDataType = "TEXT", IsNullable = true)]
        public string TimeSlots { get; set; }

        /// <summary>
        /// JSON array of OpsPermitType IDs this rule applies to
        /// </summary>
        [SugarColumn(ColumnName = "permit_type_ids", Length = 500, IsNullable = true)]
        public string PermitTypeIds { get; set; }

        /// <summary>
        /// Status: active, scheduled, expired
        /// </summary>
        [SugarColumn(ColumnName = "status", Length = 20)]
        public string Status { get; set; } = "active";

        /// <summary>
        /// Display color for map
        /// </summary>
        [SugarColumn(ColumnName = "color", Length = 20, IsNullable = true)]
        public string Color { get; set; }

        // Navigation
        [SugarColumn(IsIgnore = true)]
        public string SeasonName { get; set; }
        [SugarColumn(IsIgnore = true)]
        public string ZoneName { get; set; }
    }
}
