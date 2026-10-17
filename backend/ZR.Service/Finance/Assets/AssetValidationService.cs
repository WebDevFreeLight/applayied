using System;
using System.Collections.Generic;
using Infrastructure;
using Infrastructure.Attribute;
using Infrastructure.Exceptions;
using SqlSugar;
using ZR.Model.Finance.Assets;
using ZR.Model.Finance.Assets.Dto;
using ZR.Model.Finance.Assets.Enums;
using ZR.Model.Finance.Exceptions;
using ZR.Repository.Finance.Assets;
using ZR.Service.IService.Finance.GL;

namespace ZR.Service.Finance.Assets
{
    /// <summary>
    /// Asset Validation Service - خدمة التحقق من صحة الأصول
    /// Provides comprehensive validation for all asset operations
    /// </summary>
    public interface IAssetValidationService
    {
        // Fixed Asset Validations
        void ValidateCreateFixedAsset(CreateFixedAssetDto dto);
        void ValidateUpdateFixedAsset(UpdateFixedAssetDto dto, FixedAsset existingAsset);
        void ValidateDeleteFixedAsset(FixedAsset asset);
        void ValidateAssetSale(AssetSaleDto dto, FixedAsset asset);
        void ValidateAssetDisposal(AssetDisposalDto dto, FixedAsset asset);
        void ValidateAssetRevaluation(AssetRevaluationDto dto, FixedAsset asset);

        // Category Validations
        void ValidateCreateCategory(CreateAssetCategoryDto dto);
        void ValidateUpdateCategory(UpdateAssetCategoryDto dto, AssetCategory existingCategory);
        void ValidateDeleteCategory(Guid categoryId);

        // Current Asset Validations
        void ValidateCreateCurrentAsset(CreateCurrentAssetDto dto);
        void ValidateUpdateCurrentAsset(UpdateCurrentAssetDto dto, CurrentAsset existingAsset);

        // Depreciation Validations
        void ValidateDepreciation(CreateDepreciationRecordDto dto, FixedAsset asset, AssetCategory category);

        // Fiscal Period Validations
        void ValidateFiscalPeriodOpen(DateTime transactionDate, long? companyId = null);
        (int FiscalYear, int FiscalPeriod) GetFiscalPeriodForDate(DateTime date, long? companyId = null);
    }

    [AppService(ServiceType = typeof(IAssetValidationService), ServiceLifetime = LifeTime.Transient)]
    public class AssetValidationService : IAssetValidationService
    {
        private readonly AssetCategoryRepository _categoryRepository;
        private readonly FixedAssetRepository _assetRepository;
        private readonly AssetDepreciationRepository _depreciationRepository;
        private readonly IGLPeriodClosingService _periodClosingService;

        public AssetValidationService(
            AssetCategoryRepository categoryRepository,
            FixedAssetRepository assetRepository,
            AssetDepreciationRepository depreciationRepository,
            IGLPeriodClosingService periodClosingService)
        {
            _categoryRepository = categoryRepository;
            _assetRepository = assetRepository;
            _depreciationRepository = depreciationRepository;
            _periodClosingService = periodClosingService;
        }

        #region Fixed Asset Validations

        public void ValidateCreateFixedAsset(CreateFixedAssetDto dto)
        {
            var errors = new List<string>();

            // CategoryId must exist
            var category = _categoryRepository.GetById(dto.CategoryId);
            if (category == null)
            {
                errors.Add("Category not found - الفئة غير موجودة");
            }

            // PurchaseCost > 0
            if (dto.PurchaseCost <= 0)
            {
                errors.Add("Purchase cost must be greater than zero - تكلفة الشراء يجب أن تكون أكبر من صفر");
            }

            // SalvageValue >= 0
            if (dto.SalvageValue < 0)
            {
                errors.Add("Salvage value cannot be negative - القيمة التخريدية لا يمكن أن تكون سالبة");
            }

            // SalvageValue < PurchaseCost
            if (dto.SalvageValue >= dto.PurchaseCost)
            {
                errors.Add("Salvage value must be less than purchase cost - القيمة التخريدية يجب أن تكون أقل من تكلفة الشراء");
            }

            // PurchaseDate cannot be in the future
            if (dto.PurchaseDate.Date > DateTime.Now.Date)
            {
                errors.Add("Purchase date cannot be in the future - تاريخ الشراء لا يمكن أن يكون في المستقبل");
            }

            // Validate fiscal period is open
            try
            {
                ValidateFiscalPeriodOpen(dto.PurchaseDate, dto.CompanyId);
            }
            catch (BasePeriodException ex)
            {
                // Handle all period exceptions: PeriodNotFoundException, PeriodClosedException, PeriodLockedException
                errors.Add($"{ex.MessageEn} - {ex.MessageAr}");
            }

            if (errors.Count > 0)
            {
                throw new AppException(ExceptionCode.FINANCE_ASSET_VALIDATION_FAILED, string.Join("; ", errors));
            }
        }

