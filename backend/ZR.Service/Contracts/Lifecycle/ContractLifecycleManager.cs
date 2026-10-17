using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.Attribute;
using SqlSugar;
using SqlSugar.IOC;
using ZR.Model.Boulevard;
using ZR.Model.Contracts;
using ZR.Model.Contracts.Enums;
using ZR.Service.IService.Contracts.Lifecycle;
using ZR.Service.IService.Contracts.Billing;

namespace ZR.Service.Contracts.Lifecycle
{
    /// <summary>
    /// ContractLifecycleManager - Contract Lifecycle Management Implementation
    /// مدير دورة حياة العقد
    /// Updated to use [AppService] for DI registration
    /// </summary>
    [AppService(ServiceType = typeof(IContractLifecycleManager), ServiceLifetime = LifeTime.Transient)]
    public class ContractLifecycleManager : IContractLifecycleManager
    {
        /// <summary>
        /// Database context using SqlSugar IOC pattern
        /// </summary>
        private ISqlSugarClient Db => DbScoped.SugarScope.GetConnectionScope(0);

        private readonly IPricingRuleEngine _pricingEngine;

        public ContractLifecycleManager(IPricingRuleEngine pricingEngine = null)
        {
            _pricingEngine = pricingEngine;
        }

        #region Contract Renewal

        public async Task<RenewalResult> InitiateRenewalAsync(RenewalRequest request)
        {
            var result = new RenewalResult();

            try
            {
                var contract = await Db.Queryable<ErpContract>()
                    .Where(c => c.ContractId == request.ContractId)
                    .FirstAsync();

                if (contract == null)
                {
                    result.Success = false;
                    result.Message = "Contract not found";
                    return result;
                }

                var eligibility = await ValidateRenewalEligibilityAsync(contract);
                if (!eligibility.IsEligible)
                {
                    result.Success = false;
                    result.Message = eligibility.BlockingReasons.FirstOrDefault() ?? "Contract not eligible for renewal";
                    result.Warnings = eligibility.BlockingReasons;
                    return result;
                }

                var options = new RenewalOptions
                {
                    RenewalDurationMonths = contract.EndDate.HasValue
                        ? (int)Math.Round((request.NewEndDate - contract.EndDate.Value).TotalDays / 30)
                        : 12,
                    ApplyEscalation = request.ApplyEscalation
                };

                var terms = await CalculateRenewalTermsAsync(request.ContractId, options);

                if (request.NewRentAmount.HasValue)
                    terms.NewRentAmount = request.NewRentAmount.Value;

                if (request.EscalationPercentage.HasValue)
                {
                    terms.EscalationPercentage = request.EscalationPercentage.Value;
                    terms.EscalationAmount = terms.CurrentRentAmount * (request.EscalationPercentage.Value / 100);
                    terms.NewRentAmount = terms.CurrentRentAmount + terms.EscalationAmount;
                }

                terms.NewEndDate = request.NewEndDate;
                result = await ApplyRenewalAsync(request.ContractId, terms, request.UserName);
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Renewal failed: {ex.Message}";
            }

            return result;
        }

        public async Task<AutoRenewalBatchResult> ProcessAutoRenewalsAsync(DateTime asOfDate)
        {
            var batchResult = new AutoRenewalBatchResult { ProcessedAt = DateTime.Now };

            var autoRenewalContracts = await Db.Queryable<ErpContract>()
                .Where(c => c.Status == ContractStatus.Active
                    && c.AutoRenew == true
                    && c.EndDate != null
                    && c.EndDate <= asOfDate.AddDays(30))
                .ToListAsync();

            foreach (var contract in autoRenewalContracts)
            {
                batchResult.TotalProcessed++;

                try
                {
                    var renewalMonths = GetRenewalMonths(contract.RenewalType);
                    var newEndDate = contract.EndDate.Value.AddMonths(renewalMonths);

                    var request = new RenewalRequest
                    {
                        ContractId = contract.ContractId,
                        NewEndDate = newEndDate,
                        ApplyEscalation = true,
                        GenerateNewBillingSchedules = true,
                        UserName = "System.AutoRenewal",
                        Notes = $"Auto-renewal processed on {DateTime.Now:yyyy-MM-dd}"
                    };

                    var renewalResult = await InitiateRenewalAsync(request);
                    batchResult.Results.Add(renewalResult);

                    if (renewalResult.Success)
                        batchResult.SuccessfulRenewals++;
                    else
                        batchResult.FailedRenewals++;
                }
                catch (Exception ex)
                {
                    batchResult.FailedRenewals++;
                    batchResult.Results.Add(new RenewalResult
                    {
                        Success = false,
                        Message = $"Auto-renewal failed for contract {contract.ContractNumber}: {ex.Message}"
                    });
                }
            }

            return batchResult;
        }

