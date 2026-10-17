using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using ZR.Model.Finance;
using ZR.Model.Finance.Enums;
using ZR.Service.Tests.ERP.Finance.TestHelpers;

namespace ZR.Service.Tests.ERP.Finance.Unit.GL
{
    /// <summary>
    /// Unit tests for GL Account Seeder - اختبارات زراعة الحسابات
    ///
    /// This test suite validates:
    /// - IFRS chart of accounts seeding
    /// - System account creation with proper flags
    /// - Hierarchy structure correctness
    /// - CompanyId isolation for multi-tenancy
    /// - Idempotent seeding (prevent duplicate seeds)
    /// </summary>
    public class GLAccountSeederTests : IClassFixture<FinanceTestFixture>
    {
        private readonly FinanceTestFixture _fixture;
        private readonly FinanceTestDataBuilder _builder;

        public GLAccountSeederTests(FinanceTestFixture fixture)
        {
            _fixture = fixture;
            _builder = new FinanceTestDataBuilder(_fixture.Db, _fixture.TestTenantId, _fixture.TestUserId);
        }

        #region IFRS Chart of Accounts Structure Tests

        /// <summary>
        /// Tests that seeding creates all 5 main account categories
        /// </summary>
        [Fact]
        public void SeedDefaultAccounts_CreatesMainCategories()
        {
            // Arrange
            long companyId = 100;
            var seeder = new TestGLAccountSeeder(_fixture.Db);

            // Act
            var count = seeder.SeedDefaultAccounts(companyId, _fixture.TestUserId.ToString());

            // Assert
            count.Should().BeGreaterThan(0);

            // Verify main categories exist
            var assets = _fixture.Db.Queryable<GLAccount>()
                .First(a => a.AccountNumber == "1000" && a.CompanyID == companyId);
            var liabilities = _fixture.Db.Queryable<GLAccount>()
                .First(a => a.AccountNumber == "2000" && a.CompanyID == companyId);
            var equity = _fixture.Db.Queryable<GLAccount>()
                .First(a => a.AccountNumber == "3000" && a.CompanyID == companyId);
            var revenue = _fixture.Db.Queryable<GLAccount>()
                .First(a => a.AccountNumber == "4000" && a.CompanyID == companyId);
            var expenses = _fixture.Db.Queryable<GLAccount>()
                .First(a => a.AccountNumber == "5000" && a.CompanyID == companyId);

            assets.Should().NotBeNull();
            liabilities.Should().NotBeNull();
            equity.Should().NotBeNull();
            revenue.Should().NotBeNull();
            expenses.Should().NotBeNull();
        }

        /// <summary>
        /// Tests that seeded accounts have correct account types
        /// </summary>
        [Fact]
        public void SeedDefaultAccounts_CorrectAccountTypes()
        {
            // Arrange
            long companyId = 101;
            var seeder = new TestGLAccountSeeder(_fixture.Db);

            // Act
            seeder.SeedDefaultAccounts(companyId, _fixture.TestUserId.ToString());

            // Assert
            var accounts = _fixture.Db.Queryable<GLAccount>()
                .Where(a => a.CompanyID == companyId)
                .ToList();

            accounts.First(a => a.AccountNumber == "1000").AccountType.Should().Be(AccountType.Asset);
            accounts.First(a => a.AccountNumber == "2000").AccountType.Should().Be(AccountType.Liability);
            accounts.First(a => a.AccountNumber == "3000").AccountType.Should().Be(AccountType.Equity);
            accounts.First(a => a.AccountNumber == "4000").AccountType.Should().Be(AccountType.Revenue);
            accounts.First(a => a.AccountNumber == "5000").AccountType.Should().Be(AccountType.Expense);
        }

        /// <summary>
        /// Tests that seeded accounts have correct normal balances
        /// </summary>
        [Fact]
        public void SeedDefaultAccounts_CorrectNormalBalances()
        {
            // Arrange
            long companyId = 102;
            var seeder = new TestGLAccountSeeder(_fixture.Db);

            // Act
            seeder.SeedDefaultAccounts(companyId, _fixture.TestUserId.ToString());

            // Assert
            var accounts = _fixture.Db.Queryable<GLAccount>()
                .Where(a => a.CompanyID == companyId)
                .ToList();

            // Assets and Expenses have Debit normal balance
            accounts.First(a => a.AccountNumber == "1000").NormalBalance.Should().Be(NormalBalance.Debit);
            accounts.First(a => a.AccountNumber == "5000").NormalBalance.Should().Be(NormalBalance.Debit);

            // Liabilities, Equity, and Revenue have Credit normal balance
            accounts.First(a => a.AccountNumber == "2000").NormalBalance.Should().Be(NormalBalance.Credit);
            accounts.First(a => a.AccountNumber == "3000").NormalBalance.Should().Be(NormalBalance.Credit);
            accounts.First(a => a.AccountNumber == "4000").NormalBalance.Should().Be(NormalBalance.Credit);
        }

