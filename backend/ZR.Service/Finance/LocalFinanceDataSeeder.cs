using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Infrastructure.Attribute;
using ZR.Model.Finance.GL;
using ZR.Model.Finance.ARAP;
using ZR.Model.Customers;
using ZR.Model.Finance.Configuration;
using ZR.Model.Finance.Treasury;
using ZR.Model.Finance.Treasury.Enums;
using ZR.Model.Finance.Assets;
using ZR.Model.Finance.Assets.Enums;
using ZR.Repository.Finance.GL;
using ZR.Repository.Finance.ARAP;
using ZR.Repository.Customers;
using ZR.Repository.Finance.Configuration;
using ZR.Repository.Finance.Assets;
using ZR.Repository.Finance.Treasury;
using ZR.Service.IService.Finance;
using ZR.Service.IService.Finance.GL;
using ZR.Service.IService.Finance.Treasury;
using AccountNature = ZR.Model.Finance.Enums.AccountNature;

namespace ZR.Service.Finance
{
    /// <summary>
    /// Local Finance Data Seeder - زراعة البيانات المالية المحلية
    /// Seeds default data for all finance modules directly to local database
    /// </summary>
    [AppService(ServiceType = typeof(ILocalFinanceDataSeeder), ServiceLifetime = LifeTime.Transient)]
    public class LocalFinanceDataSeeder : ILocalFinanceDataSeeder
    {
        private readonly IGLAccountRepository _accountRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IVendorRepository _vendorRepository;
        private readonly ICurrencyRepository _currencyRepository;
        private readonly AssetCategoryRepository _assetCategoryRepository;
        private readonly BankAccountRepository _bankAccountRepository;
        private readonly CashAccountRepository _cashAccountRepository;
        private readonly TreasuryConfigRepository _treasuryConfigRepository;
        private readonly IGLAccountSeeder _glAccountSeeder;
        private readonly ITreasuryGLAccountMappingSeeder _treasuryMappingSeeder;

        public LocalFinanceDataSeeder(
            IGLAccountRepository accountRepository,
            ICustomerRepository customerRepository,
            IVendorRepository vendorRepository,
            ICurrencyRepository currencyRepository,
            AssetCategoryRepository assetCategoryRepository,
            BankAccountRepository bankAccountRepository,
            CashAccountRepository cashAccountRepository,
            TreasuryConfigRepository treasuryConfigRepository,
            IGLAccountSeeder glAccountSeeder,
            ITreasuryGLAccountMappingSeeder treasuryMappingSeeder)
        {
            _accountRepository = accountRepository;
            _customerRepository = customerRepository;
            _vendorRepository = vendorRepository;
            _currencyRepository = currencyRepository;
            _assetCategoryRepository = assetCategoryRepository;
            _bankAccountRepository = bankAccountRepository;
            _cashAccountRepository = cashAccountRepository;
            _treasuryConfigRepository = treasuryConfigRepository;
            _glAccountSeeder = glAccountSeeder;
            _treasuryMappingSeeder = treasuryMappingSeeder;
        }

