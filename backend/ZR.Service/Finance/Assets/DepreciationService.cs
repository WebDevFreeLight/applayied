using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
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
    /// Depreciation Service - خدمة الإهلاك
    /// Enhanced with comprehensive validation, GL integration, and atomic operations
    /// </summary>
    [AppService(ServiceType = typeof(IDepreciationService), ServiceLifetime = LifeTime.Transient)]
    public class DepreciationService : BaseService<AssetDepreciation>, IDepreciationService
    {
        private readonly AssetDepreciationRepository _repository;
        private readonly FixedAssetRepository _assetRepository;
        private readonly AssetCategoryRepository _categoryRepository;
        private readonly AssetTransactionRepository _transactionRepository;
        private readonly IFixedAssetService _fixedAssetService;
        private readonly IAssetValidationService _validationService;
        private readonly IAssetGLIntegrationService _glIntegrationService;

        public DepreciationService(
            AssetDepreciationRepository repository,
            FixedAssetRepository assetRepository,
            AssetCategoryRepository categoryRepository,
            AssetTransactionRepository transactionRepository,
            IFixedAssetService fixedAssetService,
            IAssetValidationService validationService,
            IAssetGLIntegrationService glIntegrationService)
        {
            _repository = repository;
            _assetRepository = assetRepository;
            _categoryRepository = categoryRepository;
            _transactionRepository = transactionRepository;
            _fixedAssetService = fixedAssetService;
            _validationService = validationService;
            _glIntegrationService = glIntegrationService;
        }

        /// <summary>
        /// Get depreciation records with paging - الحصول على سجلات الإهلاك مع ترقيم الصفحات
        /// </summary>
        public PagedInfo<DepreciationRecordListDto> GetList(AssetDepreciationQueryDto parm)
        {
            var predicate = Expressionable.Create<AssetDepreciation>();

            // Filter by tenant
            if (parm.CompanyId.HasValue)
            {
                predicate = predicate.And(x => x.CompanyID == parm.CompanyId.Value);
            }

            // Filter by asset
            if (parm.AssetId.HasValue)
            {
                predicate = predicate.And(x => x.AssetId == parm.AssetId.Value);
            }

            // Filter by fiscal year
            if (parm.FiscalYear.HasValue)
            {
                predicate = predicate.And(x => x.FiscalYear == parm.FiscalYear.Value);
            }

            // Filter by fiscal period
            if (parm.FiscalPeriod.HasValue)
            {
                predicate = predicate.And(x => x.FiscalPeriod == parm.FiscalPeriod.Value);
            }

            // Filter by depreciation date range
            if (parm.DepreciationDateFrom.HasValue)
            {
                predicate = predicate.And(x => x.DepreciationDate >= parm.DepreciationDateFrom.Value);
            }
            if (parm.DepreciationDateTo.HasValue)
            {
                predicate = predicate.And(x => x.DepreciationDate <= parm.DepreciationDateTo.Value);
            }

            var response = _repository.GetPages(predicate.ToExpression(), parm);

            var pagedResult = response.Adapt<PagedInfo<DepreciationRecordListDto>>();

            // Enrich with asset information
            foreach (var item in pagedResult.Result)
            {
                var asset = _assetRepository.GetById(item.AssetId);
                if (asset != null)
                {
                    item.AssetCode = asset.AssetCode;
                    item.AssetName = asset.NameEn;
                }
            }

            return pagedResult;
        }

        /// <summary>
        /// Get depreciation records for an asset - الحصول على سجلات الإهلاك لأصل معين
        /// </summary>
        public List<DepreciationRecordListDto> GetAssetDepreciationHistory(Guid assetId)
        {
            var predicate = Expressionable.Create<AssetDepreciation>();
            predicate = predicate.And(x => x.AssetId == assetId);

            var list = _repository.GetList(predicate.ToExpression())
                .OrderBy(x => x.DepreciationDate)
                .ToList();

            var result = list.Adapt<List<DepreciationRecordListDto>>();

            // Get asset info
            var asset = _assetRepository.GetById(assetId);
            if (asset != null)
            {
                foreach (var item in result)
                {
                    item.AssetCode = asset.AssetCode;
                    item.AssetName = asset.NameEn;
                }
            }

            return result;
        }

        /// <summary>
        /// Generate monthly depreciation for all active assets - توليد الإهلاك الشهري لجميع الأصول النشطة
        /// Uses TransactionScope for atomicity - if any asset fails, all are rolled back
        /// </summary>
        public DepreciationSummaryDto GenerateMonthlyDepreciation(GenerateMonthlyDepreciationDto dto, string userId)
        {
            // CRITICAL: Validate fiscal period is open before processing
            _validationService.ValidateFiscalPeriodOpen(dto.DepreciationDate, dto.CompanyId);

            var summary = new DepreciationSummaryDto
            {
                FiscalYear = dto.FiscalYear,
                FiscalPeriod = dto.FiscalPeriod,
                ProcessedDate = DateTime.Now,
                TotalDepreciation = 0,
                AssetCount = 0,
                Details = new List<DepreciationDetailItemDto>()
            };

            // Get all depreciable assets
            var assets = _fixedAssetService.GetDepreciableAssets(dto.CompanyId);

            // Filter out already depreciated assets for this period
            var eligibleAssets = assets.Where(a => !DepreciationExistsForPeriod(a.Id, dto.FiscalYear, dto.FiscalPeriod)).ToList();

            if (!eligibleAssets.Any())
            {
                summary.Message = "No eligible assets for depreciation - لا توجد أصول مؤهلة للإهلاك";
                return summary;
            }

            using var scope = new TransactionScope(TransactionScopeOption.Required,
                new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted });

            try
            {
                foreach (var asset in eligibleAssets)
                {
                    // Calculate depreciation amount
                    var depreciationAmount = _fixedAssetService.CalculateDepreciationAmount(asset.Id);
                    if (depreciationAmount <= 0)
                    {
                        continue;
                    }

                    var category = _categoryRepository.GetById(asset.CategoryId);

                    // Create depreciation record
                    var depRecord = new CreateDepreciationRecordDto
                    {
                        CompanyId = dto.CompanyId,
                        AssetId = asset.Id,
                        DepreciationDate = dto.DepreciationDate,
                        DepreciationAmount = depreciationAmount,
                        FiscalYear = dto.FiscalYear,
                        FiscalPeriod = dto.FiscalPeriod,
                        Notes = $"Monthly depreciation for {dto.FiscalYear}/{dto.FiscalPeriod}"
                    };

                    var depResult = DepreciateAssetInternal(depRecord, userId, asset, category);

                    summary.TotalDepreciation += depreciationAmount;
                    summary.AssetCount++;
                    summary.Details.Add(new DepreciationDetailItemDto
                    {
                        AssetId = asset.Id,
                        AssetCode = asset.AssetCode,
                        AssetName = asset.NameEn,
                        DepreciationAmount = depreciationAmount,
                        BookValueAfter = depResult.BookValueAfter,
                        JournalId = depResult.GLJournalId
                    });
                }

                scope.Complete();
                summary.Message = $"Successfully depreciated {summary.AssetCount} assets";
                return summary;
            }
            catch
            {
                // Transaction will be rolled back automatically
                throw;
            }
        }

        /// <summary>
        /// Generate depreciation for a single asset - توليد الإهلاك لأصل واحد
        /// Uses TransactionScope for atomicity
        /// </summary>
        public AssetDepreciation DepreciateAsset(CreateDepreciationRecordDto dto, string userId)
        {
            var asset = _assetRepository.GetById(dto.AssetId);
            if (asset == null)
            {
                throw new AppException(ExceptionCode.FINANCE_ASSET_NOT_FOUND)
                    .WithParameter("AssetId", dto.AssetId);
            }

            var category = _categoryRepository.GetById(asset.CategoryId);
            if (category == null)
            {
                throw new AppException(ExceptionCode.FINANCE_ASSET_CATEGORY_NOT_FOUND)
                    .WithParameter("CategoryId", asset.CategoryId);
            }

            // Run comprehensive validations
            _validationService.ValidateDepreciation(dto, asset, category);

            // Check if already depreciated for this period
            if (DepreciationExistsForPeriod(dto.AssetId, dto.FiscalYear, dto.FiscalPeriod))
            {
                throw new AppException(ExceptionCode.FINANCE_ASSET_DEPRECIATION_EXISTS);
            }

            // Check if asset can be depreciated
            if (!_fixedAssetService.CanDepreciate(dto.AssetId))
            {
                throw new AppException(ExceptionCode.FINANCE_ASSET_CANNOT_DEPRECIATE)
                    .WithParameter("AssetId", dto.AssetId);
            }

            using var scope = new TransactionScope(TransactionScopeOption.Required,
                new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted });

            try
            {
                var result = DepreciateAssetInternal(dto, userId, asset, category);
                scope.Complete();
                return result;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Internal depreciation logic (used by both single and batch depreciation)
        /// </summary>
        private AssetDepreciation DepreciateAssetInternal(CreateDepreciationRecordDto dto, string userId, FixedAsset asset, AssetCategory category)
        {
            Guid? userGuid = null;
            if (Guid.TryParse(userId, out Guid parsedGuid))
            {
                userGuid = parsedGuid;
            }

            // Create GL Journal for depreciation
            Guid? journalId = null;
            try
            {
                var journal = _glIntegrationService.CreateDepreciationJournal(
                    asset,
                    dto.DepreciationAmount,
                    dto.DepreciationDate,
                    dto.FiscalYear,
                    dto.FiscalPeriod,
                    userId,
                    dto.CompanyId);
                journalId = journal?.Id;
            }
            catch (Exception ex)
            {
                // Log the error but continue - GL integration is important but not blocking
                Console.WriteLine($"Warning: Failed to create GL Journal for depreciation: {ex.Message}");
            }

            // Create depreciation record
            var entity = new AssetDepreciation
            {
                Id = Guid.NewGuid(),
                CompanyID = dto.CompanyId ?? asset.CompanyID,
                AssetId = dto.AssetId,
                DepreciationDate = dto.DepreciationDate,
                DepreciationAmount = dto.DepreciationAmount,
                AccumulatedValueAfter = asset.AccumulatedDepreciation + dto.DepreciationAmount,
                BookValueAfter = asset.CurrentBookValue - dto.DepreciationAmount,
                FiscalYear = dto.FiscalYear,
                FiscalPeriod = dto.FiscalPeriod,
                GLJournalId = journalId ?? dto.GLJournalId,
                Create_by = userGuid?.ToString() ?? userId,
                Create_time = DateTime.Now
            };

            _repository.Insert(entity);

            // Record depreciation transaction with fiscal period
            var transaction = new AssetTransaction
            {
                Id = Guid.NewGuid(),
                CompanyID = dto.CompanyId ?? asset.CompanyID,
                AssetId = asset.Id,
                TransactionType = AssetTransactionType.Depreciation,
                TransactionDate = dto.DepreciationDate,
                Amount = dto.DepreciationAmount,
                PreviousBookValue = asset.CurrentBookValue,
                NewBookValue = asset.CurrentBookValue - dto.DepreciationAmount,
                GainLossAmount = null,
                Notes = dto.Notes,
                GLJournalId = journalId ?? dto.GLJournalId,
                FiscalYear = dto.FiscalYear,
                FiscalPeriod = dto.FiscalPeriod,
                Create_by = userGuid?.ToString() ?? userId,
                Create_time = DateTime.Now
            };

            _transactionRepository.Insert(transaction);

            // Update asset book value
            _fixedAssetService.UpdateBookValue(dto.AssetId, dto.DepreciationAmount);

            return entity;
        }

        /// <summary>
        /// Check if depreciation already exists for period - التحقق من وجود إهلاك للفترة
        /// </summary>
        public bool DepreciationExistsForPeriod(Guid assetId, int fiscalYear, int fiscalPeriod)
        {
            var predicate = Expressionable.Create<AssetDepreciation>();
            predicate = predicate.And(x => x.AssetId == assetId);
            predicate = predicate.And(x => x.FiscalYear == fiscalYear);
            predicate = predicate.And(x => x.FiscalPeriod == fiscalPeriod);

            return _repository.Count(predicate.ToExpression()) > 0;
        }

        /// <summary>
        /// Get total depreciation for an asset - الحصول على إجمالي الإهلاك لأصل
        /// </summary>
        public decimal GetTotalDepreciation(Guid assetId)
        {
            var predicate = Expressionable.Create<AssetDepreciation>();
            predicate = predicate.And(x => x.AssetId == assetId);

            var records = _repository.GetList(predicate.ToExpression());
            return records.Sum(x => x.DepreciationAmount);
        }

        /// <summary>
        /// Get depreciation summary by period - الحصول على ملخص الإهلاك بالفترة
        /// </summary>
        public DepreciationSummaryDto GetPeriodSummary(int fiscalYear, int fiscalPeriod, long? companyId = null)
        {
            var predicate = Expressionable.Create<AssetDepreciation>();
            predicate = predicate.And(x => x.FiscalYear == fiscalYear);
            predicate = predicate.And(x => x.FiscalPeriod == fiscalPeriod);

            if (companyId.HasValue)
            {
                predicate = predicate.And(x => x.CompanyID == companyId.Value);
            }

            var records = _repository.GetList(predicate.ToExpression());

            var summary = new DepreciationSummaryDto
            {
                FiscalYear = fiscalYear,
                FiscalPeriod = fiscalPeriod,
                TotalDepreciation = records.Sum(x => x.DepreciationAmount),
                AssetCount = records.Select(x => x.AssetId).Distinct().Count(),
                ProcessedDate = records.Any() ? records.Max(x => x.Create_time) : DateTime.Now,
                Details = new List<DepreciationDetailItemDto>()
            };

            // Add details for each asset
            foreach (var group in records.GroupBy(r => r.AssetId))
            {
                var asset = _assetRepository.GetById(group.Key);
                if (asset != null)
                {
                    var latestRecord = group.OrderByDescending(r => r.DepreciationDate).First();
                    summary.Details.Add(new DepreciationDetailItemDto
                    {
                        AssetId = asset.Id,
                        AssetCode = asset.AssetCode,
                        AssetName = asset.NameEn,
                        DepreciationAmount = group.Sum(r => r.DepreciationAmount),
                        BookValueAfter = latestRecord.BookValueAfter,
                        JournalId = latestRecord.GLJournalId
                    });
                }
            }

            return summary;
        }

    }
}
