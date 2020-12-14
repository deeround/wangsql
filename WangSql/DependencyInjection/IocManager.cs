using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace WangSql.DependencyInjection
{
    public static class IocManager
    {
        private static readonly ConcurrentDictionary<string, ConcurrentDictionary<Type, object>> serviceCollection = new ConcurrentDictionary<string, ConcurrentDictionary<Type, object>>();
        private static readonly int serviceCollectionCacheSize = 1000;

        public static void AddService(string dbProviderName, Type serviceType, Type implementationType)
        {
            if (!serviceCollection.ContainsKey(dbProviderName)) serviceCollection.TryAdd(dbProviderName, new ConcurrentDictionary<Type, object>());

            serviceCollection[dbProviderName][serviceType] = Activator.CreateInstance(implementationType);
        }

        public static void AddService(Type serviceType, Type implementationType)
        {
            var dbProvider = DbProviderManager.Get();
            AddService(dbProvider.Name, serviceType, implementationType);
        }

        public static void AddService<TService, TImplementation>(string dbProviderName)
        {
            AddService(dbProviderName, typeof(TService), typeof(TImplementation));
        }

        public static void AddService<TService, TImplementation>()
        {
            var dbProvider = DbProviderManager.Get();
            AddService(dbProvider.Name, typeof(TService), typeof(TImplementation));
        }

        public static T GetService<T>(string dbProviderName)
        {
            if (serviceCollection.ContainsKey(dbProviderName))
            {
                var obj = serviceCollection[dbProviderName][typeof(T)];
                if (obj != null)
                {
                    return (T)obj;
                }
            }
            return default(T);
        }

        public static T GetService<T>() where T : class
        {
            var dbProvider = DbProviderManager.Get();
            return GetService<T>(dbProvider.Name);
        }

    }
}
