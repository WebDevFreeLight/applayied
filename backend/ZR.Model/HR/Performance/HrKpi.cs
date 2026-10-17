
using Infrastructure.Converter;
using SqlSugar;
using System;
using ZR.Model.System;

namespace ZR.Model.HR.Performance
{
    /// <summary>
    /// Key Performance Indicator (KPI) Table
    /// </summary>
    [SugarTable("hr_kpi")]
    
    [SqlDict(
        dictType: "kpi",
        dictName: "Hr Kpi - أنواع",
        valueColumn: "kpicode",
        labelColumns: "kpicode, ' - ', kpiname, ' - ', kpinamear",
        RemarkColumn = "description",
        OrderBy = "positionid ASC",
        Description = "Auto-generated from HrKpi table"
    )]
    public class HrKpi : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long KpiId { get; set; }

        [SugarColumn(Length = 100)]
        public string KpiCode { get; set; }

        [SugarColumn(Length = 200)]
        public string KpiName { get; set; }

        [SugarColumn(Length = 200)]
        public string KpiNameAr { get; set; }

        [SugarColumn(Length = 1000)]
        public string Description { get; set; }

        [SugarColumn(Length = 50)]
        public string Category { get; set; } // financial, customer, process, learning

        [SugarColumn(Length = 50)]
        public string MeasurementType { get; set; } // percentage, number, currency, ratio

        [SugarColumn(Length = 50)]
        public string Unit { get; set; }

        public decimal TargetValue { get; set; }

        public decimal MinValue { get; set; }

        public decimal MaxValue { get; set; }

        [SugarColumn(Length = 20)]
        public string Frequency { get; set; } // daily, weekly, monthly, quarterly, yearly

        public decimal Weight { get; set; } = 1.0M;

        [SugarColumn(Length = 500)]
        public string Formula { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? DepartmentId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? PositionId { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsCompanyWide { get; set; } = false;

        [SugarColumn(Length = 20)]
        public string Trend { get; set; } // higher_better, lower_better, range

        [SugarColumn(Length = 500)]
        public string DataSource { get; set; }
 
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public decimal ActualValue { get; set; }
    }

    /// <summary>
    /// Employee KPI Assignment Table
    /// </summary>
    [SugarTable("hr_employee_kpi")]
    public class HrEmployeeKpi : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long AssignmentId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long KpiId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long departmentid { get; set; }

        public int Year { get; set; }

        public int? Quarter { get; set; }

        public decimal TargetValue { get; set; }

        public decimal Weight { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime AssignmentDate { get; set; }

        public DateTime? EndDate { get; set; }

        [SugarColumn(Length = 20)]
        public string Status { get; set; } // active, completed, cancelled

        [SugarColumn(Length = 500)]
        public string Notes { get; set; }

      
    }

    /// <summary>
    /// KPI Achievement Table
    /// </summary>
    [SugarTable("hr_kpi_achievement")]
    public class HrKpiAchievement : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long AchievementId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long AssignmentId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long KpiId { get; set; }

        public DateTime PeriodDate { get; set; }

        public DateTime AchievementDate { get; set; }

        public decimal ActualValue { get; set; }

        public decimal TargetValue { get; set; }

        public decimal AchievementPercentage { get; set; }

        public decimal Score { get; set; }

        [SugarColumn(Length = 500)]
        public string Comments { get; set; }

        [SugarColumn(Length = 500)]
        public string EvidenceUrl { get; set; }

        public bool IsVerified { get; set; } = false;

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? VerifiedBy { get; set; }

        public DateTime? VerificationDate { get; set; }

       
    }
}

