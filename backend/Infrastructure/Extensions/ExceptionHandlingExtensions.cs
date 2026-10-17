using Infrastructure.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace Infrastructure.Extensions
{
    /// <summary>
    /// Extension methods for configuring exception handling services
    /// </summary>
    public static class ExceptionHandlingExtensions
    {
        /// <summary>
        /// Add exception handling services to DI container
        /// </summary>
        public static IServiceCollection AddExceptionHandling(this IServiceCollection services)
        {
            // Register localization
            services.AddLocalization(options => options.ResourcesPath = "Resources");

            // Register localization service
            services.AddScoped<ILocalizationService, LocalizationService>();
            services.AddScoped<IStringLocalizer<ExceptionMessages>, StringLocalizer<ExceptionMessages>>();

            return services;
        }
    }
}
