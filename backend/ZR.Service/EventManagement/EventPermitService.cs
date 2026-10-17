using Infrastructure.Attribute;
using Infrastructure.Extensions;
using ZR.Model.EventManagement;
using ZR.Model.EventManagement.Dto;
using ZR.Repository;
using ZR.Service.EventManagement.IEventManagementService;

namespace ZR.Service.EventManagement
{
    /// <summary>
    /// Event Permit - التصاريح
    /// Service
    /// </summary>
    [AppService(ServiceType = typeof(IEventPermitService), ServiceLifetime = LifeTime.Transient)]
    public class EventPermitService : BaseService<EventPermit>, IEventPermitService
    {
        public PagedInfo<EventPermitDto> GetList(EventPermitQueryDto parm)
        {
            var predicate = QueryExp(parm);
            return Queryable()
                .Where(predicate.ToExpression())
                .ToPage<EventPermit, EventPermitDto>(parm);
        }

        public EventPermit GetInfo(long permitId)
        {
            return Queryable().Where(x => x.PermitId == permitId).First();
        }

        public EventPermit AddEventPermit(EventPermit model)
        {
            // Generate permit number if not provided
            if (string.IsNullOrEmpty(model.PermitNumber))
            {
                model.PermitNumber = $"PERMIT-{DateTime.Now:yyyyMMddHHmmss}-{Guid.NewGuid().ToString().Substring(0, 6).ToUpper()}";
            }

            model.IssueDate = DateTime.Now;
            model.Status = "pending";

            Insertable(model).ExecuteReturnSnowflakeId();
            return model;
        }

        public int UpdateEventPermit(EventPermit model)
        {
            return Update(model, true, "Update Event Permit");
        }

        public List<EventPermit> GetPermitsByEventId(long eventId)
        {
            return Queryable()
                .Where(x => x.EventId == eventId)
                .OrderBy(x => x.Create_time, OrderByType.Desc)
                .ToList();
        }

        public int ApprovePermit(long permitId, long approvedBy)
        {
            return Update(x => x.PermitId == permitId, it => new EventPermit
            {
                Status = "approved",
                ApprovedBy = approvedBy,
                ApprovedDate = DateTime.Now,
                ApprovalDate = DateTime.Now,
                RejectionReason = null
            });
        }

        public int RejectPermit(long permitId, long rejectedBy, string reason)
        {
            var trimmedReason = reason == null ? null : reason.Trim();

            return Update(x => x.PermitId == permitId, it => new EventPermit
            {
                Status = "rejected",
                ApprovedBy = rejectedBy,
                ApprovedDate = DateTime.Now,
                ApprovalDate = DateTime.Now,
                RejectionReason = trimmedReason
            });
        }

        private static Expressionable<EventPermit> QueryExp(EventPermitQueryDto parm)
        {
            var predicate = Expressionable.Create<EventPermit>();

            predicate = predicate.AndIF(parm.EventId != null, it => it.EventId == parm.EventId);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.PermitNumber), it => it.PermitNumber.Contains(parm.PermitNumber));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.PermitType), it => it.PermitType == parm.PermitType);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.HolderName), it => it.HolderName.Contains(parm.HolderName));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Status), it => it.Status == parm.Status);
            predicate = predicate.AndIF(parm.BeginIssueDate != null, it => it.IssueDate >= parm.BeginIssueDate);
            predicate = predicate.AndIF(parm.EndIssueDate != null, it => it.IssueDate <= parm.EndIssueDate);

            return predicate;
        }


       
        public PagedInfo<EventPermitDto> ExportList(EventPermitQueryDto parm)
        {

            parm.PageNum = 1;
            parm.PageSize = 100000;
            var predicate = QueryExp(parm);


            return Queryable()
                    .Where(predicate.ToExpression()) .ToPage<EventPermit,EventPermitDto>(parm);

            throw new NotImplementedException();
        }
    }
}