        public void ValidateUpdateFixedAsset(UpdateFixedAssetDto dto, FixedAsset existingAsset)
        {
            var errors = new List<string>();

            // Cannot modify sold or disposed asset
            if (existingAsset.Status == AssetStatus.Sold || existingAsset.Status == AssetStatus.Disposed)
            {
                errors.Add("Cannot modify a sold or disposed asset - لا يمكن تعديل أصل مباع أو مستبعد");
            }

            // CategoryId must exist
            var category = _categoryRepository.GetById(dto.CategoryId);
            if (category == null)
            {
                errors.Add("Category not found - الفئة غير موجودة");
            }

            // Cannot change category if depreciation already exists
            if (dto.CategoryId != existingAsset.CategoryId)
            {
                if (HasDepreciationRecords(existingAsset.Id))
                {
                    errors.Add("Cannot change category when depreciation records exist - لا يمكن تغيير الفئة عند وجود سجلات إهلاك");
                }
            }

            // PurchaseCost > 0
            if (dto.PurchaseCost <= 0)
            {
                errors.Add("Purchase cost must be greater than zero - تكلفة الشراء يجب أن تكون أكبر من صفر");
            }

            // SalvageValue >= 0
            if (dto.SalvageValue < 0)
            {
                errors.Add("Salvage value cannot be negative - القيمة التخريدية لا يمكن أن تكون سالبة");
            }

            // SalvageValue < PurchaseCost
            if (dto.SalvageValue >= dto.PurchaseCost)
            {
                errors.Add("Salvage value must be less than purchase cost - القيمة التخريدية يجب أن تكون أقل من تكلفة الشراء");
            }

            // Cannot reduce PurchaseCost below AccumulatedDepreciation
            if (dto.PurchaseCost < existingAsset.AccumulatedDepreciation)
            {
                errors.Add("Purchase cost cannot be less than accumulated depreciation - تكلفة الشراء لا يمكن أن تكون أقل من مجمع الإهلاك");
            }

            // PurchaseDate cannot be in the future
            if (dto.PurchaseDate.Date > DateTime.Now.Date)
            {
                errors.Add("Purchase date cannot be in the future - تاريخ الشراء لا يمكن أن يكون في المستقبل");
            }

            if (errors.Count > 0)
            {
                throw new AppException(ExceptionCode.FINANCE_ASSET_VALIDATION_FAILED, string.Join("; ", errors));
            }
        }

        public void ValidateDeleteFixedAsset(FixedAsset asset)
        {
            var errors = new List<string>();

            // Cannot delete sold or disposed asset
            if (asset.Status == AssetStatus.Sold || asset.Status == AssetStatus.Disposed)
            {
                errors.Add($"Cannot delete sold or disposed asset: {asset.AssetCode}");
            }

            // Cannot delete if depreciation records exist
            if (HasDepreciationRecords(asset.Id))
            {
                errors.Add($"Cannot delete asset with depreciation records: {asset.AssetCode}");
            }

            if (errors.Count > 0)
            {
                throw new AppException(ExceptionCode.FINANCE_ASSET_VALIDATION_FAILED, string.Join("; ", errors));
            }
        }