        /// <summary>
        /// Seed all finance data locally
        /// زراعة جميع البيانات المالية محلياً
        /// </summary>
        public async Task<LocalSeederResult> SeedAllLocalDataAsync(long? companyId, string createdBy)
        {
            var result = new LocalSeederResult();
            var now = DateTime.Now;

            try
            {
                // 0. Seed GL Accounts FIRST (must be before bank/cash accounts as they depend on GL accounts)
                var glAccountsExist = await _glAccountSeeder.DefaultAccountsExistAsync(companyId);
                if (!glAccountsExist)
                {
                    await _glAccountSeeder.SeedDefaultAccountsAsync(companyId, createdBy);
                }

                // 1. Seed Currencies (must be first as other modules may depend on it)
                result.CurrenciesCreated = await SeedCurrenciesAsync(companyId, createdBy, now);

                // 2. Seed Asset Categories
                result.AssetCategoriesCreated = await SeedAssetCategoriesAsync(companyId, createdBy, now);

                // 3. Seed Customers
                result.CustomersCreated = await SeedCustomersAsync(companyId, createdBy, now);

                // 4. Seed Vendors
                result.VendorsCreated = await SeedVendorsAsync(companyId, createdBy, now);

                // 5. Seed Bank Accounts (Al Rajhi & NCB)
                result.BankAccountsCreated = await SeedBankAccountsAsync(companyId, createdBy, now);

                // 6. Seed Cash Accounts (Main Cash Box)
                result.CashAccountsCreated = await SeedCashAccountsAsync(companyId, createdBy, now);

                // 7. Link Treasury Accounts to GL Accounts
                try
                {
                    await _glAccountSeeder.LinkTreasuryAccountsToGLAsync(companyId, createdBy);
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Treasury Account Linking: {ex.Message}");
                }

                // 8. Seed Treasury GL Account Mappings
                try
                {
                    result.TreasuryMappingsCreated = await _treasuryMappingSeeder.SeedDefaultMappingsAsync(companyId, createdBy);
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Treasury Mappings: {ex.Message}");
                }

                // 9. Seed Treasury Config
                result.TreasuryConfigCreated = await SeedTreasuryConfigAsync(companyId, createdBy, now);

                result.Success = true;
                result.Message = $"Successfully seeded all finance data - تم زراعة جميع البيانات المالية بنجاح";
                result.TotalRecordsCreated = result.CurrenciesCreated + result.AssetCategoriesCreated + 
                    result.CustomersCreated + result.VendorsCreated + result.BankAccountsCreated + 
                    result.CashAccountsCreated + result.TreasuryMappingsCreated + (result.TreasuryConfigCreated > 0 ? 1 : 0);
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Failed to seed finance data: {ex.Message}";
                result.Errors.Add(ex.Message);
            }

            return result;
        }

        #region Currency Seeding

        private async Task<int> SeedCurrenciesAsync(long? companyId, string createdBy, DateTime now)
        {
            try
            {
                // Check if currencies already exist
                var existing = _currencyRepository.GetActiveCurrencies();
                if (existing != null && existing.Count > 0)
                {
                    return 0; // Already seeded
                }

                var currencies = new List<Currency>
                {
                    new Currency
                    {
                        Id = Guid.NewGuid(),
                        Code = "SAR",
                        Name = "Saudi Riyal",
                        NameAr = "الريال السعودي",
                        Symbol = "﷼",
                        IsBaseCurrency = true,
                        DecimalPlaces = 2,
                        IsActive = true,
                        RoundingMethod = 0,
                        DisplayFormat = "#,##0.00",
                        SortOrder = 1,
                        CompanyID = companyId,
                        Create_by = createdBy,
                        Create_time = now
                    },
                    new Currency
                    {
                        Id = Guid.NewGuid(),
                        Code = "USD",
                        Name = "US Dollar",
                        NameAr = "الدولار الأمريكي",
                        Symbol = "$",
                        IsBaseCurrency = false,
                        DecimalPlaces = 2,
                        IsActive = true,
                        RoundingMethod = 0,
                        DisplayFormat = "#,##0.00",
                        SortOrder = 2,
                        CompanyID = companyId,
                        Create_by = createdBy,
                        Create_time = now
                    },
                    new Currency
                    {
                        Id = Guid.NewGuid(),
                        Code = "EUR",
                        Name = "Euro",
                        NameAr = "اليورو",
                        Symbol = "€",
                        IsBaseCurrency = false,
                        DecimalPlaces = 2,
                        IsActive = true,
                        RoundingMethod = 0,
                        DisplayFormat = "#,##0.00",
                        SortOrder = 3,
                        CompanyID = companyId,
                        Create_by = createdBy,
                        Create_time = now
                    },
                    new Currency
                    {
                        Id = Guid.NewGuid(),
                        Code = "GBP",
                        Name = "British Pound",
                        NameAr = "الجنيه الإسترليني",
                        Symbol = "£",
                        IsBaseCurrency = false,
                        DecimalPlaces = 2,
                        IsActive = true,
                        RoundingMethod = 0,
                        DisplayFormat = "#,##0.00",
                        SortOrder = 4,
                        CompanyID = companyId,
                        Create_by = createdBy,
                        Create_time = now
                    }
                };

                int count = 0;
                foreach (var currency in currencies)
                {
                    // Check if currency code already exists
                    var existingCurrency = _currencyRepository.GetByCode(currency.Code);
                    if (existingCurrency == null)
                    {
                        _currencyRepository.CreateCurrency(currency);
                        count++;
                    }
                }

                return count;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to seed currencies: {ex.Message}", ex);
            }
        }

