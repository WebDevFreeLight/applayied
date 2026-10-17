using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.Attribute;
using SqlSugar;
using ZR.Model.Contracts;
using ZR.Model.Contracts.Dto;
using ZR.Model.Contracts.Enums;
using ZR.Repository;

namespace ZR.Repository.Contracts
{
    /// <summary>
    /// Contract Amendment Repository - مستودع تعديلات العقد
    /// Updated to follow Finance module pattern with DI
    /// </summary>
    [AppService(ServiceType = typeof(IContractAmendmentRepository), ServiceLifetime = LifeTime.Transient)]
    public class ContractAmendmentRepository : BaseRepository<ErpContractAmendment>, IContractAmendmentRepository
    {
        /// <summary>
        /// Get amendments by contract ID
        /// </summary>
        public List<ErpContractAmendmentListDto> GetByContractId(Guid contractId, AmendmentStatus? status = null)
        {
            var query = Context.Queryable<ErpContractAmendment>()
                .Where(a => a.ContractId == contractId);

            if (status.HasValue)
            {
                query = query.Where(a => a.Status == status.Value);
            }

            return query
                .OrderByDescending(a => a.AmendmentNumber)
                .Select(a => new ErpContractAmendmentListDto
                {
                    AmendmentId = a.AmendmentId,
                    AmendmentType = a.AmendmentType,
                    Status = a.Status,
                    EffectiveDate = a.EffectiveDate,
                    Create_time = a.Create_time
                })
                .ToList();
        }

        /// <summary>
        /// Get amendment by ID
        /// </summary>
        public ErpContractAmendment GetById(Guid amendmentId)
        {
            return Context.Queryable<ErpContractAmendment>()
                .Where(a => a.AmendmentId == amendmentId)
                .First();
        }

        /// <summary>
        /// Create amendment
        /// </summary>
        public Guid Create(ErpContractAmendment amendment)
        {
            amendment.AmendmentId = Guid.NewGuid();
            amendment.Create_time = DateTime.Now;
            amendment.RequestedDate = DateTime.Now;

            // Generate amendment number
            var maxNumber = Context.Queryable<ErpContractAmendment>()
                .Where(a => a.ContractId == amendment.ContractId)
                .Max(a => (int?)a.AmendmentNumber) ?? 0;
            amendment.AmendmentNumber = maxNumber + 1;

            // Generate reference
            var contract = Context.Queryable<ErpContract>()
                .Where(c => c.ContractId == amendment.ContractId)
                .Select(c => c.ContractNumber)
                .First();
            amendment.AmendmentReference = $"{contract}-AMD-{amendment.AmendmentNumber:D3}";

            Context.Insertable(amendment).ExecuteCommand();
            return amendment.AmendmentId;
        }

        /// <summary>
        /// Update amendment
        /// </summary>
        public int UpdateAmendment(ErpContractAmendment amendment)
        {
            amendment.Update_time = DateTime.Now;
            return Context.Updateable(amendment)
                .IgnoreColumns(a => new { a.AmendmentId, a.ContractId, a.AmendmentNumber, a.AmendmentReference, a.Create_by, a.Create_time })
                .Where(a => a.AmendmentId == amendment.AmendmentId)
                .ExecuteCommand();
        }

        /// <summary>
        /// Delete amendment
        /// </summary>
        public int Delete(Guid amendmentId)
        {
            return Context.Deleteable<ErpContractAmendment>()
                .Where(a => a.AmendmentId == amendmentId)
                .ExecuteCommand();
        }

        /// <summary>
        /// Delete amendments by contract ID
        /// </summary>
        public int DeleteByContractId(Guid contractId)
        {
            return Context.Deleteable<ErpContractAmendment>()
                .Where(a => a.ContractId == contractId)
                .ExecuteCommand();
        }

        /// <summary>
        /// Update status
        /// </summary>
        public int UpdateStatus(Guid amendmentId, AmendmentStatus status, string updatedBy)
        {
            return Context.Updateable<ErpContractAmendment>()
                .SetColumns(a => a.Status == status)
                .SetColumns(a => a.Update_by == updatedBy)
                .SetColumns(a => a.Update_time == DateTime.Now)
                .Where(a => a.AmendmentId == amendmentId)
                .ExecuteCommand();
        }

        /// <summary>
        /// Approve amendment
        /// </summary>
        public int Approve(Guid amendmentId, string approvedBy)
        {
            return Context.Updateable<ErpContractAmendment>()
                .SetColumns(a => a.Status == AmendmentStatus.Approved)
                .SetColumns(a => a.ApprovedBy == approvedBy)
                .SetColumns(a => a.ApprovedDate == DateTime.Now)
                .SetColumns(a => a.Update_by == approvedBy)
                .SetColumns(a => a.Update_time == DateTime.Now)
                .Where(a => a.AmendmentId == amendmentId)
                .ExecuteCommand();
        }

