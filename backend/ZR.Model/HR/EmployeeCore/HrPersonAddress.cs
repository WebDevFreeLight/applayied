using Infrastructure.Converter;
using Newtonsoft.Json;
using SqlSugar;
using System;
using ZR.Model.System;

namespace ZR.Model.HR.EmployeeCore
{
    /// <summary>
    /// Person addresses (effective-dated) - hr_person_address
    /// </summary>
    [SugarTable("hr_person_address")]
    public class HrPersonAddress : SysBase
    {
        [SugarColumn(ColumnName = "person_address_id", IsPrimaryKey = true, IsIdentity = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long PersonAddressId { get; set; }

        [SugarColumn(ColumnName = "person_id")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long PersonId { get; set; }

        [SugarColumn(ColumnName = "address_type_dict_code")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long AddressTypeDictCode { get; set; }

        [SugarColumn(ColumnName = "country_dict_code", IsNullable = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? CountryDictCode { get; set; }

        [SugarColumn(ColumnName = "city", Length = 100, IsNullable = true)]
        public string City { get; set; }

        [SugarColumn(ColumnName = "postal_code", Length = 20, IsNullable = true)]
        public string PostalCode { get; set; }

        [SugarColumn(ColumnName = "address_line_1", Length = 250, IsNullable = true)]
        public string AddressLine1 { get; set; }

        [SugarColumn(ColumnName = "address_line_2", Length = 250, IsNullable = true)]
        public string AddressLine2 { get; set; }

        [SugarColumn(ColumnName = "effective_from")]
        public DateTime EffectiveFrom { get; set; }

        [SugarColumn(ColumnName = "effective_to", IsNullable = true)]
        public DateTime? EffectiveTo { get; set; }

        [SugarColumn(ColumnName = "is_primary")]
        public bool IsPrimary { get; set; } = false;
    }
}














