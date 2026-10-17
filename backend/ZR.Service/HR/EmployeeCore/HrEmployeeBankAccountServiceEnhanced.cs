using Infrastructure.Attribute;
using Infrastructure.Exceptions;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZR.Model.HR.EmployeeCore;
using ZR.Model.HR.Dto.EmployeeCore;
using ZR.Service.HR.IService;
using ZR.ServiceCore.Security;

namespace ZR.Service.HR.EmployeeCore
{
    /// <summary>
    /// Enhanced Employee Bank Account Service with encryption and validation (Partial Class Extension)
    /// </summary>
    public partial class HrEmployeeBankAccountService
    {
        private readonly ISensitiveDataProtector _sensitiveDataProtector;

        public HrEmployeeBankAccountService(ISensitiveDataProtector sensitiveDataProtector)
        {
            _sensitiveDataProtector = sensitiveDataProtector;
        }

        #region Create/Update with Encryption

        /// <summary>
        /// Create bank account with encryption and validation
        /// </summary>
        public async Task<long> CreateAsync(CreateEmployeeBankAccountDto dto)
        {
            // Validation
            ValidateBankAccountDto(dto);

            // Ensure employee exists
            var employeeExists = await Context.Queryable<HrEmployeeCore>()
                .AnyAsync(e => e.EmployeeId == dto.EmployeeId);
            if (!employeeExists)
            {
                throw new AppException(ExceptionCode.HR_EMPLOYEE_NOT_FOUND)
                    .WithParameter("EmployeeId", dto.EmployeeId);
            }

            // Validate effective date range
            ValidateEffectiveDateRange(dto.EffectiveFrom, dto.EffectiveTo);

            // If this is being set as primary, unset other primaries
            if (dto.IsPrimary)
            {
                await UnsetOtherPrimariesAsync(dto.EmployeeId, dto.EffectiveFrom);
            }

            var entity = new HrEmployeeBankAccount
            {
                EmployeeId = dto.EmployeeId,
                BankName = dto.BankName,
                CurrencyId = dto.CurrencyId,
                EffectiveFrom = dto.EffectiveFrom,
                EffectiveTo = dto.EffectiveTo,
                IsPrimary = dto.IsPrimary,
                Create_time = DateTime.Now
            };

            // Encrypt sensitive fields
            EncryptBankData(entity, dto.Iban, dto.AccountNumber, dto.BankAccount);

            var result = await Context.Insertable(entity)
                .ExecuteReturnSnowflakeIdAsync();

            return result;
        }

        /// <summary>
        /// Update bank account with encryption and validation
        /// </summary>
        public async Task<int> UpdateAsync(UpdateEmployeeBankAccountDto dto)
        {
            // Validation
            ValidateBankAccountDto(dto);

            // Ensure entity exists
            var existing = await GetByIdAsync(dto.EmployeeBankAccountId);
            if (existing == null)
            {
                throw new AppException(ExceptionCode.NOT_FOUND, "Bank account not found")
                    .WithParameter("EmployeeBankAccountId", dto.EmployeeBankAccountId);
            }

            // Validate effective date range
            ValidateEffectiveDateRange(dto.EffectiveFrom, dto.EffectiveTo);

            // If this is being set as primary, unset other primaries (except this one)
            if (dto.IsPrimary && !existing.IsPrimary)
            {
                await UnsetOtherPrimariesAsync(existing.EmployeeId, dto.EffectiveFrom, dto.EmployeeBankAccountId);
            }

            existing.BankName = dto.BankName;
            existing.CurrencyId = dto.CurrencyId;
            existing.EffectiveFrom = dto.EffectiveFrom;
            existing.EffectiveTo = dto.EffectiveTo;
            existing.IsPrimary = dto.IsPrimary;
            existing.Update_time = DateTime.Now;

            // Encrypt sensitive fields
            EncryptBankData(existing, dto.Iban, dto.AccountNumber, dto.BankAccount);

            return await Context.Updateable(existing).ExecuteCommandAsync();
        }

        #endregion

        #region Encryption/Decryption

        /// <summary>
        /// Encrypt bank data before storage
        /// </summary>
        private void EncryptBankData(HrEmployeeBankAccount entity, string plainIban, string plainAccountNumber, string plainBankAccount)
        {
            if (!string.IsNullOrWhiteSpace(plainIban))
            {
                entity.IbanEncrypted = _sensitiveDataProtector.Protect(plainIban);
            }

            if (!string.IsNullOrWhiteSpace(plainAccountNumber))
            {
                entity.AccountNumberEncrypted = _sensitiveDataProtector.Protect(plainAccountNumber);
            }

            if (!string.IsNullOrWhiteSpace(plainBankAccount))
            {
                entity.BankAccountEncrypted = _sensitiveDataProtector.Protect(plainBankAccount);
            }
        }

