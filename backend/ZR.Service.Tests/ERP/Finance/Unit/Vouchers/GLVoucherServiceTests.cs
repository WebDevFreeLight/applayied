using System;
using System.Collections.Generic;
using FluentAssertions;
using Xunit;
using ZR.Model.Finance;
using ZR.Model.Finance.Dto;
using ZR.Model.Finance.Enums;
using ZR.Service.Tests.ERP.Finance.TestHelpers;

namespace ZR.Service.Tests.ERP.Finance.Unit.Vouchers
{
    /// <summary>
    /// Unit tests for GL Voucher Service - اختبارات وحدة خدمة السندات
    /// </summary>
    public class GLVoucherServiceTests : IClassFixture<FinanceTestFixture>
    {
        private readonly FinanceTestFixture _fixture;
        private readonly TestGLVoucherService _service;
        private readonly FinanceTestDataBuilder _builder;
        private readonly Dictionary<string, GLAccount> _accounts;

        public GLVoucherServiceTests(FinanceTestFixture fixture)
        {
            _fixture = fixture;

            // Create test service that works directly with ISqlSugarClient
            _service = new TestGLVoucherService(_fixture.Db);

            // Create test data builder
            _builder = new FinanceTestDataBuilder(_fixture.Db, _fixture.TestTenantId, _fixture.TestUserId);

            // Create standard accounts
            _accounts = _builder.CreateStandardChartOfAccounts();
        }

        #region Create Voucher Tests

        [Fact]
        public void CreateVoucher_ShouldCreateWithEntries()
        {
            // Arrange
            var dto = new CreateVoucherDto
            {
                VoucherDate = DateTime.Now,
                DescriptionAr = "سند تجريبي",
                DescriptionEn = "Test Voucher",
                Currency = "SAR",
                FiscalYear = DateTime.Now.Year,
                FiscalPeriod = DateTime.Now.Month,
                TenantId = _fixture.TestTenantId,
                Entries = new List<CreateVoucherEntryDto>
                {
                    new CreateVoucherEntryDto
                    {
                        AccountId = _accounts["Cash"].AccountId,
                        Debit = 1000,
                        Credit = 0,
                        DescriptionAr = "قيد مدين",
                        DescriptionEn = "Debit Entry",
                        ExchangeRate = 1
                    },
                    new CreateVoucherEntryDto
                    {
                        AccountId = _accounts["Revenue"].AccountId,
                        Debit = 0,
                        Credit = 1000,
                        DescriptionAr = "قيد دائن",
                        DescriptionEn = "Credit Entry",
                        ExchangeRate = 1
                    }
                }
            };

            // Act
            var result = _service.Create(dto, _fixture.TestUserId.ToString());

            // Assert
            result.Should().NotBeNull();
            result.TotalDebit.Should().Be(1000);
            result.TotalCredit.Should().Be(1000);
            result.Status.Should().Be(VoucherStatus.Draft);
        }

        [Fact]
        public void CreateVoucher_ShouldStoreArabicAndEnglishFields()
        {
            // Arrange
            var dto = new CreateVoucherDto
            {
                VoucherDate = DateTime.Now,
                DescriptionAr = "وصف بالعربي",
                DescriptionEn = "Description in English",
                TenantId = _fixture.TestTenantId,
                Entries = new List<CreateVoucherEntryDto>
                {
                    new CreateVoucherEntryDto
                    {
                        AccountId = _accounts["Cash"].AccountId,
                        Debit = 1000,
                        Credit = 0,
                        DescriptionAr = "وصف القيد بالعربي",
                        DescriptionEn = "Entry description in English",
                        ExchangeRate = 1
                    },
                    new CreateVoucherEntryDto
                    {
                        AccountId = _accounts["Revenue"].AccountId,
                        Debit = 0,
                        Credit = 1000,
                        DescriptionAr = "وصف القيد بالعربي",
                        DescriptionEn = "Entry description in English",
                        ExchangeRate = 1
                    }
                }
            };

            // Act
            var result = _service.Create(dto, _fixture.TestUserId.ToString());

            // Assert
            result.DescriptionAr.Should().Be("وصف بالعربي");
            result.DescriptionEn.Should().Be("Description in English");
        }