        /// <summary>
        /// Reject amendment
        /// </summary>
        public int Reject(Guid amendmentId, string rejectedBy, string reason)
        {
            return Context.Updateable<ErpContractAmendment>()
                .SetColumns(a => a.Status == AmendmentStatus.Rejected)
                .SetColumns(a => a.RejectedBy == rejectedBy)
                .SetColumns(a => a.RejectedDate == DateTime.Now)
                .SetColumns(a => a.RejectionReason == reason)
                .SetColumns(a => a.Update_by == rejectedBy)
                .SetColumns(a => a.Update_time == DateTime.Now)
                .Where(a => a.AmendmentId == amendmentId)
                .ExecuteCommand();
        }

        /// <summary>
        /// Cancel amendment
        /// </summary>
        public int Cancel(Guid amendmentId, string cancelledBy)
        {
            return Context.Updateable<ErpContractAmendment>()
                .SetColumns(a => a.Status == AmendmentStatus.Cancelled)
                .SetColumns(a => a.Update_by == cancelledBy)
                .SetColumns(a => a.Update_time == DateTime.Now)
                .Where(a => a.AmendmentId == amendmentId)
                .ExecuteCommand();
        }

        /// <summary>
        /// Get pending amendments count
        /// </summary>
        public int GetPendingAmendmentsCount(Guid? contractId = null)
        {
            var query = Context.Queryable<ErpContractAmendment>()
                .Where(a => a.Status == AmendmentStatus.PendingApproval);

            if (contractId.HasValue)
            {
                query = query.Where(a => a.ContractId == contractId.Value);
            }

            return query.Count();
        }

        /// <summary>
        /// Get recent amendments
        /// </summary>
        public List<ErpContractAmendmentListDto> GetRecentAmendments(Guid contractId, int count = 5)
        {
            return Context.Queryable<ErpContractAmendment>()
                .Where(a => a.ContractId == contractId)
                .OrderByDescending(a => a.Create_time)
                .Take(count)
                .Select(a => new ErpContractAmendmentListDto
                {
                    AmendmentId = a.AmendmentId,
                    AmendmentType = a.AmendmentType,
                    Status = a.Status,
                    EffectiveDate = a.EffectiveDate,
                    Create_time = a.Create_time
                })
                .ToList();
        }

        #region Async Methods

        /// <summary>
        /// Get amendments by contract ID async
        /// </summary>
        public async Task<List<ErpContractAmendmentListDto>> GetByContractIdAsync(Guid contractId, AmendmentStatus? status = null)
        {
            var query = Context.Queryable<ErpContractAmendment>()
                .Where(a => a.ContractId == contractId);

            if (status.HasValue)
            {
                query = query.Where(a => a.Status == status.Value);
            }

            return await query
                .OrderByDescending(a => a.AmendmentNumber)
                .Select(a => new ErpContractAmendmentListDto
                {
                    AmendmentId = a.AmendmentId,
                    AmendmentType = a.AmendmentType,
                    Status = a.Status,
                    EffectiveDate = a.EffectiveDate,
                    Create_time = a.Create_time
                })
                .ToListAsync();
        }

        /// <summary>
        /// Get amendment by ID async
        /// </summary>
        public async Task<ErpContractAmendment> GetByIdAsync(Guid amendmentId)
        {
            return await Context.Queryable<ErpContractAmendment>()
                .Where(a => a.AmendmentId == amendmentId)
                .FirstAsync();
        }

        /// <summary>
        /// Create amendment async
        /// </summary>
        public async Task<Guid> CreateAsync(ErpContractAmendment amendment)
        {
            amendment.AmendmentId = Guid.NewGuid();
            amendment.Create_time = DateTime.Now;
            amendment.RequestedDate = DateTime.Now;

            var maxNumber = await Context.Queryable<ErpContractAmendment>()
                .Where(a => a.ContractId == amendment.ContractId)
                .MaxAsync(a => (int?)a.AmendmentNumber) ?? 0;
            amendment.AmendmentNumber = maxNumber + 1;

            var contract = await Context.Queryable<ErpContract>()
                .Where(c => c.ContractId == amendment.ContractId)
                .Select(c => c.ContractNumber)
                .FirstAsync();
            amendment.AmendmentReference = $"{contract}-AMD-{amendment.AmendmentNumber:D3}";

            await Context.Insertable(amendment).ExecuteCommandAsync();
            return amendment.AmendmentId;
        }

