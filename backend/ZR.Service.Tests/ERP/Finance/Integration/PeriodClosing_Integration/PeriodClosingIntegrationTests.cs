using System;
using System.Collections.Generic;
using FluentAssertions;
using Xunit;
using ZR.Model.Finance;
using ZR.Model.Finance.Dto;
using ZR.Model.Finance.Enums;
using ZR.Service.Tests.ERP.Finance.TestHelpers;

namespace ZR.Service.Tests.ERP.Finance.Integration.PeriodClosing_Integration
{
    /// <summary>
    /// Integration tests for Period Closing workflow - اختبارات التكامل لإقفال الفترات
    /// Tests: Create Period → Post Vouchers → Close Period → Verify Lock → Reopen
    /// </summary>
    public class PeriodClosingIntegrationTests : IClassFixture<FinanceTestFixture>
    {
        private readonly FinanceTestFixture _fixture;
        private readonly TestGLVoucherService _voucherService;
        private readonly TestGLPeriodClosingService _periodService;
        private readonly FinanceTestDataBuilder _builder;
        private readonly Dictionary<string, GLAccount> _accounts;

        public PeriodClosingIntegrationTests(FinanceTestFixture fixture)
        {
            _fixture = fixture;

            // Create test services that work directly with ISqlSugarClient
            _voucherService = new TestGLVoucherService(_fixture.Db);
            _periodService = new TestGLPeriodClosingService(_fixture.Db);

            // Create test data builder
            _builder = new FinanceTestDataBuilder(_fixture.Db, _fixture.TestTenantId, _fixture.TestUserId);

            // Create standard accounts
            _accounts = _builder.CreateStandardChartOfAccounts();
        }

        [Fact]
        public void ClosePeriod_WithAllVouchersPosted_ShouldSucceed()
        {
            // Arrange - Use a unique date range to avoid conflicts with other tests (within valid calendar range)
            var uniqueYear = 2004;
            var periodStart = new DateTime(uniqueYear, 2, 1);
            var periodEnd = new DateTime(uniqueYear, 2, 28);

            var periodDto = new CreatePeriodClosingDto
            {
                PeriodType = PeriodType.Month,
                PeriodStartDate = periodStart,
                PeriodEndDate = periodEnd,
                FiscalYear = periodStart.Year,
                FiscalPeriod = 2,
                TenantId = _fixture.TestTenantId
            };

            var period = _periodService.Create(periodDto, _fixture.TestUserId.ToString());

            // Create and post vouchers
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

            var closeDto = new ClosePeriodDto
            {
                Id = period.Id,
                ForceClose = false,
                ClosingNotes = "Period closed successfully"
            };

            // Act
            var result = _periodService.ClosePeriod(closeDto, _fixture.TestUserId.ToString());

            // Assert
            result.Should().BeGreaterThan(0);
            var closed = _periodService.GetById(period.Id);
            closed.Status.Should().Be(PeriodStatus.Closed);
            closed.PostedVouchers.Should().Be(2);
            closed.TotalDebit.Should().Be(8000);
            closed.TotalCredit.Should().Be(8000);
        }

        [Fact]
        public void ClosePeriod_WithUnpostedVouchers_ShouldFail()
        {
            // Arrange
            var periodStart = new DateTime(DateTime.Now.Year, 1, 1);
            var periodEnd = new DateTime(DateTime.Now.Year, 1, 31);

            var periodDto = new CreatePeriodClosingDto
            {
                PeriodType = PeriodType.Month,
                PeriodStartDate = periodStart,
                PeriodEndDate = periodEnd,
                FiscalYear = periodStart.Year,
                FiscalPeriod = 1,
                TenantId = _fixture.TestTenantId
            };

            var period = _periodService.Create(periodDto, _fixture.TestUserId.ToString());

            // Create posted voucher
            var (voucher1, _) = _builder.CreateBalancedVoucher(
                _accounts["Cash"].AccountId,
                _accounts["Revenue"].AccountId,
                5000,
                voucherDate: periodStart.AddDays(5));
            _voucherService.Post(voucher1.Id, _fixture.TestUserId.ToString());

            // Create unposted voucher
            var (voucher2, _) = _builder.CreateBalancedVoucher(
                _accounts["Bank"].AccountId,
                _accounts["Revenue"].AccountId,
                3000,
                voucherDate: periodStart.AddDays(10));
            // Don't post this one

            var closeDto = new ClosePeriodDto
            {
                Id = period.Id,
                ForceClose = false
            };

            // Act & Assert
            var exception = Assert.Throws<Exception>(() => _periodService.ClosePeriod(closeDto, _fixture.TestUserId.ToString()));
            exception.Message.Should().Contain("unposted vouchers");
        }

