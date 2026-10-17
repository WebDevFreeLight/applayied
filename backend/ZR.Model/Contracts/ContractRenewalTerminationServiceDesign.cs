using System;
using System.Collections.Generic;

namespace ZR.Model.Contracts
{
    /// <summary>
    /// Contract Renewal and Termination Service Design - تصميم خدمة تجديد وإنهاء العقود
    ///
    /// This file documents the renewal and termination service interfaces,
    /// billing cancellation rules, and journal impact.
    /// </summary>
    public static class ContractRenewalTerminationServiceDesign
    {
        /*
        ============================================================================
        RENEWAL SERVICE INTERFACE
        ============================================================================

        public interface IContractRenewalService
        {
            // CRUD Operations
            Task<ErpContractRenewal> CreateRenewalAsync(CreateContractRenewalDto dto);
            Task<ErpContractRenewal> GetRenewalByIdAsync(Guid renewalId);
            Task<PagedResult<ErpContractRenewalListDto>> GetRenewalsAsync(ContractRenewalQueryDto query);
            Task<List<ErpContractRenewalListDto>> GetRenewalsByContractAsync(Guid contractId);
            Task UpdateRenewalAsync(Guid renewalId, UpdateContractRenewalDto dto);
            Task DeleteRenewalAsync(Guid renewalId);

            // Auto-Renewal
            Task<ErpContractRenewal> CreateAutoRenewalAsync(CreateAutoRenewalDto dto);
            Task<List<ErpContractRenewal>> ProcessDueAutoRenewalsAsync(DateTime asOfDate);

            // Renewal Workflow
            Task SubmitRenewalOfferAsync(Guid renewalId);
            Task RecordCustomerResponseAsync(Guid renewalId, DateTime responseDate, bool accepted);
            Task ApproveRenewalAsync(Guid renewalId, ApproveRenewalDto dto);
            Task DeclineRenewalAsync(Guid renewalId, DeclineRenewalDto dto);
            Task CancelRenewalAsync(Guid renewalId, CancelRenewalDto dto);

            // Renewal Execution
            Task<RenewalExecutionResultDto> ExecuteRenewalAsync(ExecuteRenewalDto dto);
            Task<RenewalPreviewDto> PreviewRenewalAsync(Guid renewalId);

            // Queries
            Task<PagedResult<ContractDueForRenewalDto>> GetContractsDueForRenewalAsync(
                ContractsDueForRenewalQueryDto query);
            Task<int> GetRenewalCountByContractAsync(Guid contractId);
        }

        ============================================================================
        TERMINATION SERVICE INTERFACE
        ============================================================================

        public interface IContractTerminationService
        {
            // CRUD Operations
            Task<ErpContractTermination> CreateTerminationAsync(CreateContractTerminationDto dto);
            Task<ErpContractTermination> GetTerminationByIdAsync(Guid terminationId);
            Task<PagedResult<ErpContractTerminationListDto>> GetTerminationsAsync(
                ContractTerminationQueryDto query);
            Task<ErpContractTermination> GetTerminationByContractAsync(Guid contractId);
            Task UpdateTerminationAsync(Guid terminationId, UpdateContractTerminationDto dto);
            Task DeleteTerminationAsync(Guid terminationId);

            // Termination Workflow
            Task SubmitForApprovalAsync(Guid terminationId);
            Task ApproveTerminationAsync(Guid terminationId, ApproveTerminationDto dto);
            Task RejectTerminationAsync(Guid terminationId, string reason);
            Task CancelTerminationAsync(Guid terminationId, CancelTerminationDto dto);

            // Termination Processing
            Task<TerminationExecutionResultDto> ProcessTerminationAsync(Guid terminationId);
            Task<TerminationPreviewDto> PreviewTerminationAsync(Guid terminationId);

            // Settlement
            Task ProcessSecurityDepositAsync(ProcessSecurityDepositDto dto);
            Task RecordHandoverInspectionAsync(RecordHandoverInspectionDto dto);
            Task CompleteSettlementAsync(CompleteSettlementDto dto);
            Task<TerminationFinancialSummaryDto> GetFinancialSummaryAsync(Guid terminationId);

            // Invoice Generation
            Task<Guid> GenerateFinalInvoiceAsync(Guid terminationId);
            Task<Guid> GeneratePenaltyInvoiceAsync(Guid terminationId);
        }

        ============================================================================
        RENEWAL EXECUTION WORKFLOW
        ============================================================================

        public async Task<RenewalExecutionResultDto> ExecuteRenewalAsync(ExecuteRenewalDto dto)
        {
            var result = new RenewalExecutionResultDto();

            using var transaction = await _db.BeginTransactionAsync();
            try
            {
                // 1. Load and validate renewal
                var renewal = await GetRenewalWithContractAsync(dto.RenewalId);
                ValidateCanExecute(renewal);

                // 2. Calculate new values
                var newEndDate = renewal.NewEndDate;
                var newBaseAmount = CalculateNewBaseAmount(renewal);
                var newContractValue = CalculateNewContractValue(renewal);

                // 3. Update contract master
                var contract = renewal.Contract;
                contract.EndDate = newEndDate;
                contract.BaseAmount = newBaseAmount;
                contract.TotalAmount += newContractValue - renewal.OriginalContractValue;
                contract.LastRenewalDate = DateTime.UtcNow;
                contract.TotalRenewals = (contract.TotalRenewals ?? 0) + 1;

                // Apply terms changes if any
                if (renewal.TermsChanged)
                {
                    if (renewal.NewPaymentTerms.HasValue)
                        contract.PaymentTerms = renewal.NewPaymentTerms.Value;
                    if (renewal.NewBillingFrequency.HasValue)
                        contract.BillingFrequency = renewal.NewBillingFrequency.Value;
                }

                await UpdateContractAsync(contract);

                // 4. Generate new billing schedules
                if (dto.GenerateBillingSchedules)
                {
                    var scheduleResult = await GenerateRenewalBillingSchedulesAsync(renewal);
                    result.SchedulesGenerated = scheduleResult.SchedulesCreated;
                    result.TotalSchedulesAmount = scheduleResult.TotalAmount;

                    // Update renewal with schedule info
                    renewal.SchedulesGenerated = scheduleResult.SchedulesCreated;
                    renewal.FirstNewScheduleDate = scheduleResult.FirstPeriodStart;
                    renewal.LastNewScheduleDate = scheduleResult.LastPeriodEnd;
                    renewal.TotalNewSchedulesAmount = scheduleResult.TotalAmount;
                }

                // 5. Create amendment record (optional)
                if (renewal.TermsChanged || renewal.NewBaseAmount != renewal.PreviousBaseAmount)
                {
                    var amendment = await CreateRenewalAmendmentAsync(renewal);
                    renewal.AmendmentId = amendment.AmendmentId;
                    result.AmendmentId = amendment.AmendmentId;
                }

                // 6. Update renewal status
                renewal.Status = RenewalStatus.Executed;
                renewal.RenewalExecutionDate = DateTime.UtcNow;
                await UpdateRenewalAsync(renewal);

                // 7. Create audit log
                await CreateAuditLogAsync("RenewalExecuted", renewal);

                await transaction.CommitAsync();

                result.Success = true;
                result.RenewalId = renewal.RenewalId;
                result.RenewalNumber = renewal.RenewalNumber;
                result.NewContractEndDate = newEndDate;
                result.NewContractValue = newContractValue;
                result.ExecutedAt = DateTime.UtcNow;

                return result;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                result.Success = false;
                result.ErrorMessage = ex.Message;
                return result;
            }
        }

        ============================================================================
        TERMINATION EXECUTION WORKFLOW
        ============================================================================

        public async Task<TerminationExecutionResultDto> ProcessTerminationAsync(Guid terminationId)
        {
            var result = new TerminationExecutionResultDto();

            using var transaction = await _db.BeginTransactionAsync();
            try
            {
                // 1. Load and validate termination
                var termination = await GetTerminationWithContractAsync(terminationId);
                ValidateCanProcess(termination);

                // 2. Cancel pending billing schedules
                var cancelResult = await CancelPendingSchedulesAsync(termination);
                result.SchedulesCancelled = cancelResult.SchedulesCancelled;
                result.CancelledAmount = cancelResult.CancelledAmount;

                termination.SchedulesCancelled = cancelResult.SchedulesCancelled;
                termination.CancelledSchedulesAmount = cancelResult.CancelledAmount;

                // 3. Generate final prorated invoice if needed
                if (termination.FinalInvoiceRequired)
                {
                    var finalInvoice = await GenerateFinalProratedInvoiceAsync(termination);
                    termination.FinalInvoiceId = finalInvoice.InvoiceId;
                    result.FinalInvoiceId = finalInvoice.InvoiceId;
                    result.FinalInvoiceNumber = finalInvoice.InvoiceNumber;
                }

                // 4. Generate penalty invoice if applicable
                if (termination.PenaltyApplicable && !termination.PenaltyWaived &&
                    termination.PenaltyAmount > 0)
                {
                    var penaltyInvoice = await GeneratePenaltyInvoiceAsync(termination);
                    termination.PenaltyInvoiceId = penaltyInvoice.InvoiceId;
                    result.PenaltyInvoiceId = penaltyInvoice.InvoiceId;
                    result.PenaltyInvoiceNumber = penaltyInvoice.InvoiceNumber;
                }

                // 5. Process unit returns
                await ProcessUnitReturnsAsync(termination);

                // 6. Calculate settlement
                termination.SettlementAmount = CalculateSettlementAmount(termination);
                result.SettlementAmount = termination.SettlementAmount;

                // 7. Update contract status
                var contract = termination.Contract;
                contract.Status = ContractStatus.Terminated;
                contract.ActualEndDate = termination.EffectiveDate;
                contract.TerminationDate = DateTime.UtcNow;
                contract.TerminationReason = termination.TerminationReason;
                await UpdateContractAsync(contract);

                // 8. Update termination status
                termination.Status = termination.HandoverInspectionRequired
                    ? TerminationStatus.PendingHandover
                    : TerminationStatus.PendingSettlement;

                await UpdateTerminationAsync(termination);

                // 9. Create GL journal entries
                await CreateTerminationJournalEntriesAsync(termination);

                // 10. Create audit log
                await CreateAuditLogAsync("TerminationProcessed", termination);

                await transaction.CommitAsync();

                result.Success = true;
                result.TerminationId = termination.TerminationId;
                result.TerminationNumber = termination.TerminationNumber;
                result.NewStatus = termination.Status;
                result.ProcessedAt = DateTime.UtcNow;

                return result;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                result.Success = false;
                result.ErrorMessage = ex.Message;
                return result;
            }
        }

        ============================================================================
        BILLING CANCELLATION RULES
        ============================================================================

        RULE 1: Only Pending Schedules Can Be Cancelled
        ------------------------------------------------
        - Status = Pending: Can be cancelled
        - Status = Invoiced: Cannot cancel (invoice exists)
        - Status = Paid: Cannot cancel (payment received)
        - Status = Skipped: Already skipped, no action needed
        - Status = Cancelled: Already cancelled

        RULE 2: Prorated Final Schedule
        -------------------------------
        If termination effective date falls within a billing period:
        - Adjust the schedule to cover only up to effective date
        - Calculate prorated amount: Amount * (DaysUsed / TotalDays)
        - Mark as "Prorated" in notes

        RULE 3: Cancellation Effective Date
        -----------------------------------
        - Cancel schedules where PeriodStart > EffectiveDate
        - Prorate schedules where PeriodStart <= EffectiveDate < PeriodEnd
        - Leave schedules where PeriodEnd < EffectiveDate unchanged

        IMPLEMENTATION:
        ---------------
        public async Task<BillingCancellationResult> CancelPendingSchedulesAsync(
            ErpContractTermination termination)
        {
            var result = new BillingCancellationResult();

            // Get all billing schedules for the contract
            var schedules = await GetSchedulesAsync(termination.ContractId);

            foreach (var schedule in schedules)
            {
                if (schedule.Status != BillingScheduleStatus.Pending)
                    continue;

                if (schedule.PeriodStart > termination.EffectiveDate)
                {
                    // Full cancellation - period entirely after termination
                    schedule.Status = BillingScheduleStatus.Cancelled;
                    schedule.CancellationDate = DateTime.UtcNow;
                    schedule.CancellationReason = $"Contract terminated: {termination.TerminationNumber}";

                    result.SchedulesCancelled++;
                    result.CancelledAmount += schedule.TotalAmount;
                }
                else if (schedule.PeriodStart <= termination.EffectiveDate &&
                         schedule.PeriodEnd > termination.EffectiveDate)
                {
                    // Partial period - needs proration
                    var originalAmount = schedule.TotalAmount;
                    var daysUsed = (termination.EffectiveDate - schedule.PeriodStart).Days + 1;
                    var totalDays = (schedule.PeriodEnd - schedule.PeriodStart).Days + 1;
                    var proratedRatio = (decimal)daysUsed / totalDays;

                    // Adjust schedule
                    schedule.PeriodEnd = termination.EffectiveDate;
                    schedule.BaseAmount = Math.Round(schedule.BaseAmount * proratedRatio, 2);
                    schedule.AdjustmentAmount = Math.Round(schedule.AdjustmentAmount * proratedRatio, 2);
                    schedule.Subtotal = schedule.BaseAmount + schedule.AdjustmentAmount;
                    schedule.TaxAmount = Math.Round(schedule.Subtotal * taxRate, 2);
                    schedule.TotalAmount = schedule.Subtotal + schedule.TaxAmount;
                    schedule.Notes = $"Prorated for early termination. Original: {originalAmount:C}";

                    result.ProratedSchedules++;
                    result.ProratedSavings += originalAmount - schedule.TotalAmount;

                    // This becomes the final billing schedule
                    termination.FinalBillingScheduleId = schedule.ScheduleId;
                }
                // else: schedule is before termination date, leave unchanged
            }

            await SaveSchedulesAsync(schedules);

            return result;
        }

        ============================================================================
        JOURNAL IMPACT - RENEWAL
        ============================================================================

        Renewal typically doesn't require immediate journal entries unless:
        1. Prepaid rent is recognized differently
        2. Deferred revenue needs adjustment

        For contract value change tracking:
        - Record in off-balance sheet if required by accounting policy
        - Update commitment disclosure

        ============================================================================
        JOURNAL IMPACT - TERMINATION
        ============================================================================

        SCENARIO 1: Early Termination with Penalty
        ------------------------------------------
        When penalty invoice is generated:

        DR: Accounts Receivable (Customer)     XXX
            CR: Early Termination Penalty Income    XXX

        GL Account Mapping:
        - Penalty Income: Use from ContractPricingRule or system default
        - Receivable: Customer's AR account

        SCENARIO 2: Security Deposit Refund
        -----------------------------------
        When deposit is refunded:

        DR: Security Deposits Held (Liability)     XXX
            CR: Bank/Cash                              XXX

        If deposit applied to outstanding balance:

        DR: Security Deposits Held (Liability)     XXX
            CR: Accounts Receivable (Customer)         XXX

        SCENARIO 3: Deposit Forfeiture (Damages)
        ----------------------------------------
        When deposit is forfeited due to damages:

        DR: Security Deposits Held (Liability)     XXX
            CR: Other Income - Forfeited Deposits      XXX

        SCENARIO 4: Write-Off of Uncollectible Balance
        ----------------------------------------------
        If remaining balance is written off:

        DR: Bad Debt Expense                       XXX
            CR: Accounts Receivable (Customer)         XXX

        SCENARIO 5: Cancelled Schedule Revenue Reversal
        -----------------------------------------------
        If deferred revenue was recognized for cancelled schedules:

        DR: Deferred Revenue (Liability)           XXX
            CR: Contract Revenue                       XXX (reversal)

        IMPLEMENTATION:
        ---------------
        public async Task CreateTerminationJournalEntriesAsync(
            ErpContractTermination termination)
        {
            var journalEntries = new List<JournalEntry>();

            // 1. Penalty income entry
            if (termination.PenaltyAmount > 0 && !termination.PenaltyWaived)
            {
                var penaltyEntry = new JournalEntry
                {
                    Date = termination.EffectiveDate,
                    Reference = termination.TerminationNumber,
                    Description = $"Early termination penalty - Contract {termination.Contract.ContractNumber}",
                    Lines = new List<JournalLine>
                    {
                        new JournalLine
                        {
                            AccountId = GetARAccountId(termination.Contract.CustomerId),
                            DebitAmount = termination.PenaltyAmount.Value,
                            Description = "Early termination penalty receivable"
                        },
                        new JournalLine
                        {
                            AccountId = GetPenaltyIncomeAccountId(),
                            CreditAmount = termination.PenaltyAmount.Value,
                            Description = "Early termination penalty income"
                        }
                    }
                };
                journalEntries.Add(penaltyEntry);
            }

            // 2. Security deposit settlement entry
            if (termination.SecurityDepositAmount > 0)
            {
                var depositEntry = CreateDepositSettlementEntry(termination);
                journalEntries.Add(depositEntry);
            }

            // 3. Deferred revenue adjustment (if applicable)
            var deferredAdjustment = await CalculateDeferredRevenueAdjustmentAsync(termination);
            if (deferredAdjustment != 0)
            {
                var deferredEntry = CreateDeferredRevenueEntry(termination, deferredAdjustment);
                journalEntries.Add(deferredEntry);
            }

            await SaveJournalEntriesAsync(journalEntries);
        }

        ============================================================================
        AUTO-RENEWAL PROCESSING (Scheduled Job)
        ============================================================================

        public async Task<List<ErpContractRenewal>> ProcessDueAutoRenewalsAsync(DateTime asOfDate)
        {
            var processedRenewals = new List<ErpContractRenewal>();

            // Find contracts due for auto-renewal
            // Criteria: AutoRenew = true, Status = Active, EndDate within notice period
            var contractsDue = await GetContractsForAutoRenewalAsync(asOfDate);

            foreach (var contract in contractsDue)
            {
                try
                {
                    // Check if renewal already exists
                    var existingRenewal = await GetPendingRenewalAsync(contract.ContractId);
                    if (existingRenewal != null)
                        continue;

                    // Create auto-renewal
                    var renewal = await CreateAutoRenewalAsync(new CreateAutoRenewalDto
                    {
                        ContractId = contract.ContractId,
                        UseContractDefaults = true
                    });

                    // Auto-execute if configured
                    if (contract.AutoRenewalAutoExecute)
                    {
                        await ExecuteRenewalAsync(new ExecuteRenewalDto
                        {
                            RenewalId = renewal.RenewalId,
                            GenerateBillingSchedules = true,
                            ExecutionNotes = "Auto-executed by system"
                        });
                    }

                    processedRenewals.Add(renewal);
                }
                catch (Exception ex)
                {
                    // Log error but continue processing other contracts
                    await LogAutoRenewalErrorAsync(contract.ContractId, ex);
                }
            }

            return processedRenewals;
        }

        ============================================================================
        CONTRACT EXPIRY NOTIFICATION (Scheduled Job)
        ============================================================================

        public async Task SendExpiryNotificationsAsync(DateTime asOfDate)
        {
            // Get notification thresholds from settings
            var thresholds = new[] { 90, 60, 30, 14, 7 }; // days before expiry

            foreach (var days in thresholds)
            {
                var expiringContracts = await GetContractsExpiringInDaysAsync(asOfDate, days);

                foreach (var contract in expiringContracts)
                {
                    // Check if notification already sent for this threshold
                    if (await NotificationAlreadySentAsync(contract.ContractId, days))
                        continue;

                    await SendExpiryNotificationAsync(contract, days);
                    await RecordNotificationSentAsync(contract.ContractId, days);
                }
            }
        }

        ============================================================================
        */
    }

