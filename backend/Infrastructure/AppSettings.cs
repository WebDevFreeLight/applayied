using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Infrastructure
{
    public class AppSettings
    {
        static IConfiguration Configuration { get; set; }

        public AppSettings(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// Encapsulates the characters to be operated on
        /// </summary>
        /// <param name="sections">Node configuration</param>
        /// <returns></returns>
        public static string App(params string[] sections)
        {
            try
            {
                if (sections.Any())
                {
                    return Configuration[string.Join(":", sections)];
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return "";
        }

        /// <summary>
        /// Recursively gets configuration information array
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sections"></param>
        /// <returns></returns>
        public static List<T> App<T>(params string[] sections)
        {
            List<T> list = new();
            try
            {
                if (Configuration != null && sections.Any())
                {
                    Configuration.Bind(string.Join(":", sections), list);
                }
            }
            catch
            {
                return list;
            }
            return list;
        }
        public static T Bind<T>(string key, T t)
        {
            Configuration.Bind(key, t);
            return t;
        }


        public static T GetAppConfig<T>(string key, T defaultValue = default)
        {
            T setting = (T)Convert.ChangeType(Configuration[key], typeof(T));
            var value = setting;
            if (setting == null)
                value = defaultValue;
            return value;
        }

        /// <summary>
        /// Get configuration file
        /// </summary>
        /// <param name="key">eg: WeChat:Token</param>
        /// <returns></returns>
        public static string GetConfig(string key)
        {
            return Configuration[key];
        }

        /// <summary>
        /// Get configuration node and convert to specified type
        /// </summary>
        /// <typeparam name="T">Node type</typeparam>
        /// <param name="key">Node path</param>
        /// <returns>Node type instance</returns>
        public static T Get<T>(string key)
        {
            return Configuration.GetSection(key).Get<T>();
        }
    }
}

