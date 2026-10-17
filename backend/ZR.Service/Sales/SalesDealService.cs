using Infrastructure.Attribute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZR.Model.Sales;
using ZR.Model.Sales.Dto;
using ZR.Model.Customers.Leads;
using ZR.Model.HR.EmployeeCore;
using ZR.Model.Boulevard;
using ZR.Model.Contracts;
using ZR.Model.Contracts.Enums;
using ZR.Repository;
using ZR.Service.Sales.ISalesService;
using ZR.Service.Customers.ICustomersService;
using ZR.Service.IService.Finance.ARAP;
using ZR.Service.IService.Customers;
using ZR.Model.Finance.ARAP.Dto;
using ZR.Model.Customers.Dto;
using ZR.Service.Contracts.IService;
using ZR.Model.Contracts.Dto;
using ZR.Model.Audit;

namespace ZR.Service.Sales
{
    [AppService(ServiceType = typeof(ISalesDealService), ServiceLifetime = LifeTime.Transient)]
    public class SalesDealService : BaseService<SalesDeal>, ISalesDealService
    {
        private readonly IBpLeadService _leadService;
        private readonly ICustomerService _customerService;
        private readonly IContractService _contractService;
        private readonly IContractUnitService _contractUnitService;

        public SalesDealService(
            IBpLeadService leadService,
            ICustomerService customerService,
            IContractService contractService,
            IContractUnitService contractUnitService)
        {
            _leadService = leadService;
            _customerService = customerService;
            _contractService = contractService;
            _contractUnitService = contractUnitService;
        }

        /// <summary>
        /// Get employee ID by user ID
        /// </summary>
        public async Task<long?> GetEmployeeIdByUserIdAsync(long userId)
        {
            var employee = await Context.Queryable<HrEmployeeCore>()
                .Where(e => e.UserId.HasValue && e.UserId.Value == userId)
                .FirstAsync();
            return employee?.EmployeeId;
        }

        public async Task<PagedInfo<SalesDeal>> GetDealsListAsync(DealQueryDto query)
        {
            // Role-based filtering
            long? filterEmployeeId = null;

            if (!query.IsCurrentUserAdmin && query.CurrentUserId.HasValue)
            {
                filterEmployeeId = await GetEmployeeIdByUserIdAsync(query.CurrentUserId.Value);

                if (!filterEmployeeId.HasValue)
                {
                    return new PagedInfo<SalesDeal>
                    {
                        Result = new List<SalesDeal>(),
                        TotalNum = 0,
                        PageIndex = query.PageNum,
                        PageSize = query.PageSize
                    };
                }
            }

            var now = DateTime.Now;
            var pageQuery = Context
                .Queryable<SalesDeal, BpLead, HrEmployeeCore, HrPersonName>(
                    (deal, lead, emp, name) => new JoinQueryInfos(
                        JoinType.Left, deal.LeadId == lead.LeadId,
                        JoinType.Left, deal.AssignedTo == emp.EmployeeId,
                        JoinType.Left, emp.PersonId == name.PersonId &&
                                       name.IsPrimary == true &&
                                       name.EffectiveFrom <= now &&
                                       (name.EffectiveTo == null || name.EffectiveTo > now)))
                // Role-based filter
                .WhereIF(filterEmployeeId.HasValue, (deal, lead, emp, name) => deal.AssignedTo == filterEmployeeId.Value)
                // Regular filters
                .WhereIF(query.LeadId.HasValue, (deal, lead, emp, name) => deal.LeadId == query.LeadId.Value)
                .WhereIF(!string.IsNullOrEmpty(query.DealStatus), (deal, lead, emp, name) => deal.DealStatus == query.DealStatus)
                .WhereIF(query.AssignedTo.HasValue && !filterEmployeeId.HasValue, (deal, lead, emp, name) => deal.AssignedTo == query.AssignedTo.Value)
                .WhereIF(!string.IsNullOrEmpty(query.Priority), (deal, lead, emp, name) => deal.Priority == query.Priority)
                .WhereIF(!string.IsNullOrEmpty(query.DealTitle), (deal, lead, emp, name) => deal.DealTitle.Contains(query.DealTitle))
                .WhereIF(query.ExpectedCloseDateStart.HasValue, (deal, lead, emp, name) => deal.ExpectedCloseDate >= query.ExpectedCloseDateStart.Value)
                .WhereIF(query.ExpectedCloseDateEnd.HasValue, (deal, lead, emp, name) => deal.ExpectedCloseDate <= query.ExpectedCloseDateEnd.Value)
                .Select((deal, lead, emp, name) => new SalesDeal
                {
                    DealId = deal.DealId,
                    LeadId = deal.LeadId,
                    LeadName = lead.BrandName,
                    LeadStatus = lead.LeadStatus,
                    DealTitle = deal.DealTitle,
                    DealValue = deal.DealValue,
                    Currency = deal.Currency,
                    DealStatus = deal.DealStatus,
                    DealStage = deal.DealStage,
                    AssignedTo = deal.AssignedTo,
                    AssignedToName = name.FullNameAr ?? (name.GivenName + " " + name.FamilyName),
                    ExpectedCloseDate = deal.ExpectedCloseDate,
                    ActualCloseDate = deal.ActualCloseDate,
                    WonReason = deal.WonReason,
                    LostReason = deal.LostReason,
                    Notes = deal.Notes,
                    Priority = deal.Priority,
                    DealType = deal.DealType,
                    SharePercentage = deal.SharePercentage,
                    Create_by = deal.Create_by,
                    Create_time = deal.Create_time,
                    Update_by = deal.Update_by,
                    Update_time = deal.Update_time,
                    Remark = deal.Remark,
                    CompanyID = deal.CompanyID,
                    BranchID = deal.BranchID
                });

            return await pageQuery.ToPageAsync(query);
        }