    /// <summary>
    /// Billing Cancellation Result - نتيجة إلغاء الفوترة
    /// </summary>
    public class BillingCancellationResult
    {
        public int SchedulesCancelled { get; set; }
        public decimal CancelledAmount { get; set; }
        public int ProratedSchedules { get; set; }
        public decimal ProratedSavings { get; set; }
        public Guid? FinalScheduleId { get; set; }
        public decimal? FinalScheduleAmount { get; set; }
    }

    /// <summary>
    /// Renewal Schedule Generation Result - نتيجة توليد جدول التجديد
    /// </summary>
    public class RenewalScheduleGenerationResult
    {
        public int SchedulesCreated { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime FirstPeriodStart { get; set; }
        public DateTime LastPeriodEnd { get; set; }
        public List<Guid> ScheduleIds { get; set; } = new();
    }

    /// <summary>
    /// Contract Expiry Notification DTO - إشعار انتهاء العقد
    /// </summary>
    public class ContractExpiryNotificationDto
    {
        public Guid ContractId { get; set; }
        public string ContractNumber { get; set; }
        public string ContractTitle { get; set; }
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public DateTime EndDate { get; set; }
        public int DaysUntilExpiry { get; set; }
        public bool AutoRenew { get; set; }
        public decimal ContractValue { get; set; }
        public string Currency { get; set; }
        public string AccountManagerEmail { get; set; }
    }
}
