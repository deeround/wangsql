using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WangSql.Abstract.Paged;

namespace WangSql.Sqlite.Paged
{
    public class SqlitePageProvider : DefaultPageProvider, IPageProvider
    {
        public override IEnumerable<T> QueryPage<T>(string sql, object param, int pageIndex, int pageSize, int? timeout = null)
        {
            return this.QueryPage<T>(sql, param, pageIndex, pageSize, true, timeout);
        }

        public override IEnumerable<T> QueryPage<T>(string sql, object param, int pageIndex, int pageSize, bool buffered = true, int? timeout = null)
        {
            if (pageIndex == 1)
            {
                sql = $@"SELECT llll.* FROM ({sql}) llll LIMIT {pageSize}";
            }
            else
            {
                sql = $@"SELECT llll.* FROM ({sql}) llll LIMIT {pageSize} OFFSET {(pageIndex - 1) * pageSize}";
            }
            return _sqlMapper.Query<T>(sql, param, buffered, timeout);
        }

        public override Task<IEnumerable<T>> QueryPageAsync<T>(string sql, object param, int pageIndex, int pageSize, int? timeout = null)
        {
            return QueryPageAsync<T>(sql, param, pageIndex, pageSize, true, timeout);
        }

        public override Task<IEnumerable<T>> QueryPageAsync<T>(string sql, object param, int pageIndex, int pageSize, bool buffered = true, int? timeout = null)
        {
            if (pageIndex == 1)
            {
                sql = $@"SELECT llll.* FROM ({sql}) llll LIMIT {pageSize}";
            }
            else
            {
                sql = $@"SELECT llll.* FROM ({sql}) llll LIMIT {pageSize} OFFSET {(pageIndex - 1) * pageSize}";
            }
            return _sqlMapper.QueryAsync<T>(sql, param, buffered, timeout);
        }
    }
}