        #endregion

        #region System Account Flag Tests

        /// <summary>
        /// Tests that all seeded accounts are marked as system accounts
        /// </summary>
        [Fact]
        public void SeedDefaultAccounts_AllMarkedAsSystemAccounts()
        {
            // Arrange
            long companyId = 103;
            var seeder = new TestGLAccountSeeder(_fixture.Db);

            // Act
            seeder.SeedDefaultAccounts(companyId, _fixture.TestUserId.ToString());

            // Assert
            var accounts = _fixture.Db.Queryable<GLAccount>()
                .Where(a => a.CompanyID == companyId)
                .ToList();

            accounts.Should().OnlyContain(a => a.IsSystemAccount == true);
        }

        /// <summary>
        /// Tests that system accounts cannot be modified
        /// </summary>
        [Fact]
        public void SystemAccount_CannotBeModified()
        {
            // Arrange
            long companyId = 104;
            var seeder = new TestGLAccountSeeder(_fixture.Db);
            seeder.SeedDefaultAccounts(companyId, _fixture.TestUserId.ToString());

            var systemAccount = _fixture.Db.Queryable<GLAccount>()
                .First(a => a.AccountNumber == "1000" && a.CompanyID == companyId);

            var service = new TestGLAccountService(_fixture.Db);

            // Act & Assert
            var updateDto = new ZR.Model.Finance.Dto.UpdateAccountDto
            {
                AccountId = systemAccount.AccountId,
                AccountNumber = "9999", // Try to change
                AccountNameAr = "Changed",
                AccountNameEn = "Changed",
                AccountType = AccountType.Asset,
                NormalBalance = NormalBalance.Debit,
                IsActive = true,
                IsLeaf = false,
                AllowManualEntries = true,
                RequiresCostCenter = false
            };

            // Should throw exception because it's a system account
            var action = () => service.Update(updateDto, _fixture.TestUserId.ToString());
            action.Should().Throw<Exception>().WithMessage("*system account*");
        }

        #endregion

        #region Hierarchy Structure Tests

        /// <summary>
        /// Tests that seeded accounts have correct FullAccountNumbers
        /// </summary>
        [Fact]
        public void SeedDefaultAccounts_CorrectFullAccountNumbers()
        {
            // Arrange
            long companyId = 105;
            var seeder = new TestGLAccountSeeder(_fixture.Db);

            // Act
            seeder.SeedDefaultAccounts(companyId, _fixture.TestUserId.ToString());

            // Assert
            var accounts = _fixture.Db.Queryable<GLAccount>()
                .Where(a => a.CompanyID == companyId)
                .ToList();

            // Root accounts
            accounts.First(a => a.AccountNumber == "1000").FullAccountNumber.Should().Be("1000");
            accounts.First(a => a.AccountNumber == "2000").FullAccountNumber.Should().Be("2000");

            // Level 2 accounts
            accounts.First(a => a.AccountNumber == "1100").FullAccountNumber.Should().Be("1000.1100");
            accounts.First(a => a.AccountNumber == "1200").FullAccountNumber.Should().Be("1000.1200");
            accounts.First(a => a.AccountNumber == "2100").FullAccountNumber.Should().Be("2000.2100");

            // Level 3 accounts
            accounts.First(a => a.AccountNumber == "1110").FullAccountNumber.Should().Be("1000.1100.1110");
            accounts.First(a => a.AccountNumber == "2110").FullAccountNumber.Should().Be("2000.2100.2110");
        }

        /// <summary>
        /// Tests that seeded accounts have correct levels
        /// </summary>
        [Fact]
        public void SeedDefaultAccounts_CorrectLevels()
        {
            // Arrange
            long companyId = 106;
            var seeder = new TestGLAccountSeeder(_fixture.Db);

            // Act
            seeder.SeedDefaultAccounts(companyId, _fixture.TestUserId.ToString());

            // Assert
            var accounts = _fixture.Db.Queryable<GLAccount>()
                .Where(a => a.CompanyID == companyId)
                .ToList();

            // Level 1 (root)
            accounts.First(a => a.AccountNumber == "1000").Level.Should().Be(1);
            accounts.First(a => a.AccountNumber == "2000").Level.Should().Be(1);

            // Level 2
            accounts.First(a => a.AccountNumber == "1100").Level.Should().Be(2);
            accounts.First(a => a.AccountNumber == "2100").Level.Should().Be(2);

            // Level 3
            accounts.First(a => a.AccountNumber == "1110").Level.Should().Be(3);
        }

