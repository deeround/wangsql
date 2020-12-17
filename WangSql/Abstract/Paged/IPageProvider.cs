using System.Collections.Generic;
using System.Threading.Tasks;

namespace WangSql.Abstract.Paged
{
    public interface IPageProvider
    {
        IEnumerable<T> QueryPage<T>(ISqlExe sqlMapper, string sql, object param, int pageIndex, int pageSize, int? timeout = null);
        IEnumerable<T> QueryPage<T>(ISqlExe sqlMapper, string sql, object param, int pageIndex, int pageSize, bool buffered = true, int? timeout = null);
        Task<IEnumerable<T>> QueryPageAsync<T>(ISqlExe sqlMapper, string sql, object param, int pageIndex, int pageSize, int? timeout = null);
        Task<IEnumerable<T>> QueryPageAsync<T>(ISqlExe sqlMapper, string sql, object param, int pageIndex, int pageSize, bool buffered = true, int? timeout = null);
    }
}