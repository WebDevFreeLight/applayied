using System;
using System.Collections.Generic;
using FluentAssertions;
using Xunit;
using ZR.Model.Finance;
using ZR.Model.Finance.Dto;
using CurrencyModel = ZR.Model.Finance.Configuration.Currency;
using ZR.Service.Tests.ERP.Finance.TestHelpers;

namespace ZR.Service.Tests.ERP.Finance.Integration
{
    /// <summary>
    /// Integration tests for Multi-Tenant & Multi-Currency Scenarios - اختبارات التكامل: متعدد المستأجرين والعملات
    /// </summary>
    public class MultiTenantMultiCurrencyTests : IClassFixture<FinanceTestFixture>
    {
        private readonly FinanceTestFixture _fixture;
        private readonly TestGLVoucherService _voucherService;
        private readonly TestGLAccountService _accountService;
        private readonly FinanceTestDataBuilder _builder;

        public MultiTenantMultiCurrencyTests(FinanceTestFixture fixture)
        {
            _fixture = fixture;
            _voucherService = new TestGLVoucherService(_fixture.Db);
            _accountService = new TestGLAccountService(_fixture.Db);
            _builder = new FinanceTestDataBuilder(_fixture.Db, _fixture.TestTenantId, _fixture.TestUserId);
        }

        [Fact]
        public void CreateAccountsAndVouchers_TwoTenants_ShouldIsolateCorrectly()
        {
            // Arrange
            var tenant1Id = Guid.NewGuid();
            var tenant2Id = Guid.NewGuid();

            var builder1 = new FinanceTestDataBuilder(_fixture.Db, tenant1Id, _fixture.TestUserId);
            var builder2 = new FinanceTestDataBuilder(_fixture.Db, tenant2Id, _fixture.TestUserId);

            var accounts1 = builder1.CreateStandardChartOfAccounts();
            var accounts2 = builder2.CreateStandardChartOfAccounts();

            // Act - Create vouchers for both tenants
            var (voucher1, _) = builder1.CreateBalancedVoucher(
                accounts1["Cash"].AccountId,
                accounts1["Revenue"].AccountId,
                1000);
            _voucherService.Post(voucher1.Id, _fixture.TestUserId.ToString());

            var (voucher2, _) = builder2.CreateBalancedVoucher(
                accounts2["Cash"].AccountId,
                accounts2["Revenue"].AccountId,
                2000);
            _voucherService.Post(voucher2.Id, _fixture.TestUserId.ToString());

            // Assert - CompanyID is used instead of TenantId
            voucher1.CompanyID.Should().NotBeNull();
            voucher2.CompanyID.Should().NotBeNull();

            var balance1 = _accountService.GetAccountBalance(accounts1["Cash"].AccountId);
            var balance2 = _accountService.GetAccountBalance(accounts2["Cash"].AccountId);

            // Balances should be independent (opening balance is 0, so just check the transaction amounts)
            balance1.Should().Be(1000);
            balance2.Should().Be(2000);
        }

        [Fact]
        public void PostTransactions_MultipleCurrencies_ShouldConsolidateInBaseCurrency()
        {
            // Arrange
            var accounts = _builder.CreateStandardChartOfAccounts();
            var currencies = new Dictionary<string, CurrencyModel>
            {
                ["SAR"] = _builder.CreateCurrency("SAR", "ريال سعودي", "Saudi Riyal", 1.0m, true, "ر.س"),
                ["USD"] = _builder.CreateCurrency("USD", "دولار أمريكي", "US Dollar", 3.75m, false, "$"),
                ["EUR"] = _builder.CreateCurrency("EUR", "يورو", "Euro", 4.10m, false, "€")
            };

            // Post USD transaction
            var (usdVoucher, _) = _builder.CreateBalancedMultiCurrencyVoucher(
                accounts["Cash"].AccountId,
                accounts["Revenue"].AccountId,
                1000m,
                "USD",
                3.75m);
            _voucherService.Post(usdVoucher.Id, _fixture.TestUserId.ToString());

            // Post EUR transaction
            var (eurVoucher, _) = _builder.CreateBalancedMultiCurrencyVoucher(
                accounts["Bank"].AccountId,
                accounts["Revenue"].AccountId,
                500m,
                "EUR",
                4.10m);
            _voucherService.Post(eurVoucher.Id, _fixture.TestUserId.ToString());

            // Act - Get balances (should be in base currency)
            var cashBalance = _accountService.GetAccountBalance(accounts["Cash"].AccountId);
            var bankBalance = _accountService.GetAccountBalance(accounts["Bank"].AccountId);
            var revenueBalance = _accountService.GetAccountBalance(accounts["Revenue"].AccountId);

            // Assert (opening balance is 0, so just check transaction amounts in base currency)
            var expectedCash = 1000m * 3.75m; // USD converted
            var expectedBank = 500m * 4.10m; // EUR converted
            var expectedRevenue = (1000m * 3.75m + 500m * 4.10m); // Credit normal balance

            cashBalance.Should().BeApproximately(expectedCash, 0.01m);
            bankBalance.Should().BeApproximately(expectedBank, 0.01m);
            revenueBalance.Should().BeApproximately(expectedRevenue, 0.01m);
        }

        [Fact]
        public void GenerateReport_MultiCurrencyTransactions_ShouldConsolidateInBaseCurrency()
        {
            // Arrange
            var accounts = _builder.CreateStandardChartOfAccounts();
            var currencies = new Dictionary<string, CurrencyModel>
            {
                ["SAR"] = _builder.CreateCurrency("SAR", "ريال سعودي", "Saudi Riyal", 1.0m, true),
                ["USD"] = _builder.CreateCurrency("USD", "دولار أمريكي", "US Dollar", 3.75m, false),
                ["EUR"] = _builder.CreateCurrency("EUR", "يورو", "Euro", 4.10m, false)
            };

            var periodStart = new DateTime(2024, 1, 1);
            var periodEnd = new DateTime(2024, 1, 31);

            // Post transactions in different currencies
            var (usdVoucher, _) = _builder.CreateBalancedMultiCurrencyVoucher(
                accounts["Cash"].AccountId,
                accounts["Revenue"].AccountId,
                1000m,
                "USD",
                3.75m,
                voucherDate: periodStart.AddDays(5));
            _voucherService.Post(usdVoucher.Id, _fixture.TestUserId.ToString());

            var (eurVoucher, _) = _builder.CreateBalancedMultiCurrencyVoucher(
                accounts["Bank"].AccountId,
                accounts["Revenue"].AccountId,
                500m,
                "EUR",
                4.10m,
                voucherDate: periodStart.AddDays(10));
            _voucherService.Post(eurVoucher.Id, _fixture.TestUserId.ToString());

            var reportService = new TestGLFinancialReportService(_fixture.Db);
            var reportDto = new GenerateReportDto
            {
                PeriodStartDate = periodStart,
                PeriodEndDate = periodEnd,
                FiscalYear = 2024,
                TenantId = _fixture.TestTenantId
            };

            // Act
            var trialBalance = reportService.GenerateTrialBalance(reportDto, _fixture.TestUserId.ToString());

            // Assert
            trialBalance.Should().NotBeNull();
            var expectedTotal = (1000m * 3.75m) + (500m * 4.10m); // All in base currency
            trialBalance.TotalDebit.Should().BeApproximately(expectedTotal, 0.01m);
            trialBalance.TotalCredit.Should().BeApproximately(expectedTotal, 0.01m);
        }
    }
}
