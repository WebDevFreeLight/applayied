// Licensed to the .NET Foundation under one or more agreements.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZR.Pdf.Pdf
{
    using HtmlAgilityPack;
    using Newtonsoft.Json;
    using PdfiumViewer;
    using PuppeteerSharp;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml;
    using ZR.Pdf.Core;
    using ZR.PuppeteerPDF;
    using Formatting = Newtonsoft.Json.Formatting;

    namespace MourjanScraper
    {




       




        public class ScrapingSession
        {
            public DateTime StartTime { get; set; }
            public DateTime EndTime { get; set; }
            public int TotalLinks { get; set; }
            public int ProcessedLinks { get; set; }
            public int SuccessfulExtractions { get; set; }
            public int FailedExtractions { get; set; }
            public List<string> Errors { get; set; } = new List<string>();
            public List<string> ProcessedUrls { get; set; } = new List<string>();
            public ScrapingConfiguration Configuration { get; set; }
        }

        public class ScrapingConfiguration
        {
            public int MaxPages { get; set; } = 10;
            public int DelayBetweenRequestsMs { get; set; } = 5;
            public bool UseHeadlessBrowser { get; set; } = true;
            public string ProxyUrl { get; set; }
            public bool ExportExcel { get; set; } = true;
            public bool ExportJson { get; set; } = true;
            public bool ExportTxt { get; set; } = true;
        }

        public class JobListing
        {
            public string PhoneNumber { get; set; } = "";
            public string Email { get; set; } = "";
            public string City { get; set; } = "";
            public string AdType { get; set; } = "";
            public string JobTitle { get; set; } = "";
            public string Description { get; set; } = "";
            public string PostDate { get; set; } = "";
            public string Url { get; set; } = "";
            public string DataUid { get; set; } = "";
            public string Category { get; set; } = "";
            public bool HasWhatsApp { get; set; } = false;
            public bool HasMobile { get; set; } = false;
            public string ExtractionStatus { get; set; } = "";
            public string CompanyName { get; set; } = "";
            public string Salary { get; set; } = "";
            public string ExperienceLevel { get; set; } = "";
             
        }



      

        
 

        public class EnhancedMourjanScraper : IDisposable
        {
            private readonly HttpClient _httpClient;
            public List<JobListing> _jobListings;
            public ScrapingSession _session;


            private readonly IBrowserProvider browserProvider;
            private readonly IPdfRenderer _pdfRenderer;
            private IBrowser _browser;


            private bool _useBrowser;

            public ScrapingConfiguration Config { get; set; }

            public EnhancedMourjanScraper(IBrowserProvider browserProvider,   IPdfRenderer  pdfRenderer   , ScrapingConfiguration config = null)
            {

                _httpClient = new HttpClient(new HttpClientHandler
                {
                    Proxy = config?.ProxyUrl != null ? new System.Net.WebProxy(config.ProxyUrl) : null,
                    UseProxy = config?.ProxyUrl != null
                });

                _httpClient.DefaultRequestHeaders.Add("User-Agent",
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
                _httpClient.Timeout = TimeSpan.FromSeconds(3);

                _jobListings = new List<JobListing>();
                _session = new ScrapingSession
                {
                    StartTime = DateTime.Now,
                    Configuration = config ?? new ScrapingConfiguration()
                };

                this.browserProvider = browserProvider;
                this._pdfRenderer =  pdfRenderer;
                _useBrowser = _session.Configuration.UseHeadlessBrowser;
            }

            public EnhancedMourjanScraper(IPdfRenderer pdfRenderer, ScrapingConfiguration config)
            {
                Config = config;


                _httpClient = new HttpClient(new HttpClientHandler
                {
                    Proxy = config?.ProxyUrl != null ? new System.Net.WebProxy(config.ProxyUrl) : null,
                    UseProxy = config?.ProxyUrl != null
                });

                _jobListings = new List<JobListing>();
                _session = new ScrapingSession
                {
                    StartTime = DateTime.Now,
                    Configuration = config ?? new ScrapingConfiguration()
                };

                _useBrowser = _session.Configuration.UseHeadlessBrowser;
                this._pdfRenderer = pdfRenderer;








            }

            public async Task InitializeBrowserAsync()
            {
                if (!_useBrowser) return;



                _browser = await browserProvider.GetBrowserAsync();

            }
















































            public async Task<List<JobListing>> ScrapeJobsAsync(string baseUrl, int maxPages = 20, CancellationToken cancellationToken = default)
            {
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    Console.WriteLine("?? Starting Mourjan job scraping session...");
                    Console.WriteLine($"? Start time: {_session.StartTime:yyyy-MM-dd HH:mm:ss}");
                    Console.WriteLine($"?? Configuration: {JsonConvert.SerializeObject(_session.Configuration, Formatting.Indented)}");

                    _jobListings = new List<JobListing>();
                    await InitializeBrowserAsync();

                    // Phase 1: Collect all job links
                    Console.WriteLine("\n?? Phase 1: Collecting job listings");
                    Console.WriteLine(new string('-', 50));

                    var jobLinks = await CollectJobLinksAsync(baseUrl, maxPages, cancellationToken);
                    // Replace the problematic line:

                    // With the following corrected line:
                    //Console.WriteLine($"\n??   collected links: {string.Join(",", jobLinks)}");

                    _session.TotalLinks = jobLinks.Count;
                    Console.WriteLine($"\n?? Total collected links: {jobLinks.Count}");

                    // Phase 2: Extract detailed information
                    Console.WriteLine("\n?? Phase 2: Extracting job details");
                    Console.WriteLine(new string('-', 50));

                    await ProcessJobListingsAsync(jobLinks, cancellationToken);

                    _session.EndTime = DateTime.Now;
                    DisplaySessionSummary();

                    return _jobListings;
                }
                catch (OperationCanceledException ex)
                {
                    Console.WriteLine("?? Scraping was canceled by user.");
                    _session.EndTime = DateTime.Now;
                    DisplaySessionSummary();
                    LogError(ex);



                    return _jobListings;
                }
            }


            private async Task<List<string>> CollectJobLinksAsync(string baseUrl, int maxPages, CancellationToken cancellationToken = default)
            {
                var jobLinks = new List<string>();
                string currentUrl = baseUrl;
                int page = 1;
                bool hasMorePages = true;

                //var maxPages = _session.Configuration.MaxPages;
                while (page <= maxPages && hasMorePages)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    try
                    {
                        Console.Write($"?? Page {page}... ");

                        string html = _useBrowser ?
                            await GetPageContentWithBrowserAsync(currentUrl, cancellationToken) :
                            await _httpClient.GetStringAsync(currentUrl, cancellationToken);

                        var doc = new HtmlDocument();
                        doc.LoadHtml(html);

                        // Extract job links
                        var jobLinkNodes = doc.DocumentNode.SelectNodes("//a[@class='link' and contains(@href, '/vacancies/')]");
                        int linksOnPage = 0;

                        if (jobLinkNodes != null)
                        {
                            foreach (var linkNode in jobLinkNodes)
                            {
                                cancellationToken.ThrowIfCancellationRequested();
                                string href = linkNode.GetAttributeValue("href", "");
                                if (!string.IsNullOrEmpty(href))
                                {

                                    Console.WriteLine($"?? Found link: {href}");

                                    string fullUrl = href.StartsWith("http") ? href : $"https://www.mourjan.com{href}";
                                    if (!jobLinks.Contains(fullUrl))
                                    {
                                        jobLinks.Add(fullUrl);
                                        linksOnPage++;
                                    }
                                }
                            }
                        }

                        Console.WriteLine($"? {linksOnPage} links found");

                        // Handle pagination
                        var pgnUl = doc.DocumentNode.SelectSingleNode("//ul[@class='pgn']");
                        string nextPageUrl = null;

                        if (pgnUl != null)
                        {
                            // Find the next page URL from the navigation arrow
                            var nextPageNode = pgnUl.SelectSingleNode("./li/a[@class='nav' and contains(text(),'?')]");
                            nextPageUrl = nextPageNode?.GetAttributeValue("href", null);

                            // If no next arrow, try to find the next numbered page
                            if (string.IsNullOrEmpty(nextPageUrl))
                            {
                                var activeItem = pgnUl.SelectSingleNode("./li[@class='active']");
                                if (activeItem != null)
                                {
                                    int activeIndex = pgnUl.SelectNodes("./li").IndexOf(activeItem);
                                    var nextItem = pgnUl.SelectNodes("./li")[activeIndex + 1];
                                    nextPageUrl = nextItem?.SelectSingleNode("./a")?.GetAttributeValue("href", null);
                                }
                            }
                        }

                        // Determine if we should continue to next page
                        if (string.IsNullOrEmpty(nextPageUrl))
                        {
                            Console.WriteLine("?? Last page reached");
                            hasMorePages = false;
                        }
                        else
                        {
                            currentUrl = nextPageUrl.StartsWith("http") ? nextPageUrl : $"https://www.mourjan.com{nextPageUrl}";
                            page++;
                            await Task.Delay(_session.Configuration.DelayBetweenRequestsMs, cancellationToken);
                        }
                    }
                    catch (OperationCanceledException ex)
                    {
                        LogError(ex);
                        throw;
                    }
                    catch (Exception ex)
                    {
                        LogError(ex);
                        _session.Errors.Add($"Page {page}: {ex.Message}");
                        Console.WriteLine($"? Error: {ex.Message}");
                        break;
                    }
                }

                return jobLinks;
            }

     
            private async Task ProcessJobListingsAsync(List<string> jobLinks, CancellationToken cancellationToken = default)
            {
                int processedCount = 0;
                int successCount = 0;
                int failedCount = 0;

                foreach (var jobUrl in jobLinks)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    try
                    {
                        processedCount++;
                        Console.Write($"?? {processedCount}/{jobLinks.Count} - Processing: {jobUrl}  ");

                        var jobListing = _useBrowser ?
                            await ExtractJobDetailsWithBrowserAsync(jobUrl, cancellationToken) :
                            await ExtractJobDetailsAsync(jobUrl, cancellationToken);

                        if (jobListing != null)
                        {
                            _jobListings.Add(jobListing);
                            successCount++;

                            // Show extraction summary
                            var indicators = new List<string>();
                            if (!string.IsNullOrEmpty(jobListing.PhoneNumber)) indicators.Add("??");
                            if (!string.IsNullOrEmpty(jobListing.Email)) indicators.Add("??");
                            if (!string.IsNullOrEmpty(jobListing.City)) indicators.Add("???");
                            if (!string.IsNullOrEmpty(jobListing.JobTitle)) indicators.Add("??");

                            Console.WriteLine($"? {string.Join(" ", indicators)}");
                        }
                        else
                        {
                            failedCount++;
                            Console.WriteLine("? Failed to extract");
                        }

                        _session.ProcessedUrls.Add(jobUrl);
                        _session.ProcessedLinks = processedCount;
                        _session.SuccessfulExtractions = successCount;
                        _session.FailedExtractions = failedCount;

                        // Progress update
                        if (processedCount % 10 == 0 || processedCount == jobLinks.Count)
                        {
                            Console.WriteLine($"?? Progress: {processedCount}/{jobLinks.Count} - Success: {successCount} - Failed: {failedCount}");
                        }

                        await Task.Delay(_session.Configuration.DelayBetweenRequestsMs, cancellationToken);
                    }
                    catch (OperationCanceledException ex)
                    {


                        LogError(ex);
                        throw;
                    }
                    catch (Exception ex)
                    {
                        failedCount++;


                        LogError(ex);
                        _session.Errors.Add($"Job {processedCount}: {ex.Message}");
                        Console.WriteLine($"? Error: {ex.Message}");
                    }
                }
            }

            private async Task<JobListing> ExtractJobDetailsAsync(string jobUrl, CancellationToken cancellationToken = default)
            {
                try
                {

                    string html = await _httpClient.GetStringAsync(jobUrl, cancellationToken);
                    var doc = new HtmlDocument();
                    doc.LoadHtml(html);

                    var jobListing = new JobListing { Url = jobUrl };

                    // Extract basic info
                    var adDiv = doc.DocumentNode.SelectSingleNode("//div[@class='ad p']");
                    if (adDiv != null)
                    {
                        jobListing.DataUid = adDiv.GetAttributeValue("data-uid", "");
                    }



              
                    // Extract from content div
                    var contentDiv = doc.DocumentNode.SelectSingleNode("//div[@class='fdc cntnt']");
                    if (contentDiv != null)
                    {
                        // Job title and category
                        var infoNode = contentDiv.SelectSingleNode(".//p[@class='info']");
                        if (infoNode != null)
                        {
                            jobListing.JobTitle = CleanText(infoNode.InnerText);
                            jobListing.Category = jobListing.JobTitle;
                        }

                        // Description and embedded info
                        var descriptionNode = contentDiv.SelectSingleNode(".//div[@class='txt fdc']//p[@class='ar']");
                        if (descriptionNode != null)
                        {
                            jobListing.Description = CleanText(descriptionNode.InnerText);
                            ExtractInfoFromDescription(jobListing, jobListing.Description);
                        }

                        // Contact information
                        var contactsDiv = contentDiv.SelectSingleNode(".//div[@class='contacts']");
                        if (contactsDiv != null)
                        {
                            await ExtractContactInfoAsync(contactsDiv, jobListing);
                        }

                        // Additional details
                        ExtractAdditionalDetails(contentDiv, jobListing);
                    }

                    // Set defaults and status
                    SetDefaultValues(jobListing);
                    SetExtractionStatus(jobListing);

                    return jobListing;
                }
                catch (OperationCanceledException ex)
                {


                    LogError(ex);
                    throw;
                }
                catch (Exception ex)
                {


                    LogError(ex);
                    Console.WriteLine($"?? Error processing {jobUrl}: {ex.Message}");
                    return null;
                }
            }

            private static void CloseOutlookProcesses()
            {
                try
                {

                    ProcessManager.CloseOutlookProcesses();


                    return;
                    //var outlookProcesses = Process.GetProcessesByName("OUTLOOK");


                    var allProcesses = Process.GetProcesses();
                    var outlookProcesses = allProcesses
                        .Where(p => p.ProcessName.Contains("OUTLOOK"))
                        .ToArray();








                    if (outlookProcesses.Length > 0)
                    {
                        Console.WriteLine("?? Microsoft Outlook is running - attempting to close...");
                        foreach (var process in outlookProcesses)
                        {
                            try
                            {
                                process.CloseMainWindow();
                                if (!process.WaitForExit(15)) // Wait 5 seconds to close gracefully
                                {
                                    process.Kill();
                                    Console.WriteLine("? Force-closed Outlook process");
                                }
                                else
                                {
                                    Console.WriteLine("? Closed Outlook gracefully");
                                }
                            }
                            catch (Exception ex)
                            {

                                Console.WriteLine($"?? Error closing Outlook: {ex.Message}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {

                    Console.WriteLine($"?? Error checking for Outlook: {ex.Message}");
                }
            }

            private async Task<JobListing> ExtractJobDetailsWithBrowserAsync(string jobUrl, CancellationToken cancellationToken = default)
            {
                var jobListing = new JobListing { Url = jobUrl };

                try
                {

                    await using var _browser = await browserProvider.GetBrowserAsync();
                    using var page = await _browser.NewPageAsync();
                    //await page.GoToAsync(jobUrl, WaitUntilNavigation.DOMContentLoaded, cancellationToken: cancellationToken);
                    await page.GoToAsync(jobUrl, WaitUntilNavigation.DOMContentLoaded);


                    // Extract basic info
                    jobListing.DataUid = await page.EvaluateExpressionAsync<string>(
                        "document.querySelector('div.ad.p')?.getAttribute('data-uid') ?? ''");


                    // Extract ID from URL (last segment)
                    if (Uri.TryCreate(jobUrl, UriKind.Absolute, out var uri))
                    {
                        string[] segments = uri.Segments; // e.g., ["/", "jobs/", "12345"]
                        if (segments.Length > 0)
                        {
                            string lastSegment = segments.Last().Trim('/');
                            if (!string.IsNullOrEmpty(lastSegment))
                            {
                                jobListing.DataUid = lastSegment; // or parse as int if needed
                            }
                        }

                    }


                    if (string.IsNullOrEmpty(jobListing.DataUid))
                    {
                        // Format: yy_mm_dd_HH_mm_ss_fff (year_month_day_hour_minute_second_milliseconds)
                        jobListing.DataUid = DateTime.Now.ToString("yy_MM_dd_HH_mm_ss_fff");
                    }

                    // Job title and category
                    jobListing.JobTitle = await page.EvaluateExpressionAsync<string>(
                        "document.querySelector('p.info')?.innerText.trim() ?? ''");
                    jobListing.Category = jobListing.JobTitle;

                    // Description and embedded info
                    jobListing.Description = await page.EvaluateExpressionAsync<string>(
                        "document.querySelector('div.txt.fdc p.ar')?.innerText.trim() ?? ''");
                    ExtractInfoFromDescription(jobListing, jobListing.Description);

                    // Contact information
                    var contactsDiv = await page.QuerySelectorAsync("div.contacts");
                    if (contactsDiv != null)
                    {
                        //var contactsDiv = await page.QuerySelectorAsync("div.contacts");
                        await ExtractContactInfoWithBrowserAsync(contactsDiv, jobListing,page);
                    }

                    // Additional details
                    jobListing.CompanyName = await page.EvaluateExpressionAsync<string>(
                        "Array.from(document.querySelectorAll('div.txt.fdc p')) .find(p => p.innerText.includes('Company:'))?.innerText.replace('Company:', '').trim() ?? ''");


                    jobListing.Salary = await page.EvaluateExpressionAsync<string>(
                        "Array.from(document.querySelectorAll('div.txt.fdc p')).find(p => p.innerText.includes('Salary:'))?.innerText.replace('Salary:', '').trim() ?? ''");

                    // Set defaults and status
                    SetDefaultValues(jobListing);
                    SetExtractionStatus(jobListing);


                    CloseOutlookProcesses();

                    return jobListing;
                }
                catch (OperationCanceledException ex)
                {


                    LogError(ex);
                    throw;
                }
                catch (Exception ex)
                {
                    if (jobListing != null)
                    {
                        return jobListing;

                    }

                    LogError(ex);
                    Console.WriteLine($"?? Error processing {jobUrl} with browser: {ex.Message}");
                    return null;
                }
            }

            private async Task<string> GetPageContentWithBrowserAsync(string url, CancellationToken cancellationToken = default)
            {
                using var page = await _browser.NewPageAsync();
                //await page.GoToAsync(url, WaitUntilNavigation.DOMContentLoaded, cancellationToken: cancellationToken);
                await page.GoToAsync(url, WaitUntilNavigation.DOMContentLoaded);
                return await page.GetContentAsync();
            }

            private void ExtractInfoFromDescription(JobListing job, string description)
            {
                if (string.IsNullOrEmpty(description)) return;

                // Phone and email
                if (string.IsNullOrEmpty(job.PhoneNumber))
                {
                    job.PhoneNumber = ExtractPhoneNumber(description);
                }
                job.Email = ExtractEmail(description);

                // Location and type
                job.City = ExtractCity(description);
                job.AdType = ExtractAdType(description);

                // Additional fields
                job.CompanyName = ExtractCompanyName(description);
                job.Salary = ExtractSalary(description);
                job.ExperienceLevel = ExtractExperienceLevel(description);
            }

            private async Task ExtractContactInfoAsync(HtmlNode contactsDiv, JobListing job)
            {
                var whatsAppBtn = contactsDiv.SelectSingleNode(".//a[@class='btn whats']");
                var mobileBtn = contactsDiv.SelectSingleNode(".//a[@class='btn mobile']");
                var mailBtn = contactsDiv.SelectSingleNode(".//a[@class='btn mail']");


                var mobwBtn = contactsDiv.SelectSingleNode(".//a[@class='btn mobw']"); // New mobile with WhatsApp button



                if (whatsAppBtn != null)
                {
                    job.HasWhatsApp = true;
                }
                if (mobileBtn != null)
                {
                    job.HasMobile = true;
                }

                if (mobwBtn != null)
                {
                    job.HasMobile = job.HasWhatsApp = true; // This button provides both
                }


                // Try batch decoding first
                //var batchDecodeSuccess = await TryBatchDecodeContactsAsync(whatsAppBtn, mobileBtn, mailBtn, job);

                var batchDecodeSuccess = await TryBatchDecodeContactsAsync(whatsAppBtn, mobileBtn, mobwBtn, mailBtn, job);

                if (!batchDecodeSuccess)
                {
                    // Fallback to individual decoding
                    if (string.IsNullOrEmpty(job.PhoneNumber))
                    {
                        if (whatsAppBtn != null)
                        {

                            await DecodeContactButtonAsync(whatsAppBtn, job);
                        }
                        if (mobileBtn != null && string.IsNullOrEmpty(job.PhoneNumber))
                        {
                            await DecodeContactButtonAsync(mobileBtn, job);
                        }


                    }



                    if (string.IsNullOrEmpty(job.Email) && mailBtn != null)
                    {
                        await DecodeContactButtonAsync(mailBtn, job);
                    }










                }

                // Final fallback to direct href extraction
                if (whatsAppBtn != null && string.IsNullOrEmpty(job.PhoneNumber))
                {
                    ExtractPhoneFromHref(whatsAppBtn, job);
                }
            }

            private async Task ExtractContactInfoWithBrowserAsync(IElementHandle contactsDiv, JobListing job, IPage page )
            {
                var whatsAppBtn = await contactsDiv.QuerySelectorAsync("a.btn.whats");
                var mobileBtn = await contactsDiv.QuerySelectorAsync("a.btn.mobile");
                var mailBtn = await contactsDiv.QuerySelectorAsync("a.btn.mail");
                var mobwBtn = await contactsDiv.QuerySelectorAsync("a.btn.mobw");

                // Extract publication date from parent div.fdc.cntnt
                //var page =   contactsDiv;
                var dateElement = await page.QuerySelectorAsync("div.fdc.cntnt div.social.sp-between div.fw-300 span.en");
                if (dateElement != null)
                {
                    string v = await dateElement.EvaluateFunctionAsync<string>("e => e.textContent");
                    job.PostDate = v?.Trim();
                }

                // Check if contact buttons exist
                bool hasContactButtons = whatsAppBtn != null || mobileBtn != null || mailBtn != null || mobwBtn != null;

                // If no contact buttons found, try clicking the "??????? ?? ??????" button to reveal modal
                if (!hasContactButtons)
                {
                    try
                    {
                        // Find the contact trigger button - it's the next sibling after div.contacts
                        IElementHandle contactTriggerBtn = null;

                        // First try to find using JavaScript evaluation to get the next sibling
                        try
                        {
                            contactTriggerBtn = await page.QuerySelectorAsync("div.contacts + a.btn");
                        }
                        catch  (Exception ex){ 
                        
                            Console.WriteLine(ex.Message);
                        
                        }

                        // Fallback to finding by onclick attribute
                        if (contactTriggerBtn == null)
                        {
                            contactTriggerBtn = await page.QuerySelectorAsync("a.btn[onclick*='sac']");
                        }

                        if (contactTriggerBtn != null)
                        {
                            // Click the button to open the modal
                            await contactTriggerBtn.ClickAsync();
                            await Task.Delay(3500); // Wait for modal to appear

                            // Wait for SweetAlert2 modal to appear
                            var modalContainer = await page.QuerySelectorAsync("div.swal2-html-container");
                            if (modalContainer != null)
                            {
                                // Extract all list items from the modal
                                var listItems = await modalContainer.QuerySelectorAllAsync("ul.fdc > li");

                                foreach (var listItem in listItems)
                                {
                                    // Check if this is a WhatsApp item
                                    var whatsappIcon = await listItem.QuerySelectorAsync("i.icn-whatsapp");
                                    if (whatsappIcon != null)
                                    {
                                        job.HasWhatsApp = true;
                                        job.HasMobile = true;

                                        // Extract phone from href
                                        var link = await listItem.QuerySelectorAsync("a[href*='wa.me']");
                                        if (link != null)
                                        {
                                            var href = await link.EvaluateFunctionAsync<string>("el => el.getAttribute('href')");
                                            var match = Regex.Match(href ?? "", @"wa\.me/(\d+)");
                                            if (match.Success)
                                            {
                                                job.PhoneNumber = FormatSaudiPhone(match.Groups[1].Value);
                                            }

                                            // Also try to get from the span with direction:ltr
                                            var phoneSpan = await link.QuerySelectorAsync("span[style*='direction:ltr']");
                                            if (phoneSpan != null && string.IsNullOrEmpty(job.PhoneNumber))
                                            {
                                                var phoneText = await phoneSpan.EvaluateFunctionAsync<string>("el => el.textContent");
                                                job.PhoneNumber = FormatSaudiPhone(phoneText?.Replace(" ", ""));
                                            }
                                        }
                                    }

                                    // Check if this is an email item
                                    var emailIcon = await listItem.QuerySelectorAsync("i.icn-email");
                                    if (emailIcon != null)
                                    {
                                        // Extract email from href
                                        var link = await listItem.QuerySelectorAsync("a[href*='mailto:']");
                                        if (link != null)
                                        {
                                            var href = await link.EvaluateFunctionAsync<string>("el => el.getAttribute('href')");
                                            job.Email = href?.Replace("mailto:", "")?.Trim();

                                            // Also try to get from the span with direction:ltr as fallback
                                            if (string.IsNullOrEmpty(job.Email))
                                            {
                                                var emailSpan = await link.QuerySelectorAsync("span[style*='direction:ltr']");
                                                if (emailSpan != null)
                                                {
                                                    var emailText = await emailSpan.EvaluateFunctionAsync<string>("el => el.textContent");
                                                    job.Email = emailText?.Trim();
                                                }
                                            }
                                        }
                                    }

                                    // Check for mobile/phone icon (if exists separately from WhatsApp)
                                    var mobileIcon = await listItem.QuerySelectorAsync("i.icn-mobile, i.icn-phone");
                                    if (mobileIcon != null && string.IsNullOrEmpty(job.PhoneNumber))
                                    {
                                        job.HasMobile = true;

                                        var link = await listItem.QuerySelectorAsync("a[href*='tel:']");
                                        if (link != null)
                                        {
                                            var href = await link.EvaluateFunctionAsync<string>("el => el.getAttribute('href')");
                                            var match = Regex.Match(href ?? "", @"tel:(\d+)");
                                            if (match.Success)
                                            {
                                                job.PhoneNumber = FormatSaudiPhone(match.Groups[1].Value);
                                            }
                                        }
                                    }
                                }
                            }

                            // Close modal by pressing Escape
                            await page.Keyboard.PressAsync("Escape");
                            await Task.Delay(300);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogError(ex);
                        Console.WriteLine($"?? Error extracting from modal: {ex.Message}");
                    }
                }
                else
                {
                    // Use existing button extraction logic
                    if (whatsAppBtn != null)
                    {
                        job.HasWhatsApp = true;
                        job.PhoneNumber = await ExtractPhoneFromBrowserButtonAsync(whatsAppBtn);
                    }

                    if (mobwBtn != null)
                    {
                        job.HasWhatsApp = true;
                        job.HasMobile = true;
                        job.PhoneNumber = await ExtractPhoneFromBrowserButtonAsync(mobwBtn);
                    }

                    if (mobileBtn != null && string.IsNullOrEmpty(job.PhoneNumber))
                    {
                        job.HasMobile = true;
                        job.PhoneNumber = await ExtractPhoneFromBrowserButtonAsync(mobileBtn);
                    }

                    if (mailBtn != null && string.IsNullOrEmpty(job.Email))
                    {
                        job.Email = await ExtractEmailFromBrowserButtonAsync(mailBtn);
                    }
                }
            }

            private async Task<string> ExtractPhoneFromBrowserButtonAsync(IElementHandle button)
            {
                try
                {
                    // Try to click and get the revealed number
                    await button.ClickAsync();
                    await Task.Delay(500); // Wait for animation/display

                    var phone = await button.EvaluateFunctionAsync<string>("el => el.innerText.trim()");
                    return FormatSaudiPhone(phone);
                }
                catch
                {
                    // Fallback to href extraction
                    var href = await button.EvaluateFunctionAsync<string>("el => el.getAttribute('href')");
                    var match = Regex.Match(href ?? "", @"(?:tel:|wa\.me/)([0-9]+)");
                    return match.Success ? FormatSaudiPhone(match.Groups[1].Value) : "";
                }
            }

            private async Task<string> ExtractEmailFromBrowserButtonAsync(IElementHandle button)
            {
                try
                {
                    // Try to click and get the revealed email
                    await button.ClickAsync();
                    await Task.Delay(150); // Wait for animation/display

                    return await button.EvaluateFunctionAsync<string>("el => el.innerText.trim()");
                }
                catch
                {
                    // Fallback to href extraction
                    var href = await button.EvaluateFunctionAsync<string>("el => el.getAttribute('href')");
                    return href?.Replace("mailto:", "") ?? "";
                }
            }






            private async Task<bool> TryBatchDecodeContactsAsync(HtmlNode whatsAppBtn, HtmlNode mobileBtn, HtmlNode mobwBtn, HtmlNode mailBtn, JobListing job)
            {
                try
                {
                    var phoneData = new List<string>();
                    var emailData = new List<string>();

                    if (whatsAppBtn != null)
                    {
                        string dataE = whatsAppBtn.GetAttributeValue("data-e", "");
                        if (!string.IsNullOrEmpty(dataE)) phoneData.Add(dataE);
                    }

                    if (mobileBtn != null)
                    {
                        string dataE = mobileBtn.GetAttributeValue("data-e", "");
                        if (!string.IsNullOrEmpty(dataE)) phoneData.Add(dataE);
                    }

                    if (mobwBtn != null) // Add data from the new mobile with WhatsApp button
                    {
                        string dataE = mobwBtn.GetAttributeValue("data-e", "");
                        if (!string.IsNullOrEmpty(dataE)) phoneData.Add(dataE);
                    }

                    if (mailBtn != null)
                    {
                        string dataE = mailBtn.GetAttributeValue("data-e", "");
                        if (!string.IsNullOrEmpty(dataE)) emailData.Add(dataE);
                    }

                    if (!phoneData.Any() && !emailData.Any()) return false;

                    var requestData = new Dictionary<string, string>
        {
            { "newlook", "1" },
            { "s", "1" }
        };

                    if (phoneData.Any()) requestData.Add("n", string.Join(" ", phoneData));
                    if (emailData.Any()) requestData.Add("e", string.Join(" ", emailData));

                    var response = await _httpClient.PostAsync(
                        "https://www.mourjan.com/ajax-decode/",
                        new FormUrlEncodedContent(requestData));

                    if (response.IsSuccessStatusCode)
                    {
                        var jsonResponse = await response.Content.ReadAsStringAsync();
                        var decodeResult = JsonConvert.DeserializeObject<DecodeResponse>(jsonResponse);

                        if (decodeResult?.Success == 1 && decodeResult.Result != null)
                        {
                            foreach (var result in decodeResult.Result)
                            {
                                if (string.IsNullOrEmpty(job.PhoneNumber) && IsPhoneNumber(result))
                                {
                                    job.PhoneNumber = FormatSaudiPhone(result);
                                }
                                else if (string.IsNullOrEmpty(job.Email) && IsEmail(result))
                                {
                                    job.Email = result;
                                }
                            }
                            return true;
                        }
                    }
                }
                catch (Exception ex)
                {



                    LogError(ex);
                    Console.WriteLine($"?? Batch decode error: {ex.Message}");
                }
                return false;
            }





            private async Task<bool> TryBatchDecodeContactsAsync(HtmlNode whatsAppBtn, HtmlNode mobileBtn, HtmlNode mailBtn, JobListing job)
            {
                try
                {
                    var phoneData = new List<string>();
                    var emailData = new List<string>();

                    if (whatsAppBtn != null)
                    {
                        string dataE = whatsAppBtn.GetAttributeValue("data-e", "");
                        if (!string.IsNullOrEmpty(dataE)) phoneData.Add(dataE);
                    }

                    if (mobileBtn != null)
                    {
                        string dataE = mobileBtn.GetAttributeValue("data-e", "");
                        if (!string.IsNullOrEmpty(dataE)) phoneData.Add(dataE);
                    }

                    if (mailBtn != null)
                    {
                        string dataE = mailBtn.GetAttributeValue("data-e", "");
                        if (!string.IsNullOrEmpty(dataE)) emailData.Add(dataE);
                    }

                    if (!phoneData.Any() && !emailData.Any()) return false;

                    var requestData = new Dictionary<string, string>
                {
                    { "newlook", "1" },
                    { "s", "1" }
                };

                    if (phoneData.Any()) requestData.Add("n", string.Join(" ", phoneData));
                    if (emailData.Any()) requestData.Add("e", string.Join(" ", emailData));

                    var response = await _httpClient.PostAsync(
                        "https://www.mourjan.com/ajax-decode/",
                        new FormUrlEncodedContent(requestData));

                    if (response.IsSuccessStatusCode)
                    {
                        var jsonResponse = await response.Content.ReadAsStringAsync();
                        var decodeResult = JsonConvert.DeserializeObject<DecodeResponse>(jsonResponse);

                        if (decodeResult?.Success == 1 && decodeResult.Result != null)
                        {
                            foreach (var result in decodeResult.Result)
                            {
                                if (string.IsNullOrEmpty(job.PhoneNumber) && IsPhoneNumber(result))
                                {
                                    job.PhoneNumber = FormatSaudiPhone(result);
                                }
                                else if (string.IsNullOrEmpty(job.Email) && IsEmail(result))
                                {
                                    job.Email = result;
                                }
                            }
                            return true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"?? Batch decode error: {ex.Message}");
                }
                return false;
            }

            private async Task DecodeContactButtonAsync(HtmlNode button, JobListing job)
            {
                try
                {
                    string dataE = button.GetAttributeValue("data-e", "");
                    string dataT = button.GetAttributeValue("data-t", "");

                    if (string.IsNullOrEmpty(dataE)) return;

                    var requestData = new Dictionary<string, string>
                {
                    { "newlook", "1" },
                    { "s", "1" }
                };

                    bool isEmail = button.GetAttributeValue("class", "").Contains("mail");
                    requestData[isEmail ? "e" : "n"] = dataE;
                    if (!isEmail && !string.IsNullOrEmpty(dataT)) requestData["t"] = dataT;

                    var response = await _httpClient.PostAsync(
                        "https://www.mourjan.com/ajax-decode/",
                        new FormUrlEncodedContent(requestData));

                    if (response.IsSuccessStatusCode)
                    {
                        var jsonResponse = await response.Content.ReadAsStringAsync();
                        var decodeResult = JsonConvert.DeserializeObject<DecodeResponse>(jsonResponse);

                        if (decodeResult?.Success == 1 && decodeResult.Result?.Count > 0)
                        {
                            string result = decodeResult.Result[0];
                            if (isEmail)
                            {
                                if (IsEmail(result)) job.Email = result;
                            }
                            else
                            {
                                if (IsPhoneNumber(result)) job.PhoneNumber = FormatSaudiPhone(result);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {


                    LogError(ex);
                    Console.WriteLine($"?? Button decode error: {ex.Message}");
                }
            }

            private void ExtractPhoneFromHref(HtmlNode button, JobListing job)
            {
                string href = button.GetAttributeValue("href", "");
                if (!string.IsNullOrEmpty(href))
                {
                    var match = Regex.Match(href, @"(?:tel:|wa\.me/)([0-9]+)");
                    if (match.Success)
                    {
                        job.PhoneNumber = FormatSaudiPhone(match.Groups[1].Value);
                    }
                }
            }

            private void SetDefaultValues(JobListing job)
            {
                if (string.IsNullOrEmpty(job.PostDate))
                {
                    job.PostDate = DateTime.Now.ToString("yyyy-MM-dd");
                }

                if (string.IsNullOrEmpty(job.AdType))
                {
                    job.AdType = DetermineAdTypeFromUrl(job.Url);
                }

                if (string.IsNullOrEmpty(job.City))
                {
                    job.City = ExtractCityFromUrl(job.Url);
                }
            }

            private void SetExtractionStatus(JobListing job)
            {
                var extractedFields = new List<string>();
                if (!string.IsNullOrEmpty(job.PhoneNumber)) extractedFields.Add("Phone");
                if (!string.IsNullOrEmpty(job.Email)) extractedFields.Add("Email");
                if (!string.IsNullOrEmpty(job.City)) extractedFields.Add("City");
                if (!string.IsNullOrEmpty(job.JobTitle)) extractedFields.Add("Title");
                if (!string.IsNullOrEmpty(job.Description)) extractedFields.Add("Description");
                if (!string.IsNullOrEmpty(job.CompanyName)) extractedFields.Add("Company");
                if (!string.IsNullOrEmpty(job.Salary)) extractedFields.Add("Salary");

                job.ExtractionStatus = string.Join(", ", extractedFields);
            }

            private void ExtractAdditionalDetails(HtmlNode contentDiv, JobListing job)
            {
                // Extract company name if available
                var companyNode = contentDiv.SelectSingleNode(".//p[contains(text(), 'Company:')]");
                if (companyNode != null)
                {
                    job.CompanyName = CleanText(companyNode.InnerText.Replace("Company:", "").Trim());
                }

                // Extract salary if available
                var salaryNode = contentDiv.SelectSingleNode(".//p[contains(text(), 'Salary:')]");
                if (salaryNode != null)
                {
                    job.Salary = CleanText(salaryNode.InnerText.Replace("Salary:", "").Trim());
                }

                // Extract experience level if available
                var expNode = contentDiv.SelectSingleNode(".//p[contains(text(), 'Experience:')]");
                if (expNode != null)
                {
                    job.ExperienceLevel = CleanText(expNode.InnerText.Replace("Experience:", "").Trim());
                }
            }

            private string CleanText(string text)
            {
                if (string.IsNullOrEmpty(text)) return "";

                text = System.Net.WebUtility.HtmlDecode(text)
                    .Replace("\n", " ")
                    .Replace("\r", " ")
                    .Replace("\t", " ")
                    .Trim();

                return Regex.Replace(text, @"\s+", " ");
            }

            private string ExtractPhoneNumber(string text)
            {
                if (string.IsNullOrEmpty(text)) return "";

                var match = Regex.Match(text,
                    @"(05\d[\s-]?\d{3}[\s-]?\d{4})|(\+?966[\s-]?5\d[\s-]?\d{3}[\s-]?\d{4})");
                return match.Success ? FormatSaudiPhone(match.Value) : "";
            }

            private string FormatSaudiPhone(string phone)
            {
                if (string.IsNullOrEmpty(phone)) return "";

                phone = Regex.Replace(phone, @"[^\d]", "");

                if (phone.StartsWith("966") && phone.Length > 3)
                {
                    phone = phone.Substring(3);
                }
                else if (phone.StartsWith("+966") && phone.Length > 4)
                {
                    phone = phone.Substring(4);
                }

                if (phone.StartsWith("5") && phone.Length == 9)
                {
                    return $"966{phone}";
                }
                else if (phone.StartsWith("05") && phone.Length == 10)
                {
                    return $"966{phone.Substring(1)}";
                }

                return phone;
            }

            private string ExtractEmail(string text)
            {
                if (string.IsNullOrEmpty(text)) return "";

                var match = Regex.Match(text,
                    @"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}\b");
                return match.Success ? match.Value.ToLower() : "";
            }

            private string ExtractCity(string text)
            {
                if (string.IsNullOrEmpty(text)) return "";

                var saudiCities = new Dictionary<string, string>
            {
                {"??????", "??????"}, {"Riyadh", "??????"},
                {"???", "???"}, {"Jeddah", "???"},
                {"???", "???"}, {"Mecca", "???"},
                {"???????", "???????"}, {"Medina", "???????"},
                {"??????", "??????"}, {"Dammam", "??????"},
                {"?????", "?????"}, {"Khobar", "?????"},
                // Add more cities as needed
            };

                foreach (var city in saudiCities)
                {
                    if (text.Contains(city.Key, StringComparison.OrdinalIgnoreCase))
                    {
                        return city.Value;
                    }
                }

                return "";
            }

            private string ExtractCityFromUrl(string url)
            {
                var match = Regex.Match(url, @"/sa/([^/]+)/");
                if (!match.Success) return "";

                string cityFromUrl = match.Groups[1].Value;
                var cityMapping = new Dictionary<string, string>
            {
                {"riyadh", "??????"}, {"jeddah", "???"},
                {"mecca", "???"}, {"medina", "???????"},
                {"dammam", "??????"}, {"khobar", "?????"},
                // Add more city mappings as needed
            };

                return cityMapping.TryGetValue(cityFromUrl.ToLower(), out var city) ? city : "";
            }

            private string ExtractAdType(string text)
            {
                if (string.IsNullOrEmpty(text)) return "???";

                var jobTypes = new Dictionary<string, string>
            {
                {"???? ????", "???? ????"}, {"Full Time", "???? ????"},
                {"???? ????", "???? ????"}, {"Part Time", "???? ????"},
                {"????", "????"}, {"Temporary", "????"},
                {"?????", "?????"}, {"Internship", "?????"},
                {"?? ???", "?? ???"}, {"Remote", "?? ???"},
                {"?????", "?????"}, {"Delivery", "?????"},
                {"????", "????"}, {"Driver", "????"},
                {"??????", "??????"}, {"Sales", "??????"}
            };

                foreach (var type in jobTypes)
                {
                    if (text.Contains(type.Key, StringComparison.OrdinalIgnoreCase))
                    {
                        return type.Value;
                    }
                }

                return "???";
            }

            private string DetermineAdTypeFromUrl(string url)
            {
                var urlParts = url.Split('/');
                foreach (var part in urlParts)
                {
                    switch (part.ToLower())
                    {
                        case "delivery": return "?????";
                        case "driver": return "????";
                        case "sales": return "??????";
                        case "admin": return "?????";
                        case "technical": return "????";
                        case "medical": return "???";
                        case "education": return "?????";
                        case "construction": return "????";
                        case "hospitality": return "?????";
                        case "retail": return "?????";
                        default: continue;
                    }
                }
                return "???";
            }

            private string ExtractCompanyName(string text)
            {
                if (string.IsNullOrEmpty(text)) return "";

                // Look for common company name patterns
                var match = Regex.Match(text,
                    @"(?:????|?????|????|Company|Corporation|Establishment)[\s:]+([^\n\r]+)");
                return match.Success ? CleanText(match.Groups[1].Value) : "";
            }

            private string ExtractSalary(string text)
            {
                if (string.IsNullOrEmpty(text)) return "";

                // Match various salary formats
                var match = Regex.Match(text,
                    @"(?:????|Salary|Wage)[\s:]+([^\n\r]+)");
                return match.Success ? CleanText(match.Groups[1].Value) : "";
            }

            private string ExtractExperienceLevel(string text)
            {
                if (string.IsNullOrEmpty(text)) return "";

                // Match experience requirements
                var match = Regex.Match(text,
                    @"(?:????|Experience)[\s:]+([^\n\r]+)");
                return match.Success ? CleanText(match.Groups[1].Value) : "";
            }

            private bool IsPhoneNumber(string input)
            {
                return !string.IsNullOrEmpty(input) &&
                       Regex.IsMatch(input, @"[\d\s-]+") &&
                       input.Length >= 9;
            }

            private bool IsEmail(string input)
            {
                return !string.IsNullOrEmpty(input) &&
                       Regex.IsMatch(input, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            }

            private void DisplaySessionSummary()
            {
                Console.WriteLine("\n?? Scraping Session Summary:");
                Console.WriteLine($"   ?? Duration: {(_session.EndTime - _session.StartTime).TotalMinutes:F1} minutes");
                Console.WriteLine($"   ?? Success Rate: {(double)_session.SuccessfulExtractions / _session.ProcessedLinks * 100:F1}%");
                Console.WriteLine($"   ?? Jobs with Phone: {_jobListings.Count(j => !string.IsNullOrEmpty(j.PhoneNumber))}");
                Console.WriteLine($"   ?? Jobs with Email: {_jobListings.Count(j => !string.IsNullOrEmpty(j.Email))}");
                Console.WriteLine($"   ??? Jobs with City: {_jobListings.Count(j => !string.IsNullOrEmpty(j.City))}");
                Console.WriteLine($"   ?? Jobs with Title: {_jobListings.Count(j => !string.IsNullOrEmpty(j.JobTitle))}");
                Console.WriteLine($"   ?? Jobs with Salary: {_jobListings.Count(j => !string.IsNullOrEmpty(j.Salary))}");

                if (_session.Errors.Any())
                {
                    Console.WriteLine("\n?? Errors Encountered:");
                    foreach (var error in _session.Errors.Take(5))
                    {
                        Console.WriteLine($"   - {error}");
                    }
                    if (_session.Errors.Count > 5)
                    {
                        Console.WriteLine($"   + {_session.Errors.Count - 5} more errors...");
                    }
                }
            }

            public async Task ExportDataAsync(string outputDirectory)
            {
                Directory.CreateDirectory(outputDirectory);
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmm");

                if (_session.Configuration.ExportExcel)
                {
                    //await ExportToExcelAsync(Path.Combine(outputDirectory, $"mourjan_jobs_{timestamp}.xlsx"));
                }

                if (_session.Configuration.ExportJson)
                {
                    await ExportToJsonAsync(Path.Combine(outputDirectory, $"mourjan_jobs_{timestamp}.json"));
                }

                if (_session.Configuration.ExportTxt)
                {
                    await ExportToTxtAsync(Path.Combine(outputDirectory, $"mourjan_jobs_{timestamp}.txt"));
                }
            }













































            public async Task<List<JobListing>> ScrapeJobsOldAsync(string baseUrl, int maxPages = 20, CancellationToken cancellationToken = default)
            {
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    Console.WriteLine("?? Starting Mourjan job scraping session...");
                    Console.WriteLine($"? Start time: {_session.StartTime:yyyy-MM-dd HH:mm:ss}");
                    Console.WriteLine($"?? Configuration: {JsonConvert.SerializeObject(_session.Configuration, Formatting.Indented)}");

                    _jobListings = new List<JobListing>();
                    await InitializeBrowserAsync();

                    // Phase 1: Collect all job links
                    Console.WriteLine("\n?? Phase 1: Collecting job listings");
                    Console.WriteLine(new string('-', 50));

                    var jobLinks = await    CollectJobLinksAsync(baseUrl, maxPages, cancellationToken);
                    // Replace the problematic line:

                    // With the following corrected line:
                    //Console.WriteLine($"\n??   collected links: {string.Join(",", jobLinks)}");

                    _session.TotalLinks = jobLinks.Count;
                    Console.WriteLine($"\n?? Total collected links: {jobLinks.Count}");

                    // Phase 2: Extract detailed information
                    Console.WriteLine("\n?? Phase 2: Extracting job details");
                    Console.WriteLine(new string('-', 50));

                    await ProcessJobListingsAsync(jobLinks, cancellationToken);

                    _session.EndTime = DateTime.Now;
                    DisplaySessionSummary();

                    return _jobListings;
                }
                catch (OperationCanceledException ex)
                {
                    Console.WriteLine("?? Scraping was canceled by user.");
                    _session.EndTime = DateTime.Now;
                    DisplaySessionSummary();
    LogError(ex);



                    return _jobListings;
                }
            }


            private async Task<List<string>> CollectJobLinksOldAsync(string baseUrl, int maxPages, CancellationToken cancellationToken = default)
            {
                var jobLinks = new List<string>();
                string currentUrl = baseUrl;
                int page = 1;
                bool hasMorePages = true;

                //var maxPages = _session.Configuration.MaxPages;
                while (page <= maxPages && hasMorePages)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    try
                    {
                        Console.Write($"?? Page {page}... ");

                        string html = _useBrowser ?
                            await GetPageContentWithBrowserAsync(currentUrl, cancellationToken) :
                            await _httpClient.GetStringAsync(currentUrl, cancellationToken);

                        var doc = new HtmlDocument();
                        doc.LoadHtml(html);

                        // Extract job links
                        var jobLinkNodes = doc.DocumentNode.SelectNodes("//a[@class='link' and contains(@href, '/vacancies/')]");
                        int linksOnPage = 0;

                        if (jobLinkNodes != null)
                        {
                            foreach (var linkNode in jobLinkNodes)
                            {
                                cancellationToken.ThrowIfCancellationRequested();
                                string href = linkNode.GetAttributeValue("href", "");
                                if (!string.IsNullOrEmpty(href))
                                {

                                    Console.WriteLine($"?? Found link: {href}");

                                    string fullUrl = href.StartsWith("http") ? href : $"https://www.mourjan.com{href}";
                                    if (!jobLinks.Contains(fullUrl))
                                    {
                                        jobLinks.Add(fullUrl);
                                        linksOnPage++;
                                    }
                                }
                            }
                        }

                        Console.WriteLine($"? {linksOnPage} links found");

                        // Handle pagination
                        var pgnUl = doc.DocumentNode.SelectSingleNode("//ul[@class='pgn']");
                        string nextPageUrl = null;

                        if (pgnUl != null)
                        {
                            // Find the next page URL from the navigation arrow
                            var nextPageNode = pgnUl.SelectSingleNode("./li/a[@class='nav' and contains(text(),'?')]");
                            nextPageUrl = nextPageNode?.GetAttributeValue("href", null);

                            // If no next arrow, try to find the next numbered page
                            if (string.IsNullOrEmpty(nextPageUrl))
                            {
                                var activeItem = pgnUl.SelectSingleNode("./li[@class='active']");
                                if (activeItem != null)
                                {
                                    int activeIndex = pgnUl.SelectNodes("./li").IndexOf(activeItem);
                                    var nextItem = pgnUl.SelectNodes("./li")[activeIndex + 1];
                                    nextPageUrl = nextItem?.SelectSingleNode("./a")?.GetAttributeValue("href", null);
                                }
                            }
                        }

                        // Determine if we should continue to next page
                        if (string.IsNullOrEmpty(nextPageUrl))
                        {
                            Console.WriteLine("?? Last page reached");
                            hasMorePages = false;
                        }
                        else
                        {
                            currentUrl = nextPageUrl.StartsWith("http") ? nextPageUrl : $"https://www.mourjan.com{nextPageUrl}";
                            page++;
                            await Task.Delay(_session.Configuration.DelayBetweenRequestsMs, cancellationToken);
                        }
                    }
                    catch (OperationCanceledException ex)
                    {
                        LogError(ex);
                        throw;
                    }
                    catch (Exception ex)
                    {
                        LogError(ex);
                        _session.Errors.Add($"Page {page}: {ex.Message}");
                        Console.WriteLine($"? Error: {ex.Message}");
                        break;
                    }
                }

                return jobLinks;
            }

            private void LogError(Exception ex)
            {
                var method = new StackTrace(ex, true).GetFrame(0)?.GetMethod();
                string methodName = method != null ? method.Name : "UnknownMethod";
                string errorMsg = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Method: {methodName} | Error: {ex.Message}\n{ex.StackTrace}\n";
                Console.WriteLine($"? Error in {methodName}: {ex.Message}");
                File.AppendAllText("error.log", errorMsg);
            }

            public async Task<string> ExportToExcelAsync(string filePath)
            {
                try
                {
                    string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmm");
                    string fileName = Path.Combine(filePath, $"mourjan_jobs_{timestamp}.csv");

                    var csvBuilder = new StringBuilder();

                    // Headers with BOM for UTF-8 Excel compatibility
                    csvBuilder.AppendLine("Job Title,Company,Phone,Email,City,Salary,Experience,WhatsApp,Mobile,URL,Data UID,Post Date,Ad Type,Extraction Status");

                    // Data rows
                    foreach (var job in _jobListings)
                    {
                        csvBuilder.AppendLine($"{EscapeCsv(job.JobTitle)},{EscapeCsv(job.CompanyName)},{EscapeCsv(job.PhoneNumber)},{EscapeCsv(job.Email)},{EscapeCsv(job.City)},{EscapeCsv(job.Salary)},{EscapeCsv(job.ExperienceLevel)},{job.HasWhatsApp},{job.HasMobile},{EscapeCsv(job.Url)},{EscapeCsv(job.DataUid)},{EscapeCsv(job.PostDate)},{EscapeCsv(job.AdType)},{EscapeCsv(job.ExtractionStatus)}");
                    }

                    // Write with UTF-8 BOM for Excel compatibility
                    var utf8WithBom = new UTF8Encoding(true);
                    await File.WriteAllTextAsync(fileName, csvBuilder.ToString(), utf8WithBom);

                    Console.WriteLine($"? Excel-compatible CSV file saved: {fileName}");
                    return fileName;
                }
                catch (Exception ex)
                {
                    LogError(ex);
                    Console.WriteLine($"? Error exporting to Excel: {ex.Message}");
                    throw;
                }
            }



                  public async Task<string> ExportToExcelAsync(List<JobListing> jobjs,string filePath)
            {
                try
                {
                    string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmm");
                    string fileName = Path.Combine(filePath, $"mourjan_jobs_{timestamp}.csv");

                    var csvBuilder = new StringBuilder();

                    // Headers with BOM for UTF-8 Excel compatibility
                    csvBuilder.AppendLine("Job Title,Company,Phone,Email,City,Salary,Experience,WhatsApp,Mobile,URL,Data UID,Post Date,Ad Type,Extraction Status");

                    // Data rows
                    foreach (var job in jobjs)
                    {
                        csvBuilder.AppendLine($"{EscapeCsv(job.JobTitle)},{EscapeCsv(job.CompanyName)},{EscapeCsv(job.PhoneNumber)},{EscapeCsv(job.Email)},{EscapeCsv(job.City)},{EscapeCsv(job.Salary)},{EscapeCsv(job.ExperienceLevel)},{job.HasWhatsApp},{job.HasMobile},{EscapeCsv(job.Url)},{EscapeCsv(job.DataUid)},{EscapeCsv(job.PostDate)},{EscapeCsv(job.AdType)},{EscapeCsv(job.ExtractionStatus)}");
                    }

                    // Write with UTF-8 BOM for Excel compatibility
                    var utf8WithBom = new UTF8Encoding(true);
                    await File.WriteAllTextAsync(fileName, csvBuilder.ToString(), utf8WithBom);

                    Console.WriteLine($"? Excel-compatible CSV file saved: {fileName}");
                    return fileName;
                }
                catch (Exception ex)
                {
                    LogError(ex);
                    Console.WriteLine($"? Error exporting to Excel: {ex.Message}");
                    throw;
                }
            }

            // JSON Export
            public async Task<string> ExportToJsonAsync(string filePath)
            {
                try
                {
                    string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmm");
                    string fileName = Path.Combine(filePath, $"mourjan_jobs_{timestamp}.json");

                    var exportData = new
                    {
                        ExportInfo = new
                        {
                            ExportDate = DateTime.Now,
                            TotalJobs = _jobListings.Count,
                            ExportedBy = "Mourjan Scraper",
                            Version = "1.0"
                        },
                        Jobs = _jobListings.Select(job => new
                        {
                            job.DataUid,
                            job.JobTitle,
                            job.CompanyName,
                            job.PhoneNumber,
                            job.Email,
                            job.City,
                            job.Salary,
                            job.ExperienceLevel,
                            job.Category,
                            job.AdType,
                            job.HasWhatsApp,
                            job.HasMobile,
                            job.PostDate,
                            job.Url,
                            job.Description,
                            job.ExtractionStatus
                        }).ToList()
                    };

                    var jsonString = JsonConvert.SerializeObject(exportData, Formatting.Indented);
                    await File.WriteAllTextAsync(fileName, jsonString);
                    
                    Console.WriteLine($"? JSON file saved: {fileName}");
                    return fileName;
                }
                catch (Exception ex)
                {
                    LogError(ex);
                    Console.WriteLine($"? Error exporting to JSON: {ex.Message}");
                    throw;
                }
            }

            // CSV Export
            public async Task<string> ExportToCsvAsync(string filePath)
            {
                try
                {
                    string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmm");
                    string fileName = Path.Combine(filePath, $"mourjan_jobs_{timestamp}.csv");
                    
                    var csvBuilder = new StringBuilder();
                    
                    // Headers
                    csvBuilder.AppendLine("DataUid,JobTitle,CompanyName,PhoneNumber,Email,City,Salary,ExperienceLevel,Category,AdType,HasWhatsApp,HasMobile,PostDate,Url,Description,ExtractionStatus");
                    
                    // Data rows
                    foreach (var job in _jobListings)
                    {
                        csvBuilder.AppendLine($"\"{EscapeCsv(job.DataUid)}\",\"{EscapeCsv(job.JobTitle)}\",\"{EscapeCsv(job.CompanyName)}\",\"{EscapeCsv(job.PhoneNumber)}\",\"{EscapeCsv(job.Email)}\",\"{EscapeCsv(job.City)}\",\"{EscapeCsv(job.Salary)}\",\"{EscapeCsv(job.ExperienceLevel)}\",\"{EscapeCsv(job.Category)}\",\"{EscapeCsv(job.AdType)}\",{job.HasWhatsApp},{job.HasMobile},\"{EscapeCsv(job.PostDate)}\",\"{EscapeCsv(job.Url)}\",\"{EscapeCsv(job.Description)}\",\"{EscapeCsv(job.ExtractionStatus)}\"");
                    }
                    
                    await File.WriteAllTextAsync(fileName, csvBuilder.ToString());
                    
                    Console.WriteLine($"? CSV file saved: {fileName}");
                    return fileName;
                }
                catch (Exception ex)
                {
                    LogError(ex);
                    Console.WriteLine($"? Error exporting to CSV: {ex.Message}");
                    throw;
                }
            }

            // XML Export
            public async Task<string> ExportToXmlAsync(string filePath)
            {
                try
                {
                    string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmm");
                    string fileName = Path.Combine(filePath, $"mourjan_jobs_{timestamp}.xml");

                    var xmlDoc = new XmlDocument();
                    xmlDoc.AppendChild(xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", null));
                    
                    var root = xmlDoc.CreateElement("JobExport");
                    xmlDoc.AppendChild(root);
                    
                    // Export info
                    var exportInfo = xmlDoc.CreateElement("ExportInfo");
                    exportInfo.SetAttribute("ExportDate", DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"));
                    exportInfo.SetAttribute("TotalJobs", _jobListings.Count.ToString());
                    exportInfo.SetAttribute("ExportedBy", "Mourjan Scraper");
                    exportInfo.SetAttribute("Version", "1.0");
                    root.AppendChild(exportInfo);
                    
                    // Jobs
                    var jobsElement = xmlDoc.CreateElement("Jobs");
                    root.AppendChild(jobsElement);
                    
                    foreach (var job in _jobListings)
                    {
                        var jobElement = xmlDoc.CreateElement("Job");
                        jobElement.SetAttribute("DataUid", job.DataUid ?? "");
                        
                        AddXmlElement(xmlDoc, jobElement, "JobTitle", job.JobTitle);
                        AddXmlElement(xmlDoc, jobElement, "CompanyName", job.CompanyName);
                        AddXmlElement(xmlDoc, jobElement, "PhoneNumber", job.PhoneNumber);
                        AddXmlElement(xmlDoc, jobElement, "Email", job.Email);
                        AddXmlElement(xmlDoc, jobElement, "City", job.City);
                        AddXmlElement(xmlDoc, jobElement, "Salary", job.Salary);
                        AddXmlElement(xmlDoc, jobElement, "ExperienceLevel", job.ExperienceLevel);
                        AddXmlElement(xmlDoc, jobElement, "Category", job.Category);
                        AddXmlElement(xmlDoc, jobElement, "AdType", job.AdType);
                        AddXmlElement(xmlDoc, jobElement, "HasWhatsApp", job.HasWhatsApp.ToString());
                        AddXmlElement(xmlDoc, jobElement, "HasMobile", job.HasMobile.ToString());
                        AddXmlElement(xmlDoc, jobElement, "PostDate", job.PostDate);
                        AddXmlElement(xmlDoc, jobElement, "Url", job.Url);
                        AddXmlElement(xmlDoc, jobElement, "Description", job.Description);
                        AddXmlElement(xmlDoc, jobElement, "ExtractionStatus", job.ExtractionStatus);
                        
                        jobsElement.AppendChild(jobElement);
                    }
                    
                    xmlDoc.Save(fileName);
                    
                    Console.WriteLine($"? XML file saved: {fileName}");
                    return fileName;
                }
                catch (Exception ex)
                {
                    LogError(ex);
                    Console.WriteLine($"? Error exporting to XML: {ex.Message}");
                    throw;
                }
            }

            // PDF Export
            public async Task<string> ExportToPdfAsync(string filePath)
            {
                try
                {
                    string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmm");
                    string fileName = Path.Combine(filePath, $"mourjan_jobs_{timestamp}.pdf");
                    
                    // For now, create an HTML file that can be converted to PDF
                    // In a real implementation, you would use a PDF library like iText7
                    var htmlBuilder = new StringBuilder();
                    
                    htmlBuilder.AppendLine("<!DOCTYPE html>");
                    htmlBuilder.AppendLine("<html><head>");
                    htmlBuilder.AppendLine("<meta charset='UTF-8'>");
                    htmlBuilder.AppendLine("<title>Mourjan Jobs Export</title>");
                    htmlBuilder.AppendLine("<style>");
                    htmlBuilder.AppendLine("body { font-family: Arial, sans-serif; margin: 20px; }");
                    htmlBuilder.AppendLine("h1 { color: #2c3e50; border-bottom: 2px solid #3498db; padding-bottom: 10px; }");
                    htmlBuilder.AppendLine("table { width: 100%; border-collapse: collapse; margin-top: 20px; }");
                    htmlBuilder.AppendLine("th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }");
                    htmlBuilder.AppendLine("th { background-color: #f2f2f2; font-weight: bold; }");
                    htmlBuilder.AppendLine("tr:nth-child(even) { background-color: #f9f9f9; }");
                    htmlBuilder.AppendLine(".job-card { border: 1px solid #ddd; margin: 10px 0; padding: 15px; border-radius: 5px; }");
                    htmlBuilder.AppendLine(".job-title { font-size: 18px; font-weight: bold; color: #2c3e50; margin-bottom: 10px; }");
                    htmlBuilder.AppendLine(".job-company { font-size: 14px; color: #7f8c8d; margin-bottom: 5px; }");
                    htmlBuilder.AppendLine(".job-details { font-size: 12px; color: #34495e; }");
                    htmlBuilder.AppendLine("</style>");
                    htmlBuilder.AppendLine("</head><body>");
                    
                    htmlBuilder.AppendLine($"<h1>Mourjan Jobs Export Report</h1>");
                    htmlBuilder.AppendLine($"<p><strong>Export Date:</strong> {DateTime.Now:yyyy-MM-dd HH:mm:ss}</p>");
                    htmlBuilder.AppendLine($"<p><strong>Total Jobs:</strong> {_jobListings.Count}</p>");
                    htmlBuilder.AppendLine($"<p><strong>Jobs with Email:</strong> {_jobListings.Count(j => !string.IsNullOrEmpty(j.Email))}</p>");
                    htmlBuilder.AppendLine($"<p><strong>Jobs with Phone:</strong> {_jobListings.Count(j => !string.IsNullOrEmpty(j.PhoneNumber))}</p>");
                    
                    foreach (var job in _jobListings)
                    {
                        htmlBuilder.AppendLine("<div class='job-card'>");
                        htmlBuilder.AppendLine($"<div class='job-title'>{EscapeHtml(job.JobTitle)}</div>");
                        htmlBuilder.AppendLine($"<div class='job-company'>{EscapeHtml(job.CompanyName)}</div>");
                        htmlBuilder.AppendLine("<div class='job-details'>");
                        
                        if (!string.IsNullOrEmpty(job.City))
                            htmlBuilder.AppendLine($"<p><strong>Location:</strong> {EscapeHtml(job.City)}</p>");
                        if (!string.IsNullOrEmpty(job.Salary))
                            htmlBuilder.AppendLine($"<p><strong>Salary:</strong> {EscapeHtml(job.Salary)}</p>");
                        if (!string.IsNullOrEmpty(job.PhoneNumber))
                            htmlBuilder.AppendLine($"<p><strong>Phone:</strong> {EscapeHtml(job.PhoneNumber)}</p>");
                        if (!string.IsNullOrEmpty(job.Email))
                            htmlBuilder.AppendLine($"<p><strong>Email:</strong> {EscapeHtml(job.Email)}</p>");
                        if (!string.IsNullOrEmpty(job.ExperienceLevel))
                            htmlBuilder.AppendLine($"<p><strong>Experience:</strong> {EscapeHtml(job.ExperienceLevel)}</p>");
                        if (!string.IsNullOrEmpty(job.PostDate))
                            htmlBuilder.AppendLine($"<p><strong>Posted:</strong> {EscapeHtml(job.PostDate)}</p>");
                        
                        htmlBuilder.AppendLine("</div>");
                        htmlBuilder.AppendLine("</div>");
                    }
                    
                    htmlBuilder.AppendLine("</body></html>");



                    var bytes22 = await _pdfRenderer.GetPdfByHtmlAsync(htmlBuilder.ToString());


                    await File.WriteAllBytesAsync(fileName, bytes22);
                    // Save as HTML (can be converted to PDF later)
                    string htmlFileName = fileName.Replace(".pdf", ".html");






                    await File.WriteAllTextAsync(htmlFileName, htmlBuilder.ToString());

                    Console.WriteLine($"? PDF-ready HTML file saved: {htmlFileName}");
                    Console.WriteLine("?? Note: For PDF conversion, use a tool like wkhtmltopdf or Puppeteer");
                    return fileName;
                }
                catch (Exception ex)
                {
                    LogError(ex);
                    Console.WriteLine($"? Error exporting to PDF: {ex.Message}");
                    throw;
                }
            }

            // TXT Export
            public async Task<string> ExportToTxtAsync(string filePath)
            {
                try
                {
                    string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmm");
                    string fileName = Path.Combine(filePath, $"mourjan_jobs_{timestamp}.txt");
                    
                    var txtBuilder = new StringBuilder();
                    
                    txtBuilder.AppendLine("MOURJAN JOBS EXPORT REPORT");
                    txtBuilder.AppendLine("=" + new string('=', 50));
                    txtBuilder.AppendLine($"Export Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                    txtBuilder.AppendLine($"Total Jobs: {_jobListings.Count}");
                    txtBuilder.AppendLine($"Jobs with Email: {_jobListings.Count(j => !string.IsNullOrEmpty(j.Email))}");
                    txtBuilder.AppendLine($"Jobs with Phone: {_jobListings.Count(j => !string.IsNullOrEmpty(j.PhoneNumber))}");
                    txtBuilder.AppendLine($"Jobs with WhatsApp: {_jobListings.Count(j => j.HasWhatsApp)}");
                    txtBuilder.AppendLine();
                    
                    int jobNumber = 1;
                    foreach (var job in _jobListings)
                    {
                        txtBuilder.AppendLine($"JOB #{jobNumber}");
                        txtBuilder.AppendLine("-" + new string('-', 20));
                        txtBuilder.AppendLine($"Title: {job.JobTitle ?? "N/A"}");
                        txtBuilder.AppendLine($"Company: {job.CompanyName ?? "N/A"}");
                        txtBuilder.AppendLine($"Location: {job.City ?? "N/A"}");
                        txtBuilder.AppendLine($"Salary: {job.Salary ?? "N/A"}");
                        txtBuilder.AppendLine($"Experience: {job.ExperienceLevel ?? "N/A"}");
                        txtBuilder.AppendLine($"Phone: {job.PhoneNumber ?? "N/A"}");
                        txtBuilder.AppendLine($"Email: {job.Email ?? "N/A"}");
                        txtBuilder.AppendLine($"WhatsApp: {(job.HasWhatsApp ? "Yes" : "No")}");
                        txtBuilder.AppendLine($"Mobile: {(job.HasMobile ? "Yes" : "No")}");
                        txtBuilder.AppendLine($"Category: {job.Category ?? "N/A"}");
                        txtBuilder.AppendLine($"Ad Type: {job.AdType ?? "N/A"}");
                        txtBuilder.AppendLine($"Posted: {job.PostDate ?? "N/A"}");
                        txtBuilder.AppendLine($"URL: {job.Url ?? "N/A"}");
                        txtBuilder.AppendLine($"Data UID: {job.DataUid ?? "N/A"}");
                        txtBuilder.AppendLine($"Status: {job.ExtractionStatus ?? "N/A"}");
                        
                        if (!string.IsNullOrEmpty(job.Description))
                        {
                            txtBuilder.AppendLine($"Description: {job.Description.Substring(0, Math.Min(200, job.Description.Length))}...");
                        }
                        
                        txtBuilder.AppendLine();
                        jobNumber++;
                    }
                    
                    txtBuilder.AppendLine("=" + new string('=', 50));
                    txtBuilder.AppendLine("End of Report");
                    
                    await File.WriteAllTextAsync(fileName, txtBuilder.ToString());
                    
                    Console.WriteLine($"? TXT file saved: {fileName}");
                    return fileName;
                }
                catch (Exception ex)
                {
                    LogError(ex);
                    Console.WriteLine($"? Error exporting to TXT: {ex.Message}");
                    throw;
                }
            }

            // Selected Jobs Export Methods
            public async Task<string> ExportSelectedJobsToJsonAsync(List<JobListing> selectedJobs, string filePath)
            {
                var originalJobs = _jobListings;
                _jobListings = selectedJobs;
                var result = await ExportToJsonAsync(filePath);
                _jobListings = originalJobs;
                return result;
            }

            public async Task<string> ExportSelectedJobsToCsvAsync(List<JobListing> selectedJobs, string filePath)
            {
                var originalJobs = _jobListings;
                _jobListings = selectedJobs;
                var result = await ExportToCsvAsync(filePath);
                _jobListings = originalJobs;
                return result;
            }

            public async Task<string> ExportSelectedJobsToXmlAsync(List<JobListing> selectedJobs, string filePath)
            {
                var originalJobs = _jobListings;
                _jobListings = selectedJobs;
                var result = await ExportToXmlAsync(filePath);
                _jobListings = originalJobs;
                return result;
            }

            public async Task<string> ExportSelectedJobsToPdfAsync(List<JobListing> selectedJobs, string filePath)
            {
                var originalJobs = _jobListings;
                _jobListings = selectedJobs;
                var result = await ExportToPdfAsync(filePath);
                _jobListings = originalJobs;
                return result;
            }

            public async Task<string> ExportSelectedJobsToTxtAsync(List<JobListing> selectedJobs, string filePath)
            {
                var originalJobs = _jobListings;
                _jobListings = selectedJobs;
                var result = await ExportToTxtAsync(filePath);
                _jobListings = originalJobs;
                return result;
            }

            public async Task<string> ExportSelectedJobsToExcelAsync(List<JobListing> selectedJobs, string filePath)
            {
                return await ExportToExcelAsync(selectedJobs, filePath);
            }

            // Helper methods
            private string EscapeCsv(string input)
            {
                if (string.IsNullOrEmpty(input))
                    return "\"\"";

                // Replace quotes with double quotes and handle newlines
                var escaped = input.Replace("\"", "\"\"").Replace("\r\n", " ").Replace("\n", " ").Replace("\r", " ");

                // Always wrap in quotes for proper CSV formatting
                return $"\"{escaped}\"";
            }

            private string EscapeHtml(string input)
            {
                if (string.IsNullOrEmpty(input))
                    return "";
                
                return input.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;").Replace("'", "&#39;");
            }

            private void AddXmlElement(XmlDocument doc, XmlElement parent, string elementName, string value)
            {
                var element = doc.CreateElement(elementName);
                element.InnerText = value ?? "";
                parent.AppendChild(element);
            }

            private void DisplaySessionOldSummary()
            {
                Console.WriteLine("\n?? Scraping Session Summary:");
                Console.WriteLine($"   ?? Duration: {(_session.EndTime - _session.StartTime).TotalMinutes:F1} minutes");
                Console.WriteLine($"   ?? Success Rate: {(double)_session.SuccessfulExtractions / _session.ProcessedLinks * 100:F1}%");
                Console.WriteLine($"   ?? Jobs with Phone: {_jobListings.Count(j => !string.IsNullOrEmpty(j.PhoneNumber))}");
                Console.WriteLine($"   ?? Jobs with Email: {_jobListings.Count(j => !string.IsNullOrEmpty(j.Email))}");
                Console.WriteLine($"   ??? Jobs with City: {_jobListings.Count(j => !string.IsNullOrEmpty(j.City))}");
                Console.WriteLine($"   ?? Jobs with Title: {_jobListings.Count(j => !string.IsNullOrEmpty(j.JobTitle))}");
                Console.WriteLine($"   ?? Jobs with Salary: {_jobListings.Count(j => !string.IsNullOrEmpty(j.Salary))}");

                if (_session.Errors.Any())
                {
                    Console.WriteLine("\n?? Errors Encountered:");
                    foreach (var error in _session.Errors.Take(5))
                    {
                        Console.WriteLine($"   - {error}");
                    }
                    if (_session.Errors.Count > 5)
                    {
                        Console.WriteLine($"   + {_session.Errors.Count - 5} more errors...");
                    }
                }
            }

            private async Task ProcessJobListingsOldAsync(List<string> jobLinks, CancellationToken cancellationToken = default)
            {
                // Implementation stub - would process individual job listings
                foreach (var link in jobLinks)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    // Process each job link...
                    _session.ProcessedLinks++;
                }
            }

            private async Task<string> GetPageContentWithBrowserOldAsync(string url, CancellationToken cancellationToken = default)
            {
                using var page = await _browser.NewPageAsync();
                await page.GoToAsync(url, WaitUntilNavigation.DOMContentLoaded);
                return await page.GetContentAsync();
            }

            public void Dispose()
            {
                _httpClient?.Dispose();
                _browser?.DisposeAsync().GetAwaiter().GetResult();
            }

            private class DecodeResponse
            {
                [JsonProperty("success")]
                public int Success { get; set; }

                [JsonProperty("result")]
                public List<string> Result { get; set; }
            }
        }


    }








}
