using System;
using System.Collections.Generic;
using FluentAssertions;
using Xunit;
using ZR.Model.Finance;
using ZR.Model.Finance.Dto;
using ZR.Model.Finance.Enums;
using ZR.Service.Tests.ERP.Finance.TestHelpers;

namespace ZR.Service.Tests.ERP.Finance.Integration.GL_Voucher_Integration
{
    /// <summary>
    /// Integration tests for GL-Voucher flow - اختبارات التكامل بين الحسابات والسندات
    /// Tests the complete flow: Create Account → Create Voucher → Post → Verify Balances
    /// </summary>
    public class GLVoucherIntegrationTests : IClassFixture<FinanceTestFixture>
    {
        private readonly FinanceTestFixture _fixture;
        private readonly TestGLAccountService _accountService;
        private readonly TestGLVoucherService _voucherService;
        private readonly FinanceTestDataBuilder _builder;
        private readonly Dictionary<string, GLAccount> _accounts;

        public GLVoucherIntegrationTests(FinanceTestFixture fixture)
        {
            _fixture = fixture;

            // Create test services that work directly with ISqlSugarClient
            _accountService = new TestGLAccountService(_fixture.Db);
            _voucherService = new TestGLVoucherService(_fixture.Db);

            // Create test data builder
            _builder = new FinanceTestDataBuilder(_fixture.Db, _fixture.TestTenantId, _fixture.TestUserId);

            // Create standard accounts
            _accounts = _builder.CreateStandardChartOfAccounts();
        }

        [Fact]
        public void CreateVoucher_Post_ShouldUpdateAccountBalances()
        {
            // Arrange
            var cashAccount = _accounts["Cash"];
            var revenueAccount = _accounts["Revenue"];

            var initialCashBalance = _accountService.GetAccountBalance(cashAccount.AccountId);
            var initialRevenueBalance = _accountService.GetAccountBalance(revenueAccount.AccountId);

            var dto = new CreateVoucherDto
            {
                VoucherDate = DateTime.Now,
                DescriptionAr = "سند بيع",
                DescriptionEn = "Sales Voucher",
                Currency = "SAR",
                FiscalYear = DateTime.Now.Year,
                FiscalPeriod = DateTime.Now.Month,
                Entries = new List<CreateVoucherEntryDto>
                {
                    new CreateVoucherEntryDto
                    {
                        AccountId = cashAccount.AccountId,
                        Debit = 5000,
                        Credit = 0,
                        DescriptionAr = "استلام نقدي",
                        DescriptionEn = "Cash Receipt",
                        ExchangeRate = 1
                    },
                    new CreateVoucherEntryDto
                    {
                        AccountId = revenueAccount.AccountId,
                        Debit = 0,
                        Credit = 5000,
                        DescriptionAr = "إيراد مبيعات",
                        DescriptionEn = "Sales Revenue",
                        ExchangeRate = 1
                    }
                }
            };

            // Act
            var voucher = _voucherService.Create(dto, _fixture.TestUserId.ToString());
            _voucherService.Post(voucher.Id, _fixture.TestUserId.ToString());

            // Assert
            var finalCashBalance = _accountService.GetAccountBalance(cashAccount.AccountId);
            var finalRevenueBalance = _accountService.GetAccountBalance(revenueAccount.AccountId);

            finalCashBalance.Should().Be(initialCashBalance + 5000);
            finalRevenueBalance.Should().Be(initialRevenueBalance + 5000); // Revenue is credit normal
        }

