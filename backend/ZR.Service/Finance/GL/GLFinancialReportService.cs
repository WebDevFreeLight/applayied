
using Infrastructure.Attribute;

using ZR.Model.Finance.GL;
using ZR.Model.Finance.GL.Dto;
using ZR.Repository.Finance.GL;
using ZR.Service.IService.Finance.Configuration;
using ZR.Service.IService.Finance.GL;
using AccountFlags = ZR.Model.Finance.Enums.GLAccountFlags;
using WorkingCapitalType = ZR.Model.Finance.Enums.WorkingCapitalType;
using ReportType = ZR.Model.Finance.Enums.ReportType;
using AccountType = ZR.Model.Finance.Enums.AccountType;
using AccountNature = ZR.Model.Finance.Enums.AccountNature;

namespace ZR.Service.Finance
{
    /// <summary>
    /// GL Financial Report Service - خدمة التقارير المالية
    /// </summary>
    [AppService(ServiceType = typeof(IGLFinancialReportService), ServiceLifetime = LifeTime.Transient)]
    public class GLFinancialReportService : BaseService<GLFinancialReport>, IGLFinancialReportService
    {
        private readonly IGLFinancialReportRepository _reportRepository;
        private readonly IGLAccountRepository _accountRepository;
        private readonly IGLJournalEntryRepository _entryRepository;
        private readonly IGLAccountOpeningBalanceRepository _openingBalanceRepository;
        private readonly IFiscalYearService _fiscalYearService;
        private readonly INormalBalanceResolver _normalBalanceResolver;

        public GLFinancialReportService(
            IGLFinancialReportRepository reportRepository,
            IGLAccountRepository accountRepository,
            IGLJournalEntryRepository entryRepository,
            IGLAccountOpeningBalanceRepository openingBalanceRepository,
            IFiscalYearService fiscalYearService,
            INormalBalanceResolver normalBalanceResolver)
        {
            _reportRepository = reportRepository;
            _accountRepository = accountRepository;
            _entryRepository = entryRepository;
            _openingBalanceRepository = openingBalanceRepository;
            _fiscalYearService = fiscalYearService;
            _normalBalanceResolver = normalBalanceResolver;
        }

        public PagedInfo<FinancialReportResponseDto> GetList(GLFinancialReportQueryDto query, PagerInfo pager)
        {
            var searchModel = new GLFinancialReport
            {
                ReportType = query.ReportType ?? ReportType.TrialBalance,
                FiscalYear = query.FiscalYear ?? 0,
                CompanyID = query.CompanyId
            };

            var result = _reportRepository.GetList(searchModel, pager);
            var dtoList = result.Result.Select(MapToDto).ToList();

            return new PagedInfo<FinancialReportResponseDto>
            {
                TotalNum = result.TotalNum,
                PageIndex = result.PageIndex,
                PageSize = result.PageSize,
                Result = dtoList
            };
        }

        public FinancialReportResponseDto GetById(Guid id)
        {
            var entity = _reportRepository.GetById(id);
            return entity == null ? null : MapToDto(entity);
        }

        public async Task<FinancialReportResponseDto> GetByIdAsync(Guid id)
        {
            var entity = await _reportRepository.GetByIdAsync(id);
            return entity == null ? null : MapToDto(entity);
        }

        public List<FinancialReportResponseDto> GetByType(ReportType reportType, long? companyId = null)
        {
            // TODO: Repository still uses Guid? tenantId - pass null for now during migration
            var entities = _reportRepository.GetByType(reportType, null);
            return entities.Select(MapToDto).ToList();
        }

        public List<FinancialReportResponseDto> GetByPeriod(DateTime periodStart, DateTime periodEnd, long? companyId = null)
        {
            // TODO: Repository still uses Guid? tenantId - pass null for now during migration
            var entities = _reportRepository.GetByPeriod(periodStart, periodEnd, null);
            return entities.Select(MapToDto).ToList();
        }

        public FinancialReportResponseDto GenerateTrialBalance(GenerateReportDto dto, string generatedBy)
        {
            var trialBalanceData = GetTrialBalanceData(dto.PeriodStartDate, dto.PeriodEndDate, dto.CompanyId);

            Guid? generatedByGuid = null;
            if (Guid.TryParse(generatedBy, out Guid parsedGuid))
                generatedByGuid = parsedGuid;

            var report = new GLFinancialReport
            {
                Id = Guid.NewGuid(),
                ReportType = ReportType.TrialBalance,
                ReportNumber = _reportRepository.GenerateReportNumber(ReportType.TrialBalance, null, dto.FiscalYear),
                PeriodStartDate = dto.PeriodStartDate,
                PeriodEndDate = dto.PeriodEndDate,
                FiscalYear = dto.FiscalYear ?? dto.PeriodStartDate.Year,
                DescriptionAr = dto.DescriptionAr ?? $"ميزان المراجعة للفترة من {dto.PeriodStartDate:yyyy-MM-dd} إلى {dto.PeriodEndDate:yyyy-MM-dd}",
                DescriptionEn = dto.DescriptionEn ?? $"Trial Balance for period {dto.PeriodStartDate:yyyy-MM-dd} to {dto.PeriodEndDate:yyyy-MM-dd}",
                ReportData = System.Text.Json.JsonSerializer.Serialize(trialBalanceData),
                TotalDebit = trialBalanceData.TotalDebit,
                TotalCredit = trialBalanceData.TotalCredit,
                Currency = dto.Currency ?? "SAR",
                GeneratedAt = DateTime.Now,
                GeneratedBy = generatedByGuid,
                CompanyID = dto.CompanyId
            };

            _reportRepository.CreateReport(report);
            return MapToDto(report);
        }

        public async Task<FinancialReportResponseDto> GenerateTrialBalanceAsync(GenerateReportDto dto, string generatedBy)
        {
            var trialBalanceData = await GetTrialBalanceDataAsync(dto.PeriodStartDate, dto.PeriodEndDate, dto.CompanyId);

            Guid? generatedByGuid = null;
            if (Guid.TryParse(generatedBy, out Guid parsedGuid))
                generatedByGuid = parsedGuid;

            var report = new GLFinancialReport
            {
                Id = Guid.NewGuid(),
                ReportType = ReportType.TrialBalance,
                ReportNumber = await _reportRepository.GenerateReportNumberAsync(ReportType.TrialBalance, null, dto.FiscalYear),
                PeriodStartDate = dto.PeriodStartDate,
                PeriodEndDate = dto.PeriodEndDate,
                FiscalYear = dto.FiscalYear ?? dto.PeriodStartDate.Year,
                DescriptionAr = dto.DescriptionAr ?? $"ميزان المراجعة للفترة من {dto.PeriodStartDate:yyyy-MM-dd} إلى {dto.PeriodEndDate:yyyy-MM-dd}",
                DescriptionEn = dto.DescriptionEn ?? $"Trial Balance for period {dto.PeriodStartDate:yyyy-MM-dd} to {dto.PeriodEndDate:yyyy-MM-dd}",
                ReportData = System.Text.Json.JsonSerializer.Serialize(trialBalanceData),
                TotalDebit = trialBalanceData.TotalDebit,
                TotalCredit = trialBalanceData.TotalCredit,
                Currency = dto.Currency ?? "SAR",
                GeneratedAt = DateTime.Now,
                GeneratedBy = generatedByGuid,
                CompanyID = dto.CompanyId
            };

            await _reportRepository.CreateReportAsync(report);
            return MapToDto(report);
        }

        public FinancialReportResponseDto GenerateBalanceSheet(GenerateReportDto dto, string generatedBy)
        {
            var balanceSheetData = GetBalanceSheetData(dto.PeriodEndDate, dto.CompanyId);

            Guid? generatedByGuid = null;
            if (Guid.TryParse(generatedBy, out Guid parsedGuid))
                generatedByGuid = parsedGuid;

            var report = new GLFinancialReport
            {
                Id = Guid.NewGuid(),
                ReportType = ReportType.BalanceSheet,
                ReportNumber = _reportRepository.GenerateReportNumber(ReportType.BalanceSheet, null, dto.FiscalYear),
                PeriodStartDate = dto.PeriodStartDate,
                PeriodEndDate = dto.PeriodEndDate,
                FiscalYear = dto.FiscalYear ?? dto.PeriodEndDate.Year,
                DescriptionAr = dto.DescriptionAr ?? $"الميزانية العمومية كما في {dto.PeriodEndDate:yyyy-MM-dd}",
                DescriptionEn = dto.DescriptionEn ?? $"Balance Sheet as of {dto.PeriodEndDate:yyyy-MM-dd}",
                ReportData = System.Text.Json.JsonSerializer.Serialize(balanceSheetData),
                TotalDebit = balanceSheetData.TotalAssets,
                TotalCredit = balanceSheetData.TotalLiabilities + balanceSheetData.TotalEquity,
                Currency = dto.Currency ?? "SAR",
                GeneratedAt = DateTime.Now,
                GeneratedBy = generatedByGuid,
                CompanyID = dto.CompanyId
            };

            _reportRepository.CreateReport(report);
            return MapToDto(report);
        }

        public async Task<FinancialReportResponseDto> GenerateBalanceSheetAsync(GenerateReportDto dto, string generatedBy)
        {
            var balanceSheetData = await GetBalanceSheetDataAsync(dto.PeriodEndDate, dto.CompanyId);

            Guid? generatedByGuid = null;
            if (Guid.TryParse(generatedBy, out Guid parsedGuid))
                generatedByGuid = parsedGuid;

            var report = new GLFinancialReport
            {
                Id = Guid.NewGuid(),
                ReportType = ReportType.BalanceSheet,
                ReportNumber = await _reportRepository.GenerateReportNumberAsync(ReportType.BalanceSheet, null, dto.FiscalYear),
                PeriodStartDate = dto.PeriodStartDate,
                PeriodEndDate = dto.PeriodEndDate,
                FiscalYear = dto.FiscalYear ?? dto.PeriodEndDate.Year,
                DescriptionAr = dto.DescriptionAr ?? $"الميزانية العمومية كما في {dto.PeriodEndDate:yyyy-MM-dd}",
                DescriptionEn = dto.DescriptionEn ?? $"Balance Sheet as of {dto.PeriodEndDate:yyyy-MM-dd}",
                ReportData = System.Text.Json.JsonSerializer.Serialize(balanceSheetData),
                TotalDebit = balanceSheetData.TotalAssets,
                TotalCredit = balanceSheetData.TotalLiabilities + balanceSheetData.TotalEquity,
                Currency = dto.Currency ?? "SAR",
                GeneratedAt = DateTime.Now,
                GeneratedBy = generatedByGuid,
                CompanyID = dto.CompanyId
            };

            await _reportRepository.CreateReportAsync(report);
            return MapToDto(report);
        }

        public FinancialReportResponseDto GenerateIncomeStatement(GenerateReportDto dto, string generatedBy)
        {
            var incomeData = GetIncomeStatementData(dto.PeriodStartDate, dto.PeriodEndDate, dto.CompanyId);

            Guid? generatedByGuid = null;
            if (Guid.TryParse(generatedBy, out Guid parsedGuid))
                generatedByGuid = parsedGuid;

            var report = new GLFinancialReport
            {
                Id = Guid.NewGuid(),
                ReportType = ReportType.IncomeStatement,
                ReportNumber = _reportRepository.GenerateReportNumber(ReportType.IncomeStatement, null, dto.FiscalYear),
                PeriodStartDate = dto.PeriodStartDate,
                PeriodEndDate = dto.PeriodEndDate,
                FiscalYear = dto.FiscalYear ?? dto.PeriodStartDate.Year,
                DescriptionAr = dto.DescriptionAr ?? $"قائمة الدخل للفترة من {dto.PeriodStartDate:yyyy-MM-dd} إلى {dto.PeriodEndDate:yyyy-MM-dd}",
                DescriptionEn = dto.DescriptionEn ?? $"Income Statement for period {dto.PeriodStartDate:yyyy-MM-dd} to {dto.PeriodEndDate:yyyy-MM-dd}",
                ReportData = System.Text.Json.JsonSerializer.Serialize(incomeData),
                TotalDebit = incomeData.TotalExpenses,
                TotalCredit = incomeData.TotalRevenue,
                Currency = dto.Currency ?? "SAR",
                GeneratedAt = DateTime.Now,
                GeneratedBy = generatedByGuid,
                CompanyID = dto.CompanyId
            };

            _reportRepository.CreateReport(report);
            return MapToDto(report);
        }

        public async Task<FinancialReportResponseDto> GenerateIncomeStatementAsync(GenerateReportDto dto, string generatedBy)
        {
            var incomeData = await GetIncomeStatementDataAsync(dto.PeriodStartDate, dto.PeriodEndDate, dto.CompanyId);

            Guid? generatedByGuid = null;
            if (Guid.TryParse(generatedBy, out Guid parsedGuid))
                generatedByGuid = parsedGuid;

            var report = new GLFinancialReport
            {
                Id = Guid.NewGuid(),
                ReportType = ReportType.IncomeStatement,
                ReportNumber = await _reportRepository.GenerateReportNumberAsync(ReportType.IncomeStatement, null, dto.FiscalYear),
                PeriodStartDate = dto.PeriodStartDate,
                PeriodEndDate = dto.PeriodEndDate,
                FiscalYear = dto.FiscalYear ?? dto.PeriodStartDate.Year,
                DescriptionAr = dto.DescriptionAr ?? $"قائمة الدخل للفترة من {dto.PeriodStartDate:yyyy-MM-dd} إلى {dto.PeriodEndDate:yyyy-MM-dd}",
                DescriptionEn = dto.DescriptionEn ?? $"Income Statement for period {dto.PeriodStartDate:yyyy-MM-dd} to {dto.PeriodEndDate:yyyy-MM-dd}",
                ReportData = System.Text.Json.JsonSerializer.Serialize(incomeData),
                TotalDebit = incomeData.TotalExpenses,
                TotalCredit = incomeData.TotalRevenue,
                Currency = dto.Currency ?? "SAR",
                GeneratedAt = DateTime.Now,
                GeneratedBy = generatedByGuid,
                CompanyID = dto.CompanyId
            };

            await _reportRepository.CreateReportAsync(report);
            return MapToDto(report);
        }

        public FinancialReportResponseDto GenerateCashFlow(GenerateReportDto dto, string generatedBy)
        {
            // Simplified cash flow - get cash account movements
            var cashFlowData = GetCashFlowData(dto.PeriodStartDate, dto.PeriodEndDate, dto.CompanyId);

            Guid? generatedByGuid = null;
            if (Guid.TryParse(generatedBy, out Guid parsedGuid))
                generatedByGuid = parsedGuid;

            var report = new GLFinancialReport
            {
                Id = Guid.NewGuid(),
                ReportType = ReportType.CashFlow,
                ReportNumber = _reportRepository.GenerateReportNumber(ReportType.CashFlow, null, dto.FiscalYear),
                PeriodStartDate = dto.PeriodStartDate,
                PeriodEndDate = dto.PeriodEndDate,
                FiscalYear = dto.FiscalYear ?? dto.PeriodStartDate.Year,
                DescriptionAr = dto.DescriptionAr ?? $"قائمة التدفقات النقدية للفترة من {dto.PeriodStartDate:yyyy-MM-dd} إلى {dto.PeriodEndDate:yyyy-MM-dd}",
                DescriptionEn = dto.DescriptionEn ?? $"Cash Flow Statement for period {dto.PeriodStartDate:yyyy-MM-dd} to {dto.PeriodEndDate:yyyy-MM-dd}",
                ReportData = System.Text.Json.JsonSerializer.Serialize(cashFlowData),
                TotalDebit = cashFlowData.TotalInflows,
                TotalCredit = cashFlowData.TotalOutflows,
                Currency = dto.Currency ?? "SAR",
                GeneratedAt = DateTime.Now,
                GeneratedBy = generatedByGuid,
                CompanyID = dto.CompanyId
            };

            _reportRepository.CreateReport(report);
            return MapToDto(report);
        }

        public async Task<FinancialReportResponseDto> GenerateCashFlowAsync(GenerateReportDto dto, string generatedBy)
        {
            var cashFlowData = await GetCashFlowDataAsync(dto.PeriodStartDate, dto.PeriodEndDate, dto.CompanyId);

            Guid? generatedByGuid = null;
            if (Guid.TryParse(generatedBy, out Guid parsedGuid))
                generatedByGuid = parsedGuid;

            var report = new GLFinancialReport
            {
                Id = Guid.NewGuid(),
                ReportType = ReportType.CashFlow,
                ReportNumber = await _reportRepository.GenerateReportNumberAsync(ReportType.CashFlow, null, dto.FiscalYear),
                PeriodStartDate = dto.PeriodStartDate,
                PeriodEndDate = dto.PeriodEndDate,
                FiscalYear = dto.FiscalYear ?? dto.PeriodStartDate.Year,
                DescriptionAr = dto.DescriptionAr ?? $"قائمة التدفقات النقدية للفترة من {dto.PeriodStartDate:yyyy-MM-dd} إلى {dto.PeriodEndDate:yyyy-MM-dd}",
                DescriptionEn = dto.DescriptionEn ?? $"Cash Flow Statement for period {dto.PeriodStartDate:yyyy-MM-dd} to {dto.PeriodEndDate:yyyy-MM-dd}",
                ReportData = System.Text.Json.JsonSerializer.Serialize(cashFlowData),
                TotalDebit = cashFlowData.TotalInflows,
                TotalCredit = cashFlowData.TotalOutflows,
                Currency = dto.Currency ?? "SAR",
                GeneratedAt = DateTime.Now,
                GeneratedBy = generatedByGuid,
                CompanyID = dto.CompanyId
            };

            await _reportRepository.CreateReportAsync(report);
            return MapToDto(report);
        }

        public int Delete(Guid id) => _reportRepository.DeleteReport(id);

        public async Task<int> DeleteAsync(Guid id) => await _reportRepository.DeleteReportAsync(id);