        /// <summary>
        /// Get bank account with decrypted data (for authorized viewing)
        /// </summary>
        public async Task<BankAccountDecryptedDto> GetDecryptedAsync(long bankAccountId)
        {
            var entity = await GetByIdAsync(bankAccountId);
            if (entity == null)
            {
                throw new AppException(ExceptionCode.NOT_FOUND, "Bank account not found")
                    .WithParameter("BankAccountId", bankAccountId);
            }

            return new BankAccountDecryptedDto
            {
                EmployeeBankAccountId = entity.EmployeeBankAccountId,
                EmployeeId = entity.EmployeeId,
                BankName = entity.BankName,
                Iban = DecryptIfNotEmpty(entity.IbanEncrypted),
                AccountNumber = DecryptIfNotEmpty(entity.AccountNumberEncrypted),
                BankAccount = DecryptIfNotEmpty(entity.BankAccountEncrypted),
                CurrencyId = entity.CurrencyId,
                EffectiveFrom = entity.EffectiveFrom,
                EffectiveTo = entity.EffectiveTo,
                IsPrimary = entity.IsPrimary
            };
        }

        /// <summary>
        /// Get bank account with masked data (for display purposes)
        /// </summary>
        public async Task<BankAccountMaskedDto> GetMaskedAsync(long bankAccountId)
        {
            var entity = await GetByIdAsync(bankAccountId);
            if (entity == null)
            {
                throw new AppException(ExceptionCode.NOT_FOUND, "Bank account not found")
                    .WithParameter("BankAccountId", bankAccountId);
            }

            var decryptedIban = DecryptIfNotEmpty(entity.IbanEncrypted);
            var decryptedAccountNumber = DecryptIfNotEmpty(entity.AccountNumberEncrypted);

            return new BankAccountMaskedDto
            {
                EmployeeBankAccountId = entity.EmployeeBankAccountId,
                EmployeeId = entity.EmployeeId,
                BankName = entity.BankName,
                IbanMasked = _sensitiveDataProtector.MaskIban(decryptedIban),
                AccountNumberMasked = _sensitiveDataProtector.MaskAccountNumber(decryptedAccountNumber),
                CurrencyId = entity.CurrencyId,
                EffectiveFrom = entity.EffectiveFrom,
                EffectiveTo = entity.EffectiveTo,
                IsPrimary = entity.IsPrimary
            };
        }

        private string DecryptIfNotEmpty(string encryptedValue)
        {
            if (string.IsNullOrWhiteSpace(encryptedValue))
                return null;

            try
            {
                return _sensitiveDataProtector.Unprotect(encryptedValue);
            }
            catch
            {
                return null; // Return null if decryption fails
            }
        }

        #endregion

        #region Business Logic Helpers

        /// <summary>
        /// Get current effective bank account for an employee
        /// </summary>
        public async Task<HrEmployeeBankAccount> GetCurrentEffectiveAccountAsync(long employeeId)
        {
            var now = DateTime.Now;
            return await Context.Queryable<HrEmployeeBankAccount>()
                .Where(b => b.EmployeeId == employeeId)
                .Where(b => b.EffectiveFrom <= now)
                .Where(b => b.EffectiveTo == null || b.EffectiveTo > now)
                .Where(b => b.IsPrimary == true)
                .OrderByDescending(b => b.EffectiveFrom)
                .FirstAsync();
        }

        /// <summary>
        /// Get primary bank account at a specific date
        /// </summary>
        public async Task<HrEmployeeBankAccount> GetPrimaryAccountAtDateAsync(long employeeId, DateTime asOfDate)
        {
            return await Context.Queryable<HrEmployeeBankAccount>()
                .Where(b => b.EmployeeId == employeeId)
                .Where(b => b.EffectiveFrom <= asOfDate)
                .Where(b => b.EffectiveTo == null || b.EffectiveTo > asOfDate)
                .Where(b => b.IsPrimary == true)
                .OrderByDescending(b => b.EffectiveFrom)
                .FirstAsync();
        }

        /// <summary>
        /// Unset other primaries when setting a new primary
        /// </summary>
        private async Task UnsetOtherPrimariesAsync(long employeeId, DateTime effectiveFrom, long? excludeBankAccountId = null)
        {
            var query = Context.Queryable<HrEmployeeBankAccount>()
                .Where(b => b.EmployeeId == employeeId && b.IsPrimary == true);

            if (excludeBankAccountId.HasValue)
            {
                query = query.Where(b => b.EmployeeBankAccountId != excludeBankAccountId.Value);
            }

            var otherPrimaries = await query.ToListAsync();

            foreach (var account in otherPrimaries)
            {
                account.EffectiveTo = effectiveFrom.AddDays(-1);
                account.IsPrimary = false;
                account.Update_time = DateTime.Now;
            }

            if (otherPrimaries.Any())
            {
                await Context.Updateable(otherPrimaries).ExecuteCommandAsync();
            }
        }

