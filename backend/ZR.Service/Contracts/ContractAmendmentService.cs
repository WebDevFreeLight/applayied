using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.Attribute;
using Mapster;
using ZR.Model.Contracts;
using ZR.Model.Contracts.Dto;
using ZR.Model.Contracts.Enums;
using ZR.Repository.Contracts;
using ZR.Service.Contracts.IService;
using ZR.ServiceCore;
using JsonSerializer = System.Text.Json.JsonSerializer;
using JsonElement = System.Text.Json.JsonElement;

namespace ZR.Service.Contracts
{
    /// <summary>
    /// Contract Amendment Service - خدمة تعديلات العقد
    /// Business logic layer for contract amendment operations
    /// Updated to follow Finance module pattern with BaseService and DI
    /// </summary>
    [AppService(ServiceType = typeof(IContractAmendmentService), ServiceLifetime = LifeTime.Transient)]
    public class ContractAmendmentService : BaseService<ErpContractAmendment>, IContractAmendmentService
    {
        private readonly IContractAmendmentRepository _amendmentRepository;
        private readonly IContractRepository _contractRepository;

        public ContractAmendmentService(
            IContractAmendmentRepository amendmentRepository,
            IContractRepository contractRepository)
        {
            _amendmentRepository = amendmentRepository;
            _contractRepository = contractRepository;
        }

        /// <summary>
        /// Get amendments by contract ID
        /// </summary>
        public List<ErpContractAmendmentListDto> GetByContractId(Guid contractId, AmendmentStatus? status = null)
        {
            return _amendmentRepository.GetByContractId(contractId, status);
        }

        /// <summary>
        /// Get amendments by contract ID async
        /// </summary>
        public async Task<List<ErpContractAmendmentListDto>> GetByContractIdAsync(Guid contractId, AmendmentStatus? status = null)
        {
            return await _amendmentRepository.GetByContractIdAsync(contractId, status);
        }

        /// <summary>
        /// Get amendment by ID
        /// </summary>
        public ErpContractAmendment GetById(Guid amendmentId)
        {
            return _amendmentRepository.GetById(amendmentId);
        }

        /// <summary>
        /// Get amendment by ID async
        /// </summary>
        public async Task<ErpContractAmendment> GetByIdAsync(Guid amendmentId)
        {
            return await _amendmentRepository.GetByIdAsync(amendmentId);
        }

        /// <summary>
        /// Create amendment
        /// Captures original values from contract and extension tables
        /// </summary>
        public Guid Create(CreateErpContractAmendmentDto dto, string createdBy)
        {
            var contract = _contractRepository.GetEntityById(dto.ContractId);
            if (contract == null)
            {
                throw new InvalidOperationException("Contract not found");
            }

            // Only allow amendments on active contracts
            if (contract.Status != ContractStatus.Active)
            {
                throw new InvalidOperationException("Can only create amendments for active contracts");
            }

            // Load extension data using Context from BaseService
            var financial = Context.Queryable<ErpContractFinancial>().First(f => f.ContractId == dto.ContractId);

            var amendment = dto.Adapt<ErpContractAmendment>();
            amendment.Status = AmendmentStatus.Draft;
            amendment.Create_by = createdBy;
            amendment.RequestedBy = createdBy;
            amendment.RequestedDate = DateTime.Now;
            amendment.Currency = financial?.Currency ?? "SAR";

            // Store original values in ChangesJson based on amendment type
            var changes = new Dictionary<string, object>();
            switch (dto.AmendmentType)
            {
                case AmendmentType.PriceChange:
                    changes["total_amount"] = new { previous = financial?.TotalAmount ?? 0, @new = dto.NewAmount };
                    changes["base_amount"] = new { previous = financial?.BaseAmount ?? 0, @new = dto.NewAmount };
                    break;
                case AmendmentType.DurationExtension:
                case AmendmentType.DurationReduction:
                    changes["end_date"] = new { previous = contract.EndDate?.ToString("yyyy-MM-dd"), @new = dto.NewEndDate?.ToString("yyyy-MM-dd") };
                    break;
                case AmendmentType.TermsChange:
                    changes["description"] = new { previous = contract.Description ?? "", @new = dto.NewValue ?? "" };
                    break;
                case AmendmentType.UnitAddition:
                case AmendmentType.UnitRemoval:
                    changes["unit_id"] = new { previous = (object)null, @new = dto.UnitId };
                    break;
            }
            amendment.ChangesJson = JsonSerializer.Serialize(changes);

            return _amendmentRepository.Create(amendment);
        }

