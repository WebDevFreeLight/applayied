using System;
using System.Collections.Generic;
using System.Linq;
using Infrastructure.Attribute;
using SqlSugar;
using ZR.Model;
using ZR.Model.CustomerAssets;
using ZR.Model.CustomerAssets.Dto;
using ZR.Repository;
using ZR.Service.IService.CustomerAssets;

namespace ZR.Service.CustomerAssets
{
    [AppService(ServiceType = typeof(IAssetTypeService), ServiceLifetime = LifeTime.Transient)]
    public class AssetTypeService : BaseService<AssetType>, IAssetTypeService
    {
        public PagedInfo<AssetTypeListDto> GetList(AssetTypeQueryDto query, PagerInfo pager)
        {
            var predicate = Expressionable.Create<AssetType>();
            predicate = predicate.AndIF(!string.IsNullOrEmpty(query.Code), it => it.Code.Contains(query.Code));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(query.NameEn), it => it.NameEn.Contains(query.NameEn));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(query.NameAr), it => it.NameAr.Contains(query.NameAr));
            predicate = predicate.AndIF(query.IsActive.HasValue, it => it.IsActive == query.IsActive.Value);

            var result = Queryable()
                .Where(predicate.ToExpression())
                .OrderBy(x => x.Code)
                .ToPage(pager);

            return new PagedInfo<AssetTypeListDto>
            {
                TotalNum = result.TotalNum,
                PageIndex = result.PageIndex,
                PageSize = result.PageSize,
                Result = result.Result.Select(x => new AssetTypeListDto
                {
                    Id = x.Id,
                    Code = x.Code,
                    NameEn = x.NameEn,
                    NameAr = x.NameAr,
                    Description = x.Description,
                    IsActive = x.IsActive,
                    Create_time = x.Create_time
                }).ToList()
            };
        }

        public AssetType GetById(Guid id)
        {
            return GetFirst(x => x.Id == id);
        }

        public AssetType Create(CreateAssetTypeDto dto, string userName)
        {
            var model = new AssetType
            {
                Id = Guid.NewGuid(),
                Code = dto.Code,
                NameEn = dto.NameEn,
                NameAr = dto.NameAr,
                Description = dto.Description,
                IsActive = dto.IsActive,
                Create_by = userName
            };
            Insert(model);
            return model;
        }

        public int Update(UpdateAssetTypeDto dto, string userName)
        {
            var model = GetById(dto.Id);
            if (model == null) return 0;

            model.Code = dto.Code;
            model.NameEn = dto.NameEn;
            model.NameAr = dto.NameAr;
            model.Description = dto.Description;
            model.IsActive = dto.IsActive;
            model.Update_by = userName;

            return base.Update(model, true);
        }

        public int Delete(Guid[] ids)
        {
            return base.Delete(ids);
        }

        public bool CodeExists(string code, Guid? excludeId = null)
        {
            var predicate = Expressionable.Create<AssetType>();
            predicate = predicate.And(it => it.Code == code);
            predicate = predicate.AndIF(excludeId.HasValue, it => it.Id != excludeId.Value);
            return Any(predicate.ToExpression());
        }

        public List<AssetTypeSelectDto> GetSelectList()
        {
            return Queryable()
                .Where(x => x.IsActive)
                .OrderBy(x => x.Code)
                .Select(x => new AssetTypeSelectDto
                {
                    Id = x.Id,
                    Code = x.Code,
                    NameEn = x.NameEn,
                    NameAr = x.NameAr
                }).ToList();
        }
    }
}
