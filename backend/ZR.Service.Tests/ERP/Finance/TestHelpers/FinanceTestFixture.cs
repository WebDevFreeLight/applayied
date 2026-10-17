using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using SqlSugar;
using ZR.Model.Finance;
using ZR.Model.Finance.Enums;
using ZR.Model.Finance.Configuration;
using ZR.Model.Finance.Configuration.Enums;
using ZR.Model.Finance.Assets;
using ZR.Model.Finance.Assets.Enums;
using ZR.Repository.Finance;

namespace ZR.Service.Tests.ERP.Finance.TestHelpers
{
    /// <summary>
    /// Test fixture for Finance module tests - إعداد الاختبارات لوحدة المالية
    /// </summary>
    public class FinanceTestFixture : IDisposable
    {
        private readonly SqliteConnection _connection;
        public ISqlSugarClient Db { get; private set; }
        public Guid TestTenantId { get; private set; }
        public Guid TestUserId { get; private set; }

        public FinanceTestFixture()
        {
            // Create in-memory SQLite database with shared cache for testing
            // The shared cache mode allows multiple connections to access the same database
            var connectionString = $"Data Source=TestDb_{Guid.NewGuid():N};Mode=Memory;Cache=Shared";
            _connection = new SqliteConnection(connectionString);
            _connection.Open(); // Keep connection open to prevent database from being destroyed

            // Configure SqlSugar with the same SQLite connection
            Db = new SqlSugarClient(new ConnectionConfig
            {
                ConnectionString = connectionString,
                DbType = DbType.Sqlite,
                IsAutoCloseConnection = false, // Keep connections open for in-memory database
                InitKeyType = InitKeyType.Attribute
            });

            // Create tables
            CreateTables();

            // Initialize test data
            TestTenantId = Guid.NewGuid();
            TestUserId = Guid.NewGuid();
        }

        private void CreateTables()
        {
            // Create GL Account table
            Db.CodeFirst.InitTables<GLAccount>();

            // Create GL Journal tables (replaces GLVoucher)
            Db.CodeFirst.InitTables<GLJournal>();
            Db.CodeFirst.InitTables<GLJournalEntry>();

            // Create Financial Report table
            Db.CodeFirst.InitTables<GLFinancialReport>();

            // Create Period Closing table
            Db.CodeFirst.InitTables<GLPeriodClosing>();

            // Create System Configuration table
            Db.CodeFirst.InitTables<SystemConfiguration>();

            // Create Currency table
            Db.CodeFirst.InitTables<Currency>();

            // Create Fixed Asset tables
            Db.CodeFirst.InitTables<FixedAsset>();
            Db.CodeFirst.InitTables<AssetDepreciation>();
            Db.CodeFirst.InitTables<AssetCategory>();
        }

        public void Dispose()
        {
            Db?.Dispose();
            _connection?.Close();
            _connection?.Dispose();
        }
    }

    /// <summary>
    /// Test data builder for Finance entities - باني بيانات الاختبار
    /// </summary>
    public class FinanceTestDataBuilder
    {
        private readonly ISqlSugarClient _db;
        private readonly Guid _tenantId;
        private readonly Guid _userId;

        public FinanceTestDataBuilder(ISqlSugarClient db, Guid tenantId, Guid userId)
        {
            _db = db;
            _tenantId = tenantId;
            _userId = userId;
        }

