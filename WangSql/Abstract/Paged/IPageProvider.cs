using System.Collections.Generic;
using System.Threading.Tasks;

namespace WangSql.Abstract.Paged
{
    public interface IPageProvider: IProvider
    {
        IEnumerable<T> QueryPage<T>(string sql, object param, int pageIndex, int pageSize, int? timeout = null);
        IEnumerable<T> QueryPage<T>(string sql, object param, int pageIndex, int pageSize, bool buffered = true, int? timeout = null);
        Task<IEnumerable<T>> QueryPageAsync<T>(string sql, object param, int pageIndex, int pageSize, int? timeout = null);
        Task<IEnumerable<T>> QueryPageAsync<T>(string sql, object param, int pageIndex, int pageSize, bool buffered = true, int? timeout = null);
    }
}