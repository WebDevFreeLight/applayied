using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.Attribute;
using Mapster;
using ZR.Model;
using ZR.Model.Contracts;
using ZR.Model.Contracts.Dto;
using ZR.Model.Contracts.Enums;
using ZR.Repository.Contracts;
using ZR.Service.Contracts.IService;
using ZR.Model.Boulevard;
using ZR.Model.Customers;
using ZR.Model.Sales;
using ZR.ServiceCore;

namespace ZR.Service.Contracts
{
    /// <summary>
    /// Contract Service - خدمة العقود
    /// Business logic layer for contract operations
    /// Updated to follow Finance module pattern with BaseService and DI
    /// </summary>
    [AppService(ServiceType = typeof(IContractService), ServiceLifetime = LifeTime.Transient)]
    public class ContractService : BaseService<ErpContract>, IContractService
    {
        private readonly IContractRepository _contractRepository;
        private readonly IContractUnitRepository _unitRepository;
        private readonly IContractPricingRuleRepository _pricingRuleRepository;
        private readonly IContractBillingScheduleRepository _billingScheduleRepository;

        public ContractService(
            IContractRepository contractRepository,
            IContractUnitRepository unitRepository,
            IContractPricingRuleRepository pricingRuleRepository,
            IContractBillingScheduleRepository billingScheduleRepository)
        {
            _contractRepository = contractRepository;
            _unitRepository = unitRepository;
            _pricingRuleRepository = pricingRuleRepository;
            _billingScheduleRepository = billingScheduleRepository;
        }

        /// <summary>
        /// Get contracts list with pagination
        /// </summary>
        public PagedInfo<ErpContractListDto> GetList(ErpContractQueryDto query, PagerInfo pager)
        {
            return _contractRepository.GetList(query, pager);
        }

        /// <summary>
        /// Get contract by ID with full details
        /// </summary>
        public ErpContractDetailDto GetById(Guid contractId)
        {
            var contract = _contractRepository.GetById(contractId);
            if (contract == null) return null;

            // Load related data
            contract.Units = _unitRepository.GetByContractId(contractId);
            contract.PricingRules = _pricingRuleRepository.GetByContractId(contractId);
            contract.UpcomingBillingSchedules = _billingScheduleRepository.GetUpcomingSchedules(contractId);
            contract.TotalUnits = contract.Units?.Count(u => u.IsActive) ?? 0;

            // Calculate statistics
            var scheduleStats = _billingScheduleRepository.GetStatistics(contractId);
            contract.TotalInvoicedAmount = scheduleStats.TotalAmount - scheduleStats.PendingAmount;
            contract.TotalPaidAmount = scheduleStats.PaidAmount;
            contract.OutstandingAmount = scheduleStats.TotalAmount - scheduleStats.PaidAmount;
            contract.PendingInvoicesCount = scheduleStats.DueCount + scheduleStats.InvoicedCount;

            return contract;
        }

        /// <summary>
        /// Create contract
        /// Creates contract along with extension records (Financial, BillingConfig, Lifecycle)
        /// </summary>
        public Guid Create(CreateErpContractDto dto, string createdBy)
        {
            // Map to core contract entity (only core fields)
            var contract = new ErpContract
            {
                CustomerId = dto.CustomerId,
                ContractType = dto.ContractType,
                ContractTitle = dto.ContractTitle,
                ContractTitleAr = dto.ContractTitleAr,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                AutoRenew = dto.AutoRenew,
                RenewalType = dto.RenewalType,
                RenewalNoticeDays = dto.RenewalNoticeDays,
                ExternalReference = dto.ExternalReference,
                Description = dto.Description,
                TermsConditions = dto.TermsConditions,
                Notes = dto.Notes,
                Status = ContractStatus.Draft,
                Create_by = createdBy,
                CompanyID = dto.CompanyId,
                BranchID = dto.BranchId
            };

            var contractId = _contractRepository.Create(contract);

            // Create Financial extension record using Context from BaseService
            var financial = new ErpContractFinancial
            {
                ContractId = contractId,
                BaseAmount = dto.BaseAmount,
                TotalAmount = dto.TotalAmount,
                Currency = dto.Currency ?? "SAR",
                PaymentTerms = dto.PaymentTerms,
                PaymentDueDays = dto.PaymentDueDays ?? 30,
                TaxGroupId = dto.TaxGroupId,
                IsTaxInclusive = dto.IsTaxInclusive,
                SecurityDeposit = dto.SecurityDeposit,
                Create_time = DateTime.Now
            };
            Context.Insertable(financial).ExecuteCommand();

            // Create BillingConfig extension record
            var billingConfig = new ErpContractBillingConfig
            {
                ContractId = contractId,
                BillingFrequency = dto.BillingFrequency,
                BillingDay = dto.BillingDay ?? 1,
                IsBillingVariable = dto.IsBillingVariable,
                NextBillingDate = CalculateNextBillingDate(dto.StartDate, dto.BillingFrequency, dto.BillingDay),
                Create_time = DateTime.Now
            };
            Context.Insertable(billingConfig).ExecuteCommand();

            // Create Lifecycle extension record
            var lifecycle = new ErpContractLifecycle
            {
                ContractId = contractId,
                SignedDate = dto.SignedDate,
                SignatoryName = dto.SignatoryName,
                Create_time = DateTime.Now
            };
            Context.Insertable(lifecycle).ExecuteCommand();

            return contractId;
        }

