using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.Attribute;
using Mapster;
using ZR.Model.Boulevard;
using ZR.Model.Contracts;
using ZR.Model.Contracts.Dto;
using ZR.Model.Contracts.Enums;
using ZR.Model.Sales;
using ZR.Model.Sales.Dto;
using ZR.Repository.Contracts;
using ZR.Service.Contracts.IService;
using ZR.ServiceCore;

namespace ZR.Service.Contracts
{
    /// <summary>
    /// Contract Unit Service - خدمة وحدات العقد
    /// Business logic layer for contract unit operations
    /// Updated to follow Finance module pattern with BaseService and DI
    /// </summary>
    [AppService(ServiceType = typeof(IContractUnitService), ServiceLifetime = LifeTime.Transient)]
    public class ContractUnitService : BaseService<ErpContractUnit>, IContractUnitService
    {
        private readonly IContractUnitRepository _unitRepository;

        public ContractUnitService(IContractUnitRepository unitRepository)
        {
            _unitRepository = unitRepository;
        }

        /// <summary>
        /// Get units by contract ID
        /// </summary>
        public List<ErpContractUnitListDto> GetByContractId(Guid contractId, bool? isActive = null)
        {
            return _unitRepository.GetByContractId(contractId, isActive);
        }

        /// <summary>
        /// Get units by contract ID async
        /// </summary>
        public async Task<List<ErpContractUnitListDto>> GetByContractIdAsync(Guid contractId, bool? isActive = null)
        {
            return await _unitRepository.GetByContractIdAsync(contractId, isActive);
        }

        /// <summary>
        /// Get unit by ID
        /// </summary>
        public ErpContractUnit GetById(Guid unitId)
        {
            return _unitRepository.GetById(unitId);
        }

        /// <summary>
        /// Get unit by ID async
        /// </summary>
        public async Task<ErpContractUnit> GetByIdAsync(Guid unitId)
        {
            return await _unitRepository.GetByIdAsync(unitId);
        }

        /// <summary>
        /// Create unit
        /// </summary>
        public Guid Create(CreateErpContractUnitDto dto, string createdBy)
        {
            // Check if unit code already exists in this contract
            if (UnitCodeExists(dto.ContractId, dto.UnitCode))
            {
                throw new InvalidOperationException($"Unit code '{dto.UnitCode}' already exists in this contract");
            }

            var unit = dto.Adapt<ErpContractUnit>();
            unit.Create_by = createdBy;

            return _unitRepository.Create(unit);
        }

        /// <summary>
        /// Create unit async
        /// </summary>
        public async Task<Guid> CreateAsync(CreateErpContractUnitDto dto, string createdBy)
        {
            // Check if unit code already exists in this contract
            if (await UnitCodeExistsAsync(dto.ContractId, dto.UnitCode))
            {
                throw new InvalidOperationException($"Unit code '{dto.UnitCode}' already exists in this contract");
            }

            var unit = dto.Adapt<ErpContractUnit>();
            unit.Create_by = createdBy;

            return await _unitRepository.CreateAsync(unit);
        }

