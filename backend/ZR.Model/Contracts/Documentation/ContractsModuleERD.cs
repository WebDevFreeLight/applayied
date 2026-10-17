namespace ZR.Model.Contracts.Documentation
{
    /// <summary>
    /// Contracts Module ERD and Integration Points
    /// مخطط العلاقات ونقاط التكامل لوحدة العقود
    ///
    /// This file documents the Entity Relationship Diagram and all integration points.
    /// </summary>
    public static class ContractsModuleERD
    {
        /*
        ╔══════════════════════════════════════════════════════════════════════════════╗
        ║                     CONTRACTS MODULE ENTITY RELATIONSHIP DIAGRAM              ║
        ║                     مخطط علاقات كيانات وحدة العقود                            ║
        ╚══════════════════════════════════════════════════════════════════════════════╝

        ============================================================================
        DATABASE TABLES OVERVIEW
        ============================================================================

        ┌─────────────────────────────────────────────────────────────────────────────┐
        │ Table Name                    │ Entity                  │ Description      │
        ├─────────────────────────────────────────────────────────────────────────────┤
        │ erp_contract                  │ ErpContract             │ Contract master  │
        │ erp_contract_unit             │ ErpContractUnit         │ Contract units   │
        │ erp_contract_pricing_rule     │ ErpContractPricingRule  │ Pricing rules    │
        │ erp_contract_billing_schedule │ ErpContractBillingSchedule│ Billing sched  │
        │ erp_contract_billing_line     │ ErpContractBillingScheduleLine│ Line items │
        │ erp_contract_amendment        │ ErpContractAmendment    │ Amendments       │
        │ erp_contract_renewal          │ ErpContractRenewal      │ Renewals         │
        │ erp_contract_termination      │ ErpContractTermination  │ Terminations     │
        └─────────────────────────────────────────────────────────────────────────────┘

        ============================================================================
        ENTITY RELATIONSHIP DIAGRAM (ASCII)
        ============================================================================

                                    ┌─────────────────────┐
                                    │  BusinessPartner    │
                                    │  (Existing Module)  │
                                    └──────────┬──────────┘
                                               │
                                               │ 1:N (CustomerId)
                                               │
        ┌──────────────────────────────────────┼──────────────────────────────────────┐
        │                                      ▼                                      │
        │    ┌─────────────────────────────────────────────────────────────────────┐  │
        │    │                         erp_contract                                │  │
        │    │  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━ │  │
        │    │  PK: contract_id (GUID)                                             │  │
        │    │  ─────────────────────────────────────────────────────────────────  │  │
        │    │  FK: customer_id → business_partner.partner_id                      │  │
        │    │  FK: tax_group_id → tax_group.id (Optional - Finance Module)        │  │
        │    │  ─────────────────────────────────────────────────────────────────  │  │
        │    │  contract_number, contract_type, status, start_date, end_date,      │  │
        │    │  base_amount, total_amount, currency, payment_terms, auto_renew,    │  │
        │    │  billing_frequency, security_deposit, renewal_type, etc.            │  │
        │    └─────────────────────────────────────────────────────────────────────┘  │
        │              │              │              │              │                  │
        │              │ 1:N          │ 1:N          │ 1:N          │ 1:N              │
        │              ▼              ▼              ▼              ▼                  │
        │    ┌─────────────┐ ┌───────────────┐ ┌────────────┐ ┌────────────────┐      │
        │    │erp_contract_│ │erp_contract_  │ │erp_contract│ │erp_contract_   │      │
        │    │   unit      │ │pricing_rule   │ │_billing_   │ │  amendment     │      │
        │    │             │ │               │ │ schedule   │ │                │      │
        │    │ PK: unit_id │ │PK: rule_id    │ │PK: sched_id│ │PK: amendment_id│      │
        │    │ FK: contract│ │FK: contract_id│ │FK: contract│ │FK: contract_id │      │
        │    │ FK: unit_id?│ │FK: unit_id?   │ │FK: invoice?│ │                │      │
        │    └─────────────┘ └───────────────┘ └──────┬─────┘ └────────────────┘      │
        │                                            │ 1:N                            │
        │                                            ▼                                │
        │                                   ┌────────────────┐                        │
        │                                   │erp_contract_   │                        │
        │                                   │billing_line    │                        │
        │                                   │                │                        │
        │                                   │PK: line_id     │                        │
        │                                   │FK: schedule_id │                        │
        │                                   │FK: unit_id?    │                        │
        │                                   │FK: rule_id?    │                        │
        │                                   └────────────────┘                        │
        │                                                                             │
        │    ┌─────────────────────────────┐    ┌─────────────────────────────┐       │
        │    │    erp_contract_renewal     │    │   erp_contract_termination  │       │
        │    │ ━━━━━━━━━━━━━━━━━━━━━━━━━━━ │    │ ━━━━━━━━━━━━━━━━━━━━━━━━━━━ │       │
        │    │ PK: renewal_id              │    │ PK: termination_id          │       │
        │    │ FK: contract_id             │    │ FK: contract_id             │       │
        │    │ FK: amendment_id? (opt)     │    │ FK: final_invoice_id? (opt) │       │
        │    └─────────────────────────────┘    │ FK: penalty_invoice_id?(opt)│       │
        │                                       └─────────────────────────────┘       │
        │                                                                             │
        └─────────────────────────────────────────────────────────────────────────────┘
                                               │
                                               │ Integration via nullable FKs
                                               ▼
        ┌─────────────────────────────────────────────────────────────────────────────┐
        │                           FINANCE MODULE (Existing)                          │
        │  ┌─────────────────────┐  ┌─────────────────────┐  ┌─────────────────────┐  │
        │  │   arap_ar_invoice   │  │arap_ar_invoice_line │  │     gl_journal      │  │
        │  │                     │  │                     │  │                     │  │
        │  │ + contract_id       │  │ + contract_unit_id  │  │ (Journal entries    │  │
        │  │ + contract_schedule │  │ + contract_pricing  │  │  for penalties,     │  │
        │  │ + contract_number   │  │   _rule_id          │  │  deposits, etc.)    │  │
        │  │ + billing_period_*  │  │ + contract_unit_code│  │                     │  │
        │  │ + is_recurring      │  │                     │  │                     │  │
        │  └─────────────────────┘  └─────────────────────┘  └─────────────────────┘  │
        └─────────────────────────────────────────────────────────────────────────────┘

        ============================================================================
        DETAILED RELATIONSHIPS
        ============================================================================

        1. ErpContract (1) ←→ (N) ErpContractUnit
           - One contract can have multiple units
           - Each unit belongs to exactly one contract
           - FK: erp_contract_unit.contract_id → erp_contract.contract_id

        2. ErpContract (1) ←→ (N) ErpContractPricingRule
           - One contract can have multiple pricing rules
           - Rules can apply to specific units or entire contract
           - FK: erp_contract_pricing_rule.contract_id → erp_contract.contract_id
           - FK: erp_contract_pricing_rule.unit_id → erp_contract_unit.unit_id (optional)

        3. ErpContract (1) ←→ (N) ErpContractBillingSchedule
           - One contract generates multiple billing schedules
           - FK: erp_contract_billing_schedule.contract_id → erp_contract.contract_id
           - FK: erp_contract_billing_schedule.invoice_id → arap_ar_invoice.id (optional)

        4. ErpContractBillingSchedule (1) ←→ (N) ErpContractBillingScheduleLine
           - Each schedule has multiple line items
           - FK: erp_contract_billing_line.schedule_id → erp_contract_billing_schedule.schedule_id
           - FK: erp_contract_billing_line.unit_id → erp_contract_unit.unit_id (optional)
           - FK: erp_contract_billing_line.pricing_rule_id → erp_contract_pricing_rule.rule_id (optional)

        5. ErpContract (1) ←→ (N) ErpContractAmendment
           - One contract can have multiple amendments
           - FK: erp_contract_amendment.contract_id → erp_contract.contract_id

        6. ErpContract (1) ←→ (N) ErpContractRenewal
           - One contract can have multiple renewals
           - FK: erp_contract_renewal.contract_id → erp_contract.contract_id
           - FK: erp_contract_renewal.amendment_id → erp_contract_amendment.amendment_id (optional)

        7. ErpContract (1) ←→ (0..1) ErpContractTermination
           - One contract can have at most one active termination
           - FK: erp_contract_termination.contract_id → erp_contract.contract_id

        8. ErpContract (N) ←→ (1) BusinessPartner
           - Multiple contracts can belong to one customer
           - FK: erp_contract.customer_id → business_partner.partner_id

        ============================================================================
        INTEGRATION POINTS
        ============================================================================

        ╔═══════════════════════════════════════════════════════════════════════════╗
        ║  INTEGRATION POINT 1: Customer Master (Business Partner Module)           ║
        ╠═══════════════════════════════════════════════════════════════════════════╣
        ║  Direction: Contracts → Business Partners                                  ║
        ║  Type: Foreign Key Reference                                               ║
        ║  ──────────────────────────────────────────────────────────────────────── ║
        ║  ErpContract.CustomerId → BusinessPartner.PartnerId                        ║
        ║  ──────────────────────────────────────────────────────────────────────── ║
        ║  Usage:                                                                    ║
        ║  - Validate customer exists and is active when creating contract           ║
        ║  - Get customer details for contract display                               ║
        ║  - Get customer default payment terms                                      ║
        ║  - Get customer billing address for invoices                               ║
        ╚═══════════════════════════════════════════════════════════════════════════╝

        ╔═══════════════════════════════════════════════════════════════════════════╗
        ║  INTEGRATION POINT 2: AR Invoice (Finance Module)                         ║
        ╠═══════════════════════════════════════════════════════════════════════════╣
        ║  Direction: Contracts → AR Invoice                                         ║
        ║  Type: Data Extension (Nullable Foreign Keys)                              ║
        ║  ──────────────────────────────────────────────────────────────────────── ║
        ║  ARInvoice Extended Fields:                                                ║
        ║    - contract_id (GUID, nullable)                                          ║
        ║    - contract_schedule_id (GUID, nullable)                                 ║
        ║    - contract_number (varchar(50), nullable)                               ║
        ║    - billing_period_start (datetime, nullable)                             ║
        ║    - billing_period_end (datetime, nullable)                               ║
        ║    - is_recurring (boolean, nullable)                                      ║
        ║  ──────────────────────────────────────────────────────────────────────── ║
        ║  ARInvoiceLine Extended Fields:                                            ║
        ║    - contract_unit_id (GUID, nullable)                                     ║
        ║    - contract_pricing_rule_id (GUID, nullable)                             ║
        ║    - contract_unit_code (varchar(50), nullable)                            ║
        ║  ──────────────────────────────────────────────────────────────────────── ║
        ║  Usage:                                                                    ║
        ║  - Create AR Invoice from billing schedule                                 ║
        ║  - Link invoice back to contract for reporting                             ║
        ║  - Track recurring invoice generation                                      ║
        ║  - Generate contract-specific invoice reports                              ║
        ╚═══════════════════════════════════════════════════════════════════════════╝

        ╔═══════════════════════════════════════════════════════════════════════════╗
        ║  INTEGRATION POINT 3: General Ledger (Finance Module)                     ║
        ╠═══════════════════════════════════════════════════════════════════════════╣
        ║  Direction: Contracts → GL                                                 ║
        ║  Type: Journal Entry Creation                                              ║
        ║  ──────────────────────────────────────────────────────────────────────── ║
        ║  Scenarios requiring GL entries:                                           ║
        ║  ──────────────────────────────────────────────────────────────────────── ║
        ║  1. Early Termination Penalty                                              ║
        ║     DR: Accounts Receivable                                                ║
        ║     CR: Penalty Income                                                     ║
        ║  ──────────────────────────────────────────────────────────────────────── ║
        ║  2. Security Deposit Received                                              ║
        ║     DR: Bank                                                               ║
        ║     CR: Security Deposits Liability                                        ║
        ║  ──────────────────────────────────────────────────────────────────────── ║
        ║  3. Security Deposit Refund                                                ║
        ║     DR: Security Deposits Liability                                        ║
        ║     CR: Bank                                                               ║
        ║  ──────────────────────────────────────────────────────────────────────── ║
        ║  4. Security Deposit Forfeiture                                            ║
        ║     DR: Security Deposits Liability                                        ║
        ║     CR: Other Income                                                       ║
        ║  ──────────────────────────────────────────────────────────────────────── ║
        ║  5. Bad Debt Write-Off                                                     ║
        ║     DR: Bad Debt Expense                                                   ║
        ║     CR: Accounts Receivable                                                ║
        ╚═══════════════════════════════════════════════════════════════════════════╝

        ╔═══════════════════════════════════════════════════════════════════════════╗
        ║  INTEGRATION POINT 4: Tax Module (Finance Module)                         ║
        ╠═══════════════════════════════════════════════════════════════════════════╣
        ║  Direction: Contracts ↔ Tax                                                ║
        ║  Type: Lookup Reference                                                    ║
        ║  ──────────────────────────────────────────────────────────────────────── ║
        ║  ErpContract.TaxGroupId → TaxGroup.Id                                      ║
        ║  ──────────────────────────────────────────────────────────────────────── ║
        ║  Usage:                                                                    ║
        ║  - Get tax rate for billing schedule calculation                           ║
        ║  - Apply VAT/Tax to invoice generation                                     ║
        ║  - Support ZATCA compliance                                                ║
        ╚═══════════════════════════════════════════════════════════════════════════╝

        ╔═══════════════════════════════════════════════════════════════════════════╗
        ║  INTEGRATION POINT 5: Property/Asset Module (Future)                      ║
        ╠═══════════════════════════════════════════════════════════════════════════╣
        ║  Direction: Contracts ↔ Property                                           ║
        ║  Type: Reference (Prepared for future integration)                         ║
        ║  ──────────────────────────────────────────────────────────────────────── ║
        ║  ErpContractUnit can reference:                                            ║
        ║  - PropertyId: Physical property/unit in property module                   ║
        ║  - AssetId: Equipment/asset in asset module                                ║
        ║  ──────────────────────────────────────────────────────────────────────── ║
        ║  Current: Stores unit details directly in ErpContractUnit                  ║
        ║  Future: Can be linked to centralized property/asset master                ║
        ╚═══════════════════════════════════════════════════════════════════════════╝

        ╔═══════════════════════════════════════════════════════════════════════════╗
        ║  INTEGRATION POINT 6: Notification Module (System)                        ║
        ╠═══════════════════════════════════════════════════════════════════════════╣
        ║  Direction: Contracts → Notifications                                      ║
        ║  Type: Event-Based                                                         ║
        ║  ──────────────────────────────────────────────────────────────────────── ║
        ║  Events triggering notifications:                                          ║
        ║  - Contract expiring (90, 60, 30, 14, 7 days)                              ║
        ║  - Billing schedule due                                                    ║
        ║  - Amendment pending approval                                              ║
        ║  - Renewal pending decision                                                ║
        ║  - Termination processed                                                   ║
        ╚═══════════════════════════════════════════════════════════════════════════╝

        ============================================================================
        DATABASE INDEXES
        ============================================================================

        erp_contract:
        - PK: contract_id
        - UNIQUE: contract_number
        - INDEX: customer_id
        - INDEX: status
        - INDEX: end_date (for expiry queries)
        - INDEX: company_id, branch_id

        erp_contract_unit:
        - PK: unit_id
        - INDEX: contract_id
        - UNIQUE: contract_id + unit_code

        erp_contract_pricing_rule:
        - PK: rule_id
        - INDEX: contract_id
        - INDEX: unit_id (where not null)
        - INDEX: is_active

        erp_contract_billing_schedule:
        - PK: schedule_id
        - UNIQUE: schedule_number
        - INDEX: contract_id
        - INDEX: status
        - INDEX: due_date (for due queries)
        - INDEX: invoice_id (where not null)

        erp_contract_billing_line:
        - PK: line_id
        - INDEX: schedule_id

        erp_contract_amendment:
        - PK: amendment_id
        - UNIQUE: amendment_number
        - INDEX: contract_id
        - INDEX: status

        erp_contract_renewal:
        - PK: renewal_id
        - UNIQUE: renewal_number
        - INDEX: contract_id
        - INDEX: status

        erp_contract_termination:
        - PK: termination_id
        - UNIQUE: termination_number
        - INDEX: contract_id
        - INDEX: status

        ============================================================================
        DATA MIGRATION NOTES
        ============================================================================

        When implementing the module:

        1. Create tables in order:
           - erp_contract (no FK dependencies)
           - erp_contract_unit (depends on contract)
           - erp_contract_pricing_rule (depends on contract, unit)
           - erp_contract_billing_schedule (depends on contract)
           - erp_contract_billing_line (depends on schedule, unit, rule)
           - erp_contract_amendment (depends on contract)
           - erp_contract_renewal (depends on contract, amendment)
           - erp_contract_termination (depends on contract)

        2. Add integration columns to existing tables:
           - ALTER arap_ar_invoice ADD contract integration columns
           - ALTER arap_ar_invoice_line ADD contract integration columns

        3. Create indexes after data migration

        4. Set InitDb: true to auto-create tables via SqlSugar

        */
    }
}
