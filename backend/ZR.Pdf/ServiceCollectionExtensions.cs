using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ZR.Pdf.Core;
using ZR.Pdf.Pdf;
using ZR.PuppeteerPDF;

namespace ZR.Pdf
{
    /// <summary>
    /// Service collection extensions for PDF rendering
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Add PDF rendering services with Puppeteer
        /// </summary>
        public static IServiceCollection AddPdfRenderer(this IServiceCollection services, IConfiguration configuration)
        {
            // Add Puppeteer services first (if not already added)
            services.AddPuppeteer(configuration);
            // Configure PDF renderer options from configuration
            IConfigurationSection config = configuration.GetSection("PdfRenderer");
            services.Configure<PdfRendererOptions>(config);
            // Register PDF renderer as scoped (new instance per request)
            services.AddScoped<IPdfRenderer, PdfRenderer>();
            return services;
        }

        /// <summary>
        /// Add PDF rendering services with custom options
        /// </summary>
        public static IServiceCollection AddPdfRenderer(this IServiceCollection services, IConfiguration configuration, Action<PdfRendererOptions> configurePdfOptions)
        {
            // Add Puppeteer services first
            services.AddPuppeteer(configuration);

            // Configure PDF renderer options using action
            services.Configure(configurePdfOptions);

            // Register PDF renderer as scoped
            services.AddScoped<IPdfRenderer, PdfRenderer>();

            return services;
        }
    }

    /// <summary>
    /// PDF Renderer configuration options
    /// </summary>
    public class PdfRendererOptions
    {
        /// <summary>
        /// Default page size (A4, Letter, Legal, etc.)
        /// </summary>
        public string DefaultPageSize { get; set; } = "A4";

        /// <summary>
        /// Default page orientation (Portrait, Landscape)
        /// </summary>
        public string DefaultOrientation { get; set; } = "Portrait";

        /// <summary>
        /// Default margin top in pixels
        /// </summary>
        public string DefaultMarginTop { get; set; } = "20px";

        /// <summary>
        /// Default margin bottom in pixels
        /// </summary>
        public string DefaultMarginBottom { get; set; } = "20px";

        /// <summary>
        /// Default margin left in pixels
        /// </summary>
        public string DefaultMarginLeft { get; set; } = "20px";

        /// <summary>
        /// Default margin right in pixels
        /// </summary>
        public string DefaultMarginRight { get; set; } = "20px";

        /// <summary>
        /// Enable printing background graphics
        /// </summary>
        public bool PrintBackground { get; set; } = true;
    }
}

