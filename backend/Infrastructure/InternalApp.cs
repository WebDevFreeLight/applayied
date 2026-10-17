using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System;

namespace Infrastructure
{
    public static class InternalApp
    {
        /// <summary>
        /// ????
        /// </summary>
        public static IServiceProvider ServiceProvider;

        /// <summary>
        /// ???????
        /// </summary>
        public static IConfiguration Configuration;

        /// <summary>
        /// ??Web????
        /// </summary>
        public static IWebHostEnvironment WebHostEnvironment;

        /// <summary>
        /// ????????
        /// </summary>
        //public static IHostEnvironment HostEnvironment;
    }
}

