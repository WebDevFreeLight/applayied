using Microsoft.Extensions.Logging;
using PuppeteerSharp;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Printing;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using iText.Kernel.Pdf;
using Serilog;
using ZR.PuppeteerPDF;
using ZR.Pdf.Core;
using Infrastructure.Attribute;

public class PrinterSettings
{
    public string PrinterName { get; set; } // For local/by name printers

    public string IpAddress { get; set; }
    public int Port { get; set; } = 9100; // Default raw print port

    public string Username { get; set; }
    public string Password { get; set; }
}




[AppService(ServiceType = typeof(IPdfRenderer))]


public class PdfRenderer : IPdfRenderer
{
    private readonly IBrowserProvider browserProvider;
    private readonly PdfOptions defaultOptions = new() { PrintBackground = true };

    private readonly ScreenshotOptions defaultScreenshotOptions =
        new() { FullPage = true, Type = ScreenshotType.Png };

    private readonly ILogger<PdfRenderer> logger;


    public PdfRenderer(IBrowserProvider browserProvider,
        ILogger<PdfRenderer> logger)
    {
        this.browserProvider = browserProvider;
        this.logger = logger;

    }

    public async Task<byte[]> GetPdfByUrlAsync(string url, PdfOptions? options = null, TimeSpan? delay = null)
    {
        try
        {
            await using var browser = await browserProvider.GetBrowserAsync();
            var page = await GetPageByUrl(browser, url, delay);
            options ??= defaultOptions;
            var pdf = await page.PdfDataAsync(options);
            await browser.CloseAsync();
            return pdf;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while generating pdf from url: {ErrorText}", ex.ToString());
            throw new InvalidOperationException(ex.Message, ex);
        }
    }




