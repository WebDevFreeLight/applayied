using System;
using System.Collections.Generic;
using System.Linq;
using Infrastructure;
using Infrastructure.Attribute;
using Infrastructure.Exceptions;
using Mapster;
using SqlSugar;
using ZR.Model;
using ZR.Model.Finance.Assets;
using ZR.Model.Finance.Assets.Dto;
using ZR.Model.Finance.Assets.Enums;
using ZR.Repository.Finance.Assets;
using ZR.Service.IService.Finance.Assets;

namespace ZR.Service.Finance.Assets
{
    /// <summary>
    /// Current Asset Service - خدمة الأصول المتداولة
    /// </summary>
    [AppService(ServiceType = typeof(ICurrentAssetService), ServiceLifetime = LifeTime.Transient)]
    public class CurrentAssetService : BaseService<CurrentAsset>, ICurrentAssetService
    {
        private readonly CurrentAssetRepository _repository;

        public CurrentAssetService(CurrentAssetRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// Get asset list with paging - الحصول على قائمة الأصول مع ترقيم الصفحات
        /// </summary>
        public PagedInfo<CurrentAssetListDto> GetList(CurrentAssetQueryDto parm)
        {
            var predicate = Expressionable.Create<CurrentAsset>();

            // Filter by company
            if (parm.CompanyId.HasValue)
            {
                predicate = predicate.And(x => x.CompanyID == parm.CompanyId.Value);
            }

            // Filter by code
            if (!string.IsNullOrEmpty(parm.ItemCode))
            {
                predicate = predicate.And(x => x.ItemCode.Contains(parm.ItemCode));
            }

            // Filter by name
            if (!string.IsNullOrEmpty(parm.ItemName))
            {
                predicate = predicate.And(x => x.NameEn.Contains(parm.ItemName) ||
                    (x.NameAr != null && x.NameAr.Contains(parm.ItemName)));
            }

            // Filter by category
            if (!string.IsNullOrEmpty(parm.Category))
            {
                predicate = predicate.And(x => x.Category == parm.Category);
            }

            // Filter by status
            if (parm.Status.HasValue)
            {
                predicate = predicate.And(x => x.Status == parm.Status.Value);
            }

            // Filter by purchase date range
            if (parm.AcquisitionDateFrom.HasValue)
            {
                predicate = predicate.And(x => x.PurchaseDate >= parm.AcquisitionDateFrom.Value);
            }
            if (parm.AcquisitionDateTo.HasValue)
            {
                predicate = predicate.And(x => x.PurchaseDate <= parm.AcquisitionDateTo.Value);
            }

            // Filter by value range
            if (parm.MinValue.HasValue)
            {
                predicate = predicate.And(x => x.CurrentValue >= parm.MinValue.Value);
            }
            if (parm.MaxValue.HasValue)
            {
                predicate = predicate.And(x => x.CurrentValue <= parm.MaxValue.Value);
            }

            var response = _repository.GetPages(predicate.ToExpression(), parm);

            return response.Adapt<PagedInfo<CurrentAssetListDto>>();
        }

        /// <summary>
        /// Get asset by ID - الحصول على أصل بالمعرف
        /// </summary>
        public CurrentAssetDetailDto GetDetail(Guid id)
        {
            var entity = _repository.GetById(id);
            if (entity == null)
            {
                return null;
            }
            return entity.Adapt<CurrentAssetDetailDto>();
        }

        /// <summary>
        /// Create asset - إنشاء أصل
        /// </summary>
        public CurrentAsset Create(CreateCurrentAssetDto dto, string userId)
        {
            // Check for duplicate code
            if (CodeExists(dto.ItemCode, null, dto.CompanyId))
            {
                throw new AppException(ExceptionCode.FINANCE_ASSET_CODE_DUPLICATE)
                    .WithParameter("Code", dto.ItemCode);
            }

            var entity = dto.Adapt<CurrentAsset>();
            entity.Id = Guid.NewGuid();
            entity.Status = CurrentAssetStatus.Active;
            entity.Create_time = DateTime.Now;
            entity.Update_time = DateTime.Now;
            entity.CompanyID = dto.CompanyId;

            if (Guid.TryParse(userId, out Guid userGuid))
            {
                entity.Create_by = userGuid.ToString();
                entity.Update_by = userGuid.ToString();
            }
            else
            {
                entity.Create_by = userId;
                entity.Update_by = userId;
            }

            _repository.Insert(entity);
            return entity;
        }

        /// <summary>
        /// Update asset - تحديث أصل
        /// </summary>
        public int Update(UpdateCurrentAssetDto dto, string userId)
        {
            // Check for duplicate code (excluding current record)
            if (CodeExists(dto.ItemCode, dto.Id, dto.CompanyId))
            {
                throw new AppException(ExceptionCode.FINANCE_ASSET_CODE_DUPLICATE)
                    .WithParameter("Code", dto.ItemCode);
            }

            var entity = _repository.GetById(dto.Id);
            if (entity == null)
            {
                throw new AppException(ExceptionCode.FINANCE_ASSET_NOT_FOUND)
                    .WithParameter("AssetId", dto.Id);
            }

            // Update properties
            entity.ItemCode = dto.ItemCode;
            entity.NameEn = dto.NameEn;
            entity.NameAr = dto.NameAr;
            entity.Category = dto.Category;
            entity.PurchaseDate = dto.PurchaseDate;
            entity.Cost = dto.Cost;
            entity.CurrentValue = dto.CurrentValue;
            entity.GLAccountId = dto.GLAccountId;
            entity.Status = dto.Status;
            entity.Notes = dto.Notes;
            entity.Update_time = DateTime.Now;

            if (Guid.TryParse(userId, out Guid userGuid))
            {
                entity.Update_by = userGuid.ToString();
            }
            else
            {
                entity.Update_by = userId;
            }

            return _repository.Update(entity);
        }

        /// <summary>
        /// Delete assets - حذف أصول
        /// </summary>
        public int Delete(Guid[] ids)
        {
            return _repository.Delete(ids);
        }

        /// <summary>
        /// Check if item code exists - التحقق من وجود رمز الصنف
        /// </summary>
        public bool CodeExists(string code, Guid? excludeId = null, long? companyId = null)
        {
            var predicate = Expressionable.Create<CurrentAsset>();
            predicate = predicate.And(x => x.ItemCode == code);

            if (excludeId.HasValue)
            {
                predicate = predicate.And(x => x.Id != excludeId.Value);
            }

            if (companyId.HasValue)
            {
                predicate = predicate.And(x => x.CompanyID == companyId.Value);
            }

            return _repository.Count(predicate.ToExpression()) > 0;
        }

        /// <summary>
        /// Get assets by category - الحصول على الأصول حسب الفئة
        /// </summary>
        public List<CurrentAssetListDto> GetByCategory(string category, long? companyId = null)
        {
            var predicate = Expressionable.Create<CurrentAsset>();
            predicate = predicate.And(x => x.Category == category);
            predicate = predicate.And(x => x.Status == CurrentAssetStatus.Active);

            if (companyId.HasValue)
            {
                predicate = predicate.And(x => x.CompanyID == companyId.Value);
            }

            var list = _repository.GetList(predicate.ToExpression());
            return list.Adapt<List<CurrentAssetListDto>>();
        }

        /// <summary>
        /// Get total value of current assets - الحصول على إجمالي قيمة الأصول المتداولة
        /// </summary>
        public decimal GetTotalValue(long? companyId = null)
        {
            var predicate = Expressionable.Create<CurrentAsset>();
            predicate = predicate.And(x => x.Status == CurrentAssetStatus.Active);

            if (companyId.HasValue)
            {
                predicate = predicate.And(x => x.CompanyID == companyId.Value);
            }

            var assets = _repository.GetList(predicate.ToExpression());
            return assets.Sum(x => x.CurrentValue);
        }
    }
}