        public TrialBalanceReportDto GetTrialBalanceData(DateTime periodStart, DateTime periodEnd, long? companyId = null)
        {
            // Delegate to enhanced method with default query
            var query = new TrialBalanceQueryDto
            {
                PeriodStartDate = periodStart,
                PeriodEndDate = periodEnd,
                CompanyId = companyId,
                ShowZeroBalances = false,
                ShowHierarchy = false,
                IncludeInactive = false
            };
            return GetEnhancedTrialBalance(query);
        }

        public async Task<TrialBalanceReportDto> GetTrialBalanceDataAsync(DateTime periodStart, DateTime periodEnd, long? companyId = null)
        {
            // Delegate to enhanced method with default query
            var query = new TrialBalanceQueryDto
            {
                PeriodStartDate = periodStart,
                PeriodEndDate = periodEnd,
                CompanyId = companyId,
                ShowZeroBalances = false,
                ShowHierarchy = false,
                IncludeInactive = false
            };
            return await GetEnhancedTrialBalanceAsync(query);
        }

        #region Enhanced Trial Balance Methods - طرق ميزان المراجعة المحسنة

        /// <summary>
        /// Get Enhanced Trial Balance with IFRS-compliant structure
        /// Supports two modes: "hierarchical" (tree with roll-ups) and "flat" (no hierarchy)
        /// </summary>
        public TrialBalanceReportDto GetEnhancedTrialBalance(TrialBalanceQueryDto query)
        {
            // Determine view mode (default to hierarchical for backward compatibility)
            bool isHierarchicalMode = string.IsNullOrEmpty(query.ViewMode) ||
                                      query.ViewMode.Equals("hierarchical", StringComparison.OrdinalIgnoreCase);

            // Get all accounts (filter by company if specified)
            var allAccounts = _accountRepository.ListAll(null)
                .Where(a => query.IncludeInactive || a.IsActive)
                .Where(a => !query.AccountType.HasValue || a.AccountType == query.AccountType.Value)
                .ToList();

            // Apply level filter only if not showing hierarchy (hierarchy needs all levels)
            var accounts = isHierarchicalMode
                ? allAccounts
                : allAccounts.Where(a => !query.Level.HasValue || a.Level == query.Level.Value).ToList();

            // Create account lookup for quick parent resolution
            var accountLookup = allAccounts.ToDictionary(a => a.AccountId);

            // Get trial balance data efficiently (opening + period in two queries)
            var balanceData = _entryRepository.GetTrialBalanceData(
                query.PeriodStartDate,
                query.PeriodEndDate,
                query.CompanyId);

            // Build flat list of lines with all data
            var lines = new List<TrialBalanceLineDto>();
            var accountsWithActivity = new HashSet<Guid>();

            // First pass: identify all accounts with activity
            foreach (var account in accounts)
            {
                var data = balanceData.ContainsKey(account.AccountId)
                    ? balanceData[account.AccountId]
                    : null;

                if (data != null && data.HasActivity)
                {
                    accountsWithActivity.Add(account.AccountId);
                }
            }

            // Second pass: if hierarchical mode, also include parent accounts of those with activity
            if (isHierarchicalMode && !query.ShowZeroBalances)
            {
                var parentsToAdd = new HashSet<Guid>();
                foreach (var accountId in accountsWithActivity)
                {
                    // Walk up the parent chain
                    var currentId = accountId;
                    while (accountLookup.TryGetValue(currentId, out var acc) && acc.ParentAccountId.HasValue && acc.ParentAccountId != Guid.Empty)
                    {
                        parentsToAdd.Add(acc.ParentAccountId.Value);
                        currentId = acc.ParentAccountId.Value;
                    }
                }
                // Add parent accounts to the activity set
                foreach (var parentId in parentsToAdd)
                {
                    accountsWithActivity.Add(parentId);
                }
            }

            // Third pass: build the lines
            foreach (var account in accounts)
            {
                var data = balanceData.ContainsKey(account.AccountId)
                    ? balanceData[account.AccountId]
                    : new TrialBalanceAccountData { AccountId = account.AccountId };

                // Skip zero balances if not requested
                // In hierarchical mode, keep parent accounts with children activity
                // In flat mode, only show accounts with direct activity or if showZeroBalances is true
                if (!query.ShowZeroBalances && !data.HasActivity)
                {
                    if (isHierarchicalMode)
                    {
                        // In hierarchical mode, keep parents with active children
                        if (!accountsWithActivity.Contains(account.AccountId))
                            continue;
                    }
                    else
                    {
                        // In flat mode, skip accounts without direct activity
                        continue;
                    }
                }

                var line = new TrialBalanceLineDto
                {
                    AccountId = account.AccountId,
                    AccountCode = account.AccountNumber,
                    FullAccountNumber = account.FullAccountNumber,
                    AccountNameAr = account.AccountNameAr,
                    AccountNameEn = account.AccountNameEn,
                    AccountType = account.AccountType.ToString(),
                    AccountTypeEnum = account.AccountType,
                    Level = account.Level,
                    ParentAccountId = account.ParentAccountId,
                    IsLeaf = account.IsLeaf,
                    NormalBalance = account.NormalBalance,
                    IsActive = account.IsActive,
                    Currency = query.Currency ?? "SAR",
                    CompanyId = query.CompanyId,

                    // Opening Balance (own balance only - roll-up happens in BuildHierarchicalTrialBalance)
                    OpeningDebit = data.OpeningDebit,
                    OpeningCredit = data.OpeningCredit,

                    // Period Movement (own balance only)
                    PeriodDebit = data.PeriodDebit,
                    PeriodCredit = data.PeriodCredit,

                    // Closing Balance (own balance only)
                    ClosingDebit = data.ClosingDebit,
                    ClosingCredit = data.ClosingCredit
                };

                lines.Add(line);
            }

            // Build result based on view mode
            List<TrialBalanceLineDto> resultLines;
            if (isHierarchicalMode)
            {
                // Hierarchical mode: build tree structure with roll-up totals
                resultLines = BuildHierarchicalTrialBalance(lines, query.ParentAccountId);

                // Fallback: if hierarchy building failed but we have data, return flat list
                if (resultLines.Count == 0 && lines.Count > 0)
                {
                    resultLines = lines.OrderBy(l => l.FullAccountNumber ?? l.AccountCode).ToList();
                }
            }
            else
            {
                // Flat mode: simple flat list without hierarchy or roll-ups
                // Each account shows only its own balance
                resultLines = lines
                    .OrderBy(l => l.FullAccountNumber ?? l.AccountCode)
                    .Select(l => new TrialBalanceLineDto
                    {
                        AccountId = l.AccountId,
                        AccountCode = l.AccountCode,
                        FullAccountNumber = l.FullAccountNumber,
                        AccountNameAr = l.AccountNameAr,
                        AccountNameEn = l.AccountNameEn,
                        AccountType = l.AccountType,
                        AccountTypeEnum = l.AccountTypeEnum,
                        Level = l.Level,
                        ParentAccountId = null, // Clear parent reference in flat mode
                        IsLeaf = l.IsLeaf,
                        NormalBalance = l.NormalBalance,
                        IsActive = l.IsActive,
                        Currency = l.Currency,
                        CompanyId = l.CompanyId,
                        Children = null, // No children in flat mode
                        IsExpanded = false,
                        // Own balances only (no roll-up)
                        OpeningDebit = l.OpeningDebit,
                        OpeningCredit = l.OpeningCredit,
                        PeriodDebit = l.PeriodDebit,
                        PeriodCredit = l.PeriodCredit,
                        ClosingDebit = l.ClosingDebit,
                        ClosingCredit = l.ClosingCredit
                    })
                    .ToList();
            }

            // Calculate totals from leaf accounts only (to avoid double counting)
            var leafLines = lines.Where(l => l.IsLeaf).ToList();

            return new TrialBalanceReportDto
            {
                PeriodStartDate = query.PeriodStartDate,
                PeriodEndDate = query.PeriodEndDate,
                FiscalYear = query.PeriodStartDate.Year,
                Currency = query.Currency ?? "SAR",
                CompanyId = query.CompanyId,
                GeneratedAt = DateTime.Now,
                ViewMode = isHierarchicalMode ? "hierarchical" : "flat",

                // Opening totals
                TotalOpeningDebit = leafLines.Sum(l => l.OpeningDebit),
                TotalOpeningCredit = leafLines.Sum(l => l.OpeningCredit),

                // Period totals
                TotalPeriodDebit = leafLines.Sum(l => l.PeriodDebit),
                TotalPeriodCredit = leafLines.Sum(l => l.PeriodCredit),

                // Closing totals
                TotalClosingDebit = leafLines.Sum(l => l.ClosingDebit),
                TotalClosingCredit = leafLines.Sum(l => l.ClosingCredit),

                Lines = resultLines
            };
        }

        /// <summary>
        /// Get Enhanced Trial Balance async
        /// Supports two modes: "hierarchical" (tree with roll-ups) and "flat" (no hierarchy)
        /// </summary>
        public async Task<TrialBalanceReportDto> GetEnhancedTrialBalanceAsync(TrialBalanceQueryDto query)
        {
            // Determine view mode (default to hierarchical for backward compatibility)
            bool isHierarchicalMode = string.IsNullOrEmpty(query.ViewMode) ||
                                      query.ViewMode.Equals("hierarchical", StringComparison.OrdinalIgnoreCase);

            // Get all accounts (filter by company if specified)
            var allAccounts = (await _accountRepository.ListAllAsync(null))
                .Where(a => query.IncludeInactive || a.IsActive)
                .Where(a => !query.AccountType.HasValue || a.AccountType == query.AccountType.Value)
                .ToList();

            // Apply level filter only if not showing hierarchy (hierarchy needs all levels)
            var accounts = isHierarchicalMode
                ? allAccounts
                : allAccounts.Where(a => !query.Level.HasValue || a.Level == query.Level.Value).ToList();

            // Create account lookup for quick parent resolution
            var accountLookup = allAccounts.ToDictionary(a => a.AccountId);

            // Get trial balance data efficiently (opening + period in two queries)
            var balanceData = await _entryRepository.GetTrialBalanceDataAsync(
                query.PeriodStartDate,
                query.PeriodEndDate,
                query.CompanyId);

            // Build flat list of lines with all data
            var lines = new List<TrialBalanceLineDto>();
            var accountsWithActivity = new HashSet<Guid>();

            // First pass: identify all accounts with activity
            foreach (var account in accounts)
            {
                var data = balanceData.ContainsKey(account.AccountId)
                    ? balanceData[account.AccountId]
                    : null;

                if (data != null && data.HasActivity)
                {
                    accountsWithActivity.Add(account.AccountId);
                }
            }

            // Second pass: if hierarchical mode, also include parent accounts of those with activity
            if (isHierarchicalMode && !query.ShowZeroBalances)
            {
                var parentsToAdd = new HashSet<Guid>();
                foreach (var accountId in accountsWithActivity)
                {
                    // Walk up the parent chain
                    var currentId = accountId;
                    while (accountLookup.TryGetValue(currentId, out var acc) && acc.ParentAccountId.HasValue && acc.ParentAccountId != Guid.Empty)
                    {
                        parentsToAdd.Add(acc.ParentAccountId.Value);
                        currentId = acc.ParentAccountId.Value;
                    }
                }
                // Add parent accounts to the activity set
                foreach (var parentId in parentsToAdd)
                {
                    accountsWithActivity.Add(parentId);
                }
            }

            // Third pass: build the lines
            foreach (var account in accounts)
            {
                var data = balanceData.ContainsKey(account.AccountId)
                    ? balanceData[account.AccountId]
                    : new TrialBalanceAccountData { AccountId = account.AccountId };

                // Skip zero balances if not requested
                if (!query.ShowZeroBalances && !data.HasActivity)
                {
                    if (isHierarchicalMode)
                    {
                        if (!accountsWithActivity.Contains(account.AccountId))
                            continue;
                    }
                    else
                    {
                        continue;
                    }
                }

                var line = new TrialBalanceLineDto
                {
                    AccountId = account.AccountId,
                    AccountCode = account.AccountNumber,
                    FullAccountNumber = account.FullAccountNumber,
                    AccountNameAr = account.AccountNameAr,
                    AccountNameEn = account.AccountNameEn,
                    AccountType = account.AccountType.ToString(),
                    AccountTypeEnum = account.AccountType,
                    Level = account.Level,
                    ParentAccountId = account.ParentAccountId,
                    IsLeaf = account.IsLeaf,
                    NormalBalance = account.NormalBalance,
                    IsActive = account.IsActive,
                    Currency = query.Currency ?? "SAR",
                    CompanyId = query.CompanyId,

                    // Opening Balance (own balance only)
                    OpeningDebit = data.OpeningDebit,
                    OpeningCredit = data.OpeningCredit,

                    // Period Movement (own balance only)
                    PeriodDebit = data.PeriodDebit,
                    PeriodCredit = data.PeriodCredit,

                    // Closing Balance (own balance only)
                    ClosingDebit = data.ClosingDebit,
                    ClosingCredit = data.ClosingCredit
                };

                lines.Add(line);
            }

            // Build result based on view mode
            List<TrialBalanceLineDto> resultLines;
            if (isHierarchicalMode)
            {
                resultLines = BuildHierarchicalTrialBalance(lines, query.ParentAccountId);

                if (resultLines.Count == 0 && lines.Count > 0)
                {
                    resultLines = lines.OrderBy(l => l.FullAccountNumber ?? l.AccountCode).ToList();
                }
            }
            else
            {
                // Flat mode: simple flat list without hierarchy or roll-ups
                resultLines = lines
                    .OrderBy(l => l.FullAccountNumber ?? l.AccountCode)
                    .Select(l => new TrialBalanceLineDto
                    {
                        AccountId = l.AccountId,
                        AccountCode = l.AccountCode,
                        FullAccountNumber = l.FullAccountNumber,
                        AccountNameAr = l.AccountNameAr,
                        AccountNameEn = l.AccountNameEn,
                        AccountType = l.AccountType,
                        AccountTypeEnum = l.AccountTypeEnum,
                        Level = l.Level,
                        ParentAccountId = null,
                        IsLeaf = l.IsLeaf,
                        NormalBalance = l.NormalBalance,
                        IsActive = l.IsActive,
                        Currency = l.Currency,
                        CompanyId = l.CompanyId,
                        Children = null,
                        IsExpanded = false,
                        OpeningDebit = l.OpeningDebit,
                        OpeningCredit = l.OpeningCredit,
                        PeriodDebit = l.PeriodDebit,
                        PeriodCredit = l.PeriodCredit,
                        ClosingDebit = l.ClosingDebit,
                        ClosingCredit = l.ClosingCredit
                    })
                    .ToList();
            }

            // Calculate totals from leaf accounts only
            var leafLines = lines.Where(l => l.IsLeaf).ToList();

            return new TrialBalanceReportDto
            {
                PeriodStartDate = query.PeriodStartDate,
                PeriodEndDate = query.PeriodEndDate,
                FiscalYear = query.PeriodStartDate.Year,
                Currency = query.Currency ?? "SAR",
                CompanyId = query.CompanyId,
                GeneratedAt = DateTime.Now,
                ViewMode = isHierarchicalMode ? "hierarchical" : "flat",

                TotalOpeningDebit = leafLines.Sum(l => l.OpeningDebit),
                TotalOpeningCredit = leafLines.Sum(l => l.OpeningCredit),
                TotalPeriodDebit = leafLines.Sum(l => l.PeriodDebit),
                TotalPeriodCredit = leafLines.Sum(l => l.PeriodCredit),
                TotalClosingDebit = leafLines.Sum(l => l.ClosingDebit),
                TotalClosingCredit = leafLines.Sum(l => l.ClosingCredit),

                Lines = resultLines
            };
        }

        /// <summary>
        /// Build hierarchical trial balance structure from flat list
        /// </summary>
        private List<TrialBalanceLineDto> BuildHierarchicalTrialBalance(
            List<TrialBalanceLineDto> flatLines,
            Guid? rootParentId = null)
        {
            // Create a lookup dictionary
            var lookup = flatLines.ToDictionary(l => l.AccountId);

            // Find root nodes (no parent or specified parent)
            // Check for both null and Guid.Empty as root-level accounts may have either
            var rootNodes = flatLines
                .Where(l => rootParentId.HasValue
                    ? l.ParentAccountId == rootParentId.Value
                    : (l.ParentAccountId == null || l.ParentAccountId == Guid.Empty))
                .OrderBy(l => l.FullAccountNumber ?? l.AccountCode)
                .ToList();

            // Build tree recursively
            foreach (var root in rootNodes)
            {
                BuildChildren(root, flatLines, lookup);
            }

            return rootNodes;
        }

        /// <summary>
        /// Recursively build children for a parent node and roll up totals
        /// </summary>
        private void BuildChildren(
            TrialBalanceLineDto parent,
            List<TrialBalanceLineDto> allLines,
            Dictionary<Guid, TrialBalanceLineDto> lookup)
        {
            // Find children
            var children = allLines
                .Where(l => l.ParentAccountId == parent.AccountId)
                .OrderBy(l => l.FullAccountNumber ?? l.AccountCode)
                .ToList();

            parent.Children = children;

            // Recursively build grandchildren
            foreach (var child in children)
            {
                BuildChildren(child, allLines, lookup);
            }

            // Roll up totals from children if this is a parent account (not a leaf)
            if (children.Any())
            {
                // Sum up all descendants' leaf values
                var leafDescendants = GetLeafDescendants(parent);

                parent.OpeningDebit = leafDescendants.Sum(l => l.OpeningDebit);
                parent.OpeningCredit = leafDescendants.Sum(l => l.OpeningCredit);
                parent.PeriodDebit = leafDescendants.Sum(l => l.PeriodDebit);
                parent.PeriodCredit = leafDescendants.Sum(l => l.PeriodCredit);
                parent.ClosingDebit = leafDescendants.Sum(l => l.ClosingDebit);
                parent.ClosingCredit = leafDescendants.Sum(l => l.ClosingCredit);
            }
        }

        /// <summary>
        /// Get all leaf descendants of a parent node
        /// </summary>
        private List<TrialBalanceLineDto> GetLeafDescendants(TrialBalanceLineDto parent)
        {
            var leaves = new List<TrialBalanceLineDto>();

            if (parent.Children == null || !parent.Children.Any())
            {
                // This is a leaf
                leaves.Add(parent);
            }
            else
            {
                // Recursively get leaves from children
                foreach (var child in parent.Children)
                {
                    leaves.AddRange(GetLeafDescendants(child));
                }
            }

            return leaves;
        }

