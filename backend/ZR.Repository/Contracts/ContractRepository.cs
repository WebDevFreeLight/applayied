using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.Attribute;
using SqlSugar;
using ZR.Model;
using ZR.Model.Contracts;
using ZR.Model.Contracts.Dto;
using ZR.Model.Contracts.Enums;
using ZR.Repository;

namespace ZR.Repository.Contracts
{
    /// <summary>
    /// Contract Repository - مستودع العقود
    /// Data access layer for contract operations
    /// Updated to use extension tables (Phase 1 schema changes)
    /// </summary>
    [AppService(ServiceType = typeof(IContractRepository), ServiceLifetime = LifeTime.Transient)]
    public class ContractRepository : BaseRepository<ErpContract>, IContractRepository
    {
        /// <summary>
        /// Get contracts list with pagination
        /// Includes data from Financial and BillingConfig extension tables
        /// </summary>
        public PagedInfo<ErpContractListDto> GetList(ErpContractQueryDto query, PagerInfo pager)
        {
            var predicate = Expressionable.Create<ErpContract>();

            if (query.ContractId.HasValue)
                predicate.And(c => c.ContractId == query.ContractId.Value);
            if (!string.IsNullOrEmpty(query.ContractNumber))
                predicate.And(c => c.ContractNumber.Contains(query.ContractNumber));
            if (!string.IsNullOrEmpty(query.CustomerName))
                predicate.And(c => c.ContractTitle.Contains(query.CustomerName) || c.ContractTitleAr.Contains(query.CustomerName));
            if (query.CustomerId.HasValue)
                predicate.And(c => c.CustomerId == query.CustomerId.Value);
            if (query.ContractType.HasValue)
                predicate.And(c => c.ContractType == query.ContractType.Value);
            if (query.Status.HasValue)
                predicate.And(c => c.Status == query.Status.Value);
            if (query.StartDateFrom.HasValue)
                predicate.And(c => c.StartDate >= query.StartDateFrom.Value);
            if (query.StartDateTo.HasValue)
                predicate.And(c => c.StartDate <= query.StartDateTo.Value);
            if (query.EndDateFrom.HasValue)
                predicate.And(c => c.EndDate >= query.EndDateFrom.Value);
            if (query.EndDateTo.HasValue)
                predicate.And(c => c.EndDate <= query.EndDateTo.Value);

            var totalCount = 0;

            // Use left join to include extension table data
            var list = Context.Queryable<ErpContract>()
                .LeftJoin<ErpContractFinancial>((c, f) => c.ContractId == f.ContractId)
                .LeftJoin<ErpContractBillingConfig>((c, f, b) => c.ContractId == b.ContractId)
                .Where(predicate.ToExpression())
                .OrderByDescending((c, f, b) => c.Create_time)
                .Select((c, f, b) => new ErpContractListDto
                {
                    ContractId = c.ContractId,
                    ContractNumber = c.ContractNumber,
                    ContractTitle = c.ContractTitle,
                    CustomerId = c.CustomerId,
                    ContractType = c.ContractType,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    Status = c.Status,
                    BaseAmount = f.BaseAmount,
                    TotalAmount = f.TotalAmount,
                    Currency = f.Currency,
                    BillingFrequency = b.BillingFrequency,
                    NextBillingDate = b.NextBillingDate,
                    AutoRenew = c.AutoRenew,
                    Create_time = c.Create_time
                })
                .ToPageList(pager.PageNum, pager.PageSize, ref totalCount);

            return new PagedInfo<ErpContractListDto>
            {
                Result = list,
                TotalNum = totalCount,
                PageIndex = pager.PageNum,
                PageSize = pager.PageSize
            };
        }

