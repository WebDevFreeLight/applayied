using System;
using System.Collections.Generic;
using Infrastructure.Converter;

namespace ZR.Model.Customers.DataMine.Dto
{
    // ==================== QUERY ====================

    public class DmContactQueryDto : PagerInfo
    {
        public string SearchTerm { get; set; }
        public string Industry { get; set; }
        public string City { get; set; }
        public string Region { get; set; }
        public string DataSource { get; set; }
        public bool? IsPulled { get; set; }
        public bool? IsMain { get; set; }
        public bool? ExistsInLeads { get; set; }
        public string Tags { get; set; }
        public string Categories { get; set; }
    }

    // ==================== EXCEL IMPORT ====================

    public class DmContactExcelImportDto
    {
        public string BrandName { get; set; }
        public string BrandNameAr { get; set; }
        public string OwnerName { get; set; }
        public string OwnerPhone { get; set; }
        public string OwnerEmail { get; set; }
        public string CompanyName { get; set; }
        public string Industry { get; set; }
        public string City { get; set; }
        public string Region { get; set; }
        public string CompanySize { get; set; }
        public string Website { get; set; }
        public string SocialMedia { get; set; }
        public string IsMain { get; set; }
        public string Tags { get; set; }
        public string LeadSource { get; set; }
        public string BudgetRange { get; set; }
        public decimal? ExpectedRevenue { get; set; }
        public string Notes { get; set; }
        public string FullAddress { get; set; }
        public string SecondaryPhone { get; set; }

        [MiniExcelLibs.Attributes.ExcelIgnore]
        public string ValidationError { get; set; }

        [MiniExcelLibs.Attributes.ExcelIgnore]
        public bool IsValid => string.IsNullOrEmpty(ValidationError);
    }

    public class DmContactImportPreviewDto
    {
        public int TotalRows { get; set; }
        public int ValidRows { get; set; }
        public int InvalidRows { get; set; }
        public List<DmContactExcelImportDto> PreviewData { get; set; }
        public List<string> ValidationErrors { get; set; }
    }

    public class DmContactImportRequestDto
    {
        public string DataSource { get; set; }
        public bool SkipInvalidRows { get; set; } = true;
        public string Categories { get; set; }
        public string City { get; set; }
    }

    public class DmContactImportResultDto
    {
        public int TotalRows { get; set; }
        public int SuccessCount { get; set; }
        public int FailedCount { get; set; }
        public int SkippedCount { get; set; }
        public List<string> Errors { get; set; }
        public List<long> ImportedContactIds { get; set; }
    }

    // ==================== PULL TO LEADS ====================

    public class PullToLeadsRequestDto
    {
        public long[] ContactIds { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? AssignedToEmployeeId { get; set; }
    }

    public class PullToLeadsResultDto
    {
        public int SuccessCount { get; set; }
        public int FailedCount { get; set; }
        public List<string> Errors { get; set; }
        public List<long> CreatedLeadIds { get; set; }
    }
}