        public async Task<SalesDeal> GetDealByIdAsync(long dealId)
        {
            var now = DateTime.Now;
            var deal = await Context
                .Queryable<SalesDeal, BpLead, HrEmployeeCore, HrPersonName>(
                    (d, lead, emp, name) => new JoinQueryInfos(
                        JoinType.Left, d.LeadId == lead.LeadId,
                        JoinType.Left, d.AssignedTo == emp.EmployeeId,
                        JoinType.Left, emp.PersonId == name.PersonId &&
                                       name.IsPrimary == true &&
                                       name.EffectiveFrom <= now &&
                                       (name.EffectiveTo == null || name.EffectiveTo > now)))
                .Where((d, lead, emp, name) => d.DealId == dealId)
                .Select((d, lead, emp, name) => new SalesDeal
                {
                    DealId = d.DealId,
                    LeadId = d.LeadId,
                    LeadName = lead.BrandName,
                    LeadStatus = lead.LeadStatus,
                    DealTitle = d.DealTitle,
                    DealValue = d.DealValue,
                    Currency = d.Currency,
                    DealStatus = d.DealStatus,
                    DealStage = d.DealStage,
                    AssignedTo = d.AssignedTo,
                    AssignedToName = name.FullNameAr ?? (name.GivenName + " " + name.FamilyName),
                    ExpectedCloseDate = d.ExpectedCloseDate,
                    ActualCloseDate = d.ActualCloseDate,
                    WonReason = d.WonReason,
                    LostReason = d.LostReason,
                    Notes = d.Notes,
                    Priority = d.Priority,
                    DealType = d.DealType,
                    SharePercentage = d.SharePercentage,
                    Create_by = d.Create_by,
                    Create_time = d.Create_time,
                    Update_by = d.Update_by,
                    Update_time = d.Update_time,
                    Remark = d.Remark,
                    CompanyID = d.CompanyID,
                    BranchID = d.BranchID
                })
                .FirstAsync();

            return deal;
        }

        public async Task<List<SalesDeal>> GetDealsByLeadAsync(long leadId)
        {
            return await GetListAsync(d => d.LeadId == leadId);
        }

        public async Task<SalesDeal> CreateDealAsync(SalesDeal deal, long? currentUserId = null, bool isAdmin = true)
        {
            // Validate lead is confirmed before creating a deal
            if (deal.LeadId > 0)
            {
                var lead = await _leadService.GetLeadByIdAsync(deal.LeadId);
                if (lead == null)
                    throw new Exception("Lead not found");
                if (lead.LeadStatus != "Confirmed" && lead.LeadStatus != "Converted")
                    throw new Exception("Lead must be confirmed before creating a deal");
            }

            deal.DealId = SnowFlakeSingle.Instance.NextId();
            deal.Create_time = DateTime.Now;

            if (string.IsNullOrEmpty(deal.DealStatus))
            {
                deal.DealStatus = "Under Negotiation";
            }

            // Auto-assign for non-admin users
            if (!isAdmin && deal.AssignedTo == 0 && currentUserId.HasValue)
            {
                var employeeId = await GetEmployeeIdByUserIdAsync(currentUserId.Value);
                if (employeeId.HasValue)
                {
                    deal.AssignedTo = employeeId.Value;
                }
            }

            Add(deal);
            return await GetDealByIdAsync(deal.DealId);
        }

