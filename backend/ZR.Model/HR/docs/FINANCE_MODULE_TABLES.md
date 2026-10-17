## @Finance

> Tables and fields for `ZR.Model/HR/Finance/*.cs`.
> Format: `FieldName (CSharpType)`

### hr_finance_reconciliation

- ReconciliationId (Guid)
- PayrollId (long)
- APInvoiceId (Guid)
- ReconciledAmount (decimal)
- Status (HrFinanceReconciliationStatus)
- AIInsight (string)

### hr_financial_audit_log

- LogId (Guid)
- Action (string)
- UserId (long)
- Timestamp (DateTime)
- Changes (string)

### hr_payroll_zatca

- Id (Guid)
- PayrollId (long)
- ZatcaHashId (Guid)
- SubmissionStatus (ZatcaSubmissionStatus)

