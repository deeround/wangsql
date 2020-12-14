using System.Collections.Generic;
using System.Threading.Tasks;

namespace WangSql
{
    public interface IPageProvider
    {
        IEnumerable<T> QueryPage<T>(ISqlExe sqlMapper, string sql, object param, int pageIndex, int pageSize);
        Task<IEnumerable<T>> QueryPageAsync<T>(ISqlExe sqlMapper, string sql, object param, int pageIndex, int pageSize);
    }
}