        public async Task<int> UpdateDealAsync(SalesDeal deal)
        {
            deal.Update_time = DateTime.Now;
            return await Task.FromResult(Update(deal, true));
        }

        public async Task<int> UpdateDealStatusAsync(long dealId, string newStatus, string notes)
        {
            var deal = await GetByIdAsync(dealId);
            if (deal == null)
                throw new Exception("Deal not found");

            var oldStatus = deal.DealStatus;

            // Pre-transaction validation (don't start transaction for validation failures)
            if (newStatus == "Won")
            {
                var dealUnitsToCheck = await Context.Queryable<SalesDealUnit>()
                    .Where(du => du.DealId == dealId)
                    .ToListAsync();

                foreach (var du in dealUnitsToCheck)
                {
                    var shop = await Context.Queryable<BlvBoulevardShops>()
                        .Where(s => s.Id == du.ShopId)
                        .FirstAsync();
                    if (shop != null && shop.ShopStatus == 2)
                    {
                        throw new Exception($"Unit '{du.ShopName}' is already reserved by another deal. Please remove it before marking this deal as Won.");
                    }
                }

                var existingCustomer = _customerService.GetByLeadId(deal.LeadId);
                if (existingCustomer == null)
                    throw new Exception("Customer not found. The lead must be confirmed first to create a customer.");

                if (!_customerService.IsProfileComplete(existingCustomer.Id))
                {
                    var completion = _customerService.GetProfileCompletionPercentage(existingCustomer.Id);
                    throw new Exception($"Customer profile is only {completion}% complete. Operations must complete the customer profile to 100% before the deal can be marked as Won.");
                }
            }

            var tranResult = await UseTranAsync(async () =>
            {
                deal.DealStatus = newStatus;
                deal.Update_time = DateTime.Now;

                if (newStatus == "Won")
                {
                    var existingCustomer = _customerService.GetByLeadId(deal.LeadId);

                    deal.ActualCloseDate = DateTime.Now;
                    deal.WonReason = notes;
                    deal.IsConverted = true;

                    // Update lead to "Converted"
                    var lead = await _leadService.GetLeadByIdAsync(deal.LeadId);
                    if (lead != null)
                    {
                        lead.LeadStatus = "Converted";
                        lead.ConversionDate = DateTime.Now;
                        await _leadService.UpdateLeadAsync(lead);

                        var contractType = deal.DealType switch
                        {
                            "Rent" => ContractType.Rental,
                            "Shared" => ContractType.Service,
                            "Mixed" => ContractType.Other,
                            _ => ContractType.Other
                        };

                        var dealUnits = await Context.Queryable<SalesDealUnit>()
                            .Where(du => du.DealId == dealId)
                            .ToListAsync();

                        // Calculate total: Fixed uses FixedRentAmount, RevShare uses ShopBasePrice
                        decimal totalValue = dealUnits.Any()
                            ? dealUnits.Sum(du =>
                            {
                                if (du.PricingType == PricingType.RevenueShare)
                                    return du.ShopBasePrice ?? du.MinimumGuarantee ?? 0;
                                return du.FixedRentAmount ?? du.ShopBasePrice ?? 0;
                            })
                            : deal.DealValue;

                        // Resolve contract dates from season or deal
                        DateTime contractStartDate = DateTime.Now;
                        DateTime? contractEndDate = null;

                        // Try deal-level season first, then fall back to deal-unit season
                        long? resolvedSeasonId = deal.SeasonId;
                        if (!resolvedSeasonId.HasValue && dealUnits.Any())
                        {
                            resolvedSeasonId = dealUnits.First().SeasonId;
                        }

                        if (resolvedSeasonId.HasValue)
                        {
                            var season = await Context.Queryable<BlvBoulevardSeasons>()
                                .Where(s => s.Id == resolvedSeasonId.Value)
                                .FirstAsync();
                            if (season != null)
                            {
                                contractStartDate = season.StartDate > DateTime.Now ? season.StartDate : DateTime.Now;
                                contractEndDate = season.EndDate;
                            }
                        }

                        if (contractEndDate == null && deal.ExpectedCloseDate.HasValue)
                        {
                            // Fallback: 1 year from expected close date
                            contractEndDate = deal.ExpectedCloseDate.Value.AddYears(1);
                        }

                        // Count billing periods exactly like GenerateBillingSchedules does
                        int totalPeriods = 0;
                        if (contractEndDate.HasValue)
                        {
                            var tempDate = contractStartDate;
                            while (tempDate < contractEndDate.Value)
                            {
                                tempDate = tempDate.AddMonths(1);
                                totalPeriods++;
                            }
                        }
                        totalPeriods = Math.Max(1, totalPeriods);
                        decimal baseAmountPerPeriod = Math.Round(totalValue / totalPeriods, 2);

                        var customerDisplayName = lead.BrandName ?? lead.OwnerName ?? "Customer";
                        var contractDto = new CreateErpContractDto
                        {
                            CustomerId = existingCustomer.Id,
                            ContractType = contractType,
                            ContractTitle = $"Contract of {customerDisplayName}",
                            ContractTitleAr = $"عقد {customerDisplayName}",
                            StartDate = contractStartDate,
                            EndDate = contractEndDate,
                            BillingFrequency = BillingFrequency.Monthly,
                            BaseAmount = baseAmountPerPeriod,
                            TotalAmount = totalValue,
                            Currency = deal.Currency ?? "SAR",
                            Notes = $"Auto-generated from Deal #{deal.DealId}: {deal.DealTitle}",
                            ExternalReference = deal.DealId.ToString()
                        };

                        var contractId = _contractService.Create(contractDto, "System");

                        if (dealUnits.Any())
                        {
                            int seq = 0;
                            foreach (var du in dealUnits)
                            {
                                seq++;
                                // Determine unit price: Fixed uses FixedRentAmount, RevShare uses ShopBasePrice
                                decimal? unitPrice = du.PricingType == PricingType.RevenueShare
                                    ? (du.ShopBasePrice ?? du.MinimumGuarantee)
                                    : (du.FixedRentAmount ?? du.ShopBasePrice);

                                var contractUnit = new ErpContractUnit
                                {
                                    UnitId = Guid.NewGuid(),
                                    ContractId = contractId,
                                    UnitType = ContractUnitType.RetailShop,
                                    UnitCode = $"SHOP-{du.ShopId}",
                                    UnitName = du.ShopName,
                                    UnitNameAr = du.ShopName,
                                    Area = du.ShopArea,
                                    AreaUnit = "sqm",
                                    BasePrice = unitPrice,
                                    PricePerArea = du.ShopArea.HasValue && du.ShopArea.Value > 0 && unitPrice.HasValue
                                        ? unitPrice.Value / du.ShopArea.Value
                                        : null,
                                    CalculatedPrice = unitPrice,
                                    Currency = deal.Currency ?? "SAR",
                                    IsActive = true,
                                    StartDate = contractStartDate,
                                    EndDate = contractEndDate,
                                    SequenceNumber = seq,
                                    BlvShopId = du.ShopId,
                                    ExternalRefCode = du.ShopId.ToString(),
                                    PricingType = du.PricingType,
                                    FixedRentAmount = du.FixedRentAmount,
                                    RevenuePercentage = du.RevenuePercentage,
                                    MinimumGuarantee = du.MinimumGuarantee,
                                    SettlementFrequency = du.SettlementFrequency,
                                    PricingLocked = true,
                                    DiscountAmount = du.DiscountAmount,
                                    DiscountPercentage = du.DiscountPercentage,
                                    Create_by = "System",
                                    Create_time = DateTime.Now
                                };

                                Context.Insertable(contractUnit).ExecuteCommand();

                                du.IsConverted = true;
                                du.Update_time = DateTime.Now;
                                du.Update_by = "System";
                                Context.Updateable(du)
                                    .UpdateColumns(x => new { x.IsConverted, x.Update_time, x.Update_by })
                                    .Where(x => x.DealUnitId == du.DealUnitId)
                                    .ExecuteCommand();
                            }
                        }
                    }
                }
                else if (newStatus == "Lost")
                {
                    deal.ActualCloseDate = DateTime.Now;
                    deal.LostReason = notes;

                    var dealUnits = await Context.Queryable<SalesDealUnit>()
                        .Where(du => du.DealId == dealId && !du.IsConverted)
                        .ToListAsync();

                    foreach (var du in dealUnits)
                    {
                        Context.Updateable<BlvBoulevardShops>()
                            .SetColumns(s => s.ShopStatus == 1)
                            .SetColumns(s => s.Update_time == DateTime.Now)
                            .Where(s => s.Id == du.ShopId)
                            .ExecuteCommand();
                    }
                }
                else if (!string.IsNullOrEmpty(notes))
                {
                    var statusNote = $"[{DateTime.Now:yyyy-MM-dd HH:mm}] Status changed to {newStatus}: {notes}";
                    deal.Notes = string.IsNullOrEmpty(deal.Notes)
                        ? statusNote
                        : $"{deal.Notes}\n{statusNote}";
                }

                Update(deal, true);

                // Audit log
                await Context.Insertable(new EntityStatusAudit
                {
                    Id = SnowFlakeSingle.Instance.NextId(),
                    EntityType = "Deal",
                    EntityId = dealId.ToString(),
                    OldStatus = oldStatus,
                    NewStatus = newStatus,
                    Notes = notes,
                    ChangedBy = deal.Update_by ?? "System",
                    ChangedAt = DateTime.Now
                }).ExecuteCommandAsync();
            });

            if (!tranResult.IsSuccess)
                throw new Exception($"Transaction failed: {tranResult.ErrorMessage}");

            return 1;
        }