    public async Task ShouldHonorThePrice()
    {
        try
        {
            // ??? ????? ???? ???????? ???? ?????? (browserProvider)
            await using var browser = await browserProvider.GetBrowserAsync();

            // ????? ?????? ???? ???? ???? ?? ???????
            var url = "https://www.despegar.com.ar/shop/flights/results/roundtrip/BUE/MDZ/2018-12-01/2018-12-08/1/0/0/NA/NA/NA/NA/NA?from=SB&di=1-0";

            // ????? ???? ????? ?? ???????
            using (var page = await browser.NewPageAsync())
            {
                try
                {
                    // ???????? ??? ?????? ?????? ????????? ??? ??? ????? ?????? ???????
                    await page.GoToAsync(url, WaitUntilNavigation.Networkidle0);

                    // ???????? ??? ??? ????? ?? ?????? (buy-button) ??? ??????
                    await page.WaitForSelectorAsync("buy-button");

                    // ??????? ????? ?????? ?? ???? ????? ?? ???????
                    // ??? ??????? JavaScript ?????? ??????? ???? ????? ??? ??????? (.price-amount)
                    var bestPrice = await page.EvaluateFunctionAsync<string>(@"() => {
                    var elements = document.querySelectorAll('.main-content .price-amount');
                    return elements.length ? elements[0].innerText : '0'; // ??? ?? ??? ???? ?????? ??? ????? '0'
                }");

                    // ????? ?? ?????? ????? ?? ????? ??????? (.main-content buy-button)
                    // ??? ??????? JavaScript ?????? ?????? ??????? ????? ???? ?????
                    var clickElement = await page.EvaluateExpressionHandleAsync(@"
                    document.querySelectorAll('.main-content buy-button:first-child A')[0]") as ElementHandle;

                    // ????? ??? ?? ??????
                    await clickElement.ClickAsync();

                    // ???????? ??? ??? ????? ????? ?? ???? ????? (.price-container .amount)
                    await page.WaitForSelectorAsync(".price-container .amount");

                    // ??????? ????? ??????? ?? ???? ?????
                    // ??? ??????? JavaScript ?????? ?????? ???? ????? ??? ????? ???????
                    var checkoutPrice = await page.EvaluateExpressionAsync<string>(@"
                    document.querySelectorAll('.price-container .amount')[0].innerText
                ");

                    // ??? ?????? ????? ???? ???????? ??? ??????? (bestPrice ? checkoutPrice)
                    if (bestPrice == checkoutPrice)
                    {
                        Console.WriteLine("????? ???? ??????? ??? ???? ????? ????? ?????.");
                    }
                    else
                    {
                        Console.WriteLine("????? ??? ??????. ????? ?? ???? ?????: " + bestPrice + ", ????? ?? ???? ?????: " + checkoutPrice);
                    }
                }
                catch (Exception ex)
                {
                    // ?????? ?? ??? ???? ???? ??????
                    Console.WriteLine("??? ??? ????? ??????? ?? ??????: " + ex.Message);
                }
            }
        }
        catch (Exception ex)
        {
            // ?????? ?? ??? ???? ????? ??? ??????? ?? ????? ??????
            Console.WriteLine("??? ??? ????? ??? ??????? ?? ????? ??????: " + ex.Message);
        }
    }






    public async Task<byte[]> GetPdfByHtmlAsync(string html, PdfOptions? options = null, TimeSpan? delay = null)
    {
        try
        {
            await using var browser = await browserProvider.GetBrowserAsync();
            var page = await GetPageWithHtml(browser, html, delay);
            options ??= defaultOptions;
            var pdf = await page.PdfDataAsync(options);




            await browser.CloseAsync();


            var metadata = new Dictionary<string, string>
            {
                ["Title"] = "My Custom Title",
                ["producer"] = "My Custom Title",
                ["Keywords"] = "My Custom Title",
                ["Author"] = "John Doe",
                ["Subject"] = "Important Document"

            };
            var pdf2 = await AddPdfMetadata(pdf, metadata);

            return pdf2;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while generating pdf from html: {ErrorText}", ex.Message.ToString());
            throw new InvalidOperationException(ex.Message, ex);
        }
    }


    //public async Task<byte[]> AddPdfMetadata(byte[] pdfData, Dictionary<string, string> metadata)
    //{
    //    try
    //    {
    //        // Initialize Bouncy Castle provider
    //        var provider = new BouncyCastleProvider();
    //        Security.AddProvider(provider);

    //        using var pdfReader = new PdfReader(new MemoryStream(pdfData));
    //        using var ms = new MemoryStream();
    //        using var pdfWriter = new PdfWriter(ms);
    //        using var pdfDoc = new PdfDocument(pdfReader, pdfWriter);

    //        var info = pdfDoc.GetDocumentInfo();

    //        foreach (var item in metadata)
    //        {
    //            switch (item.Key.ToLower())
    //            {
    //                case "title":
    //                    info.SetTitle(item.Value);
    //                    break;
    //                case "author":
    //                    info.SetAuthor(item.Value);
    //                    break;
    //                    // ... other cases ...
    //            }
    //        }

    //        pdfDoc.Close();
    //        return ms.ToArray();
    //    }
    //    catch (Exception ex)
    //    {
    //        logger.LogError(ex, "Error adding PDF metadata");
    //        throw;
    //    }
    //}


    public async Task<byte[]> AddPdfMetadata(byte[] pdfData, Dictionary<string, string> metadata)
    {
        try
        {

            //return pdfData;



            var writerProperties = new WriterProperties()
                    .SetPdfVersion(PdfVersion.PDF_1_4)
                    //.AddPdfAXmpMetadata(PdfAConformance.PDF_A_4)
                    //.AddPdfUaXmpMetadata(PdfUAConformance.PDF_UA_1)
                    //.AddXmpMetadata();
                    ;
            using var pdfReader = new PdfReader(new MemoryStream(pdfData));
            using var ms = new MemoryStream();

            using var pdfWriter = new PdfWriter(ms, writerProperties);

            using var pdfDoc = new iText.Kernel.Pdf.PdfDocument(pdfReader, pdfWriter);

            var info = pdfDoc.GetDocumentInfo();
            info.SetMoreInfo(metadata);
            info.RemoveCreationDate();
            info.AddModDate();
            info.AddCreationDate();
            foreach (var item in metadata)
            {
                switch (item.Key.ToLower())
                {
                    case "title":
                        info.SetTitle(item.Value);
                        break;
                    case "author":
                        info.SetAuthor(item.Value);
                        break;
                    case "subject":
                        info.SetSubject(item.Value);
                        break;
                    case "keywords":
                        info.SetKeywords(item.Value);
                        break;
                    case "creator":
                        info.SetCreator(item.Value);
                        break;
                    case "producer":
                        info.SetProducer(item.Value);
                        break;
                }
            }



            pdfDoc.Close();
            return ms.ToArray();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error adding PDF metadata: {ErrorText}", ex.ToString());
            throw;
        }
    }



    public async Task<byte[]> GetScreenshotByUrlAsync(string url, ScreenshotOptions? options = null,
        TimeSpan? delay = null)
    {
        try
        {
            await using var browser = await browserProvider.GetBrowserAsync();
            var page = await GetPageByUrl(browser, url, delay);
            options ??= defaultScreenshotOptions;
            var screenshot = await page.ScreenshotDataAsync(options);
            await browser.CloseAsync();
            return screenshot;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while generating pdf from url: {ErrorText}", ex.ToString());
            throw new InvalidOperationException(ex.Message, ex);
        }
    }

    public async Task<byte[]> GetScreenshotByHtmlAsync(string html, ScreenshotOptions? options = null,
        TimeSpan? delay = null)
    {
        try
        {
            await using var browser = await browserProvider.GetBrowserAsync();
            var page = await GetPageWithHtml(browser, html, delay);
            options ??= defaultScreenshotOptions;
            var screenshot = await page.ScreenshotDataAsync(options);
            await browser.CloseAsync();
            return screenshot;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while generating pdf from url: {ErrorText}", ex.ToString());
            throw new InvalidOperationException(ex.Message, ex);
        }
    }

    private static async Task<IPage> GetPageByUrl(IBrowser browser, string url, TimeSpan? delay = null)
    {
        var page = await browser.NewPageAsync();
        await page.GoToAsync(url);

        // await page.GoToAsync("http://www.google.com"); // In case of fonts being loaded from a CDN, use WaitUntilNavigation.Networkidle0 as a second param.
        await page.EvaluateExpressionHandleAsync("document.fonts.ready"); // Wait for fonts to be loaded. Omitting this might result in no text rendered in pdf.







        // await page.PdfAsync(outputFile);




        if (delay != null)
        {
            await Task.Delay(delay.Value);
        }

        return page;
    }

    private static async Task<IPage> GetPageWithHtmlOld(IBrowser browser, string html, TimeSpan? delay = null)
    {
        var page = await browser.NewPageAsync();

        await page.SetContentAsync(html);
        await page.EvaluateExpressionHandleAsync("document.fonts.ready"); // Wait for fonts to be loaded. Omitting this might result in no text rendered in pdf.
                                                                          // await page.PdfAsync(outputFile);


        if (delay != null)
        {
            await Task.Delay(delay.Value);
        }

        return page;
    }






    private static async Task<IPage> GetPageWithHtml(IBrowser browser, string html, TimeSpan? delay = null)
    {
        IPage page = null;
        try
        {
            Console.WriteLine("[INFO] Creating new browser page...");

            Console.WriteLine("[INFO] Setting up console logging...");





            //browser.AcceptInsecureCerts = true;
            //browser.CreateBrowserContextAsync( new BrowserContextOptions() { });




            page = await browser.NewPageAsync();
            await page.SetCacheEnabledAsync(true);
            await page.SetJavaScriptEnabledAsync(true);

            //await page.SetBypassCSPAsync(true);
            //await page.SetBypassServiceWorkerAsync(true);
            //await page.SetUserAgentAsync("true",new UserAgentMetadata()
            //{

            //    FullVersion="V1",
            //    PlatformVersion="2",

            //});
            await page.SetViewportAsync(new ViewPortOptions()
            {
                IsMobile = true,
                HasTouch = true,

            });
            // await page.SetOfflineModeAsync(true);
            // await page.SetRequestInterceptionAsync(true);
            //clsawait page.set
            Console.WriteLine("[SUCCESS] New page created successfully");

            // page.Console += async (object? sender, ConsoleEventArgs e) =>
            //        {
            //            Console.WriteLine($"[JS_CONSOLE] MSG  {e.Message + " " + e.Message.Text} URl {e.Message.Location.URL} LineNumber {e.Message.Location.LineNumber} {e.Message.Location.ColumnNumber} Type {e.Message.Type} ");


            //            if (e.Message.Args != null)
            //            {
            //                foreach (var arg in e.Message.Args)
            //                {
            //                    try
            //                    {
            //                        var value = await arg.JsonValueAsync(); // ????? ???? ???????
            //                        Console.WriteLine($"[JS_CONSOLE_ARG] Value: {value}");
            //                    }
            //                    catch (Exception ex)
            //                    {
            //                        Console.WriteLine($"[ERROR] [JS_CONSOLE] Failed to serialize argument: {ex.Message}");
            //                    }
            //                }
            //            }


            //            await Task.CompletedTask;
            //        };
            Console.WriteLine("[SUCCESS] Console logging set up");




            Console.WriteLine("[INFO] Setting page content...");
            await page.SetContentAsync(html

            // , new()
            // {
            //     Timeout = 15 * 1000, // 15 seconds in milliseconds
            //     WaitUntil = new[] { WaitUntilNavigation.DOMContentLoaded
            //     //, WaitUntilNavigation.Networkidle0,WaitUntilNavigation.Load
            //     }
            // }

            );

            Console.WriteLine("[SUCCESS] Content set successfully");

            Console.WriteLine("[INFO] Waiting for fonts to load...");
            await page.EvaluateExpressionHandleAsync("document.fonts.ready");
            Console.WriteLine("[SUCCESS] Fonts loaded");








            Console.WriteLine("[INFO] Executing additional evaluations...");
            //    await page.EvaluateExpressionHandleAsync(@"
            //    console.log('Browser console: Page initialization complete');
            //    // Add any additional client-side logging here
            //");


            //     string imageName2 = @"/imagesquare.png"; // Dynamically determined
            //     string imageurl222 = GetHttpUrl(imageName2); // ????? ???? ?????? ???????

            //     // ????? JavaScript ??????? ?? ????? ???????
            //     string script = @$"
            //     (function () {{
            //         const images = document.querySelectorAll('img');
            //         const fallbackImageUrl = '{imageurl222}'; // URL ?????? ???????

            //         images.forEach(img => {{
            //             img.onerror = function () {{
            //                 console.log('Image failed to load', this.src);
            //                 this.onerror = null; // ??? ?????? ?????????? ??? ???? ?????? ??????? ?????

            //                 // ????? ?????? ???????
            //                 this.src = fallbackImageUrl;

            //                 // ????? ???? ?????? (alt) ???? ??? ?????
            //                 this.alt = 'Image not available. Original source: ' + (this.dataset.originalSrc || this.src);

            //                 // ????? ??? CSS ??????? ????? ???????
            //                 this.classList.add('broken-image');
            //             }};

            //             // ????? ???? ?????? ?????? ?? ??? ???????? ?????? ????? ??????
            //             if (!img.dataset.originalSrc) {{
            //                 img.dataset.originalSrc = img.src;
            //             }}
            //         }});
            //     }})();
            // ";

            //     await page.EvaluateExpressionAsync(script);

            if (delay != null)
            {
                Console.WriteLine($"[INFO] Applying delay of {delay.Value.TotalMilliseconds}ms...");
                await Task.Delay(delay.Value);
                Console.WriteLine("[SUCCESS] Delay completed");
            }

            Console.WriteLine("[INFO] Page initialization complete");
            return page;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Error in GetPageWithHtml: {ex.Message}");
            Console.WriteLine($"[DEBUG] Stack Trace: {ex.StackTrace}");

            try
            {
                // Attempt to capture more context from the browser
                if (page != null)
                {
                    var pageContent = await page.GetContentAsync();
                    Console.WriteLine($"[DEBUG] Page content (first 500 chars): {pageContent?.Substring(0, Math.Min(500, pageContent.Length))}");

                    var consoleMessages = await page.EvaluateExpressionAsync<string[]>("Array.from(console._messages).map(m => m.text)");
                    Console.WriteLine($"[DEBUG] Browser console messages: {string.Join("\n", consoleMessages ?? new string[0])}");
                }
            }
            catch (Exception innerEx)
            {
                Console.WriteLine($"[WARNING] Failed to gather additional debug info: {innerEx.Message}");
            }

            // Clean up if something went wrong
            if (page != null)
            {
                try
                {
                    await page.CloseAsync();
                }
                catch (Exception closeEx)
                {
                    Console.WriteLine($"[WARNING] Failed to close page: {closeEx.Message}");
                }
            }

            throw; // Re-throw the original exception
        }
    }

    // ???? ?????? ???? ?????? ???????
    private static string GetHttpUrl(string imageName)
    {
        // ????? ????? ??? ?????? ?????? ?????? ?????? ??????
        return $"http://localhost:8888{imageName}";
    }



    private static async Task<IPage> GetPageWithHtmlOldData(IBrowser browser, string html, TimeSpan? delay = null)
    {
        IPage page = null;
        try
        {


            Console.WriteLine("[INFO] Creating new browser page...");
            page = await browser.NewPageAsync();
            Console.WriteLine("[SUCCESS] New page created successfully");

            Console.WriteLine("[INFO] Setting page content...");
            await page.SetContentAsync(html, new()
            {

                WaitUntil = new[] { WaitUntilNavigation.DOMContentLoaded, WaitUntilNavigation.Networkidle0 }

            }).ConfigureAwait(false);

            Console.WriteLine("[SUCCESS] Content set successfully");

            Console.WriteLine("[INFO] Waiting for fonts to load...");
            await page.EvaluateExpressionHandleAsync("document.fonts.ready");
            Console.WriteLine("[SUCCESS] Fonts loaded");

            Console.WriteLine("[INFO] Executing additional evaluations...");
            await page.EvaluateExpressionHandleAsync(@"
            console.log('Browser console: Page initialization complete');
            // Add any additional client-side logging here
        ");



            string imageName2 = @"/imagesquare.png"; // Dynamically determined




            string imageurl222 = GetHttpUrl(imageName2); // ????? ???? ?????? ???????

            // ????? JavaScript ??????? ?? ????? ???????
            string script = @$"
            (function () {{
                const images = document.querySelectorAll('img');
                const fallbackImageUrl = '{imageurl222}'; // URL ?????? ???????

                images.forEach(img => {{
                    img.onerror = function () {{
                        console.log('Image failed to load', this.src);
                        this.onerror = null; // ??? ?????? ?????????? ??? ???? ?????? ??????? ?????

                        // ????? ?????? ???????
                        this.src = fallbackImageUrl;

                        // ????? ???? ?????? (alt) ???? ??? ?????
                        this.alt = 'Image not available. Original source: ' + (this.dataset.originalSrc || this.src);

                        // ????? ??? CSS ??????? ????? ???????
                        this.classList.add('broken-image');
                    }};

                    // ????? ???? ?????? ?????? ?? ??? ???????? ?????? ????? ??????
                    if (!img.dataset.originalSrc) {{
                        img.dataset.originalSrc = img.src;
                    }}
                }});
            }})();
        ";

            await page.EvaluateExpressionAsync(script);




            if (delay != null)
            {
                Console.WriteLine($"[INFO] Applying delay of {delay.Value.TotalMilliseconds}ms...");
                await Task.Delay(delay.Value);
                Console.WriteLine("[SUCCESS] Delay completed");
            }

            Console.WriteLine("[INFO] Page initialization complete");
            return page;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Error in GetPageWithHtml: {ex.Message}");
            Console.WriteLine($"[DEBUG] Stack Trace: {ex.StackTrace}");

            try
            {
                // Attempt to capture more context from the browser
                if (page != null)
                {
                    var pageContent = await page.GetContentAsync();
                    Console.WriteLine($"[DEBUG] Page content (first 500 chars): {pageContent?.Substring(0, Math.Min(500, pageContent.Length))}");

                    var consoleMessages = await page.EvaluateExpressionAsync<string[]>("Array.from(console._messages).map(m => m.text)");
                    Console.WriteLine($"[DEBUG] Browser console messages: {string.Join("\n", consoleMessages ?? new string[0])}");
                }
            }
            catch (Exception innerEx)
            {
                Console.WriteLine($"[WARNING] Failed to gather additional debug info: {innerEx.Message}");
            }

            // Clean up if something went wrong
            if (page != null)
            {
                try
                {
                    await page.CloseAsync();
                }
                catch (Exception closeEx)
                {
                    Console.WriteLine($"[WARNING] Failed to close page: {closeEx.Message}");
                }
            }

            throw; // Re-throw the original exception
        }
    }











    private async Task SendViaRawTcp(byte[] pdfData, string ipAddress, int port)
    {
        try
        {
            using var client = new TcpClient();
            await client.ConnectAsync(ipAddress, port);
            await using var stream = client.GetStream();
            await stream.WriteAsync(pdfData, 0, pdfData.Length);
        }
        catch (SocketException ex)
        {
            //Logging.Error("Socket error: {Message}", ex.Message);
            Log.Error("Socket error: {Message}", ex.Message);
            logger.LogError(ex, "Socket error: {Message}", ex.Message);
            throw;
        }
    }
    public byte[] Hex2ByteArr(string hexString)
    {
        hexString = hexString.Replace(" ", "");
        int length = hexString.Length;
        byte[] bytes = new byte[length / 2];
        for (int i = 0; i < length; i += 2)
        {
            bytes[i / 2] = Convert.ToByte(hexString.Substring(i, 2), 16);
        }
        return bytes;
    }




    public async Task PrintPdfByUrlAsync(string url, PrinterSettings settings, PdfOptions? options = null, TimeSpan? delay = null)
    {
        var pdfBytes = await GetPdfByUrlAsync(url, options, delay);
        await PrintPdfAsync(pdfBytes, settings);
    }

    public async Task PrintPdfByHtmlAsync(string html, PrinterSettings settings, PdfOptions? options = null, TimeSpan? delay = null)
    {
        var pdfBytes = await GetPdfByHtmlAsync(html, options, delay);
        await PrintPdfAsync(pdfBytes, settings);
    }

    private async Task PrintPdfAsync(byte[] pdfData, PrinterSettings settings)
    {
        if (!string.IsNullOrEmpty(settings.IpAddress))
        {
            await PrintViaNetworkAsync(pdfData, settings);
        }
        else if (!string.IsNullOrEmpty(settings.PrinterName))
        {

            await SendViaSystemPrinter(pdfData, settings.PrinterName);


            //PrintViaLocal(pdfData, settings);

        }
        else
        {
            throw new ArgumentException("Invalid printer settings: Provide either IP or Printer Name");
        }
    }

    private async Task PrintViaNetworkAsync(byte[] pdfData, PrinterSettings settings)
    {
        using var client = new TcpClient();
        await client.ConnectAsync(settings.IpAddress, settings.Port);
        using var stream = client.GetStream();
        await stream.WriteAsync(pdfData, 0, pdfData.Length);
    }



    // Existing PDF generation methods remain unchanged


    public async Task PrintByUrlAsync(
        string url,
        string printerIp = null,
        int port = 9100,
        string printerName = null,
        string username = null,
        string password = null,
        PdfOptions options = null,
        TimeSpan? delay = null)
    {
        var pdfData = await GetPdfByUrlAsync(url, options, delay);
        await SendToPrinter(pdfData, printerIp, port, printerName, username, password);
    }

    public async Task PrintByHtmlAsync(
        string html,
        string printerIp = null,
        int port = 9100,
        string printerName = null,
        string username = null,
        string password = null,
        PdfOptions options = null,
        TimeSpan? delay = null)
    {
        var pdfData = await GetPdfByHtmlAsync(html, options, delay);
        await SendToPrinter(pdfData, printerIp, port, printerName, username, password);
    }

    private async Task SendToPrinter(
        byte[] pdfData,
        string printerIp,
        int port,
        string printerName,
        string username,
        string password)
    {
        try
        {
            if (!string.IsNullOrEmpty(printerIp))
            {
                if (string.IsNullOrEmpty(username) && string.IsNullOrEmpty(password))
                {
                    await SendViaRawTcp(pdfData, printerIp, port);
                }
                else
                {
                    await SendViaIpp(pdfData, printerIp, port, username, password);
                }
            }
            else if (!string.IsNullOrEmpty(printerName))
            {
                await SendViaSystemPrinter(pdfData, printerName);
            }
            else
            {
                throw new ArgumentException("Either printer IP or printer name must be specified");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error sending PDF to printer: {ErrorText}", ex.ToString());
            throw new InvalidOperationException("Failed to send PDF to printer", ex);
        }
    }



    private async Task SendViaIpp(byte[] pdfData, string ipAddress, int port, string username, string password)
    {
        using var client = new HttpClient();
        var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);

        var content = new ByteArrayContent(pdfData);
        content.Headers.ContentType = new MediaTypeHeaderValue("Application/pdf");

        var printerUri = $"Http://{ipAddress}:{port}/Printers/default";
        var response = await client.PostAsync(printerUri, content);
        response.EnsureSuccessStatusCode();
    }

    private async Task SendViaSystemPrinterV2(byte[] pdfData, string printerName)
    {

        try
        {


            var printDocument = new PrintDocument
            {
                PrinterSettings = { PrinterName = printerName,Copies=1,

             PrintFileName= "WaterAll.pdf",
             PrintToFile=true,

             },
            };

            using var stream = new MemoryStream(pdfData);
            printDocument.PrintController = new StandardPrintController();
            printDocument.PrintPage += (sender, e) =>
            {
                using var image = Image.FromStream(stream, true);
                e.Graphics.DrawImage(image, e.PageBounds);
                // e.PageSettings.Color
            };
            printDocument.DocumentName = "WaterAll";

            await Task.Run(() => printDocument.Print());

        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("Error sending PDF to printer: {0}", ex.Message);

            logger.LogError(ex, "Error sending PDF to printer: {ErrorText}", ex.ToString());
            throw new InvalidOperationException("Failed to send PDF to printer " + ex.Message, ex);
        }


    }










    public async Task SendViaSystemPrinter(byte[] pdfData, string printerName)
    {
        try
        {


            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                await PrintOnWindows(pdfData, printerName);
            }
            else
            {
                await PrintOnLinux(pdfData, printerName);
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error sending PDF to printer: {ex.Message}");
            // Log the exception as needed
            throw new InvalidOperationException($"Failed to send PDF to printer: {ex.Message}", ex);
        }
    }







    private async Task PrintOnWindows(byte[] pdfData, string printerName)
    {

        var tempFile = Path.GetTempFileName();
        try
        {
            await File.WriteAllBytesAsync(tempFile, pdfData);




            var printDocument = new PrintDocument
            {
                PrinterSettings =
            {
                    
                PrinterName = printerName,
                    // PrinterResolutions=  new System.Drawing.Printing.PrinterSettings.PrinterResolutionCollection(){} ,
                 PrintToFile=true,
                Copies = 1,
                PrintFileName="WaterAll.pdf"
            }
            };

            // using var pdfStream = new MemoryStream(pdfData);
            using var document = PdfiumViewer.PdfDocument.Load(tempFile);



            int pageCount = document.PageCount;
            int currentPage = 0;

            printDocument.PrintController = new StandardPrintController();
            printDocument.PrintPage += (sender, e) =>
            {
                if (currentPage >= pageCount)
                {
                    e.HasMorePages = false;
                    return;
                }

                // Render the current page at the printer's DPI
                using var pageImage = document.Render(
                    currentPage,
                    (int)e.Graphics.DpiX,
                    (int)e.Graphics.DpiY,
                    forPrinting: true
                );

                // Adjust the image to fit within the printable area
                var aspectRatio = (double)pageImage.Width / pageImage.Height;
                var printArea = e.MarginBounds;
                var printWidth = printArea.Width;
                var printHeight = (int)(printArea.Width / aspectRatio);

                if (printHeight > printArea.Height)
                {
                    printHeight = printArea.Height;
                    printWidth = (int)(printArea.Height * aspectRatio);
                }

                e.Graphics.DrawImage(
                    pageImage,
                    new Rectangle(
                        printArea.Left + (printArea.Width - printWidth) / 2,
                        printArea.Top + (printArea.Height - printHeight) / 2,
                        printWidth,
                        printHeight
                    )
                );

                currentPage++;
                e.HasMorePages = currentPage < pageCount;
            };

            printDocument.DocumentName = "WaterAll";
            await Task.Run(() => printDocument.Print());
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("Error sending PDF to printer: {0}", ex.Message);
            logger.LogError(ex, "Error sending PDF to printer: {ErrorText}", ex.ToString());
            throw new InvalidOperationException("Failed to send PDF to printer: " + ex.Message, ex);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }








    public async Task PrintPdfOnLinux(byte[] pdfData, string printerName)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "lp",
            Arguments = $"-d {printerName} -o raw",
            RedirectStandardInput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (var process = new Process { StartInfo = startInfo })
        {
            process.Start();



            string text = Encoding.UTF8.GetString(pdfData);
            char[] charArray = text.ToCharArray();


            await process.StandardInput.WriteAsync(charArray, 0, pdfData.Length);
            process.StandardInput.Close();
            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                throw new Exception($"Failed to print PDF. Exit code: {process.ExitCode}");
            }
        }
    }



    private async Task PrintOnLinux(byte[] pdfData, string printerName)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "lp",
            Arguments = $"-d {printerName} -o raw",
            RedirectStandardInput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (var process = new Process { StartInfo = startInfo })
        {
            process.Start();


            string text = Encoding.UTF8.GetString(pdfData);
            char[] charArray = text.ToCharArray();



            await process.StandardInput.WriteAsync(charArray, 0, pdfData.Length);
            process.StandardInput.Close();
            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                throw new Exception($"Failed to print PDF. Exit code: {process.ExitCode}");
            }
        }
    }




