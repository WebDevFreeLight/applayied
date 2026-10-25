using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace ZR.Service.PMO.IPmoService
{
    /// <summary>
    /// Bridge to the Python PMO ML service (ml/ folder, FastAPI). The portal never calls the ML
    /// service from the browser: the .NET API forwards each request, keeps RBAC in one place, and
    /// adds the optional shared secret (X-ML-Key). Responses are passed through as JSON so new
    /// fields in the ML API reach the UI without a .NET change.
    /// </summary>
    public interface IPmoMlService
    {
        /// <summary>True when "ML:BaseUrl" is set (it is by default: http://localhost:8001).</summary>
        bool IsConfigured { get; }

        /// <summary>GET {BaseUrl}/{path}?query. Never throws: failures return { available:false, message }.</summary>
        Task<JToken> GetAsync(string path, IDictionary<string, string> query = null, bool longRunning = false);

        /// <summary>POST {BaseUrl}/{path} with a JSON body. Never throws (same contract as GetAsync).</summary>
        Task<JToken> PostAsync(string path, object body, bool longRunning = false);

        /// <summary>Raw bytes for an EDA / evaluation chart under the ML service's /reports. Null if missing.</summary>
        Task<byte[]> GetChartAsync(string reportPath);
    }
}