        /// <summary>
        /// Update contract
        /// Updates contract and all extension records (Financial, BillingConfig, Lifecycle)
        /// For Active contracts with financial changes: auto-creates amendment, regenerates pending schedules
        /// </summary>
        public ContractUpdateResultDto Update(UpdateErpContractDto dto, string updatedBy)
        {
            var existing = _contractRepository.GetEntityById(dto.ContractId);
            if (existing == null) return new ContractUpdateResultDto { Success = false, Warning = "Contract not found" };

            // Only allow updates to draft or active contracts
            if (existing.Status != ContractStatus.Draft && existing.Status != ContractStatus.Active)
            {
                throw new InvalidOperationException("Can only update draft or active contracts");
            }

            // Load current financial values to detect changes
            var financial = Context.Queryable<ErpContractFinancial>().First(f => f.ContractId == dto.ContractId);
            var oldBaseAmount = financial?.BaseAmount;
            var oldTotalAmount = financial?.TotalAmount;
            bool financialChanged = existing.Status == ContractStatus.Active
                && (oldBaseAmount != dto.BaseAmount || oldTotalAmount != dto.TotalAmount);

            var resultDto = new ContractUpdateResultDto { Success = true };

            // If Active contract has financial changes, handle safeguards
            if (financialChanged)
            {
                // Count schedule statuses
                var schedules = Context.Queryable<ErpContractBillingSchedule>()
                    .Where(s => s.ContractId == dto.ContractId)
                    .ToList();

                int invoicedCount = schedules.Count(s => s.Status == BillingScheduleStatus.Invoiced);
                int paidCount = schedules.Count(s => s.Status == BillingScheduleStatus.Paid);
                int pendingCount = schedules.Count(s => s.Status == BillingScheduleStatus.Pending || s.Status == BillingScheduleStatus.Due);

                // Auto-create PriceChange amendment for audit trail
                var amendment = new ErpContractAmendment
                {
                    AmendmentId = Guid.NewGuid(),
                    ContractId = dto.ContractId,
                    AmendmentType = AmendmentType.PriceChange,
                    Status = AmendmentStatus.Approved,
                    ChangesJson = System.Text.Json.JsonSerializer.Serialize(new
                    {
                        base_amount = new { previous = oldBaseAmount, @new = dto.BaseAmount },
                        total_amount = new { previous = oldTotalAmount, @new = dto.TotalAmount }
                    }),
                    AmendmentReference = $"{existing.ContractNumber}-AMD-AUTO",
                    EffectiveDate = DateTime.Now,
                    RequestedDate = DateTime.Now,
                    ApprovedDate = DateTime.Now,
                    AppliedDate = DateTime.Now,
                    RequestedBy = updatedBy,
                    ApprovedBy = updatedBy,
                    AppliedBy = updatedBy,
                    RecalculateSchedules = true,
                    SchedulesRecalculated = true,
                    SchedulesAffectedCount = pendingCount,
                    ApplyToPendingOnly = true,
                    Currency = financial?.Currency ?? "SAR",
                    MonthlyImpact = (dto.BaseAmount ?? 0) - (oldBaseAmount ?? 0),
                    TotalImpact = (dto.TotalAmount ?? 0) - (oldTotalAmount ?? 0),
                    Create_by = updatedBy,
                    Create_time = DateTime.Now
                };
                Context.Insertable(amendment).ExecuteCommand();

                resultDto.AmendmentId = amendment.AmendmentId;
                resultDto.InvoicedSchedulesKept = invoicedCount + paidCount;
                resultDto.PendingSchedulesRegenerated = pendingCount;
                resultDto.Warning = $"Financial values changed on active contract. {invoicedCount + paidCount} invoiced/paid schedules kept unchanged. {pendingCount} pending schedules will be recalculated.";
            }

            // Update core contract fields
            existing.CustomerId = dto.CustomerId;
            existing.ContractType = dto.ContractType;
            existing.ContractTitle = dto.ContractTitle;
            existing.ContractTitleAr = dto.ContractTitleAr;
            existing.StartDate = dto.StartDate;
            existing.EndDate = dto.EndDate;
            existing.AutoRenew = dto.AutoRenew;
            existing.RenewalType = dto.RenewalType;
            existing.RenewalNoticeDays = dto.RenewalNoticeDays;
            existing.ExternalReference = dto.ExternalReference;
            existing.Description = dto.Description;
            existing.TermsConditions = dto.TermsConditions;
            existing.Notes = dto.Notes;
            existing.Update_by = updatedBy;

            _contractRepository.UpdateContract(existing);

            // Update Financial extension using Context from BaseService
            Context.Updateable<ErpContractFinancial>()
                .SetColumns(f => f.BaseAmount == dto.BaseAmount)
                .SetColumns(f => f.TotalAmount == dto.TotalAmount)
                .SetColumns(f => f.Currency == (dto.Currency ?? "SAR"))
                .SetColumns(f => f.PaymentTerms == dto.PaymentTerms)
                .SetColumns(f => f.PaymentDueDays == (dto.PaymentDueDays ?? 30))
                .SetColumns(f => f.TaxGroupId == dto.TaxGroupId)
                .SetColumns(f => f.IsTaxInclusive == dto.IsTaxInclusive)
                .SetColumns(f => f.SecurityDeposit == dto.SecurityDeposit)
                .SetColumns(f => f.Update_time == DateTime.Now)
                .Where(f => f.ContractId == dto.ContractId)
                .ExecuteCommand();

            // Update BillingConfig extension
            var nextBillingDate = CalculateNextBillingDate(dto.StartDate, dto.BillingFrequency, dto.BillingDay);
            Context.Updateable<ErpContractBillingConfig>()
                .SetColumns(b => b.BillingFrequency == dto.BillingFrequency)
                .SetColumns(b => b.BillingDay == (dto.BillingDay ?? 1))
                .SetColumns(b => b.IsBillingVariable == dto.IsBillingVariable)
                .SetColumns(b => b.NextBillingDate == nextBillingDate)
                .SetColumns(b => b.Update_time == DateTime.Now)
                .Where(b => b.ContractId == dto.ContractId)
                .ExecuteCommand();

            // Update Lifecycle extension
            Context.Updateable<ErpContractLifecycle>()
                .SetColumns(l => l.SignedDate == dto.SignedDate)
                .SetColumns(l => l.SignatoryName == dto.SignatoryName)
                .SetColumns(l => l.Update_time == DateTime.Now)
                .Where(l => l.ContractId == dto.ContractId)
                .ExecuteCommand();

            // If financial values changed on Active contract, regenerate pending/due schedules
            if (financialChanged)
            {
                // Delete only Pending and Due schedules (leave Invoiced/Paid/Cancelled/Skipped untouched)
                Context.Deleteable<ErpContractBillingSchedule>()
                    .Where(s => s.ContractId == dto.ContractId
                        && (s.Status == BillingScheduleStatus.Pending || s.Status == BillingScheduleStatus.Due))
                    .ExecuteCommand();

                // Regenerate ONLY for remaining periods (after kept schedules)
                RegeneratePendingSchedules(dto.ContractId, existing, updatedBy);
            }

            return resultDto;
        }

