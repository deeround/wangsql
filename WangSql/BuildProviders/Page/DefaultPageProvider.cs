using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WangSql.BuildProviders.Page
{
    public class DefaultPageProvider : IPageProvider
    {
        protected ISqlExe sqlMapper;

        public virtual IPageProvider Instance(ISqlExe sqlMapper)
        {
            this.sqlMapper = sqlMapper;
            return this;
        }

        public virtual int BuildPageCountSql<T>(string sql, object param)
        {
            var r = sqlMapper.Query<T>(sql, param);
            return r.Count();
        }
        public virtual IEnumerable<T> BuildPageSql<T>(string sql, object param, int pageIndex, int pageSize)
        {
            var r = sqlMapper.Query<T>(sql, param);
            var rr = r.Skip((pageIndex - 1) * pageSize).Take(pageSize);
            return rr;
        }
    }
}