    private async Task SendViaSystemPrinterV1(byte[] pdfData, string printerName)
    {
        try
        {
            // Load the PDF document using PdfiumViewer
            using var pdfStream = new MemoryStream(pdfData);
            using var pdfDocument = PdfiumViewer.PdfDocument.Load(pdfStream);

            // Create a PrintDocument instance
            var printDocument = new PrintDocument
            {
                PrinterSettings = { PrinterName = printerName, Copies = 1 },
                DocumentName = "WaterAll"
            };

            // Set up the PrintController to suppress dialogs
            printDocument.PrintController = new StandardPrintController();

            // Track the current page index
            int currentPageIndex = 0;

            // Handle the PrintPage event to render each page of the PDF
            printDocument.PrintPage += (sender, e) =>
            {
                if (currentPageIndex < pdfDocument.PageCount)
                {
                    // Render the current page as an image
                    using var image = pdfDocument.Render(currentPageIndex, 300, 300, true); // 300 DPI resolution
                    e.Graphics.DrawImage(image, e.PageBounds);

                    // Move to the next page
                    currentPageIndex++;

                    // Indicate whether there are more pages to print
                    e.HasMorePages = currentPageIndex < pdfDocument.PageCount;
                }
            };

            // Print the document asynchronously
            await Task.Run(() => printDocument.Print());


            await Task.Run(() => pdfDocument.CreatePrintDocument(PdfiumViewer.PdfPrintMode.ShrinkToMargin));


        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("Error sending PDF to printer: {0}", ex.Message);
            logger.LogError(ex, "Error sending PDF to printer: {ErrorText}", ex.ToString());
            throw new InvalidOperationException("Failed to send PDF to printer: " + ex.Message, ex);
        }
    }


}










