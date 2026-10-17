using System.Collections.Generic;

namespace Infrastructure.Localization
{
    /// <summary>
    /// Service for localizing exception messages
    /// </summary>
    public interface ILocalizationService
    {
        /// <summary>
        /// Get localized message for exception code
        /// </summary>
        string GetMessage(string code, string language = null);

        /// <summary>
        /// Get localized message with parameter substitution
        /// Example: "Invoice {InvoiceNumber} not found" with parameters { InvoiceNumber = "INV-001" }
        /// </summary>
        string GetMessage(string code, Dictionary<string, object> parameters, string language = null);

        /// <summary>
        /// Get current user language from context
        /// </summary>
        string GetCurrentLanguage();

        /// <summary>
        /// Set language for current request
        /// </summary>
        void SetLanguage(string language);
    }
}