        public void ValidateAssetSale(AssetSaleDto dto, FixedAsset asset)
        {
            var errors = new List<string>();

            // Asset must be active
            if (asset.Status != AssetStatus.Active && asset.Status != AssetStatus.FullyDepreciated)
            {
                errors.Add("Asset must be active to be sold - يجب أن يكون الأصل نشطًا للبيع");
            }

            // Sale date cannot be in the future
            if (dto.SaleDate.Date > DateTime.Now.Date)
            {
                errors.Add("Sale date cannot be in the future - تاريخ البيع لا يمكن أن يكون في المستقبل");
            }

            // Sale price >= 0
            if (dto.SalePrice < 0)
            {
                errors.Add("Sale price cannot be negative - سعر البيع لا يمكن أن يكون سالبًا");
            }

            // Validate fiscal period is open
            try
            {
                ValidateFiscalPeriodOpen(dto.SaleDate, null);
            }
            catch (BasePeriodException ex)
            {
                // Handle all period exceptions: PeriodNotFoundException, PeriodClosedException, PeriodLockedException
                errors.Add($"{ex.MessageEn} - {ex.MessageAr}");
            }

            if (errors.Count > 0)
            {
                throw new AppException(ExceptionCode.FINANCE_ASSET_VALIDATION_FAILED, string.Join("; ", errors));
            }
        }

        public void ValidateAssetDisposal(AssetDisposalDto dto, FixedAsset asset)
        {
            var errors = new List<string>();

            // Asset must be active
            if (asset.Status != AssetStatus.Active && asset.Status != AssetStatus.FullyDepreciated)
            {
                errors.Add("Asset must be active to be disposed - يجب أن يكون الأصل نشطًا للاستبعاد");
            }

            // Disposal date cannot be in the future
            if (dto.DisposalDate.Date > DateTime.Now.Date)
            {
                errors.Add("Disposal date cannot be in the future - تاريخ الاستبعاد لا يمكن أن يكون في المستقبل");
            }

            // Validate fiscal period is open
            try
            {
                ValidateFiscalPeriodOpen(dto.DisposalDate, null);
            }
            catch (BasePeriodException ex)
            {
                // Handle all period exceptions: PeriodNotFoundException, PeriodClosedException, PeriodLockedException
                errors.Add($"{ex.MessageEn} - {ex.MessageAr}");
            }

            if (errors.Count > 0)
            {
                throw new AppException(ExceptionCode.FINANCE_ASSET_VALIDATION_FAILED, string.Join("; ", errors));
            }
        }

        public void ValidateAssetRevaluation(AssetRevaluationDto dto, FixedAsset asset)
        {
            var errors = new List<string>();

            // Asset must be active
            if (asset.Status != AssetStatus.Active)
            {
                errors.Add("Can only revalue active assets - يمكن إعادة تقييم الأصول النشطة فقط");
            }

            // Revaluation date cannot be in the future
            if (dto.RevaluationDate.Date > DateTime.Now.Date)
            {
                errors.Add("Revaluation date cannot be in the future - تاريخ إعادة التقييم لا يمكن أن يكون في المستقبل");
            }

            // NewValue must be >= SalvageValue (prevent negative NBV)
            if (dto.NewValue < asset.SalvageValue)
            {
                errors.Add("New value cannot be less than salvage value - القيمة الجديدة لا يمكن أن تكون أقل من القيمة التخريدية");
            }

            // NewValue must be > 0
            if (dto.NewValue <= 0)
            {
                errors.Add("New value must be greater than zero - القيمة الجديدة يجب أن تكون أكبر من صفر");
            }

            // Validate fiscal period is open
            try
            {
                ValidateFiscalPeriodOpen(dto.RevaluationDate, null);
            }
            catch (BasePeriodException ex)
            {
                // Handle all period exceptions: PeriodNotFoundException, PeriodClosedException, PeriodLockedException
                errors.Add($"{ex.MessageEn} - {ex.MessageAr}");
            }

            if (errors.Count > 0)
            {
                throw new AppException(ExceptionCode.FINANCE_ASSET_VALIDATION_FAILED, string.Join("; ", errors));
            }
        }

        #endregion

        #region Category Validations

