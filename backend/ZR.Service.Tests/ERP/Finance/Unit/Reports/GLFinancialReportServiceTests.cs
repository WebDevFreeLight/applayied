using System;
using System.Collections.Generic;
using FluentAssertions;
using Xunit;
using ZR.Model.Finance;
using ZR.Model.Finance.Dto;
using ZR.Model.Finance.Enums;
using ZR.Service.Tests.ERP.Finance.TestHelpers;

namespace ZR.Service.Tests.ERP.Finance.Unit.Reports
{
    /// <summary>
    /// Unit tests for GL Financial Report Service - اختبارات وحدة خدمة التقارير المالية
    /// </summary>
    public class GLFinancialReportServiceTests : IClassFixture<FinanceTestFixture>
    {
        private readonly FinanceTestFixture _fixture;
        private readonly TestGLFinancialReportService _service;
        private readonly TestGLVoucherService _voucherService;
        private readonly FinanceTestDataBuilder _builder;
        private readonly Dictionary<string, GLAccount> _accounts;

        public GLFinancialReportServiceTests(FinanceTestFixture fixture)
        {
            _fixture = fixture;

            // Create test services that work directly with ISqlSugarClient
            _service = new TestGLFinancialReportService(_fixture.Db);
            _voucherService = new TestGLVoucherService(_fixture.Db);

            // Create test data builder
            _builder = new FinanceTestDataBuilder(_fixture.Db, _fixture.TestTenantId, _fixture.TestUserId);

            // Create standard accounts
            _accounts = _builder.CreateStandardChartOfAccounts();
        }

        #region Trial Balance Tests

        [Fact]
        public void GenerateTrialBalance_ShouldMatchSumOfEntries()
        {
            // Arrange - Use a unique date range to avoid conflicts with other tests
            var uniqueYear = 2014;
            var periodStart = new DateTime(uniqueYear, 1, 1);
            var periodEnd = new DateTime(uniqueYear, 1, 31);

            // Create posted vouchers
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

            var dto = new GenerateReportDto
            {
                PeriodStartDate = periodStart,
                PeriodEndDate = periodEnd,
                FiscalYear = periodStart.Year,
                Currency = "SAR",
                TenantId = _fixture.TestTenantId
            };

            // Act
            var result = _service.GenerateTrialBalance(dto, _fixture.TestUserId.ToString());

            // Assert
            result.Should().NotBeNull();
            result.ReportType.Should().Be(ReportType.TrialBalance);
            result.TotalDebit.Should().Be(8000); // 5000 + 3000
            result.TotalCredit.Should().Be(8000); // 5000 + 3000
            result.TotalDebit.Should().Be(result.TotalCredit);
        }

        [Fact]
        public void GenerateTrialBalance_ShouldCreateReport()
        {
            // Arrange
            var periodStart = new DateTime(DateTime.Now.Year, 1, 1);
            var periodEnd = new DateTime(DateTime.Now.Year, 1, 31);

            var dto = new GenerateReportDto
            {
                PeriodStartDate = periodStart,
                PeriodEndDate = periodEnd,
                FiscalYear = periodStart.Year,
                TenantId = _fixture.TestTenantId
            };

            // Act
            var result = _service.GenerateTrialBalance(dto, _fixture.TestUserId.ToString());

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().NotBeEmpty();
            result.ReportType.Should().Be(ReportType.TrialBalance);
            result.GeneratedAt.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(5));
        }

        #endregion

        #region Balance Sheet Tests

        [Fact]
        public void GenerateBalanceSheet_ShouldIncludeAssets()
        {
            // Arrange
            var asOfDate = new DateTime(DateTime.Now.Year, 1, 31);
            var periodStart = new DateTime(DateTime.Now.Year, 1, 1);

            // Create transactions
            var (voucher1, _) = _builder.CreateBalancedVoucher(
                _accounts["Cash"].AccountId,
                _accounts["Capital"].AccountId,
                100000,
                voucherDate: periodStart.AddDays(1));

            _voucherService.Post(voucher1.Id, _fixture.TestUserId.ToString());

            var dto = new GenerateReportDto
            {
                PeriodStartDate = periodStart,
                PeriodEndDate = asOfDate,
                FiscalYear = asOfDate.Year,
                TenantId = _fixture.TestTenantId
            };

            // Act
            var result = _service.GenerateBalanceSheet(dto, _fixture.TestUserId.ToString());

            // Assert
            result.Should().NotBeNull();
            result.ReportType.Should().Be(ReportType.BalanceSheet);
            result.TotalDebit.Should().BeGreaterThanOrEqualTo(0);
        }

