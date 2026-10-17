using Infrastructure;
using Infrastructure.Attribute;
using Infrastructure.Exceptions;
using Infrastructure.Extensions;
using Mapster;
using Microsoft.Extensions.Logging;
using SqlSugar;
using System;
using System.Linq;
using ZR.Model;
using ZR.Model.Finance.Treasury;
using ZR.Model.Finance.Treasury.Dto;
using ZR.Model.Finance.Treasury.Enums;
using ZR.Repository.Finance.Treasury;
using ZR.Service.IService.Finance.Treasury;

namespace ZR.Service.Finance.Treasury
{
    /// <summary>
    /// Treasury Config Service - خدمة إعدادات الخزينة
    /// </summary>
    [AppService(ServiceType = typeof(ITreasuryConfigService), ServiceLifetime = LifeTime.Transient)]
    public class TreasuryConfigService : BaseService<TreasuryConfig>, ITreasuryConfigService
    {
        private readonly TreasuryConfigRepository _repository;
        private readonly TreasuryGLAccountMappingRepository _glMappingRepository;
        private readonly ILogger<TreasuryConfigService> _logger;

        public TreasuryConfigService(
            TreasuryConfigRepository repository,
            TreasuryGLAccountMappingRepository glMappingRepository,
            ILogger<TreasuryConfigService> logger)
        {
            _repository = repository;
            _glMappingRepository = glMappingRepository;
            _logger = logger;
        }

        public PagedInfo<TreasuryConfigDetailDto> GetList(TreasuryConfigQueryDto parm)
        {
            var predicate = Expressionable.Create<TreasuryConfig>();

            if (parm.TenantId.HasValue)
            {
                // TenantId in this module represents CompanyID (do NOT hash it)
                predicate = predicate.And(x => x.CompanyID == parm.TenantId.Value);
            }

            var response = _repository.GetPages(predicate.ToExpression(), parm);
            // Map list items explicitly so TenantId/CreatedAt are correctly populated
            return new PagedInfo<TreasuryConfigDetailDto>
            {
                TotalNum = response.TotalNum,
                PageIndex = response.PageIndex,
                PageSize = response.PageSize,
                Result = response.Result.Select(MapToDetailDto).ToList()
            };
        }

        public TreasuryConfigDetailDto GetDetail(Guid id)
        {
            var entity = _repository.GetById(id);
            if (entity == null)
            {
                return null;
            }
            return MapToDetailDto(entity);
        }

        public TreasuryConfigDetailDto GetByTenant(long? companyId)
        {
            var scopedCompanyId = ResolveCompanyId(companyId, null);
            _logger.LogDebug($"[TreasuryConfig] GetByTenant called - TenantId: {companyId}, ScopedCompanyId: {scopedCompanyId}");

            if (!scopedCompanyId.HasValue)
            {
                return null;
            }

            // Always scope by company (do NOT fall back to CompanyID IS NULL)
            var entity = _repository.GetFirst(x => x.CompanyID == scopedCompanyId.Value);

            if (entity == null)
            {
                _logger.LogWarning("[TreasuryConfig] No treasury configuration found");
                return null;
            }

            _logger.LogDebug($"[TreasuryConfig] Found record: {entity.Id}");
            return MapToDetailDto(entity);
        }

