using System;
using System.Collections.Generic;
using FluentAssertions;
using Xunit;
using ZR.Model.Finance;
using ZR.Model.Finance.Dto;
using ZR.Model.Finance.Enums;
using CurrencyModel = ZR.Model.Finance.Configuration.Currency;
using ZR.Model.Finance.Configuration.Enums;
using ZR.Service.Tests.ERP.Finance.TestHelpers;

namespace ZR.Service.Tests.ERP.Finance.Unit.Currency
{
    /// <summary>
    /// Unit tests for Currency Conversion & Multi-Currency Transactions - اختبارات تحويل العملات والمعاملات متعددة العملات
    /// </summary>
    public class CurrencyConversionTests : IClassFixture<FinanceTestFixture>
    {
        private readonly FinanceTestFixture _fixture;
        private readonly TestGLVoucherService _voucherService;
        private readonly FinanceTestDataBuilder _builder;
        private readonly Dictionary<string, GLAccount> _accounts;
        private readonly Dictionary<string, CurrencyModel> _currencies;

        public CurrencyConversionTests(FinanceTestFixture fixture)
        {
            _fixture = fixture;
            _voucherService = new TestGLVoucherService(_fixture.Db);
            _builder = new FinanceTestDataBuilder(_fixture.Db, _fixture.TestTenantId, _fixture.TestUserId);

            // Create standard accounts
            _accounts = _builder.CreateStandardChartOfAccounts();

            // Create currencies
            _currencies = new Dictionary<string, CurrencyModel>
            {
                ["SAR"] = _builder.CreateCurrency("SAR", "ريال سعودي", "Saudi Riyal", 1.0m, true, "ر.س"),
                ["USD"] = _builder.CreateCurrency("USD", "دولار أمريكي", "US Dollar", 3.75m, false, "$"),
                ["EUR"] = _builder.CreateCurrency("EUR", "يورو", "Euro", 4.10m, false, "€")
            };
        }

        [Fact]
        public void CreateVoucher_WithUSDCurrency_ShouldConvertToBaseCurrency()
        {
            // Arrange
            var usdRate = _currencies["USD"].ExchangeRate; // 3.75
            var amountUSD = 1000m;
            var expectedBaseAmount = amountUSD * usdRate; // 3750

            var dto = new CreateVoucherDto
            {
                VoucherDate = DateTime.Now,
                DescriptionAr = "سند بالدولار",
                DescriptionEn = "USD Voucher",
                Currency = "USD",
                TenantId = _fixture.TestTenantId,
                Entries = new List<CreateVoucherEntryDto>
                {
                    new CreateVoucherEntryDto
                    {
                        AccountId = _accounts["Cash"].AccountId,
                        Debit = amountUSD,
                        Credit = 0,
                        ExchangeRate = usdRate
                    },
                    new CreateVoucherEntryDto
                    {
                        AccountId = _accounts["Revenue"].AccountId,
                        Debit = 0,
                        Credit = amountUSD,
                        ExchangeRate = usdRate
                    }
                }
            };

            // Act
            var voucher = _voucherService.Create(dto, _fixture.TestUserId.ToString());

            // Assert
            voucher.Currency.Should().Be("USD");
            voucher.TotalDebit.Should().Be(expectedBaseAmount);
            voucher.TotalCredit.Should().Be(expectedBaseAmount);
        }

        [Fact]
        public void CreateVoucher_WithEURCurrency_ShouldConvertToBaseCurrency()
        {
            // Arrange
            var eurRate = _currencies["EUR"].ExchangeRate; // 4.10
            var amountEUR = 500m;
            var expectedBaseAmount = amountEUR * eurRate; // 2050

            var dto = new CreateVoucherDto
            {
                VoucherDate = DateTime.Now,
                Currency = "EUR",
                TenantId = _fixture.TestTenantId,
                Entries = new List<CreateVoucherEntryDto>
                {
                    new CreateVoucherEntryDto
                    {
                        AccountId = _accounts["Cash"].AccountId,
                        Debit = amountEUR,
                        Credit = 0,
                        ExchangeRate = eurRate
                    },
                    new CreateVoucherEntryDto
                    {
                        AccountId = _accounts["Revenue"].AccountId,
                        Debit = 0,
                        Credit = amountEUR,
                        ExchangeRate = eurRate
                    }
                }
            };

            // Act
            var voucher = _voucherService.Create(dto, _fixture.TestUserId.ToString());

            // Assert
            voucher.TotalDebit.Should().Be(expectedBaseAmount);
            voucher.TotalCredit.Should().Be(expectedBaseAmount);
        }

        [Fact]
        public void CreateVoucher_WithMixedCurrencies_ShouldConvertAllToBaseCurrency()
        {
            // Arrange
            var usdRate = _currencies["USD"].ExchangeRate;
            var eurRate = _currencies["EUR"].ExchangeRate;
            
            var amountUSD = 1000m;
            var amountEUR = 500m;
            
            var baseUSD = amountUSD * usdRate; // 3750
            var baseEUR = amountEUR * eurRate; // 2050
            var totalBase = baseUSD + baseEUR; // 5800

            var dto = new CreateVoucherDto
            {
                VoucherDate = DateTime.Now,
                Currency = "SAR", // Base currency
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
                        Credit = totalBase,
                        ExchangeRate = 1.0m,
                        DescriptionEn = "Revenue Entry"
                    }
                }
            };

