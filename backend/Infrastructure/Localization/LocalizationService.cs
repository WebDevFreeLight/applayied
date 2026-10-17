using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using Microsoft.Extensions.Localization;

namespace Infrastructure.Localization
{
    /// <summary>
    /// Implementation of localization service using .NET resource files
    /// </summary>
    public class LocalizationService : ILocalizationService
    {
        private readonly IStringLocalizer _localizer;
        private static AsyncLocal<string> _currentLanguage = new AsyncLocal<string>();

        public LocalizationService(IStringLocalizer<ExceptionMessages> localizer)
        {
            _localizer = localizer;
        }

        public string GetMessage(string code, string language = null)
        {
            SetCulture(language);

            var localizedString = _localizer[code];

            // Return code itself if not found (fallback)
            return localizedString.ResourceNotFound ? code : localizedString.Value;
        }

        public string GetMessage(string code, Dictionary<string, object> parameters, string language = null)
        {
            var message = GetMessage(code, language);

            if (parameters == null || parameters.Count == 0)
                return message;

            // Replace {ParameterName} with actual values
            foreach (var param in parameters)
            {
                var placeholder = $"{{{param.Key}}}";
                message = message.Replace(placeholder, param.Value?.ToString() ?? "null");
            }

            return message;
        }

        public string GetCurrentLanguage()
        {
            return _currentLanguage.Value ?? CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
        }

        public void SetLanguage(string language)
        {
            if (!string.IsNullOrEmpty(language))
            {
                _currentLanguage.Value = language;
            }
        }

        private void SetCulture(string language)
        {
            var cultureToUse = language ?? GetCurrentLanguage();

            try
            {
                var culture = new CultureInfo(cultureToUse);
                CultureInfo.CurrentCulture = culture;
                CultureInfo.CurrentUICulture = culture;
            }
            catch
            {
                // Fallback to English if culture not found
                CultureInfo.CurrentCulture = new CultureInfo("en");
                CultureInfo.CurrentUICulture = new CultureInfo("en");
            }
        }
    }

    /// <summary>
    /// Dummy class for resource file reference
    /// Actual resources should be in ExceptionMessages.resx
    /// </summary>
    public class ExceptionMessages
    {
    }
}
