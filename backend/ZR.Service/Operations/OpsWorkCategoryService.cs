using Infrastructure.Attribute;
using Infrastructure.Extensions;
using ZR.Model.Operations;
using ZR.Model.Operations.Dto;
using ZR.Repository;

namespace ZR.Service.Operations
{
    /// <summary>
    /// Work Category Service - تصنيف العمل
    /// </summary>
    [AppService(ServiceType = typeof(IOpsWorkCategoryService), ServiceLifetime = LifeTime.Transient)]
    public class OpsWorkCategoryService : BaseService<OpsWorkCategory>, IOpsWorkCategoryService
    {
        public PagedInfo<OpsWorkCategoryDto> GetList(OpsWorkCategoryQueryDto parm)
        {
            var predicate = Expressionable.Create<OpsWorkCategory>();
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Code), it => it.Code.Contains(parm.Code));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Name), it => it.Name.Contains(parm.Name));
            predicate = predicate.AndIF(parm.IsActive != null, it => it.IsActive == parm.IsActive);

            var response = Queryable()
                .Where(predicate.ToExpression())
                .OrderBy(it => it.DisplayOrder)
                .ToPage<OpsWorkCategory, OpsWorkCategoryDto>(parm);

            return response;
        }

        public OpsWorkCategory GetInfo(long workCategoryId)
        {
            return Queryable().Where(x => x.WorkCategoryId == workCategoryId).First();
        }

        public OpsWorkCategory AddWorkCategory(OpsWorkCategory model)
        {
            Insertable(model).ExecuteReturnSnowflakeId();
            return model;
        }

        public int UpdateWorkCategory(OpsWorkCategory model)
        {
            return Update(model, true, "Update work category");
        }

        public PagedInfo<OpsWorkCategoryDto> ExportList(OpsWorkCategoryQueryDto parm)
        {
            parm.PageNum = 1;
            parm.PageSize = 100000;
            return GetList(parm);
        }

        public List<OpsWorkCategoryDto> GetAllActive()
        {
            return Queryable()
                .Where(x => x.IsActive)
                .OrderBy(x => x.DisplayOrder)
                .Select(x => new OpsWorkCategoryDto
                {
                    WorkCategoryId = x.WorkCategoryId,
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