        /// <summary>
        /// Tests that IsLeaf is correctly set for parent vs leaf accounts
        /// </summary>
        [Fact]
        public void SeedDefaultAccounts_CorrectIsLeafFlags()
        {
            // Arrange
            long companyId = 107;
            var seeder = new TestGLAccountSeeder(_fixture.Db);

            // Act
            seeder.SeedDefaultAccounts(companyId, _fixture.TestUserId.ToString());

            // Assert
            var accounts = _fixture.Db.Queryable<GLAccount>()
                .Where(a => a.CompanyID == companyId)
                .ToList();

            // Parent accounts should have IsLeaf = false
            accounts.First(a => a.AccountNumber == "1000").IsLeaf.Should().BeFalse();
            accounts.First(a => a.AccountNumber == "1100").IsLeaf.Should().BeFalse();
            accounts.First(a => a.AccountNumber == "2000").IsLeaf.Should().BeFalse();

            // Leaf accounts should have IsLeaf = true
            accounts.First(a => a.AccountNumber == "1110").IsLeaf.Should().BeTrue();
            accounts.First(a => a.AccountNumber == "1120").IsLeaf.Should().BeTrue();
        }

        #endregion

        #region Bilingual Data Tests

        /// <summary>
        /// Tests that seeded accounts have both Arabic and English names
        /// </summary>
        [Fact]
        public void SeedDefaultAccounts_BilingualNames()
        {
            // Arrange
            long companyId = 108;
            var seeder = new TestGLAccountSeeder(_fixture.Db);

            // Act
            seeder.SeedDefaultAccounts(companyId, _fixture.TestUserId.ToString());

            // Assert
            var assets = _fixture.Db.Queryable<GLAccount>()
                .First(a => a.AccountNumber == "1000" && a.CompanyID == companyId);

            assets.AccountNameAr.Should().NotBeNullOrEmpty();
            assets.AccountNameEn.Should().NotBeNullOrEmpty();
            assets.AccountNameAr.Should().Contain("الأصول");
            assets.AccountNameEn.Should().Contain("Assets");
        }

        #endregion

        #region Multi-Tenancy Tests

        /// <summary>
        /// Tests that seeding for different companies creates isolated accounts
        /// </summary>
        [Fact]
        public void SeedDefaultAccounts_DifferentCompanies_Isolated()
        {
            // Arrange
            long company1Id = 200;
            long company2Id = 201;
            var seeder = new TestGLAccountSeeder(_fixture.Db);

            // Act
            var count1 = seeder.SeedDefaultAccounts(company1Id, _fixture.TestUserId.ToString());
            var count2 = seeder.SeedDefaultAccounts(company2Id, _fixture.TestUserId.ToString());

            // Assert
            count1.Should().BeGreaterThan(0);
            count2.Should().BeGreaterThan(0);

            var company1Accounts = _fixture.Db.Queryable<GLAccount>()
                .Where(a => a.CompanyID == company1Id)
                .ToList();
            var company2Accounts = _fixture.Db.Queryable<GLAccount>()
                .Where(a => a.CompanyID == company2Id)
                .ToList();

            // Both companies should have accounts
            company1Accounts.Count.Should().BeGreaterThan(0);
            company2Accounts.Count.Should().BeGreaterThan(0);

            // Account IDs should be different
            var company1Ids = company1Accounts.Select(a => a.AccountId).ToList();
            var company2Ids = company2Accounts.Select(a => a.AccountId).ToList();
            company1Ids.Should().NotIntersectWith(company2Ids);
        }

        #endregion

        #region Idempotency Tests

        /// <summary>
        /// Tests that seeding twice for the same company doesn't create duplicates
        /// </summary>
        [Fact]
        public void SeedDefaultAccounts_SecondCall_ReturnsZero()
        {
            // Arrange
            long companyId = 300;
            var seeder = new TestGLAccountSeeder(_fixture.Db);

            // Act
            var count1 = seeder.SeedDefaultAccounts(companyId, _fixture.TestUserId.ToString());
            var count2 = seeder.SeedDefaultAccounts(companyId, _fixture.TestUserId.ToString());

            // Assert
            count1.Should().BeGreaterThan(0);
            count2.Should().Be(0); // Second call should return 0 (no new accounts created)
        }