        /// <summary>
        /// Get contract by ID with full details
        /// Includes data from all extension tables (Financial, BillingConfig, Lifecycle)
        /// </summary>
        public ErpContractDetailDto GetById(Guid contractId)
        {
            var result = Context.Queryable<ErpContract>()
                .LeftJoin<ErpContractFinancial>((c, f) => c.ContractId == f.ContractId)
                .LeftJoin<ErpContractBillingConfig>((c, f, b) => c.ContractId == b.ContractId)
                .LeftJoin<ErpContractLifecycle>((c, f, b, l) => c.ContractId == l.ContractId)
                .Where((c, f, b, l) => c.ContractId == contractId)
                .Select((c, f, b, l) => new ErpContractDetailDto
                {
                    // Core contract fields
                    ContractId = c.ContractId,
                    ContractNumber = c.ContractNumber,
                    ContractTitle = c.ContractTitle,
                    ContractTitleAr = c.ContractTitleAr,
                    CustomerId = c.CustomerId,
                    ContractType = c.ContractType,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    Status = c.Status,
                    AutoRenew = c.AutoRenew,
                    RenewalType = c.RenewalType,
                    RenewalNoticeDays = c.RenewalNoticeDays,
                    ExternalReference = c.ExternalReference,
                    Description = c.Description,
                    TermsConditions = c.TermsConditions,
                    Notes = c.Notes,
                    ParentContractId = c.ParentContractId,
                    RenewalCount = c.RenewalCount,
                    Create_by = c.Create_by,
                    Create_time = c.Create_time,
                    Update_by = c.Update_by,
                    Update_time = c.Update_time,

                    // Financial extension fields
                    BaseAmount = f.BaseAmount,
                    TotalAmount = f.TotalAmount,
                    Currency = f.Currency,
                    PaymentTerms = f.PaymentTerms,
                    PaymentDueDays = f.PaymentDueDays,
                    TaxGroupId = f.TaxGroupId,
                    IsTaxInclusive = f.IsTaxInclusive,
                    SecurityDeposit = f.SecurityDeposit,

                    // BillingConfig extension fields
                    BillingFrequency = b.BillingFrequency,
                    BillingDay = b.BillingDay,
                    IsBillingVariable = b.IsBillingVariable,
                    NextBillingDate = b.NextBillingDate,

                    // Lifecycle extension fields
                    SignedDate = l.SignedDate,
                    SignatoryName = l.SignatoryName,
                    ActivatedDate = l.ActivatedDate,
                    ActivatedBy = l.ActivatedBy,
                    TerminatedDate = l.TerminatedDate,
                    TerminatedBy = l.TerminatedBy,
                    TerminationReason = l.TerminationReason
                })
                .First();

            return result;
        }

        /// <summary>
        /// Get contract entity by ID
        /// </summary>
        public ErpContract GetEntityById(Guid contractId)
        {
            return Context.Queryable<ErpContract>()
                .Where(c => c.ContractId == contractId)
                .First();
        }

        /// <summary>
        /// Create contract
        /// </summary>
        public Guid Create(ErpContract contract)
        {
            contract.ContractId = Guid.NewGuid();
            contract.Create_time = DateTime.Now;

            // Generate contract number if not provided
            if (string.IsNullOrEmpty(contract.ContractNumber))
            {
                contract.ContractNumber = GenerateContractNumber();
            }

            Context.Insertable(contract).ExecuteCommand();
            return contract.ContractId;
        }

        /// <summary>
        /// Update contract
        /// </summary>
        public int UpdateContract(ErpContract contract)
        {
            contract.Update_time = DateTime.Now;
            return Context.Updateable(contract)
                .IgnoreColumns(c => new { c.ContractId, c.ContractNumber, c.Create_by, c.Create_time })
                .Where(c => c.ContractId == contract.ContractId)
                .ExecuteCommand();
        }

        /// <summary>
        /// Update contract async
        /// </summary>
        public async Task<int> UpdateContractAsync(ErpContract contract)
        {
            contract.Update_time = DateTime.Now;
            return await Context.Updateable(contract)
                .IgnoreColumns(c => new { c.ContractId, c.ContractNumber, c.Create_by, c.Create_time })
                .Where(c => c.ContractId == contract.ContractId)
                .ExecuteCommandAsync();
        }

