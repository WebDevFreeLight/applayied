using System;
using System.Collections.Generic;
using System.Linq;
using Infrastructure.Attribute;
using SqlSugar;
using ZR.Model;
using ZR.Model.CustomerAssets;
using ZR.Model.CustomerAssets.Dto;
using ZR.Model.CustomerAssets.Enums;
using ZR.Repository;
using ZR.Service.IService.CustomerAssets;

namespace ZR.Service.CustomerAssets
{
    [AppService(ServiceType = typeof(ICustomerAssetService), ServiceLifetime = LifeTime.Transient)]
    public class CustomerAssetService : BaseService<CustomerAsset>, ICustomerAssetService
    {
        public PagedInfo<CustomerAssetListDto> GetList(CustomerAssetQueryDto query, PagerInfo pager)
        {
            var result = Context.Queryable<CustomerAsset, AssetType>(
                (a, t) => new JoinQueryInfos(JoinType.Left, a.AssetTypeId == t.Id))
                .WhereIF(!string.IsNullOrEmpty(query.AssetCode), (a, t) => a.AssetCode.Contains(query.AssetCode))
                .WhereIF(!string.IsNullOrEmpty(query.SerialNumber), (a, t) => a.SerialNumber.Contains(query.SerialNumber))
                .WhereIF(!string.IsNullOrEmpty(query.NameEn), (a, t) => a.NameEn.Contains(query.NameEn) || a.NameAr.Contains(query.NameEn))
                .WhereIF(query.AssetTypeId.HasValue && query.AssetTypeId != Guid.Empty, (a, t) => a.AssetTypeId == query.AssetTypeId)
                .WhereIF(query.CurrentStatus.HasValue, (a, t) => a.CurrentStatus == query.CurrentStatus)
                .WhereIF(query.CurrentCondition.HasValue, (a, t) => a.CurrentCondition == query.CurrentCondition)
                .OrderByDescending((a, t) => a.Create_time)
                .Select((a, t) => new CustomerAssetListDto
                {
                    Id = a.Id,
                    AssetCode = a.AssetCode,
                    SerialNumber = a.SerialNumber,
                    NameEn = a.NameEn,
                    NameAr = a.NameAr,
                    CurrentCondition = a.CurrentCondition,
                    CurrentStatus = a.CurrentStatus,
                    AssetTypeId = a.AssetTypeId,
                    AssetTypeNameEn = t.NameEn,
                    AssetTypeNameAr = t.NameAr,
                    PurchaseCost = a.PurchaseCost,
                    WarrantyExpiry = a.WarrantyExpiry,
                    Create_time = a.Create_time
                }).ToPage(pager);

            return result;
        }

        public CustomerAssetDetailDto GetById(Guid id)
        {
            return Context.Queryable<CustomerAsset, AssetType>(
                (a, t) => new JoinQueryInfos(JoinType.Left, a.AssetTypeId == t.Id))
                .Where((a, t) => a.Id == id)
                .Select((a, t) => new CustomerAssetDetailDto
                {
                    Id = a.Id,
                    AssetCode = a.AssetCode,
                    SerialNumber = a.SerialNumber,
                    NameEn = a.NameEn,
                    NameAr = a.NameAr,
                    CurrentCondition = a.CurrentCondition,
                    CurrentStatus = a.CurrentStatus,
                    AssetTypeId = a.AssetTypeId,
                    AssetTypeNameEn = t.NameEn,
                    AssetTypeNameAr = t.NameAr,
                    PurchaseCost = a.PurchaseCost,
                    PurchaseDate = a.PurchaseDate,
                    WarrantyExpiry = a.WarrantyExpiry,
                    Notes = a.Notes,
                    Create_time = a.Create_time,
                    Create_by = a.Create_by,
                    Update_by = a.Update_by,
                    Update_time = a.Update_time
                }).First();
        }

        public CustomerAsset Create(CreateCustomerAssetDto dto, string userName)
        {
            var model = new CustomerAsset
            {
                Id = Guid.NewGuid(),
                AssetTypeId = dto.AssetTypeId,
                AssetCode = GenerateAssetCode(),
                SerialNumber = dto.SerialNumber,
                NameEn = dto.NameEn,
                NameAr = dto.NameAr,
                CurrentCondition = dto.CurrentCondition,
                CurrentStatus = InventoryAssetStatus.Available,
                PurchaseDate = dto.PurchaseDate,
                PurchaseCost = dto.PurchaseCost,
                WarrantyExpiry = dto.WarrantyExpiry,
                Notes = dto.Notes,
                Create_by = userName
            };
            Insert(model);
            return model;
        }

        public int Update(UpdateCustomerAssetDto dto, string userName)
        {
            var model = GetFirst(x => x.Id == dto.Id);
            if (model == null) return 0;

            model.AssetTypeId = dto.AssetTypeId;
            model.SerialNumber = dto.SerialNumber;
            model.NameEn = dto.NameEn;
            model.NameAr = dto.NameAr;
            model.CurrentCondition = dto.CurrentCondition;
            model.PurchaseDate = dto.PurchaseDate;
            model.PurchaseCost = dto.PurchaseCost;
            model.WarrantyExpiry = dto.WarrantyExpiry;
            model.Notes = dto.Notes;
            model.Update_by = userName;

            return base.Update(model, true);
        }

        public int Delete(Guid[] ids)
        {
            // Only allow deleting Available assets
            var hasAssigned = Context.Queryable<CustomerAsset>()
                .Where(x => ids.Contains(x.Id) && x.CurrentStatus != InventoryAssetStatus.Available)
                .Any();
            if (hasAssigned) return 0;

            return base.Delete(ids);
        }

        public List<AvailableAssetSelectDto> GetAvailableAssets()
        {
            return Context.Queryable<CustomerAsset, AssetType>(
                (a, t) => new JoinQueryInfos(JoinType.Left, a.AssetTypeId == t.Id))
                .Where((a, t) => a.CurrentStatus == InventoryAssetStatus.Available)
                .OrderBy((a, t) => a.AssetCode)
                .Select((a, t) => new AvailableAssetSelectDto
                {
                    Id = a.Id,
                    AssetCode = a.AssetCode,
                    NameEn = a.NameEn,
                    NameAr = a.NameAr,
                    SerialNumber = a.SerialNumber,
                    AssetTypeNameEn = t.NameEn,
                    AssetTypeNameAr = t.NameAr
                }).ToList();
        }

        public string GenerateAssetCode()
        {
            var year = DateTime.Now.Year;
            var prefix = $"CA-{year}-";

            var lastCode = Context.Queryable<CustomerAsset>()
                .Where(x => x.AssetCode.StartsWith(prefix))
                .OrderByDescending(x => x.AssetCode)
                .Select(x => x.AssetCode)
                .First();

            int nextNum = 1;
            if (!string.IsNullOrEmpty(lastCode))
            {
                var numPart = lastCode.Replace(prefix, "");
                if (int.TryParse(numPart, out int current))
                    nextNum = current + 1;
            }

            return $"{prefix}{nextNum:D4}";
        }
    }
}
