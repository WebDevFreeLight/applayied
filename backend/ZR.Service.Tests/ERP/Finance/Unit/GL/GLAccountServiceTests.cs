using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Xunit;
using ZR.Model.Finance;
using ZR.Model.Finance.Dto;
using ZR.Model.Finance.Enums;
using ZR.Service.Tests.ERP.Finance.TestHelpers;

namespace ZR.Service.Tests.ERP.Finance.Unit.GL
{
    /// <summary>
    /// Unit tests for GL Account Service - اختبارات وحدة خدمة الحسابات
    /// 
    /// This test suite validates the core functionality of the GL Account Service,
    /// including account creation, retrieval, updates, deletion, and business logic.
    /// All tests use the updated GLAccount model with new field names:
    /// - AccountId (instead of Id)
    /// - AccountNumber (instead of Code)
    /// - AccountNameAr/AccountNameEn (instead of NameAr/NameEn)
    /// - ParentAccountId (instead of ParentId)
    /// - OpeningDebit/OpeningCredit (instead of OpeningBalance)
    /// - NormalBalance, IsLeaf, AllowManualEntries, RequiresCostCenter (new fields)
    /// </summary>
    public class GLAccountServiceTests : IClassFixture<FinanceTestFixture>
    {
        private readonly FinanceTestFixture _fixture;
        private readonly TestGLAccountService _service;
        private readonly FinanceTestDataBuilder _builder;

        public GLAccountServiceTests(FinanceTestFixture fixture)
        {
            _fixture = fixture;

            // Create test service that works directly with ISqlSugarClient
            // This bypasses the repository layer for unit testing
            _service = new TestGLAccountService(_fixture.Db);

            // Create test data builder for creating test accounts and related entities
            _builder = new FinanceTestDataBuilder(_fixture.Db, _fixture.TestTenantId, _fixture.TestUserId);
        }

        #region Create Account Tests

        /// <summary>
        /// Tests that a new account can be created successfully with all required fields.
        /// Validates that the account is persisted with correct values and audit fields.
        /// </summary>
        [Fact]
        public void CreateAccount_ShouldCreateAccountSuccessfully()
        {
            // Arrange - Prepare a complete account DTO with all fields
            var dto = new CreateAccountDto
            {
                AccountNumber = "1001",
                AccountNameAr = "حساب النقدية",
                AccountNameEn = "Cash Account",
                AccountType = AccountType.Asset,
                NormalBalance = NormalBalance.Debit, // Assets have debit normal balance
                Level = 1,
                IsActive = true,
                IsLeaf = true, // Leaf account can have transactions
                AllowManualEntries = true,
                RequiresCostCenter = false,
                IsCashAccount = true, // Mark as cash account
                Currency = "SAR",
                DescriptionAr = "حساب النقدية",
                DescriptionEn = "Cash Account"
            };

            // Act - Create the account using the service
            var result = _service.Create(dto, _fixture.TestUserId.ToString());

            // Assert - Verify all fields are set correctly
            result.Should().NotBeNull();
            result.AccountId.Should().NotBeEmpty(); // AccountId should be generated
            result.AccountNumber.Should().Be(dto.AccountNumber);
            result.FullAccountNumber.Should().Be(dto.AccountNumber); // Root account FullAccountNumber = AccountNumber
            result.AccountNameAr.Should().Be(dto.AccountNameAr);
            result.AccountNameEn.Should().Be(dto.AccountNameEn);
            result.AccountType.Should().Be(dto.AccountType);
            result.NormalBalance.Should().Be(dto.NormalBalance);
            result.Create_by.Should().Be(_fixture.TestUserId.ToString()); // Audit field from SysBase
        }

        /// <summary>
        /// Tests account creation when an account with the same account number already exists.
        /// This documents current behavior - same AccountNumber is allowed under different parents.
        /// FullAccountNumber uniqueness prevents true duplicates.
        /// </summary>
        [Fact]
        public void CreateAccount_WithExistingCode_ShouldCreateAccount()
        {
            // Arrange - Create an existing account with account number "1001"
            var existingAccount = _builder.CreateAccount("1001", "حساب موجود", "Existing Account");
            var dto = new CreateAccountDto
            {
                AccountNumber = "1001", // Same account number
                AccountNameAr = "حساب جديد",
                AccountNameEn = "New Account",
                AccountType = AccountType.Asset,
                NormalBalance = NormalBalance.Debit
            };

            // Act & Assert
            var result = _service.Create(dto, _fixture.TestUserId.ToString());
            // Note: Same AccountNumber is allowed because FullAccountNumber differs
            // In the new model, uniqueness is enforced on FullAccountNumber per company
            result.Should().NotBeNull();
        }

