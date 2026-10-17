using System;
using System.Collections.Generic;
using Infrastructure;
using Infrastructure.Attribute;
using ZR.Model.PMO;
using ZR.Model.PMO.Dto;
using ZR.Repository;
using ZR.ServiceCore;
using ZR.Service.PMO.IPmoService;

namespace ZR.Service.PMO
{
    /// <summary>Cost category management (administered). Also serves the finance category lookup.</summary>
    [AppService(ServiceType = typeof(IPmoCostCategoryService), ServiceLifetime = LifeTime.Transient)]
    public class PmoCostCategoryService : BaseService<PmoCostCategory>, IPmoCostCategoryService
    {
        public List<PmoCostCategory> GetAll() => Queryable().OrderBy(c => c.Name).ToList();

        public PmoCostCategory Add(PmoCostCategoryInputDto dto, PmoActor actor)
        {
            if (Queryable().Any(c => c.Code == dto.Code))
                throw new CustomException(ResultCode.FAIL, $"Category code '{dto.Code}' already exists");
            var entity = new PmoCostCategory
            {
                Code = dto.Code.Trim(),
                Name = dto.Name.Trim(),
                Create_by = actor.UserName,
                Create_time = DateTime.Now
            };
            entity.CategoryId = Insertable(entity).ExecuteReturnSnowflakeId();
            return entity;
        }

        public int Update(PmoCostCategoryInputDto dto, PmoActor actor)
        {
            var existing = Queryable().First(c => c.CategoryId == dto.CategoryId)
                ?? throw new CustomException(ResultCode.FAIL, "Category not found");
            if (Queryable().Any(c => c.Code == dto.Code && c.CategoryId != dto.CategoryId))
                throw new CustomException(ResultCode.FAIL, $"Category code '{dto.Code}' already exists");
            existing.Code = dto.Code.Trim();
            existing.Name = dto.Name.Trim();
            existing.Update_by = actor.UserName;
            existing.Update_time = DateTime.Now;
            return base.Update(existing, true, "Updated cost category");
        }
    }
}
