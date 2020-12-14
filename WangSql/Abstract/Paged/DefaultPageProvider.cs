using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WangSql
{
    public class DefaultPageProvider : IPageProvider
    {
        public virtual IEnumerable<T> QueryPage<T>(ISqlExe sqlMapper, string sql, object param, int pageIndex, int pageSize)
        {
            var r = sqlMapper.Query<T>(sql, param);
            var rr = r.Skip((pageIndex - 1) * pageSize).Take(pageSize);
            return rr;
        }
        public virtual async Task<IEnumerable<T>> QueryPageAsync<T>(ISqlExe sqlMapper, string sql, object param, int pageIndex, int pageSize)
        {
            var r = await sqlMapper.QueryAsync<T>(sql, param);
            var rr = r.Skip((pageIndex - 1) * pageSize).Take(pageSize);
            return rr;
        }
    }
}
