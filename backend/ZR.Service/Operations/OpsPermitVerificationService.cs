using Infrastructure.Attribute;
using Infrastructure.Extensions;
using ZR.Model.Operations;
using ZR.Model.Operations.Dto;
using ZR.Repository;

namespace ZR.Service.Operations
{
    [AppService(ServiceType = typeof(IOpsPermitVerificationService), ServiceLifetime = LifeTime.Transient)]
    public class OpsPermitVerificationService : BaseService<OpsPermitVerification>, IOpsPermitVerificationService
    {
        public PagedInfo<OpsPermitVerificationDto> GetList(OpsPermitVerificationQueryDto parm)
        {
            var predicate = Expressionable.Create<OpsPermitVerification>();
            predicate = predicate.AndIF(parm.PermitId != null, it => it.PermitId == parm.PermitId);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.CustomerId), it => it.CustomerId == parm.CustomerId);
            predicate = predicate.AndIF(parm.WorkerId != null, it => it.WorkerId == parm.WorkerId);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.ApplicantName), it => it.ApplicantName.Contains(parm.ApplicantName));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.IdNumber), it => it.IdNumber.Contains(parm.IdNumber));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.DocumentType), it => it.DocumentType == parm.DocumentType);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Status), it => it.Status == parm.Status);

            var response = Queryable()
                .Where(predicate.ToExpression())
                .OrderBy(it => it.Create_time, OrderByType.Desc)
                .ToPage<OpsPermitVerification, OpsPermitVerificationDto>(parm);

            return response;
        }

        public OpsPermitVerification GetInfo(long VerificationId)
        {
            return Queryable().Where(x => x.VerificationId == VerificationId).First();
        }

        public OpsPermitVerification AddOpsPermitVerification(OpsPermitVerification model)
        {
            Insertable(model).ExecuteReturnSnowflakeId();
            return model;
        }

        public int UpdateOpsPermitVerification(OpsPermitVerification model)
        {
            return Update(model, true, "Update permit verification");
        }

        public PagedInfo<OpsPermitVerificationDto> ExportList(OpsPermitVerificationQueryDto parm)
        {
            parm.PageNum = 1;
            parm.PageSize = 100000;
            return GetList(parm);
        }

        public int VerifyDocument(long verificationId, string verifiedBy)
        {
            var record = GetInfo(verificationId);
            if (record == null) return 0;
            record.Status = "verified";
            record.VerifiedBy = verifiedBy;
            record.VerifiedDate = DateTime.Now;
            record.Update_time = DateTime.Now;
            return Update(record, true, "Verify document");
        }

        public int RejectDocument(long verificationId, string reason)
        {
            var record = GetInfo(verificationId);
            if (record == null) return 0;
            record.Status = "rejected";
            record.RejectionReason = reason;
            record.Update_time = DateTime.Now;
            return Update(record, true, "Reject document");
        }
    }
}