public class PrintLPTUtility
{
    [StructLayout(LayoutKind.Sequential)]
    private struct OVERLAPPED
    {
        private int Internal;
        private int InternalHigh;
        private int Offset;
        private int OffSetHigh;
        private int hEvent;
    }

    #region Directive definition


    private static Byte[] Const_Init = new byte[] { 0x1B, 0x40,
            0x20, 0x20, 0x20, 0x0A,
            0x1B, 0x64,0x10};

    //Set the left margin

    private const string Const_SetLeft = "1D 4C ";
    //Set up printer initialization

    private const string Const_Reset = "1b 40 ";


    //Set bold

    private const string Const_SetBold = "1b 45 ";
    private const String Const_Bold_YES = "01";
    private const String Const_Bold_NO = "00";


    //Setting the alignment

    private const string Const_SetAlign = "1b 61 ";
    private const String Const_Align_Left = "30";
    private const String Const_Align_Middle = "31";
    private const String Const_Align_Right = "32";

    //Set the font size, and cannot be used at the same time as SetBigFont

    private const string Const_SetFontSize = "1D 21 ";

    //Set whether to set large fonts, equivalent to SetFontSize = 2
    //private const String Const_SetBigFontBold = "1B 21 38";
    //private const String Const_SetBigFontNotBold = "1B 21 30";
    //private const String Const_SetCancelBigFont = "1B 21 00";