        #endregion

        #region Public Data Methods (without saving to database)

        /// <summary>
        /// Get Balance Sheet data (without saving) - الحصول على بيانات الميزانية العمومية
        /// </summary>
        public BalanceSheetReportDto GetBalanceSheetData(DateTime asOfDate, long? companyId = null)
        {
            // Get fiscal year configuration for the asOfDate
            var fiscalYear = _fiscalYearService.GetFiscalYearByDate(companyId, asOfDate);

            // For balance sheet accounts (permanent accounts: Assets, Liabilities, Equity),
            // we need cumulative balances from the earliest fiscal year, not just current year.
            // This ensures prior year balances are carried forward correctly.
            DateTime balanceStartDate;
            int fiscalYearNumber;

            if (fiscalYear != null)
            {
                // Use fiscal year configuration
                fiscalYearNumber = fiscalYear.YearStartDate.Year;

                // For balance sheet, get cumulative balances from the earliest fiscal year
                var earliestFiscalYear = _fiscalYearService.GetEarliestFiscalYear(companyId);
                balanceStartDate = earliestFiscalYear?.YearStartDate ?? fiscalYear.YearStartDate;
            }
            else
            {
                // Fallback: if no fiscal year configuration exists, use calendar year start
                // This maintains backward compatibility but is not recommended
                balanceStartDate = new DateTime(asOfDate.Year, 1, 1);
                fiscalYearNumber = asOfDate.Year;
            }

            var accounts = _accountRepository.ListAll(null);
            var assets = new List<BalanceSheetSectionDto>();
            var liabilities = new List<BalanceSheetSectionDto>();
            var equity = new List<BalanceSheetSectionDto>();

            foreach (var account in accounts.Where(a => a.IsActive))
            {
                // Balance sheet accounts get cumulative balance from earliest fiscal year
                var balance = _entryRepository.GetAccountBalance(account.AccountId, balanceStartDate, asOfDate);
                // Use normalBalance-aware calculation for correct balance direction
                var netBalance = _normalBalanceResolver.CalculateBalance(
                    balance.TotalDebit,
                    balance.TotalCredit,
                    account.NormalBalance);

                if (netBalance == 0) continue;

                var section = new BalanceSheetSectionDto
                {
                    AccountId = account.AccountId,
                    AccountCode = account.AccountNumber,
                    AccountNameAr = account.AccountNameAr,
                    AccountNameEn = account.AccountNameEn,
                    Level = account.Level,
                    // Use normalized balance - positive indicates normal direction
                    Balance = netBalance
                };

                switch (account.AccountType)
                {
                    case AccountType.Asset:
                    case AccountType.ContraAsset:
                        assets.Add(section);
                        break;
                    case AccountType.Liability:
                    case AccountType.ContraLiability:
                        liabilities.Add(section);
                        break;
                    case AccountType.Equity:
                    case AccountType.ContraEquity:
                        equity.Add(section);
                        break;
                }
            }

            return new BalanceSheetReportDto
            {
                AsOfDate = asOfDate,
                FiscalYear = fiscalYearNumber,
                Currency = "SAR",
                TotalAssets = assets.Sum(a => a.Balance),
                TotalLiabilities = liabilities.Sum(l => l.Balance),
                TotalEquity = equity.Sum(e => e.Balance),
                Assets = assets.OrderBy(a => a.AccountCode).ToList(),
                Liabilities = liabilities.OrderBy(l => l.AccountCode).ToList(),
                Equity = equity.OrderBy(e => e.AccountCode).ToList()
            };
        }

        /// <summary>
        /// Get Balance Sheet data async - الحصول على بيانات الميزانية العمومية (غير متزامن)
        /// </summary>
        public async Task<BalanceSheetReportDto> GetBalanceSheetDataAsync(DateTime asOfDate, long? companyId = null)
        {
            // Get fiscal year configuration for the asOfDate
            var fiscalYear = _fiscalYearService.GetFiscalYearByDate(companyId, asOfDate);

            // For balance sheet accounts (permanent accounts: Assets, Liabilities, Equity),
            // we need cumulative balances from the earliest fiscal year, not just current year.
            // This ensures prior year balances are carried forward correctly.
            DateTime balanceStartDate;
            int fiscalYearNumber;

            if (fiscalYear != null)
            {
                // Use fiscal year configuration
                fiscalYearNumber = fiscalYear.YearStartDate.Year;

                // For balance sheet, get cumulative balances from the earliest fiscal year
                var earliestFiscalYear = _fiscalYearService.GetEarliestFiscalYear(companyId);
                balanceStartDate = earliestFiscalYear?.YearStartDate ?? fiscalYear.YearStartDate;
            }
            else
            {
                // Fallback: if no fiscal year configuration exists, use calendar year start
                // This maintains backward compatibility but is not recommended
                balanceStartDate = new DateTime(asOfDate.Year, 1, 1);
                fiscalYearNumber = asOfDate.Year;
            }

            var accounts = await _accountRepository.ListAllAsync(null);
            var assets = new List<BalanceSheetSectionDto>();
            var liabilities = new List<BalanceSheetSectionDto>();
            var equity = new List<BalanceSheetSectionDto>();

            foreach (var account in accounts.Where(a => a.IsActive))
            {
                // Balance sheet accounts get cumulative balance from earliest fiscal year
                var balance = await _entryRepository.GetAccountBalanceAsync(account.AccountId, balanceStartDate, asOfDate);
                // Use normalBalance-aware calculation for correct balance direction
                var netBalance = _normalBalanceResolver.CalculateBalance(
                    balance.TotalDebit,
                    balance.TotalCredit,
                    account.NormalBalance);

                if (netBalance == 0) continue;

                var section = new BalanceSheetSectionDto
                {
                    AccountId = account.AccountId,
                    AccountCode = account.AccountNumber,
                    AccountNameAr = account.AccountNameAr,
                    AccountNameEn = account.AccountNameEn,
                    Level = account.Level,
                    // Use normalized balance - positive indicates normal direction
                    Balance = netBalance
                };

                switch (account.AccountType)
                {
                    case AccountType.Asset:
                    case AccountType.ContraAsset:
                        assets.Add(section);
                        break;
                    case AccountType.Liability:
                    case AccountType.ContraLiability:
                        liabilities.Add(section);
                        break;
                    case AccountType.Equity:
                    case AccountType.ContraEquity:
                        equity.Add(section);
                        break;
                }
            }

            return new BalanceSheetReportDto
            {
                AsOfDate = asOfDate,
                FiscalYear = fiscalYearNumber,
                Currency = "SAR",
                TotalAssets = assets.Sum(a => a.Balance),
                TotalLiabilities = liabilities.Sum(l => l.Balance),
                TotalEquity = equity.Sum(e => e.Balance),
                Assets = assets.OrderBy(a => a.AccountCode).ToList(),
                Liabilities = liabilities.OrderBy(l => l.AccountCode).ToList(),
                Equity = equity.OrderBy(e => e.AccountCode).ToList()
            };
        }

        #region IFRS Balance Sheet - الميزانية العمومية وفق المعايير الدولية

        /// <summary>
        /// Get IFRS-compliant Balance Sheet data
        /// الحصول على بيانات الميزانية العمومية وفق المعايير الدولية (IAS 1)
        /// </summary>
        /// <param name="query">Query parameters including asOfDate, viewMode, etc.</param>
        /// <returns>IFRS-compliant balance sheet report</returns>
        public async Task<IFRSBalanceSheetReportDto> GetIFRSBalanceSheetAsync(BalanceSheetQueryDto query)
        {
            var asOfDate = query.AsOfDate;
            var companyId = query.CompanyId;
            var viewMode = query.ViewMode ?? "hierarchical";
            var isHierarchical = viewMode.Equals("hierarchical", StringComparison.OrdinalIgnoreCase);

            // Get fiscal year configuration
            var fiscalYear = _fiscalYearService.GetFiscalYearByDate(companyId, asOfDate);
            var earliestFiscalYear = _fiscalYearService.GetEarliestFiscalYear(companyId);
            var balanceStartDate = earliestFiscalYear?.YearStartDate ?? fiscalYear?.YearStartDate ?? new DateTime(asOfDate.Year, 1, 1);
            var fiscalYearNumber = fiscalYear?.YearStartDate.Year ?? asOfDate.Year;

            // Previous period date (same date last year)
            DateTime? previousPeriodDate = query.IncludePreviousPeriod
                ? asOfDate.AddYears(-1)
                : null;
            var previousBalanceStartDate = previousPeriodDate.HasValue
                ? balanceStartDate.AddYears(-1)
                : (DateTime?)null;

            // Get all active accounts
            var allAccounts = await _accountRepository.ListAllAsync(null);
            var activeAccounts = allAccounts.Where(a => a.IsActive).ToList();

            // Create account lookup for hierarchy building
            var accountLookup = activeAccounts.ToDictionary(a => a.AccountId);

            // Load opening balances for the fiscal year
            // الحصول على الأرصدة الافتتاحية للسنة المالية
            var openingBalances = await _openingBalanceRepository.GetByFiscalYearAsync(fiscalYearNumber);
            var openingBalanceLookup = openingBalances.ToDictionary(ob => ob.AccountId, ob => ob);

            // Load previous year opening balances if needed
            Dictionary<Guid, GLAccountOpeningBalance> prevYearOpeningBalanceLookup = null;
            if (query.IncludePreviousPeriod)
            {
                var prevYearOpeningBalances = await _openingBalanceRepository.GetByFiscalYearAsync(fiscalYearNumber - 1);
                prevYearOpeningBalanceLookup = prevYearOpeningBalances.ToDictionary(ob => ob.AccountId, ob => ob);
            }

            // Prepare category lists
            var currentAssetLines = new List<BalanceSheetLineDto>();
            var nonCurrentAssetLines = new List<BalanceSheetLineDto>();
            var currentLiabilityLines = new List<BalanceSheetLineDto>();
            var nonCurrentLiabilityLines = new List<BalanceSheetLineDto>();
            var equityLines = new List<BalanceSheetLineDto>();

            // Process each account
            foreach (var account in activeAccounts)
            {
                // Only process balance sheet accounts
                if (!IsBalanceSheetAccount(account.AccountType))
                    continue;

                // Get opening balance for this account (if exists)
                // الحصول على الرصيد الافتتاحي للحساب
                decimal openingBalance = 0;
                if (openingBalanceLookup.TryGetValue(account.AccountId, out var ob))
                {
                    // Opening balance is already stored as net (Debit - Credit)
                    // For debit-normal accounts (Assets), positive opening = debit balance
                    // For credit-normal accounts (Liabilities, Equity), we need to adjust sign
                    openingBalance = ob.OpeningBalance;
                }

                // Get period movements (debits and credits from journal entries)
                // الحصول على حركات الفترة من القيود المحاسبية
                var periodMovement = await _entryRepository.GetAccountBalanceAsync(account.AccountId, balanceStartDate, asOfDate);

                // Calculate closing balance:
                // Closing Balance = Opening Balance + Period Movement
                // Period Movement = Debits - Credits (adjusted for normal balance)
                // حساب الرصيد الختامي = الرصيد الافتتاحي + حركة الفترة
                var periodNetMovement = _normalBalanceResolver.CalculateBalance(
                    periodMovement.TotalDebit,
                    periodMovement.TotalCredit,
                    account.NormalBalance);

                var closingBalance = openingBalance + periodNetMovement;

                // Calculate previous period closing balance if requested
                decimal? previousClosingBalance = null;
                if (previousPeriodDate.HasValue && previousBalanceStartDate.HasValue)
                {
                    // Get previous year opening balance
                    decimal prevOpeningBalance = 0;
                    if (prevYearOpeningBalanceLookup != null && prevYearOpeningBalanceLookup.TryGetValue(account.AccountId, out var prevOb))
                    {
                        prevOpeningBalance = prevOb.OpeningBalance;
                    }

                    // Get previous year period movements
                    var prevPeriodMovement = await _entryRepository.GetAccountBalanceAsync(
                        account.AccountId, previousBalanceStartDate.Value, previousPeriodDate.Value);
                    var prevPeriodNetMovement = _normalBalanceResolver.CalculateBalance(
                        prevPeriodMovement.TotalDebit,
                        prevPeriodMovement.TotalCredit,
                        account.NormalBalance);

                    previousClosingBalance = prevOpeningBalance + prevPeriodNetMovement;
                }

                // In flat mode, skip zero balances
                if (!isHierarchical && closingBalance == 0 && (previousClosingBalance ?? 0) == 0)
                    continue;

                // Determine the balance sheet category
                var category = DetermineBalanceSheetCategory(account);

                // Create line item with closing balance only
                // إنشاء سطر بالرصيد الختامي فقط
                var line = new BalanceSheetLineDto
                {
                    AccountId = account.AccountId,
                    AccountCode = account.AccountNumber,
                    FullAccountNumber = BuildFullAccountNumber(account, accountLookup),
                    AccountNameAr = account.AccountNameAr,
                    AccountNameEn = account.AccountNameEn,
                    AccountType = account.AccountType.ToString(),
                    AccountTypeEnum = account.AccountType,
                    Level = account.Level,
                    ParentAccountId = account.ParentAccountId,
                    IsLeaf = account.IsLeaf,
                    Balance = closingBalance,
                    PreviousPeriodBalance = previousClosingBalance,
                    BalanceSheetCategory = category,
                    IsCurrent = category == "CurrentAssets" || category == "CurrentLiabilities"
                };

                // Add to appropriate category list
                switch (category)
                {
                    case "CurrentAssets":
                        currentAssetLines.Add(line);
                        break;
                    case "NonCurrentAssets":
                        nonCurrentAssetLines.Add(line);
                        break;
                    case "CurrentLiabilities":
                        currentLiabilityLines.Add(line);
                        break;
                    case "NonCurrentLiabilities":
                        nonCurrentLiabilityLines.Add(line);
                        break;
                    case "Equity":
                        equityLines.Add(line);
                        break;
                }
            }

            // Build hierarchy if needed
            if (isHierarchical)
            {
                currentAssetLines = BuildBalanceSheetHierarchy(currentAssetLines, accountLookup);
                nonCurrentAssetLines = BuildBalanceSheetHierarchy(nonCurrentAssetLines, accountLookup);
                currentLiabilityLines = BuildBalanceSheetHierarchy(currentLiabilityLines, accountLookup);
                nonCurrentLiabilityLines = BuildBalanceSheetHierarchy(nonCurrentLiabilityLines, accountLookup);
                equityLines = BuildBalanceSheetHierarchy(equityLines, accountLookup);
            }
            else
            {
                // In flat mode, sort by account code and remove hierarchy references
                currentAssetLines = currentAssetLines.OrderBy(l => l.AccountCode).ToList();
                nonCurrentAssetLines = nonCurrentAssetLines.OrderBy(l => l.AccountCode).ToList();
                currentLiabilityLines = currentLiabilityLines.OrderBy(l => l.AccountCode).ToList();
                nonCurrentLiabilityLines = nonCurrentLiabilityLines.OrderBy(l => l.AccountCode).ToList();
                equityLines = equityLines.OrderBy(l => l.AccountCode).ToList();

                // Clear parent references for flat mode
                foreach (var line in currentAssetLines.Concat(nonCurrentAssetLines)
                    .Concat(currentLiabilityLines).Concat(nonCurrentLiabilityLines).Concat(equityLines))
                {
                    line.ParentAccountId = null;
                    line.Children = null;
                }
            }

            // Calculate totals
            var currentAssetsTotal = CalculateCategoryTotal(currentAssetLines, isHierarchical);
            var nonCurrentAssetsTotal = CalculateCategoryTotal(nonCurrentAssetLines, isHierarchical);
            var currentLiabilitiesTotal = CalculateCategoryTotal(currentLiabilityLines, isHierarchical);
            var nonCurrentLiabilitiesTotal = CalculateCategoryTotal(nonCurrentLiabilityLines, isHierarchical);
            var equityTotal = CalculateCategoryTotal(equityLines, isHierarchical);

            // Calculate previous period totals if needed
            decimal? prevCurrentAssetsTotal = null, prevNonCurrentAssetsTotal = null;
            decimal? prevCurrentLiabilitiesTotal = null, prevNonCurrentLiabilitiesTotal = null;
            decimal? prevEquityTotal = null;

            if (query.IncludePreviousPeriod)
            {
                prevCurrentAssetsTotal = CalculatePreviousPeriodTotal(currentAssetLines, isHierarchical);
                prevNonCurrentAssetsTotal = CalculatePreviousPeriodTotal(nonCurrentAssetLines, isHierarchical);
                prevCurrentLiabilitiesTotal = CalculatePreviousPeriodTotal(currentLiabilityLines, isHierarchical);
                prevNonCurrentLiabilitiesTotal = CalculatePreviousPeriodTotal(nonCurrentLiabilityLines, isHierarchical);
                prevEquityTotal = CalculatePreviousPeriodTotal(equityLines, isHierarchical);
            }

            // Build the report
            var report = new IFRSBalanceSheetReportDto
            {
                AsOfDate = asOfDate,
                PreviousPeriodDate = previousPeriodDate,
                FiscalYear = fiscalYearNumber,
                Currency = query.Currency ?? "SAR",
                ViewMode = viewMode,
                CompanyId = companyId,
                GeneratedAt = DateTime.Now,

                CurrentAssets = new BalanceSheetCategoryDto
                {
                    CategoryNameEn = "Current Assets",
                    CategoryNameAr = "الأصول المتداولة",
                    Lines = currentAssetLines,
                    Total = currentAssetsTotal,
                    PreviousPeriodTotal = prevCurrentAssetsTotal
                },
                NonCurrentAssets = new BalanceSheetCategoryDto
                {
                    CategoryNameEn = "Non-Current Assets",
                    CategoryNameAr = "الأصول غير المتداولة",
                    Lines = nonCurrentAssetLines,
                    Total = nonCurrentAssetsTotal,
                    PreviousPeriodTotal = prevNonCurrentAssetsTotal
                },
                TotalAssets = currentAssetsTotal + nonCurrentAssetsTotal,

                CurrentLiabilities = new BalanceSheetCategoryDto
                {
                    CategoryNameEn = "Current Liabilities",
                    CategoryNameAr = "الالتزامات المتداولة",
                    Lines = currentLiabilityLines,
                    Total = currentLiabilitiesTotal,
                    PreviousPeriodTotal = prevCurrentLiabilitiesTotal
                },
                NonCurrentLiabilities = new BalanceSheetCategoryDto
                {
                    CategoryNameEn = "Non-Current Liabilities",
                    CategoryNameAr = "الالتزامات غير المتداولة",
                    Lines = nonCurrentLiabilityLines,
                    Total = nonCurrentLiabilitiesTotal,
                    PreviousPeriodTotal = prevNonCurrentLiabilitiesTotal
                },
                TotalLiabilities = currentLiabilitiesTotal + nonCurrentLiabilitiesTotal,

                Equity = new BalanceSheetCategoryDto
                {
                    CategoryNameEn = "Equity",
                    CategoryNameAr = "حقوق الملكية",
                    Lines = equityLines,
                    Total = equityTotal,
                    PreviousPeriodTotal = prevEquityTotal
                },
                TotalEquity = equityTotal
            };

            return report;
        }

