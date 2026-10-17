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
    /// Unit tests for Advanced Voucher Scenarios - اختبارات السيناريوهات المتقدمة للسندات
    /// </summary>
    public class GLVoucherAdvancedScenariosTests : IClassFixture<FinanceTestFixture>
    {
        private readonly FinanceTestFixture _fixture;
        private readonly TestGLVoucherService _service;
        private readonly FinanceTestDataBuilder _builder;
        private readonly Dictionary<string, GLAccount> _accounts;

        public GLVoucherAdvancedScenariosTests(FinanceTestFixture fixture)
        {
            _fixture = fixture;
            _service = new TestGLVoucherService(_fixture.Db);
            _builder = new FinanceTestDataBuilder(_fixture.Db, _fixture.TestTenantId, _fixture.TestUserId);
            _accounts = _builder.CreateStandardChartOfAccounts();
        }

        [Fact]
        public void PostVoucher_WithPartiallyBalancedEntries_ShouldFail()
        {
            // Arrange
            var voucher = _builder.CreateVoucher();
            _builder.CreateVoucherEntry(voucher.Id, _accounts["Cash"].AccountId, debit: 1000, lineNumber: 1);
            _builder.CreateVoucherEntry(voucher.Id, _accounts["Revenue"].AccountId, credit: 500, lineNumber: 2); // Unbalanced

            // Act & Assert
            Assert.Throws<Exception>(() => _service.Post(voucher.Id, _fixture.TestUserId.ToString()));
        }

        [Fact]
        public void CreateVoucher_WithMultiLineEntriesAcrossCurrencies_ShouldConvertAllToBase()
        {
            // Arrange
            var usdRate = 3.75m;
            var eurRate = 4.10m;
            
            var amountUSD = 1000m;
            var amountEUR = 500m;
            
            var baseUSD = amountUSD * usdRate;
            var baseEUR = amountEUR * eurRate;
            var totalDebit = baseUSD + baseEUR;
            var totalCredit = totalDebit; // Balanced

            var dto = new CreateVoucherDto
            {
                VoucherDate = DateTime.Now,
                Currency = "SAR",
                TenantId = _fixture.TestTenantId,
                Entries = new List<CreateVoucherEntryDto>
                {
                    new CreateVoucherEntryDto
                    {
                        AccountId = _accounts["Cash"].AccountId,
                        Debit = amountUSD,
                        Credit = 0,
                        ExchangeRate = usdRate,
                        DescriptionEn = "USD Entry"
                    },
                    new CreateVoucherEntryDto
                    {
                        AccountId = _accounts["Bank"].AccountId,
                        Debit = amountEUR,
                        Credit = 0,
                        ExchangeRate = eurRate,
                        DescriptionEn = "EUR Entry"
                    },
                    new CreateVoucherEntryDto
                    {
                        AccountId = _accounts["Revenue"].AccountId,
                        Debit = 0,
                        Credit = totalCredit,
                        ExchangeRate = 1.0m,
                        DescriptionEn = "Revenue Entry"
                    }
                }
            };

            // Act
            var voucher = _service.Create(dto, _fixture.TestUserId.ToString());

            // Assert
            voucher.TotalDebit.Should().BeApproximately(totalDebit, 0.01m);
            voucher.TotalCredit.Should().BeApproximately(totalCredit, 0.01m);
        }

        [Fact]
        public void ReverseVoucher_ThatIsAlreadyReversed_ShouldFail()
        {
            // Arrange
            var (voucher, _) = _builder.CreateBalancedVoucher(
                _accounts["Cash"].AccountId,
                _accounts["Revenue"].AccountId,
                1000);

            _service.Post(voucher.Id, _fixture.TestUserId.ToString());
            _service.Reverse(voucher.Id, _fixture.TestUserId.ToString());

            // Act
            var result = _service.Reverse(voucher.Id, _fixture.TestUserId.ToString());

            // Assert
            result.Should().Be(0); // Should fail - already reversed
        }

        [Fact]
        public void PostVoucher_WithFutureDate_ShouldSucceed()
        {
            // Arrange
            var futureDate = DateTime.Now.AddDays(30);
            var dto = new CreateVoucherDto
            {
                VoucherDate = futureDate,
                DescriptionAr = "سند بتاريخ مستقبلي",
                DescriptionEn = "Future Date Voucher",
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
            var voucher = _service.Create(dto, _fixture.TestUserId.ToString());
            var result = _service.Post(voucher.Id, _fixture.TestUserId.ToString());

            // Assert
            result.Should().BeGreaterThan(0);
            var posted = _service.GetById(voucher.Id);
            posted.Status.Should().Be(VoucherStatus.Posted);
            posted.VoucherDate.Date.Should().Be(futureDate.Date);
            // Note: Production should validate future dates based on business rules
        }

        [Fact]
        public void PostVoucher_AcrossFiscalPeriods_ShouldSucceed()
        {
            // Arrange
            var period1 = _builder.CreatePeriod(
                new DateTime(2024, 1, 1),
                new DateTime(2024, 1, 31),
                PeriodType.Month,
                PeriodStatus.Open,
                2024,
                1);

            var period2 = _builder.CreatePeriod(
                new DateTime(2024, 2, 1),
                new DateTime(2024, 2, 29),
                PeriodType.Month,
                PeriodStatus.Open,
                2024,
                2);

            // Create voucher on last day of period 1
            var voucherDate = new DateTime(2024, 1, 31);
            var dto = new CreateVoucherDto
            {
                VoucherDate = voucherDate,
                FiscalYear = 2024,
                FiscalPeriod = 1,
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
            var voucher = _service.Create(dto, _fixture.TestUserId.ToString());
            var result = _service.Post(voucher.Id, _fixture.TestUserId.ToString());

            // Assert
            result.Should().BeGreaterThan(0);
            var posted = _service.GetById(voucher.Id);
            posted.FiscalYear.Should().Be(2024);
            posted.FiscalPeriod.Should().Be(1);
        }

        [Fact]
        public void CreateVoucher_WithZeroAmount_ShouldSucceed()
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
                        Debit = 0,
                        Credit = 0,
                        ExchangeRate = 1
                    },
                    new CreateVoucherEntryDto
                    {
                        AccountId = _accounts["Revenue"].AccountId,
                        Debit = 0,
                        Credit = 0,
                        ExchangeRate = 1
                    }
                }
            };

            // Act
            var voucher = _service.Create(dto, _fixture.TestUserId.ToString());

            // Assert
            voucher.Should().NotBeNull();
            voucher.TotalDebit.Should().Be(0);
            voucher.TotalCredit.Should().Be(0);
            // Note: Production should validate zero-amount vouchers
        }

        [Fact]
        public void CreateVoucher_WithVeryLargeAmount_ShouldSucceed()
        {
            // Arrange
            var veryLargeAmount = 999999999999.99m;
            var dto = new CreateVoucherDto
            {
                VoucherDate = DateTime.Now,
                TenantId = _fixture.TestTenantId,
                Entries = new List<CreateVoucherEntryDto>
                {
                    new CreateVoucherEntryDto
                    {
                        AccountId = _accounts["Cash"].AccountId,
                        Debit = veryLargeAmount,
                        Credit = 0,
                        ExchangeRate = 1
                    },
                    new CreateVoucherEntryDto
                    {
                        AccountId = _accounts["Revenue"].AccountId,
                        Debit = 0,
                        Credit = veryLargeAmount,
                        ExchangeRate = 1
                    }
                }
            };

            // Act
            var voucher = _service.Create(dto, _fixture.TestUserId.ToString());

            // Assert
            voucher.Should().NotBeNull();
            voucher.TotalDebit.Should().Be(veryLargeAmount);
            voucher.TotalCredit.Should().Be(veryLargeAmount);
        }

        [Fact]
        public void CreateVoucher_WithManyEntries_ShouldSucceed()
        {
            // Arrange
            var voucher = _builder.CreateVoucher();
            var entries = new List<GLVoucherEntry>();
            var totalDebit = 0m;
            var totalCredit = 0m;

            // Create 20 debit entries
            for (int i = 0; i < 20; i++)
            {
                var account = _builder.CreateAccount($"1{i:D3}", $"حساب {i}", $"Account {i}", AccountType.Asset);
                var entry = _builder.CreateVoucherEntry(voucher.Id, account.AccountId, debit: 100, lineNumber: i + 1);
                entries.Add(entry);
                totalDebit += 100;
            }

            // Create 1 credit entry to balance
            var revenueAccount = _accounts["Revenue"];
            var creditEntry = _builder.CreateVoucherEntry(voucher.Id, revenueAccount.AccountId, credit: totalDebit, lineNumber: 21);
            entries.Add(creditEntry);
            totalCredit = totalDebit;

            // Update voucher totals
            voucher.TotalDebit = totalDebit;
            voucher.TotalCredit = totalCredit;
            _fixture.Db.Updateable(voucher).ExecuteCommand();

            // Act
            var validation = _service.ValidateDoubleEntry(voucher.Id);

            // Assert
            validation.IsValid.Should().BeTrue();
            voucher.TotalDebit.Should().Be(totalDebit);
            voucher.TotalCredit.Should().Be(totalCredit);
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

            var updateDto = new UpdateVoucherDto
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
            var result = _service.Update(updateDto, _fixture.TestUserId.ToString());

            // Assert
            result.Should().Be(0); // Should fail - posted voucher cannot be updated
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
    }
}


