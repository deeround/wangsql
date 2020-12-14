using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WangSql.Sqlite.Providers.Paged
{
    public class PageProvider : IPageProvider
    {
        public virtual IEnumerable<T> QueryPage<T>(ISqlExe sqlMapper, string sql, object param, int pageIndex, int pageSize)
        {
            if (pageIndex == 1)
            {
                sql = $@"SELECT llll.* FROM ({sql}) llll LIMIT {pageSize}";
            }
            else
            {
                sql = $@"SELECT llll.* FROM ({sql}) llll LIMIT {pageSize} OFFSET {(pageIndex - 1) * pageSize}";
            }
            return sqlMapper.Query<T>(sql, param);
        }
        public virtual async Task<IEnumerable<T>> QueryPageAsync<T>(ISqlExe sqlMapper, string sql, object param, int pageIndex, int pageSize)
        {
            if (pageIndex == 1)
            {
                sql = $@"SELECT llll.* FROM ({sql}) llll LIMIT {pageSize}";
            }
            else
            {
                sql = $@"SELECT llll.* FROM ({sql}) llll LIMIT {pageSize} OFFSET {(pageIndex - 1) * pageSize}";
            }
            return await sqlMapper.QueryAsync<T>(sql, param);
        }
    }
}
