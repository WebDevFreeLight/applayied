using Infrastructure.Converter;
using Newtonsoft.Json;
using SqlSugar;
using System;
using ZR.Model.System;

namespace ZR.Model.HR.EmployeeCore
{
    /// <summary>
    /// Employee contract (effective-dated) - hr_employee_contract
    /// </summary>
    [SugarTable("hr_employee_contract")]
    public class HrEmployeeContract : SysBase
    {
        [SugarColumn(ColumnName = "employee_contract_id", IsPrimaryKey = true, IsIdentity = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeContractId { get; set; }

        [SugarColumn(ColumnName = "employee_id")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }

        [SugarColumn(ColumnName = "contract_type_dict_code", IsNullable = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? ContractTypeDictCode { get; set; }

        [SugarColumn(ColumnName = "contract_start_date", IsNullable = true)]
        public DateTime? ContractStartDate { get; set; }

        [SugarColumn(ColumnName = "contract_end_date", IsNullable = true)]
        public DateTime? ContractEndDate { get; set; }

        [SugarColumn(ColumnName = "probation_end_date", IsNullable = true)]
        public DateTime? ProbationEndDate { get; set; }

        [SugarColumn(ColumnName = "effective_from")]
        public DateTime EffectiveFrom { get; set; }

        [SugarColumn(ColumnName = "effective_to", IsNullable = true)]
        public DateTime? EffectiveTo { get; set; }

        [SugarColumn(ColumnName = "is_current")]
        public bool IsCurrent { get; set; } = false;
    }
}