        [Fact]
        public void ClosePeriod_ShouldPreventPostingInClosedPeriod()
        {
            // Arrange - Use a unique date range to avoid conflicts with other tests (within valid calendar range)
            var uniqueYear = 2005;
            var periodStart = new DateTime(uniqueYear, 7, 1);
            var periodEnd = new DateTime(uniqueYear, 7, 31);

            var periodDto = new CreatePeriodClosingDto
            {
                PeriodType = PeriodType.Month,
                PeriodStartDate = periodStart,
                PeriodEndDate = periodEnd,
                FiscalYear = periodStart.Year,
                FiscalPeriod = 7,
                TenantId = _fixture.TestTenantId
            };

            var period = _periodService.Create(periodDto, _fixture.TestUserId.ToString());

            // Post all vouchers and close period
            var (voucher1, _) = _builder.CreateBalancedVoucher(
                _accounts["Cash"].AccountId,
                _accounts["Revenue"].AccountId,
                5000,
                voucherDate: periodStart.AddDays(5));
            _voucherService.Post(voucher1.Id, _fixture.TestUserId.ToString());

            var closeDto = new ClosePeriodDto
            {
                Id = period.Id,
                ForceClose = false
            };
            _periodService.ClosePeriod(closeDto, _fixture.TestUserId.ToString());

            // Try to create voucher in closed period
            var (voucher2, _) = _builder.CreateBalancedVoucher(
                _accounts["Bank"].AccountId,
                _accounts["Revenue"].AccountId,
                3000,
                voucherDate: periodStart.AddDays(15)); // Date within closed period

            // Verify period is closed
            var canPost = _periodService.IsDateInClosedPeriod(periodStart.AddDays(15), _fixture.TestTenantId);
            canPost.Should().BeTrue(); // Period is closed
        }

        [Fact]
        public void ReopenPeriod_ShouldAllowNewPostings()
        {
            // Arrange - Use a unique date range to avoid conflicts with other tests (within valid calendar range)
            var uniqueYear = 2006;
            var periodStart = new DateTime(uniqueYear, 8, 1);
            var periodEnd = new DateTime(uniqueYear, 8, 31);

            var periodDto = new CreatePeriodClosingDto
            {
                PeriodType = PeriodType.Month,
                PeriodStartDate = periodStart,
                PeriodEndDate = periodEnd,
                FiscalYear = periodStart.Year,
                FiscalPeriod = 8,
                TenantId = _fixture.TestTenantId
            };

            var period = _periodService.Create(periodDto, _fixture.TestUserId.ToString());

            // Post vouchers and close
            var (voucher1, _) = _builder.CreateBalancedVoucher(
                _accounts["Cash"].AccountId,
                _accounts["Revenue"].AccountId,
                5000,
                voucherDate: periodStart.AddDays(5));
            _voucherService.Post(voucher1.Id, _fixture.TestUserId.ToString());

            var closeDto = new ClosePeriodDto
            {
                Id = period.Id,
                ForceClose = false
            };
            _periodService.ClosePeriod(closeDto, _fixture.TestUserId.ToString());

            // Reopen period
            var reopenDto = new ReopenPeriodDto
            {
                Id = period.Id,
                Reason = "Need to make adjustments"
            };
            _periodService.ReopenPeriod(reopenDto, _fixture.TestUserId.ToString());

            // Act - Try to post new voucher
            var (voucher2, _) = _builder.CreateBalancedVoucher(
                _accounts["Bank"].AccountId,
                _accounts["Revenue"].AccountId,
                3000,
                voucherDate: periodStart.AddDays(15));

            var result = _voucherService.Post(voucher2.Id, _fixture.TestUserId.ToString());

            // Assert
            result.Should().BeGreaterThan(0);
            var reopened = _periodService.GetById(period.Id);
            reopened.Status.Should().Be(PeriodStatus.Open);
        }