        /// <summary>
        /// Delete contracts (soft-delete + reset linked deal)
        /// </summary>
        public int Delete(Guid[] contractIds)
        {
            int totalAffected = 0;

            foreach (var id in contractIds)
            {
                var contract = _contractRepository.GetEntityById(id);
                if (contract == null) continue;

                if (contract.Status != ContractStatus.Draft
                    && contract.Status != ContractStatus.Cancelled
                    && contract.Status != ContractStatus.Terminated)
                {
                    throw new InvalidOperationException($"Cannot delete contract {contract.ContractNumber}. Only draft, cancelled, or terminated contracts can be deleted.");
                }

                // 1. Release shops back to Available (query WITHOUT soft-delete filter so terminated units are found)
                var contractUnits = Context.Queryable<ErpContractUnit>()
                    .ClearFilter()
                    .Where(u => u.ContractId == id && u.BlvShopId != null && u.BlvShopId > 0)
                    .ToList();
                foreach (var unit in contractUnits)
                {
                    Context.Updateable<BlvBoulevardShops>()
                        .SetColumns(s => s.ShopStatus == 1) // 1 = Available
                        .SetColumns(s => s.Update_time == DateTime.Now)
                        .Where(s => s.Id == unit.BlvShopId.Value)
                        .ExecuteCommand();
                }

                // 2. Soft-delete contract units (ISoftDelete)
                Context.Updateable<ErpContractUnit>()
                    .SetColumns(u => u.IsDeleted == true)
                    .SetColumns(u => u.DeletedAt == DateTime.Now)
                    .Where(u => u.ContractId == id)
                    .ExecuteCommand();

                // 3. Hard-delete config data (no ISoftDelete on these)
                _pricingRuleRepository.DeleteByContractId(id);
                _billingScheduleRepository.DeleteByContractId(id);

                // 4. Soft-delete the contract itself
                Context.Updateable<ErpContract>()
                    .SetColumns(c => c.IsDeleted == true)
                    .SetColumns(c => c.DeletedAt == DateTime.Now)
                    .Where(c => c.ContractId == id)
                    .ExecuteCommand();

                // 5. Reset linked deal back to "Under Negotiation"
                if (!string.IsNullOrEmpty(contract.ExternalReference)
                    && long.TryParse(contract.ExternalReference, out long dealId))
                {
                    Context.Updateable<SalesDeal>()
                        .SetColumns(d => d.DealStatus == "Under Negotiation")
                        .SetColumns(d => d.IsConverted == false)
                        .SetColumns(d => d.ActualCloseDate == null)
                        .SetColumns(d => d.WonReason == null)
                        .SetColumns(d => d.Update_time == DateTime.Now)
                        .Where(d => d.DealId == dealId)
                        .ExecuteCommand();

                    // 6. Reset deal units
                    Context.Updateable<SalesDealUnit>()
                        .SetColumns(du => du.IsConverted == false)
                        .SetColumns(du => du.Update_time == DateTime.Now)
                        .Where(du => du.DealId == dealId)
                        .ExecuteCommand();
                }

                totalAffected++;
            }

            return totalAffected;
        }

