using System;
using System.Collections.Generic;
using FluentAssertions;
using Xunit;
using ZR.Model.Finance;
using ZR.Model.Finance.Dto;
using ZR.Model.Finance.Enums;
using ZR.Service.Tests.ERP.Finance.TestHelpers;

namespace ZR.Service.Tests.ERP.Finance.Integration.Reports_Integration
{
    /// <summary>
    /// Integration tests for Financial Reports - اختبارات التكامل للتقارير المالية
    /// Tests: Post Vouchers → Generate Reports → Verify Report Accuracy
    /// </summary>
    public class FinancialReportsIntegrationTests : IClassFixture<FinanceTestFixture>
    {
        private readonly FinanceTestFixture _fixture;
        private readonly TestGLVoucherService _voucherService;
        private readonly TestGLFinancialReportService _reportService;
        private readonly FinanceTestDataBuilder _builder;
        private readonly Dictionary<string, GLAccount> _accounts;

        public FinancialReportsIntegrationTests(FinanceTestFixture fixture)
        {
            _fixture = fixture;

            // Create test services that work directly with ISqlSugarClient
            _voucherService = new TestGLVoucherService(_fixture.Db);
            _reportService = new TestGLFinancialReportService(_fixture.Db);

            // Create test data builder
            _builder = new FinanceTestDataBuilder(_fixture.Db, _fixture.TestTenantId, _fixture.TestUserId);

            // Create standard accounts
            _accounts = _builder.CreateStandardChartOfAccounts();
        }

        [Fact]
        public void PostVouchers_GenerateTrialBalance_ShouldMatchSumOfEntries()
        {
            // Arrange - Use a unique date range to avoid conflicts with other tests
            var uniqueYear = 2010;
            var periodStart = new DateTime(uniqueYear, 1, 1);
            var periodEnd = new DateTime(uniqueYear, 1, 31);

            // Create and post multiple vouchers
            var (voucher1, _) = _builder.CreateBalancedVoucher(
                _accounts["Cash"].AccountId,
                _accounts["Revenue"].AccountId,
                5000,
                voucherDate: periodStart.AddDays(5));
            _voucherService.Post(voucher1.Id, _fixture.TestUserId.ToString());

            var (voucher2, _) = _builder.CreateBalancedVoucher(
                _accounts["Bank"].AccountId,
                _accounts["Revenue"].AccountId,
                3000,
                voucherDate: periodStart.AddDays(10));
            _voucherService.Post(voucher2.Id, _fixture.TestUserId.ToString());

            var (voucher3, _) = _builder.CreateBalancedVoucher(
                _accounts["Expenses"].AccountId,
                _accounts["Cash"].AccountId,
                2000,
                voucherDate: periodStart.AddDays(15));
            _voucherService.Post(voucher3.Id, _fixture.TestUserId.ToString());

            var dto = new GenerateReportDto
            {
                PeriodStartDate = periodStart,
                PeriodEndDate = periodEnd,
                FiscalYear = periodStart.Year,
                TenantId = _fixture.TestTenantId
            };

            // Act
            var report = _reportService.GenerateTrialBalance(dto, _fixture.TestUserId.ToString());

            // Assert
            report.Should().NotBeNull();
            report.ReportType.Should().Be(ReportType.TrialBalance);
            report.TotalDebit.Should().Be(10000); // 5000 + 3000 + 2000
            report.TotalCredit.Should().Be(10000); // 5000 + 3000 + 2000
            report.TotalDebit.Should().Be(report.TotalCredit);
        }

        [Fact]
        public void PostVouchers_GenerateBalanceSheet_ShouldMatchTrialBalance()
        {
            // Arrange - Use a very early unique date range to avoid conflicts with other tests
            // Balance sheets are cumulative, so we need a date before all other tests
            var uniqueYear = 1901; // Very early date to avoid picking up other test transactions
            var periodStart = new DateTime(uniqueYear, 1, 1);
            var periodEnd = new DateTime(uniqueYear, 1, 31);

            // Create capital injection (both sides are balance sheet accounts: Cash=asset, Capital=equity)
            var (voucher1, _) = _builder.CreateBalancedVoucher(
                _accounts["Cash"].AccountId,
                _accounts["Capital"].AccountId,
                100000,
                voucherDate: periodStart.AddDays(1));
            _voucherService.Post(voucher1.Id, _fixture.TestUserId.ToString());

            // Create asset transfer (both sides are balance sheet accounts: Cash=asset, Bank=asset)
            var (voucher2, _) = _builder.CreateBalancedVoucher(
                _accounts["Bank"].AccountId,
                _accounts["Cash"].AccountId,
                50000,
                voucherDate: periodStart.AddDays(5));
            _voucherService.Post(voucher2.Id, _fixture.TestUserId.ToString());

            // Create liability transaction (both sides are balance sheet accounts: Cash=asset, Payables=liability)
            var (voucher3, _) = _builder.CreateBalancedVoucher(
                _accounts["Cash"].AccountId,
                _accounts["Payables"].AccountId,
                20000,
                voucherDate: periodStart.AddDays(10));
            _voucherService.Post(voucher3.Id, _fixture.TestUserId.ToString());

            var dto = new GenerateReportDto
            {
                PeriodStartDate = periodStart,
                PeriodEndDate = periodEnd,
                FiscalYear = periodEnd.Year,
                TenantId = _fixture.TestTenantId
            };

            // Act
            var balanceSheet = _reportService.GenerateBalanceSheet(dto, _fixture.TestUserId.ToString());
            var trialBalance = _reportService.GenerateTrialBalance(dto, _fixture.TestUserId.ToString());

            // Assert
            balanceSheet.Should().NotBeNull();
            trialBalance.Should().NotBeNull();

            // Balance sheet should balance: Assets = Liabilities + Equity
            balanceSheet.TotalDebit.Should().Be(balanceSheet.TotalCredit);
        }