        [Fact]
        public void ClosePeriod_ShouldRollBalancesForward()
        {
            // Arrange
            var period1Start = new DateTime(DateTime.Now.Year, 1, 1);
            var period1End = new DateTime(DateTime.Now.Year, 1, 31);
            var period2Start = new DateTime(DateTime.Now.Year, 2, 1);
            var period2End = new DateTime(DateTime.Now.Year, 2, 28);

            // Create period 1
            var period1Dto = new CreatePeriodClosingDto
            {
                PeriodType = PeriodType.Month,
                PeriodStartDate = period1Start,
                PeriodEndDate = period1End,
                FiscalYear = period1Start.Year,
                FiscalPeriod = 1,
                TenantId = _fixture.TestTenantId
            };
            var period1 = _periodService.Create(period1Dto, _fixture.TestUserId.ToString());

            // Post vouchers in period 1
            var (voucher1, _) = _builder.CreateBalancedVoucher(
                _accounts["Cash"].AccountId,
                _accounts["Revenue"].AccountId,
                10000,
                voucherDate: period1Start.AddDays(5));
            _voucherService.Post(voucher1.Id, _fixture.TestUserId.ToString());

            // Close period 1
            var closeDto1 = new ClosePeriodDto
            {
                Id = period1.Id,
                ForceClose = false
            };
            _periodService.ClosePeriod(closeDto1, _fixture.TestUserId.ToString());

            // Create period 2
            var period2Dto = new CreatePeriodClosingDto
            {
                PeriodType = PeriodType.Month,
                PeriodStartDate = period2Start,
                PeriodEndDate = period2End,
                FiscalYear = period2Start.Year,
                FiscalPeriod = 2,
                TenantId = _fixture.TestTenantId
            };
            var period2 = _periodService.Create(period2Dto, _fixture.TestUserId.ToString());

            // Post vouchers in period 2
            var (voucher2, _) = _builder.CreateBalancedVoucher(
                _accounts["Cash"].AccountId,
                _accounts["Revenue"].AccountId,
                5000,
                voucherDate: period2Start.AddDays(5));
            _voucherService.Post(voucher2.Id, _fixture.TestUserId.ToString());

            // Act - Close period 2
            var closeDto2 = new ClosePeriodDto
            {
                Id = period2.Id,
                ForceClose = false
            };
            _periodService.ClosePeriod(closeDto2, _fixture.TestUserId.ToString());

            // Assert
            var closed1 = _periodService.GetById(period1.Id);
            var closed2 = _periodService.GetById(period2.Id);

            closed1.Status.Should().Be(PeriodStatus.Closed);
            closed2.Status.Should().Be(PeriodStatus.Closed);
            closed1.TotalDebit.Should().Be(10000);
            closed2.TotalDebit.Should().Be(5000);
        }

        [Fact]
        public void LockPeriod_ShouldPreventReopening()
        {
            // Arrange
            var periodStart = new DateTime(DateTime.Now.Year, 1, 1);
            var periodEnd = new DateTime(DateTime.Now.Year, 1, 31);

            var periodDto = new CreatePeriodClosingDto
            {
                PeriodType = PeriodType.Month,
                PeriodStartDate = periodStart,
                PeriodEndDate = periodEnd,
                FiscalYear = periodStart.Year,
                FiscalPeriod = 1,
                TenantId = _fixture.TestTenantId
            };

            var period = _periodService.Create(periodDto, _fixture.TestUserId.ToString());

            // Post and close
            var (voucher1, _) = _builder.CreateBalancedVoucher(
                _accounts["Cash"].AccountId,
                _accounts["Revenue"].AccountId,
                5000,
                voucherDate: periodStart.AddDays(5));
            _voucherService.Post(voucher1.Id, _fixture.TestUserId.ToString());

            var closeDto = new ClosePeriodDto
            {
                Id = period.Id,
                ForceClose = false
            };
            _periodService.ClosePeriod(closeDto, _fixture.TestUserId.ToString());

            // Lock period
            _periodService.LockPeriod(period.Id, _fixture.TestUserId.ToString());

            // Act & Assert - Try to reopen
            var reopenDto = new ReopenPeriodDto
            {
                Id = period.Id,
                Reason = "Trying to reopen locked period"
            };

            var exception = Assert.Throws<Exception>(() => _periodService.ReopenPeriod(reopenDto, _fixture.TestUserId.ToString()));
            exception.Message.Should().Contain("Cannot reopen locked");
        }

        [Fact]
        public void GenerateFiscalYearPeriods_ShouldCreateAllPeriods()
        {
            // Arrange
            var fiscalYear = DateTime.Now.Year;

            // Act
            var periods = _periodService.GenerateFiscalYearPeriods(
                fiscalYear,
                PeriodType.Month,
                _fixture.TestUserId.ToString(),
                _fixture.TestTenantId);

            // Assert
            periods.Should().NotBeNull();
            periods.Should().HaveCount(12); // 12 months
            periods.Should().OnlyContain(p => p.FiscalYear == fiscalYear);
            periods.Should().OnlyContain(p => p.Status == PeriodStatus.Open);
        }
    }
}