        /// <summary>
        /// Determine if an account is a balance sheet account
        /// </summary>
        private bool IsBalanceSheetAccount(AccountType accountType)
        {
            return accountType switch
            {
                AccountType.Asset or AccountType.ContraAsset or
                AccountType.Liability or AccountType.ContraLiability or
                AccountType.Equity or AccountType.ContraEquity => true,
                _ => false
            };
        }

        /// <summary>
        /// Determine the balance sheet category for an account
        /// تحديد فئة الميزانية العمومية للحساب
        /// </summary>
        /// <param name="account">The GL account</param>
        /// <returns>Category name: CurrentAssets, NonCurrentAssets, CurrentLiabilities, NonCurrentLiabilities, Equity</returns>
        private string DetermineBalanceSheetCategory(GLAccount account)
        {
            // Check Account Type first
            if (account.AccountType == AccountType.Equity || account.AccountType == AccountType.ContraEquity)
            {
                return "Equity";
            }

            // For Assets
            if (account.AccountType == AccountType.Asset || account.AccountType == AccountType.ContraAsset)
            {
                // Priority 1: Check AccountFlags for specific classifications
                if (account.AccountFlags.HasFlag(AccountFlags.WorkingCapital))
                    return "CurrentAssets";

                if (account.AccountFlags.HasFlag(AccountFlags.FixedAsset) ||
                    account.AccountFlags.HasFlag(AccountFlags.IntangibleAsset) ||
                    account.AccountFlags.HasFlag(AccountFlags.Investment))
                    return "NonCurrentAssets";

                // Priority 2: Check account number pattern
                var code = account.AccountNumber ?? "";
                if (code.StartsWith("11") || code.StartsWith("1-1") || code.StartsWith("1.1"))
                    return "CurrentAssets";
                if (code.StartsWith("12") || code.StartsWith("1-2") || code.StartsWith("1.2"))
                    return "NonCurrentAssets";

                // Default: Current Assets for unclassified assets
                return "CurrentAssets";
            }

            // For Liabilities
            if (account.AccountType == AccountType.Liability || account.AccountType == AccountType.ContraLiability)
            {
                // Priority 1: Check AccountFlags
                if (account.AccountFlags.HasFlag(AccountFlags.WorkingCapital))
                    return "CurrentLiabilities";

                if (account.AccountFlags.HasFlag(AccountFlags.Loan))
                {
                    // Long-term loans are typically non-current
                    // But short-term loans should be current - use account code to distinguish
                    var code = account.AccountNumber ?? "";
                    if (code.StartsWith("21") || code.StartsWith("2-1") || code.StartsWith("2.1"))
                        return "CurrentLiabilities";
                    return "NonCurrentLiabilities";
                }

                // Priority 2: Check account number pattern
                var accountCode = account.AccountNumber ?? "";
                if (accountCode.StartsWith("21") || accountCode.StartsWith("2-1") || accountCode.StartsWith("2.1"))
                    return "CurrentLiabilities";
                if (accountCode.StartsWith("22") || accountCode.StartsWith("2-2") || accountCode.StartsWith("2.2"))
                    return "NonCurrentLiabilities";

                // Default: Current Liabilities for unclassified liabilities
                return "CurrentLiabilities";
            }

            // Fallback
            return "Equity";
        }

        /// <summary>
        /// Build full account number from parent chain
        /// </summary>
        private string BuildFullAccountNumber(GLAccount account, Dictionary<Guid, GLAccount> lookup)
        {
            var parts = new List<string> { account.AccountNumber };
            var currentId = account.ParentAccountId;

            while (currentId.HasValue && currentId != Guid.Empty && lookup.TryGetValue(currentId.Value, out var parent))
            {
                parts.Insert(0, parent.AccountNumber);
                currentId = parent.ParentAccountId;
            }

            return string.Join("-", parts);
        }