        [Fact]
        public void CreateMultipleVouchers_Post_ShouldAccumulateBalances()
        {
            // Arrange
            var cashAccount = _accounts["Cash"];
            var revenueAccount = _accounts["Revenue"];

            var initialCashBalance = _accountService.GetAccountBalance(cashAccount.AccountId);
            var initialRevenueBalance = _accountService.GetAccountBalance(revenueAccount.AccountId);

            // Act - Create and post multiple vouchers
            for (int i = 0; i < 3; i++)
            {
                var dto = new CreateVoucherDto
                {
                    VoucherDate = DateTime.Now.AddDays(i),
                    DescriptionAr = $"سند {i + 1}",
                    DescriptionEn = $"Voucher {i + 1}",
                    Entries = new List<CreateVoucherEntryDto>
                    {
                        new CreateVoucherEntryDto
                        {
                            AccountId = cashAccount.AccountId,
                            Debit = 1000,
                            Credit = 0,
                            ExchangeRate = 1
                        },
                        new CreateVoucherEntryDto
                        {
                            AccountId = revenueAccount.AccountId,
                            Debit = 0,
                            Credit = 1000,
                            ExchangeRate = 1
                        }
                    }
                };

                var voucher = _voucherService.Create(dto, _fixture.TestUserId.ToString());
                _voucherService.Post(voucher.Id, _fixture.TestUserId.ToString());
            }

            // Assert
            var finalCashBalance = _accountService.GetAccountBalance(cashAccount.AccountId);
            var finalRevenueBalance = _accountService.GetAccountBalance(revenueAccount.AccountId);

            finalCashBalance.Should().Be(initialCashBalance + 3000);
            finalRevenueBalance.Should().Be(initialRevenueBalance + 3000);
        }

        [Fact]
        public void CreateVoucher_Delete_ShouldNotAffectBalances()
        {
            // Arrange
            var cashAccount = _accounts["Cash"];
            var revenueAccount = _accounts["Revenue"];

            var initialCashBalance = _accountService.GetAccountBalance(cashAccount.AccountId);

            var dto = new CreateVoucherDto
            {
                VoucherDate = DateTime.Now,
                Entries = new List<CreateVoucherEntryDto>
                {
                    new CreateVoucherEntryDto
                    {
                        AccountId = cashAccount.AccountId,
                        Debit = 5000,
                        Credit = 0,
                        ExchangeRate = 1
                    },
                    new CreateVoucherEntryDto
                    {
                        AccountId = revenueAccount.AccountId,
                        Debit = 0,
                        Credit = 5000,
                        ExchangeRate = 1
                    }
                }
            };

            // Act
            var voucher = _voucherService.Create(dto, _fixture.TestUserId.ToString());
            _voucherService.Delete(voucher.Id);

            // Assert
            var finalCashBalance = _accountService.GetAccountBalance(cashAccount.AccountId);
            finalCashBalance.Should().Be(initialCashBalance); // Should not change
        }

        [Fact]
        public void CreateVoucher_Update_Post_ShouldReflectUpdatedAmounts()
        {
            // Arrange
            var cashAccount = _accounts["Cash"];
            var revenueAccount = _accounts["Revenue"];

            var initialCashBalance = _accountService.GetAccountBalance(cashAccount.AccountId);

            var dto = new CreateVoucherDto
            {
                VoucherDate = DateTime.Now,
                Entries = new List<CreateVoucherEntryDto>
                {
                    new CreateVoucherEntryDto
                    {
                        AccountId = cashAccount.AccountId,
                        Debit = 1000,
                        Credit = 0,
                        ExchangeRate = 1
                    },
                    new CreateVoucherEntryDto
                    {
                        AccountId = revenueAccount.AccountId,
                        Debit = 0,
                        Credit = 1000,
                        ExchangeRate = 1
                    }
                }
            };

            var voucher = _voucherService.Create(dto, _fixture.TestUserId.ToString());

            // Update voucher
            var updateDto = new UpdateVoucherDto
            {
                Id = voucher.Id,
                VoucherDate = voucher.VoucherDate,
                Entries = new List<UpdateVoucherEntryDto>
                {
                    new UpdateVoucherEntryDto
                    {
                        AccountId = cashAccount.AccountId,
                        Debit = 2000, // Updated amount
                        Credit = 0,
                        ExchangeRate = 1
                    },
                    new UpdateVoucherEntryDto
                    {
                        AccountId = revenueAccount.AccountId,
                        Debit = 0,
                        Credit = 2000, // Updated amount
                        ExchangeRate = 1
                    }
                }
            };

            // Act
            _voucherService.Update(updateDto, _fixture.TestUserId.ToString());
            _voucherService.Post(voucher.Id, _fixture.TestUserId.ToString());

            // Assert
            var finalCashBalance = _accountService.GetAccountBalance(cashAccount.AccountId);
            finalCashBalance.Should().Be(initialCashBalance + 2000); // Should reflect updated amount
        }