        /// <summary>
        /// Activate contract
        /// </summary>
        public int Activate(ActivateContractDto dto, string activatedBy)
        {
            var contract = _contractRepository.GetEntityById(dto.ContractId);
            if (contract == null) return 0;

            if (contract.Status != ContractStatus.Draft && contract.Status != ContractStatus.PendingApproval)
            {
                throw new InvalidOperationException("Only draft or pending approval contracts can be activated");
            }

            var result = _contractRepository.Activate(dto.ContractId, activatedBy, dto.ActivationDate);

            if (result > 0)
            {
                // Reserve boulevard shops linked to this contract's units
                var contractUnits = Context.Queryable<ErpContractUnit>()
                    .Where(u => u.ContractId == dto.ContractId && u.BlvShopId != null && u.BlvShopId > 0)
                    .ToList();
                foreach (var unit in contractUnits)
                {
                    Context.Updateable<BlvBoulevardShops>()
                        .SetColumns(s => s.ShopStatus == 2) // 2 = Reserved
                        .SetColumns(s => s.Update_time == DateTime.Now)
                        .Where(s => s.Id == unit.BlvShopId.Value)
                        .ExecuteCommand();
                }

                // Lock unit pricing on activation
                Context.Updateable<ErpContractUnit>()
                    .SetColumns(u => u.PricingLocked == true)
                    .Where(u => u.ContractId == dto.ContractId)
                    .ExecuteCommand();

                // Generate billing schedule if requested
                if (dto.GenerateBillingSchedule)
                {
                    // Clear any existing schedules first (from previous failed activation or draft testing)
                    Context.Deleteable<ErpContractBillingSchedule>()
                        .Where(s => s.ContractId == dto.ContractId)
                        .ExecuteCommand();

                    GenerateBillingSchedules(dto.ContractId, contract, activatedBy);
                }
            }

            return result;
        }

        /// <summary>
        /// Suspend contract
        /// </summary>
        public int Suspend(SuspendContractDto dto, string suspendedBy)
        {
            var contract = _contractRepository.GetEntityById(dto.ContractId);
            if (contract == null) return 0;

            if (contract.Status != ContractStatus.Active)
            {
                throw new InvalidOperationException("Only active contracts can be suspended");
            }

            var result = _contractRepository.UpdateStatus(dto.ContractId, ContractStatus.Suspended, suspendedBy);

            if (result > 0)
            {
                // Deactivate all units when contract is suspended (safe if no units exist)
                try { _unitRepository.DeactivateByContractId(dto.ContractId); }
                catch (Exception) { /* No units to deactivate - not an error */ }

                if (dto.CancelPendingSchedules)
                {
                    _billingScheduleRepository.CancelFutureSchedules(dto.ContractId, DateTime.Today, suspendedBy);
                }
            }

            return result;
        }

        /// <summary>
        /// Resume contract
        /// </summary>
        public int Resume(Guid contractId, string resumedBy)
        {
            var contract = _contractRepository.GetEntityById(contractId);
            if (contract == null) return 0;

            if (contract.Status != ContractStatus.Suspended)
            {
                throw new InvalidOperationException("Only suspended contracts can be resumed");
            }

            var result = _contractRepository.UpdateStatus(contractId, ContractStatus.Active, resumedBy);

            if (result > 0)
            {
                // Reactivate all units when contract is resumed (safe if no units exist)
                try { _unitRepository.ReactivateByContractId(contractId); }
                catch (Exception) { /* No units to reactivate - not an error */ }
            }

            return result;
        }

