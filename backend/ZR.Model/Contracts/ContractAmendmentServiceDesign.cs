using System;
using System.Collections.Generic;

namespace ZR.Model.Contracts
{
    /// <summary>
    /// Contract Amendment Service Design - تصميم خدمة تعديلات العقود
    ///
    /// This file documents the amendment service interface and billing schedule recalculation logic.
    /// It serves as a blueprint for implementing the actual service in ZR.Service.
    /// </summary>
    public static class ContractAmendmentServiceDesign
    {
        /*
        ============================================================================
        AMENDMENT SERVICE INTERFACE
        ============================================================================

        public interface IContractAmendmentService
        {
            // CRUD Operations
            Task<ErpContractAmendment> CreateAmendmentAsync(CreateAmendmentDto dto);
            Task<ErpContractAmendment> GetAmendmentByIdAsync(Guid amendmentId);
            Task<PagedResult<ErpContractAmendmentListDto>> GetAmendmentsAsync(ContractAmendmentQueryDto query);
            Task<List<ErpContractAmendmentListDto>> GetAmendmentsByContractAsync(Guid contractId);
            Task UpdateAmendmentAsync(Guid amendmentId, UpdateAmendmentDto dto);
            Task DeleteAmendmentAsync(Guid amendmentId);

            // Type-Specific Creation
            Task<ErpContractAmendment> CreatePriceChangeAmendmentAsync(CreatePriceChangeAmendmentDto dto);
            Task<ErpContractAmendment> CreateDurationExtensionAsync(CreateDurationExtensionAmendmentDto dto);
            Task<ErpContractAmendment> CreateUnitAdditionAsync(CreateUnitAdditionAmendmentDto dto);
            Task<ErpContractAmendment> CreateUnitRemovalAsync(CreateUnitRemovalAmendmentDto dto);
            Task<ErpContractAmendment> CreateDiscountAmendmentAsync(CreateDiscountAmendmentDto dto);
            Task<ErpContractAmendment> CreateTermsChangeAsync(CreateTermsChangeAmendmentDto dto);
            Task<ErpContractAmendment> CreateScopeChangeAsync(CreateScopeChangeAmendmentDto dto);

            // Approval Workflow
            Task SubmitForApprovalAsync(Guid amendmentId);
            Task ApproveAmendmentAsync(Guid amendmentId, ApproveAmendmentDto dto);
            Task RejectAmendmentAsync(Guid amendmentId, RejectAmendmentDto dto);

            // Amendment Application
            Task<AmendmentApplicationResult> ApplyAmendmentAsync(Guid amendmentId);
            Task<bool> RevertAmendmentAsync(Guid amendmentId, RevertAmendmentDto dto);

            // Impact Preview
            Task<AmendmentImpactPreviewDto> PreviewAmendmentImpactAsync(Guid amendmentId);
            Task<BillingScheduleImpactDto> PreviewBillingImpactAsync(Guid amendmentId);

            // Amendment History
            Task<List<AmendmentHistoryDto>> GetAmendmentHistoryAsync(Guid contractId);
            Task<ContractValueTimelineDto> GetContractValueTimelineAsync(Guid contractId);
        }

        ============================================================================
        BILLING SCHEDULE RECALCULATION LOGIC
        ============================================================================

        When an amendment is applied, the billing schedule must be recalculated.
        The logic varies based on amendment type and effective date.

        IMPORTANT PRINCIPLES:
        1. Never modify invoiced schedules (Status = Invoiced or Paid)
        2. Pending schedules can be regenerated
        3. Calculate prorated amounts for partial periods
        4. Maintain audit trail of all changes

        ============================================================================
        SCENARIO 1: PRICE CHANGE AMENDMENT
        ============================================================================

        When: Contract price changes (increase/decrease) from effective date

        Steps:
        1. Get all pending billing schedules where PeriodEnd >= EffectiveDate
        2. For each affected schedule:
           a. If PeriodStart >= EffectiveDate:
              - Apply new price for entire period
              - Recalculate: BaseAmount = NewPrice * PeriodDays / TotalDaysInCycle
           b. If PeriodStart < EffectiveDate < PeriodEnd:
              - Split into two portions (proration)
              - OldPortion = OldPrice * DaysBeforeEffective
              - NewPortion = NewPrice * DaysAfterEffective
              - BaseAmount = OldPortion + NewPortion
        3. Recalculate adjustments and tax for each schedule
        4. Update TotalAmount
        5. Log changes in schedule notes

        Proration Formula:
        ------------------
        DaysBeforeEffective = (EffectiveDate - PeriodStart).Days
        DaysAfterEffective = (PeriodEnd - EffectiveDate).Days + 1
        TotalDays = (PeriodEnd - PeriodStart).Days + 1

        OldAmount = (OldPrice / DaysInMonth) * DaysBeforeEffective
        NewAmount = (NewPrice / DaysInMonth) * DaysAfterEffective

        ============================================================================
        SCENARIO 2: DURATION EXTENSION
        ============================================================================

        When: Contract end date is extended

        Steps:
        1. Update contract EndDate
        2. Identify the last billing schedule before original end date
        3. Generate new billing schedules from original end date to new end date
        4. Apply current pricing rules to new schedules
        5. Update any renewal-related fields

        New Schedule Generation:
        -----------------------
        - Use existing BillingScheduleGenerator logic
        - Apply all active pricing rules
        - Include all active contract units
        - Set status = Pending for all new schedules

        ============================================================================
        SCENARIO 3: UNIT ADDITION
        ============================================================================

        When: New units added to contract from effective date

        Steps:
        1. Add new ErpContractUnit records
        2. Get pending schedules where PeriodEnd >= EffectiveDate
        3. For each affected schedule:
           a. If PeriodStart >= EffectiveDate:
              - Add line items for new units
              - Calculate full period amount
           b. If PeriodStart < EffectiveDate < PeriodEnd:
              - Add prorated line items for new units
              - Calculate partial period amount
        4. Recalculate schedule totals
        5. Update contract TotalAmount and BaseAmount

        Unit Addition Amount Calculation:
        --------------------------------
        if (schedule.PeriodStart >= effectiveDate)
        {
            lineAmount = unit.BasePrice; // Full period
        }
        else
        {
            daysActive = (schedule.PeriodEnd - effectiveDate).Days + 1;
            totalDays = (schedule.PeriodEnd - schedule.PeriodStart).Days + 1;
            lineAmount = unit.BasePrice * daysActive / totalDays;
        }

        ============================================================================
        SCENARIO 4: UNIT REMOVAL
        ============================================================================

        When: Units removed from contract from effective date

        Steps:
        1. Mark ErpContractUnit as inactive (set ReturnDate)
        2. Get pending schedules where PeriodEnd >= EffectiveDate
        3. For each affected schedule:
           a. If PeriodStart >= EffectiveDate:
              - Remove line items for removed units
           b. If PeriodStart < EffectiveDate < PeriodEnd:
              - Adjust line items to prorate only up to effective date
        4. Recalculate schedule totals
        5. Update contract TotalAmount and BaseAmount

        Unit Removal Proration:
        ----------------------
        if (schedule.PeriodStart >= effectiveDate)
        {
            lineAmount = 0; // Unit not active in period
        }
        else
        {
            daysActive = (effectiveDate - schedule.PeriodStart).Days;
            totalDays = (schedule.PeriodEnd - schedule.PeriodStart).Days + 1;
            lineAmount = originalAmount * daysActive / totalDays;
        }

        ============================================================================
        SCENARIO 5: DISCOUNT AMENDMENT
        ============================================================================

        When: New discount applied or existing discount modified

        Steps:
        1. Create/Update ErpContractPricingRule of type Discount
        2. Get pending schedules within discount validity period
        3. For each affected schedule:
           a. Add adjustment line item with negative amount
           b. Calculate discount based on rule type:
              - Percentage: AdjustmentAmount = -BaseAmount * DiscountPercent
              - Fixed: AdjustmentAmount = -FixedDiscountAmount
              - Volume: Calculate based on tier thresholds
        4. Recalculate schedule totals

        Discount Application:
        --------------------
        foreach (var schedule in affectedSchedules)
        {
            var discountAmount = pricingRule.RuleType switch
            {
                PricingRuleType.Discount =>
                    schedule.BaseAmount * (pricingRule.DiscountPercentage / 100),
                PricingRuleType.FixedDiscount =>
                    pricingRule.FixedDiscountAmount ?? 0,
                PricingRuleType.TieredDiscount =>
                    CalculateTieredDiscount(schedule.BaseAmount, pricingRule),
                _ => 0
            };

            schedule.AdjustmentAmount -= discountAmount;
            schedule.Subtotal = schedule.BaseAmount + schedule.AdjustmentAmount;
            schedule.TaxAmount = schedule.Subtotal * taxRate;
            schedule.TotalAmount = schedule.Subtotal + schedule.TaxAmount;
        }

        ============================================================================
        SCENARIO 6: TERMS CHANGE
        ============================================================================

        When: Payment terms or billing frequency changes

        For Payment Terms Change:
        1. Update contract PaymentTerms
        2. Recalculate DueDate for all pending schedules:
           DueDate = PeriodEnd.AddDays(NewPaymentTerms)

        For Billing Frequency Change:
        1. Cancel all pending schedules after effective date
        2. Regenerate schedules with new frequency
        3. Maintain continuity (no gaps, no overlaps)

        Frequency Change Logic:
        ----------------------
        // Cancel existing pending schedules
        var schedulesToCancel = await GetSchedulesAsync(contractId,
            s => s.Status == BillingScheduleStatus.Pending &&
                 s.PeriodStart >= effectiveDate);

        foreach (var schedule in schedulesToCancel)
        {
            schedule.Status = BillingScheduleStatus.Cancelled;
            schedule.CancelledByAmendmentId = amendmentId;
            schedule.CancellationDate = DateTime.UtcNow;
        }

        // Generate new schedules with new frequency
        var generateRequest = new GenerateBillingScheduleRequestDto
        {
            ContractId = contractId,
            StartDate = effectiveDate,
            EndDate = contract.EndDate,
            BillingFrequency = newFrequency,
            IncludeUnits = true,
            ApplyPricingRules = true
        };

        await BillingScheduleGenerator.GenerateAsync(generateRequest);

        ============================================================================
        AMENDMENT APPLICATION WORKFLOW
        ============================================================================

        public async Task<AmendmentApplicationResult> ApplyAmendmentAsync(Guid amendmentId)
        {
            var result = new AmendmentApplicationResult();

            using var transaction = await _db.BeginTransactionAsync();
            try
            {
                // 1. Load amendment and contract
                var amendment = await GetAmendmentWithContractAsync(amendmentId);
                ValidateCanApply(amendment);

                // 2. Store snapshot of current state
                await StorePreAmendmentSnapshotAsync(amendment);

                // 3. Apply changes based on amendment type
                switch (amendment.AmendmentType)
                {
                    case AmendmentType.PriceChange:
                        result.SchedulesAffected = await ApplyPriceChangeAsync(amendment);
                        break;
                    case AmendmentType.DurationExtension:
                        result.SchedulesGenerated = await ApplyDurationExtensionAsync(amendment);
                        break;
                    case AmendmentType.UnitAddition:
                        result.UnitsAdded = await ApplyUnitAdditionAsync(amendment);
                        result.SchedulesAffected = await RecalculateSchedulesForUnitsAsync(amendment);
                        break;
                    case AmendmentType.UnitRemoval:
                        result.UnitsRemoved = await ApplyUnitRemovalAsync(amendment);
                        result.SchedulesAffected = await RecalculateSchedulesForUnitsAsync(amendment);
                        break;
                    case AmendmentType.Discount:
                        result.SchedulesAffected = await ApplyDiscountAsync(amendment);
                        break;
                    case AmendmentType.TermsChange:
                        result.SchedulesAffected = await ApplyTermsChangeAsync(amendment);
                        break;
                    case AmendmentType.ScopeChange:
                        result.SchedulesAffected = await ApplyScopeChangeAsync(amendment);
                        break;
                    default:
                        throw new InvalidOperationException($"Unknown amendment type: {amendment.AmendmentType}");
                }

                // 4. Update contract master values
                await UpdateContractMasterAsync(amendment);

                // 5. Update amendment status
                amendment.Status = AmendmentStatus.Applied;
                amendment.ApplicationDate = DateTime.UtcNow;
                amendment.AppliedBy = _currentUser.UserId;

                // 6. Create audit log entry
                await CreateAuditLogAsync(amendment, result);

                await transaction.CommitAsync();

                result.Success = true;
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
        AMENDMENT REVERSION WORKFLOW
        ============================================================================

        public async Task<bool> RevertAmendmentAsync(Guid amendmentId, RevertAmendmentDto dto)
        {
            using var transaction = await _db.BeginTransactionAsync();
            try
            {
                var amendment = await GetAmendmentAsync(amendmentId);
                ValidateCanRevert(amendment);

                // 1. Check no invoiced schedules affected
                var hasInvoicedSchedules = await HasInvoicedSchedulesAfterAsync(
                    amendment.ContractId, amendment.EffectiveDate);
                if (hasInvoicedSchedules)
                {
                    throw new BusinessException(
                        "Cannot revert: Some affected schedules have been invoiced");
                }

                // 2. Restore from snapshot
                await RestorePreAmendmentSnapshotAsync(amendment);

                // 3. Regenerate billing schedules from effective date
                await RegenerateBillingSchedulesAsync(
                    amendment.ContractId,
                    amendment.EffectiveDate);

                // 4. Update amendment status
                amendment.Status = AmendmentStatus.Reverted;
                amendment.RevertedDate = DateTime.UtcNow;
                amendment.RevertedBy = _currentUser.UserId;
                amendment.ReversionReason = dto.Reason;

                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        ============================================================================
        IMPACT PREVIEW CALCULATIONS
        ============================================================================

        public async Task<AmendmentImpactPreviewDto> PreviewAmendmentImpactAsync(Guid amendmentId)
        {
            var amendment = await GetAmendmentWithContractAsync(amendmentId);
            var preview = new AmendmentImpactPreviewDto();

            // Calculate financial impact
            preview.CurrentContractValue = amendment.Contract.TotalAmount;
            preview.NewContractValue = CalculateNewContractValue(amendment);
            preview.ValueChange = preview.NewContractValue - preview.CurrentContractValue;
            preview.PercentageChange = preview.CurrentContractValue > 0
                ? (preview.ValueChange / preview.CurrentContractValue) * 100
                : 0;

            // Calculate billing impact
            var billingImpact = await PreviewBillingImpactAsync(amendmentId);
            preview.BillingImpact = billingImpact;

            // Generate warnings
            preview.Warnings = new List<string>();

            if (preview.PercentageChange > 20)
                preview.Warnings.Add("Value change exceeds 20% - may require additional approval");

            if (billingImpact.SchedulesToCancel > 0)
                preview.Warnings.Add($"{billingImpact.SchedulesToCancel} schedules will be cancelled");

            if (amendment.EffectiveDate < DateTime.Today)
                preview.Warnings.Add("Effective date is in the past - retroactive amendment");

            return preview;
        }

        ============================================================================
        HELPER METHODS
        ============================================================================
        */
    }