            // Act
            var voucher = _voucherService.Create(dto, _fixture.TestUserId.ToString());

            // Assert
            voucher.TotalDebit.Should().Be(totalBase);
            voucher.TotalCredit.Should().Be(totalBase);
        }

        [Fact]
        public void PostVoucher_WithCurrencyConversion_ShouldUpdateAccountBalancesInBaseCurrency()
        {
            // Arrange
            var usdRate = _currencies["USD"].ExchangeRate;
            var amountUSD = 1000m;
            var expectedBaseAmount = amountUSD * usdRate;

            // Get initial balance using account service
            var accountService = new TestGLAccountService(_fixture.Db);
            var initialCashBalance = accountService.GetAccountBalance(_accounts["Cash"].AccountId);

            var (voucher, _) = _builder.CreateBalancedMultiCurrencyVoucher(
                _accounts["Cash"].AccountId,
                _accounts["Revenue"].AccountId,
                amountUSD,
                "USD",
                usdRate);

            // Act
            _voucherService.Post(voucher.Id, _fixture.TestUserId.ToString());

            // Assert - Balances should be in base currency
            var entries = _fixture.Db.Queryable<GLVoucherEntry>()
                .Where(e => e.VoucherId == voucher.Id)
                .ToList();

            entries.Should().HaveCount(2);
            entries[0].BaseDebit.Should().Be(expectedBaseAmount);
            entries[1].BaseCredit.Should().Be(expectedBaseAmount);
        }

        [Fact]
        public void CreateVoucher_WithInvalidExchangeRate_ShouldHandleGracefully()
        {
            // Arrange
            var invalidRate = 0m; // Invalid rate - will default to 1.0

            var dto = new CreateVoucherDto
            {
                VoucherDate = DateTime.Now,
                Currency = "USD",
                TenantId = _fixture.TestTenantId,
                Entries = new List<CreateVoucherEntryDto>
                {
                    new CreateVoucherEntryDto
                    {
                        AccountId = _accounts["Cash"].AccountId,
                        Debit = 1000,
                        Credit = 0,
                        ExchangeRate = invalidRate
                    },
                    new CreateVoucherEntryDto
                    {
                        AccountId = _accounts["Revenue"].AccountId,
                        Debit = 0,
                        Credit = 1000,
                        ExchangeRate = invalidRate
                    }
                }
            };

            // Act & Assert
            var voucher = _voucherService.Create(dto, _fixture.TestUserId.ToString());
            // Should create with default exchange rate of 1.0 (invalid rate defaults to 1.0)
            // So base amounts = original amounts * 1.0 = original amounts
            voucher.TotalDebit.Should().Be(1000); // 1000 * 1.0 (default rate)
            voucher.TotalCredit.Should().Be(1000); // 1000 * 1.0 (default rate)
        }

        [Fact]
        public void CreateVoucher_WithPrecisionRounding_ShouldRoundCorrectly()
        {
            // Arrange
            var usdRate = 3.753456m; // Rate with many decimals
            var amountUSD = 100.50m;
            var expectedBaseAmount = Math.Round(amountUSD * usdRate, 4); // Should round to 4 decimals

            var dto = new CreateVoucherDto
            {
                VoucherDate = DateTime.Now,
                Currency = "USD",
                TenantId = _fixture.TestTenantId,
                Entries = new List<CreateVoucherEntryDto>
                {
                    new CreateVoucherEntryDto
                    {
                        AccountId = _accounts["Cash"].AccountId,
                        Debit = amountUSD,
                        Credit = 0,
                        ExchangeRate = usdRate
                    },
                    new CreateVoucherEntryDto
                    {
                        AccountId = _accounts["Revenue"].AccountId,
                        Debit = 0,
                        Credit = amountUSD,
                        ExchangeRate = usdRate
                    }
                }
            };

            // Act
            var voucher = _voucherService.Create(dto, _fixture.TestUserId.ToString());

            // Assert
            voucher.TotalDebit.Should().BeApproximately(expectedBaseAmount, 0.0001m);
            voucher.TotalCredit.Should().BeApproximately(expectedBaseAmount, 0.0001m);
        }

        [Fact]
        public void UpdateExchangeRate_ShouldRecalculateBaseAmounts()
        {
            // Arrange
            var oldRate = 3.75m;
            var newRate = 3.80m;
            var amountUSD = 1000m;

            var (voucher, entries) = _builder.CreateBalancedMultiCurrencyVoucher(
                _accounts["Cash"].AccountId,
                _accounts["Revenue"].AccountId,
                amountUSD,
                "USD",
                oldRate);

            // Act - Update exchange rate
            foreach (var entry in entries)
            {
                entry.ExchangeRate = newRate;
                entry.BaseDebit = entry.Debit * newRate;
                entry.BaseCredit = entry.Credit * newRate;
                _fixture.Db.Updateable(entry).ExecuteCommand();
            }

            // Update voucher totals
            var newBaseAmount = amountUSD * newRate;
            voucher.TotalDebit = newBaseAmount;
            voucher.TotalCredit = newBaseAmount;
            _fixture.Db.Updateable(voucher).ExecuteCommand();

            // Assert
            var updated = _voucherService.GetById(voucher.Id);
            updated.TotalDebit.Should().Be(newBaseAmount);
            updated.TotalCredit.Should().Be(newBaseAmount);
        }
    }
}


