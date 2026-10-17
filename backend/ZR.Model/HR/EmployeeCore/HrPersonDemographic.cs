using Infrastructure.Converter;
using Newtonsoft.Json;
using SqlSugar;
using System;
using ZR.Model.System;

namespace ZR.Model.HR.EmployeeCore
{
    /// <summary>
    /// Person demographics - hr_person_demographic
    /// </summary>
    [SugarTable("hr_person_demographic")]
    public class HrPersonDemographic : SysBase
    {
        [SugarColumn(ColumnName = "person_id", IsPrimaryKey = true, IsIdentity = false)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long PersonId { get; set; }

        [SugarColumn(ColumnName = "date_of_birth", IsNullable = true)]
        public DateTime? DateOfBirth { get; set; }

        [SugarColumn(ColumnName = "gender_dict_code", IsNullable = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? GenderDictCode { get; set; }

        [SugarColumn(ColumnName = "marital_status_dict_code", IsNullable = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? MaritalStatusDictCode { get; set; }

        [SugarColumn(ColumnName = "nationality_dict_code", IsNullable = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? NationalityDictCode { get; set; }
    }
}














