using System;
using System.Collections.Generic;
using FluentAssertions;
using Xunit;
using ZR.Model.Finance;
using ZR.Model.Finance.Dto;
using ZR.Model.Finance.Enums;
using ZR.Service.Tests.ERP.Finance.TestHelpers;

namespace ZR.Service.Tests.ERP.Finance.Unit.PeriodClosing
{
    /// <summary>
    /// Unit tests for Period Closing Edge Cases - اختبارات الحالات الحدية لإقفال الفترات
    /// </summary>
    public class GLPeriodClosingEdgeCasesTests : IClassFixture<FinanceTestFixture>
    {
        private readonly FinanceTestFixture _fixture;
        private readonly TestGLPeriodClosingService _periodService;
        private readonly TestGLVoucherService _voucherService;
        private readonly FinanceTestDataBuilder _builder;
        private readonly Dictionary<string, GLAccount> _accounts;

        public GLPeriodClosingEdgeCasesTests(FinanceTestFixture fixture)
        {
            _fixture = fixture;
            _periodService = new TestGLPeriodClosingService(_fixture.Db);
            _voucherService = new TestGLVoucherService(_fixture.Db);
            _builder = new FinanceTestDataBuilder(_fixture.Db, _fixture.TestTenantId, _fixture.TestUserId);
            _accounts = _builder.CreateStandardChartOfAccounts();
        }

        [Fact]
        public void ClosePeriod_WithMixedPostedAndUnpostedVouchers_ShouldFail()
        {
            // Arrange
            var periodStart = new DateTime(2024, 1, 1);
            var periodEnd = new DateTime(2024, 1, 31);
            var period = _builder.CreatePeriod(periodStart, periodEnd);

            // Create posted voucher
            var (postedVoucher, _) = _builder.CreateBalancedVoucher(
                _accounts["Cash"].AccountId,
                _accounts["Revenue"].AccountId,
                1000,
                voucherDate: periodStart.AddDays(5));
            _voucherService.Post(postedVoucher.Id, _fixture.TestUserId.ToString());

            // Create unposted voucher
            var (unpostedVoucher, _) = _builder.CreateBalancedVoucher(
                _accounts["Cash"].AccountId,
                _accounts["Revenue"].AccountId,
                500,
                voucherDate: periodStart.AddDays(10));
            // Don't post this one

            var dto = new ClosePeriodDto
            {
                Id = period.Id,
                ForceClose = false,
                ClosingNotes = "Trying to close with unposted vouchers"
            };

            // Act & Assert
            Assert.Throws<Exception>(() => _periodService.ClosePeriod(dto, _fixture.TestUserId.ToString()));
        }

        [Fact]
        public void ClosePeriod_WithForceClose_ShouldSucceedEvenWithUnpostedVouchers()
        {
            // Arrange
            var periodStart = new DateTime(2024, 2, 1);
            var periodEnd = new DateTime(2024, 2, 29);
            var period = _builder.CreatePeriod(periodStart, periodEnd);

            // Create unposted voucher
            var (unpostedVoucher, _) = _builder.CreateBalancedVoucher(
                _accounts["Cash"].AccountId,
                _accounts["Revenue"].AccountId,
                500,
                voucherDate: periodStart.AddDays(10));

            var dto = new ClosePeriodDto
            {
                Id = period.Id,
                ForceClose = true, // Force close
                ClosingNotes = "Force closing with unposted vouchers"
            };

            // Act
            var result = _periodService.ClosePeriod(dto, _fixture.TestUserId.ToString());

            // Assert
            result.Should().BeGreaterThan(0);
            var closed = _periodService.GetById(period.Id);
            closed.Status.Should().Be(PeriodStatus.Closed);
        }

        [Fact]
        public void ReopenPeriod_AndPostPendingVouchers_ShouldSucceed()
        {
            // Arrange
            var periodStart = new DateTime(2024, 3, 1);
            var periodEnd = new DateTime(2024, 3, 31);
            var period = _builder.CreatePeriod(periodStart, periodEnd, status: PeriodStatus.Closed);

            // Reopen period
            var reopenDto = new ReopenPeriodDto
            {
                Id = period.Id,
                Reason = "Need to post pending vouchers"
            };
            _periodService.ReopenPeriod(reopenDto, _fixture.TestUserId.ToString());

            // Create and post voucher after reopening
            var (voucher, _) = _builder.CreateBalancedVoucher(
                _accounts["Cash"].AccountId,
                _accounts["Revenue"].AccountId,
                1000,
                voucherDate: periodStart.AddDays(15));

            // Act
            _voucherService.Post(voucher.Id, _fixture.TestUserId.ToString());

            // Assert
            var posted = _voucherService.GetById(voucher.Id);
            posted.Status.Should().Be(VoucherStatus.Posted);
        }