        #endregion

        #region Asset Category Seeding

        private async Task<int> SeedAssetCategoriesAsync(long? companyId, string createdBy, DateTime now)
        {
            try
            {
                // Check if the specific asset categories we're seeding already exist
                var existingVEH = _assetCategoryRepository.GetFirst(x => x.CategoryCode == "VEH");
                var existingEQUIP = _assetCategoryRepository.GetFirst(x => x.CategoryCode == "EQUIP");
                var existingBUILD = _assetCategoryRepository.GetFirst(x => x.CategoryCode == "BUILD");
                if (existingVEH != null && existingEQUIP != null && existingBUILD != null)
                {
                    return 0; // Already seeded (at least the main categories exist)
                }

                // Get GL accounts for asset categories (use async methods and correct account numbers)
                var assetAccount = await _accountRepository.GetByAccountNumberAsync("1210000000"); // Property, Plant & Equipment
                var depreciationExpenseAccount = await _accountRepository.GetByAccountNumberAsync("5410000000"); // Depreciation Expense
                var accumulatedDepreciationAccount = await _accountRepository.GetByAccountNumberAsync("1211000000"); // Accumulated Depreciation (Contra-asset)
                var gainLossAccount = await _accountRepository.GetByAccountNumberAsync("5411000000"); // Gain/Loss on Asset Disposal

                var categories = new List<AssetCategory>
                {
                    new AssetCategory
                    {
                        Id = Guid.NewGuid(),
                        CategoryCode = "VEH",
                        NameAr = "المركبات",
                        NameEn = "Vehicles",
                        DepreciationMethod = DepreciationMethod.StraightLine,
                        UsefulLifeMonths = 60, // 5 years
                        DefaultAssetAccountId = assetAccount?.AccountId,
                        DefaultDepreciationExpenseAccountId = depreciationExpenseAccount?.AccountId,
                        DefaultAccumulatedDepreciationAccountId = accumulatedDepreciationAccount?.AccountId,
                        DefaultGainLossAccountId = gainLossAccount?.AccountId,
                        IsActive = true,
                        CompanyID = companyId,
                        Create_by = createdBy,
                        Create_time = now
                    },
                    new AssetCategory
                    {
                        Id = Guid.NewGuid(),
                        CategoryCode = "EQUIP",
                        NameAr = "المعدات",
                        NameEn = "Equipment",
                        DepreciationMethod = DepreciationMethod.StraightLine,
                        UsefulLifeMonths = 84, // 7 years
                        DefaultAssetAccountId = assetAccount?.AccountId,
                        DefaultDepreciationExpenseAccountId = depreciationExpenseAccount?.AccountId,
                        DefaultAccumulatedDepreciationAccountId = accumulatedDepreciationAccount?.AccountId,
                        DefaultGainLossAccountId = gainLossAccount?.AccountId,
                        IsActive = true,
                        CompanyID = companyId,
                        Create_by = createdBy,
                        Create_time = now
                    },
                    new AssetCategory
                    {
                        Id = Guid.NewGuid(),
                        CategoryCode = "BUILD",
                        NameAr = "المباني",
                        NameEn = "Buildings",
                        DepreciationMethod = DepreciationMethod.StraightLine,
                        UsefulLifeMonths = 240, // 20 years
                        DefaultAssetAccountId = assetAccount?.AccountId,
                        DefaultDepreciationExpenseAccountId = depreciationExpenseAccount?.AccountId,
                        DefaultAccumulatedDepreciationAccountId = accumulatedDepreciationAccount?.AccountId,
                        DefaultGainLossAccountId = gainLossAccount?.AccountId,
                        IsActive = true,
                        CompanyID = companyId,
                        Create_by = createdBy,
                        Create_time = now
                    },
                    new AssetCategory
                    {
                        Id = Guid.NewGuid(),
                        CategoryCode = "FURN",
                        NameAr = "الأثاث",
                        NameEn = "Furniture & Fixtures",
                        DepreciationMethod = DepreciationMethod.StraightLine,
                        UsefulLifeMonths = 60, // 5 years
                        DefaultAssetAccountId = assetAccount?.AccountId,
                        DefaultDepreciationExpenseAccountId = depreciationExpenseAccount?.AccountId,
                        DefaultAccumulatedDepreciationAccountId = accumulatedDepreciationAccount?.AccountId,
                        DefaultGainLossAccountId = gainLossAccount?.AccountId,
                        IsActive = true,
                        CompanyID = companyId,
                        Create_by = createdBy,
                        Create_time = now
                    },
                    new AssetCategory
                    {
                        Id = Guid.NewGuid(),
                        CategoryCode = "COMP",
                        NameAr = "أجهزة الكمبيوتر",
                        NameEn = "Computers & IT Equipment",
                        DepreciationMethod = DepreciationMethod.StraightLine,
                        UsefulLifeMonths = 36, // 3 years
                        DefaultAssetAccountId = assetAccount?.AccountId,
                        DefaultDepreciationExpenseAccountId = depreciationExpenseAccount?.AccountId,
                        DefaultAccumulatedDepreciationAccountId = accumulatedDepreciationAccount?.AccountId,
                        DefaultGainLossAccountId = gainLossAccount?.AccountId,
                        IsActive = true,
                        CompanyID = companyId,
                        Create_by = createdBy,
                        Create_time = now
                    }
                };

                int count = 0;
                foreach (var category in categories)
                {
                    // Check if category code already exists
                    var existingCategory = _assetCategoryRepository.GetFirst(x => x.CategoryCode == category.CategoryCode);
                    if (existingCategory == null)
                    {
                        _assetCategoryRepository.Insert(category);
                        count++;
                    }
                }

                return count;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to seed asset categories: {ex.Message}", ex);
            }
        }

