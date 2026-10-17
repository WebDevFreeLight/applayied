using Infrastructure.Attribute;
using Infrastructure.Extensions;
using ZR.Model.Operations;
using ZR.Model.Operations.Dto;
using ZR.Repository;

namespace ZR.Service.Operations
{
    /// <summary>
    /// Safety Precaution Service - احتياطات السلامة
    /// </summary>
    [AppService(ServiceType = typeof(IOpsSafetyPrecautionService), ServiceLifetime = LifeTime.Transient)]
    public class OpsSafetyPrecautionService : BaseService<OpsSafetyPrecaution>, IOpsSafetyPrecautionService
    {
        public PagedInfo<OpsSafetyPrecautionDto> GetList(OpsSafetyPrecautionQueryDto parm)
        {
            var predicate = Expressionable.Create<OpsSafetyPrecaution>();
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Code), it => it.Code.Contains(parm.Code));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Description), it => it.Description.Contains(parm.Description));
            predicate = predicate.AndIF(parm.IsActive != null, it => it.IsActive == parm.IsActive);

            var response = Queryable()
                .Where(predicate.ToExpression())
                .OrderBy(it => it.DisplayOrder)
                .ToPage<OpsSafetyPrecaution, OpsSafetyPrecautionDto>(parm);

            return response;
        }

        public OpsSafetyPrecaution GetInfo(long precautionId)
        {
            return Queryable().Where(x => x.PrecautionId == precautionId).First();
        }

        public OpsSafetyPrecaution AddPrecaution(OpsSafetyPrecaution model)
        {
            Insertable(model).ExecuteReturnSnowflakeId();
            return model;
        }

        public int UpdatePrecaution(OpsSafetyPrecaution model)
        {
            return Update(model, true, "Update safety precaution");
        }

        public PagedInfo<OpsSafetyPrecautionDto> ExportList(OpsSafetyPrecautionQueryDto parm)
        {
            parm.PageNum = 1;
            parm.PageSize = 100000;
            return GetList(parm);
        }

        public List<OpsSafetyPrecautionDto> GetAllActive()
        {
            return Queryable()
                .Where(x => x.IsActive)
                .OrderBy(x => x.DisplayOrder)
                .Select(x => new OpsSafetyPrecautionDto
                {
                    PrecautionId = x.PrecautionId,
                    Code = x.Code,
                    Description = x.Description,
                    DescriptionAr = x.DescriptionAr,
                    DisplayOrder = x.DisplayOrder,
                    IsActive = x.IsActive
                })
                .ToList();
        }
    }
}
