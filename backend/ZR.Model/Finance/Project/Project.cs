using System;
using SqlSugar;
using ZR.Model.System;

namespace ZR.Model.Finance.Project
{
    [SugarTable("finance_projects", "Project master data")]
    public class Project : SysBase
    {
        [SugarColumn(IsPrimaryKey = true)]
        public Guid Id { get; set; }

        public string ProjectCode { get; set; }
        public string NameEn { get; set; }
        public string NameAr { get; set; }
        public string Description { get; set; }

        public Guid? ManagerId { get; set; } // FK to Users
        public long? DepartmentId { get; set; } // FK to sys_dept
        public Guid? CostCenterId { get; set; } // FK to finance_cost_centers

        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status { get; set; } // Active, Completed, Cancelled

        public string Currency { get; set; }
        public bool IsCapitalProject { get; set; }

        public Guid? GLAccountId { get; set; } // Optional control account

        // Fields for integration
        public bool IsActive { get; set; }
    }
}