        public async Task<List<RenewalEligibility>> GetRenewalEligibleContractsAsync(int daysBeforeExpiry = 90)
        {
            var cutoffDate = DateTime.Now.AddDays(daysBeforeExpiry);
            var eligibilityList = new List<RenewalEligibility>();

            var contracts = await Db.Queryable<ErpContract>()
                .LeftJoin<ErpContractFinancial>((c, f) => c.ContractId == f.ContractId)
                .Where((c, f) => c.Status == ContractStatus.Active
                    && c.EndDate != null
                    && c.EndDate <= cutoffDate
                    && c.EndDate >= DateTime.Now)
                .Select((c, f) => new { Contract = c, Financial = f })
                .ToListAsync();

            foreach (var item in contracts)
            {
                var contract = item.Contract;
                var financial = item.Financial;

                var outstandingAmount = await GetOutstandingAmountAsync(contract.ContractId);
                var currentRent = financial?.BaseAmount ?? 0;

                var eligibility = new RenewalEligibility
                {
                    ContractId = contract.ContractId,
                    ContractNumber = contract.ContractNumber,
                    CustomerName = contract.CustomerName,
                    ExpiryDate = contract.EndDate.Value,
                    DaysToExpiry = (int)(contract.EndDate.Value - DateTime.Now).TotalDays,
                    AutoRenewEnabled = contract.AutoRenew,
                    CurrentRentAmount = currentRent,
                    SuggestedRenewalAmount = currentRent,
                    EscalationPercentage = 0,
                    HasUnpaidInvoices = outstandingAmount > 0,
                    UnpaidAmount = outstandingAmount
                };

                var validation = await ValidateRenewalEligibilityAsync(contract);
                eligibility.EligibilityStatus = validation.IsEligible ? "Eligible" : "Blocked";
                eligibility.BlockingReasons = validation.BlockingReasons;

                eligibilityList.Add(eligibility);
            }

            return eligibilityList.OrderBy(e => e.DaysToExpiry).ToList();
        }

        public async Task<RenewalTerms> CalculateRenewalTermsAsync(Guid contractId, RenewalOptions options)
        {
            var contract = await Db.Queryable<ErpContract>()
                .Where(c => c.ContractId == contractId)
                .FirstAsync();

            var financial = await Db.Queryable<ErpContractFinancial>()
                .Where(f => f.ContractId == contractId)
                .FirstAsync();

            var billingConfig = await Db.Queryable<ErpContractBillingConfig>()
                .Where(b => b.ContractId == contractId)
                .FirstAsync();

            var currentRent = financial?.BaseAmount ?? 0;

            var terms = new RenewalTerms
            {
                NewStartDate = contract.EndDate?.AddDays(1) ?? DateTime.Now,
                NewEndDate = (contract.EndDate ?? DateTime.Now).AddMonths(options.RenewalDurationMonths),
                DurationMonths = options.RenewalDurationMonths,
                CurrentRentAmount = currentRent,
                EscalationPercentage = 0,
                EscalationAmount = 0,
                Currency = financial?.Currency ?? "SAR",
                CopyPricingRules = options.CopyExistingRules,
                CopyEscalationRules = options.CopyExistingRules
            };

            terms.NewRentAmount = terms.CurrentRentAmount + terms.EscalationAmount;

            var frequency = billingConfig?.BillingFrequency ?? BillingFrequency.Monthly;
            terms.EstimatedBillingPeriods = CalculateBillingPeriods(options.RenewalDurationMonths, frequency);
            terms.EstimatedTotalValue = terms.NewRentAmount * terms.EstimatedBillingPeriods;

            return terms;
        }

        public async Task<RenewalResult> ApplyRenewalAsync(Guid contractId, RenewalTerms terms, string userName)
        {
            var result = new RenewalResult();

            try
            {
                await Db.Ado.BeginTranAsync();

                var existingContract = await Db.Queryable<ErpContract>()
                    .Where(c => c.ContractId == contractId)
                    .FirstAsync();

                // Update existing contract status
                existingContract.Status = ContractStatus.Renewed;
                await Db.Updateable(existingContract).ExecuteCommandAsync();

                // Create renewal record
                var renewal = new ErpContractRenewal
                {
                    RenewalId = Guid.NewGuid(),
                    RenewalNumber = GenerateRenewalNumber(existingContract.ContractNumber, existingContract.RenewalCount + 1),
                    ContractId = contractId,
                    RenewalSequence = existingContract.RenewalCount + 1,
                    OriginalStartDate = existingContract.StartDate,
                    OriginalEndDate = existingContract.EndDate ?? DateTime.Now,
                    OriginalContractValue = terms.CurrentRentAmount * terms.EstimatedBillingPeriods,
                    NewStartDate = terms.NewStartDate,
                    NewEndDate = terms.NewEndDate,
                    RenewalDurationMonths = terms.DurationMonths,
                    NewContractValue = terms.EstimatedTotalValue,
                    RenewalType = existingContract.RenewalType,
                    Status = RenewalStatus.Executed,
                    IsAutoRenewal = userName == "System.AutoRenewal",
                    IsExtension = true,
                    TermsChanged = terms.EscalationPercentage > 0,
                    RenewalExecutionDate = DateTime.Now,
                    RequiresApproval = false
                };
                await Db.Insertable(renewal).ExecuteCommandAsync();

                // Extend contract dates (simple extension)
                existingContract.EndDate = terms.NewEndDate;
                existingContract.RenewalCount = existingContract.RenewalCount + 1;
                existingContract.Status = ContractStatus.Active;
                await Db.Updateable(existingContract).ExecuteCommandAsync();

                // Update financial if rent changed
                if (terms.EscalationAmount != 0)
                {
                    var financial = await Db.Queryable<ErpContractFinancial>()
                        .Where(f => f.ContractId == contractId)
                        .FirstAsync();

                    if (financial != null)
                    {
                        financial.BaseAmount = terms.NewRentAmount;
                        await Db.Updateable(financial).ExecuteCommandAsync();
                    }
                }

                await Db.Ado.CommitTranAsync();

                result.Success = true;
                result.NewContractId = contractId;
                result.RenewalNumber = renewal.RenewalNumber;
                result.NewStartDate = terms.NewStartDate;
                result.NewEndDate = terms.NewEndDate;
                result.NewRentAmount = terms.NewRentAmount;
                result.BillingSchedulesGenerated = 0;
                result.Message = "Contract renewed successfully";
            }
            catch (Exception ex)
            {
                await Db.Ado.RollbackTranAsync();
                result.Success = false;
                result.Message = $"Renewal failed: {ex.Message}";
            }

            return result;
        }