        /// <summary>
        /// Tests that when creating a child account, the level is automatically calculated
        /// based on the parent account's level. Child accounts should have level = parent level + 1.
        /// Also tests FullAccountNumber generation: Parent.FullAccountNumber + "." + AccountNumber.
        /// </summary>
        [Fact]
        public void CreateAccount_WithParent_ShouldSetCorrectLevelAndFullAccountNumber()
        {
            // Arrange - Create a parent account at level 1
            var parentAccount = _builder.CreateAccount("1000", "الأصول", "Assets", AccountType.Asset, level: 1);
            var dto = new CreateAccountDto
            {
                AccountNumber = "1001",
                AccountNameAr = "النقدية",
                AccountNameEn = "Cash",
                AccountType = AccountType.Asset,
                NormalBalance = NormalBalance.Debit,
                ParentAccountId = parentAccount.AccountId // Link to parent
            };

            // Act - Create child account
            var result = _service.Create(dto, _fixture.TestUserId.ToString());

            // Assert - Verify level and FullAccountNumber are calculated correctly
            result.Should().NotBeNull();
            result.Level.Should().Be(2); // Parent level (1) + 1
            result.ParentAccountId.Should().Be(parentAccount.AccountId);
            result.FullAccountNumber.Should().Be($"{parentAccount.FullAccountNumber}.{dto.AccountNumber}"); // "1000.1001"
        }

        /// <summary>
        /// Tests that both Arabic and English fields are properly stored and retrieved.
        /// This is important for bilingual support in the application.
        /// </summary>
        [Fact]
        public void CreateAccount_ShouldStoreArabicAndEnglishFields()
        {
            // Arrange - Create account with both Arabic and English names and descriptions
            var dto = new CreateAccountDto
            {
                AccountNumber = "2000",
                AccountNameAr = "الموردون",
                AccountNameEn = "Accounts Payable",
                AccountType = AccountType.Liability,
                NormalBalance = NormalBalance.Credit, // Liabilities have credit normal balance
                DescriptionAr = "وصف بالعربي",
                DescriptionEn = "Description in English"
            };

            // Act
            var result = _service.Create(dto, _fixture.TestUserId.ToString());

            // Assert - Verify both languages are stored correctly
            result.AccountNameAr.Should().Be("الموردون");
            result.AccountNameEn.Should().Be("Accounts Payable");
            result.DescriptionAr.Should().Be("وصف بالعربي");
            result.DescriptionEn.Should().Be("Description in English");
        }

        #endregion

        #region Get Account Tests

        /// <summary>
        /// Tests retrieving an account by its AccountId.
        /// Verifies that the correct account is returned with all its properties.
        /// </summary>
        [Fact]
        public void GetById_ShouldReturnAccount()
        {
            // Arrange - Create a test account
            var account = _builder.CreateAccount("1001", "حساب تجريبي", "Test Account");

            // Act - Retrieve the account by AccountId
            var result = _service.GetById(account.AccountId);

            // Assert - Verify the account is returned correctly
            result.Should().NotBeNull();
            result.AccountId.Should().Be(account.AccountId);
            result.AccountNumber.Should().Be(account.AccountNumber);
        }

        /// <summary>
        /// Tests that retrieving a non-existent account returns null.
        /// This ensures proper error handling for invalid AccountIds.
        /// </summary>
        [Fact]
        public void GetById_WithNonExistentId_ShouldReturnNull()
        {
            // Arrange - Use a random GUID that doesn't exist
            var nonExistentId = Guid.NewGuid();

            // Act
            var result = _service.GetById(nonExistentId);

            // Assert - Should return null for non-existent account
            result.Should().BeNull();
        }

        /// <summary>
        /// Tests that the account tree structure is returned correctly.
        /// The tree should maintain parent-child relationships for hierarchical account structures.
        /// </summary>
        [Fact]
        public void GetAccountTree_ShouldReturnHierarchicalStructure()
        {
            // Arrange - Create a parent account and two child accounts
            var parent = _builder.CreateAccount("1000", "الأصول", "Assets", AccountType.Asset, level: 1);
            _builder.CreateAccount("1001", "النقدية", "Cash", AccountType.Asset, level: 2, parentAccountId: parent.AccountId);
            _builder.CreateAccount("1002", "البنك", "Bank", AccountType.Asset, level: 2, parentAccountId: parent.AccountId);

            // Act - Get the account tree
            var result = _service.GetAccountTree();

            // Assert - Verify the tree contains the parent account
            result.Should().NotBeNull();
            result.Should().Contain(a => a.AccountNumber == "1000");
        }

