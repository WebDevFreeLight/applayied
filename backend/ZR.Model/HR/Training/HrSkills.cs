 
using Infrastructure.Converter;
using SqlSugar;
using System;
using ZR.Model.System;

namespace ZR.Model.HR
{
    /// <summary>
    /// Skills Matrix Table
    /// </summary>
    [SugarTable("hr_skills_matrix")]
    public class HrSkillsMatrix : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long SkillId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }

        [SugarColumn(Length = 100)]
        public string SkillName { get; set; }

        [SugarColumn(Length = 50)]
        public string SkillCategory { get; set; }

        [SugarColumn(Length = 20)]
        public string ProficiencyLevel { get; set; } // beginner, intermediate, advanced, expert

        public int YearsOfExperience { get; set; }

        public DateTime? LastAssessedDate { get; set; }

        public DateTime? CertificationDate { get; set; }

        [SugarColumn(Length = 200)]
        public string CertificationBody { get; set; }

        public DateTime? ExpiryDate { get; set; }

        public bool IsVerified { get; set; }

        [SugarColumn(Length = 50)]
        public string VerifiedBy { get; set; }

        [SugarColumn(Length = 500)]
        public string Notes { get; set; }
 
    }
}