        public TreasuryConfig Create(CreateTreasuryConfigDto dto, string userId)
        {
            // Resolve company ID using same pattern as ERPSettingsService
            var scopedCompanyId = ResolveCompanyId(null, dto.TenantId);

            _logger.LogInformation($"[TreasuryConfig] Creating/Updating treasury configuration for CompanyId: {scopedCompanyId}");

            // Check if config already exists for company - UPSERT pattern
            var existing = GetByTenant(scopedCompanyId);
            if (existing != null)
            {
                _logger.LogInformation($"[TreasuryConfig] Configuration already exists for CompanyId: {scopedCompanyId}, updating instead");

                // Update existing config instead of throwing error
                var updateDto = new UpdateTreasuryConfigDto
                {
                    Id = existing.Id,
                    AutoImportBankStatements = dto.AutoImportBankStatements,
                    BankStatementImportFormat = dto.BankStatementImportFormat,
                    AutoReconcile = dto.AutoReconcile,
                    DefaultPaymentApprovalThreshold = dto.DefaultPaymentApprovalThreshold,
                    PettyCashLimit = dto.PettyCashLimit,
                    ApprovalWorkflowEnabled = dto.ApprovalWorkflowEnabled,
                    // Copy GL account mappings
                    DefaultCashAccountId = dto.DefaultCashAccountId,
                    DefaultBankAccountId = dto.DefaultBankAccountId,
                    CashGLAccountId = dto.CashGLAccountId,
                    BankGLAccountId = dto.BankGLAccountId,
                    PaymentGLAccountId = dto.PaymentGLAccountId,
                    ChequeIssuedGLAccountId = dto.ChequeIssuedGLAccountId,
                    ChequeClearedGLAccountId = dto.ChequeClearedGLAccountId,
                    ReconciliationAdjustmentGLAccountId = dto.ReconciliationAdjustmentGLAccountId,
                    AccountsReceivableGLAccountId = dto.AccountsReceivableGLAccountId,
                    Notes = dto.Notes
                };

                Update(updateDto, userId);

                // Return the updated entity
                var entity = _repository.GetById(existing.Id);
                return entity;
            }

            try
            {
                var entity = dto.Adapt<TreasuryConfig>();
                entity.Id = Guid.NewGuid();
                entity.CompanyID = scopedCompanyId;
                entity.Create_time = DateTime.Now;

                if (Guid.TryParse(userId, out Guid userGuid))
                {
                    entity.Create_by = userGuid.ToString();
                }

                _repository.Insert(entity);
                _logger.LogInformation($"[TreasuryConfig] Created treasury configuration with ID: {entity.Id}");

                // Save GL account mappings
                SaveGLAccountMappings(dto, scopedCompanyId, userId);

                return entity;
            }
            catch (Exception ex) when (!(ex is AppException))
            {
                _logger.LogError(ex, "[TreasuryConfig] Failed to create treasury configuration");
                throw new AppException(ExceptionCode.FINANCE_CONFIG_TREASURY_SAVE_FAILED)
                {
                    Details = new { CompanyId = scopedCompanyId, Error = ex.Message }
                };
            }
        }

        /// <summary>
        /// Get or Create treasury configuration for tenant
        /// Returns existing config if found, creates default config if not
        /// </summary>
        public TreasuryConfigDetailDto GetOrCreate(long? companyId, string userId)
        {
            // Resolve company ID using same pattern as ERPSettingsService
            var scopedCompanyId = ResolveCompanyId(companyId, null);

            _logger.LogDebug($"[TreasuryConfig] GetOrCreate called - ScopedCompanyId: {scopedCompanyId}");

            var config = GetByTenant(scopedCompanyId);

            if (config != null)
            {
                return config;
            }

            // Create default treasury configuration
            _logger.LogInformation($"[TreasuryConfig] No configuration found for CompanyId: {scopedCompanyId}, creating default");

            var defaultDto = new CreateTreasuryConfigDto
            {
                TenantId = scopedCompanyId,
                AutoImportBankStatements = false,
                BankStatementImportFormat = BankStatementImportSource.CSV,
                AutoReconcile = false,
                DefaultPaymentApprovalThreshold = 10000,
                PettyCashLimit = 1000,
                ApprovalWorkflowEnabled = true
            };

            var created = Create(defaultDto, userId);
            return MapToDetailDto(created);
        }