        public void ValidateCreateCategory(CreateAssetCategoryDto dto)
        {
            var errors = new List<string>();

            // UsefulLifeMonths > 0 for depreciable categories
            if (dto.DepreciationMethod != DepreciationMethod.None && dto.UsefulLifeMonths <= 0)
            {
                errors.Add("Useful life must be greater than zero for depreciable categories - العمر الإنتاجي يجب أن يكون أكبر من صفر للفئات القابلة للإهلاك");
            }

            // GL accounts should be linked for depreciable categories (warning, not blocking)
            if (dto.DepreciationMethod != DepreciationMethod.None)
            {
                if (!dto.DefaultAssetAccountId.HasValue)
                {
                    errors.Add("Default asset account is required for depreciable categories - حساب الأصل الافتراضي مطلوب للفئات القابلة للإهلاك");
                }
                if (!dto.DefaultDepreciationExpenseAccountId.HasValue)
                {
                    errors.Add("Default depreciation expense account is required for depreciable categories - حساب مصروف الإهلاك الافتراضي مطلوب للفئات القابلة للإهلاك");
                }
                if (!dto.DefaultAccumulatedDepreciationAccountId.HasValue)
                {
                    errors.Add("Default accumulated depreciation account is required for depreciable categories - حساب مجمع الإهلاك الافتراضي مطلوب للفئات القابلة للإهلاك");
                }
            }

            if (errors.Count > 0)
            {
                throw new AppException(ExceptionCode.FINANCE_ASSET_VALIDATION_FAILED, string.Join("; ", errors));
            }
        }

        public void ValidateUpdateCategory(UpdateAssetCategoryDto dto, AssetCategory existingCategory)
        {
            var errors = new List<string>();

            // Check if assets exist under this category
            var hasAssets = HasAssetsInCategory(dto.Id);

            // Cannot edit depreciation method if assets exist
            if (hasAssets && dto.DepreciationMethod != existingCategory.DepreciationMethod)
            {
                errors.Add("Cannot change depreciation method when assets exist in category - لا يمكن تغيير طريقة الإهلاك عند وجود أصول في الفئة");
            }

            // Cannot switch between depreciable and non-depreciable if assets exist
            if (hasAssets)
            {
                bool wasDepreciable = existingCategory.DepreciationMethod != DepreciationMethod.None;
                bool isDepreciable = dto.DepreciationMethod != DepreciationMethod.None;
                if (wasDepreciable != isDepreciable)
                {
                    errors.Add("Cannot switch between depreciable and non-depreciable when assets exist - لا يمكن التبديل بين القابل للإهلاك وغير القابل للإهلاك عند وجود أصول");
                }
            }

            // UsefulLifeMonths > 0 for depreciable categories
            if (dto.DepreciationMethod != DepreciationMethod.None && dto.UsefulLifeMonths <= 0)
            {
                errors.Add("Useful life must be greater than zero for depreciable categories - العمر الإنتاجي يجب أن يكون أكبر من صفر للفئات القابلة للإهلاك");
            }

            if (errors.Count > 0)
            {
                throw new AppException(ExceptionCode.FINANCE_ASSET_VALIDATION_FAILED, string.Join("; ", errors));
            }
        }

        public void ValidateDeleteCategory(Guid categoryId)
        {
            if (HasAssetsInCategory(categoryId))
            {
                throw new AppException(ExceptionCode.FINANCE_ASSET_CATEGORY_HAS_ASSETS)
                    .WithParameter("CategoryId", categoryId);
            }
        }

        #endregion

        #region Current Asset Validations

        public void ValidateCreateCurrentAsset(CreateCurrentAssetDto dto)
        {
            var errors = new List<string>();

            // Cost > 0
            if (dto.Cost <= 0)
            {
                errors.Add("Cost must be greater than zero - التكلفة يجب أن تكون أكبر من صفر");
            }

            // PurchaseDate cannot be in future
            if (dto.PurchaseDate.Date > DateTime.Now.Date)
            {
                errors.Add("Purchase date cannot be in the future - تاريخ الشراء لا يمكن أن يكون في المستقبل");
            }

            if (errors.Count > 0)
            {
                throw new AppException(ExceptionCode.FINANCE_ASSET_VALIDATION_FAILED, string.Join("; ", errors));
            }
        }

        public void ValidateUpdateCurrentAsset(UpdateCurrentAssetDto dto, CurrentAsset existingAsset)
        {
            var errors = new List<string>();

            // Cost > 0
            if (dto.Cost <= 0)
            {
                errors.Add("Cost must be greater than zero - التكلفة يجب أن تكون أكبر من صفر");
            }

            // CurrentValue >= 0
            if (dto.CurrentValue < 0)
            {
                errors.Add("Current value cannot be negative - القيمة الحالية لا يمكن أن تكون سالبة");
            }

            // PurchaseDate cannot be in future
            if (dto.PurchaseDate.Date > DateTime.Now.Date)
            {
                errors.Add("Purchase date cannot be in the future - تاريخ الشراء لا يمكن أن يكون في المستقبل");
            }

            if (errors.Count > 0)
            {
                throw new AppException(ExceptionCode.FINANCE_ASSET_VALIDATION_FAILED, string.Join("; ", errors));
            }
        }

