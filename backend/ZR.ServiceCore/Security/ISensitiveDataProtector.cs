namespace ZR.ServiceCore.Security
{
    /// <summary>
    /// Centralized field-level protection for sensitive HR data (bank details, identifiers, etc.).
    /// Uses ASP.NET Core DataProtection under the hood.
    /// </summary>
    public interface ISensitiveDataProtector
    {
        string Protect(string plaintext);
        string Unprotect(string protectedData);

        string MaskIban(string iban);
        string MaskAccountNumber(string accountNumber);
    }
}














