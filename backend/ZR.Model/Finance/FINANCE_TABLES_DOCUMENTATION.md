# Finance Module - Database Tables Documentation

This document provides a comprehensive overview of all database tables in the Finance module (`ZR.Model\ERP\Finance\`), organized by folder structure.

## Table of Contents
- [ARAP (Accounts Receivable/Payable)](#arap-accounts-receivablepayable)
- [Assets](#assets)
- [Budget](#budget)
- [Configuration](#configuration)
- [Cost Center](#cost-center)
- [GL (General Ledger)](#gl-general-ledger)
- [Payment Hub](#payment-hub)
- [Tax](#tax)
- [Treasury](#treasury)
- [ZATCA (E-Invoicing)](#zatca-e-invoicing)

---

## ARAP (Accounts Receivable/Payable)

### 1. APInvoice (arap_ap_invoice)
Accounts Payable invoices - vendor bills

**Fields:**
- Id (Guid) - Primary Key
- InvoiceNumber (string)
- VendorId (Guid) - FK to Vendor
- InvoiceDate (DateTime)
- DueDate (DateTime)
- Currency (string)
- ExchangeRate (decimal)
- Subtotal (decimal)
- TaxAmount (decimal)
- TotalAmount (decimal)
- PaidAmount (decimal)
- RemainingAmount (decimal)
- Status (InvoiceStatus enum)
- TaxGroupId (Guid?)
- IsTaxInclusive (bool)
- Description (string)
- ReferenceNumber (string)
- GLJournalId (Guid?)
- FiscalYearId (Guid?)
- FiscalPeriodId (Guid?)
- Notes (string)

### 2. APInvoiceLine (arap_ap_invoice_line)
Line items for AP invoices

**Fields:**
- Id (Guid) - Primary Key
- InvoiceId (Guid) - FK to APInvoice
- LineNumber (int)
- AccountId (Guid) - FK to GLAccount
- Description (string)
- Quantity (decimal)
- UnitPrice (decimal)
- Amount (decimal)
- TaxCodeId (Guid?)
- TaxRate (decimal)
- TaxAmount (decimal)
- TotalAmount (decimal)
- TaxableAmount (decimal?)
- TaxInclusiveAmount (decimal?)
- CostCenterId (Guid?)
- DepartmentId (Guid?)
- ProjectId (Guid?)

### 3. ARInvoice (arap_ar_invoice)
Accounts Receivable invoices - customer invoices

**Fields:**
- Id (Guid) - Primary Key
- InvoiceNumber (string)
- CustomerId (Guid) - FK to Customer
- InvoiceDate (DateTime)
- DueDate (DateTime)
- Currency (string)
- ExchangeRate (decimal)
- Subtotal (decimal)
- TaxAmount (decimal)
- TotalAmount (decimal)
- PaidAmount (decimal)
- RemainingAmount (decimal)
- Status (InvoiceStatus enum)
- TaxGroupId (Guid?)
- IsTaxInclusive (bool)
- Description (string)
- ReferenceNumber (string)
- GLJournalId (Guid?)
- FiscalYearId (Guid?)
- FiscalPeriodId (Guid?)
- Notes (string)
- **ZATCA Fields:**
  - ZatcaUuid (string)
  - ZatcaInvoiceType (ZatcaInvoiceType?)
  - ZatcaInvoiceSubType (ZatcaInvoiceSubType?)
  - ZatcaStatus (ZatcaSubmissionStatus?)
  - ZatcaHashId (Guid?)
  - ZatcaCounter (long?)
  - ZatcaQrCode (string)
  - ZatcaHash (string)
  - ZatcaSubmittedAt (DateTime?)
  - ZatcaClearedAt (DateTime?)
  - ZatcaPaymentMeansCode (ZatcaPaymentMeansCode?)
  - IsB2B (bool?)
  - SupplyDate (DateTime?)
  - SupplyEndDate (DateTime?)

### 4. ARInvoiceLine (arap_ar_invoice_line)
Line items for AR invoices

**Fields:**
- Id (Guid) - Primary Key
- InvoiceId (Guid) - FK to ARInvoice
- LineNumber (int)
- AccountId (Guid) - FK to GLAccount
- Description (string)
- Quantity (decimal)
- UnitPrice (decimal)
- Amount (decimal)
- TaxCodeId (Guid?)
- TaxRate (decimal)
- TaxAmount (decimal)
- TotalAmount (decimal)
- TaxableAmount (decimal?)
- TaxInclusiveAmount (decimal?)
- CostCenterId (Guid?)
- DepartmentId (Guid?)
- ProjectId (Guid?)
- **ZATCA Fields:**
  - ZatcaTaxCategory (ZatcaTaxCategory?)
  - TaxExemptionReason (string)
  - TaxExemptionReasonCode (string)

### 5. CreditDebitNote (arap_credit_debit_note)
Credit and debit notes for invoice adjustments

**Fields:**
- Id (Guid) - Primary Key
- NoteNumber (string)
- NoteType (NoteType enum)
- CustomerId (Guid?)
- VendorId (Guid?)
- Amount (decimal)
- NoteDate (DateTime)
- Reason (string)
- GLJournalId (Guid?)
- OriginalInvoiceId (Guid?)
- OriginalInvoiceNumber (string)
- **ZATCA Fields:**
  - ZatcaUuid (string)
  - ZatcaInvoiceType (ZatcaInvoiceType?)
  - ZatcaInvoiceSubType (ZatcaInvoiceSubType?)
  - ZatcaStatus (ZatcaSubmissionStatus?)
  - ZatcaHashId (Guid?)
  - ZatcaCounter (long?)
  - ZatcaQrCode (string)
  - ZatcaHash (string)
  - ZatcaSubmittedAt (DateTime?)
  - ZatcaClearedAt (DateTime?)
  - ZatcaInstructionNoteReason (string)

### 6. Customer (arap_customer)
Customer master data

**Fields:**
- Id (Guid) - Primary Key
- CustomerCode (string)
- NameAr (string)
- NameEn (string)
- ContactPerson (string)
- Email (string)
- Phone (string)
- CreditLimit (decimal)
- GLAccountId (Guid?)
- IsActive (bool)
- AddressAr (string)
- AddressEn (string)
- TaxNumber (string)
- DefaultCurrencyId (Guid?)
- DefaultTaxGroupId (Guid?)
- PaymentTermsId (int?)
- CreditDays (int?)

### 7. Vendor (arap_vendor)
Vendor master data

**Fields:**
- Id (Guid) - Primary Key
- VendorCode (string)
- NameAr (string)
- NameEn (string)
- ContactPerson (string)
- Email (string)
- Phone (string)
- GLAccountId (Guid?)
- IsActive (bool)
- AddressAr (string)
- AddressEn (string)
- TaxNumber (string)
- DefaultCurrencyId (Guid?)
- DefaultTaxGroupId (Guid?)
- PaymentTermsId (int?)
- CreditDays (int?)

### 8. PaymentTerms (arap_payment_terms)
Payment terms configuration (e.g., Net 30, 2/10 Net 30)

**Fields:**
- Id (int) - Primary Key (Identity)
- Code (string)
- NameEn (string)
- NameAr (string)
- NetDays (int) - Days until payment is due
- DiscountDays (int?) - Days to receive discount
- DiscountPercent (decimal?) - Early payment discount
- DescriptionEn (string)
- DescriptionAr (string)
- IsActive (bool)
- SortOrder (int)

### 9. InvoiceNumberSequence (arap_invoice_number_sequence)
Atomic counter for invoice number generation

**Fields:**
- Id (Guid) - Primary Key
- SequenceType (string) - AR/AP/CN/DN
- CompanyId (long?)
- FiscalYear (int)
- CurrentNumber (long)
- MinDigits (int)
- RowVersion (int) - Optimistic concurrency

---

## Assets

### 1. AssetCategory (asset_category)
Asset categories with depreciation settings

**Fields:**
- Id (Guid) - Primary Key
- CategoryCode (string)
- NameAr (string)
- NameEn (string)
- DepreciationMethod (DepreciationMethod enum)
- UsefulLifeMonths (int)
- DefaultAssetAccountId (Guid?)
- DefaultDepreciationExpenseAccountId (Guid?)
- DefaultAccumulatedDepreciationAccountId (Guid?)
- DefaultGainLossAccountId (Guid?)
- IsActive (bool)

### 2. CurrentAsset (current_asset)
Current/short-term assets

**Fields:**
- Id (Guid) - Primary Key
- ItemCode (string)
- NameAr (string)
- NameEn (string)
- PurchaseDate (DateTime)
- Cost (decimal)
- CurrentValue (decimal)
- GLAccountId (Guid?)
- Status (CurrentAssetStatus enum)
- Category (string)
- Notes (string)

### 3. FixedAsset (fixed_asset)
Fixed/long-term assets

**Fields:**
- Id (Guid) - Primary Key
- AssetCode (string)
- NameAr (string)
- NameEn (string)
- PurchaseDate (DateTime)
- PurchaseCost (decimal)
- SalvageValue (decimal)
- CategoryId (Guid) - FK to AssetCategory
- GLAssetAccountId (Guid?)
- GLAccumulatedDepreciationAccountId (Guid?)
- GLDepreciationExpenseAccountId (Guid?)
- CurrentBookValue (decimal)
- AccumulatedDepreciation (decimal)
- LastDepreciationDate (DateTime?)
- Status (AssetStatus enum)
- Notes (string)

### 4. AssetDepreciation (asset_depreciation)
Depreciation history for assets

**Fields:**
- Id (Guid) - Primary Key
- AssetId (Guid) - FK to FixedAsset
- DepreciationDate (DateTime)
- DepreciationAmount (decimal)
- AccumulatedValueAfter (decimal)
- BookValueAfter (decimal)
- GLJournalId (Guid?)
- FiscalYear (int)
- FiscalPeriod (int)

### 5. AssetTransaction (asset_transaction)
Asset transactions (acquisition, disposal, transfer)

**Fields:**
- Id (Guid) - Primary Key
- AssetId (Guid) - FK to FixedAsset
- TransactionType (AssetTransactionType enum)
- TransactionDate (DateTime)
- Amount (decimal)
- PreviousBookValue (decimal)
- NewBookValue (decimal)
- GainLossAmount (decimal?)
- Notes (string)
- GLJournalId (Guid?)
- ReferenceNumber (string)
- FiscalYear (int)
- FiscalPeriod (int)

---

## Budget

### 1. BudgetHeader (finance_budget_header)
Budget planning header

**Fields:**
- Id (Guid) - Primary Key
- BudgetCode (string)
- Name (string)
- NameAr (string)
- Description (string)
- FiscalYear (int)
- Currency (string)
- Type (string) - Operational/Capital/Departmental/Project
- Status (string) - Draft/Approved/Closed
- ProjectId (Guid?)
- DepartmentId (long?)
- CostCenterId (Guid?)
- CreatedAt (DateTime)
- ApprovedBy (Guid?)
- ApprovedAt (DateTime?)
- ClosedBy (Guid?)
- ClosedAt (DateTime?)
- IsActive (bool)

### 2. BudgetLine (finance_budget_lines)
Budget line items per GL account and period

**Fields:**
- Id (Guid) - Primary Key
- BudgetHeaderId (Guid) - FK to BudgetHeader
- GLAccountId (Guid) - FK to GLAccount
- Period (int)
- BudgetedAmount (decimal)
- RevisedAmount (decimal)
- ActualAmount (decimal)
- CommittedAmount (decimal)
- LastActualUpdate (DateTime?)
- Notes (string)

### 3. BudgetVarianceSummary (finance_budget_variance_summary)
Pre-calculated variance data (background job)

**Fields:**
- Id (Guid) - Primary Key
- BudgetHeaderId (Guid) - FK to BudgetHeader
- GLAccountId (Guid) - FK to GLAccount
- Period (int)
- BudgetAmount (decimal)
- ActualAmount (decimal)
- Variance (decimal)
- VariancePercentage (decimal)
- CommittedAmount (decimal)
- AvailableAmount (decimal)
- LastRefreshed (DateTime)
- RefreshedByJobId (string)
- IsStale (bool)

---

## Configuration

### 1. Currency (currencies)
Currency master data (ISO 4217 compliant)

**Fields:**
- Id (Guid) - Primary Key
- Code (string) - ISO 4217 code (e.g., USD, EUR, SAR)
- Name (string)
- NameAr (string)
- Symbol (string) - Currency symbol
- IsBaseCurrency (bool)
- DecimalPlaces (int)
- IsActive (bool)
- RoundingMethod (int)
- DisplayFormat (string)
- Description (string)
- SortOrder (int)

### 2. CurrencySetting (currency_settings)
Global currency configuration

**Fields:**
- Id (Guid) - Primary Key
- BaseCurrencyCode (string)
- DefaultDisplayCurrency (string)
- AllowManualExchangeRate (bool)
- RequireRateApproval (bool)
- MaxRateAgeDays (int)
- AutoCreateRoundingAdjustment (bool)
- RoundingAdjustmentAccountId (Guid?)
- MaxRoundingDifference (decimal)
- AllowMulticurrencyReporting (bool)
- DefaultRateSource (string)
- TenantId (long?)

### 3. ExchangeRate (exchange_rates)
Daily exchange rates for currencies

**Fields:**
- Id (Guid) - Primary Key
- CurrencyId (Guid) - FK to Currency
- RateDate (DateTime)
- Rate (decimal) - Exchange rate
- InverseRate (decimal?) - Auto-calculated
- IsOverride (bool)
- Source (string)
- Notes (string)
- IsActive (bool)
- ApprovedBy (Guid?)
- ApprovedAt (DateTime?)

### 4. FiscalYear (finance_fiscal_year)
Fiscal year periods

**Fields:**
- Id (Guid) - Primary Key
- YearStartDate (DateTime)
- YearEndDate (DateTime)
- Status (FiscalYearStatus enum)
- Currency (string)
- DescriptionAr (string)
- DescriptionEn (string)

### 5. ERPSettings (finance_erp_settings)
ERP system configuration

**Fields:**
- Id (Guid) - Primary Key
- DefaultCurrencyId (Guid?)
- EnableMultiCurrency (bool)
- DefaultApprovalLimit (decimal)
- AutoPostToGL (bool)
- RequireApprovalForVouchers (bool)
- AllowBackdating (bool)
- MaxBackdatingDays (int)
- EnableBudgetControl (bool)
- FiscalYearStartMonth (int)
- NumberOfFiscalPeriods (int)
- DefaultTaxRuleId (Guid?)
- Notes (string)

### 6. AutoPostingRule (finance_auto_posting_rule)
Automatic GL posting rules

**Fields:**
- Id (Guid) - Primary Key
- TransactionType (TransactionType enum)
- DebitAccountId (Guid?)
- CreditAccountId (Guid?)
- DescriptionTemplateAr (string)
- DescriptionTemplateEn (string)
- Active (bool)
- Priority (int)

### 7. TaxRule (finance_tax_rule)
Tax rules configuration

**Fields:**
- Id (Guid) - Primary Key
- TaxName (string)
- TaxType (TaxType enum)
- Rate (decimal)
- ApplicableAccountId (Guid?)
- Status (bool)
- EffectiveFrom (DateTime?)
- EffectiveTo (DateTime?)
- DescriptionAr (string)
- DescriptionEn (string)

### 8. TaxConfiguration (tax_configuration)
Tax GL account mappings and VAT defaults

**Fields:**
- Id (Guid) - Primary Key
- VATInputAccountId (Guid?) - FK to GLAccount
- VATOutputAccountId (Guid?) - FK to GLAccount
- DefaultARAccountId (Guid?) - FK to GLAccount
- DefaultAPAccountId (Guid?) - FK to GLAccount
- IsConfigured (bool)

### 9. SystemConfiguration (system_configuration)
System-wide key-value configuration

**Fields:**
- Id (Guid) - Primary Key
- Key (string) - Configuration key
- Value (string) - Configuration value
- ValueType (string) - string/int/bool/json
- DescriptionAr (string)
- DescriptionEn (string)

---

## Cost Center

### 1. CostCenter (finance_cost_centers)
Cost centers for cost accounting

**Fields:**
- CostCenterId (long) - Primary Key
- ParentId (long?) - Hierarchical structure
- CostCenterCode (string)
- CostCenterName (string)
- CostCenterNameAr (string)
- CostCenterType (string) - branch/department/project
- Level (int)
- IsParent (bool)
- Description (string)
- IsActive (bool)

---

## GL (General Ledger)

### 1. GLAccount (gl_account)
Chart of Accounts (IFRS compliant)

**Fields:**
- AccountId (Guid) - Primary Key
- AccountNumber (string)
- FullAccountNumber (string) - Hierarchical code
- AccountNameAr (string)
- AccountNameEn (string)
- AccountType (AccountType enum)
- NormalBalance (NormalBalance enum)
- Level (int)
- ParentAccountId (Guid?)
- IsLeaf (bool)
- AccountPath (string)
- DisplayOrder (int)
- ChildCount (int)
- DescendantCount (int)
- IsActive (bool)
- AllowManualEntries (bool)
- AllowPostingToParent (bool)
- RequiresCostCenter (bool)
- RequiresDepartment (bool)
- RequiresProject (bool)
- IsCashAccount (bool)
- IsBankAccount (bool)
- IsControlAccount (bool)
- IsReconciliationAccount (bool)
- IsSystemAccount (bool)
- IsRetainedEarningsAccount (bool)
- Currency (string)
- IsForeignCurrency (bool)
- DescriptionAr (string)
- DescriptionEn (string)
- RowVersion (int)
- LastTransactionDate (DateTime?)
- ExternalCode (string)
- TaxCode (string)

### 2. GLJournal (gl_journal)
Journal entries (replaces GLVoucher)

**Fields:**
- Id (Guid) - Primary Key
- JournalNumber (string)
- JournalDate (DateTime)
- DescriptionAr (string)
- DescriptionEn (string)
- Status (JournalStatus enum)
- Source (string) - manual/auto/import/invoice/payment
- SourceDocumentType (string)
- SourceDocumentId (Guid?)
- TotalDebit (decimal)
- TotalCredit (decimal)
- Currency (string)
- CurrencyId (Guid?)
- ExchangeRate (decimal)
- IsRateOverride (bool)
- ReferenceNumber (string)
- PostedDate (DateTime?)
- PostedBy (Guid?)
- ApprovedDate (DateTime?)
- ApprovedBy (Guid?)
- FiscalYear (int?)
- FiscalPeriod (int?)
- ReversalJournalId (Guid?)
- OriginalJournalId (Guid?)

### 3. GLJournalEntry (gl_journal_entry)
Journal entry lines (replaces GLVoucherEntry)

**Fields:**
- Id (Guid) - Primary Key
- JournalId (Guid) - FK to GLJournal
- AccountId (Guid) - FK to GLAccount
- LineNumber (int)
- Debit (decimal) - Foreign currency
- Credit (decimal) - Foreign currency
- DescriptionAr (string)
- DescriptionEn (string)
- CostCenterId (Guid?)
- DepartmentId (Guid?)
- ProjectId (Guid?)
- ReferenceNumber (string)
- Currency (string)
- ExchangeRate (decimal)
- BaseDebit (decimal) - Base currency
- BaseCredit (decimal) - Base currency

### 4. GLAccountOpeningBalance (gl_account_opening_balance)
Opening balances per account per fiscal year

**Fields:**
- Id (Guid) - Primary Key
- AccountId (Guid) - FK to GLAccount
- FiscalYear (int)
- PeriodStartDate (DateTime)
- PeriodEndDate (DateTime)
- OpeningDebit (decimal)
- OpeningCredit (decimal)
- OpeningBalance (decimal)
- Currency (string)
- BaseOpeningDebit (decimal)
- BaseOpeningCredit (decimal)
- ExchangeRate (decimal)
- SourceType (string) - Manual/Carryforward/Migration
- SourceReference (string)
- IsLocked (bool)
- LockedAt (DateTime?)
- LockedBy (Guid?)
- Notes (string)

### 5. GLAccountBalanceSummary (gl_account_balance_summary)
Aggregated balances for performance

**Fields:**
- Id (Guid) - Primary Key
- AccountId (Guid) - FK to GLAccount
- FiscalYear (int)
- FiscalPeriod (int) - 0 = annual
- PeriodStartDate (DateTime)
- PeriodEndDate (DateTime)
- OpeningDebit (decimal)
- OpeningCredit (decimal)
- PeriodDebit (decimal)
- PeriodCredit (decimal)
- PeriodNet (decimal)
- ClosingDebit (decimal)
- ClosingCredit (decimal)
- ClosingBalance (decimal)
- YtdDebit (decimal)
- YtdCredit (decimal)
- YtdBalance (decimal)
- Currency (string)
- BasePeriodDebit (decimal)
- BasePeriodCredit (decimal)
- BaseClosingBalance (decimal)
- TransactionCount (int)
- DebitCount (int)
- CreditCount (int)
- LastCalculatedAt (DateTime)
- LastVoucherId (Guid?)
- LastVoucherDate (DateTime?)
- IsStale (bool)
- IsPeriodClosed (bool)

### 6. GLFinancialReport (gl_financial_report)
Generated financial reports

**Fields:**
- Id (Guid) - Primary Key
- ReportType (ReportType enum)
- ReportNumber (string)
- PeriodStartDate (DateTime)
- PeriodEndDate (DateTime)
- FiscalYear (int)
- DescriptionAr (string)
- DescriptionEn (string)
- ReportData (string) - JSON
- TotalDebit (decimal)
- TotalCredit (decimal)
- Currency (string)
- GeneratedAt (DateTime)
- GeneratedBy (Guid?)

### 7. GLPeriodClosing (gl_period_closing)
Period closing management

**Fields:**
- Id (Guid) - Primary Key
- PeriodType (PeriodType enum)
- PeriodStartDate (DateTime)
- PeriodEndDate (DateTime)
- FiscalYear (int)
- FiscalPeriod (int)
- Status (PeriodStatus enum)
- DescriptionAr (string)
- DescriptionEn (string)
- TotalVouchers (int)
- PostedVouchers (int)
- TotalDebit (decimal)
- TotalCredit (decimal)
- ClosingNotes (string)
- ClosedAt (DateTime?)
- ClosedBy (Guid?)
- ReopenedAt (DateTime?)
- ReopenedBy (Guid?)

---

## Payment Hub

### 1. Payment (hub_payment)
Unified payment transactions

**Fields:**
- Id (Guid) - Primary Key
- PaymentNumber (string)
- IdempotencyKey (string)
- PaymentType (PaymentType enum)
- PaymentMethod (PaymentMethod enum)
- Status (PaymentStatus enum)
- PaymentDate (DateTime)
- ValueDate (DateTime?)
- PostedAt (DateTime?)
- ApprovedAt (DateTime?)
- ReversedAt (DateTime?)
- Amount (decimal) - Transaction currency
- Currency (string)
- ExchangeRate (decimal)
- BaseCurrencyAmount (decimal)
- AllocatedAmount (decimal)
- UnallocatedAmount (decimal)
- FxGainLoss (decimal)
- SourceAccountId (Guid?)
- SourceAccountType (PaymentAccountType?)
- DestinationAccountId (Guid?)
- DestinationAccountType (PaymentAccountType?)
- CounterpartyType (CounterpartyType?)
- CounterpartyId (Guid?)
- CounterpartyName (string)
- GLJournalId (Guid?)
- ReversalGLJournalId (Guid?)
- BalanceLedgerEntryId (Guid?)
- FiscalYear (int)
- FiscalPeriod (int)
- CostCenterId (Guid?)
- DepartmentId (Guid?)
- ProjectId (Guid?)
- IsReversed (bool)
- ReversalOfId (Guid?)
- ReversedById (Guid?)
- ReversalReason (string)
- ApprovedBy (string)
- PostedBy (string)
- ApprovalComments (string)
- ReferenceNumber (string)
- BankReference (string)
- ChequeNumber (string)
- ChequeId (Guid?)
- Description (string)
- Notes (string)
- RowVersion (int)

### 2. PaymentAllocation (hub_payment_allocation)
Payment-to-invoice allocations

**Fields:**
- Id (Guid) - Primary Key
- PaymentId (Guid) - FK to Payment
- PaymentNumber (string)
- DocumentType (AllocatableDocumentType enum)
- DocumentId (Guid)
- DocumentNumber (string)
- AllocatedAmount (decimal) - Payment currency
- PaymentCurrency (string)
- AllocatedAmountDocCurrency (decimal)
- DocumentCurrency (string)
- ExchangeRate (decimal)
- FxGainLoss (decimal)
- FxGLJournalId (Guid?)
- DocumentBalanceBefore (decimal)
- DocumentBalanceAfter (decimal)
- Status (AllocationStatus enum)
- IsReversed (bool)
- ReversalDate (DateTime?)
- ReversalReason (string)
- ReversedBy (string)
- AllocationDate (DateTime)
- Notes (string)
- RowVersion (int)

### 3. PaymentEvent (hub_payment_event)
Payment audit trail (immutable)

**Fields:**
- Id (Guid) - Primary Key
- PaymentId (Guid) - FK to Payment
- PaymentNumber (string)
- EventType (PaymentEventType enum)
- EventTimestamp (DateTime)
- SequenceNumber (int)
- PreviousStatus (PaymentStatus?)
- NewStatus (PaymentStatus?)
- Amount (decimal?)
- Currency (string)
- AllocationId (Guid?)
- DocumentId (Guid?)
- GLJournalId (Guid?)
- BalanceLedgerEntryId (Guid?)
- UserId (string)
- UserName (string)
- IpAddress (string)
- UserAgent (string)
- CorrelationId (string)
- RequestId (string)
- Description (string)
- EventData (string) - JSON
- ErrorMessage (string)
- CompanyId (long?)

---

## Tax

### 1. TaxCode (tax_code)
Tax codes with GL mappings

**Fields:**
- Id (Guid) - Primary Key
- CompanyID (long?)
- Code (string)
- NameEn (string)
- NameAr (string)
- DescriptionEn (string)
- DescriptionAr (string)
- TaxType (TaxType enum)
- IsPercent (bool)
- Rate (decimal)
- DefaultPayableAccountId (Guid?)
- DefaultRecoverableAccountId (Guid?)
- ReverseChargeAccountId (Guid?)
- IsActive (bool)
- SortOrder (int)
- CountryCode (string)
- IsDefault (bool)
- IsRecoverable (bool)

### 2. TaxGroup (tax_group)
Composite tax groups (multiple taxes)

**Fields:**
- Id (Guid) - Primary Key
- CompanyID (long?)
- Code (string)
- NameEn (string)
- NameAr (string)
- DescriptionEn (string)
- DescriptionAr (string)
- IsCompound (bool) - Tax on tax
- IsActive (bool)
- SortOrder (int)

### 3. TaxGroupLine (tax_group_line)
Tax codes within a tax group

**Fields:**
- Id (Guid) - Primary Key
- TaxGroupId (Guid) - FK to TaxGroup
- TaxCodeId (Guid) - FK to TaxCode
- Sequence (int)
- OverrideRate (decimal?)
- IsActive (bool)

### 4. TaxRate (tax_rate)
Effective-dated tax rates

**Fields:**
- Id (Guid) - Primary Key
- TaxCodeId (Guid) - FK to TaxCode
- Rate (decimal)
- EffectiveFrom (DateTime)
- EffectiveTo (DateTime?)
- ChangeReason (string)
- LegalReference (string)
- IsApproved (bool)
- ApprovedBy (string)
- ApprovedAt (DateTime?)

### 5. TaxJournalLine (tax_journal_line)
Tax audit trail

**Fields:**
- Id (Guid) - Primary Key
- CompanyID (long?)
- JournalId (Guid?) - FK to GLJournal
- JournalEntryId (Guid?)
- InvoiceId (Guid?)
- InvoiceLineId (Guid?)
- SourceModule (string)
- SourceDocumentId (Guid?)
- SourceDocumentNumber (string)
- TaxCodeId (Guid) - FK to TaxCode
- TaxCodeValue (string)
- TaxRate (decimal)
- IsTaxInclusive (bool)
- TaxableAmountBase (decimal)
- TaxableAmountForeign (decimal?)
- TaxAmountBase (decimal)
- TaxAmountForeign (decimal?)
- Currency (string)
- ExchangeRate (decimal)
- TaxDate (DateTime)
- TaxAccountId (Guid?)
- IsRecoverable (bool)
- IsReverseCharge (bool)
- RoundingDelta (decimal)
- RoundingMode (string)
- IsPosted (bool)
- PostedAt (DateTime?)
- PostedBy (string)
- IsManualOverride (bool)
- OverrideReason (string)
- Notes (string)

---

## Treasury

### 1. BankAccount (bank_account)
Bank account master data

**Fields:**
- Id (Guid) - Primary Key
- BankCode (string)
- BankNameAr (string)
- BankNameEn (string)
- AccountNumber (string)
- IBAN (string)
- SwiftCode (string)
- Currency (string)
- CurrencyId (Guid?)
- Branch (string)
- BankContact (string)
- GLAccountId (Guid?)
- PendingChequesAmount (decimal)
- OverdraftLimit (decimal)
- IsOverdraftAllowed (bool)
- ReconciliationDate (DateTime?)
- Status (AccountStatus enum)
- IsDeleted (bool)
- DeletedAt (DateTime?)
- DeletedBy (string)
- RowVersion (int)
- Notes (string)
- CostCenterId (Guid?)
- DepartmentId (Guid?)

### 2. BalanceLedger (treasury_balance_ledger)
Immutable balance movement audit trail

**Fields:**
- Id (Guid) - Primary Key
- AccountId (Guid)
- AccountType (TreasuryAccountType enum)
- TransactionType (BalanceLedgerTransactionType enum)
- ReferenceEntityType (TreasuryEntityType enum)
- ReferenceEntityId (Guid)
- ReferenceNumber (string)
- TransactionDate (DateTime)
- ValueDate (DateTime?)
- DebitAmount (decimal)
- CreditAmount (decimal)
- NetAmount (decimal)
- BalanceBefore (decimal)
- BalanceAfter (decimal)
- AvailableBalanceBefore (decimal?)
- AvailableBalanceAfter (decimal?)
- Currency (string)
- ExchangeRate (decimal)
- BaseCurrencyAmount (decimal?)
- GLJournalId (Guid?)
- Description (string)
- IsReversal (bool)
- OriginalEntryId (Guid?)
- FiscalYear (int)
- FiscalPeriod (int)
- CostCenterId (Guid?)
- DepartmentId (Guid?)
- ProjectId (Guid?)
- CreatedByUserId (string)
- CreatedByUserName (string)
- IpAddress (string)
- UserAgent (string)
- TraceId (string)
- RequestId (string)
- Timestamp (DateTime)
- IsReconciled (bool)
- ReconciliationId (Guid?)
- RowVersion (int)
- Notes (string)

### 3. ApprovalLimit (treasury_approval_limit)
Role-based approval limits

**Fields:**
- Id (Guid) - Primary Key
- RoleId (long)
- RoleName (string)
- TransactionType (string) - Payment/Cheque/Transfer/All
- MinAmount (decimal)
- MaxAmount (decimal)
- Currency (string)
- DailyLimit (decimal)
- MonthlyLimit (decimal)
- RequiresDualApproval (bool)
- DualApprovalThreshold (decimal)
- CanApproveOwn (bool)
- EffectiveFrom (DateTime)
- EffectiveTo (DateTime?)
- IsActive (bool)
- Priority (int)
- Notes (string)

### 4. BankParsingProfile (bank_parsing_profile)
Bank statement parsing rules

**Fields:**
- Id (Guid) - Primary Key
- ProfileName (string)
- ProfileNameAr (string)
- BankCode (string)
- BankName (string)
- CountryCode (string)
- StatementFormat (BankStatementFormat enum)
- FileExtension (string)
- Encoding (string)
- CsvDelimiter (string)
- DateFormat (string)
- DecimalSeparator (string)
- ThousandSeparator (string)
- HasHeaderRow (bool)
- SkipRows (int)
- ColumnMapping (string) - JSON
- TransactionTypeMapping (string) - JSON
- ReferencePattern (string) - Regex
- ChequeNumberPattern (string) - Regex
- CreditIndicator (string)
- DebitIndicator (string)
- AmountFormat (string)
- ValidationRules (string) - JSON
- SampleData (string)
- IsActive (bool)
- Notes (string)


### 3.1 bank_account | جدول الحسابات البنكية

**Description:** Bank Account master data  
**Description Ar:** بيانات الحسابات البنكية الرئيسية

| Field Name | Column Name | Data Type | Nullable | Description |
|------------|-------------|-----------|----------|-------------|
| Id | id | Guid (PK) | No | Account ID - معرف الحساب |
| BankCode | bank_code | string(50) | No | Bank Code - رمز البنك |
| BankNameAr | bank_name_ar | string(200) | No | Bank Name (Arabic) - اسم البنك بالعربي |
| BankNameEn | bank_name_en | string(200) | No | Bank Name (English) - اسم البنك بالإنجليزي |
| AccountNumber | account_number | string(100) | No | Account Number - رقم الحساب |
| IBAN | iban | string(34) | Yes | IBAN - رقم الآيبان |
| SwiftCode | swift_code | string(11) | Yes | SWIFT Code - رمز السويفت |
| Currency | currency | string(3) | No | Currency Code - رمز العملة |
| CurrencyId | currency_id | Guid? | Yes | Currency ID - معرف العملة |
| Branch | branch | string(200) | Yes | Branch - الفرع |
| BankContact | bank_contact | string(500) | Yes | Bank Contact - جهة اتصال البنك |
| GLAccountId | gl_account_id | Guid? | Yes | GL Account ID - معرف حساب دفتر الأستاذ |
| PendingChequesAmount | pending_cheques_amount | decimal(18,4) | No | Pending Cheques Amount - مبلغ الشيكات المعلقة |
| OverdraftLimit | overdraft_limit | decimal(18,4) | No | Overdraft Limit - حد السحب على المكشوف |
| IsOverdraftAllowed | is_overdraft_allowed | bool | No | Is Overdraft Allowed - هل السحب على المكشوف مسموح |
| ReconciliationDate | reconciliation_date | DateTime? | Yes | Reconciliation Date - تاريخ آخر تسوية |
| Status | status | AccountStatus (enum) | No | Status - الحالة |
| IsDeleted | is_deleted | bool | No | Is Deleted - هل محذوف |
| DeletedAt | deleted_at | DateTime? | Yes | Deleted At - تاريخ الحذف |
| DeletedBy | deleted_by | string(100) | Yes | Deleted By - محذوف بواسطة |
| RowVersion | row_version | int | No | Row Version - نسخة الصف |
| Notes | notes | string(1000) | Yes | Notes - ملاحظات |
| CostCenterId | cost_center_id | Guid? | Yes | Cost Center ID - معرف مركز التكلفة |
| DepartmentId | department_id | Guid? | Yes | Department ID - معرف القسم |
| CompanyID | company_id | long? | Yes | Company ID - معرف الشركة |
| Create_time | create_time | DateTime | No | Create Time - وقت الإنشاء |
| Create_by | create_by | string(100) | Yes | Create By - منشئ بواسطة |
| Update_time | update_time | DateTime? | Yes | Update Time - وقت التحديث |
| Update_by | update_by | string(100) | Yes | Update By - محدث بواسطة |

---

### 3.2 cash_account | جدول الحسابات النقدية

**Description:** Cash Account master data  
**Description Ar:** بيانات الحسابات النقدية الرئيسية

| Field Name | Column Name | Data Type | Nullable | Description |
|------------|-------------|-----------|----------|-------------|
| Id | id | Guid (PK) | No | Account ID - معرف الحساب |
| Code | code | string(50) | No | Account Code - رمز الحساب |
| NameAr | name_ar | string(200) | No | Name (Arabic) - الاسم بالعربي |
| NameEn | name_en | string(200) | No | Name (English) - الاسم بالإنجليزي |
| Currency | currency | string(3) | No | Currency Code - رمز العملة |
| CurrencyId | currency_id | Guid? | Yes | Currency ID - معرف العملة |
| BranchId | branch_id | Guid? | Yes | Branch ID - معرف الفرع |
| GLAccountId | gl_account_id | Guid? | Yes | GL Account ID - معرف حساب دفتر الأستاذ |
| CashLimit | cash_limit | decimal(18,4) | No | Cash Limit - حد الصندوق |
| MinimumBalance | minimum_balance | decimal(18,4) | No | Minimum Balance - الحد الأدنى للرصيد |
| CustodianId | custodian_id | Guid? | Yes | Custodian ID - معرف أمين الصندوق |
| CustodianName | custodian_name | string(200) | Yes | Custodian Name - اسم أمين الصندوق |
| Location | location | string(500) | Yes | Location - الموقع |
| Type | type | AccountType (enum) | No | Account Type - نوع الحساب |
| AcceptsCheques | accepts_cheques | bool | No | Accepts Cheques - هل يقبل الشيكات |
| Status | status | AccountStatus (enum) | No | Status - الحالة |
| IsDeleted | is_deleted | bool | No | Is Deleted - هل محذوف |
| DeletedAt | deleted_at | DateTime? | Yes | Deleted At - تاريخ الحذف |
| DeletedBy | deleted_by | string(100) | Yes | Deleted By - محذوف بواسطة |
| RowVersion | row_version | int | No | Row Version - نسخة الصف |
| Notes | notes | string(1000) | Yes | Notes - ملاحظات |
| CostCenterId | cost_center_id | Guid? | Yes | Cost Center ID - معرف مركز التكلفة |
| DepartmentId | department_id | Guid? | Yes | Department ID - معرف القسم |
| CompanyID | company_id | long? | Yes | Company ID - معرف الشركة |
| Create_time | create_time | DateTime | No | Create Time - وقت الإنشاء |
| Create_by | create_by | string(100) | Yes | Create By - منشئ بواسطة |
| Update_time | update_time | DateTime? | Yes | Update Time - وقت التحديث |
| Update_by | update_by | string(100) | Yes | Update By - محدث بواسطة |

---

### 3.3 cheque | جدول الشيكات

**Description:** Cheque master data - Enhanced with Full Lifecycle, Available Balance Impact, Bounce Handling, and Attachments  
**Description Ar:** بيانات الشيكات الرئيسية - محسّن مع دورة حياة كاملة وتأثير الرصيد المتاح ومعالجة الارتجاع والمرفقات

| Field Name | Column Name | Data Type | Nullable | Description |
|------------|-------------|-----------|----------|-------------|
| Id | id | Guid (PK) | No | Cheque ID - معرف الشيك |
| ChequeNumber | cheque_number | string(50) | No | Cheque Number - رقم الشيك |
| ChequeType | cheque_type | ChequeType (enum) | No | Cheque Type - نوع الشيك (صادر/وارد) |
| IdempotencyKey | idempotency_key | string(100) | Yes | Idempotency Key - مفتاح التكرار |
| BankAccountId | bank_account_id | Guid (FK) | No | Bank Account ID - معرف الحساب البنكي المصدر |
| DrawerName | drawer_name | string(200) | Yes | Drawer Name - اسم الساحب |
| DrawerBankName | drawer_bank_name | string(200) | Yes | Drawer Bank Name - اسم بنك الساحب |
| PayeeName | payee_name | string(200) | No | Payee Name - اسم المستفيد |
| IssueDate | issue_date | DateTime | No | Issue Date - تاريخ الإصدار |
| DueDate | due_date | DateTime | No | Due Date - تاريخ الاستحقاق |
| PresentedDate | presented_date | DateTime? | Yes | Presented Date - تاريخ التقديم |
| ClearedDate | cleared_date | DateTime? | Yes | Cleared Date - تاريخ التصفية |
| BouncedDate | bounced_date | DateTime? | Yes | Bounced Date - تاريخ الارتجاع |
| CancelledDate | cancelled_date | DateTime? | Yes | Cancelled Date - تاريخ الإلغاء |
| Amount | amount | decimal(18,4) | No | Amount - المبلغ |
| Currency | currency | string(3) | No | Currency Code - رمز العملة |
| CurrencyId | currency_id | Guid? | Yes | Currency ID - معرف العملة (FK) |
| BankFees | bank_fees | decimal(18,4) | No | Bank Fees - رسوم البنك |
| Status | status | ChequeStatus (enum) | No | Status - الحالة |
| BounceReason | bounce_reason | string(500) | Yes | Bounce Reason - سبب الارتجاع |
| BounceCount | bounce_count | int | No | Bounce Count - عدد مرات الارتجاع |
| CancellationReason | cancellation_reason | string(500) | Yes | Cancellation Reason - سبب الإلغاء |
| RelatedPaymentId | related_payment_id | Guid? | Yes | Related Payment ID - معرف الدفعة المرتبطة |
| ReversalPaymentId | reversal_payment_id | Guid? | Yes | Reversal Payment ID - معرف دفعة العكس |
| IssuanceJournalId | issuance_journal_id | Guid? | Yes | GL Journal ID (Issuance) - معرف قيد الإصدار |
| ClearingJournalId | clearing_journal_id | Guid? | Yes | GL Journal ID (Clearing) - معرف قيد التصفية |
| BounceJournalId | bounce_journal_id | Guid? | Yes | GL Journal ID (Bounce) - معرف قيد الارتجاع |
| BankFeesJournalId | bank_fees_journal_id | Guid? | Yes | Bank Fees Journal ID - معرف قيد رسوم البنك |
| ClearingLedgerEntryId | clearing_ledger_entry_id | Guid? | Yes | Clearing Ledger Entry ID - معرف قيد سجل التصفية |
| BounceLedgerEntryId | bounce_ledger_entry_id | Guid? | Yes | Bounce Ledger Entry ID - معرف قيد سجل الارتجاع |
| ImpactsAvailableBalance | impacts_available_balance | bool | No | Impacts Available Balance - هل يؤثر على الرصيد المتاح |
| AvailableBalanceAdjusted | available_balance_adjusted | bool | No | Available Balance Adjusted - هل تم تعديل الرصيد المتاح |
| AttachmentPath | attachment_path | string(500) | Yes | Attachment Path - مسار المرفق |
| ReferenceNo | reference_no | string(100) | Yes | Reference Number - رقم المرجع |
| Memo | memo | string(500) | Yes | Memo - الملاحظة |
| CostCenterId | cost_center_id | Guid? | Yes | Cost Center ID - معرف مركز التكلفة |
| DepartmentId | department_id | Guid? | Yes | Department ID - معرف القسم |
| ProjectId | project_id | Guid? | Yes | Project ID - معرف المشروع |
| RowVersion | row_version | int | No | Row Version - نسخة الصف |
| Notes | notes | string(1000) | Yes | Notes - ملاحظات |
| CompanyID | company_id | long? | Yes | Company ID - معرف الشركة |
| Create_time | create_time | DateTime | No | Create Time - وقت الإنشاء |
| Create_by | create_by | string(100) | Yes | Create By - منشئ بواسطة |
| Update_time | update_time | DateTime? | Yes | Update Time - وقت التحديث |
| Update_by | update_by | string(100) | Yes | Update By - محدث بواسطة |

---

### 3.4 bank_statement | جدول كشوف الحسابات البنكية

**Description:** Bank Statement header  
**Description Ar:** رأس كشف الحساب البنكي

| Field Name | Column Name | Data Type | Nullable | Description |
|------------|-------------|-----------|----------|-------------|
| Id | id | Guid (PK) | No | Statement ID - معرف الكشف |
| BankAccountId | bank_account_id | Guid (FK) | No | Bank Account ID - معرف الحساب البنكي |
| StatementDate | statement_date | DateTime | No | Statement Date - تاريخ الكشف |
| StatementReference | statement_reference | string(200) | No | Statement Reference - مرجع الكشف |
| OpeningBalance | opening_balance | decimal(18,4) | No | Opening Balance - الرصيد الافتتاحي |
| ClosingBalance | closing_balance | decimal(18,4) | No | Closing Balance - الرصيد الختامي |
| Imported | imported | bool | No | Is Imported - مستورد |
| ImportSource | import_source | BankStatementImportSource (enum) | No | Import Source - مصدر الاستيراد |
| Notes | notes | string(1000) | Yes | Notes - ملاحظات |
| CompanyID | company_id | long? | Yes | Company ID - معرف الشركة |
| Create_time | create_time | DateTime | No | Create Time - وقت الإنشاء |
| Create_by | create_by | string(100) | Yes | Create By - منشئ بواسطة |
| Update_time | update_time | DateTime? | Yes | Update Time - وقت التحديث |
| Update_by | update_by | string(100) | Yes | Update By - محدث بواسطة |

---

### 3.5 bank_statement_line | جدول بنود كشف الحساب البنكي

**Description:** Bank Statement line items  
**Description Ar:** بنود كشف الحساب البنكي

| Field Name | Column Name | Data Type | Nullable | Description |
|------------|-------------|-----------|----------|-------------|
| Id | id | Guid (PK) | No | Line ID - معرف البند |
| BankStatementId | bank_statement_id | Guid (FK) | No | Bank Statement ID - معرف كشف الحساب |
| LineDate | line_date | DateTime | No | Line Date - تاريخ البند |
| ValueDate | value_date | DateTime | No | Value Date - تاريخ القيمة |
| Description | description | string(500) | No | Description - الوصف |
| Amount | amount | decimal(18,4) | No | Amount - المبلغ (موجب للائتمان/وارد، سالب للخصم/صادر) |
| RunningBalance | running_balance | decimal(18,4) | No | Running Balance - الرصيد المتجمع |
| ReferenceNo | reference_no | string(100) | Yes | Reference Number - رقم المرجع |
| Matched | matched | bool | No | Is Matched - مطابق |
| MatchedTransactionId | matched_transaction_id | Guid? | Yes | Matched Transaction ID - معرف المعاملة المطابقة |
| CompanyID | company_id | long? | Yes | Company ID - معرف الشركة |
| Create_time | create_time | DateTime | No | Create Time - وقت الإنشاء |
| Create_by | create_by | string(100) | Yes | Create By - منشئ بواسطة |
| Update_time | update_time | DateTime? | Yes | Update Time - وقت التحديث |
| Update_by | update_by | string(100) | Yes | Update By - محدث بواسطة |

---

### 3.6 bank_reconciliation_line | جدول بنود تسوية الحسابات البنكية

**Description:** Bank Reconciliation Line items  
**Description Ar:** بنود تسوية الحسابات البنكية

| Field Name | Column Name | Data Type | Nullable | Description |
|------------|-------------|-----------|----------|-------------|
| Id | id | Guid (PK) | No | Line ID - معرف البند |
| ReconciliationId | reconciliation_id | Guid (FK) | No | Reconciliation ID - معرف التسوية |
| BankStatementLineId | bank_statement_line_id | Guid (FK) | Yes | Bank Statement Line ID - معرف بند كشف الحساب |
| MatchedPaymentId | matched_payment_id | Guid (FK) | Yes | Matched Payment ID - معرف الدفعة المطابقة |
| MatchedChequeId | matched_cheque_id | Guid (FK) | Yes | Matched Cheque ID - معرف الشيك المطابق |
| LineStatus | line_status | ReconciliationLineStatus (enum) | No | Line Status - حالة البند |
| MatchType | match_type | string(20) | Yes | Match Type - نوع المطابقة |
| MatchConfidence | match_confidence | int | No | Match Confidence - درجة الثقة في المطابقة |
| StatementAmount | statement_amount | decimal(18,4) | No | Statement Amount - مبلغ الكشف |
| LedgerAmount | ledger_amount | decimal(18,4) | No | Ledger Amount - مبلغ السجل |
| DifferenceAmount | difference_amount | decimal(18,4) | No | Difference Amount - مبلغ الفرق |
| StatementDate | statement_date | DateTime? | Yes | Statement Date - تاريخ الكشف |
| StatementDescription | statement_description | string(500) | Yes | Statement Description - وصف الكشف |
| StatementReference | statement_reference | string(100) | Yes | Statement Reference - مرجع الكشف |
| LedgerDescription | ledger_description | string(500) | Yes | Ledger Description - وصف السجل |
| RequiresManualReview | requires_manual_review | bool | No | Requires Manual Review - هل تتطلب مراجعة يدوية |
| ReviewReason | review_reason | string(500) | Yes | Review Reason - سبب المراجعة |
| ReviewedBy | reviewed_by | string(100) | Yes | Reviewed By - مراجع بواسطة |
| ReviewedAt | reviewed_at | DateTime? | Yes | Reviewed At - تاريخ المراجعة |
| ExclusionReason | exclusion_reason | string(500) | Yes | Exclusion Reason - سبب الاستبعاد |
| CreatedJournalId | created_journal_id | Guid (FK) | Yes | Created Journal ID - معرف القيد المنشأ |
| AttachmentPath | attachment_path | string(500) | Yes | Attachment Path - مسار المرفق |
| Notes | notes | string(1000) | Yes | Notes - ملاحظات |
| CompanyID | company_id | long? | Yes | Company ID - معرف الشركة |
| Create_time | create_time | DateTime | No | Create Time - وقت الإنشاء |
| Create_by | create_by | string(100) | Yes | Create By - منشئ بواسطة |
| Update_time | update_time | DateTime? | Yes | Update Time - وقت التحديث |
| Update_by | update_by | string(100) | Yes | Update By - محدث بواسطة |

---

### 3.7 bank_reconciliation | جدول تسوية الحسابات البنكية

**Description:** Bank Reconciliation records  
**Description Ar:** سجلات تسوية الحسابات البنكية

| Field Name | Column Name | Data Type | Nullable | Description |
|------------|-------------|-----------|----------|-------------|
| Id | id | Guid (PK) | No | Reconciliation ID - معرف التسوية |
| ReconciliationNumber | reconciliation_number | string(50) | Yes | Reconciliation Number - رقم التسوية |
| BankAccountId | bank_account_id | Guid (FK) | No | Bank Account ID - معرف الحساب البنكي |
| BankStatementId | bank_statement_id | Guid (FK) | Yes | Bank Statement ID - معرف كشف الحساب |
| Currency | currency | string(3) | Yes | Currency - العملة |
| ReconciliationDate | reconciliation_date | DateTime | No | Reconciliation Date - تاريخ التسوية |
| PeriodStart | period_start | DateTime | No | Period Start - بداية الفترة |
| PeriodEnd | period_end | DateTime | No | Period End - نهاية الفترة |
| StatementOpeningBalance | statement_opening_balance | decimal(18,4) | No | Statement Opening Balance - الرصيد الافتتاحي للكشف |
| StatementClosingBalance | statement_closing_balance | decimal(18,4) | No | Statement Closing Balance - الرصيد الختامي للكشف |
| LedgerOpeningBalance | ledger_opening_balance | decimal(18,4) | No | Ledger Opening Balance - الرصيد الافتتاحي للسجل |
| LedgerClosingBalance | ledger_closing_balance | decimal(18,4) | No | Ledger Closing Balance - الرصيد الختامي للسجل |
| AdjustedBookBalance | adjusted_book_balance | decimal(18,4) | No | Adjusted Book Balance - الرصيد الدفتري المعدل |
| OutstandingCheques | outstanding_cheques | decimal(18,4) | No | Outstanding Cheques - الشيكات المعلقة |
| OutstandingChequesCount | outstanding_cheques_count | int | No | Outstanding Cheques Count - عدد الشيكات المعلقة |
| DepositsInTransit | deposits_in_transit | decimal(18,4) | No | Deposits In Transit - الإيداعات في الطريق |
| DepositsInTransitCount | deposits_in_transit_count | int | No | Deposits In Transit Count - عدد الإيداعات في الطريق |
| BankCharges | bank_charges | decimal(18,4) | No | Bank Charges - رسوم البنك |
| BankInterest | bank_interest | decimal(18,4) | No | Bank Interest - فوائد البنك |
| NsfCheques | nsf_cheques | decimal(18,4) | No | NSF Cheques - شيكات بدون رصيد |
| OtherAdjustments | other_adjustments | decimal(18,4) | No | Other Adjustments - تعديلات أخرى |
| FinalDifference | final_difference | decimal(18,4) | No | Final Difference - الفرق النهائي |
| ToleranceAmount | tolerance_amount | decimal(18,4) | No | Tolerance Amount - مبلغ التسامح |
| IsWithinTolerance | is_within_tolerance | bool | No | Is Within Tolerance - هل ضمن التسامح |
| MatchedItemsCount | matched_items_count | int | No | Matched Items Count - عدد البنود المتطابقة |
| UnmatchedStatementItems | unmatched_statement_items | int | No | Unmatched Statement Items - عدد بنود الكشف غير المتطابقة |
| UnmatchedLedgerItems | unmatched_ledger_items | int | No | Unmatched Ledger Items - عدد بنود السجل غير المتطابقة |
| DiscrepanciesSummary | discrepancies_summary | text | Yes | Discrepancies Summary - ملخص الفروقات |
| BankChargesJournalId | bank_charges_journal_id | Guid (FK) | Yes | Bank Charges Journal ID - معرف قيد رسوم البنك |
| BankInterestJournalId | bank_interest_journal_id | Guid (FK) | Yes | Bank Interest Journal ID - معرف قيد فوائد البنك |
| OtherAdjustmentsJournalId | other_adjustments_journal_id | Guid (FK) | Yes | Other Adjustments Journal ID - معرف قيد التعديلات الأخرى |
| AdjustmentJournalId | adjustment_journal_id | Guid (FK) | Yes | Adjustment Journal ID - معرف قيد التعديلات |
| FiscalYear | fiscal_year | int | No | Fiscal Year - السنة المالية |
| FiscalPeriod | fiscal_period | int | No | Fiscal Period - الفترة المالية |
| Status | status | ReconciliationStatus (enum) | No | Status - الحالة |
| ReconciledAt | reconciled_at | DateTime? | Yes | Reconciled At - تاريخ إتمام التسوية |
| ReconciledBy | reconciled_by | string(100) | Yes | Reconciled By - مسوى بواسطة |
| AttachmentPath | attachment_path | string(500) | Yes | Attachment Path - مسار المرفق |
| RowVersion | row_version | int | No | Row Version - نسخة الصف |
| Notes | notes | string(1000) | Yes | Notes - ملاحظات |
| CompanyID | company_id | long? | Yes | Company ID - معرف الشركة |
| Create_time | create_time | DateTime | No | Create Time - وقت الإنشاء |
| Create_by | create_by | string(100) | Yes | Create By - منشئ بواسطة |
| Update_time | update_time | DateTime? | Yes | Update Time - وقت التحديث |
| Update_by | update_by | string(100) | Yes | Update By - محدث بواسطة |

---

### 3.8 treasury_balance_ledger | جدول سجل الرصيد

**Description:** Balance Ledger - Immutable audit trail of balance movements  
**Description Ar:** سجل الرصيد - سجل تدقيق غير قابل للتعديل لحركات الرصيد

| Field Name | Column Name | Data Type | Nullable | Description |
|------------|-------------|-----------|----------|-------------|
| Id | id | Guid (PK) | No | Ledger Entry ID - معرف قيد السجل |
| AccountId | account_id | Guid (FK) | No | Account ID - معرف الحساب |
| AccountType | account_type | TreasuryAccountType (enum) | No | Account Type - نوع الحساب |
| TransactionType | transaction_type | BalanceLedgerTransactionType (enum) | No | Transaction Type - نوع المعاملة |
| ReferenceEntityType | reference_entity_type | TreasuryEntityType (enum) | No | Reference Entity Type - نوع الكيان المرجعي |
| ReferenceEntityId | reference_entity_id | Guid (FK) | No | Reference Entity ID - معرف الكيان المرجعي |
| ReferenceNumber | reference_number | string(100) | Yes | Reference Number - رقم المرجع |
| TransactionDate | transaction_date | DateTime | No | Transaction Date - تاريخ المعاملة |
| ValueDate | value_date | DateTime? | Yes | Value Date - تاريخ القيمة |
| DebitAmount | debit_amount | decimal(18,4) | No | Debit Amount - مبلغ المدين |
| CreditAmount | credit_amount | decimal(18,4) | No | Credit Amount - مبلغ الدائن |
| NetAmount | net_amount | decimal(18,4) | No | Net Amount - صافي المبلغ |
| BalanceBefore | balance_before | decimal(18,4) | No | Balance Before - الرصيد قبل المعاملة |
| BalanceAfter | balance_after | decimal(18,4) | No | Balance After - الرصيد بعد المعاملة |
| Currency | currency | string(3) | No | Currency - العملة |
| ExchangeRate | exchange_rate | decimal(18,6) | No | Exchange Rate - سعر الصرف |
| Description | description | string(500) | Yes | Description - الوصف |
| DescriptionAr | description_ar | string(500) | Yes | Description (Arabic) - الوصف بالعربي |
| FiscalYear | fiscal_year | int | No | Fiscal Year - السنة المالية |
| FiscalPeriod | fiscal_period | int | No | Fiscal Period - الفترة المالية |
| IsReversed | is_reversed | bool | No | Is Reversed - هل معكوس |
| ReversalOfId | reversal_of_id | Guid? | Yes | Reversal Of ID - معرف القيد الأصلي |
| ReversedById | reversed_by_id | Guid? | Yes | Reversed By ID - معرف قيد العكس |
| ReversalReason | reversal_reason | string(500) | Yes | Reversal Reason - سبب العكس |
| UserId | user_id | string(100) | No | User ID - معرف المستخدم |
| CompanyID | company_id | long? | Yes | Company ID - معرف الشركة |
| Create_time | create_time | DateTime | No | Create Time - وقت الإنشاء |

---

### 3.9 treasury_config | جدول إعدادات الخزينة

**Description:** Treasury Configuration master data  
**Description Ar:** بيانات إعدادات الخزينة الرئيسية

| Field Name | Column Name | Data Type | Nullable | Description |
|------------|-------------|-----------|----------|-------------|
| Id | id | Guid (PK) | No | Config ID - معرف الإعداد |
| AutoImportBankStatements | auto_import_bank_statements | bool | No | Auto Import Bank Statements - استيراد تلقائي لكشوف الحساب |
| BankStatementImportFormat | bank_statement_import_format | BankStatementImportSource (enum) | No | Bank Statement Import Format - تنسيق استيراد كشف الحساب |
| AutoReconcile | auto_reconcile | bool | No | Auto Reconcile - تسوية تلقائية |
| DefaultPaymentApprovalThreshold | default_payment_approval_threshold | decimal(18,4) | No | Default Payment Approval Threshold - حد الموافقة الافتراضي للدفعات |
| PettyCashLimit | petty_cash_limit | decimal(18,4) | No | Petty Cash Limit - حد الصندوق الصغير |
| ApprovalWorkflowEnabled | approval_workflow_enabled | bool | No | Approval Workflow Enabled - تفعيل سير عمل الموافقة |
| CompanyID | company_id | long? | Yes | Company ID - معرف الشركة |
| Create_time | create_time | DateTime | No | Create Time - وقت الإنشاء |
| Create_by | create_by | string(100) | Yes | Create By - منشئ بواسطة |
| Update_time | update_time | DateTime? | Yes | Update Time - وقت التحديث |
| Update_by | update_by | string(100) | Yes | Update By - محدث بواسطة |

---

### 3.10 treasury_approval_limit | جدول حدود الموافقة

**Description:** Approval Limit Configuration - Role-based approval limits  
**Description Ar:** إعدادات حدود الموافقة - حدود الموافقة القائمة على الأدوار

| Field Name | Column Name | Data Type | Nullable | Description |
|------------|-------------|-----------|----------|-------------|
| Id | id | Guid (PK) | No | Approval Limit ID - معرف حد الموافقة |
| RoleId | role_id | long | No | Role ID - معرف الدور |
| RoleName | role_name | string(100) | Yes | Role Name - اسم الدور |
| TransactionType | transaction_type | string(50) | No | Transaction Type - نوع المعاملة |
| MinAmount | min_amount | decimal(18,4) | No | Minimum Amount - الحد الأدنى للمبلغ |
| MaxAmount | max_amount | decimal(18,4) | No | Maximum Amount - الحد الأقصى للمبلغ |
| Currency | currency | string(3) | Yes | Currency Code - رمز العملة |
| DailyLimit | daily_limit | decimal(18,4) | No | Daily Limit - الحد اليومي |
| MonthlyLimit | monthly_limit | decimal(18,4) | No | Monthly Limit - الحد الشهري |
| RequiresDualApproval | requires_dual_approval | bool | No | Requires Dual Approval - يتطلب موافقة مزدوجة |
| DualApprovalThreshold | dual_approval_threshold | decimal(18,4) | No | Dual Approval Threshold - حد الموافقة المزدوجة |
| CanApproveOwn | can_approve_own | bool | No | Can Approve Own Transactions - يمكنه الموافقة على معاملاته الخاصة |
| EffectiveFrom | effective_from | DateTime | No | Effective From Date - تاريخ بدء السريان |
| EffectiveTo | effective_to | DateTime? | Yes | Effective To Date - تاريخ انتهاء السريان |
| IsActive | is_active | bool | No | Is Active - نشط |
- Priority 
- Notes 
- CompanyID 
-Create_time 
-Create_by 
-Update_time 
-Update_by 


### 3.14 treasury_data_retention_config | جدول إعدادات الاحتفاظ بالبيانات

**Description:** Treasury Data Retention Configuration - Audit log archiving and data retention policy  
**Description Ar:** إعدادات الاحتفاظ بالبيانات - أرشفة سجل التدقيق وسياسة الاحتفاظ بالبيانات

| Field Name | Column Name | Data Type | Nullable | Description |
|------------|-------------|-----------|----------|-------------|
| Id | id | Guid (PK) | No | Config ID - معرف الإعدادات |
| ConfigName | config_name | string(100) | No | Config Name - اسم الإعدادات |
| AuditLogRetentionDays | audit_log_retention_days | int | No | Audit Log Retention Days - أيام الاحتفاظ بسجل التدقيق |
| AuditLogArchiveEnabled | audit_log_archive_enabled | bool | No | Audit Log Archive Enabled - تفعيل أرشفة سجل التدقيق |
| AuditLogArchivePath | audit_log_archive_path | string(500) | Yes | Audit Log Archive Path - مسار أرشيف سجل التدقيق |
| LastAuditArchiveDate | last_audit_archive_date | DateTime? | Yes | Last Audit Archive Date - تاريخ آخر أرشفة |
| LastAuditArchiveCount | last_audit_archive_count | long | No | Last Audit Archive Record Count - عدد السجلات المؤرشفة |
| BankStatementRetentionDays | bank_statement_retention_days | int | No | Bank Statement Retention Days - أيام الاحتفاظ بكشوف البنك |
| BankStatementArchiveEnabled | bank_statement_archive_enabled | bool | No | Bank Statement Archive Enabled - تفعيل أرشفة كشوف البنك |
| ReconciliationRetentionDays | reconciliation_retention_days | int | No | Reconciliation Retention Days - أيام الاحتفاظ بالتسويات |
| KeepReconciliationAttachments | keep_reconciliation_attachments | bool | No | Keep Reconciliation Attachments - الاحتفاظ بمرفقات التسويات |
| PaymentRetentionDays | payment_retention_days | int | No | Payment Retention Days - أيام الاحتفاظ بالدفعات |
| ArchivePostedPaymentsOnly | archive_posted_payments_only | bool | No | Archive Posted Payments Only - أرشفة الدفعات المسجلة فقط |
| ChequeRetentionDays | cheque_retention_days | int | No | Cheque Retention Days - أيام الاحتفاظ بالشيكات |
| ArchiveClearedChequesOnly | archive_cleared_cheques_only | bool | No | Archive Cleared Cheques Only - أرشفة الشيكات المسددة فقط |
| AttachmentRetentionDays | attachment_retention_days | int | No | Attachment Retention Days - أيام الاحتفاظ بالمرفقات |
| CompressAttachments | compress_attachments | bool | No | Compress Attachments - ضغط المرفقات قبل الأرشفة |
| ArchiveSchedule | archive_schedule | string(50) | Yes | Archive Schedule - جدول الأرشفة (Cron expression) |
| ArchiveBatchSize | archive_batch_size | int | No | Archive Batch Size - حجم دفعة الأرشفة |
| ArchiveFormat | archive_format | string(20) | Yes | Archive Format - تنسيق الأرشيف |
| ArchiveStorageType | archive_storage_type | string(20) | Yes | Archive Storage Type - نوع تخزين الأرشيف |
| ArchiveStorageConnection | archive_storage_connection | string(500) | Yes | Archive Storage Connection - اتصال تخزين الأرشيف |
| ZATCACompliance | zatca_compliance | bool | No | ZATCA Compliance - التوافق مع هيئة الزكاة والضريبة والجمارك |
| IFRSCompliance | ifrs_compliance | bool | No | IFRS Compliance - التوافق مع معايير التقارير المالية الدولية |
| LegalHoldEnabled | legal_hold_enabled | bool | No | Legal Hold Enabled - تفعيل الحجز القانوني |
| IsActive | is_active | bool | No | Is Active - نشط |
| Notes | notes | string(1000) | Yes | Notes - ملاحظات |
| CompanyID | company_id | long? | Yes | Company ID - معرف الشركة |
| Create_time | create_time | DateTime | No | Create Time - وقت الإنشاء |
| Create_by | create_by | string(100) | Yes | Create By - منشئ بواسطة |
| Update_time | update_time | DateTime? | Yes | Update Time - وقت التحديث |
| Update_by | update_by | string(100) | Yes | Update By - محدث بواسطة |

---

### 3.15 treasury_audit_log | جدول سجل تدقيق الخزينة

**Description:** Treasury Audit Log - Tracking all sensitive treasury operations  
**Description Ar:** سجل تدقيق الخزينة - تتبع جميع عمليات الخزينة الحساسة

| Field Name | Column Name | Data Type | Nullable | Description |
|------------|-------------|-----------|----------|-------------|
| Id | id | Guid (PK) | No | Audit Log ID - معرف سجل التدقيق |
| EntityType | entity_type | TreasuryEntityType (enum) | No | Entity Type - نوع الكيان |
| EntityId | entity_id | Guid | No | Entity ID - معرف الكيان |
| EntityNumber | entity_number | string(100) | Yes | Entity Number - رقم الكيان |
| Action | action | TreasuryAuditAction (enum) | No | Action - الإجراء |
| ActionDescription | action_description | string(500) | Yes | Action Description - وصف الإجراء |
| OldValues | old_values | text | Yes | Old Values - القيم القديمة (JSON) |
| NewValues | new_values | text | Yes | New Values - القيم الجديدة (JSON) |
| ChangedFields | changed_fields | string(1000) | Yes | Changed Fields - الحقول المتغيرة |
| Amount | amount | decimal(18,4)? | Yes | Amount - المبلغ |
| Currency | currency | string(3) | Yes | Currency - العملة |
| UserId | user_id | string(100) | No | User ID - معرف المستخدم |
| UserName | user_name | string(200) | Yes | User Name - اسم المستخدم |
| IpAddress | ip_address | string(50) | Yes | IP Address - عنوان IP |
| UserAgent | user_agent | string(500) | Yes | User Agent - وكيل المستخدم |
| TraceId | trace_id | string(100) | Yes | Trace ID - معرف التتبع |
| RequestId | request_id | string(100) | Yes | Request ID - معرف الطلب |
| Timestamp | timestamp | DateTime | No | Timestamp - الطابع الزمني |
| IsSuccessful | is_successful | bool | No | Is Successful - هل نجح الإجراء |
| ErrorMessage | error_message | string(1000) | Yes | Error Message - رسالة الخطأ |
| AdditionalContext | additional_context | text | Yes | Additional Context - سياق إضافي (JSON) |
| CompanyID | company_id | long? | Yes | Company ID - معرف الشركة |
| Create_time | create_time | DateTime | No | Create Time - وقت الإنشاء |
| Create_by | create_by | string(100) | Yes | Create By - منشئ بواسطة |

---


###  11 cheque_status_history
-Id
-ChequeId
-previous_status
-new_status
-changed_at
-changed_by
-reason
-notes
-bank_reference
-gl_journal_id
-ip_address
-created_at





### 15. TreasuryGLAccountMapping (treasury_gl_account_mapping)
GL account mappings for treasury

**Fields:**
- Id (Guid) - Primary Key
- MappingType (string)
- GLAccountId (Guid)
- GLAccountCode (string)
- GLAccountName (string)
- Currency (string)
- Description (string)
- IsActive (bool)
- Priority (int)
- EffectiveFrom (DateTime)
- EffectiveTo (DateTime?)

### 16. TreasuryNumberSequence (treasury_number_sequence)
Number sequence for treasury documents

**Fields:**
- Id (Guid) - Primary Key
- SequenceType (string) - PAY/RCP/TRF/CHQ
- FiscalYear (int)
- Prefix (string)
- Suffix (string)
- CurrentNumber (long)
- IncrementBy (int)
- MinDigits (int)
- ResetOnNewYear (bool)
- LastUsedAt (DateTime?)
- IsActive (bool)
- RowVersion (int)

---

## ZATCA (E-Invoicing)

### 1. ZatcaConfiguration (zatca_configuration)
Company-level ZATCA settings

**Fields:**
- Id (Guid) - Primary Key
- **Seller Information:**
  - SellerVatNumber (string)
  - SellerNameAr (string)
  - SellerNameEn (string)
  - SellerStreet (string)
  - SellerBuildingNumber (string)
  - SellerPlotIdentification (string)
  - SellerCitySubdivision (string)
  - SellerCity (string)
  - SellerPostalCode (string)
  - SellerCountryCode (string)
  - SellerCountrySubentity (string)
  - SellerAdditionalStreet (string)
- **API Configuration:**
  - Environment (ZatcaEnvironment enum)
  - SandboxApiUrl (string)
  - SimulationApiUrl (string)
  - ProductionApiUrl (string)
  - ApiTimeout (int)
  - MaxRetryAttempts (int)
  - RetryDelayMs (int)
- **Invoice Settings:**
  - DefaultPaymentMeansCode (ZatcaPaymentMeansCode enum)
  - AutoSubmitStandard (bool)
  - AutoSubmitSimplified (bool)
  - QrForStandard (bool)
  - QrForSimplified (bool)
  - DefaultCurrency (string)
- **Status:**
  - ActiveDeviceId (Guid?)
  - IsEnabled (bool)
  - IsConfigured (bool)
  - LastConfigCheck (DateTime?)

### 2. ZatcaDevice (zatca_device)
Registered e-invoicing devices

**Fields:**
- Id (Guid) - Primary Key
- DeviceSerialNumber (string)
- DeviceName (string)
- OrganizationIdentifier (string)
- OrganizationUnitName (string)
- OrganizationNameEn (string)
- OrganizationNameAr (string)
- CountryCode (string)
- InvoiceType (string) - Binary flags
- LocationAddress (string)
- IndustryBusinessCategory (string)
- **Certificate Fields:**
  - Csr (string)
  - PrivateKey (string)
  - ComplianceCsid (string)
  - ComplianceCsidSecret (string)
  - ComplianceCertificate (string)
  - ProductionCsid (string)
  - ProductionCsidSecret (string)
  - ProductionCertificate (string)
  - CertificateExpiryDate (DateTime?)
- **Status:**
  - Environment (ZatcaEnvironment enum)
  - IsActive (bool)
  - IsComplianceCompleted (bool)
  - IsProductionReady (bool)
  - ComplianceCompletedAt (DateTime?)
  - ProductionActivatedAt (DateTime?)
  - LastUsedAt (DateTime?)

### 3. ZatcaInvoiceHash (zatca_invoice_hash)
Invoice hash chain tracking

**Fields:**
- Id (Guid) - Primary Key
- InvoiceId (Guid)
- InvoiceSourceType (string)
- ZatcaInvoiceType (ZatcaInvoiceType enum)
- ZatcaInvoiceSubType (ZatcaInvoiceSubType enum)
- InvoiceCounterValue (long)
- InvoiceHash (string) - SHA-256
- PreviousInvoiceHash (string)
- InvoiceUuid (string)
- InvoiceNumber (string)
- IssueDateTime (DateTime)
- DeviceId (Guid)
- SignedXml (string)
- QrCode (string)
- CryptographicStamp (string)
- **Submission Status:**
  - SubmissionStatus (ZatcaSubmissionStatus enum)
  - SubmittedAt (DateTime?)
  - ZatcaResponseCode (string)
  - ClearanceStatus (string)
  - ReportingStatus (string)
  - ValidationResults (string) - JSON
  - ErrorMessages (string) - JSON
  - WarningCount (int)
  - ErrorCount (int)
  - RetryCount (int)
  - LastRetryAt (DateTime?)

### 4. ZatcaSubmissionLog (zatca_submission_log)
ZATCA API call audit trail

**Fields:**
- Id (Guid) - Primary Key
- InvoiceHashId (Guid) - FK to ZatcaInvoiceHash
- DeviceId (Guid)
- ApiEndpoint (string)
- OperationType (string)
- Environment (ZatcaEnvironment enum)
- SubmissionDate (DateTime)
- RequestHeaders (string) - JSON
- RequestBody (string)
- HttpStatusCode (int?)
- ResponseHeaders (string) - JSON
- ResponseBody (string)
- DispositionStatus (string)
- IsSuccess (bool)
- ErrorCode (string)
- ErrorMessage (string)
- ValidationSteps (string) - JSON
- ResponseTimeMs (long?)
- RetryAttempt (int)
- ClientIp (string)
- UserAgent (string)

---

## Summary

**Total Tables: 58**

- ARAP: 9 tables
- Assets: 5 tables
- Budget: 3 tables
- Configuration: 9 tables
- Cost Center: 1 table
- GL: 7 tables
- Payment Hub: 3 tables
- Tax: 5 tables
- Treasury: 16 tables
- ZATCA: 4 tables
- Enums: 6 tables (TreasuryAccountType, BalanceLedgerTransactionType defined in BalanceLedger.cs)

**Notes:**
- All tables inherit from `SysBase` which provides common fields: `CompanyID`, `BranchID`, `Create_by`, `Create_time`, `Update_by`, `Update_time`
- Primary keys are mostly `Guid` type for distributed ID generation
- SqlSugar ORM is used with `[SugarTable]` and `[SugarColumn]` attributes
- Multi-currency support is integrated throughout
- ZATCA e-invoicing compliance fields are embedded in AR/AP invoices
- Payment Hub provides unified payment processing across AR/AP/Treasury