        #endregion

        #region Contract Termination

        public async Task<TerminationResult> InitiateTerminationAsync(TerminationRequest request)
        {
            var result = new TerminationResult();

            try
            {
                var contract = await Db.Queryable<ErpContract>()
                    .Where(c => c.ContractId == request.ContractId)
                    .FirstAsync();

                if (contract == null)
                {
                    result.Success = false;
                    result.Message = "Contract not found";
                    return result;
                }

                var financials = await CalculateTerminationFinancialsAsync(
                    request.ContractId,
                    request.TerminationDate,
                    request.TerminationType);

                if (request.WaivePenalty)
                {
                    financials.PenaltyWaived = true;
                    financials.PenaltyWaiverReason = request.WaiverReason;
                    financials.EarlyTerminationPenalty = 0;
                    financials.NetAmountDue = financials.TotalOutstanding - financials.DepositRefundAmount;
                }

                result.Financials = financials;

                await Db.Ado.BeginTranAsync();

                // Update contract status
                contract.Status = request.TerminationType == TerminationType.NaturalExpiry
                    ? ContractStatus.Expired
                    : ContractStatus.Terminated;

                await Db.Updateable(contract).ExecuteCommandAsync();

                // Create termination record
                var termination = new ErpContractTermination
                {
                    TerminationId = Guid.NewGuid(),
                    TerminationNumber = $"TRM-{contract.ContractNumber}-{DateTime.Now:yyyyMMdd}",
                    ContractId = request.ContractId,
                    TerminationReason = ConvertToTerminationReason(request.TerminationReason),
                    TerminationType = request.TerminationType,
                    Status = TerminationStatus.InProgress,
                    TerminationReasonDetail = request.Notes,
                    NoticeDate = DateTime.Now,
                    EffectiveDate = request.TerminationDate,
                    OriginalEndDate = contract.EndDate ?? DateTime.Now,
                    TotalContractValue = financials.TotalOutstanding,
                    OutstandingBalance = financials.TotalOutstanding,
                    PenaltyApplicable = financials.EarlyTerminationPenalty > 0,
                    PenaltyAmount = financials.EarlyTerminationPenalty,
                    PenaltyWaived = request.WaivePenalty,
                    PenaltyWaiverReason = request.WaiverReason,
                    SecurityDepositAmount = financials.SecurityDepositBalance,
                    DepositDeductions = financials.DepositDeductions,
                    DepositRefundAmount = financials.DepositRefundAmount,
                    SettlementAmount = financials.NetAmountDue,
                    HandoverInspectionRequired = true,
                    FinalInvoiceRequired = financials.TotalOutstanding > 0,
                    RequiresApproval = true
                };

                await Db.Insertable(termination).ExecuteCommandAsync();

                // Release boulevard shops back to Available (status 1)
                var contractUnits = await Db.Queryable<ErpContractUnit>()
                    .Where(u => u.ContractId == request.ContractId && u.BlvShopId != null && u.BlvShopId > 0)
                    .ToListAsync();
                foreach (var unit in contractUnits)
                {
                    await Db.Updateable<BlvBoulevardShops>()
                        .SetColumns(s => s.ShopStatus == 1) // 1 = Available
                        .SetColumns(s => s.Update_time == DateTime.Now)
                        .Where(s => s.Id == unit.BlvShopId.Value)
                        .ExecuteCommandAsync();
                }

                // Deactivate all units when contract is terminated/expired
                await Db.Updateable<ErpContractUnit>()
                    .SetColumns(u => u.IsActive == false)
                    .SetColumns(u => u.Update_time == DateTime.Now)
                    .Where(u => u.ContractId == request.ContractId && u.IsActive)
                    .ExecuteCommandAsync();

                // Cancel future billing schedules
                var cancelledCount = await CancelFutureBillingSchedulesAsync(
                    request.ContractId,
                    request.TerminationDate,
                    request.UserName);

                await Db.Ado.CommitTranAsync();

                result.Success = true;
                result.NewStatus = contract.Status;
                result.EffectiveDate = request.TerminationDate;
                result.TotalPenalty = financials.EarlyTerminationPenalty;
                result.TotalRefund = financials.PrepaidRentRefund;
                result.DepositRefund = financials.DepositRefundAmount;
                result.CancelledSchedules = cancelledCount;
                result.Message = "Contract terminated successfully";
            }
            catch (Exception ex)
            {
                await Db.Ado.RollbackTranAsync();
                result.Success = false;
                result.Message = $"Termination failed: {ex.Message}";
            }

            return result;
        }