        /// <summary>
        /// Update amendment async
        /// </summary>
        public async Task<int> UpdateAmendmentAsync(ErpContractAmendment amendment)
        {
            amendment.Update_time = DateTime.Now;
            return await Context.Updateable(amendment)
                .IgnoreColumns(a => new { a.AmendmentId, a.ContractId, a.AmendmentNumber, a.AmendmentReference, a.Create_by, a.Create_time })
                .Where(a => a.AmendmentId == amendment.AmendmentId)
                .ExecuteCommandAsync();
        }

        /// <summary>
        /// Delete amendment async
        /// </summary>
        public async Task<int> DeleteAsync(Guid amendmentId)
        {
            return await Context.Deleteable<ErpContractAmendment>()
                .Where(a => a.AmendmentId == amendmentId)
                .ExecuteCommandAsync();
        }

        /// <summary>
        /// Delete amendments by contract ID async
        /// </summary>
        public async Task<int> DeleteByContractIdAsync(Guid contractId)
        {
            return await Context.Deleteable<ErpContractAmendment>()
                .Where(a => a.ContractId == contractId)
                .ExecuteCommandAsync();
        }

        /// <summary>
        /// Update status async
        /// </summary>
        public async Task<int> UpdateStatusAsync(Guid amendmentId, AmendmentStatus status, string updatedBy)
        {
            return await Context.Updateable<ErpContractAmendment>()
                .SetColumns(a => a.Status == status)
                .SetColumns(a => a.Update_by == updatedBy)
                .SetColumns(a => a.Update_time == DateTime.Now)
                .Where(a => a.AmendmentId == amendmentId)
                .ExecuteCommandAsync();
        }

        /// <summary>
        /// Approve amendment async
        /// </summary>
        public async Task<int> ApproveAsync(Guid amendmentId, string approvedBy)
        {
            return await Context.Updateable<ErpContractAmendment>()
                .SetColumns(a => a.Status == AmendmentStatus.Approved)
                .SetColumns(a => a.ApprovedBy == approvedBy)
                .SetColumns(a => a.ApprovedDate == DateTime.Now)
                .SetColumns(a => a.Update_by == approvedBy)
                .SetColumns(a => a.Update_time == DateTime.Now)
                .Where(a => a.AmendmentId == amendmentId)
                .ExecuteCommandAsync();
        }

        /// <summary>
        /// Reject amendment async
        /// </summary>
        public async Task<int> RejectAsync(Guid amendmentId, string rejectedBy, string reason)
        {
            return await Context.Updateable<ErpContractAmendment>()
                .SetColumns(a => a.Status == AmendmentStatus.Rejected)
                .SetColumns(a => a.RejectedBy == rejectedBy)
                .SetColumns(a => a.RejectedDate == DateTime.Now)
                .SetColumns(a => a.RejectionReason == reason)
                .SetColumns(a => a.Update_by == rejectedBy)
                .SetColumns(a => a.Update_time == DateTime.Now)
                .Where(a => a.AmendmentId == amendmentId)
                .ExecuteCommandAsync();
        }

        /// <summary>
        /// Cancel amendment async
        /// </summary>
        public async Task<int> CancelAsync(Guid amendmentId, string cancelledBy)
        {
            return await Context.Updateable<ErpContractAmendment>()
                .SetColumns(a => a.Status == AmendmentStatus.Cancelled)
                .SetColumns(a => a.Update_by == cancelledBy)
                .SetColumns(a => a.Update_time == DateTime.Now)
                .Where(a => a.AmendmentId == amendmentId)
                .ExecuteCommandAsync();
        }

        /// <summary>
        /// Get pending amendments count async
        /// </summary>
        public async Task<int> GetPendingAmendmentsCountAsync(Guid? contractId = null)
        {
            var query = Context.Queryable<ErpContractAmendment>()
                .Where(a => a.Status == AmendmentStatus.PendingApproval);

            if (contractId.HasValue)
            {
                query = query.Where(a => a.ContractId == contractId.Value);
            }

            return await query.CountAsync();
        }

        /// <summary>
        /// Get recent amendments async
        /// </summary>
        public async Task<List<ErpContractAmendmentListDto>> GetRecentAmendmentsAsync(Guid contractId, int count = 5)
        {
            return await Context.Queryable<ErpContractAmendment>()
                .Where(a => a.ContractId == contractId)
                .OrderByDescending(a => a.Create_time)
                .Take(count)
                .Select(a => new ErpContractAmendmentListDto
                {
                    AmendmentId = a.AmendmentId,
                    AmendmentType = a.AmendmentType,
                    Status = a.Status,
                    EffectiveDate = a.EffectiveDate,
                    Create_time = a.Create_time
                })
                .ToListAsync();
        }

        #endregion
    }
}