        /// <summary>
        /// Create amendment async
        /// </summary>
        public async Task<Guid> CreateAsync(CreateErpContractAmendmentDto dto, string createdBy)
        {
            var contract = await _contractRepository.GetEntityByIdAsync(dto.ContractId);
            if (contract == null)
            {
                throw new InvalidOperationException("Contract not found");
            }

            if (contract.Status != ContractStatus.Active)
            {
                throw new InvalidOperationException("Can only create amendments for active contracts");
            }

            var financial = await Context.Queryable<ErpContractFinancial>().FirstAsync(f => f.ContractId == dto.ContractId);

            var amendment = dto.Adapt<ErpContractAmendment>();
            amendment.Status = AmendmentStatus.Draft;
            amendment.Create_by = createdBy;
            amendment.RequestedBy = createdBy;
            amendment.RequestedDate = DateTime.Now;
            amendment.Currency = financial?.Currency ?? "SAR";

            var changes = new Dictionary<string, object>();
            switch (dto.AmendmentType)
            {
                case AmendmentType.PriceChange:
                    changes["total_amount"] = new { previous = financial?.TotalAmount ?? 0, @new = dto.NewAmount };
                    changes["base_amount"] = new { previous = financial?.BaseAmount ?? 0, @new = dto.NewAmount };
                    break;
                case AmendmentType.DurationExtension:
                case AmendmentType.DurationReduction:
                    changes["end_date"] = new { previous = contract.EndDate?.ToString("yyyy-MM-dd"), @new = dto.NewEndDate?.ToString("yyyy-MM-dd") };
                    break;
                case AmendmentType.TermsChange:
                    changes["description"] = new { previous = contract.Description ?? "", @new = dto.NewValue ?? "" };
                    break;
                case AmendmentType.UnitAddition:
                case AmendmentType.UnitRemoval:
                    changes["unit_id"] = new { previous = (object)null, @new = dto.UnitId };
                    break;
            }
            amendment.ChangesJson = JsonSerializer.Serialize(changes);

            return await _amendmentRepository.CreateAsync(amendment);
        }

        /// <summary>
        /// Update amendment
        /// </summary>
        public int Update(UpdateErpContractAmendmentDto dto, string updatedBy)
        {
            var existing = _amendmentRepository.GetById(dto.AmendmentId);
            if (existing == null) return 0;

            // Only allow updates to draft amendments
            if (existing.Status != AmendmentStatus.Draft)
            {
                throw new InvalidOperationException("Can only update draft amendments");
            }

            var amendment = dto.Adapt<ErpContractAmendment>();
            amendment.ContractId = existing.ContractId;
            amendment.AmendmentNumber = existing.AmendmentNumber;
            amendment.AmendmentReference = existing.AmendmentReference;
            amendment.ChangesJson = existing.ChangesJson; // Preserve original changes
            amendment.Update_by = updatedBy;

            return _amendmentRepository.UpdateAmendment(amendment);
        }

        /// <summary>
        /// Update amendment async
        /// </summary>
        public async Task<int> UpdateAsync(UpdateErpContractAmendmentDto dto, string updatedBy)
        {
            var existing = await _amendmentRepository.GetByIdAsync(dto.AmendmentId);
            if (existing == null) return 0;

            if (existing.Status != AmendmentStatus.Draft)
            {
                throw new InvalidOperationException("Can only update draft amendments");
            }

            var amendment = dto.Adapt<ErpContractAmendment>();
            amendment.ContractId = existing.ContractId;
            amendment.AmendmentNumber = existing.AmendmentNumber;
            amendment.AmendmentReference = existing.AmendmentReference;
            amendment.ChangesJson = existing.ChangesJson;
            amendment.Update_by = updatedBy;

            return await _amendmentRepository.UpdateAmendmentAsync(amendment);
        }

        /// <summary>
        /// Delete amendment
        /// </summary>
        public int Delete(Guid amendmentId)
        {
            var existing = _amendmentRepository.GetById(amendmentId);
            if (existing == null) return 0;

            // Only allow deletion of draft amendments
            if (existing.Status != AmendmentStatus.Draft)
            {
                throw new InvalidOperationException("Can only delete draft amendments");
            }

            return _amendmentRepository.Delete(amendmentId);
        }

        /// <summary>
        /// Delete amendment async
        /// </summary>
        public async Task<int> DeleteAsync(Guid amendmentId)
        {
            var existing = await _amendmentRepository.GetByIdAsync(amendmentId);
            if (existing == null) return 0;

            if (existing.Status != AmendmentStatus.Draft)
            {
                throw new InvalidOperationException("Can only delete draft amendments");
            }

            return await _amendmentRepository.DeleteAsync(amendmentId);
        }

        /// <summary>
        /// Submit amendment for approval
        /// </summary>
        public int SubmitForApproval(Guid amendmentId, string submittedBy)
        {
            var amendment = _amendmentRepository.GetById(amendmentId);
            if (amendment == null) return 0;

            if (amendment.Status != AmendmentStatus.Draft)
            {
                throw new InvalidOperationException("Only draft amendments can be submitted for approval");
            }

            return _amendmentRepository.UpdateStatus(amendmentId, AmendmentStatus.PendingApproval, submittedBy);
        }

