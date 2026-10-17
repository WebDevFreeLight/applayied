using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZR.Model.Contracts;
using ZR.Model.Contracts.Dto;

namespace ZR.Repository.Contracts
{
    /// <summary>
    /// Contract Unit Repository Interface - واجهة مستودع وحدات العقد
    /// </summary>
    public interface IContractUnitRepository : IBaseRepository<ErpContractUnit>
    {
        /// <summary>
        /// Get units by contract ID
        /// </summary>
        List<ErpContractUnitListDto> GetByContractId(Guid contractId, bool? isActive = null);

        /// <summary>
        /// Get units by contract ID async
        /// </summary>
        Task<List<ErpContractUnitListDto>> GetByContractIdAsync(Guid contractId, bool? isActive = null);

        /// <summary>
        /// Get unit by ID
        /// </summary>
        ErpContractUnit GetById(Guid unitId);

        /// <summary>
        /// Get unit by ID async
        /// </summary>
        Task<ErpContractUnit> GetByIdAsync(Guid unitId);

        /// <summary>
        /// Create unit
        /// </summary>
        Guid Create(ErpContractUnit unit);

        /// <summary>
        /// Create unit async
        /// </summary>
        Task<Guid> CreateAsync(ErpContractUnit unit);

        /// <summary>
        /// Update unit
        /// </summary>
        int UpdateUnit(ErpContractUnit unit);

        /// <summary>
        /// Update unit async
        /// </summary>
        Task<int> UpdateUnitAsync(ErpContractUnit unit);

        /// <summary>
        /// Delete unit
        /// </summary>
        int Delete(Guid unitId);

        /// <summary>
        /// Delete unit async
        /// </summary>
        Task<int> DeleteAsync(Guid unitId);

        /// <summary>
        /// Delete units by contract ID
        /// </summary>
        int DeleteByContractId(Guid contractId);

        /// <summary>
        /// Delete units by contract ID async
        /// </summary>
        Task<int> DeleteByContractIdAsync(Guid contractId);

        /// <summary>
        /// Toggle active status
        /// </summary>
        int ToggleActive(Guid unitId);

        /// <summary>
        /// Toggle active status async
        /// </summary>
        Task<int> ToggleActiveAsync(Guid unitId);

        /// <summary>
        /// Check if unit code exists in contract
        /// </summary>
        bool UnitCodeExists(Guid contractId, string unitCode, Guid? excludeId = null);

        /// <summary>
        /// Check if unit code exists in contract async
        /// </summary>
        Task<bool> UnitCodeExistsAsync(Guid contractId, string unitCode, Guid? excludeId = null);

        /// <summary>
        /// Calculate total price for contract units
        /// </summary>
        decimal CalculateTotalPrice(Guid contractId);

        /// <summary>
        /// Calculate total price for contract units async
        /// </summary>
        Task<decimal> CalculateTotalPriceAsync(Guid contractId);

        /// <summary>
        /// Get active units count
        /// </summary>
        int GetActiveUnitsCount(Guid contractId);

        /// <summary>
        /// Get active units count async
        /// </summary>
        Task<int> GetActiveUnitsCountAsync(Guid contractId);

        /// <summary>
        /// Deactivate all units for a contract (used when contract is terminated/cancelled/expired)
        /// </summary>
        int DeactivateByContractId(Guid contractId);

        /// <summary>
        /// Reactivate all units for a contract (used when contract is resumed)
        /// </summary>
        int ReactivateByContractId(Guid contractId);
    }
}