        #endregion

        #region Customer Seeding

        private async Task<int> SeedCustomersAsync(long? companyId, string createdBy, DateTime now)
        {
            try
            {
                // Check if customers already exist
                var existing = _customerRepository.GetActiveCustomers();
                if (existing != null && existing.Count > 0)
                {
                    return 0; // Already seeded
                }

                // Get AR Control Account (Accounts Receivable)
                var arAccount = _accountRepository.GetByAccountNumber("1120"); // Accounts Receivable

                var customers = new List<Customer>
                {
                    new Customer
                    {
                        Id = Guid.NewGuid(),
                        CustomerCode = "CUST001",
                        NameAr = "عميل تجريبي 1",
                        NameEn = "Sample Customer 1",
                        ContactPerson = "أحمد محمد",
                        Email = "customer1@example.com",
                        Phone = "+966501234567",
                        CreditLimit = 100000,
                        GLAccountId = arAccount?.AccountId,
                        IsActive = true,
                        CompanyID = companyId,
                        Create_by = createdBy,
                        Create_time = now
                    },
                    new Customer
                    {
                        Id = Guid.NewGuid(),
                        CustomerCode = "CUST002",
                        NameAr = "عميل تجريبي 2",
                        NameEn = "Sample Customer 2",
                        ContactPerson = "فاطمة علي",
                        Email = "customer2@example.com",
                        Phone = "+966507654321",
                        CreditLimit = 50000,
                        GLAccountId = arAccount?.AccountId,
                        IsActive = true,
                        CompanyID = companyId,
                        Create_by = createdBy,
                        Create_time = now
                    },
                    new Customer
                    {
                        Id = Guid.NewGuid(),
                        CustomerCode = "CUST003",
                        NameAr = "عميل تجريبي 3",
                        NameEn = "Sample Customer 3",
                        ContactPerson = "خالد سعيد",
                        Email = "customer3@example.com",
                        Phone = "+966509876543",
                        CreditLimit = 200000,
                        GLAccountId = arAccount?.AccountId,
                        IsActive = true,
                        CompanyID = companyId,
                        Create_by = createdBy,
                        Create_time = now
                    }
                };

                int count = 0;
                foreach (var customer in customers)
                {
                    // Check if customer code already exists
                    var existingCustomer = _customerRepository.GetByCode(customer.CustomerCode);
                    if (existingCustomer == null)
                    {
                        _customerRepository.Insert(customer);
                        count++;
                    }
                }

                return count;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to seed customers: {ex.Message}", ex);
            }
        }

