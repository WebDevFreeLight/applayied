using Infrastructure.Converter;
using Newtonsoft.Json;
using SqlSugar;
using System;
using ZR.Model.System;

namespace ZR.Model.HR.EmployeeCore
{
    /// <summary>
    /// Person name (effective-dated) - hr_person_name
    /// </summary>
    [SugarTable("hr_person_name")]
    public class HrPersonName : SysBase
    {
        [SugarColumn(ColumnName = "person_name_id", IsPrimaryKey = true, IsIdentity = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long PersonNameId { get; set; }

        [SugarColumn(ColumnName = "person_id")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long PersonId { get; set; }

        [SugarColumn(ColumnName = "name_type_dict_code", IsNullable = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? NameTypeDictCode { get; set; }

        [SugarColumn(ColumnName = "given_name", Length = 100, IsNullable = true)]
        public string GivenName { get; set; }

        [SugarColumn(ColumnName = "middle_name", Length = 100, IsNullable = true)]
        public string MiddleName { get; set; }

        [SugarColumn(ColumnName = "family_name", Length = 100, IsNullable = true)]
        public string FamilyName { get; set; }

        [SugarColumn(ColumnName = "full_name_ar", Length = 200, IsNullable = true)]
        public string FullNameAr { get; set; }

        [SugarColumn(ColumnName = "effective_from")]
        public DateTime EffectiveFrom { get; set; }

        [SugarColumn(ColumnName = "effective_to", IsNullable = true)]
        public DateTime? EffectiveTo { get; set; }

        [SugarColumn(ColumnName = "is_primary")]
        public bool IsPrimary { get; set; } = false;
    }
}














