using Infrastructure.Attribute;
using Microsoft.AspNetCore.DataProtection;
using System;

namespace ZR.ServiceCore.Security
{
    [AppService(ServiceType = typeof(ISensitiveDataProtector), ServiceLifetime = LifeTime.Singleton)]
    public class SensitiveDataProtector : ISensitiveDataProtector
    {
        private const string Purpose = "ZR.HR.SensitiveData.v1";
        private readonly IDataProtector _protector;

        public SensitiveDataProtector(IDataProtectionProvider provider)
        {
            _protector = (provider ?? throw new ArgumentNullException(nameof(provider))).CreateProtector(Purpose);
        }

        public string Protect(string plaintext)
        {
            if (string.IsNullOrWhiteSpace(plaintext)) return null;
            return _protector.Protect(plaintext);
        }

        public string Unprotect(string protectedData)
        {
            if (string.IsNullOrWhiteSpace(protectedData)) return null;
            return _protector.Unprotect(protectedData);
        }

        public string MaskIban(string iban)
        {
            if (string.IsNullOrWhiteSpace(iban)) return iban;
            var normalized = iban.Replace(" ", "").Trim();
            if (normalized.Length <= 6) return new string('*', normalized.Length);
            return normalized.Substring(0, 2) + new string('*', normalized.Length - 6) + normalized.Substring(normalized.Length - 4);
        }

        public string MaskAccountNumber(string accountNumber)
        {
            if (string.IsNullOrWhiteSpace(accountNumber)) return accountNumber;
            var normalized = accountNumber.Replace(" ", "").Trim();
            if (normalized.Length <= 4) return new string('*', normalized.Length);
            return new string('*', normalized.Length - 4) + normalized.Substring(normalized.Length - 4);
        }
    }
}














