using System;
using System.Collections.Generic;
using Infrastructure;
using Infrastructure.Attribute;
using Infrastructure.Exceptions;
using ZR.Model.Finance.Assets;
using ZR.Model.Finance.Assets.Enums;
using ZR.Model.Finance.GL;
using ZR.Model.Finance.GL.Dto;
using ZR.Model.Finance.Enums;
using ZR.Repository.Finance.Assets;
using ZR.Service.IService.Finance.GL;
using ZR.Service.Finance.Configuration;

namespace ZR.Service.Finance.Assets
{
    /// <summary>
    /// Asset GL Integration Service - خدمة ربط الأصول بالأستاذ العام
    /// Handles all GL Journal creation for asset operations
    /// </summary>
    public interface IAssetGLIntegrationService
    {
        /// <summary>
        /// Create GL Journal for asset purchase
        /// Dr Fixed Asset Account
        /// Cr Cash/Accounts Payable
        /// </summary>
        GLJournal CreatePurchaseJournal(FixedAsset asset, Guid cashAccountId, string userId, long? companyId = null);

        /// <summary>
        /// Create GL Journal for depreciation
        /// Dr Depreciation Expense
        /// Cr Accumulated Depreciation
        /// </summary>
        GLJournal CreateDepreciationJournal(FixedAsset asset, decimal depreciationAmount, DateTime depreciationDate,
            int fiscalYear, int fiscalPeriod, string userId, long? companyId = null);

        /// <summary>
        /// Create GL Journal for asset sale
        /// Dr Cash (sale price)
        /// Dr Accumulated Depreciation (to remove)
        /// Dr/Cr Gain/Loss on Sale
        /// Cr Fixed Asset Account (original cost)
        /// </summary>
        GLJournal CreateSaleJournal(FixedAsset asset, decimal salePrice, DateTime saleDate,
            Guid cashAccountId, int fiscalYear, int fiscalPeriod, string userId, long? companyId = null);

        /// <summary>
        /// Create GL Journal for asset disposal
        /// Dr Accumulated Depreciation (to remove)
        /// Dr Loss on Disposal (book value)
        /// Cr Fixed Asset Account (original cost)
        /// </summary>
        GLJournal CreateDisposalJournal(FixedAsset asset, DateTime disposalDate,
            int fiscalYear, int fiscalPeriod, string userId, long? companyId = null);

        /// <summary>
        /// Create GL Journal for asset revaluation
        /// For Increase: Dr Fixed Asset, Cr Revaluation Reserve
        /// For Decrease: Dr Revaluation Reserve, Cr Fixed Asset
        /// </summary>
        GLJournal CreateRevaluationJournal(FixedAsset asset, decimal previousValue, decimal newValue,
            DateTime revaluationDate, int fiscalYear, int fiscalPeriod, string userId, long? companyId = null);

        /// <summary>
        /// Preview journal entries without posting (for UI)
        /// </summary>
        JournalPreviewDto PreviewDepreciationJournal(FixedAsset asset, decimal depreciationAmount, DateTime depreciationDate);
        JournalPreviewDto PreviewSaleJournal(FixedAsset asset, decimal salePrice);
        JournalPreviewDto PreviewDisposalJournal(FixedAsset asset);
        JournalPreviewDto PreviewRevaluationJournal(FixedAsset asset, decimal newValue);
    }

    /// <summary>
    /// Journal Preview DTO for frontend display
    /// </summary>
    public class JournalPreviewDto
    {
        public string Description { get; set; }
        public DateTime JournalDate { get; set; }
        public List<JournalEntryPreviewDto> Entries { get; set; } = new List<JournalEntryPreviewDto>();
        public decimal TotalDebit => Entries?.Sum(e => e.Debit) ?? 0;
        public decimal TotalCredit => Entries?.Sum(e => e.Credit) ?? 0;
        public bool IsBalanced => TotalDebit == TotalCredit;
    }

    public class JournalEntryPreviewDto
    {
        public string AccountCode { get; set; }
        public string AccountName { get; set; }
        public Guid AccountId { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public string Description { get; set; }
    }