        /// <summary>
        /// Create a test GL Account - إنشاء حساب تجريبي
        /// Updated to support new IFRS-compliant model with FullAccountNumber, AccountPath, ChildCount, DescendantCount
        /// </summary>
        public GLAccount CreateAccount(
            string accountNumber = "1000",
            string nameAr = "حساب تجريبي",
            string nameEn = "Test Account",
            AccountType accountType = AccountType.Asset,
            int level = 1,
            Guid? parentAccountId = null,
            bool isActive = true,
            bool isLeaf = true,
            bool isSystemAccount = false,
            bool allowPostingToParent = false,
            bool isCashAccount = false,
            bool isBankAccount = false,
            bool isControlAccount = false,
            long? companyId = null)
        {
            // Determine normal balance based on account type
            var normalBalance = (accountType == AccountType.Asset || accountType == AccountType.Expense)
                ? NormalBalance.Debit
                : NormalBalance.Credit;

            // Build FullAccountNumber based on parent
            string fullAccountNumber = accountNumber;
            string accountPath = "";

            if (parentAccountId.HasValue)
            {
                var parent = _db.Queryable<GLAccount>().First(a => a.AccountId == parentAccountId.Value);
                if (parent != null)
                {
                    fullAccountNumber = $"{parent.FullAccountNumber}.{accountNumber}";
                    accountPath = string.IsNullOrEmpty(parent.AccountPath)
                        ? parent.AccountId.ToString()
                        : parent.AccountPath;
                    level = parent.Level + 1;

                    // Update parent's IsLeaf and ChildCount
                    parent.IsLeaf = false;
                    parent.ChildCount++;
                    _db.Updateable(parent).ExecuteCommand();

                    // Update ancestor DescendantCount
                    UpdateAncestorDescendantCounts(parent.AccountId, 1);
                }
            }

            var account = new GLAccount
            {
                AccountId = Guid.NewGuid(),
                AccountNumber = accountNumber,
                FullAccountNumber = fullAccountNumber,
                AccountNameAr = nameAr,
                AccountNameEn = nameEn,
                AccountType = accountType,
                NormalBalance = normalBalance,
                Level = level,
                ParentAccountId = parentAccountId,
                IsActive = isActive,
                IsLeaf = isLeaf,
                IsSystemAccount = isSystemAccount,
                AllowManualEntries = true,
                AllowPostingToParent = allowPostingToParent,
                RequiresCostCenter = false,
                RequiresDepartment = false,
                RequiresProject = false,
                IsCashAccount = isCashAccount,
                IsBankAccount = isBankAccount,
                IsControlAccount = isControlAccount,
                IsReconciliationAccount = false,
                Currency = "SAR",
                IsForeignCurrency = false,
                DescriptionAr = $"وصف بالعربي - {nameAr}",
                DescriptionEn = $"Description in English - {nameEn}",
                DisplayOrder = 0,
                ChildCount = 0,
                DescendantCount = 0,
                RowVersion = 1,
                CompanyID = companyId,
                Create_by = _userId.ToString(),
                Create_time = DateTime.Now
            };

            // Set AccountPath after AccountId is generated
            account.AccountPath = string.IsNullOrEmpty(accountPath)
                ? account.AccountId.ToString()
                : $"{accountPath}/{account.AccountId}";

            _db.Insertable(account).ExecuteCommand();
            return account;
        }

        /// <summary>
        /// Update ancestor descendant counts recursively
        /// </summary>
        private void UpdateAncestorDescendantCounts(Guid accountId, int delta)
        {
            var currentId = accountId;
            while (true)
            {
                var account = _db.Queryable<GLAccount>().First(a => a.AccountId == currentId);
                if (account == null) break;

                account.DescendantCount += delta;
                _db.Updateable(account).UpdateColumns(a => a.DescendantCount).ExecuteCommand();

                if (!account.ParentAccountId.HasValue) break;
                currentId = account.ParentAccountId.Value;
            }
        }

        /// <summary>
        /// Create a test GL Account with legacy string-based AccountType (backward compatibility)
        /// </summary>
        public GLAccount CreateAccountLegacy(
            string accountNumber = "1000",
            string nameAr = "حساب تجريبي",
            string nameEn = "Test Account",
            string accountTypeStr = "asset",
            int level = 1,
            Guid? parentAccountId = null,
            bool isActive = true,
            bool isLeaf = true)
        {
            var accountType = accountTypeStr.ToLower() switch
            {
                "asset" => AccountType.Asset,
                "liability" => AccountType.Liability,
                "equity" => AccountType.Equity,
                "revenue" => AccountType.Revenue,
                "expense" => AccountType.Expense,
                _ => AccountType.Asset
            };

            return CreateAccount(accountNumber, nameAr, nameEn, accountType, level, parentAccountId, isActive, isLeaf);
        }

