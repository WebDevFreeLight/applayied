using System;
using Infrastructure.Converter;

namespace ZR.Model.Customers.DataMine
{
    [SugarTable("dm_contact")]
    public class DmContact : SysBase
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(ColumnName = "contact_id", IsPrimaryKey = true, IsIdentity = false)]
        public long ContactId { get; set; }

        [SugarColumn(ColumnName = "brand_name", Length = 200)]
        public string BrandName { get; set; }

        [SugarColumn(ColumnName = "brand_name_ar", Length = 200, IsNullable = true)]
        public string BrandNameAr { get; set; }

        [SugarColumn(ColumnName = "owner_name", Length = 200)]
        public string OwnerName { get; set; }

        [SugarColumn(ColumnName = "owner_phone", Length = 50)]
        public string OwnerPhone { get; set; }

        [SugarColumn(ColumnName = "owner_email", Length = 200, IsNullable = true)]
        public string OwnerEmail { get; set; }

        [SugarColumn(ColumnName = "company_name", Length = 200, IsNullable = true)]
        public string CompanyName { get; set; }

        [SugarColumn(ColumnName = "industry", Length = 100, IsNullable = true)]
        public string Industry { get; set; }

        [SugarColumn(ColumnName = "city", Length = 100, IsNullable = true)]
        public string City { get; set; }

        [SugarColumn(ColumnName = "region", Length = 100, IsNullable = true)]
        public string Region { get; set; }

        [SugarColumn(ColumnName = "company_size", Length = 50, IsNullable = true)]
        public string CompanySize { get; set; }

        [SugarColumn(ColumnName = "website", Length = 500, IsNullable = true)]
        public string Website { get; set; }

        [SugarColumn(ColumnName = "social_media", Length = 500, IsNullable = true)]
        public string SocialMedia { get; set; }

        [SugarColumn(ColumnName = "data_source", Length = 200, IsNullable = true)]
        public string DataSource { get; set; }

        [SugarColumn(ColumnName = "is_main")]
        public bool IsMain { get; set; }

        [SugarColumn(ColumnName = "tags", Length = 500, IsNullable = true)]
        public string Tags { get; set; }

        [SugarColumn(ColumnName = "lead_source", Length = 100, IsNullable = true)]
        public string LeadSource { get; set; }

        [SugarColumn(ColumnName = "budget_range", Length = 50, IsNullable = true)]
        public string BudgetRange { get; set; }

        [SugarColumn(ColumnName = "expected_revenue", DecimalDigits = 2, IsNullable = true)]
        public decimal? ExpectedRevenue { get; set; }

        [SugarColumn(ColumnName = "notes", ColumnDataType = "text", IsNullable = true)]
        public string Notes { get; set; }

        [SugarColumn(ColumnName = "is_pulled")]
        public bool IsPulled { get; set; }

        [SugarColumn(ColumnName = "pulled_at", IsNullable = true)]
        public DateTime? PulledAt { get; set; }

        [SugarColumn(ColumnName = "pulled_by", Length = 100, IsNullable = true)]
        public string PulledBy { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(ColumnName = "pulled_lead_id", IsNullable = true)]
        public long? PulledLeadId { get; set; }

        [SugarColumn(ColumnName = "full_address", Length = 500, IsNullable = true)]
        public string FullAddress { get; set; }

        [SugarColumn(ColumnName = "secondary_phone", Length = 50, IsNullable = true)]
        public string SecondaryPhone { get; set; }

        [SugarColumn(ColumnName = "categories", Length = 500, IsNullable = true)]
        public string Categories { get; set; }

        /// <summary>
        /// True if a lead with the same phone already exists in bp_lead (not mapped to DB)
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public bool ExistsInLeads { get; set; }

        /// <summary>
        /// The lead status if ExistsInLeads is true (not mapped to DB)
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public string ExistingLeadStatus { get; set; }
    }
}
