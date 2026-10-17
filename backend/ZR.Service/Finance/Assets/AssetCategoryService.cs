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
using ZR.Repository.Finance.Assets;
using ZR.Service.IService.Finance.Assets;

namespace ZR.Service.Finance.Assets
{
    /// <summary>
    /// Asset Category Service - خدمة فئات الأصول
    /// Enhanced with comprehensive validation
    /// </summary>
    [AppService(ServiceType = typeof(IAssetCategoryService), ServiceLifetime = LifeTime.Transient)]
    public class AssetCategoryService : BaseService<AssetCategory>, IAssetCategoryService
    {
        private readonly AssetCategoryRepository _repository;
        private readonly IAssetValidationService _validationService;

        public AssetCategoryService(
            AssetCategoryRepository repository,
            IAssetValidationService validationService)
        {
            _repository = repository;
            _validationService = validationService;
        }

        /// <summary>
        /// Get category list with paging - الحصول على قائمة الفئات مع ترقيم الصفحات
        /// </summary>
        public PagedInfo<AssetCategoryListDto> GetList(AssetCategoryQueryDto parm)
        {
            var predicate = Expressionable.Create<AssetCategory>();

            // Filter by tenant
            if (parm.CompanyId.HasValue)
            {
                predicate = predicate.And(x => x.CompanyID == parm.CompanyId.Value);
            }

            // Filter by code
            if (!string.IsNullOrEmpty(parm.CategoryCode))
            {
                predicate = predicate.And(x => x.CategoryCode.Contains(parm.CategoryCode));
            }

            // Filter by name
            if (!string.IsNullOrEmpty(parm.CategoryName))
            {
                predicate = predicate.And(x => x.NameEn.Contains(parm.CategoryName) ||
                    (x.NameAr != null && x.NameAr.Contains(parm.CategoryName)));
            }

            // Filter by IsActive
            if (parm.IsActive.HasValue)
            {
                predicate = predicate.And(x => x.IsActive == parm.IsActive.Value);
            }

            // Filter by depreciation method
            if (parm.DefaultDepreciationMethod.HasValue)
            {
                predicate = predicate.And(x => x.DepreciationMethod == parm.DefaultDepreciationMethod.Value);
            }

            var response = _repository.GetPages(predicate.ToExpression(), parm);

            return response.Adapt<PagedInfo<AssetCategoryListDto>>();
        }

        /// <summary>
        /// Get category by ID - الحصول على فئة بالمعرف
        /// </summary>
        public AssetCategoryDetailDto GetDetail(Guid id)
        {
            var entity = _repository.GetById(id);
            if (entity == null)
            {
                return null;
            }
            return entity.Adapt<AssetCategoryDetailDto>();
        }

        /// <summary>
        /// Create category - إنشاء فئة
        /// </summary>
        public AssetCategory Create(CreateAssetCategoryDto dto, string userId)
        {
            // Auto-generate code if not provided
            if (string.IsNullOrWhiteSpace(dto.CategoryCode))
            {
                dto.CategoryCode = GetNextCategoryCode(dto.CompanyId);
            }

            // Run validations
            _validationService.ValidateCreateCategory(dto);

            // Check for duplicate code
            if (CodeExists(dto.CategoryCode, null, dto.CompanyId))
            {
                throw new AppException(ExceptionCode.FINANCE_ASSET_CATEGORY_CODE_DUPLICATE)
                    .WithParameter("Code", dto.CategoryCode);
            }

            var entity = dto.Adapt<AssetCategory>();
            entity.Id = Guid.NewGuid();
            entity.CompanyID = dto.CompanyId;
            entity.Create_time = DateTime.Now;
            entity.Update_time = DateTime.Now;

            // Parse userId to Guid if provided
            if (Guid.TryParse(userId, out Guid userGuid))
            {
                entity.Create_by = userGuid.ToString();
                entity.Update_by = userGuid.ToString();
            }

            _repository.Insert(entity);
            return entity;
        }

