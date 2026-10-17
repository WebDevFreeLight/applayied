using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace ZR.PuppeteerPDF
{
    /// <summary>
    /// Service collection extensions for Puppeteer
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Add Puppeteer browser automation services
        /// </summary>
        public static IServiceCollection AddPuppeteer(this IServiceCollection services, IConfiguration configuration)
        {
            // Configure Puppeteer options from configuration
            services.Configure<PuppeteerModuleOptions>(configuration.GetSection("Puppeteer"));

            // Register browser provider as singleton (one instance for entire application)
            services.AddSingleton<IBrowserProvider, BrowserProvider>();

            return services;
        }

        /// <summary>
        /// Add Puppeteer browser automation services with custom options
        /// </summary>
        public static IServiceCollection AddPuppeteer(this IServiceCollection services, Action<PuppeteerModuleOptions> configureOptions)
        {
            // Configure Puppeteer options using action
            services.Configure(configureOptions);

            // Register browser provider as singleton
            services.AddSingleton<IBrowserProvider, BrowserProvider>();

            return services;
        }
    }
}

