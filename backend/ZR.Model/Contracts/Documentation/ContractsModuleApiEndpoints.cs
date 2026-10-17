namespace ZR.Model.Contracts.Documentation
{
    /// <summary>
    /// Contracts Module API Endpoints Documentation
    /// توثيق نقاط نهاية واجهة برمجة التطبيقات لوحدة العقود
    ///
    /// This file documents all API endpoints for the Contracts Module.
    /// Use this as a reference when implementing controllers in ZR.Admin.WebApi.
    /// </summary>
    public static class ContractsModuleApiEndpoints
    {
        /*
        ╔══════════════════════════════════════════════════════════════════════════════╗
        ║                     CONTRACTS MODULE API ENDPOINTS                            ║
        ║                     نقاط نهاية وحدة العقود                                    ║
        ╚══════════════════════════════════════════════════════════════════════════════╝

        Base URL: /api/erp/contracts
        Authentication: JWT Bearer Token (Required for all endpoints)
        Authorization: Role-based with ActionPermissionFilter

        ============================================================================
        1. CONTRACT MASTER ENDPOINTS - نقاط نهاية العقد الرئيسي
        ============================================================================

        Controller: ContractController
        Route Prefix: /api/erp/contracts

        ┌─────────────────────────────────────────────────────────────────────────────┐
        │ CRUD Operations                                                              │
        ├─────────────────────────────────────────────────────────────────────────────┤
        │ GET    /api/erp/contracts                    │ Get paginated contracts list │
        │        Query: ContractQueryDto               │ Returns: PagedResult<List>   │
        │                                              │                              │
        │ GET    /api/erp/contracts/{id}               │ Get contract by ID           │
        │        Path: Guid id                         │ Returns: ContractDetailDto   │
        │                                              │                              │
        │ GET    /api/erp/contracts/number/{number}    │ Get contract by number       │
        │        Path: string number                   │ Returns: ContractDetailDto   │
        │                                              │                              │
        │ POST   /api/erp/contracts                    │ Create new contract          │
        │        Body: CreateContractDto               │ Returns: ErpContract         │
        │                                              │                              │
        │ PUT    /api/erp/contracts/{id}               │ Update contract              │
        │        Path: Guid id                         │ Returns: Success/Error       │
        │        Body: UpdateContractDto               │                              │
        │                                              │                              │
        │ DELETE /api/erp/contracts/{id}               │ Delete contract (soft)       │
        │        Path: Guid id                         │ Returns: Success/Error       │
        └─────────────────────────────────────────────────────────────────────────────┘

        ┌─────────────────────────────────────────────────────────────────────────────┐
        │ Status Management                                                            │
        ├─────────────────────────────────────────────────────────────────────────────┤
        │ POST   /api/erp/contracts/{id}/activate      │ Activate draft contract      │
        │        Body: ActivateContractDto             │ Returns: Success/Error       │
        │                                              │                              │
        │ POST   /api/erp/contracts/{id}/suspend       │ Suspend active contract      │
        │        Body: SuspendContractDto              │ Returns: Success/Error       │
        │                                              │                              │
        │ POST   /api/erp/contracts/{id}/resume        │ Resume suspended contract    │
        │        Body: (none)                          │ Returns: Success/Error       │
        │                                              │                              │
        │ POST   /api/erp/contracts/{id}/terminate     │ Initiate termination         │
        │        Body: TerminateContractDto            │ Returns: TerminationId       │
        └─────────────────────────────────────────────────────────────────────────────┘

        ┌─────────────────────────────────────────────────────────────────────────────┐
        │ Queries & Reports                                                            │
        ├─────────────────────────────────────────────────────────────────────────────┤
        │ GET    /api/erp/contracts/select             │ Get contracts for dropdown   │
        │        Query: ContractSelectQueryDto         │ Returns: List<SelectDto>     │
        │                                              │                              │
        │ GET    /api/erp/contracts/customer/{id}      │ Get contracts by customer    │
        │        Path: Guid customerId                 │ Returns: List<ListDto>       │
        │                                              │                              │
        │ GET    /api/erp/contracts/expiring           │ Get expiring contracts       │
        │        Query: int days                       │ Returns: List<ListDto>       │
        │                                              │                              │
        │ GET    /api/erp/contracts/{id}/summary       │ Get contract financial sum   │
        │        Path: Guid id                         │ Returns: FinancialSummary    │
        │                                              │                              │
        │ GET    /api/erp/contracts/export             │ Export contracts to Excel    │
        │        Query: ContractQueryDto               │ Returns: File (xlsx)         │
        └─────────────────────────────────────────────────────────────────────────────┘

        ============================================================================
        2. CONTRACT UNITS ENDPOINTS - نقاط نهاية وحدات العقد
        ============================================================================

        Controller: ContractUnitController
        Route Prefix: /api/erp/contracts/{contractId}/units

        ┌─────────────────────────────────────────────────────────────────────────────┐
        │ CRUD Operations                                                              │
        ├─────────────────────────────────────────────────────────────────────────────┤
        │ GET    /api/erp/contracts/{contractId}/units │ Get units for contract       │
        │        Query: ContractUnitQueryDto           │ Returns: List<UnitListDto>   │
        │                                              │                              │
        │ GET    /api/erp/contracts/units/{id}         │ Get unit by ID               │
        │        Path: Guid id                         │ Returns: UnitDetailDto       │
        │                                              │                              │
        │ POST   /api/erp/contracts/{contractId}/units │ Add unit to contract         │
        │        Body: CreateContractUnitDto           │ Returns: ErpContractUnit     │
        │                                              │                              │
        │ POST   /api/erp/contracts/{contractId}/units/bulk                           │
        │        Body: BulkAddContractUnitsDto         │ Add multiple units           │
        │                                              │ Returns: List<UnitId>        │
        │                                              │                              │
        │ PUT    /api/erp/contracts/units/{id}         │ Update unit                  │
        │        Body: UpdateContractUnitDto           │ Returns: Success/Error       │
        │                                              │                              │
        │ DELETE /api/erp/contracts/units/{id}         │ Remove unit from contract    │
        │        Path: Guid id                         │ Returns: Success/Error       │
        └─────────────────────────────────────────────────────────────────────────────┘

        ┌─────────────────────────────────────────────────────────────────────────────┐
        │ Unit Operations                                                              │
        ├─────────────────────────────────────────────────────────────────────────────┤
        │ POST   /api/erp/contracts/units/{id}/handover│ Record unit handover         │
        │        Body: UnitHandoverDto                 │ Returns: Success/Error       │
        │                                              │                              │
        │ POST   /api/erp/contracts/units/{id}/return  │ Record unit return           │
        │        Body: UnitReturnDto                   │ Returns: Success/Error       │
        │                                              │                              │
        │ PUT    /api/erp/contracts/units/{id}/price   │ Update unit price            │
        │        Body: UpdateUnitPriceDto              │ Returns: Success/Error       │
        │                                              │                              │
        │ GET    /api/erp/contracts/{contractId}/units/summary                        │
        │                                              │ Get units summary            │
        │                                              │ Returns: UnitsSummaryDto     │
        └─────────────────────────────────────────────────────────────────────────────┘

        ============================================================================
        3. PRICING RULES ENDPOINTS - نقاط نهاية قواعد التسعير
        ============================================================================

        Controller: ContractPricingRuleController
        Route Prefix: /api/erp/contracts/{contractId}/pricing-rules

        ┌─────────────────────────────────────────────────────────────────────────────┐
        │ CRUD Operations                                                              │
        ├─────────────────────────────────────────────────────────────────────────────┤
        │ GET    /api/erp/contracts/{contractId}/pricing-rules                        │
        │        Query: PricingRuleQueryDto            │ Get pricing rules            │
        │                                              │ Returns: List<RuleListDto>   │
        │                                              │                              │
        │ GET    /api/erp/contracts/pricing-rules/{id} │ Get rule by ID               │
        │        Path: Guid id                         │ Returns: RuleDetailDto       │
        │                                              │                              │
        │ POST   /api/erp/contracts/{contractId}/pricing-rules                        │
        │        Body: CreatePricingRuleDto            │ Create pricing rule          │
        │                                              │ Returns: PricingRule         │
        │                                              │                              │
        │ PUT    /api/erp/contracts/pricing-rules/{id} │ Update pricing rule          │
        │        Body: UpdatePricingRuleDto            │ Returns: Success/Error       │
        │                                              │                              │
        │ DELETE /api/erp/contracts/pricing-rules/{id} │ Delete pricing rule          │
        │        Path: Guid id                         │ Returns: Success/Error       │
        └─────────────────────────────────────────────────────────────────────────────┘

        ┌─────────────────────────────────────────────────────────────────────────────┐
        │ Pricing Calculations                                                         │
        ├─────────────────────────────────────────────────────────────────────────────┤
        │ POST   /api/erp/contracts/{contractId}/pricing-rules/calculate              │
        │        Body: PricingCalculationRequestDto    │ Calculate pricing            │
        │                                              │ Returns: CalculationResult   │
        │                                              │                              │
        │ POST   /api/erp/contracts/pricing-rules/{id}/apply-escalation               │
        │        Body: (none)                          │ Apply escalation             │
        │                                              │ Returns: Success/Error       │
        │                                              │                              │
        │ GET    /api/erp/contracts/{contractId}/pricing-rules/active                 │
        │                                              │ Get active rules only        │
        │                                              │ Returns: List<RuleListDto>   │
        └─────────────────────────────────────────────────────────────────────────────┘

        ============================================================================
        4. BILLING SCHEDULE ENDPOINTS - نقاط نهاية جدول الفوترة
        ============================================================================

        Controller: ContractBillingScheduleController
        Route Prefix: /api/erp/contracts/{contractId}/billing-schedules

        ┌─────────────────────────────────────────────────────────────────────────────┐
        │ CRUD Operations                                                              │
        ├─────────────────────────────────────────────────────────────────────────────┤
        │ GET    /api/erp/contracts/{contractId}/billing-schedules                    │
        │        Query: BillingScheduleQueryDto        │ Get schedules                │
        │                                              │ Returns: List<ScheduleDto>   │
        │                                              │                              │
        │ GET    /api/erp/contracts/billing-schedules/{id}                            │
        │        Path: Guid id                         │ Get schedule by ID           │
        │                                              │ Returns: ScheduleDetailDto   │
        │                                              │                              │
        │ PUT    /api/erp/contracts/billing-schedules/{id}                            │
        │        Body: UpdateBillingScheduleDto        │ Update schedule              │
        │                                              │ Returns: Success/Error       │
        │                                              │                              │
        │ DELETE /api/erp/contracts/billing-schedules/{id}                            │
        │        Path: Guid id                         │ Delete pending schedule      │
        │                                              │ Returns: Success/Error       │
        └─────────────────────────────────────────────────────────────────────────────┘

        ┌─────────────────────────────────────────────────────────────────────────────┐
        │ Schedule Generation                                                          │
        ├─────────────────────────────────────────────────────────────────────────────┤
        │ POST   /api/erp/contracts/{contractId}/billing-schedules/generate           │
        │        Body: GenerateBillingScheduleRequest  │ Generate schedules           │
        │                                              │ Returns: GenerationResult    │
        │                                              │                              │
        │ POST   /api/erp/contracts/{contractId}/billing-schedules/preview            │
        │        Body: GenerateBillingScheduleRequest  │ Preview generation           │
        │                                              │ Returns: List<PreviewDto>    │
        │                                              │                              │
        │ POST   /api/erp/contracts/{contractId}/billing-schedules/regenerate         │
        │        Body: RegenerateBillingScheduleDto    │ Regenerate pending           │
        │                                              │ Returns: GenerationResult    │
        └─────────────────────────────────────────────────────────────────────────────┘

        ┌─────────────────────────────────────────────────────────────────────────────┐
        │ Schedule Actions                                                             │
        ├─────────────────────────────────────────────────────────────────────────────┤
        │ POST   /api/erp/contracts/billing-schedules/{id}/skip                       │
        │        Body: SkipBillingScheduleDto          │ Skip schedule                │
        │                                              │ Returns: Success/Error       │
        │                                              │                              │
        │ POST   /api/erp/contracts/billing-schedules/{id}/cancel                     │
        │        Body: CancelBillingScheduleDto        │ Cancel schedule              │
        │                                              │ Returns: Success/Error       │
        │                                              │                              │
        │ POST   /api/erp/contracts/billing-schedules/{id}/create-invoice             │
        │        Body: (none)                          │ Create invoice               │
        │                                              │ Returns: InvoiceId           │
        │                                              │                              │
        │ POST   /api/erp/contracts/billing-schedules/bulk-invoice                    │
        │        Body: BulkCreateInvoicesDto           │ Create multiple invoices     │
        │                                              │ Returns: List<InvoiceId>     │
        └─────────────────────────────────────────────────────────────────────────────┘

        ┌─────────────────────────────────────────────────────────────────────────────┐
        │ Schedule Queries                                                             │
        ├─────────────────────────────────────────────────────────────────────────────┤
        │ GET    /api/erp/contracts/billing-schedules/due                             │
        │        Query: DateTime date, int days        │ Get schedules due            │
        │                                              │ Returns: List<ScheduleDto>   │
        │                                              │                              │
        │ GET    /api/erp/contracts/billing-schedules/pending                         │
        │        Query: ContractId, DateRange          │ Get pending schedules        │
        │                                              │ Returns: List<ScheduleDto>   │
        │                                              │                              │
        │ GET    /api/erp/contracts/{contractId}/billing-schedules/summary            │
        │                                              │ Get billing summary          │
        │                                              │ Returns: BillingSummaryDto   │
        └─────────────────────────────────────────────────────────────────────────────┘

        ============================================================================
        5. CONTRACT AMENDMENT ENDPOINTS - نقاط نهاية تعديلات العقد
        ============================================================================

        Controller: ContractAmendmentController
        Route Prefix: /api/erp/contracts/{contractId}/amendments

        ┌─────────────────────────────────────────────────────────────────────────────┐
        │ CRUD Operations                                                              │
        ├─────────────────────────────────────────────────────────────────────────────┤
        │ GET    /api/erp/contracts/{contractId}/amendments                           │
        │        Query: AmendmentQueryDto              │ Get amendments               │
        │                                              │ Returns: List<AmendmentDto>  │
        │                                              │                              │
        │ GET    /api/erp/contracts/amendments/{id}    │ Get amendment by ID          │
        │        Path: Guid id                         │ Returns: AmendmentDetailDto  │
        │                                              │                              │
        │ PUT    /api/erp/contracts/amendments/{id}    │ Update amendment             │
        │        Body: UpdateAmendmentDto              │ Returns: Success/Error       │
        │                                              │                              │
        │ DELETE /api/erp/contracts/amendments/{id}    │ Delete draft amendment       │
        │        Path: Guid id                         │ Returns: Success/Error       │
        └─────────────────────────────────────────────────────────────────────────────┘

        ┌─────────────────────────────────────────────────────────────────────────────┐
        │ Type-Specific Creation                                                       │
        ├─────────────────────────────────────────────────────────────────────────────┤
        │ POST   /api/erp/contracts/{contractId}/amendments/price-change              │
        │        Body: CreatePriceChangeAmendmentDto   │ Create price change          │
        │                                              │ Returns: Amendment           │
        │                                              │                              │
        │ POST   /api/erp/contracts/{contractId}/amendments/duration-extension        │
        │        Body: CreateDurationExtensionDto      │ Create extension             │
        │                                              │ Returns: Amendment           │
        │                                              │                              │
        │ POST   /api/erp/contracts/{contractId}/amendments/unit-addition             │
        │        Body: CreateUnitAdditionAmendmentDto  │ Add units amendment          │
        │                                              │ Returns: Amendment           │
        │                                              │                              │
        │ POST   /api/erp/contracts/{contractId}/amendments/unit-removal              │
        │        Body: CreateUnitRemovalAmendmentDto   │ Remove units amendment       │
        │                                              │ Returns: Amendment           │
        │                                              │                              │
        │ POST   /api/erp/contracts/{contractId}/amendments/discount                  │
        │        Body: CreateDiscountAmendmentDto      │ Discount amendment           │
        │                                              │ Returns: Amendment           │
        │                                              │                              │
        │ POST   /api/erp/contracts/{contractId}/amendments/terms-change              │
        │        Body: CreateTermsChangeAmendmentDto   │ Terms change amendment       │
        │                                              │ Returns: Amendment           │
        │                                              │                              │
        │ POST   /api/erp/contracts/{contractId}/amendments/scope-change              │
        │        Body: CreateScopeChangeAmendmentDto   │ Scope change amendment       │
        │                                              │ Returns: Amendment           │
        └─────────────────────────────────────────────────────────────────────────────┘

        ┌─────────────────────────────────────────────────────────────────────────────┐
        │ Amendment Workflow                                                           │
        ├─────────────────────────────────────────────────────────────────────────────┤
        │ POST   /api/erp/contracts/amendments/{id}/submit                            │
        │        Body: (none)                          │ Submit for approval          │
        │                                              │ Returns: Success/Error       │
        │                                              │                              │
        │ POST   /api/erp/contracts/amendments/{id}/approve                           │
        │        Body: ApproveAmendmentDto             │ Approve amendment            │
        │                                              │ Returns: Success/Error       │
        │                                              │                              │
        │ POST   /api/erp/contracts/amendments/{id}/reject                            │
        │        Body: RejectAmendmentDto              │ Reject amendment             │
        │                                              │ Returns: Success/Error       │
        │                                              │                              │
        │ POST   /api/erp/contracts/amendments/{id}/apply                             │
        │        Body: (none)                          │ Apply to contract            │
        │                                              │ Returns: ApplicationResult   │
        │                                              │                              │
        │ POST   /api/erp/contracts/amendments/{id}/revert                            │
        │        Body: RevertAmendmentDto              │ Revert amendment             │
        │                                              │ Returns: Success/Error       │
        │                                              │                              │
        │ GET    /api/erp/contracts/amendments/{id}/preview                           │
        │                                              │ Preview impact               │
        │                                              │ Returns: ImpactPreviewDto    │
        └─────────────────────────────────────────────────────────────────────────────┘

        ============================================================================
        6. CONTRACT RENEWAL ENDPOINTS - نقاط نهاية تجديد العقد
        ============================================================================

        Controller: ContractRenewalController
        Route Prefix: /api/erp/contracts/{contractId}/renewals

        ┌─────────────────────────────────────────────────────────────────────────────┐
        │ CRUD Operations                                                              │
        ├─────────────────────────────────────────────────────────────────────────────┤
        │ GET    /api/erp/contracts/{contractId}/renewals                             │
        │        Query: RenewalQueryDto                │ Get renewals for contract    │
        │                                              │ Returns: List<RenewalDto>    │
        │                                              │                              │
        │ GET    /api/erp/contracts/renewals/{id}      │ Get renewal by ID            │
        │        Path: Guid id                         │ Returns: RenewalDetailDto    │
        │                                              │                              │
        │ POST   /api/erp/contracts/{contractId}/renewals                             │
        │        Body: CreateContractRenewalDto        │ Create renewal               │
        │                                              │ Returns: Renewal             │
        │                                              │                              │
        │ PUT    /api/erp/contracts/renewals/{id}      │ Update renewal               │
        │        Body: UpdateContractRenewalDto        │ Returns: Success/Error       │
        │                                              │                              │
        │ DELETE /api/erp/contracts/renewals/{id}      │ Delete renewal               │
        │        Path: Guid id                         │ Returns: Success/Error       │
        └─────────────────────────────────────────────────────────────────────────────┘

        ┌─────────────────────────────────────────────────────────────────────────────┐
        │ Renewal Workflow                                                             │
        ├─────────────────────────────────────────────────────────────────────────────┤
        │ POST   /api/erp/contracts/renewals/{id}/offer                               │
        │        Body: (none)                          │ Send renewal offer           │
        │                                              │ Returns: Success/Error       │
        │                                              │                              │
        │ POST   /api/erp/contracts/renewals/{id}/approve                             │
        │        Body: ApproveRenewalDto               │ Approve renewal              │
        │                                              │ Returns: Success/Error       │
        │                                              │                              │
        │ POST   /api/erp/contracts/renewals/{id}/decline                             │
        │        Body: DeclineRenewalDto               │ Decline renewal              │
        │                                              │ Returns: Success/Error       │
        │                                              │                              │
        │ POST   /api/erp/contracts/renewals/{id}/execute                             │
        │        Body: ExecuteRenewalDto               │ Execute renewal              │
        │                                              │ Returns: ExecutionResult     │
        │                                              │                              │
        │ POST   /api/erp/contracts/renewals/{id}/cancel                              │
        │        Body: CancelRenewalDto                │ Cancel renewal               │
        │                                              │ Returns: Success/Error       │
        │                                              │                              │
        │ GET    /api/erp/contracts/renewals/{id}/preview                             │
        │                                              │ Preview renewal              │
        │                                              │ Returns: RenewalPreviewDto   │
        └─────────────────────────────────────────────────────────────────────────────┘

        ┌─────────────────────────────────────────────────────────────────────────────┐
        │ Renewal Queries                                                              │
        ├─────────────────────────────────────────────────────────────────────────────┤
        │ GET    /api/erp/contracts/renewals/due       │ Get contracts due for renew  │
        │        Query: ContractsDueForRenewalQueryDto │ Returns: List<DueDto>        │
        │                                              │                              │
        │ GET    /api/erp/contracts/renewals/pending   │ Get all pending renewals     │
        │        Query: RenewalQueryDto                │ Returns: List<RenewalDto>    │
        └─────────────────────────────────────────────────────────────────────────────┘

        ============================================================================
        7. CONTRACT TERMINATION ENDPOINTS - نقاط نهاية إنهاء العقد
        ============================================================================

        Controller: ContractTerminationController
        Route Prefix: /api/erp/contracts/{contractId}/terminations

        ┌─────────────────────────────────────────────────────────────────────────────┐
        │ CRUD Operations                                                              │
        ├─────────────────────────────────────────────────────────────────────────────┤
        │ GET    /api/erp/contracts/terminations       │ Get all terminations         │
        │        Query: TerminationQueryDto            │ Returns: List<TermDto>       │
        │                                              │                              │
        │ GET    /api/erp/contracts/terminations/{id}  │ Get termination by ID        │
        │        Path: Guid id                         │ Returns: TermDetailDto       │
        │                                              │                              │
        │ GET    /api/erp/contracts/{contractId}/termination                          │
        │                                              │ Get contract's termination   │
        │                                              │ Returns: TermDetailDto       │
        │                                              │                              │
        │ POST   /api/erp/contracts/{contractId}/terminations                         │
        │        Body: CreateTerminationDto            │ Create termination           │
        │                                              │ Returns: Termination         │
        │                                              │                              │
        │ PUT    /api/erp/contracts/terminations/{id}  │ Update termination           │
        │        Body: UpdateTerminationDto            │ Returns: Success/Error       │
        │                                              │                              │
        │ DELETE /api/erp/contracts/terminations/{id}  │ Delete draft termination     │
        │        Path: Guid id                         │ Returns: Success/Error       │
        └─────────────────────────────────────────────────────────────────────────────┘

        ┌─────────────────────────────────────────────────────────────────────────────┐
        │ Termination Workflow                                                         │
        ├─────────────────────────────────────────────────────────────────────────────┤
        │ POST   /api/erp/contracts/terminations/{id}/submit                          │
        │        Body: (none)                          │ Submit for approval          │
        │                                              │ Returns: Success/Error       │
        │                                              │                              │
        │ POST   /api/erp/contracts/terminations/{id}/approve                         │
        │        Body: ApproveTerminationDto           │ Approve termination          │
        │                                              │ Returns: Success/Error       │
        │                                              │                              │
        │ POST   /api/erp/contracts/terminations/{id}/reject                          │
        │        Body: string reason                   │ Reject termination           │
        │                                              │ Returns: Success/Error       │
        │                                              │                              │
        │ POST   /api/erp/contracts/terminations/{id}/process                         │
        │        Body: (none)                          │ Process termination          │
        │                                              │ Returns: ExecutionResult     │
        │                                              │                              │
        │ POST   /api/erp/contracts/terminations/{id}/cancel                          │
        │        Body: CancelTerminationDto            │ Cancel termination           │
        │                                              │ Returns: Success/Error       │
        │                                              │                              │
        │ GET    /api/erp/contracts/terminations/{id}/preview                         │
        │                                              │ Preview termination          │
        │                                              │ Returns: TermPreviewDto      │
        └─────────────────────────────────────────────────────────────────────────────┘

        ┌─────────────────────────────────────────────────────────────────────────────┐
        │ Settlement Operations                                                        │
        ├─────────────────────────────────────────────────────────────────────────────┤
        │ POST   /api/erp/contracts/terminations/{id}/process-deposit                 │
        │        Body: ProcessSecurityDepositDto       │ Process security deposit     │
        │                                              │ Returns: Success/Error       │
        │                                              │                              │
        │ POST   /api/erp/contracts/terminations/{id}/record-inspection               │
        │        Body: RecordHandoverInspectionDto     │ Record handover inspection   │
        │                                              │ Returns: Success/Error       │
        │                                              │                              │
        │ POST   /api/erp/contracts/terminations/{id}/complete-settlement             │
        │        Body: CompleteSettlementDto           │ Complete settlement          │
        │                                              │ Returns: Success/Error       │
        │                                              │                              │
        │ GET    /api/erp/contracts/terminations/{id}/financial-summary               │
        │                                              │ Get financial summary        │
        │                                              │ Returns: FinancialSummary    │
        │                                              │                              │
        │ POST   /api/erp/contracts/terminations/{id}/generate-final-invoice          │
        │        Body: (none)                          │ Generate final invoice       │
        │                                              │ Returns: InvoiceId           │
        │                                              │                              │
        │ POST   /api/erp/contracts/terminations/{id}/generate-penalty-invoice        │
        │        Body: (none)                          │ Generate penalty invoice     │
        │                                              │ Returns: InvoiceId           │
        └─────────────────────────────────────────────────────────────────────────────┘

        ============================================================================
        8. CONTRACT INVOICE ENDPOINTS - نقاط نهاية فواتير العقد
        ============================================================================

        Controller: ContractInvoiceController
        Route Prefix: /api/erp/contracts/invoices

        ┌─────────────────────────────────────────────────────────────────────────────┐
        │ Invoice Generation                                                           │
        ├─────────────────────────────────────────────────────────────────────────────┤
        │ POST   /api/erp/contracts/invoices/from-schedule                            │
        │        Body: CreateInvoiceFromScheduleDto    │ Create from schedule         │
        │                                              │ Returns: InvoiceResultDto    │
        │                                              │                              │
        │ POST   /api/erp/contracts/invoices/bulk      │ Create multiple invoices     │
        │        Body: BulkCreateInvoicesDto           │ Returns: BulkResultDto       │
        │                                              │                              │
        │ POST   /api/erp/contracts/invoices/{id}/void │ Void contract invoice        │
        │        Body: VoidContractInvoiceDto          │ Returns: Success/Error       │
        │                                              │                              │
        │ POST   /api/erp/contracts/invoices/{id}/credit-note                         │
        │        Body: CreateCreditNoteDto             │ Create credit note           │
        │                                              │ Returns: CreditNoteId        │
        └─────────────────────────────────────────────────────────────────────────────┘

        ┌─────────────────────────────────────────────────────────────────────────────┐
        │ Invoice Queries                                                              │
        ├─────────────────────────────────────────────────────────────────────────────┤
        │ GET    /api/erp/contracts/{contractId}/invoices                             │
        │        Query: ContractInvoiceQueryDto        │ Get contract invoices        │
        │                                              │ Returns: List<InvoiceDto>    │
        │                                              │                              │
        │ GET    /api/erp/contracts/{contractId}/invoices/summary                     │
        │                                              │ Get invoice summary          │
        │                                              │ Returns: InvoiceSummaryDto   │
        │                                              │                              │
        │ GET    /api/erp/contracts/{contractId}/revenue-summary                      │
        │        Query: DateTime from, DateTime to     │ Get revenue summary          │
        │                                              │ Returns: RevenueSummaryDto   │
        └─────────────────────────────────────────────────────────────────────────────┘

        ============================================================================
        COMMON RESPONSE FORMATS
        ============================================================================

        Success Response:
        {
            "code": 200,
            "msg": "Success",
            "data": { ... }
        }

        Error Response:
        {
            "code": 400/401/403/404/500,
            "msg": "Error message",
            "data": null
        }

        Paginated Response:
        {
            "code": 200,
            "msg": "Success",
            "data": {
                "result": [ ... ],
                "totalNum": 100,
                "totalPage": 10,
                "pageNum": 1,
                "pageSize": 10
            }
        }

        ============================================================================
        PERMISSION CODES
        ============================================================================

        Contract Module Permissions:
        - erp:contract:list          - View contracts list
        - erp:contract:query         - Query contract details
        - erp:contract:add           - Create contracts
        - erp:contract:edit          - Edit contracts
        - erp:contract:delete        - Delete contracts
        - erp:contract:activate      - Activate contracts
        - erp:contract:suspend       - Suspend contracts
        - erp:contract:terminate     - Terminate contracts
        - erp:contract:export        - Export contracts

        Contract Unit Permissions:
        - erp:contract:unit:list     - View units
        - erp:contract:unit:add      - Add units
        - erp:contract:unit:edit     - Edit units
        - erp:contract:unit:delete   - Remove units

        Billing Schedule Permissions:
        - erp:contract:billing:list  - View billing schedules
        - erp:contract:billing:generate - Generate schedules
        - erp:contract:billing:edit  - Edit schedules
        - erp:contract:billing:invoice - Create invoices

        Amendment Permissions:
        - erp:contract:amendment:list - View amendments
        - erp:contract:amendment:add  - Create amendments
        - erp:contract:amendment:approve - Approve amendments
        - erp:contract:amendment:apply - Apply amendments

        Renewal Permissions:
        - erp:contract:renewal:list  - View renewals
        - erp:contract:renewal:add   - Create renewals
        - erp:contract:renewal:approve - Approve renewals
        - erp:contract:renewal:execute - Execute renewals

        Termination Permissions:
        - erp:contract:termination:list - View terminations
        - erp:contract:termination:add  - Create terminations
        - erp:contract:termination:approve - Approve terminations
        - erp:contract:termination:process - Process terminations

        */
    }
}
