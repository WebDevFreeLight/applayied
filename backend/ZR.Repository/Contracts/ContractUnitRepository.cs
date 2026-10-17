using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.Attribute;
using SqlSugar;
using ZR.Model.Contracts;
using ZR.Model.Contracts.Dto;
using ZR.Repository;

namespace ZR.Repository.Contracts
{
    /// <summary>
    /// Contract Unit Repository - مستودع وحدات العقد
    /// Updated to follow Finance module pattern with DI
    /// </summary>
    [AppService(ServiceType = typeof(IContractUnitRepository), ServiceLifetime = LifeTime.Transient)]
    public class ContractUnitRepository : BaseRepository<ErpContractUnit>, IContractUnitRepository
    {
        /// <summary>
        /// Get units by contract ID
        /// </summary>
        public List<ErpContractUnitListDto> GetByContractId(Guid contractId, bool? isActive = null)
        {
            var query = Context.Queryable<ErpContractUnit>()
                .Where(u => u.ContractId == contractId);

            if (isActive.HasValue)
            {
                query = query.Where(u => u.IsActive == isActive.Value);
            }

            return query
                .OrderBy(u => u.SequenceNumber)
                .Select(u => new ErpContractUnitListDto
                {
                    UnitId = u.UnitId,
                    ContractId = u.ContractId,
                    UnitCode = u.UnitCode,
                    UnitType = u.UnitType,
                    UnitName = u.UnitName,
                    UnitNameAr = u.UnitNameAr,
                    Description = u.Description,
                    Area = u.Area,
                    AreaUnit = u.AreaUnit,
                    BasePrice = u.BasePrice,
                    Currency = u.Currency,
                    IsActive = u.IsActive,
                    StartDate = u.StartDate,
                    EndDate = u.EndDate,
                    Create_time = u.Create_time,
                    BlvShopId = u.BlvShopId,
                    PricingType = u.PricingType,
                    PricingLocked = u.PricingLocked
                })
                .ToList();
        }

        /// <summary>
        /// Get unit by ID
        /// </summary>
        public ErpContractUnit GetById(Guid unitId)
        {
            return Context.Queryable<ErpContractUnit>()
                .Where(u => u.UnitId == unitId)
                .First();
        }

        /// <summary>
        /// Create unit
        /// </summary>
        public Guid Create(ErpContractUnit unit)
        {
            unit.UnitId = Guid.NewGuid();
            unit.Create_time = DateTime.Now;

            // Calculate price
            if (unit.PricePerArea.HasValue && unit.Area.HasValue)
            {
                unit.CalculatedPrice = unit.PricePerArea.Value * unit.Area.Value;
            }
            else
            {
                unit.CalculatedPrice = unit.BasePrice;
            }

            // Get next sequence number
            var maxSeq = Context.Queryable<ErpContractUnit>()
                .Where(u => u.ContractId == unit.ContractId)
                .Max(u => (int?)u.SequenceNumber) ?? 0;
            unit.SequenceNumber = maxSeq + 1;

            Context.Insertable(unit).ExecuteCommand();
            return unit.UnitId;
        }

        /// <summary>
        /// Get unit by ID async
        /// </summary>
        public async Task<ErpContractUnit> GetByIdAsync(Guid unitId)
        {
            return await Context.Queryable<ErpContractUnit>()
                .Where(u => u.UnitId == unitId)
                .FirstAsync();
        }

        /// <summary>
        /// Get units by contract ID async
        /// </summary>
        public async Task<List<ErpContractUnitListDto>> GetByContractIdAsync(Guid contractId, bool? isActive = null)
        {
            var query = Context.Queryable<ErpContractUnit>()
                .Where(u => u.ContractId == contractId);

            if (isActive.HasValue)
            {
                query = query.Where(u => u.IsActive == isActive.Value);
            }

            return await query
                .OrderBy(u => u.SequenceNumber)
                .Select(u => new ErpContractUnitListDto
                {
                    UnitId = u.UnitId,
                    ContractId = u.ContractId,
                    UnitCode = u.UnitCode,
                    UnitType = u.UnitType,
                    UnitName = u.UnitName,
                    UnitNameAr = u.UnitNameAr,
                    Description = u.Description,
                    Area = u.Area,
                    AreaUnit = u.AreaUnit,
                    BasePrice = u.BasePrice,
                    Currency = u.Currency,
                    IsActive = u.IsActive,
                    StartDate = u.StartDate,
                    EndDate = u.EndDate,
                    Create_time = u.Create_time,
                    BlvShopId = u.BlvShopId,
                    PricingType = u.PricingType,
                    PricingLocked = u.PricingLocked
                })
                .ToListAsync();
        }