        /// <summary>
        /// Update unit
        /// When price changes on an Active contract:
        /// 1. Update the unit price
        /// 2. Recalculate contract BaseAmount = SUM of all unit prices
        /// 3. Remaining = NewBaseAmount - (sum of Invoiced + Paid schedules)
        /// 4. Redistribute remaining equally across Pending/Due schedules
        /// 5. Auto-create amendment for audit trail
        /// </summary>
        public UnitPriceUpdateResultDto Update(UpdateErpContractUnitDto dto, string updatedBy)
        {
            var existing = _unitRepository.GetById(dto.UnitId);
            if (existing == null) return new UnitPriceUpdateResultDto { Success = false, Warning = "Unit not found" };

            // Check if unit code already exists in this contract (excluding current unit)
            if (UnitCodeExists(existing.ContractId, dto.UnitCode, dto.UnitId))
            {
                throw new InvalidOperationException($"Unit code '{dto.UnitCode}' already exists in this contract");
            }

            // Capture old price before update
            decimal oldUnitPrice = existing.CalculatedPrice ?? existing.BasePrice ?? 0;

            // Update the unit
            var unit = dto.Adapt<ErpContractUnit>();
            unit.ContractId = existing.ContractId;
            unit.Update_by = updatedBy;
            _unitRepository.UpdateUnit(unit);

            // Calculate new price after update
            decimal newUnitPrice = unit.CalculatedPrice ?? unit.BasePrice ?? dto.BasePrice ?? 0;
            bool priceChanged = oldUnitPrice != newUnitPrice;

            var result = new UnitPriceUpdateResultDto
            {
                Success = true,
                PriceChanged = priceChanged,
                OldUnitPrice = oldUnitPrice,
                NewUnitPrice = newUnitPrice
            };

            // Always sync contract BaseAmount/TotalAmount when unit price changes
            if (priceChanged)
            {
                var contract = Context.Queryable<ErpContract>()
                    .Where(c => c.ContractId == existing.ContractId)
                    .First();

                if (contract != null)
                {
                    // Recalculate contract BaseAmount = SUM of all active unit prices
                    decimal newBaseAmount = Context.Queryable<ErpContractUnit>()
                        .Where(u => u.ContractId == existing.ContractId && u.IsActive)
                        .Sum(u => u.CalculatedPrice ?? u.BasePrice ?? 0);

                    var financial = Context.Queryable<ErpContractFinancial>()
                        .Where(f => f.ContractId == existing.ContractId)
                        .First();
                    decimal oldBaseAmount = financial?.BaseAmount ?? 0;

                    result.OldContractBaseAmount = oldBaseAmount;
                    result.NewContractBaseAmount = newBaseAmount;

                    // Update contract financial
                    Context.Updateable<ErpContractFinancial>()
                        .SetColumns(f => f.BaseAmount == newBaseAmount)
                        .SetColumns(f => f.TotalAmount == newBaseAmount)
                        .SetColumns(f => f.Update_time == DateTime.Now)
                        .Where(f => f.ContractId == existing.ContractId)
                        .ExecuteCommand();

                    // Recalculate billing schedules based on contract status
                    if (contract.Status == ContractStatus.Active)
                    {
                        // Active: redistribute remaining across pending schedules + create amendment
                        RecalculateContractAndBilling(existing.ContractId, contract, oldUnitPrice, newUnitPrice, updatedBy, result);
                    }
                    else
                    {
                        // Draft/other: delete all existing schedules and regenerate with new price
                        RegenerateDraftBillingSchedules(existing.ContractId, contract, newBaseAmount, updatedBy, result);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Update unit async
        /// Same logic as sync Update — recalculates contract + billing on price change
        /// </summary>
        public async Task<int> UpdateAsync(UpdateErpContractUnitDto dto, string updatedBy)
        {
            var existing = await _unitRepository.GetByIdAsync(dto.UnitId);
            if (existing == null) return 0;

            // Check if unit code already exists in this contract (excluding current unit)
            if (await UnitCodeExistsAsync(existing.ContractId, dto.UnitCode, dto.UnitId))
            {
                throw new InvalidOperationException($"Unit code '{dto.UnitCode}' already exists in this contract");
            }

            decimal oldUnitPrice = existing.CalculatedPrice ?? existing.BasePrice ?? 0;

            var unit = dto.Adapt<ErpContractUnit>();
            unit.ContractId = existing.ContractId;
            unit.Update_by = updatedBy;
            var updateResult = await _unitRepository.UpdateUnitAsync(unit);

            decimal newUnitPrice = unit.CalculatedPrice ?? unit.BasePrice ?? dto.BasePrice ?? 0;
            bool priceChanged = oldUnitPrice != newUnitPrice;

            if (priceChanged)
            {
                var contract = await Context.Queryable<ErpContract>()
                    .Where(c => c.ContractId == existing.ContractId)
                    .FirstAsync();

                if (contract != null)
                {
                    // Recalculate contract BaseAmount = SUM of all active unit prices
                    decimal newBaseAmount = await Context.Queryable<ErpContractUnit>()
                        .Where(u => u.ContractId == existing.ContractId && u.IsActive)
                        .SumAsync(u => u.CalculatedPrice ?? u.BasePrice ?? 0);

                    await Context.Updateable<ErpContractFinancial>()
                        .SetColumns(f => f.BaseAmount == newBaseAmount)
                        .SetColumns(f => f.TotalAmount == newBaseAmount)
                        .SetColumns(f => f.Update_time == DateTime.Now)
                        .Where(f => f.ContractId == existing.ContractId)
                        .ExecuteCommandAsync();

                    // For Active contracts, also redistribute billing schedules
                    if (contract.Status == ContractStatus.Active)
                    {
                        var result = new UnitPriceUpdateResultDto
                        {
                            Success = true,
                            OldContractBaseAmount = 0,
                            NewContractBaseAmount = newBaseAmount
                        };
                        RecalculateContractAndBilling(existing.ContractId, contract, oldUnitPrice, newUnitPrice, updatedBy, result);
                    }
                }
            }

            return updateResult;
        }

        /// <summary>
        /// Delete unit
        /// </summary>
        public int Delete(Guid unitId)
        {
            return _unitRepository.Delete(unitId);
        }

        /// <summary>
        /// Delete unit async
        /// </summary>
        public async Task<int> DeleteAsync(Guid unitId)
        {
            return await _unitRepository.DeleteAsync(unitId);
        }

        /// <summary>
        /// Toggle unit active status
        /// </summary>
        public int ToggleActive(Guid unitId)
        {
            return _unitRepository.ToggleActive(unitId);
        }

        /// <summary>
        /// Toggle unit active status async
        /// </summary>
        public async Task<int> ToggleActiveAsync(Guid unitId)
        {
            return await _unitRepository.ToggleActiveAsync(unitId);
        }

        /// <summary>
        /// Check if unit code exists in contract
        /// </summary>
        public bool UnitCodeExists(Guid contractId, string unitCode, Guid? excludeId = null)
        {
            return _unitRepository.UnitCodeExists(contractId, unitCode, excludeId);
        }

        /// <summary>
        /// Check if unit code exists in contract async
        /// </summary>
        public async Task<bool> UnitCodeExistsAsync(Guid contractId, string unitCode, Guid? excludeId = null)
        {
            return await _unitRepository.UnitCodeExistsAsync(contractId, unitCode, excludeId);
        }

        /// <summary>
        /// Calculate total price for contract units
        /// </summary>
        public decimal CalculateTotalPrice(Guid contractId)
        {
            return _unitRepository.CalculateTotalPrice(contractId);
        }

        /// <summary>
        /// Calculate total price for contract units async
        /// </summary>
        public async Task<decimal> CalculateTotalPriceAsync(Guid contractId)
        {
            return await _unitRepository.CalculateTotalPriceAsync(contractId);
        }

        /// <summary>
        /// Get active units count
        /// </summary>
        public int GetActiveUnitsCount(Guid contractId)
        {
            return _unitRepository.GetActiveUnitsCount(contractId);
        }

        /// <summary>
        /// Get active units count async
        /// </summary>
        public async Task<int> GetActiveUnitsCountAsync(Guid contractId)
        {
            return await _unitRepository.GetActiveUnitsCountAsync(contractId);
        }

        /// <summary>
        /// Recalculate contract BaseAmount and redistribute billing schedules after unit price change.
        /// Unit price = season price (full contract period).
        /// Remaining = NewBaseAmount - (sum of Invoiced + Paid schedules)
        /// Distribute remaining equally across Pending/Due schedules.
        /// </summary>
        private void RecalculateContractAndBilling(
            Guid contractId, ErpContract contract,
            decimal oldUnitPrice, decimal newUnitPrice,
            string updatedBy, UnitPriceUpdateResultDto result)
        {
            // BaseAmount/TotalAmount already updated by caller
            decimal newBaseAmount = result.NewContractBaseAmount;
            decimal oldBaseAmount = result.OldContractBaseAmount;

            var financial = Context.Queryable<ErpContractFinancial>()
                .Where(f => f.ContractId == contractId)
                .First();

            // Get all billing schedules
            var allSchedules = Context.Queryable<ErpContractBillingSchedule>()
                .Where(s => s.ContractId == contractId)
                .OrderBy(s => s.ScheduleNumber)
                .ToList();

            // Calculate already collected (Invoiced + Paid — immutable)
            decimal alreadyCollected = allSchedules
                .Where(s => s.Status == BillingScheduleStatus.Invoiced || s.Status == BillingScheduleStatus.Paid)
                .Sum(s => s.BaseAmount);

            // Get pending/due schedules to redistribute
            var pendingSchedules = allSchedules
                .Where(s => s.Status == BillingScheduleStatus.Pending || s.Status == BillingScheduleStatus.Due)
                .OrderBy(s => s.ScheduleNumber)
                .ToList();

            result.AlreadyCollected = alreadyCollected;

            if (pendingSchedules.Count > 0)
            {
                // 4. Remaining = NewBaseAmount - already collected
                decimal remaining = newBaseAmount - alreadyCollected;
                if (remaining < 0) remaining = 0;

                decimal perInstallment = Math.Round(remaining / pendingSchedules.Count, 4);

                // Handle rounding: put any remainder on the last schedule
                decimal distributed = perInstallment * (pendingSchedules.Count - 1);
                decimal lastAmount = remaining - distributed;

                result.RemainingToCollect = remaining;
                result.NewInstallmentAmount = perInstallment;
                result.SchedulesRecalculated = pendingSchedules.Count;

                // 5. Update each pending/due schedule with new amount
                for (int i = 0; i < pendingSchedules.Count; i++)
                {
                    var schedule = pendingSchedules[i];
                    decimal amount = (i == pendingSchedules.Count - 1) ? lastAmount : perInstallment;

                    Context.Updateable<ErpContractBillingSchedule>()
                        .SetColumns(s => s.BaseAmount == amount)
                        .SetColumns(s => s.Subtotal == amount)
                        .SetColumns(s => s.TotalAmount == amount)
                        .SetColumns(s => s.Update_time == DateTime.Now)
                        .Where(s => s.ScheduleId == schedule.ScheduleId)
                        .ExecuteCommand();
                }
            }
            else
            {
                // No pending schedules — all are invoiced/paid
                decimal remaining = newBaseAmount - alreadyCollected;
                result.RemainingToCollect = remaining;
                result.SchedulesRecalculated = 0;

                // If there's remaining amount, generate an adjustment schedule
                if (remaining > 0)
                {
                    int lastScheduleNum = allSchedules.Any() ? allSchedules.Max(s => s.ScheduleNumber) : 0;
                    var lastSchedule = allSchedules.OrderByDescending(s => s.PeriodEnd).FirstOrDefault();

                    // Adjustment schedule covers from now until contract end
                    var adjustmentStart = DateTime.Now.Date;
                    var adjustmentEnd = contract.EndDate ?? DateTime.Now.Date.AddMonths(1);
                    var paymentDueDays = financial?.PaymentDueDays ?? 30;

                    var adjustmentSchedule = new ErpContractBillingSchedule
                    {
                        ScheduleId = Guid.NewGuid(),
                        ContractId = contractId,
                        ScheduleNumber = lastScheduleNum + 1,
                        PeriodStart = adjustmentStart,
                        PeriodEnd = adjustmentEnd > adjustmentStart ? adjustmentEnd : adjustmentStart.AddMonths(1),
                        DueDate = adjustmentStart,
                        PaymentDueDate = adjustmentStart.AddDays(paymentDueDays),
                        BaseAmount = remaining,
                        AdjustmentAmount = 0,
                        Subtotal = remaining,
                        TaxAmount = 0,
                        TotalAmount = remaining,
                        Currency = financial?.Currency ?? "SAR",
                        Status = BillingScheduleStatus.Pending,
                        Description = $"Price adjustment: {oldUnitPrice:N2} → {newUnitPrice:N2}",
                        DescriptionAr = $"تعديل سعر: {oldUnitPrice:N2} → {newUnitPrice:N2}",
                        Create_by = updatedBy,
                        Create_time = DateTime.Now
                    };
                    Context.Insertable(adjustmentSchedule).ExecuteCommand();

                    result.SchedulesRecalculated = 1;
                    result.NewInstallmentAmount = remaining;
                }
            }

            // 6. Auto-create PriceChange amendment for audit trail
            var amendment = new ErpContractAmendment
            {
                AmendmentId = Guid.NewGuid(),
                ContractId = contractId,
                AmendmentType = AmendmentType.PriceChange,
                Status = AmendmentStatus.Approved,
                ChangesJson = System.Text.Json.JsonSerializer.Serialize(new
                {
                    unit_price = new { previous = oldUnitPrice, @new = newUnitPrice },
                    contract_base_amount = new { previous = oldBaseAmount, @new = newBaseAmount },
                    already_collected = alreadyCollected,
                    remaining_redistributed = newBaseAmount - alreadyCollected,
                    schedules_recalculated = pendingSchedules.Count,
                    per_installment = result.NewInstallmentAmount
                }),
                AmendmentReference = $"{contract.ContractNumber}-AMD-UNIT",
                EffectiveDate = DateTime.Now,
                RequestedDate = DateTime.Now,
                ApprovedDate = DateTime.Now,
                AppliedDate = DateTime.Now,
                RequestedBy = updatedBy,
                ApprovedBy = updatedBy,
                AppliedBy = updatedBy,
                RecalculateSchedules = true,
                SchedulesRecalculated = true,
                SchedulesAffectedCount = pendingSchedules.Count,
                ApplyToPendingOnly = true,
                Currency = financial?.Currency ?? "SAR",
                MonthlyImpact = pendingSchedules.Count > 0 ? result.NewInstallmentAmount - (pendingSchedules.FirstOrDefault()?.BaseAmount ?? 0) : 0,
                TotalImpact = newBaseAmount - oldBaseAmount,
                Create_by = updatedBy,
                Create_time = DateTime.Now
            };
            Context.Insertable(amendment).ExecuteCommand();

            result.AmendmentId = amendment.AmendmentId;
            result.Warning = $"Unit price changed from {oldUnitPrice:N2} to {newUnitPrice:N2}. " +
                $"Contract amount updated: {oldBaseAmount:N2} → {newBaseAmount:N2}. " +
                $"{result.SchedulesRecalculated} pending schedules recalculated at {result.NewInstallmentAmount:N2} each.";
        }

        /// <summary>
        /// For Draft contracts: delete ALL existing billing schedules and regenerate with new price.
        /// Since Draft contracts have no invoiced/paid schedules, we can safely delete everything.
        /// </summary>
        private void RegenerateDraftBillingSchedules(
            Guid contractId, ErpContract contract,
            decimal newBaseAmount, string updatedBy, UnitPriceUpdateResultDto result)
        {
            // Check if any billing schedules exist
            var existingSchedules = Context.Queryable<ErpContractBillingSchedule>()
                .Where(s => s.ContractId == contractId)
                .ToList();

            if (!existingSchedules.Any())
            {
                // No schedules to regenerate
                return;
            }

            // Delete all existing schedules (Draft = all should be Pending)
            Context.Deleteable<ErpContractBillingSchedule>()
                .Where(s => s.ContractId == contractId)
                .ExecuteCommand();

            // Get billing config for frequency and dates
            var billingConfig = Context.Queryable<ErpContractBillingConfig>()
                .Where(b => b.ContractId == contractId)
                .First();
            var financial = Context.Queryable<ErpContractFinancial>()
                .Where(f => f.ContractId == contractId)
                .First();

            if (!contract.EndDate.HasValue) return;

            var startDate = contract.StartDate;
            var endDate = contract.EndDate.Value;
            var billingFrequency = billingConfig?.BillingFrequency ?? BillingFrequency.Monthly;
            var paymentDueDays = financial?.PaymentDueDays ?? 30;
            var currency = financial?.Currency ?? "SAR";

            // Step 1: Count billing periods
            var periods = new List<(DateTime Start, DateTime End, DateTime DueDate)>();
            var currentDate = startDate;

            while (currentDate < endDate)
            {
                DateTime nextDate;
                switch (billingFrequency)
                {
                    case BillingFrequency.Weekly: nextDate = currentDate.AddDays(7); break;
                    case BillingFrequency.BiWeekly: nextDate = currentDate.AddDays(14); break;
                    case BillingFrequency.Monthly: nextDate = currentDate.AddMonths(1); break;
                    case BillingFrequency.Quarterly: nextDate = currentDate.AddMonths(3); break;
                    case BillingFrequency.SemiAnnual: nextDate = currentDate.AddMonths(6); break;
                    case BillingFrequency.Yearly: nextDate = currentDate.AddYears(1); break;
                    default: nextDate = currentDate.AddYears(100); break;
                }
                if (nextDate > endDate) nextDate = endDate;

                var billingDay = billingConfig?.BillingDay ?? currentDate.Day;
                var dueDate = new DateTime(currentDate.Year, currentDate.Month,
                    Math.Min(billingDay, DateTime.DaysInMonth(currentDate.Year, currentDate.Month)));
                if (dueDate < currentDate) dueDate = dueDate.AddMonths(1);

                periods.Add((currentDate, nextDate.AddDays(-1), dueDate));
                currentDate = nextDate;
                if (billingFrequency == BillingFrequency.OneTime) break;
            }

            if (!periods.Any()) return;

            // Step 2: Divide season price equally across periods
            int periodCount = periods.Count;
            decimal perInstallment = Math.Round(newBaseAmount / periodCount, 4);
            decimal distributed = perInstallment * (periodCount - 1);
            decimal lastInstallment = newBaseAmount - distributed;

            // Step 3: Create new schedules
            var schedules = new List<ErpContractBillingSchedule>();
            for (int i = 0; i < periodCount; i++)
            {
                var period = periods[i];
                decimal installmentAmount = (i == periodCount - 1) ? lastInstallment : perInstallment;

                schedules.Add(new ErpContractBillingSchedule
                {
                    ScheduleId = Guid.NewGuid(),
                    ContractId = contractId,
                    ScheduleNumber = i + 1,
                    DueDate = period.DueDate,
                    PaymentDueDate = period.DueDate.AddDays(paymentDueDays),
                    PeriodStart = period.Start,
                    PeriodEnd = period.End,
                    BaseAmount = installmentAmount,
                    Subtotal = installmentAmount,
                    TotalAmount = installmentAmount,
                    Currency = currency,
                    Status = BillingScheduleStatus.Pending,
                    Create_by = updatedBy,
                    Create_time = DateTime.Now
                });
            }

            Context.Insertable(schedules).ExecuteCommand();

            result.SchedulesRecalculated = periodCount;
            result.NewInstallmentAmount = perInstallment;
            result.RemainingToCollect = newBaseAmount;
            result.Warning = $"Contract amount updated to {newBaseAmount:N2}. " +
                $"{periodCount} billing schedules regenerated at {perInstallment:N2} each.";
        }

        /// <summary>
        /// Change the boulevard shop assigned to a contract unit.
        /// Also updates the corresponding deal unit to stay in sync.
        /// </summary>
        public async Task<int> ChangeShopAsync(ChangeContractUnitShopDto dto, string updatedBy)
        {
            // 1. Get contract unit
            var unit = await _unitRepository.GetByIdAsync(dto.UnitId);
            if (unit == null)
                throw new Exception("Contract unit not found");

            if (!unit.BlvShopId.HasValue)
                throw new Exception("This unit is not linked to a boulevard shop");

            // 2. Get contract and validate status
            var contract = await Context.Queryable<ErpContract>()
                .Where(c => c.ContractId == unit.ContractId)
                .FirstAsync();
            if (contract == null)
                throw new Exception("Contract not found");

            if (contract.Status != ContractStatus.Draft && contract.Status != ContractStatus.PendingApproval)
                throw new Exception("Shop can only be changed on Draft or Pending Approval contracts");

            // 3. Get new shop and validate
            var newShop = await Context.Queryable<BlvBoulevardShops>()
                .Where(s => s.Id == dto.NewShopId)
                .FirstAsync();
            if (newShop == null)
                throw new Exception("New shop not found");

            if (newShop.ShopStatus == 2)
                throw new Exception("This shop is already reserved by an active contract");

            // 4. Check shop not linked to an active deal (not Lost/Won)
            var shopInActiveDeal = await Context.Queryable<SalesDealUnit, SalesDeal>(
                (du, deal) => new JoinQueryInfos(JoinType.Inner, du.DealId == deal.DealId))
                .Where((du, deal) => du.ShopId == dto.NewShopId)
                .Where((du, deal) => deal.DealStatus != "Lost" && deal.DealStatus != "Won")
                .AnyAsync();

            if (shopInActiveDeal)
                throw new Exception("This shop is currently linked to an active deal");

            // 5. Save old shop ID for finding the deal unit
            var oldBlvShopId = unit.BlvShopId.Value;

            // 6. Update contract unit
            unit.BlvShopId = dto.NewShopId;
            unit.UnitCode = $"SHOP-{dto.NewShopId}";
            unit.UnitName = newShop.Name;
            unit.UnitNameAr = newShop.Name;
            unit.Area = (decimal)newShop.ShopArea;
            unit.BasePrice = (decimal)newShop.TotalPrice;
            unit.PricePerArea = newShop.ShopArea > 0 ? (decimal)(newShop.MeterPrice) : null;
            unit.CalculatedPrice = (decimal)newShop.TotalPrice;
            unit.ExternalRefCode = dto.NewShopId.ToString();
            unit.Update_by = updatedBy;
            unit.Update_time = DateTime.Now;

            var result = await Context.Updateable(unit)
                .IgnoreColumns(u => new { u.UnitId, u.ContractId, u.Create_by, u.Create_time })
                .Where(u => u.UnitId == unit.UnitId)
                .ExecuteCommandAsync();

            // 7. Update corresponding deal unit if contract has an ExternalReference (dealId)
            if (!string.IsNullOrEmpty(contract.ExternalReference) && long.TryParse(contract.ExternalReference, out var dealId))
            {
                var dealUnit = await Context.Queryable<SalesDealUnit>()
                    .Where(du => du.DealId == dealId && du.ShopId == oldBlvShopId)
                    .FirstAsync();

                if (dealUnit != null)
                {
                    dealUnit.ShopId = dto.NewShopId;
                    dealUnit.ShopName = newShop.Name;
                    dealUnit.ShopArea = (decimal)newShop.ShopArea;
                    dealUnit.ZoneId = newShop.ZoneId;
                    dealUnit.ShopCategoryId = newShop.ShopCategory;
                    dealUnit.ShopBasePrice = (decimal)newShop.TotalPrice;

                    // Recalculate discount
                    if (dealUnit.FixedRentAmount.HasValue && dealUnit.ShopBasePrice.HasValue && dealUnit.ShopBasePrice.Value > 0)
                    {
                        dealUnit.DiscountAmount = dealUnit.ShopBasePrice.Value - dealUnit.FixedRentAmount.Value;
                        dealUnit.DiscountPercentage = Math.Round(
                            (dealUnit.ShopBasePrice.Value - dealUnit.FixedRentAmount.Value) / dealUnit.ShopBasePrice.Value * 100, 2);
                    }

                    dealUnit.Update_by = updatedBy;
                    dealUnit.Update_time = DateTime.Now;

                    await Context.Updateable(dealUnit)
                        .UpdateColumns(x => new {
                            x.ShopId, x.ShopName, x.ShopArea, x.ZoneId, x.ShopCategoryId,
                            x.ShopBasePrice, x.DiscountAmount, x.DiscountPercentage,
                            x.Update_by, x.Update_time
                        })
                        .Where(x => x.DealUnitId == dealUnit.DealUnitId)
                        .ExecuteCommandAsync();

                    // Recalculate deal summary (inlined to avoid circular dependency)
                    var allDealUnits = await Context.Queryable<SalesDealUnit>()
                        .Where(du2 => du2.DealId == dealId)
                        .ToListAsync();

                    if (allDealUnits.Any())
                    {
                        decimal totalValue = allDealUnits.Sum(du2 => du2.FixedRentAmount ?? du2.MinimumGuarantee ?? 0);
                        var hasFixed = allDealUnits.Any(u => u.PricingType == PricingType.Fixed);
                        var hasRevShare = allDealUnits.Any(u => u.PricingType == PricingType.RevenueShare);
                        var hasHybrid = allDealUnits.Any(u => u.PricingType == PricingType.Hybrid);

                        string dealType;
                        decimal? sharePct = null;
                        if (hasHybrid || (hasFixed && hasRevShare))
                            dealType = "Mixed";
                        else if (hasRevShare)
                        {
                            dealType = "Shared";
                            var revUnits = allDealUnits.Where(u => u.PricingType == PricingType.RevenueShare && u.RevenuePercentage.HasValue).ToList();
                            if (revUnits.Any())
                                sharePct = Math.Round(revUnits.Average(u => u.RevenuePercentage.Value), 2);
                        }
                        else
                            dealType = "Rent";

                        await Context.Updateable<SalesDeal>()
                            .SetColumns(d => d.DealValue == totalValue)
                            .SetColumns(d => d.DealType == dealType)
                            .SetColumns(d => d.SharePercentage == sharePct)
                            .SetColumns(d => d.Update_time == DateTime.Now)
                            .Where(d => d.DealId == dealId)
                            .ExecuteCommandAsync();
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Get available shops for a contract unit (shops not reserved and not in active deals).
        /// Uses the deal unit's season to find shops in the same season.
        /// </summary>
        public async Task<List<AvailableShopDto>> GetAvailableShopsForUnitAsync(Guid unitId)
        {
            // 1. Get contract unit
            var unit = await _unitRepository.GetByIdAsync(unitId);
            if (unit == null)
                throw new Exception("Contract unit not found");

            if (!unit.BlvShopId.HasValue)
                throw new Exception("This unit is not linked to a boulevard shop");

            // 2. Get contract
            var contract = await Context.Queryable<ErpContract>()
                .Where(c => c.ContractId == unit.ContractId)
                .FirstAsync();
            if (contract == null)
                throw new Exception("Contract not found");

            // 3. Find deal unit to get SeasonId
            long seasonId = 0;
            if (!string.IsNullOrEmpty(contract.ExternalReference) && long.TryParse(contract.ExternalReference, out var dealId))
            {
                var dealUnit = await Context.Queryable<SalesDealUnit>()
                    .Where(du => du.DealId == dealId && du.ShopId == unit.BlvShopId.Value)
                    .FirstAsync();

                if (dealUnit != null)
                {
                    seasonId = dealUnit.SeasonId;
                }
            }

            // If no season found, try to get from the shop's current season
            if (seasonId == 0)
            {
                var seasonShop = await Context.Queryable<BlvBoulevardSeasonsShops>()
                    .Where(ss => ss.ShopId == unit.BlvShopId.Value)
                    .OrderByDescending(ss => ss.SeasonId)
                    .FirstAsync();
                if (seasonShop != null)
                    seasonId = seasonShop.SeasonId;
            }

            if (seasonId == 0)
                throw new Exception("Could not determine the season for this unit");

            // 4. Get shops linked to active deals (not Lost) for this season
            var linkedShopIds = await Context.Queryable<SalesDealUnit, SalesDeal>(
                (du, deal) => new JoinQueryInfos(JoinType.Inner, du.DealId == deal.DealId))
                .Where((du, deal) => du.SeasonId == seasonId)
                .Where((du, deal) => deal.DealStatus != "Lost")
                .Select((du, deal) => du.ShopId)
                .ToListAsync();

            // 5. Also exclude shops reserved by active contracts (ShopStatus == 2)
            // But allow current shop to appear in case user wants to revert
            var query = Context.Queryable<BlvBoulevardShops, BlvBoulevardSeasonsShops, BlvBoulevardSeasonZones, BlvBoulevardShopCategory, BlvBoulevardShopStatus>(
                (shop, ss, zone, cat, status) => new JoinQueryInfos(
                    JoinType.Inner, shop.Id == ss.ShopId,
                    JoinType.Left, shop.ZoneId == zone.Id,
                    JoinType.Left, shop.ShopCategory == cat.Id,
                    JoinType.Left, shop.ShopStatus == status.Id))
                .Where((shop, ss, zone, cat, status) => ss.SeasonId == seasonId)
                .Where((shop, ss, zone, cat, status) => shop.ShopStatus != 2); // Not reserved

            if (linkedShopIds.Any())
            {
                query = query.Where((shop, ss, zone, cat, status) => !linkedShopIds.Contains(shop.Id));
            }

            return await query
                .Select((shop, ss, zone, cat, status) => new AvailableShopDto
                {
                    Id = shop.Id,
                    Name = shop.Name,
                    ZoneId = shop.ZoneId,
                    ZoneName = zone.Name,
                    ShopCategory = shop.ShopCategory,
                    CategoryName = cat.Name,
                    ShopArea = shop.ShopArea,
                    MeterPrice = shop.MeterPrice,
                    TotalPrice = shop.TotalPrice,
                    ShopStatusId = shop.ShopStatus,
                    ShopStatusName = status.Name,
                    UnitType = shop.UnitType,
                    RentalType = shop.RentalType,
                    RevenuePercentage = shop.RevenuePercentage
                })
                .ToListAsync();
        }
    }
}