    [AppService(ServiceType = typeof(IAssetGLIntegrationService), ServiceLifetime = LifeTime.Transient)]
    public class AssetGLIntegrationService : IAssetGLIntegrationService
    {
        private readonly IGLJournalService _journalService;
        private readonly AssetCategoryRepository _categoryRepository;
        private readonly IGLAccountService _accountService;
        private readonly IFiscalYearHelper _fiscalYearHelper;

        public AssetGLIntegrationService(
            IGLJournalService journalService,
            AssetCategoryRepository categoryRepository,
            IGLAccountService accountService,
            IFiscalYearHelper fiscalYearHelper)
        {
            _journalService = journalService;
            _categoryRepository = categoryRepository;
            _accountService = accountService;
            _fiscalYearHelper = fiscalYearHelper;
        }

        public GLJournal CreatePurchaseJournal(FixedAsset asset, Guid cashAccountId, string userId, long? companyId = null)
        {
            var category = _categoryRepository.GetById(asset.CategoryId);
            var assetAccountId = asset.GLAssetAccountId ?? category?.DefaultAssetAccountId;

            if (!assetAccountId.HasValue)
            {
                throw new AppException(ExceptionCode.FINANCE_ASSET_ACCOUNT_NOT_CONFIGURED);
            }

            var journalDto = new CreateJournalDto
            {
                JournalDate = asset.PurchaseDate,
                DescriptionAr = $"شراء أصل ثابت: {asset.NameAr ?? asset.AssetCode}",
                DescriptionEn = $"Purchase of fixed asset: {asset.NameEn ?? asset.AssetCode}",
                Source = "FixedAssets",
                SourceDocumentType = "AssetPurchase",
                SourceDocumentId = asset.Id,
                Currency = "SAR",
                ReferenceNumber = asset.AssetCode,
                FiscalYear = _fiscalYearHelper.GetFiscalYearFromDate(asset.PurchaseDate, companyId),
                FiscalPeriod = _fiscalYearHelper.GetFiscalPeriodFromDate(asset.PurchaseDate, companyId),
                Entries = new List<CreateJournalEntryDto>
                {
                    new CreateJournalEntryDto
                    {
                        AccountId = assetAccountId.Value,
                        Debit = asset.PurchaseCost,
                        Credit = 0,
                        LineNumber = 1,
                        DescriptionAr = $"شراء أصل: {asset.NameAr}",
                        DescriptionEn = $"Asset purchase: {asset.NameEn}"
                    },
                    new CreateJournalEntryDto
                    {
                        AccountId = cashAccountId,
                        Debit = 0,
                        Credit = asset.PurchaseCost,
                        LineNumber = 2,
                        DescriptionAr = "دفع نقدي لشراء أصل",
                        DescriptionEn = "Cash payment for asset purchase"
                    }
                }
            };

            return _journalService.Create(journalDto, userId);
        }

        public GLJournal CreateDepreciationJournal(FixedAsset asset, decimal depreciationAmount, DateTime depreciationDate,
            int fiscalYear, int fiscalPeriod, string userId, long? companyId = null)
        {
            var category = _categoryRepository.GetById(asset.CategoryId);

            var depExpenseAccountId = asset.GLDepreciationExpenseAccountId ?? category?.DefaultDepreciationExpenseAccountId;
            var accDepAccountId = asset.GLAccumulatedDepreciationAccountId ?? category?.DefaultAccumulatedDepreciationAccountId;

            if (!depExpenseAccountId.HasValue)
            {
                throw new AppException(ExceptionCode.FINANCE_ASSET_DEPRECIATION_ACCOUNT_NOT_CONFIGURED);
            }
            if (!accDepAccountId.HasValue)
            {
                throw new AppException(ExceptionCode.FINANCE_ASSET_ACCUMULATED_DEPRECIATION_ACCOUNT_NOT_CONFIGURED);
            }

            var journalDto = new CreateJournalDto
            {
                JournalDate = depreciationDate,
                DescriptionAr = $"إهلاك أصل: {asset.NameAr ?? asset.AssetCode} - فترة {fiscalYear}/{fiscalPeriod}",
                DescriptionEn = $"Depreciation of asset: {asset.NameEn ?? asset.AssetCode} - Period {fiscalYear}/{fiscalPeriod}",
                Source = "FixedAssets",
                SourceDocumentType = "Depreciation",
                SourceDocumentId = asset.Id,
                Currency = "SAR",
                ReferenceNumber = $"DEP-{asset.AssetCode}-{fiscalYear}-{fiscalPeriod}",
                FiscalYear = fiscalYear,
                FiscalPeriod = fiscalPeriod,
                Entries = new List<CreateJournalEntryDto>
                {
                    new CreateJournalEntryDto
                    {
                        AccountId = depExpenseAccountId.Value,
                        Debit = depreciationAmount,
                        Credit = 0,
                        LineNumber = 1,
                        DescriptionAr = $"مصروف إهلاك: {asset.NameAr}",
                        DescriptionEn = $"Depreciation expense: {asset.NameEn}"
                    },
                    new CreateJournalEntryDto
                    {
                        AccountId = accDepAccountId.Value,
                        Debit = 0,
                        Credit = depreciationAmount,
                        LineNumber = 2,
                        DescriptionAr = $"مجمع إهلاك: {asset.NameAr}",
                        DescriptionEn = $"Accumulated depreciation: {asset.NameEn}"
                    }
                }
            };

            return _journalService.Create(journalDto, userId);
        }