        /// <summary>
        /// Create unit async
        /// </summary>
        public async Task<Guid> CreateAsync(ErpContractUnit unit)
        {
            unit.UnitId = Guid.NewGuid();
            unit.Create_time = DateTime.Now;

            // Calculate price
            if (unit.PricePerArea.HasValue && unit.Area.HasValue)
            {
                unit.CalculatedPrice = unit.PricePerArea.Value * unit.Area.Value;
            }
            else
            {
                unit.CalculatedPrice = unit.BasePrice;
            }

            // Get next sequence number
            var maxSeq = await Context.Queryable<ErpContractUnit>()
                .Where(u => u.ContractId == unit.ContractId)
                .MaxAsync(u => (int?)u.SequenceNumber) ?? 0;
            unit.SequenceNumber = maxSeq + 1;

            await Context.Insertable(unit).ExecuteCommandAsync();
            return unit.UnitId;
        }

        /// <summary>
        /// Update unit
        /// </summary>
        public int UpdateUnit(ErpContractUnit unit)
        {
            unit.Update_time = DateTime.Now;

            // Recalculate price
            if (unit.PricePerArea.HasValue && unit.Area.HasValue)
            {
                unit.CalculatedPrice = unit.PricePerArea.Value * unit.Area.Value;
            }
            else
            {
                unit.CalculatedPrice = unit.BasePrice;
            }

            return Context.Updateable(unit)
                .IgnoreColumns(u => new { u.UnitId, u.ContractId, u.Create_by, u.Create_time })
                .Where(u => u.UnitId == unit.UnitId)
                .ExecuteCommand();
        }

        /// <summary>
        /// Delete unit
        /// </summary>
        public int Delete(Guid unitId)
        {
            return Context.Deleteable<ErpContractUnit>()
                .Where(u => u.UnitId == unitId)
                .ExecuteCommand();
        }

        /// <summary>
        /// Delete units by contract ID
        /// </summary>
        public int DeleteByContractId(Guid contractId)
        {
            return Context.Deleteable<ErpContractUnit>()
                .Where(u => u.ContractId == contractId)
                .ExecuteCommand();
        }

        /// <summary>
        /// Toggle active status
        /// </summary>
        public int ToggleActive(Guid unitId)
        {
            var unit = GetById(unitId);
            if (unit == null) return 0;

            return Context.Updateable<ErpContractUnit>()
                .SetColumns(u => u.IsActive == !unit.IsActive)
                .SetColumns(u => u.Update_time == DateTime.Now)
                .Where(u => u.UnitId == unitId)
                .ExecuteCommand();
        }

        /// <summary>
        /// Check if unit code exists in contract
        /// </summary>
        public bool UnitCodeExists(Guid contractId, string unitCode, Guid? excludeId = null)
        {
            var query = Context.Queryable<ErpContractUnit>()
                .Where(u => u.ContractId == contractId && u.UnitCode == unitCode);

            if (excludeId.HasValue)
            {
                query = query.Where(u => u.UnitId != excludeId.Value);
            }

            return query.Any();
        }

        /// <summary>
        /// Calculate total price for contract units
        /// </summary>
        public decimal CalculateTotalPrice(Guid contractId)
        {
            return Context.Queryable<ErpContractUnit>()
                .Where(u => u.ContractId == contractId && u.IsActive)
                .Sum(u => u.CalculatedPrice ?? u.BasePrice ?? 0);
        }

