using Infrastructure.Attribute;
using Infrastructure.Extensions;
using ZR.Model.Operations;
using ZR.Model.Operations.Dto;
using ZR.Repository;

namespace ZR.Service.Operations
{
    /// <summary>
    /// Work Type Service - نوع العمل
    /// </summary>
    [AppService(ServiceType = typeof(IOpsWorkTypeService), ServiceLifetime = LifeTime.Transient)]
    public class OpsWorkTypeService : BaseService<OpsWorkType>, IOpsWorkTypeService
    {
        public PagedInfo<OpsWorkTypeDto> GetList(OpsWorkTypeQueryDto parm)
        {
            var predicate = Expressionable.Create<OpsWorkType>();
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Code), it => it.Code.Contains(parm.Code));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Name), it => it.Name.Contains(parm.Name));
            predicate = predicate.AndIF(parm.IsActive != null, it => it.IsActive == parm.IsActive);

            var response = Queryable()
                .Where(predicate.ToExpression())
                .OrderBy(it => it.DisplayOrder)
                .ToPage<OpsWorkType, OpsWorkTypeDto>(parm);

            return response;
        }

        public OpsWorkType GetInfo(long workTypeId)
        {
            return Queryable().Where(x => x.WorkTypeId == workTypeId).First();
        }

        public OpsWorkType AddWorkType(OpsWorkType model)
        {
            Insertable(model).ExecuteReturnSnowflakeId();
            return model;
        }

        public int UpdateWorkType(OpsWorkType model)
        {
            return Update(model, true, "Update work type");
        }

        public PagedInfo<OpsWorkTypeDto> ExportList(OpsWorkTypeQueryDto parm)
        {
            parm.PageNum = 1;
            parm.PageSize = 100000;
            return GetList(parm);
        }

        public List<OpsWorkTypeDto> GetAllActive()
        {
            return Queryable()
                .Where(x => x.IsActive)
                .OrderBy(x => x.DisplayOrder)
                .Select(x => new OpsWorkTypeDto
                {
                    WorkTypeId = x.WorkTypeId,
                    Code = x.Code,
                    Name = x.Name,
                    NameAr = x.NameAr,
                    DisplayOrder = x.DisplayOrder,
                    IsActive = x.IsActive
                })
                .ToList();
        }
    }
}