        public async Task<TerminationFinancials> CalculateTerminationFinancialsAsync(
            Guid contractId,
            DateTime terminationDate,
            TerminationType terminationType)
        {
            var contract = await Db.Queryable<ErpContract>()
                .Where(c => c.ContractId == contractId)
                .FirstAsync();

            var financial = await Db.Queryable<ErpContractFinancial>()
                .Where(f => f.ContractId == contractId)
                .FirstAsync();

            var result = new TerminationFinancials
            {
                ContractId = contractId,
                TerminationDate = terminationDate,
                OriginalEndDate = contract.EndDate ?? DateTime.Now,
                Currency = financial?.Currency ?? "SAR",
                LineItems = new List<FinancialLineItem>()
            };

            if (contract.EndDate.HasValue)
            {
                result.RemainingDays = Math.Max(0, (int)(contract.EndDate.Value - terminationDate).TotalDays);
                result.RemainingMonths = Math.Max(0, (int)Math.Ceiling(result.RemainingDays / 30.0));
            }

            // Get outstanding amounts
            var outstandingSchedules = await Db.Queryable<ErpContractBillingSchedule>()
                .Where(s => s.ContractId == contractId
                    && s.DueDate <= terminationDate
                    && s.Status == BillingScheduleStatus.Due)
                .ToListAsync();

            result.OutstandingRent = outstandingSchedules.Sum(s => s.TotalAmount);
            result.TotalOutstanding = result.OutstandingRent;

            if (result.OutstandingRent > 0)
            {
                result.LineItems.Add(new FinancialLineItem
                {
                    Description = "Outstanding Rent",
                    DescriptionAr = "الإيجار المستحق",
                    Category = "Outstanding",
                    Amount = result.OutstandingRent,
                    IsCredit = false
                });
            }

            // Calculate early termination penalty
            if (terminationType == TerminationType.Early)
            {
                var penaltyMonths = 2;
                var monthlyRent = financial?.BaseAmount ?? 0;
                result.PenaltyMonths = penaltyMonths;
                result.PenaltyRate = 100;
                result.EarlyTerminationPenalty = monthlyRent * penaltyMonths;

                if (result.EarlyTerminationPenalty > 0)
                {
                    result.LineItems.Add(new FinancialLineItem
                    {
                        Description = $"Early Termination Penalty ({penaltyMonths} months)",
                        DescriptionAr = $"غرامة الإنهاء المبكر ({penaltyMonths} أشهر)",
                        Category = "Penalty",
                        Amount = result.EarlyTerminationPenalty,
                        IsCredit = false
                    });
                }
            }

            // Deposit settlement
            var depositSettlement = await CalculateDepositSettlementAsync(contractId, terminationDate);
            result.SecurityDepositBalance = depositSettlement.TotalDeposit;
            result.DepositDeductions = depositSettlement.TotalDeductions;
            result.DepositRefundAmount = depositSettlement.RefundAmount;

            if (result.DepositRefundAmount > 0)
            {
                result.LineItems.Add(new FinancialLineItem
                {
                    Description = "Security Deposit Refund",
                    DescriptionAr = "استرداد الضمان",
                    Category = "Deposit",
                    Amount = result.DepositRefundAmount,
                    IsCredit = true
                });
            }

            var totalOwed = result.TotalOutstanding + result.EarlyTerminationPenalty;
            var totalRefund = result.PrepaidRentRefund + result.ProRatedRefund + result.DepositRefundAmount;
            result.NetAmountDue = totalOwed - totalRefund;

            return result;
        }

        public async Task<TerminationResult> ProcessTerminationAsync(Guid contractId, string userName)
        {
            var termination = await Db.Queryable<ErpContractTermination>()
                .Where(t => t.ContractId == contractId)
                .OrderByDescending(t => t.Create_time)
                .FirstAsync();

            if (termination == null)
            {
                return new TerminationResult
                {
                    Success = false,
                    Message = "Termination not initiated."
                };
            }

            var contract = await Db.Queryable<ErpContract>()
                .Where(c => c.ContractId == contractId)
                .FirstAsync();

            if (contract == null)
            {
                return new TerminationResult
                {
                    Success = false,
                    Message = "Contract not found."
                };
            }

            try
            {
                await Db.Ado.BeginTranAsync();

                termination.Status = TerminationStatus.Completed;
                termination.SettlementStatus = 2;
                termination.SettlementDate = DateTime.Now;
                await Db.Updateable(termination).ExecuteCommandAsync();

                contract.Status = ContractStatus.Cancelled;
                await Db.Updateable(contract).ExecuteCommandAsync();

                // Deactivate all units when contract termination is finalized (safe if no units exist)
                await Db.Updateable<ErpContractUnit>()
                    .SetColumns(u => u.IsActive == false)
                    .SetColumns(u => u.Update_time == DateTime.Now)
                    .Where(u => u.ContractId == contractId && u.IsActive)
                    .ExecuteCommandAsync();

                await Db.Ado.CommitTranAsync();

                return new TerminationResult
                {
                    Success = true,
                    NewStatus = contract.Status,
                    EffectiveDate = termination.EffectiveDate,
                    Message = "Contract termination finalized"
                };
            }
            catch (Exception ex)
            {
                await Db.Ado.RollbackTranAsync();
                return new TerminationResult
                {
                    Success = false,
                    Message = $"Termination finalization failed: {ex.Message}"
                };
            }
        }

        public async Task<DepositSettlement> CalculateDepositSettlementAsync(Guid contractId, DateTime terminationDate)
        {
            var financial = await Db.Queryable<ErpContractFinancial>()
                .Where(f => f.ContractId == contractId)
                .FirstAsync();

            return new DepositSettlement
            {
                ContractId = contractId,
                OriginalDeposit = financial?.SecurityDeposit ?? 0,
                TotalDeposit = financial?.SecurityDeposit ?? 0,
                TotalDeductions = 0,
                RefundAmount = financial?.SecurityDeposit ?? 0,
                Currency = financial?.Currency ?? "SAR",
                Deductions = new List<DepositDeduction>()
            };
        }

