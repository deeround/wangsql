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
        public static IEnumerable<T> QueryPage<T>(this ISqlExe sqlMapper, string sql, object param, int pageIndex, int pageSize, int? timeout = null)
        {
            var pageProvider = IocManager.GetService<IPageProvider>(sqlMapper.SqlFactory.DbProvider.Name);
            return pageProvider.QueryPage<T>(sqlMapper, sql, param, pageIndex, pageSize, timeout);
        }

        public static IEnumerable<T> QueryPage<T>(this ISqlExe sqlMapper, string sql, object param, int pageIndex, int pageSize, bool buffered = true, int? timeout = null)
        {
            var pageProvider = IocManager.GetService<IPageProvider>(sqlMapper.SqlFactory.DbProvider.Name);
            return pageProvider.QueryPage<T>(sqlMapper, sql, param, pageIndex, pageSize, buffered, timeout);
        }

        public static Task<IEnumerable<T>> QueryPageAsync<T>(this ISqlExe sqlMapper, string sql, object param, int pageIndex, int pageSize, int? timeout = null)
        {
            var pageProvider = IocManager.GetService<IPageProvider>(sqlMapper.SqlFactory.DbProvider.Name);
            return pageProvider.QueryPageAsync<T>(sqlMapper, sql, param, pageIndex, pageSize, timeout);
        }

        public static Task<IEnumerable<T>> QueryPageAsync<T>(this ISqlExe sqlMapper, string sql, object param, int pageIndex, int pageSize, bool buffered = true, int? timeout = null)
        {
            var pageProvider = IocManager.GetService<IPageProvider>(sqlMapper.SqlFactory.DbProvider.Name);
            return pageProvider.QueryPageAsync<T>(sqlMapper, sql, param, pageIndex, pageSize, buffered, timeout);
        }

        public static ISqlProvider<T> From<T>(this ISqlExe sqlMapper) where T : class
        {
            return new Sqlite.Linq.DefaultSqlProvider<T>(sqlMapper);
        }

        public static IQueryable<T1, T2> From<T1, T2>(this ISqlExe sqlMapper) where T1 : class where T2 : class
        {
            return new Sqlite.Linq.DefaultSqlProvider<T1, T2>(sqlMapper);
        }

        public static IQueryable<T1, T2, T3> From<T1, T2, T3>(this ISqlExe sqlMapper) where T1 : class where T2 : class where T3 : class
        {
            return new Sqlite.Linq.DefaultSqlProvider<T1, T2, T3>(sqlMapper);
        }

        public static IMigrateProvider Migrate(this ISqlExe sqlMapper)
        {
            var migrateProvider = IocManager.GetService<IMigrateProvider>(sqlMapper.SqlFactory.DbProvider.Name);
            return migrateProvider;
        }

    }
}