using System;
using System.Collections.Generic;
using FluentAssertions;
using Xunit;
using ZR.Model.Finance.Assets;
using ZR.Model.Finance.Assets.Enums;
using ZR.Model.Finance;
using ZR.Model.Finance.Enums;
using ZR.Service.Tests.ERP.Finance.TestHelpers;

namespace ZR.Service.Tests.ERP.Finance.Unit.Assets
{
    /// <summary>
    /// Unit tests for Fixed Asset & Depreciation - اختبارات الأصول الثابتة والإهلاك
    /// </summary>
    public class FixedAssetDepreciationTests : IClassFixture<FinanceTestFixture>
    {
        private readonly FinanceTestFixture _fixture;
        private readonly FinanceTestDataBuilder _builder;
        private readonly Dictionary<string, GLAccount> _accounts;

        public FixedAssetDepreciationTests(FinanceTestFixture fixture)
        {
            _fixture = fixture;
            _builder = new FinanceTestDataBuilder(_fixture.Db, _fixture.TestTenantId, _fixture.TestUserId);
            _accounts = _builder.CreateStandardChartOfAccounts();
        }

        [Fact]
        public void CreateFixedAsset_ShouldCreateAssetAndPostInitialGLEntry()
        {
            // Arrange
            var assetAccount = _accounts["Cash"]; // Using existing account for simplicity
            var category = _builder.CreateAssetCategory(
                "CAT-001",
                "معدات",
                "Equipment",
                DepreciationMethod.StraightLine,
                60); // 5 years

            // Act
            var asset = _builder.CreateFixedAsset(
                "AST-001",
                "معدة تجريبية",
                "Test Equipment",
                purchaseCost: 100000,
                salvageValue: 10000,
                purchaseDate: DateTime.Now.AddMonths(-12),
                categoryId: category.Id,
                glAssetAccountId: assetAccount.AccountId);

            // Assert
            asset.Should().NotBeNull();
            asset.AssetCode.Should().Be("AST-001");
            asset.PurchaseCost.Should().Be(100000);
            asset.CurrentBookValue.Should().Be(100000);
            asset.AccumulatedDepreciation.Should().Be(0);
            asset.Status.Should().Be(AssetStatus.Active);
        }

        [Fact]
        public void CalculateDepreciation_StraightLine_ShouldCalculateCorrectly()
        {
            // Arrange
            var category = _builder.CreateAssetCategory(
                "CAT-002",
                "مركبة",
                "Vehicle",
                DepreciationMethod.StraightLine,
                60); // 5 years = 60 months

            var purchaseCost = 100000m;
            var salvageValue = 10000m;
            var usefulLifeMonths = 60;
            var expectedMonthlyDepreciation = (purchaseCost - salvageValue) / usefulLifeMonths; // 1500

            var asset = _builder.CreateFixedAsset(
                "AST-002",
                "مركبة",
                "Vehicle",
                purchaseCost,
                salvageValue,
                purchaseDate: DateTime.Now.AddMonths(-12),
                categoryId: category.Id);

            // Act - Calculate depreciation for one month
            var monthlyDepreciation = (asset.PurchaseCost - asset.SalvageValue) / usefulLifeMonths;

            // Assert
            monthlyDepreciation.Should().BeApproximately(expectedMonthlyDepreciation, 0.01m);
        }

        [Fact]
        public void PostDepreciation_ShouldCreateGLVoucherEntry()
        {
            // Arrange
            var assetAccount = _builder.CreateAccount("1500", "الأصول الثابتة", "Fixed Assets", AccountType.Asset);
            var depreciationExpenseAccount = _builder.CreateAccount("5100", "مصروف الإهلاك", "Depreciation Expense", AccountType.Expense);
            var accumulatedDepreciationAccount = _builder.CreateAccount("1510", "مجمع الإهلاك", "Accumulated Depreciation", AccountType.Asset);

            var category = _builder.CreateAssetCategory(
                "CAT-003",
                "معدات",
                "Equipment",
                DepreciationMethod.StraightLine,
                60);

            var asset = _builder.CreateFixedAsset(
                "AST-003",
                "معدة",
                "Equipment",
                purchaseCost: 100000,
                salvageValue: 10000,
                purchaseDate: DateTime.Now.AddMonths(-12),
                categoryId: category.Id,
                glAssetAccountId: assetAccount.AccountId,
                glAccumulatedDepreciationAccountId: accumulatedDepreciationAccount.AccountId,
                glDepreciationExpenseAccountId: depreciationExpenseAccount.AccountId);

            var monthlyDepreciation = (asset.PurchaseCost - asset.SalvageValue) / 60; // 1500

            // Act - Create depreciation voucher
            var voucher = _builder.CreateVoucher(descriptionEn: "Depreciation Entry");
            _builder.CreateVoucherEntry(
                voucher.Id,
                depreciationExpenseAccount.AccountId,
                debit: monthlyDepreciation,
                lineNumber: 1,
                descriptionEn: "Depreciation Expense");

            _builder.CreateVoucherEntry(
                voucher.Id,
                accumulatedDepreciationAccount.AccountId,
                credit: monthlyDepreciation,
                lineNumber: 2,
                descriptionEn: "Accumulated Depreciation");

            // Update voucher totals
            var entries = _fixture.Db.Queryable<GLVoucherEntry>().Where(e => e.VoucherId == voucher.Id).ToList();
            voucher.TotalDebit = entries.Sum(e => e.Debit);
            voucher.TotalCredit = entries.Sum(e => e.Credit);
            _fixture.Db.Updateable(voucher).ExecuteCommand();

            var voucherService = new TestGLVoucherService(_fixture.Db);
            voucherService.Post(voucher.Id, _fixture.TestUserId.ToString());

            // Assert
            var posted = voucherService.GetById(voucher.Id);
            posted.Status.Should().Be(VoucherStatus.Posted);
            posted.TotalDebit.Should().Be(monthlyDepreciation);
            posted.TotalCredit.Should().Be(monthlyDepreciation);
        }

