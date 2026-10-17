using System.Threading.Tasks;
using PuppeteerSharp;

namespace ZR.PuppeteerPDF
{
    /// <summary>
    /// Interface for browser provider
    /// </summary>
    public interface IBrowserProvider
    {
        /// <summary>
        /// Get a new browser instance
        /// </summary>
        Task<IBrowser> GetBrowserAsync();
    }
}