        public async Task<TerminationChecklist> GetTerminationChecklistAsync(Guid contractId)
        {
            var checklist = new TerminationChecklist
            {
                ContractId = contractId,
                Items = new List<ChecklistItem>
                {
                    new() { ItemCode = "KEYS", Description = "Return all keys", DescriptionAr = "إرجاع جميع المفاتيح", IsRequired = true },
                    new() { ItemCode = "INSPECTION", Description = "Complete unit inspection", DescriptionAr = "إكمال فحص الوحدة", IsRequired = true },
                    new() { ItemCode = "UTILITIES", Description = "Clear utility balances", DescriptionAr = "تصفية فواتير المرافق", IsRequired = true },
                    new() { ItemCode = "OUTSTANDING", Description = "Clear outstanding invoices", DescriptionAr = "تصفية الفواتير المستحقة", IsRequired = true },
                    new() { ItemCode = "DEPOSIT", Description = "Process deposit settlement", DescriptionAr = "معالجة تسوية الضمان", IsRequired = true },
                    new() { ItemCode = "HANDOVER", Description = "Complete handover form", DescriptionAr = "إكمال نموذج التسليم", IsRequired = true }
                }
            };

            var requiredItems = checklist.Items.Where(i => i.IsRequired).ToList();
            var completedRequired = requiredItems.Count(i => i.IsComplete);
            checklist.AllItemsComplete = completedRequired == requiredItems.Count;
            checklist.CompletionPercentage = requiredItems.Count > 0
                ? (decimal)completedRequired / requiredItems.Count * 100
                : 100;

            return await Task.FromResult(checklist);
        }

        #endregion

        #region Contract Amendment

        public async Task<AmendmentResult> CreateAmendmentAsync(AmendmentRequest request)
        {
            var result = new AmendmentResult();

            try
            {
                var contract = await Db.Queryable<ErpContract>()
                    .Where(c => c.ContractId == request.ContractId)
                    .FirstAsync();

                if (contract == null)
                {
                    result.Success = false;
                    result.Message = "Contract not found";
                    return result;
                }

                result.Impact = await CalculateAmendmentImpactAsync(request.ContractId, request);

                var maxNumber = await Db.Queryable<ErpContractAmendment>()
                    .Where(a => a.ContractId == request.ContractId)
                    .MaxAsync(a => (int?)a.AmendmentNumber) ?? 0;

                var amendment = new ErpContractAmendment
                {
                    AmendmentId = Guid.NewGuid(),
                    ContractId = request.ContractId,
                    AmendmentNumber = maxNumber + 1,
                    AmendmentReference = $"{contract.ContractNumber}-AMD-{(maxNumber + 1):D3}",
                    AmendmentType = request.AmendmentType,
                    EffectiveDate = request.EffectiveDate,
                    Status = request.RequiresApproval ? AmendmentStatus.Draft : AmendmentStatus.PendingApproval,
                    Description = request.Description,
                    ChangesJson = System.Text.Json.JsonSerializer.Serialize(request.Changes),
                    RequiresApproval = request.RequiresApproval,
                    RequestedBy = request.UserName,
                    RequestedDate = DateTime.Now
                };

                await Db.Insertable(amendment).ExecuteCommandAsync();

                result.Success = true;
                result.AmendmentId = amendment.AmendmentId;
                result.AmendmentNumber = amendment.AmendmentReference;
                result.Status = amendment.Status;
                result.EffectiveDate = request.EffectiveDate;
                result.Message = "Amendment created successfully";
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Failed to create amendment: {ex.Message}";
            }

            return result;
        }

        public async Task<AmendmentResult> ApplyAmendmentAsync(Guid amendmentId, string userName)
        {
            var result = new AmendmentResult();

            try
            {
                var amendment = await Db.Queryable<ErpContractAmendment>()
                    .Where(a => a.AmendmentId == amendmentId)
                    .FirstAsync();

                if (amendment == null)
                {
                    result.Success = false;
                    result.Message = "Amendment not found";
                    return result;
                }

                if (amendment.Status != AmendmentStatus.Approved)
                {
                    result.Success = false;
                    result.Message = "Amendment must be approved before applying";
                    return result;
                }

                await Db.Ado.BeginTranAsync();

                await ApplyAmendmentChangesAsync(amendment, userName);

                amendment.AppliedDate = DateTime.Now;
                amendment.AppliedBy = userName;
                await Db.Updateable(amendment).ExecuteCommandAsync();

                await Db.Ado.CommitTranAsync();

                result.Success = true;
                result.AmendmentId = amendmentId;
                result.Status = amendment.Status;
                result.EffectiveDate = amendment.EffectiveDate;
                result.Message = "Amendment applied successfully";
            }
            catch (Exception ex)
            {
                await Db.Ado.RollbackTranAsync();
                result.Success = false;
                result.Message = $"Failed to apply amendment: {ex.Message}";
            }

            return result;
        }

        public async Task<AmendmentResult> ReverseAmendmentAsync(Guid amendmentId, string userName, string reason)
        {
            var result = new AmendmentResult();

            try
            {
                var amendment = await Db.Queryable<ErpContractAmendment>()
                    .Where(a => a.AmendmentId == amendmentId)
                    .FirstAsync();

                if (amendment == null)
                {
                    result.Success = false;
                    result.Message = "Amendment not found";
                    return result;
                }

                if (amendment.AppliedDate == null)
                {
                    result.Success = false;
                    result.Message = "Amendment has not been applied";
                    return result;
                }

                var daysSinceApplied = (DateTime.Now - amendment.AppliedDate.Value).TotalDays;
                if (daysSinceApplied > 30)
                {
                    result.Success = false;
                    result.Message = "Amendment can only be reversed within 30 days of application";
                    return result;
                }

                amendment.Status = AmendmentStatus.Cancelled;
                amendment.Reverted = true;
                amendment.RevertedDate = DateTime.Now;
                amendment.RevertedBy = userName;
                amendment.RevertReason = reason;

                await Db.Updateable(amendment).ExecuteCommandAsync();

                result.Success = true;
                result.AmendmentId = amendmentId;
                result.Status = AmendmentStatus.Cancelled;
                result.Message = "Amendment reversed successfully";
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Failed to reverse amendment: {ex.Message}";
            }

            return result;
        }