        /// <summary>
        /// Terminate contract
        /// </summary>
        public int Terminate(TerminateContractDto dto, string terminatedBy)
        {
            var contract = _contractRepository.GetEntityById(dto.ContractId);
            if (contract == null) return 0;

            if (contract.Status != ContractStatus.Active && contract.Status != ContractStatus.Suspended)
            {
                throw new InvalidOperationException("Only active or suspended contracts can be terminated");
            }

            var result = _contractRepository.Terminate(dto.ContractId, dto.TerminationReason, terminatedBy, dto.TerminationDate);

            if (result > 0)
            {
                // Release boulevard shops back to Available
                var contractUnits = Context.Queryable<ErpContractUnit>()
                    .Where(u => u.ContractId == dto.ContractId && u.BlvShopId != null && u.BlvShopId > 0)
                    .ToList();
                foreach (var unit in contractUnits)
                {
                    Context.Updateable<BlvBoulevardShops>()
                        .SetColumns(s => s.ShopStatus == 1) // 1 = Available
                        .SetColumns(s => s.Update_time == DateTime.Now)
                        .Where(s => s.Id == unit.BlvShopId.Value)
                        .ExecuteCommand();
                }

                // Deactivate all units when contract is terminated (safe if no units exist)
                try { _unitRepository.DeactivateByContractId(dto.ContractId); }
                catch (Exception) { /* No units to deactivate - not an error */ }

                if (dto.CancelFutureSchedules)
                {
                    _billingScheduleRepository.CancelFutureSchedules(dto.ContractId, dto.TerminationDate, terminatedBy);
                }
            }

            return result;
        }

        /// <summary>
        /// Cancel contract
        /// </summary>
        public int Cancel(Guid contractId, string cancelledBy, string reason)
        {
            var contract = _contractRepository.GetEntityById(contractId);
            if (contract == null) return 0;

            if (contract.Status != ContractStatus.Draft && contract.Status != ContractStatus.PendingApproval)
            {
                throw new InvalidOperationException("Only draft or pending approval contracts can be cancelled");
            }

            var result = _contractRepository.UpdateStatus(contractId, ContractStatus.Cancelled, cancelledBy);

            if (result > 0)
            {
                // Deactivate all units when contract is cancelled (safe if no units exist)
                try { _unitRepository.DeactivateByContractId(contractId); }
                catch (Exception) { /* No units to deactivate - not an error */ }
            }

            return result;
        }

        /// <summary>
        /// Renew contract
        /// Creates new contract with all extension records based on old contract
        /// </summary>
        public Guid Renew(Guid contractId, DateTime newEndDate, string renewedBy)
        {
            var oldContract = _contractRepository.GetEntityById(contractId);
            if (oldContract == null) throw new InvalidOperationException("Contract not found");

            if (oldContract.Status != ContractStatus.Active && oldContract.Status != ContractStatus.Expired)
            {
                throw new InvalidOperationException("Only active or expired contracts can be renewed");
            }

            // Load extension data from old contract using Context from BaseService
            var oldFinancial = Context.Queryable<ErpContractFinancial>().First(f => f.ContractId == contractId);
            var oldBillingConfig = Context.Queryable<ErpContractBillingConfig>().First(b => b.ContractId == contractId);

            // Mark old contract as renewed
            _contractRepository.UpdateStatus(contractId, ContractStatus.Renewed, renewedBy);

            // Create new contract based on old one
            var newContract = new ErpContract
            {
                CustomerId = oldContract.CustomerId,
                ContractType = oldContract.ContractType,
                ContractTitle = oldContract.ContractTitle,
                ContractTitleAr = oldContract.ContractTitleAr,
                StartDate = oldContract.EndDate?.AddDays(1) ?? DateTime.Today,
                EndDate = newEndDate,
                AutoRenew = oldContract.AutoRenew,
                RenewalType = oldContract.RenewalType,
                RenewalNoticeDays = oldContract.RenewalNoticeDays,
                Description = oldContract.Description,
                TermsConditions = oldContract.TermsConditions,
                Status = ContractStatus.Draft,
                ParentContractId = contractId,
                RenewalCount = oldContract.RenewalCount + 1,
                Create_by = renewedBy,
                CompanyID = oldContract.CompanyID,
                BranchID = oldContract.BranchID
            };

            var newContractId = _contractRepository.Create(newContract);

            // Create Financial extension for new contract
            if (oldFinancial != null)
            {
                Context.Insertable(new ErpContractFinancial
                {
                    ContractId = newContractId,
                    BaseAmount = oldFinancial.BaseAmount,
                    TotalAmount = oldFinancial.TotalAmount,
                    Currency = oldFinancial.Currency,
                    PaymentTerms = oldFinancial.PaymentTerms,
                    PaymentDueDays = oldFinancial.PaymentDueDays,
                    TaxGroupId = oldFinancial.TaxGroupId,
                    IsTaxInclusive = oldFinancial.IsTaxInclusive,
                    SecurityDeposit = oldFinancial.SecurityDeposit,
                    Create_time = DateTime.Now
                }).ExecuteCommand();
            }

            // Create BillingConfig extension for new contract
            if (oldBillingConfig != null)
            {
                var newStartDate = oldContract.EndDate?.AddDays(1) ?? DateTime.Today;
                Context.Insertable(new ErpContractBillingConfig
                {
                    ContractId = newContractId,
                    BillingFrequency = oldBillingConfig.BillingFrequency,
                    BillingDay = oldBillingConfig.BillingDay,
                    IsBillingVariable = oldBillingConfig.IsBillingVariable,
                    NextBillingDate = CalculateNextBillingDate(newStartDate, oldBillingConfig.BillingFrequency, oldBillingConfig.BillingDay),
                    Create_time = DateTime.Now
                }).ExecuteCommand();
            }

            // Create empty Lifecycle extension for new contract
            Context.Insertable(new ErpContractLifecycle
            {
                ContractId = newContractId,
                Create_time = DateTime.Now
            }).ExecuteCommand();

            return newContractId;
        }

