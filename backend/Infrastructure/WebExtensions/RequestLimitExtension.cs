using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ZR.Infrastructure.WebExtensions
{
    public static class RequestLimitExtension
    {
        /// <summary>
        /// Request body size configuration
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public static void AddRequestLimit(this IServiceCollection services, IConfiguration configuration)
        {
            var sizeM = configuration.GetSection("upload:requestLimitSize").Get<int>();
            services.Configure<FormOptions>(x =>
            {
                x.MultipartBodyLengthLimit = sizeM * 1024 * 1024;
                x.MemoryBufferThreshold = sizeM * 1024 * 1024;
                x.ValueLengthLimit = int.MaxValue;
            });
            services.Configure<KestrelServerOptions>(options =>
            {
                options.Limits.MaxRequestBodySize = sizeM * 1024 * 1024;
            });
            services.Configure<IISServerOptions>(options =>
            {
                options.MaxRequestBodySize = sizeM * 1024 * 1024; // Set maximum request body size to 500MB
            });
        }
    }
}