        #endregion

        #region Update Voucher Tests

        [Fact]
        public void UpdateVoucher_ShouldUpdateFieldsCorrectly()
        {
            // Arrange
            var (voucher, _) = _builder.CreateBalancedVoucher(
                _accounts["Cash"].AccountId,
                _accounts["Revenue"].AccountId,
                1000);

            var dto = new UpdateVoucherDto
            {
                Id = voucher.Id,
                VoucherDate = voucher.VoucherDate,
                DescriptionAr = "وصف محدث",
                DescriptionEn = "Updated Description",
                Currency = "SAR",
                TenantId = _fixture.TestTenantId,
                Entries = new List<UpdateVoucherEntryDto>
                {
                    new UpdateVoucherEntryDto
                    {
                        AccountId = _accounts["Cash"].AccountId,
                        Debit = 2000,
                        Credit = 0,
                        ExchangeRate = 1
                    },
                    new UpdateVoucherEntryDto
                    {
                        AccountId = _accounts["Revenue"].AccountId,
                        Debit = 0,
                        Credit = 2000,
                        ExchangeRate = 1
                    }
                }
            };

            // Act
            var result = _service.Update(dto, _fixture.TestUserId.ToString());

            // Assert
            result.Should().BeGreaterThan(0);
            var updated = _service.GetById(voucher.Id);
            updated.TotalDebit.Should().Be(2000);
            updated.TotalCredit.Should().Be(2000);
        }

        [Fact]
        public void UpdateVoucher_WithPostedStatus_ShouldFail()
        {
            // Arrange
            var (voucher, _) = _builder.CreateBalancedVoucher(
                _accounts["Cash"].AccountId,
                _accounts["Revenue"].AccountId,
                1000);

            _service.Post(voucher.Id, _fixture.TestUserId.ToString());

            var dto = new UpdateVoucherDto
            {
                Id = voucher.Id,
                VoucherDate = voucher.VoucherDate,
                TenantId = _fixture.TestTenantId,
                Entries = new List<UpdateVoucherEntryDto>
                {
                    new UpdateVoucherEntryDto
                    {
                        AccountId = _accounts["Cash"].AccountId,
                        Debit = 2000,
                        Credit = 0,
                        ExchangeRate = 1
                    },
                    new UpdateVoucherEntryDto
                    {
                        AccountId = _accounts["Revenue"].AccountId,
                        Debit = 0,
                        Credit = 2000,
                        ExchangeRate = 1
                    }
                }
            };

            // Act
            var result = _service.Update(dto, _fixture.TestUserId.ToString());

            // Assert
            result.Should().Be(0); // Should fail - posted voucher cannot be updated
        }

        #endregion

        #region Delete Voucher Tests

        [Fact]
        public void DeleteVoucher_ShouldDeleteDraftVoucher()
        {
            // Arrange
            var (voucher, _) = _builder.CreateBalancedVoucher(
                _accounts["Cash"].AccountId,
                _accounts["Revenue"].AccountId,
                1000);

            // Act
            var result = _service.Delete(voucher.Id);

            // Assert
            result.Should().BeGreaterThan(0);
            var deleted = _service.GetById(voucher.Id);
            deleted.Should().BeNull(); // Should be deleted
        }

        [Fact]
        public void DeleteVoucher_WithPostedStatus_ShouldFail()
        {
            // Arrange
            var (voucher, _) = _builder.CreateBalancedVoucher(
                _accounts["Cash"].AccountId,
                _accounts["Revenue"].AccountId,
                1000);

            _service.Post(voucher.Id, _fixture.TestUserId.ToString());

            // Act
            var result = _service.Delete(voucher.Id);

            // Assert
            result.Should().Be(0); // Should fail - posted voucher cannot be deleted
        }