    /// <summary>
    /// Print and paper
    /// </summary>

    private static Byte[] Const_Cmd_Print = new byte[] { 0x1B, 0x4A, 0x00 };
    //Set the font size

    private const string Const_SizeB = "1D 21 00";
    //Leave paper

    private const string Const_FeedForward = "1b 64 ";
    private const string Const_FeedBack = "1B 6A ";
    //Open the money box

    private const string Const_OpenMbox = "1B 70 00 3C 3F";
    //Cut paper

    private static Byte[] Const_SetCut = new byte[] { 0x1D, 0x56, 0x30 };

    //Query the printer status

    private static Byte[] Const_QueryID = new byte[] { 0x1D, 0x67, 0x61 };

    //The reply frame starts with an ID 

    private static String Const_ResponseQueryID = "Id";

    /// <summary>
    /// Set icon commands
    /// </summary>

    private static Byte[] Const_SetImageCommand = new Byte[] { 0x1B, 0x2A, 0x21 };

    /// <summary>
    /// Alignment
    /// </summary>

    public enum eTextAlignMode
    {
        Left = 0,
        Middle = 1,
        Right = 2
    }

    #endregion

    #region Constant definition


    /// <summary>
    /// Maximum font size
    /// </summary>

    public const Int32 Const_MaxFontSize = 8;
    /// <summary>
    /// Maximum paper-trapping distance
    /// </summary>

