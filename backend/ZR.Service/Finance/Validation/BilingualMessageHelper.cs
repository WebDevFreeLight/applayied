using System;
using Infrastructure.Attribute;
using Infrastructure.Localization;

namespace ZR.Service.Finance.Validation
{
    /// <summary>
    /// Helper class to extract language-specific messages from bilingual strings
    /// محول الرسائل ثنائية اللغة
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class BilingualMessageHelper
    {
        private readonly ILocalizationService _localizationService;

        /// <summary>
        /// Standard separator between Arabic and English in bilingual messages
        /// </summary>
        private const string BILINGUAL_SEPARATOR = " - ";

        public BilingualMessageHelper(ILocalizationService localizationService)
        {
            _localizationService = localizationService;
        }

        /// <summary>
        /// Extract language-specific message from bilingual format
        /// Format: "Arabic Message - English Message"
        /// Returns only Arabic or English based on current language
        /// </summary>
        /// <param name="bilingualMessage">Message in format "Arabic - English"</param>
        /// <returns>Language-specific message</returns>
        public string GetLocalizedMessage(string bilingualMessage)
        {
            if (string.IsNullOrWhiteSpace(bilingualMessage))
                return string.Empty;

            var currentLanguage = _localizationService.GetCurrentLanguage();

            // If message doesn't contain separator, return as-is
            if (!bilingualMessage.Contains(BILINGUAL_SEPARATOR))
                return bilingualMessage;

            // Split by separator
            var parts = bilingualMessage.Split(new[] { BILINGUAL_SEPARATOR }, StringSplitOptions.None);
            if (parts.Length != 2)
                return bilingualMessage; // Malformed, return as-is

            var arabicPart = parts[0].Trim();
            var englishPart = parts[1].Trim();

            // Return appropriate language
            return currentLanguage.ToLower() == "ar" ? arabicPart : englishPart;
        }

        /// <summary>
        /// Create a bilingual message (for use when generating new messages)
        /// </summary>
        /// <param name="arabicMessage">Arabic message</param>
        /// <param name="englishMessage">English message</param>
        /// <returns>Combined bilingual message</returns>
        public static string CreateBilingualMessage(string arabicMessage, string englishMessage)
        {
            return $"{arabicMessage}{BILINGUAL_SEPARATOR}{englishMessage}";
        }
    }
}
