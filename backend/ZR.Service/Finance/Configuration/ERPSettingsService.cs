using System;
using System.Collections.Generic;
using System.Linq;
using Infrastructure.Attribute;
using Infrastructure.Exceptions;
using Infrastructure.Extensions;
using Infrastructure;
using Mapster;
using SqlSugar;
using ZR.Model;
using ZR.Model.Finance.Configuration;
using ZR.Model.Finance.Configuration.Dto;
using ZR.Repository.Finance.Configuration;
using ZR.Service.IService.Finance.Configuration;

namespace ZR.Service.Finance.Configuration
{
    /// <summary>
    /// ERP Settings Service - خدمة إعدادات النظام
    /// </summary>
    [AppService(ServiceType = typeof(IERPSettingsService), ServiceLifetime = LifeTime.Transient)]
    public class ERPSettingsService : BaseService<ERPSettings>, IERPSettingsService
    {
        private readonly IERPSettingsRepository _settingsRepository;
        private readonly ICurrencyRepository _currencyRepository;
        private readonly ITaxRuleRepository _taxRuleRepository;

        public ERPSettingsService(
            IERPSettingsRepository settingsRepository,
           ICurrencyRepository currencyRepository,
            ITaxRuleRepository taxRuleRepository)
        {
            _settingsRepository = settingsRepository;
            _currencyRepository = currencyRepository;
            _taxRuleRepository = taxRuleRepository;
        }

        /// <summary>
        /// Get paginated list of settings
        /// </summary>
        public PagedInfo<ERPSettingsListDto> GetList(ERPSettingsQueryDto query, PagerInfo pager)
        {
            var scopedCompanyId = ResolveCompanyId(query.TenantId, query.TenantId);
            var searchModel = new ERPSettings
            {
                CompanyID = scopedCompanyId
            };

            var result = _settingsRepository.GetList(searchModel, pager);
            var dtoList = result.Result.Select(settings => MapToDto(settings)).ToList();

            return new PagedInfo<ERPSettingsListDto>
            {
                TotalNum = result.TotalNum,
                PageIndex = result.PageIndex,
                PageSize = result.PageSize,
                Result = dtoList
            };
        }

        /// <summary>
        /// Get settings by ID
        /// </summary>
        public ERPSettingsDetailDto GetById(Guid id)
        {
            var settings = _settingsRepository.GetById(id);
            if (settings == null) return null;

            return MapToDetailDto(settings);
        }

        /// <summary>
        /// Get settings by company
        /// </summary>
        public ERPSettingsDetailDto GetByCompanyId(long? companyId)
        {
            var scopedCompanyId = ResolveCompanyId(companyId, null);
            var settings = _settingsRepository.GetByCompanyId(scopedCompanyId);
            if (settings == null) return null;

            return MapToDetailDto(settings);
        }

