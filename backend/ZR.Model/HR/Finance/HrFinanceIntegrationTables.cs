using SqlSugar;
using System;
using ZR.Model.HR.Finance.Enums;
using ZR.Model.System;
using ZR.Model.Finance.Zatca.Enums;

namespace ZR.Model.HR.Finance
{
    /// <summary>
    /// HR-Finance Reconciliation Table
    /// </summary>
    [SugarTable("hr_finance_reconciliation")]
    public class HrFinanceReconciliation : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        public Guid ReconciliationId { get; set; }

        public long PayrollId { get; set; }

        public Guid APInvoiceId { get; set; }

        [SugarColumn(Length = 18, DecimalDigits = 4)]
        public decimal ReconciledAmount { get; set; }

        public HrFinanceReconciliationStatus Status { get; set; } = HrFinanceReconciliationStatus.Pending;

        [SugarColumn(Length = 2000, IsNullable = true)]
        public string AIInsight { get; set; }
    }

    /// <summary>
    /// Payroll ZATCA table
    /// </summary>
    [SugarTable("hr_payroll_zatca")]
    public class HrPayrollZatca : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        public Guid Id { get; set; }

        public long PayrollId { get; set; }

        public Guid ZatcaHashId { get; set; }

        public ZatcaSubmissionStatus SubmissionStatus { get; set; } = ZatcaSubmissionStatus.NotSubmitted;
    }

    /// <summary>
    /// Financial audit log table for HR operations
    /// </summary>
    [SugarTable("hr_financial_audit_log")]
    public class HrFinancialAuditLog : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        public Guid LogId { get; set; }

        [SugarColumn(Length = 200, IsNullable = false)]
        public string Action { get; set; }

        public long UserId { get; set; }

        public DateTime Timestamp { get; set; }

        [SugarColumn(IsNullable = true, ColumnDataType = StaticConfig.CodeFirst_BigString)]
        public string Changes { get; set; }
    }
}















