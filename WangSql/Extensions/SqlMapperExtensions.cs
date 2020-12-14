using System.Collections.Generic;
using System.Threading.Tasks;
using WangSql.DependencyInjection;

namespace WangSql
{
    public static class SqlMapperExtensions
    {
        public static IEnumerable<T> QueryPage<T>(this ISqlExe sqlMapper, string sql, object param, int pageIndex, int pageSize)
        {
            var pageProvider = IocManager.GetService<IPageProvider>(sqlMapper.SqlFactory.DbProvider.Name);
            return pageProvider.QueryPage<T>(sqlMapper, sql, param, pageIndex, pageSize);
        }

        public static async Task<IEnumerable<T>> QueryPageAsync<T>(this ISqlExe sqlMapper, string sql, object param, int pageIndex, int pageSize)
        {
            var pageProvider = IocManager.GetService<IPageProvider>(sqlMapper.SqlFactory.DbProvider.Name);
            return await pageProvider.QueryPageAsync<T>(sqlMapper, sql, param, pageIndex, pageSize);
        }
    }
}