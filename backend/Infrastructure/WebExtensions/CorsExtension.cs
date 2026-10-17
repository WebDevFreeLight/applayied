using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace Infrastructure
{
    /// <summary>
    /// CORS Extension
    /// </summary>
    public static class CorsExtension
    {
        /// <summary>
        /// CORS Configuration
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public static void AddCors(this IServiceCollection services, IConfiguration configuration)
        {
            //var corsUrls = configuration.GetSection("corsUrls").Get<string[]>();















            var corsUrls = configuration.GetSection("corsUrls").Get<string[]>() ?? Array.Empty<string>();



            //var newUrls = Enumerable.Range(4000, 9000)
            //    .SelectMany(port => new[]
            //    {
            //        $"http://localhost:{port}",
            //        $"https://localhost:{port}"
            //    })
            //    .Where(url => !corsUrls.Contains(url))
            //    .ToArray();

            var newUrls = Enumerable.Range(80, 9000)
                    .SelectMany(port => new[]
                    {
                            $"http://localhost:{port}",
                            $"https://localhost:{port}",
                            $"http://127.0.0.1:{port}",
                            $"https://127.0.0.1:{port}"
                    })
    // Only add URLs that aren't already in corsUrls
    .Where(url => !corsUrls.Contains(url))
    .ToList();
            newUrls.Add("http://srv1331506.hstgr.cloud");
            newUrls.Add("https://srv1331506.hstgr.cloud");
            newUrls.Add("http://srv1331506.hstgr.cloud:8080");
            newUrls.Add("http://onemay.sa:8081");

            var updatedCorsUrls = corsUrls.Concat(newUrls).ToArray();




            //Configure CORS all
            services.AddCors(c =>
            {
                c.AddPolicy("Policy", policy =>
                {
                    policy.WithOrigins(updatedCorsUrls ?? Array.Empty<string>())
                    .AllowAnyHeader()//Allow any header
                    .AllowCredentials()//Allow cookies
                    .AllowAnyMethod();//Allow any method
                });
            });
        }
    }
}

