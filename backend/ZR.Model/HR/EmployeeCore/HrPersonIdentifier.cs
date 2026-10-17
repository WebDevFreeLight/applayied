using Infrastructure.Converter;
using Newtonsoft.Json;
using SqlSugar;
using System;
using ZR.Model.System;

namespace ZR.Model.HR.EmployeeCore
{
    /// <summary>
    /// Person identifiers (Iqama/National ID/Passport...) - hr_person_identifier
    /// </summary>
    [SugarTable("hr_person_identifier")]
    public class HrPersonIdentifier : SysBase
    {
        [SugarColumn(ColumnName = "person_identifier_id", IsPrimaryKey = true, IsIdentity = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long PersonIdentifierId { get; set; }

        [SugarColumn(ColumnName = "person_id")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long PersonId { get; set; }

        [SugarColumn(ColumnName = "identifier_type_dict_code")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long IdentifierTypeDictCode { get; set; }

        [SugarColumn(ColumnName = "identifier_number", Length = 100)]
        public string IdentifierNumber { get; set; }

        [SugarColumn(ColumnName = "issuing_country_dict_code", IsNullable = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? IssuingCountryDictCode { get; set; }

        [SugarColumn(ColumnName = "issue_date", IsNullable = true)]
        public DateTime? IssueDate { get; set; }

        [SugarColumn(ColumnName = "expiry_date", IsNullable = true)]
        public DateTime? ExpiryDate { get; set; }

        [SugarColumn(ColumnName = "is_primary")]
        public bool IsPrimary { get; set; } = false;

        [SugarColumn(ColumnName = "is_verified")]
        public bool IsVerified { get; set; } = false;

        [SugarColumn(ColumnName = "verified_time", IsNullable = true)]
        public DateTime? VerifiedTime { get; set; }
    }
}














