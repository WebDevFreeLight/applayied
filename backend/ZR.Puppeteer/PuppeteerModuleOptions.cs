using Newtonsoft.Json;
using PuppeteerSharp;
using System.Collections.Generic;

namespace ZR.PuppeteerPDF
{
    /// <summary>
    /// Configuration options for Puppeteer browser automation
    /// </summary>
    public class PuppeteerModuleOptions  
    {
        /// <summary>
        /// Path to Chrome/Chromium executable. Leave empty for automatic download.
        /// </summary>
        public string? ExecutablePath { get; set; }

        /// <summary>
        /// Run browser in headless mode (no UI)
        /// </summary>
        public bool Headless { get; set; } = true;

        /// <summary>
        /// Browser launch arguments
        /// </summary>
        public List<string> Args { get; set; } = new List<string>
        {
            "--no-sandbox",
            "--disable-setuid-sandbox",
            "--disable-dev-shm-usage",
            "--disable-gpu"
        };

        /// <summary>
        /// Browser revision to download (leave empty for latest)
        /// </summary>
        public string? Revision { get; set; }

        /// <summary>
        /// Accept insecure certificates
        /// </summary>
        public bool AcceptInsecureCerts { get; set; } = false;

        public string? BrowserWsEndpoint { get; set; }
        [JsonIgnore] 
        
        public ViewPortOptions ViewPortOptions { get; set; } = ViewPortOptions.Default;

        public string[] BrowserArgs { get; set; } = { "--no-sandbox" };
        public SupportedBrowser Product { get; set; } = SupportedBrowser.Chrome;

    }
}