        /// <summary>
        /// Validate effective date range
        /// </summary>
        private void ValidateEffectiveDateRange(DateTime effectiveFrom, DateTime? effectiveTo)
        {
            if (effectiveTo.HasValue && effectiveTo.Value <= effectiveFrom)
            {
                throw new AppException(ExceptionCode.INVALID_PARAMETER, "Invalid effective date range")
                    .WithParameter("EffectiveFrom", effectiveFrom)
                    .WithParameter("EffectiveTo", effectiveTo);
            }
        }

        /// <summary>
        /// Validate bank account DTO
        /// </summary>
        private void ValidateBankAccountDto(object dto)
        {
            if (dto is CreateEmployeeBankAccountDto createDto)
            {
                if (createDto.EmployeeId <= 0)
                {
                    throw new AppException(ExceptionCode.INVALID_PARAMETER, "EmployeeId is required");
                }

                if (string.IsNullOrWhiteSpace(createDto.BankName))
                {
                    throw new AppException(ExceptionCode.INVALID_PARAMETER, "Bank name is required");
                }

                // At least one bank identifier must be provided
                if (string.IsNullOrWhiteSpace(createDto.Iban) &&
                    string.IsNullOrWhiteSpace(createDto.AccountNumber) &&
                    string.IsNullOrWhiteSpace(createDto.BankAccount))
                {
                    throw new AppException(ExceptionCode.INVALID_PARAMETER, "Bank account identifier required");
                }

                // Validate IBAN format if provided
                if (!string.IsNullOrWhiteSpace(createDto.Iban) && !IsValidIban(createDto.Iban))
                {
                    throw new AppException(ExceptionCode.INVALID_PARAMETER, "Invalid IBAN format")
                        .WithParameter("Iban", createDto.Iban);
                }
            }
            else if (dto is UpdateEmployeeBankAccountDto updateDto)
            {
                if (updateDto.EmployeeBankAccountId <= 0)
                {
                    throw new AppException(ExceptionCode.INVALID_PARAMETER, "Bank account ID required");
                }

                if (string.IsNullOrWhiteSpace(updateDto.BankName))
                {
                    throw new AppException(ExceptionCode.INVALID_PARAMETER, "Bank name is required");
                }

                // At least one bank identifier must be provided
                if (string.IsNullOrWhiteSpace(updateDto.Iban) &&
                    string.IsNullOrWhiteSpace(updateDto.AccountNumber) &&
                    string.IsNullOrWhiteSpace(updateDto.BankAccount))
                {
                    throw new AppException(ExceptionCode.INVALID_PARAMETER, "Bank account identifier required");
                }

                // Validate IBAN format if provided
                if (!string.IsNullOrWhiteSpace(updateDto.Iban) && !IsValidIban(updateDto.Iban))
                {
                    throw new AppException(ExceptionCode.INVALID_PARAMETER, "Invalid IBAN format")
                        .WithParameter("Iban", updateDto.Iban);
                }
            }
        }

        /// <summary>
        /// Basic IBAN format validation
        /// </summary>
        private bool IsValidIban(string iban)
        {
            if (string.IsNullOrWhiteSpace(iban))
                return false;

            // Remove spaces
            iban = iban.Replace(" ", "");

            // IBAN should be 15-34 characters, start with 2 letters, followed by 2 digits
            if (iban.Length < 15 || iban.Length > 34)
                return false;

            if (!char.IsLetter(iban[0]) || !char.IsLetter(iban[1]))
                return false;

            if (!char.IsDigit(iban[2]) || !char.IsDigit(iban[3]))
                return false;

            // Saudi IBAN is exactly 24 characters and starts with SA
            if (iban.StartsWith("SA") && iban.Length != 24)
                return false;

            return true;
        }

        #endregion
    }

    #region Helper DTOs

    /// <summary>
    /// Bank Account with decrypted data (for authorized viewing)
    /// </summary>
    public class BankAccountDecryptedDto
    {
        public long EmployeeBankAccountId { get; set; }
        public long EmployeeId { get; set; }
        public string BankName { get; set; }
        public string Iban { get; set; }
        public string AccountNumber { get; set; }
        public string BankAccount { get; set; }
        public Guid? CurrencyId { get; set; }
        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public bool IsPrimary { get; set; }
    }

    /// <summary>
    /// Bank Account with masked data (for display purposes)
    /// </summary>
    public class BankAccountMaskedDto
    {
        public long EmployeeBankAccountId { get; set; }
        public long EmployeeId { get; set; }
        public string BankName { get; set; }
        public string IbanMasked { get; set; }
        public string AccountNumberMasked { get; set; }
        public Guid? CurrencyId { get; set; }
        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public bool IsPrimary { get; set; }
    }

    #endregion
}
