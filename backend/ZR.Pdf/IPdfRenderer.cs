using PuppeteerSharp;

namespace ZR.Pdf.Core;

public interface IPdfRenderer
{
    Task ShouldHonorThePrice();

    Task<byte[]> GetPdfByUrlAsync(string url, PdfOptions? options = null, TimeSpan? delay = null);
    Task<byte[]> GetPdfByHtmlAsync(string html, PdfOptions? options = null, TimeSpan? delay = null);
    Task<byte[]> GetScreenshotByUrlAsync(string url, ScreenshotOptions? options = null, TimeSpan? delay = null);
    Task<byte[]> GetScreenshotByHtmlAsync(string html, ScreenshotOptions? options = null, TimeSpan? delay = null);


    public Task PrintByUrlAsync(
       string url,
       string printerIp = null,
       int port = 9100,
       string printerName = null,
       string username = null,
       string password = null,
       PdfOptions options = null,
       TimeSpan? delay = null);

    public Task PrintByHtmlAsync(
      string html,
      string printerIp = null,
      int port = 9100,
      string printerName = null,
      string username = null,
      string password = null,
      PdfOptions options = null,
      TimeSpan? delay = null);


    public Task PrintPdfByHtmlAsync(string html, PrinterSettings settings, PdfOptions? options = null, TimeSpan? delay = null);

    public Task PrintPdfByUrlAsync(string url, PrinterSettings settings, PdfOptions? options = null, TimeSpan? delay = null);

}