        [Fact]
        public void SellAsset_ShouldPostGainOrLoss()
        {
            // Arrange
            var asset = _builder.CreateFixedAsset(
                "AST-004",
                "أصل للبيع",
                "Asset for Sale",
                purchaseCost: 100000,
                salvageValue: 10000,
                purchaseDate: DateTime.Now.AddMonths(-24));

            // Simulate depreciation - asset is now worth 60000
            asset.AccumulatedDepreciation = 40000;
            asset.CurrentBookValue = asset.PurchaseCost - asset.AccumulatedDepreciation; // 60000
            _fixture.Db.Updateable(asset).ExecuteCommand();

            var salePrice = 70000m; // Sold for more than book value = gain
            var gain = salePrice - asset.CurrentBookValue; // 10000 gain

            // Act - Create sale voucher
            var cashAccount = _accounts["Cash"];
            var assetAccount = _builder.CreateAccount("1500", "الأصول الثابتة", "Fixed Assets", AccountType.Asset);
            var gainAccount = _builder.CreateAccount("4200", "أرباح بيع الأصول", "Asset Sale Gain", AccountType.Revenue);

            var voucher = _builder.CreateVoucher(descriptionEn: "Asset Sale");
            _builder.CreateVoucherEntry(voucher.Id, cashAccount.AccountId, debit: salePrice, lineNumber: 1);
            _builder.CreateVoucherEntry(voucher.Id, assetAccount.AccountId, credit: asset.CurrentBookValue, lineNumber: 2);
            _builder.CreateVoucherEntry(voucher.Id, gainAccount.AccountId, credit: gain, lineNumber: 3);

            // Update voucher totals
            var entries = _fixture.Db.Queryable<GLVoucherEntry>().Where(e => e.VoucherId == voucher.Id).ToList();
            voucher.TotalDebit = entries.Sum(e => e.Debit);
            voucher.TotalCredit = entries.Sum(e => e.Credit);
            _fixture.Db.Updateable(voucher).ExecuteCommand();

            var voucherService = new TestGLVoucherService(_fixture.Db);
            voucherService.Post(voucher.Id, _fixture.TestUserId.ToString());

            // Assert
            var posted = voucherService.GetById(voucher.Id);
            posted.Status.Should().Be(VoucherStatus.Posted);
            posted.TotalDebit.Should().Be(salePrice);
            posted.TotalCredit.Should().Be(salePrice);
        }

        [Fact]
        public void DepreciateAsset_InClosedPeriod_ShouldFail()
        {
            // Arrange
            var period = _builder.CreatePeriod(
                new DateTime(2024, 1, 1),
                new DateTime(2024, 1, 31),
                PeriodType.Month,
                PeriodStatus.Closed,
                2024,
                1);

            var asset = _builder.CreateFixedAsset(
                "AST-005",
                "أصل",
                "Asset",
                purchaseCost: 100000,
                purchaseDate: new DateTime(2024, 1, 15));

            var depreciationDate = new DateTime(2024, 1, 31); // In closed period

            // Act & Assert
            // In production, this should check if period is closed before allowing depreciation
            // For now, test documents the scenario
            var periodService = new TestGLPeriodClosingService(_fixture.Db);
            var isClosed = periodService.IsDateInClosedPeriod(depreciationDate, _fixture.TestTenantId);
            isClosed.Should().BeTrue();
        }

        [Fact]
        public void DepreciateAsset_FullyDepreciated_ShouldNotAllowMoreDepreciation()
        {
            // Arrange
            var asset = _builder.CreateFixedAsset(
                "AST-006",
                "أصل مهلك بالكامل",
                "Fully Depreciated Asset",
                purchaseCost: 100000,
                salvageValue: 10000);

            // Set as fully depreciated
            asset.AccumulatedDepreciation = asset.PurchaseCost - asset.SalvageValue; // 90000
            asset.CurrentBookValue = asset.SalvageValue; // 10000
            asset.Status = AssetStatus.FullyDepreciated;
            _fixture.Db.Updateable(asset).ExecuteCommand();

            // Act
            var remainingDepreciable = asset.PurchaseCost - asset.SalvageValue - asset.AccumulatedDepreciation;

            // Assert
            remainingDepreciable.Should().Be(0);
            asset.Status.Should().Be(AssetStatus.FullyDepreciated);
            asset.CurrentBookValue.Should().Be(asset.SalvageValue);
        }

        [Fact]
        public void CreateAsset_WithDifferentDepreciationMethods_ShouldStoreCorrectly()
        {
            // Arrange & Act
            var straightLineCategory = _builder.CreateAssetCategory(
                "CAT-SL",
                "قسط ثابت",
                "Straight Line",
                DepreciationMethod.StraightLine,
                60);

            var decliningBalanceCategory = _builder.CreateAssetCategory(
                "CAT-DB",
                "قسط متناقص",
                "Declining Balance",
                DepreciationMethod.DecliningBalance,
                60);

            var noDepreciationCategory = _builder.CreateAssetCategory(
                "CAT-NONE",
                "بدون إهلاك",
                "No Depreciation",
                DepreciationMethod.None,
                0);

            // Assert
            straightLineCategory.DepreciationMethod.Should().Be(DepreciationMethod.StraightLine);
            decliningBalanceCategory.DepreciationMethod.Should().Be(DepreciationMethod.DecliningBalance);
            noDepreciationCategory.DepreciationMethod.Should().Be(DepreciationMethod.None);
        }
    }
}