        /// <summary>
        /// Create or Update settings (Upsert)
        /// If settings already exist for the company, update them; otherwise create new
        /// </summary>
        public ERPSettings Create(CreateERPSettingsDto dto, string userName)
        {
            // Request contract: tenantId only
            var scopedCompanyId = ResolveCompanyId(null, dto.TenantId);
            if (!scopedCompanyId.HasValue)
            {
                throw new AppException(ExceptionCode.INVALID_PARAMETER)
                    .WithParameter("CompanyId", null);
            }

            // Validate default currency if provided
            if (dto.DefaultCurrencyId.HasValue)
            {
                var currency = _currencyRepository.GetById(dto.DefaultCurrencyId.Value);
                if (currency == null)
                {
                    throw new AppException(ExceptionCode.FINANCE_CONFIG_ERPSETTINGS_DEFAULT_CURRENCY_NOT_FOUND)
                        .WithParameter("CurrencyId", dto.DefaultCurrencyId.Value);
                }
            }

            // Validate multi-currency requirement
            if (dto.EnableMultiCurrency && !dto.DefaultCurrencyId.HasValue)
            {
                throw new AppException(ExceptionCode.FINANCE_CONFIG_ERPSETTINGS_DEFAULT_CURRENCY_REQUIRED);
            }

            // Validate default tax rule if provided
            if (dto.DefaultTaxRuleId.HasValue)
            {
                var taxRule = _taxRuleRepository.GetById(dto.DefaultTaxRuleId.Value);
                if (taxRule == null || !taxRule.Status)
                {
                    throw new AppException(ExceptionCode.FINANCE_CONFIG_ERPSETTINGS_DEFAULT_TAXRULE_INVALID)
                        .WithParameter("TaxRuleId", dto.DefaultTaxRuleId.Value);
                }
            }

            // Check if settings already exist for this company
            var existingSettings = _settingsRepository.GetByCompanyId(scopedCompanyId);

            if (existingSettings != null)
            {
                // Update existing settings
                var updateDto = new UpdateERPSettingsDto
                {
                    Id = existingSettings.Id,
                    TenantId = scopedCompanyId,
                    DefaultCurrencyId = dto.DefaultCurrencyId,
                    EnableMultiCurrency = dto.EnableMultiCurrency,
                    DefaultApprovalLimit = dto.DefaultApprovalLimit,
                    AutoPostToGL = dto.AutoPostToGL,
                    RequireApprovalForVouchers = dto.RequireApprovalForVouchers,
                    AllowBackdating = dto.AllowBackdating,
                    MaxBackdatingDays = dto.MaxBackdatingDays,
                    EnableBudgetControl = dto.EnableBudgetControl,
                    FiscalYearStartMonth = dto.FiscalYearStartMonth,
                    NumberOfFiscalPeriods = dto.NumberOfFiscalPeriods,
                    DefaultTaxRuleId = dto.DefaultTaxRuleId,
                    Notes = dto.Notes
                };

                Update(updateDto, userName);
                return _settingsRepository.GetById(existingSettings.Id);
            }

            // Create new settings
            var model = dto.Adapt<ERPSettings>();
            model.Id = Guid.NewGuid();
            model.CompanyID = scopedCompanyId;
            model.Create_by = userName;
            model.Create_time = DateTime.Now;

            _settingsRepository.Insert(model);
            return model;
        }

        /// <summary>
        /// Update settings
        /// </summary>
        public int Update(UpdateERPSettingsDto dto, string userName)
        {
            // Request contract: tenantId only
            var scopedCompanyId = ResolveCompanyId(null, dto.TenantId);
            if (!scopedCompanyId.HasValue)
            {
                throw new AppException(ExceptionCode.INVALID_PARAMETER)
                    .WithParameter("CompanyId", null);
            }

            // Validate default currency if provided
            if (dto.DefaultCurrencyId.HasValue)
            {
                var currency = _currencyRepository.GetById(dto.DefaultCurrencyId.Value);
                if (currency == null)
                {
                    throw new AppException(ExceptionCode.FINANCE_CONFIG_ERPSETTINGS_DEFAULT_CURRENCY_NOT_FOUND)
                        .WithParameter("CurrencyId", dto.DefaultCurrencyId.Value);
                }
            }

            // Validate multi-currency requirement
            if (dto.EnableMultiCurrency && !dto.DefaultCurrencyId.HasValue)
            {
                throw new AppException(ExceptionCode.FINANCE_CONFIG_ERPSETTINGS_DEFAULT_CURRENCY_REQUIRED);
            }

            // Validate default tax rule if provided
            if (dto.DefaultTaxRuleId.HasValue)
            {
                var taxRule = _taxRuleRepository.GetById(dto.DefaultTaxRuleId.Value);
                if (taxRule == null || !taxRule.Status)
                {
                    throw new AppException(ExceptionCode.FINANCE_CONFIG_ERPSETTINGS_DEFAULT_TAXRULE_INVALID)
                        .WithParameter("TaxRuleId", dto.DefaultTaxRuleId.Value);
                }
            }

            var model = dto.Adapt<ERPSettings>();
            model.CompanyID = scopedCompanyId;
            model.Update_by = userName;
            model.Update_time = DateTime.Now;

            return Update(model, it => new
            {
                it.CompanyID,
                it.DefaultCurrencyId,
                it.EnableMultiCurrency,
                it.DefaultApprovalLimit,
                it.AutoPostToGL,
                it.RequireApprovalForVouchers,
                it.AllowBackdating,
                it.MaxBackdatingDays,
                it.EnableBudgetControl,
                it.FiscalYearStartMonth,
                it.NumberOfFiscalPeriods,
                it.DefaultTaxRuleId,
                it.Notes,
                it.Update_by,
                it.Update_time
            }, f => f.Id == model.Id);
        }

