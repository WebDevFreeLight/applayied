using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;

//Don't change namespace, would require modifying many places
namespace ZR.Common
{
    public class CacheHelper
    {
        public static MemoryCache Cache { get; set; }
        private static readonly List<string> _keys;
        static CacheHelper()
        {
            Cache = new MemoryCache(new MemoryCacheOptions
            {
                //SizeLimit = 1024
            });
            _keys = [];
        }

        /// <summary>
        /// Get cache
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public static T GetCache<T>(string key) where T : class
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            //return Cache.Get(key) as T; //??
            return Cache.Get<T>(key);
        }

        /// <summary>
        /// Get cache
        /// </summary>
        /// <param name="CacheKey"></param>
        /// <returns></returns>
        public static object GetCache(string CacheKey)
        {
            return Cache.Get<object>(CacheKey);
        }

        public static object Get(string CacheKey)
        {
            return Cache.Get(CacheKey);
        }

        /// <summary>
        /// Set cache, permanent cache
        /// </summary>
        /// <param name="CacheKey">key</param>
        /// <param name="objObject">value</param>
        public static object SetCache(string CacheKey, object objObject)
        {
            return Cache.Set(CacheKey, objObject);
        }

        /// <summary>
        /// Set cache
        /// </summary>
        /// <param name="CacheKey">key</param>
        /// <param name="objObject">value</param>
        /// <param name="Timeout">expiration time (minutes)</param>
        public static object SetCache(string CacheKey, object objObject, int Timeout)
        {
            return Cache.Set(CacheKey, objObject, DateTime.Now.AddMinutes(Timeout));
        }

        /// <summary>
        /// Set cache (seconds)
        /// </summary>
        /// <param name="CacheKey">key</param>
        /// <param name="objObject">value</param>
        /// <param name="Timeout">expiration time (seconds)</param>
        public static void SetCaches(string CacheKey, object objObject, int Timeout)
        {
            if (!_keys.Contains(CacheKey))
            {
                _keys.Add(CacheKey);
            }
            Cache.Set(CacheKey, objObject, DateTime.Now.AddSeconds(Timeout));
        }

        /// <summary>
        /// Set cache
        /// </summary>
        /// <param name="CacheKey">key</param>
        /// <param name="objObject">value</param>
        /// <param name="absoluteExpiration">expiration time</param>
        /// <param name="slidingExpiration">expiration time interval</param>
        public static object SetCache(string CacheKey, object objObject, DateTime absoluteExpiration, TimeSpan slidingExpiration)
        {
            return Cache.Set(CacheKey, objObject, absoluteExpiration);
        }

        /// <summary>
        /// Set absolute expiration time
        /// </summary>
        /// <param name="CacheKey"></param>
        /// <param name="objObject"></param>
        /// <param name="Seconds">seconds after which it expires</param>
        public static void SetCacheDateTime(string CacheKey, object objObject, long Seconds)
        {
            Cache.Set(CacheKey, objObject, DateTime.Now.AddSeconds(Seconds));
        }

        /// <summary>
        /// Delete cache
        /// </summary>
        /// <param name="key">key</param>
        public static void Remove(string key)
        {
            _keys.Remove(key);
            Cache.Remove(key);
        }

        /// <summary>
        /// Verify if cache item exists
        /// </summary>
        /// <param name="key">cache key</param>
        /// <returns></returns>
        public static bool Exists(string key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            return Cache.TryGetValue(key, out _);
        }


        /// <summary>
        /// Get all cache keys
        /// </summary>
        /// <returns></returns>
        //public static List<string> GetCacheKeys()
        //{
        //    const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        //    //var entries = Cache.GetType().GetField("_entries", flags).GetValue(Cache);

        //    //.net7????? 
        //    var coherentState = Cache.GetType().GetField("_coherentState", flags).GetValue(Cache);
        //    var entries = coherentState.GetType().GetField("_entries", flags).GetValue(coherentState);

        //    var keys = new List<string>();
        //    if (entries is not IDictionary cacheItems) return keys;
        //    foreach (DictionaryEntry cacheItem in cacheItems)
        //    {
        //        keys.Add(cacheItem.Key.ToString());
        //        //Console.WriteLine("cache key=" +cacheItem.Key);
        //    }
        //    return keys;
        //}
        public static List<string> GetCacheKeys()
        {
            return new List<string>(_keys);
        }

        // Dispose cache
        public void Dispose()
        {
            Cache.Dispose();
        }
    }
}