        public GLJournal CreateSaleJournal(FixedAsset asset, decimal salePrice, DateTime saleDate,
            Guid cashAccountId, int fiscalYear, int fiscalPeriod, string userId, long? companyId = null)
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
            // Book Value = PurchaseCost - AccumulatedDepreciation
            // Gain/Loss = SalePrice - BookValue
            var bookValue = asset.PurchaseCost - asset.AccumulatedDepreciation;
            var gainLoss = salePrice - bookValue;

            var entries = new List<CreateJournalEntryDto>();
            int lineNumber = 1;

            // Dr Cash (sale price)
            entries.Add(new CreateJournalEntryDto
            {
                AccountId = cashAccountId,
                Debit = salePrice,
                Credit = 0,
                LineNumber = lineNumber++,
                DescriptionAr = "النقد المستلم من بيع الأصل",
                DescriptionEn = "Cash received from asset sale"
            });

            // Dr Accumulated Depreciation (to remove)
            if (accDepAccountId.HasValue && asset.AccumulatedDepreciation > 0)
            {
                entries.Add(new CreateJournalEntryDto
                {
                    AccountId = accDepAccountId.Value,
                    Debit = asset.AccumulatedDepreciation,
                    Credit = 0,
                    LineNumber = lineNumber++,
                    DescriptionAr = "إزالة مجمع الإهلاك",
                    DescriptionEn = "Remove accumulated depreciation"
                });
            }

            // Dr Loss (if loss) or Cr Gain (if gain)
            if (gainLoss < 0)
            {
                // Loss on sale
                entries.Add(new CreateJournalEntryDto
                {
                    AccountId = gainLossAccountId.Value,
                    Debit = Math.Abs(gainLoss),
                    Credit = 0,
                    LineNumber = lineNumber++,
                    DescriptionAr = "خسارة بيع أصل",
                    DescriptionEn = "Loss on asset sale"
                });
            }
            else if (gainLoss > 0)
            {
                // Gain on sale
                entries.Add(new CreateJournalEntryDto
                {
                    AccountId = gainLossAccountId.Value,
                    Debit = 0,
                    Credit = gainLoss,
                    LineNumber = lineNumber++,
                    DescriptionAr = "ربح بيع أصل",
                    DescriptionEn = "Gain on asset sale"
                });
            }

            // Cr Asset Account (original cost)
            entries.Add(new CreateJournalEntryDto
            {
                AccountId = assetAccountId.Value,
                Debit = 0,
                Credit = asset.PurchaseCost,
                LineNumber = lineNumber,
                DescriptionAr = "إزالة تكلفة الأصل",
                DescriptionEn = "Remove asset cost"
            });

            var journalDto = new CreateJournalDto
            {
                JournalDate = saleDate,
                DescriptionAr = $"بيع أصل: {asset.NameAr ?? asset.AssetCode}",
                DescriptionEn = $"Sale of asset: {asset.NameEn ?? asset.AssetCode}",
                Source = "FixedAssets",
                SourceDocumentType = "AssetSale",
                SourceDocumentId = asset.Id,
                Currency = "SAR",
                ReferenceNumber = $"SALE-{asset.AssetCode}",
                FiscalYear = fiscalYear,
                FiscalPeriod = fiscalPeriod,
                Entries = entries
            };