        public async Task<AmendmentImpact> CalculateAmendmentImpactAsync(Guid contractId, AmendmentRequest request)
        {
            var financial = await Db.Queryable<ErpContractFinancial>()
                .Where(f => f.ContractId == contractId)
                .FirstAsync();

            var impact = new AmendmentImpact
            {
                ContractId = contractId,
                AmendmentType = request.AmendmentType,
                CurrentValue = financial?.BaseAmount ?? 0,
                Currency = financial?.Currency ?? "SAR"
            };

            if (request.AmendmentType == AmendmentType.PriceChange)
            {
                impact.NewValue = request.NewRentAmount ?? impact.CurrentValue;
                impact.ValueChange = impact.NewValue - impact.CurrentValue;
                impact.ValueChangePercentage = impact.CurrentValue > 0
                    ? (impact.ValueChange / impact.CurrentValue) * 100
                    : 0;
                impact.RequiresBillingRegeneration = true;

                impact.AffectedBillingPeriods = await Db.Queryable<ErpContractBillingSchedule>()
                    .Where(s => s.ContractId == contractId
                        && s.PeriodStart >= request.EffectiveDate
                        && s.Status == BillingScheduleStatus.Pending)
                    .CountAsync();

                impact.SchedulesToAdjust = impact.AffectedBillingPeriods;
                impact.BillingAdjustmentAmount = impact.ValueChange * impact.AffectedBillingPeriods;
            }

            return impact;
        }

        public async Task<List<AmendmentHistory>> GetAmendmentHistoryAsync(Guid contractId)
        {
            var amendments = await Db.Queryable<ErpContractAmendment>()
                .Where(a => a.ContractId == contractId)
                .OrderByDescending(a => a.Create_time)
                .ToListAsync();

            return amendments.Select(a => new AmendmentHistory
            {
                AmendmentId = a.AmendmentId,
                AmendmentNumber = a.AmendmentReference,
                AmendmentType = a.AmendmentType,
                Status = a.Status,
                CreatedDate = a.Create_time,
                EffectiveDate = a.EffectiveDate,
                AppliedDate = a.AppliedDate,
                CreatedBy = a.Create_by,
                ApprovedBy = a.ApprovedBy,
                Description = a.Description,
                ChangeSummary = a.Description
            }).ToList();
        }

        #endregion

        #region Contract Suspension

        public async Task<SuspensionResult> SuspendContractAsync(SuspensionRequest request)
        {
            var result = new SuspensionResult();

            try
            {
                var contract = await Db.Queryable<ErpContract>()
                    .Where(c => c.ContractId == request.ContractId)
                    .FirstAsync();

                if (contract == null)
                {
                    result.Success = false;
                    result.Message = "Contract not found";
                    return result;
                }

                if (contract.Status != ContractStatus.Active)
                {
                    result.Success = false;
                    result.Message = "Only active contracts can be suspended";
                    return result;
                }

                await Db.Ado.BeginTranAsync();

                contract.Status = ContractStatus.Suspended;
                await Db.Updateable(contract).ExecuteCommandAsync();

                // Deactivate all units when contract is suspended
                await Db.Updateable<ErpContractUnit>()
                    .SetColumns(u => u.IsActive == false)
                    .SetColumns(u => u.Update_time == DateTime.Now)
                    .Where(u => u.ContractId == request.ContractId && u.IsActive)
                    .ExecuteCommandAsync();

                int affectedSchedules = 0;
                if (request.SuspendBilling)
                {
                    affectedSchedules = await Db.Updateable<ErpContractBillingSchedule>()
                        .SetColumns(s => new ErpContractBillingSchedule { Status = BillingScheduleStatus.Skipped })
                        .Where(s => s.ContractId == request.ContractId
                            && s.PeriodStart >= request.SuspensionDate
                            && (s.Status == BillingScheduleStatus.Pending || s.Status == BillingScheduleStatus.Due))
                        .ExecuteCommandAsync();
                }

                await Db.Ado.CommitTranAsync();

                result.Success = true;
                result.NewStatus = ContractStatus.Suspended;
                result.SuspensionStart = request.SuspensionDate;
                result.SuspensionEnd = request.ExpectedResumeDate;
                result.AffectedBillingSchedules = affectedSchedules;
                result.Message = "Contract suspended successfully";
            }
            catch (Exception ex)
            {
                await Db.Ado.RollbackTranAsync();
                result.Success = false;
                result.Message = $"Suspension failed: {ex.Message}";
            }

            return result;
        }

