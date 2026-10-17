using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace ZR.Infrastructure.WebExtensions
{
    public static class IPRateExtension
    {
        public static void AddIPRate(this IServiceCollection services, IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(services);

            //Load general configuration from appsettings.json, IpRateLimiting corresponds to the node in the configuration file
            services.Configure<IpRateLimitOptions>(configuration.GetSection("IpRateLimiting"));

            //Load IP rules from appsettings.json
            services.Configure<IpRateLimitPolicies>(configuration.GetSection("IpRateLimitPolicies"));
            //Inject counter and rule storage
            services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
            services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
            //Configure (parser, counter key generator)
            services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
            services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();

            services.AddRateLimiter(limiterOptions =>
            {
                // Configure rate limiting strategy
            });
        }
    }
}

