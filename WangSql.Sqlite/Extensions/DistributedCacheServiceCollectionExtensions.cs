using Microsoft.Extensions.DependencyInjection;
using System;
using WangSql.Sqlite.Options;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DistributedCacheServiceCollectionExtensions
    {
        public static IServiceCollection AddDistributedCache(this IServiceCollection services, SqliteProviderOptions options)
        {
            return services;
        }
    }
}