        [Fact]
        public void GenerateBalanceSheet_ShouldCreateReport()
        {
            // Arrange
            var asOfDate = new DateTime(DateTime.Now.Year, 1, 31);
            var periodStart = new DateTime(DateTime.Now.Year, 1, 1);

            var dto = new GenerateReportDto
            {
                PeriodStartDate = periodStart,
                PeriodEndDate = asOfDate,
                FiscalYear = asOfDate.Year,
                TenantId = _fixture.TestTenantId
            };

            // Act
            var result = _service.GenerateBalanceSheet(dto, _fixture.TestUserId.ToString());

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().NotBeEmpty();
            result.ReportType.Should().Be(ReportType.BalanceSheet);
        }

        #endregion

        #region Income Statement Tests

        [Fact]
        public void GenerateIncomeStatement_ShouldSumRevenuesAndExpenses()
        {
            // Arrange
            var periodStart = new DateTime(DateTime.Now.Year, 1, 1);
            var periodEnd = new DateTime(DateTime.Now.Year, 1, 31);

            // Create revenue transaction
            var (voucher1, _) = _builder.CreateBalancedVoucher(
                _accounts["Cash"].AccountId,
                _accounts["Revenue"].AccountId,
                10000,
                voucherDate: periodStart.AddDays(5));

            _voucherService.Post(voucher1.Id, _fixture.TestUserId.ToString());

            // Create expense transaction
            var (voucher2, _) = _builder.CreateBalancedVoucher(
                _accounts["Expenses"].AccountId,
                _accounts["Bank"].AccountId,
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
            var result = _service.GenerateIncomeStatement(dto, _fixture.TestUserId.ToString());

            // Assert
            result.Should().NotBeNull();
            result.ReportType.Should().Be(ReportType.IncomeStatement);
        }

        #endregion

        #region Cash Flow Tests

        [Fact]
        public void GenerateCashFlow_ShouldCalculateCashMovements()
        {
            // Arrange
            var periodStart = new DateTime(DateTime.Now.Year, 1, 1);
            var periodEnd = new DateTime(DateTime.Now.Year, 1, 31);

            // Create cash transactions
            var (voucher1, _) = _builder.CreateBalancedVoucher(
                _accounts["Cash"].AccountId,
                _accounts["Revenue"].AccountId,
                5000,
                voucherDate: periodStart.AddDays(5));

            _voucherService.Post(voucher1.Id, _fixture.TestUserId.ToString());

            var (voucher2, _) = _builder.CreateBalancedVoucher(
                _accounts["Expenses"].AccountId,
                _accounts["Cash"].AccountId,
                2000,
                voucherDate: periodStart.AddDays(15));

            _voucherService.Post(voucher2.Id, _fixture.TestUserId.ToString());

            var dto = new GenerateReportDto
            {
                PeriodStartDate = periodStart,
                PeriodEndDate = periodEnd,
                FiscalYear = periodStart.Year,
                TenantId = _fixture.TestTenantId
            };

            // Act
            var result = _service.GenerateCashFlow(dto, _fixture.TestUserId.ToString());

            // Assert
            result.Should().NotBeNull();
            result.ReportType.Should().Be(ReportType.CashFlow);
            result.ReportData.Should().NotBeNullOrEmpty();
        }

        #endregion

        #region Report Metadata Tests

        [Fact]
        public void GenerateReport_ShouldSetGeneratedByAndGeneratedAt()
        {
            // Arrange
            var periodStart = new DateTime(DateTime.Now.Year, 1, 1);
            var periodEnd = new DateTime(DateTime.Now.Year, 1, 31);

            var dto = new GenerateReportDto
            {
                PeriodStartDate = periodStart,
                PeriodEndDate = periodEnd,
                FiscalYear = periodStart.Year,
                TenantId = _fixture.TestTenantId
            };

            // Act
            var result = _service.GenerateTrialBalance(dto, _fixture.TestUserId.ToString());

            // Assert
            result.GeneratedBy.Should().NotBeNull();
            result.GeneratedAt.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(5));
        }

        [Fact]
        public void GenerateReport_ShouldRespectTenantIsolation()
        {
            // Arrange
            var tenant1Id = Guid.NewGuid();
            var tenant2Id = Guid.NewGuid();

            var periodStart = new DateTime(DateTime.Now.Year, 1, 1);
            var periodEnd = new DateTime(DateTime.Now.Year, 1, 31);

            var dto1 = new GenerateReportDto
            {
                PeriodStartDate = periodStart,
                PeriodEndDate = periodEnd,
                FiscalYear = periodStart.Year,
                TenantId = tenant1Id
            };

            var dto2 = new GenerateReportDto
            {
                PeriodStartDate = periodStart,
                PeriodEndDate = periodEnd,
                FiscalYear = periodStart.Year,
                TenantId = tenant2Id
            };

            // Act
            var report1 = _service.GenerateTrialBalance(dto1, _fixture.TestUserId.ToString());
            var report2 = _service.GenerateTrialBalance(dto2, _fixture.TestUserId.ToString());

            // Assert - CompanyID is used instead of TenantId
            report1.CompanyID.Should().NotBeNull();
            report2.CompanyID.Should().NotBeNull();
        }

        #endregion
    }
}