    public const Int32 Const_MaxFeedLength = 5000;

    /// <summary>
    /// Maximum height and width
    /// </summary>

    public const Int32 Const_MaxImageLength = 480;

    /// <summary>
    /// The maximum number of lines printed per communication
    /// </summary>

    public const Int32 Const_OncePrintRowCount = 24;

    public const Int32 Const_BrightnessGate = 100;

    /// <summary>
    /// Invalid handle
    /// </summary>

    public const Int32 Const_InvalidHandle = -1;
    #endregion

    #region Private Members


    /// <summary>
    /// Printer handle
    /// </summary>

    private int m_Handle = -1;

    /// <summary>
    /// Is it initialized
    /// </summary>

    private Boolean m_Inited = false;


    #endregion

    #region Private functions


    [DllImport("Kernel32.dll")]
    private static extern bool CloseHandle(int hObject);
    [DllImport("Kernel32.dll")]
    private static extern int CreateFile(string lpFileName, uint dwDesiredAccess, int dwShareMode, int lpSecurityAttributes, int dwCreationDisposition, int dwFlagsAndAttributes, int hTemplateFile);
    [DllImport("Kernel32.dll")]
    private static extern bool WriteFile(int hFile, byte[] lpBuffer, int nNumberOfBytesToWrite, out int lpNumberOfBytesWritten, out OVERLAPPED lpOverlapped);


