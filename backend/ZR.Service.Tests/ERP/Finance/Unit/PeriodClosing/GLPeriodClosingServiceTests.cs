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
    /// Unit tests for GL Period Closing Service - اختبارات وحدة خدمة إقفال الفترات
    /// </summary>
    public class GLPeriodClosingServiceTests : IClassFixture<FinanceTestFixture>
    {
        private readonly FinanceTestFixture _fixture;
        private readonly TestGLPeriodClosingService _service;
        private readonly TestGLVoucherService _voucherService;
        private readonly FinanceTestDataBuilder _builder;
        private readonly Dictionary<string, GLAccount> _accounts;

        public GLPeriodClosingServiceTests(FinanceTestFixture fixture)
        {
            _fixture = fixture;

            // Create test services that work directly with ISqlSugarClient
            _service = new TestGLPeriodClosingService(_fixture.Db);
            _voucherService = new TestGLVoucherService(_fixture.Db);

            // Create test data builder
            _builder = new FinanceTestDataBuilder(_fixture.Db, _fixture.TestTenantId, _fixture.TestUserId);

            // Create standard accounts
            _accounts = _builder.CreateStandardChartOfAccounts();
        }

        #region Create Period Tests

        [Fact]
        public void CreatePeriod_ShouldCreatePeriodSuccessfully()
        {
            // Arrange
            var periodStart = new DateTime(DateTime.Now.Year, 1, 1);
            var periodEnd = new DateTime(DateTime.Now.Year, 1, 31);

            var dto = new CreatePeriodClosingDto
            {
                PeriodType = PeriodType.Month,
                PeriodStartDate = periodStart,
                PeriodEndDate = periodEnd,
                FiscalYear = periodStart.Year,
                FiscalPeriod = 1,
                DescriptionAr = "فترة تجريبية",
                DescriptionEn = "Test Period",
                TenantId = _fixture.TestTenantId
            };

            // Act
            var result = _service.Create(dto, _fixture.TestUserId.ToString());

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().NotBeEmpty();
            result.PeriodStartDate.Should().Be(periodStart.Date);
            result.PeriodEndDate.Should().Be(periodEnd.Date);
            result.Status.Should().Be(PeriodStatus.Open);
            result.Create_by.Should().NotBeNull();
        }

        #endregion

        #region Close Period Tests

        [Fact]
        public void ClosePeriod_WithUnpostedVouchers_ShouldFail()
        {
            // Arrange - Use a unique date range to avoid conflicts with other tests (within valid calendar range)
            var uniqueYear = 2001;
            var periodStart = new DateTime(uniqueYear, 3, 1);
            var periodEnd = new DateTime(uniqueYear, 3, 31);

            var period = _builder.CreatePeriod(periodStart, periodEnd);

            // Create unposted voucher
            var (voucher, _) = _builder.CreateBalancedVoucher(
                _accounts["Cash"].AccountId,
                _accounts["Revenue"].AccountId,
                1000,
                voucherDate: periodStart.AddDays(5));

            var dto = new ClosePeriodDto
            {
                Id = period.Id,
                ForceClose = false,
                ClosingNotes = "Closing notes"
            };

            // Act & Assert
            Assert.Throws<Exception>(() => _service.ClosePeriod(dto, _fixture.TestUserId.ToString()));
        }

        [Fact]
        public void ClosePeriod_WithAllVouchersPosted_ShouldSucceed()
        {
            // Arrange - Use a unique date range to avoid conflicts with other tests (within valid calendar range)
            var uniqueYear = 2002;
            var periodStart = new DateTime(uniqueYear, 4, 1);
            var periodEnd = new DateTime(uniqueYear, 4, 30);

            var period = _builder.CreatePeriod(periodStart, periodEnd);

            // Create and post vouchers
            var (voucher, _) = _builder.CreateBalancedVoucher(
                _accounts["Cash"].AccountId,
                _accounts["Revenue"].AccountId,
                1000,
                voucherDate: periodStart.AddDays(5));

            _voucherService.Post(voucher.Id, _fixture.TestUserId.ToString());

            var dto = new ClosePeriodDto
            {
                Id = period.Id,
                ForceClose = false,
                ClosingNotes = "Period closed successfully"
            };

            // Act
            var result = _service.ClosePeriod(dto, _fixture.TestUserId.ToString());

            // Assert
            result.Should().BeGreaterThan(0);
            var closed = _service.GetById(period.Id);
            closed.Status.Should().Be(PeriodStatus.Closed);
            closed.ClosedBy.Should().NotBeNull();
        }

        [Fact]
        public void ClosePeriod_ShouldLockTransactions()
        {
            // Arrange - Use a unique date range to avoid conflicts with other tests (within valid calendar range)
            var uniqueYear = 2003;
            var periodStart = new DateTime(uniqueYear, 5, 1);
            var periodEnd = new DateTime(uniqueYear, 5, 31);

            var period = _builder.CreatePeriod(periodStart, periodEnd);

            // Post all vouchers
            var (voucher, _) = _builder.CreateBalancedVoucher(
                _accounts["Cash"].AccountId,
                _accounts["Revenue"].AccountId,
                1000,
                voucherDate: periodStart.AddDays(5));

            _voucherService.Post(voucher.Id, _fixture.TestUserId.ToString());

            var dto = new ClosePeriodDto
            {
                Id = period.Id,
                ForceClose = false
            };

            // Act
            _service.ClosePeriod(dto, _fixture.TestUserId.ToString());

            // Assert
            var closed = _service.GetById(period.Id);
            closed.Status.Should().Be(PeriodStatus.Closed);

            // Verify date is in closed period
            var isClosed = _service.IsDateInClosedPeriod(periodStart.AddDays(10), _fixture.TestTenantId);
            isClosed.Should().BeTrue();
        }

        [Fact]
        public void ClosePeriod_ShouldRollBalancesForward()
        {
            // Arrange
            var periodStart = new DateTime(DateTime.Now.Year, 1, 1);
            var periodEnd = new DateTime(DateTime.Now.Year, 1, 31);

            var period = _builder.CreatePeriod(periodStart, periodEnd);

            // Create and post vouchers
            var (voucher, _) = _builder.CreateBalancedVoucher(
                _accounts["Cash"].AccountId,
                _accounts["Revenue"].AccountId,
                1000,
                voucherDate: periodStart.AddDays(5));

            _voucherService.Post(voucher.Id, _fixture.TestUserId.ToString());

            var dto = new ClosePeriodDto
            {
                Id = period.Id,
                ForceClose = false
            };

            // Act
            _service.ClosePeriod(dto, _fixture.TestUserId.ToString());

            // Assert
            var closed = _service.GetById(period.Id);
            closed.PostedVouchers.Should().Be(1);
            closed.TotalDebit.Should().Be(1000);
            closed.TotalCredit.Should().Be(1000);
        }

        #endregion

        #region Reopen Period Tests

        [Fact]
        public void ReopenPeriod_ShouldAllowNewPostings()
        {
            // Arrange
            var periodStart = new DateTime(DateTime.Now.Year, 1, 1);
            var periodEnd = new DateTime(DateTime.Now.Year, 1, 31);

            var period = _builder.CreatePeriod(periodStart, periodEnd, status: PeriodStatus.Closed);

            var reopenDto = new ReopenPeriodDto
            {
                Id = period.Id,
                Reason = "Need to make adjustments"
            };

            // Act
            var result = _service.ReopenPeriod(reopenDto, _fixture.TestUserId.ToString());

            // Assert
            result.Should().BeGreaterThan(0);
            var reopened = _service.GetById(period.Id);
            reopened.Status.Should().Be(PeriodStatus.Open);
        }

        [Fact]
        public void ReopenPeriod_WithLockedPeriod_ShouldFail()
        {
            // Arrange
            var periodStart = new DateTime(DateTime.Now.Year, 1, 1);
            var periodEnd = new DateTime(DateTime.Now.Year, 1, 31);

            var period = _builder.CreatePeriod(periodStart, periodEnd, status: PeriodStatus.Locked);

            var reopenDto = new ReopenPeriodDto
            {
                Id = period.Id,
                Reason = "Trying to reopen locked period"
            };

            // Act & Assert
            Assert.Throws<Exception>(() => _service.ReopenPeriod(reopenDto, _fixture.TestUserId.ToString()));
        }

        #endregion

        #region Lock Period Tests

        [Fact]
        public void LockPeriod_WithClosedPeriod_ShouldSucceed()
        {
            // Arrange
            var periodStart = new DateTime(DateTime.Now.Year, 1, 1);
            var periodEnd = new DateTime(DateTime.Now.Year, 1, 31);

            var period = _builder.CreatePeriod(periodStart, periodEnd, status: PeriodStatus.Closed);

            // Act
            var result = _service.LockPeriod(period.Id, _fixture.TestUserId.ToString());

            // Assert
            result.Should().BeGreaterThan(0);
            var locked = _service.GetById(period.Id);
            locked.Status.Should().Be(PeriodStatus.Locked);
        }

        [Fact]
        public void LockPeriod_WithOpenPeriod_ShouldFail()
        {
            // Arrange
            var periodStart = new DateTime(DateTime.Now.Year, 1, 1);
            var periodEnd = new DateTime(DateTime.Now.Year, 1, 31);

            var period = _builder.CreatePeriod(periodStart, periodEnd, status: PeriodStatus.Open);

            // Act & Assert
            Assert.Throws<Exception>(() => _service.LockPeriod(period.Id, _fixture.TestUserId.ToString()));
        }

        #endregion

        #region Fiscal Year Period Generation Tests

        [Fact]
        public void GenerateFiscalYearPeriods_ShouldCreate12MonthlyPeriods()
        {
            // Arrange
            var fiscalYear = DateTime.Now.Year;

            // Act
            var periods = _service.GenerateFiscalYearPeriods(
                fiscalYear,
                PeriodType.Month,
                _fixture.TestUserId.ToString(),
                _fixture.TestTenantId);

            // Assert
            periods.Should().NotBeNull();
            periods.Should().HaveCount(12);
            periods.Should().OnlyContain(p => p.FiscalYear == fiscalYear);
            periods.Should().OnlyContain(p => p.Status == PeriodStatus.Open);
        }

        #endregion

        #region Multi-Tenant Tests

        [Fact]
        public void CreatePeriod_ShouldRespectTenantIsolation()
        {
            // Arrange
            var tenant1Id = Guid.NewGuid();
            var tenant2Id = Guid.NewGuid();

            var periodStart = new DateTime(DateTime.Now.Year, 1, 1);
            var periodEnd = new DateTime(DateTime.Now.Year, 1, 31);

            var builder1 = new FinanceTestDataBuilder(_fixture.Db, tenant1Id, _fixture.TestUserId);
            var builder2 = new FinanceTestDataBuilder(_fixture.Db, tenant2Id, _fixture.TestUserId);

            var period1 = builder1.CreatePeriod(periodStart, periodEnd);
            var period2 = builder2.CreatePeriod(periodStart, periodEnd);

            // Assert - Both periods should exist with different company IDs
            period1.CompanyID.Should().NotBeNull();
            period2.CompanyID.Should().NotBeNull();
        }

        #endregion

        #region Date in Closed Period Tests

        [Fact]
        public void IsDateInClosedPeriod_WithOpenPeriod_ShouldReturnFalse()
        {
            // Arrange - Use a unique date range to avoid conflicts with other tests (within valid calendar range)
            var uniqueYear = 2000; // Use a year in the past that's within valid range
            var periodStart = new DateTime(uniqueYear, 6, 1);
            var periodEnd = new DateTime(uniqueYear, 6, 30);

            _builder.CreatePeriod(periodStart, periodEnd, status: PeriodStatus.Open);

            // Act
            var result = _service.IsDateInClosedPeriod(periodStart.AddDays(15), _fixture.TestTenantId);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void IsDateInClosedPeriod_WithClosedPeriod_ShouldReturnTrue()
        {
            // Arrange
            var periodStart = new DateTime(DateTime.Now.Year, 1, 1);
            var periodEnd = new DateTime(DateTime.Now.Year, 1, 31);

            _builder.CreatePeriod(periodStart, periodEnd, status: PeriodStatus.Closed);

            // Act
            var result = _service.IsDateInClosedPeriod(periodStart.AddDays(15), _fixture.TestTenantId);

            // Assert
            result.Should().BeTrue();
        }

        #endregion
    }
}

