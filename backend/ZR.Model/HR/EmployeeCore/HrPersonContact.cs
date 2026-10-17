using Infrastructure.Converter;
using Newtonsoft.Json;
using SqlSugar;
using System;
using ZR.Model.System;

namespace ZR.Model.HR.EmployeeCore
{
    /// <summary>
    /// Person contacts (effective-dated) - hr_person_contact
    /// </summary>
    [SugarTable("hr_person_contact")]
    public class HrPersonContact : SysBase
    {
        [SugarColumn(ColumnName = "person_contact_id", IsPrimaryKey = true, IsIdentity = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long PersonContactId { get; set; }

        [SugarColumn(ColumnName = "person_id")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long PersonId { get; set; }

        [SugarColumn(ColumnName = "contact_type_dict_code")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ContactTypeDictCode { get; set; }

        [SugarColumn(ColumnName = "contact_value", Length = 200)]
        public string ContactValue { get; set; }

        [SugarColumn(ColumnName = "effective_from")]
        public DateTime EffectiveFrom { get; set; }

        [SugarColumn(ColumnName = "effective_to", IsNullable = true)]
        public DateTime? EffectiveTo { get; set; }

        [SugarColumn(ColumnName = "is_primary")]
        public bool IsPrimary { get; set; } = false;

        [SugarColumn(ColumnName = "is_verified")]
        public bool IsVerified { get; set; } = false;

        [SugarColumn(ColumnName = "verified_time", IsNullable = true)]
        public DateTime? VerifiedTime { get; set; }
    }
}