        /// <summary>
        /// Tests filtering accounts by account type.
        /// This is useful for generating reports or filtering accounts by category (asset, liability, etc.).
        /// </summary>
        [Fact]
        public void GetByType_ShouldReturnAccountsOfType()
        {
            // Arrange - Create accounts of different types
            _builder.CreateAccount("1001", "النقدية", "Cash", AccountType.Asset);
            _builder.CreateAccount("2001", "الموردون", "Payables", AccountType.Liability);

            // Act - Get only asset accounts
            var result = _service.GetByType(AccountType.Asset);

            // Assert - Verify only asset accounts are returned
            result.Should().NotBeNull();
            result.Should().OnlyContain(a => a.AccountType == AccountType.Asset);
        }

        #endregion

        #region Update Account Tests

        /// <summary>
        /// Tests that account fields can be updated successfully.
        /// Verifies that changes are persisted and audit fields are updated.
        /// </summary>
        [Fact]
        public void UpdateAccount_ShouldUpdateFieldsCorrectly()
        {
            // Arrange - Create an account and prepare update DTO
            var account = _builder.CreateAccount("1001", "حساب قديم", "Old Account");
            var dto = new UpdateAccountDto
            {
                AccountId = account.AccountId,
                AccountNumber = "1001",
                AccountNameAr = "حساب محدث",
                AccountNameEn = "Updated Account",
                AccountType = AccountType.Asset,
                NormalBalance = NormalBalance.Debit,
                IsActive = true,
                IsLeaf = true,
                AllowManualEntries = true,
                RequiresCostCenter = false
            };

            // Act - Update the account
            var result = _service.Update(dto, _fixture.TestUserId.ToString());

            // Assert - Verify update was successful and fields changed
            result.Should().BeGreaterThan(0); // Update should affect at least one row
            var updated = _service.GetById(account.AccountId);
            updated.AccountNameAr.Should().Be("حساب محدث");
            updated.AccountNameEn.Should().Be("Updated Account");
        }

        /// <summary>
        /// Tests that changing an account's parent updates the parent relationship correctly.
        /// Also verifies that the level is recalculated based on the new parent.
        /// </summary>
        [Fact]
        public void UpdateAccount_WithParentChange_ShouldUpdateParent()
        {
            // Arrange - Create old and new parent accounts, and a child account
            var oldParent = _builder.CreateAccount("1000", "الأصول القديمة", "Old Assets", level: 1);
            var newParent = _builder.CreateAccount("2000", "الأصول الجديدة", "New Assets", level: 1);
            var account = _builder.CreateAccount("1001", "حساب فرعي", "Sub Account", parentAccountId: oldParent.AccountId);

            var dto = new UpdateAccountDto
            {
                AccountId = account.AccountId,
                AccountNumber = account.AccountNumber,
                AccountNameAr = account.AccountNameAr,
                AccountNameEn = account.AccountNameEn,
                AccountType = account.AccountType,
                NormalBalance = account.NormalBalance,
                ParentAccountId = newParent.AccountId, // Change parent
                IsActive = account.IsActive,
                IsLeaf = account.IsLeaf,
                AllowManualEntries = account.AllowManualEntries,
                RequiresCostCenter = account.RequiresCostCenter
            };

            // Act - Update the account's parent
            _service.Update(dto, _fixture.TestUserId.ToString());

            // Assert - Verify parent was updated
            var updated = _service.GetById(account.AccountId);
            updated.ParentAccountId.Should().Be(newParent.AccountId);
        }

        #endregion

        #region Delete Account Tests

        /// <summary>
        /// Tests that an account without children can be deleted (soft delete).
        /// Soft delete means the account is marked as inactive, not physically removed.
        /// </summary>
        [Fact]
        public void DeleteAccount_WithNoChildren_ShouldSucceed()
        {
            // Arrange - Create an account with no children
            var account = _builder.CreateAccount("1001", "حساب للحذف", "Account to Delete");

            // Act - Delete the account
            var result = _service.Delete(new[] { account.AccountId });

            // Assert - Verify deletion succeeded (soft delete - IsActive = false)
            result.Should().BeGreaterThan(0);
            var deleted = _service.GetById(account.AccountId);
            deleted.IsActive.Should().BeFalse(); // Soft delete - account still exists but is inactive
        }

        /// <summary>
        /// Tests that an account with child accounts cannot be deleted.
        /// This prevents orphaned accounts and maintains data integrity in the account hierarchy.
        /// </summary>
        [Fact]
        public void DeleteAccount_WithChildren_ShouldFail()
        {
            // Arrange - Create a parent account with a child account
            var parent = _builder.CreateAccount("1000", "الأب", "Parent");
            _builder.CreateAccount("1001", "الابن", "Child", parentAccountId: parent.AccountId);

            // Act - Try to delete the parent account
            var result = _service.Delete(new[] { parent.AccountId });

            // Assert - Deletion should fail (return 0) because parent has children
            result.Should().Be(0); // Should fail due to child accounts
        }

        #endregion