        /// <summary>
        /// Delete contracts
        /// </summary>
        public int Delete(Guid[] contractIds)
        {
            return Context.Deleteable<ErpContract>()
                .In(c => c.ContractId, contractIds)
                .ExecuteCommand();
        }

        /// <summary>
        /// Delete contracts async
        /// </summary>
        public async Task<int> DeleteAsync(Guid[] contractIds)
        {
            return await Context.Deleteable<ErpContract>()
                .In(c => c.ContractId, contractIds)
                .ExecuteCommandAsync();
        }

        /// <summary>
        /// Update contract status
        /// </summary>
        public int UpdateStatus(Guid contractId, ContractStatus status, string updatedBy)
        {
            return Context.Updateable<ErpContract>()
                .SetColumns(c => c.Status == status)
                .SetColumns(c => c.Update_by == updatedBy)
                .SetColumns(c => c.Update_time == DateTime.Now)
                .Where(c => c.ContractId == contractId)
                .ExecuteCommand();
        }

        /// <summary>
        /// Activate contract
        /// Updates both ErpContract status and ErpContractLifecycle activation fields
        /// </summary>
        public int Activate(Guid contractId, string activatedBy, DateTime? activationDate)
        {
            var effectiveDate = activationDate ?? DateTime.Today;

            // Update contract status
            var contractResult = Context.Updateable<ErpContract>()
                .SetColumns(c => c.Status == ContractStatus.Active)
                .SetColumns(c => c.Update_by == activatedBy)
                .SetColumns(c => c.Update_time == DateTime.Now)
                .Where(c => c.ContractId == contractId)
                .ExecuteCommand();

            // Update lifecycle extension - upsert pattern
            var existingLifecycle = Context.Queryable<ErpContractLifecycle>()
                .Where(l => l.ContractId == contractId)
                .First();

            if (existingLifecycle != null)
            {
                Context.Updateable<ErpContractLifecycle>()
                    .SetColumns(l => l.ActivatedDate == effectiveDate)
                    .SetColumns(l => l.ActivatedBy == activatedBy)
                    .SetColumns(l => l.Update_time == DateTime.Now)
                    .Where(l => l.ContractId == contractId)
                    .ExecuteCommand();
            }
            else
            {
                Context.Insertable(new ErpContractLifecycle
                {
                    ContractId = contractId,
                    ActivatedDate = effectiveDate,
                    ActivatedBy = activatedBy,
                    Create_time = DateTime.Now
                }).ExecuteCommand();
            }

            return contractResult;
        }

        /// <summary>
        /// Terminate contract
        /// Updates both ErpContract status and ErpContractLifecycle termination fields
        /// </summary>
        public int Terminate(Guid contractId, TerminationReason terminationReason, string terminatedBy, DateTime terminationDate)
        {
            // Update contract status
            var contractResult = Context.Updateable<ErpContract>()
                .SetColumns(c => c.Status == ContractStatus.Terminated)
                .SetColumns(c => c.Update_by == terminatedBy)
                .SetColumns(c => c.Update_time == DateTime.Now)
                .Where(c => c.ContractId == contractId)
                .ExecuteCommand();

            // Update lifecycle extension - upsert pattern
            var existingLifecycle = Context.Queryable<ErpContractLifecycle>()
                .Where(l => l.ContractId == contractId)
                .First();

            if (existingLifecycle != null)
            {
                Context.Updateable<ErpContractLifecycle>()
                    .SetColumns(l => l.TerminatedDate == terminationDate)
                    .SetColumns(l => l.TerminatedBy == terminatedBy)
                    .SetColumns(l => l.TerminationReason == terminationReason)
                    .SetColumns(l => l.Update_time == DateTime.Now)
                    .Where(l => l.ContractId == contractId)
                    .ExecuteCommand();
            }
            else
            {
                Context.Insertable(new ErpContractLifecycle
                {
                    ContractId = contractId,
                    TerminatedDate = terminationDate,
                    TerminatedBy = terminatedBy,
                    TerminationReason = terminationReason,
                    Create_time = DateTime.Now
                }).ExecuteCommand();
            }

            return contractResult;
        }