        /// <summary>
        /// Create a test GL Journal - إنشاء قيد تجريبي
        /// </summary>
        public GLJournal CreateJournal(
            string? journalNumber = null,
            DateTime? journalDate = null,
            string descriptionAr = "قيد تجريبي",
            string descriptionEn = "Test Journal",
            JournalStatus status = JournalStatus.Draft,
            int fiscalYear = 0,
            int fiscalPeriod = 0)
        {
            if (journalNumber == null)
                journalNumber = $"JRN-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8)}";

            if (journalDate == null)
                journalDate = DateTime.Now;

            if (fiscalYear == 0)
                fiscalYear = journalDate.Value.Year;

            if (fiscalPeriod == 0)
                fiscalPeriod = journalDate.Value.Month;

            var journal = new GLJournal
            {
                Id = Guid.NewGuid(),
                JournalNumber = journalNumber,
                JournalDate = journalDate.Value,
                DescriptionAr = descriptionAr,
                DescriptionEn = descriptionEn,
                Status = status,
                TotalDebit = 0,
                TotalCredit = 0,
                Currency = "SAR",
                FiscalYear = fiscalYear,
                FiscalPeriod = fiscalPeriod,
                Remark = $"Test journal: {descriptionEn}",
                CompanyID = _tenantId.GetHashCode(), // Convert Guid to long for CompanyID
                Create_by = _userId.ToString(),
                Create_time = DateTime.Now
            };

            _db.Insertable(journal).ExecuteCommand();
            return journal;
        }

        /// <summary>
        /// Create a test GL Journal Entry - إنشاء سطر قيد تجريبي
        /// </summary>
        public GLJournalEntry CreateJournalEntry(
            Guid journalId,
            Guid accountId,
            decimal debit = 0,
            decimal credit = 0,
            int lineNumber = 1,
            string descriptionAr = "سطر قيد تجريبي",
            string descriptionEn = "Test Entry")
        {
            var entry = new GLJournalEntry
            {
                Id = Guid.NewGuid(),
                JournalId = journalId,
                AccountId = accountId,
                LineNumber = lineNumber,
                Debit = debit,
                Credit = credit,
                DescriptionAr = descriptionAr,
                DescriptionEn = descriptionEn,
                Currency = "SAR",
                ExchangeRate = 1,
                BaseDebit = debit,
                BaseCredit = credit,
                Remark = $"Test entry: {descriptionEn}",
                CompanyID = _tenantId.GetHashCode(),
                Create_by = _userId.ToString(),
                Create_time = DateTime.Now
            };

            _db.Insertable(entry).ExecuteCommand();
            return entry;
        }

        /// <summary>
        /// Create a balanced journal with entries - إنشاء قيد متوازن مع السطور
        /// </summary>
        public (GLJournal Journal, List<GLJournalEntry> Entries) CreateBalancedJournal(
            Guid debitAccountId,
            Guid creditAccountId,
            decimal amount,
            DateTime? journalDate = null)
        {
            var journal = CreateJournal(journalDate: journalDate);

            var entries = new List<GLJournalEntry>
            {
                CreateJournalEntry(journal.Id, debitAccountId, debit: amount, lineNumber: 1),
                CreateJournalEntry(journal.Id, creditAccountId, credit: amount, lineNumber: 2)
            };

            // Update journal totals
            journal.TotalDebit = amount;
            journal.TotalCredit = amount;
            _db.Updateable(journal).ExecuteCommand();

            return (journal, entries);
        }

        // Backward compatibility aliases
        public GLJournal CreateVoucher(
            string? voucherNumber = null,
            DateTime? voucherDate = null,
            string descriptionAr = "قيد تجريبي",
            string descriptionEn = "Test Voucher",
            JournalStatus status = JournalStatus.Draft,
            int fiscalYear = 0,
            int fiscalPeriod = 0)
            => CreateJournal(voucherNumber, voucherDate, descriptionAr, descriptionEn, status, fiscalYear, fiscalPeriod);

        public GLJournalEntry CreateVoucherEntry(
            Guid voucherId,
            Guid accountId,
            decimal debit = 0,
            decimal credit = 0,
            int lineNumber = 1,
            string descriptionAr = "سطر قيد تجريبي",
            string descriptionEn = "Test Entry")
            => CreateJournalEntry(voucherId, accountId, debit, credit, lineNumber, descriptionAr, descriptionEn);

