using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace WangSql.Abstract.Linq
{
    public static class ServiceUtil
    {
        /// <summary>
        /// 使用静态变量注意内存溢出
        /// </summary>
        private static readonly ConcurrentDictionary<string, ConcurrentDictionary<Type, object>> serviceCollection = new ConcurrentDictionary<string, ConcurrentDictionary<Type, object>>();
        private static readonly int serviceCollectionCacheSize = 100;

        public static void AddService(string dbProviderName, Type serviceType, Type implementationType)
        {
            if (!serviceCollection.ContainsKey(dbProviderName))
            {
                serviceCollection.TryAdd(dbProviderName, new ConcurrentDictionary<Type, object>());
            }
            serviceCollection[dbProviderName][serviceType] = Activator.CreateInstance(implementationType);
        }

        public static void AddService<TService, TImplementation>(string dbProviderName) where TImplementation : TService
        {
            AddService(dbProviderName, typeof(TService), typeof(TImplementation));
        }

        public static object GetService(string dbProviderName, Type serviceType)
        {
            if (serviceCollection.ContainsKey(dbProviderName))
            {
                var obj = serviceCollection[dbProviderName][serviceType];
                if (obj != null)
                {
                    return obj;
                }
            }
            return null;
        }

        public static T GetService<T>(string dbProviderName)
        {
            var obj = GetService(dbProviderName, typeof(T));
            return obj == null ? default(T) : (T)obj;
        }
    }
}