        /// <summary>
        /// Get expiring contracts
        /// Includes financial data from extension table
        /// </summary>
        public List<ErpContractListDto> GetExpiringContracts(int daysFromNow)
        {
            var expiryDate = DateTime.Today.AddDays(daysFromNow);
            return Context.Queryable<ErpContract>()
                .LeftJoin<ErpContractFinancial>((c, f) => c.ContractId == f.ContractId)
                .Where((c, f) => c.Status == ContractStatus.Active && c.EndDate <= expiryDate && c.EndDate >= DateTime.Today)
                .OrderBy((c, f) => c.EndDate)
                .Select((c, f) => new ErpContractListDto
                {
                    ContractId = c.ContractId,
                    ContractNumber = c.ContractNumber,
                    ContractTitle = c.ContractTitle,
                    CustomerId = c.CustomerId,
                    ContractType = c.ContractType,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    Status = c.Status,
                    TotalAmount = f.TotalAmount,
                    Currency = f.Currency
                })
                .ToList();
        }

        /// <summary>
        /// Get contracts for selection
        /// </summary>
        public List<ErpContractSelectDto> GetSelectList(ContractStatus? status = null)
        {
            var query = Context.Queryable<ErpContract>();
            if (status.HasValue)
            {
                query = query.Where(c => c.Status == status.Value);
            }

            return query
                .OrderByDescending(c => c.Create_time)
                .Select(c => new ErpContractSelectDto
                {
                    ContractId = c.ContractId,
                    ContractNumber = c.ContractNumber,
                    ContractTitle = c.ContractTitle,
                    Status = c.Status
                })
                .ToList();
        }

        /// <summary>
        /// Get contracts by customer
        /// Includes financial data from extension table
        /// </summary>
        public List<ErpContractListDto> GetByCustomerId(Guid customerId)
        {
            return Context.Queryable<ErpContract>()
                .LeftJoin<ErpContractFinancial>((c, f) => c.ContractId == f.ContractId)
                .Where((c, f) => c.CustomerId == customerId)
                .OrderByDescending((c, f) => c.Create_time)
                .Select((c, f) => new ErpContractListDto
                {
                    ContractId = c.ContractId,
                    ContractNumber = c.ContractNumber,
                    ContractTitle = c.ContractTitle,
                    CustomerId = c.CustomerId,
                    ContractType = c.ContractType,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    Status = c.Status,
                    TotalAmount = f.TotalAmount,
                    Currency = f.Currency
                })
                .ToList();
        }

        /// <summary>
        /// Get statistics
        /// Joins with financial table for TotalValue calculation
        /// </summary>
        public ContractStatisticsDto GetStatistics()
        {
            var contracts = Context.Queryable<ErpContract>().ToList();

            // Get total value from financial extension for active contracts
            var totalValue = Context.Queryable<ErpContract>()
                .LeftJoin<ErpContractFinancial>((c, f) => c.ContractId == f.ContractId)
                .Where((c, f) => c.Status == ContractStatus.Active)
                .Sum((c, f) => f.TotalAmount ?? 0);

            return new ContractStatisticsDto
            {
                TotalContracts = contracts.Count,
                ActiveContracts = contracts.Count(c => c.Status == ContractStatus.Active),
                DraftContracts = contracts.Count(c => c.Status == ContractStatus.Draft),
                SuspendedContracts = contracts.Count(c => c.Status == ContractStatus.Suspended),
                ExpiredContracts = contracts.Count(c => c.Status == ContractStatus.Expired),
                TotalValue = totalValue
            };
        }

