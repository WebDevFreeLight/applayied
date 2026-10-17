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
using ZR.Model.Finance.GL;
using ZR.Model.Finance.Enums;
using ZR.Repository.Finance.Assets;
using ZR.Service.IService.Finance.Assets;

namespace ZR.Service.Finance.Assets
{
    /// <summary>
    /// Fixed Asset Service - خدمة الأصول الثابتة
    /// Enhanced with comprehensive validation, GL integration, and atomic operations
    /// </summary>
    [AppService(ServiceType = typeof(IFixedAssetService), ServiceLifetime = LifeTime.Transient)]
    public class FixedAssetService : BaseService<FixedAsset>, IFixedAssetService
    {
        private readonly FixedAssetRepository _repository;
        private readonly AssetCategoryRepository _categoryRepository;
        private readonly AssetDepreciationRepository _depreciationRepository;
        private readonly AssetTransactionRepository _transactionRepository;
        private readonly IAssetValidationService _validationService;
        private readonly IAssetGLIntegrationService _glIntegrationService;

        public FixedAssetService(
            FixedAssetRepository repository,
            AssetCategoryRepository categoryRepository,
            AssetDepreciationRepository depreciationRepository,
            AssetTransactionRepository transactionRepository,
            IAssetValidationService validationService,
            IAssetGLIntegrationService glIntegrationService)
        {
            _repository = repository;
            _categoryRepository = categoryRepository;
            _depreciationRepository = depreciationRepository;
            _transactionRepository = transactionRepository;
            _validationService = validationService;
            _glIntegrationService = glIntegrationService;
        }

        /// <summary>
        /// Get asset list with paging - الحصول على قائمة الأصول مع ترقيم الصفحات
        /// </summary>
        public PagedInfo<FixedAssetListDto> GetList(FixedAssetQueryDto parm)
        {
            var predicate = Expressionable.Create<FixedAsset>();

            // Filter by company
            if (parm.CompanyId.HasValue)
            {
                predicate = predicate.And(x => x.CompanyID == parm.CompanyId.Value);
            }

            // Filter by code
            if (!string.IsNullOrEmpty(parm.AssetCode))
            {
                predicate = predicate.And(x => x.AssetCode.Contains(parm.AssetCode));
            }

            // Filter by name
            if (!string.IsNullOrEmpty(parm.AssetName))
            {
                predicate = predicate.And(x => x.NameEn.Contains(parm.AssetName) ||
                    (x.NameAr != null && x.NameAr.Contains(parm.AssetName)));
            }

            // Filter by category
            if (parm.CategoryId.HasValue)
            {
                predicate = predicate.And(x => x.CategoryId == parm.CategoryId.Value);
            }

            // Filter by status
            if (parm.Status.HasValue)
            {
                predicate = predicate.And(x => x.Status == parm.Status.Value);
            }

            // Filter by purchase date range
            if (parm.PurchaseDateFrom.HasValue)
            {
                predicate = predicate.And(x => x.PurchaseDate >= parm.PurchaseDateFrom.Value);
            }
            if (parm.PurchaseDateTo.HasValue)
            {
                predicate = predicate.And(x => x.PurchaseDate <= parm.PurchaseDateTo.Value);
            }

            var response = _repository.GetPages(predicate.ToExpression(), parm);

            var pagedResult = response.Adapt<PagedInfo<FixedAssetListDto>>();

            // Enrich with category information
            foreach (var item in pagedResult.Result)
            {
                var category = _categoryRepository.GetById(item.CategoryId);
                if (category != null)
                {
                    item.CategoryNameAr = category.NameAr;
                    item.CategoryNameEn = category.NameEn;
                }
            }

            return pagedResult;
        }

        /// <summary>
        /// Get asset by ID - الحصول على أصل بالمعرف
        /// </summary>
        public FixedAssetDetailDto GetDetail(Guid id)
        {
            var entity = _repository.GetById(id);
            if (entity == null)
            {
                return null;
            }

            var dto = entity.Adapt<FixedAssetDetailDto>();

            // Get category info
            var category = _categoryRepository.GetById(entity.CategoryId);
            if (category != null)
            {
                dto.CategoryName = category.NameEn;
                dto.CategoryNameAr = category.NameAr;
                dto.CategoryNameEn = category.NameEn;
                dto.CategoryCode = category.CategoryCode;
            }

            return dto;
        }