        public async Task<SuspensionResult> ResumeContractAsync(Guid contractId, string userName, DateTime? resumeDate = null)
        {
            var result = new SuspensionResult();

            try
            {
                var contract = await Db.Queryable<ErpContract>()
                    .Where(c => c.ContractId == contractId)
                    .FirstAsync();

                if (contract == null)
                {
                    result.Success = false;
                    result.Message = "Contract not found";
                    return result;
                }

                if (contract.Status != ContractStatus.Suspended)
                {
                    result.Success = false;
                    result.Message = "Contract is not suspended";
                    return result;
                }

                await Db.Ado.BeginTranAsync();

                contract.Status = ContractStatus.Active;
                await Db.Updateable(contract).ExecuteCommandAsync();

                // Reactivate all units when contract is resumed
                await Db.Updateable<ErpContractUnit>()
                    .SetColumns(u => u.IsActive == true)
                    .SetColumns(u => u.Update_time == DateTime.Now)
                    .Where(u => u.ContractId == contractId && !u.IsActive)
                    .ExecuteCommandAsync();

                var reactivatedCount = await Db.Updateable<ErpContractBillingSchedule>()
                    .SetColumns(s => new ErpContractBillingSchedule { Status = BillingScheduleStatus.Pending })
                    .Where(s => s.ContractId == contractId && s.Status == BillingScheduleStatus.Skipped)
                    .ExecuteCommandAsync();

                await Db.Ado.CommitTranAsync();

                result.Success = true;
                result.NewStatus = ContractStatus.Active;
                result.SuspensionEnd = resumeDate ?? DateTime.Now;
                result.AffectedBillingSchedules = reactivatedCount;
                result.Message = "Contract resumed successfully";
            }
            catch (Exception ex)
            {
                await Db.Ado.RollbackTranAsync();
                result.Success = false;
                result.Message = $"Resume failed: {ex.Message}";
            }

            return result;
        }

        public async Task<SuspensionImpact> CalculateSuspensionImpactAsync(
            Guid contractId,
            DateTime suspensionStart,
            DateTime? suspensionEnd)
        {
            var contract = await Db.Queryable<ErpContract>()
                .Where(c => c.ContractId == contractId)
                .FirstAsync();

            var impact = new SuspensionImpact
            {
                ContractId = contractId,
                SuspensionStart = suspensionStart,
                SuspensionEnd = suspensionEnd,
                CurrentEndDate = contract.EndDate ?? DateTime.Now
            };

            impact.SuspensionDays = suspensionEnd.HasValue
                ? (int)(suspensionEnd.Value - suspensionStart).TotalDays
                : 0;

            impact.NewEndDate = impact.CurrentEndDate.AddDays(impact.SuspensionDays);
            impact.ExtendedDays = impact.SuspensionDays;

            var affectedSchedules = await Db.Queryable<ErpContractBillingSchedule>()
                .Where(s => s.ContractId == contractId
                    && s.PeriodStart >= suspensionStart
                    && (suspensionEnd == null || s.PeriodStart <= suspensionEnd)
                    && (s.Status == BillingScheduleStatus.Pending || s.Status == BillingScheduleStatus.Due))
                .ToListAsync();

            impact.AffectedSchedules = affectedSchedules.Count;
            impact.SuspendedBillingAmount = affectedSchedules.Sum(s => s.TotalAmount);
            impact.SchedulesToDefer = affectedSchedules.Count;

            return impact;
        }

        #endregion

        #region Security Deposit Management

        public async Task<DepositResult> RecordDepositAsync(DepositRequest request)
        {
            var result = new DepositResult();

            try
            {
                var financial = await Db.Queryable<ErpContractFinancial>()
                    .Where(f => f.ContractId == request.ContractId)
                    .FirstAsync();

                if (financial == null)
                {
                    financial = new ErpContractFinancial
                    {
                        ContractId = request.ContractId,
                        SecurityDeposit = request.Amount,
                        Currency = "SAR"
                    };
                    await Db.Insertable(financial).ExecuteCommandAsync();
                }
                else
                {
                    financial.SecurityDeposit = (financial.SecurityDeposit ?? 0) + request.Amount;
                    await Db.Updateable(financial).ExecuteCommandAsync();
                }

                result.Success = true;
                result.TotalDepositBalance = financial.SecurityDeposit ?? 0;
                result.Message = "Deposit recorded successfully";
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Failed to record deposit: {ex.Message}";
            }

            return result;
        }

        public async Task<DepositRefundResult> ProcessDepositRefundAsync(Guid contractId, DepositRefundRequest request)
        {
            var result = new DepositRefundResult();

            try
            {
                var financial = await Db.Queryable<ErpContractFinancial>()
                    .Where(f => f.ContractId == contractId)
                    .FirstAsync();

                var currentBalance = financial?.SecurityDeposit ?? 0;

                if (request.RefundAmount > currentBalance)
                {
                    result.Success = false;
                    result.Message = "Refund amount exceeds deposit balance";
                    return result;
                }

                financial.SecurityDeposit = currentBalance - request.RefundAmount;
                await Db.Updateable(financial).ExecuteCommandAsync();

                result.Success = true;
                result.RefundedAmount = request.RefundAmount;
                result.RemainingBalance = financial.SecurityDeposit ?? 0;
                result.RefundReference = request.RefundReference;
                result.Message = "Deposit refund processed successfully";
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Failed to process refund: {ex.Message}";
            }

            return result;
        }

        public async Task<DepositDeductionResult> ApplyDepositDeductionAsync(Guid contractId, DepositDeductionRequest request)
        {
            var result = new DepositDeductionResult();

            try
            {
                var financial = await Db.Queryable<ErpContractFinancial>()
                    .Where(f => f.ContractId == contractId)
                    .FirstAsync();

                var currentBalance = financial?.SecurityDeposit ?? 0;

                if (request.DeductionAmount > currentBalance)
                {
                    result.Success = false;
                    result.Message = "Deduction amount exceeds deposit balance";
                    return result;
                }

                financial.SecurityDeposit = currentBalance - request.DeductionAmount;
                await Db.Updateable(financial).ExecuteCommandAsync();

                result.Success = true;
                result.DeductedAmount = request.DeductionAmount;
                result.RemainingBalance = financial.SecurityDeposit ?? 0;
                result.Message = "Deposit deduction applied successfully";
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Failed to apply deduction: {ex.Message}";
            }

            return result;
        }