        public (GLJournal Voucher, List<GLJournalEntry> Entries) CreateBalancedVoucher(
            Guid debitAccountId,
            Guid creditAccountId,
            decimal amount,
            DateTime? voucherDate = null)
            => CreateBalancedJournal(debitAccountId, creditAccountId, amount, voucherDate);

        /// <summary>
        /// Create a test Period Closing - إنشاء فترة تجريبية
        /// </summary>
        public GLPeriodClosing CreatePeriod(
            DateTime periodStart,
            DateTime periodEnd,
            PeriodType periodType = PeriodType.Month,
            PeriodStatus status = PeriodStatus.Open,
            int fiscalYear = 0,
            int fiscalPeriod = 0)
        {
            if (fiscalYear == 0)
                fiscalYear = periodStart.Year;

            if (fiscalPeriod == 0)
                fiscalPeriod = periodStart.Month;

            var period = new GLPeriodClosing
            {
                Id = Guid.NewGuid(),
                PeriodType = periodType,
                PeriodStartDate = periodStart.Date,
                PeriodEndDate = periodEnd.Date,
                FiscalYear = fiscalYear,
                FiscalPeriod = fiscalPeriod,
                Status = status,
                DescriptionAr = $"فترة تجريبية من {periodStart:yyyy-MM-dd} إلى {periodEnd:yyyy-MM-dd}",
                DescriptionEn = $"Test period from {periodStart:yyyy-MM-dd} to {periodEnd:yyyy-MM-dd}",
                Remark = $"Test period closing for {periodStart:yyyy-MM-dd} to {periodEnd:yyyy-MM-dd}",
                CompanyID = _tenantId.GetHashCode(),
                Create_by = _userId.ToString(),
                Create_time = DateTime.Now
            };

            _db.Insertable(period).ExecuteCommand();
            return period;
        }

        /// <summary>
        /// Create standard chart of accounts - إنشاء دليل حسابات قياسي
        /// Updated for new IFRS-compliant model
        /// </summary>
        public Dictionary<string, GLAccount> CreateStandardChartOfAccounts(long? companyId = null)
        {
            var accounts = new Dictionary<string, GLAccount>();

            // Assets
            var cashAccount = CreateAccount("1000", "النقدية", "Cash", AccountType.Asset, 1, null, true, true, false, false, true, false, false, companyId);
            accounts["Cash"] = cashAccount;

            var bankAccount = CreateAccount("1010", "البنك", "Bank", AccountType.Asset, 1, null, true, true, false, false, false, true, false, companyId);
            accounts["Bank"] = bankAccount;

            var receivablesAccount = CreateAccount("1200", "العملاء", "Accounts Receivable", AccountType.Asset, 1, null, true, true, false, false, false, false, true, companyId);
            accounts["Receivables"] = receivablesAccount;

            // Liabilities
            var payablesAccount = CreateAccount("2000", "الموردون", "Accounts Payable", AccountType.Liability, 1, null, true, true, false, false, false, false, true, companyId);
            accounts["Payables"] = payablesAccount;

            // Equity
            var capitalAccount = CreateAccount("3000", "رأس المال", "Capital", AccountType.Equity, 1, null, true, true, false, false, false, false, false, companyId);
            accounts["Capital"] = capitalAccount;

            // Revenue
            var revenueAccount = CreateAccount("4000", "الإيرادات", "Revenue", AccountType.Revenue, 1, null, true, true, false, false, false, false, false, companyId);
            accounts["Revenue"] = revenueAccount;

            // Expenses
            var expenseAccount = CreateAccount("5000", "المصروفات", "Expenses", AccountType.Expense, 1, null, true, true, false, false, false, false, false, companyId);
            accounts["Expenses"] = expenseAccount;

            return accounts;
        }