        /// <summary>
        /// Update category - تحديث فئة
        /// </summary>
        public int Update(UpdateAssetCategoryDto dto, string userId)
        {
            // Check for duplicate code (excluding current record)
            if (CodeExists(dto.CategoryCode, dto.Id, dto.CompanyId))
            {
                throw new AppException(ExceptionCode.FINANCE_ASSET_CATEGORY_CODE_DUPLICATE)
                    .WithParameter("Code", dto.CategoryCode);
            }

            var entity = _repository.GetById(dto.Id);
            if (entity == null)
            {
                throw new AppException(ExceptionCode.FINANCE_ASSET_CATEGORY_NOT_FOUND)
                    .WithParameter("CategoryId", dto.Id);
            }

            // Run validations (checks for assets and depreciation method changes)
            _validationService.ValidateUpdateCategory(dto, entity);

            // Update properties
            entity.CategoryCode = dto.CategoryCode;
            entity.NameEn = dto.NameEn;
            entity.NameAr = dto.NameAr;
            entity.DepreciationMethod = dto.DepreciationMethod;
            entity.UsefulLifeMonths = dto.UsefulLifeMonths;
            entity.DefaultAssetAccountId = dto.DefaultAssetAccountId;
            entity.DefaultAccumulatedDepreciationAccountId = dto.DefaultAccumulatedDepreciationAccountId;
            entity.DefaultDepreciationExpenseAccountId = dto.DefaultDepreciationExpenseAccountId;
            entity.DefaultGainLossAccountId = dto.DefaultGainLossAccountId;
            entity.IsActive = dto.IsActive;
            entity.Update_time = DateTime.Now;

            if (Guid.TryParse(userId, out Guid userGuid))
            {
                entity.Update_by = userGuid.ToString();
            }

            return _repository.Update(entity);
        }

        /// <summary>
        /// Delete categories - حذف فئات
        /// </summary>
        public int Delete(Guid[] ids)
        {
            // Validate each category can be deleted (no assets assigned)
            foreach (var id in ids)
            {
                _validationService.ValidateDeleteCategory(id);
            }

            return _repository.Delete(ids);
        }

        /// <summary>
        /// Check if category code exists - التحقق من وجود رمز الفئة
        /// </summary>
        public bool CodeExists(string code, Guid? excludeId = null, long? companyId = null)
        {
            var predicate = Expressionable.Create<AssetCategory>();
            predicate = predicate.And(x => x.CategoryCode == code);

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
        /// Get all active categories - الحصول على جميع الفئات النشطة
        /// </summary>
        public List<AssetCategoryListDto> GetActiveCategories(long? companyId = null)
        {
            var predicate = Expressionable.Create<AssetCategory>();
            predicate = predicate.And(x => x.IsActive == true);

            if (companyId.HasValue)
            {
                predicate = predicate.And(x => x.CompanyID == companyId.Value);
            }

            var list = _repository.GetList(predicate.ToExpression());
            return list.Adapt<List<AssetCategoryListDto>>();
        }

        /// <summary>
        /// Get next category code - الحصول على رمز الفئة التالي
        /// Format: CAT-YYYY-NNNN (e.g., CAT-2025-0001)
        /// </summary>
        public string GetNextCategoryCode(long? companyId = null)
        {
            var prefix = "CAT";
            var currentYear = DateTime.Now.Year;
            var yearPrefix = $"{prefix}-{currentYear}-";

            // Find the max existing code for this year
            var predicate = Expressionable.Create<AssetCategory>();
            predicate = predicate.And(x => x.CategoryCode.StartsWith(yearPrefix));

            if (companyId.HasValue)
            {
                predicate = predicate.And(x => x.CompanyID == companyId.Value);
            }

            var existingCodes = _repository.GetList(predicate.ToExpression())
                .Select(x => x.CategoryCode)
                .ToList();

            int nextNumber = 1;
            if (existingCodes.Any())
            {
                var maxNumber = existingCodes
                    .Select(code =>
                    {
                        var parts = code.Split('-');
                        if (parts.Length >= 3 && int.TryParse(parts[2], out int num))
                            return num;
                        return 0;
                    })
                    .Max();
                nextNumber = maxNumber + 1;
            }

            return $"{yearPrefix}{nextNumber:D4}";
        }

    }
}