        /// <summary>
        /// Get active units count
        /// </summary>
        public int GetActiveUnitsCount(Guid contractId)
        {
            return Context.Queryable<ErpContractUnit>()
                .Where(u => u.ContractId == contractId && u.IsActive)
                .Count();
        }

        #region Async Methods

        /// <summary>
        /// Update unit async
        /// </summary>
        public async Task<int> UpdateUnitAsync(ErpContractUnit unit)
        {
            unit.Update_time = DateTime.Now;

            if (unit.PricePerArea.HasValue && unit.Area.HasValue)
            {
                unit.CalculatedPrice = unit.PricePerArea.Value * unit.Area.Value;
            }
            else
            {
                unit.CalculatedPrice = unit.BasePrice;
            }

            return await Context.Updateable(unit)
                .IgnoreColumns(u => new { u.UnitId, u.ContractId, u.Create_by, u.Create_time })
                .Where(u => u.UnitId == unit.UnitId)
                .ExecuteCommandAsync();
        }

        /// <summary>
        /// Delete unit async
        /// </summary>
        public async Task<int> DeleteAsync(Guid unitId)
        {
            return await Context.Deleteable<ErpContractUnit>()
                .Where(u => u.UnitId == unitId)
                .ExecuteCommandAsync();
        }

        /// <summary>
        /// Delete units by contract ID async
        /// </summary>
        public async Task<int> DeleteByContractIdAsync(Guid contractId)
        {
            return await Context.Deleteable<ErpContractUnit>()
                .Where(u => u.ContractId == contractId)
                .ExecuteCommandAsync();
        }

        /// <summary>
        /// Toggle active status async
        /// </summary>
        public async Task<int> ToggleActiveAsync(Guid unitId)
        {
            var unit = await GetByIdAsync(unitId);
            if (unit == null) return 0;

            return await Context.Updateable<ErpContractUnit>()
                .SetColumns(u => u.IsActive == !unit.IsActive)
                .SetColumns(u => u.Update_time == DateTime.Now)
                .Where(u => u.UnitId == unitId)
                .ExecuteCommandAsync();
        }

        /// <summary>
        /// Check if unit code exists in contract async
        /// </summary>
        public async Task<bool> UnitCodeExistsAsync(Guid contractId, string unitCode, Guid? excludeId = null)
        {
            var query = Context.Queryable<ErpContractUnit>()
                .Where(u => u.ContractId == contractId && u.UnitCode == unitCode);

            if (excludeId.HasValue)
            {
                query = query.Where(u => u.UnitId != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        /// <summary>
        /// Calculate total price for contract units async
        /// </summary>
        public async Task<decimal> CalculateTotalPriceAsync(Guid contractId)
        {
            return await Context.Queryable<ErpContractUnit>()
                .Where(u => u.ContractId == contractId && u.IsActive)
                .SumAsync(u => u.CalculatedPrice ?? u.BasePrice ?? 0);
        }

        /// <summary>
        /// Get active units count async
        /// </summary>
        public async Task<int> GetActiveUnitsCountAsync(Guid contractId)
        {
            return await Context.Queryable<ErpContractUnit>()
                .Where(u => u.ContractId == contractId && u.IsActive)
                .CountAsync();
        }

        #endregion

        /// <summary>
        /// Deactivate all units for a contract (used when contract is terminated/cancelled/expired)
        /// </summary>
        public int DeactivateByContractId(Guid contractId)
        {
            return Context.Updateable<ErpContractUnit>()
                .SetColumns(u => u.IsActive == false)
                .SetColumns(u => u.Update_time == DateTime.Now)
                .Where(u => u.ContractId == contractId && u.IsActive)
                .ExecuteCommand();
        }

        /// <summary>
        /// Reactivate all units for a contract (used when contract is resumed)
        /// </summary>
        public int ReactivateByContractId(Guid contractId)
        {
            return Context.Updateable<ErpContractUnit>()
                .SetColumns(u => u.IsActive == true)
                .SetColumns(u => u.Update_time == DateTime.Now)
                .Where(u => u.ContractId == contractId && !u.IsActive)
                .ExecuteCommand();
        }
    }
}
