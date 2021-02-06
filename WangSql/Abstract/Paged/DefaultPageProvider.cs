using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WangSql.Abstract.Paged
{
    public class DefaultPageProvider : IPageProvider
    {
        #region constructor
        protected ISqlExe _sqlMapper { get; set; }
        public virtual void Init()
        {
        }
        public virtual void Init(ISqlExe sqlMapper)
        {
            _sqlMapper = sqlMapper;
        }
        #endregion

        public virtual IEnumerable<T> QueryPage<T>(string sql, object param, int pageIndex, int pageSize, int? timeout = null)
        {
            var r = _sqlMapper.Query<T>(sql, param, timeout);
            var rr = r.Skip((pageIndex - 1) * pageSize).Take(pageSize);
            return rr;
        }

        public virtual async Task<IEnumerable<T>> QueryPageAsync<T>(string sql, object param, int pageIndex, int pageSize, int? timeout = null)
        {
            var r = await _sqlMapper.QueryAsync<T>(sql, param, timeout);
            var rr = r.Skip((pageIndex - 1) * pageSize).Take(pageSize);
            return rr;
        }
    }
}
