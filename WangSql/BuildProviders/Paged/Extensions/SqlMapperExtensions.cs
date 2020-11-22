using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WangSql
{
    public static class SqlMapperPagedExtensions
    {
        public static int PageCount(this ISqlExe sqlExe, string sql, object param)
        {
            return sqlExe.SqlFactory.DbProvider.PageProvider.Instance(sqlExe).PageCount(sql, param);
        }

        public static async Task<int> PageCountAsync(this ISqlExe sqlExe, string sql, object param)
        {
            return await sqlExe.SqlFactory.DbProvider.PageProvider.Instance(sqlExe).PageCountAsync(sql, param);
        }

        public static IEnumerable<T> PageQuery<T>(this ISqlExe sqlExe, string sql, object param, int pageIndex, int pageSize)
        {
            return sqlExe.SqlFactory.DbProvider.PageProvider.Instance(sqlExe).PageQuery<T>(sql, param, pageIndex, pageSize);
        }

        public static async Task<IEnumerable<T>> PageQueryAsync<T>(this ISqlExe sqlExe, string sql, object param, int pageIndex, int pageSize)
        {
            return await sqlExe.SqlFactory.DbProvider.PageProvider.Instance(sqlExe).PageQueryAsync<T>(sql, param, pageIndex, pageSize);
        }
    }
}