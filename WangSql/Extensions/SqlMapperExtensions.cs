using System.Collections.Generic;
using System.Threading.Tasks;
using WangSql.Abstract.Linq;
using WangSql.Abstract.Migrate;
using WangSql.Abstract.Paged;
using WangSql.DependencyInjection;

namespace WangSql
{
    public static class SqlMapperExtensions
    {
        public static IMigrateProvider Migrate(this ISqlExe sqlMapper)
        {
            var provider = IocManager.GetService<IMigrateProvider>(sqlMapper.SqlFactory.DbProvider.Name);
            provider.Init(sqlMapper);
            return provider;
        }

        public static IEnumerable<T> QueryPage<T>(this ISqlExe sqlMapper, string sql, object param, int pageIndex, int pageSize, int? timeout = null)
        {
            var provider = IocManager.GetService<IPageProvider>(sqlMapper.SqlFactory.DbProvider.Name);
            provider.Init(sqlMapper);
            return provider.QueryPage<T>(sql, param, pageIndex, pageSize, timeout);
        }

        public static IEnumerable<T> QueryPage<T>(this ISqlExe sqlMapper, string sql, object param, int pageIndex, int pageSize, bool buffered = true, int? timeout = null)
        {
            var provider = IocManager.GetService<IPageProvider>(sqlMapper.SqlFactory.DbProvider.Name);
            provider.Init(sqlMapper);
            return provider.QueryPage<T>(sql, param, pageIndex, pageSize, buffered, timeout);
        }

        public static Task<IEnumerable<T>> QueryPageAsync<T>(this ISqlExe sqlMapper, string sql, object param, int pageIndex, int pageSize, int? timeout = null)
        {
            var provider = IocManager.GetService<IPageProvider>(sqlMapper.SqlFactory.DbProvider.Name);
            provider.Init(sqlMapper);
            return provider.QueryPageAsync<T>(sql, param, pageIndex, pageSize, timeout);
        }

        public static Task<IEnumerable<T>> QueryPageAsync<T>(this ISqlExe sqlMapper, string sql, object param, int pageIndex, int pageSize, bool buffered = true, int? timeout = null)
        {
            var provider = IocManager.GetService<IPageProvider>(sqlMapper.SqlFactory.DbProvider.Name);
            provider.Init(sqlMapper);
            return provider.QueryPageAsync<T>(sql, param, pageIndex, pageSize, buffered, timeout);
        }

        public static ISqlProvider<T> From<T>(this ISqlExe sqlMapper) where T : class
        {
            var provider = new DefaultSqlProvider<T>();
            provider.Init(sqlMapper);
            return provider;
        }

        public static ISqlProvider<T1, T2> From<T1, T2>(this ISqlExe sqlMapper) where T1 : class where T2 : class
        {
            var provider = new DefaultSqlProvider<T1, T2>();
            provider.Init(sqlMapper);
            return provider;
        }

        public static ISqlProvider<T1, T2, T3> From<T1, T2, T3>(this ISqlExe sqlMapper) where T1 : class where T2 : class where T3 : class
        {
            var provider = new DefaultSqlProvider<T1, T2, T3>();
            provider.Init(sqlMapper);
            return provider;
        }
    }
}