        [Fact]
        public void CreateVoucher_WithMultipleAccounts_ShouldUpdateAllBalances()
        {
            // Arrange
            var cashAccount = _accounts["Cash"];
            var bankAccount = _accounts["Bank"];
            var revenueAccount = _accounts["Revenue"];

            var initialCashBalance = _accountService.GetAccountBalance(cashAccount.AccountId);
            var initialBankBalance = _accountService.GetAccountBalance(bankAccount.AccountId);
            var initialRevenueBalance = _accountService.GetAccountBalance(revenueAccount.AccountId);

            var dto = new CreateVoucherDto
            {
                VoucherDate = DateTime.Now,
                DescriptionAr = "سند متعدد الحسابات",
                DescriptionEn = "Multi-Account Voucher",
                Entries = new List<CreateVoucherEntryDto>
                {
                    new CreateVoucherEntryDto
                    {
                        AccountId = cashAccount.AccountId,
                        Debit = 3000,
                        Credit = 0,
                        ExchangeRate = 1
                    },
                    new CreateVoucherEntryDto
                    {
                        AccountId = bankAccount.AccountId,
                        Debit = 2000,
                        Credit = 0,
                        ExchangeRate = 1
                    },
                    new CreateVoucherEntryDto
                    {
                        AccountId = revenueAccount.AccountId,
                        Debit = 0,
                        Credit = 5000,
                        ExchangeRate = 1
                    }
                }
            };

            // Act
            var voucher = _voucherService.Create(dto, _fixture.TestUserId.ToString());
            _voucherService.Post(voucher.Id, _fixture.TestUserId.ToString());

            // Assert
            var finalCashBalance = _accountService.GetAccountBalance(cashAccount.AccountId);
            var finalBankBalance = _accountService.GetAccountBalance(bankAccount.AccountId);
            var finalRevenueBalance = _accountService.GetAccountBalance(revenueAccount.AccountId);

            finalCashBalance.Should().Be(initialCashBalance + 3000);
            finalBankBalance.Should().Be(initialBankBalance + 2000);
            finalRevenueBalance.Should().Be(initialRevenueBalance + 5000);
        }

        [Fact]
        public void CreateVoucher_VerifyDoubleEntryIntegrity()
        {
            // Arrange
            var cashAccount = _accounts["Cash"];
            var revenueAccount = _accounts["Revenue"];

            var dto = new CreateVoucherDto
            {
                VoucherDate = DateTime.Now,
                Entries = new List<CreateVoucherEntryDto>
                {
                    new CreateVoucherEntryDto
                    {
                        AccountId = cashAccount.AccountId,
                        Debit = 5000,
                        Credit = 0,
                        ExchangeRate = 1
                    },
                    new CreateVoucherEntryDto
                    {
                        AccountId = revenueAccount.AccountId,
                        Debit = 0,
                        Credit = 5000,
                        ExchangeRate = 1
                    }
                }
            };

            // Act
            var voucher = _voucherService.Create(dto, _fixture.TestUserId.ToString());
            var validation = _voucherService.ValidateDoubleEntry(voucher.Id);
            var totals = _voucherService.GetVoucherTotals(voucher.Id);

            // Assert
            validation.IsValid.Should().BeTrue();
            totals.IsBalanced.Should().BeTrue();
            totals.TotalDebit.Should().Be(5000);
            totals.TotalCredit.Should().Be(5000);
        }
    }
}