        /// <summary>
        /// Create asset - إنشاء أصل
        /// Uses SqlSugar transaction for atomicity
        /// </summary>
        public FixedAsset Create(CreateFixedAssetDto dto, string userId)
        {
            // Auto-generate code if not provided
            if (string.IsNullOrWhiteSpace(dto.AssetCode))
            {
                dto.AssetCode = GetNextAssetCode(dto.CompanyId);
            }

            // Run all validations
            _validationService.ValidateCreateFixedAsset(dto);

            // Check for duplicate code
            if (CodeExists(dto.AssetCode, null, dto.CompanyId))
            {
                throw new AppException(ExceptionCode.FINANCE_ASSET_CODE_DUPLICATE)
                    .WithParameter("Code", dto.AssetCode);
            }

            // Get fiscal period for the purchase date BEFORE transaction
            var (fiscalYear, fiscalPeriod) = _validationService.GetFiscalPeriodForDate(dto.PurchaseDate, dto.CompanyId);

            var entity = dto.Adapt<FixedAsset>();
            entity.Id = Guid.NewGuid();
            entity.Status = AssetStatus.Active;

            // CORRECT: CurrentBookValue = PurchaseCost - AccumulatedDepreciation
            entity.CurrentBookValue = dto.PurchaseCost;
            entity.AccumulatedDepreciation = 0;

            // Set GL accounts from category if not provided
            var category = _categoryRepository.GetById(dto.CategoryId);
            if (category != null)
            {
                entity.GLAssetAccountId = dto.GLAssetAccountId ?? category.DefaultAssetAccountId;
                entity.GLAccumulatedDepreciationAccountId = dto.GLAccumulatedDepreciationAccountId ?? category.DefaultAccumulatedDepreciationAccountId;
                entity.GLDepreciationExpenseAccountId = dto.GLDepreciationExpenseAccountId ?? category.DefaultDepreciationExpenseAccountId;
            }

            entity.CompanyID = dto.CompanyId;
            entity.Create_time = DateTime.Now;
            entity.Update_time = DateTime.Now;

            if (Guid.TryParse(userId, out Guid userGuid))
            {
                entity.Create_by = userGuid.ToString();
                entity.Update_by = userGuid.ToString();
            }

            // Use SqlSugar's native transaction
            var db = _repository.Context;
            try
            {
                db.Ado.BeginTran();

                db.Insertable(entity).ExecuteCommand();

                // Record purchase transaction with fiscal period
                var transaction = RecordPurchaseTransactionInternal(db, entity, userId, fiscalYear, fiscalPeriod);

                db.Ado.CommitTran();
                return entity;
            }
            catch (Exception ex)
            {
                db.Ado.RollbackTran();
                throw new AppException(ExceptionCode.FINANCE_ASSET_CREATE_FAILED, ex.Message);
            }
        }

        /// <summary>
        /// Update asset - تحديث أصل
        /// </summary>
        public int Update(UpdateFixedAssetDto dto, string userId)
        {
            // Check for duplicate code (excluding current record)
            if (CodeExists(dto.AssetCode, dto.Id, dto.CompanyId))
            {
                throw new AppException(ExceptionCode.FINANCE_ASSET_CODE_DUPLICATE)
                    .WithParameter("Code", dto.AssetCode);
            }

            var entity = _repository.GetById(dto.Id);
            if (entity == null)
            {
                throw new AppException(ExceptionCode.FINANCE_ASSET_NOT_FOUND)
                    .WithParameter("AssetId", dto.Id);
            }

            // Run all validations
            _validationService.ValidateUpdateFixedAsset(dto, entity);

            // Update properties
            entity.AssetCode = dto.AssetCode;
            entity.NameEn = dto.NameEn;
            entity.NameAr = dto.NameAr;
            entity.CategoryId = dto.CategoryId;
            entity.PurchaseDate = dto.PurchaseDate;
            entity.PurchaseCost = dto.PurchaseCost;
            entity.SalvageValue = dto.SalvageValue;
            entity.GLAssetAccountId = dto.GLAssetAccountId;
            entity.GLAccumulatedDepreciationAccountId = dto.GLAccumulatedDepreciationAccountId;
            entity.GLDepreciationExpenseAccountId = dto.GLDepreciationExpenseAccountId;
            entity.Notes = dto.Notes;
            entity.Update_time = DateTime.Now;

            if (Guid.TryParse(userId, out Guid userGuid))
            {
                entity.Update_by = userGuid.ToString();
            }

            // CORRECT: Recalculate book value
            // CurrentBookValue = PurchaseCost - AccumulatedDepreciation
            entity.CurrentBookValue = entity.PurchaseCost - entity.AccumulatedDepreciation;
            if (entity.CurrentBookValue < 0) entity.CurrentBookValue = 0;

            // Check if fully depreciated (book value at or below salvage value)
            if (entity.CurrentBookValue <= entity.SalvageValue && entity.Status == AssetStatus.Active)
            {
                entity.Status = AssetStatus.FullyDepreciated;
            }

            return _repository.Update(entity);
        }

        /// <summary>
        /// Delete assets - حذف أصول
        /// </summary>
        public int Delete(Guid[] ids)
        {
            // Validate each asset before deletion
            foreach (var id in ids)
            {
                var asset = _repository.GetById(id);
                if (asset != null)
                {
                    _validationService.ValidateDeleteFixedAsset(asset);
                }
            }

            return _repository.Delete(ids);
        }