            return _journalService.Create(journalDto, userId);
        }

        public GLJournal CreateDisposalJournal(FixedAsset asset, DateTime disposalDate,
            int fiscalYear, int fiscalPeriod, string userId, long? companyId = null)
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

            var entries = new List<CreateJournalEntryDto>();
            int lineNumber = 1;

            // Dr Accumulated Depreciation (to remove)
            if (accDepAccountId.HasValue && asset.AccumulatedDepreciation > 0)
            {
                entries.Add(new CreateJournalEntryDto
                {
                    AccountId = accDepAccountId.Value,
                    Debit = asset.AccumulatedDepreciation,
                    Credit = 0,
                    LineNumber = lineNumber++,
                    DescriptionAr = "إزالة مجمع الإهلاك",
                    DescriptionEn = "Remove accumulated depreciation"
                });
            }

            // Dr Loss on Disposal (book value)
            if (bookValue > 0)
            {
                entries.Add(new CreateJournalEntryDto
                {
                    AccountId = gainLossAccountId.Value,
                    Debit = bookValue,
                    Credit = 0,
                    LineNumber = lineNumber++,
                    DescriptionAr = "خسارة استبعاد أصل",
                    DescriptionEn = "Loss on asset disposal"
                });
            }

            // Cr Asset Account (original cost)
            entries.Add(new CreateJournalEntryDto
            {
                AccountId = assetAccountId.Value,
                Debit = 0,
                Credit = asset.PurchaseCost,
                LineNumber = lineNumber,
                DescriptionAr = "إزالة تكلفة الأصل",
                DescriptionEn = "Remove asset cost"
            });

            var journalDto = new CreateJournalDto
            {
                JournalDate = disposalDate,
                DescriptionAr = $"استبعاد أصل: {asset.NameAr ?? asset.AssetCode}",
                DescriptionEn = $"Disposal of asset: {asset.NameEn ?? asset.AssetCode}",
                Source = "FixedAssets",
                SourceDocumentType = "AssetDisposal",
                SourceDocumentId = asset.Id,
                Currency = "SAR",
                ReferenceNumber = $"DISP-{asset.AssetCode}",
                FiscalYear = fiscalYear,
                FiscalPeriod = fiscalPeriod,
                Entries = entries
            };

            return _journalService.Create(journalDto, userId);
        }

        public GLJournal CreateRevaluationJournal(FixedAsset asset, decimal previousValue, decimal newValue,
            DateTime revaluationDate, int fiscalYear, int fiscalPeriod, string userId, long? companyId = null)
        {
            var category = _categoryRepository.GetById(asset.CategoryId);

            var assetAccountId = asset.GLAssetAccountId ?? category?.DefaultAssetAccountId;
            var gainLossAccountId = category?.DefaultGainLossAccountId; // Used for revaluation reserve

            if (!assetAccountId.HasValue)
            {
                throw new AppException(ExceptionCode.FINANCE_ASSET_ACCOUNT_NOT_CONFIGURED);
            }
            if (!gainLossAccountId.HasValue)
            {
                throw new AppException(ExceptionCode.FINANCE_ASSET_REVALUATION_ACCOUNT_NOT_CONFIGURED);
            }

            var adjustmentAmount = newValue - previousValue;
            var entries = new List<CreateJournalEntryDto>();

            if (adjustmentAmount > 0)
            {
                // Increase in value
                // Dr Fixed Asset Account
                // Cr Revaluation Reserve
                entries.Add(new CreateJournalEntryDto
                {
                    AccountId = assetAccountId.Value,
                    Debit = adjustmentAmount,
                    Credit = 0,
                    LineNumber = 1,
                    DescriptionAr = "زيادة قيمة الأصل",
                    DescriptionEn = "Increase in asset value"
                });
                entries.Add(new CreateJournalEntryDto
                {
                    AccountId = gainLossAccountId.Value,
                    Debit = 0,
                    Credit = adjustmentAmount,
                    LineNumber = 2,
                    DescriptionAr = "احتياطي إعادة التقييم",
                    DescriptionEn = "Revaluation reserve"
                });
            }
            else if (adjustmentAmount < 0)
            {
                // Decrease in value
                // Dr Revaluation Reserve (or P&L)
                // Cr Fixed Asset Account
                entries.Add(new CreateJournalEntryDto
                {
                    AccountId = gainLossAccountId.Value,
                    Debit = Math.Abs(adjustmentAmount),
                    Credit = 0,
                    LineNumber = 1,
                    DescriptionAr = "انخفاض قيمة الأصل",
                    DescriptionEn = "Decrease in asset value"
                });
                entries.Add(new CreateJournalEntryDto
                {
                    AccountId = assetAccountId.Value,
                    Debit = 0,
                    Credit = Math.Abs(adjustmentAmount),
                    LineNumber = 2,
                    DescriptionAr = "تخفيض الأصل",
                    DescriptionEn = "Asset reduction"
                });
            }

            var journalDto = new CreateJournalDto
            {
                JournalDate = revaluationDate,
                DescriptionAr = $"إعادة تقييم أصل: {asset.NameAr ?? asset.AssetCode}",
                DescriptionEn = $"Revaluation of asset: {asset.NameEn ?? asset.AssetCode}",
                Source = "FixedAssets",
                SourceDocumentType = "AssetRevaluation",
                SourceDocumentId = asset.Id,
                Currency = "SAR",
                ReferenceNumber = $"REVAL-{asset.AssetCode}",
                FiscalYear = fiscalYear,
                FiscalPeriod = fiscalPeriod,
                Entries = entries
            };

            return _journalService.Create(journalDto, userId);
        }

