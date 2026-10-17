using System;
using System.Linq;
using Infrastructure;
using Infrastructure.Attribute;
using Infrastructure.Exceptions;
using Infrastructure.Extensions;
using Mapster;
using Microsoft.Extensions.Logging;
using SqlSugar;
using ZR.Model.Finance.Configuration;
using ZR.Model.Finance.Configuration.Dto;
using ZR.Model.Finance.GL;
using ZR.Repository.Finance.Configuration;
using ZR.Repository.Finance.GL;
using ZR.Service.IService.Finance.Configuration;

namespace ZR.Service.Finance.Configuration
{
    /// <summary>
    /// Tax Configuration Service - خدمة إعدادات الضرائب
    /// Service for managing VAT Input/Output and AR/AP account configurations
    /// </summary>
    [AppService(ServiceType = typeof(ITaxConfigurationService), ServiceLifetime = LifeTime.Transient)]
    public class TaxConfigurationService : BaseService<TaxConfiguration>, ITaxConfigurationService
    {
        private readonly TaxConfigurationRepository _repository;
        private readonly IGLAccountRepository _glAccountRepository;
        private readonly ILogger<TaxConfigurationService> _logger;

        public TaxConfigurationService(
            TaxConfigurationRepository repository,
            IGLAccountRepository glAccountRepository,
            ILogger<TaxConfigurationService> logger)
        {
            _repository = repository;
            _glAccountRepository = glAccountRepository;
            _logger = logger;
        }

        /// <summary>
        /// Get tax configuration by company ID
        /// If companyId is null, resolves it from request context (HttpContext.Items/claims)
        /// </summary>
        public TaxConfigurationDetailDto GetByCompanyId(long? companyId)
        {
            var scopedCompanyId = ResolveCompanyId(companyId);
            _logger.LogDebug($"[TaxConfig] GetByCompanyId called - CompanyId: {companyId}, ScopedCompanyId: {scopedCompanyId}");

            if (!scopedCompanyId.HasValue)
            {
                throw new AppException(ExceptionCode.INVALID_PARAMETER).WithParameter("CompanyId", null);
            }

            // Always scope by company; do NOT fall back to "CompanyID IS NULL" because it causes cross-company drift and duplicates.
            var predicate = Expressionable.Create<TaxConfiguration>();
            predicate = predicate.And(x => x.CompanyID == scopedCompanyId.Value);
            var entity = _repository.GetList(predicate.ToExpression()).FirstOrDefault();

            if (entity == null)
            {
                _logger.LogWarning($"[TaxConfig] No tax configuration found for CompanyID={scopedCompanyId.Value}");
                return null;
            }

            _logger.LogDebug($"[TaxConfig] Found record ID: {entity.Id}, AR: {entity.DefaultARAccountId}, AP: {entity.DefaultAPAccountId}");

            var dto = entity.Adapt<TaxConfigurationDetailDto>();

            // Load account names
            EnrichWithAccountNames(dto);

            _logger.LogDebug($"[TaxConfig] Returning DTO - AR: {dto.DefaultARAccountId}, AP: {dto.DefaultAPAccountId}");

            return dto;
        }

        /// <summary>
        /// Get tax configuration detail by ID
        /// </summary>
        public TaxConfigurationDetailDto GetDetail(Guid id)
        {
            var entity = _repository.GetById(id);
            if (entity == null)
            {
                return null;
            }

            var dto = entity.Adapt<TaxConfigurationDetailDto>();
            EnrichWithAccountNames(dto);

            return dto;
        }