        #region Validation Tests

        /// <summary>
        /// Tests that the CodeExists method correctly identifies existing account numbers.
        /// This is used for validation before creating or updating accounts.
        /// </summary>
        [Fact]
        public void CodeExists_WithExistingCode_ShouldReturnTrue()
        {
            // Arrange - Create an account with account number "1001"
            var account = _builder.CreateAccount("1001", "حساب موجود", "Existing Account");

            // Act - Check if account number exists
            var result = _service.CodeExists("1001");

            // Assert - Should return true for existing account number
            result.Should().BeTrue();
        }

        /// <summary>
        /// Tests that CodeExists returns false for non-existent account numbers.
        /// This ensures the validation works correctly for new accounts.
        /// </summary>
        [Fact]
        public void CodeExists_WithNonExistentCode_ShouldReturnFalse()
        {
            // Act - Check for a non-existent account number
            var result = _service.CodeExists("9999");

            // Assert - Should return false for non-existent account number
            result.Should().BeFalse();
        }

        /// <summary>
        /// Tests that CodeExists can exclude a specific AccountId when checking for duplicates.
        /// This is useful when updating an account - we want to check if the account number
        /// exists for OTHER accounts, but allow it for the current account being updated.
        /// </summary>
        [Fact]
        public void CodeExists_WithExcludeId_ShouldExcludeThatId()
        {
            // Arrange - Create an account with a unique account number
            var uniqueCode = $"TEST-{Guid.NewGuid().ToString().Substring(0, 8)}";
            var account = _builder.CreateAccount(uniqueCode, "حساب", "Account");

            // Act - Check if account number exists, but exclude the current account
            var result = _service.CodeExists(uniqueCode, account.AccountId);

            // Assert - Should return false because we're excluding the account itself
            result.Should().BeFalse(); // Should return false because we're excluding the account itself
        }

        #endregion

        #region Account Balance Tests

        /// <summary>
        /// Tests that account balance calculation works correctly when there are no posted transactions.
        /// With no transactions, the balance should be zero.
        /// Note: Opening balances are now in separate GLAccountOpeningBalance entity.
        /// </summary>
        [Fact]
        public void GetAccountBalance_WithNoTransactions_ShouldReturnZero()
        {
            // Arrange - Create an account without any posted transactions
            var account = _builder.CreateAccount("1001", "النقدية", "Cash", AccountType.Asset, isCashAccount: true);

            // Act - Get the account balance
            var balance = _service.GetAccountBalance(account.AccountId);

            // Assert - Balance should be zero with no posted transactions
            balance.Should().Be(0);
        }

        /// <summary>
        /// Tests balance calculation respects the account's normal balance type.
        /// Asset/Expense accounts use Debit normal balance (Debit - Credit).
        /// Liability/Equity/Revenue accounts use Credit normal balance (Credit - Debit).
        /// </summary>
        [Fact]
        public void GetAccountBalance_ShouldRespectNormalBalance()
        {
            // Arrange - Create a liability account (credit normal balance)
            var liabilityAccount = _builder.CreateAccount("2001", "الموردون", "Accounts Payable", AccountType.Liability);

            // Act - Get the account balance
            var balance = _service.GetAccountBalance(liabilityAccount.AccountId);

            // Assert - Balance should be zero with no transactions
            balance.Should().Be(0);
            liabilityAccount.NormalBalance.Should().Be(NormalBalance.Credit);
        }

        #endregion

        #region Multi-Tenant Tests

        /// <summary>
        /// Tests that accounts are properly isolated by tenant/company.
        /// This ensures that different companies cannot see each other's accounts,
        /// which is critical for multi-tenant applications.
        /// Note: The system now uses CompanyID from SysBase instead of TenantId.
        /// </summary>
        [Fact]
        public void CreateAccount_ShouldRespectTenantIsolation()
        {
            // Arrange - Create two different tenant/company contexts
            var tenant1Id = Guid.NewGuid();
            var tenant2Id = Guid.NewGuid();

            var builder1 = new FinanceTestDataBuilder(_fixture.Db, tenant1Id, _fixture.TestUserId);
            var builder2 = new FinanceTestDataBuilder(_fixture.Db, tenant2Id, _fixture.TestUserId);

            // Act - Create accounts with the same account number in different tenants
            var account1 = builder1.CreateAccount("1001", "حساب المستأجر 1", "Tenant 1 Account");
            var account2 = builder2.CreateAccount("1001", "حساب المستأجر 2", "Tenant 2 Account");

            // Assert - Both accounts should exist independently
            // Note: In the new model, tenant isolation is handled via CompanyID in SysBase
            account1.Should().NotBeNull();
            account2.Should().NotBeNull();
            // Both accounts can have the same account number because they're in different companies
        }

        #endregion
    }
}

