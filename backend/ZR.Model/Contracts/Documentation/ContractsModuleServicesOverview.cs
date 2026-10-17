namespace ZR.Model.Contracts.Documentation
{
    /// <summary>
    /// Contracts Module Services Overview - نظرة عامة على خدمات وحدة العقود
    ///
    /// This file provides a comprehensive overview of all services in the Contracts Module.
    /// Use this as a reference when implementing services in ZR.Service.
    /// </summary>
    public static class ContractsModuleServicesOverview
    {
        /*
        ╔══════════════════════════════════════════════════════════════════════════════╗
        ║                     CONTRACTS MODULE SERVICES OVERVIEW                        ║
        ║                     نظرة عامة على خدمات وحدة العقود                           ║
        ╚══════════════════════════════════════════════════════════════════════════════╝

        ============================================================================
        SERVICE ARCHITECTURE
        ============================================================================

        Location: ZR.Service/Contracts/
        Interfaces: ZR.Service/IService/Contracts/

        Service Hierarchy:
        ├── IContractService                 - Contract master operations
        │   └── ContractService
        ├── IContractUnitService             - Contract unit operations
        │   └── ContractUnitService
        ├── IContractPricingRuleService      - Pricing rule operations
        │   └── ContractPricingRuleService
        ├── IContractBillingScheduleService  - Billing schedule operations
        │   └── ContractBillingScheduleService
        ├── IContractAmendmentService        - Amendment operations
        │   └── ContractAmendmentService
        ├── IContractRenewalService          - Renewal operations
        │   └── ContractRenewalService
        ├── IContractTerminationService      - Termination operations
        │   └── ContractTerminationService
        └── IContractInvoiceService          - Invoice integration
            └── ContractInvoiceService

        ============================================================================
        1. CONTRACT SERVICE - خدمة العقد
        ============================================================================

        Interface: IContractService
        Implementation: ContractService

        Dependencies:
        - IContractRepository
        - IContractUnitService
        - IContractPricingRuleService
        - IContractBillingScheduleService
        - IBusinessPartnerService (from existing module)

        Core Methods:
        ┌────────────────────────────────────────────────────────────────────────────┐
        │ Method                              │ Description                          │
        ├────────────────────────────────────────────────────────────────────────────┤
        │ CreateContractAsync()               │ Create new contract                  │
        │ GetContractByIdAsync()              │ Get contract with all details        │
        │ GetContractsAsync()                 │ Paginated contract list              │
        │ UpdateContractAsync()               │ Update contract details              │
        │ DeleteContractAsync()               │ Soft delete contract                 │
        │ ActivateContractAsync()             │ Draft → Active                       │
        │ SuspendContractAsync()              │ Active → Suspended                   │
        │ ResumeContractAsync()               │ Suspended → Active                   │
        │ GetContractsByCustomerAsync()       │ Get customer's contracts             │
        │ GetExpiringContractsAsync()         │ Get contracts expiring soon          │
        │ GetContractFinancialSummaryAsync()  │ Financial overview                   │
        │ GenerateContractNumberAsync()       │ Generate unique contract number      │
        │ ValidateContractAsync()             │ Validate contract data               │
        └────────────────────────────────────────────────────────────────────────────┘

        Business Rules:
        - Contract number must be unique per company
        - End date must be after start date
        - Customer must exist and be active
        - Cannot delete contracts with invoices
        - Cannot modify terminated contracts

        ============================================================================
        2. CONTRACT UNIT SERVICE - خدمة وحدات العقد
        ============================================================================

        Interface: IContractUnitService
        Implementation: ContractUnitService

        Dependencies:
        - IContractUnitRepository
        - IContractService
        - IPropertyService (if integrating with property module)

        Core Methods:
        ┌────────────────────────────────────────────────────────────────────────────┐
        │ Method                              │ Description                          │
        ├────────────────────────────────────────────────────────────────────────────┤
        │ AddUnitToContractAsync()            │ Add single unit                      │
        │ BulkAddUnitsAsync()                 │ Add multiple units                   │
        │ GetUnitsByContractAsync()           │ Get all units for contract           │
        │ GetUnitByIdAsync()                  │ Get unit details                     │
        │ UpdateUnitAsync()                   │ Update unit details                  │
        │ RemoveUnitAsync()                   │ Remove unit from contract            │
        │ RecordHandoverAsync()               │ Record unit handover to tenant       │
        │ RecordReturnAsync()                 │ Record unit return                   │
        │ UpdateUnitPriceAsync()              │ Update unit pricing                  │
        │ GetActiveUnitsAsync()               │ Get currently active units           │
        │ CalculateUnitsTotalAsync()          │ Sum of all unit values               │
        │ GetUnitsSummaryAsync()              │ Units summary for contract           │
        └────────────────────────────────────────────────────────────────────────────┘

        Business Rules:
        - Unit code must be unique within contract
        - Cannot add units to terminated contracts
        - Handover date required before billing
        - Return date must be after handover date

        ============================================================================
        3. CONTRACT PRICING RULE SERVICE - خدمة قواعد التسعير
        ============================================================================

        Interface: IContractPricingRuleService
        Implementation: ContractPricingRuleService

        Dependencies:
        - IContractPricingRuleRepository
        - IContractService
        - IContractUnitService

        Core Methods:
        ┌────────────────────────────────────────────────────────────────────────────┐
        │ Method                              │ Description                          │
        ├────────────────────────────────────────────────────────────────────────────┤
        │ CreatePricingRuleAsync()            │ Create new pricing rule              │
        │ GetPricingRulesAsync()              │ Get rules for contract               │
        │ GetActivePricingRulesAsync()        │ Get currently active rules           │
        │ UpdatePricingRuleAsync()            │ Update rule                          │
        │ DeletePricingRuleAsync()            │ Delete rule                          │
        │ CalculatePricingAsync()             │ Calculate total with all rules       │
        │ ApplyEscalationAsync()              │ Apply escalation rule                │
        │ GetRulesForPeriodAsync()            │ Get rules valid for a period         │
        │ CalculateUnitPricingAsync()         │ Calculate pricing for single unit    │
        │ ValidatePricingRuleAsync()          │ Validate rule configuration          │
        └────────────────────────────────────────────────────────────────────────────┘

        Pricing Calculation Order:
        1. Base price from contract/unit
        2. Apply fixed adjustments
        3. Apply percentage adjustments
        4. Apply area-based calculations
        5. Apply seasonal adjustments
        6. Apply tiered pricing
        7. Apply discounts
        8. Apply escalations (if due)
        9. Calculate final amount

        ============================================================================
        4. BILLING SCHEDULE SERVICE - خدمة جدول الفوترة
        ============================================================================

        Interface: IContractBillingScheduleService
        Implementation: ContractBillingScheduleService

        Dependencies:
        - IContractBillingScheduleRepository
        - IContractService
        - IContractUnitService
        - IContractPricingRuleService

        Core Methods:
        ┌────────────────────────────────────────────────────────────────────────────┐
        │ Method                              │ Description                          │
        ├────────────────────────────────────────────────────────────────────────────┤
        │ GenerateSchedulesAsync()            │ Generate billing schedules           │
        │ PreviewSchedulesAsync()             │ Preview without saving               │
        │ GetSchedulesByContractAsync()       │ Get schedules for contract           │
        │ GetScheduleByIdAsync()              │ Get schedule details                 │
        │ UpdateScheduleAsync()               │ Update pending schedule              │
        │ DeleteScheduleAsync()               │ Delete pending schedule              │
        │ SkipScheduleAsync()                 │ Skip a billing period                │
        │ CancelScheduleAsync()               │ Cancel pending schedule              │
        │ GetDueSchedulesAsync()              │ Get schedules due for billing        │
        │ GetPendingSchedulesAsync()          │ Get all pending schedules            │
        │ RegenerateSchedulesAsync()          │ Regenerate from date                 │
        │ RecalculateScheduleAsync()          │ Recalculate single schedule          │
        │ MarkAsInvoicedAsync()               │ Mark schedule as invoiced            │
        │ GetBillingSummaryAsync()            │ Billing summary for contract         │
        └────────────────────────────────────────────────────────────────────────────┘

        Schedule Generation Algorithm:
        1. Determine billing frequency (Monthly, Quarterly, etc.)
        2. Calculate period dates based on frequency
        3. For each period:
           a. Get active units for period
           b. Get active pricing rules for period
           c. Calculate base amount from units
           d. Apply pricing rules
           e. Calculate tax
           f. Create schedule record
        4. Return generated schedules

        ============================================================================
        5. CONTRACT AMENDMENT SERVICE - خدمة تعديلات العقد
        ============================================================================

        Interface: IContractAmendmentService
        Implementation: ContractAmendmentService

        Dependencies:
        - IContractAmendmentRepository
        - IContractService
        - IContractUnitService
        - IContractPricingRuleService
        - IContractBillingScheduleService

        Core Methods:
        ┌────────────────────────────────────────────────────────────────────────────┐
        │ Method                              │ Description                          │
        ├────────────────────────────────────────────────────────────────────────────┤
        │ CreatePriceChangeAmendmentAsync()   │ Create price change amendment        │
        │ CreateDurationExtensionAsync()      │ Create duration extension            │
        │ CreateUnitAdditionAsync()           │ Create unit addition amendment       │
        │ CreateUnitRemovalAsync()            │ Create unit removal amendment        │
        │ CreateDiscountAmendmentAsync()      │ Create discount amendment            │
        │ CreateTermsChangeAsync()            │ Create terms change amendment        │
        │ GetAmendmentsByContractAsync()      │ Get amendments for contract          │
        │ GetAmendmentByIdAsync()             │ Get amendment details                │
        │ SubmitForApprovalAsync()            │ Submit amendment for approval        │
        │ ApproveAmendmentAsync()             │ Approve amendment                    │
        │ RejectAmendmentAsync()              │ Reject amendment                     │
        │ ApplyAmendmentAsync()               │ Apply to contract                    │
        │ RevertAmendmentAsync()              │ Revert applied amendment             │
        │ PreviewAmendmentImpactAsync()       │ Preview impact before apply          │
        │ GetAmendmentHistoryAsync()          │ Get amendment history                │
        └────────────────────────────────────────────────────────────────────────────┘

        Amendment Application Logic:
        1. Validate amendment can be applied
        2. Store pre-amendment snapshot
        3. Apply changes based on type:
           - Price Change: Update contract base, recalculate schedules
           - Duration Extension: Extend end date, generate new schedules
           - Unit Addition: Add units, add to pending schedules
           - Unit Removal: Mark units inactive, adjust schedules
           - Discount: Add pricing rule, recalculate schedules
           - Terms Change: Update terms, adjust due dates/frequencies
        4. Update contract master values
        5. Mark amendment as applied
        6. Create audit log

        ============================================================================
        6. CONTRACT RENEWAL SERVICE - خدمة تجديد العقد
        ============================================================================

        Interface: IContractRenewalService
        Implementation: ContractRenewalService

        Dependencies:
        - IContractRenewalRepository
        - IContractService
        - IContractBillingScheduleService
        - IContractAmendmentService

        Core Methods:
        ┌────────────────────────────────────────────────────────────────────────────┐
        │ Method                              │ Description                          │
        ├────────────────────────────────────────────────────────────────────────────┤
        │ CreateRenewalAsync()                │ Create manual renewal                │
        │ CreateAutoRenewalAsync()            │ Create automatic renewal             │
        │ GetRenewalsByContractAsync()        │ Get renewals for contract            │
        │ GetRenewalByIdAsync()               │ Get renewal details                  │
        │ UpdateRenewalAsync()                │ Update renewal                       │
        │ SubmitRenewalOfferAsync()           │ Send renewal offer                   │
        │ ApproveRenewalAsync()               │ Approve renewal                      │
        │ DeclineRenewalAsync()               │ Decline renewal                      │
        │ ExecuteRenewalAsync()               │ Execute approved renewal             │
        │ CancelRenewalAsync()                │ Cancel renewal                       │
        │ PreviewRenewalAsync()               │ Preview renewal impact               │
        │ GetContractsDueForRenewalAsync()    │ Get contracts expiring soon          │
        │ ProcessDueAutoRenewalsAsync()       │ Process automatic renewals (job)     │
        └────────────────────────────────────────────────────────────────────────────┘

        Renewal Execution Logic:
        1. Validate renewal can be executed
        2. Calculate new end date
        3. Calculate new base amount (with escalation if applicable)
        4. Update contract master:
           - EndDate = NewEndDate
           - BaseAmount = NewBaseAmount
           - TotalRenewals++
        5. Apply terms changes if any
        6. Generate new billing schedules
        7. Create amendment record (optional)
        8. Mark renewal as executed
        9. Create audit log

        ============================================================================
        7. CONTRACT TERMINATION SERVICE - خدمة إنهاء العقد
        ============================================================================

        Interface: IContractTerminationService
        Implementation: ContractTerminationService

        Dependencies:
        - IContractTerminationRepository
        - IContractService
        - IContractUnitService
        - IContractBillingScheduleService
        - IContractInvoiceService

        Core Methods:
        ┌────────────────────────────────────────────────────────────────────────────┐
        │ Method                              │ Description                          │
        ├────────────────────────────────────────────────────────────────────────────┤
        │ CreateTerminationAsync()            │ Create termination request           │
        │ GetTerminationByContractAsync()     │ Get contract's termination           │
        │ GetTerminationByIdAsync()           │ Get termination details              │
        │ UpdateTerminationAsync()            │ Update termination                   │
        │ SubmitForApprovalAsync()            │ Submit for approval                  │
        │ ApproveTerminationAsync()           │ Approve termination                  │
        │ RejectTerminationAsync()            │ Reject termination                   │
        │ ProcessTerminationAsync()           │ Execute termination                  │
        │ CancelTerminationAsync()            │ Cancel and reactivate                │
        │ ProcessSecurityDepositAsync()       │ Process deposit settlement           │
        │ RecordHandoverInspectionAsync()     │ Record inspection results            │
        │ CompleteSettlementAsync()           │ Complete financial settlement        │
        │ GenerateFinalInvoiceAsync()         │ Generate prorated final invoice      │
        │ GeneratePenaltyInvoiceAsync()       │ Generate penalty invoice             │
        │ PreviewTerminationAsync()           │ Preview termination impact           │
        │ GetFinancialSummaryAsync()          │ Get settlement summary               │
        └────────────────────────────────────────────────────────────────────────────┘

        Termination Processing Logic:
        1. Validate termination can be processed
        2. Cancel pending billing schedules after effective date
        3. Prorate final billing period if needed
        4. Calculate early termination penalty
        5. Generate final invoice (prorated)
        6. Generate penalty invoice (if applicable)
        7. Update unit return status
        8. Update contract status to Terminated
        9. Create GL journal entries
        10. Create audit log

        ============================================================================
        8. CONTRACT INVOICE SERVICE - خدمة فواتير العقد
        ============================================================================

        Interface: IContractInvoiceService
        Implementation: ContractInvoiceService

        Dependencies:
        - IARInvoiceService (from Finance module)
        - IContractService
        - IContractBillingScheduleService
        - IGLService (for journal entries)

        Core Methods:
        ┌────────────────────────────────────────────────────────────────────────────┐
        │ Method                              │ Description                          │
        ├────────────────────────────────────────────────────────────────────────────┤
        │ CreateInvoiceFromScheduleAsync()    │ Create invoice from schedule         │
        │ BulkCreateInvoicesAsync()           │ Create multiple invoices             │
        │ GetContractInvoicesAsync()          │ Get invoices for contract            │
        │ GetInvoiceSummaryAsync()            │ Invoice summary for contract         │
        │ VoidContractInvoiceAsync()          │ Void a contract invoice              │
        │ CreateCreditNoteAsync()             │ Create credit note                   │
        │ GetRevenueSummaryAsync()            │ Revenue summary for period           │
        │ SyncInvoiceStatusAsync()            │ Sync with AR Invoice status          │
        │ GetInvoiceByScheduleIdAsync()       │ Get invoice for schedule             │
        └────────────────────────────────────────────────────────────────────────────┘

        Invoice Creation Flow:
        1. Get billing schedule details
        2. Get schedule line items
        3. Create AR Invoice header with contract fields:
           - ContractId
           - ContractScheduleId
           - ContractNumber
           - BillingPeriodStart
           - BillingPeriodEnd
           - IsRecurring = true
        4. Create AR Invoice lines from schedule lines:
           - ContractUnitId
           - ContractPricingRuleId
           - ContractUnitCode
        5. Apply tax calculation
        6. Save invoice
        7. Update schedule status to Invoiced
        8. Return invoice ID

        ============================================================================
        BACKGROUND JOBS
        ============================================================================

        Location: ZR.Tasks/

        ┌────────────────────────────────────────────────────────────────────────────┐
        │ Job Name                            │ Schedule      │ Description          │
        ├────────────────────────────────────────────────────────────────────────────┤
        │ ContractAutoRenewalJob              │ Daily 1:00 AM │ Process auto-renewals│
        │ ContractExpiryNotificationJob       │ Daily 6:00 AM │ Send expiry notices  │
        │ BillingScheduleDueNotificationJob   │ Daily 7:00 AM │ Notify due schedules │
        │ ContractEscalationJob               │ Monthly 1st   │ Apply escalations    │
        │ ContractStatusUpdateJob             │ Daily 0:00 AM │ Update expired status│
        └────────────────────────────────────────────────────────────────────────────┘

        ============================================================================
        SERVICE REGISTRATION
        ============================================================================

        Add to Program.cs or ServiceExtensions:

        // Contract Services
        builder.Services.AddScoped<IContractService, ContractService>();
        builder.Services.AddScoped<IContractUnitService, ContractUnitService>();
        builder.Services.AddScoped<IContractPricingRuleService, ContractPricingRuleService>();
        builder.Services.AddScoped<IContractBillingScheduleService, ContractBillingScheduleService>();
        builder.Services.AddScoped<IContractAmendmentService, ContractAmendmentService>();
        builder.Services.AddScoped<IContractRenewalService, ContractRenewalService>();
        builder.Services.AddScoped<IContractTerminationService, ContractTerminationService>();
        builder.Services.AddScoped<IContractInvoiceService, ContractInvoiceService>();

        // OR use auto-injection by adding namespace to InjectClass in appsettings.json:
        "InjectClass": [
            ...existing namespaces...,
            "ZR.Service.Contracts"
        ]

        ============================================================================
        REPOSITORY REGISTRATION
        ============================================================================

        // Contract Repositories
        builder.Services.AddScoped<IContractRepository, ContractRepository>();
        builder.Services.AddScoped<IContractUnitRepository, ContractUnitRepository>();
        builder.Services.AddScoped<IContractPricingRuleRepository, ContractPricingRuleRepository>();
        builder.Services.AddScoped<IContractBillingScheduleRepository, ContractBillingScheduleRepository>();
        builder.Services.AddScoped<IContractAmendmentRepository, ContractAmendmentRepository>();
        builder.Services.AddScoped<IContractRenewalRepository, ContractRenewalRepository>();
        builder.Services.AddScoped<IContractTerminationRepository, ContractTerminationRepository>();

        */
    }
}