        /// <summary>
        /// Create a test Currency - إنشاء عملة تجريبية
        /// </summary>
        public Currency CreateCurrency(
            string currencyCode = "USD",
            string nameAr = "دولار أمريكي",
            string nameEn = "US Dollar",
            decimal exchangeRate = 3.75m,
            bool isBaseCurrency = false,
            string symbol = "$",
            int decimalPlaces = 2)
        {
            var currency = new Currency
            {
                Id = Guid.NewGuid(),
                CurrencyCode = currencyCode,
                CurrencyNameAr = nameAr,
                CurrencyNameEn = nameEn,
                ExchangeRate = exchangeRate,
                IsBaseCurrency = isBaseCurrency,
                Symbol = symbol,
                DecimalPlaces = decimalPlaces,
                Status = CurrencyStatus.Active,
                CompanyID = _tenantId.GetHashCode(),
                Create_by = _userId.ToString(),
                Create_time = DateTime.Now
            };

            _db.Insertable(currency).ExecuteCommand();
            return currency;
        }

        /// <summary>
        /// Create a multi-currency journal entry - إنشاء سطر قيد متعدد العملات
        /// </summary>
        public GLJournalEntry CreateMultiCurrencyJournalEntry(
            Guid journalId,
            Guid accountId,
            decimal debit,
            decimal credit,
            string currency,
            decimal exchangeRate,
            int lineNumber = 1)
        {
            var entry = new GLJournalEntry
            {
                Id = Guid.NewGuid(),
                JournalId = journalId,
                AccountId = accountId,
                LineNumber = lineNumber,
                Debit = debit,
                Credit = credit,
                Currency = currency,
                ExchangeRate = exchangeRate,
                BaseDebit = debit * exchangeRate,
                BaseCredit = credit * exchangeRate,
                DescriptionAr = $"قيد بعملة {currency}",
                DescriptionEn = $"Entry in {currency}",
                CompanyID = _tenantId.GetHashCode(),
                Create_by = _userId.ToString(),
                Create_time = DateTime.Now
            };

            _db.Insertable(entry).ExecuteCommand();
            return entry;
        }

        /// <summary>
        /// Create a balanced multi-currency journal - إنشاء قيد متوازن متعدد العملات
        /// </summary>
        public (GLJournal Journal, List<GLJournalEntry> Entries) CreateBalancedMultiCurrencyJournal(
            Guid debitAccountId,
            Guid creditAccountId,
            decimal amount,
            string currency,
            decimal exchangeRate,
            DateTime? journalDate = null)
        {
            var journal = CreateJournal(journalDate: journalDate);
            journal.Currency = currency;
            _db.Updateable(journal).ExecuteCommand();

            var entries = new List<GLJournalEntry>
            {
                CreateMultiCurrencyJournalEntry(journal.Id, debitAccountId, debit: amount, credit: 0, currency, exchangeRate, 1),
                CreateMultiCurrencyJournalEntry(journal.Id, creditAccountId, debit: 0, credit: amount, currency, exchangeRate, 2)
            };

            // Update journal totals in base currency
            var baseAmount = amount * exchangeRate;
            journal.TotalDebit = baseAmount;
            journal.TotalCredit = baseAmount;
            _db.Updateable(journal).ExecuteCommand();

            return (journal, entries);
        }

        // Backward compatibility aliases for multi-currency
        public GLJournalEntry CreateMultiCurrencyVoucherEntry(
            Guid voucherId,
            Guid accountId,
            decimal debit,
            decimal credit,
            string currency,
            decimal exchangeRate,
            int lineNumber = 1)
            => CreateMultiCurrencyJournalEntry(voucherId, accountId, debit, credit, currency, exchangeRate, lineNumber);

        public (GLJournal Voucher, List<GLJournalEntry> Entries) CreateBalancedMultiCurrencyVoucher(
            Guid debitAccountId,
            Guid creditAccountId,
            decimal amount,
            string currency,
            decimal exchangeRate,
            DateTime? voucherDate = null)
            => CreateBalancedMultiCurrencyJournal(debitAccountId, creditAccountId, amount, currency, exchangeRate, voucherDate);

        /// <summary>
        /// Create an Asset Category - إنشاء فئة أصل
        /// </summary>
        public AssetCategory CreateAssetCategory(
            string categoryCode = "CAT-001",
            string nameAr = "فئة تجريبية",
            string nameEn = "Test Category",
            DepreciationMethod depreciationMethod = DepreciationMethod.StraightLine,
            int usefulLifeMonths = 60)
        {
            var category = new AssetCategory
            {
                Id = Guid.NewGuid(),
                CategoryCode = categoryCode,
                NameAr = nameAr,
                NameEn = nameEn,
                DepreciationMethod = depreciationMethod,
                UsefulLifeMonths = usefulLifeMonths,
                CompanyID = _tenantId.GetHashCode(),
                Create_by = _userId.ToString(),
                Create_time = DateTime.Now
            };

            _db.Insertable(category).ExecuteCommand();
            return category;
        }