        /// <summary>
        /// Submit amendment for approval async
        /// </summary>
        public async Task<int> SubmitForApprovalAsync(Guid amendmentId, string submittedBy)
        {
            var amendment = await _amendmentRepository.GetByIdAsync(amendmentId);
            if (amendment == null) return 0;

            if (amendment.Status != AmendmentStatus.Draft)
            {
                throw new InvalidOperationException("Only draft amendments can be submitted for approval");
            }

            return await _amendmentRepository.UpdateStatusAsync(amendmentId, AmendmentStatus.PendingApproval, submittedBy);
        }

        /// <summary>
        /// Approve amendment
        /// Applies the changes to the contract and extension tables
        /// </summary>
        public int Approve(Guid amendmentId, string approvedBy)
        {
            var amendment = _amendmentRepository.GetById(amendmentId);
            if (amendment == null) return 0;

            if (amendment.Status != AmendmentStatus.PendingApproval)
            {
                throw new InvalidOperationException("Only pending approval amendments can be approved");
            }

            // Apply the amendment to the contract
            ApplyAmendmentToContract(amendment);

            return _amendmentRepository.Approve(amendmentId, approvedBy);
        }

        /// <summary>
        /// Approve amendment async
        /// </summary>
        public async Task<int> ApproveAsync(Guid amendmentId, string approvedBy)
        {
            var amendment = await _amendmentRepository.GetByIdAsync(amendmentId);
            if (amendment == null) return 0;

            if (amendment.Status != AmendmentStatus.PendingApproval)
            {
                throw new InvalidOperationException("Only pending approval amendments can be approved");
            }

            await ApplyAmendmentToContractAsync(amendment);

            return await _amendmentRepository.ApproveAsync(amendmentId, approvedBy);
        }

        /// <summary>
        /// Reject amendment
        /// </summary>
        public int Reject(Guid amendmentId, string rejectedBy, string reason)
        {
            var amendment = _amendmentRepository.GetById(amendmentId);
            if (amendment == null) return 0;

            if (amendment.Status != AmendmentStatus.PendingApproval)
            {
                throw new InvalidOperationException("Only pending approval amendments can be rejected");
            }

            return _amendmentRepository.Reject(amendmentId, rejectedBy, reason);
        }

        /// <summary>
        /// Reject amendment async
        /// </summary>
        public async Task<int> RejectAsync(Guid amendmentId, string rejectedBy, string reason)
        {
            var amendment = await _amendmentRepository.GetByIdAsync(amendmentId);
            if (amendment == null) return 0;

            if (amendment.Status != AmendmentStatus.PendingApproval)
            {
                throw new InvalidOperationException("Only pending approval amendments can be rejected");
            }

            return await _amendmentRepository.RejectAsync(amendmentId, rejectedBy, reason);
        }

        /// <summary>
        /// Cancel amendment
        /// </summary>
        public int Cancel(Guid amendmentId, string cancelledBy)
        {
            var amendment = _amendmentRepository.GetById(amendmentId);
            if (amendment == null) return 0;

            if (amendment.Status == AmendmentStatus.Approved)
            {
                throw new InvalidOperationException("Cannot cancel approved amendments");
            }

            return _amendmentRepository.Cancel(amendmentId, cancelledBy);
        }

        /// <summary>
        /// Cancel amendment async
        /// </summary>
        public async Task<int> CancelAsync(Guid amendmentId, string cancelledBy)
        {
            var amendment = await _amendmentRepository.GetByIdAsync(amendmentId);
            if (amendment == null) return 0;

            if (amendment.Status == AmendmentStatus.Approved)
            {
                throw new InvalidOperationException("Cannot cancel approved amendments");
            }

            return await _amendmentRepository.CancelAsync(amendmentId, cancelledBy);
        }

        /// <summary>
        /// Get pending amendments count
        /// </summary>
        public int GetPendingAmendmentsCount(Guid? contractId = null)
        {
            return _amendmentRepository.GetPendingAmendmentsCount(contractId);
        }

        /// <summary>
        /// Get pending amendments count async
        /// </summary>
        public async Task<int> GetPendingAmendmentsCountAsync(Guid? contractId = null)
        {
            return await _amendmentRepository.GetPendingAmendmentsCountAsync(contractId);
        }

        /// <summary>
        /// Get recent amendments
        /// </summary>
        public List<ErpContractAmendmentListDto> GetRecentAmendments(Guid contractId, int count = 5)
        {
            return _amendmentRepository.GetRecentAmendments(contractId, count);
        }

