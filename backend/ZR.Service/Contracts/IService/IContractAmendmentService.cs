using System;
using System.Collections.Generic;
using ZR.Model.Contracts;
using ZR.Model.Contracts.Dto;
using ZR.Model.Contracts.Enums;

namespace ZR.Service.Contracts.IService
{
    /// <summary>
    /// Contract Amendment Service Interface - واجهة خدمة تعديلات العقد
    /// </summary>
    public interface IContractAmendmentService
    {
        /// <summary>
        /// Get amendments by contract ID
        /// الحصول على التعديلات بمعرف العقد
        /// </summary>
        List<ErpContractAmendmentListDto> GetByContractId(Guid contractId, AmendmentStatus? status = null);

        /// <summary>
        /// Get amendment by ID
        /// الحصول على التعديل بالمعرف
        /// </summary>
        ErpContractAmendment GetById(Guid amendmentId);

        /// <summary>
        /// Create amendment
        /// إنشاء تعديل
        /// </summary>
        Guid Create(CreateErpContractAmendmentDto dto, string createdBy);

        /// <summary>
        /// Update amendment
        /// تحديث التعديل
        /// </summary>
        int Update(UpdateErpContractAmendmentDto dto, string updatedBy);

        /// <summary>
        /// Delete amendment
        /// حذف التعديل
        /// </summary>
        int Delete(Guid amendmentId);

        /// <summary>
        /// Submit amendment for approval
        /// تقديم التعديل للموافقة
        /// </summary>
        int SubmitForApproval(Guid amendmentId, string submittedBy);

        /// <summary>
        /// Approve amendment
        /// الموافقة على التعديل
        /// </summary>
        int Approve(Guid amendmentId, string approvedBy);

        /// <summary>
        /// Reject amendment
        /// رفض التعديل
        /// </summary>
        int Reject(Guid amendmentId, string rejectedBy, string reason);

        /// <summary>
        /// Cancel amendment
        /// إلغاء التعديل
        /// </summary>
        int Cancel(Guid amendmentId, string cancelledBy);

        /// <summary>
        /// Get pending amendments count
        /// الحصول على عدد التعديلات المعلقة
        /// </summary>
        int GetPendingAmendmentsCount(Guid? contractId = null);

        /// <summary>
        /// Get recent amendments
        /// الحصول على التعديلات الأخيرة
        /// </summary>
        List<ErpContractAmendmentListDto> GetRecentAmendments(Guid contractId, int count = 5);
    }
}