        /// <summary>
        /// Create a Fixed Asset - إنشاء أصل ثابت
        /// </summary>
        public FixedAsset CreateFixedAsset(
            string assetCode = "AST-001",
            string nameAr = "أصل تجريبي",
            string nameEn = "Test Asset",
            decimal purchaseCost = 100000,
            decimal salvageValue = 10000,
            DateTime? purchaseDate = null,
            Guid? categoryId = null,
            Guid? glAssetAccountId = null,
            Guid? glAccumulatedDepreciationAccountId = null,
            Guid? glDepreciationExpenseAccountId = null)
        {
            if (purchaseDate == null)
                purchaseDate = DateTime.Now.AddMonths(-12);

            var asset = new FixedAsset
            {
                Id = Guid.NewGuid(),
                AssetCode = assetCode,
                NameAr = nameAr,
                NameEn = nameEn,
                PurchaseDate = purchaseDate.Value,
                PurchaseCost = purchaseCost,
                SalvageValue = salvageValue,
                CategoryId = categoryId ?? Guid.NewGuid(), // Will be set by test if needed
                GLAssetAccountId = glAssetAccountId,
                GLAccumulatedDepreciationAccountId = glAccumulatedDepreciationAccountId,
                GLDepreciationExpenseAccountId = glDepreciationExpenseAccountId,
                CurrentBookValue = purchaseCost,
                AccumulatedDepreciation = 0,
                Status = AssetStatus.Active,
                CompanyID = _tenantId.GetHashCode(),
                Create_by = _userId.ToString(),
                Create_time = DateTime.Now
            };

            _db.Insertable(asset).ExecuteCommand();
            return asset;
        }