        #region Preview Methods

        public JournalPreviewDto PreviewDepreciationJournal(FixedAsset asset, decimal depreciationAmount, DateTime depreciationDate)
        {
            var category = _categoryRepository.GetById(asset.CategoryId);

            return new JournalPreviewDto
            {
                Description = $"Depreciation of asset: {asset.NameEn ?? asset.AssetCode}",
                JournalDate = depreciationDate,
                Entries = new List<JournalEntryPreviewDto>
                {
                    new JournalEntryPreviewDto
                    {
                        AccountId = asset.GLDepreciationExpenseAccountId ?? category?.DefaultDepreciationExpenseAccountId ?? Guid.Empty,
                        AccountName = "Depreciation Expense",
                        Debit = depreciationAmount,
                        Credit = 0,
                        Description = $"Depreciation expense: {asset.NameEn}"
                    },
                    new JournalEntryPreviewDto
                    {
                        AccountId = asset.GLAccumulatedDepreciationAccountId ?? category?.DefaultAccumulatedDepreciationAccountId ?? Guid.Empty,
                        AccountName = "Accumulated Depreciation",
                        Debit = 0,
                        Credit = depreciationAmount,
                        Description = $"Accumulated depreciation: {asset.NameEn}"
                    }
                }
            };
        }

        public JournalPreviewDto PreviewSaleJournal(FixedAsset asset, decimal salePrice)
        {
            var category = _categoryRepository.GetById(asset.CategoryId);
            var bookValue = asset.PurchaseCost - asset.AccumulatedDepreciation;
            var gainLoss = salePrice - bookValue;

            var entries = new List<JournalEntryPreviewDto>
            {
                new JournalEntryPreviewDto
                {
                    AccountName = "Cash",
                    Debit = salePrice,
                    Credit = 0,
                    Description = "Cash received from asset sale"
                }
            };

            if (asset.AccumulatedDepreciation > 0)
            {
                entries.Add(new JournalEntryPreviewDto
                {
                    AccountId = asset.GLAccumulatedDepreciationAccountId ?? category?.DefaultAccumulatedDepreciationAccountId ?? Guid.Empty,
                    AccountName = "Accumulated Depreciation",
                    Debit = asset.AccumulatedDepreciation,
                    Credit = 0,
                    Description = "Remove accumulated depreciation"
                });
            }

            if (gainLoss != 0)
            {
                entries.Add(new JournalEntryPreviewDto
                {
                    AccountId = category?.DefaultGainLossAccountId ?? Guid.Empty,
                    AccountName = gainLoss > 0 ? "Gain on Sale" : "Loss on Sale",
                    Debit = gainLoss < 0 ? Math.Abs(gainLoss) : 0,
                    Credit = gainLoss > 0 ? gainLoss : 0,
                    Description = gainLoss > 0 ? "Gain on asset sale" : "Loss on asset sale"
                });
            }

            entries.Add(new JournalEntryPreviewDto
            {
                AccountId = asset.GLAssetAccountId ?? category?.DefaultAssetAccountId ?? Guid.Empty,
                AccountName = "Fixed Asset",
                Debit = 0,
                Credit = asset.PurchaseCost,
                Description = "Remove asset cost"
            });

            return new JournalPreviewDto
            {
                Description = $"Sale of asset: {asset.NameEn ?? asset.AssetCode}",
                JournalDate = DateTime.Now,
                Entries = entries
            };
        }

