using Infrastructure.Extensions;
using Infrastructure.Model;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Security.Claims;

namespace Infrastructure
{
    public static class App
    {
        /// <summary>
        /// Global configuration file
        /// </summary>
        public static OptionsSetting OptionsSetting => CatchOrDefault(() => ServiceProvider?.GetService<IOptions<OptionsSetting>>()?.Value);

        /// <summary>
        /// Service provider
        /// </summary>
        public static IServiceProvider ServiceProvider => InternalApp.ServiceProvider;
        /// <summary>
        /// Get request context
        /// </summary>
        public static HttpContext HttpContext => CatchOrDefault(() => ServiceProvider?.GetService<IHttpContextAccessor>()?.HttpContext);
        /// <summary>
        /// Get request context user
        /// </summary>
        public static ClaimsPrincipal User => HttpContext?.User;
        /// <summary>
        /// Get username
        /// </summary>
        public static string UserName => User?.Identity?.Name;
        /// <summary>
        /// Get web host environment
        /// </summary>
        public static IWebHostEnvironment WebHostEnvironment => InternalApp.WebHostEnvironment; 
        /// <summary>
        /// Get global configuration
        /// </summary>
        public static IConfiguration Configuration => CatchOrDefault(() => InternalApp.Configuration, new ConfigurationBuilder().Build());
        /// <summary>
        /// Get request-scoped service
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <returns></returns>
        public static TService GetService<TService>()
            where TService : class
        {
            return GetService(typeof(TService)) as TService;
        }















        //private static readonly object _lock = new object();
        //private static IPdfRenderer _pdfRenderer = null;

        //public static IPdfRenderer PdfRenderer
        //{
        //    get
        //    {
        //        lock (_lock)
        //        {
        //            try
        //            {
        //                if (_pdfRenderer == null)
        //                {
        //                    var service = App.GetRequiredService<IPdfRenderer>();
        //                    _pdfRenderer = service;
        //                }
        //                return _pdfRenderer;
        //            }
        //            catch (Exception ex)
        //            {
        //                Console.WriteLine(ex.Message);
        //                throw new InvalidOperationException("Failed to get PDF renderer service", ex);
        //            }
        //        }
        //    }
        //    set
        //    {
        //        lock (_lock)
        //        {
        //            try
        //            {
        //                _pdfRenderer = value;
        //            }
        //            catch (Exception ex)
        //            {
        //                throw new InvalidOperationException("Failed to set PDF renderer", ex);
        //            }
        //        }
        //    }
        //}






        /// <summary>
        /// Get request-scoped service
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object GetService(Type type)
        {
            return ServiceProvider.GetService(type);
        }

        /// <summary>
        /// Get required request-scoped service
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <returns></returns>
        public static TService GetRequiredService<TService>()
            where TService : class
        {
            return GetRequiredService(typeof(TService)) as TService;
        }

        /// <summary>
        /// Get required request-scoped service
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object GetRequiredService(Type type)
        {
            return ServiceProvider.GetRequiredService(type);
        }

        /// <summary>
        /// Handle exception when getting objects
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="action">Get object delegate</param>
        /// <param name="defaultValue">Default value</param>
        /// <returns>T</returns>
        private static T CatchOrDefault<T>(Func<T> action, T defaultValue = null)
            where T : class
        {
            try
            {
                return action();
            }
            catch
            {
                return defaultValue ?? null;
            }
        }



        public static String UserIdStr
        {
            get
            {
                return (HttpContext?.GetUId().ToString());
            }
        }




        public static long UserId
        {
            get
            {
                return (long)(HttpContext?.GetUId());
            }
        }



        public static long? branchID
        {
            get
            {

                if (HttpContext == null)
                {

                    return null;
                }
                return (long)(HttpContext?.GetBranchID());
            }
        }






        /// <summary>
        /// Get default tenant ID
        /// </summary>
        /// <returns></returns>
        public static string GetCurrentTenantId()
        {
            var headerId = HttpContext?.Request?.Headers["tenantId"].ToString();
            var claimId = User?.Claims.FirstOrDefault(f => f.Type == ClaimTypes.PrimaryGroupSid)?.Value;
            return !string.IsNullOrEmpty(headerId) ? headerId : (claimId ?? "tenant0");
        }

    }
}

