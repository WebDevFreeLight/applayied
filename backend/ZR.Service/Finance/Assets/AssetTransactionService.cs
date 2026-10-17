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
    /// Asset Transaction Service - خدمة معاملات الأصول
    /// </summary>
    [AppService(ServiceType = typeof(IAssetTransactionService), ServiceLifetime = LifeTime.Transient)]
    public class AssetTransactionService : BaseService<AssetTransaction>, IAssetTransactionService
    {
        private readonly AssetTransactionRepository _repository;
        private readonly FixedAssetRepository _assetRepository;

        public AssetTransactionService(
            AssetTransactionRepository repository,
            FixedAssetRepository assetRepository)
        {
            _repository = repository;
            _assetRepository = assetRepository;
        }

        /// <summary>
        /// Get transaction list with paging - الحصول على قائمة المعاملات مع ترقيم الصفحات
        /// </summary>
        public PagedInfo<AssetTransactionListDto> GetList(AssetTransactionQueryDto parm)
        {
            var predicate = Expressionable.Create<AssetTransaction>();

            // Filter by company
            if (parm.CompanyId.HasValue)
            {
                predicate = predicate.And(x => x.CompanyID == parm.CompanyId.Value);
            }

            // Filter by asset
            if (parm.AssetId.HasValue)
            {
                predicate = predicate.And(x => x.AssetId == parm.AssetId.Value);
            }

            // Filter by transaction type
            if (parm.TransactionType.HasValue)
            {
                predicate = predicate.And(x => x.TransactionType == parm.TransactionType.Value);
            }

            // Filter by transaction date range
            if (parm.TransactionDateFrom.HasValue)
            {
                predicate = predicate.And(x => x.TransactionDate >= parm.TransactionDateFrom.Value);
            }
            if (parm.TransactionDateTo.HasValue)
            {
                predicate = predicate.And(x => x.TransactionDate <= parm.TransactionDateTo.Value);
            }

            // Filter by reference number
            if (!string.IsNullOrEmpty(parm.ReferenceNumber))
            {
                predicate = predicate.And(x => x.ReferenceNumber != null && x.ReferenceNumber.Contains(parm.ReferenceNumber));
            }

            var response = _repository.GetPages(predicate.ToExpression(), parm);

            var pagedResult = response.Adapt<PagedInfo<AssetTransactionListDto>>();

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
        /// Get transaction by ID - الحصول على معاملة بالمعرف
        /// </summary>
        public AssetTransactionDetailDto GetDetail(Guid id)
        {
            var entity = _repository.GetById(id);
            if (entity == null)
            {
                return null;
            }

            var dto = entity.Adapt<AssetTransactionDetailDto>();

            // Get asset info
            var asset = _assetRepository.GetById(entity.AssetId);
            if (asset != null)
            {
                dto.AssetCode = asset.AssetCode;
                dto.AssetName = asset.NameEn;
            }

            return dto;
        }

        /// <summary>
        /// Get transactions for an asset - الحصول على معاملات لأصل معين
        /// </summary>
        public List<AssetTransactionListDto> GetAssetTransactions(Guid assetId)
        {
            var predicate = Expressionable.Create<AssetTransaction>();
            predicate = predicate.And(x => x.AssetId == assetId);

            var list = _repository.GetList(predicate.ToExpression())
                .OrderByDescending(x => x.TransactionDate)
                .ThenByDescending(x => x.Create_time)
                .ToList();

            var result = list.Adapt<List<AssetTransactionListDto>>();

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
        /// Record purchase transaction - تسجيل معاملة شراء
        /// </summary>
        public AssetTransaction RecordPurchase(Guid assetId, decimal amount, DateTime transactionDate, string userId, string notes = null, string referenceNumber = null)
        {
            var asset = _assetRepository.GetById(assetId);
            if (asset == null)
            {
                throw new AppException(ExceptionCode.FINANCE_ASSET_NOT_FOUND)
                    .WithParameter("AssetId", assetId);
            }

            Guid? userGuid = null;
            if (Guid.TryParse(userId, out Guid parsedGuid))
            {
                userGuid = parsedGuid;
            }

            var transaction = new AssetTransaction
            {
                Id = Guid.NewGuid(),
                CompanyID = asset.CompanyID,
                AssetId = assetId,
                TransactionType = AssetTransactionType.Purchase,
                TransactionDate = transactionDate,
                Amount = amount,
                PreviousBookValue = 0,
                NewBookValue = amount - asset.SalvageValue,
                GainLossAmount = null,
                Notes = notes,
                ReferenceNumber = referenceNumber,
                Create_by = userGuid?.ToString() ?? userId,
                Create_time = DateTime.Now
            };

            _repository.Insert(transaction);
            return transaction;
        }

        /// <summary>
        /// Record depreciation transaction - تسجيل معاملة إهلاك
        /// </summary>
        public AssetTransaction RecordDepreciation(Guid assetId, decimal amount, DateTime transactionDate, string userId, Guid? glJournalId = null)
        {
            var asset = _assetRepository.GetById(assetId);
            if (asset == null)
            {
                throw new AppException(ExceptionCode.FINANCE_ASSET_NOT_FOUND)
                    .WithParameter("AssetId", assetId);
            }

            Guid? userGuid = null;
            if (Guid.TryParse(userId, out Guid parsedGuid))
            {
                userGuid = parsedGuid;
            }

            var transaction = new AssetTransaction
            {
                Id = Guid.NewGuid(),
                CompanyID = asset.CompanyID,
                AssetId = assetId,
                TransactionType = AssetTransactionType.Depreciation,
                TransactionDate = transactionDate,
                Amount = amount,
                PreviousBookValue = asset.CurrentBookValue,
                NewBookValue = asset.CurrentBookValue - amount,
                GainLossAmount = null,
                GLJournalId = glJournalId,
                Create_by = userGuid?.ToString() ?? userId,
                Create_time = DateTime.Now
            };

            _repository.Insert(transaction);
            return transaction;
        }

        /// <summary>
        /// Record sale transaction - تسجيل معاملة بيع
        /// </summary>
        public AssetTransaction RecordSale(Guid assetId, decimal salePrice, decimal gainLoss, DateTime transactionDate, string userId, string notes = null, string referenceNumber = null)
        {
            var asset = _assetRepository.GetById(assetId);
            if (asset == null)
            {
                throw new AppException(ExceptionCode.FINANCE_ASSET_NOT_FOUND)
                    .WithParameter("AssetId", assetId);
            }

            Guid? userGuid = null;
            if (Guid.TryParse(userId, out Guid parsedGuid))
            {
                userGuid = parsedGuid;
            }

            var transaction = new AssetTransaction
            {
                Id = Guid.NewGuid(),
                CompanyID = asset.CompanyID,
                AssetId = assetId,
                TransactionType = AssetTransactionType.Sale,
                TransactionDate = transactionDate,
                Amount = salePrice,
                PreviousBookValue = asset.CurrentBookValue,
                NewBookValue = 0,
                GainLossAmount = gainLoss,
                Notes = notes,
                ReferenceNumber = referenceNumber,
                Create_by = userGuid?.ToString() ?? userId,
                Create_time = DateTime.Now
            };

            _repository.Insert(transaction);
            return transaction;
        }

        /// <summary>
        /// Record disposal transaction - تسجيل معاملة استبعاد
        /// </summary>
        public AssetTransaction RecordDisposal(Guid assetId, decimal bookValue, DateTime transactionDate, string userId, string notes = null, string referenceNumber = null)
        {
            var asset = _assetRepository.GetById(assetId);
            if (asset == null)
            {
                throw new AppException(ExceptionCode.FINANCE_ASSET_NOT_FOUND)
                    .WithParameter("AssetId", assetId);
            }

            Guid? userGuid = null;
            if (Guid.TryParse(userId, out Guid parsedGuid))
            {
                userGuid = parsedGuid;
            }

            var transaction = new AssetTransaction
            {
                Id = Guid.NewGuid(),
                CompanyID = asset.CompanyID,
                AssetId = assetId,
                TransactionType = AssetTransactionType.Disposal,
                TransactionDate = transactionDate,
                Amount = bookValue,
                PreviousBookValue = bookValue,
                NewBookValue = 0,
                GainLossAmount = -bookValue, // Loss equals book value on disposal
                Notes = notes,
                ReferenceNumber = referenceNumber,
                Create_by = userGuid?.ToString() ?? userId,
                Create_time = DateTime.Now
            };

            _repository.Insert(transaction);
            return transaction;
        }

        /// <summary>
        /// Record revaluation transaction - تسجيل معاملة إعادة تقييم
        /// </summary>
        public AssetTransaction RecordRevaluation(Guid assetId, decimal adjustmentAmount, DateTime transactionDate, string userId, string notes = null, string referenceNumber = null)
        {
            var asset = _assetRepository.GetById(assetId);
            if (asset == null)
            {
                throw new AppException(ExceptionCode.FINANCE_ASSET_NOT_FOUND)
                    .WithParameter("AssetId", assetId);
            }

            Guid? userGuid = null;
            if (Guid.TryParse(userId, out Guid parsedGuid))
            {
                userGuid = parsedGuid;
            }

            var transaction = new AssetTransaction
            {
                Id = Guid.NewGuid(),
                CompanyID = asset.CompanyID,
                AssetId = assetId,
                TransactionType = AssetTransactionType.Revaluation,
                TransactionDate = transactionDate,
                Amount = adjustmentAmount,
                PreviousBookValue = asset.CurrentBookValue,
                NewBookValue = asset.CurrentBookValue + adjustmentAmount,
                GainLossAmount = null,
                Notes = notes,
                ReferenceNumber = referenceNumber,
                Create_by = userGuid?.ToString() ?? userId,
                Create_time = DateTime.Now
            };

            _repository.Insert(transaction);
            return transaction;
        }
    }
}