        #endregion

        #region Depreciation Validations

        public void ValidateDepreciation(CreateDepreciationRecordDto dto, FixedAsset asset, AssetCategory category)
        {
            var errors = new List<string>();

            // Asset must exist (already validated by caller)

            // Asset must be active
            if (asset.Status != AssetStatus.Active)
            {
                errors.Add("Can only depreciate active assets - يمكن إهلاك الأصول النشطة فقط");
            }

            // Category must allow depreciation
            if (category.DepreciationMethod == DepreciationMethod.None)
            {
                errors.Add("Asset category does not allow depreciation - فئة الأصل لا تسمح بالإهلاك");
            }

            // Depreciation amount > 0
            if (dto.DepreciationAmount <= 0)
            {
                errors.Add("Depreciation amount must be greater than zero - مبلغ الإهلاك يجب أن يكون أكبر من صفر");
            }

            // Depreciation date cannot be in the future
            if (dto.DepreciationDate.Date > DateTime.Now.Date)
            {
                errors.Add("Depreciation date cannot be in the future - تاريخ الإهلاك لا يمكن أن يكون في المستقبل");
            }

            // Prevent negative NBV (book value after depreciation should be >= salvage value)
            var newBookValue = asset.CurrentBookValue - dto.DepreciationAmount;
            if (newBookValue < 0)
            {
                errors.Add("Depreciation would result in negative book value - الإهلاك سيؤدي إلى قيمة دفترية سالبة");
            }

            // Don't allow depreciation below salvage value
            if (newBookValue < asset.SalvageValue && asset.CurrentBookValue > asset.SalvageValue)
            {
                var maxDepreciation = asset.CurrentBookValue - asset.SalvageValue;
                errors.Add($"Maximum depreciation amount is {maxDepreciation} - الحد الأقصى لمبلغ الإهلاك هو {maxDepreciation}");
            }

            // Validate fiscal period is open
            try
            {
                ValidateFiscalPeriodOpen(dto.DepreciationDate, dto.CompanyId);
            }
            catch (BasePeriodException ex)
            {
                // Handle all period exceptions: PeriodNotFoundException, PeriodClosedException, PeriodLockedException
                errors.Add($"{ex.MessageEn} - {ex.MessageAr}");
            }

            if (errors.Count > 0)
            {
                throw new AppException(ExceptionCode.FINANCE_ASSET_VALIDATION_FAILED, string.Join("; ", errors));
            }
        }

        #endregion

        #region Fiscal Period Validations

        public void ValidateFiscalPeriodOpen(DateTime transactionDate, long? companyId = null)
        {
            try
            {
                _periodClosingService.CheckIfDateInClosedOrLockedPeriod(transactionDate, companyId, false);
            }
            catch (PeriodNotFoundException)
            {
                // Re-throw PeriodNotFoundException so callers can handle it with proper message
                throw;
            }
            catch (PeriodClosedException)
            {
                throw;
            }
            catch (PeriodLockedException)
            {
                throw new PeriodClosedException(transactionDate);
            }
        }

        public (int FiscalYear, int FiscalPeriod) GetFiscalPeriodForDate(DateTime date, long? companyId = null)
        {
            // Get open periods and find the one that contains the date
            var openPeriods = _periodClosingService.GetOpenPeriods(companyId);

            foreach (var period in openPeriods)
            {
                // Check if this period contains the date
                if (date >= period.PeriodStartDate && date <= period.PeriodEndDate)
                {
                    return (period.FiscalYear, period.FiscalPeriod);
                }
            }

            // Default: use transaction date's year and month
            return (date.Year, date.Month);
        }

        #endregion

        #region Helper Methods

        private bool HasDepreciationRecords(Guid assetId)
        {
            var predicate = Expressionable.Create<AssetDepreciation>();
            predicate = predicate.And(x => x.AssetId == assetId);
            return _depreciationRepository.Count(predicate.ToExpression()) > 0;
        }

        private bool HasAssetsInCategory(Guid categoryId)
        {
            var predicate = Expressionable.Create<FixedAsset>();
            predicate = predicate.And(x => x.CategoryId == categoryId);
            return _assetRepository.Count(predicate.ToExpression()) > 0;
        }

        #endregion
    }
}