        #endregion

        #region Vendor Seeding

        private async Task<int> SeedVendorsAsync(long? companyId, string createdBy, DateTime now)
        {
            try
            {
                // Check if vendors already exist
                var existing = _vendorRepository.GetActiveVendors();
                if (existing != null && existing.Count > 0)
                {
                    return 0; // Already seeded
                }

                // Get AP Control Account (Accounts Payable)
                var apAccount = _accountRepository.GetByAccountNumber("2110"); // Accounts Payable

                var vendors = new List<Vendor>
                {
                    new Vendor
                    {
                        Id = Guid.NewGuid(),
                        VendorCode = "VEND001",
                        NameAr = "مورد تجريبي 1",
                        NameEn = "Sample Vendor 1",
                        ContactPerson = "محمد أحمد",
                        Email = "vendor1@example.com",
                        Phone = "+966501111111",
                        GLAccountId = apAccount?.AccountId,
                        IsActive = true,
                        CompanyID = companyId,
                        Create_by = createdBy,
                        Create_time = now
                    },
                    new Vendor
                    {
                        Id = Guid.NewGuid(),
                        VendorCode = "VEND002",
                        NameAr = "مورد تجريبي 2",
                        NameEn = "Sample Vendor 2",
                        ContactPerson = "سارة خالد",
                        Email = "vendor2@example.com",
                        Phone = "+966502222222",
                        GLAccountId = apAccount?.AccountId,
                        IsActive = true,
                        CompanyID = companyId,
                        Create_by = createdBy,
                        Create_time = now
                    },
                    new Vendor
                    {
                        Id = Guid.NewGuid(),
                        VendorCode = "VEND003",
                        NameAr = "مورد تجريبي 3",
                        NameEn = "Sample Vendor 3",
                        ContactPerson = "عمر يوسف",
                        Email = "vendor3@example.com",
                        Phone = "+966503333333",
                        GLAccountId = apAccount?.AccountId,
                        IsActive = true,
                        CompanyID = companyId,
                        Create_by = createdBy,
                        Create_time = now
                    }
                };

                int count = 0;
                foreach (var vendor in vendors)
                {
                    // Check if vendor code already exists
                    var existingVendor = _vendorRepository.GetByCode(vendor.VendorCode);
                    if (existingVendor == null)
                    {
                        _vendorRepository.Insert(vendor);
                        count++;
                    }
                }

                return count;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to seed vendors: {ex.Message}", ex);
            }
        }

        #endregion

        #region Bank Account Seeding

