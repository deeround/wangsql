using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WangSql.DefaultProvider.Paged
{
    public class DefaultPageProvider : IPageProvider
    {
        protected ISqlExe sqlExe;
        protected ISqlMapper sqlMapper;

        public virtual IPageProvider Instance(ISqlExe sqlExe)
        {
            this.sqlExe = sqlExe;
            return this;
        }
        public virtual IPageProvider Instance(ISqlMapper sqlMapper)
        {
            this.sqlMapper = sqlMapper;
            this.sqlExe = sqlMapper;
            return this;
        }

        public virtual IEnumerable<T> QueryPage<T>(string sql, object param, int pageIndex, int pageSize)
        {
            var r = sqlExe.Query<T>(sql, param);
            var rr = r.Skip((pageIndex - 1) * pageSize).Take(pageSize);
            return rr;
        }
        public virtual async Task<IEnumerable<T>> QueryPageAsync<T>(string sql, object param, int pageIndex, int pageSize)
        {
            var r = await sqlExe.QueryAsync<T>(sql, param);
            var rr = r.Skip((pageIndex - 1) * pageSize).Take(pageSize);
            return rr;
        }
    }
}