        /// <summary>
        /// Check if contract number exists
        /// </summary>
        public bool ContractNumberExists(string contractNumber, Guid? excludeId = null)
        {
            var query = Context.Queryable<ErpContract>()
                .Where(c => c.ContractNumber == contractNumber);

            if (excludeId.HasValue)
            {
                query = query.Where(c => c.ContractId != excludeId.Value);
            }

            return query.Any();
        }

        /// <summary>
        /// Generate contract number
        /// </summary>
        private string GenerateContractNumber()
        {
            var year = DateTime.Now.Year;
            var prefix = $"CNT-{year}-";

            var lastNumber = Context.Queryable<ErpContract>()
                .ClearFilter() // include soft-deleted records to avoid duplicate numbers
                .Where(c => c.ContractNumber.StartsWith(prefix))
                .OrderByDescending(c => c.ContractNumber)
                .Select(c => c.ContractNumber)
                .First();

            var nextNumber = 1;
            if (!string.IsNullOrEmpty(lastNumber))
            {
                var numberPart = lastNumber.Replace(prefix, "");
                if (int.TryParse(numberPart, out var parsed))
                {
                    nextNumber = parsed + 1;
                }
            }

            return $"{prefix}{nextNumber:D5}";
        }

        #region Async Methods

        /// <summary>
        /// Get contract by ID with full details async
        /// </summary>
        public async Task<ErpContractDetailDto> GetByIdAsync(Guid contractId)
        {
            var contract = await Context.Queryable<ErpContract>()
                .LeftJoin<ErpContractFinancial>((c, f) => c.ContractId == f.ContractId)
                .LeftJoin<ErpContractBillingConfig>((c, f, b) => c.ContractId == b.ContractId)
                .LeftJoin<ErpContractLifecycle>((c, f, b, l) => c.ContractId == l.ContractId)
                .Where((c, f, b, l) => c.ContractId == contractId)
                .Select((c, f, b, l) => new ErpContractDetailDto
                {
                    // Core contract fields
                    ContractId = c.ContractId,
                    ContractNumber = c.ContractNumber,
                    ContractTitle = c.ContractTitle,
                    ContractTitleAr = c.ContractTitleAr,
                    CustomerId = c.CustomerId,
                    ContractType = c.ContractType,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    Status = c.Status,
                    AutoRenew = c.AutoRenew,
                    RenewalType = c.RenewalType,
                    RenewalNoticeDays = c.RenewalNoticeDays,
                    ExternalReference = c.ExternalReference,
                    Description = c.Description,
                    TermsConditions = c.TermsConditions,
                    Notes = c.Notes,
                    ParentContractId = c.ParentContractId,
                    RenewalCount = c.RenewalCount,
                    Create_by = c.Create_by,
                    Create_time = c.Create_time,
                    Update_by = c.Update_by,
                    Update_time = c.Update_time,

                    // Financial extension fields
                    BaseAmount = f.BaseAmount,
                    TotalAmount = f.TotalAmount,
                    Currency = f.Currency,
                    PaymentTerms = f.PaymentTerms,
                    PaymentDueDays = f.PaymentDueDays,
                    TaxGroupId = f.TaxGroupId,
                    IsTaxInclusive = f.IsTaxInclusive,
                    SecurityDeposit = f.SecurityDeposit,

                    // BillingConfig extension fields
                    BillingFrequency = b.BillingFrequency,
                    BillingDay = b.BillingDay,
                    IsBillingVariable = b.IsBillingVariable,
                    NextBillingDate = b.NextBillingDate,

                    // Lifecycle extension fields
                    SignedDate = l.SignedDate,
                    SignatoryName = l.SignatoryName,
                    ActivatedDate = l.ActivatedDate,
                    ActivatedBy = l.ActivatedBy,
                    TerminatedDate = l.TerminatedDate,
                    TerminatedBy = l.TerminatedBy,
                    TerminationReason = l.TerminationReason
                })
                .FirstAsync();

            return contract;
        }