    /// <summary>
    /// Result of applying an amendment - نتيجة تطبيق التعديل
    /// </summary>
    public class AmendmentApplicationResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public int SchedulesAffected { get; set; }
        public int SchedulesGenerated { get; set; }
        public int SchedulesCancelled { get; set; }
        public int UnitsAdded { get; set; }
        public int UnitsRemoved { get; set; }
        public int PricingRulesModified { get; set; }
        public decimal PreviousContractValue { get; set; }
        public decimal NewContractValue { get; set; }
        public decimal ValueChange { get; set; }
        public DateTime AppliedAt { get; set; }
        public string AppliedBy { get; set; }
    }

    /// <summary>
    /// Billing schedule impact details - تفاصيل تأثير جدول الفوترة
    /// </summary>
    public class BillingScheduleImpactDto
    {
        /// <summary>
        /// Total pending schedules - إجمالي الجداول المعلقة
        /// </summary>
        public int TotalPendingSchedules { get; set; }

        /// <summary>
        /// Schedules to be modified - الجداول المراد تعديلها
        /// </summary>
        public int SchedulesToModify { get; set; }

        /// <summary>
        /// Schedules to be cancelled - الجداول المراد إلغاؤها
        /// </summary>
        public int SchedulesToCancel { get; set; }

        /// <summary>
        /// New schedules to generate - الجداول الجديدة المراد إنشاؤها
        /// </summary>
        public int SchedulesToGenerate { get; set; }

        /// <summary>
        /// Schedules unaffected - الجداول غير المتأثرة
        /// </summary>
        public int SchedulesUnaffected { get; set; }

        /// <summary>
        /// Current total pending amount - المبلغ المعلق الحالي
        /// </summary>
        public decimal CurrentPendingAmount { get; set; }

        /// <summary>
        /// New total pending amount - المبلغ المعلق الجديد
        /// </summary>
        public decimal NewPendingAmount { get; set; }

        /// <summary>
        /// Amount difference - فرق المبلغ
        /// </summary>
        public decimal AmountDifference { get; set; }

        /// <summary>
        /// First affected period start - بداية أول فترة متأثرة
        /// </summary>
        public DateTime? FirstAffectedPeriodStart { get; set; }

        /// <summary>
        /// Last affected period end - نهاية آخر فترة متأثرة
        /// </summary>
        public DateTime? LastAffectedPeriodEnd { get; set; }

        /// <summary>
        /// Affected schedule details - تفاصيل الجداول المتأثرة
        /// </summary>
        public List<AffectedSchedulePreviewDto> AffectedSchedules { get; set; } = new();
    }

    /// <summary>
    /// Preview of an affected schedule - معاينة جدول متأثر
    /// </summary>
    public class AffectedSchedulePreviewDto
    {
        public Guid ScheduleId { get; set; }
        public int ScheduleNumber { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public decimal CurrentAmount { get; set; }
        public decimal NewAmount { get; set; }
        public decimal AmountChange { get; set; }
        public string ChangeType { get; set; } // Modified, Cancelled, New
        public string ChangeDescription { get; set; }
        public bool RequiresProration { get; set; }
        public int? DaysAtOldRate { get; set; }
        public int? DaysAtNewRate { get; set; }
    }

    /// <summary>
    /// Amendment history entry - سجل تاريخ التعديل
    /// </summary>
    public class AmendmentHistoryDto
    {
        public Guid AmendmentId { get; set; }
        public string AmendmentNumber { get; set; }
        public int AmendmentType { get; set; }
        public string AmendmentTypeName { get; set; }
        public int Status { get; set; }
        public string StatusName { get; set; }
        public DateTime EffectiveDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? AppliedDate { get; set; }
        public string CreatedBy { get; set; }
        public string AppliedBy { get; set; }
        public decimal? ValueChange { get; set; }
        public string Description { get; set; }
    }

    /// <summary>
    /// Contract value timeline - الجدول الزمني لقيمة العقد
    /// </summary>
    public class ContractValueTimelineDto
    {
        public Guid ContractId { get; set; }
        public string ContractNumber { get; set; }
        public decimal InitialValue { get; set; }
        public decimal CurrentValue { get; set; }
        public decimal TotalValueChange { get; set; }
        public List<ValueChangePointDto> Timeline { get; set; } = new();
    }

    /// <summary>
    /// Point in contract value timeline - نقطة في الجدول الزمني لقيمة العقد
    /// </summary>
    public class ValueChangePointDto
    {
        public DateTime Date { get; set; }
        public string Event { get; set; } // "Contract Created", "Amendment Applied", etc.
        public Guid? AmendmentId { get; set; }
        public string AmendmentNumber { get; set; }
        public decimal ValueBefore { get; set; }
        public decimal ValueAfter { get; set; }
        public decimal Change { get; set; }
        public string Description { get; set; }
    }

    /// <summary>
    /// Pre-amendment snapshot for reversion - لقطة ما قبل التعديل للاسترجاع
    /// </summary>
    public class AmendmentSnapshot
    {
        public Guid SnapshotId { get; set; }
        public Guid AmendmentId { get; set; }
        public DateTime SnapshotDate { get; set; }

        // Contract state
        public decimal ContractBaseAmount { get; set; }
        public decimal ContractTotalAmount { get; set; }
        public DateTime ContractEndDate { get; set; }
        public int ContractPaymentTerms { get; set; }

        // Serialized states (JSON)
        public string UnitsSnapshotJson { get; set; }
        public string PricingRulesSnapshotJson { get; set; }
        public string BillingSchedulesSnapshotJson { get; set; }
    }
}
