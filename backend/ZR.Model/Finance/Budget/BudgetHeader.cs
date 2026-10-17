using System;
using System.Collections.Generic;
using SqlSugar;
using ZR.Model.System;
using ZR.Model.Finance.GL;

namespace ZR.Model.Finance.Budget
{
    /// <summary>
    /// Budget Header - رأس الميزانية
    /// Contains budget planning header information with support for different budget types
    /// </summary>
    [SugarTable("finance_budget_header", "Budget planning header")]
    // Unique constraint: BudgetCode must be unique per company
    [SugarIndex("UX_budget_header_company_code", nameof(CompanyID), OrderByType.Asc, nameof(BudgetCode), OrderByType.Asc, true)]
    // Company isolation index
    [SugarIndex("IX_budget_header_company", nameof(CompanyID), OrderByType.Asc)]
    // Fiscal year queries per company
    [SugarIndex("IX_budget_header_company_fiscal_year", nameof(CompanyID), OrderByType.Asc, nameof(FiscalYear), OrderByType.Asc)]
    // Status filtering per company
    [SugarIndex("IX_budget_header_company_status", nameof(CompanyID), OrderByType.Asc, nameof(Status), OrderByType.Asc)]
    // Cost center and project lookups
    [SugarIndex("IX_budget_header_cost_center", nameof(CostCenterId), OrderByType.Asc)]
    [SugarIndex("IX_budget_header_project", nameof(ProjectId), OrderByType.Asc)]
    [SugarIndex("IX_budget_header_department", nameof(DepartmentId), OrderByType.Asc)]
    // Type filtering per company
    [SugarIndex("IX_budget_header_company_type", nameof(CompanyID), OrderByType.Asc, nameof(Type), OrderByType.Asc)]
    public class BudgetHeader : SysBase
    {
        #region Primary Key

        /// <summary>
        /// Budget ID - معرف الميزانية
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, ColumnName = "id")]
        public Guid Id { get; set; }

        #endregion

        #region Budget Identification

        /// <summary>
        /// Budget Code - كود الميزانية
        /// Unique identifier code for the budget
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = false, ColumnName = "budget_code")]
        public string BudgetCode { get; set; }

        /// <summary>
        /// Budget Name - اسم الميزانية
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = false, ColumnName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Budget Name (Arabic) - اسم الميزانية بالعربي
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = true, ColumnName = "name_ar")]
        public string NameAr { get; set; }

        /// <summary>
        /// Description - الوصف
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnName = "description")]
        public string Description { get; set; }

        #endregion

        #region Period Information

        /// <summary>
        /// Fiscal Year - السنة المالية
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "fiscal_year")]
        public int FiscalYear { get; set; }

        /// <summary>
        /// Currency - العملة
        /// </summary>
        [SugarColumn(Length = 10, IsNullable = false, ColumnName = "currency", DefaultValue = "SAR")]
        public string Currency { get; set; } = "SAR";

        #endregion

        #region Budget Classification

        /// <summary>
        /// Budget Type - نوع الميزانية
        /// Values: Operational, Capital, Departmental, Project
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = false, ColumnName = "type")]
        public string Type { get; set; }

        /// <summary>
        /// Budget Status - حالة الميزانية
        /// Values: Draft, Approved, Closed
        /// </summary>
        [SugarColumn(Length = 20, IsNullable = false, ColumnName = "status", DefaultValue = "Draft")]
        public string Status { get; set; } = "Draft";

        #endregion

        #region Reference Links

        /// <summary>
        /// Project ID - معرف المشروع
        /// FK to finance_projects
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "project_id")]
        public Guid? ProjectId { get; set; }

        /// <summary>
        /// Department ID - معرف القسم
        /// FK to sys_dept
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "department_id")]
        public long? DepartmentId { get; set; }

        /// <summary>
        /// Cost Center ID - معرف مركز التكلفة
        /// FK to finance_cost_centers (CostCenter.CostCenterId is long)
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "cost_center_id")]
        public long? CostCenterId { get; set; }

        #endregion

        #region Audit Information

        /// <summary>
        /// Created At - تاريخ الإنشاء
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Approved By - المعتمد بواسطة
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "approved_by")]
        public Guid? ApprovedBy { get; set; }

        /// <summary>
        /// Approved At - تاريخ الاعتماد
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "approved_at")]
        public DateTime? ApprovedAt { get; set; }

        /// <summary>
        /// Closed By - المغلق بواسطة
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "closed_by")]
        public Guid? ClosedBy { get; set; }

        /// <summary>
        /// Closed At - تاريخ الإغلاق
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "closed_at")]
        public DateTime? ClosedAt { get; set; }

        /// <summary>
        /// Is Active - نشط
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "is_active", DefaultValue = "1")]
        public bool IsActive { get; set; } = true;

        #endregion

        #region Navigation Properties (Ignored in DB)

        /// <summary>
        /// Budget Lines - بنود الميزانية
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public List<BudgetLine> Lines { get; set; }

        /// <summary>
        /// Variance Summaries - ملخصات الانحراف
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public List<BudgetVarianceSummary> VarianceSummaries { get; set; }

        #endregion

        #region Helper Properties

        /// <summary>
        /// Is Draft - مسودة
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public bool IsDraft => Status == "Draft";

        /// <summary>
        /// Is Approved - معتمد
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public bool IsApproved => Status == "Approved";

        /// <summary>
        /// Is Closed - مغلق
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public bool IsClosed => Status == "Closed";

        /// <summary>
        /// Can Edit - يمكن التعديل
        /// Only draft budgets can be edited
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public bool CanEdit => Status == "Draft";

        #endregion
    }

    /// <summary>
    /// Budget Status Constants - ثوابت حالة الميزانية
    /// </summary>
    public static class BudgetStatus
    {
        public const string Draft = "Draft";
        public const string Approved = "Approved";
        public const string Closed = "Closed";
    }

    /// <summary>
    /// Budget Type Constants - ثوابت نوع الميزانية
    /// </summary>
    public static class BudgetType
    {
        public const string Operational = "Operational";
        public const string Capital = "Capital";
        public const string Departmental = "Departmental";
        public const string Project = "Project";
    }
}
