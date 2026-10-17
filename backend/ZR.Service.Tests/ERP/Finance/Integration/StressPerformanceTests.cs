using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FluentAssertions;
using Xunit;
using ZR.Model.Finance;
using ZR.Model.Finance.Dto;
using ZR.Model.Finance.Enums;
using ZR.Service.Tests.ERP.Finance.TestHelpers;

namespace ZR.Service.Tests.ERP.Finance.Integration
{
    /// <summary>
    /// Stress & Performance Tests - اختبارات الضغط والأداء
    /// </summary>
    public class StressPerformanceTests : IClassFixture<FinanceTestFixture>
    {
        private readonly FinanceTestFixture _fixture;
        private readonly TestGLVoucherService _voucherService;
        private readonly TestGLAccountService _accountService;
        private readonly TestGLFinancialReportService _reportService;
        private readonly FinanceTestDataBuilder _builder;
        private readonly Dictionary<string, GLAccount> _accounts;

        public StressPerformanceTests(FinanceTestFixture fixture)
        {
            _fixture = fixture;
            _voucherService = new TestGLVoucherService(_fixture.Db);
            _accountService = new TestGLAccountService(_fixture.Db);
            _reportService = new TestGLFinancialReportService(_fixture.Db);
            _builder = new FinanceTestDataBuilder(_fixture.Db, _fixture.TestTenantId, _fixture.TestUserId);
            _accounts = _builder.CreateStandardChartOfAccounts();
        }

        [Fact]
        public void BulkCreateVouchers_1000Vouchers_ShouldCompleteInReasonableTime()
        {
            // Arrange
            const int voucherCount = 1000;
            var stopwatch = Stopwatch.StartNew();

            // Act - Create and post 1000 vouchers
            for (int i = 0; i < voucherCount; i++)
            {
                var (voucher, _) = _builder.CreateBalancedVoucher(
                    _accounts["Cash"].AccountId,
                    _accounts["Revenue"].AccountId,
                    amount: 100 + i, // Varying amounts
                    voucherDate: DateTime.Now.AddDays(-voucherCount + i));

                _voucherService.Post(voucher.Id, _fixture.TestUserId.ToString());
            }

            stopwatch.Stop();

            // Assert
            var vouchers = _fixture.Db.Queryable<GLVoucher>()
                .Where(v => v.CompanyID != null)
                .Count();

            vouchers.Should().BeGreaterThanOrEqualTo(voucherCount);
            stopwatch.ElapsedMilliseconds.Should().BeLessThan(60000); // Should complete in under 60 seconds
        }

        [Fact]
        public void BulkCreateVouchers_ValidateBalances_ShouldBeAccurate()
        {
            // Arrange
            const int voucherCount = 500;
            var totalAmount = 0m;
            var initialCashBalance = _accountService.GetAccountBalance(_accounts["Cash"].AccountId);
            var initialRevenueBalance = _accountService.GetAccountBalance(_accounts["Revenue"].AccountId);

            // Act - Create and post vouchers
            for (int i = 0; i < voucherCount; i++)
            {
                var amount = 100m + i;
                totalAmount += amount;

                var (voucher, _) = _builder.CreateBalancedVoucher(
                    _accounts["Cash"].AccountId,
                    _accounts["Revenue"].AccountId,
                    amount);

                _voucherService.Post(voucher.Id, _fixture.TestUserId.ToString());
            }

            // Assert - Validate balances
            var cashBalance = _accountService.GetAccountBalance(_accounts["Cash"].AccountId);
            var revenueBalance = _accountService.GetAccountBalance(_accounts["Revenue"].AccountId);

            var expectedCash = initialCashBalance + totalAmount;
            var expectedRevenue = initialRevenueBalance + totalAmount;

            cashBalance.Should().BeApproximately(expectedCash, 0.01m);
            revenueBalance.Should().BeApproximately(expectedRevenue, 0.01m);
        }

        [Fact]
        public void GenerateReport_HighVolumeData_ShouldCompleteInReasonableTime()
        {
            // Arrange
            const int voucherCount = 1000;
            var periodStart = new DateTime(2024, 1, 1);
            var periodEnd = new DateTime(2024, 1, 31);

            // Create high volume of vouchers
            for (int i = 0; i < voucherCount; i++)
            {
                var (voucher, _) = _builder.CreateBalancedVoucher(
                    _accounts["Cash"].AccountId,
                    _accounts["Revenue"].AccountId,
                    amount: 100,
                    voucherDate: periodStart.AddDays(i % 31));

                _voucherService.Post(voucher.Id, _fixture.TestUserId.ToString());
            }

            var reportDto = new GenerateReportDto
            {
                PeriodStartDate = periodStart,
                PeriodEndDate = periodEnd,
                FiscalYear = 2024,
                TenantId = _fixture.TestTenantId
            };

            // Act
            var stopwatch = Stopwatch.StartNew();
            var trialBalance = _reportService.GenerateTrialBalance(reportDto, _fixture.TestUserId.ToString());
            stopwatch.Stop();

            // Assert
            trialBalance.Should().NotBeNull();
            var expectedTotal = 100m * voucherCount;
            trialBalance.TotalDebit.Should().BeApproximately(expectedTotal, 0.01m);
            stopwatch.ElapsedMilliseconds.Should().BeLessThan(10000); // Should complete in under 10 seconds
        }