    /// <summary>
    /// Send command
    /// </summary>
    /// <param name="Cmd"></param>
    /// <returns></returns>

    private Boolean SendCommand(Byte[] cmd)
    {
        if (m_Handle == Const_InvalidHandle || cmd == null || cmd.Length < 2)
        {
            return false;
        }
        OVERLAPPED overlapped;
        int writelen = 0;
        Boolean bl = WriteFile(m_Handle, cmd, cmd.Length, out writelen, out overlapped);

        if (!bl)
            return false;
        return (writelen >= cmd.Length);
    }

    /// <summary>
    /// Send text format instructions
    /// </summary>
    /// <param name="Cmd"></param>
    /// <returns></returns>

    private Boolean SendCommand(String hexstrcmd)
    {
        if (m_Handle == Const_InvalidHandle || hexstrcmd == null || hexstrcmd.Length < 4)
        {
            return false;
        }

        byte[] mybyte = Hex2ByteArr(hexstrcmd);
        Boolean bl = SendCommand(mybyte);
        return bl;
    }
    /// <summary>
    /// Instruction conversion
    /// </summary>
    /// <param name="New string"></param>
    /// <returns></returns>

    public byte[] Hex2ByteArr(string newString)
    {
        newString = newString.Replace(" ", "");
        int num = newString.Length / 2;
        byte[] buffer = new byte[num];
        for (int i = 0; i < num; i++)
        {
            buffer[i] = Convert.ToByte(newString.Substring(i * 2, 2), 0x10);
        }
        return buffer;
    }

    #endregion

    #region Expose functions


    /// <summary>
    /// Turn on the printer
    /// </summary>
    /// <param name="Printername"></param>
    /// <returns></returns>

    public Boolean Open(String lptname)
    {
        bool flag = false;
        if (m_Inited)
        {
            flag = true;
        }
        m_Handle = CreateFile(lptname, 3, 0, 0, 3, 0, 0);
        if (m_Handle != -1)
        {
            m_Inited = (m_Handle != 0);
            flag = true;
        }
        return flag;
    }

    /// <summary>
    /// ??
    /// </summary>
    /// <returns></returns>

    public Boolean Close()
    {
        if (!m_Inited)
        {
            return true;
        }
        m_Inited = false;

        SendCommand(Const_Reset);
        //Close the device handle

        CloseHandle(m_Handle);
        m_Handle = -1;
        return true;
    }

    /// <summary>
    /// Print text. Before calling this function, you must call the correct font and left margin.
    /// </summary>
    /// <param name="Content"></param>
    /// <returns></returns>

    public Boolean PrintText(String content)
    {
        if (!m_Inited)
        {
            return false;
        }

        byte[] bytes = null;
        if (content.Length < 1)
        {
            content = "  ";
        }

        if (content[content.Length - 1] != (char)0x0D &&
            content[content.Length - 1] != (char)0x0A)
        {
            content = content + (char)0x0A;
        }

        bytes = Encoding.Default.GetBytes(content);
        bool bl = SendCommand(bytes);
        return bl;
    }

    /// <summary>
    /// Setting the alignment
    /// </summary>
    /// <param name="Left"></param>
    /// <returns></returns>

    public bool SetAlignMode(eTextAlignMode alignmode)
    {
        if (!m_Inited)
        {
            return false;
        }

        String code = String.Empty;
        switch (alignmode)
        {
            case eTextAlignMode.Left:
                code = Const_Align_Left;
                break;
            case eTextAlignMode.Middle:
                code = Const_Align_Middle;
                break;
            case eTextAlignMode.Right:
                code = Const_Align_Right;
                break;
            default:
                code = Const_Align_Left;
                break;
        }

        //Note: Lower byte first and then high byte 

        string str = Const_SetAlign + code;
        bool bl = SendCommand(str);
        return bl;
    }

    /// <summary>
    /// Set the left margin
    /// </summary>
    /// <param name="Left"></param>
    /// <returns></returns>

    public bool SetLeft(int left)
    {
        if (!m_Inited)
        {
            return false;
        }

        //Note: Lower byte first and then high byte

        String hexstr = left.ToString("Kch");
        string str = Const_SetLeft + hexstr.Substring(2, 2) + hexstr.Substring(0, 2);
        bool bl = SendCommand(str);
        return bl;
    }

    /// <summary>
    /// Set bold
    /// </summary>
    /// <param name="Bold"></param>
    /// <returns></returns>

    public Boolean SetBold(Boolean bold)
    {
        if (!m_Inited)
        {
            return false;
        }

        //Note: Lower byte first and then high byte

        String str = String.Empty;
        if (bold)
        {
            str = Const_SetBold + Const_Bold_YES;
        }
        else
        {
            str = Const_SetBold + Const_Bold_NO;
        }
        bool bl = SendCommand(str);
        return bl;
    }

    /// <summary>
    /// ??
    /// </summary>
    /// <returns></returns>

    public bool Cut()
    {
        if (!m_Inited)
        {
            return false;
        }
        bool bl = SendCommand(Const_SetCut);
        return bl;
    }
    /// <summary>
    /// Open the money box
    /// </summary>
    /// <returns></returns>

    public bool OpenMbox()
    {
        if (!m_Inited)
        {
            return false;
        }
        bool bl = SendCommand(Const_OpenMbox);
        return bl;
    }
    /// <summary>
    /// ??
    /// </summary>
    /// <param name="Length"></param>
    /// <returns></returns>

    public bool Feed(int length)
    {
        if (!m_Inited)
        {
            return false;
        }
        if (length < 1)
            length = 1;
        //if (length > Const_MaxFeedLength)
        //{
        //    length = Const_MaxFeedLength;
        //}

        string len = length.ToString("Cover");
        len = Const_FeedForward + len;
        bool bl = SendCommand(len);
        return bl;
    }