        public async Task<int> DeleteDealsAsync(long[] dealIds)
        {
            return await Context.Updateable<SalesDeal>()
                .SetColumns(d => new SalesDeal { IsDeleted = true, DeletedAt = DateTime.Now })
                .Where(d => dealIds.Contains(d.DealId))
                .ExecuteCommandAsync();
        }

        /// <summary>
        /// Recalculate deal summary from its units:
        /// - dealValue = SUM(fixedRentAmount ?? minimumGuarantee ?? 0)
        /// - dealType = "Rent" / "Shared" / "Mixed" based on unit pricing types
        /// </summary>
        public async Task RecalculateDealSummaryAsync(long dealId)
        {
            var units = await Context.Queryable<SalesDealUnit>()
                .Where(du => du.DealId == dealId)
                .ToListAsync();

            if (!units.Any())
            {
                // No units: reset to defaults
                await Context.Updateable<SalesDeal>()
                    .SetColumns(d => d.DealValue == 0)
                    .SetColumns(d => d.DealType == null)
                    .SetColumns(d => d.SharePercentage == null)
                    .SetColumns(d => d.Update_time == DateTime.Now)
                    .Where(d => d.DealId == dealId)
                    .ExecuteCommandAsync();
                return;
            }

            // Calculate total value
            // Fixed/Hybrid: use FixedRentAmount
            // RevenueShare: use ShopBasePrice (the shop's season price = area × meter price)
            decimal totalValue = units.Sum(du =>
            {
                if (du.PricingType == PricingType.RevenueShare)
                    return du.ShopBasePrice ?? du.MinimumGuarantee ?? 0;
                return du.FixedRentAmount ?? du.ShopBasePrice ?? 0;
            });

            // Derive deal type from unit pricing types
            var hasFixed = units.Any(u => u.PricingType == PricingType.Fixed);
            var hasRevShare = units.Any(u => u.PricingType == PricingType.RevenueShare);
            var hasHybrid = units.Any(u => u.PricingType == PricingType.Hybrid);

            string dealType;
            decimal? sharePercentage = null;

            if (hasHybrid || (hasFixed && hasRevShare))
            {
                dealType = "Mixed";
            }
            else if (hasRevShare)
            {
                dealType = "Shared";
                // Average revenue percentage across all RevenueShare units
                var revUnits = units.Where(u => u.PricingType == PricingType.RevenueShare && u.RevenuePercentage.HasValue).ToList();
                if (revUnits.Any())
                    sharePercentage = Math.Round(revUnits.Average(u => u.RevenuePercentage.Value), 2);
            }
            else
            {
                dealType = "Rent";
            }

            await Context.Updateable<SalesDeal>()
                .SetColumns(d => d.DealValue == totalValue)
                .SetColumns(d => d.DealType == dealType)
                .SetColumns(d => d.SharePercentage == sharePercentage)
                .SetColumns(d => d.Update_time == DateTime.Now)
                .Where(d => d.DealId == dealId)
                .ExecuteCommandAsync();
        }

        public async Task<DealDashboardDto> GetDashboardStatsAsync(long? assignedTo)
        {
            var query = Context.Queryable<SalesDeal>();

            if (assignedTo.HasValue)
            {
                query = query.Where(d => d.AssignedTo == assignedTo.Value);
            }

            var deals = await query.ToListAsync();

            return new DealDashboardDto
            {
                OpenDeals = deals.Count(d => d.DealStatus == "Under Negotiation"),
                WonDeals = deals.Count(d => d.DealStatus == "Won"),
                LostDeals = deals.Count(d => d.DealStatus == "Lost"),
                TotalDeals = deals.Count,
                TotalPipelineValue = deals
                    .Where(d => d.DealStatus == "Under Negotiation")
                    .Sum(d => d.DealValue),
                WonValue = deals
                    .Where(d => d.DealStatus == "Won")
                    .Sum(d => d.DealValue),
                LostValue = deals
                    .Where(d => d.DealStatus == "Lost")
                    .Sum(d => d.DealValue)
            };
        }
    }
}