        /// <summary>
        /// Tests that DefaultAccountsExist returns true after seeding
        /// </summary>
        [Fact]
        public void DefaultAccountsExist_AfterSeeding_ReturnsTrue()
        {
            // Arrange
            long companyId = 301;
            var seeder = new TestGLAccountSeeder(_fixture.Db);

            // Act
            var existsBefore = seeder.DefaultAccountsExist(companyId);
            seeder.SeedDefaultAccounts(companyId, _fixture.TestUserId.ToString());
            var existsAfter = seeder.DefaultAccountsExist(companyId);

            // Assert
            existsBefore.Should().BeFalse();
            existsAfter.Should().BeTrue();
        }

        #endregion

        #region Account Nature Flags Tests

        /// <summary>
        /// Tests that cash accounts are properly flagged
        /// </summary>
        [Fact]
        public void SeedDefaultAccounts_CashAccountsFlagged()
        {
            // Arrange
            long companyId = 400;
            var seeder = new TestGLAccountSeeder(_fixture.Db);

            // Act
            seeder.SeedDefaultAccounts(companyId, _fixture.TestUserId.ToString());

            // Assert
            var cashAccount = _fixture.Db.Queryable<GLAccount>()
                .First(a => a.AccountNumber == "1110" && a.CompanyID == companyId);
            cashAccount.IsCashAccount.Should().BeTrue();
        }

        /// <summary>
        /// Tests that control accounts (AR/AP) are properly flagged
        /// </summary>
        [Fact]
        public void SeedDefaultAccounts_ControlAccountsFlagged()
        {
            // Arrange
            long companyId = 401;
            var seeder = new TestGLAccountSeeder(_fixture.Db);

            // Act
            seeder.SeedDefaultAccounts(companyId, _fixture.TestUserId.ToString());

            // Assert
            var arAccount = _fixture.Db.Queryable<GLAccount>()
                .First(a => a.AccountNumber == "1120" && a.CompanyID == companyId);
            var apAccount = _fixture.Db.Queryable<GLAccount>()
                .First(a => a.AccountNumber == "2110" && a.CompanyID == companyId);

            arAccount.IsControlAccount.Should().BeTrue();
            apAccount.IsControlAccount.Should().BeTrue();
        }

        #endregion
    }

    /// <summary>
    /// Test-specific GL Account Seeder for testing purposes
    /// </summary>
    public class TestGLAccountSeeder
    {
        private readonly SqlSugar.ISqlSugarClient _db;

        public TestGLAccountSeeder(SqlSugar.ISqlSugarClient db)
        {
            _db = db;
        }