        /// <summary>
        /// Get contract entity by ID async
        /// </summary>
        public async Task<ErpContract> GetEntityByIdAsync(Guid contractId)
        {
            return await Context.Queryable<ErpContract>()
                .Where(c => c.ContractId == contractId)
                .FirstAsync();
        }

        /// <summary>
        /// Create contract async
        /// </summary>
        public async Task<Guid> CreateAsync(ErpContract contract)
        {
            contract.ContractId = Guid.NewGuid();
            contract.Create_time = DateTime.Now;

            if (string.IsNullOrEmpty(contract.ContractNumber))
            {
                contract.ContractNumber = await GenerateContractNumberAsync();
            }

            await Context.Insertable(contract).ExecuteCommandAsync();
            return contract.ContractId;
        }

        /// <summary>
        /// Update contract status async
        /// </summary>
        public async Task<int> UpdateStatusAsync(Guid contractId, ContractStatus status, string updatedBy)
        {
            return await Context.Updateable<ErpContract>()
                .SetColumns(c => c.Status == status)
                .SetColumns(c => c.Update_by == updatedBy)
                .SetColumns(c => c.Update_time == DateTime.Now)
                .Where(c => c.ContractId == contractId)
                .ExecuteCommandAsync();
        }

        /// <summary>
        /// Activate contract async
        /// </summary>
        public async Task<int> ActivateAsync(Guid contractId, string activatedBy, DateTime? activationDate)
        {
            var result = await Context.Updateable<ErpContract>()
                .SetColumns(c => c.Status == ContractStatus.Active)
                .SetColumns(c => c.Update_by == activatedBy)
                .SetColumns(c => c.Update_time == DateTime.Now)
                .Where(c => c.ContractId == contractId)
                .ExecuteCommandAsync();

            await Context.Updateable<ErpContractLifecycle>()
                .SetColumns(l => l.ActivatedDate == (activationDate ?? DateTime.Now))
                .SetColumns(l => l.ActivatedBy == activatedBy)
                .SetColumns(l => l.Update_time == DateTime.Now)
                .Where(l => l.ContractId == contractId)
                .ExecuteCommandAsync();

            return result;
        }

        /// <summary>
        /// Terminate contract async
        /// </summary>
        public async Task<int> TerminateAsync(Guid contractId, TerminationReason terminationReason, string terminatedBy, DateTime terminationDate)
        {
            var result = await Context.Updateable<ErpContract>()
                .SetColumns(c => c.Status == ContractStatus.Terminated)
                .SetColumns(c => c.Update_by == terminatedBy)
                .SetColumns(c => c.Update_time == DateTime.Now)
                .Where(c => c.ContractId == contractId)
                .ExecuteCommandAsync();

            await Context.Updateable<ErpContractLifecycle>()
                .SetColumns(l => l.TerminatedDate == terminationDate)
                .SetColumns(l => l.TerminatedBy == terminatedBy)
                .SetColumns(l => l.TerminationReason == terminationReason)
                .SetColumns(l => l.Update_time == DateTime.Now)
                .Where(l => l.ContractId == contractId)
                .ExecuteCommandAsync();

            return result;
        }

        /// <summary>
        /// Get expiring contracts async
        /// </summary>
        public async Task<List<ErpContractListDto>> GetExpiringContractsAsync(int daysFromNow)
        {
            var today = DateTime.Today;
            var futureDate = today.AddDays(daysFromNow);

            return await Context.Queryable<ErpContract>()
                .LeftJoin<ErpContractFinancial>((c, f) => c.ContractId == f.ContractId)
                .Where((c, f) => c.Status == ContractStatus.Active && c.EndDate.HasValue && c.EndDate.Value <= futureDate && c.EndDate.Value >= today)
                .OrderBy((c, f) => c.EndDate)
                .Select((c, f) => new ErpContractListDto
                {
                    ContractId = c.ContractId,
                    ContractNumber = c.ContractNumber,
                    ContractTitle = c.ContractTitle,
                    CustomerId = c.CustomerId,
                    ContractType = c.ContractType,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    Status = c.Status,
                    TotalAmount = f.TotalAmount,
                    Currency = f.Currency
                })
                .ToListAsync();
        }