        private async Task<int> SeedBankAccountsAsync(long? companyId, string createdBy, DateTime now)
        {
            try
            {
                // Check if the specific bank accounts we're seeding already exist
                var existingAlRajhi = _bankAccountRepository.GetFirst(x => x.BankCode == "ALRAJHI" && !x.IsDeleted);
                var existingNCB = _bankAccountRepository.GetFirst(x => x.BankCode == "NCB" && !x.IsDeleted);
                if (existingAlRajhi != null && existingNCB != null)
                {
                    return 0; // Already seeded
                }

                // Get GL account for bank accounts (1112000000 - Bank Accounts)
                // This is the Level 4 account for all bank accounts
                var bankGLAccount = await _accountRepository.GetByAccountNumberAsync("1112000000");
                if (bankGLAccount == null)
                {
                    // Try to get a child account if parent doesn't exist
                    var cashGLParent = await _accountRepository.GetByAccountNumberAsync("1110000000");
                    if (cashGLParent == null)
                    {
                        throw new Exception("GL Account 1112000000 (Bank Accounts) not found. Please seed GL accounts first.");
                    }
                    // Use a child account if available
                    var bankGLAccounts = await _accountRepository.GetChildAccountsAsync(cashGLParent.AccountId);
                    bankGLAccount = bankGLAccounts.FirstOrDefault(x => x.AccountNature == AccountNature.Bank) ?? bankGLAccounts.FirstOrDefault();
                    if (bankGLAccount == null)
                    {
                        throw new Exception("No suitable GL account found for bank accounts. Please seed GL accounts first.");
                    }
                }

                // Get SAR currency
                var sarCurrency = _currencyRepository.GetByCode("SAR");

                var bankAccounts = new List<BankAccount>
                {
                    new BankAccount
                    {
                        Id = Guid.NewGuid(),
                        BankCode = "ALRAJHI",
                        BankNameAr = "البنك الأهلي السعودي",
                        BankNameEn = "Al Rajhi Bank",
                        AccountNumber = "1234567890",
                        IBAN = "SA1234567890123456789012",
                        SwiftCode = "RJHISARI",
                        Currency = "SAR",
                        CurrencyId = sarCurrency?.Id,
                        Branch = "الرياض - الفرع الرئيسي",
                        BankContact = "info@alrajhibank.com.sa",
                        GLAccountId = bankGLAccount?.AccountId,
                      
                        PendingChequesAmount = 0,
                        OverdraftLimit = 0,
                        IsOverdraftAllowed = false,
                        Status = AccountStatus.Active,
                        IsDeleted = false,
                        CompanyID = companyId,
                        Create_by = createdBy,
                        Create_time = now
                    },
                    new BankAccount
                    {
                        Id = Guid.NewGuid(),
                        BankCode = "NCB",
                        BankNameAr = "البنك الأهلي التجاري",
                        BankNameEn = "National Commercial Bank",
                        AccountNumber = "9876543210",
                        IBAN = "SA9876543210987654321098",
                        SwiftCode = "NCBKSAJE",
                        Currency = "SAR",
                        CurrencyId = sarCurrency?.Id,
                        Branch = "جدة - فرع الكورنيش",
                        BankContact = "contact@alahli.com",
                        GLAccountId = bankGLAccount?.AccountId,
                     
                        PendingChequesAmount = 0,
                        OverdraftLimit = 100000,
                        IsOverdraftAllowed = true,
                        Status = AccountStatus.Active,
                        IsDeleted = false,
                        CompanyID = companyId,
                        Create_by = createdBy,
                        Create_time = now
                    }
                };

                int count = 0;
                foreach (var bankAccount in bankAccounts)
                {
                    // Check if bank account number already exists
                    var existingBank = _bankAccountRepository.GetFirst(x => x.AccountNumber == bankAccount.AccountNumber && !x.IsDeleted);
                    if (existingBank == null)
                    {
                        await _bankAccountRepository.InsertAsync(bankAccount);
                        count++;
                    }
                }

                return count;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to seed bank accounts: {ex.Message}", ex);
            }
        }

        #endregion

        #region Cash Account Seeding

