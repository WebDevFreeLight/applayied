using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZR.Model.Contracts;
using ZR.Model.Contracts.Dto;
using ZR.Model.Contracts.Enums;

namespace ZR.Repository.Contracts
{
    /// <summary>
    /// Contract Amendment Repository Interface - واجهة مستودع تعديلات العقد
    /// </summary>
    public interface IContractAmendmentRepository : IBaseRepository<ErpContractAmendment>
    {
        /// <summary>
        /// Get amendments by contract ID
        /// </summary>
        List<ErpContractAmendmentListDto> GetByContractId(Guid contractId, AmendmentStatus? status = null);

        /// <summary>
        /// Get amendments by contract ID async
        /// </summary>
        Task<List<ErpContractAmendmentListDto>> GetByContractIdAsync(Guid contractId, AmendmentStatus? status = null);

        /// <summary>
        /// Get amendment by ID
        /// </summary>
        ErpContractAmendment GetById(Guid amendmentId);

        /// <summary>
        /// Get amendment by ID async
        /// </summary>
        Task<ErpContractAmendment> GetByIdAsync(Guid amendmentId);

        /// <summary>
        /// Create amendment
        /// </summary>
        Guid Create(ErpContractAmendment amendment);

        /// <summary>
        /// Create amendment async
        /// </summary>
        Task<Guid> CreateAsync(ErpContractAmendment amendment);

        /// <summary>
        /// Update amendment
        /// </summary>
        int UpdateAmendment(ErpContractAmendment amendment);

        /// <summary>
        /// Update amendment async
        /// </summary>
        Task<int> UpdateAmendmentAsync(ErpContractAmendment amendment);

        /// <summary>
        /// Delete amendment
        /// </summary>
        int Delete(Guid amendmentId);

        /// <summary>
        /// Delete amendment async
        /// </summary>
        Task<int> DeleteAsync(Guid amendmentId);

        /// <summary>
        /// Delete amendments by contract ID
        /// </summary>
        int DeleteByContractId(Guid contractId);

        /// <summary>
        /// Delete amendments by contract ID async
        /// </summary>
        Task<int> DeleteByContractIdAsync(Guid contractId);

        /// <summary>
        /// Update status
        /// </summary>
        int UpdateStatus(Guid amendmentId, AmendmentStatus status, string updatedBy);

        /// <summary>
        /// Update status async
        /// </summary>
        Task<int> UpdateStatusAsync(Guid amendmentId, AmendmentStatus status, string updatedBy);

        /// <summary>
        /// Approve amendment
        /// </summary>
        int Approve(Guid amendmentId, string approvedBy);

        /// <summary>
        /// Approve amendment async
        /// </summary>
        Task<int> ApproveAsync(Guid amendmentId, string approvedBy);

        /// <summary>
        /// Reject amendment
        /// </summary>
        int Reject(Guid amendmentId, string rejectedBy, string reason);

        /// <summary>
        /// Reject amendment async
        /// </summary>
        Task<int> RejectAsync(Guid amendmentId, string rejectedBy, string reason);

        /// <summary>
        /// Cancel amendment
        /// </summary>
        int Cancel(Guid amendmentId, string cancelledBy);

        /// <summary>
        /// Cancel amendment async
        /// </summary>
        Task<int> CancelAsync(Guid amendmentId, string cancelledBy);

        /// <summary>
        /// Get pending amendments count
        /// </summary>
        int GetPendingAmendmentsCount(Guid? contractId = null);

        /// <summary>
        /// Get pending amendments count async
        /// </summary>
        Task<int> GetPendingAmendmentsCountAsync(Guid? contractId = null);

        /// <summary>
        /// Get recent amendments
        /// </summary>
        List<ErpContractAmendmentListDto> GetRecentAmendments(Guid contractId, int count = 5);

        /// <summary>
        /// Get recent amendments async
        /// </summary>
        Task<List<ErpContractAmendmentListDto>> GetRecentAmendmentsAsync(Guid contractId, int count = 5);
    }
}