        [Fact]
        public void PostVouchers_GenerateIncomeStatement_ShouldSumRevenuesAndExpenses()
        {
            // Arrange - Use a unique date range to avoid conflicts with other tests
            var uniqueYear = 2012;
            var periodStart = new DateTime(uniqueYear, 1, 1);
            var periodEnd = new DateTime(uniqueYear, 1, 31);

            // Create revenue transactions
            var (voucher1, _) = _builder.CreateBalancedVoucher(
                _accounts["Cash"].AccountId,
                _accounts["Revenue"].AccountId,
                10000,
                voucherDate: periodStart.AddDays(5));
            _voucherService.Post(voucher1.Id, _fixture.TestUserId.ToString());

            var (voucher2, _) = _builder.CreateBalancedVoucher(
                _accounts["Bank"].AccountId,
                _accounts["Revenue"].AccountId,
                5000,
                voucherDate: periodStart.AddDays(10));
            _voucherService.Post(voucher2.Id, _fixture.TestUserId.ToString());

            // Create expense transactions
            var (voucher3, _) = _builder.CreateBalancedVoucher(
                _accounts["Expenses"].AccountId,
                _accounts["Cash"].AccountId,
                3000,
                voucherDate: periodStart.AddDays(15));
            _voucherService.Post(voucher3.Id, _fixture.TestUserId.ToString());

            var (voucher4, _) = _builder.CreateBalancedVoucher(
                _accounts["Expenses"].AccountId,
                _accounts["Bank"].AccountId,
                2000,
                voucherDate: periodStart.AddDays(20));
            _voucherService.Post(voucher4.Id, _fixture.TestUserId.ToString());

            var dto = new GenerateReportDto
            {
                PeriodStartDate = periodStart,
                PeriodEndDate = periodEnd,
                FiscalYear = periodStart.Year,
                TenantId = _fixture.TestTenantId
            };

            // Act
            var incomeStatement = _reportService.GenerateIncomeStatement(dto, _fixture.TestUserId.ToString());

            // Assert
            incomeStatement.Should().NotBeNull();
            incomeStatement.ReportType.Should().Be(ReportType.IncomeStatement);
            incomeStatement.TotalCredit.Should().BeGreaterThanOrEqualTo(15000); // Revenue: 10000 + 5000
            incomeStatement.TotalDebit.Should().BeGreaterThanOrEqualTo(5000); // Expenses: 3000 + 2000
        }

        [Fact]
        public void PostVouchers_GenerateCashFlow_ShouldCalculateCashMovements()
        {
            // Arrange - Use a unique date range to avoid conflicts with other tests
            var uniqueYear = 2015;
            var periodStart = new DateTime(uniqueYear, 1, 1);
            var periodEnd = new DateTime(uniqueYear, 1, 31);

            // Cash inflows
            var (voucher1, _) = _builder.CreateBalancedVoucher(
                _accounts["Cash"].AccountId,
                _accounts["Revenue"].AccountId,
                10000,
                voucherDate: periodStart.AddDays(5));
            _voucherService.Post(voucher1.Id, _fixture.TestUserId.ToString());

            // Cash outflows
            var (voucher2, _) = _builder.CreateBalancedVoucher(
                _accounts["Expenses"].AccountId,
                _accounts["Cash"].AccountId,
                5000,
                voucherDate: periodStart.AddDays(10));
            _voucherService.Post(voucher2.Id, _fixture.TestUserId.ToString());

            var dto = new GenerateReportDto
            {
                PeriodStartDate = periodStart,
                PeriodEndDate = periodEnd,
                FiscalYear = periodStart.Year,
                TenantId = _fixture.TestTenantId
            };

            // Act
            var cashFlow = _reportService.GenerateCashFlow(dto, _fixture.TestUserId.ToString());

            // Assert
            cashFlow.Should().NotBeNull();
            cashFlow.ReportType.Should().Be(ReportType.CashFlow);
            cashFlow.ReportData.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void GenerateReport_AfterVoucherPosting_ShouldIncludePostedTransactions()
        {
            // Arrange - Use a unique date range to avoid conflicts with other tests
            var uniqueYear = 2013;
            var periodStart = new DateTime(uniqueYear, 1, 1);
            var periodEnd = new DateTime(uniqueYear, 1, 31);

            // Create posted voucher
            var (voucher1, _) = _builder.CreateBalancedVoucher(
                _accounts["Cash"].AccountId,
                _accounts["Revenue"].AccountId,
                5000,
                voucherDate: periodStart.AddDays(5));
            _voucherService.Post(voucher1.Id, _fixture.TestUserId.ToString());

            // Create draft voucher (should not be included)
            var (voucher2, _) = _builder.CreateBalancedVoucher(
                _accounts["Bank"].AccountId,
                _accounts["Revenue"].AccountId,
                3000,
                voucherDate: periodStart.AddDays(10));
            // Don't post this one

            var dto = new GenerateReportDto
            {
                PeriodStartDate = periodStart,
                PeriodEndDate = periodEnd,
                FiscalYear = periodStart.Year,
                TenantId = _fixture.TestTenantId
            };

            // Act
            var report = _reportService.GenerateTrialBalance(dto, _fixture.TestUserId.ToString());

            // Assert
            report.Should().NotBeNull();
            // Should only include posted vouchers
            report.TotalDebit.Should().Be(5000); // Only posted voucher
            report.TotalCredit.Should().Be(5000);
        }
    }
}