        /// <summary>
        /// Get or Create settings for company
        /// Returns existing settings if found, or creates default settings if not
        /// </summary>
        public ERPSettingsDetailDto GetOrCreate(long? companyId, string userName)
        {
            var scopedCompanyId = ResolveCompanyId(companyId, null);
            if (!scopedCompanyId.HasValue)
            {
                throw new AppException(ExceptionCode.INVALID_PARAMETER)
                    .WithParameter("CompanyId", null);
            }

            var settings = _settingsRepository.GetByCompanyId(scopedCompanyId);

            if (settings != null)
            {
                return MapToDetailDto(settings);
            }

            // Create default settings
            var defaultDto = new CreateERPSettingsDto
            {
                TenantId = scopedCompanyId,
                EnableMultiCurrency = false,
                DefaultApprovalLimit = 0,
                AutoPostToGL = false,
                RequireApprovalForVouchers = false,
                AllowBackdating = true,
                MaxBackdatingDays = 30,
                EnableBudgetControl = false,
                FiscalYearStartMonth = 1, // January
                NumberOfFiscalPeriods = 12, // Monthly
                Notes = "Auto-created default settings"
            };

            var created = Create(defaultDto, userName);
            return MapToDetailDto(created);
        }

        /// <summary>
        /// Delete settings
        /// </summary>
        public int Delete(Guid[] ids)
        {
            return Context.Deleteable<ERPSettings>()
                .Where(it => ids.Contains(it.Id))
                .ExecuteCommand();
        }

        private ERPSettingsListDto MapToDto(ERPSettings settings)
        {
            var dto = new ERPSettingsListDto
            {
                Id = settings.Id,
                TenantId = settings.CompanyID,
                // Backward-compatible: many frontend flows treat TenantId == CompanyId in this codebase
                //TenantId = settings.CompanyID,
                DefaultCurrencyId = settings.DefaultCurrencyId,
                EnableMultiCurrency = settings.EnableMultiCurrency,
                DefaultApprovalLimit = settings.DefaultApprovalLimit,
                AutoPostToGL = settings.AutoPostToGL,
                CreatedAt = settings.Create_time
            };
            
            if (settings.DefaultCurrencyId.HasValue)
            {
                var currency = _currencyRepository.GetById(settings.DefaultCurrencyId.Value);
                dto.DefaultCurrencyCode = currency != null ? currency.Code : string.Empty;
            }

            return dto;
        }

        private ERPSettingsDetailDto MapToDetailDto(ERPSettings settings)
        {
            var dto = new ERPSettingsDetailDto
            {
                Id = settings.Id,
                CompanyId = settings.CompanyID,
                TenantId = settings.CompanyID,
                DefaultCurrencyId = settings.DefaultCurrencyId,
                EnableMultiCurrency = settings.EnableMultiCurrency,
                DefaultApprovalLimit = settings.DefaultApprovalLimit,
                AutoPostToGL = settings.AutoPostToGL,
                CreatedAt = settings.Create_time,
                RequireApprovalForVouchers = settings.RequireApprovalForVouchers,
                AllowBackdating = settings.AllowBackdating,
                MaxBackdatingDays = settings.MaxBackdatingDays,
                EnableBudgetControl = settings.EnableBudgetControl,
                FiscalYearStartMonth = settings.FiscalYearStartMonth,
                NumberOfFiscalPeriods = settings.NumberOfFiscalPeriods,
                DefaultTaxRuleId = settings.DefaultTaxRuleId,
                Notes = settings.Notes,
                CreatedBy = settings.Create_by,
                UpdatedAt = settings.Update_time,
                UpdatedBy = settings.Update_by
            };
            
            if (settings.DefaultCurrencyId.HasValue)
            {
                var currency = _currencyRepository.GetById(settings.DefaultCurrencyId.Value);
                dto.DefaultCurrencyCode = currency != null ? currency.Code : string.Empty;
            }

            return dto;
        }

        private long? ResolveCompanyId(long? companyId, long? tenantId)
        {
            // Explicit parameters win (admin/multi-company scenarios)
            if (companyId.HasValue) return companyId;
            if (tenantId.HasValue) return tenantId;

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