        [Fact]
        public void GenerateMultipleReports_ShouldCompleteEfficiently()
        {
            // Arrange
            const int voucherCount = 500;
            var periodStart = new DateTime(2024, 1, 1);
            var periodEnd = new DateTime(2024, 1, 31);

            // Create vouchers
            for (int i = 0; i < voucherCount; i++)
            {
                var (voucher, _) = _builder.CreateBalancedVoucher(
                    _accounts["Cash"].AccountId,
                    _accounts["Revenue"].AccountId,
                    amount: 100,
                    voucherDate: periodStart.AddDays(i % 31));

                _voucherService.Post(voucher.Id, _fixture.TestUserId.ToString());
            }

            var reportDto = new GenerateReportDto
            {
                PeriodStartDate = periodStart,
                PeriodEndDate = periodEnd,
                FiscalYear = 2024,
                TenantId = _fixture.TestTenantId
            };

            // Act - Generate multiple reports
            var stopwatch = Stopwatch.StartNew();
            var trialBalance = _reportService.GenerateTrialBalance(reportDto, _fixture.TestUserId.ToString());
            var balanceSheet = _reportService.GenerateBalanceSheet(reportDto, _fixture.TestUserId.ToString());
            var incomeStatement = _reportService.GenerateIncomeStatement(reportDto, _fixture.TestUserId.ToString());
            var cashFlow = _reportService.GenerateCashFlow(reportDto, _fixture.TestUserId.ToString());
            stopwatch.Stop();

            // Assert
            trialBalance.Should().NotBeNull();
            balanceSheet.Should().NotBeNull();
            incomeStatement.Should().NotBeNull();
            cashFlow.Should().NotBeNull();
            stopwatch.ElapsedMilliseconds.Should().BeLessThan(15000); // All reports in under 15 seconds
        }

        [Fact]
        public void ConcurrentVoucherCreation_ShouldMaintainDataIntegrity()
        {
            // Arrange
            const int concurrentVouchers = 100;
            var accounts = _builder.CreateStandardChartOfAccounts();
            var initialCashBalance = _accountService.GetAccountBalance(accounts["Cash"].AccountId);

            // Act - Create vouchers (simulating concurrent operations)
            var vouchers = new List<GLVoucher>();
            for (int i = 0; i < concurrentVouchers; i++)
            {
                var (voucher, _) = _builder.CreateBalancedVoucher(
                    accounts["Cash"].AccountId,
                    accounts["Revenue"].AccountId,
                    amount: 100);
                vouchers.Add(voucher);
            }

            // Post all vouchers
            foreach (var voucher in vouchers)
            {
                _voucherService.Post(voucher.Id, _fixture.TestUserId.ToString());
            }

            // Assert - Validate data integrity
            var postedCount = _fixture.Db.Queryable<GLVoucher>()
                .Where(v => v.CompanyID != null && v.Status == VoucherStatus.Posted)
                .Count();

            postedCount.Should().BeGreaterThanOrEqualTo(concurrentVouchers);

            // Validate balances
            var cashBalance = _accountService.GetAccountBalance(accounts["Cash"].AccountId);
            var expectedBalance = initialCashBalance + (100m * concurrentVouchers);
            cashBalance.Should().BeApproximately(expectedBalance, 0.01m);
        }

        [Fact]
        public void LargeVoucher_WithManyEntries_ShouldProcessCorrectly()
        {
            // Arrange
            var voucher = _builder.CreateVoucher();
            var revenueAccount = _accounts["Revenue"];
            var totalDebit = 0m;
            var lineNumber = 1;

            // Create 50 debit entries
            for (int i = 0; i < 50; i++)
            {
                var account = _builder.CreateAccount($"1{i:D3}", $"حساب {i}", $"Account {i}", AccountType.Asset);
                var amount = 100m + i;
                _builder.CreateVoucherEntry(voucher.Id, account.AccountId, debit: amount, lineNumber: lineNumber++);
                totalDebit += amount;
            }

            // Create 1 credit entry to balance
            _builder.CreateVoucherEntry(voucher.Id, revenueAccount.AccountId, credit: totalDebit, lineNumber: lineNumber);

            // Update voucher totals
            voucher.TotalDebit = totalDebit;
            voucher.TotalCredit = totalDebit;
            _fixture.Db.Updateable(voucher).ExecuteCommand();

            // Act
            var validation = _voucherService.ValidateDoubleEntry(voucher.Id);
            _voucherService.Post(voucher.Id, _fixture.TestUserId.ToString());

            // Assert
            validation.IsValid.Should().BeTrue();
            var posted = _voucherService.GetById(voucher.Id);
            posted.Status.Should().Be(VoucherStatus.Posted);
            posted.TotalDebit.Should().Be(totalDebit);
            posted.TotalCredit.Should().Be(totalDebit);
        }
    }
}




