        /// <summary>
        /// Check if asset code exists - التحقق من وجود رمز الأصل
        /// </summary>
        public bool CodeExists(string code, Guid? excludeId = null, long? companyId = null)
        {
            var predicate = Expressionable.Create<FixedAsset>();
            predicate = predicate.And(x => x.AssetCode == code);

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
        /// Calculate depreciation amount for an asset - حساب مبلغ الإهلاك للأصل
        /// </summary>
        public decimal CalculateDepreciationAmount(Guid assetId)
        {
            var asset = _repository.GetById(assetId);
            if (asset == null)
            {
                throw new AppException(ExceptionCode.FINANCE_ASSET_NOT_FOUND)
                    .WithParameter("AssetId", assetId);
            }

            if (!CanDepreciate(assetId))
            {
                return 0;
            }

            // Get category for depreciation settings
            var category = _categoryRepository.GetById(asset.CategoryId);
            if (category == null)
            {
                return 0;
            }

            // Depreciable amount = PurchaseCost - SalvageValue
            decimal depreciableAmount = asset.PurchaseCost - asset.SalvageValue;
            // Remaining depreciable = CurrentBookValue - SalvageValue
            decimal remainingDepreciable = asset.CurrentBookValue - asset.SalvageValue;

            switch (category.DepreciationMethod)
            {
                case DepreciationMethod.StraightLine:
                    // Monthly depreciation = (PurchaseCost - SalvageValue) / UsefulLifeMonths
                    if (category.UsefulLifeMonths > 0)
                    {
                        var monthlyDepreciation = Math.Round(depreciableAmount / category.UsefulLifeMonths, 2);
                        // Don't exceed remaining depreciable amount
                        return Math.Min(monthlyDepreciation, remainingDepreciable);
                    }
                    return 0;

                case DepreciationMethod.DecliningBalance:
                    // Double declining balance: (2 / UsefulLifeMonths) * BookValue
                    if (category.UsefulLifeMonths > 0 && remainingDepreciable > 0)
                    {
                        decimal rate = 2.0m / category.UsefulLifeMonths;
                        decimal depreciation = Math.Round(asset.CurrentBookValue * rate, 2);

                        // Don't depreciate below salvage value
                        if (depreciation > remainingDepreciable)
                        {
                            depreciation = remainingDepreciable;
                        }
                        return depreciation;
                    }
                    return 0;

                case DepreciationMethod.None:
                default:
                    return 0;
            }
        }

        /// <summary>
        /// Check if asset can be deprecated - التحقق من إمكانية إهلاك الأصل
        /// </summary>
        public bool CanDepreciate(Guid assetId)
        {
            var asset = _repository.GetById(assetId);
            if (asset == null)
            {
                return false;
            }

            // Cannot depreciate if not active
            if (asset.Status != AssetStatus.Active)
            {
                return false;
            }

            // Get category for depreciation settings
            var category = _categoryRepository.GetById(asset.CategoryId);
            if (category == null || category.DepreciationMethod == DepreciationMethod.None)
            {
                return false;
            }

            // Cannot depreciate if book value is at or below salvage value
            if (asset.CurrentBookValue <= asset.SalvageValue)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Get depreciable assets - الحصول على الأصول القابلة للإهلاك
        /// </summary>
        public List<FixedAsset> GetDepreciableAssets(long? companyId = null)
        {
            var predicate = Expressionable.Create<FixedAsset>();
            predicate = predicate.And(x => x.Status == AssetStatus.Active);
            predicate = predicate.And(x => x.CurrentBookValue > x.SalvageValue);

            if (companyId.HasValue)
            {
                predicate = predicate.And(x => x.CompanyID == companyId.Value);
            }

            var assets = _repository.GetList(predicate.ToExpression());

            // Filter by depreciation eligibility (check category depreciation method)
            return assets.Where(a => CanDepreciate(a.Id)).ToList();
        }

        /// <summary>
        /// Update asset book value - تحديث القيمة الدفترية للأصل
        /// </summary>
        public int UpdateBookValue(Guid assetId, decimal depreciationAmount)
        {
            var asset = _repository.GetById(assetId);
            if (asset == null)
            {
                throw new AppException(ExceptionCode.FINANCE_ASSET_NOT_FOUND)
                    .WithParameter("AssetId", assetId);
            }

            asset.AccumulatedDepreciation += depreciationAmount;
            // CORRECT: CurrentBookValue = PurchaseCost - AccumulatedDepreciation
            asset.CurrentBookValue = asset.PurchaseCost - asset.AccumulatedDepreciation;
            asset.LastDepreciationDate = DateTime.Now;

            if (asset.CurrentBookValue < 0)
            {
                asset.CurrentBookValue = 0;
            }

            // Check if fully depreciated (book value at or below salvage value)
            if (asset.CurrentBookValue <= asset.SalvageValue)
            {
                asset.Status = AssetStatus.FullyDepreciated;
            }

            asset.Update_time = DateTime.Now;

            return _repository.Update(asset);
        }

        /// <summary>
        /// Sell asset - بيع الأصل
        /// Uses SqlSugar transaction for atomicity, creates GL Journal
        /// </summary>
        public AssetTransaction SellAsset(AssetSaleDto dto, string userId)
        {
            var asset = _repository.GetById(dto.AssetId);
            if (asset == null)
            {
                throw new AppException(ExceptionCode.FINANCE_ASSET_NOT_FOUND)
                    .WithParameter("AssetId", dto.AssetId);
            }

            // Run all validations BEFORE starting transaction
            _validationService.ValidateAssetSale(dto, asset);

            // Get fiscal period for the sale date BEFORE transaction
            var (fiscalYear, fiscalPeriod) = _validationService.GetFiscalPeriodForDate(dto.SaleDate, null);

            // Calculate gain/loss: SalePrice - BookValue
            decimal bookValue = asset.PurchaseCost - asset.AccumulatedDepreciation;
            decimal gainLoss = dto.SalePrice - bookValue;

            Guid? userGuid = null;
            if (Guid.TryParse(userId, out Guid parsedGuid))
            {
                userGuid = parsedGuid;
            }

            // Prepare the transaction record
            var assetTransaction = new AssetTransaction
            {
                Id = Guid.NewGuid(),
                CompanyID = asset.CompanyID,
                AssetId = asset.Id,
                TransactionType = AssetTransactionType.Sale,
                TransactionDate = dto.SaleDate,
                Amount = dto.SalePrice,
                PreviousBookValue = bookValue,
                NewBookValue = 0,
                GainLossAmount = gainLoss,
                Notes = dto.Notes,
                ReferenceNumber = dto.ReferenceNumber,
                FiscalYear = fiscalYear,
                FiscalPeriod = fiscalPeriod,
                Create_by = userGuid?.ToString() ?? userId,
                Create_time = DateTime.Now
            };

            // Use SqlSugar's native transaction - ALL operations in one transaction
            var db = _repository.Context;
            try
            {
                db.Ado.BeginTran();

                // 1. Insert asset transaction record
                db.Insertable(assetTransaction).ExecuteCommand();

                // 2. Update asset status
                asset.Status = AssetStatus.Sold;
                asset.CurrentBookValue = 0;
                asset.Update_time = DateTime.Now;
                asset.Update_by = userGuid?.ToString() ?? userId;
                db.Updateable(asset).ExecuteCommand();

                // 3. Create GL Journal for the sale - use the SAME db context
                var journal = CreateSaleJournalInternal(db, asset, dto.SalePrice, dto.SaleDate,
                    dto.CashAccountId, fiscalYear, fiscalPeriod, userId);

                if (journal == null)
                {
                    throw new AppException(ExceptionCode.FINANCE_ASSET_GL_JOURNAL_FAILED);
                }

                // 4. Update transaction with journal ID
                assetTransaction.GLJournalId = journal.Id;
                db.Updateable(assetTransaction).UpdateColumns(t => new { t.GLJournalId }).ExecuteCommand();

                // Commit all changes
                db.Ado.CommitTran();

                return assetTransaction;
            }
            catch (Exception ex)
            {
                db.Ado.RollbackTran();
                throw new AppException(ExceptionCode.FINANCE_ASSET_SALE_FAILED, ex.Message);
            }
        }

        /// <summary>
        /// Create sale journal using the same database context (for transaction support)
        /// </summary>
        private GLJournal CreateSaleJournalInternal(ISqlSugarClient db, FixedAsset asset, decimal salePrice,
            DateTime saleDate, Guid cashAccountId, int fiscalYear, int fiscalPeriod, string userId)
        {
            var category = _categoryRepository.GetById(asset.CategoryId);

            var assetAccountId = asset.GLAssetAccountId ?? category?.DefaultAssetAccountId;
            var accDepAccountId = asset.GLAccumulatedDepreciationAccountId ?? category?.DefaultAccumulatedDepreciationAccountId;
            var gainLossAccountId = category?.DefaultGainLossAccountId;

            if (!assetAccountId.HasValue)
            {
                throw new AppException(ExceptionCode.FINANCE_ASSET_ACCOUNT_NOT_CONFIGURED);
            }
            if (!gainLossAccountId.HasValue)
            {
                throw new AppException(ExceptionCode.FINANCE_ASSET_GAINLOSS_ACCOUNT_NOT_CONFIGURED);
            }

            // Calculate gain/loss
            var bookValue = asset.PurchaseCost - asset.AccumulatedDepreciation;
            var gainLoss = salePrice - bookValue;

            // Generate journal number
            var year = DateTime.Now.Year;
            var prefix = $"JV-{year}-";
            var lastJournal = db.Queryable<GLJournal>()
                .Where(x => x.JournalNumber.StartsWith(prefix))
                .OrderByDescending(x => x.JournalNumber)
                .First();

            string journalNumber;
            if (lastJournal == null)
            {
                journalNumber = $"{prefix}00001";
            }
            else
            {
                var lastNumber = lastJournal.JournalNumber.Replace(prefix, "");
                if (int.TryParse(lastNumber, out int num))
                {
                    journalNumber = $"{prefix}{(num + 1):D5}";
                }
                else
                {
                    journalNumber = $"{prefix}00001";
                }
            }

            // Create journal
            var journal = new GLJournal
            {
                Id = Guid.NewGuid(),
                JournalNumber = journalNumber,
                JournalDate = saleDate,
                DescriptionAr = $"بيع أصل: {asset.NameAr ?? asset.AssetCode}",
                DescriptionEn = $"Sale of asset: {asset.NameEn ?? asset.AssetCode}",
                Status = JournalStatus.Draft,
                Source = "FixedAssets",
                SourceDocumentType = "AssetSale",
                SourceDocumentId = asset.Id,
                Currency = "SAR",
                ExchangeRate = 1.0m,
                IsRateOverride = false,
                TotalDebit = 0,
                TotalCredit = 0,
                ReferenceNumber = $"SALE-{asset.AssetCode}",
                FiscalYear = fiscalYear,
                FiscalPeriod = fiscalPeriod,
                Create_by = userId,
                Create_time = DateTime.Now
            };

            db.Insertable(journal).ExecuteCommand();

            // Create journal entries
            var entries = new List<GLJournalEntry>();
            int lineNumber = 1;

            // Dr Cash (sale price)
            entries.Add(new GLJournalEntry
            {
                Id = Guid.NewGuid(),
                JournalId = journal.Id,
                AccountId = cashAccountId,
                LineNumber = lineNumber++,
                Debit = Math.Round(salePrice, 4),
                Credit = 0,
                BaseDebit = Math.Round(salePrice, 4),
                BaseCredit = 0,
                Currency = "SAR",
                ExchangeRate = 1,
                DescriptionAr = "النقد المستلم من بيع الأصل",
                DescriptionEn = "Cash received from asset sale",
                Create_by = userId,
                Create_time = DateTime.Now
            });

            // Dr Accumulated Depreciation (to remove)
            if (accDepAccountId.HasValue && asset.AccumulatedDepreciation > 0)
            {
                entries.Add(new GLJournalEntry
                {
                    Id = Guid.NewGuid(),
                    JournalId = journal.Id,
                    AccountId = accDepAccountId.Value,
                    LineNumber = lineNumber++,
                    Debit = Math.Round(asset.AccumulatedDepreciation, 4),
                    Credit = 0,
                    BaseDebit = Math.Round(asset.AccumulatedDepreciation, 4),
                    BaseCredit = 0,
                    Currency = "SAR",
                    ExchangeRate = 1,
                    DescriptionAr = "إزالة مجمع الإهلاك",
                    DescriptionEn = "Remove accumulated depreciation",
                    Create_by = userId,
                    Create_time = DateTime.Now
                });
            }

            // Dr Loss (if loss) or Cr Gain (if gain)
            if (gainLoss < 0)
            {
                var lossAmount = Math.Round(Math.Abs(gainLoss), 4);
                entries.Add(new GLJournalEntry
                {
                    Id = Guid.NewGuid(),
                    JournalId = journal.Id,
                    AccountId = gainLossAccountId.Value,
                    LineNumber = lineNumber++,
                    Debit = lossAmount,
                    Credit = 0,
                    BaseDebit = lossAmount,
                    BaseCredit = 0,
                    Currency = "SAR",
                    ExchangeRate = 1,
                    DescriptionAr = "خسارة بيع أصل",
                    DescriptionEn = "Loss on asset sale",
                    Create_by = userId,
                    Create_time = DateTime.Now
                });
            }
            else if (gainLoss > 0)
            {
                var gainAmount = Math.Round(gainLoss, 4);
                entries.Add(new GLJournalEntry
                {
                    Id = Guid.NewGuid(),
                    JournalId = journal.Id,
                    AccountId = gainLossAccountId.Value,
                    LineNumber = lineNumber++,
                    Debit = 0,
                    Credit = gainAmount,
                    BaseDebit = 0,
                    BaseCredit = gainAmount,
                    Currency = "SAR",
                    ExchangeRate = 1,
                    DescriptionAr = "ربح بيع أصل",
                    DescriptionEn = "Gain on asset sale",
                    Create_by = userId,
                    Create_time = DateTime.Now
                });
            }

            // Cr Asset Account (original cost)
            var purchaseCostRounded = Math.Round(asset.PurchaseCost, 4);
            entries.Add(new GLJournalEntry
            {
                Id = Guid.NewGuid(),
                JournalId = journal.Id,
                AccountId = assetAccountId.Value,
                LineNumber = lineNumber,
                Debit = 0,
                Credit = purchaseCostRounded,
                BaseDebit = 0,
                BaseCredit = purchaseCostRounded,
                Currency = "SAR",
                ExchangeRate = 1,
                DescriptionAr = "إزالة تكلفة الأصل",
                DescriptionEn = "Remove asset cost",
                Create_by = userId,
                Create_time = DateTime.Now
            });

            // Insert all entries
            db.Insertable(entries).ExecuteCommand();

            // Update journal totals
            journal.TotalDebit = Math.Round(entries.Sum(e => e.Debit), 4);
            journal.TotalCredit = Math.Round(entries.Sum(e => e.Credit), 4);
            db.Updateable(journal).UpdateColumns(j => new { j.TotalDebit, j.TotalCredit }).ExecuteCommand();

            return journal;
        }

        /// <summary>
        /// Dispose asset - استبعاد الأصل
        /// Uses SqlSugar transaction for atomicity, creates GL Journal
        /// </summary>
        public AssetTransaction DisposeAsset(AssetDisposalDto dto, string userId)
        {
            var asset = _repository.GetById(dto.AssetId);
            if (asset == null)
            {
                throw new AppException(ExceptionCode.FINANCE_ASSET_NOT_FOUND)
                    .WithParameter("AssetId", dto.AssetId);
            }

            // Run all validations BEFORE starting transaction
            _validationService.ValidateAssetDisposal(dto, asset);

            // Get fiscal period for the disposal date BEFORE transaction
            var (fiscalYear, fiscalPeriod) = _validationService.GetFiscalPeriodForDate(dto.DisposalDate, null);

            // Book value = PurchaseCost - AccumulatedDepreciation
            decimal bookValue = asset.PurchaseCost - asset.AccumulatedDepreciation;
            // Loss on disposal equals the book value
            decimal gainLoss = -bookValue;

            Guid? userGuid = null;
            if (Guid.TryParse(userId, out Guid parsedGuid))
            {
                userGuid = parsedGuid;
            }

            // Prepare the transaction record
            var assetTransaction = new AssetTransaction
            {
                Id = Guid.NewGuid(),
                CompanyID = asset.CompanyID,
                AssetId = asset.Id,
                TransactionType = AssetTransactionType.Disposal,
                TransactionDate = dto.DisposalDate,
                Amount = bookValue,
                PreviousBookValue = bookValue,
                NewBookValue = 0,
                GainLossAmount = gainLoss,
                Notes = dto.Notes ?? dto.DisposalReason,
                ReferenceNumber = dto.ReferenceNumber,
                FiscalYear = fiscalYear,
                FiscalPeriod = fiscalPeriod,
                Create_by = userGuid?.ToString() ?? userId,
                Create_time = DateTime.Now
            };

            // Use SqlSugar's native transaction - ALL operations in one transaction
            var db = _repository.Context;
            try
            {
                db.Ado.BeginTran();

                // 1. Insert asset transaction record
                db.Insertable(assetTransaction).ExecuteCommand();

                // 2. Create GL Journal for disposal - use the SAME db context
                var journal = CreateDisposalJournalInternal(db, asset, dto.DisposalDate, fiscalYear, fiscalPeriod, userId);

                if (journal == null)
                {
                    throw new AppException(ExceptionCode.FINANCE_ASSET_GL_JOURNAL_FAILED);
                }

                // 3. Update transaction with journal ID
                assetTransaction.GLJournalId = journal.Id;
                db.Updateable(assetTransaction).UpdateColumns(t => new { t.GLJournalId }).ExecuteCommand();

                // 4. Update asset status
                asset.Status = AssetStatus.Disposed;
                asset.CurrentBookValue = 0;
                asset.Update_time = DateTime.Now;
                asset.Update_by = userGuid?.ToString() ?? userId;
                db.Updateable(asset).ExecuteCommand();

                // Commit all changes
                db.Ado.CommitTran();

                return assetTransaction;
            }
            catch (Exception ex)
            {
                db.Ado.RollbackTran();
                throw new AppException(ExceptionCode.FINANCE_ASSET_DISPOSAL_FAILED, ex.Message);
            }
        }

        /// <summary>
        /// Create disposal journal using the same database context (for transaction support)
        /// </summary>
        private GLJournal CreateDisposalJournalInternal(ISqlSugarClient db, FixedAsset asset,
            DateTime disposalDate, int fiscalYear, int fiscalPeriod, string userId)
        {
            var category = _categoryRepository.GetById(asset.CategoryId);

            var assetAccountId = asset.GLAssetAccountId ?? category?.DefaultAssetAccountId;
            var accDepAccountId = asset.GLAccumulatedDepreciationAccountId ?? category?.DefaultAccumulatedDepreciationAccountId;
            var gainLossAccountId = category?.DefaultGainLossAccountId;

            if (!assetAccountId.HasValue)
            {
                throw new AppException(ExceptionCode.FINANCE_ASSET_ACCOUNT_NOT_CONFIGURED);
            }
            if (!gainLossAccountId.HasValue)
            {
                throw new AppException(ExceptionCode.FINANCE_ASSET_GAINLOSS_ACCOUNT_NOT_CONFIGURED);
            }

            // Book Value = PurchaseCost - AccumulatedDepreciation (this is the loss)
            var bookValue = asset.PurchaseCost - asset.AccumulatedDepreciation;

            // Generate journal number
            var year = DateTime.Now.Year;
            var prefix = $"JV-{year}-";
            var lastJournal = db.Queryable<GLJournal>()
                .Where(x => x.JournalNumber.StartsWith(prefix))
                .OrderByDescending(x => x.JournalNumber)
                .First();

            string journalNumber;
            if (lastJournal == null)
            {
                journalNumber = $"{prefix}00001";
            }
            else
            {
                var lastNumber = lastJournal.JournalNumber.Replace(prefix, "");
                if (int.TryParse(lastNumber, out int num))
                {
                    journalNumber = $"{prefix}{(num + 1):D5}";
                }
                else
                {
                    journalNumber = $"{prefix}00001";
                }
            }

            // Create journal
            var journal = new GLJournal
            {
                Id = Guid.NewGuid(),
                JournalNumber = journalNumber,
                JournalDate = disposalDate,
                DescriptionAr = $"استبعاد أصل: {asset.NameAr ?? asset.AssetCode}",
                DescriptionEn = $"Disposal of asset: {asset.NameEn ?? asset.AssetCode}",
                Status = JournalStatus.Draft,
                Source = "FixedAssets",
                SourceDocumentType = "AssetDisposal",
                SourceDocumentId = asset.Id,
                Currency = "SAR",
                ExchangeRate = 1.0m,
                IsRateOverride = false,
                TotalDebit = 0,
                TotalCredit = 0,
                ReferenceNumber = $"DISP-{asset.AssetCode}",
                FiscalYear = fiscalYear,
                FiscalPeriod = fiscalPeriod,
                Create_by = userId,
                Create_time = DateTime.Now
            };

            db.Insertable(journal).ExecuteCommand();

            // Create journal entries
            var entries = new List<GLJournalEntry>();
            int lineNumber = 1;

            // Dr Accumulated Depreciation (to remove)
            if (accDepAccountId.HasValue && asset.AccumulatedDepreciation > 0)
            {
                var accDepAmount = Math.Round(asset.AccumulatedDepreciation, 4);
                entries.Add(new GLJournalEntry
                {
                    Id = Guid.NewGuid(),
                    JournalId = journal.Id,
                    AccountId = accDepAccountId.Value,
                    LineNumber = lineNumber++,
                    Debit = accDepAmount,
                    Credit = 0,
                    BaseDebit = accDepAmount,
                    BaseCredit = 0,
                    Currency = "SAR",
                    ExchangeRate = 1,
                    DescriptionAr = "إزالة مجمع الإهلاك",
                    DescriptionEn = "Remove accumulated depreciation",
                    Create_by = userId,
                    Create_time = DateTime.Now
                });
            }

            // Dr Loss on Disposal (book value)
            if (bookValue > 0)
            {
                var bookValueRounded = Math.Round(bookValue, 4);
                entries.Add(new GLJournalEntry
                {
                    Id = Guid.NewGuid(),
                    JournalId = journal.Id,
                    AccountId = gainLossAccountId.Value,
                    LineNumber = lineNumber++,
                    Debit = bookValueRounded,
                    Credit = 0,
                    BaseDebit = bookValueRounded,
                    BaseCredit = 0,
                    Currency = "SAR",
                    ExchangeRate = 1,
                    DescriptionAr = "خسارة استبعاد أصل",
                    DescriptionEn = "Loss on asset disposal",
                    Create_by = userId,
                    Create_time = DateTime.Now
                });
            }

            // Cr Asset Account (original cost)
            var purchaseCostRounded = Math.Round(asset.PurchaseCost, 4);
            entries.Add(new GLJournalEntry
            {
                Id = Guid.NewGuid(),
                JournalId = journal.Id,
                AccountId = assetAccountId.Value,
                LineNumber = lineNumber,
                Debit = 0,
                Credit = purchaseCostRounded,
                BaseDebit = 0,
                BaseCredit = purchaseCostRounded,
                Currency = "SAR",
                ExchangeRate = 1,
                DescriptionAr = "إزالة تكلفة الأصل",
                DescriptionEn = "Remove asset cost",
                Create_by = userId,
                Create_time = DateTime.Now
            });

            // Insert all entries
            db.Insertable(entries).ExecuteCommand();

            // Update journal totals
            journal.TotalDebit = Math.Round(entries.Sum(e => e.Debit), 4);
            journal.TotalCredit = Math.Round(entries.Sum(e => e.Credit), 4);
            db.Updateable(journal).UpdateColumns(j => new { j.TotalDebit, j.TotalCredit }).ExecuteCommand();

            return journal;
        }

        /// <summary>
        /// Revalue asset - إعادة تقييم الأصل
        /// Uses SqlSugar transaction for atomicity, creates GL Journal
        /// </summary>
        public AssetTransaction RevalueAsset(AssetRevaluationDto dto, string userId)
        {
            var asset = _repository.GetById(dto.AssetId);
            if (asset == null)
            {
                throw new AppException(ExceptionCode.FINANCE_ASSET_NOT_FOUND)
                    .WithParameter("AssetId", dto.AssetId);
            }

            // Run all validations BEFORE starting transaction
            _validationService.ValidateAssetRevaluation(dto, asset);

            // Get fiscal period for the revaluation date BEFORE transaction
            var (fiscalYear, fiscalPeriod) = _validationService.GetFiscalPeriodForDate(dto.RevaluationDate, null);

            // Calculate adjustment
            decimal previousBookValue = asset.CurrentBookValue;
            decimal adjustmentAmount = dto.NewValue - previousBookValue;

            Guid? userGuid = null;
            if (Guid.TryParse(userId, out Guid parsedGuid))
            {
                userGuid = parsedGuid;
            }

            // Prepare the transaction record
            var assetTransaction = new AssetTransaction
            {
                Id = Guid.NewGuid(),
                CompanyID = asset.CompanyID,
                AssetId = asset.Id,
                TransactionType = AssetTransactionType.Revaluation,
                TransactionDate = dto.RevaluationDate,
                Amount = adjustmentAmount,
                PreviousBookValue = previousBookValue,
                NewBookValue = dto.NewValue,
                GainLossAmount = null,
                Notes = dto.Notes ?? dto.Reason,
                ReferenceNumber = dto.ReferenceNumber,
                FiscalYear = fiscalYear,
                FiscalPeriod = fiscalPeriod,
                Create_by = userGuid?.ToString() ?? userId,
                Create_time = DateTime.Now
            };

            // Use SqlSugar's native transaction - ALL operations in one transaction
            var db = _repository.Context;
            try
            {
                db.Ado.BeginTran();

                // 1. Insert asset transaction record
                db.Insertable(assetTransaction).ExecuteCommand();

                // 2. Create GL Journal for revaluation - use the SAME db context
                var journal = CreateRevaluationJournalInternal(db, asset, previousBookValue, dto.NewValue,
                    dto.RevaluationDate, fiscalYear, fiscalPeriod, userId);

                if (journal == null)
                {
                    throw new AppException(ExceptionCode.FINANCE_ASSET_GL_JOURNAL_FAILED);
                }

                // 3. Update transaction with journal ID
                assetTransaction.GLJournalId = journal.Id;
                db.Updateable(assetTransaction).UpdateColumns(t => new { t.GLJournalId }).ExecuteCommand();

                // 4. Update asset book value
                asset.CurrentBookValue = dto.NewValue;
                asset.Update_time = DateTime.Now;
                asset.Update_by = userGuid?.ToString() ?? userId;
                db.Updateable(asset).ExecuteCommand();

                // Commit all changes
                db.Ado.CommitTran();

                return assetTransaction;
            }
            catch (Exception ex)
            {
                db.Ado.RollbackTran();
                throw new AppException(ExceptionCode.FINANCE_ASSET_REVALUATION_FAILED, ex.Message);
            }
        }

        /// <summary>
        /// Create revaluation journal using the same database context (for transaction support)
        /// </summary>
        private GLJournal CreateRevaluationJournalInternal(ISqlSugarClient db, FixedAsset asset,
            decimal previousValue, decimal newValue, DateTime revaluationDate,
            int fiscalYear, int fiscalPeriod, string userId)
        {
            var category = _categoryRepository.GetById(asset.CategoryId);

            var assetAccountId = asset.GLAssetAccountId ?? category?.DefaultAssetAccountId;
            var gainLossAccountId = category?.DefaultGainLossAccountId;

            if (!assetAccountId.HasValue)
            {
                throw new AppException(ExceptionCode.FINANCE_ASSET_ACCOUNT_NOT_CONFIGURED);
            }
            if (!gainLossAccountId.HasValue)
            {
                throw new AppException(ExceptionCode.FINANCE_ASSET_REVALUATION_ACCOUNT_NOT_CONFIGURED);
            }

            var adjustmentAmount = newValue - previousValue;

            // Generate journal number
            var year = DateTime.Now.Year;
            var prefix = $"JV-{year}-";
            var lastJournal = db.Queryable<GLJournal>()
                .Where(x => x.JournalNumber.StartsWith(prefix))
                .OrderByDescending(x => x.JournalNumber)
                .First();

            string journalNumber;
            if (lastJournal == null)
            {
                journalNumber = $"{prefix}00001";
            }
            else
            {
                var lastNumber = lastJournal.JournalNumber.Replace(prefix, "");
                if (int.TryParse(lastNumber, out int num))
                {
                    journalNumber = $"{prefix}{(num + 1):D5}";
                }
                else
                {
                    journalNumber = $"{prefix}00001";
                }
            }

            // Create journal
            var journal = new GLJournal
            {
                Id = Guid.NewGuid(),
                JournalNumber = journalNumber,
                JournalDate = revaluationDate,
                DescriptionAr = $"إعادة تقييم أصل: {asset.NameAr ?? asset.AssetCode}",
                DescriptionEn = $"Revaluation of asset: {asset.NameEn ?? asset.AssetCode}",
                Status = JournalStatus.Draft,
                Source = "FixedAssets",
                SourceDocumentType = "AssetRevaluation",
                SourceDocumentId = asset.Id,
                Currency = "SAR",
                ExchangeRate = 1.0m,
                IsRateOverride = false,
                TotalDebit = 0,
                TotalCredit = 0,
                ReferenceNumber = $"REVAL-{asset.AssetCode}",
                FiscalYear = fiscalYear,
                FiscalPeriod = fiscalPeriod,
                Create_by = userId,
                Create_time = DateTime.Now
            };

            db.Insertable(journal).ExecuteCommand();

            // Create journal entries
            var entries = new List<GLJournalEntry>();

            if (adjustmentAmount > 0)
            {
                var adjustmentRounded = Math.Round(adjustmentAmount, 4);
                // Increase in value: Dr Asset, Cr Revaluation Reserve
                entries.Add(new GLJournalEntry
                {
                    Id = Guid.NewGuid(),
                    JournalId = journal.Id,
                    AccountId = assetAccountId.Value,
                    LineNumber = 1,
                    Debit = adjustmentRounded,
                    Credit = 0,
                    BaseDebit = adjustmentRounded,
                    BaseCredit = 0,
                    Currency = "SAR",
                    ExchangeRate = 1,
                    DescriptionAr = "زيادة قيمة الأصل",
                    DescriptionEn = "Increase in asset value",
                    Create_by = userId,
                    Create_time = DateTime.Now
                });
                entries.Add(new GLJournalEntry
                {
                    Id = Guid.NewGuid(),
                    JournalId = journal.Id,
                    AccountId = gainLossAccountId.Value,
                    LineNumber = 2,
                    Debit = 0,
                    Credit = adjustmentRounded,
                    BaseDebit = 0,
                    BaseCredit = adjustmentRounded,
                    Currency = "SAR",
                    ExchangeRate = 1,
                    DescriptionAr = "احتياطي إعادة التقييم",
                    DescriptionEn = "Revaluation reserve",
                    Create_by = userId,
                    Create_time = DateTime.Now
                });
            }
            else if (adjustmentAmount < 0)
            {
                var absAdjustment = Math.Round(Math.Abs(adjustmentAmount), 4);
                // Decrease in value: Dr Revaluation Reserve, Cr Asset
                entries.Add(new GLJournalEntry
                {
                    Id = Guid.NewGuid(),
                    JournalId = journal.Id,
                    AccountId = gainLossAccountId.Value,
                    LineNumber = 1,
                    Debit = absAdjustment,
                    Credit = 0,
                    BaseDebit = absAdjustment,
                    BaseCredit = 0,
                    Currency = "SAR",
                    ExchangeRate = 1,
                    DescriptionAr = "انخفاض قيمة الأصل",
                    DescriptionEn = "Decrease in asset value",
                    Create_by = userId,
                    Create_time = DateTime.Now
                });
                entries.Add(new GLJournalEntry
                {
                    Id = Guid.NewGuid(),
                    JournalId = journal.Id,
                    AccountId = assetAccountId.Value,
                    LineNumber = 2,
                    Debit = 0,
                    Credit = absAdjustment,
                    BaseDebit = 0,
                    BaseCredit = absAdjustment,
                    Currency = "SAR",
                    ExchangeRate = 1,
                    DescriptionAr = "تخفيض الأصل",
                    DescriptionEn = "Asset reduction",
                    Create_by = userId,
                    Create_time = DateTime.Now
                });
            }

            // Insert all entries
            if (entries.Count > 0)
            {
                db.Insertable(entries).ExecuteCommand();
            }

            // Update journal totals
            journal.TotalDebit = Math.Round(entries.Sum(e => e.Debit), 4);
            journal.TotalCredit = Math.Round(entries.Sum(e => e.Credit), 4);
            db.Updateable(journal).UpdateColumns(j => new { j.TotalDebit, j.TotalCredit }).ExecuteCommand();

            return journal;
        }

        /// <summary>
        /// Record purchase transaction using same db context - تسجيل معاملة شراء
        /// </summary>
        private AssetTransaction RecordPurchaseTransactionInternal(ISqlSugarClient db, FixedAsset asset, string userId, int fiscalYear, int fiscalPeriod)
        {
            Guid? userGuid = null;
            if (Guid.TryParse(userId, out Guid parsedGuid))
            {
                userGuid = parsedGuid;
            }

            var transaction = new AssetTransaction
            {
                Id = Guid.NewGuid(),
                CompanyID = asset.CompanyID,
                AssetId = asset.Id,
                TransactionType = AssetTransactionType.Purchase,
                TransactionDate = asset.PurchaseDate,
                Amount = asset.PurchaseCost,
                PreviousBookValue = 0,
                NewBookValue = asset.CurrentBookValue,
                GainLossAmount = null,
                Notes = $"Initial purchase of asset {asset.AssetCode}",
                FiscalYear = fiscalYear,
                FiscalPeriod = fiscalPeriod,
                Create_by = userGuid?.ToString() ?? userId,
                Create_time = DateTime.Now
            };

            db.Insertable(transaction).ExecuteCommand();
            return transaction;
        }

        /// <summary>
        /// Get next asset code - الحصول على رمز الأصل التالي
        /// Format: FA-YYYY-NNNN (e.g., FA-2025-0001)
        /// </summary>
        public string GetNextAssetCode(long? companyId = null)
        {
            var prefix = "FA";
            var currentYear = DateTime.Now.Year;
            var yearPrefix = $"{prefix}-{currentYear}-";

            // Find the max existing code for this year
            var predicate = Expressionable.Create<FixedAsset>();
            predicate = predicate.And(x => x.AssetCode.StartsWith(yearPrefix));

            if (companyId.HasValue)
            {
                predicate = predicate.And(x => x.CompanyID == companyId.Value);
            }

            var existingCodes = _repository.GetList(predicate.ToExpression())
                .Select(x => x.AssetCode)
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
