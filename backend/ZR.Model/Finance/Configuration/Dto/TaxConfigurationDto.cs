using System;

namespace ZR.Model.Finance.Configuration.Dto
{
    /// <summary>
    /// Tax Configuration Query DTO
    /// </summary>
    public class TaxConfigurationQueryDto : PagerInfo
    {
        public long? TenantId { get; set; }
    }

    /// <summary>
    /// Tax Configuration Detail DTO
    /// </summary>
    public class TaxConfigurationDetailDto
    {
        public Guid Id { get; set; }
        public long? CompanyId { get; set; }

        public Guid? VATInputAccountId { get; set; }
        public string VATInputAccountCode { get; set; }
        public string VATInputAccountName { get; set; }

        public Guid? VATOutputAccountId { get; set; }
        public string VATOutputAccountCode { get; set; }
        public string VATOutputAccountName { get; set; }

        public Guid? DefaultARAccountId { get; set; }
        public string DefaultARAccountCode { get; set; }
        public string DefaultARAccountName { get; set; }

        public Guid? DefaultAPAccountId { get; set; }
        public string DefaultAPAccountCode { get; set; }
        public string DefaultAPAccountName { get; set; }

        public bool IsConfigured { get; set; }

        public DateTime? Create_time { get; set; }
        public DateTime? Update_time { get; set; }
    }

    /// <summary>
    /// Save Tax Configuration DTO (Create or Update)
    /// </summary>
    public class SaveTaxConfigurationDto
    {
        // Request contract: tenantId only
        public long? TenantId { get; set; }
        public Guid? VATInputAccountId { get; set; }
        public Guid? VATOutputAccountId { get; set; }
        public Guid? DefaultARAccountId { get; set; }
        public Guid? DefaultAPAccountId { get; set; }
    }

    /// <summary>
    /// Tax Configuration Status DTO - For checking if configuration is complete
    /// </summary>
    public class TaxConfigurationStatusDto
    {
        public bool IsConfigured { get; set; }
        public bool HasVATInputAccount { get; set; }
        public bool HasVATOutputAccount { get; set; }
        public bool HasDefaultARAccount { get; set; }
        public bool HasDefaultAPAccount { get; set; }
        public string Message { get; set; }
        public string MessageAr { get; set; }
    }
}