        /// <summary>
        /// Save tax configuration (create or update)
        /// </summary>
        public TaxConfiguration SaveConfiguration(SaveTaxConfigurationDto dto, string userId)
        {
            // Request contract: tenantId only
            var scopedCompanyId = ResolveCompanyId(dto.TenantId);
            _logger.LogInformation($"[TaxConfig] SaveConfiguration called - TenantId: {dto.TenantId}, ScopedCompanyId: {scopedCompanyId}, AR: {dto.DefaultARAccountId}, AP: {dto.DefaultAPAccountId}");

            if (!scopedCompanyId.HasValue)
            {
                throw new AppException(ExceptionCode.INVALID_PARAMETER).WithParameter("CompanyId", null);
            }

            // Find existing configuration
            TaxConfiguration existing;
            _logger.LogDebug($"[TaxConfig] Searching for record with CompanyID = {scopedCompanyId.Value}");
            existing = _repository.GetFirst(x => x.CompanyID == scopedCompanyId.Value);

            if (existing != null)
            {
                _logger.LogInformation($"[TaxConfig] Updating existing record: {existing.Id}");

                // Update existing
                existing.VATInputAccountId = dto.VATInputAccountId;
                existing.VATOutputAccountId = dto.VATOutputAccountId;
                existing.DefaultARAccountId = dto.DefaultARAccountId;
                existing.DefaultAPAccountId = dto.DefaultAPAccountId;
                existing.IsConfigured = dto.VATInputAccountId.HasValue && dto.VATOutputAccountId.HasValue;
                existing.Update_time = DateTime.Now;
                existing.Update_by = userId;

                _logger.LogDebug($"[TaxConfig] Updating - AR: {existing.DefaultARAccountId}, AP: {existing.DefaultAPAccountId}");

                try
                {
                    // Use explicit column update to ensure nullable GUIDs are updated
                    var result = _repository.Context.Updateable(existing)
                        .UpdateColumns(x => new
                        {
                            x.VATInputAccountId,
                            x.VATOutputAccountId,
                            x.DefaultARAccountId,
                            x.DefaultAPAccountId,
                            x.IsConfigured,
                            x.Update_time,
                            x.Update_by
                        })
                        .ExecuteCommand();

                    _logger.LogInformation($"[TaxConfig] Update successful - {result} rows affected");

                    // Re-fetch from database to verify it was saved
                    var verified = _repository.GetById(existing.Id);
                    _logger.LogDebug($"[TaxConfig] Verified from DB - AR: {verified?.DefaultARAccountId}, AP: {verified?.DefaultAPAccountId}");

                    if (verified == null)
                    {
                        _logger.LogError("[TaxConfig] Failed to verify updated record");
                        throw new AppException(ExceptionCode.FINANCE_CONFIG_TAX_UPDATE_FAILED);
                    }

                    return existing;
                }
                catch (Exception ex) when (!(ex is AppException))
                {
                    _logger.LogError(ex, "[TaxConfig] Failed to update tax configuration");
                    throw new AppException(ExceptionCode.FINANCE_CONFIG_TAX_UPDATE_FAILED);
                }
            }
            else
            {
                _logger.LogInformation("[TaxConfig] Creating new configuration record");

                try
                {
                    // Create new
                    var entity = new TaxConfiguration
                    {
                        Id = Guid.NewGuid(),
                        CompanyID = scopedCompanyId,
                        VATInputAccountId = dto.VATInputAccountId,
                        VATOutputAccountId = dto.VATOutputAccountId,
                        DefaultARAccountId = dto.DefaultARAccountId,
                        DefaultAPAccountId = dto.DefaultAPAccountId,
                        IsConfigured = dto.VATInputAccountId.HasValue && dto.VATOutputAccountId.HasValue,
                        Create_time = DateTime.Now,
                        Create_by = userId
                    };

                    _repository.Insert(entity);
                    _logger.LogInformation($"[TaxConfig] Created new record with ID: {entity.Id}");

                    return entity;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[TaxConfig] Failed to create tax configuration");
                    throw new AppException(ExceptionCode.FINANCE_CONFIG_TAX_SAVE_FAILED);
                }
            }
        }

        /// <summary>
        /// Get configuration status
        /// </summary>
        public TaxConfigurationStatusDto GetConfigurationStatus(long? companyId)
        {
            var scopedCompanyId = ResolveCompanyId(companyId);
            var config = GetByCompanyId(scopedCompanyId);

            if (config == null)
            {
                return new TaxConfigurationStatusDto
                {
                    IsConfigured = false,
                    HasVATInputAccount = false,
                    HasVATOutputAccount = false,
                    HasDefaultARAccount = false,
                    HasDefaultAPAccount = false,
                    Message = "Tax configuration not configured",
                    MessageAr = "إعدادات الضريبة غير مكونة"
                };
            }

            var status = new TaxConfigurationStatusDto
            {
                IsConfigured = config.IsConfigured,
                HasVATInputAccount = config.VATInputAccountId.HasValue,
                HasVATOutputAccount = config.VATOutputAccountId.HasValue,
                HasDefaultARAccount = config.DefaultARAccountId.HasValue,
                HasDefaultAPAccount = config.DefaultAPAccountId.HasValue
            };

            if (status.IsConfigured)
            {
                status.Message = "Tax configuration is complete";
                status.MessageAr = "إعدادات الضريبة مكتملة";
            }
            else
            {
                var missingItems = new System.Collections.Generic.List<string>();
                var missingItemsAr = new System.Collections.Generic.List<string>();

                if (!status.HasVATInputAccount)
                {
                    missingItems.Add("VAT Input Account");
                    missingItemsAr.Add("حساب ضريبة المدخلات");
                }
                if (!status.HasVATOutputAccount)
                {
                    missingItems.Add("VAT Output Account");
                    missingItemsAr.Add("حساب ضريبة المخرجات");
                }

                status.Message = $"Tax configuration incomplete. Missing: {string.Join(", ", missingItems)}";
                status.MessageAr = $"إعدادات الضريبة غير مكتملة. المفقود: {string.Join("، ", missingItemsAr)}";
            }

            return status;
        }