        /// <summary>
        /// Create a complex hierarchical account tree - إنشاء شجرة حسابات معقدة
        /// Updated to demonstrate FullAccountNumber hierarchy (e.g., 1000.1100.1110)
        /// </summary>
        public Dictionary<string, GLAccount> CreateComplexAccountTree(long? companyId = null)
        {
            var accounts = new Dictionary<string, GLAccount>();

            // Level 1 - Main Categories (FullAccountNumber = AccountNumber)
            var assets = CreateAccount("1000", "الأصول", "Assets", AccountType.Asset, 1, null, true, false, false, false, false, false, false, companyId);
            accounts["Assets"] = assets;

            var liabilities = CreateAccount("2000", "الخصوم", "Liabilities", AccountType.Liability, 1, null, true, false, false, false, false, false, false, companyId);
            accounts["Liabilities"] = liabilities;

            var equity = CreateAccount("3000", "حقوق الملكية", "Equity", AccountType.Equity, 1, null, true, false, false, false, false, false, false, companyId);
            accounts["Equity"] = equity;

            // Level 2 - Current Assets (FullAccountNumber = 1000.1100)
            var currentAssets = CreateAccount("1100", "الأصول المتداولة", "Current Assets", AccountType.Asset, 2, assets.AccountId, true, false, false, false, false, false, false, companyId);
            accounts["CurrentAssets"] = currentAssets;

            var fixedAssets = CreateAccount("1200", "الأصول الثابتة", "Fixed Assets", AccountType.Asset, 2, assets.AccountId, true, false, false, false, false, false, false, companyId);
            accounts["FixedAssets"] = fixedAssets;

            // Level 3 - Under Current Assets (FullAccountNumber = 1000.1100.1110)
            var cash = CreateAccount("1110", "النقدية", "Cash", AccountType.Asset, 3, currentAssets.AccountId, true, true, false, false, true, false, false, companyId);
            accounts["Cash"] = cash;

            var bank = CreateAccount("1120", "البنك", "Bank", AccountType.Asset, 3, currentAssets.AccountId, true, true, false, false, false, true, false, companyId);
            accounts["Bank"] = bank;

            var receivables = CreateAccount("1130", "العملاء", "Accounts Receivable", AccountType.Asset, 3, currentAssets.AccountId, true, true, false, false, false, false, true, companyId);
            accounts["Receivables"] = receivables;

            // Level 3 - Under Fixed Assets (FullAccountNumber = 1000.1200.1210)
            var equipment = CreateAccount("1210", "المعدات", "Equipment", AccountType.Asset, 3, fixedAssets.AccountId, true, true, false, false, false, false, false, companyId);
            accounts["Equipment"] = equipment;

            var vehicles = CreateAccount("1220", "المركبات", "Vehicles", AccountType.Asset, 3, fixedAssets.AccountId, true, true, false, false, false, false, false, companyId);
            accounts["Vehicles"] = vehicles;

            // Level 2 - Current Liabilities (FullAccountNumber = 2000.2100)
            var currentLiabilities = CreateAccount("2100", "الخصوم المتداولة", "Current Liabilities", AccountType.Liability, 2, liabilities.AccountId, true, false, false, false, false, false, false, companyId);
            accounts["CurrentLiabilities"] = currentLiabilities;

            var payables = CreateAccount("2110", "الموردون", "Accounts Payable", AccountType.Liability, 3, currentLiabilities.AccountId, true, true, false, false, false, false, true, companyId);
            accounts["Payables"] = payables;

            // Level 1 - Revenue (FullAccountNumber = 4000)
            var revenue = CreateAccount("4000", "الإيرادات", "Revenue", AccountType.Revenue, 1, null, true, false, false, false, false, false, false, companyId);
            accounts["Revenue"] = revenue;

            var salesRevenue = CreateAccount("4100", "إيرادات المبيعات", "Sales Revenue", AccountType.Revenue, 2, revenue.AccountId, true, true, false, false, false, false, false, companyId);
            accounts["SalesRevenue"] = salesRevenue;

            // Level 1 - Expenses (FullAccountNumber = 5000)
            var expenses = CreateAccount("5000", "المصروفات", "Expenses", AccountType.Expense, 1, null, true, false, false, false, false, false, false, companyId);
            accounts["Expenses"] = expenses;

            var depreciationExpense = CreateAccount("5100", "مصروف الإهلاك", "Depreciation Expense", AccountType.Expense, 2, expenses.AccountId, true, true, false, false, false, false, false, companyId);
            accounts["DepreciationExpense"] = depreciationExpense;

            return accounts;
        }

        /// <summary>
        /// Create fiscal year periods with custom start/end - إنشاء فترات سنة مالية مخصصة
        /// </summary>
        public List<GLPeriodClosing> CreateFiscalYearPeriods(
            int fiscalYear,
            DateTime? fiscalYearStart = null,
            PeriodType periodType = PeriodType.Month)
        {
            var periods = new List<GLPeriodClosing>();

            if (fiscalYearStart == null)
                fiscalYearStart = new DateTime(fiscalYear, 1, 1);

            for (int i = 0; i < 12; i++)
            {
                var periodStart = fiscalYearStart.Value.AddMonths(i);
                var periodEnd = periodStart.AddMonths(1).AddDays(-1);

                var period = CreatePeriod(
                    periodStart,
                    periodEnd,
                    periodType,
                    PeriodStatus.Open,
                    fiscalYear,
                    periodStart.Month);

                periods.Add(period);
            }

            return periods;
        }

        /// <summary>
        /// Create account with maximum field lengths - إنشاء حساب بأقصى طول للحقول
        /// </summary>
        public GLAccount CreateAccountWithMaxFieldLengths()
        {
            var maxNameAr = new string('أ', 200); // 200 Arabic characters
            var maxNameEn = new string('A', 200); // 200 English characters
            var maxAccountNumber = new string('1', 50); // 50 characters

            var account = CreateAccount(
                accountNumber: maxAccountNumber,
                nameAr: maxNameAr,
                nameEn: maxNameEn);

            // Update with max description lengths
            account.DescriptionAr = new string('و', 500); // 500 Arabic characters
            account.DescriptionEn = new string('D', 500); // 500 English characters
            _db.Updateable(account).ExecuteCommand();

            return account;
        }
    }
}