        /// <summary>
        /// Get expiring contracts
        /// </summary>
        public List<ErpContractListDto> GetExpiringContracts(int daysFromNow)
        {
            return _contractRepository.GetExpiringContracts(daysFromNow);
        }

        /// <summary>
        /// Get contracts for selection
        /// </summary>
        public List<ErpContractSelectDto> GetSelectList(ContractStatus? status = null)
        {
            return _contractRepository.GetSelectList(status);
        }

        /// <summary>
        /// Get contracts by customer
        /// </summary>
        public List<ErpContractListDto> GetByCustomerId(Guid customerId)
        {
            return _contractRepository.GetByCustomerId(customerId);
        }

        /// <summary>
        /// Get statistics
        /// </summary>
        public ContractStatisticsDto GetStatistics()
        {
            return _contractRepository.GetStatistics();
        }

        /// <summary>
        /// Check if contract number exists
        /// </summary>
        public bool ContractNumberExists(string contractNumber, Guid? excludeId = null)
        {
            return _contractRepository.ContractNumberExists(contractNumber, excludeId);
        }

        /// <summary>
        /// Export contracts to Excel
        /// </summary>
        public List<ErpContractListDto> GetExportList(ErpContractQueryDto query)
        {
            var pager = new PagerInfo { PageSize = int.MaxValue, PageNum = 1 };
            return _contractRepository.GetList(query, pager).Result.ToList();
        }

        /// <summary>
        /// Get schedule impact for a contract (preview how pricing changes would affect schedules)
        /// </summary>
        public ScheduleImpactDto GetScheduleImpact(Guid contractId)
        {
            var schedules = Context.Queryable<ErpContractBillingSchedule>()
                .Where(s => s.ContractId == contractId)
                .ToList();

            return new ScheduleImpactDto
            {
                HasInvoicedSchedules = schedules.Any(s => s.Status == BillingScheduleStatus.Invoiced || s.Status == BillingScheduleStatus.Paid),
                InvoicedCount = schedules.Count(s => s.Status == BillingScheduleStatus.Invoiced),
                PendingCount = schedules.Count(s => s.Status == BillingScheduleStatus.Pending),
                PaidCount = schedules.Count(s => s.Status == BillingScheduleStatus.Paid),
                DueCount = schedules.Count(s => s.Status == BillingScheduleStatus.Due)
            };
        }

        #region Private Helper Methods

        private DateTime? CalculateNextBillingDate(DateTime startDate, BillingFrequency frequency, int? billingDay)
        {
            var today = DateTime.Today;
            DateTime nextDate;

            switch (frequency)
            {
                case BillingFrequency.OneTime:
                    return startDate;
                case BillingFrequency.Weekly:
                    nextDate = startDate;
                    while (nextDate <= today) nextDate = nextDate.AddDays(7);
                    return nextDate;
                case BillingFrequency.BiWeekly:
                    nextDate = startDate;
                    while (nextDate <= today) nextDate = nextDate.AddDays(14);
                    return nextDate;
                case BillingFrequency.Monthly:
                    var day = billingDay ?? startDate.Day;
                    nextDate = new DateTime(startDate.Year, startDate.Month, Math.Min(day, DateTime.DaysInMonth(startDate.Year, startDate.Month)));
                    while (nextDate <= today) nextDate = nextDate.AddMonths(1);
                    return nextDate;
                case BillingFrequency.Quarterly:
                    nextDate = startDate;
                    while (nextDate <= today) nextDate = nextDate.AddMonths(3);
                    return nextDate;
                case BillingFrequency.SemiAnnual:
                    nextDate = startDate;
                    while (nextDate <= today) nextDate = nextDate.AddMonths(6);
                    return nextDate;
                case BillingFrequency.Yearly:
                    nextDate = startDate;
                    while (nextDate <= today) nextDate = nextDate.AddYears(1);
                    return nextDate;
                default:
                    return startDate;
            }
        }