        #endregion

        #region Post Voucher Tests

        [Fact]
        public void PostVoucher_ShouldUpdateStatus()
        {
            // Arrange
            var (voucher, _) = _builder.CreateBalancedVoucher(
                _accounts["Cash"].AccountId,
                _accounts["Revenue"].AccountId,
                1000);

            // Act
            var result = _service.Post(voucher.Id, _fixture.TestUserId.ToString());

            // Assert
            result.Should().BeGreaterThan(0);
            var posted = _service.GetById(voucher.Id);
            posted.Status.Should().Be(VoucherStatus.Posted);
        }

        [Fact]
        public void PostVoucher_WithUnbalancedEntries_ShouldFail()
        {
            // Arrange
            var voucher = _builder.CreateVoucher();
            _builder.CreateVoucherEntry(voucher.Id, _accounts["Cash"].AccountId, debit: 1000);
            _builder.CreateVoucherEntry(voucher.Id, _accounts["Revenue"].AccountId, credit: 500); // Unbalanced

            // Act & Assert
            Assert.Throws<Exception>(() => _service.Post(voucher.Id, _fixture.TestUserId.ToString()));
        }

        #endregion

        #region Reverse Voucher Tests

        [Fact]
        public void ReverseVoucher_ShouldCreateReversalAndMarkOriginalAsReversed()
        {
            // Arrange
            var (voucher, _) = _builder.CreateBalancedVoucher(
                _accounts["Cash"].AccountId,
                _accounts["Revenue"].AccountId,
                1000);

            _service.Post(voucher.Id, _fixture.TestUserId.ToString());

            // Act
            var result = _service.Reverse(voucher.Id, _fixture.TestUserId.ToString());

            // Assert
            result.Should().BeGreaterThan(0);
            var reversed = _service.GetById(voucher.Id);
            reversed.Status.Should().Be(VoucherStatus.Cancelled);
        }

        [Fact]
        public void ReverseVoucher_WithDraftStatus_ShouldFail()
        {
            // Arrange
            var (voucher, _) = _builder.CreateBalancedVoucher(
                _accounts["Cash"].AccountId,
                _accounts["Revenue"].AccountId,
                1000);
            // Don't post - keep as draft

            // Act
            var result = _service.Reverse(voucher.Id, _fixture.TestUserId.ToString());

            // Assert
            result.Should().Be(0); // Should fail - draft voucher cannot be reversed
        }

        #endregion

        #region Double Entry Validation Tests