        /// <summary>
        /// Build hierarchical structure for balance sheet lines
        /// </summary>
        private List<BalanceSheetLineDto> BuildBalanceSheetHierarchy(
            List<BalanceSheetLineDto> flatLines,
            Dictionary<Guid, GLAccount> accountLookup)
        {
            if (!flatLines.Any())
                return flatLines;

            // Create lookup for lines
            var lineLookup = flatLines.ToDictionary(l => l.AccountId);

            // Ensure all parent accounts are included
            var accountsToAdd = new HashSet<Guid>();
            foreach (var line in flatLines)
            {
                var currentId = line.ParentAccountId;
                while (currentId.HasValue && currentId != Guid.Empty)
                {
                    if (!lineLookup.ContainsKey(currentId.Value) && !accountsToAdd.Contains(currentId.Value))
                    {
                        accountsToAdd.Add(currentId.Value);
                    }
                    if (accountLookup.TryGetValue(currentId.Value, out var parent))
                    {
                        currentId = parent.ParentAccountId;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            // Add missing parent accounts with zero balance
            foreach (var parentId in accountsToAdd)
            {
                if (accountLookup.TryGetValue(parentId, out var parentAccount))
                {
                    var category = DetermineBalanceSheetCategory(parentAccount);
                    var parentLine = new BalanceSheetLineDto
                    {
                        AccountId = parentAccount.AccountId,
                        AccountCode = parentAccount.AccountNumber,
                        FullAccountNumber = BuildFullAccountNumber(parentAccount, accountLookup),
                        AccountNameAr = parentAccount.AccountNameAr,
                        AccountNameEn = parentAccount.AccountNameEn,
                        AccountType = parentAccount.AccountType.ToString(),
                        AccountTypeEnum = parentAccount.AccountType,
                        Level = parentAccount.Level,
                        ParentAccountId = parentAccount.ParentAccountId,
                        IsLeaf = false,
                        Balance = 0,
                        PreviousPeriodBalance = null,
                        BalanceSheetCategory = category,
                        IsCurrent = category == "CurrentAssets" || category == "CurrentLiabilities"
                    };
                    flatLines.Add(parentLine);
                    lineLookup[parentId] = parentLine;
                }
            }

            // Find root nodes
            var rootNodes = flatLines
                .Where(l => l.ParentAccountId == null || l.ParentAccountId == Guid.Empty ||
                           !lineLookup.ContainsKey(l.ParentAccountId.Value))
                .OrderBy(l => l.FullAccountNumber ?? l.AccountCode)
                .ToList();

            // Build tree recursively
            foreach (var root in rootNodes)
            {
                BuildBalanceSheetChildren(root, flatLines, lineLookup);
            }

            return rootNodes;
        }

        /// <summary>
        /// Recursively build children for balance sheet hierarchy and roll up totals
        /// </summary>
        private void BuildBalanceSheetChildren(
            BalanceSheetLineDto parent,
            List<BalanceSheetLineDto> allLines,
            Dictionary<Guid, BalanceSheetLineDto> lookup)
        {
            var children = allLines
                .Where(l => l.ParentAccountId == parent.AccountId)
                .OrderBy(l => l.FullAccountNumber ?? l.AccountCode)
                .ToList();

            parent.Children = children;

            foreach (var child in children)
            {
                BuildBalanceSheetChildren(child, allLines, lookup);
            }

            // Roll up totals from leaf descendants
            if (children.Any())
            {
                var leafDescendants = GetBalanceSheetLeafDescendants(parent);
                parent.Balance = leafDescendants.Sum(l => l.Balance);

                // Roll up previous period balance if any children have it
                if (leafDescendants.Any(l => l.PreviousPeriodBalance.HasValue))
                {
                    parent.PreviousPeriodBalance = leafDescendants.Sum(l => l.PreviousPeriodBalance ?? 0);
                }
            }
        }

        /// <summary>
        /// Get all leaf descendants from a balance sheet parent node
        /// </summary>
        private List<BalanceSheetLineDto> GetBalanceSheetLeafDescendants(BalanceSheetLineDto parent)
        {
            var leaves = new List<BalanceSheetLineDto>();

            if (parent.Children == null || !parent.Children.Any())
            {
                leaves.Add(parent);
            }
            else
            {
                foreach (var child in parent.Children)
                {
                    leaves.AddRange(GetBalanceSheetLeafDescendants(child));
                }
            }

            return leaves;
        }

        /// <summary>
        /// Calculate total for a category (sum of root-level balances in hierarchical, or all in flat)
        /// </summary>
        private decimal CalculateCategoryTotal(List<BalanceSheetLineDto> lines, bool isHierarchical)
        {
            if (isHierarchical)
            {
                // In hierarchical mode, root nodes already have rolled-up totals
                return lines.Sum(l => l.Balance);
            }
            else
            {
                // In flat mode, sum all leaf account balances
                return lines.Where(l => l.IsLeaf).Sum(l => l.Balance);
            }
        }

        /// <summary>
        /// Calculate previous period total for a category
        /// </summary>
        private decimal? CalculatePreviousPeriodTotal(List<BalanceSheetLineDto> lines, bool isHierarchical)
        {
            var hasAnyPrevious = lines.Any(l => l.PreviousPeriodBalance.HasValue);
            if (!hasAnyPrevious)
                return null;

            if (isHierarchical)
            {
                return lines.Sum(l => l.PreviousPeriodBalance ?? 0);
            }
            else
            {
                return lines.Where(l => l.IsLeaf).Sum(l => l.PreviousPeriodBalance ?? 0);
            }
        }

        #endregion

        /// <summary>
        /// Get Income Statement data (without saving) - الحصول على بيانات قائمة الدخل
        /// </summary>
        public IncomeStatementReportDto GetIncomeStatementData(DateTime periodStart, DateTime periodEnd, long? companyId = null)
        {
            // TODO: Repository still uses Guid? tenantId - pass null for now during migration
            var accounts = _accountRepository.ListAll(null);
            var revenue = new List<IncomeStatementLineDto>();
            var expenses = new List<IncomeStatementLineDto>();

            foreach (var account in accounts.Where(a => a.IsActive))
            {
                var balance = _entryRepository.GetAccountBalance(account.AccountId, periodStart, periodEnd);
                // Use normalBalance-aware calculation for correct balance direction
                var netBalance = _normalBalanceResolver.CalculateBalance(
                    balance.TotalDebit,
                    balance.TotalCredit,
                    account.NormalBalance);

                if (netBalance == 0) continue;

                var line = new IncomeStatementLineDto
                {
                    AccountId = account.AccountId,
                    AccountCode = account.AccountNumber,
                    AccountNameAr = account.AccountNameAr,
                    AccountNameEn = account.AccountNameEn,
                    // Use normalized balance - positive indicates normal direction
                    Amount = netBalance
                };

                switch (account.AccountType)
                {
                    case AccountType.Revenue:
                    case AccountType.ContraRevenue:
                        revenue.Add(line);
                        break;
                    case AccountType.Expense:
                        expenses.Add(line);
                        break;
                }
            }

            return new IncomeStatementReportDto
            {
                PeriodStartDate = periodStart,
                PeriodEndDate = periodEnd,
                FiscalYear = periodStart.Year,
                Currency = "SAR",
                TotalRevenue = revenue.Sum(r => r.Amount),
                TotalExpenses = expenses.Sum(e => e.Amount),
                Revenue = revenue.OrderBy(r => r.AccountCode).ToList(),
                Expenses = expenses.OrderBy(e => e.AccountCode).ToList()
            };
        }

        /// <summary>
        /// Get Income Statement data async - الحصول على بيانات قائمة الدخل (غير متزامن)
        /// </summary>
        public async Task<IncomeStatementReportDto> GetIncomeStatementDataAsync(DateTime periodStart, DateTime periodEnd, long? companyId = null)
        {
            // TODO: Repository still uses Guid? tenantId - pass null for now during migration
            var accounts = await _accountRepository.ListAllAsync(null);
            var revenue = new List<IncomeStatementLineDto>();
            var expenses = new List<IncomeStatementLineDto>();

            foreach (var account in accounts.Where(a => a.IsActive))
            {
                var balance = await _entryRepository.GetAccountBalanceAsync(account.AccountId, periodStart, periodEnd);
                // Use normalBalance-aware calculation for correct balance direction
                var netBalance = _normalBalanceResolver.CalculateBalance(
                    balance.TotalDebit,
                    balance.TotalCredit,
                    account.NormalBalance);

                if (netBalance == 0) continue;

                var line = new IncomeStatementLineDto
                {
                    AccountId = account.AccountId,
                    AccountCode = account.AccountNumber,
                    AccountNameAr = account.AccountNameAr,
                    AccountNameEn = account.AccountNameEn,
                    // Use normalized balance - positive indicates normal direction
                    Amount = netBalance
                };

                switch (account.AccountType)
                {
                    case AccountType.Revenue:
                    case AccountType.ContraRevenue:
                        revenue.Add(line);
                        break;
                    case AccountType.Expense:
                        expenses.Add(line);
                        break;
                }
            }

            return new IncomeStatementReportDto
            {
                PeriodStartDate = periodStart,
                PeriodEndDate = periodEnd,
                FiscalYear = periodStart.Year,
                Currency = "SAR",
                TotalRevenue = revenue.Sum(r => r.Amount),
                TotalExpenses = expenses.Sum(e => e.Amount),
                Revenue = revenue.OrderBy(r => r.AccountCode).ToList(),
                Expenses = expenses.OrderBy(e => e.AccountCode).ToList()
            };
        }

        /// <summary>
        /// Get IFRS-compliant Income Statement data
        /// الحصول على بيانات قائمة الدخل وفق المعايير الدولية (IAS 1)
        /// </summary>
        public async Task<IFRSIncomeStatementReportDto> GetIFRSIncomeStatementAsync(IncomeStatementQueryDto query)
        {
            var periodStart = query.PeriodStart;
            var periodEnd = query.PeriodEnd;
            var companyId = query.CompanyId;
            var viewMode = query.ViewMode ?? "hierarchical";
            var isHierarchical = viewMode.Equals("hierarchical", StringComparison.OrdinalIgnoreCase);

            // Get fiscal year configuration
            var fiscalYear = _fiscalYearService.GetFiscalYearByDate(companyId, periodStart);
            var fiscalYearNumber = fiscalYear?.YearStartDate.Year ?? periodStart.Year;

            // Previous period (same duration, previous year)
            DateTime? previousPeriodStart = null;
            DateTime? previousPeriodEnd = null;
            if (query.IncludePreviousPeriod)
            {
                previousPeriodStart = periodStart.AddYears(-1);
                previousPeriodEnd = periodEnd.AddYears(-1);
            }

            // Get all active accounts
            var allAccounts = await _accountRepository.ListAllAsync(null);
            var activeAccounts = allAccounts.Where(a => a.IsActive).ToList();

            // Create account lookup for hierarchy building
            var accountLookup = activeAccounts.ToDictionary(a => a.AccountId);

            // Note: For P&L accounts, opening balance is typically 0 at start of each fiscal year
            // as they are closed to retained earnings. So we just calculate period activity.

            // Prepare category lists
            var revenueLines = new List<IncomeStatementLineItemDto>();
            var cogsLines = new List<IncomeStatementLineItemDto>();
            var operatingExpenseLines = new List<IncomeStatementLineItemDto>();
            var otherIncomeLines = new List<IncomeStatementLineItemDto>();
            var otherExpenseLines = new List<IncomeStatementLineItemDto>();

            // Process each account
            foreach (var account in activeAccounts)
            {
                // Only process income statement accounts (Revenue and Expense)
                if (!IsIncomeStatementAccount(account.AccountType))
                    continue;

                // Get period activity (debits and credits from journal entries)
                var periodActivity = await _entryRepository.GetAccountBalanceAsync(account.AccountId, periodStart, periodEnd);

                // Calculate closing balance using normal balance resolver
                // For Revenue (credit-normal): positive when Credits > Debits
                // For Expense (debit-normal): positive when Debits > Credits
                var closingBalance = _normalBalanceResolver.CalculateBalance(
                    periodActivity.TotalDebit,
                    periodActivity.TotalCredit,
                    account.NormalBalance);

                // Calculate previous period balance if requested
                decimal? previousBalance = null;
                if (previousPeriodStart.HasValue && previousPeriodEnd.HasValue)
                {
                    var prevActivity = await _entryRepository.GetAccountBalanceAsync(
                        account.AccountId, previousPeriodStart.Value, previousPeriodEnd.Value);
                    previousBalance = _normalBalanceResolver.CalculateBalance(
                        prevActivity.TotalDebit,
                        prevActivity.TotalCredit,
                        account.NormalBalance);
                }

                // In flat mode, skip zero balances
                if (!isHierarchical && closingBalance == 0 && (previousBalance ?? 0) == 0)
                    continue;

                // Determine the income statement category
                var category = DetermineIncomeStatementCategory(account);

                // Create line item
                var line = new IncomeStatementLineItemDto
                {
                    AccountId = account.AccountId,
                    AccountCode = account.AccountNumber,
                    FullAccountNumber = BuildFullAccountNumber(account, accountLookup),
                    AccountNameAr = account.AccountNameAr,
                    AccountNameEn = account.AccountNameEn,
                    AccountType = account.AccountType.ToString(),
                    AccountTypeEnum = account.AccountType,
                    Level = account.Level,
                    ParentAccountId = account.ParentAccountId,
                    IsLeaf = account.IsLeaf,
                    Balance = closingBalance,
                    PreviousPeriodBalance = previousBalance,
                    IncomeStatementCategory = category
                };

                // Add to appropriate category list
                switch (category)
                {
                    case "Revenue":
                        revenueLines.Add(line);
                        break;
                    case "COGS":
                        cogsLines.Add(line);
                        break;
                    case "OperatingExpenses":
                        operatingExpenseLines.Add(line);
                        break;
                    case "OtherIncome":
                        otherIncomeLines.Add(line);
                        break;
                    case "OtherExpenses":
                        otherExpenseLines.Add(line);
                        break;
                }
            }

            // Build hierarchy if needed
            if (isHierarchical)
            {
                revenueLines = BuildIncomeStatementHierarchy(revenueLines, accountLookup);
                cogsLines = BuildIncomeStatementHierarchy(cogsLines, accountLookup);
                operatingExpenseLines = BuildIncomeStatementHierarchy(operatingExpenseLines, accountLookup);
                otherIncomeLines = BuildIncomeStatementHierarchy(otherIncomeLines, accountLookup);
                otherExpenseLines = BuildIncomeStatementHierarchy(otherExpenseLines, accountLookup);
            }
            else
            {
                // In flat mode, sort by account code and remove hierarchy references
                revenueLines = revenueLines.OrderBy(l => l.AccountCode).ToList();
                cogsLines = cogsLines.OrderBy(l => l.AccountCode).ToList();
                operatingExpenseLines = operatingExpenseLines.OrderBy(l => l.AccountCode).ToList();
                otherIncomeLines = otherIncomeLines.OrderBy(l => l.AccountCode).ToList();
                otherExpenseLines = otherExpenseLines.OrderBy(l => l.AccountCode).ToList();

                // Clear parent references for flat mode
                foreach (var line in revenueLines.Concat(cogsLines).Concat(operatingExpenseLines)
                    .Concat(otherIncomeLines).Concat(otherExpenseLines))
                {
                    line.ParentAccountId = null;
                    line.Children = null;
                }
            }

            // Calculate totals
            var totalRevenue = CalculateIncomeStatementCategoryTotal(revenueLines, isHierarchical);
            var totalCOGS = CalculateIncomeStatementCategoryTotal(cogsLines, isHierarchical);
            var totalOperatingExpenses = CalculateIncomeStatementCategoryTotal(operatingExpenseLines, isHierarchical);
            var totalOtherIncome = CalculateIncomeStatementCategoryTotal(otherIncomeLines, isHierarchical);
            var totalOtherExpenses = CalculateIncomeStatementCategoryTotal(otherExpenseLines, isHierarchical);

            // Calculate previous period totals if needed
            decimal? prevTotalRevenue = null, prevTotalCOGS = null;
            decimal? prevTotalOperatingExpenses = null, prevTotalOtherIncome = null, prevTotalOtherExpenses = null;

            if (query.IncludePreviousPeriod)
            {
                prevTotalRevenue = CalculateIncomeStatementPreviousPeriodTotal(revenueLines, isHierarchical);
                prevTotalCOGS = CalculateIncomeStatementPreviousPeriodTotal(cogsLines, isHierarchical);
                prevTotalOperatingExpenses = CalculateIncomeStatementPreviousPeriodTotal(operatingExpenseLines, isHierarchical);
                prevTotalOtherIncome = CalculateIncomeStatementPreviousPeriodTotal(otherIncomeLines, isHierarchical);
                prevTotalOtherExpenses = CalculateIncomeStatementPreviousPeriodTotal(otherExpenseLines, isHierarchical);
            }

            // Build the report
            var report = new IFRSIncomeStatementReportDto
            {
                PeriodStartDate = periodStart,
                PeriodEndDate = periodEnd,
                PreviousPeriodStart = previousPeriodStart,
                PreviousPeriodEnd = previousPeriodEnd,
                FiscalYear = fiscalYearNumber,
                Currency = query.Currency ?? "SAR",
                ViewMode = viewMode,
                CompanyId = companyId,
                GeneratedAt = DateTime.Now,

                Revenue = new IncomeStatementCategoryDto
                {
                    CategoryNameEn = "Revenue",
                    CategoryNameAr = "الإيرادات",
                    Lines = revenueLines,
                    Total = totalRevenue,
                    PreviousPeriodTotal = prevTotalRevenue
                },
                TotalRevenue = totalRevenue,
                PreviousTotalRevenue = prevTotalRevenue,

                CostOfGoodsSold = new IncomeStatementCategoryDto
                {
                    CategoryNameEn = "Cost of Goods Sold",
                    CategoryNameAr = "تكلفة البضاعة المباعة",
                    Lines = cogsLines,
                    Total = totalCOGS,
                    PreviousPeriodTotal = prevTotalCOGS
                },
                TotalCOGS = totalCOGS,
                PreviousTotalCOGS = prevTotalCOGS,

                OperatingExpenses = new IncomeStatementCategoryDto
                {
                    CategoryNameEn = "Operating Expenses",
                    CategoryNameAr = "المصروفات التشغيلية",
                    Lines = operatingExpenseLines,
                    Total = totalOperatingExpenses,
                    PreviousPeriodTotal = prevTotalOperatingExpenses
                },
                TotalOperatingExpenses = totalOperatingExpenses,
                PreviousTotalOperatingExpenses = prevTotalOperatingExpenses,

                OtherIncome = new IncomeStatementCategoryDto
                {
                    CategoryNameEn = "Other Income",
                    CategoryNameAr = "الإيرادات الأخرى",
                    Lines = otherIncomeLines,
                    Total = totalOtherIncome,
                    PreviousPeriodTotal = prevTotalOtherIncome
                },
                TotalOtherIncome = totalOtherIncome,

                OtherExpenses = new IncomeStatementCategoryDto
                {
                    CategoryNameEn = "Other Expenses",
                    CategoryNameAr = "المصروفات الأخرى",
                    Lines = otherExpenseLines,
                    Total = totalOtherExpenses,
                    PreviousPeriodTotal = prevTotalOtherExpenses
                },
                TotalOtherExpenses = totalOtherExpenses,

                PreviousNetOtherIncomeExpense = prevTotalOtherIncome.HasValue && prevTotalOtherExpenses.HasValue
                    ? prevTotalOtherIncome.Value - prevTotalOtherExpenses.Value
                    : null,
                PreviousNetProfitBeforeTax = prevTotalRevenue.HasValue && prevTotalCOGS.HasValue &&
                    prevTotalOperatingExpenses.HasValue && prevTotalOtherIncome.HasValue && prevTotalOtherExpenses.HasValue
                    ? (prevTotalRevenue.Value - prevTotalCOGS.Value - prevTotalOperatingExpenses.Value +
                       prevTotalOtherIncome.Value - prevTotalOtherExpenses.Value)
                    : null,
                PreviousNetProfit = prevTotalRevenue.HasValue && prevTotalCOGS.HasValue &&
                    prevTotalOperatingExpenses.HasValue && prevTotalOtherIncome.HasValue && prevTotalOtherExpenses.HasValue
                    ? (prevTotalRevenue.Value - prevTotalCOGS.Value - prevTotalOperatingExpenses.Value +
                       prevTotalOtherIncome.Value - prevTotalOtherExpenses.Value)
                    : null
            };

            return report;
        }

        /// <summary>
        /// Determine if an account is an income statement account
        /// </summary>
        private bool IsIncomeStatementAccount(AccountType accountType)
        {
            return accountType switch
            {
                AccountType.Revenue  or
                AccountType.Expense => true,
                _ => false
            };
        }

        /// <summary>
        /// Determine the income statement category for an account
        /// تحديد فئة قائمة الدخل للحساب
        /// </summary>
        /// <remarks>
        /// Classification based on account code patterns:
        /// - 4xxx: Revenue (except 47xx, 48xx, 49xx which are Other Income)
        /// - 5xxx: Cost of Goods Sold (COGS)
        /// - 6xxx: Operating Expenses
        /// - 7xxx, 8xxx: Other Expenses
        /// - Interest/Dividend flags: Other Income/Expense
        /// </remarks>
        private string DetermineIncomeStatementCategory(GLAccount account)
        {
            var code = account.AccountNumber ?? "";
            var nameAr = account.AccountNameAr?.ToLower() ?? "";
            var nameEn = account.AccountNameEn?.ToLower() ?? "";

            // Revenue accounts
            if (account.AccountType == AccountType.Revenue )
            {
                // Check for Other Income based on Interest or Dividend flags
                if (account.AccountFlags.HasFlag(AccountFlags.Interest) ||
                    account.AccountFlags.HasFlag(AccountFlags.Dividend) ||
                    account.AccountFlags.HasFlag(AccountFlags.GainLossOnDisposal) ||
                    account.AccountFlags.HasFlag(AccountFlags.ForeignExchange))
                    return "OtherIncome";

                // Check account code pattern - typically 4xxx for revenue, 47xx-49xx for other income
                if (code.StartsWith("49") || code.StartsWith("47") || code.StartsWith("48"))
                    return "OtherIncome";

                return "Revenue";
            }

            // Expense accounts
            if (account.AccountType == AccountType.Expense)
            {
                // Single-digit expense codes (like "5") are top-level and span multiple categories
                // They should not be categorized into any specific category
                if (code.Length == 1)
                    return "TopLevelExpense"; // Special marker for top-level expense accounts

                // Check for non-operating expenses based on flags FIRST
                if (account.AccountFlags.HasFlag(AccountFlags.Interest) ||
                    account.AccountFlags.HasFlag(AccountFlags.GainLossOnDisposal) ||
                    account.AccountFlags.HasFlag(AccountFlags.ForeignExchange))
                    return "OtherExpenses";

                // 7xxx, 8xxx typically for other expenses (non-operating)
                if (code.StartsWith("7") || code.StartsWith("8"))
                    return "OtherExpenses";

                // COGS Detection - only 50, 51 are COGS (Cost of Sales)
                // 52 and above are Operating Expenses in typical Saudi COA structure
                bool isCOGS = false;
                if (code.Length >= 2)
                {
                    var prefix = code.Substring(0, 2);
                    // Only 50, 51 are COGS (direct production/sales costs)
                    // 52+ are operating expenses (salaries, rent, utilities, etc.)
                    isCOGS = prefix == "50" || prefix == "51";
                }

                // Also check for COGS keywords in account name
                if (!isCOGS)
                {
                    isCOGS = nameAr.Contains("تكلفة المبيعات") ||
                             nameAr.Contains("تكلفة البضاعة") ||
                             nameAr.Contains("تكاليف مباشرة") ||
                             nameEn.Contains("cost of goods sold") ||
                             nameEn.Contains("cost of sales") ||
                             nameEn.Contains("cogs") ||
                             nameEn.Contains("direct cost");
                }

                if (isCOGS)
                    return "COGS";

                // Check for Other Expenses patterns (53, 54 often contain finance costs, other expenses)
                if (code.Length >= 2)
                {
                    var prefix = code.Substring(0, 2);
                    // 53 = Finance Costs, 54 = Other Expenses - these are typically non-operating
                    if (prefix == "53" || prefix == "54")
                        return "OtherExpenses";
                }

                // Operating Expenses: 52, 55, 56, 6xxx, and other expense accounts
                // This includes: salaries, rent, utilities, admin expenses, selling/marketing expenses
                return "OperatingExpenses";
            }

            return "OperatingExpenses";
        }

        /// <summary>
        /// Build hierarchy for income statement lines
        /// </summary>
        /// <remarks>
        /// Important: Only adds parent accounts that belong to the SAME income statement category.
        /// Top-level expense accounts (like "5 - Expenses") that span multiple categories
        /// (COGS, Operating Expenses, Other Expenses) are NOT added to avoid duplication.
        /// </remarks>
        private List<IncomeStatementLineItemDto> BuildIncomeStatementHierarchy(
            List<IncomeStatementLineItemDto> flatLines,
            Dictionary<Guid, GLAccount> accountLookup)
        {
            if (flatLines.Count == 0)
                return flatLines;

            // Get the category of this list (all items should be in the same category)
            var targetCategory = flatLines.FirstOrDefault()?.IncomeStatementCategory;

            // Get all unique account IDs in the flat list
            var accountIdsInList = flatLines.Select(l => l.AccountId).ToHashSet();

            // Find all necessary parent accounts that should be included
            // IMPORTANT: Only add parents that belong to the same category
            var parentsToAdd = new Dictionary<Guid, IncomeStatementLineItemDto>();

            foreach (var line in flatLines)
            {
                var parentId = line.ParentAccountId;
                while (parentId.HasValue && !accountIdsInList.Contains(parentId.Value) && !parentsToAdd.ContainsKey(parentId.Value))
                {
                    if (accountLookup.TryGetValue(parentId.Value, out var parentAccount))
                    {
                        // Check if this parent account belongs to the same category
                        // Skip top-level expense accounts that would span multiple categories
                        var parentCategory = DetermineIncomeStatementCategory(parentAccount);

                        // Only add parent if it's in the same category as the children
                        // This prevents account "5" from appearing in both COGS and Operating Expenses
                        if (parentCategory != targetCategory)
                        {
                            // This parent spans multiple categories, don't add it
                            // But we need to update the child's parent reference to null
                            // so it becomes a root node
                            break;
                        }

                        var parentLine = new IncomeStatementLineItemDto
                        {
                            AccountId = parentAccount.AccountId,
                            AccountCode = parentAccount.AccountNumber,
                            FullAccountNumber = BuildFullAccountNumber(parentAccount, accountLookup),
                            AccountNameAr = parentAccount.AccountNameAr,
                            AccountNameEn = parentAccount.AccountNameEn,
                            AccountType = parentAccount.AccountType.ToString(),
                            AccountTypeEnum = parentAccount.AccountType,
                            Level = parentAccount.Level,
                            ParentAccountId = parentAccount.ParentAccountId,
                            IsLeaf = false,
                            Balance = 0, // Will be calculated from children
                            PreviousPeriodBalance = 0,
                            IncomeStatementCategory = targetCategory
                        };
                        parentsToAdd[parentId.Value] = parentLine;
                        parentId = parentAccount.ParentAccountId;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            // Add parent accounts to the list
            var allLines = flatLines.Concat(parentsToAdd.Values).ToList();

            // Build the tree structure
            var lineLookup = allLines.ToDictionary(l => l.AccountId);
            var rootLines = new List<IncomeStatementLineItemDto>();

            foreach (var line in allLines)
            {
                // Check if the parent is in our list AND has the same category
                if (line.ParentAccountId.HasValue && lineLookup.TryGetValue(line.ParentAccountId.Value, out var parentLine))
                {
                    parentLine.Children ??= new List<IncomeStatementLineItemDto>();
                    parentLine.Children.Add(line);
                }
                else
                {
                    // No valid parent in this category - this becomes a root node
                    rootLines.Add(line);
                }
            }

            // Sort children and calculate parent totals
            foreach (var line in allLines)
            {
                if (line.Children != null && line.Children.Count > 0)
                {
                    line.Children = line.Children.OrderBy(c => c.AccountCode).ToList();
                }
            }

            // Calculate roll-up totals for parent accounts
            CalculateIncomeStatementRollupTotals(rootLines);

            return rootLines.OrderBy(l => l.AccountCode).ToList();
        }

        /// <summary>
        /// Calculate roll-up totals for parent accounts in income statement
        /// </summary>
        private void CalculateIncomeStatementRollupTotals(List<IncomeStatementLineItemDto> lines)
        {
            foreach (var line in lines)
            {
                if (line.Children != null && line.Children.Count > 0)
                {
                    // First, recursively calculate children
                    CalculateIncomeStatementRollupTotals(line.Children);

                    // Then sum up children's balances
                    line.Balance = line.Children.Sum(c => c.Balance);
                    line.PreviousPeriodBalance = line.Children.Any(c => c.PreviousPeriodBalance.HasValue)
                        ? line.Children.Sum(c => c.PreviousPeriodBalance ?? 0)
                        : null;
                }
            }
        }

        /// <summary>
        /// Calculate total for income statement category
        /// </summary>
        private decimal CalculateIncomeStatementCategoryTotal(List<IncomeStatementLineItemDto> lines, bool isHierarchical)
        {
            if (isHierarchical)
            {
                // Sum only root-level balances (which already include roll-up from children)
                return lines.Sum(l => l.Balance);
            }
            else
            {
                // In flat mode, sum all leaf accounts
                return lines.Where(l => l.IsLeaf).Sum(l => l.Balance);
            }
        }

        /// <summary>
        /// Calculate previous period total for income statement category
        /// </summary>
        private decimal? CalculateIncomeStatementPreviousPeriodTotal(List<IncomeStatementLineItemDto> lines, bool isHierarchical)
        {
            var hasAnyPreviousPeriod = lines.Any(l => l.PreviousPeriodBalance.HasValue);
            if (!hasAnyPreviousPeriod)
                return null;

            if (isHierarchical)
            {
                return lines.Sum(l => l.PreviousPeriodBalance ?? 0);
            }
            else
            {
                return lines.Where(l => l.IsLeaf).Sum(l => l.PreviousPeriodBalance ?? 0);
            }
        }

        private CashFlowReportDto GetCashFlowData(DateTime periodStart, DateTime periodEnd, long? companyId)
        {
            // TODO: Repository still uses Guid? tenantId - pass null for now during migration
            var accounts = _accountRepository.ListAll(null);
            var cashAccounts = accounts.Where(a => a.IsActive && a.IsCashOrBank()).ToList();

            decimal totalInflows = 0;
            decimal totalOutflows = 0;

            foreach (var account in cashAccounts)
            {
                var balance = _entryRepository.GetAccountBalance(account.AccountId, periodStart, periodEnd);
                totalInflows += balance.TotalDebit;
                totalOutflows += balance.TotalCredit;
            }

            return new CashFlowReportDto
            {
                PeriodStartDate = periodStart,
                PeriodEndDate = periodEnd,
                TotalInflows = totalInflows,
                TotalOutflows = totalOutflows,
                NetCashFlow = totalInflows - totalOutflows
            };
        }

        private async Task<CashFlowReportDto> GetCashFlowDataAsync(DateTime periodStart, DateTime periodEnd, long? companyId)
        {
            // TODO: Repository still uses Guid? tenantId - pass null for now during migration
            var accounts = await _accountRepository.ListAllAsync(null);
            var cashAccounts = accounts.Where(a => a.IsActive && a.IsCashOrBank()).ToList();

            decimal totalInflows = 0;
            decimal totalOutflows = 0;

            foreach (var account in cashAccounts)
            {
                var balance = await _entryRepository.GetAccountBalanceAsync(account.AccountId, periodStart, periodEnd);
                totalInflows += balance.TotalDebit;
                totalOutflows += balance.TotalCredit;
            }

            return new CashFlowReportDto
            {
                PeriodStartDate = periodStart,
                PeriodEndDate = periodEnd,
                TotalInflows = totalInflows,
                TotalOutflows = totalOutflows,
                NetCashFlow = totalInflows - totalOutflows
            };
        }

        private FinancialReportResponseDto MapToDto(GLFinancialReport entity)
        {
            return new FinancialReportResponseDto
            {
                Id = entity.Id,
                ReportType = entity.ReportType,
                ReportNumber = entity.ReportNumber,
                PeriodStartDate = entity.PeriodStartDate,
                PeriodEndDate = entity.PeriodEndDate,
                FiscalYear = entity.FiscalYear,
                DescriptionAr = entity.DescriptionAr,
                DescriptionEn = entity.DescriptionEn,
                TotalDebit = entity.TotalDebit,
                TotalCredit = entity.TotalCredit,
                Currency = entity.Currency,
                ReportData = entity.ReportData,
                GeneratedAt = entity.GeneratedAt,
                GeneratedBy = entity.GeneratedBy,
                CompanyId = entity.CompanyID
            };
        }

        #endregion

        #region IAS 7 Cash Flow Statement (Indirect Method)

        /// <summary>
        /// Generate IAS 7 Compliant Cash Flow Statement (Indirect Method)
        /// </summary>
        public FinancialReportResponseDto GenerateCashFlowIAS7(GenerateReportDto dto, string generatedBy)
        {
            var cashFlowData = GetCashFlowDataIAS7(dto.PeriodStartDate, dto.PeriodEndDate, dto.CompanyId);

            Guid? generatedByGuid = null;
            if (Guid.TryParse(generatedBy, out Guid parsedGuid))
                generatedByGuid = parsedGuid;

            var report = new GLFinancialReport
            {
                Id = Guid.NewGuid(),
                ReportType = ReportType.CashFlow,
                ReportNumber = _reportRepository.GenerateReportNumber(ReportType.CashFlow, null, dto.FiscalYear),
                PeriodStartDate = dto.PeriodStartDate,
                PeriodEndDate = dto.PeriodEndDate,
                FiscalYear = dto.FiscalYear ?? dto.PeriodStartDate.Year,
                DescriptionAr = dto.DescriptionAr ?? $"قائمة التدفقات النقدية (IAS 7) للفترة من {dto.PeriodStartDate:yyyy-MM-dd} إلى {dto.PeriodEndDate:yyyy-MM-dd}",
                DescriptionEn = dto.DescriptionEn ?? $"Cash Flow Statement (IAS 7) for period {dto.PeriodStartDate:yyyy-MM-dd} to {dto.PeriodEndDate:yyyy-MM-dd}",
                ReportData = System.Text.Json.JsonSerializer.Serialize(cashFlowData),
                TotalDebit = cashFlowData.NetCashFromOperating + cashFlowData.NetCashFromInvesting + cashFlowData.NetCashFromFinancing,
                TotalCredit = cashFlowData.ClosingCashBalance,
                Currency = dto.Currency ?? "SAR",
                GeneratedAt = DateTime.Now,
                GeneratedBy = generatedByGuid,
                CompanyID = dto.CompanyId
            };

            _reportRepository.CreateReport(report);
            return MapToDto(report);
        }

        /// <summary>
        /// Generate IAS 7 Cash Flow Statement async
        /// </summary>
        public async Task<FinancialReportResponseDto> GenerateCashFlowIAS7Async(GenerateReportDto dto, string generatedBy)
        {
            var cashFlowData = await GetCashFlowDataIAS7Async(dto.PeriodStartDate, dto.PeriodEndDate, dto.CompanyId);

            Guid? generatedByGuid = null;
            if (Guid.TryParse(generatedBy, out Guid parsedGuid))
                generatedByGuid = parsedGuid;

            var report = new GLFinancialReport
            {
                Id = Guid.NewGuid(),
                ReportType = ReportType.CashFlow,
                ReportNumber = await _reportRepository.GenerateReportNumberAsync(ReportType.CashFlow, null, dto.FiscalYear),
                PeriodStartDate = dto.PeriodStartDate,
                PeriodEndDate = dto.PeriodEndDate,
                FiscalYear = dto.FiscalYear ?? dto.PeriodStartDate.Year,
                DescriptionAr = dto.DescriptionAr ?? $"قائمة التدفقات النقدية (IAS 7) للفترة من {dto.PeriodStartDate:yyyy-MM-dd} إلى {dto.PeriodEndDate:yyyy-MM-dd}",
                DescriptionEn = dto.DescriptionEn ?? $"Cash Flow Statement (IAS 7) for period {dto.PeriodStartDate:yyyy-MM-dd} to {dto.PeriodEndDate:yyyy-MM-dd}",
                ReportData = System.Text.Json.JsonSerializer.Serialize(cashFlowData),
                TotalDebit = cashFlowData.NetCashFromOperating + cashFlowData.NetCashFromInvesting + cashFlowData.NetCashFromFinancing,
                TotalCredit = cashFlowData.ClosingCashBalance,
                Currency = dto.Currency ?? "SAR",
                GeneratedAt = DateTime.Now,
                GeneratedBy = generatedByGuid,
                CompanyID = dto.CompanyId
            };

            await _reportRepository.CreateReportAsync(report);
            return MapToDto(report);
        }

        /// <summary>
        /// Get IAS 7 Cash Flow Statement Data using Indirect Method
        /// IAS 7.18-20: Start with profit/loss and adjust for non-cash items and working capital changes
        /// </summary>
        public CashFlowStatementDto GetCashFlowDataIAS7(DateTime periodStart, DateTime periodEnd, long? companyId = null)
        {
            // Get all accounts for classification
            var accounts = _accountRepository.ListAll(null);
            if (companyId.HasValue)
            {
                accounts = accounts.Where(a => a.CompanyID == companyId).ToList();
            }

            // Calculate period start for opening balance (day before period start)
            var openingDate = periodStart.AddDays(-1);
            var fiscalYearStart = new DateTime(periodStart.Year, 1, 1);

            // STEP 1: Get Net Income from Income Statement
            var incomeData = GetIncomeStatementData(periodStart, periodEnd, companyId);
            decimal netIncome = incomeData.TotalRevenue - incomeData.TotalExpenses;

            // STEP 2: Calculate Opening and Closing Cash Balances
            var cashAccounts = accounts.Where(a => a.IsActive && a.IsCashOrBank()).ToList();
            decimal openingCashBalance = CalculateCashBalance(cashAccounts, fiscalYearStart, openingDate);
            decimal closingCashBalance = CalculateCashBalance(cashAccounts, fiscalYearStart, periodEnd);

            // STEP 3: Calculate Non-Cash Adjustments
            var operatingActivities = CalculateOperatingActivities(accounts, periodStart, periodEnd, netIncome, companyId);

            // STEP 4: Calculate Investing Activities
            var investingActivities = CalculateInvestingActivities(accounts, periodStart, periodEnd, companyId);

            // STEP 5: Calculate Financing Activities
            var financingActivities = CalculateFinancingActivities(accounts, periodStart, periodEnd, companyId);

            return new CashFlowStatementDto
            {
                PeriodStartDate = periodStart,
                PeriodEndDate = periodEnd,
                FiscalYear = periodStart.Year,
                Currency = "SAR",
                CompanyId = companyId,
                OpeningCashBalance = openingCashBalance,
                ActualCashBalance = closingCashBalance,
                OperatingActivities = operatingActivities,
                InvestingActivities = investingActivities,
                FinancingActivities = financingActivities
            };
        }

        /// <summary>
        /// Get IAS 7 Cash Flow Statement Data async
        /// </summary>
        public async Task<CashFlowStatementDto> GetCashFlowDataIAS7Async(DateTime periodStart, DateTime periodEnd, long? companyId = null)
        {
            // Get all accounts for classification
            var accounts = await _accountRepository.ListAllAsync(null);
            if (companyId.HasValue)
            {
                accounts = accounts.Where(a => a.CompanyID == companyId).ToList();
            }

            // Calculate period start for opening balance (day before period start)
            var openingDate = periodStart.AddDays(-1);
            var fiscalYearStart = new DateTime(periodStart.Year, 1, 1);

            // STEP 1: Get Net Income from Income Statement
            var incomeData = await GetIncomeStatementDataAsync(periodStart, periodEnd, companyId);
            decimal netIncome = incomeData.TotalRevenue - incomeData.TotalExpenses;

            // STEP 2: Calculate Opening and Closing Cash Balances
            var cashAccounts = accounts.Where(a => a.IsActive && a.IsCashOrBank()).ToList();
            decimal openingCashBalance = await CalculateCashBalanceAsync(cashAccounts, fiscalYearStart, openingDate);
            decimal closingCashBalance = await CalculateCashBalanceAsync(cashAccounts, fiscalYearStart, periodEnd);

            // STEP 3: Calculate Non-Cash Adjustments
            var operatingActivities = await CalculateOperatingActivitiesAsync(accounts, periodStart, periodEnd, netIncome, companyId);

            // STEP 4: Calculate Investing Activities
            var investingActivities = await CalculateInvestingActivitiesAsync(accounts, periodStart, periodEnd, companyId);

            // STEP 5: Calculate Financing Activities
            var financingActivities = await CalculateFinancingActivitiesAsync(accounts, periodStart, periodEnd, companyId);

            return new CashFlowStatementDto
            {
                PeriodStartDate = periodStart,
                PeriodEndDate = periodEnd,
                FiscalYear = periodStart.Year,
                Currency = "SAR",
                CompanyId = companyId,
                OpeningCashBalance = openingCashBalance,
                ActualCashBalance = closingCashBalance,
                OperatingActivities = operatingActivities,
                InvestingActivities = investingActivities,
                FinancingActivities = financingActivities
            };
        }

        /// <summary>
        /// Calculate total cash balance from cash accounts
        /// Uses normalBalance-aware calculation for correct balance direction
        /// </summary>
        private decimal CalculateCashBalance(List<GLAccount> cashAccounts, DateTime fromDate, DateTime toDate)
        {
            decimal totalBalance = 0;
            foreach (var account in cashAccounts)
            {
                var balance = _entryRepository.GetAccountBalance(account.AccountId, fromDate, toDate);
                // Cash accounts are Asset accounts with Debit normal balance
                totalBalance += _normalBalanceResolver.CalculateBalance(
                    balance.TotalDebit,
                    balance.TotalCredit,
                    account.NormalBalance);
            }
            return totalBalance;
        }

        /// <summary>
        /// Calculate total cash balance async
        /// Uses normalBalance-aware calculation for correct balance direction
        /// </summary>
        private async Task<decimal> CalculateCashBalanceAsync(List<GLAccount> cashAccounts, DateTime fromDate, DateTime toDate)
        {
            decimal totalBalance = 0;
            foreach (var account in cashAccounts)
            {
                var balance = await _entryRepository.GetAccountBalanceAsync(account.AccountId, fromDate, toDate);
                // Cash accounts are Asset accounts with Debit normal balance
                totalBalance += _normalBalanceResolver.CalculateBalance(
                    balance.TotalDebit,
                    balance.TotalCredit,
                    account.NormalBalance);
            }
            return totalBalance;
        }

        /// <summary>
        /// Calculate Operating Activities using Indirect Method
        /// IAS 7.18-20: Start with net income and adjust for non-cash items
        /// </summary>
        private CashFlowOperatingActivitiesDto CalculateOperatingActivities(
            List<GLAccount> accounts, DateTime periodStart, DateTime periodEnd, decimal netIncome, long? companyId)
        {
            var operatingActivities = new CashFlowOperatingActivitiesDto
            {
                NetIncome = netIncome,
                LineItems = new List<CashFlowLineItemDto>()
            };

            // Calculate opening and closing dates for balance comparisons
            var fiscalYearStart = new DateTime(periodStart.Year, 1, 1);
            var openingDate = periodStart.AddDays(-1);

            // NON-CASH ADJUSTMENTS
            // Using explicit flags for accurate classification instead of string matching

            // 1. Depreciation - accounts with Depreciation flag
            var depreciationAccounts = accounts.Where(a =>
                a.IsActive && a.AccountFlags.HasFlag(AccountFlags.Depreciation)).ToList();

            // Fallback to string matching for backwards compatibility with existing data
            if (!depreciationAccounts.Any())
            {
                depreciationAccounts = accounts.Where(a =>
                    a.AccountType == AccountType.ContraAsset &&
                    a.IsActive &&
                    (a.AccountNameEn?.ToLower().Contains("depreciation") == true ||
                     a.AccountNameAr?.Contains("إهلاك") == true ||
                     a.AccountNameAr?.Contains("استهلاك") == true)).ToList();
            }

            foreach (var account in depreciationAccounts)
            {
                var balance = _entryRepository.GetAccountBalance(account.AccountId, periodStart, periodEnd);
                // Use normalBalance-aware calculation - ContraAsset accounts are Credit-normal
                var periodMovement = _normalBalanceResolver.CalculateBalance(
                    balance.TotalDebit,
                    balance.TotalCredit,
                    account.NormalBalance);
                operatingActivities.Depreciation += periodMovement;

                if (periodMovement != 0)
                {
                    operatingActivities.LineItems.Add(new CashFlowLineItemDto
                    {
                        AccountId = account.AccountId,
                        AccountCode = account.AccountNumber,
                        DescriptionEn = $"Add: Depreciation - {account.AccountNameEn}",
                        DescriptionAr = $"يضاف: الإهلاك - {account.AccountNameAr}",
                        Category = CashFlowCategory.Operating,
                        Amount = periodMovement,
                        SortOrder = 10
                    });
                }
            }

            // 2. Amortization - accounts with Amortization flag
            var amortizationAccounts = accounts.Where(a =>
                a.IsActive && a.AccountFlags.HasFlag(AccountFlags.Amortization)).ToList();

            // Fallback to string matching
            if (!amortizationAccounts.Any())
            {
                amortizationAccounts = accounts.Where(a =>
                    a.AccountType == AccountType.ContraAsset &&
                    a.IsActive &&
                    (a.AccountNameEn?.ToLower().Contains("amortization") == true ||
                     a.AccountNameAr?.Contains("إطفاء") == true)).ToList();
            }

            foreach (var account in amortizationAccounts)
            {
                var balance = _entryRepository.GetAccountBalance(account.AccountId, periodStart, periodEnd);
                // Use normalBalance-aware calculation - ContraAsset accounts are Credit-normal
                var periodMovement = _normalBalanceResolver.CalculateBalance(
                    balance.TotalDebit,
                    balance.TotalCredit,
                    account.NormalBalance);
                operatingActivities.Amortization += periodMovement;

                if (periodMovement != 0)
                {
                    operatingActivities.LineItems.Add(new CashFlowLineItemDto
                    {
                        AccountId = account.AccountId,
                        AccountCode = account.AccountNumber,
                        DescriptionEn = $"Add: Amortization - {account.AccountNameEn}",
                        DescriptionAr = $"يضاف: الإطفاء - {account.AccountNameAr}",
                        Category = CashFlowCategory.Operating,
                        Amount = periodMovement,
                        SortOrder = 11
                    });
                }
            }

            // 3. Provisions - accounts with Provision flag
            var provisionAccounts = accounts.Where(a =>
                a.IsActive && a.AccountFlags.HasFlag(AccountFlags.Provision)).ToList();

            // Fallback to string matching
            if (!provisionAccounts.Any())
            {
                provisionAccounts = accounts.Where(a =>
                    a.AccountType == AccountType.Liability &&
                    a.IsActive &&
                    (a.AccountNameEn?.ToLower().Contains("provision") == true ||
                     a.AccountNameAr?.Contains("مخصص") == true)).ToList();
            }

            foreach (var account in provisionAccounts)
            {
                var openingBalance = _entryRepository.GetAccountBalance(account.AccountId, fiscalYearStart, openingDate);
                var closingBalance = _entryRepository.GetAccountBalance(account.AccountId, fiscalYearStart, periodEnd);
                // Use normalBalance-aware calculation - Liability (Provision) accounts are Credit-normal
                var openingNet = _normalBalanceResolver.CalculateBalance(
                    openingBalance.TotalDebit,
                    openingBalance.TotalCredit,
                    account.NormalBalance);
                var closingNet = _normalBalanceResolver.CalculateBalance(
                    closingBalance.TotalDebit,
                    closingBalance.TotalCredit,
                    account.NormalBalance);
                var change = closingNet - openingNet;

                if (change > 0) // Increase in provision is non-cash expense
                {
                    operatingActivities.IncreaseInProvisions += change;
                    operatingActivities.LineItems.Add(new CashFlowLineItemDto
                    {
                        AccountId = account.AccountId,
                        AccountCode = account.AccountNumber,
                        DescriptionEn = $"Add: Increase in {account.AccountNameEn}",
                        DescriptionAr = $"يضاف: الزيادة في {account.AccountNameAr}",
                        Category = CashFlowCategory.Operating,
                        Amount = change,
                        SortOrder = 12
                    });
                }
            }

            // WORKING CAPITAL CHANGES
            // Using explicit flags and WorkingCapitalType for accurate classification

            // 4. Trade Receivables - accounts with WorkingCapital flag and Receivables type
            var receivableAccounts = accounts.Where(a =>
                a.IsActive &&
                a.AccountFlags.HasFlag(AccountFlags.WorkingCapital) &&
                a.WorkingCapitalType == WorkingCapitalType.Receivables).ToList();

            // Fallback to string matching
            if (!receivableAccounts.Any())
            {
                receivableAccounts = accounts.Where(a =>
                    a.AccountType == AccountType.Asset &&
                    a.IsActive && a.IsControlAccount &&
                    (a.AccountNameEn?.ToLower().Contains("receivable") == true ||
                     a.AccountNameAr?.Contains("مدين") == true ||
                     a.AccountNameAr?.Contains("ذمم") == true)).ToList();
            }

            decimal receivablesChange = CalculateWorkingCapitalChange(receivableAccounts, fiscalYearStart, openingDate, periodEnd, true);
            operatingActivities.ChangeInTradeReceivables = -receivablesChange; // Increase in asset = negative cash flow
            if (receivablesChange != 0)
            {
                operatingActivities.LineItems.Add(new CashFlowLineItemDto
                {
                    DescriptionEn = receivablesChange > 0 ? "Less: Increase in Trade Receivables" : "Add: Decrease in Trade Receivables",
                    DescriptionAr = receivablesChange > 0 ? "يطرح: الزيادة في المدينون التجاريون" : "يضاف: النقص في المدينون التجاريون",
                    Category = CashFlowCategory.Operating,
                    Amount = -receivablesChange,
                    SortOrder = 20
                });
            }

            // 5. Inventory - accounts with WorkingCapital flag and Inventory type
            var inventoryAccounts = accounts.Where(a =>
                a.IsActive &&
                a.AccountFlags.HasFlag(AccountFlags.WorkingCapital) &&
                a.WorkingCapitalType == WorkingCapitalType.Inventory).ToList();

            // Fallback to string matching
            if (!inventoryAccounts.Any())
            {
                inventoryAccounts = accounts.Where(a =>
                    a.AccountType == AccountType.Asset &&
                    a.IsActive &&
                    (a.AccountNameEn?.ToLower().Contains("inventory") == true ||
                     a.AccountNameAr?.Contains("مخزون") == true ||
                     a.AccountNameAr?.Contains("بضاعة") == true)).ToList();
            }

            decimal inventoryChange = CalculateWorkingCapitalChange(inventoryAccounts, fiscalYearStart, openingDate, periodEnd, true);
            operatingActivities.ChangeInInventory = -inventoryChange;
            if (inventoryChange != 0)
            {
                operatingActivities.LineItems.Add(new CashFlowLineItemDto
                {
                    DescriptionEn = inventoryChange > 0 ? "Less: Increase in Inventory" : "Add: Decrease in Inventory",
                    DescriptionAr = inventoryChange > 0 ? "يطرح: الزيادة في المخزون" : "يضاف: النقص في المخزون",
                    Category = CashFlowCategory.Operating,
                    Amount = -inventoryChange,
                    SortOrder = 21
                });
            }

            // 6. Prepaid Expenses - accounts with WorkingCapital flag and Prepaid type
            var prepaidAccounts = accounts.Where(a =>
                a.IsActive &&
                a.AccountFlags.HasFlag(AccountFlags.WorkingCapital) &&
                a.WorkingCapitalType == WorkingCapitalType.Prepaid).ToList();

            // Fallback to string matching
            if (!prepaidAccounts.Any())
            {
                prepaidAccounts = accounts.Where(a =>
                    a.AccountType == AccountType.Asset &&
                    a.IsActive &&
                    (a.AccountNameEn?.ToLower().Contains("prepaid") == true ||
                     a.AccountNameAr?.Contains("مقدم") == true ||
                     a.AccountNameAr?.Contains("مصروفات مدفوعة") == true)).ToList();
            }

            decimal prepaidChange = CalculateWorkingCapitalChange(prepaidAccounts, fiscalYearStart, openingDate, periodEnd, true);
            operatingActivities.ChangeInPrepaidExpenses = -prepaidChange;
            if (prepaidChange != 0)
            {
                operatingActivities.LineItems.Add(new CashFlowLineItemDto
                {
                    DescriptionEn = prepaidChange > 0 ? "Less: Increase in Prepaid Expenses" : "Add: Decrease in Prepaid Expenses",
                    DescriptionAr = prepaidChange > 0 ? "يطرح: الزيادة في المصروفات المدفوعة مقدماً" : "يضاف: النقص في المصروفات المدفوعة مقدماً",
                    Category = CashFlowCategory.Operating,
                    Amount = -prepaidChange,
                    SortOrder = 22
                });
            }

            // 7. Trade Payables - accounts with WorkingCapital flag and Payables type
            var payableAccounts = accounts.Where(a =>
                a.IsActive &&
                a.AccountFlags.HasFlag(AccountFlags.WorkingCapital) &&
                a.WorkingCapitalType == WorkingCapitalType.Payables).ToList();

            // Fallback to string matching
            if (!payableAccounts.Any())
            {
                payableAccounts = accounts.Where(a =>
                    a.AccountType == AccountType.Liability &&
                    a.IsActive && a.IsControlAccount &&
                    (a.AccountNameEn?.ToLower().Contains("payable") == true ||
                     a.AccountNameAr?.Contains("دائن") == true ||
                     a.AccountNameAr?.Contains("موردين") == true)).ToList();
            }

            decimal payablesChange = CalculateWorkingCapitalChange(payableAccounts, fiscalYearStart, openingDate, periodEnd, false);
            operatingActivities.ChangeInTradePayables = payablesChange; // Increase in liability = positive cash flow
            if (payablesChange != 0)
            {
                operatingActivities.LineItems.Add(new CashFlowLineItemDto
                {
                    DescriptionEn = payablesChange > 0 ? "Add: Increase in Trade Payables" : "Less: Decrease in Trade Payables",
                    DescriptionAr = payablesChange > 0 ? "يضاف: الزيادة في الدائنون التجاريون" : "يطرح: النقص في الدائنون التجاريون",
                    Category = CashFlowCategory.Operating,
                    Amount = payablesChange,
                    SortOrder = 30
                });
            }

            // 8. Accrued Expenses - accounts with WorkingCapital flag and Accrued type
            var accruedAccounts = accounts.Where(a =>
                a.IsActive &&
                a.AccountFlags.HasFlag(AccountFlags.WorkingCapital) &&
                a.WorkingCapitalType == WorkingCapitalType.Accrued).ToList();

            // Fallback to string matching
            if (!accruedAccounts.Any())
            {
                accruedAccounts = accounts.Where(a =>
                    a.AccountType == AccountType.Liability &&
                    a.IsActive &&
                    (a.AccountNameEn?.ToLower().Contains("accrued") == true ||
                     a.AccountNameAr?.Contains("مستحق") == true)).ToList();
            }

            decimal accruedChange = CalculateWorkingCapitalChange(accruedAccounts, fiscalYearStart, openingDate, periodEnd, false);
            operatingActivities.ChangeInAccruedExpenses = accruedChange;
            if (accruedChange != 0)
            {
                operatingActivities.LineItems.Add(new CashFlowLineItemDto
                {
                    DescriptionEn = accruedChange > 0 ? "Add: Increase in Accrued Expenses" : "Less: Decrease in Accrued Expenses",
                    DescriptionAr = accruedChange > 0 ? "يضاف: الزيادة في المصروفات المستحقة" : "يطرح: النقص في المصروفات المستحقة",
                    Category = CashFlowCategory.Operating,
                    Amount = accruedChange,
                    SortOrder = 31
                });
            }

            return operatingActivities;
        }

        /// <summary>
        /// Calculate Operating Activities async
        /// </summary>
        private async Task<CashFlowOperatingActivitiesDto> CalculateOperatingActivitiesAsync(
            List<GLAccount> accounts, DateTime periodStart, DateTime periodEnd, decimal netIncome, long? companyId)
        {
            // For simplicity, call the sync version - in a real implementation you'd want to parallelize
            return await Task.Run(() => CalculateOperatingActivities(accounts, periodStart, periodEnd, netIncome, companyId));
        }

        /// <summary>
        /// Calculate change in working capital accounts
        /// Uses normalBalance-aware calculation for correct balance direction
        /// </summary>
        private decimal CalculateWorkingCapitalChange(List<GLAccount> accounts, DateTime fiscalYearStart, DateTime openingDate, DateTime periodEnd, bool isAsset)
        {
            decimal openingBalance = 0;
            decimal closingBalance = 0;

            foreach (var account in accounts)
            {
                var openingBal = _entryRepository.GetAccountBalance(account.AccountId, fiscalYearStart, openingDate);
                var closingBal = _entryRepository.GetAccountBalance(account.AccountId, fiscalYearStart, periodEnd);

                // Use normalBalance-aware calculation for correct balance direction
                openingBalance += _normalBalanceResolver.CalculateBalance(
                    openingBal.TotalDebit,
                    openingBal.TotalCredit,
                    account.NormalBalance);
                closingBalance += _normalBalanceResolver.CalculateBalance(
                    closingBal.TotalDebit,
                    closingBal.TotalCredit,
                    account.NormalBalance);
            }

            return closingBalance - openingBalance;
        }

        /// <summary>
        /// Calculate Investing Activities
        /// IAS 7.16: Cash flows from acquisition/disposal of long-term assets
        /// </summary>
        private CashFlowInvestingActivitiesDto CalculateInvestingActivities(
            List<GLAccount> accounts, DateTime periodStart, DateTime periodEnd, long? companyId)
        {
            var investingActivities = new CashFlowInvestingActivitiesDto
            {
                LineItems = new List<CashFlowLineItemDto>()
            };

            // 1. Fixed Assets / PPE - accounts with FixedAsset flag
            var fixedAssetAccounts = accounts.Where(a =>
                a.IsActive && a.AccountFlags.HasFlag(AccountFlags.FixedAsset)).ToList();

            // Fallback to string matching for backwards compatibility
            if (!fixedAssetAccounts.Any())
            {
                fixedAssetAccounts = accounts.Where(a =>
                    a.AccountType == AccountType.Asset &&
                    a.IsActive &&
                    (a.AccountNameEn?.ToLower().Contains("property") == true ||
                     a.AccountNameEn?.ToLower().Contains("plant") == true ||
                     a.AccountNameEn?.ToLower().Contains("equipment") == true ||
                     a.AccountNameEn?.ToLower().Contains("fixed asset") == true ||
                     a.AccountNameAr?.Contains("أصول ثابتة") == true ||
                     a.AccountNameAr?.Contains("معدات") == true ||
                     a.AccountNameAr?.Contains("آلات") == true ||
                     a.AccountNameAr?.Contains("مباني") == true)).ToList();
            }

            foreach (var account in fixedAssetAccounts)
            {
                var balance = _entryRepository.GetAccountBalance(account.AccountId, periodStart, periodEnd);
                // Use normalBalance-aware calculation - positive = increase in asset (purchase)
                var netPurchases = _normalBalanceResolver.CalculateBalance(
                    balance.TotalDebit,
                    balance.TotalCredit,
                    account.NormalBalance);

                if (netPurchases > 0)
                {
                    investingActivities.PurchaseOfPPE += netPurchases;
                    investingActivities.LineItems.Add(new CashFlowLineItemDto
                    {
                        AccountId = account.AccountId,
                        AccountCode = account.AccountNumber,
                        DescriptionEn = $"Purchase of {account.AccountNameEn}",
                        DescriptionAr = $"شراء {account.AccountNameAr}",
                        Category = CashFlowCategory.Investing,
                        Amount = -netPurchases, // Outflow is negative
                        SortOrder = 40
                    });
                }
                else if (netPurchases < 0)
                {
                    investingActivities.SaleOfPPE += Math.Abs(netPurchases);
                    investingActivities.LineItems.Add(new CashFlowLineItemDto
                    {
                        AccountId = account.AccountId,
                        AccountCode = account.AccountNumber,
                        DescriptionEn = $"Sale of {account.AccountNameEn}",
                        DescriptionAr = $"بيع {account.AccountNameAr}",
                        Category = CashFlowCategory.Investing,
                        Amount = Math.Abs(netPurchases), // Inflow is positive
                        SortOrder = 41
                    });
                }
            }

            // 2. Intangible Assets - accounts with IntangibleAsset flag
            var intangibleAccounts = accounts.Where(a =>
                a.IsActive && a.AccountFlags.HasFlag(AccountFlags.IntangibleAsset)).ToList();

            // Fallback to string matching for backwards compatibility
            if (!intangibleAccounts.Any())
            {
                intangibleAccounts = accounts.Where(a =>
                    a.AccountType == AccountType.Asset &&
                    a.IsActive &&
                    (a.AccountNameEn?.ToLower().Contains("intangible") == true ||
                     a.AccountNameEn?.ToLower().Contains("goodwill") == true ||
                     a.AccountNameEn?.ToLower().Contains("software") == true ||
                     a.AccountNameEn?.ToLower().Contains("patent") == true ||
                     a.AccountNameAr?.Contains("أصول غير ملموسة") == true ||
                     a.AccountNameAr?.Contains("شهرة") == true ||
                     a.AccountNameAr?.Contains("برمجيات") == true)).ToList();
            }

            foreach (var account in intangibleAccounts)
            {
                var balance = _entryRepository.GetAccountBalance(account.AccountId, periodStart, periodEnd);
                // Use normalBalance-aware calculation - positive = increase in asset (purchase)
                var netPurchases = _normalBalanceResolver.CalculateBalance(
                    balance.TotalDebit,
                    balance.TotalCredit,
                    account.NormalBalance);

                if (netPurchases > 0)
                {
                    investingActivities.PurchaseOfIntangibles += netPurchases;
                    investingActivities.LineItems.Add(new CashFlowLineItemDto
                    {
                        AccountId = account.AccountId,
                        AccountCode = account.AccountNumber,
                        DescriptionEn = $"Purchase of {account.AccountNameEn}",
                        DescriptionAr = $"شراء {account.AccountNameAr}",
                        Category = CashFlowCategory.Investing,
                        Amount = -netPurchases,
                        SortOrder = 42
                    });
                }
                else if (netPurchases < 0)
                {
                    investingActivities.SaleOfIntangibles += Math.Abs(netPurchases);
                    investingActivities.LineItems.Add(new CashFlowLineItemDto
                    {
                        AccountId = account.AccountId,
                        AccountCode = account.AccountNumber,
                        DescriptionEn = $"Sale of {account.AccountNameEn}",
                        DescriptionAr = $"بيع {account.AccountNameAr}",
                        Category = CashFlowCategory.Investing,
                        Amount = Math.Abs(netPurchases),
                        SortOrder = 43
                    });
                }
            }

            // 3. Investments - Use Investment flag first, fallback to string matching
            var investmentAccounts = accounts.Where(a =>
                a.IsActive && a.AccountFlags.HasFlag(AccountFlags.Investment)).ToList();

            // Fallback for backward compatibility (remove after data migration)
            if (!investmentAccounts.Any())
            {
                investmentAccounts = accounts.Where(a =>
                    a.AccountType == AccountType.Asset &&
                    a.IsActive &&
                    (a.AccountNameEn?.ToLower().Contains("investment") == true ||
                     a.AccountNameAr?.Contains("استثمار") == true)).ToList();
            }

            foreach (var account in investmentAccounts)
            {
                var balance = _entryRepository.GetAccountBalance(account.AccountId, periodStart, periodEnd);
                // Use normalBalance-aware calculation - positive = increase in asset (purchase)
                var netPurchases = _normalBalanceResolver.CalculateBalance(
                    balance.TotalDebit,
                    balance.TotalCredit,
                    account.NormalBalance);

                if (netPurchases > 0)
                {
                    investingActivities.PurchaseOfInvestments += netPurchases;
                    investingActivities.LineItems.Add(new CashFlowLineItemDto
                    {
                        AccountId = account.AccountId,
                        AccountCode = account.AccountNumber,
                        DescriptionEn = $"Purchase of {account.AccountNameEn}",
                        DescriptionAr = $"شراء {account.AccountNameAr}",
                        Category = CashFlowCategory.Investing,
                        Amount = -netPurchases,
                        SortOrder = 44
                    });
                }
                else if (netPurchases < 0)
                {
                    investingActivities.SaleOfInvestments += Math.Abs(netPurchases);
                    investingActivities.LineItems.Add(new CashFlowLineItemDto
                    {
                        AccountId = account.AccountId,
                        AccountCode = account.AccountNumber,
                        DescriptionEn = $"Sale of {account.AccountNameEn}",
                        DescriptionAr = $"بيع {account.AccountNameAr}",
                        Category = CashFlowCategory.Investing,
                        Amount = Math.Abs(netPurchases),
                        SortOrder = 45
                    });
                }
            }

            return investingActivities;
        }

        /// <summary>
        /// Calculate Investing Activities async
        /// </summary>
        private async Task<CashFlowInvestingActivitiesDto> CalculateInvestingActivitiesAsync(
            List<GLAccount> accounts, DateTime periodStart, DateTime periodEnd, long? companyId)
        {
            return await Task.Run(() => CalculateInvestingActivities(accounts, periodStart, periodEnd, companyId));
        }

        /// <summary>
        /// Calculate Financing Activities
        /// IAS 7.17: Cash flows from changes in equity and borrowings
        /// </summary>
        private CashFlowFinancingActivitiesDto CalculateFinancingActivities(
            List<GLAccount> accounts, DateTime periodStart, DateTime periodEnd, long? companyId)
        {
            var financingActivities = new CashFlowFinancingActivitiesDto
            {
                LineItems = new List<CashFlowLineItemDto>()
            };

            // 1. Share Capital / Equity Issuance
            var equityAccounts = accounts.Where(a =>
                a.AccountType == AccountType.Equity &&
                a.IsActive &&
                (a.AccountNameEn?.ToLower().Contains("capital") == true ||
                 a.AccountNameEn?.ToLower().Contains("share") == true ||
                 a.AccountNameAr?.Contains("رأس مال") == true ||
                 a.AccountNameAr?.Contains("أسهم") == true)).ToList();

            foreach (var account in equityAccounts)
            {
                var balance = _entryRepository.GetAccountBalance(account.AccountId, periodStart, periodEnd);
                // Use normalBalance-aware calculation - positive = increase in equity (cash inflow)
                var netIncrease = _normalBalanceResolver.CalculateBalance(
                    balance.TotalDebit,
                    balance.TotalCredit,
                    account.NormalBalance);

                if (netIncrease > 0)
                {
                    financingActivities.ProceedsFromIssuingShares += netIncrease;
                    financingActivities.LineItems.Add(new CashFlowLineItemDto
                    {
                        AccountId = account.AccountId,
                        AccountCode = account.AccountNumber,
                        DescriptionEn = $"Proceeds from {account.AccountNameEn}",
                        DescriptionAr = $"متحصلات من {account.AccountNameAr}",
                        Category = CashFlowCategory.Financing,
                        Amount = netIncrease,
                        SortOrder = 50
                    });
                }
                else if (netIncrease < 0)
                {
                    financingActivities.RepurchaseOfShares += Math.Abs(netIncrease);
                    financingActivities.LineItems.Add(new CashFlowLineItemDto
                    {
                        AccountId = account.AccountId,
                        AccountCode = account.AccountNumber,
                        DescriptionEn = $"Repurchase of {account.AccountNameEn}",
                        DescriptionAr = $"إعادة شراء {account.AccountNameAr}",
                        Category = CashFlowCategory.Financing,
                        Amount = netIncrease, // Negative for outflow
                        SortOrder = 51
                    });
                }
            }

            // 2. Borrowings / Loans (Long-term liabilities) - Use Loan flag first, fallback to string matching
            var loanAccounts = accounts.Where(a =>
                a.IsActive && a.AccountFlags.HasFlag(AccountFlags.Loan)).ToList();

            // Fallback for backward compatibility (remove after data migration)
            if (!loanAccounts.Any())
            {
                loanAccounts = accounts.Where(a =>
                    a.AccountType == AccountType.Liability &&
                    a.IsActive &&
                    (a.AccountNameEn?.ToLower().Contains("loan") == true ||
                     a.AccountNameEn?.ToLower().Contains("borrowing") == true ||
                     a.AccountNameEn?.ToLower().Contains("long-term") == true ||
                     a.AccountNameEn?.ToLower().Contains("debt") == true ||
                     a.AccountNameAr?.Contains("قرض") == true ||
                     a.AccountNameAr?.Contains("اقتراض") == true ||
                     a.AccountNameAr?.Contains("طويل الأجل") == true)).ToList();
            }

            foreach (var account in loanAccounts)
            {
                var balance = _entryRepository.GetAccountBalance(account.AccountId, periodStart, periodEnd);
                // Use normalBalance-aware calculation - positive = increase in liability (cash inflow)
                var netIncrease = _normalBalanceResolver.CalculateBalance(
                    balance.TotalDebit,
                    balance.TotalCredit,
                    account.NormalBalance);

                if (netIncrease > 0)
                {
                    financingActivities.ProceedsFromBorrowings += netIncrease;
                    financingActivities.LineItems.Add(new CashFlowLineItemDto
                    {
                        AccountId = account.AccountId,
                        AccountCode = account.AccountNumber,
                        DescriptionEn = $"Proceeds from {account.AccountNameEn}",
                        DescriptionAr = $"متحصلات من {account.AccountNameAr}",
                        Category = CashFlowCategory.Financing,
                        Amount = netIncrease,
                        SortOrder = 52
                    });
                }
                else if (netIncrease < 0)
                {
                    financingActivities.RepaymentOfBorrowings += Math.Abs(netIncrease);
                    financingActivities.LineItems.Add(new CashFlowLineItemDto
                    {
                        AccountId = account.AccountId,
                        AccountCode = account.AccountNumber,
                        DescriptionEn = $"Repayment of {account.AccountNameEn}",
                        DescriptionAr = $"سداد {account.AccountNameAr}",
                        Category = CashFlowCategory.Financing,
                        Amount = netIncrease, // Negative for outflow
                        SortOrder = 53
                    });
                }
            }

            // 3. Dividends (Contra-Equity or direct payment accounts) - Use Dividend flag first
            var dividendAccounts = accounts.Where(a =>
                a.IsActive && a.AccountFlags.HasFlag(AccountFlags.Dividend)).ToList();

            // Fallback for backward compatibility (remove after data migration)
            if (!dividendAccounts.Any())
            {
                dividendAccounts = accounts.Where(a =>
                    a.IsActive &&
                    (a.AccountNameEn?.ToLower().Contains("dividend") == true ||
                     a.AccountNameAr?.Contains("أرباح موزعة") == true ||
                     a.AccountNameAr?.Contains("توزيعات") == true)).ToList();
            }

            foreach (var account in dividendAccounts)
            {
                var balance = _entryRepository.GetAccountBalance(account.AccountId, periodStart, periodEnd);
                var dividendsPaid = balance.TotalDebit; // Dividends are debited when paid

                if (dividendsPaid > 0)
                {
                    financingActivities.DividendsPaid += dividendsPaid;
                    financingActivities.LineItems.Add(new CashFlowLineItemDto
                    {
                        AccountId = account.AccountId,
                        AccountCode = account.AccountNumber,
                        DescriptionEn = $"Dividends Paid - {account.AccountNameEn}",
                        DescriptionAr = $"أرباح موزعة مدفوعة - {account.AccountNameAr}",
                        Category = CashFlowCategory.Financing,
                        Amount = -dividendsPaid, // Negative for outflow
                        SortOrder = 54
                    });
                }
            }

            return financingActivities;
        }

        /// <summary>
        /// Calculate Financing Activities async
        /// </summary>
        private async Task<CashFlowFinancingActivitiesDto> CalculateFinancingActivitiesAsync(
            List<GLAccount> accounts, DateTime periodStart, DateTime periodEnd, long? companyId)
        {
            return await Task.Run(() => CalculateFinancingActivities(accounts, periodStart, periodEnd, companyId));
        }

        #endregion

        #region Cost Center & Project P&L Reports - تقارير الربح والخسارة حسب مركز التكلفة والمشروع

        /// <summary>
        /// Get Cost Center P&L Report - تقرير الربح والخسارة حسب مركز التكلفة
        /// Groups Revenue and Expense accounts by Cost Center
        /// </summary>
        public CostCenterPLReportDto GetCostCenterPLReport(CostCenterPLQueryDto query)
        {
            var report = new CostCenterPLReportDto
            {
                PeriodStartDate = query.PeriodStartDate,
                PeriodEndDate = query.PeriodEndDate,
                Currency = query.Currency,
                CompanyId = query.CompanyId,
                GeneratedAt = DateTime.Now,
                Items = new List<CostCenterPLItemDto>()
            };

            // Get all accounts (Revenue and Expense only)
            var accounts = _accountRepository.ListAll(null)
                .Where(a => a.IsActive && (a.AccountType == AccountType.Revenue || a.AccountType == AccountType.Expense))
                .ToList();

            if (query.CompanyId.HasValue)
            {
                accounts = accounts.Where(a => a.CompanyID == query.CompanyId).ToList();
            }

            var accountDict = accounts.ToDictionary(a => a.AccountId, a => a);
            var accountIds = accounts.Select(a => a.AccountId).ToList();

            // Get journal entries for the period with proper join to GLJournal for date filtering
            var entries = _entryRepository.GetEntriesForDimensionPL(
                query.PeriodStartDate,
                query.PeriodEndDate,
                accountIds,
                query.CompanyId,
                query.CostCenterId,
                null); // null for projectId since this is cost center report

            // Group entries by CostCenterId
            var groupedByCostCenter = entries
                .GroupBy(e => e.CostCenterId)
                .OrderBy(g => g.Key.HasValue ? 0 : 1) // Assigned first, unassigned last
                .ThenBy(g => g.Key);

            foreach (var costCenterGroup in groupedByCostCenter)
            {
                // Skip unassigned if not requested
                if (!costCenterGroup.Key.HasValue && !query.IncludeUnassigned)
                    continue;

                var item = new CostCenterPLItemDto
                {
                    CostCenterId = costCenterGroup.Key,
                    RevenueAccounts = new List<PLAccountLineDto>(),
                    ExpenseAccounts = new List<PLAccountLineDto>()
                };

                // Get cost center details (if assigned)
                if (costCenterGroup.Key.HasValue)
                {
                    // Note: We store Guid? in GLJournalEntry.CostCenterId but CostCenter uses long
                    // This is a known type mismatch - cost center lookup would need adjustment
                    item.CostCenterCode = costCenterGroup.Key.Value.ToString().Substring(0, 8);
                    item.CostCenterNameEn = $"Cost Center {costCenterGroup.Key.Value.ToString().Substring(0, 8)}";
                    item.CostCenterNameAr = $"مركز التكلفة {costCenterGroup.Key.Value.ToString().Substring(0, 8)}";
                }
                else
                {
                    item.CostCenterCode = "UNASSIGNED";
                    item.CostCenterNameEn = "Unassigned";
                    item.CostCenterNameAr = "غير مخصص";
                }

                // Group by account within each cost center
                var accountGroups = costCenterGroup
                    .GroupBy(e => e.AccountId)
                    .Where(g => accountDict.ContainsKey(g.Key));

                foreach (var accountGroup in accountGroups)
                {
                    var account = accountDict[accountGroup.Key];
                    var totalDebit = accountGroup.Sum(e => e.BaseDebit);
                    var totalCredit = accountGroup.Sum(e => e.BaseCredit);
                    var transactionCount = accountGroup.Count();

                    var accountLine = new PLAccountLineDto
                    {
                        AccountId = account.AccountId,
                        AccountCode = account.AccountNumber,
                        AccountNameEn = account.AccountNameEn,
                        AccountNameAr = account.AccountNameAr,
                        AccountType = account.AccountType.ToString(),
                        TotalDebit = totalDebit,
                        TotalCredit = totalCredit,
                        TransactionCount = transactionCount
                    };

                    // Calculate amount based on account type
                    if (account.AccountType == AccountType.Revenue)
                    {
                        // Revenue: Credit - Debit (positive = revenue)
                        accountLine.Amount = totalCredit - totalDebit;
                        item.RevenueAccounts.Add(accountLine);
                    }
                    else if (account.AccountType == AccountType.Expense)
                    {
                        // Expense: Debit - Credit (positive = expense)
                        accountLine.Amount = totalDebit - totalCredit;
                        item.ExpenseAccounts.Add(accountLine);
                    }
                }

                // Sort accounts by code
                item.RevenueAccounts = item.RevenueAccounts.OrderBy(a => a.AccountCode).ToList();
                item.ExpenseAccounts = item.ExpenseAccounts.OrderBy(a => a.AccountCode).ToList();

                report.Items.Add(item);
            }

            return report;
        }

        /// <summary>
        /// Get Cost Center P&L Report async
        /// </summary>
        public async Task<CostCenterPLReportDto> GetCostCenterPLReportAsync(CostCenterPLQueryDto query)
        {
            return await Task.Run(() => GetCostCenterPLReport(query));
        }

        /// <summary>
        /// Get Project P&L Report - تقرير الربح والخسارة حسب المشروع
        /// Groups Revenue and Expense accounts by Project
        /// </summary>
        public ProjectPLReportDto GetProjectPLReport(ProjectPLQueryDto query)
        {
            var report = new ProjectPLReportDto
            {
                PeriodStartDate = query.PeriodStartDate,
                PeriodEndDate = query.PeriodEndDate,
                Currency = query.Currency,
                CompanyId = query.CompanyId,
                GeneratedAt = DateTime.Now,
                Items = new List<ProjectPLItemDto>()
            };

            // Get all accounts (Revenue and Expense only)
            var accounts = _accountRepository.ListAll(null)
                .Where(a => a.IsActive && (a.AccountType == AccountType.Revenue || a.AccountType == AccountType.Expense))
                .ToList();

            if (query.CompanyId.HasValue)
            {
                accounts = accounts.Where(a => a.CompanyID == query.CompanyId).ToList();
            }

            var accountDict = accounts.ToDictionary(a => a.AccountId, a => a);
            var accountIds = accounts.Select(a => a.AccountId).ToList();

            // Get journal entries for the period with proper join to GLJournal for date filtering
            var entries = _entryRepository.GetEntriesForDimensionPL(
                query.PeriodStartDate,
                query.PeriodEndDate,
                accountIds,
                query.CompanyId,
                null, // null for costCenterId since this is project report
                query.ProjectId);

            // Group entries by ProjectId
            var groupedByProject = entries
                .GroupBy(e => e.ProjectId)
                .OrderBy(g => g.Key.HasValue ? 0 : 1) // Assigned first, unassigned last
                .ThenBy(g => g.Key);

            foreach (var projectGroup in groupedByProject)
            {
                // Skip unassigned if not requested
                if (!projectGroup.Key.HasValue && !query.IncludeUnassigned)
                    continue;

                var item = new ProjectPLItemDto
                {
                    ProjectId = projectGroup.Key,
                    RevenueAccounts = new List<PLAccountLineDto>(),
                    ExpenseAccounts = new List<PLAccountLineDto>()
                };

                // Get project details (if assigned)
                if (projectGroup.Key.HasValue)
                {
                    item.ProjectCode = projectGroup.Key.Value.ToString().Substring(0, 8);
                    item.ProjectNameEn = $"Project {projectGroup.Key.Value.ToString().Substring(0, 8)}";
                    item.ProjectNameAr = $"مشروع {projectGroup.Key.Value.ToString().Substring(0, 8)}";
                    item.ProjectStatus = "Active"; // Would need project lookup for actual status
                }
                else
                {
                    item.ProjectCode = "UNASSIGNED";
                    item.ProjectNameEn = "Unassigned";
                    item.ProjectNameAr = "غير مخصص";
                    item.ProjectStatus = "N/A";
                }

                // Group by account within each project
                var accountGroups = projectGroup
                    .GroupBy(e => e.AccountId)
                    .Where(g => accountDict.ContainsKey(g.Key));

                foreach (var accountGroup in accountGroups)
                {
                    var account = accountDict[accountGroup.Key];
                    var totalDebit = accountGroup.Sum(e => e.BaseDebit);
                    var totalCredit = accountGroup.Sum(e => e.BaseCredit);
                    var transactionCount = accountGroup.Count();

                    var accountLine = new PLAccountLineDto
                    {
                        AccountId = account.AccountId,
                        AccountCode = account.AccountNumber,
                        AccountNameEn = account.AccountNameEn,
                        AccountNameAr = account.AccountNameAr,
                        AccountType = account.AccountType.ToString(),
                        TotalDebit = totalDebit,
                        TotalCredit = totalCredit,
                        TransactionCount = transactionCount
                    };

                    // Calculate amount based on account type
                    if (account.AccountType == AccountType.Revenue)
                    {
                        // Revenue: Credit - Debit (positive = revenue)
                        accountLine.Amount = totalCredit - totalDebit;
                        item.RevenueAccounts.Add(accountLine);
                    }
                    else if (account.AccountType == AccountType.Expense)
                    {
                        // Expense: Debit - Credit (positive = expense)
                        accountLine.Amount = totalDebit - totalCredit;
                        item.ExpenseAccounts.Add(accountLine);
                    }
                }

                // Sort accounts by code
                item.RevenueAccounts = item.RevenueAccounts.OrderBy(a => a.AccountCode).ToList();
                item.ExpenseAccounts = item.ExpenseAccounts.OrderBy(a => a.AccountCode).ToList();

                report.Items.Add(item);
            }

            return report;
        }

        /// <summary>
        /// Get Project P&L Report async
        /// </summary>
        public async Task<ProjectPLReportDto> GetProjectPLReportAsync(ProjectPLQueryDto query)
        {
            return await Task.Run(() => GetProjectPLReport(query));
        }

        /// <summary>
        /// Get Cost Center P&L Summary for dashboard widgets
        /// Returns top N cost centers by net profit/loss
        /// </summary>
        public List<DimensionPLSummaryDto> GetCostCenterPLSummary(DateTime periodStart, DateTime periodEnd, long? companyId = null, int topN = 10)
        {
            var query = new CostCenterPLQueryDto
            {
                PeriodStartDate = periodStart,
                PeriodEndDate = periodEnd,
                CompanyId = companyId,
                IncludeUnassigned = false // Exclude unassigned from summary
            };

            var report = GetCostCenterPLReport(query);

            return report.Items
                .OrderByDescending(i => Math.Abs(i.NetProfitLoss))
                .Take(topN)
                .Select(i => new DimensionPLSummaryDto
                {
                    DimensionType = "CostCenter",
                    DimensionId = i.CostCenterId,
                    DimensionCode = i.CostCenterCode,
                    DimensionName = i.CostCenterNameEn,
                    TotalRevenue = i.TotalRevenue,
                    TotalExpenses = i.TotalExpenses,
                    NetProfitLoss = i.NetProfitLoss,
                    ProfitMarginPercent = i.ProfitMarginPercent,
                    IsProfitable = i.IsProfitable
                })
                .ToList();
        }

        /// <summary>
        /// Get Project P&L Summary for dashboard widgets
        /// Returns top N projects by net profit/loss
        /// </summary>
        public List<DimensionPLSummaryDto> GetProjectPLSummary(DateTime periodStart, DateTime periodEnd, long? companyId = null, int topN = 10)
        {
            var query = new ProjectPLQueryDto
            {
                PeriodStartDate = periodStart,
                PeriodEndDate = periodEnd,
                CompanyId = companyId,
                IncludeUnassigned = false // Exclude unassigned from summary
            };

            var report = GetProjectPLReport(query);

            return report.Items
                .OrderByDescending(i => Math.Abs(i.NetProfitLoss))
                .Take(topN)
                .Select(i => new DimensionPLSummaryDto
                {
                    DimensionType = "Project",
                    DimensionId = i.ProjectId,
                    DimensionCode = i.ProjectCode,
                    DimensionName = i.ProjectNameEn,
                    TotalRevenue = i.TotalRevenue,
                    TotalExpenses = i.TotalExpenses,
                    NetProfitLoss = i.NetProfitLoss,
                    ProfitMarginPercent = i.ProfitMarginPercent,
                    IsProfitable = i.IsProfitable
                })
                .ToList();
        }

        #endregion
    }
}