        private void GenerateBillingSchedules(Guid contractId, ErpContract contract, string createdBy)
        {
            if (!contract.EndDate.HasValue) return;

            // Load extension data
            var financial = Context.Queryable<ErpContractFinancial>().First(f => f.ContractId == contractId);
            var billingConfig = Context.Queryable<ErpContractBillingConfig>().First(b => b.ContractId == contractId);

            var billingFrequency = billingConfig?.BillingFrequency ?? BillingFrequency.Monthly;
            var paymentDueDays = financial?.PaymentDueDays ?? 30;
            var currency = financial?.Currency ?? "SAR";
            var totalSeasonPrice = financial?.BaseAmount ?? 0; // This is the FULL season price

            // Step 1: Count the number of billing periods
            var periods = new List<(DateTime Start, DateTime End, DateTime DueDate)>();
            var currentDate = contract.StartDate;

            while (currentDate < contract.EndDate)
            {
                DateTime nextDate;
                switch (billingFrequency)
                {
                    case BillingFrequency.Weekly:
                        nextDate = currentDate.AddDays(7); break;
                    case BillingFrequency.BiWeekly:
                        nextDate = currentDate.AddDays(14); break;
                    case BillingFrequency.Monthly:
                        nextDate = currentDate.AddMonths(1); break;
                    case BillingFrequency.Quarterly:
                        nextDate = currentDate.AddMonths(3); break;
                    case BillingFrequency.SemiAnnual:
                        nextDate = currentDate.AddMonths(6); break;
                    case BillingFrequency.Yearly:
                        nextDate = currentDate.AddYears(1); break;
                    case BillingFrequency.OneTime:
                        nextDate = contract.EndDate.Value; break;
                    default:
                        nextDate = currentDate.AddMonths(1); break;
                }

                if (nextDate > contract.EndDate) nextDate = contract.EndDate.Value;

                var billingDay = billingConfig?.BillingDay ?? currentDate.Day;
                var dueDate = new DateTime(currentDate.Year, currentDate.Month, Math.Min(billingDay, DateTime.DaysInMonth(currentDate.Year, currentDate.Month)));
                if (dueDate < currentDate) dueDate = dueDate.AddMonths(1);

                periods.Add((currentDate, nextDate.AddDays(-1), dueDate));

                currentDate = nextDate;
                if (billingFrequency == BillingFrequency.OneTime) break;
            }

            if (!periods.Any()) return;

            // Step 2: Divide total season price equally across periods
            int periodCount = periods.Count;
            decimal perInstallment = Math.Round(totalSeasonPrice / periodCount, 4);
            // Put rounding remainder on last installment
            decimal distributed = perInstallment * (periodCount - 1);
            decimal lastInstallment = totalSeasonPrice - distributed;

            // Step 3: Create schedule entries
            var schedules = new List<ErpContractBillingSchedule>();
            for (int i = 0; i < periodCount; i++)
            {
                var period = periods[i];
                decimal amount = (i == periodCount - 1) ? lastInstallment : perInstallment;

                schedules.Add(new ErpContractBillingSchedule
                {
                    ContractId = contractId,
                    ScheduleNumber = i + 1,
                    DueDate = period.DueDate,
                    PaymentDueDate = period.DueDate.AddDays(paymentDueDays),
                    PeriodStart = period.Start,
                    PeriodEnd = period.End,
                    BaseAmount = amount,
                    AdjustmentAmount = 0,
                    Subtotal = amount,
                    TotalAmount = amount,
                    Currency = currency,
                    Status = BillingScheduleStatus.Pending,
                    Create_by = createdBy
                });
            }

            _billingScheduleRepository.BulkCreate(schedules);
        }