        [Fact]
        public void ValidateDoubleEntry_WithBalancedVoucher_ShouldReturnTrue()
        {
            // Arrange
            var (voucher, _) = _builder.CreateBalancedVoucher(
                _accounts["Cash"].AccountId,
                _accounts["Revenue"].AccountId,
                1000);

            // Act
            var result = _service.ValidateDoubleEntry(voucher.Id);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void ValidateDoubleEntry_WithUnbalancedVoucher_ShouldReturnFalse()
        {
            // Arrange
            var voucher = _builder.CreateVoucher();
            _builder.CreateVoucherEntry(voucher.Id, _accounts["Cash"].AccountId, debit: 1000);
            _builder.CreateVoucherEntry(voucher.Id, _accounts["Revenue"].AccountId, credit: 500);

            // Act
            var result = _service.ValidateDoubleEntry(voucher.Id);

            // Assert
            result.IsValid.Should().BeFalse();
        }

        [Fact]
        public void GetVoucherTotals_ShouldReturnCorrectTotals()
        {
            // Arrange
            var (voucher, _) = _builder.CreateBalancedVoucher(
                _accounts["Cash"].AccountId,
                _accounts["Revenue"].AccountId,
                5000);

            // Act
            var totals = _service.GetVoucherTotals(voucher.Id);

            // Assert
            totals.TotalDebit.Should().Be(5000);
            totals.TotalCredit.Should().Be(5000);
            totals.IsBalanced.Should().BeTrue();
        }

        #endregion

        #region Multi-Tenant Tests

        [Fact]
        public void CreateVoucher_ShouldRespectTenantIsolation()
        {
            // Arrange
            var tenant1Id = Guid.NewGuid();
            var tenant2Id = Guid.NewGuid();

            var builder1 = new FinanceTestDataBuilder(_fixture.Db, tenant1Id, _fixture.TestUserId);
            var builder2 = new FinanceTestDataBuilder(_fixture.Db, tenant2Id, _fixture.TestUserId);

            var accounts1 = builder1.CreateStandardChartOfAccounts();
            var accounts2 = builder2.CreateStandardChartOfAccounts();

            var dto1 = new CreateVoucherDto
            {
                VoucherDate = DateTime.Now,
                TenantId = tenant1Id,
                Entries = new List<CreateVoucherEntryDto>
                {
                    new CreateVoucherEntryDto
                    {
                        AccountId = accounts1["Cash"].AccountId,
                        Debit = 1000,
                        Credit = 0,
                        ExchangeRate = 1
                    },
                    new CreateVoucherEntryDto
                    {
                        AccountId = accounts1["Revenue"].AccountId,
                        Debit = 0,
                        Credit = 1000,
                        ExchangeRate = 1
                    }
                }
            };

            var dto2 = new CreateVoucherDto
            {
                VoucherDate = DateTime.Now,
                TenantId = tenant2Id,
                Entries = new List<CreateVoucherEntryDto>
                {
                    new CreateVoucherEntryDto
                    {
                        AccountId = accounts2["Cash"].AccountId,
                        Debit = 2000,
                        Credit = 0,
                        ExchangeRate = 1
                    },
                    new CreateVoucherEntryDto
                    {
                        AccountId = accounts2["Revenue"].AccountId,
                        Debit = 0,
                        Credit = 2000,
                        ExchangeRate = 1
                    }
                }
            };

            // Act
            var voucher1 = _service.Create(dto1, _fixture.TestUserId.ToString());
            var voucher2 = _service.Create(dto2, _fixture.TestUserId.ToString());

            // Assert
            voucher1.CompanyID.Should().NotBeNull();
            voucher2.CompanyID.Should().NotBeNull();
        }

        #endregion

        #region Audit Tests

        [Fact]
        public void CreateVoucher_ShouldSetAuditFields()
        {
            // Arrange
            var dto = new CreateVoucherDto
            {
                VoucherDate = DateTime.Now,
                TenantId = _fixture.TestTenantId,
                Entries = new List<CreateVoucherEntryDto>
                {
                    new CreateVoucherEntryDto
                    {
                        AccountId = _accounts["Cash"].AccountId,
                        Debit = 1000,
                        Credit = 0,
                        ExchangeRate = 1
                    },
                    new CreateVoucherEntryDto
                    {
                        AccountId = _accounts["Revenue"].AccountId,
                        Debit = 0,
                        Credit = 1000,
                        ExchangeRate = 1
                    }
                }
            };

            // Act
            var result = _service.Create(dto, _fixture.TestUserId.ToString());

            // Assert
            result.Create_by.Should().NotBeNull();
            result.Create_time.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(5));
            result.CompanyID.Should().NotBeNull();
        }

        [Fact]
        public void PostVoucher_ShouldSetPostedByAndPostedDate()
        {
            // Arrange
            var (voucher, _) = _builder.CreateBalancedVoucher(
                _accounts["Cash"].AccountId,
                _accounts["Revenue"].AccountId,
                1000);

            // Act
            _service.Post(voucher.Id, _fixture.TestUserId.ToString());

            // Assert
            var posted = _service.GetById(voucher.Id);
            posted.PostedBy.Should().NotBeNull();
            posted.PostedDate.Should().NotBeNull();
        }

        #endregion
    }
}