        public int SeedDefaultAccounts(long? companyId, string createdBy)
        {
            if (DefaultAccountsExist(companyId))
                return 0;

            var accounts = new System.Collections.Generic.List<GLAccount>();
            var now = DateTime.Now;

            // ==================== ASSETS (1000) ====================
            var assets = CreateSystemAccount("1000", "الأصول", "Assets", AccountType.Asset, NormalBalance.Debit, 1, null, companyId, createdBy, now);
            accounts.Add(assets);

            // Current Assets (1100)
            var currentAssets = CreateSystemAccount("1100", "الأصول المتداولة", "Current Assets", AccountType.Asset, NormalBalance.Debit, 2, assets.AccountId, companyId, createdBy, now);
            accounts.Add(currentAssets);

            accounts.Add(CreateSystemAccount("1110", "النقد وما يعادله", "Cash and Cash Equivalents", AccountType.Asset, NormalBalance.Debit, 3, currentAssets.AccountId, companyId, createdBy, now, isCash: true));
            accounts.Add(CreateSystemAccount("1120", "الذمم المدينة", "Accounts Receivable", AccountType.Asset, NormalBalance.Debit, 3, currentAssets.AccountId, companyId, createdBy, now, isControl: true));
            accounts.Add(CreateSystemAccount("1130", "المخزون", "Inventory", AccountType.Asset, NormalBalance.Debit, 3, currentAssets.AccountId, companyId, createdBy, now));
            accounts.Add(CreateSystemAccount("1140", "المصروفات المدفوعة مقدماً", "Prepaid Expenses", AccountType.Asset, NormalBalance.Debit, 3, currentAssets.AccountId, companyId, createdBy, now));

            // Non-Current Assets (1200)
            var nonCurrentAssets = CreateSystemAccount("1200", "الأصول غير المتداولة", "Non-Current Assets", AccountType.Asset, NormalBalance.Debit, 2, assets.AccountId, companyId, createdBy, now);
            accounts.Add(nonCurrentAssets);

            accounts.Add(CreateSystemAccount("1210", "الممتلكات والمعدات", "Property, Plant & Equipment", AccountType.Asset, NormalBalance.Debit, 3, nonCurrentAssets.AccountId, companyId, createdBy, now));
            accounts.Add(CreateSystemAccount("1220", "الأصول غير الملموسة", "Intangible Assets", AccountType.Asset, NormalBalance.Debit, 3, nonCurrentAssets.AccountId, companyId, createdBy, now));
            accounts.Add(CreateSystemAccount("1230", "الاستثمارات", "Investments", AccountType.Asset, NormalBalance.Debit, 3, nonCurrentAssets.AccountId, companyId, createdBy, now));

            // ==================== LIABILITIES (2000) ====================
            var liabilities = CreateSystemAccount("2000", "الالتزامات", "Liabilities", AccountType.Liability, NormalBalance.Credit, 1, null, companyId, createdBy, now);
            accounts.Add(liabilities);

            // Current Liabilities (2100)
            var currentLiabilities = CreateSystemAccount("2100", "الالتزامات المتداولة", "Current Liabilities", AccountType.Liability, NormalBalance.Credit, 2, liabilities.AccountId, companyId, createdBy, now);
            accounts.Add(currentLiabilities);

            accounts.Add(CreateSystemAccount("2110", "الذمم الدائنة", "Accounts Payable", AccountType.Liability, NormalBalance.Credit, 3, currentLiabilities.AccountId, companyId, createdBy, now, isControl: true));
            accounts.Add(CreateSystemAccount("2120", "المصروفات المستحقة", "Accrued Expenses", AccountType.Liability, NormalBalance.Credit, 3, currentLiabilities.AccountId, companyId, createdBy, now));
            accounts.Add(CreateSystemAccount("2130", "القروض قصيرة الأجل", "Short-term Loans", AccountType.Liability, NormalBalance.Credit, 3, currentLiabilities.AccountId, companyId, createdBy, now));

            // Non-Current Liabilities (2200)
            var nonCurrentLiabilities = CreateSystemAccount("2200", "الالتزامات غير المتداولة", "Non-Current Liabilities", AccountType.Liability, NormalBalance.Credit, 2, liabilities.AccountId, companyId, createdBy, now);
            accounts.Add(nonCurrentLiabilities);

            accounts.Add(CreateSystemAccount("2210", "القروض طويلة الأجل", "Long-term Loans", AccountType.Liability, NormalBalance.Credit, 3, nonCurrentLiabilities.AccountId, companyId, createdBy, now));
            accounts.Add(CreateSystemAccount("2220", "الضرائب المؤجلة", "Deferred Tax Liabilities", AccountType.Liability, NormalBalance.Credit, 3, nonCurrentLiabilities.AccountId, companyId, createdBy, now));

            // ==================== EQUITY (3000) ====================
            var equity = CreateSystemAccount("3000", "حقوق الملكية", "Equity", AccountType.Equity, NormalBalance.Credit, 1, null, companyId, createdBy, now);
            accounts.Add(equity);

            accounts.Add(CreateSystemAccount("3100", "رأس المال", "Share Capital", AccountType.Equity, NormalBalance.Credit, 2, equity.AccountId, companyId, createdBy, now));
            accounts.Add(CreateSystemAccount("3200", "الأرباح المحتجزة", "Retained Earnings", AccountType.Equity, NormalBalance.Credit, 2, equity.AccountId, companyId, createdBy, now));
            accounts.Add(CreateSystemAccount("3300", "الاحتياطيات الأخرى", "Other Reserves", AccountType.Equity, NormalBalance.Credit, 2, equity.AccountId, companyId, createdBy, now));

            // ==================== REVENUE (4000) ====================
            var revenue = CreateSystemAccount("4000", "الإيرادات", "Revenue", AccountType.Revenue, NormalBalance.Credit, 1, null, companyId, createdBy, now);
            accounts.Add(revenue);

            accounts.Add(CreateSystemAccount("4100", "إيرادات التشغيل", "Operating Revenue", AccountType.Revenue, NormalBalance.Credit, 2, revenue.AccountId, companyId, createdBy, now));
            accounts.Add(CreateSystemAccount("4200", "إيرادات أخرى", "Other Income", AccountType.Revenue, NormalBalance.Credit, 2, revenue.AccountId, companyId, createdBy, now));

            // ==================== EXPENSES (5000) ====================
            var expenses = CreateSystemAccount("5000", "المصروفات", "Expenses", AccountType.Expense, NormalBalance.Debit, 1, null, companyId, createdBy, now);
            accounts.Add(expenses);

            accounts.Add(CreateSystemAccount("5100", "تكلفة البضاعة المباعة", "Cost of Goods Sold", AccountType.Expense, NormalBalance.Debit, 2, expenses.AccountId, companyId, createdBy, now));
            accounts.Add(CreateSystemAccount("5200", "مصروفات التشغيل", "Operating Expenses", AccountType.Expense, NormalBalance.Debit, 2, expenses.AccountId, companyId, createdBy, now));
            accounts.Add(CreateSystemAccount("5300", "المصروفات الإدارية", "Administrative Expenses", AccountType.Expense, NormalBalance.Debit, 2, expenses.AccountId, companyId, createdBy, now));
            accounts.Add(CreateSystemAccount("5400", "المصروفات المالية", "Financial Expenses", AccountType.Expense, NormalBalance.Debit, 2, expenses.AccountId, companyId, createdBy, now));

            // Build account paths and full account numbers
            var accountDict = new System.Collections.Generic.Dictionary<Guid, GLAccount>();
            foreach (var account in accounts)
            {
                accountDict[account.AccountId] = account;

                if (account.ParentAccountId.HasValue && accountDict.TryGetValue(account.ParentAccountId.Value, out var parent))
                {
                    account.FullAccountNumber = account.BuildFullAccountNumber(parent.FullAccountNumber);
                    account.AccountPath = string.IsNullOrEmpty(parent.AccountPath)
                        ? $"{parent.AccountId}/{account.AccountId}"
                        : $"{parent.AccountPath}/{account.AccountId}";
                }
                else
                {
                    account.FullAccountNumber = account.AccountNumber;
                    account.AccountPath = account.AccountId.ToString();
                }
            }

            // Update IsLeaf for parent accounts
            var parentIds = new System.Collections.Generic.HashSet<Guid>();
            foreach (var account in accounts)
            {
                if (account.ParentAccountId.HasValue)
                {
                    parentIds.Add(account.ParentAccountId.Value);
                }
            }

            foreach (var account in accounts)
            {
                if (parentIds.Contains(account.AccountId))
                {
                    account.IsLeaf = false;
                }
            }

            // Insert all accounts
            int count = 0;
            foreach (var account in accounts)
            {
                _db.Insertable(account).ExecuteCommand();
                count++;
            }

            return count;
        }