        /// <summary>
        /// Regenerate only the pending/due schedules after a pricing change.
        /// Starts AFTER the last kept (Invoiced/Paid) schedule's period end,
        /// and continues the schedule numbering from where the kept schedules left off.
        /// </summary>
        private void RegeneratePendingSchedules(Guid contractId, ErpContract contract, string createdBy)
        {
            if (!contract.EndDate.HasValue) return;

            var financial = Context.Queryable<ErpContractFinancial>().First(f => f.ContractId == contractId);
            var billingConfig = Context.Queryable<ErpContractBillingConfig>().First(b => b.ContractId == contractId);

            // Find kept schedules (Invoiced, Paid, etc.) to determine where to resume
            var keptSchedules = Context.Queryable<ErpContractBillingSchedule>()
                .Where(s => s.ContractId == contractId)
                .OrderBy(s => s.PeriodEnd)
                .ToList();

            DateTime regenerateFrom;
            int nextScheduleNumber;
            if (keptSchedules.Any())
            {
                var lastKept = keptSchedules.OrderByDescending(s => s.PeriodEnd).First();
                regenerateFrom = lastKept.PeriodEnd != default ? lastKept.PeriodEnd.AddDays(1) : contract.StartDate;
                nextScheduleNumber = keptSchedules.Max(s => s.ScheduleNumber) + 1;
            }
            else
            {
                regenerateFrom = contract.StartDate;
                nextScheduleNumber = 1;
            }

            if (regenerateFrom >= contract.EndDate) return;

            var billingFrequency = billingConfig?.BillingFrequency ?? BillingFrequency.Monthly;
            var totalSeasonPrice = financial?.BaseAmount ?? 0;
            var paymentDueDays = financial?.PaymentDueDays ?? 30;
            var currency = financial?.Currency ?? "SAR";

            // Already collected from kept schedules
            decimal alreadyCollected = keptSchedules
                .Where(s => s.Status == BillingScheduleStatus.Invoiced || s.Status == BillingScheduleStatus.Paid)
                .Sum(s => s.BaseAmount);
            decimal remaining = totalSeasonPrice - alreadyCollected;
            if (remaining < 0) remaining = 0;

            // Count new periods first
            var periods = new List<(DateTime Start, DateTime End, DateTime DueDate)>();
            var currentDate = regenerateFrom;

            while (currentDate < contract.EndDate)
            {
                DateTime nextDate;
                switch (billingFrequency)
                {
                    case BillingFrequency.Weekly:
                        nextDate = currentDate.AddDays(7); break;
                    case BillingFrequency.BiWeekly:
                        nextDate = currentDate.AddDays(14); break;
                    case BillingFrequency.Monthly:
                        nextDate = currentDate.AddMonths(1); break;
                    case BillingFrequency.Quarterly:
                        nextDate = currentDate.AddMonths(3); break;
                    case BillingFrequency.SemiAnnual:
                        nextDate = currentDate.AddMonths(6); break;
                    case BillingFrequency.Yearly:
                        nextDate = currentDate.AddYears(1); break;
                    case BillingFrequency.OneTime:
                        nextDate = contract.EndDate.Value; break;
                    default:
                        nextDate = currentDate.AddMonths(1); break;
                }

                if (nextDate > contract.EndDate) nextDate = contract.EndDate.Value;

                var billingDay = billingConfig?.BillingDay ?? currentDate.Day;
                var dueDate = new DateTime(currentDate.Year, currentDate.Month, Math.Min(billingDay, DateTime.DaysInMonth(currentDate.Year, currentDate.Month)));
                if (dueDate < currentDate) dueDate = dueDate.AddMonths(1);

                periods.Add((currentDate, nextDate.AddDays(-1), dueDate));
                currentDate = nextDate;
                if (billingFrequency == BillingFrequency.OneTime) break;
            }

            if (!periods.Any()) return;

            // Distribute remaining equally across new periods
            int periodCount = periods.Count;
            decimal perInstallment = Math.Round(remaining / periodCount, 4);
            decimal distributed = perInstallment * (periodCount - 1);
            decimal lastInstallment = remaining - distributed;

            var schedules = new List<ErpContractBillingSchedule>();
            for (int i = 0; i < periodCount; i++)
            {
                var period = periods[i];
                decimal amount = (i == periodCount - 1) ? lastInstallment : perInstallment;

                schedules.Add(new ErpContractBillingSchedule
                {
                    ContractId = contractId,
                    ScheduleNumber = nextScheduleNumber + i,
                    DueDate = period.DueDate,
                    PaymentDueDate = period.DueDate.AddDays(paymentDueDays),
                    PeriodStart = period.Start,
                    PeriodEnd = period.End,
                    BaseAmount = amount,
                    AdjustmentAmount = 0,
                    Subtotal = amount,
                    TotalAmount = amount,
                    Currency = currency,
                    Status = BillingScheduleStatus.Pending,
                    Create_by = createdBy
                });
            }

            _billingScheduleRepository.BulkCreate(schedules);
        }

        #endregion

        #region Boulevard Reports

        /// <summary>
        /// Get contract info by shop IDs for boulevard reports
        /// </summary>
        public List<ShopContractInfoDto> GetContractInfoByShopIds(long[] shopIds)
        {
            if (shopIds == null || !shopIds.Any()) return new List<ShopContractInfoDto>();

            return Context.Queryable<ErpContractUnit, ErpContract, Customer>(
                (cu, c, cust) => new JoinQueryInfos(
                    JoinType.Inner, cu.ContractId == c.ContractId,
                    JoinType.Inner, c.CustomerId == cust.Id))
                .Where((cu, c, cust) => cu.BlvShopId != null && shopIds.Contains(cu.BlvShopId.Value))
                .Select((cu, c, cust) => new ShopContractInfoDto
                {
                    BlvShopId = cu.BlvShopId.Value,
                    ContractNumber = c.ContractNumber,
                    ContractTitle = c.ContractTitle,
                    ContractStatus = (int)c.Status,
                    CustomerNameAr = cust.NameAr,
                    CustomerNameEn = cust.NameEn,
                    BrandName = cust.BrandName
                })
                .ToList();
        }

        #endregion
    }
}