        public JournalPreviewDto PreviewDisposalJournal(FixedAsset asset)
        {
            var category = _categoryRepository.GetById(asset.CategoryId);
            var bookValue = asset.PurchaseCost - asset.AccumulatedDepreciation;

            var entries = new List<JournalEntryPreviewDto>();

            if (asset.AccumulatedDepreciation > 0)
            {
                entries.Add(new JournalEntryPreviewDto
                {
                    AccountId = asset.GLAccumulatedDepreciationAccountId ?? category?.DefaultAccumulatedDepreciationAccountId ?? Guid.Empty,
                    AccountName = "Accumulated Depreciation",
                    Debit = asset.AccumulatedDepreciation,
                    Credit = 0,
                    Description = "Remove accumulated depreciation"
                });
            }

            if (bookValue > 0)
            {
                entries.Add(new JournalEntryPreviewDto
                {
                    AccountId = category?.DefaultGainLossAccountId ?? Guid.Empty,
                    AccountName = "Loss on Disposal",
                    Debit = bookValue,
                    Credit = 0,
                    Description = "Loss on asset disposal"
                });
            }

            entries.Add(new JournalEntryPreviewDto
            {
                AccountId = asset.GLAssetAccountId ?? category?.DefaultAssetAccountId ?? Guid.Empty,
                AccountName = "Fixed Asset",
                Debit = 0,
                Credit = asset.PurchaseCost,
                Description = "Remove asset cost"
            });

            return new JournalPreviewDto
            {
                Description = $"Disposal of asset: {asset.NameEn ?? asset.AssetCode}",
                JournalDate = DateTime.Now,
                Entries = entries
            };
        }

        public JournalPreviewDto PreviewRevaluationJournal(FixedAsset asset, decimal newValue)
        {
            var category = _categoryRepository.GetById(asset.CategoryId);
            var adjustmentAmount = newValue - asset.CurrentBookValue;

            var entries = new List<JournalEntryPreviewDto>();

            if (adjustmentAmount > 0)
            {
                entries.Add(new JournalEntryPreviewDto
                {
                    AccountId = asset.GLAssetAccountId ?? category?.DefaultAssetAccountId ?? Guid.Empty,
                    AccountName = "Fixed Asset",
                    Debit = adjustmentAmount,
                    Credit = 0,
                    Description = "Increase in asset value"
                });
                entries.Add(new JournalEntryPreviewDto
                {
                    AccountId = category?.DefaultGainLossAccountId ?? Guid.Empty,
                    AccountName = "Revaluation Reserve",
                    Debit = 0,
                    Credit = adjustmentAmount,
                    Description = "Revaluation reserve"
                });
            }
            else if (adjustmentAmount < 0)
            {
                entries.Add(new JournalEntryPreviewDto
                {
                    AccountId = category?.DefaultGainLossAccountId ?? Guid.Empty,
                    AccountName = "Revaluation Loss",
                    Debit = Math.Abs(adjustmentAmount),
                    Credit = 0,
                    Description = "Decrease in asset value"
                });
                entries.Add(new JournalEntryPreviewDto
                {
                    AccountId = asset.GLAssetAccountId ?? category?.DefaultAssetAccountId ?? Guid.Empty,
                    AccountName = "Fixed Asset",
                    Debit = 0,
                    Credit = Math.Abs(adjustmentAmount),
                    Description = "Asset reduction"
                });
            }

            return new JournalPreviewDto
            {
                Description = $"Revaluation of asset: {asset.NameEn ?? asset.AssetCode}",
                JournalDate = DateTime.Now,
                Entries = entries
            };
        }

        #endregion
    }
}
