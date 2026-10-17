using Infrastructure.Converter;
using Newtonsoft.Json;
using SqlSugar;
using System;
using ZR.Model.System;

namespace ZR.Model.HR.EmployeeCore
{
    /// <summary>
    /// Employee statutory/compliance (effective-dated) - hr_employee_statutory
    /// </summary>
    [SugarTable("hr_employee_statutory")]
    public class HrEmployeeStatutory : SysBase
    {
        [SugarColumn(ColumnName = "employee_statutory_id", IsPrimaryKey = true, IsIdentity = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeStatutoryId { get; set; }

        [SugarColumn(ColumnName = "employee_id")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }

        [SugarColumn(ColumnName = "country_dict_code", IsNullable = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? CountryDictCode { get; set; }

        [SugarColumn(ColumnName = "has_gosi")]
        public bool HasGosi { get; set; } = false;

        [SugarColumn(ColumnName = "gosi_number", Length = 50, IsNullable = true)]
        public string GosiNumber { get; set; }

        [SugarColumn(ColumnName = "is_saudi")]
        public bool IsSaudi { get; set; } = false;

        [SugarColumn(ColumnName = "medical_insurance_number", Length = 50, IsNullable = true)]
        public string MedicalInsuranceNumber { get; set; }

        [SugarColumn(ColumnName = "effective_from")]
        public DateTime EffectiveFrom { get; set; }

        [SugarColumn(ColumnName = "effective_to", IsNullable = true)]
        public DateTime? EffectiveTo { get; set; }
    }
}