        /// <summary>
        /// Get contracts for selection async
        /// </summary>
        public async Task<List<ErpContractSelectDto>> GetSelectListAsync(ContractStatus? status = null)
        {
            var query = Context.Queryable<ErpContract>();
            if (status.HasValue)
            {
                query = query.Where(c => c.Status == status.Value);
            }

            return await query
                .OrderByDescending(c => c.Create_time)
                .Select(c => new ErpContractSelectDto
                {
                    ContractId = c.ContractId,
                    ContractNumber = c.ContractNumber,
                    ContractTitle = c.ContractTitle,
                    Status = c.Status
                })
                .ToListAsync();
        }

        /// <summary>
        /// Get contracts by customer async
        /// </summary>
        public async Task<List<ErpContractListDto>> GetByCustomerIdAsync(Guid customerId)
        {
            return await Context.Queryable<ErpContract>()
                .LeftJoin<ErpContractFinancial>((c, f) => c.ContractId == f.ContractId)
                .Where((c, f) => c.CustomerId == customerId)
                .OrderByDescending((c, f) => c.Create_time)
                .Select((c, f) => new ErpContractListDto
                {
                    ContractId = c.ContractId,
                    ContractNumber = c.ContractNumber,
                    ContractTitle = c.ContractTitle,
                    CustomerId = c.CustomerId,
                    ContractType = c.ContractType,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    Status = c.Status,
                    TotalAmount = f.TotalAmount,
                    Currency = f.Currency
                })
                .ToListAsync();
        }

        /// <summary>
        /// Get statistics async
        /// </summary>
        public async Task<ContractStatisticsDto> GetStatisticsAsync()
        {
            var contracts = await Context.Queryable<ErpContract>().ToListAsync();

            var totalValue = await Context.Queryable<ErpContract>()
                .LeftJoin<ErpContractFinancial>((c, f) => c.ContractId == f.ContractId)
                .Where((c, f) => c.Status == ContractStatus.Active)
                .SumAsync((c, f) => f.TotalAmount ?? 0);

            return new ContractStatisticsDto
            {
                TotalContracts = contracts.Count,
                ActiveContracts = contracts.Count(c => c.Status == ContractStatus.Active),
                DraftContracts = contracts.Count(c => c.Status == ContractStatus.Draft),
                SuspendedContracts = contracts.Count(c => c.Status == ContractStatus.Suspended),
                ExpiredContracts = contracts.Count(c => c.Status == ContractStatus.Expired),
                TotalValue = totalValue
            };
        }

        /// <summary>
        /// Check if contract number exists async
        /// </summary>
        public async Task<bool> ContractNumberExistsAsync(string contractNumber, Guid? excludeId = null)
        {
            var query = Context.Queryable<ErpContract>()
                .Where(c => c.ContractNumber == contractNumber);

            if (excludeId.HasValue)
            {
                query = query.Where(c => c.ContractId != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        /// <summary>
        /// Generate contract number async
        /// </summary>
        private async Task<string> GenerateContractNumberAsync()
        {
            var year = DateTime.Now.Year;
            var prefix = $"CNT-{year}-";

            var lastNumber = await Context.Queryable<ErpContract>()
                .ClearFilter() // include soft-deleted records to avoid duplicate numbers
                .Where(c => c.ContractNumber.StartsWith(prefix))
                .OrderByDescending(c => c.ContractNumber)
                .Select(c => c.ContractNumber)
                .FirstAsync();

            var nextNumber = 1;
            if (!string.IsNullOrEmpty(lastNumber))
            {
                var numberPart = lastNumber.Replace(prefix, "");
                if (int.TryParse(numberPart, out var parsed))
                {
                    nextNumber = parsed + 1;
                }
            }

            return $"{prefix}{nextNumber:D5}";
        }

        #endregion
    }
}
