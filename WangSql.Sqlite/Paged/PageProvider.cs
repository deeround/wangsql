using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WangSql.Abstract.Paged;

namespace WangSql.Sqlite.Paged
{
    public class PageProvider : IPageProvider
    {
        public IEnumerable<T> QueryPage<T>(ISqlExe sqlMapper, string sql, object param, int pageIndex, int pageSize, int? timeout = null)
        {
            return this.QueryPage<T>(sqlMapper, sql, param, pageIndex, pageSize, true, timeout);
        }

        public IEnumerable<T> QueryPage<T>(ISqlExe sqlMapper, string sql, object param, int pageIndex, int pageSize, bool buffered = true, int? timeout = null)
        {
            if (pageIndex == 1)
            {
                sql = $@"SELECT llll.* FROM ({sql}) llll LIMIT {pageSize}";
            }
            else
            {
                sql = $@"SELECT llll.* FROM ({sql}) llll LIMIT {pageSize} OFFSET {(pageIndex - 1) * pageSize}";
            }
            return sqlMapper.Query<T>(sql, param, buffered, timeout);
        }

        public Task<IEnumerable<T>> QueryPageAsync<T>(ISqlExe sqlMapper, string sql, object param, int pageIndex, int pageSize, int? timeout = null)
        {
            return QueryPageAsync<T>(sqlMapper, sql, param, pageIndex, pageSize, true, timeout);
        }

        public Task<IEnumerable<T>> QueryPageAsync<T>(ISqlExe sqlMapper, string sql, object param, int pageIndex, int pageSize, bool buffered = true, int? timeout = null)
        {
            if (pageIndex == 1)
            {
                sql = $@"SELECT llll.* FROM ({sql}) llll LIMIT {pageSize}";
            }
            else
            {
                sql = $@"SELECT llll.* FROM ({sql}) llll LIMIT {pageSize} OFFSET {(pageIndex - 1) * pageSize}";
            }
            return sqlMapper.QueryAsync<T>(sql, param, buffered, timeout);
        }
    }
}