        public bool DefaultAccountsExist(long? companyId)
        {
            return _db.Queryable<GLAccount>()
                .Where(a => a.FullAccountNumber == "1000")
                .WhereIF(companyId.HasValue, a => a.CompanyID == companyId.Value)
                .Any();
        }

        private GLAccount CreateSystemAccount(
            string accountNumber,
            string nameAr,
            string nameEn,
            AccountType accountType,
            NormalBalance normalBalance,
            int level,
            Guid? parentAccountId,
            long? companyId,
            string createdBy,
            DateTime createdAt,
            bool isCash = false,
            bool isBank = false,
            bool isControl = false)
        {
            return new GLAccount
            {
                AccountId = Guid.NewGuid(),
                AccountNumber = accountNumber,
                FullAccountNumber = accountNumber,
                AccountNameAr = nameAr,
                AccountNameEn = nameEn,
                AccountType = accountType,
                NormalBalance = normalBalance,
                Level = level,
                ParentAccountId = parentAccountId,
                IsLeaf = true,
                IsActive = true,
                IsSystemAccount = true,
                AllowManualEntries = true,
                AllowPostingToParent = false,
                RequiresCostCenter = false,
                RequiresDepartment = false,
                RequiresProject = false,
                IsCashAccount = isCash,
                IsBankAccount = isBank,
                IsControlAccount = isControl,
                IsReconciliationAccount = false,
                Currency = "SAR",
                IsForeignCurrency = false,
                DisplayOrder = int.Parse(accountNumber),
                RowVersion = 1,
                ChildCount = 0,
                DescendantCount = 0,
                CompanyID = companyId,
                Create_by = createdBy,
                Create_time = createdAt
            };
        }
    }
}