        public int Update(UpdateTreasuryConfigDto dto, string userId)
        {
            _logger.LogInformation($"[TreasuryConfig] Updating treasury configuration ID: {dto.Id}");

            var entity = _repository.GetById(dto.Id);
            if (entity == null)
            {
                _logger.LogWarning($"[TreasuryConfig] Configuration not found for ID: {dto.Id}");
                throw new AppException(ExceptionCode.FINANCE_CONFIG_TREASURY_NOT_FOUND)
                {
                    Details = new { dto.Id }
                };
            }

            try
            {
                // Basic validation to keep user-facing errors clear and localized
                if (dto.DefaultPaymentApprovalThreshold < 0)
                {
                    throw new AppException(ExceptionCode.FINANCE_CONFIG_TREASURY_INVALID_APPROVAL_THRESHOLD)
                        .WithParameter("Value", dto.DefaultPaymentApprovalThreshold);
                }
                if (dto.PettyCashLimit < 0)
                {
                    throw new AppException(ExceptionCode.FINANCE_CONFIG_TREASURY_INVALID_PETTY_CASH_LIMIT)
                        .WithParameter("Value", dto.PettyCashLimit);
                }

                entity.AutoImportBankStatements = dto.AutoImportBankStatements;
                // Avoid overwriting stored value when client doesn't send this field
                if (dto.BankStatementImportFormat.HasValue)
                {
                    entity.BankStatementImportFormat = dto.BankStatementImportFormat.Value;
                }
                entity.AutoReconcile = dto.AutoReconcile;
                entity.DefaultPaymentApprovalThreshold = dto.DefaultPaymentApprovalThreshold;
                entity.PettyCashLimit = dto.PettyCashLimit;
                entity.ApprovalWorkflowEnabled = dto.ApprovalWorkflowEnabled;
                entity.Update_time = DateTime.Now;

                if (Guid.TryParse(userId, out Guid userGuid))
                {
                    entity.Update_by = userGuid.ToString();
                }

                var result = _repository.Update(entity);
                _logger.LogInformation($"[TreasuryConfig] Updated treasury configuration - {result} rows affected");

                // Save GL account mappings
                var createDto = new CreateTreasuryConfigDto
                {
                    CashGLAccountId = dto.CashGLAccountId,
                    BankGLAccountId = dto.BankGLAccountId,
                    PaymentGLAccountId = dto.PaymentGLAccountId,
                    ChequeIssuedGLAccountId = dto.ChequeIssuedGLAccountId,
                    ChequeClearedGLAccountId = dto.ChequeClearedGLAccountId,
                    ReconciliationAdjustmentGLAccountId = dto.ReconciliationAdjustmentGLAccountId,
                    AccountsReceivableGLAccountId = dto.AccountsReceivableGLAccountId
                };
                SaveGLAccountMappings(createDto, entity.CompanyID, userId);

                return result;
            }
            catch (Exception ex) when (!(ex is AppException))
            {
                _logger.LogError(ex, "[TreasuryConfig] Failed to update treasury configuration");
                throw new AppException(ExceptionCode.FINANCE_CONFIG_TREASURY_UPDATE_FAILED)
                {
                    Details = new { dto.Id, Error = ex.Message }
                };
            }
        }

        private TreasuryConfigDetailDto MapToDetailDto(TreasuryConfig entity)
        {
            Guid? createdBy = null;
            if (!string.IsNullOrWhiteSpace(entity.Create_by) && Guid.TryParse(entity.Create_by, out var cb))
            {
                createdBy = cb;
            }

            Guid? updatedBy = null;
            if (!string.IsNullOrWhiteSpace(entity.Update_by) && Guid.TryParse(entity.Update_by, out var ub))
            {
                updatedBy = ub;
            }

            // Load GL account mappings
            var cashGLMapping = _glMappingRepository.GetFirst(x =>
                x.MappingType == GLMappingType.DefaultCashAccount &&
                x.CompanyID == entity.CompanyID);

            var bankGLMapping = _glMappingRepository.GetFirst(x =>
                x.MappingType == GLMappingType.DefaultBankAccount &&
                x.CompanyID == entity.CompanyID);

            var paymentGLMapping = _glMappingRepository.GetFirst(x =>
                x.MappingType == GLMappingType.AccountsPayable &&
                x.CompanyID == entity.CompanyID);

            var chequeIssuedGLMapping = _glMappingRepository.GetFirst(x =>
                x.MappingType == GLMappingType.ChequesIssued &&
                x.CompanyID == entity.CompanyID);

            var reconciliationGLMapping = _glMappingRepository.GetFirst(x =>
                x.MappingType == GLMappingType.ReconciliationDifference &&
                x.CompanyID == entity.CompanyID);

            var accountsReceivableGLMapping = _glMappingRepository.GetFirst(x =>
                x.MappingType == GLMappingType.AccountsReceivable &&
                x.CompanyID == entity.CompanyID);

            return new TreasuryConfigDetailDto
            {
                Id = entity.Id,
                AutoImportBankStatements = entity.AutoImportBankStatements,
                BankStatementImportFormat = entity.BankStatementImportFormat,
                AutoReconcile = entity.AutoReconcile,
                DefaultPaymentApprovalThreshold = entity.DefaultPaymentApprovalThreshold,
                PettyCashLimit = entity.PettyCashLimit,
                ApprovalWorkflowEnabled = entity.ApprovalWorkflowEnabled,
                // GL Account Mappings
                CashGLAccountId = cashGLMapping?.GLAccountId.ToString(),
                BankGLAccountId = bankGLMapping?.GLAccountId.ToString(),
                PaymentGLAccountId = paymentGLMapping?.GLAccountId.ToString(),
                ChequeIssuedGLAccountId = chequeIssuedGLMapping?.GLAccountId.ToString(),
                ReconciliationAdjustmentGLAccountId = reconciliationGLMapping?.GLAccountId.ToString(),
                AccountsReceivableGLAccountId = accountsReceivableGLMapping?.GLAccountId.ToString(),
                TenantId = entity.CompanyID,
                CreatedAt = entity.Create_time,
                CreatedBy = createdBy,
                UpdatedAt = entity.Update_time,
                UpdatedBy = updatedBy
            };
        }