    /// <summary>
    /// Rewind paper
    /// </summary>
    /// <param name="Length"></param>
    /// <returns></returns>

    public bool FeedBack(int length)
    {
        if (!m_Inited)
        {
            return false;
        }
        if (length < 1)
            length = 1;
        if (length > Const_MaxFeedLength)
        {
            length = Const_MaxFeedLength;
        }
        string len = length.ToString("Cover");
        len = Const_FeedBack + len;
        bool bl = SendCommand(len);
        return bl;
    }

    /// <summary>
    /// Set the font size. This function cannot be used simultaneously with SetBigFont
    /// </summary>
    /// <param name="Sizerate">Size magnification, value range 1 -8</param>
    /// <returns></returns>

    public bool SetFontSize(Int32 sizerate)
    {
        if (!m_Inited)
        {
            return false;
        }

        if (sizerate < 1)
        {
            sizerate = 1;
        }

        if (sizerate > Const_MaxFontSize)
        {
            sizerate = Const_MaxFontSize;
        }
        sizerate--;
        String sizecodestr = Const_SetFontSize + sizerate.ToString("X1") + sizerate.ToString("X1");
        bool bl = SendCommand(sizecodestr);
        return bl;
    }

    public bool SetFontSizeB()
    {
        bool bl = SendCommand(Const_SizeB);
        return bl;
    }

    #endregion
}




/// <summary>
/// Printer parameters
/// </summary>

public class WinPrinterParameter
{
    private Int32 _fontSize = 10;
    /// <summary>
    /// Text size
    /// </summary>

    public Int32 FontSize
    {
        get
        {
            return _fontSize;
        }
        set
        {
            _fontSize = value;
        }
    }
    private String _fontFamily = "Song style";
    /// <summary>
    /// Text Fonts
    /// </summary>

    public String FontFamily
    {
        get
        {
            return _fontFamily;
        }
        set
        {
            _fontFamily = value;
        }
    }
    private FontStyle _fontStyle = FontStyle.Regular;
    /// <summary>
    /// Text format
    /// </summary>

    public FontStyle FontStyle
    {
        get
        {
            return _fontStyle;
        }
        set
        {
            _fontStyle = value;
        }
    }
    private String _fontText = String.Empty;
    /// <summary>
    /// Font text
    /// </summary>

    public String FontText
    {
        get
        {
            return _fontText;
        }
        set
        {
            _fontText = value;
        }
    }
    private TextAlign _textAlign = TextAlign.Left;
    /// <summary>
    /// Text alignment
    /// </summary>

    public TextAlign TextAlign
    {
        get { return _textAlign; }
        set { _textAlign = value; }
    }
}
/// <summary>
/// The way the text is
/// </summary>

public enum TextAlign
{
    Left = 0,
    Center = 1,
    Right = 2
}







/// <summary>
/// Printing tool class
/// </summary>

public class PrinterUtility
{
    #region Singleton mode

    /// <summary>
    /// Entity lock
    /// </summary>

    private static object sync = new object();
    private static PrinterUtility _instance;
    /// <summary>
    /// Private constructor
    /// </summary>

    private PrinterUtility()
    {

    }
    /// <summary>
    /// entity
    /// </summary>

    public static PrinterUtility Instance
    {
        get
        {
            lock (sync)
            {
                if (_instance == null)
                {
                    lock (sync)
                    {
                        _instance = new PrinterUtility();
                    }
                }
            }
            return _instance;
        }
    }
    #endregion
    #region Public Methods

    /// <summary>
    /// Print out the output on the draft printer
    /// </summary>
    /// <param name="Parameters">Print content list</param>
    /// <param name="Printer name">Printer name</param>
    /// <param name="Err message">Error message, if there is</param>
    /// <returns>Successfully true Otherwise false</returns>

    public bool Print(List<WinPrinterParameter> _parameters, String printerName, int _width, ref String errMessage)
    {




        bool isok = false;
        //PrintDialog dialog = null;
        PrintDocument document = null;
        try
        {
            //dialog = new PrintDialog();
            document = new PrintDocument() { DocumentName = "Pos" };
            document.PrinterSettings.PrinterName = printerName;
            document.DefaultPageSettings.Margins = new Margins(0, 0, 0, 0);
            document.PrintController = new StandardPrintController();
            document.PrintPage += (obj, e) =>
            {
                isok = PrintContent(_parameters, 0, e, _width);
            };
            //dialog.Document = document;
            document.Print();
            isok = true;
        }
        catch (Exception ex)
        {
            errMessage = ex.ToString();
        }
        finally
        {
            if (document != null)
            {
                document.Dispose();
            }
            //if (dialog != null)
            //{
            //    dialog.Dispose();
            //}
        }
        return isok;
    }

    #endregion
    #region Private method


    /// <summary>
    /// Print out content
    /// </summary>
    /// <param name="Parameter">Print content list</param>
    /// <param name="Start index">Print content list</param>
    /// <param name="Print page event args">Print event parameters</param>

    private bool PrintContent(List<WinPrinterParameter> _paramters, int _startIndex, PrintPageEventArgs e, int _width)
    {
        bool isok = true;
        float y = e.MarginBounds.Y;
        float top = e.MarginBounds.Top;
        float height = 0f;
        try
        {
            for (int index = _startIndex; index < _paramters.Count; index++)
            {
                WinPrinterParameter _para = _paramters[index];
                using (Font font = new Font(_para.FontFamily, _para.FontSize, _para.FontStyle))
                {
                    e.Graphics.DrawString(_para.FontText, font, Brushes.Black, (float)e.MarginBounds.X, y + 10);
                    height = font.GetHeight();
                    y += font.GetHeight();
                }
                //if ((((y + (height *3f)) *96f) /100f) > e.PageSettings.PrintableArea.Height)
                //{
                //    e.HasMorePages = true;
                //    PrintContent(_paramters, index + 1, e, _width);
                //}

            }
        }
        catch
        {
            isok = false;
        }
        return isok;
    }

    #endregion
}
