using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZR.Model;
using ZR.Model.Contracts;
using ZR.Model.Contracts.Dto;
using ZR.Model.Contracts.Enums;

namespace ZR.Repository.Contracts
{
    /// <summary>
    /// Contract Repository Interface - واجهة مستودع العقود
    /// </summary>
    public interface IContractRepository : IBaseRepository<ErpContract>
    {
        /// <summary>
        /// Get contracts list with pagination
        /// </summary>
        PagedInfo<ErpContractListDto> GetList(ErpContractQueryDto query, PagerInfo pager);

        /// <summary>
        /// Get contract by ID with full details
        /// </summary>
        ErpContractDetailDto GetById(Guid contractId);

        /// <summary>
        /// Get contract by ID with full details async
        /// </summary>
        Task<ErpContractDetailDto> GetByIdAsync(Guid contractId);

        /// <summary>
        /// Get contract entity by ID
        /// </summary>
        ErpContract GetEntityById(Guid contractId);

        /// <summary>
        /// Get contract entity by ID async
        /// </summary>
        Task<ErpContract> GetEntityByIdAsync(Guid contractId);

        /// <summary>
        /// Create contract
        /// </summary>
        Guid Create(ErpContract contract);

        /// <summary>
        /// Create contract async
        /// </summary>
        Task<Guid> CreateAsync(ErpContract contract);

        /// <summary>
        /// Update contract
        /// </summary>
        int UpdateContract(ErpContract contract);

        /// <summary>
        /// Update contract async
        /// </summary>
        Task<int> UpdateContractAsync(ErpContract contract);

        /// <summary>
        /// Delete contracts
        /// </summary>
        int Delete(Guid[] contractIds);

        /// <summary>
        /// Delete contracts async
        /// </summary>
        Task<int> DeleteAsync(Guid[] contractIds);

        /// <summary>
        /// Update contract status
        /// </summary>
        int UpdateStatus(Guid contractId, ContractStatus status, string updatedBy);

        /// <summary>
        /// Update contract status async
        /// </summary>
        Task<int> UpdateStatusAsync(Guid contractId, ContractStatus status, string updatedBy);

        /// <summary>
        /// Activate contract
        /// </summary>
        int Activate(Guid contractId, string activatedBy, DateTime? activationDate);

        /// <summary>
        /// Activate contract async
        /// </summary>
        Task<int> ActivateAsync(Guid contractId, string activatedBy, DateTime? activationDate);

        /// <summary>
        /// Terminate contract
        /// </summary>
        int Terminate(Guid contractId, TerminationReason terminationReason, string terminatedBy, DateTime terminationDate);

        /// <summary>
        /// Terminate contract async
        /// </summary>
        Task<int> TerminateAsync(Guid contractId, TerminationReason terminationReason, string terminatedBy, DateTime terminationDate);

        /// <summary>
        /// Get expiring contracts
        /// </summary>
        List<ErpContractListDto> GetExpiringContracts(int daysFromNow);

        /// <summary>
        /// Get expiring contracts async
        /// </summary>
        Task<List<ErpContractListDto>> GetExpiringContractsAsync(int daysFromNow);

        /// <summary>
        /// Get contracts for selection
        /// </summary>
        List<ErpContractSelectDto> GetSelectList(ContractStatus? status = null);

        /// <summary>
        /// Get contracts for selection async
        /// </summary>
        Task<List<ErpContractSelectDto>> GetSelectListAsync(ContractStatus? status = null);

        /// <summary>
        /// Get contracts by customer
        /// </summary>
        List<ErpContractListDto> GetByCustomerId(Guid customerId);

        /// <summary>
        /// Get contracts by customer async
        /// </summary>
        Task<List<ErpContractListDto>> GetByCustomerIdAsync(Guid customerId);

        /// <summary>
        /// Get statistics
        /// </summary>
        ContractStatisticsDto GetStatistics();

        /// <summary>
        /// Get statistics async
        /// </summary>
        Task<ContractStatisticsDto> GetStatisticsAsync();

        /// <summary>
        /// Check if contract number exists
        /// </summary>
        bool ContractNumberExists(string contractNumber, Guid? excludeId = null);

        /// <summary>
        /// Check if contract number exists async
        /// </summary>
        Task<bool> ContractNumberExistsAsync(string contractNumber, Guid? excludeId = null);
    }
}