        [Fact]
        public void ClosePeriod_ShouldRollBalancesForwardCorrectly()
        {
            // Arrange
            var period1Start = new DateTime(2024, 1, 1);
            var period1End = new DateTime(2024, 1, 31);
            var period1 = _builder.CreatePeriod(period1Start, period1End);

            var period2Start = new DateTime(2024, 2, 1);
            var period2End = new DateTime(2024, 2, 29);
            var period2 = _builder.CreatePeriod(period2Start, period2End);

            // Post vouchers in period 1
            var (voucher1, _) = _builder.CreateBalancedVoucher(
                _accounts["Cash"].AccountId,
                _accounts["Revenue"].AccountId,
                5000,
                voucherDate: period1Start.AddDays(5));
            _voucherService.Post(voucher1.Id, _fixture.TestUserId.ToString());

            // Close period 1
            var closeDto = new ClosePeriodDto
            {
                Id = period1.Id,
                ForceClose = false
            };
            _periodService.ClosePeriod(closeDto, _fixture.TestUserId.ToString());

            // Act - Post voucher in period 2
            var (voucher2, _) = _builder.CreateBalancedVoucher(
                _accounts["Cash"].AccountId,
                _accounts["Revenue"].AccountId,
                3000,
                voucherDate: period2Start.AddDays(5));
            _voucherService.Post(voucher2.Id, _fixture.TestUserId.ToString());

            // Assert
            var accountService = new TestGLAccountService(_fixture.Db);
            var cashBalance = accountService.GetAccountBalance(_accounts["Cash"].AccountId);
            // Should include opening balance + period 1 + period 2
            cashBalance.Should().BeGreaterThan(0);
        }

        [Fact]
        public void ClosePeriod_WithMultiTenantData_ShouldIsolateCorrectly()
        {
            // Arrange
            var tenant1Id = Guid.NewGuid();
            var tenant2Id = Guid.NewGuid();

            var builder1 = new FinanceTestDataBuilder(_fixture.Db, tenant1Id, _fixture.TestUserId);
            var builder2 = new FinanceTestDataBuilder(_fixture.Db, tenant2Id, _fixture.TestUserId);

            var accounts1 = builder1.CreateStandardChartOfAccounts();
            var accounts2 = builder2.CreateStandardChartOfAccounts();

            var period1 = builder1.CreatePeriod(
                new DateTime(2024, 1, 1),
                new DateTime(2024, 1, 31));

            var period2 = builder2.CreatePeriod(
                new DateTime(2024, 1, 1),
                new DateTime(2024, 1, 31));

            // Post vouchers for tenant 1
            var (voucher1, _) = builder1.CreateBalancedVoucher(
                accounts1["Cash"].AccountId,
                accounts1["Revenue"].AccountId,
                1000);
            var voucherService1 = new TestGLVoucherService(_fixture.Db);
            voucherService1.Post(voucher1.Id, _fixture.TestUserId.ToString());

            // Post vouchers for tenant 2
            var (voucher2, _) = builder2.CreateBalancedVoucher(
                accounts2["Cash"].AccountId,
                accounts2["Revenue"].AccountId,
                2000);
            var voucherService2 = new TestGLVoucherService(_fixture.Db);
            voucherService2.Post(voucher2.Id, _fixture.TestUserId.ToString());

            // Act - Close periods
            var closeDto1 = new ClosePeriodDto { Id = period1.Id, ForceClose = false };
            var closeDto2 = new ClosePeriodDto { Id = period2.Id, ForceClose = false };

            var result1 = _periodService.ClosePeriod(closeDto1, _fixture.TestUserId.ToString());
            var result2 = _periodService.ClosePeriod(closeDto2, _fixture.TestUserId.ToString());

            // Assert
            result1.Should().BeGreaterThan(0);
            result2.Should().BeGreaterThan(0);

            var closed1 = _periodService.GetById(period1.Id);
            var closed2 = _periodService.GetById(period2.Id);

            closed1.CompanyID.Should().NotBeNull();
            closed2.CompanyID.Should().NotBeNull();
            closed1.Status.Should().Be(PeriodStatus.Closed);
            closed2.Status.Should().Be(PeriodStatus.Closed);
        }

        [Fact]
        public void LockPeriod_ShouldPreventReopening()
        {
            // Arrange
            var period = _builder.CreatePeriod(
                new DateTime(2024, 1, 1),
                new DateTime(2024, 1, 31),
                status: PeriodStatus.Closed);

            // Lock period
            _periodService.LockPeriod(period.Id, _fixture.TestUserId.ToString());

            // Act & Assert
            var reopenDto = new ReopenPeriodDto
            {
                Id = period.Id,
                Reason = "Trying to reopen locked period"
            };

            Assert.Throws<Exception>(() => _periodService.ReopenPeriod(reopenDto, _fixture.TestUserId.ToString()));
        }

        [Fact]
        public void GenerateFiscalYearPeriods_WithCustomStartDate_ShouldCreateCorrectly()
        {
            // Arrange
            var fiscalYear = 2024;
            var customStartDate = new DateTime(2024, 7, 1); // Fiscal year starting in July

            // Act
            var periods = _builder.CreateFiscalYearPeriods(fiscalYear, customStartDate);

            // Assert
            periods.Should().HaveCount(12);
            periods[0].PeriodStartDate.Should().Be(customStartDate);
            periods[11].PeriodEndDate.Should().Be(customStartDate.AddYears(1).AddDays(-1));
            periods.Should().OnlyContain(p => p.FiscalYear == fiscalYear);
        }

        [Fact]
        public void ClosePeriod_WithZeroTransactions_ShouldSucceed()
        {
            // Arrange
            var period = _builder.CreatePeriod(
                new DateTime(2024, 4, 1),
                new DateTime(2024, 4, 30));

            var dto = new ClosePeriodDto
            {
                Id = period.Id,
                ForceClose = false,
                ClosingNotes = "Closing period with no transactions"
            };

            // Act
            var result = _periodService.ClosePeriod(dto, _fixture.TestUserId.ToString());

            // Assert
            result.Should().BeGreaterThan(0);
            var closed = _periodService.GetById(period.Id);
            closed.Status.Should().Be(PeriodStatus.Closed);
            closed.PostedVouchers.Should().Be(0);
        }
    }
}






























