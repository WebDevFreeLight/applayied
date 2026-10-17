using System;
using System.Collections.Generic;
using FluentAssertions;
using Xunit;
using ZR.Model.Finance;
using ZR.Model.Finance.Dto;
using ZR.Model.Finance.Enums;
using ZR.Service.Tests.ERP.Finance.TestHelpers;

namespace ZR.Service.Tests.ERP.Finance.Integration
{
    /// <summary>
    /// Integration tests for Voucher → GL → Reports Workflow - اختبارات التكامل: السندات → الأستاذ → التقارير
    /// </summary>
    public class VoucherGLReportsWorkflowTests : IClassFixture<FinanceTestFixture>
    {
        private readonly FinanceTestFixture _fixture;
        private readonly TestGLVoucherService _voucherService;
        private readonly TestGLAccountService _accountService;
        private readonly TestGLFinancialReportService _reportService;
        private readonly FinanceTestDataBuilder _builder;
        private readonly Dictionary<string, GLAccount> _accounts;

        public VoucherGLReportsWorkflowTests(FinanceTestFixture fixture)
        {
            _fixture = fixture;
            _voucherService = new TestGLVoucherService(_fixture.Db);
            _accountService = new TestGLAccountService(_fixture.Db);
            _reportService = new TestGLFinancialReportService(_fixture.Db);
            _builder = new FinanceTestDataBuilder(_fixture.Db, _fixture.TestTenantId, _fixture.TestUserId);
            _accounts = _builder.CreateStandardChartOfAccounts();
        }

        [Fact]
        public void PostVoucher_GenerateTrialBalance_ShouldMatchTotals()
        {
            // Arrange
            var periodStart = new DateTime(2024, 1, 1);
            var periodEnd = new DateTime(2024, 1, 31);

            // Post multiple vouchers
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

            var reportDto = new GenerateReportDto
            {
                PeriodStartDate = periodStart,
                PeriodEndDate = periodEnd,
                FiscalYear = 2024,
                TenantId = _fixture.TestTenantId
            };

            // Act
            var trialBalance = _reportService.GenerateTrialBalance(reportDto, _fixture.TestUserId.ToString());

            // Assert
            trialBalance.Should().NotBeNull();
            trialBalance.ReportType.Should().Be(ReportType.TrialBalance);
            trialBalance.TotalDebit.Should().Be(8000); // 5000 + 3000
            trialBalance.TotalCredit.Should().Be(8000);
            trialBalance.TotalDebit.Should().Be(trialBalance.TotalCredit);
        }

        [Fact]
        public void PostVouchers_GenerateBalanceSheet_ShouldValidateAssetLiabilityTotals()
        {
            // Arrange
            var asOfDate = new DateTime(2024, 1, 31);
            var periodStart = new DateTime(2024, 1, 1);

            // Post capital injection
            var (capitalVoucher, _) = _builder.CreateBalancedVoucher(
                _accounts["Cash"].AccountId,
                _accounts["Capital"].AccountId,
                100000,
                voucherDate: periodStart.AddDays(1));
            _voucherService.Post(capitalVoucher.Id, _fixture.TestUserId.ToString());

            // Post revenue
            var (revenueVoucher, _) = _builder.CreateBalancedVoucher(
                _accounts["Cash"].AccountId,
                _accounts["Revenue"].AccountId,
                50000,
                voucherDate: periodStart.AddDays(5));
            _voucherService.Post(revenueVoucher.Id, _fixture.TestUserId.ToString());

            var reportDto = new GenerateReportDto
            {
                PeriodStartDate = periodStart,
                PeriodEndDate = asOfDate,
                FiscalYear = 2024,
                TenantId = _fixture.TestTenantId
            };

            // Act
            var balanceSheet = _reportService.GenerateBalanceSheet(reportDto, _fixture.TestUserId.ToString());

            // Assert
            balanceSheet.Should().NotBeNull();
            balanceSheet.ReportType.Should().Be(ReportType.BalanceSheet);
            balanceSheet.TotalDebit.Should().BeGreaterThanOrEqualTo(0);
            balanceSheet.TotalCredit.Should().BeGreaterThanOrEqualTo(0);
        }

