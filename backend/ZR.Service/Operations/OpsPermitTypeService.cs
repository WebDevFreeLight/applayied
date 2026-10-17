using Infrastructure.Attribute;
using Infrastructure.Extensions;
using ZR.Model.Operations;
using ZR.Model.Operations.Dto;
using ZR.Repository;

namespace ZR.Service.Operations
{
    [AppService(ServiceType = typeof(IOpsPermitTypeService), ServiceLifetime = LifeTime.Transient)]
    public class OpsPermitTypeService : BaseService<OpsPermitType>, IOpsPermitTypeService
    {
        public PagedInfo<OpsPermitTypeDto> GetList(OpsPermitTypeQueryDto parm)
        {
            var predicate = Expressionable.Create<OpsPermitType>();
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.TypeCode), it => it.TypeCode.Contains(parm.TypeCode));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.TypeName), it => it.TypeName.Contains(parm.TypeName));
            predicate = predicate.AndIF(parm.IsActive != null, it => it.IsActive == parm.IsActive);
            predicate = predicate.AndIF(parm.RequiresApproval != null, it => it.RequiresApproval == parm.RequiresApproval);

            var response = Queryable()
                .Where(predicate.ToExpression())
                .OrderBy(it => it.DisplayOrder)
                .ToPage<OpsPermitType, OpsPermitTypeDto>(parm);

            return response;
        }

        public OpsPermitType GetInfo(long PermitTypeId)
        {
            return Queryable().Where(x => x.PermitTypeId == PermitTypeId).First();
        }

        public OpsPermitType AddOpsPermitType(OpsPermitType model)
        {
            Insertable(model).ExecuteReturnSnowflakeId();
            return model;
        }

        public int UpdateOpsPermitType(OpsPermitType model)
        {
            return Update(model, true, "Update permit type");
        }

        public PagedInfo<OpsPermitTypeDto> ExportList(OpsPermitTypeQueryDto parm)
        {
            parm.PageNum = 1;
            parm.PageSize = 100000;
            return GetList(parm);
        }
    }
}