        public async Task<DepositBalance> GetDepositBalanceAsync(Guid contractId)
        {
            var financial = await Db.Queryable<ErpContractFinancial>()
                .Where(f => f.ContractId == contractId)
                .FirstAsync();

            return new DepositBalance
            {
                ContractId = contractId,
                TotalDeposits = financial?.SecurityDeposit ?? 0,
                CurrentBalance = financial?.SecurityDeposit ?? 0,
                Currency = financial?.Currency ?? "SAR",
                Transactions = new List<DepositTransaction>()
            };
        }

        #endregion

        #region Private Helper Methods

        private async Task<(bool IsEligible, List<string> BlockingReasons)> ValidateRenewalEligibilityAsync(ErpContract contract)
        {
            var reasons = new List<string>();

            if (contract.Status != ContractStatus.Active)
                reasons.Add($"Contract status is {contract.Status}, must be Active for renewal");

            var outstanding = await GetOutstandingAmountAsync(contract.ContractId);
            if (outstanding > 0)
                reasons.Add($"Contract has outstanding balance of {outstanding:N2}");

            var pendingAmendments = await Db.Queryable<ErpContractAmendment>()
                .Where(a => a.ContractId == contract.ContractId
                    && (a.Status == AmendmentStatus.Draft || a.Status == AmendmentStatus.PendingApproval))
                .CountAsync();

            if (pendingAmendments > 0)
                reasons.Add($"Contract has {pendingAmendments} pending amendment(s)");

            return (reasons.Count == 0, reasons);
        }

        private async Task<decimal> GetOutstandingAmountAsync(Guid contractId)
        {
            var outstandingSchedules = await Db.Queryable<ErpContractBillingSchedule>()
                .Where(s => s.ContractId == contractId && s.Status == BillingScheduleStatus.Due)
                .ToListAsync();

            return outstandingSchedules.Sum(s => s.TotalAmount);
        }

        private string GenerateRenewalNumber(string contractNumber, int sequence)
        {
            return $"REN-{contractNumber}-{sequence:D3}";
        }

        private int GetRenewalMonths(RenewalType renewalType)
        {
            return renewalType switch
            {
                RenewalType.Monthly => 1,
                RenewalType.Quarterly => 3,
                RenewalType.SemiAnnual => 6,
                RenewalType.Yearly => 12,
                _ => 12
            };
        }

        private int CalculateBillingPeriods(int durationMonths, BillingFrequency frequency)
        {
            return frequency switch
            {
                BillingFrequency.Monthly => durationMonths,
                BillingFrequency.Quarterly => (int)Math.Ceiling(durationMonths / 3.0),
                BillingFrequency.SemiAnnual => (int)Math.Ceiling(durationMonths / 6.0),
                BillingFrequency.Yearly => (int)Math.Ceiling(durationMonths / 12.0),
                BillingFrequency.Weekly => durationMonths * 4,
                BillingFrequency.BiWeekly => durationMonths * 2,
                BillingFrequency.OneTime => 1,
                _ => durationMonths
            };
        }

        private async Task<int> CancelFutureBillingSchedulesAsync(Guid contractId, DateTime fromDate, string userName)
        {
            return await Db.Updateable<ErpContractBillingSchedule>()
                .SetColumns(s => new ErpContractBillingSchedule { Status = BillingScheduleStatus.Cancelled })
                .Where(s => s.ContractId == contractId
                    && s.PeriodStart > fromDate
                    && (s.Status == BillingScheduleStatus.Pending || s.Status == BillingScheduleStatus.Due))
                .ExecuteCommandAsync();
        }

        private TerminationReason ConvertToTerminationReason(string reason)
        {
            if (string.IsNullOrEmpty(reason)) return TerminationReason.Other;
            return Enum.TryParse<TerminationReason>(reason, true, out var result) ? result : TerminationReason.Other;
        }

        private async Task ApplyAmendmentChangesAsync(ErpContractAmendment amendment, string userName)
        {
            if (string.IsNullOrEmpty(amendment.ChangesJson)) return;

            var changes = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(amendment.ChangesJson);
            if (changes == null) return;

            var contract = await Db.Queryable<ErpContract>()
                .Where(c => c.ContractId == amendment.ContractId)
                .FirstAsync();

            switch (amendment.AmendmentType)
            {
                case AmendmentType.PriceChange:
                    if (changes.TryGetValue("new_base_amount", out var newAmount))
                    {
                        var financial = await Db.Queryable<ErpContractFinancial>()
                            .Where(f => f.ContractId == amendment.ContractId)
                            .FirstAsync();

                        if (financial != null && decimal.TryParse(newAmount?.ToString(), out var amount))
                        {
                            financial.BaseAmount = amount;
                            await Db.Updateable(financial).ExecuteCommandAsync();
                        }
                    }
                    break;

                case AmendmentType.DurationExtension:
                case AmendmentType.DurationReduction:
                    if (changes.TryGetValue("new_end_date", out var newEndDate))
                    {
                        if (DateTime.TryParse(newEndDate?.ToString(), out var endDate))
                        {
                            contract.EndDate = endDate;
                            await Db.Updateable(contract).ExecuteCommandAsync();
                        }
                    }
                    break;
            }
        }

        #endregion
    }
}