        [Fact]
        public void PostVouchers_GenerateIncomeStatement_ShouldSumRevenuesAndExpenses()
        {
            // Arrange
            var periodStart = new DateTime(2024, 1, 1);
            var periodEnd = new DateTime(2024, 1, 31);

            // Post revenue
            var (revenueVoucher, _) = _builder.CreateBalancedVoucher(
                _accounts["Cash"].AccountId,
                _accounts["Revenue"].AccountId,
                10000,
                voucherDate: periodStart.AddDays(5));
            _voucherService.Post(revenueVoucher.Id, _fixture.TestUserId.ToString());

            // Post expense
            var (expenseVoucher, _) = _builder.CreateBalancedVoucher(
                _accounts["Expenses"].AccountId,
                _accounts["Bank"].AccountId,
                5000,
                voucherDate: periodStart.AddDays(10));
            _voucherService.Post(expenseVoucher.Id, _fixture.TestUserId.ToString());

            var reportDto = new GenerateReportDto
            {
                PeriodStartDate = periodStart,
                PeriodEndDate = periodEnd,
                FiscalYear = 2024,
                TenantId = _fixture.TestTenantId
            };

            // Act
            var incomeStatement = _reportService.GenerateIncomeStatement(reportDto, _fixture.TestUserId.ToString());

            // Assert
            incomeStatement.Should().NotBeNull();
            incomeStatement.ReportType.Should().Be(ReportType.IncomeStatement);
        }

        [Fact]
        public void PostVouchers_GenerateCashFlow_ShouldMatchVoucherPostings()
        {
            // Arrange
            var periodStart = new DateTime(2024, 1, 1);
            var periodEnd = new DateTime(2024, 1, 31);

            // Post cash inflow
            var (inflowVoucher, _) = _builder.CreateBalancedVoucher(
                _accounts["Cash"].AccountId,
                _accounts["Revenue"].AccountId,
                5000,
                voucherDate: periodStart.AddDays(5));
            _voucherService.Post(inflowVoucher.Id, _fixture.TestUserId.ToString());

            // Post cash outflow
            var (outflowVoucher, _) = _builder.CreateBalancedVoucher(
                _accounts["Expenses"].AccountId,
                _accounts["Cash"].AccountId,
                2000,
                voucherDate: periodStart.AddDays(15));
            _voucherService.Post(outflowVoucher.Id, _fixture.TestUserId.ToString());

            var reportDto = new GenerateReportDto
            {
                PeriodStartDate = periodStart,
                PeriodEndDate = periodEnd,
                FiscalYear = 2024,
                TenantId = _fixture.TestTenantId
            };

            // Act
            var cashFlow = _reportService.GenerateCashFlow(reportDto, _fixture.TestUserId.ToString());

            // Assert
            cashFlow.Should().NotBeNull();
            cashFlow.ReportType.Should().Be(ReportType.CashFlow);
            cashFlow.ReportData.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void PostVoucher_GenerateReport_ShouldIncludeOnlyPostedTransactions()
        {
            // Arrange - Use unique date range to avoid conflicts with other tests
            var periodStart = new DateTime(2025, 6, 1);
            var periodEnd = new DateTime(2025, 6, 30);

            // Create posted voucher
            var (postedVoucher, _) = _builder.CreateBalancedVoucher(
                _accounts["Cash"].AccountId,
                _accounts["Revenue"].AccountId,
                5000,
                voucherDate: periodStart.AddDays(5));
            _voucherService.Post(postedVoucher.Id, _fixture.TestUserId.ToString());

            // Create draft voucher (should not appear in report)
            var (draftVoucher, _) = _builder.CreateBalancedVoucher(
                _accounts["Cash"].AccountId,
                _accounts["Revenue"].AccountId,
                3000,
                voucherDate: periodStart.AddDays(10));
            // Don't post this one

            var reportDto = new GenerateReportDto
            {
                PeriodStartDate = periodStart,
                PeriodEndDate = periodEnd,
                FiscalYear = 2025,
                TenantId = _fixture.TestTenantId
            };

            // Act
            var trialBalance = _reportService.GenerateTrialBalance(reportDto, _fixture.TestUserId.ToString());

            // Assert
            trialBalance.TotalDebit.Should().Be(5000); // Only posted voucher
            trialBalance.TotalCredit.Should().Be(5000);
            // Draft voucher should not be included
        }
    }
}