        /// <summary>
        /// Get recent amendments async
        /// </summary>
        public async Task<List<ErpContractAmendmentListDto>> GetRecentAmendmentsAsync(Guid contractId, int count = 5)
        {
            return await _amendmentRepository.GetRecentAmendmentsAsync(contractId, count);
        }

        #region Private Helper Methods

        /// <summary>
        /// Apply amendment changes to contract and extension tables
        /// Parses ChangesJson to determine what to update
        /// </summary>
        private void ApplyAmendmentToContract(ErpContractAmendment amendment)
        {
            var contract = _contractRepository.GetEntityById(amendment.ContractId);
            if (contract == null) return;

            if (string.IsNullOrEmpty(amendment.ChangesJson)) return;

            var changes = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(amendment.ChangesJson);
            if (changes == null) return;

            switch (amendment.AmendmentType)
            {
                case AmendmentType.PriceChange:
                    if (changes.TryGetValue("total_amount", out var totalAmountChange))
                    {
                        var newValue = totalAmountChange.GetProperty("new").GetDecimal();
                        Context.Updateable<ErpContractFinancial>()
                            .SetColumns(f => f.TotalAmount == newValue)
                            .SetColumns(f => f.Update_time == DateTime.Now)
                            .Where(f => f.ContractId == amendment.ContractId)
                            .ExecuteCommand();
                    }
                    if (changes.TryGetValue("base_amount", out var baseAmountChange))
                    {
                        var newValue = baseAmountChange.GetProperty("new").GetDecimal();
                        Context.Updateable<ErpContractFinancial>()
                            .SetColumns(f => f.BaseAmount == newValue)
                            .SetColumns(f => f.Update_time == DateTime.Now)
                            .Where(f => f.ContractId == amendment.ContractId)
                            .ExecuteCommand();
                    }
                    break;

                case AmendmentType.DurationExtension:
                case AmendmentType.DurationReduction:
                    if (changes.TryGetValue("end_date", out var endDateChange))
                    {
                        var newDateStr = endDateChange.GetProperty("new").GetString();
                        if (DateTime.TryParse(newDateStr, out var newEndDate))
                        {
                            contract.EndDate = newEndDate;
                        }
                    }
                    break;

                case AmendmentType.TermsChange:
                    if (changes.TryGetValue("description", out var descChange))
                    {
                        contract.Description = descChange.GetProperty("new").GetString();
                    }
                    break;
            }

            contract.Update_by = amendment.Update_by ?? amendment.Create_by;
            _contractRepository.UpdateContract(contract);
        }

        /// <summary>
        /// Apply amendment changes to contract async
        /// </summary>
        private async Task ApplyAmendmentToContractAsync(ErpContractAmendment amendment)
        {
            var contract = await _contractRepository.GetEntityByIdAsync(amendment.ContractId);
            if (contract == null) return;

            if (string.IsNullOrEmpty(amendment.ChangesJson)) return;

            var changes = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(amendment.ChangesJson);
            if (changes == null) return;

            switch (amendment.AmendmentType)
            {
                case AmendmentType.PriceChange:
                    if (changes.TryGetValue("total_amount", out var totalAmountChange))
                    {
                        var newValue = totalAmountChange.GetProperty("new").GetDecimal();
                        await Context.Updateable<ErpContractFinancial>()
                            .SetColumns(f => f.TotalAmount == newValue)
                            .SetColumns(f => f.Update_time == DateTime.Now)
                            .Where(f => f.ContractId == amendment.ContractId)
                            .ExecuteCommandAsync();
                    }
                    if (changes.TryGetValue("base_amount", out var baseAmountChange))
                    {
                        var newValue = baseAmountChange.GetProperty("new").GetDecimal();
                        await Context.Updateable<ErpContractFinancial>()
                            .SetColumns(f => f.BaseAmount == newValue)
                            .SetColumns(f => f.Update_time == DateTime.Now)
                            .Where(f => f.ContractId == amendment.ContractId)
                            .ExecuteCommandAsync();
                    }
                    break;

                case AmendmentType.DurationExtension:
                case AmendmentType.DurationReduction:
                    if (changes.TryGetValue("end_date", out var endDateChange))
                    {
                        var newDateStr = endDateChange.GetProperty("new").GetString();
                        if (DateTime.TryParse(newDateStr, out var newEndDate))
                        {
                            contract.EndDate = newEndDate;
                        }
                    }
                    break;

                case AmendmentType.TermsChange:
                    if (changes.TryGetValue("description", out var descChange))
                    {
                        contract.Description = descChange.GetProperty("new").GetString();
                    }
                    break;
            }

            contract.Update_by = amendment.Update_by ?? amendment.Create_by;
            await _contractRepository.UpdateContractAsync(contract);
        }

        #endregion
    }
}