        /// <summary>
        /// Get VAT Input Account ID
        /// </summary>
        public Guid? GetVATInputAccountId(long? companyId)
        {
            var config = GetConfigurationEntity(companyId);
            return config?.VATInputAccountId;
        }

        /// <summary>
        /// Get VAT Output Account ID
        /// </summary>
        public Guid? GetVATOutputAccountId(long? companyId)
        {
            var config = GetConfigurationEntity(companyId);
            return config?.VATOutputAccountId;
        }

        /// <summary>
        /// Get Default AR Account ID
        /// </summary>
        public Guid? GetDefaultARAccountId(long? companyId)
        {
            var config = GetConfigurationEntity(companyId);
            return config?.DefaultARAccountId;
        }

        /// <summary>
        /// Get Default AP Account ID
        /// </summary>
        public Guid? GetDefaultAPAccountId(long? companyId)
        {
            var config = GetConfigurationEntity(companyId);
            return config?.DefaultAPAccountId;
        }

        /// <summary>
        /// Ensure configuration exists, creating default if needed
        /// </summary>
        public void EnsureConfigurationExists(long? companyId)
        {
            var scopedCompanyId = ResolveCompanyId(companyId);
            if (!scopedCompanyId.HasValue)
            {
                throw new AppException(ExceptionCode.INVALID_PARAMETER).WithParameter("CompanyId", null);
            }

            var predicate = Expressionable.Create<TaxConfiguration>();
            predicate = predicate.And(x => x.CompanyID == scopedCompanyId.Value);

            var existing = _repository.GetList(predicate.ToExpression()).FirstOrDefault();

            if (existing == null)
            {
                // Create default unconfigured record
                var entity = new TaxConfiguration
                {
                    Id = Guid.NewGuid(),
                    CompanyID = scopedCompanyId,
                    IsConfigured = false,
                    Create_time = DateTime.Now,
                    Create_by = "system"
                };

                _repository.Insert(entity);
            }
        }

        #region Private Helpers

        private TaxConfiguration GetConfigurationEntity(long? companyId)
        {
            var scopedCompanyId = ResolveCompanyId(companyId);
            if (!scopedCompanyId.HasValue)
            {
                return null;
            }

            _logger.LogDebug($"[TaxConfig] GetConfigurationEntity - Searching for CompanyID = {scopedCompanyId.Value}");
            return _repository.GetFirst(x => x.CompanyID == scopedCompanyId.Value);
        }

        private long? ResolveCompanyId(long? companyId)
        {
            if (companyId.HasValue) return companyId;

            // Prefer injected request scope (FixedCompanyBranchScopingMiddleware)
            var ctx = App.HttpContext;
            if (ctx?.Items != null && ctx.Items.TryGetValue("CompanyId", out var companyObj) && companyObj != null)
            {
                if (companyObj is long l) return l;
                if (companyObj is int i) return i;
                if (companyObj is string s && long.TryParse(s, out var parsed)) return parsed;
            }

            // Fallback to claims-based helper
            var claimCompany = ctx?.GetCompanyId() ?? 0;
            return claimCompany > 0 ? claimCompany : (long?)null;
        }

        private void EnrichWithAccountNames(TaxConfigurationDetailDto dto)
        {
            if (dto == null) return;

            if (dto.VATInputAccountId.HasValue)
            {
                var account = _glAccountRepository.GetById(dto.VATInputAccountId.Value);
                if (account != null)
                {
                    dto.VATInputAccountCode = account.AccountNumber;
                    dto.VATInputAccountName = account.AccountNameEn;
                }
            }

            if (dto.VATOutputAccountId.HasValue)
            {
                var account = _glAccountRepository.GetById(dto.VATOutputAccountId.Value);
                if (account != null)
                {
                    dto.VATOutputAccountCode = account.AccountNumber;
                    dto.VATOutputAccountName = account.AccountNameEn;
                }
            }

            if (dto.DefaultARAccountId.HasValue)
            {
                var account = _glAccountRepository.GetById(dto.DefaultARAccountId.Value);
                if (account != null)
                {
                    dto.DefaultARAccountCode = account.AccountNumber;
                    dto.DefaultARAccountName = account.AccountNameEn;
                }
            }

            if (dto.DefaultAPAccountId.HasValue)
            {
                var account = _glAccountRepository.GetById(dto.DefaultAPAccountId.Value);
                if (account != null)
                {
                    dto.DefaultAPAccountCode = account.AccountNumber;
                    dto.DefaultAPAccountName = account.AccountNameEn;
                }
            }
        }

        #endregion
    }
}