        private async Task<int> SeedCashAccountsAsync(long? companyId, string createdBy, DateTime now)
        {
            try
            {
                // Check if the specific cash accounts we're seeding already exist
                var existingMainCash = _cashAccountRepository.GetFirst(x => x.Code == "CASH001" && !x.IsDeleted);
                var existingPettyCash = _cashAccountRepository.GetFirst(x => x.Code == "PETTY001" && !x.IsDeleted);
                if (existingMainCash != null && existingPettyCash != null)
                {
                    return 0; // Already seeded
                }

                // Get GL account for cash accounts (1111000000 - Cash Boxes)
                // This is the Level 4 account for cash accounts
                var cashGLAccount = await _accountRepository.GetByAccountNumberAsync("1111000000");
                if (cashGLAccount == null)
                {
                    // Try to get petty cash account as fallback
                    cashGLAccount = await _accountRepository.GetByAccountNumberAsync("1113000000");
                    if (cashGLAccount == null)
                    {
                        // Try parent account
                        var cashGLParent = await _accountRepository.GetByAccountNumberAsync("1110000000");
                        if (cashGLParent == null)
                        {
                            throw new Exception("GL Account 1111000000 (Cash Boxes) not found. Please seed GL accounts first.");
                        }
                        var cashGLAccounts = await _accountRepository.GetChildAccountsAsync(cashGLParent.AccountId);
                        cashGLAccount = cashGLAccounts.FirstOrDefault(x => x.AccountNature == AccountNature.Cash) ?? cashGLAccounts.FirstOrDefault();
                        if (cashGLAccount == null)
                        {
                            throw new Exception("No suitable GL account found for cash accounts. Please seed GL accounts first.");
                        }
                    }
                }

                // Get SAR currency
                var sarCurrency = _currencyRepository.GetByCode("SAR");

                var cashAccounts = new List<CashAccount>
                {
                    new CashAccount
                    {
                        Id = Guid.NewGuid(),
                        Code = "CASH001",
                        NameAr = "الصندوق الرئيسي",
                        NameEn = "Main Cash Box",
                        Currency = "SAR",
                        CurrencyId = sarCurrency?.Id,
                        GLAccountId = cashGLAccount?.AccountId,
                     
                        CashLimit = 50000,
                        MinimumBalance = 1000,
                        CustodianName = "أمين الصندوق",
                        Location = "المكتب الرئيسي - الطابق الأول",
                        Type = AccountType.CashBox,
                        AcceptsCheques = false,
                        Status = AccountStatus.Active,
                        IsDeleted = false,
                        CompanyID = companyId,
                        Create_by = createdBy,
                        Create_time = now
                    },
                    new CashAccount
                    {
                        Id = Guid.NewGuid(),
                        Code = "PETTY001",
                        NameAr = "الصندوق الصغير",
                        NameEn = "Petty Cash",
                        Currency = "SAR",
                        CurrencyId = sarCurrency?.Id,
                        GLAccountId = cashGLAccount?.AccountId,
                        
                        CashLimit = 5000,
                        MinimumBalance = 500,
                        CustodianName = "أمين الصندوق الصغير",
                        Location = "المكتب الرئيسي - الطابق الأول",
                        Type = AccountType.PettyCash,
                        AcceptsCheques = false,
                        Status = AccountStatus.Active,
                        IsDeleted = false,
                        CompanyID = companyId,
                        Create_by = createdBy,
                        Create_time = now
                    }
                };

                int count = 0;
                foreach (var cashAccount in cashAccounts)
                {
                    // Check if cash account code already exists
                    var existingCash = _cashAccountRepository.GetFirst(x => x.Code == cashAccount.Code && !x.IsDeleted);
                    if (existingCash == null)
                    {
                        await _cashAccountRepository.InsertAsync(cashAccount);
                        count++;
                    }
                }

                return count;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to seed cash accounts: {ex.Message}", ex);
            }
        }

        #endregion

        #region Treasury Config Seeding

        private async Task<int> SeedTreasuryConfigAsync(long? companyId, string createdBy, DateTime now)
        {
            try
            {
                // Check if treasury config already exists for this company
                var existing = companyId.HasValue 
                    ? _treasuryConfigRepository.GetFirst(x => x.CompanyID == companyId.Value)
                    : _treasuryConfigRepository.GetFirst(x => x.CompanyID == null);
                if (existing != null)
                {
                    return 0; // Already seeded
                }

                var treasuryConfig = new TreasuryConfig
                {
                    Id = Guid.NewGuid(),
                    AutoImportBankStatements = false,
                    BankStatementImportFormat = BankStatementImportSource.CSV,
                    AutoReconcile = false,
                    DefaultPaymentApprovalThreshold = 10000,
                    PettyCashLimit = 5000,
                    ApprovalWorkflowEnabled = true,
                    CompanyID = companyId,
                    Create_by = createdBy,
                    Create_time = now
                };

                await _treasuryConfigRepository.InsertAsync(treasuryConfig);
                return 1;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to seed treasury config: {ex.Message}", ex);
            }
        }

        #endregion
    }

    #region DTOs

    public class LocalSeederResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int TotalRecordsCreated { get; set; }
        public int CurrenciesCreated { get; set; }
        public int AssetCategoriesCreated { get; set; }
        public int CustomersCreated { get; set; }
        public int VendorsCreated { get; set; }
        public int BankAccountsCreated { get; set; }
        public int CashAccountsCreated { get; set; }
        public int TreasuryMappingsCreated { get; set; }
        public int TreasuryConfigCreated { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }

    #endregion
}

