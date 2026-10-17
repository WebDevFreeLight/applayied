
using ZR.Model.System;

namespace ZR.Model.Business
{

    [SugarTable("BP_BusinessPartner")]

    public class BusinessPartner:SysBase
    {

        [SugarColumn(IsPrimaryKey = true )]


        public long PartnerId { get; set; }
        public long  Type { get; set; }
        public long Status { get; set; }
        public long  Kind { get; set; }
        public long? AssignedDepartment { get; set; }
        public long? AssignedTo { get; set; }
        public long? EntityId { get; set; }
        public long? LegacyId { get; set; }
        public string PartnerStage { get; set; } = "lead";
        public string? CommercialName { get; set; }
        public string LegalName { get; set; } = string.Empty;
        public string? VatId { get; set; }
        public string? CrNumber { get; set; }
        public string? Industry { get; set; }
        public string? SubIndustry { get; set; }
        public string? ContactFirstName { get; set; }
        public string? ContactLastName { get; set; }
        public string? BusinessPhone { get; set; }
        public string? ContactPhone { get; set; }
        public string? BusinessEmail { get; set; }
        public string? ContactEmail { get; set; }
        public string? Mobile { get; set; }
        public string? StreetAddress { get; set; }

        public string? BuildingNumber { get; set; }

        public string? UnitNumber { get; set; }

        public string? City { get; set; }
        public string? Province { get; set; }
        public string? PostalCode { get; set; }
        public string Country { get; set; } = "Saudi Arabia";
        [SugarColumn(ColumnName = "extra_attributes", IsJson = true)]
        public string ExtraAttributes { get; set; } = "{}";

        public string? ComplianceStatus { get; set; } = "pending";

        public long? PreferredContactMethod { get; set; }

        [SugarColumn(ColumnName = "budget_range", IsJson = true, IsNullable = true)]
        public string? BudgetRange { get; set; } = "{\"max\": 0, \"min\": 0, \"currency\": \"SAR\"}";





    }







    /// <summary>
    /// ContactMethod
    /// </summary>
    [SugarTable("BP_ContactMethod")]
    public class ContactMethod : SysBase
    {
        /// <summary>
        ///   ContactMethod  ID
        /// </summary>


        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        public long ContactMethodEnumid { get; set; }

        /// <summary>
        ///    Type name
        /// </summary>
        public string Name { get; set; }
    }










    /// <summary>
    /// BusinessPartnerStatus
    /// </summary>
    [SugarTable("BP_BusinessPartnerStatus")]
    public class BusinessPartnerStatus : SysBase
    {
        /// <summary>
        /// Status ID
        /// </summary>


        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        public long  Statusid { get; set; }

        /// <summary>
        /// Status name
        /// </summary>
        public string Name { get; set; }
    }



    /// <summary>
    /// BusinessPartnerkind
    /// </summary>
    [SugarTable("BP_BusinessPartnerkind")]
    public class BusinessPartnerkind : SysBase
    {
        /// <summary>
        /// kind ID
        /// </summary>


        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        public long kindid { get; set; }

        /// <summary>
        /// kind name
        /// </summary>
        public string Name { get; set; }
    }


}
