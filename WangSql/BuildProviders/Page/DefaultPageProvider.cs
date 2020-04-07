using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WangSql
{
    public class DefaultPageProvider : IPageProvider
    {
        public virtual int BuildPageCountSql<T>(ISqlExe sqlMapper, string sql, object param)
        {
            var r = sqlMapper.Query<T>(sql, param);
            return r.Count();
        }
        public virtual IEnumerable<T> BuildPageSql<T>(ISqlExe sqlMapper, string sql, object param, int pageIndex, int pageSize)
        {
            var r = sqlMapper.Query<T>(sql, param);
            var rr = r.Skip((pageIndex - 1) * pageSize).Take(pageSize);
            return rr;
        }
    }
}
