using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WangSql.BuildProviders.Paged
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

        public virtual int PageCount(string sql, object param)
        {
            var r = sqlExe.Query<object>(sql, param);
            return r.Count();
        }
        public virtual async Task<int> PageCountAsync(string sql, object param)
        {
            var r = await sqlExe.QueryAsync<object>(sql, param);
            return r.Count();
        }
        public virtual IEnumerable<T> PageQuery<T>(string sql, object param, int pageIndex, int pageSize)
        {
            var r = sqlExe.Query<T>(sql, param);
            var rr = r.Skip((pageIndex - 1) * pageSize).Take(pageSize);
            return rr;
        }
        public virtual async Task<IEnumerable<T>> PageQueryAsync<T>(string sql, object param, int pageIndex, int pageSize)
        {
            var r = await sqlExe.QueryAsync<T>(sql, param);
            var rr = r.Skip((pageIndex - 1) * pageSize).Take(pageSize);
            return rr;
        }
    }
}
