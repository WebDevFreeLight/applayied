using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Attribute;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PuppeteerSharp;
using PuppeteerSharp.BrowserData;

namespace ZR.PuppeteerPDF
{
    /// <summary>
    /// Browser provider implementation
    /// </summary>
    [AppService(ServiceType = typeof(IBrowserProvider))]

    public class BrowserProvider : IBrowserProvider
    {
        private readonly IOptionsMonitor<PuppeteerModuleOptions> _optionsMonitor;
        private readonly ILogger<BrowserProvider> _logger;
        private readonly SemaphoreSlim _downloadLock = new(1, 1);
        private bool _browserDownloaded;

        public BrowserProvider(
            IOptionsMonitor<PuppeteerModuleOptions> optionsMonitor,
            ILogger<BrowserProvider> logger)
        {
            _optionsMonitor = optionsMonitor;
            _logger = logger;
        }



        public async Task<IBrowser> GetBrowserAsync()
        {
            await EnsureBrowserDownloadedAsync();

            var options = _optionsMonitor.CurrentValue;

            // Essential arguments for container environment
            var essentialArgs = new HashSet<string>
    {
        "--no-sandbox",
        "--disable-setuid-sandbox",
        "--disable-dev-shm-usage",
        "--disable-gpu",
        "--disable-web-security",
        "--disable-features=VizDisplayCompositor",
        "--disable-background-timer-throttling",
        "--disable-backgrounding-occluded-windows",
        "--disable-renderer-backgrounding"
    };

            // Combine configured args with essential args, removing duplicates
            var configuredArgs = options.Args?.ToList() ?? new List<string>();
            var allArgs = configuredArgs.Union(essentialArgs).ToList();

            var launchOptions = new LaunchOptions
            {
                Headless = options.Headless ,
                Args = allArgs.ToArray(),
                IgnoredDefaultArgs = new[] { "--disable-extensions" }
            };

            if (!string.IsNullOrWhiteSpace(options.ExecutablePath))
            {
                launchOptions.ExecutablePath = options.ExecutablePath;
            }

            return await PuppeteerSharp.Puppeteer.LaunchAsync(launchOptions);
        }


        public async Task<IBrowser> GetBrowserV2Async()
        {
            await EnsureBrowserDownloadedAsync();

            var options = _optionsMonitor.CurrentValue;
            var launchOptions = new LaunchOptions
            {
                Headless = options.Headless,
                Args = options.Args.ToArray()
                //, ExecutablePath = options.ExecutablePath
            };
            if (!string.IsNullOrWhiteSpace(options.ExecutablePath))
            {
                launchOptions.ExecutablePath = options.ExecutablePath;
            }


            var browser = await PuppeteerSharp.Puppeteer.LaunchAsync(launchOptions);
            return browser;
        }

        private async Task EnsureBrowserDownloadedAsync()
        {
            if (_browserDownloaded)
            {
                return;
            }

            await _downloadLock.WaitAsync();
            try
            {
                if (_browserDownloaded)
                {
                    return;
                }

                _logger.LogInformation("Downloading browser...");

                var browserFetcher = new BrowserFetcher();
                await browserFetcher.DownloadAsync();

                _logger.LogInformation("Browser downloaded successfully");
                _browserDownloaded = true;
            }
            finally
            {
                _downloadLock.Release();
            }
        }
    }
}