        /// <summary>
        /// Save or update GL account mappings from DTO
        /// </summary>
        private void SaveGLAccountMappings(CreateTreasuryConfigDto dto, long? companyId, string userId)
        {
            var now = DateTime.Now;
            var mappings = new[]
            {
                (Type: GLMappingType.DefaultCashAccount, AccountIdStr: dto.CashGLAccountId, Description: "Default cash GL account"),
                (Type: GLMappingType.DefaultBankAccount, AccountIdStr: dto.BankGLAccountId, Description: "Default bank GL account"),
                (Type: GLMappingType.AccountsPayable, AccountIdStr: dto.PaymentGLAccountId, Description: "Default payment GL account"),
                (Type: GLMappingType.ChequesIssued, AccountIdStr: dto.ChequeIssuedGLAccountId, Description: "Cheques issued GL account"),
                (Type: GLMappingType.ReconciliationDifference, AccountIdStr: dto.ReconciliationAdjustmentGLAccountId, Description: "Reconciliation adjustment GL account"),
                (Type: GLMappingType.AccountsReceivable, AccountIdStr: dto.AccountsReceivableGLAccountId, Description: "Accounts Receivable GL account")
            };

            foreach (var mapping in mappings)
            {
                if (string.IsNullOrWhiteSpace(mapping.AccountIdStr))
                    continue;

                if (!Guid.TryParse(mapping.AccountIdStr, out Guid accountId))
                {
                    _logger.LogWarning($"[TreasuryConfig] Invalid GL Account ID format: {mapping.AccountIdStr}");
                    continue;
                }

                // Check if mapping exists
                var existing = _glMappingRepository.GetFirst(x =>
                    x.MappingType == mapping.Type &&
                    x.CompanyID == companyId);

                if (existing != null)
                {
                    // Update existing mapping
                    existing.GLAccountId = accountId;
                    existing.Update_by = userId;
                    existing.Update_time = now;
                    _glMappingRepository.Update(existing);
                }
                else
                {
                    // Create new mapping
                    var newMapping = new TreasuryGLAccountMapping
                    {
                        Id = Guid.NewGuid(),
                        MappingType = mapping.Type,
                        GLAccountId = accountId,
                        Description = mapping.Description,
                        IsActive = true,
                        Priority = 100,
                        EffectiveFrom = now,
                        CompanyID = companyId,
                        Create_by = userId,
                        Create_time = now
                    };
                    _glMappingRepository.Insert(newMapping);
                }
            }

            // Handle cheque cleared GL account - check if we need a new mapping type
            if (!string.IsNullOrWhiteSpace(dto.ChequeClearedGLAccountId))
            {
                // For now, we'll skip this as there's no mapping type defined
                // TODO: Add new mapping type for cleared cheques or use existing bank account
                _logger.LogDebug($"[TreasuryConfig] ChequeClearedGLAccountId provided but no mapping type defined yet");
            }
        }

        /// <summary>
        /// Resolve company ID from multiple sources (same pattern as ERPSettingsService)
        /// Priority: explicit parameter > HttpContext.Items > claims
        /// </summary>
        private long? ResolveCompanyId(long? companyId, long? tenantId)
        {
            // Explicit parameters win (admin/multi-company scenarios)
            if (companyId.HasValue) return companyId;
            if (tenantId.HasValue) return tenantId.Value;

            // Prefer HttpContext.Items injection (FixedCompanyBranchScopingMiddleware)
            var ctx = App.HttpContext;
            if (ctx?.Items != null && ctx.Items.TryGetValue("CompanyId", out var companyObj) && companyObj != null)
            {
                if (companyObj is long l) return l;
                if (companyObj is int i) return i;
                if (companyObj is string s && long.TryParse(s, out var parsed)) return parsed;
            }

            // Fallback to claims-based helper (existing behavior)
            var claimCompany = ctx?.GetCompanyId() ?? 0;
            return claimCompany > 0 ? claimCompany : (long?)null;
        }
    }
}


